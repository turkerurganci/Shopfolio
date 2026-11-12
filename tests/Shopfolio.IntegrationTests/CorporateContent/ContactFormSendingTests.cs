using System.Net;
using System.Reflection;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Npgsql;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Errors;
using Shopfolio.Infrastructure.Jobs;
using Shopfolio.Infrastructure.Security;
using Shopfolio.Infrastructure.Values;
using Shopfolio.IntegrationTests.Database;
using Shopfolio.Modules.CorporateContent;
using Shopfolio.Modules.CorporateContent.Contracts;
using Shopfolio.Modules.CorporateContent.Contracts.Results;
using Shopfolio.Modules.Notifications;
using Shopfolio.Modules.Notifications.Contracts;
using Shopfolio.Modules.Settings.Contracts;
using Shopfolio.Modules.Settings.Contracts.Gates;
using Shopfolio.UnitTests.Fakes;

namespace Shopfolio.IntegrationTests.CorporateContent;

/// <summary>The collection of the contact form tests (T365): one database copied from the template.</summary>
[CollectionDefinition(ContactFormSendingDefinition.Name)]
public sealed class ContactFormSendingDefinition : ICollectionFixture<CollectionDatabaseFixture>
{
    public const string Name = "ContactFormSending";
}

/// <summary>
/// The send of the contact form on a real PostgreSQL, written as the application user (T365 criteria 1-10; KK1-KK10; 02 3.32; 07 UC-15): the form needs no sign-in; the four required
/// fields and the closed topic list are refused together and uncounted; the trap field drops a send without a trace; the limit L-4 counts every send on the address; a closed gate
/// closes the form before any field is read; the request is born open with the firm's notice F-2 and nothing goes to the sender. The gate is a stub the test opens and closes; the
/// attempt counter, the notice queue, the request writer and the transaction runner are the real ones, and each call runs in the runner's transaction the way a page handler does.
/// </summary>
[Collection(ContactFormSendingDefinition.Name)]
public sealed class ContactFormSendingTests : IAsyncLifetime
{
    private static readonly DateTimeOffset Start = new(2026, 11, 9, 9, 0, 0, TimeSpan.Zero);

    private static readonly IPAddress Ip = IPAddress.Parse("203.0.113.7");

    private readonly TemplateDatabaseFixture _template;
    private readonly CollectionDatabaseFixture _collection;
    private readonly ManualTimeProvider _time = new(Start);
    private readonly ISettingsModule _settings = DispatchProxy.Create<ISettingsModule, GateProxy>();
    private ServiceProvider _provider = null!;

