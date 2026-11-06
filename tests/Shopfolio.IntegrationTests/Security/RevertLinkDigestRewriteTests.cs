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
using Shopfolio.Modules.Notifications;
using Shopfolio.Modules.Notifications.Contracts;
using Shopfolio.UnitTests.Fakes;

namespace Shopfolio.IntegrationTests.Security;

/// <summary>
/// The start-up rewrite of the pending revert links' digests on a real PostgreSQL (T222 KK8; 05 6.x.20, 6.x.15; K-1137; K-1441): the digest of
/// the link record named by every "queued" <c>EmailChangeNotice</c> is written again from the present secret in one transaction, so the link the
/// body will carry matches its record; a sent notice's record, a link that is not a revert link, a notice of another type and a damaged value
/// list are left as they are, and the same secret changes no row. A row's change is seen from <c>xmin</c> as well as the values.
/// </summary>
[Collection(LinkRecordsDefinition.Name)]
public sealed class RevertLinkDigestRewriteTests : IAsyncLifetime
{
    private static readonly DateTimeOffset Now = new(2026, 11, 5, 9, 0, 0, TimeSpan.Zero);

    private readonly CollectionDatabaseFixture _collection;
    private readonly LinkKeys _oldKeys = NewKeys();
    private readonly LinkKeys _newKeys = NewKeys();

