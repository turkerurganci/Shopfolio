using Microsoft.EntityFrameworkCore;

using Npgsql;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.EmailComposition;
using Shopfolio.Infrastructure.InstallationSettings;
using Shopfolio.Infrastructure.Jobs;
using Shopfolio.Infrastructure.SearchEngine;
using Shopfolio.IntegrationTests.Database;
using Shopfolio.Modules.Administration;
using Shopfolio.Modules.CorporateContent.Contracts;
using Shopfolio.Modules.Notifications;
using Shopfolio.Modules.Notifications.Contracts;
using Shopfolio.Modules.Payment.Contracts;
using Shopfolio.UnitTests.Fakes;

namespace Shopfolio.IntegrationTests.Model;

/// <summary>The collection of the firm notice queue tests (T308): one database copied from the template, made by the real migrations.</summary>
[CollectionDefinition(CompanyNoticeQueueDefinition.Name)]
public sealed class CompanyNoticeQueueDefinition : ICollectionFixture<CollectionDatabaseFixture>
{
    public const string Name = "CompanyNoticeQueue";
}

/// <summary>
/// The dispatch rows of the firm notices F-1…F-6 on a real PostgreSQL, written as the application user (T308 criteria 1, 3, 4, 9, 10; KK1, KK3, KK4, KK9, KK10; 08 2.2.7;
/// 06 4.58): the row is written in the caller's transaction and goes with it, F-1…F-4 are one row to the firm's contact e-mail, F-5 and F-6 one separate row for each
/// administrator address, the rows satisfy the table's type and related record rule, carry the event's values (and for F-5 the last four digits only), have no order
/// link and so never put the order's mark, and the body is made from the stored row after the administrator it names is gone.
/// </summary>
[Collection(CompanyNoticeQueueDefinition.Name)]
public sealed class CompanyNoticeQueueTests : IAsyncLifetime
{
    private static readonly DateTimeOffset Now = new(2026, 11, 5, 9, 0, 0, TimeSpan.Zero);
    private static readonly Guid OrderId = Guid.Parse("018f0000-0000-7000-8000-0000000000a1");
    private static readonly Guid RecordId = Guid.Parse("018f0000-0000-7000-8000-0000000000b2");

    private const string WholeIban = "TR330006100519786457841326";

    private readonly TemplateDatabaseFixture _template;
    private readonly CollectionDatabaseFixture _collection;
    private readonly ManualTimeProvider _time = new(Now);

