using Microsoft.EntityFrameworkCore;

using Npgsql;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Jobs;
using Shopfolio.IntegrationTests.Database;
using Shopfolio.Modules.CorporateContent;
using Shopfolio.Modules.CorporateContent.Contracts;
using Shopfolio.Modules.Notifications;
using Shopfolio.Modules.Notifications.Contracts;
using Shopfolio.UnitTests.Fakes;

namespace Shopfolio.IntegrationTests.CorporateContent;

/// <summary>The collection of the contact request status tests (T322): one database copied from the template.</summary>
[CollectionDefinition(ContactRequestStatusDefinition.Name)]
public sealed class ContactRequestStatusDefinition : ICollectionFixture<CollectionDatabaseFixture>
{
    public const string Name = "ContactRequestStatus";
}

/// <summary>
/// The contact request's two statuses on a real PostgreSQL, written as the application user (T322 criteria 7-9; KK7-KK9; 02 5.13; 03 1.9.2; Z-33): the request
/// is born open and the notice F-2 goes to the firm in the same transaction; only an administrator closes and reopens it, with no notice; the retention
/// runs from the last closing, from the opening of a request that was never closed, and keeps its old closing while a closed request stands reopened.
/// </summary>
[Collection(ContactRequestStatusDefinition.Name)]
public sealed class ContactRequestStatusTests : IAsyncLifetime
{
    private static readonly DateTimeOffset Start = new(2026, 11, 1, 9, 0, 0, TimeSpan.Zero);

    private static readonly Guid AdminId = Guid.Parse("018f0000-0000-7000-8000-0000000000d4");

    private readonly TemplateDatabaseFixture _template;
    private readonly CollectionDatabaseFixture _collection;
    private readonly ManualTimeProvider _time = new(Start);