    public RevertLinkDigestRewriteTests(CollectionDatabaseFixture collection)
    {
        _collection = collection;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public async ValueTask InitializeAsync()
    {
        await ExecuteAsync("TRUNCATE link_records, sessions, dispatch_records, customers, admins CASCADE");
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    [Fact]
    public async Task Run_WithANewSecret_WritesTheDigestOfTheQueuedCustomerNoticeFromTheNewSecret_AndTheNewKeyOpensTheLink()
    {
        Customer customer = await SeedCustomerAsync();
        (LinkRecord link, DispatchRecord notice) = NewPendingRevert(customer.Id, null, DispatchRelatedRecordType.Customer);
        await SaveAsync(c => { c.LinkRecords.Add(link); c.DispatchRecords.Add(notice); });
        Assert.Equal(_oldKeys.DigestFor(link.Id), await DigestOfAsync(link.Id));

        RevertLinkDigestRewriteResult result = await RunAsync(_newKeys);

        Assert.Equal(1, result.Rewritten);
        Assert.Equal(_newKeys.DigestFor(link.Id), await DigestOfAsync(link.Id));
        Assert.NotEqual(_oldKeys.DigestFor(link.Id), await DigestOfAsync(link.Id));
        Assert.Equal(LinkState.Valid, (await ReadAsync(_newKeys.Derive(link.Id))).State);
        Assert.False((await ReadAsync(_oldKeys.Derive(link.Id))).Exists);
    }

    [Fact]
    public async Task Run_ForAnAdminAccount_RewritesTheDigestToo()
    {
        Admin admin = Admin.Create("Veli", "veli@example.com", "digest-placeholder", Now);
        await SaveAsync(c => c.Admins.Add(admin));
        (LinkRecord link, DispatchRecord notice) = NewPendingRevert(null, admin.Id, DispatchRelatedRecordType.Admin);
        await SaveAsync(c => { c.LinkRecords.Add(link); c.DispatchRecords.Add(notice); });

        RevertLinkDigestRewriteResult result = await RunAsync(_newKeys);

        Assert.Equal(1, result.Rewritten);
        Assert.Equal(_newKeys.DigestFor(link.Id), await DigestOfAsync(link.Id));
        Assert.Equal(LinkState.Valid, (await ReadAsync(_newKeys.Derive(link.Id))).State);
    }

    [Fact]
    public async Task Run_LeavesTheLinkOfASentNoticeAsItIs_RowVersionIncluded()
    {
        Customer customer = await SeedCustomerAsync();
        (LinkRecord link, DispatchRecord notice) = NewPendingRevert(customer.Id, null, DispatchRelatedRecordType.Customer);
        notice.DispatchState = DispatchState.Sent;
        notice.SentAt = Now;
        await SaveAsync(c => { c.LinkRecords.Add(link); c.DispatchRecords.Add(notice); });
        string before = await LinkSnapshotAsync();

        RevertLinkDigestRewriteResult result = await RunAsync(_newKeys);

        Assert.Equal(0, result.Rewritten);
        Assert.Equal(before, await LinkSnapshotAsync());
        Assert.Equal(_oldKeys.DigestFor(link.Id), await DigestOfAsync(link.Id));
    }

    [Fact]
    public async Task Run_LeavesAQueuedNoticeOfAnotherTypeAndALinkThatIsNotARevertLinkAndADamagedValueList()
    {
        Customer customer = await SeedCustomerAsync();

        // A queued notice of another type that names a revert link.
        (LinkRecord otherTypeLink, DispatchRecord otherTypeNotice) = NewPendingRevert(customer.Id, null, DispatchRelatedRecordType.Customer);
        otherTypeNotice.DispatchType = DispatchType.PasswordResetLink;

        // A queued revert notice that names a link that is not a revert link.
        LinkRecord resetLink = LinkRecord.ForCustomer(_oldKeys, LinkType.PasswordReset, customer.Id, Now.AddHours(1), Now);
        DispatchRecord resetNotice = DispatchRecord.Queue(DispatchType.EmailChangeNotice, "old@example.com", null, DispatchRelatedRecordType.Customer, customer.Id, Now);
        resetNotice.EventValues = LinkEventValues.ForLink(resetLink.Id);

        // Queued revert notices whose value lists name no link record.
        DispatchRecord[] damaged =
        [
            NoticeWith(customer.Id, "\"a json text, not a list\""),
            NoticeWith(customer.Id, "[1,\"x\",null]"),
            NoticeWith(customer.Id, "{\"Name\":\"LinkRecordId\"}"),
            NoticeWith(customer.Id, "[{\"Name\":\"LinkRecordId\",\"Value\":\"not-a-guid\"}]"),
            NoticeWith(customer.Id, "[{\"Name\":\"Other\",\"Value\":\"" + otherTypeLink.Id.ToString("D") + "\"}]"),
            NoticeWith(customer.Id, "[]"),
            NoticeWith(customer.Id, null),
        ];
        await SaveAsync(c =>
        {
            c.LinkRecords.AddRange(otherTypeLink, resetLink);
            c.DispatchRecords.AddRange(otherTypeNotice, resetNotice);
            c.DispatchRecords.AddRange(damaged);
        });
        string before = await LinkSnapshotAsync();

        RevertLinkDigestRewriteResult result = await RunAsync(_newKeys);

        Assert.Equal(0, result.Rewritten);
        Assert.Equal(before, await LinkSnapshotAsync());
    }

    [Fact]
    public async Task Run_WithTheSameSecret_RewritesNothing_AndNoRowVersionMoves()
    {
        Customer customer = await SeedCustomerAsync();
        (LinkRecord link, DispatchRecord notice) = NewPendingRevert(customer.Id, null, DispatchRelatedRecordType.Customer);
        await SaveAsync(c => { c.LinkRecords.Add(link); c.DispatchRecords.Add(notice); });
        string before = await LinkSnapshotAsync();

        RevertLinkDigestRewriteResult result = await RunAsync(_oldKeys);

        Assert.Equal(0, result.Rewritten);
        Assert.Equal(before, await LinkSnapshotAsync());
    }

    [Fact]
    public async Task Run_RunTwice_TheSecondRunRewritesNothing()
    {
        Customer customer = await SeedCustomerAsync();
        (LinkRecord link, DispatchRecord notice) = NewPendingRevert(customer.Id, null, DispatchRelatedRecordType.Customer);
        await SaveAsync(c => { c.LinkRecords.Add(link); c.DispatchRecords.Add(notice); });

        Assert.Equal(1, (await RunAsync(_newKeys)).Rewritten);
        string afterFirst = await LinkSnapshotAsync();

        Assert.Equal(0, (await RunAsync(_newKeys)).Rewritten);
        Assert.Equal(afterFirst, await LinkSnapshotAsync());
    }

    [Fact]
    public async Task Run_WithManyPendingNotices_IsOneTransaction_AndRewritesEach()
    {
        Customer customer = await SeedCustomerAsync();
        List<LinkRecord> links = [];
        for (int i = 0; i < 4; i++)
        {
            (LinkRecord link, DispatchRecord notice) = NewPendingRevert(customer.Id, null, DispatchRelatedRecordType.Customer);
            links.Add(link);
            await SaveAsync(c => { c.LinkRecords.Add(link); c.DispatchRecords.Add(notice); });
        }

        CountingRunner runner;
        RevertLinkDigestRewriteResult result;
        await using (AppDbContext db = NewContext())
        {
            runner = new CountingRunner(new TransactionRunner(db, new JobEnqueueTracker(), new JobSignal(), new CacheInvalidations(db), new AppCache()));
            result = await new RevertLinkDigestRewrite(db, runner, _newKeys).RewriteAsync(Ct);
        }

        Assert.Equal(1, runner.Calls);
        Assert.Equal(links.Count, result.Rewritten);
        foreach (LinkRecord link in links)
        {
            Assert.Equal(_newKeys.DigestFor(link.Id), await DigestOfAsync(link.Id));
        }
    }

    // ---- Helpers ----

    private sealed class CountingRunner : ITransactionRunner
    {
        private readonly ITransactionRunner _inner;

        public CountingRunner(ITransactionRunner inner)
        {
            _inner = inner;
        }

        public int Calls { get; private set; }

        public Task<T> RunAsync<T>(Func<CancellationToken, Task<T>> work, IReadOnlyCollection<string> retryOnUniqueIndexes, CancellationToken ct)
            where T : IOperationResult
        {
            Calls++;
            return _inner.RunAsync(work, retryOnUniqueIndexes, ct);
        }

        public Task<T> RunAsync<T>(Func<CancellationToken, Task<T>> work, CancellationToken ct)
            where T : IOperationResult
        {
            Calls++;
            return _inner.RunAsync(work, ct);
        }
    }

    private static LinkKeys NewKeys() =>
        new(Options.Create(new LinkKeyOptions { DerivationSecret = Convert.ToBase64String(RandomNumberGenerator.GetBytes(LinkKeyOptions.DerivationSecretMinBytes)) }));

    private static DispatchRecord NoticeWith(Guid customerId, string? eventValues)
    {
        DispatchRecord notice = DispatchRecord.Queue(DispatchType.EmailChangeNotice, "old@example.com", null, DispatchRelatedRecordType.Customer, customerId, Now);
        notice.EventValues = eventValues;
        return notice;
    }

    // A revert link made under the old secret and the queued notice that names it, as the e-mail change will write them (05 6.x.15).
    private (LinkRecord Link, DispatchRecord Notice) NewPendingRevert(Guid? customerId, Guid? adminId, DispatchRelatedRecordType owner)
    {
        LinkRecord link = customerId is { } c
            ? LinkRecord.ForCustomer(_oldKeys, LinkType.EmailChangeRevert, c, Now.AddDays(7), Now)
            : LinkRecord.ForAdmin(_oldKeys, LinkType.EmailChangeRevert, adminId!.Value, Now.AddDays(7), Now);
        link.PreviousEmail = "old@example.com";
        DispatchRecord notice = DispatchRecord.Queue(DispatchType.EmailChangeNotice, "old@example.com", null, owner, customerId ?? adminId, Now);
        notice.EventValues = LinkEventValues.ForLink(link.Id);
        return (link, notice);
    }

    private async Task<RevertLinkDigestRewriteResult> RunAsync(LinkKeys keys)
    {
        await using AppDbContext db = NewContext();
        return await new RevertLinkDigestRewrite(db, new TransactionRunner(db, new JobEnqueueTracker(), new JobSignal(), new CacheInvalidations(db), new AppCache()), keys).RewriteAsync(Ct);
    }

    private async Task<LinkReading> ReadAsync(string key)
    {
        await using AppDbContext db = NewContext();
        return await new LinkRecords(db, new ManualTimeProvider(Now)).ReadAsync(key, Ct);
    }

    private async Task<Customer> SeedCustomerAsync()
    {
        Customer customer = Customer.Create("Ali", "ali@example.com", "digest-placeholder", Now, null, Now);
        await SaveAsync(c => c.Customers.Add(customer));
        return customer;
    }

    private Task<string> DigestOfAsync(Guid linkId) => ScalarAsync<string>($"SELECT token_hash FROM link_records WHERE id = '{linkId}'");

    // The values and the row version of every link row.
    private Task<string> LinkSnapshotAsync() =>
        ScalarAsync<string>("SELECT coalesce(string_agg(t.xmin::text || to_jsonb(t)::text, '|' ORDER BY t.id), '') FROM link_records t");

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
