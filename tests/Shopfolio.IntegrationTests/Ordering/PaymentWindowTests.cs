using System.Net;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Npgsql;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Files;
using Shopfolio.Infrastructure.Imaging;
using Shopfolio.Infrastructure.Integrations;
using Shopfolio.Infrastructure.Jobs;
using Shopfolio.Infrastructure.Values;
using Shopfolio.IntegrationTests.Database;
using Shopfolio.IntegrationTests.Model;
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
using Shopfolio.Modules.Settings.Contracts;
using Shopfolio.UnitTests.Fakes;

namespace Shopfolio.IntegrationTests.Ordering;

/// <summary>
/// The payment window's time-outs on a real PostgreSQL and a fake clock (T324 criteria 4-8, KK4-KK8; 03 4.1.7-4.1.9; 02 6.1.2; 06 4.14 path 3; D-100):
/// the transfer order's system cancellation with the release of its reservation, the refused "paid" mark after it, the B-3 reminder once per moment
/// and not after the window, the card window's final query - Ö1, the cancellation, the unanswered query that leaves the order with the "payment
/// outcome unknown" mark and is asked again - and the window's end read from the firm's setting and from the product constant.
/// </summary>
[Collection(OrderTransitionsDefinition.Name)]
public sealed class PaymentWindowTests : IAsyncLifetime
{
    private const string ContactEmail = "Ali@Example.com";

    private static readonly DateTimeOffset Start = new(2026, 11, 9, 9, 0, 0, TimeSpan.Zero);

    private readonly CollectionDatabaseFixture _collection;
    private readonly ManualTimeProvider _clock = new(Start);
    private ServiceProvider _provider = null!;
    private int _counter;

