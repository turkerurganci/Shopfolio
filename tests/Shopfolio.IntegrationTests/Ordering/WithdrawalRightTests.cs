using System.Net;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Npgsql;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Files;
using Shopfolio.Infrastructure.Imaging;
using Shopfolio.Infrastructure.Jobs;
using Shopfolio.Infrastructure.Operations;
using Shopfolio.IntegrationTests.Database;
using Shopfolio.IntegrationTests.Model;
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
/// The withdrawal right on a real PostgreSQL (T371 criteria 2-6, 8; 02 7.3.1-7.3.4): the customer's withdrawal button as the order module's operation decision
/// reads it from the order's own records - the physical window from the delivery date, the service window from the order's date or the order's delivery
/// date and never pulled back when the physical items closed before the delivery, the shipped item withdrawable before it is delivered, the closed
/// window that leaves an earlier declaration standing and the firm's record of another channel open (m.10), the digital item that has none, and the
/// exemption reason frozen on the item: no button for an absolute one, the button for a conditional one.
/// </summary>
[Collection(OrderTransitionsDefinition.Name)]
public sealed class WithdrawalRightTests : IAsyncLifetime
{
    // Monday 9 November 2026, 12:00 Turkey time.
    private static readonly DateTimeOffset Start = new(2026, 11, 9, 9, 0, 0, TimeSpan.Zero);

    private static readonly DateOnly Today = TurkeyTime.DateOf(Start);

    private static readonly ItemAmounts Amounts = new(100m, 0m, 16.67m);

    private readonly CollectionDatabaseFixture _collection;
    private readonly ManualTimeProvider _clock = new(Start);
    private ServiceProvider _provider = null!;
    private int _counter;

