using System.Net;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

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

namespace Shopfolio.IntegrationTests.Ordering;

/// <summary>
/// The delivery marks on a real PostgreSQL (T351 KK1-KK7; 02 3.20.9-3.20.12, 5.6.1, 5.8, 7.3.1, 7.3.3; 03 1.4.3, 1.5, 8.2.6, 8.2.9): the three types' ways to the mark - the physical item's S6 by the firm,
/// the digital item's by the system at the approval, the service item's "completed" by the firm -, the payment that lets the mark be set, the mixed order's single shipment axis, the physical
/// delivery date's rules and the single date of the open physical items, the order whose physical items are all cancelled and the order that never had one. Every operation runs in the
/// transaction runner like a page would call it; what it wrote is read back from the tables and every expected value comes from the table, the rule or the contract.
/// </summary>
[Collection(OrderTransitionsDefinition.Name)]
public sealed class DeliveryMarksTests : IAsyncLifetime
{
    private static readonly DateTimeOffset Start = new(2026, 11, 7, 9, 0, 0, TimeSpan.Zero);

    private static readonly DateOnly StartDay = DateOnly.FromDateTime(Start.UtcDateTime);

    private static readonly ItemAmounts Amounts = new(100m, 0m, 16.67m);

    private static readonly ShipmentInfo Listed = new("YurticiKargo", null, "TR-1001", false);

    private readonly CollectionDatabaseFixture _collection;

    private readonly MovableClock _clock = new(Start);

    private Admin? _admin;

    private int _counter;