    public PaymentWindowTests(CollectionDatabaseFixture collection)
    {
        _collection = collection;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private DateTimeOffset Now => _clock.GetUtcNow();

    public async ValueTask InitializeAsync()
    {
        await ExecuteAsync(
            "TRUNCATE stock_reservations, dispatch_records, jobs, payment_records, cancellations, internal_notes, transition_records, order_items, orders, "
            + "coupons, variants, products, heartbeat_records, outage_records, legal_text_versions, provinces CASCADE");
        _provider = BuildProvider();
    }

    public async ValueTask DisposeAsync() => await _provider.DisposeAsync();

    // ---- KK6: Z-8, the transfer order's cancellation ----

    [Fact]
    public async Task TransferWindowEnd_CancelsTheOrder_WithS3AndO2AndB7_AndReleasesItsReservation_KK6()
    {
        Order order = await SeedOrderAsync(PaymentMethod.BankTransfer, Start.AddMinutes(-1));
        (Guid variantId, Guid couponId) = await SeedReservationAsync(order.Id, units: 2);

        IReadOnlyList<DueRecord> due = await FindDueAsync<TransferPaymentExpiry>();
        Assert.Equal([order.Id], due.Select(d => d.Id));
        await ProcessAsync<TransferPaymentExpiry>(order.Id);

        await using AppDbContext reader = NewContext();
        Order after = await reader.Orders.SingleAsync(o => o.Id == order.Id, Ct);
        Assert.Equal((OrderStatus.Cancelled, PaymentStatus.Failed, 2), (after.OrderStatus, after.PaymentStatus, after.Version));
        List<TransitionRecord> records = await reader.TransitionRecords.Where(t => t.OrderId == order.Id).ToListAsync(Ct);
        Assert.Equal(new[] { TransitionCode.O2, TransitionCode.S3 }.Order(), records.Select(r => r.TransitionCode).Order());
        Assert.All(records, r => Assert.Equal((ActorKind.System, TransitionReason.PaymentWindowExpired), (r.ActorKind, r.TransitionReason!.Value)));
        Assert.Equal([DispatchType.B7], await reader.DispatchRecords.Where(d => d.OrderId == order.Id).Select(d => d.DispatchType).ToListAsync(Ct));

        // The release (06 4.14 path 3): the rows are gone, the reserved quantity and the reserved right went down, the stock did not move.
        Assert.False(await reader.StockReservations.AnyAsync(r => r.OrderId == order.Id, Ct));
        Variant variant = await reader.Variants.SingleAsync(v => v.Id == variantId, Ct);
        Assert.Equal((5, 0), (variant.StockQuantity!.Value, variant.ReservedQuantity));
        Coupon coupon = await reader.Coupons.SingleAsync(c => c.Id == couponId, Ct);
        Assert.Equal((0, 0), (coupon.ReservedCount, coupon.UsedCount));
    }

    [Fact]
    public async Task CancelledTransferOrder_CannotBeMarkedPaidAfterwards_KK6()
    {
        Order order = await SeedOrderAsync(PaymentMethod.BankTransfer, Start.AddMinutes(-1));
        await ProcessAsync<TransferPaymentExpiry>(order.Id);

        await using AsyncServiceScope scope = _provider.CreateAsyncScope();
        PaymentTransitionResult result = await scope.ServiceProvider.GetRequiredService<ITransactionRunner>().RunAsync(
            cancel => scope.ServiceProvider.GetRequiredService<IOrderTransitions>().MarkTransferPaidAsync(new TransferPaidMark(order.Id, Guid.NewGuid(), "Yönetici", 2, true), cancel),
            Ct);

        Assert.True(result.IsRejected);
        await using AppDbContext reader = NewContext();
        Assert.Equal(OrderStatus.Cancelled, (await reader.Orders.SingleAsync(o => o.Id == order.Id, Ct)).OrderStatus);
        Assert.False(await reader.PaymentRecords.AnyAsync(p => p.OrderId == order.Id, Ct));
    }

    [Fact]
    public async Task TransferWindow_ThatStillRuns_OrAPaidOrder_IsNotCancelled()
    {
        Order running = await SeedOrderAsync(PaymentMethod.BankTransfer, Start.AddMinutes(1));
        Order paid = await SeedOrderAsync(PaymentMethod.BankTransfer, Start.AddMinutes(-1), OrderStatus.Preparing, PaymentStatus.Paid);
        Order card = await SeedOrderAsync(PaymentMethod.Card, Start.AddMinutes(-1));

        Assert.Empty(await FindDueAsync<TransferPaymentExpiry>());
        await ProcessAsync<TransferPaymentExpiry>(running.Id);
        await ProcessAsync<TransferPaymentExpiry>(paid.Id);
        await ProcessAsync<TransferPaymentExpiry>(card.Id);

        await using AppDbContext reader = NewContext();
        Assert.Equal(3, await reader.Orders.CountAsync(o => o.Version == 1 && o.OrderStatus != OrderStatus.Cancelled, Ct));
        Assert.False(await reader.TransitionRecords.AnyAsync(Ct));
    }

    // ---- KK7: Z-9, the reminder ----

    [Fact]
    public async Task Reminder_GoesOnceAtItsMoment_AndOnceMoreWhenACorrectionWritesANewMoment_KK7()
    {
        DateTimeOffset windowEnd = Start.AddDays(2);
        Order order = await SeedOrderAsync(PaymentMethod.BankTransfer, windowEnd, reminderAt: Start.AddMinutes(1));

        Assert.Empty(await FindDueAsync<TransferPaymentReminder>());
        _clock.Advance(TimeSpan.FromMinutes(1));
        Assert.Equal([order.Id], (await FindDueAsync<TransferPaymentReminder>()).Select(d => d.Id));
        await ProcessAsync<TransferPaymentReminder>(order.Id);
        await ProcessAsync<TransferPaymentReminder>(order.Id);
        Assert.Empty(await FindDueAsync<TransferPaymentReminder>());
        Assert.Equal(1, await CountRemindersAsync(order.Id));
        Assert.Null((await ReadOrderAsync(order.Id)).PaymentReminderAt);

        // The correction of the "paid" mark starts a new window and writes its end and its reminder moment (03 4.1.9; K-685; T319): the reminder goes once more.
        await ExecuteAsync($"UPDATE orders SET payment_window_ends_at = '{windowEnd.AddDays(1):O}', payment_reminder_at = '{Now:O}' WHERE id = '{order.Id}'");
        await ProcessAsync<TransferPaymentReminder>(order.Id);
        Assert.Equal(2, await CountRemindersAsync(order.Id));
        DispatchRecord reminder = (await ReadRemindersAsync(order.Id)).First();
        Assert.Equal(ContactEmail, reminder.RecipientEmail);
    }

    [Fact]
    public async Task MissedReminder_GoesOnTheReturnIfTheWindowStillRuns_AndNotIfItEnded_KK7()
    {
        Order running = await SeedOrderAsync(PaymentMethod.BankTransfer, Start.AddHours(5), reminderAt: Start.AddHours(-3));
        Order ended = await SeedOrderAsync(PaymentMethod.BankTransfer, Start.AddMinutes(-1), reminderAt: Start.AddHours(-3));

        Assert.Equal([running.Id], (await FindDueAsync<TransferPaymentReminder>()).Select(d => d.Id));
        await ProcessAsync<TransferPaymentReminder>(running.Id);
        await ProcessAsync<TransferPaymentReminder>(ended.Id);

        Assert.Equal(1, await CountRemindersAsync(running.Id));
        Assert.Equal(0, await CountRemindersAsync(ended.Id));
        Assert.Equal(Start.AddHours(-3), (await ReadOrderAsync(ended.Id)).PaymentReminderAt);
    }

    [Fact]
    public async Task WakeUpBeforeTheReturnsFirstHeartbeat_SendsNoReminderForAnEndedWindow_AndCancelsNothing_D100()
    {
        // The site was down for an hour; the scan's first wake-up comes before the return's first heartbeat - the last beat is old and the deferral has
        // not run, so the ended window still carries its reminder moment (05 5.4.3; D-100).
        await SeedHeartbeatAsync(Start.AddHours(-1));
        Order ended = await SeedOrderAsync(PaymentMethod.BankTransfer, Start.AddMinutes(-30), reminderAt: Start.AddHours(-2));
        Order running = await SeedOrderAsync(PaymentMethod.BankTransfer, Start.AddDays(1), reminderAt: Start.AddMinutes(-30));

        await _provider.GetRequiredService<SchedulerScan>().RunAsync(Ct);

        Order endedAfter = await ReadOrderAsync(ended.Id);
        Assert.Equal((OrderStatus.Placed, PaymentStatus.Pending, 1), (endedAfter.OrderStatus, endedAfter.PaymentStatus, endedAfter.Version));
        Assert.Equal(0, await CountRemindersAsync(ended.Id));
        Assert.Equal(1, await CountRemindersAsync(running.Id));
    }

    // ---- KK4, KK5: Z-7, the card window's final query ----

    [Fact]
    public async Task CardWindowEnd_WritesOneFinalQueryRow_AndLeavesTheOrderAsItIs()
    {
        Order order = await SeedOrderAsync(PaymentMethod.Card, Start.AddMinutes(-1));
        Order running = await SeedOrderAsync(PaymentMethod.Card, Start.AddMinutes(1));

        Assert.Equal([order.Id], (await FindDueAsync<CardPaymentExpiry>()).Select(d => d.Id));
        await ProcessAsync<CardPaymentExpiry>(order.Id);
        await ProcessAsync<CardPaymentExpiry>(order.Id);
        await ProcessAsync<CardPaymentExpiry>(running.Id);

        await using AppDbContext reader = NewContext();
        Job row = Assert.Single(await reader.Jobs.ToListAsync(Ct));
        Assert.Equal((JobType.FinalPaymentQuery, order.Id, JobState.Pending), (row.JobType, row.RelatedRecordId!.Value, row.JobState));
        Assert.Equal(1, (await ReadOrderAsync(order.Id)).Version);
    }

    [Fact]
    public async Task CardWindowEnd_WritesTheQueryAgain_WhenTheRunnerGaveUpOnTheRow_KK5()
    {
        Order order = await SeedOrderAsync(PaymentMethod.Card, Start.AddMinutes(-1));
        await ProcessAsync<CardPaymentExpiry>(order.Id);
        await ExecuteAsync($"UPDATE jobs SET job_state = 'Failed', next_attempt_at = NULL WHERE related_record_id = '{order.Id}'");

        // The order still waits with its window ended, so it stays due; the next wake-up writes a new pending row (03 4.1.7: the query does not stop).
        Assert.Equal([order.Id], (await FindDueAsync<CardPaymentExpiry>()).Select(d => d.Id));
        await ProcessAsync<CardPaymentExpiry>(order.Id);

        await using AppDbContext reader = NewContext();
        List<Job> rows = await reader.Jobs.Where(j => j.RelatedRecordId == order.Id).ToListAsync(Ct);
        Assert.Equal(new[] { JobState.Pending, JobState.Failed }.Order(), rows.Select(r => r.JobState).Order());
    }

    [Fact]
    public async Task FinalQuery_ThatFindsThePayment_AppliesO1WithTheProvidersTransactionId_KK4()
    {
        Order order = await SeedOrderAsync(PaymentMethod.Card, Start.AddMinutes(-1));
        FakePaymentGateway provider = new();
        provider.EnqueueQuery(new PaymentQueryResult.Paid("TX-324-1", 120m, 120m, true, true, false));

        JobOutcome outcome = await HandleAsync(provider, order.Id);

        Assert.Equal(JobOutcome.Completed, outcome);
        Assert.Equal(order.OrderNumber, Assert.Single(provider.Calls).Argument);
        await using AppDbContext reader = NewContext();
        Order after = await reader.Orders.SingleAsync(o => o.Id == order.Id, Ct);
        Assert.Equal((OrderStatus.Preparing, PaymentStatus.Paid), (after.OrderStatus, after.PaymentStatus));
        Assert.Equal("TX-324-1", (await reader.PaymentRecords.SingleAsync(p => p.OrderId == order.Id, Ct)).ProviderTransactionId);
    }

    [Fact]
    public async Task FinalQuery_ThatFindsNoPayment_CancelsTheOrder_AndReleasesItsReservation_KK4()
    {
        Order order = await SeedOrderAsync(PaymentMethod.Card, Start.AddMinutes(-1));
        (Guid variantId, _) = await SeedReservationAsync(order.Id, units: 1);
        FakePaymentGateway provider = new();
        provider.EnqueueQuery(new PaymentQueryResult.NoPayment());

        Assert.Equal(JobOutcome.Completed, await HandleAsync(provider, order.Id));

        await using AppDbContext reader = NewContext();
        Order after = await reader.Orders.SingleAsync(o => o.Id == order.Id, Ct);
        Assert.Equal((OrderStatus.Cancelled, PaymentStatus.Failed), (after.OrderStatus, after.PaymentStatus));
        Assert.Equal(
            TransitionReason.PaymentWindowExpired,
            (await reader.TransitionRecords.SingleAsync(t => t.OrderId == order.Id && t.TransitionCode == TransitionCode.S3, Ct)).TransitionReason);
        Assert.False(await reader.StockReservations.AnyAsync(Ct));
        Assert.Equal(0, await reader.Variants.Where(v => v.Id == variantId).Select(v => v.ReservedQuantity).SingleAsync(Ct));
    }

    [Theory]
    [InlineData(ExternalCallFailureKind.TimedOut)]
    [InlineData(ExternalCallFailureKind.Unreachable)]
    [InlineData(ExternalCallFailureKind.Unrecognized)]
    public async Task UnansweredFinalQuery_CancelsNothing_TheOrderCarriesTheUnknownMark_AndTheQueryIsAskedAgainUntilAnAnswer_KK5(ExternalCallFailureKind kind)
    {
        Order order = await SeedOrderAsync(PaymentMethod.Card, Start.AddMinutes(-1));
        FakePaymentGateway provider = new() { Outage = kind };

        for (int round = 1; round <= 3; round++)
        {
            JobOutcome outcome = await HandleAsync(provider, order.Id);
            Assert.Equal(JobOutcome.RescheduleAfter(JobTimings.FinalPaymentQueryInterval), outcome);
            Order waiting = await ReadOrderAsync(order.Id);
            Assert.Equal((OrderStatus.Placed, PaymentStatus.Pending, 1), (waiting.OrderStatus, waiting.PaymentStatus, waiting.Version));
            Assert.True(PaymentWindowRules.IsPaymentOutcomeUnknown(waiting, Now));
            _clock.Advance(JobTimings.FinalPaymentQueryInterval);
        }

        // The provider answers: the ordinary transition runs and the mark is gone (03 4.1.7).
        provider.Outage = null;
        provider.EnqueueQuery(new PaymentQueryResult.Paid("TX-324-2", 120m, 120m, true, true, false));
        Assert.Equal(JobOutcome.Completed, await HandleAsync(provider, order.Id));
        Order paid = await ReadOrderAsync(order.Id);
        Assert.Equal(PaymentStatus.Paid, paid.PaymentStatus);
        Assert.False(PaymentWindowRules.IsPaymentOutcomeUnknown(paid, Now));
        Assert.Equal(4, provider.Calls.Count);
    }

    [Fact]
    public async Task FinalQuery_ForAnOrderNoLongerWaiting_AsksNothing_AndANoPaymentBeforeTheWindowsEnd_CancelsNothing()
    {
        Order paid = await SeedOrderAsync(PaymentMethod.Card, Start.AddMinutes(-1), OrderStatus.Preparing, PaymentStatus.Paid);
        Order early = await SeedOrderAsync(PaymentMethod.Card, Start.AddMinutes(5));
        FakePaymentGateway provider = new();

        Assert.Equal(JobOutcome.Completed, await HandleAsync(provider, paid.Id));
        Assert.Empty(provider.Calls);
        Assert.Equal(JobOutcome.Completed, await HandleAsync(provider, early.Id));

        Order after = await ReadOrderAsync(early.Id);
        Assert.Equal((OrderStatus.Placed, PaymentStatus.Pending, 1), (after.OrderStatus, after.PaymentStatus, after.Version));
    }

    // ---- KK8: the window's length ----

    [Fact]
    public async Task TransferWindow_FollowsTheFirmSetting_AndTheCardWindow_TheProductConstant_KK8()
    {
        // Monday 2026-11-09, noon in Turkey: one business day ends at 00:00 of Wednesday, three at 00:00 of the Friday after (no holiday in the week).
        DateTimeOffset placed = Start;
        PaymentWindow one;
        PaymentWindow three;
        PaymentWindow card;
        await SeedCompanySettingsAsync();
        try
        {
            one = await WindowAsync(PaymentMethod.BankTransfer, placed, businessDays: 1);
            three = await WindowAsync(PaymentMethod.BankTransfer, placed, businessDays: 3);
            card = await WindowAsync(PaymentMethod.Card, placed, businessDays: 3);
        }
        finally
        {
            // The collection's other tests start without the firm's settings row; it is taken away again.
            await ExecuteAsync("TRUNCATE company_settings CASCADE");
        }

        Assert.Equal(PeriodCounting.EndOf(CountedPeriod.BankTransferPaymentWindow, placed, 1, BusinessCalendar.Product), one.EndsAt);
        Assert.Equal(PeriodCounting.EndOf(CountedPeriod.BankTransferPaymentWindow, placed, 3, BusinessCalendar.Product), three.EndsAt);
        Assert.True(three.EndsAt > one.EndsAt);
        Assert.Equal(TransferSchedule.ReminderAt(three.EndsAt), three.ReminderAt);
        Assert.Equal(placed + TimeSpan.FromMinutes(ProductConstants.CardPaymentMinutes), card.EndsAt);
        Assert.Null(card.ReminderAt);
    }

    private async Task SeedCompanySettingsAsync()
    {
        await ExecuteAsync("TRUNCATE company_settings CASCADE");
        CompanySettings settings = CompanySettings.Create(Start);
        settings.BankTransferPaymentWindow = 1;
        settings.DownloadAllowance = 1;
        settings.HomepageLayout = Shopfolio.Modules.CorporateContent.Contracts.HomepageLayout.StoreFirst;
        settings.MenuNameProducts = "Ürünler";
        settings.MenuNameOfferings = "Hizmetler";
        settings.MenuNamePortfolioItems = "Çalışmalar";
        settings.MenuNameAboutPage = "Hakkımızda";
        settings.MenuNameFaqItems = "Sıkça Sorulanlar";
        settings.MenuNameContact = "İletişim";
        await using AppDbContext db = NewContext();
        db.CompanySettings.Add(settings);
        await db.SaveChangesAsync(Ct);
    }

    private async Task<PaymentWindow> WindowAsync(PaymentMethod method, DateTimeOffset start, int businessDays)
    {
        await ExecuteAsync($"UPDATE company_settings SET bank_transfer_payment_window = {businessDays}");
        await using ServiceProvider fresh = BuildProvider();
        await using AsyncServiceScope scope = fresh.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<PaymentWindows>().StartAsync(method, start, Ct);
    }

    private async Task<JobOutcome> HandleAsync(FakePaymentGateway provider, Guid orderId)
    {
        await using AsyncServiceScope scope = _provider.CreateAsyncScope();
        FinalPaymentQueryJobHandler handler = ActivatorUtilities.CreateInstance<FinalPaymentQueryJobHandler>(scope.ServiceProvider, provider);
        return await handler.HandleAsync(new JobWork(Guid.NewGuid(), JobType.FinalPaymentQuery, orderId, Now, 0), Ct);
    }

    private async Task<IReadOnlyList<DueRecord>> FindDueAsync<TSource>()
        where TSource : IDueRecordSource
    {
        await using AsyncServiceScope scope = _provider.CreateAsyncScope();
        return await Source<TSource>().FindDueAsync(scope.ServiceProvider, Now, Ct);
    }

    private async Task ProcessAsync<TSource>(Guid orderId)
        where TSource : IDueRecordSource
    {
        await using AsyncServiceScope scope = _provider.CreateAsyncScope();
        await Source<TSource>().ProcessAsync(scope.ServiceProvider, orderId, Ct);
    }

    private TSource Source<TSource>()
        where TSource : IDueRecordSource => _provider.GetServices<IDueRecordSource>().OfType<TSource>().Single();

    private async Task<int> CountRemindersAsync(Guid orderId) => (await ReadRemindersAsync(orderId)).Count;

    private async Task<List<DispatchRecord>> ReadRemindersAsync(Guid orderId)
    {
        await using AppDbContext reader = NewContext();
        return await reader.DispatchRecords.Where(d => d.OrderId == orderId && d.DispatchType == DispatchType.B3).ToListAsync(Ct);
    }

    private async Task<Order> ReadOrderAsync(Guid orderId)
    {
        await using AppDbContext reader = NewContext();
        return await reader.Orders.AsNoTracking().SingleAsync(o => o.Id == orderId, Ct);
    }

    private async Task SeedHeartbeatAsync(DateTimeOffset lastBeat)
    {
        await using AppDbContext db = NewContext();
        HeartbeatRecord heartbeat = HeartbeatRecord.Create(lastBeat);
        heartbeat.NightlyJobLastSucceededAt = Start;
        heartbeat.NightlyJobLastAttemptedAt = Start;
        db.HeartbeatRecords.Add(heartbeat);
        await db.SaveChangesAsync(Ct);
    }

    /// <summary>A variant with five in stock and a coupon, both with the order's reservation: the counters and the rows the confirmation wrote.</summary>
    private async Task<(Guid VariantId, Guid CouponId)> SeedReservationAsync(Guid orderId, int units)
    {
        int n = Interlocked.Increment(ref _counter);
        Product product = Product.Create("Kapı", $"kapi-t324-{n}", ProductType.Physical, 20m, Start).WithSeededOrigin();
        Variant variant = Variant.Create(product.Id, $"KAPI-T324-{n}", 100m, Start, 100);
        variant.StockQuantity = 5;
        Coupon coupon = Coupon.CreatePercentage($"T324K{n}", 10m, new DateOnly(2026, 11, 1), new DateOnly(2026, 11, 30), 10, Start);
        await using (AppDbContext db = NewContext())
        {
            db.Products.Add(product);
            db.Variants.Add(variant);
            db.Coupons.Add(coupon);
            db.StockReservations.Add(StockReservation.ForVariant(orderId, variant.Id, units, Start));
            db.StockReservations.Add(StockReservation.ForCoupon(orderId, coupon.Id, Start));
            await db.SaveChangesAsync(Ct);
        }

        await ExecuteAsync($"UPDATE variants SET reserved_quantity = {units} WHERE id = '{variant.Id}'");
        await ExecuteAsync($"UPDATE coupons SET reserved_count = 1 WHERE id = '{coupon.Id}'");
        return (variant.Id, coupon.Id);
    }

    private async Task<Order> SeedOrderAsync(
        PaymentMethod method,
        DateTimeOffset windowEndsAt,
        OrderStatus orderStatus = OrderStatus.Placed,
        PaymentStatus paymentStatus = PaymentStatus.Pending,
        DateTimeOffset? reminderAt = null)
    {
        int n = Interlocked.Increment(ref _counter);
        Province province = Province.Create($"{n:D2}", $"İl {n}", Start);
        LegalTextVersion form = LegalTextVersion.CreatePublished(LegalTextType.PreInformationForm, n, Start, "metin", Start);
        LegalTextVersion contract = LegalTextVersion.CreatePublished(LegalTextType.DistanceSalesContract, n, Start, "metin", Start);
        LegalTextVersion notice = LegalTextVersion.CreatePublished(LegalTextType.PrivacyNotice, n, Start, "metin", Start).WithHosting();
        Order order = Order.Create(
            $"2026-{n + 324000:D6}", $"access-placeholder-{n}", $"confirm-placeholder-{n}", method, true, ContactEmail, 120m, 0m, windowEndsAt, Start.AddDays(-1));
        order.IpAddress = IPAddress.Parse("203.0.113.5");
        order.CartId = EntityIds.New();
        order.BankTransferIban = method == PaymentMethod.BankTransfer ? "TR330006100519786457841326" : null;
        order.PaymentReminderAt = reminderAt;
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
        OrderItem item = OrderItem.Create(order.Id, null, null, ProductType.Physical, "Ürün", 1, 100m, 20m, 16.67m, Start);
        await using (AppDbContext db = NewContext())
        {
            db.Provinces.Add(province);
            db.LegalTextVersions.AddRange(form, contract, notice);
            db.Orders.Add(order);
            db.OrderItems.Add(item);
            await db.SaveChangesAsync(Ct);
        }

        if (orderStatus != OrderStatus.Placed || paymentStatus != PaymentStatus.Pending)
        {
            await ExecuteAsync($"UPDATE orders SET order_status = '{orderStatus}', payment_status = '{paymentStatus}' WHERE id = '{order.Id}'");
        }

        return order;
    }

    private ServiceProvider BuildProvider()
    {
        ServiceCollection services = new();
        services.AddDbContext<AppDbContext>(options => AppDbContext.Configure(options, _collection.Dsn));
        services.AddSingleton(NpgsqlDataSource.Create(_collection.Dsn));
        services.AddScoped<JobEnqueueTracker>();
        services.AddSingleton<IJobSignal, JobSignal>();
        services.AddScoped<ITransactionRunner, TransactionRunner>();
        services.AddAppCache();
        services.AddScoped<AdvisoryLocks>();
        services.AddSingleton<TimeProvider>(_clock);
        services.AddScoped<IJobQueue, JobQueue>();
        services.AddFileStore();
        services.AddOptions<Shopfolio.Infrastructure.InstallationSettings.StatusThresholdOptions>();
        services.AddSingleton<Shopfolio.Infrastructure.Status.IDiskUsageReader, Shopfolio.Infrastructure.Status.DriveDiskUsageReader>();
        services.AddSingleton<Shopfolio.Infrastructure.Status.DiskUsageGuard>();
        services.AddSingleton<ImageProcessor>();
        services.AddLogging(builder => builder.SetMinimumLevel(LogLevel.None));
        services.AddCatalogModule();
        services.AddAfterSalesModule();
        services.AddNoticeDependencies();
        services.AddOrderingModule();
        services.AddPaymentModule();
        services.AddSingleton<NightlyJobRunner>();
        services.AddSingleton<SchedulerScan>();
        return services.BuildServiceProvider();
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
}
