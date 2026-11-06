using System.Security.Cryptography;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

using Npgsql;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.InstallationSettings;
using Shopfolio.Infrastructure.Jobs;
using Shopfolio.Infrastructure.Security;
using Shopfolio.Infrastructure.Values;
using Shopfolio.IntegrationTests.Database;
using Shopfolio.Modules.Administration;
using Shopfolio.Modules.Membership;
using Shopfolio.UnitTests.Fakes;

namespace Shopfolio.IntegrationTests.Security;

/// <summary>The collection of the link record tests (T222): one database copied from the template, made by the real migrations.</summary>
[CollectionDefinition(LinkRecordsDefinition.Name)]
public sealed class LinkRecordsDefinition : ICollectionFixture<CollectionDatabaseFixture>
{
    public const string Name = "LinkRecords";
}

/// <summary>
/// The link record layer on a real PostgreSQL (T222; 05 6.4; 09 15.1 items 3 and 4): the record holds only the digest of the key (KK3), the
/// five states read from the record come out as five different results (KK4), reading changes no record (KK5), and consuming is one
/// conditional update that takes only a valid link - a second consuming, an expired and a superseded link change no row, and of two requests at
/// once exactly one succeeds (KK6). A row's change is seen from <c>xmin</c> as well as the values, so a write that sets the same value shows.
/// </summary>
[Collection(LinkRecordsDefinition.Name)]
public sealed class LinkRecordsTests : IAsyncLifetime
{
    private static readonly DateTimeOffset Now = new(2026, 11, 5, 9, 0, 0, TimeSpan.Zero);

    // The tables a link's opening must leave alone (KK5): the link, the account and the sessions of both kinds of account.
    private static readonly string[] UntouchedTables = ["link_records", "customers", "admins", "sessions", "orders", "dispatch_records"];

    private readonly CollectionDatabaseFixture _collection;
    private readonly LinkKeys _keys = new(Options.Create(new LinkKeyOptions { DerivationSecret = Convert.ToBase64String(RandomNumberGenerator.GetBytes(LinkKeyOptions.DerivationSecretMinBytes)) }));
    private readonly ManualTimeProvider _time = new(Now);

