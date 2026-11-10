using System.Net;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using Npgsql;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.EmailComposition;
using Shopfolio.Infrastructure.Files;
using Shopfolio.Infrastructure.Imaging;
using Shopfolio.Infrastructure.InstallationSettings;
using Shopfolio.Infrastructure.Integrations;
using Shopfolio.Infrastructure.Jobs;
using Shopfolio.Infrastructure.SearchEngine;
using Shopfolio.Infrastructure.Security;
using Shopfolio.IntegrationTests.Database;
using Shopfolio.IntegrationTests.Model;
using Shopfolio.IntegrationTests.Ordering;
using Shopfolio.Modules.Administration;
using Shopfolio.Modules.AfterSales;
using Shopfolio.Modules.AfterSales.Contracts;
using Shopfolio.Modules.Catalog;
using Shopfolio.Modules.CorporateContent;
using Shopfolio.Modules.CorporateContent.Contracts;
using Shopfolio.Modules.Notifications;
using Shopfolio.Modules.Notifications.Contracts;
using Shopfolio.Modules.Ordering;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Modules.Payment;
using Shopfolio.Modules.Payment.Contracts;
using Shopfolio.Modules.Settings;
using Shopfolio.Modules.Settings.Contracts;
using Shopfolio.UnitTests.Fakes;

namespace Shopfolio.IntegrationTests.Outages;

/// <summary>
/// The queue's sender, the retry rule and the "e-mail did not arrive" mark on a real PostgreSQL (T385 criteria 1-9; KK1-KK9; 02 9.1.6; 08 2.2.2, 2.2.4; 06 4.58): the three retries are counted from the
/// first attempt and do not drift when a retry runs late; the mark is read from the group's latest row and never written - the trail and the order do not move; the limit's refusal and a later
/// permanent bounce mark; the firm notices and the account e-mails other than the invitation do not; every notice B-1…B-16 is made from the order and sent again through the ordinary queue to the order's
/// current address (B-15 to the one before the correction), writes the trail row and clears the mark on success. The clock is fake and the gateway is the fake service.
/// </summary>
[Collection(RecipientSendLimitDefinition.Name)]
public sealed class DispatchRetryAndMarkTests : IAsyncLifetime
{
    private const string OldAddress = "eski@example.com";
    private const string CurrentAddress = "yeni@example.com";
    private const string Iban = "TR330006100519786457841326";

    private static readonly DateTimeOffset Start = new(2026, 11, 9, 10, 0, 0, TimeSpan.Zero);

    private readonly CollectionDatabaseFixture _collection;
    private readonly ManualTimeProvider _time = new(Start);
    private readonly FakeEmailGateway _gateway = new();

    private ServiceProvider _provider = null!;
    private Guid _admin;
    private Guid _province;
    private int _orderCounter;