    public ContactFormSendingTests(TemplateDatabaseFixture template, CollectionDatabaseFixture collection)
    {
        _template = template;
        _collection = collection;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private string AppDsn => _template.DsnFor(_collection.DatabaseName, _template.Users.App, _template.Users.AppPassword, pooling: false);

    private GateProxy Gate => (GateProxy)(object)_settings;

    public async ValueTask InitializeAsync()
    {
        await ExecuteAsync("TRUNCATE contact_requests, dispatch_records, attempt_records CASCADE");
        ServiceCollection services = new();
        services.AddSingleton<TimeProvider>(_time);
        services.AddDbContext<AppDbContext>(options => AppDbContext.Configure(options, AppDsn));
        services.AddScoped<ITransactionRunner, TransactionRunner>();
        services.AddAppCache();
        services.AddScoped<JobEnqueueTracker>();
        services.AddSingleton<IJobSignal, JobSignal>();
        services.AddLogging(builder => builder.SetMinimumLevel(LogLevel.None));
        services.AddScoped<AttemptCounter>();
        services.AddSingleton(_settings);
        services.AddSingleton<ICompanyContactEmail>(new FixedContactEmail("firma@example.com"));
        services.AddScoped<CompanyNoticeQueue>();
        services.AddScoped<INotificationsModule, FirmNoticeOnlyNotifications>();
        services.AddScoped<ContactRequests>();
        services.AddScoped<ContactFormSending>();
        _provider = services.BuildServiceProvider();
    }

    public async ValueTask DisposeAsync() => await _provider.DisposeAsync();

    // ---- KK1: no sign-in; the member's values start the form ----

    [Fact]
    public async Task ASendWithoutAnySignIn_WritesOneRequest_BornOpen_LinkedToNoAccountOrOrder_KK1()
    {
        ContactFormResult result = await SendAsync(Form());

        Assert.Equal(ContactFormOutcome.Sent, result.Outcome);
        Assert.Equal(1L, await ScalarAsync<long>("SELECT count(*) FROM contact_requests"));
        Assert.Equal("Open", await ScalarAsync<string>("SELECT contact_request_status FROM contact_requests"));
        Assert.Equal("Ayşe Yılmaz", await ScalarAsync<string>("SELECT name FROM contact_requests"));
        Assert.Equal("ayse@example.com", await ScalarAsync<string>("SELECT email FROM contact_requests"));
        Assert.Equal("5321234567", await ScalarAsync<string>("SELECT phone FROM contact_requests"));
        Assert.Equal(Start, await ReadCreatedAtAsync());
    }

    [Fact]
    public async Task TheForm_StartsWithTheMembersNameAndEmail_ForAVisitorEmpty_AndTheyCanBeChangedInTheSend_KK1()
    {
        ContactFormView member = await ReadFormAsync("  Ayşe Yılmaz ", " ayse@example.com ");
        ContactFormView visitor = await ReadFormAsync(null, null);
        ContactFormResult another = await SendAsync(Form() with { Email = "baska@example.com" });

        Assert.Equal("Ayşe Yılmaz", member.Name);
        Assert.Equal("ayse@example.com", member.Email);
        Assert.Equal(string.Empty, visitor.Name);
        Assert.Equal(string.Empty, visitor.Email);
        Assert.True(member.IsOpen);
        Assert.Equal(ContactFormOutcome.Sent, another.Outcome);
        Assert.Equal("baska@example.com", await ScalarAsync<string>("SELECT email FROM contact_requests"));
    }

    [Fact]
    public async Task ASecondSend_IsASecondRequest_NotCompared_KK1()
    {
        await SendAsync(Form());
        await SendAsync(Form());

        Assert.Equal(2L, await ScalarAsync<long>("SELECT count(*) FROM contact_requests"));
        Assert.Equal(2L, await ScalarAsync<long>("SELECT count(*) FROM dispatch_records"));
    }

    // ---- KK2: the four required fields; phone optional; no file or order number field ----

    [Fact]
    public async Task EveryEmptyRequiredField_IsRefusedTogether_NothingIsWritten_AndNothingIsCounted_KK2()
    {
        ContactFormResult result = await SendAsync(new ContactFormInput("  ", "", null, null, " \r\n ", null));

        Assert.Equal(ContactFormOutcome.FieldsRefused, result.Outcome);
        string[] expectedFields = [ContactFormFields.Name, ContactFormFields.Email, ContactFormFields.ContactTopic, ContactFormFields.Message];
        Assert.Equal(expectedFields, result.Fields.Select(f => f.Field));
        Assert.All(result.Fields, f => Assert.Equal(ErrorCode.FieldRequired, f.Code));
        Assert.Equal(0L, await ScalarAsync<long>("SELECT count(*) FROM contact_requests"));
        Assert.Equal(0L, await ScalarAsync<long>("SELECT count(*) FROM attempt_records"));
        Assert.Equal(0L, await ScalarAsync<long>("SELECT count(*) FROM dispatch_records"));
    }

    [Fact]
    public async Task AnEmailAndAPhoneOfTheWrongForm_AreRefusedByField_AndAnEmptyPhoneIsNoError_KK2()
    {
        ContactFormResult bad = await SendAsync(Form() with { Email = "ayse-at-example", Phone = "12345" });
        ContactFormResult noPhone = await SendAsync(Form() with { Phone = "   " });

        Assert.Equal(ContactFormOutcome.FieldsRefused, bad.Outcome);
        Assert.Contains(new ContactFormFieldError(ContactFormFields.Email, ErrorCode.InvalidEmail), bad.Fields);
        Assert.Contains(new ContactFormFieldError(ContactFormFields.Phone, ErrorCode.InvalidPhone), bad.Fields);
        Assert.Equal(ContactFormOutcome.Sent, noPhone.Outcome);
        Assert.Equal(1L, await ScalarAsync<long>("SELECT count(*) FROM contact_requests"));
        Assert.Null(await ScalarAsync<string?>("SELECT phone FROM contact_requests"));
    }

    [Fact]
    public async Task APhoneInAcceptedForms_IsStoredAsTenDigits_KK2()
    {
        await SendAsync(Form() with { Phone = "0 (532) 123-45-67" });

        Assert.Equal("5321234567", await ScalarAsync<string>("SELECT phone FROM contact_requests"));
    }

    [Fact]
    public async Task TheRequestTable_HasNoFileColumnAndNoOrderNumberColumn_AndNoLinkToAnAccountOrAnOrder_KK2()
    {
        string columns = await ScalarAsync<string>("SELECT string_agg(column_name, ',' ORDER BY column_name) FROM information_schema.columns WHERE table_name = 'contact_requests'");
        long foreignKeys = await ScalarAsync<long>("SELECT count(*) FROM information_schema.table_constraints WHERE table_name = 'contact_requests' AND constraint_type = 'FOREIGN KEY'");

        Assert.Equal("contact_request_status,contact_topic,created_at,email,id,last_closed_at,message,name,phone", columns);
        Assert.Equal(0L, foreignKeys);
    }

    // ---- KK3: the closed topic list ----

    [Theory]
    [InlineData(nameof(ContactTopic.GeneralQuestion))]
    [InlineData(nameof(ContactTopic.AboutOrder))]
    [InlineData(nameof(ContactTopic.AboutProduct))]
    [InlineData(nameof(ContactTopic.PersonalDataRequest))]
    [InlineData(nameof(ContactTopic.Other))]
    public async Task EachOfTheFiveTopics_IsAccepted_AndStoredByItsCodeName_KK3(string topic)
    {
        ContactFormResult result = await SendAsync(Form() with { ContactTopic = topic });

        Assert.Equal(ContactFormOutcome.Sent, result.Outcome);
        Assert.Equal(topic, await ScalarAsync<string>("SELECT contact_topic FROM contact_requests"));
    }

    [Theory]
    [InlineData("DefectClaim")]
    [InlineData("Complaint")]
    [InlineData("Genel soru")]
    [InlineData("1")]
    public async Task AValueOutsideTheList_IsRefusedAsNotInTheList_AndNothingIsWritten_KK3(string topic)
    {
        ContactFormResult result = await SendAsync(Form() with { ContactTopic = topic });

        Assert.Equal(ContactFormOutcome.FieldsRefused, result.Outcome);
        Assert.Equal([new ContactFormFieldError(ContactFormFields.ContactTopic, ErrorCode.InvalidListValue)], result.Fields);
        Assert.Equal(0L, await ScalarAsync<long>("SELECT count(*) FROM contact_requests"));
    }

    [Fact]
    public async Task TheFormView_ListsTheFiveTopicsOfTheEnum_KK3()
    {
        ContactFormView view = await ReadFormAsync(null, null);

        Assert.Equal([.. Enum.GetValues<ContactTopic>()], view.Topics);
        Assert.Equal(5, view.Topics.Count);
    }

    // ---- KK4: two statuses ----

    [Fact]
    public async Task ARequestFromTheForm_IsClosedAndReopenedByTheAdministrator_WithTheTwoStatuses_KK4()
    {
        await SendAsync(Form());
        Guid id = await ScalarAsync<Guid>("SELECT id FROM contact_requests");
        Guid admin = Guid.CreateVersion7();

        await InTransactionAsync(async scope => await scope.GetRequiredService<ContactRequests>().CloseAsync(id, admin, Ct));
        string closed = await ScalarAsync<string>("SELECT contact_request_status FROM contact_requests");
        await InTransactionAsync(async scope => await scope.GetRequiredService<ContactRequests>().ReopenAsync(id, admin, Ct));
        string reopened = await ScalarAsync<string>("SELECT contact_request_status FROM contact_requests");

        Assert.Equal("Closed", closed);
        Assert.Equal("Open", reopened);
        Assert.Equal([nameof(ContactRequestStatus.Open), nameof(ContactRequestStatus.Closed)], Enum.GetNames<ContactRequestStatus>());
    }

    // ---- KK5: the retention starts where the contract says ----

    [Fact]
    public async Task ARequestFromTheForm_CountsItsRetentionFromTheMomentItWasSent_UntilItIsClosed_KK5()
    {
        await SendAsync(Form() with { ContactTopic = nameof(ContactTopic.AboutOrder) });
        await SendAsync(Form() with { ContactTopic = nameof(ContactTopic.GeneralQuestion) });
        _time.Advance(TimeSpan.FromDays(40));

        await using AppDbContext db = NewContext();
        List<ContactRequest> rows = await db.ContactRequests.AsNoTracking().ToListAsync(Ct);

        Assert.Equal(2, rows.Count);
        Assert.All(rows, r => Assert.Equal(Start, r.RetentionStart));
        Assert.All(rows, r => Assert.Null(r.LastClosedAt));
        Assert.Contains(rows, r => r.ContactTopic == ContactTopic.AboutOrder);
    }

    // ---- KK6, KK7: the firm is notified; nothing goes to the sender; no reply or follow-up surface ----

    [Fact]
    public async Task ASend_QueuesTheNoticeF2ForTheFirmAndNothingForTheSender_KK6_KK7()
    {
        await SendAsync(Form());

        await using AppDbContext db = NewContext();
        DispatchRecord row = await db.DispatchRecords.AsNoTracking().SingleAsync(Ct);

        Assert.Equal(DispatchType.F2, row.DispatchType);
        Assert.Equal("firma@example.com", row.RecipientEmail);
        Assert.Equal(DispatchRelatedRecordType.ContactRequest, row.RelatedRecordType);
        Assert.DoesNotContain("ayse@example.com", row.RecipientEmail, StringComparison.Ordinal);
        Assert.Equal(1L, await ScalarAsync<long>("SELECT count(*) FROM dispatch_records"));
    }

    [Fact]
    public async Task NoTableForRepliesThreadsOrFollowingARequestExists_KK6_KK7()
    {
        string tables = await ScalarAsync<string>(
            "SELECT coalesce(string_agg(table_name, ',' ORDER BY table_name), '') FROM information_schema.tables WHERE table_schema = 'public' AND (table_name LIKE '%contact%' OR table_name LIKE '%complaint%' OR table_name LIKE '%chat%' OR table_name LIKE '%message%' OR table_name LIKE '%ticket%')");

        Assert.Equal("contact_requests", tables);
    }

    // ---- KK8: the trap and the limit L-4 ----

    [Fact]
    public async Task ASendWithTheTrapFilled_ShowsTheSentState_AndLeavesNoRequestNoNoticeAndNoAttemptRow_KK8()
    {
        ContactFormResult sent = await SendAsync(Form());
        await ExecuteAsync("TRUNCATE contact_requests, dispatch_records, attempt_records CASCADE");

        ContactFormResult dropped = await SendAsync(Form() with { Website = "http://spam.example" });

        Assert.True(dropped.ShowsSentState);
        Assert.Equal(sent.Code, dropped.Code);
        Assert.Equal(sent.Fields, dropped.Fields);
        Assert.Equal(0L, await ScalarAsync<long>("SELECT count(*) FROM contact_requests"));
        Assert.Equal(0L, await ScalarAsync<long>("SELECT count(*) FROM dispatch_records"));
        Assert.Equal(0L, await ScalarAsync<long>("SELECT count(*) FROM attempt_records"));
    }

    [Fact]
    public async Task TheTrapIsDroppedBeforeTheFieldsAreRead_SoAnInvalidSendWithItFilledIsTheSameAnswer_KK8()
    {
        ContactFormResult dropped = await SendAsync(new ContactFormInput("", "yanlis", "1", "x", "", "bot"));

        Assert.Equal(ContactFormOutcome.DroppedSilently, dropped.Outcome);
        Assert.Equal(0L, await ScalarAsync<long>("SELECT count(*) FROM attempt_records"));
    }

    [Fact]
    public async Task TheSendsPastTheLimit_AreRefusedWithTheNeutralCode_NothingIsWritten_ButEverySendIsCounted_KK8()
    {
        for (int i = 0; i < ProductConstants.ContactFormAttemptsPerIp; i++)
        {
            Assert.Equal(ContactFormOutcome.Sent, (await SendAsync(Form())).Outcome);
        }

        ContactFormResult over = await SendAsync(Form());

        Assert.Equal(ContactFormOutcome.LimitExceeded, over.Outcome);
        Assert.Equal(ErrorCode.ContactRequestLimitExceeded, over.Code);
        Assert.Equal(ProductConstants.ContactFormAttemptsPerIp, (int)await ScalarAsync<long>("SELECT count(*) FROM contact_requests"));
        Assert.Equal(ProductConstants.ContactFormAttemptsPerIp + 1, (int)await ScalarAsync<long>("SELECT count(*) FROM attempt_records WHERE attempt_limit = 'L4'"));
        Assert.Equal(ProductConstants.ContactFormAttemptsPerIp, (int)await ScalarAsync<long>("SELECT count(*) FROM dispatch_records"));
    }

    [Fact]
    public async Task TheLimitCountsTheAddressAlone_AnotherAddressIsFree_AndTheWindowEnds_KK8()
    {
        for (int i = 0; i <= ProductConstants.ContactFormAttemptsPerIp; i++)
        {
            await SendAsync(Form());
        }

        ContactFormResult other = await SendAsync(Form(), IPAddress.Parse("198.51.100.9"));
        _time.Advance(TimeSpan.FromMinutes(ProductConstants.ContactFormAttemptWindowMinutes) + TimeSpan.FromSeconds(1));
        ContactFormResult later = await SendAsync(Form());

        Assert.Equal(ContactFormOutcome.Sent, other.Outcome);
        Assert.Equal(ContactFormOutcome.Sent, later.Outcome);
    }

    [Fact]
    public async Task ARefusedSend_DoesNotMoveTheCounter_SoFieldErrorsNeverLockTheAddress_KK8()
    {
        for (int i = 0; i < ProductConstants.ContactFormAttemptsPerIp * 2; i++)
        {
            await SendAsync(Form() with { Message = "" });
        }

        ContactFormResult ok = await SendAsync(Form());

        Assert.Equal(ContactFormOutcome.Sent, ok.Outcome);
        Assert.Equal(1L, await ScalarAsync<long>("SELECT count(*) FROM attempt_records"));
    }

    // ---- KK9: the gate of the data-collecting entries ----

    [Fact]
    public async Task AClosedGate_ClosesTheForm_BeforeAnyFieldIsReadOrAnyAttemptCounted_KK9()
    {
        Gate.Value = new SalesGate([SalesGateCondition.PrivacyNotice], []);

        ContactFormResult result = await SendAsync(new ContactFormInput("", "", null, null, "", null));
        ContactFormResult valid = await SendAsync(Form());

        Assert.Equal(ContactFormOutcome.GateClosed, result.Outcome);
        Assert.Equal(ContactFormOutcome.GateClosed, valid.Outcome);
        Assert.Equal(ErrorCode.DataCollectionClosed, valid.Code);
        Assert.Equal([SalesGateCondition.PrivacyNotice], valid.Gate!.UnmetConditions);
        Assert.Equal(0L, await ScalarAsync<long>("SELECT count(*) FROM contact_requests"));
        Assert.Equal(0L, await ScalarAsync<long>("SELECT count(*) FROM attempt_records"));
    }

    [Fact]
    public async Task ATrapFilledSend_OnAClosedGate_IsRefusedAsClosed_NotShownAsSent_KK9()
    {
        Gate.Value = new SalesGate([SalesGateCondition.CookiePolicy], []);

        ContactFormResult result = await SendAsync(Form() with { Website = "bot" });

        Assert.Equal(ContactFormOutcome.GateClosed, result.Outcome);
    }

    [Fact]
    public async Task EachOfTheTwoConditions_ClosesTheForm_AndTheOpenFormShowsThePrivacyLink_KK9()
    {
        ContactFormView open = await ReadFormAsync(null, null);
        Gate.Value = new SalesGate([SalesGateCondition.PrivacyNotice], []);
        ContactFormView noNotice = await ReadFormAsync(null, null);
        Gate.Value = new SalesGate([SalesGateCondition.CookiePolicy], []);
        ContactFormView noCookies = await ReadFormAsync(null, null);
        Gate.Value = new SalesGate([SalesGateCondition.PaymentMethod, SalesGateCondition.TemporaryClosure], []);
        ContactFormView salesOnly = await ReadFormAsync(null, null);

        Assert.True(open.IsOpen);
        Assert.Equal(LegalLinkScreen.ContactForm, open.PrivacyNoticeScreen);
        Assert.False(noNotice.IsOpen);
        Assert.Equal(ErrorCode.DataCollectionClosed, noNotice.Refusal!.Code);
        Assert.False(noCookies.IsOpen);
        Assert.True(salesOnly.IsOpen);
    }

    // ---- KK10: no complaint channel, no live chat; the download renewal comes with "about an order" ----

    [Fact]
    public async Task TheTopicsHaveNoComplaintOrChatTopic_AndTheOrderTopicCarriesTheDownloadRenewal_KK10()
    {
        ContactFormView view = await ReadFormAsync(null, null);

        Assert.Contains(ContactTopic.AboutOrder, view.Topics);
        Assert.DoesNotContain(Enum.GetNames<ContactTopic>(), n => n.Contains("Complaint", StringComparison.OrdinalIgnoreCase) || n.Contains("Chat", StringComparison.OrdinalIgnoreCase));
        ContactFormResult renewal = await SendAsync(Form() with { ContactTopic = nameof(ContactTopic.AboutOrder), Message = "İndirme hakkımı yenilemek istiyorum" });
        Assert.Equal(ContactFormOutcome.Sent, renewal.Outcome);
    }

    [Fact]
    public async Task ACallOutsideATransaction_IsAProgrammingError_BeforeAnythingIsRead_KK1()
    {
        await using AsyncServiceScope scope = _provider.CreateAsyncScope();
        ContactFormSending sending = scope.ServiceProvider.GetRequiredService<ContactFormSending>();

        await Assert.ThrowsAsync<InvalidOperationException>(() => sending.SendAsync(Form(), Ip, Ct));
    }

    // ---- helpers ----

    private static ContactFormInput Form() =>
        new("  Ayşe Yılmaz ", " ayse@example.com ", "5321234567", nameof(ContactTopic.GeneralQuestion), "Merhaba, bir sorum var.", null);

    private Task<ContactFormResult> SendAsync(ContactFormInput input, IPAddress? ip = null) =>
        InTransactionAsync(scope => scope.GetRequiredService<ContactFormSending>().SendAsync(input, ip ?? Ip, Ct));

    private async Task<ContactFormView> ReadFormAsync(string? name, string? email)
    {
        await using AsyncServiceScope scope = _provider.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<ContactFormSending>().ReadAsync(name, email, Ct);
    }

    /// <summary>Runs the work the way a page handler does: in the transaction runner of one scope, so a refused result rolls everything back.</summary>
    private async Task<T> InTransactionAsync<T>(Func<IServiceProvider, Task<T>> work)
        where T : IOperationResult
    {
        await using AsyncServiceScope scope = _provider.CreateAsyncScope();
        ITransactionRunner runner = scope.ServiceProvider.GetRequiredService<ITransactionRunner>();
        return await runner.RunAsync(_ => work(scope.ServiceProvider), Ct);
    }

    private AppDbContext NewContext()
    {
        DbContextOptionsBuilder<AppDbContext> builder = new();
        AppDbContext.Configure(builder, AppDsn);
        return new AppDbContext(builder.Options);
    }

    private async Task<DateTimeOffset> ReadCreatedAtAsync()
    {
        await using AppDbContext db = NewContext();
        return (await db.ContactRequests.AsNoTracking().SingleAsync(Ct)).CreatedAt;
    }

    private async Task<T> ScalarAsync<T>(string sql)
    {
        await using NpgsqlConnection connection = new(_collection.Dsn);
        await connection.OpenAsync(Ct);
        await using NpgsqlCommand command = new(sql, connection);
        object? value = await command.ExecuteScalarAsync(Ct);
        if (value is DateTime moment && typeof(T) == typeof(DateTimeOffset))
        {
            return (T)(object)new DateTimeOffset(DateTime.SpecifyKind(moment, DateTimeKind.Utc));
        }

        return value is null or DBNull ? default! : (T)value;
    }

    private async Task ExecuteAsync(string sql)
    {
        await using NpgsqlConnection connection = new(_collection.Dsn);
        await connection.OpenAsync(Ct);
        await using NpgsqlCommand command = new(sql, connection);
        await command.ExecuteNonQueryAsync(Ct);
    }

    /// <summary>The settings module reduced to the gate: only the sales gate is answered; any other call is a test mistake.</summary>
    public class GateProxy : DispatchProxy
    {
        public SalesGate Value { get; set; } = new([], []);

        protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
        {
            if (targetMethod?.Name == nameof(ISettingsModule.GetSalesGateAsync))
            {
                return Task.FromResult(Value);
            }

            throw new NotSupportedException(targetMethod?.Name);
        }
    }

    /// <summary>The Notifications module reduced to the firm notice the contact request uses: it is the real queue, so the real dispatch row is written.</summary>
    private sealed class FirmNoticeOnlyNotifications(CompanyNoticeQueue queue) : INotificationsModule
    {
        public Task<int> QueueCompanyNoticeAsync(CompanyNotice notice, CancellationToken ct) => queue.QueueCompanyAsync(notice, ct);

        public Task<int> QueueCustomerNoticeAsync(CustomerNotice notice, string recipientEmail, CancellationToken ct) => throw new NotSupportedException("A customer notice would be an acknowledgement (02 3.32.6).");

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

    private sealed class FixedContactEmail(string? email) : ICompanyContactEmail
    {
        public Task<string?> GetAsync(CancellationToken ct) => Task.FromResult(email);
    }
}
