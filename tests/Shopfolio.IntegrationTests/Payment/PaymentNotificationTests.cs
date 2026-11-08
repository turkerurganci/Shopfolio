using System.Net;
using System.Text;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

using Npgsql;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Errors;
using Shopfolio.Infrastructure.Files;
using Shopfolio.Infrastructure.Imaging;
using Shopfolio.Infrastructure.InstallationSettings;
using Shopfolio.Infrastructure.Integrations;
using Shopfolio.Infrastructure.Jobs;
using Shopfolio.Infrastructure.Logging;
using Shopfolio.Infrastructure.Security;
using Shopfolio.Infrastructure.SignedEndpoints;
using Shopfolio.Infrastructure.Values;
using Shopfolio.IntegrationTests.Database;
using Shopfolio.IntegrationTests.SignedEndpoints;
using Shopfolio.IntegrationTests.Web;
using Shopfolio.Modules.AfterSales;
using Shopfolio.Modules.Catalog;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Notifications;
using Shopfolio.Modules.Notifications.Contracts;
using Shopfolio.Modules.Ordering;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Modules.Payment;
using Shopfolio.Modules.Payment.Contracts;
using Shopfolio.Modules.Settings;
using Shopfolio.UnitTests.Fakes;

namespace Shopfolio.IntegrationTests.Payment;

/// <summary>The collection of the payment notification endpoint's tests (T311): one database copied from the template.</summary>
[CollectionDefinition(PaymentNotificationDefinition.Name)]
public sealed class PaymentNotificationDefinition : ICollectionFixture<CollectionDatabaseFixture>
{
    public const string Name = "PaymentNotification";
}

/// <summary>
/// The payment provider's notification endpoint on a real PostgreSQL behind a test server (T311 KK1-KK10; 07 4.30.1; 08 2.1.5; 05 4.5.2-4.5.5): the
/// application's own mapping (<see cref="PaymentModuleRegistration.MapPaymentNotificationEndpoint"/>), the signed endpoint pipeline, the Payment and
/// Ordering modules and the job runner, with the link layer's hand-written fake. Every look at a row is made from a separate connection after the answer;
/// the order changes only when the runner processes the notification's job row.
/// </summary>
[Collection(PaymentNotificationDefinition.Name)]
public sealed class PaymentNotificationTests : IAsyncLifetime
{
    private const string ContactEmail = "Ali@Example.com";
    private const decimal Total = 120m;
    private const string Path = AntiforgeryExemptions.PaymentNotificationPath;

    private static readonly DateTimeOffset Now = new(2026, 11, 7, 9, 0, 0, TimeSpan.Zero);

    private readonly CollectionDatabaseFixture _collection;
    private readonly CommitStep _commit = new();
    private readonly CollectingLoggerProvider _logs = new();
    private readonly FakePaymentGateway _gateway = new();
    private WebApplication? _app;
    private HttpClient? _client;
    private int _counter;

