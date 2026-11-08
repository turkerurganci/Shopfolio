using System.Net;
using System.Text.RegularExpressions;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

using Npgsql;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Jobs;
using Shopfolio.Infrastructure.Routing;
using Shopfolio.IntegrationTests.Database;
using Shopfolio.IntegrationTests.Pages;
using Shopfolio.Modules.Catalog;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.CorporateContent;
using Shopfolio.Modules.CorporateContent.Contracts;
using Shopfolio.Modules.Settings;
using Shopfolio.Modules.Settings.Contracts;
using Shopfolio.Pages.Storefront;
using Shopfolio.UnitTests.Fakes;

namespace Shopfolio.IntegrationTests.Web;

/// <summary>The collection of the in-memory cache tests (T229): one database copied from the template.</summary>
[CollectionDefinition(AppCacheDefinition.Name)]
public sealed class AppCacheDefinition : ICollectionFixture<CollectionDatabaseFixture>
{
    public const string Name = "AppCache";
}

/// <summary>
/// The in-memory cache on a real PostgreSQL and the opened application (T229 criteria 2 to 7; KK2 to KK7; 09 10.4 item 6; 05 7.10.1, 4.1.5): the second read of an area
/// does not go to the database; a save drops the area after its commit and a refused or failed save leaves it; an old read cannot write its value over a save, with
/// real concurrent transactions; a dated announcement shows and hides with the clock and no invalidation; the price changes at once on the page while the cached
/// setting is old, and the order confirmation's transaction reads the setting and the sales gate from the database; the page's output is rendered on every request.
/// The pattern's module reads and page model are <c>CachePattern.cs</c>, linked from the unit tests, where the architecture test reads the same types.
/// </summary>
[Collection(AppCacheDefinition.Name)]
public sealed partial class AppCacheTests : IAsyncLifetime
{
    private const decimal InitialFee = 50m;

    private static readonly DateTimeOffset Now = new(2026, 11, 9, 12, 0, 0, TimeSpan.Zero);

    private readonly CollectionDatabaseFixture _database;

