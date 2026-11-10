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
using Shopfolio.Modules.Administration;
using Shopfolio.Modules.AfterSales;
using Shopfolio.Modules.AfterSales.Contracts;
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
/// The operation decision on a real PostgreSQL (T323 criteria 2-8, KK2-KK8; 03 1.11): the facts are read from the order's own records and from the AfterSales and Payment modules'
/// open reads, the screen's list (<see cref="IOrderOperations.ReadAsync"/>) and the POST's check (<see cref="IOrderOperations.CheckAsync"/>) say the same thing for the same state,
/// a hidden operation is refused when asked for by hand, an item of another order or a missing order is closed, and the two sides never mix.
/// </summary>
[Collection(OrderTransitionsDefinition.Name)]
public sealed class OrderOperationsTests : IAsyncLifetime
{
    private static readonly DateTimeOffset Start = new(2026, 11, 9, 9, 0, 0, TimeSpan.Zero);

    private readonly CollectionDatabaseFixture _collection;
    private readonly ManualTimeProvider _clock = new(Start);
    private ServiceProvider _provider = null!;
    private int _counter;

    public OrderOperationsTests(CollectionDatabaseFixture collection)
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
    public async Task TheScreensList_AndThePostsCheck_SayTheSameThing_AtEveryStageOfAnOrder_KK2()
    {
        (Order order, List<OrderItem> items) = await SeedAsync(PaymentMethod.Card, ProductType.Physical, ProductType.Digital, ProductType.Service);
        (string Order, string Payment)[] stages =
        [
            ("Placed", "Pending"), ("Placed", "Paid"), ("Preparing", "Paid"), ("Shipped", "Paid"), ("DeliveryFailed", "Paid"), ("Delivered", "Paid"), ("Cancelled", "Refunded"),
        ];

        foreach ((string orderStatus, string paymentStatus) in stages)
        {
            await ExecuteAsync($"UPDATE orders SET order_status = '{orderStatus}', payment_status = '{paymentStatus}' WHERE id = '{order.Id}'");
            foreach (ActorKind actor in new[] { ActorKind.Buyer, ActorKind.Admin })
            {
                OrderOperationsView view = (await WithOperationsAsync(o => o.ReadAsync(order.Id, actor, Ct)))!;
                foreach (OrderOperation operation in Enum.GetValues<OrderOperation>().Where(o => (actor == ActorKind.Buyer) == ((int)o < 14)))
                {
                    foreach (Guid? itemId in new Guid?[] { null, items[0].Id, items[1].Id, items[2].Id })
                    {
                        bool listed = view.IsOpen(operation, itemId);
                        OrderOperationEntry? entry = view.Open.FirstOrDefault(e => e.Operation == operation && e.OrderItemId == itemId);
                        int? quantity = entry is { Availability.HasQuantity: true } ? entry.Availability.MaxQuantity : null;
                        OperationVerdict verdict = await WithOperationsAsync(o => o.CheckAsync(order.Id, operation, itemId, quantity, Ct));
                        Assert.True(listed == verdict.IsAllowed, $"{orderStatus}/{paymentStatus} {actor} {operation} {itemId}");
                    }
                }
            }
        }
    }

    [Fact]
    public async Task APhysicalItemThatHasNotShipped_HasNoWithdrawalAndNoDefectClaim_AndShippedOneHasBoth_KK4()
    {
        (Order order, List<OrderItem> items) = await SeedAsync(PaymentMethod.Card, ProductType.Physical);
        Guid itemId = items[0].Id;
        await ExecuteAsync($"UPDATE orders SET order_status = 'Preparing', payment_status = 'Paid' WHERE id = '{order.Id}'");

        OrderOperationsView before = (await WithOperationsAsync(o => o.ReadAsync(order.Id, ActorKind.Buyer, Ct)))!;
        Assert.False(before.IsOpen(OrderOperation.WithdrawPhysicalItem, itemId));
        Assert.False(before.IsOpen(OrderOperation.OpenDefectClaim, itemId));
        Assert.True(before.IsOpen(OrderOperation.CancelPhysicalItem, itemId));
        Assert.Equal(OperationDecision.Closed, (await WithOperationsAsync(o => o.CheckAsync(order.Id, OrderOperation.WithdrawPhysicalItem, itemId, 1, Ct))).Decision);

        await ExecuteAsync($"UPDATE orders SET order_status = 'Shipped' WHERE id = '{order.Id}'");
        OrderOperationsView after = (await WithOperationsAsync(o => o.ReadAsync(order.Id, ActorKind.Buyer, Ct)))!;
        Assert.True(after.IsOpen(OrderOperation.WithdrawPhysicalItem, itemId));
        Assert.True(after.IsOpen(OrderOperation.OpenDefectClaim, itemId));
        Assert.False(after.IsOpen(OrderOperation.CancelPhysicalItem, itemId));
    }