    public PaymentNotificationTests(CollectionDatabaseFixture collection)
    {
        _collection = collection;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public async ValueTask InitializeAsync()
    {
        await ExecuteAsync(
            "TRUNCATE inbound_notification_records, dispatch_records, jobs, refund_records, refunds, payment_records, cancellations, internal_notes, "
            + "transition_records, order_items, orders, legal_text_versions, provinces CASCADE");

        WebApplicationBuilder builder = WebApplication.CreateSlimBuilder();
        builder.WebHost.UseTestServer();
        builder.Logging.ClearProviders().AddProvider(_logs);
        IServiceCollection services = builder.Services;
        services.AddDbContext<AppDbContext>(options => AppDbContext.Configure(options, _collection.Dsn).AddInterceptors(_commit));
        services.AddScoped<JobEnqueueTracker>();
        services.AddSingleton<IJobSignal, JobSignal>();
        services.AddScoped<ITransactionRunner, TransactionRunner>();
        services.AddAppCache();
        services.AddScoped<AdvisoryLocks>();
        services.AddSingleton(TimeProvider.System);
        services.AddScoped<IJobQueue, JobQueue>();
        services.AddFileStore();
        services.AddOptions<StatusThresholdOptions>();
        services.AddSingleton<Shopfolio.Infrastructure.Status.IDiskUsageReader, Shopfolio.Infrastructure.Status.DriveDiskUsageReader>();
        services.AddSingleton<Shopfolio.Infrastructure.Status.DiskUsageGuard>();
        services.AddSingleton<ImageProcessor>();
        services.AddCatalogModule();
        services.AddAfterSalesModule();
        services.AddOrderingModule();
        services.AddPaymentModule();
        services.AddScoped<INotificationsModule, NotificationsModule>();
        services.AddScoped<CustomerNoticeQueue>();
        services.AddScoped<CompanyNoticeQueue>();
        services.AddScoped<AccountEmailQueue>();
        services.AddScoped<ICompanyContactEmail, FixedCompanyContactEmail>();
        services.AddOptions<LinkKeyOptions>();
        services.AddSingleton<LinkKeys>();
        services.AddSingleton<IPaymentGateway>(_gateway);
        services.AddSingleton(provider => new JobHandlerRegistry(provider.GetServices<JobHandlerRegistration>()));
        services.AddAntiforgery();
        services.AddSignedEndpoints();
        services.AddSingleton<UnexpectedErrorCounter>();
        services.AddSingleton<UnexpectedErrorLayer>();

        _app = builder.Build();
        _app.UseUnexpectedErrorLayer();
        _app.UseAntiforgery();
        _app.MapPaymentNotificationEndpoint();
        await _app.StartAsync(Ct);
        _client = _app.GetTestClient();
    }

    public async ValueTask DisposeAsync()
    {
        _client?.Dispose();
        if (_app is not null)
        {
            await _app.StopAsync(Ct);
            await _app.DisposeAsync();
        }

        _logs.Dispose();
    }

    // ---- KK1, KK2: the signature in the link layer; a request whose signature does not hold is no notification ----

    [Fact]
    public async Task SignatureThatDoesNotHold_Gets400WithAnEmptyBody_NoRecordNoJob_AndAThinnedSecurityLine_KK1_KK2()
    {
        Order order = await SeedOrderAsync(OrderStatus.Placed, PaymentStatus.Pending);
        _gateway.EnqueueVerification(new NotificationVerification.SignatureInvalid());

        using HttpResponseMessage response = await PostAsync();

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Empty(await response.Content.ReadAsByteArrayAsync(Ct));
        Assert.Equal((0L, 0L), (await CountAsync("inbound_notification_records"), await CountAsync("jobs")));
        Assert.Contains(_logs.Lines, line => line.Contains(LogEvents.SignatureFailureSampled, StringComparison.Ordinal));
        Assert.Equal(nameof(IPaymentGateway.VerifyNotification), Assert.Single(_gateway.Calls).Job);
        await AssertOrderAsync(order.Id, OrderStatus.Placed, PaymentStatus.Pending, 1);
    }

    // ---- KK3, KK6, KK7: the record and its job row in one transaction, "received" after the commit, the order only in the job ----

    [Fact]
    public async Task Success_IsRecordedWithItsJobRow_ReceivedAfterTheCommit_AndTheOrderChangesOnlyInTheJob_KK3_KK6_KK7()
    {
        Order order = await SeedOrderAsync(OrderStatus.Placed, PaymentStatus.Pending);
        PaymentNotice notice = Notice(order, FakePaymentGateway.SucceededOutcome, total: Total + 4.5m);
        _gateway.EnqueueVerification(new NotificationVerification.SignatureValid(notice));
        long seenFromOutsideAtTheCommit = -1;
        _commit.BeforeCommit = async _ => seenFromOutsideAtTheCommit = await CountAsync("inbound_notification_records");

        using HttpResponseMessage response = await PostAsync();
        _commit.BeforeCommit = null;

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("OK", await response.Content.ReadAsStringAsync(Ct));
        Assert.Equal(0, seenFromOutsideAtTheCommit);
        InboundNotificationRecord record = await SingleRecordAsync();
        Assert.Equal(
            (notice.ProviderTransactionId, notice.ProviderTransactionId, FakePaymentGateway.PaymentEventType, FakePaymentGateway.SucceededOutcome, (decimal?)(Total + 4.5m), (Guid?)order.Id),
            (record.ProviderTransactionId, record.OrderReference, record.EventType, record.Outcome, record.Amount, record.OrderId));
        Assert.Null(record.ProviderNotificationId);
        Assert.Null(record.ProcessedAt);
        Assert.Equal(1, await JobRowsOfAsync(record.Id));
        await AssertOrderAsync(order.Id, OrderStatus.Placed, PaymentStatus.Pending, 1);

        Assert.True(await RunNextJobAsync());

        await AssertOrderAsync(order.Id, OrderStatus.Preparing, PaymentStatus.Paid, 2);
        await using AppDbContext reader = NewContext();
        PaymentRecord payment = await reader.PaymentRecords.SingleAsync(p => p.OrderId == order.Id, Ct);
        Assert.Equal(notice.ProviderTransactionId, payment.ProviderTransactionId);
        Assert.NotNull(payment.TransitionRecordId);
        Assert.NotNull((await SingleRecordAsync()).ProcessedAt);
        Assert.Null((await SingleRecordAsync()).UnprocessedReason);
        Assert.Equal(nameof(JobState.Completed), await ScalarAsync<string>($"SELECT job_state FROM jobs WHERE related_record_id = '{record.Id}'"));
    }

    [Fact]
    public async Task JobRowThatComesBack_FindsTheRecordProcessed_AndAppliesNothingAgain_KK4_KK7()
    {
        Order order = await SeedOrderAsync(OrderStatus.Placed, PaymentStatus.Pending);
        _gateway.EnqueueVerification(new NotificationVerification.SignatureValid(Notice(order, FakePaymentGateway.SucceededOutcome)));
        using HttpResponseMessage response = await PostAsync();
        Assert.True(await RunNextJobAsync());
        InboundNotificationRecord record = await SingleRecordAsync();

        await HandleAgainAsync(record.Id);

        await AssertOrderAsync(order.Id, OrderStatus.Preparing, PaymentStatus.Paid, 2);
        await using AppDbContext reader = NewContext();
        Assert.Equal(1, await reader.PaymentRecords.CountAsync(p => p.OrderId == order.Id, Ct));
        Assert.Equal(1, await reader.DispatchRecords.CountAsync(d => d.OrderId == order.Id && d.DispatchType == DispatchType.B4, Ct));
    }

    [Fact]
    public async Task Failure_CancelsTheUnpaidOrderInTheJob_S3WithO2AndTheCardFailureReason()
    {
        Order order = await SeedOrderAsync(OrderStatus.Placed, PaymentStatus.Pending);
        _gateway.EnqueueVerification(new NotificationVerification.SignatureValid(Notice(order, FakePaymentGateway.FailedOutcome, total: 0m, started: null)));

        using HttpResponseMessage response = await PostAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        await AssertOrderAsync(order.Id, OrderStatus.Placed, PaymentStatus.Pending, 1);
        Assert.True(await RunNextJobAsync());
        await AssertOrderAsync(order.Id, OrderStatus.Cancelled, PaymentStatus.Failed, 2);
        await using AppDbContext reader = NewContext();
        List<TransitionRecord> records = await reader.TransitionRecords.Where(t => t.OrderId == order.Id).ToListAsync(Ct);
        Assert.Equal([TransitionCode.O2, TransitionCode.S3], records.Select(r => r.TransitionCode).Order());
        Assert.All(records, r => Assert.Equal((ActorKind.System, (TransitionReason?)TransitionReason.CardPaymentFailed), (r.ActorKind, r.TransitionReason)));
    }

    [Fact]
    public async Task FailureOnAnOrderThatIsPaidAlready_ChangesNothing_AndTheRecordIsProcessed()
    {
        Order order = await SeedOrderAsync(OrderStatus.Preparing, PaymentStatus.Paid);
        _gateway.EnqueueVerification(new NotificationVerification.SignatureValid(Notice(order, FakePaymentGateway.FailedOutcome)));
        using HttpResponseMessage response = await PostAsync();

        Assert.True(await RunNextJobAsync());

        await AssertOrderAsync(order.Id, OrderStatus.Preparing, PaymentStatus.Paid, 1);
        Assert.NotNull((await SingleRecordAsync()).ProcessedAt);
    }

    // ---- KK4: the same notification again ----

    [Fact]
    public async Task SameNotificationAgain_IsReceivedWithTheSameAnswer_AndWritesNoSecondRecordOrJobRow_KK4()
    {
        Order order = await SeedOrderAsync(OrderStatus.Placed, PaymentStatus.Pending);
        PaymentNotice notice = Notice(order, FakePaymentGateway.SucceededOutcome);
        _gateway.EnqueueVerification(new NotificationVerification.SignatureValid(notice), new NotificationVerification.SignatureValid(notice));

        using HttpResponseMessage first = await PostAsync();
        using HttpResponseMessage again = await PostAsync();

        Assert.Equal((HttpStatusCode.OK, HttpStatusCode.OK), (first.StatusCode, again.StatusCode));
        Assert.Equal(await first.Content.ReadAsStringAsync(Ct), await again.Content.ReadAsStringAsync(Ct));
        Assert.Equal((1L, 1L), (await CountAsync("inbound_notification_records"), await CountAsync("jobs")));
    }

    [Fact]
    public async Task SameTransactionWithAnotherOutcome_IsAnotherNotification_KK4()
    {
        Order order = await SeedOrderAsync(OrderStatus.Placed, PaymentStatus.Pending);
        _gateway.EnqueueVerification(
            new NotificationVerification.SignatureValid(Notice(order, FakePaymentGateway.FailedOutcome)),
            new NotificationVerification.SignatureValid(Notice(order, FakePaymentGateway.SucceededOutcome)));

        using HttpResponseMessage failed = await PostAsync();
        using HttpResponseMessage succeeded = await PostAsync();

        Assert.Equal((2L, 2L), (await CountAsync("inbound_notification_records"), await CountAsync("jobs")));
    }

    // ---- KK5: a notification whose signature holds but which is not processed is recorded marked and received ----

    [Fact]
    public async Task AmountMismatch_IsRecordedMarked_WithoutAJobRow_AndReceived_KK5()
    {
        Order order = await SeedOrderAsync(OrderStatus.Placed, PaymentStatus.Pending);
        _gateway.EnqueueVerification(new NotificationVerification.SignatureValid(Notice(order, FakePaymentGateway.SucceededOutcome, total: Total - 0.01m)));

        using HttpResponseMessage response = await PostAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("OK", await response.Content.ReadAsStringAsync(Ct));
        InboundNotificationRecord record = await SingleRecordAsync();
        Assert.Equal((UnprocessedNotificationReason?)UnprocessedNotificationReason.AmountMismatch, record.UnprocessedReason);
        Assert.NotNull(record.ProcessedAt);
        Assert.Equal((Guid?)order.Id, record.OrderId);
        Assert.Equal(0L, await CountAsync("jobs"));
        Assert.Contains(_logs.Lines, line => line.Contains(LogEvents.InboundNotificationUnprocessed, StringComparison.Ordinal) && line.Contains(nameof(UnprocessedNotificationReason.AmountMismatch), StringComparison.Ordinal));
        Assert.DoesNotContain(_logs.Lines, line => line.Contains(record.ProviderTransactionId!, StringComparison.Ordinal));
        await AssertOrderAsync(order.Id, OrderStatus.Placed, PaymentStatus.Pending, 1);
    }

    [Fact]
    public async Task ReferenceOfNoOrder_IsRecordedMarkedOrderNotFound_Unlinked_AndReceived_KK5()
    {
        _gateway.EnqueueVerification(new NotificationVerification.SignatureValid(
            new PaymentNotice("2026999999", "2026-999999", FakePaymentGateway.PaymentEventType, FakePaymentGateway.SucceededOutcome, Total, Total, true, true, false)));

        using HttpResponseMessage response = await PostAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        InboundNotificationRecord record = await SingleRecordAsync();
        Assert.Equal((UnprocessedNotificationReason?)UnprocessedNotificationReason.OrderNotFound, record.UnprocessedReason);
        Assert.Null(record.OrderId);
        Assert.Equal("2026999999", record.OrderReference);
        Assert.Equal(0L, await CountAsync("jobs"));
    }

    [Fact]
    public async Task UnrecognizedOutcomeOrUnreadableBody_IsRecordedMarkedUnrecognized_AndReceived_KK5()
    {
        Order order = await SeedOrderAsync(OrderStatus.Placed, PaymentStatus.Pending);
        _gateway.EnqueueVerification(
            new NotificationVerification.SignatureValid(Notice(order, "pending")),
            new NotificationVerification.SignatureValid(Notice(order, FakePaymentGateway.SucceededOutcome, total: null)));

        using HttpResponseMessage unknown = await PostAsync();
        using HttpResponseMessage unreadable = await PostAsync();

        Assert.Equal((HttpStatusCode.OK, HttpStatusCode.OK), (unknown.StatusCode, unreadable.StatusCode));
        await using AppDbContext reader = NewContext();
        List<InboundNotificationRecord> records = await reader.InboundNotificationRecords.OrderBy(n => n.Outcome).ToListAsync(Ct);
        Assert.Equal(2, records.Count);
        Assert.All(records, r => Assert.Equal((UnprocessedNotificationReason?)UnprocessedNotificationReason.UnrecognizedNotification, r.UnprocessedReason));
        Assert.Null(records.Single(r => r.Outcome == FakePaymentGateway.SucceededOutcome).Amount);
        Assert.Equal(0L, await CountAsync("jobs"));
        await AssertOrderAsync(order.Id, OrderStatus.Placed, PaymentStatus.Pending, 1);
    }

    // ---- KK6, KK9: the record and the job row roll back together; a record that cannot be written is 500 with an empty body ----

    [Fact]
    public async Task RecordThatCannotBeCommitted_Gets500WithAnEmptyBody_AndNeitherTheRecordNorTheJobRowStays_KK6_KK9()
    {
        Order order = await SeedOrderAsync(OrderStatus.Placed, PaymentStatus.Pending);
        _gateway.EnqueueVerification(new NotificationVerification.SignatureValid(Notice(order, FakePaymentGateway.SucceededOutcome)));
        _commit.BeforeCommit = _ => throw new NpgsqlException("test commit failure");

        using HttpResponseMessage response = await PostAsync();
        _commit.BeforeCommit = null;

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.Empty(await response.Content.ReadAsByteArrayAsync(Ct));
        Assert.Equal((0L, 0L), (await CountAsync("inbound_notification_records"), await CountAsync("jobs")));
    }

    // ---- KK8: the body ceiling and no forgery check ----

    [Fact]
    public async Task BodyAboveTheCeiling_Gets413_WithoutAskingTheLinkLayer_KK8()
    {
        string body = "field=" + new string('a', SignedEndpointLimits.BodyMaxBytes);

        using HttpResponseMessage response = await PostAsync(body);

        Assert.Equal(HttpStatusCode.RequestEntityTooLarge, response.StatusCode);
        Assert.Empty(_gateway.Calls);
        Assert.Equal(0L, await CountAsync("inbound_notification_records"));
    }

    [Fact]
    public async Task TokenlessRequest_IsNotRefusedByTheForgeryCheck_KK8()
    {
        Order order = await SeedOrderAsync(OrderStatus.Placed, PaymentStatus.Pending);
        _gateway.EnqueueVerification(new NotificationVerification.SignatureValid(Notice(order, FakePaymentGateway.SucceededOutcome)));

        using HttpResponseMessage response = await PostAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        RouteEndpoint mapped = ((IEndpointRouteBuilder)_app!).DataSources.SelectMany(s => s.Endpoints).OfType<RouteEndpoint>().Single(e => e.RoutePattern.RawText == Path);
        Assert.False(mapped.Metadata.GetMetadata<Microsoft.AspNetCore.Antiforgery.IAntiforgeryMetadata>()!.RequiresValidation);
    }

    // ---- KK10: a payment to a cancelled order goes the same one path and reaches the late payment's refund ----

    [Fact]
    public async Task PaymentToACancelledOrder_GoesTheOnePath_IsRecorded_AndIsPaidBack_KK10()
    {
        Order order = await SeedOrderAsync(OrderStatus.Cancelled, PaymentStatus.Failed);
        PaymentNotice notice = Notice(order, FakePaymentGateway.SucceededOutcome);
        _gateway.EnqueueVerification(new NotificationVerification.SignatureValid(notice));

        using HttpResponseMessage response = await PostAsync();
        Assert.True(await RunNextJobAsync());

        await AssertOrderAsync(order.Id, OrderStatus.Cancelled, PaymentStatus.Failed, 2);
        await using AppDbContext reader = NewContext();
        PaymentRecord payment = await reader.PaymentRecords.SingleAsync(p => p.OrderId == order.Id, Ct);
        Assert.Equal((notice.ProviderTransactionId, (Guid?)null), (payment.ProviderTransactionId, payment.TransitionRecordId));
        Refund refund = await reader.Refunds.SingleAsync(r => r.OrderId == order.Id, Ct);
        Assert.Equal((RefundType.LatePayment, Total), (refund.RefundType, refund.Amount));
        Assert.Equal(1, await JobRowsOfAsync(refund.Id, JobType.CardRefund));
    }

    private static PaymentNotice Notice(Order order, string outcome, decimal? total = Total, decimal? started = Total) =>
        new(order.OrderNumber.Replace("-", string.Empty, StringComparison.Ordinal), order.OrderNumber, FakePaymentGateway.PaymentEventType, outcome, total, started, true, true, false);

    private async Task<bool> RunNextJobAsync()
    {
        await using NpgsqlDataSource dataSource = NpgsqlDataSource.Create(_collection.Dsn);
        JobRunnerService runner = new(
            dataSource,
            _app!.Services.GetRequiredService<IServiceScopeFactory>(),
            _app.Services.GetRequiredService<JobHandlerRegistry>(),
            [],
            _app.Services.GetRequiredService<IJobSignal>(),
            TimeProvider.System,
            NullLogger<JobRunnerService>.Instance);
        return await runner.RunOneAsync(Ct);
    }

    private async Task HandleAgainAsync(Guid recordId)
    {
        await using AsyncServiceScope scope = _app!.Services.CreateAsyncScope();
        InboundNotificationJobHandler handler = scope.ServiceProvider.GetRequiredService<InboundNotificationJobHandler>();
        JobWork work = new(Guid.NewGuid(), JobType.InboundNotificationProcessing, recordId, Now, 0);
        await scope.ServiceProvider.GetRequiredService<ITransactionRunner>().RunAsync(
            async token =>
            {
                await handler.HandleAsync(work, token);
                return new InboundRecordOutcome.Recorded();
            },
            Ct);
    }

    private async Task<Order> SeedOrderAsync(OrderStatus orderStatus, PaymentStatus paymentStatus)
    {
        int n = Interlocked.Increment(ref _counter);
        Province province = Province.Create($"{n:D2}", $"İl {n}", Now);
        LegalTextVersion form = LegalTextVersion.CreatePublished(LegalTextType.PreInformationForm, n, Now, "metin", Now);
        LegalTextVersion contract = LegalTextVersion.CreatePublished(LegalTextType.DistanceSalesContract, n, Now, "metin", Now);
        LegalTextVersion notice = LegalTextVersion.CreatePublished(LegalTextType.PrivacyNotice, n, Now, "metin", Now).WithHosting();
        Order order = Order.Create(
            $"2026-{n:D6}", $"access-placeholder-{n}", $"confirm-placeholder-{n}", PaymentMethod.Card, true, ContactEmail, Total, 0m, Now.AddHours(1), Now);
        order.IpAddress = IPAddress.Parse("203.0.113.5");
        order.CartId = EntityIds.New();
        order.BillingRecipientName = "Ali Veli";
        order.BillingProvinceId = province.Id;
        order.BillingDistrict = "Çankaya";
        order.BillingAddressLine = "Atatürk Bulvarı 1";
        order.CompanyType = CompanyType.Tradesman;
        order.CompanyLegalName = "Örnek Ticaret";
        order.CompanyTaxIdNumber = "1234567890";
        order.CompanyProvinceId = province.Id;
        order.CompanyDistrict = "Çankaya";
        order.CompanyAddressLine = "Kızılay 2";
        order.CompanyPhone = "3121234567";
        order.CompanyEmail = "firma@example.com";
        order.CompanyKepAddress = "firma@hs01.kep.tr";
        order.PreInformationFormVersionId = form.Id;
        order.DistanceSalesContractVersionId = contract.Id;
        order.PrivacyNoticeVersionId = notice.Id;
        order.FreezeDocuments("ön bilgilendirme", "mesafeli satış");
        OrderItem item = OrderItem.Create(order.Id, null, null, ProductType.Physical, "Ürün", 3, 100m, 20m, 16.67m, Now);

        await using (AppDbContext context = NewContext())
        {
            context.Provinces.Add(province);
            context.LegalTextVersions.AddRange(form, contract, notice);
            context.Orders.Add(order);
            context.OrderItems.Add(item);
            await context.SaveChangesAsync(Ct);
        }

        // The states are set around the entity (it takes them only through the unit); the pair rule of the database still applies.
        await ExecuteAsync($"UPDATE orders SET order_status = '{orderStatus}', payment_status = '{paymentStatus}' WHERE id = '{order.Id}'");
        return order;
    }

    private async Task AssertOrderAsync(Guid orderId, OrderStatus orderStatus, PaymentStatus paymentStatus, int version)
    {
        await using AppDbContext reader = NewContext();
        Order saved = await reader.Orders.SingleAsync(o => o.Id == orderId, Ct);
        Assert.Equal((orderStatus, paymentStatus, version), (saved.OrderStatus, saved.PaymentStatus, saved.Version));
    }

    private async Task<InboundNotificationRecord> SingleRecordAsync()
    {
        await using AppDbContext reader = NewContext();
        return await reader.InboundNotificationRecords.SingleAsync(Ct);
    }

    private Task<long> JobRowsOfAsync(Guid relatedRecordId, JobType type = JobType.InboundNotificationProcessing) =>
        ScalarAsync<long>($"SELECT count(*) FROM jobs WHERE job_type = '{type}' AND related_record_id = '{relatedRecordId}'");

    private Task<HttpResponseMessage> PostAsync(string body = "field=value")
    {
        StringContent content = new(body, Encoding.UTF8, "application/x-www-form-urlencoded");
        return _client!.PostAsync(new Uri(Path, UriKind.Relative), content, Ct);
    }

    private Task<long> CountAsync(string table) => ScalarAsync<long>($"SELECT count(*) FROM {table}");

    private async Task<T> ScalarAsync<T>(string sql)
    {
        await using NpgsqlConnection connection = new(_collection.Dsn);
        await connection.OpenAsync(Ct);
        await using NpgsqlCommand command = new(sql, connection);
        return (T)(await command.ExecuteScalarAsync(Ct))!;
    }

    private AppDbContext NewContext()
    {
        DbContextOptionsBuilder<AppDbContext> builder = new();
        AppDbContext.Configure(builder, _collection.Dsn);
        return new AppDbContext(builder.Options);
    }

    private async Task ExecuteAsync(string sql)
    {
        await using NpgsqlConnection connection = new(_collection.Dsn);
        await connection.OpenAsync(Ct);
        await using NpgsqlCommand command = new(sql, connection);
        await command.ExecuteNonQueryAsync(Ct);
    }

    private sealed class FixedCompanyContactEmail : ICompanyContactEmail
    {
        public Task<string?> GetAsync(CancellationToken ct) => Task.FromResult<string?>("firma-iletisim@example.com");
    }
}