    public AppCacheTests(CollectionDatabaseFixture database)
    {
        _database = database;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [GeneratedRegex("""<p class="(price|shipping-fee|announcement|render)">([^<]*)</p>""", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex Field();

    public async ValueTask InitializeAsync()
    {
        await ExecuteAsync(
            "UPDATE company_settings SET shipping_fee = 50, shipping_fee_version = 1, is_sales_temporarily_closed = false; "
            + "DELETE FROM product_images; DELETE FROM variants; DELETE FROM products; DELETE FROM announcements;");
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    // ---- KK2: the second read of an area does not go to the database ----

    [Fact]
    public async Task TheSecondReadOfTheSettingsArea_DoesNotGoToTheDatabase_ButThePriceIsReadEveryTime()
    {
        string slug = await SeedProductAsync(100m);
        await using Provider provider = new(_database.Dsn, new ManualTimeProvider(Now));

        PatternShopSettingsView first = await provider.ReadDisplayAsync();
        int afterFirst = provider.Commands.SettingsReads;
        PatternShopSettingsView second = await provider.ReadDisplayAsync();

        Assert.Equal(InitialFee, first.ShippingFee);
        Assert.Same(first, second);
        Assert.Equal(1, afterFirst);
        Assert.Equal(1, provider.Commands.SettingsReads);

        await using AsyncServiceScope one = provider.Services.CreateAsyncScope();
        await using AsyncServiceScope two = provider.Services.CreateAsyncScope();
        Assert.NotNull(await one.ServiceProvider.GetRequiredService<IPatternPriceSource>().GetAsync(slug, Ct));
        Assert.NotNull(await two.ServiceProvider.GetRequiredService<IPatternPriceSource>().GetAsync(slug, Ct));
        Assert.Equal(2, provider.Commands.ProductReads);
    }

    // ---- KK3: a save drops the area after its commit; a refused or failed save and a call outside a transaction do not ----

    [Fact]
    public async Task ASavedSetting_IsTheNextRead_AfterTheCommit_AndTheGenerationRisesByOne()
    {
        await using Provider provider = new(_database.Dsn, new ManualTimeProvider(Now));
        await provider.ReadDisplayAsync();
        long before = provider.Cache.GenerationOf(CacheArea.Settings);

        Outcome outcome = await provider.SaveAsync(75m, 1);

        Assert.False(outcome.IsRejected);
        Assert.Equal(before + 1, provider.Cache.GenerationOf(CacheArea.Settings));
        Assert.Equal(75m, (await provider.ReadDisplayAsync()).ShippingFee);
        Assert.Equal(2, provider.Commands.SettingsReads);
        Assert.Equal(0, provider.Cache.GenerationOf(CacheArea.Menu));
    }

    [Fact]
    public async Task ARefusedSave_IsRolledBack_AndLeavesTheCachedValueAndTheGeneration()
    {
        await using Provider provider = new(_database.Dsn, new ManualTimeProvider(Now));
        await provider.ReadDisplayAsync();
        long before = provider.Cache.GenerationOf(CacheArea.Settings);

        await using AsyncServiceScope scope = provider.Services.CreateAsyncScope();
        IPatternShopSettings settings = scope.ServiceProvider.GetRequiredService<IPatternShopSettings>();
        Outcome outcome = await scope.ServiceProvider.GetRequiredService<ITransactionRunner>().RunAsync(
            async token =>
            {
                Assert.True(await settings.SaveShippingFeeAsync(60m, 1, token));
                return Outcome.Refused;
            },
            Ct);

        Assert.True(outcome.IsRejected);
        Assert.Equal(before, provider.Cache.GenerationOf(CacheArea.Settings));
        Assert.Equal(InitialFee, (await provider.ReadDisplayAsync()).ShippingFee);
        Assert.Equal(1, provider.Commands.SettingsReads);
        Assert.Equal(InitialFee, await ReadFeeAsync());
    }

    [Fact]
    public async Task ASaveThatThrows_IsRolledBack_AndLeavesTheCachedValueAndTheGeneration_AndTheNextSaveOfTheScopeDropsOnlyItsOwn()
    {
        await using Provider provider = new(_database.Dsn, new ManualTimeProvider(Now));
        await provider.ReadDisplayAsync();
        await provider.Cache.GetOrLoadAsync(CacheArea.Menu, _ => Task.FromResult("menu"), Ct);
        long before = provider.Cache.GenerationOf(CacheArea.Settings);

        await using AsyncServiceScope scope = provider.Services.CreateAsyncScope();
        IPatternShopSettings settings = scope.ServiceProvider.GetRequiredService<IPatternShopSettings>();
        ITransactionRunner runner = scope.ServiceProvider.GetRequiredService<ITransactionRunner>();
        await Assert.ThrowsAsync<TimeoutException>(() => runner.RunAsync<Outcome>(
            async token =>
            {
                Assert.True(await settings.SaveShippingFeeAsync(60m, 1, token));
                throw new TimeoutException("after the save");
            },
            Ct));

        Assert.Equal(before, provider.Cache.GenerationOf(CacheArea.Settings));
        Assert.Equal(InitialFee, (await provider.ReadDisplayAsync()).ShippingFee);
        Assert.Equal(InitialFee, await ReadFeeAsync());

        // The failed work's area is forgotten: the scope's next work, which names no area, drops nothing.
        CacheInvalidations invalidations = scope.ServiceProvider.GetRequiredService<CacheInvalidations>();
        await runner.RunAsync(_ => Task.FromResult(Outcome.Done), Ct);
        Assert.Empty(invalidations.TakePendingAreas());
        Assert.Equal(before, provider.Cache.GenerationOf(CacheArea.Settings));
        Assert.Equal(0, provider.Cache.GenerationOf(CacheArea.Menu));
    }

    [Fact]
    public async Task NamingAnAreaOutsideATransaction_IsRefused()
    {
        await using Provider provider = new(_database.Dsn, new ManualTimeProvider(Now));
        await using AsyncServiceScope scope = provider.Services.CreateAsyncScope();

        Assert.Throws<InvalidOperationException>(() => scope.ServiceProvider.GetRequiredService<CacheInvalidations>().Add(CacheArea.Settings));
        await Assert.ThrowsAsync<InvalidOperationException>(() => scope.ServiceProvider.GetRequiredService<IPatternShopSettings>().SaveShippingFeeAsync(60m, 1, Ct));
        Assert.Equal(InitialFee, await ReadFeeAsync());
        Assert.Equal(0, provider.Cache.GenerationOf(CacheArea.Settings));
    }

    [Fact]
    public async Task AReadDuringASaveThatIsThenRefused_FillsTheOldRow_WhichStaysRight_AndTheGenerationDoesNotMove()
    {
        await using Provider provider = new(_database.Dsn, new ManualTimeProvider(Now));
        TaskCompletionSource written = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TaskCompletionSource refuse = new(TaskCreationOptions.RunContinuationsAsynchronously);

        await using AsyncServiceScope writerScope = provider.Services.CreateAsyncScope();
        IPatternShopSettings writerSettings = writerScope.ServiceProvider.GetRequiredService<IPatternShopSettings>();
        Task<Outcome> writer = writerScope.ServiceProvider.GetRequiredService<ITransactionRunner>().RunAsync(
            async token =>
            {
                Assert.True(await writerSettings.SaveShippingFeeAsync(75m, 1, token));
                written.SetResult();
                await refuse.Task.WaitAsync(token);
                return Outcome.Refused;
            },
            Ct);
        await written.Task.WaitAsync(Ct);

        Assert.Equal(InitialFee, (await provider.ReadDisplayAsync()).ShippingFee);
        refuse.SetResult();
        Assert.True((await writer).IsRejected);

        Assert.Equal(0, provider.Cache.GenerationOf(CacheArea.Settings));
        Assert.Equal(InitialFee, (await provider.ReadDisplayAsync()).ShippingFee);
        Assert.Equal(1, provider.Commands.SettingsReads);
        Assert.Equal(InitialFee, await ReadFeeAsync());
    }

    [Fact]
    public async Task ACommitThatThrows_HasAnUnknownOutcome_SoItsAreasAreDropped_AndTheNextReadLoadsTheRowAsItStands()
    {
        await using Provider provider = new(_database.Dsn, new ManualTimeProvider(Now));
        await provider.ReadDisplayAsync();

        await using AsyncServiceScope scope = provider.Services.CreateAsyncScope();
        AppDbContext db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        IPatternShopSettings settings = scope.ServiceProvider.GetRequiredService<IPatternShopSettings>();
        await Assert.ThrowsAnyAsync<Exception>(() => scope.ServiceProvider.GetRequiredService<ITransactionRunner>().RunAsync(
            async token =>
            {
                Assert.True(await settings.SaveShippingFeeAsync(75m, 1, token));

                // This connection alone is ended before the commit: the server rolls the transaction back and the commit throws.
                int pid = ((NpgsqlConnection)db.Database.GetDbConnection()).ProcessID;
                await using NpgsqlConnection other = new(_database.Dsn);
                await other.OpenAsync(token);
                await using NpgsqlCommand terminate = new("SELECT pg_terminate_backend(@pid, 5000)", other);
                terminate.Parameters.AddWithValue("pid", pid);
                Assert.True((bool)(await terminate.ExecuteScalarAsync(token))!);
                return Outcome.Done;
            },
            Ct));

        Assert.Equal(1, provider.Cache.GenerationOf(CacheArea.Settings));
        Assert.Equal(InitialFee, (await provider.ReadDisplayAsync()).ShippingFee);
        Assert.Equal(2, provider.Commands.SettingsReads);
        Assert.Equal(InitialFee, await ReadFeeAsync());
    }

    // ---- KK4: an old read cannot write its value over a save (real concurrent transactions) ----

    [Fact]
    public async Task AReadThatLoadedTheOldRowBeforeTheCommit_AndIsHeldUntilAfterIt_GivesItsCallerTheOldValue_ButDoesNotStoreIt()
    {
        await using Provider provider = new(_database.Dsn, new ManualTimeProvider(Now));
        TaskCompletionSource loaded = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TaskCompletionSource release = new(TaskCreationOptions.RunContinuationsAsynchronously);

        await using AsyncServiceScope readerScope = provider.Services.CreateAsyncScope();
        AppDbContext readerDb = readerScope.ServiceProvider.GetRequiredService<AppDbContext>();
        Task<PatternShopSettingsView> reader = provider.Cache.GetOrLoadAsync(
            CacheArea.Settings,
            async token =>
            {
                PatternShopSettingsView value = await PatternShopSettings.LoadDisplayAsync(readerDb, token);
                loaded.SetResult();
                await release.Task.WaitAsync(token);
                return value;
            },
            Ct);
        await loaded.Task.WaitAsync(Ct);

        Assert.False((await provider.SaveAsync(75m, 1)).IsRejected);
        release.SetResult();

        Assert.Equal(InitialFee, (await reader).ShippingFee);
        Assert.Equal(75m, (await provider.ReadDisplayAsync()).ShippingFee);
        Assert.Equal(75m, (await provider.ReadDisplayAsync()).ShippingFee);
    }

    [Fact]
    public async Task AReadBetweenTheWriteAndTheCommit_FillsTheOldRow_AndTheCommitDropsIt()
    {
        await using Provider provider = new(_database.Dsn, new ManualTimeProvider(Now));
        TaskCompletionSource written = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TaskCompletionSource commit = new(TaskCreationOptions.RunContinuationsAsynchronously);

        await using AsyncServiceScope writerScope = provider.Services.CreateAsyncScope();
        IPatternShopSettings writerSettings = writerScope.ServiceProvider.GetRequiredService<IPatternShopSettings>();
        Task<Outcome> writer = writerScope.ServiceProvider.GetRequiredService<ITransactionRunner>().RunAsync(
            async token =>
            {
                Assert.True(await writerSettings.SaveShippingFeeAsync(75m, 1, token));
                written.SetResult();
                await commit.Task.WaitAsync(token);
                return Outcome.Done;
            },
            Ct);
        await written.Task.WaitAsync(Ct);
        long generation = provider.Cache.GenerationOf(CacheArea.Settings);

        // The transaction is open: the read sees the committed old row and fills the slot with it.
        Assert.Equal(InitialFee, (await provider.ReadDisplayAsync()).ShippingFee);
        Assert.Equal(InitialFee, (await provider.ReadDisplayAsync()).ShippingFee);
        Assert.Equal(1, provider.Commands.SettingsReads);
        Assert.Equal(generation, provider.Cache.GenerationOf(CacheArea.Settings));

        commit.SetResult();
        Assert.False((await writer).IsRejected);

        Assert.Equal(generation + 1, provider.Cache.GenerationOf(CacheArea.Settings));
        Assert.Equal(75m, (await provider.ReadDisplayAsync()).ShippingFee);
    }

    [Fact]
    public async Task TheWrongOrder_DroppingBeforeTheCommit_LeavesTheOldValueInTheCache_WhichIsWhyTheRunnerDropsAfterIt()
    {
        await using Provider provider = new(_database.Dsn, new ManualTimeProvider(Now));
        await using AsyncServiceScope writerScope = provider.Services.CreateAsyncScope();
        AppDbContext writerDb = writerScope.ServiceProvider.GetRequiredService<AppDbContext>();

        await using (Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction transaction = await writerDb.Database.BeginTransactionAsync(Ct))
        {
            await writerDb.CompanySettings.ExecuteUpdateAsync(s => s.SetProperty(x => x.ShippingFee, 90m), Ct);

            // Wrong: dropped before the commit; a read in between sees the old row and stores it under the new generation.
            provider.Cache.Invalidate(CacheArea.Settings);
            Assert.Equal(InitialFee, (await provider.ReadDisplayAsync()).ShippingFee);
            await transaction.CommitAsync(Ct);
        }

        Assert.Equal(90m, await ReadFeeAsync());
        Assert.Equal(InitialFee, (await provider.ReadDisplayAsync()).ShippingFee);
    }

    [Fact]
    public async Task ParallelReadersAndSavesThroughTheRunner_NeverGiveAReaderAValueOlderThanTheLastCommittedSave()
    {
        const int Saves = 20;
        const int Readers = 4;
        await using Provider provider = new(_database.Dsn, new ManualTimeProvider(Now));
        long committedFee = (long)InitialFee;
        int finished = 0;
        List<string> stale = [];

        async Task WriteAsync()
        {
            for (int i = 1; i <= Saves; i++)
            {
                decimal fee = InitialFee + i;
                Assert.False((await provider.SaveAsync(fee, i)).IsRejected);
                Interlocked.Exchange(ref committedFee, (long)fee);
            }

            Interlocked.Exchange(ref finished, 1);
        }

        async Task ReadAsync()
        {
            while (Volatile.Read(ref finished) == 0)
            {
                long floor = Interlocked.Read(ref committedFee);
                decimal read = (await provider.ReadDisplayAsync()).ShippingFee;
                if (read < floor)
                {
                    lock (stale)
                    {
                        stale.Add($"{read} < {floor}");
                    }
                }
            }
        }

        await Task.WhenAll([Task.Run(WriteAsync, Ct), .. Enumerable.Range(0, Readers).Select(_ => Task.Run(ReadAsync, Ct))]);

        Assert.Empty(stale);
        Assert.Equal(InitialFee + Saves, await ReadFeeAsync());
        Assert.Equal(InitialFee + Saves, (await provider.ReadDisplayAsync()).ShippingFee);
        Assert.Equal(Saves, provider.Cache.GenerationOf(CacheArea.Settings));
    }

    // ---- KK5: a dated announcement shows and hides with the clock, not with an invalidation ----

    [Fact]
    public async Task ADatedAnnouncementInTheCache_ShowsWhenItsRangeStarts_AndHidesWhenItEnds_WithoutAnInvalidationOrASecondLoad()
    {
        await ExecuteAsync(
            "INSERT INTO announcements (id, created_at, text, link_url, start_date, end_date, publish_status, version) "
            + "VALUES ('0193f2a0-0000-7000-8000-000000000001', now(), 'Kargo bedava', NULL, DATE '2026-11-10', DATE '2026-11-12', 'Published', 1);");
        ManualTimeProvider time = new(Now);
        await using Provider provider = new(_database.Dsn, time);

        Assert.Null(await provider.ReadAnnouncementAsync());

        // 2026-11-10 00:00 in Turkey (UTC+3) is 2026-11-09 21:00 UTC.
        time.SetUtcNow(new DateTimeOffset(2026, 11, 9, 20, 59, 59, TimeSpan.Zero));
        Assert.Null(await provider.ReadAnnouncementAsync());
        time.SetUtcNow(new DateTimeOffset(2026, 11, 9, 21, 0, 0, TimeSpan.Zero));
        Assert.Equal("Kargo bedava", (await provider.ReadAnnouncementAsync())?.Text);

        // The end date is the last day: it hides at 2026-11-13 00:00 in Turkey, 2026-11-12 21:00 UTC.
        time.SetUtcNow(new DateTimeOffset(2026, 11, 12, 20, 59, 59, TimeSpan.Zero));
        Assert.NotNull(await provider.ReadAnnouncementAsync());
        time.SetUtcNow(new DateTimeOffset(2026, 11, 12, 21, 0, 0, TimeSpan.Zero));
        Assert.Null(await provider.ReadAnnouncementAsync());

        Assert.Equal(1, provider.Commands.AnnouncementReads);
        Assert.Equal(0, provider.Cache.GenerationOf(CacheArea.HomepageBlocks));
    }

    // ---- KK6, KK7: the price at once, the confirmation from the database, no page output cache ----

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ThePriceShowsAtOnce_WhileTheCachedSettingIsOld_TheConfirmationReadsTheDatabase_AndThePageIsRenderedEveryTime(bool kestrel)
    {
        string slug = await SeedProductAsync(100m);
        await ExecuteAsync(
            "INSERT INTO announcements (id, created_at, text, link_url, start_date, end_date, publish_status, version) "
            + "VALUES ('0193f2a0-0000-7000-8000-000000000002', now(), 'Hos geldiniz', NULL, NULL, NULL, 'Published', 1);");
        await using Host host = new(_database.Dsn, kestrel);

        Dictionary<string, string> first = await GetPageAsync(host, slug);
        Assert.Equal(DisplayFormat.Amount(100m), first["price"]);
        Assert.Equal(DisplayFormat.Amount(InitialFee), first["shipping-fee"]);
        Assert.Equal("Hos geldiniz", first["announcement"]);

        // The rows change behind the cache's back (05 4.1.5): the price, the fee and the sales gate.
        await ExecuteAsync("UPDATE variants SET price = 120; UPDATE company_settings SET shipping_fee = 80, is_sales_temporarily_closed = true;");

        Dictionary<string, string> second = await GetPageAsync(host, slug);
        Assert.Equal(DisplayFormat.Amount(120m), second["price"]);
        Assert.Equal(DisplayFormat.Amount(InitialFee), second["shipping-fee"]);
        Assert.NotEqual(first["render"], second["render"]);

        // The confirmation's transaction reads the gate and the fee from the database, not from the cache that still holds 50.
        await using AsyncServiceScope scope = host.Factory.Services.CreateAsyncScope();
        IPatternShopSettings settings = scope.ServiceProvider.GetRequiredService<IPatternShopSettings>();
        PatternConfirmationSettings? read = null;
        await scope.ServiceProvider.GetRequiredService<ITransactionRunner>().RunAsync(
            async token =>
            {
                read = await settings.ReadForConfirmationAsync(token);
                return Outcome.Done;
            },
            Ct);
        Assert.NotNull(read);
        Assert.Equal(80m, read.ShippingFee);
        Assert.True(read.IsSalesTemporarilyClosed);
        Assert.Equal(InitialFee, (await settings.GetForDisplayAsync(Ct)).ShippingFee);
        await Assert.ThrowsAsync<InvalidOperationException>(() => settings.ReadForConfirmationAsync(Ct));

        // No page output cache in the application: neither the service nor a cache header.
        Assert.Null(host.Factory.Services.GetService<IOutputCacheStore>());
        Assert.Null(host.Factory.Services.GetService<Microsoft.AspNetCore.ResponseCaching.ResponseCachingOptions>());
    }

    // ---- The pieces ----

    private sealed class Outcome : IOperationResult
    {
        private Outcome(bool isRejected)
        {
            IsRejected = isRejected;
        }

        public static Outcome Done { get; } = new(false);

        public static Outcome Refused { get; } = new(true);

        public bool IsRejected { get; }
    }

    /// <summary>Counts the queries the contexts send, by the table they read.</summary>
    private sealed class QueryCounter : DbCommandInterceptor
    {
        private int _settings;
        private int _products;
        private int _announcements;

        public int SettingsReads => Volatile.Read(ref _settings);

        public int ProductReads => Volatile.Read(ref _products);

        public int AnnouncementReads => Volatile.Read(ref _announcements);

        public override ValueTask<InterceptionResult<System.Data.Common.DbDataReader>> ReaderExecutingAsync(
            System.Data.Common.DbCommand command,
            CommandEventData eventData,
            InterceptionResult<System.Data.Common.DbDataReader> result,
            CancellationToken cancellationToken = default)
        {
            string text = command.CommandText;
            if (text.StartsWith("SELECT", StringComparison.Ordinal))
            {
                if (text.Contains("FROM company_settings", StringComparison.Ordinal))
                {
                    Interlocked.Increment(ref _settings);
                }

                if (text.Contains("FROM variants", StringComparison.Ordinal))
                {
                    Interlocked.Increment(ref _products);
                }

                if (text.Contains("FROM announcements", StringComparison.Ordinal))
                {
                    Interlocked.Increment(ref _announcements);
                }
            }

            return ValueTask.FromResult(result);
        }
    }

    /// <summary>The cache's services as the application registers them, without the web host: one cache, a scope per unit of work.</summary>
    private sealed class Provider : IAsyncDisposable
    {
        private readonly ServiceProvider _services;

        public Provider(string dsn, TimeProvider time)
        {
            ServiceCollection services = new();
            services.AddDbContext<AppDbContext>(options => AppDbContext.Configure(options, dsn).AddInterceptors(Commands));
            services.AddScoped<JobEnqueueTracker>();
            services.AddSingleton<IJobSignal, JobSignal>();
            services.AddScoped<ITransactionRunner, TransactionRunner>();
            services.AddAppCache();
            services.AddSingleton(time);
            AddPattern(services);
            _services = services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true, ValidateOnBuild = true });
        }

        public QueryCounter Commands { get; } = new();

        public IServiceProvider Services => _services;

        public AppCache Cache => _services.GetRequiredService<AppCache>();

        public async Task<PatternShopSettingsView> ReadDisplayAsync()
        {
            await using AsyncServiceScope scope = _services.CreateAsyncScope();
            return await scope.ServiceProvider.GetRequiredService<IPatternShopSettings>().GetForDisplayAsync(Ct);
        }

        public async Task<PatternVisibleAnnouncement?> ReadAnnouncementAsync()
        {
            await using AsyncServiceScope scope = _services.CreateAsyncScope();
            return await scope.ServiceProvider.GetRequiredService<IPatternAnnouncementSource>().GetVisibleAsync(Ct);
        }

        /// <summary>The shipping fee's save in its own scope and transaction, as a panel save runs it.</summary>
        public async Task<Outcome> SaveAsync(decimal fee, int version)
        {
            await using AsyncServiceScope scope = _services.CreateAsyncScope();
            IPatternShopSettings settings = scope.ServiceProvider.GetRequiredService<IPatternShopSettings>();
            return await scope.ServiceProvider.GetRequiredService<ITransactionRunner>().RunAsync(
                async cancel => await settings.SaveShippingFeeAsync(fee, version, cancel) ? Outcome.Done : Outcome.Refused,
                Ct);
        }

        public async ValueTask DisposeAsync() => await _services.DisposeAsync();
    }

    private static void AddPattern(IServiceCollection services)
    {
        services.AddScoped<IPatternShopSettings, PatternShopSettings>();
        services.AddScoped<IPatternAnnouncementSource, PatternAnnouncementSource>();
        services.AddScoped<IPatternPriceSource, PatternPriceSource>();
    }

    /// <summary>What the reverse proxy adds to every request it passes on: the scheme the visitor used (05 6.3.3).</summary>
    private sealed class ProxyHandler : DelegatingHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            request.Headers.TryAddWithoutValidation("X-Forwarded-Proto", "https");
            return base.SendAsync(request, cancellationToken);
        }
    }