    [Fact]
    public async Task ADigitalItem_HasNoCancellationAndNoWithdrawal_AndAServiceCancelsUntilItsCompletedMark_KK5()
    {
        (Order order, List<OrderItem> items) = await SeedAsync(PaymentMethod.Card, ProductType.Digital, ProductType.Service);
        Guid digital = items[0].Id;
        Guid service = items[1].Id;
        await ExecuteAsync($"UPDATE orders SET order_status = 'Placed', payment_status = 'Paid' WHERE id = '{order.Id}'");

        OrderOperationsView view = (await WithOperationsAsync(o => o.ReadAsync(order.Id, ActorKind.Buyer, Ct)))!;
        Assert.DoesNotContain(view.Open, e => e.OrderItemId == digital && e.Operation is OrderOperation.CancelDigitalItem or OrderOperation.WithdrawDigitalItem or OrderOperation.CancelPhysicalItem or OrderOperation.CancelServiceItem);
        Assert.True(view.IsOpen(OrderOperation.DownloadDigitalFile, digital));
        Assert.True(view.IsOpen(OrderOperation.CancelServiceItem, service));
        Assert.Equal(OperationDecision.Closed, (await WithOperationsAsync(o => o.CheckAsync(order.Id, OrderOperation.CancelDigitalItem, digital, 1, Ct))).Decision);
        Assert.Equal(OperationDecision.Closed, (await WithOperationsAsync(o => o.CheckAsync(order.Id, OrderOperation.WithdrawDigitalItem, digital, 1, Ct))).Decision);

        await ExecuteAsync($"UPDATE order_items SET delivery_marked_at = '{Start:O}', delivery_date = '2026-11-09' WHERE id = '{service}'");
        OrderOperationsView marked = (await WithOperationsAsync(o => o.ReadAsync(order.Id, ActorKind.Buyer, Ct)))!;
        Assert.False(marked.IsOpen(OrderOperation.CancelServiceItem, service));
        Assert.Equal(OperationDecision.Closed, (await WithOperationsAsync(o => o.CheckAsync(order.Id, OrderOperation.CancelServiceItem, service, 1, Ct))).Decision);
    }

    [Fact]
    public async Task AHiddenOperation_CalledByHand_IsRefused_AndAForgedRequestIsClosed_KK3_KK6_KK8()
    {
        (Order order, List<OrderItem> items) = await SeedAsync(PaymentMethod.BankTransfer, ProductType.Physical);
        (Order other, List<OrderItem> otherItems) = await SeedAsync(PaymentMethod.Card, ProductType.Physical);
        await ExecuteAsync($"UPDATE orders SET order_status = 'Delivered', payment_status = 'Paid' WHERE id = '{order.Id}'");

        // The administrator's "ship" button is not drawn on a delivered order and the endpoint refuses it all the same.
        OrderOperationsView admin = (await WithOperationsAsync(o => o.ReadAsync(order.Id, ActorKind.Admin, Ct)))!;
        Assert.False(admin.IsOpen(OrderOperation.ShipOrder));
        Assert.Equal(OperationDecision.Closed, (await WithOperationsAsync(o => o.CheckAsync(order.Id, OrderOperation.ShipOrder, null, null, Ct))).Decision);
        Assert.Equal(OperationDecision.Closed, (await WithOperationsAsync(o => o.CheckAsync(order.Id, OrderOperation.CancelOrder, null, null, Ct))).Decision);

        // An operation that is not in the table, an item of another order, a missing order: closed.
        Assert.Equal(OperationDecision.Closed, (await WithOperationsAsync(o => o.CheckAsync(order.Id, (OrderOperation)99, null, null, Ct))).Decision);
        Assert.Equal(OperationDecision.Closed, (await WithOperationsAsync(o => o.CheckAsync(order.Id, OrderOperation.CancelPhysicalItem, otherItems[0].Id, 1, Ct))).Decision);
        Assert.Equal(OperationDecision.Closed, (await WithOperationsAsync(o => o.CheckAsync(Guid.NewGuid(), OrderOperation.WriteInternalNote, null, null, Ct))).Decision);
        Assert.Null(await WithOperationsAsync(o => o.ReadAsync(Guid.NewGuid(), ActorKind.Buyer, Ct)));
        Assert.True((await WithOperationsAsync(o => o.CheckAsync(other.Id, OrderOperation.CancelOrder, null, null, Ct))).IsAllowed);
        Assert.NotEmpty(items);
    }