    public CompanyNoticeQueueTests(TemplateDatabaseFixture template, CollectionDatabaseFixture collection)
    {
        _template = template;
        _collection = collection;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private string AppDsn => _template.DsnFor(_collection.DatabaseName, _template.Users.App, _template.Users.AppPassword, pooling: false);

    public async ValueTask InitializeAsync()
    {
        await ExecuteAsync("TRUNCATE dispatch_records, admins CASCADE");
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    // ---- the row is written in the caller's transaction ----

    [Fact]
    public async Task F1_IsOneQueuedRowToTheFirmEmail_WithTheRelatedOrderAndTheEventsValues_AndNoOrderLink()
    {
        int written = await RunAsync(queue => queue.QueueCompanyAsync(new NewOrderNotice(OrderId, "SP-1001", PaymentMethod.Card, 1200m, Now), Ct));

        Assert.Equal(1, written);
        DispatchRecord row = await ReadSingleAsync();
        Assert.Equal(DispatchType.F1, row.DispatchType);
        Assert.Equal("firma@example.com", row.RecipientEmail);
        Assert.Null(row.OrderId);
        Assert.Equal(DispatchRelatedRecordType.Order, row.RelatedRecordType);
        Assert.Equal(OrderId, row.RelatedRecordId);
        Assert.Equal(DispatchState.Queued, row.DispatchState);
        Assert.Equal(Now, row.CreatedAt);
        Assert.Contains(OrderId.ToString("D"), row.EventValues, StringComparison.Ordinal);
        Assert.Contains("SP-1001", row.EventValues, StringComparison.Ordinal);
    }

    [Fact]
    public async Task TheRow_GoesWithTheTransaction_WhenTheEventIsRefused()
    {
        await RunAsync(queue => queue.QueueCompanyAsync(new NewOrderNotice(OrderId, "SP-1001", PaymentMethod.Card, 1200m, Now), Ct), rejected: true);

        Assert.Equal(0, await CountAsync());
    }

    [Fact]
    public async Task WithoutATransaction_NoRowIsWritten()
    {
        await using AppDbContext db = NewContext();
        CompanyNoticeQueue queue = NewQueue(db, "firma@example.com");

        await Assert.ThrowsAsync<InvalidOperationException>(() => queue.QueueCompanyAsync(new NewContactRequestNotice(RecordId, ContactTopic.Other, Now), Ct));
        Assert.Throws<InvalidOperationException>(() => queue.QueueAdmins(new AdminRemovedNotice(RecordId, "Ali", "Veli", Now), ["ali@example.com"]));
        Assert.Equal(0, await CountAsync());
    }

    [Fact]
    public async Task WhileTheFirmHasNoContactEmail_NoRowIsWritten()
    {
        int written = await RunAsync(queue => queue.QueueCompanyAsync(new NewContactRequestNotice(RecordId, ContactTopic.Other, Now), Ct), contactEmail: null);

        Assert.Equal(0, written);
        Assert.Equal(0, await CountAsync());
    }

    [Fact]
    public async Task EveryFirmTypeF1ToF4_SatisfiesTheTablesRuleOfTypeAndRelatedRecord()
    {
        CompanyNotice[] notices =
        [
            new NewOrderNotice(OrderId, "SP-1", PaymentMethod.BankTransfer, 10m, Now),
            new NewContactRequestNotice(RecordId, ContactTopic.PersonalDataRequest, Now),
            new NewClaimNotice(DispatchRelatedRecordType.Withdrawal, RecordId, OrderId, "SP-1", false, Now),
            new NewClaimNotice(DispatchRelatedRecordType.LateDeliveryTermination, RecordId, OrderId, "SP-1", false, Now),
            new NewClaimNotice(DispatchRelatedRecordType.DefectClaim, RecordId, OrderId, "SP-1", true, Now),
            new CardRefundNotice(RecordId, OrderId, "SP-1", RefundType.LatePayment, 10m, Now),
        ];

        foreach (CompanyNotice notice in notices)
        {
            await RunAsync(queue => queue.QueueCompanyAsync(notice, Ct));
        }

        Assert.Equal(notices.Length, await CountAsync());
    }

    // ---- KK1: F-5 and F-6 write a separate row for every administrator ----

    [Fact]
    public async Task F5_WritesOneSeparateRowForEveryAdministratorAddress_WithTheLastFourDigitsOnly()
    {
        string[] admins = ["ali@example.com", "veli@example.com", "ayse@example.com"];

        int written = await RunAsync(queue => Task.FromResult(queue.QueueAdmins(IbanChangedNotice.For(WholeIban, "Ali Yılmaz", false, Now), admins)));

        Assert.Equal(3, written);
        List<DispatchRecord> rows = await ReadAllAsync();
        Assert.Equal(3, rows.Count);
        Assert.Equal(admins.Order(StringComparer.Ordinal), rows.Select(r => r.RecipientEmail!).Order(StringComparer.Ordinal));
        Assert.All(rows, r =>
        {
            Assert.Equal(DispatchType.F5, r.DispatchType);
            Assert.Null(r.OrderId);
            Assert.Null(r.RelatedRecordType);
            Assert.Null(r.RelatedRecordId);
            Assert.NotEqual("firma@example.com", r.RecipientEmail);
        });
        Assert.Equal(3, rows.Select(r => r.Id).Distinct().Count());
    }

    [Fact]
    public async Task F5_NeverWritesTheWholeIbanToTheRow_AndKeepsTheFourDigits()
    {
        await RunAsync(queue => Task.FromResult(queue.QueueAdmins(IbanChangedNotice.For("TR33 0006 1005 1978 6457 8413 26", "Ali Yılmaz", true, Now), ["ali@example.com"])));

        DispatchRecord row = await ReadSingleAsync();
        string everything = row.EventValues + row.RecipientEmail;
        Assert.DoesNotContain("TR33", everything, StringComparison.Ordinal);
        Assert.DoesNotContain("0006", everything, StringComparison.Ordinal);
        Assert.DoesNotContain("5197", everything, StringComparison.Ordinal);
        Assert.DoesNotContain("78413", everything, StringComparison.Ordinal);
        Assert.Contains("1326", row.EventValues, StringComparison.Ordinal);
    }

    [Fact]
    public async Task TheSameAddressTwice_IsOneRow_AndNoAddressIsAnError()
    {
        int written = await RunAsync(queue => Task.FromResult(queue.QueueAdmins(new AdminInvitedNotice(RecordId, "Ali", "yeni@example.com", Now), ["ali@example.com", "ALI@example.com"])));

        Assert.Equal(1, written);
        await Assert.ThrowsAsync<ArgumentException>(() => RunAsync(queue => Task.FromResult(queue.QueueAdmins(new AdminInvitedNotice(RecordId, "Ali", "yeni@example.com", Now), []))));
        await Assert.ThrowsAsync<ArgumentException>(() => RunAsync(queue => Task.FromResult(queue.QueueAdmins(new AdminInvitedNotice(RecordId, "Ali", "yeni@example.com", Now), [" "]))));
    }

    [Fact]
    public async Task F6_RowsCarryTheInvitationOrTheRemovedAdministrator_AsTheRelatedRecord()
    {
        await RunAsync(queue => Task.FromResult(queue.QueueAdmins(new AdminInvitedNotice(RecordId, "Ali", "yeni@example.com", Now), ["ali@example.com"])));
        await RunAsync(queue => Task.FromResult(queue.QueueAdmins(new AdminRemovedNotice(OrderId, "Ali", "Veli", Now), ["ali@example.com", "veli@example.com"])));

        List<DispatchRecord> rows = await ReadAllAsync();
        Assert.Equal(3, rows.Count);
        Assert.Single(rows, r => r.RelatedRecordType == DispatchRelatedRecordType.AdminInvitation && r.RelatedRecordId == RecordId);
        Assert.Equal(2, rows.Count(r => r.RelatedRecordType == DispatchRelatedRecordType.Admin && r.RelatedRecordId == OrderId));
    }

    // ---- KK10: the values are the row's, so a removed administrator changes nothing ----

    [Fact]
    public async Task F6_RemovalBody_IsMadeFromTheRow_AfterTheRemovedAdministratorIsDeleted()
    {
        Admin removed = await SeedAdminAsync("Veli Demir", "veli@example.com");
        await RunAsync(queue => Task.FromResult(queue.QueueAdmins(new AdminRemovedNotice(removed.Id, "Ali Yılmaz", removed.Name, Now), ["ali@example.com", removed.Email])));
        await ExecuteAsync("DELETE FROM admins");

        List<DispatchRecord> rows = await ReadAllAsync();

        Assert.Equal(2, rows.Count);
        Assert.Equal(0, await ExecuteScalarAsync("SELECT count(*) FROM admins"));
        foreach (DispatchRecord row in rows)
        {
            EmailContent content = CompanyNoticeTemplates.Compose(row.DispatchType, row.EventValues, Links())!;
            Assert.Contains(content.Blocks.OfType<EmailParagraph>(), p => p.Text.Contains("Ali Yılmaz, Veli Demir adlı yöneticiyi kaldırdı.", StringComparison.Ordinal));
        }
    }

    // ---- KK4: an undelivered firm notice puts no mark on any order ----

    [Fact]
    public async Task AFirmNoticeThatEndsUndelivered_PutsNoMarkOnTheOrderItsRelatedTo()
    {
        await RunAsync(queue => queue.QueueCompanyAsync(new NewOrderNotice(OrderId, "SP-1001", PaymentMethod.Card, 1200m, Now), Ct));
        await ExecuteAsync("UPDATE dispatch_records SET dispatch_state = 'Undelivered', next_attempt_at = NULL, attempt_count = 4, last_outcome = 'ChannelError'");

        await using AppDbContext db = NewContext();
        Assert.False(await DispatchMarks.HasMarkAsync(db, OrderId, Ct));
        Assert.Equal(DispatchState.Undelivered, (await ReadSingleAsync()).DispatchState);
    }

    // ---- helpers ----

    private static EmailLinks Links() => new(new SiteAddress(Microsoft.Extensions.Options.Options.Create(new SiteOptions { Domain = "magaza.example.com" })));

    private static TransactionRunner NewRunner(AppDbContext db) => new(db, new JobEnqueueTracker(), new JobSignal(), new CacheInvalidations(db), new AppCache());

    private CompanyNoticeQueue NewQueue(AppDbContext db, string? contactEmail) => new(db, new FixedContactEmail(contactEmail), _time);

    private async Task<int> RunAsync(Func<CompanyNoticeQueue, Task<int>> work, bool rejected = false, string? contactEmail = "firma@example.com")
    {
        await using AppDbContext db = NewContext();
        CompanyNoticeQueue queue = NewQueue(db, contactEmail);
        int written = 0;
        await NewRunner(db).RunAsync(
            async cancel =>
            {
                written = await work(queue);
                return new Outcome(rejected);
            },
            Ct);
        return written;
    }

    private AppDbContext NewContext()
    {
        DbContextOptionsBuilder<AppDbContext> builder = new();
        AppDbContext.Configure(builder, AppDsn);
        return new AppDbContext(builder.Options);
    }

    private AppDbContext NewOwnerContext()
    {
        DbContextOptionsBuilder<AppDbContext> builder = new();
        AppDbContext.Configure(builder, _collection.Dsn);
        return new AppDbContext(builder.Options);
    }

    private async Task<DispatchRecord> ReadSingleAsync()
    {
        await using AppDbContext db = NewContext();
        return await db.DispatchRecords.AsNoTracking().SingleAsync(Ct);
    }

    private async Task<List<DispatchRecord>> ReadAllAsync()
    {
        await using AppDbContext db = NewContext();
        return await db.DispatchRecords.AsNoTracking().OrderBy(d => d.RecipientEmail).ToListAsync(Ct);
    }

    private async Task<int> CountAsync()
    {
        await using AppDbContext db = NewContext();
        return await db.DispatchRecords.CountAsync(Ct);
    }

    private async Task<Admin> SeedAdminAsync(string name, string email)
    {
        await using AppDbContext db = NewOwnerContext();
        Admin admin = Admin.Create(name, email, "hash", Now);
        db.Admins.Add(admin);
        await db.SaveChangesAsync(Ct);
        return admin;
    }

    private async Task ExecuteAsync(string sql)
    {
        await using NpgsqlConnection connection = new(_collection.Dsn);
        await connection.OpenAsync(Ct);
        await using NpgsqlCommand command = new(sql, connection);
        await command.ExecuteNonQueryAsync(Ct);
    }

    private async Task<long> ExecuteScalarAsync(string sql)
    {
        await using NpgsqlConnection connection = new(_collection.Dsn);
        await connection.OpenAsync(Ct);
        await using NpgsqlCommand command = new(sql, connection);
        return Convert.ToInt64(await command.ExecuteScalarAsync(Ct), System.Globalization.CultureInfo.InvariantCulture);
    }

    private sealed class FixedContactEmail : ICompanyContactEmail
    {
        private readonly string? _email;

        public FixedContactEmail(string? email) => _email = email;

        public Task<string?> GetAsync(CancellationToken ct) => Task.FromResult(_email);
    }

    private sealed record Outcome(bool Rejected) : IOperationResult
    {
        public bool IsRejected => Rejected;
    }
}