    /// <summary>The opened application with the pattern's services and the sample page; on a real Kestrel server when asked.</summary>
    private sealed class Host : IAsyncDisposable
    {
        private readonly ShopfolioWebApplicationFactory _root;

        public Host(string dsn, bool kestrel)
        {
            _root = new ShopfolioWebApplicationFactory(dsn);
            Factory = _root.WithWebHostBuilder(b => b.ConfigureServices(s =>
            {
                AddPattern(s);
                s.AddSingleton<Shopfolio.IntegrationTests.Pages.Panel.PatternRecordStore>();
                s.AddSingleton<FormatRecordStore>();
                s.AddRazorPages().AddApplicationPart(typeof(CachePatternPage).Assembly);
                if (kestrel)
                {
                    s.PostConfigure<ForwardedHeadersOptions>(o =>
                    {
                        o.KnownProxies.Add(IPAddress.Loopback);
                        o.KnownProxies.Add(IPAddress.IPv6Loopback);
                    });
                }
            }));

            if (kestrel)
            {
                Factory.Start();
                using HttpClient probe = Factory.CreateClient();
                HttpClientHandler socket = new() { AllowAutoRedirect = false, UseCookies = false };
                Client = new HttpClient(new ForgeryTokenHandler { InnerHandler = new ProxyHandler { InnerHandler = socket } }) { BaseAddress = probe.BaseAddress };
            }
            else
            {
                Client = ForgeryTokenHandler.CreateClient(Factory);
            }
        }