    public LinkRecordsTests(CollectionDatabaseFixture collection)
    {
        _collection = collection;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public async ValueTask InitializeAsync()
    {
        await ExecuteAsync("TRUNCATE link_records, sessions, dispatch_records, customers, admins CASCADE");
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    // ---- KK3: only the digest is stored ----

    [Fact]
    public async Task Record_HoldsTheDigestOfTheDerivedKey_AndNoColumnHoldsTheKey()
    {
        Customer customer = await SeedCustomerAsync();
        LinkRecord record = LinkRecord.ForCustomer(_keys, LinkType.PasswordReset, customer.Id, Now.AddHours(1), Now);
        await SaveAsync(c => c.LinkRecords.Add(record));
        string key = _keys.Derive(record.Id);

        string stored = await ScalarAsync<string>($"SELECT token_hash FROM link_records WHERE id = '{record.Id}'");
        string wholeRow = await ScalarAsync<string>($"SELECT to_jsonb(l)::text FROM link_records l WHERE id = '{record.Id}'");

        Assert.Equal(SecretKey.Hash(key), stored);
        Assert.DoesNotContain(key, wholeRow, StringComparison.Ordinal);
        Assert.DoesNotContain(key, await WholeDatabaseTextAsync(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task Record_ForTheThreeOwners_EachHoldsItsOwnDigest()
    {
        Customer customer = await SeedCustomerAsync();
        Admin admin = Admin.Create("Veli", "veli@example.com", "digest-placeholder", Now);
        AdminInvitation invitation = AdminInvitation.Create("davet@example.com", null, "Veli", Now);
        await SaveAsync(c =>
        {
            c.Admins.Add(admin);
            c.AdminInvitations.Add(invitation);
        });
        LinkRecord[] records =
        [
            LinkRecord.ForCustomer(_keys, LinkType.EmailVerification, customer.Id, Now.AddHours(1), Now),
            LinkRecord.ForAdmin(_keys, LinkType.PasswordReset, admin.Id, Now.AddHours(1), Now),
            LinkRecord.ForInvitation(_keys, invitation.Id, Now.AddHours(1), Now),
        ];
        await SaveAsync(c => c.LinkRecords.AddRange(records));

        foreach (LinkRecord record in records)
        {
            Assert.Equal(_keys.DigestFor(record.Id), await ScalarAsync<string>($"SELECT token_hash FROM link_records WHERE id = '{record.Id}'"));
        }

        Assert.Equal(records.Length, records.Select(r => r.TokenHash).Distinct(StringComparer.Ordinal).Count());
    }

    // ---- KK4: the five states ----

    [Fact]
    public async Task Read_GivesFiveDifferentResults_ForNoRecordExpiredUsedSupersededAndValid()
    {
        Customer customer = await SeedCustomerAsync();
        LinkRecord valid = NewLink(customer, expiresAt: Now.AddHours(1));
        LinkRecord expired = NewLink(customer, expiresAt: Now.AddMinutes(-1));
        LinkRecord used = NewLink(customer, expiresAt: Now.AddHours(1));
        used.UsedAt = Now.AddMinutes(-5);
        LinkRecord superseded = NewLink(customer, expiresAt: Now.AddHours(1));
        superseded.SupersededAt = Now.AddMinutes(-5);
        await SaveAsync(c => c.LinkRecords.AddRange(valid, expired, used, superseded));

        List<(LinkState? State, bool Exists)> results = [];
        await using AppDbContext db = NewContext();
        LinkRecords links = new(db, _time);
        foreach (string key in new[] { SecretKey.Create(), _keys.Derive(expired.Id), _keys.Derive(used.Id), _keys.Derive(superseded.Id), _keys.Derive(valid.Id) })
        {
            LinkReading reading = await links.ReadAsync(key, Ct);
            results.Add((reading.State, reading.Exists));
        }

        Assert.Equal(
            [(null, false), (LinkState.Expired, true), (LinkState.Used, true), (LinkState.Superseded, true), (LinkState.Valid, true)],
            results);
        Assert.Equal(results.Count, results.Distinct().Count());
    }

    [Fact]
    public async Task Read_OfAnEmptyKey_IsTheMissingReading()
    {
        await using AppDbContext db = NewContext();

        LinkReading reading = await new LinkRecords(db, _time).ReadAsync(string.Empty, Ct);

        Assert.False(reading.Exists);
        Assert.Null(reading.State);
    }

    // ---- KK5: opening changes nothing; only the POST's consuming does ----

    [Fact]
    public async Task Read_OfEveryState_LeavesTheLinkTheAccountAndTheOtherRecordsUnchanged_AndOnlyTheConsumingWritesTheLink()
    {
        Customer customer = await SeedCustomerAsync();
        LinkRecord valid = NewLink(customer, expiresAt: Now.AddHours(1));
        LinkRecord expired = NewLink(customer, expiresAt: Now.AddMinutes(-1));
        LinkRecord used = NewLink(customer, expiresAt: Now.AddHours(1));
        used.UsedAt = Now.AddMinutes(-5);
        await SaveAsync(c => c.LinkRecords.AddRange(valid, expired, used));
        string before = await SnapshotAsync();

        await using (AppDbContext db = NewContext())
        {
            LinkRecords links = new(db, _time);
            foreach (string key in new[] { _keys.Derive(valid.Id), _keys.Derive(valid.Id), _keys.Derive(expired.Id), _keys.Derive(used.Id), SecretKey.Create() })
            {
                await links.ReadAsync(key, Ct);
            }

            Assert.Empty(db.ChangeTracker.Entries());
        }

        Assert.Equal(before, await SnapshotAsync());

        await using (AppDbContext db = NewContext())
        {
            Assert.True(await new LinkRecords(db, _time).TryConsumeAsync(valid.Id, Ct));
        }

        Assert.NotEqual(before, await SnapshotAsync());
        Assert.Equal(1L, await ScalarAsync<long>("SELECT count(*) FROM link_records WHERE used_at IS NOT NULL AND id = '" + valid.Id + "'"));
    }

    // ---- KK6: consuming is one conditional update ----

    [Fact]
    public async Task TryConsume_OfAValidLink_TakesItOnce_AndASecondConsumingChangesNoRow()
    {
        Customer customer = await SeedCustomerAsync();
        LinkRecord link = NewLink(customer, expiresAt: Now.AddHours(1));
        await SaveAsync(c => c.LinkRecords.Add(link));

        Assert.True(await ConsumeAsync(link.Id));
        string afterFirst = await SnapshotAsync();
        Assert.False(await ConsumeAsync(link.Id));

        Assert.Equal(afterFirst, await SnapshotAsync());
        Assert.Equal(Now.UtcDateTime, await ScalarAsync<DateTime>($"SELECT used_at FROM link_records WHERE id = '{link.Id}'"));
        Assert.Equal(LinkState.Used, (await ReadAsync(_keys.Derive(link.Id))).State);
    }

    [Fact]
    public async Task TryConsume_OfAnExpiredAUsedAndASupersededLinkAndOfNoRecord_ChangesNoRow()
    {
        Customer customer = await SeedCustomerAsync();
        LinkRecord expired = NewLink(customer, expiresAt: Now);
        LinkRecord used = NewLink(customer, expiresAt: Now.AddHours(1));
        used.UsedAt = Now.AddMinutes(-5);
        LinkRecord superseded = NewLink(customer, expiresAt: Now.AddHours(1));
        superseded.SupersededAt = Now.AddMinutes(-5);
        await SaveAsync(c => c.LinkRecords.AddRange(expired, used, superseded));
        string before = await SnapshotAsync();

        foreach (Guid id in new[] { expired.Id, used.Id, superseded.Id, Guid.CreateVersion7() })
        {
            Assert.False(await ConsumeAsync(id));
        }

        Assert.Equal(before, await SnapshotAsync());
    }

    [Fact]
    public async Task TryConsume_OfALinkWhoseLifetimeIsDrawnToNow_IsRefused()
    {
        // A withdrawn invitation or a link of a rotated secret has its lifetime drawn to "now" (05 6.4.6, 6.x.20): it is expired from that moment.
        Customer customer = await SeedCustomerAsync();
        LinkRecord link = NewLink(customer, expiresAt: Now.AddHours(1));
        await SaveAsync(c => c.LinkRecords.Add(link));
        await ExecuteAsync($"UPDATE link_records SET expires_at = '{Now:O}' WHERE id = '{link.Id}'");

        Assert.False(await ConsumeAsync(link.Id));
        Assert.Equal(LinkState.Expired, (await ReadAsync(_keys.Derive(link.Id))).State);
    }

    [Fact]
    public async Task TryConsume_TwoRequestsAtOnce_OneSucceedsAndOneDoesNot()
    {
        Customer customer = await SeedCustomerAsync();
        for (int round = 0; round < 15; round++)
        {
            LinkRecord link = NewLink(customer, expiresAt: Now.AddHours(1));
            await SaveAsync(c => c.LinkRecords.Add(link));
            using Barrier barrier = new(2);

            Task<bool> first = Task.Run(() => ConsumeAfterAsync(barrier, link.Id), Ct);
            Task<bool> second = Task.Run(() => ConsumeAfterAsync(barrier, link.Id), Ct);
            bool[] outcomes = await Task.WhenAll(first, second);

            Assert.Equal(1, outcomes.Count(o => o));
        }
    }

    [Fact]
    public async Task TryConsume_InsideTheTransactionRunner_JoinsTheCallersTransaction_AndARefusedResultUndoesIt()
    {
        Customer customer = await SeedCustomerAsync();
        LinkRecord link = NewLink(customer, expiresAt: Now.AddHours(1));
        await SaveAsync(c => c.LinkRecords.Add(link));
        await using AppDbContext db = NewContext();

        ConsumeResult result = await new TransactionRunner(db, new JobEnqueueTracker(), new JobSignal(), new CacheInvalidations(db), new AppCache()).RunAsync(
            async cancel => new ConsumeResult(await new LinkRecords(db, _time).TryConsumeAsync(link.Id, cancel), Rejected: true),
            Ct);

        Assert.True(result.Taken);
        Assert.Equal(0L, await ScalarAsync<long>($"SELECT count(*) FROM link_records WHERE used_at IS NOT NULL AND id = '{link.Id}'"));
        Assert.Equal(LinkState.Valid, (await ReadAsync(_keys.Derive(link.Id))).State);
    }

    // ---- The earlier links of a link asked for again (05 6.4.6) ----

    [Fact]
    public async Task SupersedeEarlier_TakesTheEarlierValidLinksOfTheSameTypeAndOwnerOnly()
    {
        Customer customer = await SeedCustomerAsync();
        Customer other = Customer.Create("Veli", "veli@example.com", "digest-placeholder", Now, null, Now);
        await SaveAsync(c => c.Customers.Add(other));
        LinkRecord earlier = NewLink(customer, expiresAt: Now.AddHours(1));
        LinkRecord earlierUsed = NewLink(customer, expiresAt: Now.AddHours(1));
        earlierUsed.UsedAt = Now.AddMinutes(-5);
        LinkRecord earlierExpired = NewLink(customer, expiresAt: Now.AddMinutes(-1));
        LinkRecord otherType = NewLink(customer, expiresAt: Now.AddHours(1), LinkType.EmailVerification);
        LinkRecord otherOwner = NewLink(other, expiresAt: Now.AddHours(1));
        LinkRecord newest = NewLink(customer, expiresAt: Now.AddHours(1));
        await SaveAsync(c => c.LinkRecords.AddRange(earlier, earlierUsed, earlierExpired, otherType, otherOwner, newest));

        int taken;
        await using (AppDbContext db = NewContext())
        {
            taken = await new LinkRecords(db, _time).SupersedeEarlierAsync(LinkType.PasswordReset, customer.Id, null, newest.Id, Ct);
        }

        Assert.Equal(1, taken);
        Assert.Equal(LinkState.Superseded, (await ReadAsync(_keys.Derive(earlier.Id))).State);
        Assert.Equal(LinkState.Used, (await ReadAsync(_keys.Derive(earlierUsed.Id))).State);
        Assert.Equal(LinkState.Expired, (await ReadAsync(_keys.Derive(earlierExpired.Id))).State);
        Assert.Equal(LinkState.Valid, (await ReadAsync(_keys.Derive(otherType.Id))).State);
        Assert.Equal(LinkState.Valid, (await ReadAsync(_keys.Derive(otherOwner.Id))).State);
        Assert.Equal(LinkState.Valid, (await ReadAsync(_keys.Derive(newest.Id))).State);
        Assert.False(await ConsumeAsync(earlier.Id));
    }

    // ---- Helpers ----

    private sealed record ConsumeResult(bool Taken, bool Rejected) : IOperationResult
    {
        public bool IsRejected => Rejected;
    }

    private LinkRecord NewLink(Customer customer, DateTimeOffset expiresAt, LinkType linkType = LinkType.PasswordReset) =>
        LinkRecord.ForCustomer(_keys, linkType, customer.Id, expiresAt, Now.AddHours(-2));

    private async Task<Customer> SeedCustomerAsync()
    {
        Customer customer = Customer.Create("Ali", "ali@example.com", "digest-placeholder", Now, null, Now);
        await SaveAsync(c => c.Customers.Add(customer));
        return customer;
    }

    private async Task<bool> ConsumeAsync(Guid linkId)
    {
        await using AppDbContext db = NewContext();
        return await new LinkRecords(db, _time).TryConsumeAsync(linkId, Ct);
    }

    private async Task<bool> ConsumeAfterAsync(Barrier barrier, Guid linkId)
    {
        await using AppDbContext db = NewContext();
        LinkRecords links = new(db, _time);
        barrier.SignalAndWait(Ct);
        return await links.TryConsumeAsync(linkId, Ct);
    }

    private async Task<LinkReading> ReadAsync(string key)
    {
        await using AppDbContext db = NewContext();
        return await new LinkRecords(db, _time).ReadAsync(key, Ct);
    }

    // The values and the row version of every row of the untouched tables, in one text.
    private async Task<string> SnapshotAsync()
    {
        List<string> parts = [];
        foreach (string table in UntouchedTables)
        {
            parts.Add(table + ":" + await ScalarAsync<string>($"SELECT coalesce(string_agg(t.xmin::text || to_jsonb(t)::text, '|' ORDER BY t.id), '') FROM {table} t"));
        }

        return string.Join('\n', parts);
    }

    // Every row of the link table, the dispatch table and the account tables as text: a key, if stored anywhere in them, shows.
    private async Task<string> WholeDatabaseTextAsync()
    {
        List<string> parts = [];
        foreach (string table in new[] { "link_records", "dispatch_records", "customers", "admins", "sessions" })
        {
            parts.Add(await ScalarAsync<string>($"SELECT coalesce(string_agg(to_jsonb(t)::text, '|'), '') FROM {table} t"));
        }

        return string.Join('\n', parts);
    }

    private AppDbContext NewContext()
    {
        DbContextOptionsBuilder<AppDbContext> builder = new();
        AppDbContext.Configure(builder, _collection.Dsn);
        return new AppDbContext(builder.Options);
    }

    private async Task SaveAsync(Action<AppDbContext> change)
    {
        await using AppDbContext context = NewContext();
        change(context);
        await context.SaveChangesAsync(Ct);
    }

    private async Task ExecuteAsync(string sql)
    {
        await using NpgsqlConnection connection = new(_collection.Dsn);
        await connection.OpenAsync(Ct);
        await using NpgsqlCommand command = new(sql, connection);
        await command.ExecuteNonQueryAsync(Ct);
    }

    private async Task<T> ScalarAsync<T>(string sql)
    {
        await using NpgsqlConnection connection = new(_collection.Dsn);
        await connection.OpenAsync(Ct);
        await using NpgsqlCommand command = new(sql, connection);
        return (T)(await command.ExecuteScalarAsync(Ct))!;
    }
}
