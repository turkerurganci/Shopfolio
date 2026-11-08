using System.Net;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Npgsql;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Files;
using Shopfolio.Infrastructure.Imaging;
using Shopfolio.Infrastructure.Jobs;
using Shopfolio.IntegrationTests.Database;
using Shopfolio.IntegrationTests.Model;
using Shopfolio.Modules.Administration;
using Shopfolio.Modules.AfterSales;
using Shopfolio.Modules.AfterSales.Contracts;
using Shopfolio.Modules.Catalog;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Notifications;
using Shopfolio.Modules.Ordering;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Modules.Payment;
using Shopfolio.Modules.Payment.Contracts;
using Shopfolio.Modules.Settings;
using Shopfolio.Modules.Settings.Contracts;
using Shopfolio.UnitTests.Fakes;

namespace Shopfolio.IntegrationTests.Ordering;

/// <summary>
/// The delivery, withdrawal and refund periods on a real PostgreSQL and a fake clock (T325 criteria 1-3, 6, 7, KK1-KK3, KK6, KK7; 03 4.1.10-4.1.18;
/// T350 criterion 7): the shipping promise counted from the card's success notice and from the transfer's "paid" mark - read from the last Ö1 record
/// that no correction took back -, the fulfilment period, the legal interest warning of a late cancellation, the late delivery termination on a shipped
/// order and its closing by the item's records, and that none of these periods acts when it runs out: no scheduled source finds the order, no job row
/// is written and the order, its items and its after-sales records stay as they were.
/// </summary>
[Collection(OrderTransitionsDefinition.Name)]
public sealed class DeliveryPeriodTests : IAsyncLifetime
{
    // Monday 9 November 2026, 12:00 in Turkey; the weeks around it carry no public holiday.
    private static readonly DateTimeOffset Start = new(2026, 11, 9, 9, 0, 0, TimeSpan.Zero);

    private static readonly DateOnly Today = TurkeyTime.DateOf(Start);

    private static readonly ItemAmounts Amounts = new(100m, 0m, 16.67m);

    private readonly CollectionDatabaseFixture _collection;
    private readonly ManualTimeProvider _clock = new(Start);
    private ServiceProvider _provider = null!;
    private Admin _admin = null!;
    private int _counter;