        public WebApplicationFactory<Program> Factory { get; }

        public HttpClient Client { get; }

        public async ValueTask DisposeAsync()
        {
            Client.Dispose();
            await Factory.DisposeAsync();
            await _root.DisposeAsync();
        }
    }

    private static async Task<Dictionary<string, string>> GetPageAsync(Host host, string slug)
    {
        using HttpResponseMessage response = await host.Client.GetAsync(new Uri($"/odeme/onbellek-ornegi/{slug}", UriKind.Relative), Ct);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Null(response.Headers.Age);
        Assert.False(response.Headers.CacheControl?.Public ?? false);
        string html = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync(Ct));
        return Field().Matches(html).ToDictionary(m => m.Groups[1].Value, m => m.Groups[2].Value, StringComparer.Ordinal);
    }

    private async Task<string> SeedProductAsync(decimal price)
    {
        await using Provider provider = new(_database.Dsn, new ManualTimeProvider(Now));
        await using AsyncServiceScope scope = provider.Services.CreateAsyncScope();
        AppDbContext db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        string slug = Slug.From("Ahşap Masa", AddressRules.ProductPrefix);
        Product product = Product.Create("Ahşap Masa", slug, ProductType.Physical, 20m, Now);
        product.PublishStatus = PublishStatus.Published;
        db.Products.Add(product);
        db.Variants.Add(Variant.Create(product.Id, "AHSAP-MASA-01", price, Now, 100));
        await db.SaveChangesAsync(Ct);
        return slug;
    }

    private async Task<decimal> ReadFeeAsync()
    {
        await using NpgsqlConnection connection = new(_database.Dsn);
        await connection.OpenAsync(Ct);
        await using NpgsqlCommand command = new("SELECT shipping_fee FROM company_settings", connection);
        return (decimal)(await command.ExecuteScalarAsync(Ct))!;
    }

    private async Task ExecuteAsync(string sql)
    {
        await using NpgsqlConnection connection = new(_database.Dsn);
        await connection.OpenAsync(Ct);
        await using NpgsqlCommand command = new(sql, connection);
        await command.ExecuteNonQueryAsync(Ct);
    }
}