    public DeliveryMarksTests(CollectionDatabaseFixture collection)
    {
        _collection = collection;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private TransitionAdmin AdminCaller => new(_admin!.Id, _admin.Name);

    public async ValueTask InitializeAsync()
    {
        await ExecuteAsync(
            "TRUNCATE dispatch_records, jobs, refund_records, refunds, payment_records, cancellations, internal_notes, transition_records, stock_reservations, order_items, orders, admins, "
            + "legal_text_versions, provinces CASCADE");
        _admin = Admin.Create("Firma Yöneticisi", "yonetici@example.com", "digest-placeholder", Start);
        await SaveAsync(c => c.Admins.Add(_admin));
        _clock.Set(Start);
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    // ---- KK1: the three types' ways to the mark ----

    [Fact]
    public async Task APhysicalItemsMark_IsTheFirmsAfterTheShipment_AndTheSystemWritesNone_KK1()
    {
        (Order order, List<OrderItem> items) = await SeedAsync(OrderStatus.Preparing, PaymentStatus.Paid, new ItemSpec(ProductType.Physical, 1));

        OrderTransitionResult early = await RunAsync((unit, token) => unit.MarkDeliveredAsync(new MarkDeliveredRequest(order.Id, AdminCaller, 1, StartDay), token));
        OrderTransitionResult shipped = await RunAsync((unit, token) => unit.ShipOrderAsync(new ShipOrderRequest(order.Id, AdminCaller, 1, true, Listed), token));
        Assert.Null((await ReadItemAsync(items[0].Id)).DeliveryMarkedAt);
        OrderTransitionResult marked = await RunAsync((unit, token) => unit.MarkDeliveredAsync(new MarkDeliveredRequest(order.Id, AdminCaller, 2, StartDay), token));

        Assert.Equal(TransitionRefusal.NotInTheTable, early.Refusal);
        Assert.False(shipped.IsRejected);
        Assert.False(marked.IsRejected);
        AssertMark(await ReadItemAsync(items[0].Id), Start, StartDay);
        await using AppDbContext reader = NewContext();
        TransitionRecord delivered = await reader.TransitionRecords.SingleAsync(t => t.OrderId == order.Id && t.TransitionCode == TransitionCode.S6, Ct);
        Assert.Equal(ActorKind.Admin, delivered.ActorKind);
        Assert.Equal(OrderStatus.Delivered, (await reader.Orders.SingleAsync(o => o.Id == order.Id, Ct)).OrderStatus);
    }

    [Fact]
    public async Task ADigitalItemsMark_IsTheSystemsAtTheApproval_AndTheFirmHasNoMarkOfItsOwn_KK1()
    {
        (Order order, List<OrderItem> items) = await SeedAsync(OrderStatus.Placed, PaymentStatus.Pending, new ItemSpec(ProductType.Digital, 1));

        PaymentTransitionResult approved = await RunAsync((unit, token) => unit.ApplyCardPaymentAsync(new CardPaymentSucceeded(order.Id, "trx-digital-mark"), token));

        Assert.Equal([TransitionCode.O1, TransitionCode.S2], approved.Applied);
        AssertMark(await ReadItemAsync(items[0].Id), Start, StartDay);
        await using AppDbContext reader = NewContext();
        Assert.All(await reader.TransitionRecords.Where(t => t.OrderId == order.Id).ToListAsync(Ct), r => Assert.Equal(ActorKind.System, r.ActorKind));

        // The service mark is not the digital item's way: the firm's "completed" is refused on a digital item, and the physical delivery operation has nothing to mark.
        (Order other, List<OrderItem> otherItems) = await SeedAsync(OrderStatus.Placed, PaymentStatus.Paid, new ItemSpec(ProductType.Digital, 1), new ItemSpec(ProductType.Service, 1));
        OrderTransitionResult onDigital = await RunAsync((unit, token) => unit.MarkServiceCompletedAsync(new MarkServiceCompletedRequest(other.Id, otherItems[0].Id, AdminCaller, 1), token));
        Assert.Equal(TransitionRefusal.ConditionNotMet, onDigital.Refusal);
        Assert.Null((await ReadItemAsync(otherItems[0].Id)).DeliveryMarkedAt);
    }

    [Fact]
    public async Task AServiceItemsMark_IsTheFirmsCompleted_WithTheDayOfTheMark_AndTheLastOneEndsTheLine_KK1()
    {
        (Order order, List<OrderItem> items) = await SeedAsync(OrderStatus.Placed, PaymentStatus.Paid, new ItemSpec(ProductType.Service, 1));
        _clock.Set(Start.AddDays(2));

        OrderTransitionResult result = await RunAsync((unit, token) => unit.MarkServiceCompletedAsync(new MarkServiceCompletedRequest(order.Id, items[0].Id, AdminCaller, 1), token));

        Assert.False(result.IsRejected);
        Assert.Equal(TransitionCode.S2, result.ClosureCode);
        Assert.Equal([items[0].Id], result.ItemIds);
        AssertMark(await ReadItemAsync(items[0].Id), Start.AddDays(2), StartDay.AddDays(2));
        await using AppDbContext reader = NewContext();
        Order saved = await reader.Orders.SingleAsync(o => o.Id == order.Id, Ct);
        Assert.Equal((OrderStatus.Delivered, PaymentStatus.Paid, 2), (saved.OrderStatus, saved.PaymentStatus, saved.Version));
        TransitionRecord record = await reader.TransitionRecords.SingleAsync(t => t.OrderId == order.Id, Ct);
        Assert.Equal((TransitionCode.S2, ActorKind.Admin), (record.TransitionCode, record.ActorKind));
        Assert.Equal(_admin!.Id, record.AdminId);
        Assert.Empty(await reader.DispatchRecords.Where(d => d.OrderId == order.Id).ToListAsync(Ct));
    }

    [Fact]
    public async Task AServiceMark_OfAnItemThatIsNotTheOrdersOrAlreadyMarkedOrClosed_IsRefused_AndWritesNothing_KK1()
    {
        (Order order, List<OrderItem> items) = await SeedAsync(
            OrderStatus.Placed, PaymentStatus.Paid, new ItemSpec(ProductType.Service, 1), new ItemSpec(ProductType.Service, 1, true), new ItemSpec(ProductType.Service, 1), new ItemSpec(ProductType.Physical, 1));
        await SaveAsync(c => c.Cancellations.Add(Cancellation.ByBuyer(items[2].Id, 1, Amounts, null, Start)));
        (Order stranger, List<OrderItem> strangerItems) = await SeedAsync(OrderStatus.Placed, PaymentStatus.Paid, new ItemSpec(ProductType.Service, 1));

        OrderTransitionResult marked = await RunAsync((unit, token) => unit.MarkServiceCompletedAsync(new MarkServiceCompletedRequest(order.Id, items[1].Id, AdminCaller, 1), token));
        OrderTransitionResult closed = await RunAsync((unit, token) => unit.MarkServiceCompletedAsync(new MarkServiceCompletedRequest(order.Id, items[2].Id, AdminCaller, 1), token));
        OrderTransitionResult physical = await RunAsync((unit, token) => unit.MarkServiceCompletedAsync(new MarkServiceCompletedRequest(order.Id, items[3].Id, AdminCaller, 1), token));
        OrderTransitionResult elsewhere = await RunAsync((unit, token) => unit.MarkServiceCompletedAsync(new MarkServiceCompletedRequest(order.Id, strangerItems[0].Id, AdminCaller, 1), token));
        OrderTransitionResult noAdmin = await RunAsync((unit, token) => unit.MarkServiceCompletedAsync(new MarkServiceCompletedRequest(order.Id, items[0].Id, new TransitionAdmin(Guid.Empty, " "), 1), token));
        OrderTransitionResult older = await RunAsync((unit, token) => unit.MarkServiceCompletedAsync(new MarkServiceCompletedRequest(order.Id, items[0].Id, AdminCaller, 7), token));
        OrderTransitionResult unknown = await RunAsync((unit, token) => unit.MarkServiceCompletedAsync(new MarkServiceCompletedRequest(EntityIds.New(), items[0].Id, AdminCaller, 1), token));

        Assert.Equal(TransitionRefusal.ConditionNotMet, marked.Refusal);
        Assert.Equal(TransitionRefusal.ConditionNotMet, closed.Refusal);
        Assert.Equal(TransitionRefusal.ConditionNotMet, physical.Refusal);
        Assert.Equal(TransitionRefusal.InvalidInput, elsewhere.Refusal);
        Assert.Equal(TransitionRefusal.ActorNotAllowed, noAdmin.Refusal);
        Assert.Equal(TransitionRefusal.VersionConflict, older.Refusal);
        Assert.Equal(TransitionRefusal.OrderNotFound, unknown.Refusal);
        await using AppDbContext reader = NewContext();
        Order saved = await reader.Orders.SingleAsync(o => o.Id == order.Id, Ct);
        Assert.Equal((OrderStatus.Placed, 1), (saved.OrderStatus, saved.Version));
        Assert.Empty(await reader.TransitionRecords.Where(t => t.OrderId == order.Id || t.OrderId == stranger.Id).ToListAsync(Ct));
        Assert.Null((await ReadItemAsync(items[0].Id)).DeliveryMarkedAt);
        Assert.Null((await ReadItemAsync(strangerItems[0].Id)).DeliveryMarkedAt);
    }

    // ---- KK2: the payment that lets the mark be set ----

    [Fact]
    public async Task TheServiceMark_IsRefusedWhileThePaymentIsPendingOrFailed_AndWritesNothing_KK2()
    {
        (Order pending, List<OrderItem> pendingItems) = await SeedAsync(OrderStatus.Placed, PaymentStatus.Pending, new ItemSpec(ProductType.Service, 1));
        (Order failed, List<OrderItem> failedItems) = await SeedAsync(OrderStatus.Cancelled, PaymentStatus.Failed, new ItemSpec(ProductType.Service, 1));

        OrderTransitionResult onPending = await RunAsync((unit, token) => unit.MarkServiceCompletedAsync(new MarkServiceCompletedRequest(pending.Id, pendingItems[0].Id, AdminCaller, 1), token));
        OrderTransitionResult onFailed = await RunAsync((unit, token) => unit.MarkServiceCompletedAsync(new MarkServiceCompletedRequest(failed.Id, failedItems[0].Id, AdminCaller, 1), token));

        Assert.Equal(TransitionRefusal.ConditionNotMet, onPending.Refusal);
        Assert.Equal(TransitionRefusal.ConditionNotMet, onFailed.Refusal);
        await using AppDbContext reader = NewContext();
        Assert.Empty(await reader.TransitionRecords.Where(t => t.OrderId == pending.Id || t.OrderId == failed.Id).ToListAsync(Ct));
        Assert.Null((await ReadItemAsync(pendingItems[0].Id)).DeliveryMarkedAt);
        Assert.Null((await ReadItemAsync(failedItems[0].Id)).DeliveryMarkedAt);
        Assert.Equal((OrderStatus.Placed, 1), await ReadStatusAsync(pending.Id));
    }

    [Fact]
    public async Task APartialRefund_DoesNotCloseTheMark_OnAServiceItemOrAPhysicalDelivery_KK2()
    {
        (Order service, List<OrderItem> serviceItems) = await SeedAsync(OrderStatus.Placed, PaymentStatus.PartiallyRefunded, new ItemSpec(ProductType.Service, 2));
        (Order physical, List<OrderItem> physicalItems) = await SeedAsync(OrderStatus.Shipped, PaymentStatus.PartiallyRefunded, new ItemSpec(ProductType.Physical, 1));

        OrderTransitionResult completed = await RunAsync((unit, token) => unit.MarkServiceCompletedAsync(new MarkServiceCompletedRequest(service.Id, serviceItems[0].Id, AdminCaller, 1), token));
        OrderTransitionResult delivered = await RunAsync((unit, token) => unit.MarkDeliveredAsync(new MarkDeliveredRequest(physical.Id, AdminCaller, 1, StartDay), token));

        Assert.False(completed.IsRejected);
        Assert.False(delivered.IsRejected);
        Assert.NotNull((await ReadItemAsync(serviceItems[0].Id)).DeliveryMarkedAt);
        Assert.NotNull((await ReadItemAsync(physicalItems[0].Id)).DeliveryMarkedAt);
        Assert.Equal((OrderStatus.Delivered, 2), await ReadStatusAsync(service.Id));
        Assert.Equal((OrderStatus.Delivered, 2), await ReadStatusAsync(physical.Id));
        await using AppDbContext reader = NewContext();
        Assert.Equal(PaymentStatus.PartiallyRefunded, (await reader.Orders.SingleAsync(o => o.Id == service.Id, Ct)).PaymentStatus);
    }

    [Fact]
    public async Task ThePhysicalMark_IsNotTakenWhileThePaymentIsPending_KK2()
    {
        (Order order, List<OrderItem> items) = await SeedAsync(OrderStatus.Placed, PaymentStatus.Pending, new ItemSpec(ProductType.Physical, 1));

        OrderTransitionResult result = await RunAsync((unit, token) => unit.MarkDeliveredAsync(new MarkDeliveredRequest(order.Id, AdminCaller, 1, StartDay), token));

        Assert.True(result.IsRejected);
        Assert.Null((await ReadItemAsync(items[0].Id)).DeliveryMarkedAt);
        Assert.Equal((OrderStatus.Placed, 1), await ReadStatusAsync(order.Id));
    }

    // ---- KK3: the mixed order has one shipment axis; the digital item does not wait for it ----

    [Fact]
    public async Task AMixedOrder_FollowsThePhysicalLine_AndTheDigitalItemIsDownloadableWhileItIsStillPreparing_KK3()
    {
        (Order order, List<OrderItem> items) = await SeedAsync(
            OrderStatus.Placed, PaymentStatus.Pending, new ItemSpec(ProductType.Physical, 1), new ItemSpec(ProductType.Digital, 1), new ItemSpec(ProductType.Service, 1));

        PaymentTransitionResult approved = await RunAsync((unit, token) => unit.ApplyCardPaymentAsync(new CardPaymentSucceeded(order.Id, "trx-mixed-mark"), token));
        DownloadTakeResult download = await RunModuleAsync((module, token) => module.TakeDownloadAsync(items[1].Id, token));

        // One axis: Preparing, by the physical line; the digital item is marked and downloadable, the physical and the service items wait for their own marks.
        Assert.Equal([TransitionCode.O1, TransitionCode.S1], approved.Applied);
        Assert.Equal((OrderStatus.Preparing, 2), await ReadStatusAsync(order.Id));
        Assert.True(download.IsTaken);
        Assert.Null((await ReadItemAsync(items[0].Id)).DeliveryMarkedAt);
        Assert.NotNull((await ReadItemAsync(items[1].Id)).DeliveryMarkedAt);
        Assert.Null((await ReadItemAsync(items[2].Id)).DeliveryMarkedAt);

        // The service item is completed while the order is still on the physical line: no closure, the axis stays where the physical items hold it.
        OrderTransitionResult completed = await RunAsync((unit, token) => unit.MarkServiceCompletedAsync(new MarkServiceCompletedRequest(order.Id, items[2].Id, AdminCaller, 2), token));
        Assert.False(completed.IsRejected);
        Assert.Null(completed.ClosureCode);
        Assert.Empty(completed.TransitionRecordIds);
        Assert.Equal((OrderStatus.Preparing, 3), await ReadStatusAsync(order.Id));
        Assert.Null((await ReadItemAsync(items[0].Id)).DeliveryMarkedAt);

        // Then the physical line runs by itself: S5 and S6 end the order, and S6 marks the physical item alone.
        await RunAsync((unit, token) => unit.ShipOrderAsync(new ShipOrderRequest(order.Id, AdminCaller, 3, true, Listed), token));
        OrderTransitionResult delivered = await RunAsync((unit, token) => unit.MarkDeliveredAsync(new MarkDeliveredRequest(order.Id, AdminCaller, 4, StartDay), token));
        Assert.Equal([items[0].Id], delivered.ItemIds);
        Assert.Equal((OrderStatus.Delivered, 5), await ReadStatusAsync(order.Id));
    }

    [Fact]
    public async Task AMixedOrdersServiceMark_NeverMovesAShippedOrder_KK3()
    {
        (Order order, List<OrderItem> items) = await SeedAsync(OrderStatus.Shipped, PaymentStatus.Paid, new ItemSpec(ProductType.Physical, 1), new ItemSpec(ProductType.Service, 1));

        OrderTransitionResult result = await RunAsync((unit, token) => unit.MarkServiceCompletedAsync(new MarkServiceCompletedRequest(order.Id, items[1].Id, AdminCaller, 1), token));

        Assert.False(result.IsRejected);
        Assert.Null(result.ClosureCode);
        Assert.Equal((OrderStatus.Shipped, 2), await ReadStatusAsync(order.Id));
        await using AppDbContext reader = NewContext();
        Assert.Empty(await reader.TransitionRecords.Where(t => t.OrderId == order.Id).ToListAsync(Ct));
    }

    // ---- KK5: the physical delivery date ----

    [Fact]
    public async Task ADeliveryDateInTheFuture_OrBeforeTheShipment_IsRefusedWithItsReason_AndThePastAndTheShippingDayAreAccepted_KK5()
    {
        (Order order, _) = await SeedAsync(OrderStatus.Preparing, PaymentStatus.Paid, new ItemSpec(ProductType.Physical, 1));
        _clock.Set(Start.AddDays(2));
        await RunAsync((unit, token) => unit.ShipOrderAsync(new ShipOrderRequest(order.Id, AdminCaller, 1, true, Listed), token));
        _clock.Set(Start.AddDays(5));
        DateOnly shippingDay = StartDay.AddDays(2);
        DateOnly today = StartDay.AddDays(5);

        OrderTransitionResult future = await RunAsync((unit, token) => unit.MarkDeliveredAsync(new MarkDeliveredRequest(order.Id, AdminCaller, 2, today.AddDays(1)), token));
        OrderTransitionResult before = await RunAsync((unit, token) => unit.MarkDeliveredAsync(new MarkDeliveredRequest(order.Id, AdminCaller, 2, shippingDay.AddDays(-1)), token));
        OrderTransitionResult past = await RunAsync((unit, token) => unit.MarkDeliveredAsync(new MarkDeliveredRequest(order.Id, AdminCaller, 2, shippingDay.AddDays(1)), token));

        Assert.Equal(TransitionRefusal.InvalidInput, future.Refusal);
        Assert.Equal(DeliveryDateObstacle.InTheFuture, future.DateObstacle);
        Assert.Equal(TransitionRefusal.InvalidInput, before.Refusal);
        Assert.Equal(DeliveryDateObstacle.BeforeShipment, before.DateObstacle);
        Assert.False(past.IsRejected);
        Assert.Null(past.DateObstacle);
        await using AppDbContext reader = NewContext();
        Assert.Equal(1, await reader.TransitionRecords.CountAsync(t => t.OrderId == order.Id && t.TransitionCode == TransitionCode.S6, Ct));
    }

    [Fact]
    public async Task ARefusedDateWritesNothing_AndARefusalOfAnotherKindCarriesNoDateObstacle_KK5()
    {
        (Order order, List<OrderItem> items) = await SeedAsync(OrderStatus.Shipped, PaymentStatus.Paid, new ItemSpec(ProductType.Physical, 1));
        await SaveAsync(c => c.Cancellations.Add(Cancellation.ByBuyer(items[0].Id, 1, Amounts, null, Start)));

        OrderTransitionResult future = await RunAsync((unit, token) => unit.MarkDeliveredAsync(new MarkDeliveredRequest(order.Id, AdminCaller, 1, StartDay.AddDays(1)), token));
        OrderTransitionResult noOpen = await RunAsync((unit, token) => unit.MarkDeliveredAsync(new MarkDeliveredRequest(order.Id, AdminCaller, 1, StartDay), token));

        Assert.Equal(DeliveryDateObstacle.InTheFuture, future.DateObstacle);
        Assert.Equal(TransitionRefusal.ConditionNotMet, noOpen.Refusal);
        Assert.Null(noOpen.DateObstacle);
        Assert.Null((await ReadItemAsync(items[0].Id)).DeliveryMarkedAt);
        Assert.Equal((OrderStatus.Shipped, 1), await ReadStatusAsync(order.Id));
    }

    [Fact]
    public async Task TheOneDate_IsWrittenOnTheOpenPhysicalItemsOnly_NotOnACancelledOneNorOnTheDigitalOrServiceItems_AndTheWithdrawalWindowRunsFromIt_KK5()
    {
        (Order order, List<OrderItem> items) = await SeedAsync(
            OrderStatus.Shipped,
            PaymentStatus.Paid,
            new ItemSpec(ProductType.Physical, 2),
            new ItemSpec(ProductType.Physical, 1),
            new ItemSpec(ProductType.Physical, 1),
            new ItemSpec(ProductType.Digital, 1, true),
            new ItemSpec(ProductType.Service, 1));
        await SaveAsync(c => c.Cancellations.Add(Cancellation.ByBuyer(items[1].Id, 1, Amounts, null, Start)));
        _clock.Set(Start.AddDays(3));
        DateOnly date = StartDay.AddDays(1);

        OrderTransitionResult result = await RunAsync((unit, token) => unit.MarkDeliveredAsync(new MarkDeliveredRequest(order.Id, AdminCaller, 1, date), token));

        Assert.False(result.IsRejected);
        Assert.Equivalent(new[] { items[0].Id, items[2].Id }, result.ItemIds);
        AssertMark(await ReadItemAsync(items[0].Id), Start.AddDays(3), date);
        AssertMark(await ReadItemAsync(items[2].Id), Start.AddDays(3), date);
        AssertMark(await ReadItemAsync(items[1].Id), null, null);
        AssertMark(await ReadItemAsync(items[3].Id), Start, StartDay);
        AssertMark(await ReadItemAsync(items[4].Id), null, null);

        // The window runs from the entered date: open until its fourteenth day runs out, closed after it (Z-13); with no mark the order has no start and the window does not close.
        Assert.True(AfterSalesPeriodRules.IsWithdrawalWindowOpen(Facts(date), AfterSalesPeriodRules.WithdrawalWindowEnd(date).AddTicks(-1)));
        Assert.False(AfterSalesPeriodRules.IsWithdrawalWindowOpen(Facts(date), AfterSalesPeriodRules.WithdrawalWindowEnd(date)));
        Assert.True(AfterSalesPeriodRules.IsWithdrawalWindowOpen(Facts(null), Start.AddYears(5)));
    }

    // ---- KK6: the physical items are all cancelled ----

    [Fact]
    public async Task WhenThePhysicalItemsAreAllCancelled_TheOrderFallsToTheLineWithoutThem_AndTheLastMarkEndsItWithS10_KK6()
    {
        (Order order, List<OrderItem> items) = await SeedAsync(
            OrderStatus.Preparing, PaymentStatus.Paid, new ItemSpec(ProductType.Physical, 1), new ItemSpec(ProductType.Digital, 1, true), new ItemSpec(ProductType.Service, 1));
        await SaveAsync(c => c.Cancellations.Add(Cancellation.ByBuyer(items[0].Id, 1, Amounts, null, Start)));

        // No physical item is open any more: the shipment is refused, and the order's line is the one without a physical item (1.5.7).
        OrderTransitionResult ship = await RunAsync((unit, token) => unit.ShipOrderAsync(new ShipOrderRequest(order.Id, AdminCaller, 1, true, Listed), token));
        Assert.Equal(TransitionRefusal.ConditionNotMet, ship.Refusal);
        Assert.Equal(OrderLine.MixedPhysicalClosed, OrderLines.LineOf([ProductType.Physical, ProductType.Digital, ProductType.Service], PaymentMethod.Card, physicalClosed: true));
        Assert.Equal([ManualStep.CompleteService], OrderLines.StepsOf([ProductType.Service], PaymentMethod.Card));

        OrderTransitionResult completed = await RunAsync((unit, token) => unit.MarkServiceCompletedAsync(new MarkServiceCompletedRequest(order.Id, items[2].Id, AdminCaller, 1), token));

        Assert.Equal(TransitionCode.S10, completed.ClosureCode);
        await using AppDbContext reader = NewContext();
        Assert.Equal((OrderStatus.Delivered, 2), await ReadStatusAsync(order.Id));
        TransitionRecord record = await reader.TransitionRecords.SingleAsync(t => t.OrderId == order.Id, Ct);
        Assert.Equal((TransitionCode.S10, ActorKind.Admin), (record.TransitionCode, record.ActorKind));
    }

    [Fact]
    public async Task WhenEveryItemIsCancelled_NoServiceMarkIsLeftToSet_KK6()
    {
        (Order order, List<OrderItem> items) = await SeedAsync(OrderStatus.Preparing, PaymentStatus.Paid, new ItemSpec(ProductType.Physical, 1), new ItemSpec(ProductType.Service, 1));
        await SaveAsync(c =>
        {
            c.Cancellations.Add(Cancellation.ByBuyer(items[0].Id, 1, Amounts, null, Start));
            c.Cancellations.Add(Cancellation.ByBuyer(items[1].Id, 1, Amounts, null, Start));
        });

        OrderTransitionResult completed = await RunAsync((unit, token) => unit.MarkServiceCompletedAsync(new MarkServiceCompletedRequest(order.Id, items[1].Id, AdminCaller, 1), token));

        Assert.Equal(TransitionRefusal.ConditionNotMet, completed.Refusal);
        Assert.Null((await ReadItemAsync(items[1].Id)).DeliveryMarkedAt);
    }

    // ---- KK7: an order without a physical item ----

    [Fact]
    public async Task AnOrderWithoutAPhysicalItem_StaysPlaced_TakesNoShippingStep_AndEndsWithTheLastMark_KK7()
    {
        (Order order, List<OrderItem> items) = await SeedAsync(OrderStatus.Placed, PaymentStatus.Paid, new ItemSpec(ProductType.Digital, 1, true), new ItemSpec(ProductType.Service, 1));

        OrderTransitionResult ship = await RunAsync((unit, token) => unit.ShipOrderAsync(new ShipOrderRequest(order.Id, AdminCaller, 1, true, Listed), token));
        OrderTransitionResult delivered = await RunAsync((unit, token) => unit.MarkDeliveredAsync(new MarkDeliveredRequest(order.Id, AdminCaller, 1, StartDay), token));
        OrderTransitionResult failed = await RunAsync((unit, token) => unit.MarkDeliveryFailedAsync(new AdminOrderRequest(order.Id, AdminCaller, 1), token));
        Assert.Equal(TransitionRefusal.NotInTheTable, ship.Refusal);
        Assert.Equal(TransitionRefusal.NotInTheTable, delivered.Refusal);
        Assert.Equal(TransitionRefusal.NotInTheTable, failed.Refusal);
        Assert.Equal((OrderStatus.Placed, 1), await ReadStatusAsync(order.Id));

        OrderTransitionResult completed = await RunAsync((unit, token) => unit.MarkServiceCompletedAsync(new MarkServiceCompletedRequest(order.Id, items[1].Id, AdminCaller, 1), token));

        Assert.Equal(TransitionCode.S2, completed.ClosureCode);
        await using AppDbContext reader = NewContext();
        Assert.Equal((OrderStatus.Delivered, 2), await ReadStatusAsync(order.Id));
        Assert.Equal([TransitionCode.S2], await reader.TransitionRecords.Where(t => t.OrderId == order.Id).Select(t => t.TransitionCode).ToListAsync(Ct));
        Assert.Null((await reader.Orders.SingleAsync(o => o.Id == order.Id, Ct)).Carrier);
    }

    [Fact]
    public async Task AnOrderWithTwoServiceItems_WaitsForTheLastMark_AndEachMarkTakesItsOwnOpenQuantity_KK7()
    {
        (Order order, List<OrderItem> items) = await SeedAsync(OrderStatus.Placed, PaymentStatus.Paid, new ItemSpec(ProductType.Service, 2), new ItemSpec(ProductType.Service, 1));
        await SaveAsync(c => c.Cancellations.Add(Cancellation.ByBuyer(items[0].Id, 1, Amounts, null, Start)));

        OrderTransitionResult first = await RunAsync((unit, token) => unit.MarkServiceCompletedAsync(new MarkServiceCompletedRequest(order.Id, items[0].Id, AdminCaller, 1), token));
        Assert.False(first.IsRejected);
        Assert.Null(first.ClosureCode);
        Assert.Equal((OrderStatus.Placed, 2), await ReadStatusAsync(order.Id));

        OrderTransitionResult last = await RunAsync((unit, token) => unit.MarkServiceCompletedAsync(new MarkServiceCompletedRequest(order.Id, items[1].Id, AdminCaller, 2), token));

        Assert.Equal(TransitionCode.S2, last.ClosureCode);
        Assert.Equal((OrderStatus.Delivered, 3), await ReadStatusAsync(order.Id));
    }

    // ---- helpers ----

    private static WithdrawalWindowFacts Facts(DateOnly? deliveryDate) => new(ProductType.Physical, deliveryDate, null, Start, true, deliveryDate);

    private static void AssertMark(OrderItem item, DateTimeOffset? markedAt, DateOnly? date)
    {
        Assert.Equal(markedAt, item.DeliveryMarkedAt);
        Assert.Equal(date, item.DeliveryDate);
    }

    private async Task<OrderItem> ReadItemAsync(Guid id)
    {
        await using AppDbContext reader = NewContext();
        return await reader.OrderItems.AsNoTracking().SingleAsync(i => i.Id == id, Ct);
    }

    private async Task<(OrderStatus Status, int Version)> ReadStatusAsync(Guid orderId)
    {
        await using AppDbContext reader = NewContext();
        Order saved = await reader.Orders.AsNoTracking().SingleAsync(o => o.Id == orderId, Ct);
        return (saved.OrderStatus, saved.Version);
    }

    private async Task<T> RunAsync<T>(Func<IOrderTransitions, CancellationToken, Task<T>> work)
        where T : IOperationResult
    {
        await using ServiceProvider provider = BuildProvider();
        await using AsyncServiceScope scope = provider.CreateAsyncScope();
        IOrderTransitions unit = scope.ServiceProvider.GetRequiredService<IOrderTransitions>();
        return await scope.ServiceProvider.GetRequiredService<ITransactionRunner>().RunAsync(token => work(unit, token), Ct);
    }

    private async Task<T> RunModuleAsync<T>(Func<IOrderingModule, CancellationToken, Task<T>> work)
        where T : IOperationResult
    {
        await using ServiceProvider provider = BuildProvider();
        await using AsyncServiceScope scope = provider.CreateAsyncScope();
        IOrderingModule module = scope.ServiceProvider.GetRequiredService<IOrderingModule>();
        return await scope.ServiceProvider.GetRequiredService<ITransactionRunner>().RunAsync(token => work(module, token), Ct);
    }

    private async Task<(Order Order, List<OrderItem> Items)> SeedAsync(OrderStatus orderStatus, PaymentStatus paymentStatus, params ItemSpec[] specs)
    {
        int n = Interlocked.Increment(ref _counter);
        Province province = Province.Create($"{n:D2}", $"İl {n}", Start);
        LegalTextVersion form = LegalTextVersion.CreatePublished(LegalTextType.PreInformationForm, n, Start, "metin", Start);
        LegalTextVersion contract = LegalTextVersion.CreatePublished(LegalTextType.DistanceSalesContract, n, Start, "metin", Start);
        LegalTextVersion notice = LegalTextVersion.CreatePublished(LegalTextType.PrivacyNotice, n, Start, "metin", Start).WithHosting();
        Order order = Order.Create(
            $"2026-{n:D6}", $"access-placeholder-{n}", $"confirm-placeholder-{n}", PaymentMethod.Card, true, "Ali@Example.com", 120m, 0m, Start.AddHours(1), Start);
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

        List<OrderItem> items = [];
        foreach (ItemSpec spec in specs)
        {
            OrderItem item = OrderItem.Create(order.Id, null, null, spec.Type, "Ürün", spec.Quantity, 100m, 20m, 16.67m, Start);
            if (spec.Type == ProductType.Digital)
            {
                item.DownloadAllowance = 5;
            }

            if (spec.Type == ProductType.Service)
            {
                item.FulfilmentPeriod = 3;
            }

            if (spec.Delivered)
            {
                item.DeliveryMarkedAt = Start;
                item.DeliveryDate = StartDay;
            }

            items.Add(item);
        }

        await SaveAsync(c =>
        {
            c.Provinces.Add(province);
            c.LegalTextVersions.AddRange(form, contract, notice);
            c.Orders.Add(order);
            c.OrderItems.AddRange(items);
        });

        // The states are set around the entity (it takes them only through the unit); the pair rule of the database still applies.
        await ExecuteAsync($"UPDATE orders SET order_status = '{orderStatus}', payment_status = '{paymentStatus}' WHERE id = '{order.Id}'");
        return (order, items);
    }

    private ServiceProvider BuildProvider()
    {
        ServiceCollection services = new();
        services.AddDbContext<AppDbContext>(options => AppDbContext.Configure(options, _collection.Dsn));
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
        services.AddCatalogModule();
        services.AddAfterSalesModule();
        services.AddNoticeDependencies();
        services.AddPaymentModule();
        services.AddOrderingModule();
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

    /// <summary>One item of a seeded order: its type, quantity and whether it already carries the delivery mark.</summary>
    public sealed record ItemSpec(ProductType Type, int Quantity, bool Delivered = false);

    /// <summary>A clock a test sets by hand: the unit reads it for the record moment, the delivery mark and the delivery date's upper bound.</summary>
    private sealed class MovableClock : TimeProvider
    {
        private DateTimeOffset _now;

        public MovableClock(DateTimeOffset now)
        {
            _now = now;
        }

        public void Set(DateTimeOffset now) => _now = now;

        public override DateTimeOffset GetUtcNow() => _now;
    }
}