    public DeliveryPeriodTests(CollectionDatabaseFixture collection)
    {
        _collection = collection;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private DateTimeOffset Now => _clock.GetUtcNow();

    public async ValueTask InitializeAsync()
    {
        await ExecuteAsync(
            "TRUNCATE dispatch_records, jobs, refund_records, refunds, payment_records, not_returned_closures, returns, return_rejections, withdrawals, "
            + "late_delivery_terminations, cancellations, internal_notes, transition_records, stock_reservations, order_items, orders, admins, "
            + "legal_text_versions, provinces CASCADE");
        _provider = BuildProvider();
        _admin = Admin.Create("Firma Yöneticisi", "yonetici@example.com", "digest-placeholder", Start);
        await SaveAsync(c => c.Admins.Add(_admin));
    }

    public async ValueTask DisposeAsync() => await _provider.DisposeAsync();

    // ---- KK1: Z-10 from the card's success notice and from the transfer's "paid" mark ----

    [Fact]
    public async Task ShippingPromise_CountsFromTheCardsSuccessNotice_KK1()
    {
        (Order order, _) = await SeedAsync(PaymentMethod.Card, shippingPeriod: 2, ProductType.Physical);
        _clock.Advance(TimeSpan.FromHours(3));
        DateTimeOffset notice = Now;

        PaymentTransitionResult paid = await RunAsync((unit, token) => unit.ApplyCardPaymentAsync(new CardPaymentSucceeded(order.Id, "trx-t325"), token));

        Assert.Equal([TransitionCode.O1, TransitionCode.S1], paid.Applied);
        Assert.Equal(notice, await PeriodsAsync(p => p.PaymentApprovedAtAsync(order.Id, Ct)));
        PeriodStanding? standing = await PeriodsAsync(p => p.ShippingStandingAsync(order.Id, Now, Ct));
        Assert.Equal(DeliveryPeriodRules.ShippingLastDay(notice, 2, BusinessCalendar.Product), standing!.Value.LastDay);
        Assert.Equal(PeriodCounting.EndOf(CountedPeriod.ShippingPeriod, notice, 2, BusinessCalendar.Product), standing.Value.EndsAt);
        Assert.Equal((2, false), (standing.Value.DaysLeft, standing.Value.IsOver));
    }

    [Fact]
    public async Task ShippingPromise_CountsFromTheTransfersPaidMark_AndAfterItsCorrection_FromTheNewMark_KK1()
    {
        (Order order, _) = await SeedAsync(PaymentMethod.BankTransfer, shippingPeriod: 1, ProductType.Physical);
        _clock.Advance(TimeSpan.FromDays(1));
        DateTimeOffset firstMark = Now;
        Assert.Equal([TransitionCode.O1, TransitionCode.S1], (await RunAsync((unit, token) => unit.MarkTransferPaidAsync(Mark(order.Id, 1), token))).Applied);
        Assert.Equal(firstMark, await PeriodsAsync(p => p.PaymentApprovedAtAsync(order.Id, Ct)));

        // The mark is corrected: the payment is no longer approved and the promise does not run.
        Guid o1 = await RecordIdAsync(order.Id, TransitionCode.O1);
        _clock.Advance(TimeSpan.FromDays(1));
        Assert.False((await RunCoreAsync((core, token) => core.CorrectAsync(
            new CorrectionRequest(order.Id, o1, CorrectionReason.WrongOrder, null, TransitionActor.Admin(_admin.Id, _admin.Name), 2), null, token))).IsRejected);
        Assert.Null(await PeriodsAsync(p => p.PaymentApprovedAtAsync(order.Id, Ct)));
        Assert.Null(await PeriodsAsync(p => p.ShippingStandingAsync(order.Id, Now, Ct)));

        // A new mark is a new approval: the promise counts from it.
        _clock.Advance(TimeSpan.FromDays(2));
        DateTimeOffset secondMark = Now;
        Assert.False((await RunAsync((unit, token) => unit.MarkTransferPaidAsync(Mark(order.Id, 3), token))).IsRejected);
        Assert.Equal(secondMark, await PeriodsAsync(p => p.PaymentApprovedAtAsync(order.Id, Ct)));
        PeriodStanding? standing = await PeriodsAsync(p => p.ShippingStandingAsync(order.Id, Now, Ct));
        Assert.Equal(DeliveryPeriodRules.ShippingPeriodEnd(secondMark, 1, BusinessCalendar.Product), standing!.Value.EndsAt);
    }

    [Fact]
    public async Task ShippingAndFulfilmentPeriods_RunOut_WithoutAnAction_AndTheOrderStaysInItsCounter_KK1()
    {
        (Order order, List<OrderItem> items) = await SeedAsync(PaymentMethod.Card, shippingPeriod: 0, ProductType.Physical, ProductType.Service);
        await RunAsync((unit, token) => unit.ApplyCardPaymentAsync(new CardPaymentSucceeded(order.Id, "trx-t325-late"), token));
        Order paid = await ReadOrderAsync(order.Id);

        // Past both promises: the zero-day shipping promise (D-161) and the three-day fulfilment period.
        _clock.Advance(TimeSpan.FromDays(10));
        PeriodStanding? shipping = await PeriodsAsync(p => p.ShippingStandingAsync(order.Id, Now, Ct));
        PeriodStanding? fulfilment = await PeriodsAsync(p => p.FulfilmentStandingAsync(items[1].Id, Now, Ct));
        Assert.True(shipping!.Value.IsOver);
        Assert.True(fulfilment!.Value.IsOver);

        await AssertNothingActsAsync(order.Id);
        Order after = await ReadOrderAsync(order.Id);
        Assert.Equal((OrderStatus.Preparing, PaymentStatus.Paid, paid.Version), (after.OrderStatus, after.PaymentStatus, after.Version));
        await using AppDbContext reader = NewContext();
        Assert.Null((await reader.OrderItems.SingleAsync(i => i.Id == items[1].Id, Ct)).DeliveryMarkedAt);
    }

    [Fact]
    public async Task FulfilmentPeriod_IsNotShown_OnceTheServiceIsCompleted()
    {
        (Order order, List<OrderItem> items) = await SeedAsync(PaymentMethod.Card, shippingPeriod: null, ProductType.Service);
        await RunAsync((unit, token) => unit.ApplyCardPaymentAsync(new CardPaymentSucceeded(order.Id, "trx-t325-svc"), token));
        Assert.NotNull(await PeriodsAsync(p => p.FulfilmentStandingAsync(items[0].Id, Now, Ct)));

        await ExecuteAsync($"UPDATE order_items SET delivery_marked_at = '{Now:O}', delivery_date = '{Today:yyyy-MM-dd}' WHERE id = '{items[0].Id}'");

        Assert.Null(await PeriodsAsync(p => p.FulfilmentStandingAsync(items[0].Id, Now, Ct)));
    }

    // ---- KK2: the legal interest warning ----

    [Fact]
    public async Task CancellationAfterThePromise_CarriesTheLegalInterestWarning_AndBeforeIt_Not_KK2()
    {
        (Order order, List<OrderItem> items) = await SeedAsync(PaymentMethod.Card, shippingPeriod: 2, ProductType.Physical, ProductType.Service);
        await RunAsync((unit, token) => unit.ApplyCardPaymentAsync(new CardPaymentSucceeded(order.Id, "trx-t325-int"), token));
        DateTimeOffset shippingEnd = DeliveryPeriodRules.ShippingPeriodEnd(Now, 2, BusinessCalendar.Product);
        DateTimeOffset fulfilmentEnd = DeliveryPeriodRules.FulfilmentPeriodEnd(Now, 3);

        Assert.False(await PeriodsAsync(p => p.IsLegalInterestWarningDueAsync(items[0].Id, shippingEnd.AddTicks(-1), Ct)));
        Assert.True(await PeriodsAsync(p => p.IsLegalInterestWarningDueAsync(items[0].Id, shippingEnd, Ct)));
        Assert.False(await PeriodsAsync(p => p.IsLegalInterestWarningDueAsync(items[1].Id, fulfilmentEnd.AddTicks(-1), Ct)));
        Assert.True(await PeriodsAsync(p => p.IsLegalInterestWarningDueAsync(items[1].Id, fulfilmentEnd, Ct)));
    }

    // ---- KK3: Z-11 on a shipped order ----

    [Fact]
    public async Task LateDeliveryTermination_OpensAfterThirtyDays_OnAShippedOrder_AndClosesWithTheItemsRecords_KK3()
    {
        (Order order, List<OrderItem> items) = await SeedAsync(PaymentMethod.Card, shippingPeriod: 2, ProductType.Physical);
        await ExecuteAsync($"UPDATE orders SET order_status = 'Shipped', payment_status = 'Paid' WHERE id = '{order.Id}'");
        DateTimeOffset limit = DeliveryPeriodRules.LegalDeliveryLimitEnd(order.CreatedAt);

        Assert.False(await PeriodsAsync(p => p.IsLateDeliveryTerminationOpenAsync(order.Id, limit.AddTicks(-1), Ct)));
        Assert.True(await PeriodsAsync(p => p.IsLateDeliveryTerminationOpenAsync(order.Id, limit, Ct)));

        // The limit running out acts on nothing.
        _clock.SetUtcNow(limit.AddDays(1));
        await AssertNothingActsAsync(order.Id);
        Assert.Equal(OrderStatus.Shipped, (await ReadOrderAsync(order.Id)).OrderStatus);

        // The item's units closed by its record leave no open quantity: the way closes.
        await SaveAsync(c => c.LateDeliveryTerminations.Add(LateDeliveryTermination.ByBuyer(items[0].Id, 1, Today.AddDays(31), Amounts, null, Now)));
        Assert.False(await PeriodsAsync(p => p.IsLateDeliveryTerminationOpenAsync(order.Id, Now, Ct)));
    }

    // ---- KK6, KK7: the refund periods and the sending period act on nothing ----

    [Fact]
    public async Task RefundAndSendingPeriods_RunOut_WithoutAJobOrAnAction_AndTheRecordsStay_KK6_KK7()
    {
        (Order order, List<OrderItem> items) = await SeedAsync(PaymentMethod.BankTransfer, shippingPeriod: 2, ProductType.Physical, ProductType.Physical);
        await ExecuteAsync($"UPDATE orders SET order_status = 'Delivered', payment_status = 'Paid' WHERE id = '{order.Id}'");
        await ExecuteAsync($"UPDATE order_items SET delivery_marked_at = '{Now:O}', delivery_date = '{Today:yyyy-MM-dd}' WHERE order_id = '{order.Id}'");
        Withdrawal withdrawal = Withdrawal.ByBuyer(items[0].Id, 1, Today.AddDays(2), null, Amounts, null, Now);
        Cancellation cancellation = Cancellation.ByBuyer(items[1].Id, 1, Amounts, null, Now);
        await SaveAsync(c =>
        {
            c.Withdrawals.Add(withdrawal);
            c.Cancellations.Add(cancellation);
        });

        // Past Z-42, Z-16 and Z-17: the goods never came back, the refunds were never made.
        _clock.Advance(TimeSpan.FromDays(AfterSalesPeriodRules.ReturnShipmentPeriodDays + AfterSalesPeriodRules.RefundPeriodDays + 5));
        bool afterDelivery = AfterSalesPeriodRules.IsAfterDelivery(ProductType.Physical, Today, withdrawal.DeclarationDate, null);
        Assert.True(AfterSalesPeriodRules.IsNotReturnedClosureOpen(afterDelivery, 1, withdrawal.DeclarationDate, Now));
        Assert.Null(AfterSalesPeriodRules.WithdrawalRefundStartDay(afterDelivery, withdrawal.DeclarationDate, null));
        Assert.True(AfterSalesPeriodRules.CancellationRefundStanding(PaymentMethod.BankTransfer, cancellation.CreatedAt, Now)!.Value.IsOver);

        await AssertNothingActsAsync(order.Id);
        await using AppDbContext reader = NewContext();
        Assert.Equal(1, await reader.Withdrawals.CountAsync(w => w.OrderItemId == items[0].Id, Ct));
        Assert.False(await reader.NotReturnedClosures.AnyAsync(Ct));
        Assert.False(await reader.Returns.AnyAsync(Ct));
        Assert.Equal(OrderStatus.Delivered, (await ReadOrderAsync(order.Id)).OrderStatus);
    }

    // ---- helpers ----

    /// <summary>No scheduled source finds the order at the clock's moment, and no job row is written: a period that runs out acts on nothing.</summary>
    private async Task AssertNothingActsAsync(Guid orderId)
    {
        await using AsyncServiceScope scope = _provider.CreateAsyncScope();
        foreach (IDueRecordSource source in _provider.GetServices<IDueRecordSource>())
        {
            IReadOnlyList<DueRecord> due = await source.FindDueAsync(scope.ServiceProvider, Now, Ct);
            Assert.DoesNotContain(due, d => d.Id == orderId);
        }

        await using AppDbContext reader = NewContext();
        Assert.False(await reader.Jobs.AnyAsync(Ct));
    }

    private TransferPaidMark Mark(Guid orderId, int version) => new(orderId, _admin.Id, _admin.Name, version, true);

    private async Task<Guid> RecordIdAsync(Guid orderId, TransitionCode code)
    {
        await using AppDbContext reader = NewContext();
        return await reader.TransitionRecords.Where(t => t.OrderId == orderId && t.TransitionCode == code).Select(t => t.Id).SingleAsync(Ct);
    }

    private async Task<T> PeriodsAsync<T>(Func<OrderPeriods, Task<T>> read)
    {
        await using AsyncServiceScope scope = _provider.CreateAsyncScope();
        return await read(scope.ServiceProvider.GetRequiredService<OrderPeriods>());
    }

    private async Task<T> RunAsync<T>(Func<IOrderTransitions, CancellationToken, Task<T>> work)
        where T : IOperationResult
    {
        await using AsyncServiceScope scope = _provider.CreateAsyncScope();
        IOrderTransitions unit = scope.ServiceProvider.GetRequiredService<IOrderTransitions>();
        return await scope.ServiceProvider.GetRequiredService<ITransactionRunner>().RunAsync(token => work(unit, token), Ct);
    }

    private async Task<T> RunCoreAsync<T>(Func<OrderTransitions, CancellationToken, Task<T>> work)
        where T : IOperationResult
    {
        await using AsyncServiceScope scope = _provider.CreateAsyncScope();
        OrderTransitions core = scope.ServiceProvider.GetRequiredService<OrderTransitions>();
        return await scope.ServiceProvider.GetRequiredService<ITransactionRunner>().RunAsync(token => work(core, token), Ct);
    }

    private async Task<Order> ReadOrderAsync(Guid orderId)
    {
        await using AppDbContext reader = NewContext();
        return await reader.Orders.AsNoTracking().SingleAsync(o => o.Id == orderId, Ct);
    }

    /// <summary>A received, unpaid order with one item per type; the shipping promise frozen on it is <paramref name="shippingPeriod"/>.</summary>
    private async Task<(Order Order, List<OrderItem> Items)> SeedAsync(PaymentMethod method, int? shippingPeriod, params ProductType[] types)
    {
        int n = Interlocked.Increment(ref _counter);
        Province province = Province.Create($"{n % 100:D2}", $"İl {n}", Start);
        LegalTextVersion form = LegalTextVersion.CreatePublished(LegalTextType.PreInformationForm, n, Start, "metin", Start);
        LegalTextVersion contract = LegalTextVersion.CreatePublished(LegalTextType.DistanceSalesContract, n, Start, "metin", Start);
        LegalTextVersion notice = LegalTextVersion.CreatePublished(LegalTextType.PrivacyNotice, n, Start, "metin", Start).WithHosting();
        Order order = Order.Create(
            $"2026-{n + 325000:D6}", $"access-placeholder-{n}", $"confirm-placeholder-{n}", method, true, "Ali@Example.com", 120m, 0m, Start.AddHours(1), Start);
        order.IpAddress = IPAddress.Parse("203.0.113.5");
        order.CartId = EntityIds.New();
        order.FreezeShippingPromise(shippingPeriod);
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
        order.BankTransferIban = method == PaymentMethod.BankTransfer ? "TR330006100519786457841326" : null;

        List<OrderItem> items = [];
        foreach (ProductType type in types)
        {
            OrderItem item = OrderItem.Create(order.Id, null, null, type, "Ürün", 1, 100m, 20m, 16.67m, Start);
            item.DownloadAllowance = type == ProductType.Digital ? 5 : null;
            item.FulfilmentPeriod = type == ProductType.Service ? 3 : null;
            items.Add(item);
        }

        await SaveAsync(c =>
        {
            c.Provinces.Add(province);
            c.LegalTextVersions.AddRange(form, contract, notice);
            c.Orders.Add(order);
            c.OrderItems.AddRange(items);
        });
        return (order, items);
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
        return services.BuildServiceProvider();
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
}