    [Fact]
    public async Task TheSidesNeverMix_AndOnlyTheBuyerAndTheAdminHaveOperations_KK7()
    {
        (Order order, _) = await SeedAsync(PaymentMethod.BankTransfer, ProductType.Physical);

        OrderOperationsView buyer = (await WithOperationsAsync(o => o.ReadAsync(order.Id, ActorKind.Buyer, Ct)))!;
        OrderOperationsView admin = (await WithOperationsAsync(o => o.ReadAsync(order.Id, ActorKind.Admin, Ct)))!;

        Assert.NotEmpty(buyer.Open);
        Assert.NotEmpty(admin.Open);
        Assert.All(buyer.Open, e => Assert.True((int)e.Operation < 14));
        Assert.All(admin.Open, e => Assert.True((int)e.Operation >= 14));
        Assert.True(admin.IsOpen(OrderOperation.MarkTransferPaid));
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => await WithOperationsAsync(o => o.ReadAsync(order.Id, ActorKind.System, Ct)));
    }

    [Fact]
    public async Task TheReturnedGoods_AreTakenIn_BeforeTheDeliveryDateToo_AndAfterAnAdminReceiptTheArrivalDateIsCorrectable_1_11_25_1_11_28()
    {
        (Order order, List<OrderItem> items) = await SeedAsync(PaymentMethod.Card, ProductType.Physical);
        Guid itemId = items[0].Id;
        await ExecuteAsync($"UPDATE orders SET order_status = 'Shipped', payment_status = 'Paid' WHERE id = '{order.Id}'");
        Admin admin = Admin.Create("Veli", $"veli-{EntityIds.New():N}@example.com", "digest-placeholder", Start);
        DateOnly today = DateOnly.FromDateTime(Start.UtcDateTime);
        Withdrawal withdrawal = Withdrawal.ByBuyer(itemId, 1, today, null, new ItemAmounts(100m, 10m, 15m), null, Start);
        await using (AppDbContext context = NewContext())
        {
            context.Admins.Add(admin);
            context.Withdrawals.Add(withdrawal);
            await context.SaveChangesAsync(Ct);
        }

        // Shipped, no delivery date, a withdrawal on its way back: the receipt writer takes the goods, so the decision opens the step (and a rejection only with a conditional exemption).
        OrderOperationsView awaiting = (await WithOperationsAsync(o => o.ReadAsync(order.Id, ActorKind.Admin, Ct)))!;
        Assert.True(awaiting.IsOpen(OrderOperation.ReceiveReturnedGoods, itemId));
        Assert.Equal(OperationDecision.Allowed, (await WithOperationsAsync(o => o.CheckAsync(order.Id, OrderOperation.ReceiveReturnedGoods, itemId, 1, Ct))).Decision);
        Assert.False(awaiting.IsOpen(OrderOperation.RejectReturnedGoods, itemId));
        Assert.False(awaiting.IsOpen(OrderOperation.CloseAsNotReturned, itemId));
        Assert.False(awaiting.IsOpen(OrderOperation.CorrectDeliveryOrArrivalDate));

        await using (AppDbContext context = NewContext())
        {
            context.Returns.Add(Return.ForWithdrawal(itemId, withdrawal.Id, 1, today, admin.Id, admin.Name, Start));
            await context.SaveChangesAsync(Ct);
        }

        // The goods are in: nothing is left to take, and the arrival date now exists to correct.
        OrderOperationsView received = (await WithOperationsAsync(o => o.ReadAsync(order.Id, ActorKind.Admin, Ct)))!;
        Assert.False(received.IsOpen(OrderOperation.ReceiveReturnedGoods, itemId));
        Assert.True(received.IsOpen(OrderOperation.CorrectDeliveryOrArrivalDate));
    }

    [Fact]
    public async Task TheGoodsOfALateDeliveryTermination_AreTakenIn_1_11_28()
    {
        (Order order, List<OrderItem> items) = await SeedAsync(PaymentMethod.Card, ProductType.Physical);
        Guid itemId = items[0].Id;
        await ExecuteAsync($"UPDATE orders SET order_status = 'Preparing', payment_status = 'Paid' WHERE id = '{order.Id}'");
        LateDeliveryTermination termination = LateDeliveryTermination.ByBuyer(itemId, 1, DateOnly.FromDateTime(Start.UtcDateTime), new ItemAmounts(100m, 10m, 15m), null, Start);
        await using (AppDbContext context = NewContext())
        {
            context.LateDeliveryTerminations.Add(termination);
            await context.SaveChangesAsync(Ct);
        }

        OrderOperationsView view = (await WithOperationsAsync(o => o.ReadAsync(order.Id, ActorKind.Admin, Ct)))!;
        Assert.True(view.IsOpen(OrderOperation.ReceiveReturnedGoods, itemId));
    }

    [Fact]
    public async Task AServiceItemsWithdrawal_HasNoGoodsToTakeIn_1_11_28()
    {
        (Order order, List<OrderItem> items) = await SeedAsync(PaymentMethod.Card, ProductType.Service);
        Guid itemId = items[0].Id;
        await ExecuteAsync($"UPDATE orders SET order_status = 'Placed', payment_status = 'Paid' WHERE id = '{order.Id}'");
        Withdrawal withdrawal = Withdrawal.ByBuyer(itemId, 1, DateOnly.FromDateTime(Start.UtcDateTime), null, new ItemAmounts(100m, 10m, 15m), null, Start);
        await using (AppDbContext context = NewContext())
        {
            context.Withdrawals.Add(withdrawal);
            await context.SaveChangesAsync(Ct);
        }

        OrderOperationsView view = (await WithOperationsAsync(o => o.ReadAsync(order.Id, ActorKind.Admin, Ct)))!;
        Assert.False(view.IsOpen(OrderOperation.ReceiveReturnedGoods, itemId));
    }

    // ---- helpers ----

    [Fact]
    public async Task TheResendEmailOperation_IsOpenOnlyWhileTheOrderCarriesTheUndeliveredMark_T385()
    {
        (Order order, _) = await SeedAsync(PaymentMethod.Card, ProductType.Physical);
        await ExecuteAsync($"UPDATE orders SET order_status = 'Preparing', payment_status = 'Paid' WHERE id = '{order.Id}'");
        Assert.False((await WithOperationsAsync(o => o.ReadAsync(order.Id, ActorKind.Admin, Ct)))!.IsOpen(OrderOperation.ResendEmail));
        Assert.Equal(OperationDecision.Closed, (await WithOperationsAsync(o => o.CheckAsync(order.Id, OrderOperation.ResendEmail, null, null, Ct))).Decision);

        DispatchRecord row = DispatchRecord.Queue(DispatchType.B2, "alici@example.com", order.Id, null, null, new DateTimeOffset(2026, 11, 6, 9, 0, 0, TimeSpan.Zero));
        row.DispatchState = DispatchState.Undelivered;
        row.LastOutcome = DispatchOutcome.ChannelError;
        row.AttemptCount = DispatchRetrySchedule.AttemptLimit;
        row.NextAttemptAt = null;
        await using (AsyncServiceScope scope = _provider.CreateAsyncScope())
        {
            AppDbContext db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.DispatchRecords.Add(row);
            await db.SaveChangesAsync(Ct);
        }

        Assert.True((await WithOperationsAsync(o => o.ReadAsync(order.Id, ActorKind.Admin, Ct)))!.IsOpen(OrderOperation.ResendEmail));
        Assert.True((await WithOperationsAsync(o => o.CheckAsync(order.Id, OrderOperation.ResendEmail, null, null, Ct))).IsAllowed);
    }

    private async Task<T> WithOperationsAsync<T>(Func<IOrderOperations, Task<T>> work)
    {
        await using AsyncServiceScope scope = _provider.CreateAsyncScope();
        return await work(scope.ServiceProvider.GetRequiredService<IOrderOperations>());
    }

    private async Task<(Order Order, List<OrderItem> Items)> SeedAsync(PaymentMethod method, params ProductType[] types)
    {
        int n = Interlocked.Increment(ref _counter);
        Province province = Province.Create($"{n % 100:D2}", $"İl {n}", Start);
        LegalTextVersion form = LegalTextVersion.CreatePublished(LegalTextType.PreInformationForm, n, Start, "metin", Start);
        LegalTextVersion contract = LegalTextVersion.CreatePublished(LegalTextType.DistanceSalesContract, n, Start, "metin", Start);
        LegalTextVersion notice = LegalTextVersion.CreatePublished(LegalTextType.PrivacyNotice, n, Start, "metin", Start).WithHosting();
        Order order = Order.Create(
            $"2026-{n + 323000:D6}", $"access-placeholder-{n}", $"confirm-placeholder-{n}", method, true, "Ali@Example.com", 120m, 0m, Start.AddHours(1), Start);
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
        order.BankTransferIban = method == PaymentMethod.BankTransfer ? "TR330006100519786457841326" : null;

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