    public DispatchRetryAndMarkTests(CollectionDatabaseFixture collection)
    {
        _collection = collection;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public async ValueTask InitializeAsync()
    {
        await ExecuteAsync("TRUNCATE dispatch_records, audit_logs, order_items, orders, legal_text_versions, provinces, admins CASCADE");
        _provider = BuildProvider();
        Admin admin = Admin.Create("Firma Yöneticisi", "yonetici@example.com", "digest-placeholder", Start);
        Province province = Province.Create("06", "Ankara", Start);
        await using (AppDbContext db = NewContext())
        {
            db.Admins.Add(admin);
            db.Provinces.Add(province);
            db.LegalTextVersions.AddRange(
                LegalTextVersion.CreatePublished(LegalTextType.PreInformationForm, 1, Start, "metin", Start),
                LegalTextVersion.CreatePublished(LegalTextType.DistanceSalesContract, 1, Start, "metin", Start),
                LegalTextVersion.CreatePublished(LegalTextType.PrivacyNotice, 1, Start, "metin", Start).WithHosting());
            await db.SaveChangesAsync(Ct);
        }

        _admin = admin.Id;
        await ExecuteAsync("INSERT INTO companies (id, created_at) VALUES (gen_random_uuid(), now())");
        await ExecuteAsync(
        await ExecuteAsync(
            $"UPDATE companies SET company_type = 'Tradesman', legal_name = 'Örnek Ticaret', tax_id_number = '1111111111', province_id = '{_province}', district = 'Çankaya', "
            + "address_line = 'Kızılay 2', phone = '3121234567', email = 'firma@example.com', kep_address = 'firma@hs01.kep.tr', chamber_name = 'Ankara Esnaf Odası', professional_conduct_rules = 'Meslek kuralları'");
    }

    public async ValueTask DisposeAsync() => await _provider.DisposeAsync();

    // ---- KK1: the three moments are counted from the first attempt, and a late retry does not shift the rest ----

    [Fact]
    public async Task AFailingRow_IsTriedAtTheFirstAttempt_ThenFiveMinutesThirtyMinutesAndTwoHoursAfterIt_AndNeverAgain()
    {
        Guid order = await SeedOrderAsync();
        Guid row = await QueueAsync(new PaymentConfirmedNotice(order), CurrentAddress);
        _gateway.DefaultSend = new EmailSendResult.ChannelError("Down");

        Assert.Equal(DispatchSendOutcome.Attempted, await SendAsync(row));
        DispatchRecord first = await RowAsync(row);
        Assert.Equal((DispatchState.Queued, 1, Start.AddMinutes(5)), (first.DispatchState, first.AttemptCount, first.NextAttemptAt));

        // Not due yet: nothing is done and the service is not called.
        _time.Advance(TimeSpan.FromMinutes(4));
        Assert.Equal(DispatchSendOutcome.NotDue, await SendAsync(row));
        Assert.Single(_gateway.Messages);

        _time.Advance(TimeSpan.FromMinutes(1));
        Assert.Equal(DispatchSendOutcome.Attempted, await SendAsync(row));
        Assert.Equal(Start.AddMinutes(30), (await RowAsync(row)).NextAttemptAt);

        _time.Advance(TimeSpan.FromMinutes(25));
        Assert.Equal(DispatchSendOutcome.Attempted, await SendAsync(row));
        Assert.Equal(Start.AddHours(2), (await RowAsync(row)).NextAttemptAt);

        _time.Advance(TimeSpan.FromMinutes(90));
        Assert.Equal(DispatchSendOutcome.Attempted, await SendAsync(row));
        DispatchRecord last = await RowAsync(row);
        Assert.Equal((DispatchState.Undelivered, 4, (DateTimeOffset?)null), (last.DispatchState, last.AttemptCount, last.NextAttemptAt));

        _time.Advance(TimeSpan.FromDays(1));
        Assert.Equal(DispatchSendOutcome.NotDue, await SendAsync(row));
        Assert.Equal(4, _gateway.Messages.Count);
    }

    [Fact]
    public async Task ALateRetry_DoesNotMoveTheNextOnesFromTheirPlaceInTheSchedule()
    {
        Guid order = await SeedOrderAsync();
        Guid row = await QueueAsync(new PaymentConfirmedNotice(order), CurrentAddress);
        _gateway.DefaultSend = new EmailSendResult.ChannelError("Down");
        await SendAsync(row);

        // The first retry is twenty minutes late; the second still falls thirty minutes after the first attempt, the third two hours after it.
        _time.Advance(TimeSpan.FromMinutes(25));
        await SendAsync(row);
        Assert.Equal(Start.AddMinutes(30), (await RowAsync(row)).NextAttemptAt);

        _time.Advance(TimeSpan.FromMinutes(95));
        await SendAsync(row);
        Assert.Equal(Start.AddHours(2), (await RowAsync(row)).NextAttemptAt);
    }

    // ---- KK2, KK3: the mark falls with the third retry's failure, on the claim's row too, and writes nothing ----

    [Fact]
    public async Task TheMark_FallsWithTheThirdRetrysFailure_ButNotBefore_AndWritesNeitherTheTrailNorTheOrder()
    {
        Guid order = await SeedOrderAsync();
        Guid row = await QueueAsync(new PaymentConfirmedNotice(order), CurrentAddress);
        _gateway.DefaultSend = new EmailSendResult.RecipientError("Mailbox");
        string orderBefore = await OrderFingerprintAsync(order);
        long trailBefore = await CountAsync("audit_logs");

        for (int attempt = 1; attempt <= 3; attempt++)
        {
            await SendAsync(row);
            Assert.False(await HasMarkAsync(order));
            _time.Advance(TimeSpan.FromHours(2));
        }

        Assert.False(await HasMarkAsync(order));
        await SendAsync(row);

        Assert.True(await HasMarkAsync(order));
        Assert.Equal(trailBefore, await CountAsync("audit_logs"));
        Assert.Equal(orderBefore, await OrderFingerprintAsync(order));
    }

    [Fact]
    public async Task AClaimsB9_ShowsTheMarkOnTheClaimToo_WhenItsRowIsUndelivered()
    {
        Guid order = await SeedOrderAsync();
        Guid claim = Guid.NewGuid();
        CustomerNotice notice = Notice(DispatchType.B9, order, claim);
        Guid row = await QueueAsync(notice, CurrentAddress);
        await MakeUndeliveredAsync(row);

        await using AppDbContext db = NewContext();
        Assert.True(await DispatchMarks.ClaimHasMarkAsync(db, DispatchRelatedRecordType.Withdrawal, claim, Ct));
        Assert.False(await DispatchMarks.ClaimHasMarkAsync(db, DispatchRelatedRecordType.Withdrawal, Guid.NewGuid(), Ct));
        Assert.True(await DispatchMarks.HasMarkAsync(db, order, Ct));
    }

    // ---- KK4: the limit's refusal and a later permanent bounce mark ----

    [Fact]
    public async Task ARowRefusedByTheRecipientLimit_IsMarkedWithoutAnyRetryAndWithoutACall()
    {
        Guid order = await SeedOrderAsync();
        for (int i = 0; i < RecipientSendLimit.Threshold; i++)
        {
            await AddSentAsync(DispatchType.B1, CurrentAddress, order, Start.AddMinutes(-(i + 1)));
        }

        Guid row = await QueueAsync(new PaymentConfirmedNotice(order), CurrentAddress);

        Assert.Equal(DispatchSendOutcome.Refused, await SendAsync(row));

        DispatchRecord refused = await RowAsync(row);
        Assert.Equal((DispatchState.Undelivered, DispatchOutcome.RecipientError, (DateTimeOffset?)null), (refused.DispatchState, refused.LastOutcome, refused.NextAttemptAt));
        Assert.Empty(_gateway.Calls);
        Assert.True(await HasMarkAsync(order));
    }

    [Fact]
    public async Task ASentRow_TheServiceLaterReportsAsAPermanentBounce_MarksTheOrder()
    {
        Guid order = await SeedOrderAsync();
        Guid row = await AddSentAsync(DispatchType.B4, CurrentAddress, order, Start);
        Assert.False(await HasMarkAsync(order));

        await using (AppDbContext db = NewContext())
        {
            DispatchRecord sent = await db.DispatchRecords.SingleAsync(d => d.Id == row, Ct);
            sent.DispatchState = DispatchState.Undelivered;
            sent.LastOutcome = DispatchOutcome.RecipientError;
            await db.SaveChangesAsync(Ct);
        }

        Assert.True(await HasMarkAsync(order));
    }

    // ---- KK5: the firm notices and most account e-mails put no mark; the marked orders list is the filtered one ----

    [Fact]
    public async Task AFirmNoticeAndAnAccountEmailThatIsNotTheInvitation_PutNoMark_ButTheInvitationDoes()
    {
        Guid order = await SeedOrderAsync();
        Guid firm = await AddRowAsync(DispatchType.F1, "firma@example.com", null, DispatchRelatedRecordType.Order, order);
        await MakeUndeliveredAsync(firm);
        Guid reset = Guid.NewGuid();
        Guid invitation = Guid.NewGuid();
        await MakeUndeliveredAsync(await AddRowAsync(DispatchType.PasswordResetLink, CurrentAddress, null, DispatchRelatedRecordType.Customer, reset));
        await MakeUndeliveredAsync(await AddRowAsync(DispatchType.AdminInvitation, CurrentAddress, null, DispatchRelatedRecordType.AdminInvitation, invitation));

        await using AppDbContext db = NewContext();
        Assert.False(await DispatchMarks.HasMarkAsync(db, order, Ct));
        Assert.Empty(await DispatchMarks.MarkedOrderIdsAsync(db, null, 10, Ct));
        Assert.False(await DispatchMarks.AccountEmailHasMarkAsync(db, DispatchType.PasswordResetLink, reset, Ct));
        Assert.True(await DispatchMarks.AccountEmailHasMarkAsync(db, DispatchType.AdminInvitation, invitation, Ct));
    }

    [Fact]
    public async Task TheMarkedOrdersList_HoldsOnlyTheMarkedOrders_InPagesThatContinue()
    {
        Guid marked1 = await SeedOrderAsync();
        Guid unmarked = await SeedOrderAsync();
        Guid marked2 = await SeedOrderAsync();
        await MakeUndeliveredAsync(await QueueAsync(new PaymentConfirmedNotice(marked1), CurrentAddress));
        await MakeUndeliveredAsync(await QueueAsync(new PaymentConfirmedNotice(marked2), CurrentAddress));
        await AddSentAsync(DispatchType.B4, CurrentAddress, unmarked, Start);

        await using AppDbContext db = NewContext();
        IReadOnlyList<Guid> firstPage = await DispatchMarks.MarkedOrderIdsAsync(db, null, 1, Ct);
        IReadOnlyList<Guid> secondPage = await DispatchMarks.MarkedOrderIdsAsync(db, firstPage[0], 1, Ct);
        IReadOnlyList<Guid> third = await DispatchMarks.MarkedOrderIdsAsync(db, secondPage[0], 1, Ct);

        Assert.Equal(new[] { marked1, marked2 }.Order(), new[] { firstPage[0], secondPage[0] }.Order());
        Assert.NotEqual(firstPage[0], secondPage[0]);
        Assert.Empty(third);
    }

    // ---- KK6, KK7, KK8, KK9: the re-sending of every notice B-1…B-16 through the ordinary queue ----

    [Theory]
    [InlineData(DispatchType.B1)]
    [InlineData(DispatchType.B2)]
    [InlineData(DispatchType.B3)]
    [InlineData(DispatchType.B4)]
    [InlineData(DispatchType.B5)]
    [InlineData(DispatchType.B6)]
    [InlineData(DispatchType.B7)]
    [InlineData(DispatchType.B8)]
    [InlineData(DispatchType.B9)]
    [InlineData(DispatchType.B10)]
    [InlineData(DispatchType.B11)]
    [InlineData(DispatchType.B12)]
    [InlineData(DispatchType.B13)]
    [InlineData(DispatchType.B14)]
    [InlineData(DispatchType.B15)]
    [InlineData(DispatchType.B16)]
    public async Task EveryCustomerNotice_IsMadeFromTheOrder_SentAgainThroughTheQueue_ToTheRightAddress_AndTheMarkFallsOnSuccess(DispatchType type)
    {
        Guid order = await SeedOrderAsync();
        Guid related = Guid.NewGuid();
        Guid root = await QueueAsync(Notice(type, order, related), type == DispatchType.B15 ? OldAddress : OldAddress);
        await MakeUndeliveredAsync(root);
        Assert.True(await HasMarkAsync(order));
        long trailBefore = await CountAsync("audit_logs");

        _time.Advance(TimeSpan.FromDays(1));
        NoticeResendResult queued = await ResendAsync(order, root);

        Assert.Equal(NoticeResendResult.Queued, queued);
        DispatchRecord again = await LatestAsync(order);
        string expectedRecipient = type == DispatchType.B15 ? OldAddress : CurrentAddress;
        Assert.Equal((type, expectedRecipient, root, DispatchState.Queued), (again.DispatchType, again.RecipientEmail, again.OriginalDispatchId, again.DispatchState));
        Assert.Equal(Start, CustomerNoticeValues.ReadFirstEmailSentAt(again.EventValues));
        Assert.NotNull(CustomerNoticeValues.Read(type, order, again.EventValues, again.RelatedRecordId));
        Assert.Equal(type == DispatchType.B9 ? related : null, again.RelatedRecordId);
        Assert.Equal(type == DispatchType.B9 ? DispatchRelatedRecordType.Withdrawal : null, again.RelatedRecordType);

        // The trail row: the operation, the order, the order number only.
        Assert.Equal(trailBefore + 1, await CountAsync("audit_logs"));
        (string operation, string target, string details) = await LastTrailAsync();
        Assert.Equal(("ResendEmail", "Order"), (operation, target));
        Assert.DoesNotContain("@", details, StringComparison.Ordinal);
        Assert.Contains("2026-", details, StringComparison.Ordinal);

        // While it waits in the queue the order carries no mark; the ordinary queue sends it and the mark stays off.
        Assert.False(await HasMarkAsync(order));
        _gateway.DefaultSend = new EmailSendResult.Sent();
        Assert.Equal(DispatchSendOutcome.Attempted, await SendAsync(again.Id));
        Assert.Equal(DispatchState.Sent, (await RowAsync(again.Id)).DispatchState);
        EmailMessage message = Assert.Single(_gateway.Messages);
        Assert.Equal(expectedRecipient, message.RecipientEmail);
        Assert.Contains("tekrar", message.TextBody, StringComparison.OrdinalIgnoreCase);
        Assert.False(await HasMarkAsync(order));
    }

    [Fact]
    public async Task AResendingThatFailsThroughItsRetries_MarksAgain_AndTheLimitDoesNotStopIt()
    {
        Guid order = await SeedOrderAsync();
        for (int i = 0; i < RecipientSendLimit.Threshold + 3; i++)
        {
            await AddSentAsync(DispatchType.B1, CurrentAddress, order, Start.AddMinutes(-(i + 1)));
        }

        Guid root = await QueueAsync(new PaymentConfirmedNotice(order), OldAddress);
        await MakeUndeliveredAsync(root);
        Assert.Equal(NoticeResendResult.Queued, await ResendAsync(order, root));
        Guid again = (await LatestAsync(order)).Id;

        _gateway.DefaultSend = new EmailSendResult.ChannelError("Down");
        for (int attempt = 0; attempt < 4; attempt++)
        {
            Assert.Equal(DispatchSendOutcome.Attempted, await SendAsync(again));
            _time.Advance(TimeSpan.FromHours(2));
        }

        Assert.Equal(4, _gateway.Messages.Count);
        Assert.Equal(DispatchState.Undelivered, (await RowAsync(again)).DispatchState);
        Assert.True(await HasMarkAsync(order));

        // The latest row of the group is the one that decides: the new row is the mark now, and it can be sent again.
        await using AppDbContext db = NewContext();
        UndeliveredNotice notice = Assert.Single(DispatchMarks.UndeliveredGroups(await DispatchMarks.ReadNoticeRowsAsync(db, order, Ct)));
        Assert.Equal((root, again), (notice.GroupId, notice.LatestDispatchId));
    }

    [Fact]
    public async Task AResending_OfARowThatIsNotMarked_OrWhoseOrderHasNoAddress_WritesNothing()
    {
        Guid order = await SeedOrderAsync();
        Guid sent = await AddSentAsync(DispatchType.B4, CurrentAddress, order, Start);
        long trailBefore = await CountAsync("audit_logs");

        Assert.Equal(NoticeResendResult.NotMarked, await ResendAsync(order, sent));
        Guid root = await QueueAsync(new PaymentConfirmedNotice(order), OldAddress);
        await MakeUndeliveredAsync(root);
        Assert.Equal(NoticeResendResult.NoRecipient, await ResendAsync(order, root, currentEmail: " "));

        Assert.Equal(trailBefore, await CountAsync("audit_logs"));
        Assert.Equal(2, await CountAsync("dispatch_records"));
    }

    // ---- the sender: the family it picks and what it does with a row it cannot make a body for ----

    [Fact]
    public async Task ARowWhoseOrderIsGone_OrWhoseValuesAreUnreadable_GoesTheRetryWayAsABodyNotMade()
    {
        Guid order = await SeedOrderAsync();
        Guid unreadable = await AddRowAsync(DispatchType.B5, CurrentAddress, order, null, null);

        Assert.Equal(DispatchSendOutcome.BodyNotMade, await SendAsync(unreadable));

        DispatchRecord row = await RowAsync(unreadable);
        Assert.Equal((DispatchState.Queued, DispatchOutcome.RecipientError, 1), (row.DispatchState, row.LastOutcome, row.AttemptCount));
        Assert.Empty(_gateway.Calls);
    }

    [Fact]
    public async Task TheSender_PicksTheSixteenCustomerNotices_TheSixFirmNotices_AndTheAccountEmails()
    {
        foreach (DispatchType type in Enum.GetValues<DispatchType>())
        {
            bool expected = DispatchTypeFamily.IsCustomerNotice(type) || type is >= DispatchType.F1 and <= DispatchType.F6 || DispatchTypeFamily.IsAccountEmail(type);
            Assert.Equal(expected, DispatchSender.Picks(type));
        }

        await using AppDbContext db = NewContext();
        Guid order = await SeedOrderAsync();
        Guid due = await QueueAsync(new PaymentConfirmedNotice(order), CurrentAddress);
        Assert.Equal([due], (await DispatchSender.FindDueAsync(db, Start, Ct)).Select(d => d.Id));
    }

    [Fact]
    public async Task AFirmNotice_IsSentByTheSameSender_AndItsFailureNeverMarksTheOrder()
    {
        Guid order = await SeedOrderAsync();
        CompanyNoticeRow made = CompanyNoticeValues.ForCompany(new NewOrderNotice(order, "2026-000001", PaymentMethod.BankTransfer, 120m, Start));
        Guid firm = await AddRowAsync(
            made.Type, "firma@example.com", null, made.RelatedRecordType, made.RelatedRecordId, made.EventValues);
        _gateway.DefaultSend = new EmailSendResult.ChannelError("Down");

        Assert.Equal(DispatchSendOutcome.Attempted, await SendAsync(firm));
        Assert.Single(_gateway.Messages);
        for (int attempt = 0; attempt < 3; attempt++)
        {
            _time.Advance(TimeSpan.FromHours(2));
            await SendAsync(firm);
        }

        Assert.Equal(DispatchState.Undelivered, (await RowAsync(firm)).DispatchState);
        Assert.False(await HasMarkAsync(order));
    }

    // ---- helpers ----

    private CustomerNotice Notice(DispatchType type, Guid order, Guid related)
    {
        NoticeAddress address = new("Ali Veli", "Atatürk Bulvarı 1", "Çankaya", _province, "3121234567");
        NoticeItem item = new("Ahşap Masa", "Meşe", "120 cm", 2);
        return type switch
        {
            DispatchType.B1 => new OrderReceivedNotice(order),
            DispatchType.B2 => new TransferChosenNotice(order, Start.AddDays(5)),
            DispatchType.B3 => new TransferReminderNotice(order, Start.AddDays(2)),
            DispatchType.B4 => new PaymentConfirmedNotice(order),
            DispatchType.B5 => new ShippedNotice(order, "ArasKargo", null, "A-9", false),
            DispatchType.B6 => new DeliveryFailedNotice(order),
            DispatchType.B7 => new CancelledNotice(order, ActorKind.Admin, CancellationReason.Other, "açıklama", null, new CancelledItem("Masa", "Meşe", "120 cm", 2)),
            DispatchType.B8 => new RefundProcessedNotice(order, RefundType.AmountBased, 266.67m, PaymentMethod.Card),
            DispatchType.B9 => new DeclarationReceivedNotice(order, DispatchRelatedRecordType.Withdrawal, related, false, DateOnly.FromDateTime(Start.UtcDateTime), null, [item], address),
            DispatchType.B10 => new AddressCorrectedNotice(order, AddressType.ShippingAddress, address),
            DispatchType.B11 => new ItemRemovedNotice(order, item),
            DispatchType.B12 => new CarrierCorrectedNotice(order, "ArasKargo", null, "A-9", false),
            DispatchType.B13 => new StatusCorrectedNotice(order, OrderStatus.Placed, null),
            DispatchType.B14 => new RefundIbanRequestedNotice(order, RefundIbanRequestReason.TransferFailed),
            DispatchType.B15 => new OrderEmailChangedNotice(order, Start),
            DispatchType.B16 => new ReturnRejectedNotice(order, item, WithdrawalExemptionReason.HygieneSensitive),
            _ => throw new ArgumentOutOfRangeException(nameof(type)),
        };
    }

    private ServiceProvider BuildProvider()
    {
        ServiceCollection services = new();
        services.AddLogging();
        services.AddDbContext<AppDbContext>(options => AppDbContext.Configure(options, _collection.Dsn));
        services.AddScoped<JobEnqueueTracker>();
        services.AddSingleton<IJobSignal, JobSignal>();
        services.AddScoped<ITransactionRunner, TransactionRunner>();
        services.AddAppCache();
        services.AddScoped<AdvisoryLocks>();
        services.AddSingleton<TimeProvider>(_time);
        services.AddScoped<IJobQueue, JobQueue>();
        services.AddFileStore();
        services.AddOptions<StatusThresholdOptions>();
        services.AddSingleton<Shopfolio.Infrastructure.Status.IDiskUsageReader, Shopfolio.Infrastructure.Status.DriveDiskUsageReader>();
        services.AddSingleton<Shopfolio.Infrastructure.Status.DiskUsageGuard>();
        services.AddSingleton<ImageProcessor>();
        services.AddCatalogModule();
        services.AddAfterSalesModule();
        services.AddNoticeDependencies();
        services.AddOrderingModule();
        services.AddPaymentModule();
        services.AddScoped<AttemptCounter>();
        services.AddCorporateContentModule();
        services.AddScoped<ICompanyContactEmail, FixedCompanyContactEmail>();
        services.AddSingleton<IReadOnlyList<ConditionalLine>>([new ConditionalLine(ConditionalLine.CardPayment, [])]);
        services.AddSingleton(Options.Create(new SiteOptions { Domain = "magaza.example.com" }));
        services.AddSingleton<SiteAddress>();
        services.AddScoped<IAuditActorSource>(_ => new FixedActor(_admin));
        services.AddScoped<Shopfolio.Modules.Administration.Contracts.IAuditTrail, AuditTrail>();
        return services.BuildServiceProvider();
    }

    private async Task<DispatchSendOutcome> SendAsync(Guid id)
    {
        await using AsyncServiceScope scope = _provider.CreateAsyncScope();
        IServiceProvider services = scope.ServiceProvider;
        AppDbContext db = services.GetRequiredService<AppDbContext>();
        ITransactionRunner runner = services.GetRequiredService<ITransactionRunner>();
        DispatchAttempts attempts = new(db, runner, _gateway, _time);
        DispatchSender sender = new(
            db,
            runner,
            _time,
            attempts,
            services.GetRequiredService<RecipientLimitCheck>(),
            services.GetRequiredService<AccountEmailBodies>(),
            services.GetRequiredService<EmailComposer>(),
            services.GetRequiredService<EmailLinks>(),
            services.GetRequiredService<ISettingsModule>(),
            services.GetRequiredService<ICorporateContentModule>(),
            services.GetRequiredService<OrderNoticeContexts>(),
            services.GetRequiredService<CustomerNoticeTemplates>());
        return await sender.SendAsync(id, Ct);
    }

    private async Task<NoticeResendResult> ResendAsync(Guid order, Guid latestRow, string? currentEmail = null)
    {
        await using AsyncServiceScope scope = _provider.CreateAsyncScope();
        INotificationsModule module = scope.ServiceProvider.GetRequiredService<INotificationsModule>();
        ITransactionRunner runner = scope.ServiceProvider.GetRequiredService<ITransactionRunner>();
        NoticeResendResult result = NoticeResendResult.NotMarked;
        string number = await TextAsync($"SELECT order_number FROM orders WHERE id = '{order}'");
        await runner.RunAsync(
            async cancel =>
            {
                result = await module.ResendCustomerNoticeAsync(new NoticeResendRequest(order, number, latestRow, currentEmail ?? CurrentAddress), cancel);
                return new Outcome(result != NoticeResendResult.Queued);
            },
            Ct);
        return result;
    }

    private async Task<bool> HasMarkAsync(Guid order)
    {
        await using AppDbContext db = NewContext();
        return await DispatchMarks.HasMarkAsync(db, order, Ct);
    }

    private async Task<Guid> QueueAsync(CustomerNotice notice, string recipient)
    {
        CustomerNoticeRow made = CustomerNoticeValues.For(notice);
        Guid row = await AddRowAsync(made.Type, recipient, notice.OrderId, made.RelatedRecordType, made.RelatedRecordId, made.EventValues, made.OriginalDispatchId);
        return row;
    }

    private async Task<Guid> AddRowAsync(
        DispatchType type,
        string recipient,
        Guid? orderId,
        DispatchRelatedRecordType? relatedType,
        Guid? relatedId,
        string? eventValues = null,
        Guid? original = null)
    {
        DispatchRecord row = DispatchRecord.Queue(type, recipient, orderId, relatedType, relatedId, Start);
        row.EventValues = eventValues;
        row.OriginalDispatchId = original;
        await using AppDbContext db = NewContext();
        db.DispatchRecords.Add(row);
        await db.SaveChangesAsync(Ct);
        return row.Id;
    }

    private async Task<Guid> AddSentAsync(DispatchType type, string recipient, Guid order, DateTimeOffset at)
    {
        DispatchRecord row = DispatchRecord.Queue(type, recipient, order, null, null, at);
        row.DispatchState = DispatchState.Sent;
        row.LastOutcome = DispatchOutcome.Sent;
        row.AttemptCount = 1;
        row.SentAt = at;
        row.NextAttemptAt = null;
        await using AppDbContext db = NewContext();
        db.DispatchRecords.Add(row);
        await db.SaveChangesAsync(Ct);
        return row.Id;
    }

    private async Task MakeUndeliveredAsync(Guid id)
    {
        await using AppDbContext db = NewContext();
        DispatchRecord row = await db.DispatchRecords.SingleAsync(d => d.Id == id, Ct);
        row.DispatchState = DispatchState.Undelivered;
        row.LastOutcome = DispatchOutcome.ChannelError;
        row.AttemptCount = DispatchRetrySchedule.AttemptLimit;
        row.NextAttemptAt = null;
        await db.SaveChangesAsync(Ct);
    }

    private async Task<DispatchRecord> RowAsync(Guid id)
    {
        await using AppDbContext db = NewContext();
        return await db.DispatchRecords.AsNoTracking().SingleAsync(d => d.Id == id, Ct);
    }

    private async Task<DispatchRecord> LatestAsync(Guid order)
    {
        await using AppDbContext db = NewContext();
        return await db.DispatchRecords.AsNoTracking().Where(d => d.OrderId == order).OrderByDescending(d => d.CreatedAt).ThenByDescending(d => d.Id).FirstAsync(Ct);
    }

    private async Task<string> OrderFingerprintAsync(Guid order) =>
        await TextAsync($"SELECT row_to_json(o)::text FROM orders o WHERE o.id = '{order}'");

    private async Task<(string Operation, string Target, string Details)> LastTrailAsync()
    {
        await using NpgsqlConnection connection = new(_collection.Dsn);
        await connection.OpenAsync(Ct);
        await using NpgsqlCommand command = new("SELECT operation::text, target_record_type, coalesce(details::text, '') FROM audit_logs ORDER BY created_at DESC, id DESC LIMIT 1", connection);
        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(Ct);
        Assert.True(await reader.ReadAsync(Ct));
        return (reader.GetString(0), reader.GetString(1), reader.GetString(2));
    }

    private async Task<Guid> SeedOrderAsync()
    {
        int n = Interlocked.Increment(ref _orderCounter);
        await using AppDbContext db = NewContext();
        LegalTextVersion form = await db.LegalTextVersions.SingleAsync(v => v.LegalTextType == LegalTextType.PreInformationForm, Ct);
        LegalTextVersion contract = await db.LegalTextVersions.SingleAsync(v => v.LegalTextType == LegalTextType.DistanceSalesContract, Ct);
        LegalTextVersion privacy = await db.LegalTextVersions.SingleAsync(v => v.LegalTextType == LegalTextType.PrivacyNotice, Ct);
        Order order = Order.Create(
            $"2026-{n:D6}", $"access-key-{n}", $"confirm-placeholder-{n}", PaymentMethod.BankTransfer, true, "Ali@Example.com", 120m, 0m, Start.AddHours(1), Start);
        order.BankTransferIban = Iban;
        order.IpAddress = IPAddress.Parse("203.0.113.5");
        order.CartId = EntityIds.New();
        order.BillingRecipientName = "Ali Veli";
        order.BillingProvinceId = _province;
        order.BillingDistrict = "Çankaya";
        order.BillingAddressLine = "Atatürk Bulvarı 1";
        order.CompanyType = CompanyType.Tradesman;
        order.CompanyLegalName = "Örnek Ticaret";
        order.CompanyTaxIdNumber = "1234567890";
        order.CompanyProvinceId = _province;
        order.CompanyDistrict = "Çankaya";
        order.CompanyAddressLine = "Kızılay 2";
        order.CompanyPhone = "3121234567";
        order.CompanyEmail = "firma@example.com";
        order.CompanyKepAddress = "firma@hs01.kep.tr";
        order.PreInformationFormVersionId = form.Id;
        order.DistanceSalesContractVersionId = contract.Id;
        order.PrivacyNoticeVersionId = privacy.Id;
        order.FreezeDocuments("ön bilgilendirme", "mesafeli satış");
        db.Orders.Add(order);
        await db.SaveChangesAsync(Ct);
        return order.Id;
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

    private async Task<string> TextAsync(string sql)
    {
        await using NpgsqlConnection connection = new(_collection.Dsn);
        await connection.OpenAsync(Ct);
        await using NpgsqlCommand command = new(sql, connection);
        return (string)(await command.ExecuteScalarAsync(Ct))!;
    }

    private async Task<long> CountAsync(string table)
    {
        await using NpgsqlConnection connection = new(_collection.Dsn);
        await connection.OpenAsync(Ct);
        await using NpgsqlCommand command = new($"SELECT count(*) FROM {table}", connection);
        return (long)(await command.ExecuteScalarAsync(Ct))!;
    }

    private sealed record Outcome(bool Rejected) : IOperationResult
    {
        public bool IsRejected => Rejected;
    }

    private sealed class FixedCompanyContactEmail : ICompanyContactEmail
    {
        public Task<string?> GetAsync(CancellationToken ct) => Task.FromResult<string?>("firma-iletisim@example.com");
    }

    private sealed class FixedActor : IAuditActorSource
    {
        private readonly Guid _adminId;

        public FixedActor(Guid adminId)
        {
            _adminId = adminId;
        }

        public Task<AuditActor?> CurrentAsync(CancellationToken ct) => Task.FromResult<AuditActor?>(new AuditActor(_adminId, "Firma Yöneticisi", "yonetici@example.com"));
    }
}