    public WithdrawalRightTests(CollectionDatabaseFixture collection)
    {
        _collection = collection;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public async ValueTask InitializeAsync()
    {
        await ExecuteAsync(
            "TRUNCATE dispatch_records, jobs, refund_records, refunds, payment_records, not_returned_closures, returns, return_rejections, withdrawals, "
            + "late_delivery_terminations, cancellations, internal_notes, transition_records, stock_reservations, order_items, orders, admins, "
            + "legal_text_versions, provinces CASCADE");
        _provider = BuildProvider();
    }

    public async ValueTask DisposeAsync() => await _provider.DisposeAsync();

    [Fact]
    public async Task APhysicalItem_IsWithdrawableFromTheShipmentOn_UntilFourteenDaysAfterItsDeliveryDate_KK2_KK3()
    {
        (Order order, List<OrderItem> items) = await SeedAsync(ProductType.Physical);
        Guid item = items[0].Id;
        await ExecuteAsync($"UPDATE orders SET order_status = 'Shipped', payment_status = 'Paid' WHERE id = '{order.Id}'");

        // On the road, with no delivery date, the window has not started and the button stays open however long the parcel travels (K-340).
        _clock.SetUtcNow(Start.AddDays(60));
        Assert.True(await IsOpenAsync(order.Id, ActorKind.Buyer, OrderOperation.WithdrawPhysicalItem, item));

        await ExecuteAsync($"UPDATE orders SET order_status = 'Delivered' WHERE id = '{order.Id}'; UPDATE order_items SET delivery_date = '{Today:yyyy-MM-dd}', delivery_marked_at = '{Start:O}' WHERE id = '{item}'");
        DateTimeOffset end = AfterSalesPeriodRules.WithdrawalWindowEnd(Today);
        _clock.SetUtcNow(end.AddTicks(-1));
        Assert.True(await IsOpenAsync(order.Id, ActorKind.Buyer, OrderOperation.WithdrawPhysicalItem, item));
        _clock.SetUtcNow(end);
        Assert.False(await IsOpenAsync(order.Id, ActorKind.Buyer, OrderOperation.WithdrawPhysicalItem, item));
    }

    [Fact]
    public async Task AServiceItem_RunsFromTheOrderDate_OrFromTheOrdersDeliveryDate_AndIsNotPulledBackWhenThePhysicalItemsClosedBeforeTheDelivery_KK2_KK6()
    {
        DateTimeOffset end = AfterSalesPeriodRules.WithdrawalWindowEnd(Today);

        (Order alone, List<OrderItem> aloneItems) = await SeedAsync(ProductType.Service);
        await ExecuteAsync($"UPDATE orders SET order_status = 'Placed', payment_status = 'Paid' WHERE id = '{alone.Id}'");

        (Order mixed, List<OrderItem> mixedItems) = await SeedAsync(ProductType.Physical, ProductType.Service);
        await ExecuteAsync($"UPDATE orders SET order_status = 'Preparing', payment_status = 'Paid' WHERE id = '{mixed.Id}'");
        await using (AppDbContext context = NewContext())
        {
            context.Cancellations.Add(Cancellation.ByBuyer(mixedItems[0].Id, 1, Amounts, null, Start));
            await context.SaveChangesAsync(Ct);
        }

        _clock.SetUtcNow(end.AddTicks(-1));
        Assert.True(await IsOpenAsync(alone.Id, ActorKind.Buyer, OrderOperation.WithdrawServiceItem, aloneItems[0].Id));
        _clock.SetUtcNow(end);
        Assert.False(await IsOpenAsync(alone.Id, ActorKind.Buyer, OrderOperation.WithdrawServiceItem, aloneItems[0].Id));

        // The order carried a physical item when it was confirmed; it closed before any delivery, so no delivery date comes and the window never starts.
        _clock.SetUtcNow(end.AddDays(90));
        Assert.True(await IsOpenAsync(mixed.Id, ActorKind.Buyer, OrderOperation.WithdrawServiceItem, mixedItems[1].Id));

        // Only the "tamamlandı" mark ends it then.
        await ExecuteAsync($"UPDATE order_items SET delivery_date = '{TurkeyTime.DateOf(end.AddDays(90)):yyyy-MM-dd}', delivery_marked_at = '{end.AddDays(90):O}' WHERE id = '{mixedItems[1].Id}'");
        Assert.False(await IsOpenAsync(mixed.Id, ActorKind.Buyer, OrderOperation.WithdrawServiceItem, mixedItems[1].Id));
    }

    [Fact]
    public async Task AServiceItemOnAMixedOrder_RunsFromTheOrdersDeliveryDate_AndItsCompletedMarkClosesItFirst_KK2_KK6()
    {
        (Order order, List<OrderItem> items) = await SeedAsync(ProductType.Physical, ProductType.Service);
        DateOnly delivered = Today.AddDays(5);
        await ExecuteAsync($"UPDATE orders SET order_status = 'Delivered', payment_status = 'Paid' WHERE id = '{order.Id}'; "
            + $"UPDATE order_items SET delivery_date = '{delivered:yyyy-MM-dd}', delivery_marked_at = '{Start.AddDays(5):O}' WHERE id = '{items[0].Id}'");
        DateTimeOffset end = AfterSalesPeriodRules.WithdrawalWindowEnd(delivered);

        _clock.SetUtcNow(end.AddTicks(-1));
        Assert.True(await IsOpenAsync(order.Id, ActorKind.Buyer, OrderOperation.WithdrawServiceItem, items[1].Id));
        _clock.SetUtcNow(end);
        Assert.False(await IsOpenAsync(order.Id, ActorKind.Buyer, OrderOperation.WithdrawServiceItem, items[1].Id));

        _clock.SetUtcNow(Start.AddDays(6));
        Assert.True(await IsOpenAsync(order.Id, ActorKind.Buyer, OrderOperation.WithdrawServiceItem, items[1].Id));
        await ExecuteAsync($"UPDATE order_items SET delivery_date = '{TurkeyTime.DateOf(Start.AddDays(6)):yyyy-MM-dd}', delivery_marked_at = '{Start.AddDays(6):O}' WHERE id = '{items[1].Id}'");
        Assert.False(await IsOpenAsync(order.Id, ActorKind.Buyer, OrderOperation.WithdrawServiceItem, items[1].Id));
    }

    [Fact]
    public async Task AClosedWindow_LeavesTheEarlierDeclarationStanding_AndTheFirmStillRecordsAnotherChannelsNotice_KK1_KK4()
    {
        (Order order, List<OrderItem> items) = await SeedAsync(ProductType.Physical, ProductType.Physical, ProductType.Physical);
        Guid declared = items[0].Id;
        Guid absolute = items[1].Id;
        Guid undeclared = items[2].Id;
        await ExecuteAsync($"UPDATE orders SET order_status = 'Delivered', payment_status = 'Paid' WHERE id = '{order.Id}'; "
            + $"UPDATE order_items SET delivery_date = '{Today:yyyy-MM-dd}', delivery_marked_at = '{Start:O}' WHERE order_id = '{order.Id}'; "
            + $"UPDATE order_items SET withdrawal_exemption_reason = 'MadeToOrder' WHERE id = '{absolute}'");
        Withdrawal withdrawal = Withdrawal.ByBuyer(declared, 1, Today.AddDays(3), null, Amounts, null, Start.AddDays(3));
        await using (AppDbContext context = NewContext())
        {
            context.Withdrawals.Add(withdrawal);
            await context.SaveChangesAsync(Ct);
        }

        _clock.SetUtcNow(AfterSalesPeriodRules.WithdrawalWindowEnd(Today).AddDays(1));
        Assert.False(await IsOpenAsync(order.Id, ActorKind.Buyer, OrderOperation.WithdrawPhysicalItem, declared));

        // The declaration is a record: nothing closed or reset it, and its goods are still taken in.
        await using (AppDbContext context = NewContext())
        {
            Withdrawal kept = await context.Withdrawals.AsNoTracking().SingleAsync(w => w.OrderItemId == declared, Ct);
            Assert.Equal((withdrawal.Id, 1, Today.AddDays(3)), (kept.Id, kept.Quantity, kept.DeclarationDate));
        }

        Assert.True(await IsOpenAsync(order.Id, ActorKind.Admin, OrderOperation.ReceiveReturnedGoods, declared));

        // The legal extension of m.10 is the firm's record: open after the window and on an absolutely exempt item too (02 7.3.1; K-606).
        Assert.False(await IsOpenAsync(order.Id, ActorKind.Buyer, OrderOperation.WithdrawPhysicalItem, undeclared));
        Assert.True(await IsOpenAsync(order.Id, ActorKind.Admin, OrderOperation.RecordWithdrawalFromOtherChannel, undeclared));
        Assert.True(await IsOpenAsync(order.Id, ActorKind.Admin, OrderOperation.RecordWithdrawalFromOtherChannel, absolute));
    }

    [Fact]
    public async Task ADigitalItem_HasNoWithdrawal_BeforeOrAfterThePayment_KK5()
    {
        (Order order, List<OrderItem> items) = await SeedAsync(ProductType.Digital);
        Guid item = items[0].Id;

        foreach (string payment in new[] { "Pending", "Paid" })
        {
            await ExecuteAsync($"UPDATE orders SET order_status = 'Placed', payment_status = '{payment}' WHERE id = '{order.Id}'");
            Assert.False(await IsOpenAsync(order.Id, ActorKind.Buyer, OrderOperation.WithdrawDigitalItem, item));
            Assert.False(await IsOpenAsync(order.Id, ActorKind.Admin, OrderOperation.RecordWithdrawalFromOtherChannel, item));
            Assert.Equal(OperationDecision.Closed, (await WithOperationsAsync(o => o.CheckAsync(order.Id, OrderOperation.WithdrawDigitalItem, item, 1, Ct))).Decision);
        }
    }

    [Fact]
    public async Task TheFrozenExemptionReason_HidesTheButtonForAnAbsoluteReason_AndKeepsItForAConditionalOne_KK8()
    {
        // 02 7.3.4: (b), (c) are absolute, (ç), (e) conditional - the protective packaging.
        WithdrawalExemptionReason[] conditional = [WithdrawalExemptionReason.HygieneSensitive, WithdrawalExemptionReason.SealedMedia];
        foreach (WithdrawalExemptionReason? reason in Enum.GetValues<WithdrawalExemptionReason>().Select(r => (WithdrawalExemptionReason?)r).Append(null))
        {
            (Order order, List<OrderItem> items) = await SeedAsync(ProductType.Physical);
            string value = reason is null ? "NULL" : $"'{reason}'";
            await ExecuteAsync($"UPDATE orders SET order_status = 'Shipped', payment_status = 'Paid' WHERE id = '{order.Id}'; "
                + $"UPDATE order_items SET withdrawal_exemption_reason = {value} WHERE id = '{items[0].Id}'");

            bool expected = reason is null || conditional.Contains(reason.Value);
            Assert.Equal(expected, await IsOpenAsync(order.Id, ActorKind.Buyer, OrderOperation.WithdrawPhysicalItem, items[0].Id));
            Assert.Equal(
                expected ? OperationDecision.Allowed : OperationDecision.Closed,
                (await WithOperationsAsync(o => o.CheckAsync(order.Id, OrderOperation.WithdrawPhysicalItem, items[0].Id, 1, Ct))).Decision);
        }
    }

    // ---- helpers ----

    private async Task<bool> IsOpenAsync(Guid orderId, ActorKind actor, OrderOperation operation, Guid itemId)
    {
        OrderOperationsView view = (await WithOperationsAsync(o => o.ReadAsync(orderId, actor, Ct)))!;
        return view.IsOpen(operation, itemId);
    }

    private async Task<T> WithOperationsAsync<T>(Func<IOrderOperations, Task<T>> work)
    {
        await using AsyncServiceScope scope = _provider.CreateAsyncScope();
        return await work(scope.ServiceProvider.GetRequiredService<IOrderOperations>());
    }

    private async Task<(Order Order, List<OrderItem> Items)> SeedAsync(params ProductType[] types)
    {
        int n = Interlocked.Increment(ref _counter);
        Province province = Province.Create($"{n % 100:D2}", $"İl {n}", Start);
        LegalTextVersion form = LegalTextVersion.CreatePublished(LegalTextType.PreInformationForm, n, Start, "metin", Start);
        LegalTextVersion contract = LegalTextVersion.CreatePublished(LegalTextType.DistanceSalesContract, n, Start, "metin", Start);
        LegalTextVersion notice = LegalTextVersion.CreatePublished(LegalTextType.PrivacyNotice, n, Start, "metin", Start).WithHosting();
        Order order = Order.Create(
            $"2026-{n + 371000:D6}", $"access-placeholder-{n}", $"confirm-placeholder-{n}", PaymentMethod.Card, true, "Ali@Example.com", 120m, 0m, Start.AddHours(1), Start);
        order.IpAddress = IPAddress.Parse("203.0.113.5");
        order.CartId = EntityIds.New();
        order.FreezeShippingPromise(2);
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

        List<OrderItem> items = [];
        foreach (ProductType type in types)
        {
            OrderItem item = OrderItem.Create(order.Id, null, null, type, "Ürün", 1, 100m, 20m, 16.67m, Start);
            item.DownloadAllowance = type == ProductType.Digital ? 5 : null;
            item.FulfilmentPeriod = type == ProductType.Service ? 3 : null;
            items.Add(item);
        }

        await using AppDbContext context = NewContext();
        context.Provinces.Add(province);
        context.LegalTextVersions.AddRange(form, contract, notice);
        context.Orders.Add(order);
        context.OrderItems.AddRange(items);
        await context.SaveChangesAsync(Ct);
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

    private async Task ExecuteAsync(string sql)
    {
        await using NpgsqlConnection connection = new(_collection.Dsn);
        await connection.OpenAsync(Ct);
        await using NpgsqlCommand command = new(sql, connection);
        await command.ExecuteNonQueryAsync(Ct);
    }
}