    public ContactRequestStatusTests(TemplateDatabaseFixture template, CollectionDatabaseFixture collection)
    {
        _template = template;
        _collection = collection;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private string AppDsn => _template.DsnFor(_collection.DatabaseName, _template.Users.App, _template.Users.AppPassword, pooling: false);

    public async ValueTask InitializeAsync()
    {
        await ExecuteAsync("TRUNCATE contact_requests, dispatch_records CASCADE");
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    // ---- KK8: born open, F-2 to the firm ----

    [Fact]
    public async Task ARequest_IsBornOpen_AndTheNoticeF2GoesToTheFirmInTheSameTransaction_KK8()
    {
        Guid id = (await RunAsync((requests, _) => requests.OpenAsync(Form(), Ct))).RequestId!.Value;

        Assert.Equal("Open", await ScalarAsync<string>($"SELECT contact_request_status FROM contact_requests WHERE id = '{id}'"));
        Assert.Equal(Start, await ReadRetentionStartAsync(id));
        DispatchRecord row = await ReadSingleDispatchAsync();
        Assert.Equal(DispatchType.F2, row.DispatchType);
        Assert.Equal("firma@example.com", row.RecipientEmail);
        Assert.Equal(DispatchRelatedRecordType.ContactRequest, row.RelatedRecordType);
        Assert.Equal(id, row.RelatedRecordId);
        Assert.Equal(DispatchState.Queued, row.DispatchState);
    }

    [Fact]
    public async Task TheNoticeF2_CarriesTheTopicOnly_NotTheSendersNameEmailPhoneOrMessage_KK8()
    {
        await RunAsync((requests, _) => requests.OpenAsync(Form(), Ct));

        DispatchRecord row = await ReadSingleDispatchAsync();

        Assert.Contains(nameof(ContactTopic.GeneralQuestion), row.EventValues, StringComparison.Ordinal);
        Assert.DoesNotContain("Ayşe", row.EventValues, StringComparison.Ordinal);
        Assert.DoesNotContain("ayse@example.com", row.EventValues, StringComparison.Ordinal);
        Assert.DoesNotContain("5321234567", row.EventValues, StringComparison.Ordinal);
        Assert.DoesNotContain("özel mesaj", row.EventValues, StringComparison.Ordinal);
    }

    [Fact]
    public async Task TheRequestAndItsNotice_GoTogether_WhenTheTransactionIsRolledBack_KK8()
    {
        await RunAsync((requests, _) => requests.OpenAsync(Form(), Ct), rollBack: true);

        Assert.Equal(0L, await ScalarAsync<long>("SELECT count(*) FROM contact_requests"));
        Assert.Equal(0L, await ScalarAsync<long>("SELECT count(*) FROM dispatch_records"));
    }

    [Fact]
    public async Task Close_AndReopen_ProduceNoNotice_AndTheFirmHearsOnlyOfTheBirth_KK8()
    {
        Guid id = (await RunAsync((requests, _) => requests.OpenAsync(Form(), Ct))).RequestId!.Value;

        await RunAsync((requests, _) => requests.CloseAsync(id, AdminId, Ct));
        await RunAsync((requests, _) => requests.ReopenAsync(id, AdminId, Ct));
        await RunAsync((requests, _) => requests.CloseAsync(id, AdminId, Ct));

        Assert.Equal(1L, await ScalarAsync<long>("SELECT count(*) FROM dispatch_records"));
        Assert.Equal(DispatchType.F2, (await ReadSingleDispatchAsync()).DispatchType);
    }

    // ---- KK8, KK7: only the administrator closes and reopens; two statuses ----

    [Fact]
    public async Task Close_WritesTheClosingMoment_AndASecondCloseChangesNothing_KK8()
    {
        Guid id = (await RunAsync((requests, _) => requests.OpenAsync(Form(), Ct))).RequestId!.Value;
        _time.Advance(TimeSpan.FromDays(2));
        DateTimeOffset closedAt = _time.GetUtcNow();

        ContactRequestResult first = await RunAsync((requests, _) => requests.CloseAsync(id, AdminId, Ct));
        _time.Advance(TimeSpan.FromDays(1));
        ContactRequestResult second = await RunAsync((requests, _) => requests.CloseAsync(id, AdminId, Ct));

        Assert.Equal(ContactRequestOutcome.Done, first.Outcome);
        Assert.Equal(ContactRequestOutcome.Unchanged, second.Outcome);
        Assert.False(second.IsRejected);
        Assert.Equal("Closed", await ScalarAsync<string>($"SELECT contact_request_status FROM contact_requests WHERE id = '{id}'"));
        Assert.Equal(closedAt, await ReadRetentionStartAsync(id));
    }

    [Fact]
    public async Task Reopen_OpensAClosedRequest_WithoutEmptyingTheClosingMoment_AndAnOpenOneStaysAsItIs_KK8()
    {
        Guid id = (await RunAsync((requests, _) => requests.OpenAsync(Form(), Ct))).RequestId!.Value;
        _time.Advance(TimeSpan.FromDays(2));
        DateTimeOffset closedAt = _time.GetUtcNow();
        await RunAsync((requests, _) => requests.CloseAsync(id, AdminId, Ct));
        _time.Advance(TimeSpan.FromDays(3));

        ContactRequestResult reopened = await RunAsync((requests, _) => requests.ReopenAsync(id, AdminId, Ct));
        ContactRequestResult again = await RunAsync((requests, _) => requests.ReopenAsync(id, AdminId, Ct));

        Assert.Equal(ContactRequestOutcome.Done, reopened.Outcome);
        Assert.Equal(ContactRequestOutcome.Unchanged, again.Outcome);
        Assert.Equal("Open", await ScalarAsync<string>($"SELECT contact_request_status FROM contact_requests WHERE id = '{id}'"));
        Assert.Equal(1L, await ScalarAsync<long>($"SELECT count(*) FROM contact_requests WHERE id = '{id}' AND last_closed_at IS NOT NULL"));
        Assert.Equal(closedAt, await ReadRetentionStartAsync(id));
    }

    [Fact]
    public async Task AMissingRequest_IsNotFound_ForCloseAndReopenAlike_KK8()
    {
        ContactRequestResult close = await RunAsync((requests, _) => requests.CloseAsync(Guid.CreateVersion7(), AdminId, Ct));
        ContactRequestResult reopen = await RunAsync((requests, _) => requests.ReopenAsync(Guid.CreateVersion7(), AdminId, Ct));

        Assert.Equal(ContactRequestOutcome.NotFound, close.Outcome);
        Assert.Equal(ContactRequestOutcome.NotFound, reopen.Outcome);
        Assert.True(close.IsRejected);
    }

    [Fact]
    public async Task ACallWithoutAnAdministratorsIdentity_IsRefused_BeforeAnythingIsWritten_KK8()
    {
        Guid id = (await RunAsync((requests, _) => requests.OpenAsync(Form(), Ct))).RequestId!.Value;

        await Assert.ThrowsAsync<ArgumentException>(() => requests(id, closing: true));
        await Assert.ThrowsAsync<ArgumentException>(() => requests(id, closing: false));

        Assert.Equal("Open", await ScalarAsync<string>($"SELECT contact_request_status FROM contact_requests WHERE id = '{id}'"));

        Task<ContactRequestResult> requests(Guid requestId, bool closing) =>
            RunAsync((r, _) => closing ? r.CloseAsync(requestId, Guid.Empty, Ct) : r.ReopenAsync(requestId, Guid.Empty, Ct));
    }

    [Fact]
    public async Task TwoAdministratorsClosingTogether_WriteOnce_KK8()
    {
        Guid id = (await RunAsync((requests, _) => requests.OpenAsync(Form(), Ct))).RequestId!.Value;

        ContactRequestResult[] results = await Task.WhenAll(
            RunAsync((requests, _) => requests.CloseAsync(id, AdminId, Ct)),
            RunAsync((requests, _) => requests.CloseAsync(id, AdminId, Ct)));

        Assert.Single(results, r => r.Outcome == ContactRequestOutcome.Done);
        Assert.Single(results, r => r.Outcome == ContactRequestOutcome.Unchanged);
    }

    [Fact]
    public async Task TheStatusColumn_AcceptsOnlyOpenAndClosed_AndNoOtherStateOrCounterColumnExists_KK7()
    {
        string definition = await ScalarAsync<string>(
            "SELECT pg_get_constraintdef(c.oid) FROM pg_constraint c JOIN pg_class t ON t.oid = c.conrelid " +
            "WHERE t.relname = 'contact_requests' AND c.contype = 'c' AND pg_get_constraintdef(c.oid) LIKE '%contact_request_status%' " +
            "AND pg_get_constraintdef(c.oid) LIKE '%ANY%'");
        long extraColumns = await ScalarAsync<long>(
            "SELECT count(*) FROM information_schema.columns WHERE table_name = 'contact_requests' AND " +
            "(column_name ~ '(answered|deadline|due|sla|timer|counter)')");

        foreach (ContactRequestStatus value in Enum.GetValues<ContactRequestStatus>())
        {
            Assert.Contains($"'{value}'", definition, StringComparison.Ordinal);
        }

        Assert.Equal(Enum.GetValues<ContactRequestStatus>().Length, definition.Split('\'').Length / 2);
        Assert.Equal(0L, extraColumns);
    }

    // ---- KK9: the retention start ----

    [Fact]
    public async Task TheRetention_RunsFromTheLastClosing_AReopenedAndClosedRequestFromTheNewOne_AnOpenReopenedFromTheOldOne_KK9()
    {
        Guid neverClosed = (await RunAsync((requests, _) => requests.OpenAsync(Form(), Ct))).RequestId!.Value;
        Guid reclosed = (await RunAsync((requests, _) => requests.OpenAsync(Form(), Ct))).RequestId!.Value;
        Guid reopened = (await RunAsync((requests, _) => requests.OpenAsync(Form(), Ct))).RequestId!.Value;

        _time.Advance(TimeSpan.FromDays(2));
        DateTimeOffset firstClosing = _time.GetUtcNow();
        await RunAsync((requests, _) => requests.CloseAsync(reclosed, AdminId, Ct));
        await RunAsync((requests, _) => requests.CloseAsync(reopened, AdminId, Ct));

        _time.Advance(TimeSpan.FromDays(5));
        await RunAsync((requests, _) => requests.ReopenAsync(reclosed, AdminId, Ct));
        await RunAsync((requests, _) => requests.ReopenAsync(reopened, AdminId, Ct));

        _time.Advance(TimeSpan.FromDays(4));
        DateTimeOffset secondClosing = _time.GetUtcNow();
        await RunAsync((requests, _) => requests.CloseAsync(reclosed, AdminId, Ct));

        Assert.Equal(Start, await ReadRetentionStartAsync(neverClosed));
        Assert.Equal(secondClosing, await ReadRetentionStartAsync(reclosed));
        Assert.Equal(firstClosing, await ReadRetentionStartAsync(reopened));
        Assert.Equal("Open", await ScalarAsync<string>($"SELECT contact_request_status FROM contact_requests WHERE id = '{reopened}'"));
    }

    // ---- helpers ----

    private static NewContactRequest Form() =>
        new("Ayşe", "ayse@example.com", "5321234567", ContactTopic.GeneralQuestion, "Merhaba, özel mesaj");

    private AppDbContext NewContext()
    {
        DbContextOptionsBuilder<AppDbContext> builder = new();
        AppDbContext.Configure(builder, AppDsn);
        return new AppDbContext(builder.Options);
    }

    private async Task<ContactRequestResult> RunAsync(Func<ContactRequests, AppDbContext, Task<ContactRequestResult>> work, bool rollBack = false)
    {
        await using AppDbContext db = NewContext();
        CacheInvalidations invalidations = new(db);
        ContactRequests requests = new(db, new NotificationsStandIn(new CompanyNoticeQueue(db, new FixedContactEmail("firma@example.com"), _time)), _time);
        TransactionRunner runner = new(db, new JobEnqueueTracker(), new JobSignal(), invalidations, new AppCache());
        ContactRequestResult result = default!;
        await runner.RunAsync(
            async cancel =>
            {
                result = await work(requests, db);
                return new Outcome(rollBack);
            },
            Ct);
        return result;
    }

    private async Task<DateTimeOffset> ReadRetentionStartAsync(Guid id)
    {
        await using AppDbContext db = NewContext();
        ContactRequest request = await db.ContactRequests.AsNoTracking().SingleAsync(c => c.Id == id, Ct);
        return request.RetentionStart;
    }

    private async Task<DispatchRecord> ReadSingleDispatchAsync()
    {
        await using AppDbContext db = NewContext();
        return await db.DispatchRecords.AsNoTracking().SingleAsync(Ct);
    }

    private async Task<T> ScalarAsync<T>(string sql)
    {
        await using NpgsqlConnection connection = new(_collection.Dsn);
        await connection.OpenAsync(Ct);
        await using NpgsqlCommand command = new(sql, connection);
        return (T)(await command.ExecuteScalarAsync(Ct))!;
    }

    private async Task ExecuteAsync(string sql)
    {
        await using NpgsqlConnection connection = new(_collection.Dsn);
        await connection.OpenAsync(Ct);
        await using NpgsqlCommand command = new(sql, connection);
        await command.ExecuteNonQueryAsync(Ct);
    }

    /// <summary>The Notifications module reduced to the firm notice the contact request uses: it is the real queue, so the real dispatch row is written.</summary>
    private sealed class NotificationsStandIn : INotificationsModule
    {
        private readonly CompanyNoticeQueue _queue;

        public NotificationsStandIn(CompanyNoticeQueue queue) => _queue = queue;

        public Task<int> QueueCompanyNoticeAsync(CompanyNotice notice, CancellationToken ct) => _queue.QueueCompanyAsync(notice, ct);

        public Task<int> QueueCustomerNoticeAsync(CustomerNotice notice, string recipientEmail, CancellationToken ct) => throw new NotSupportedException();

        public Task<int> QueueAdminNoticeAsync(AdminNotice notice, IReadOnlyCollection<string> adminEmails, CancellationToken ct) => throw new NotSupportedException();

        public Task<int> QueueAccountEmailAsync(AccountEmail email, CancellationToken ct) => throw new NotSupportedException();

        public Task<bool> QueueGoogleLinkConfirmationAsync(Guid customerId, string email, CancellationToken ct) => throw new NotSupportedException();

        public Task<bool> OrderHasUndeliveredMarkAsync(Guid orderId, CancellationToken ct) => throw new NotSupportedException();

        public Task<bool> ClaimHasUndeliveredMarkAsync(DispatchRelatedRecordType recordType, Guid recordId, CancellationToken ct) => throw new NotSupportedException();
        public Task<IReadOnlySet<Guid>> FindClaimsWithUndeliveredMarkAsync(IReadOnlyCollection<Guid> claimIds, CancellationToken ct) => throw new NotSupportedException();

        public Task<IReadOnlyList<UndeliveredNotice>> FindUndeliveredNoticesAsync(Guid orderId, CancellationToken ct) => throw new NotSupportedException();

        public Task<IReadOnlyList<Guid>> FindMarkedOrderIdsAsync(Guid? afterOrderId, int maxOrders, CancellationToken ct) => throw new NotSupportedException();

        public Task<NoticeResendResult> ResendCustomerNoticeAsync(NoticeResendRequest request, CancellationToken ct) => throw new NotSupportedException();
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
