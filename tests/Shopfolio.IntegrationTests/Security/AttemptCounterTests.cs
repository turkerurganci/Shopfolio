using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Npgsql;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Jobs;
using Shopfolio.Infrastructure.Security;
using Shopfolio.Infrastructure.Values;
using Shopfolio.IntegrationTests.Database;
using Shopfolio.UnitTests.Fakes;

namespace Shopfolio.IntegrationTests.Security;

/// <summary>The collection of the attempt counter tests (T215): one database copied from the template, nothing shared with another collection.</summary>
[CollectionDefinition(AttemptCounterDefinition.Name)]
public sealed class AttemptCounterDefinition : ICollectionFixture<CollectionDatabaseFixture>
{
    public const string Name = "AttemptCounter";
}

/// <summary>
/// The attempt counter on a real PostgreSQL and a fake clock (T215; 09 15.4 items 5 and 6; 05 6.3.1, 6.3.2): the limits counted on a failure and the
/// ones counted on every request, the forgetting, attempts arriving at the same moment, the window's edge, the cart merge and the one normalized value
/// of an e-mail. Every threshold and window is read from the product constants through the limit table; no test waits for real time.
/// </summary>
[Collection(AttemptCounterDefinition.Name)]
public sealed class AttemptCounterTests : IAsyncLifetime
{
    private static readonly DateTimeOffset Start = new(2026, 11, 5, 6, 0, 0, TimeSpan.Zero);

    private readonly CollectionDatabaseFixture _collection;
    private readonly ManualTimeProvider _time = new(Start);
    private NpgsqlDataSource _dataSource = null!;
    private ServiceProvider _provider = null!;

    public AttemptCounterTests(TemplateDatabaseFixture template, CollectionDatabaseFixture collection)
    {
        ArgumentNullException.ThrowIfNull(template);
        _collection = collection;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public async ValueTask InitializeAsync()
    {
        await using (NpgsqlConnection connection = new(_collection.Dsn))
        {
            await connection.OpenAsync(Ct);
            await using NpgsqlCommand truncate = new("TRUNCATE attempt_records CASCADE", connection);
            await truncate.ExecuteNonQueryAsync(Ct);
        }

        _dataSource = NpgsqlDataSource.Create(_collection.Dsn);
        ServiceCollection services = new();
        services.AddSingleton<TimeProvider>(_time);
        services.AddSingleton(_dataSource);
        services.AddDbContext<AppDbContext>(options => AppDbContext.Configure(options, _collection.Dsn));
        services.AddScoped<ITransactionRunner, TransactionRunner>();
        services.AddAppCache();
        services.AddScoped<JobEnqueueTracker>();
        services.AddSingleton<IJobSignal, JobSignal>();
        services.AddLogging(builder => builder.SetMinimumLevel(LogLevel.None));
        services.AddScoped<AttemptCounter>();
        _provider = services.BuildServiceProvider();
    }

    public async ValueTask DisposeAsync()
    {
        await _provider.DisposeAsync();
        await _dataSource.DisposeAsync();
    }

    [Fact]
    public async Task SignIn_limit_on_email_blocks_the_sixth_failure_in_the_window()
    {
        AttemptSubject subject = AttemptSubject.Email("ayse@example.com");
        AttemptRule rule = AttemptLimits.Of(AttemptLimit.L1, AttemptAxis.Email);

        await WithCounterAsync(async counter =>
        {
            for (int i = 1; i <= rule.Threshold; i++)
            {
                await counter.RecordAsync(AttemptLimit.L1, subject, Ct);
                Assert.False(await counter.IsExceededAsync(AttemptLimit.L1, subject, Ct), $"attempt {i} must not be exceeded");
            }

            await counter.RecordAsync(AttemptLimit.L1, subject, Ct);
            Assert.True(await counter.IsExceededAsync(AttemptLimit.L1, subject, Ct));
        });
    }

    [Fact]
    public async Task A_succeeding_attempt_forgets_its_row_so_the_count_stays_at_the_failures()
    {
        AttemptSubject subject = AttemptSubject.Email("ayse@example.com");
        AttemptRule rule = AttemptLimits.Of(AttemptLimit.L1, AttemptAxis.Email);

        await WithCounterAsync(async counter =>
        {
            for (int i = 0; i < rule.Threshold; i++)
            {
                await counter.RecordAsync(AttemptLimit.L1, subject, Ct);
            }

            Guid successful = await counter.RecordAsync(AttemptLimit.L1, subject, Ct);
            Assert.True(await counter.IsExceededAsync(AttemptLimit.L1, subject, Ct));
            await counter.ForgetAsync(successful, Ct);
            Assert.False(await counter.IsExceededAsync(AttemptLimit.L1, subject, Ct));
        });

        Assert.Equal(rule.Threshold, await CountRowsAsync(AttemptLimit.L1, subject));
    }

    [Fact]
    public async Task Forgetting_an_exceeded_attempt_removes_exactly_its_row()
    {
        AttemptSubject subject = AttemptSubject.Ip(System.Net.IPAddress.Parse("203.0.113.9"));
        Guid kept = Guid.Empty;
        Guid forgotten = Guid.Empty;

        await WithCounterAsync(async counter =>
        {
            kept = await counter.RecordAsync(AttemptLimit.L4, subject, Ct);
            forgotten = await counter.RecordAsync(AttemptLimit.L4, subject, Ct);
            await counter.ForgetAsync(forgotten, Ct);
        });

        List<Guid> ids = await ReadAsync(db => db.AttemptRecords.AsNoTracking().Select(a => a.Id).ToListAsync(Ct));
        Assert.Equal([kept], ids);
    }

    [Fact]
    public async Task Two_attempts_at_the_same_moment_see_each_others_rows()
    {
        AttemptSubject subject = AttemptSubject.Email("ayse@example.com");
        AttemptRule rule = AttemptLimits.Of(AttemptLimit.L1, AttemptAxis.Email);

        await WithCounterAsync(async counter =>
        {
            for (int i = 0; i < rule.Threshold - 1; i++)
            {
                await counter.RecordAsync(AttemptLimit.L1, subject, Ct);
            }

            // The fifth and the sixth arrive together: each commits its row before it counts, so at least the later count includes both.
            await using AsyncServiceScope first = _provider.CreateAsyncScope();
            await using AsyncServiceScope second = _provider.CreateAsyncScope();
            AttemptCounter a = first.ServiceProvider.GetRequiredService<AttemptCounter>();
            AttemptCounter b = second.ServiceProvider.GetRequiredService<AttemptCounter>();
            await Task.WhenAll(a.RecordAsync(AttemptLimit.L1, subject, Ct), b.RecordAsync(AttemptLimit.L1, subject, Ct));
            Assert.True(await a.IsExceededAsync(AttemptLimit.L1, subject, Ct));
            Assert.True(await b.IsExceededAsync(AttemptLimit.L1, subject, Ct));
        });

        Assert.Equal(rule.Threshold + 1, await CountRowsAsync(AttemptLimit.L1, subject));
    }

    [Fact]
    public async Task A_limit_counted_on_every_request_keeps_its_rows_and_exceeds_after_the_threshold()
    {
        AttemptSubject subject = AttemptSubject.Email("ayse@example.com");
        AttemptRule rule = AttemptLimits.Of(AttemptLimit.L2, AttemptAxis.Email);

        await WithCounterAsync(async counter =>
        {
            for (int i = 0; i < rule.Threshold; i++)
            {
                await counter.RecordAsync(AttemptLimit.L2, subject, Ct);
            }

            Assert.False(await counter.IsExceededAsync(AttemptLimit.L2, subject, Ct));
            await counter.RecordAsync(AttemptLimit.L2, subject, Ct);
            Assert.True(await counter.IsExceededAsync(AttemptLimit.L2, subject, Ct));
        });

        Assert.Equal(rule.Threshold + 1, await CountRowsAsync(AttemptLimit.L2, subject));
    }

    [Fact]
    public async Task The_limit_and_the_axis_keep_the_counts_apart()
    {
        AttemptSubject email = AttemptSubject.Email("ayse@example.com");
        AttemptRule rule = AttemptLimits.Of(AttemptLimit.L1, AttemptAxis.Email);

        await WithCounterAsync(async counter =>
        {
            for (int i = 0; i <= rule.Threshold; i++)
            {
                await counter.RecordAsync(AttemptLimit.L1, email, Ct);
            }

            Assert.True(await counter.IsExceededAsync(AttemptLimit.L1, email, Ct));
            Assert.False(await counter.IsExceededAsync(AttemptLimit.L2, email, Ct));
            Assert.False(await counter.IsExceededAsync(AttemptLimit.L1, AttemptSubject.Email("baska@example.com"), Ct));
        });
    }

    [Fact]
    public async Task The_guest_lookups_email_axis_counts_the_typed_address_with_no_order_behind_it_and_the_ip_axis_stays_apart_T377_KK3()
    {
        // No order exists anywhere in this database: the axis counts what was typed in the query, whether or not an order has that address (02 8.2.2, 8.3.5).
        AttemptSubject typed = AttemptSubject.Email("kimse-yok@example.com");
        AttemptSubject address = AttemptSubject.Ip(System.Net.IPAddress.Parse("203.0.113.40"));
        AttemptRule emailRule = AttemptLimits.Of(AttemptLimit.L5, AttemptAxis.Email);

        await WithCounterAsync(async counter =>
        {
            for (int i = 0; i <= emailRule.Threshold; i++)
            {
                await counter.RecordAsync(AttemptLimit.L5, typed, Ct);
            }

            Assert.True(await counter.IsExceededAsync(AttemptLimit.L5, typed, Ct));
            Assert.False(await counter.IsExceededAsync(AttemptLimit.L5, address, Ct));
            Assert.False(await counter.IsExceededAsync(AttemptLimit.L5, AttemptSubject.Email("baska@example.com"), Ct));
        });
    }

    [Fact]
    public async Task A_row_exactly_one_window_old_counts_and_one_millisecond_older_does_not()
    {
        AttemptSubject subject = AttemptSubject.Email("ayse@example.com");
        AttemptRule rule = AttemptLimits.Of(AttemptLimit.L1, AttemptAxis.Email);

        await WithCounterAsync(async counter =>
        {
            // Threshold rows at the start, then one more now: the count is over the threshold only while the first rows are inside the window.
            for (int i = 0; i < rule.Threshold; i++)
            {
                await counter.RecordAsync(AttemptLimit.L1, subject, Ct);
            }

            _time.Advance(rule.Window);
            await counter.RecordAsync(AttemptLimit.L1, subject, Ct);
            Assert.True(await counter.IsExceededAsync(AttemptLimit.L1, subject, Ct), "rows exactly one window old still count");

            _time.Advance(TimeSpan.FromMilliseconds(1));
            Assert.False(await counter.IsExceededAsync(AttemptLimit.L1, subject, Ct), "rows one millisecond older than the window do not count");
        });
    }

    [Fact]
    public async Task The_window_slides_with_every_attempt_and_the_block_opens_by_itself_when_the_count_falls_to_the_threshold_T326_KK8()
    {
        AttemptSubject subject = AttemptSubject.Email("ayse@example.com");
        AttemptRule rule = AttemptLimits.Of(AttemptLimit.L1, AttemptAxis.Email);
        TimeSpan step = TimeSpan.FromMinutes(1);
        Assert.True(step * (rule.Threshold + 1) < rule.Window, "the attempts of the test all fall inside one window");

        await WithCounterAsync(async counter =>
        {
            // Threshold + 1 failures, one minute apart: the last one passes the threshold and the subject's work is blocked.
            for (int i = 0; i <= rule.Threshold; i++)
            {
                await counter.RecordAsync(AttemptLimit.L1, subject, Ct);
                bool exceeded = await counter.IsExceededAsync(AttemptLimit.L1, subject, Ct);
                Assert.Equal(i == rule.Threshold, exceeded);
                if (i < rule.Threshold)
                {
                    _time.Advance(step);
                }
            }

            int rowsWhileBlocked = await CountRowsAsync(AttemptLimit.L1, subject);
            Assert.Equal(rule.Threshold + 1, rowsWhileBlocked);

            // The first row leaves the window (it started Threshold minutes ago): the count is the threshold and the block is open - nobody opened it.
            _time.Advance(rule.Window - (step * rule.Threshold) + TimeSpan.FromMilliseconds(1));
            Assert.False(await counter.IsExceededAsync(AttemptLimit.L1, subject, Ct));
            Assert.Equal(rowsWhileBlocked, await CountRowsAsync(AttemptLimit.L1, subject));

            // A new failure counts again from now, over the rows that are still inside the window: the block closes again by the same sliding count.
            await counter.RecordAsync(AttemptLimit.L1, subject, Ct);
            Assert.True(await counter.IsExceededAsync(AttemptLimit.L1, subject, Ct));

            // Past the window of every row the subject is clear again, with no unlock step in between.
            _time.Advance(rule.Window + TimeSpan.FromMilliseconds(1));
            Assert.False(await counter.IsExceededAsync(AttemptLimit.L1, subject, Ct));
        });
    }

    [Theory]
    [InlineData(AttemptLimit.L7)]
    [InlineData(AttemptLimit.L8)]
    public async Task The_limits_counted_from_the_orders_are_refused(AttemptLimit limit)
    {
        await WithCounterAsync(async counter =>
        {
            await Assert.ThrowsAsync<ArgumentException>(() => counter.RecordAsync(limit, AttemptSubject.Email("ayse@example.com"), Ct));
            await Assert.ThrowsAsync<ArgumentException>(() => counter.IsExceededAsync(limit, AttemptSubject.Email("ayse@example.com"), Ct));
        });

        Assert.Equal(0, await ReadAsync(db => db.AttemptRecords.CountAsync(Ct)));
    }

    [Fact]
    public async Task A_limit_on_an_axis_it_does_not_count_on_is_refused()
    {
        await WithCounterAsync(async counter =>
        {
            await Assert.ThrowsAsync<ArgumentException>(() => counter.RecordAsync(AttemptLimit.L4, AttemptSubject.Email("ayse@example.com"), Ct));
            await Assert.ThrowsAsync<ArgumentException>(() => counter.RecordAsync(AttemptLimit.L6, AttemptSubject.Ip(System.Net.IPAddress.Loopback), Ct));
        });
    }

    [Fact]
    public async Task The_email_is_one_value_whatever_the_case_or_the_dotted_and_dotless_i()
    {
        string[] spellings = ["ISPARTA@Example.com", "isparta@example.com", "ıSparta@example.com", "İsparta@example.com", "  isparta@example.com "];
        HashSet<string> values = [.. spellings.Select(s => AttemptSubject.Email(s).Value)];
        Assert.Single(values);

        await WithCounterAsync(async counter =>
        {
            foreach (string spelling in spellings)
            {
                await counter.RecordAsync(AttemptLimit.L1, AttemptSubject.Email(spelling), Ct);
            }
        });

        Assert.Equal(spellings.Length, await ReadAsync(db => db.AttemptRecords.CountAsync(a => a.NormalizedValue == values.Single(), Ct)));
    }

    [Fact]
    public async Task Merging_carts_keeps_the_rows_of_the_cart_with_more_attempts_under_the_account_cart()
    {
        Guid guest = Guid.NewGuid();
        Guid account = Guid.NewGuid();
        List<DateTimeOffset> guestMoments = [];

        await WithCounterAsync(async counter =>
        {
            await counter.RecordAsync(AttemptLimit.L6, AttemptSubject.Cart(account), Ct);
            for (int i = 0; i < 3; i++)
            {
                guestMoments.Add(_time.GetUtcNow());
                await counter.RecordAsync(AttemptLimit.L6, AttemptSubject.Cart(guest), Ct);
                _time.Advance(TimeSpan.FromSeconds(10));
            }

            await counter.MergeCartAsync(guest, account, Ct);
        });

        List<DateTimeOffset> accountMoments = await ReadAsync(db => db.AttemptRecords.AsNoTracking()
            .Where(a => a.NormalizedValue == account.ToString("D")).OrderBy(a => a.CreatedAt).Select(a => a.CreatedAt).ToListAsync(Ct));
        Assert.Equal(guestMoments, accountMoments);
        Assert.Equal(0, await CountRowsAsync(AttemptLimit.L6, AttemptSubject.Cart(guest)));
    }

    [Fact]
    public async Task Merging_carts_with_the_same_number_of_attempts_keeps_the_account_carts_rows()
    {
        Guid guest = Guid.NewGuid();
        Guid account = Guid.NewGuid();
        List<Guid> accountRows = [];

        await WithCounterAsync(async counter =>
        {
            accountRows.Add(await counter.RecordAsync(AttemptLimit.L6, AttemptSubject.Cart(account), Ct));
            accountRows.Add(await counter.RecordAsync(AttemptLimit.L6, AttemptSubject.Cart(account), Ct));
            await counter.RecordAsync(AttemptLimit.L6, AttemptSubject.Cart(guest), Ct);
            await counter.RecordAsync(AttemptLimit.L6, AttemptSubject.Cart(guest), Ct);
            await counter.MergeCartAsync(guest, account, Ct);
        });

        List<Guid> ids = await ReadAsync(db => db.AttemptRecords.AsNoTracking().Select(a => a.Id).ToListAsync(Ct));
        Assert.Equal(accountRows.Order(), ids.Order());
    }

    [Fact]
    public async Task Merging_carts_when_the_account_cart_has_more_attempts_drops_the_guest_rows()
    {
        Guid guest = Guid.NewGuid();
        Guid account = Guid.NewGuid();

        await WithCounterAsync(async counter =>
        {
            await counter.RecordAsync(AttemptLimit.L6, AttemptSubject.Cart(account), Ct);
            await counter.RecordAsync(AttemptLimit.L6, AttemptSubject.Cart(account), Ct);
            await counter.RecordAsync(AttemptLimit.L6, AttemptSubject.Cart(guest), Ct);
            await counter.MergeCartAsync(guest, account, Ct);
        });

        Assert.Equal(2, await CountRowsAsync(AttemptLimit.L6, AttemptSubject.Cart(account)));
        Assert.Equal(0, await CountRowsAsync(AttemptLimit.L6, AttemptSubject.Cart(guest)));
    }

    private async Task WithCounterAsync(Func<AttemptCounter, Task> work)
    {
        await using AsyncServiceScope scope = _provider.CreateAsyncScope();
        await work(scope.ServiceProvider.GetRequiredService<AttemptCounter>());
    }

    private async Task<T> ReadAsync<T>(Func<AppDbContext, Task<T>> read)
    {
        await using AsyncServiceScope scope = _provider.CreateAsyncScope();
        return await read(scope.ServiceProvider.GetRequiredService<AppDbContext>());
    }

    private Task<int> CountRowsAsync(AttemptLimit limit, AttemptSubject subject) =>
        ReadAsync(db => db.AttemptRecords.CountAsync(a => a.AttemptLimit == limit && a.AttemptAxis == subject.Axis && a.NormalizedValue == subject.Value, Ct));
}
