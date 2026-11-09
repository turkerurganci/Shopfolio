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
using Shopfolio.Modules.Notifications.Contracts;
using Shopfolio.Modules.Ordering;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Modules.Payment;
using Shopfolio.Modules.Payment.Contracts;
using Shopfolio.Modules.Settings;
using Shopfolio.Modules.Settings.Contracts;

namespace Shopfolio.IntegrationTests.Ordering;

/// <summary>
/// The shipment axis of the transition unit on a real PostgreSQL (T316 KK1-KK9; 03 1.2, 1.6.1; 02 5.4; 05 4.1): S1, the three cancellations, the
/// reasonless S9 closing, S5, S6, S7 and S8 through the unit's open face, each in the transaction runner like a page would call it. What a refusal wrote is
/// read back from the tables - the order, the records, the dispatch rows - and every expected value comes from the table or the contract, not from a copy.
/// </summary>
[Collection(OrderTransitionsDefinition.Name)]
public sealed class ShipmentTransitionsTests : IAsyncLifetime
{
    private static readonly DateTimeOffset Start = new(2026, 11, 7, 9, 0, 0, TimeSpan.Zero);

    private static readonly ItemAmounts Amounts = new(100m, 0m, 16.67m);

    private static readonly ShipmentInfo Listed = new("YurticiKargo", null, "TR-1001", false);

    private readonly CollectionDatabaseFixture _collection;

    private readonly MovableClock _clock = new(Start);

    private Admin? _admin;

    private int _counter;

    public ShipmentTransitionsTests(CollectionDatabaseFixture collection)
    {
        _collection = collection;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private TransitionAdmin AdminCaller => new(_admin!.Id, _admin.Name);

    public async ValueTask InitializeAsync()
    {
        await ExecuteAsync(
            "TRUNCATE dispatch_records, cancellations, internal_notes, transition_records, order_items, orders, admins, legal_text_versions, provinces CASCADE");
        _admin = Admin.Create("Firma Yöneticisi", "yonetici@example.com", "digest-placeholder", Start);
        await SaveAsync(c => c.Admins.Add(_admin));
        _clock.Set(Start);
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    // ---- KK1: the white list; the correction is not stopped by it ----

    [Theory]
    [InlineData(OrderStatus.Placed, Op.Ship)]
    [InlineData(OrderStatus.Placed, Op.MarkDelivered)]
    [InlineData(OrderStatus.Placed, Op.MarkDeliveryFailed)]
    [InlineData(OrderStatus.Placed, Op.Resend)]
    [InlineData(OrderStatus.Placed, Op.CloseReturned)]
    [InlineData(OrderStatus.Preparing, Op.MarkDelivered)]
    [InlineData(OrderStatus.Preparing, Op.MarkDeliveryFailed)]
    [InlineData(OrderStatus.Preparing, Op.Resend)]
    [InlineData(OrderStatus.Shipped, Op.Ship)]
    [InlineData(OrderStatus.Shipped, Op.Resend)]
    [InlineData(OrderStatus.Shipped, Op.CancelByAdmin)]
    [InlineData(OrderStatus.Shipped, Op.CloseReturned)]
    [InlineData(OrderStatus.DeliveryFailed, Op.Ship)]
    [InlineData(OrderStatus.DeliveryFailed, Op.MarkDelivered)]
    [InlineData(OrderStatus.DeliveryFailed, Op.MarkDeliveryFailed)]
    public async Task AnOperationOutsideTheTable_IsRefused_AndWritesNothing_KK1(OrderStatus status, Op op)
    {
        (Order order, _) = await SeedAsync(status, PaymentStatus.Paid, Physical(2));

        OrderTransitionResult result = await RunAsync((unit, token) => CallAsync(unit, op, order.Id, 1, token));

        Assert.Equal(TransitionRefusal.NotInTheTable, result.Refusal);
        await AssertNothingWrittenAsync(order.Id, status, PaymentStatus.Paid);
    }

    [Fact]
    public async Task ThereIsNoBackwardOperation_NoMethodOfTheOpenFace_LeavesShippedForPreparing_KK1()
    {
        // The open face names every transition by its operation; the table has no backward row, and the unit refuses a code that is not in it.
        (Order order, _) = await SeedAsync(OrderStatus.Shipped, PaymentStatus.Paid, Physical(1));

        TransitionOutcome outcome = await RunCoreAsync((core, token) => core.ApplyAsync(
            new TransitionRequest(order.Id, TransitionCode.S1, TransitionActor.System), null, token));

        Assert.Equal(TransitionRejection.NotInTheTable, outcome.Rejection);
        Assert.DoesNotContain(TransitionTable.Shipment, t => t.From == OrderStatus.Shipped && t.To == OrderStatus.Preparing);
        await AssertNothingWrittenAsync(order.Id, OrderStatus.Shipped, PaymentStatus.Paid);
    }

    [Fact]
    public async Task TheCorrection_IsNotStoppedByTheWhiteListsRefusal_KK1()
    {
        (Order order, _) = await SeedAsync(OrderStatus.Shipped, PaymentStatus.Paid, Physical(1));
        OrderTransitionResult shipped = await RunAsync((unit, token) => unit.MarkDeliveredAsync(
            new MarkDeliveredRequest(order.Id, AdminCaller, 1, DateOnly.FromDateTime(Start.UtcDateTime)), token));
        Assert.False(shipped.IsRejected);
        Guid deliveredRecord = shipped.TransitionRecordIds.Single();

        // Delivered to Shipped is not a row of the table (the white list refuses the S5 from Delivered); the correction puts the order back all the same.
        OrderTransitionResult refused = await RunAsync((unit, token) => unit.ShipOrderAsync(
            new ShipOrderRequest(order.Id, AdminCaller, 2, true, Listed), token));
        TransitionOutcome corrected = await RunCoreAsync((core, token) => core.CorrectAsync(
            new CorrectionRequest(order.Id, deliveredRecord, CorrectionReason.MarkedBeforeOccurred, null, TransitionActor.Admin(_admin!.Id, _admin.Name), 2), null, token));

        Assert.Equal(TransitionRefusal.NotInTheTable, refused.Refusal);
        Assert.False(corrected.IsRejected);
        await using AppDbContext reader = NewContext();
        Assert.Equal(OrderStatus.Shipped, (await reader.Orders.SingleAsync(o => o.Id == order.Id, Ct)).OrderStatus);
    }

    // ---- KK2: the terminal states ----

    [Theory]
    [InlineData(OrderStatus.Delivered, Op.CancelByAdmin)]
    [InlineData(OrderStatus.Delivered, Op.CancelByCustomer)]
    [InlineData(OrderStatus.Delivered, Op.CloseReturned)]
    [InlineData(OrderStatus.Delivered, Op.Ship)]
    [InlineData(OrderStatus.Delivered, Op.MarkDelivered)]
    [InlineData(OrderStatus.Delivered, Op.MarkDeliveryFailed)]
    [InlineData(OrderStatus.Delivered, Op.Resend)]
    [InlineData(OrderStatus.Cancelled, Op.CancelByAdmin)]
    [InlineData(OrderStatus.Cancelled, Op.CancelByCustomer)]
    [InlineData(OrderStatus.Cancelled, Op.CancelBySystem)]
    [InlineData(OrderStatus.Cancelled, Op.CloseReturned)]
    [InlineData(OrderStatus.Cancelled, Op.Ship)]
    [InlineData(OrderStatus.Cancelled, Op.MarkDelivered)]
    [InlineData(OrderStatus.Cancelled, Op.MarkDeliveryFailed)]
    [InlineData(OrderStatus.Cancelled, Op.Resend)]
    public async Task ATerminalState_IsLeftByNoOperation_KK2(OrderStatus status, Op op)
    {
        (Order order, _) = await SeedAsync(status, PaymentStatus.Paid, Physical(1));

        OrderTransitionResult result = await RunAsync((unit, token) => CallAsync(unit, op, order.Id, 1, token));

        Assert.True(result.IsRejected);
        await AssertNothingWrittenAsync(order.Id, status, PaymentStatus.Paid);
    }

    // ---- KK3: S1 ----

    [Fact]
    public async Task S1_OnAMixedOrderWithAPhysicalItem_ComesWithTheCardApprovalO1_OneVersionRaise_AndOneB4Row_KK3()
    {
        // Ö1 with S1 has one path, the payment axis's (D-389): the card payment writes both, the payment record and one B-4 row.
        (Order order, _) = await SeedAsync(OrderStatus.Placed, PaymentStatus.Pending, new ItemSpec(ProductType.Digital, 1), Physical(1));

        PaymentTransitionResult result = await RunAsync((unit, token) => unit.ApplyCardPaymentAsync(new CardPaymentSucceeded(order.Id, "trx-mixed"), token));

        Assert.Equal([TransitionCode.O1, TransitionCode.S1], result.Applied);
        await using AppDbContext reader = NewContext();
        Order saved = await reader.Orders.SingleAsync(o => o.Id == order.Id, Ct);
        Assert.Equal((OrderStatus.Preparing, PaymentStatus.Paid, 2), (saved.OrderStatus, saved.PaymentStatus, saved.Version));
        List<TransitionRecord> records = await reader.TransitionRecords.Where(t => t.OrderId == order.Id).ToListAsync(Ct);
        Assert.Equivalent(new[] { TransitionCode.O1, TransitionCode.S1 }, records.Select(r => r.TransitionCode));
        Assert.All(records, r => Assert.Equal(ActorKind.System, r.ActorKind));
        PaymentRecord payment = await reader.PaymentRecords.SingleAsync(p => p.OrderId == order.Id, Ct);
        Assert.Equal(records.Single(r => r.TransitionCode == TransitionCode.O1).Id, payment.TransitionRecordId);
        List<DispatchRecord> rows = await reader.DispatchRecords.Where(d => d.OrderId == order.Id).ToListAsync(Ct);
        DispatchRecord row = Assert.Single(rows);
        Assert.Equal(DispatchType.B4, row.DispatchType);
        Assert.Equal("Ali@Example.com", row.RecipientEmail);
    }

    // ---- KK4: the last event writes the closure ----

    [Theory]
    [InlineData(OrderStatus.Placed, ProductType.Digital, TransitionCode.S2)]
    [InlineData(OrderStatus.Preparing, ProductType.Physical, TransitionCode.S10)]
    public async Task TheLastEventThatCompletesTheClosure_WritesTheDeliveredTransition_InTheSameOperation_KK4(OrderStatus status, ProductType closedType, TransitionCode expected)
    {
        // An order with one closed item of its line and one delivered digital item: the closing record is the last event.
        ItemSpec delivered = new(ProductType.Digital, 1, true);
        (Order order, List<OrderItem> items) = await SeedAsync(status, PaymentStatus.Paid, new ItemSpec(closedType, 1), delivered);
        ClosureResult closure = ClosureResult.None;

        await RunCoreAsync(async (core, token) =>
        {
            AppDbContext db = ScopeDb!;
            await ScopeLocks!.LockAsync([order.Id], token);
            Order locked = await db.Orders.SingleAsync(o => o.Id == order.Id, token);
            db.Cancellations.Add(Cancellation.ByBuyer(items[0].Id, items[0].Quantity, Amounts, null, Start));
            locked.BumpVersion();
            closure = await core.EvaluateClosureAsync(locked, TransitionActor.Buyer, token);
            return new TransitionOutcome(null, [], closure);
        });

        Assert.Equal(expected, closure.Code);
        await using AppDbContext reader = NewContext();
        Assert.Equal(OrderStatus.Delivered, (await reader.Orders.SingleAsync(o => o.Id == order.Id, Ct)).OrderStatus);
        TransitionRecord record = await reader.TransitionRecords.SingleAsync(t => t.OrderId == order.Id, Ct);
        Assert.Equal((expected, ActorKind.Buyer), (record.TransitionCode, record.ActorKind));
    }

    [Fact]
    public async Task S7_OfAShipmentWhoseItemsAreClosedAndOthersDelivered_EndsInS11_InTheSameOperation_KK4()
    {
        (Order order, List<OrderItem> items) = await SeedAsync(OrderStatus.Shipped, PaymentStatus.Paid, Physical(1), new ItemSpec(ProductType.Digital, 1, true));
        await SaveAsync(c => c.Cancellations.Add(Cancellation.ByBuyer(items[0].Id, 1, Amounts, null, Start)));

        OrderTransitionResult result = await RunAsync((unit, token) => unit.MarkDeliveryFailedAsync(new AdminOrderRequest(order.Id, AdminCaller, 1), token));

        Assert.False(result.IsRejected);
        Assert.Equal(TransitionCode.S11, result.ClosureCode);
        await using AppDbContext reader = NewContext();
        Assert.Equal(OrderStatus.Delivered, (await reader.Orders.SingleAsync(o => o.Id == order.Id, Ct)).OrderStatus);
        Assert.Equivalent(
            new[] { TransitionCode.S7, TransitionCode.S11 },
            await reader.TransitionRecords.Where(t => t.OrderId == order.Id).Select(t => t.TransitionCode).ToListAsync(Ct));
    }

    // ---- KK5: every cancellation only by its own triggers; B-7 goes ----

    [Fact]
    public async Task S3_ByTheCustomer_OfAnUnpaidOrder_TakesBothAxes_AndWritesOneB7Row_KK5()
    {
        (Order order, _) = await SeedAsync(OrderStatus.Placed, PaymentStatus.Pending, Physical(1));

        OrderTransitionResult result = await RunAsync((unit, token) => unit.CancelOrderAsync(CancelOrderRequest.ByCustomer(order.Id), token));

        Assert.False(result.IsRejected);
        await using AppDbContext reader = NewContext();
        Order saved = await reader.Orders.SingleAsync(o => o.Id == order.Id, Ct);
        Assert.Equal((OrderStatus.Cancelled, PaymentStatus.Failed, 2), (saved.OrderStatus, saved.PaymentStatus, saved.Version));
        Assert.Equivalent(
            new[] { TransitionCode.S3, TransitionCode.O2 },
            await reader.TransitionRecords.Where(t => t.OrderId == order.Id && t.ActorKind == ActorKind.Buyer).Select(t => t.TransitionCode).ToListAsync(Ct));
        Assert.Equal(DispatchType.B7, Assert.Single(await reader.DispatchRecords.Where(d => d.OrderId == order.Id).ToListAsync(Ct)).DispatchType);
    }

    [Fact]
    public async Task S3_ByTheCustomer_OfAPaidOrder_NeedsTheLastOpenItemsCancellation_KK5()
    {
        (Order order, List<OrderItem> items) = await SeedAsync(OrderStatus.Placed, PaymentStatus.Paid, new ItemSpec(ProductType.Digital, 1), new ItemSpec(ProductType.Service, 1));
        CancelledItem named = new("Ürün", null, null, 1);

        // The whole paid order is not the customer's cancellation: only the last open item's is.
        OrderTransitionResult whole = await RunAsync((unit, token) => unit.CancelOrderAsync(CancelOrderRequest.ByCustomer(order.Id), token));
        // An item cancellation with another item still open is not the last one.
        await SaveAsync(c => c.Cancellations.Add(Cancellation.ByBuyer(items[0].Id, 1, Amounts, null, Start)));
        OrderTransitionResult notLast = await RunAsync((unit, token) => unit.CancelOrderAsync(CancelOrderRequest.ByCustomer(order.Id, named), token));
        await SaveAsync(c => c.Cancellations.Add(Cancellation.ByBuyer(items[1].Id, 1, Amounts, null, Start)));
        OrderTransitionResult last = await RunAsync((unit, token) => unit.CancelOrderAsync(CancelOrderRequest.ByCustomer(order.Id, named), token));

        Assert.Equal(TransitionRefusal.ConditionNotMet, whole.Refusal);
        Assert.Equal(TransitionRefusal.ConditionNotMet, notLast.Refusal);
        Assert.False(last.IsRejected);
        await using AppDbContext reader = NewContext();
        Assert.Equal(OrderStatus.Cancelled, (await reader.Orders.SingleAsync(o => o.Id == order.Id, Ct)).OrderStatus);
        Assert.Equal([TransitionCode.S3], await reader.TransitionRecords.Where(t => t.OrderId == order.Id).Select(t => t.TransitionCode).ToListAsync(Ct));
        Assert.Equal(DispatchType.B7, Assert.Single(await reader.DispatchRecords.Where(d => d.OrderId == order.Id).ToListAsync(Ct)).DispatchType);
    }

    [Fact]
    public async Task S4_ByTheCustomer_IsTheLastOpenItemsCancellation_OfAPreparingOrder_KK5()
    {
        (Order order, List<OrderItem> items) = await SeedAsync(OrderStatus.Preparing, PaymentStatus.Paid, Physical(1));
        await SaveAsync(c => c.Cancellations.Add(Cancellation.ByBuyer(items[0].Id, 1, Amounts, null, Start)));

        OrderTransitionResult result = await RunAsync((unit, token) => unit.CancelOrderAsync(CancelOrderRequest.ByCustomer(order.Id, new CancelledItem("Ürün", null, null, 1)), token));

        Assert.False(result.IsRejected);
        await using AppDbContext reader = NewContext();
        TransitionRecord record = await reader.TransitionRecords.SingleAsync(t => t.OrderId == order.Id, Ct);
        Assert.Equal((TransitionCode.S4, ActorKind.Buyer, (TransitionReason?)null), (record.TransitionCode, record.ActorKind, record.TransitionReason));
        Assert.Equal(DispatchType.B7, Assert.Single(await reader.DispatchRecords.Where(d => d.OrderId == order.Id).ToListAsync(Ct)).DispatchType);
    }

    [Theory]
    [InlineData(OrderStatus.Placed, PaymentStatus.Paid, TransitionCode.S3)]
    [InlineData(OrderStatus.Preparing, PaymentStatus.Paid, TransitionCode.S4)]
    [InlineData(OrderStatus.DeliveryFailed, PaymentStatus.Paid, TransitionCode.S9)]
    public async Task TheAdministratorsCancellation_ByTheState_IsS3S4OrS9_WithTheReason_AndOneB7Row_KK5(OrderStatus status, PaymentStatus payment, TransitionCode expected)
    {
        (Order order, _) = await SeedAsync(status, payment, Physical(1));

        OrderTransitionResult result = await RunAsync((unit, token) => unit.CancelOrderAsync(
            CancelOrderRequest.ByAdministrator(order.Id, AdminCaller, 1, true, CancellationReason.Other, " Ürün hasarlı "), token));

        Assert.False(result.IsRejected);
        await using AppDbContext reader = NewContext();
        TransitionRecord record = await reader.TransitionRecords.SingleAsync(t => t.OrderId == order.Id, Ct);
        Assert.Equal((expected, ActorKind.Admin, CancellationReason.Other, "Ürün hasarlı", _admin!.Id), (record.TransitionCode, record.ActorKind, record.CancellationReason, record.ReasonNote, record.AdminId));
        Assert.Equal(OrderStatus.Cancelled, (await reader.Orders.SingleAsync(o => o.Id == order.Id, Ct)).OrderStatus);
        Assert.Equal(DispatchType.B7, Assert.Single(await reader.DispatchRecords.Where(d => d.OrderId == order.Id).ToListAsync(Ct)).DispatchType);
    }

    [Theory]
    [InlineData(OrderStatus.Placed)]
    [InlineData(OrderStatus.Preparing)]
    [InlineData(OrderStatus.DeliveryFailed)]
    public async Task TheAdministratorsWholeOrderCancellation_IsRefused_WhileAnItemIsMarkedDelivered_AndWritesNothing_KK5(OrderStatus status)
    {
        // The state names the transition (Placed S3, Preparing S4, DeliveryFailed S9). The open item is a service (S3) or a physical item (S4, S9); the delivered one is a digital item. A delivered item is not cancelled, so the order does not end in Cancelled (02 5.4).
        ItemSpec open = status == OrderStatus.Placed ? new ItemSpec(ProductType.Service, 1, false) : Physical(1);
        (Order order, _) = await SeedAsync(status, PaymentStatus.Paid, open, new ItemSpec(ProductType.Digital, 1, true));

        OrderTransitionResult result = await RunAsync((unit, token) => unit.CancelOrderAsync(
            CancelOrderRequest.ByAdministrator(order.Id, AdminCaller, 1, true, CancellationReason.Other, "Ürün hasarlı"), token));

        Assert.Equal(TransitionRefusal.ConditionNotMet, result.Refusal);
        await AssertNothingWrittenAsync(order.Id, status, PaymentStatus.Paid);
    }

    [Fact]
    public async Task TheAdministratorsCancellation_OfAnUnpaidOrder_CarriesTheReasonOnBothRecords_KK5()
    {
        (Order order, _) = await SeedAsync(OrderStatus.Placed, PaymentStatus.Pending, Physical(1));

        OrderTransitionResult result = await RunAsync((unit, token) => unit.CancelOrderAsync(
            CancelOrderRequest.ByAdministrator(order.Id, AdminCaller, 1, true, CancellationReason.OutOfStock), token));

        Assert.False(result.IsRejected);
        await using AppDbContext reader = NewContext();
        List<TransitionRecord> records = await reader.TransitionRecords.Where(t => t.OrderId == order.Id).ToListAsync(Ct);
        Assert.Equivalent(new[] { TransitionCode.S3, TransitionCode.O2 }, records.Select(r => r.TransitionCode));
        Assert.All(records, r => Assert.Equal(CancellationReason.OutOfStock, r.CancellationReason));
        Assert.Equal(PaymentStatus.Failed, (await reader.Orders.SingleAsync(o => o.Id == order.Id, Ct)).PaymentStatus);
    }

    [Theory]
    [InlineData(TransitionReason.PaymentWindowExpired)]
    [InlineData(TransitionReason.CardPaymentFailed)]
    [InlineData(TransitionReason.PaymentPageNotOpened)]
    [InlineData(TransitionReason.SameCartReorder)]
    public async Task S3_BySystem_OfAnUnpaidOrder_CarriesTheKindOfAutomaticCancellation_AndB7_KK5(TransitionReason reason)
    {
        (Order order, _) = await SeedAsync(OrderStatus.Placed, PaymentStatus.Pending, Physical(1));

        OrderTransitionResult result = await RunAsync((unit, token) => unit.CancelOrderAsync(CancelOrderRequest.BySystem(order.Id, reason), token));

        Assert.False(result.IsRejected);
        await using AppDbContext reader = NewContext();
        List<TransitionRecord> records = await reader.TransitionRecords.Where(t => t.OrderId == order.Id).ToListAsync(Ct);
        Assert.All(records, r => Assert.Equal((ActorKind.System, (TransitionReason?)reason), (r.ActorKind, r.TransitionReason)));
        Assert.Equal(DispatchType.B7, Assert.Single(await reader.DispatchRecords.Where(d => d.OrderId == order.Id).ToListAsync(Ct)).DispatchType);
    }

    [Fact]
    public async Task TheOtherActors_AreRefusedByEachCancellation_AndWriteNothing_KK5()
    {
        (Order placedPaid, _) = await SeedAsync(OrderStatus.Placed, PaymentStatus.Paid, Physical(1));
        (Order preparing, _) = await SeedAsync(OrderStatus.Preparing, PaymentStatus.Paid, Physical(1));
        (Order returned, _) = await SeedAsync(OrderStatus.DeliveryFailed, PaymentStatus.Paid, Physical(1));

        // S3 of a paid order by the system; S4 by the system (closure only); S9 by the customer and by the system.
        OrderTransitionResult systemS3 = await RunAsync((unit, token) => unit.CancelOrderAsync(CancelOrderRequest.BySystem(placedPaid.Id, TransitionReason.PaymentWindowExpired), token));
        OrderTransitionResult systemS4 = await RunAsync((unit, token) => unit.CancelOrderAsync(CancelOrderRequest.BySystem(preparing.Id, TransitionReason.CardPaymentFailed), token));
        OrderTransitionResult customerS9 = await RunAsync((unit, token) => unit.CancelOrderAsync(CancelOrderRequest.ByCustomer(returned.Id), token));
        OrderTransitionResult systemS9 = await RunAsync((unit, token) => unit.CancelOrderAsync(CancelOrderRequest.BySystem(returned.Id, TransitionReason.CardPaymentFailed), token));

        Assert.Equal(TransitionRefusal.ConditionNotMet, systemS3.Refusal);
        Assert.Equal(TransitionRefusal.ReasonNotAllowed, systemS4.Refusal);
        Assert.Equal(TransitionRefusal.ActorNotAllowed, customerS9.Refusal);
        Assert.Equal(TransitionRefusal.ActorNotAllowed, systemS9.Refusal);
        await AssertNothingWrittenAsync(placedPaid.Id, OrderStatus.Placed, PaymentStatus.Paid);
        await AssertNothingWrittenAsync(preparing.Id, OrderStatus.Preparing, PaymentStatus.Paid);
        await AssertNothingWrittenAsync(returned.Id, OrderStatus.DeliveryFailed, PaymentStatus.Paid);
    }

    [Theory]
    [MemberData(nameof(AdministratorCancellationInputs))]
    public async Task TheAdministratorsCancellation_WithoutItsInputs_IsRefused_AndWritesNothing_KK5(CancellationReason? reason, string? note, bool confirmed, int version, TransitionRefusal expected)
    {
        (Order order, _) = await SeedAsync(OrderStatus.Preparing, PaymentStatus.Paid, Physical(1));
        CancelOrderRequest request = new(order.Id, ActorKind.Admin, AdminCaller, version, confirmed, reason, note, null, null);

        OrderTransitionResult result = await RunAsync((unit, token) => unit.CancelOrderAsync(request, token));

        Assert.Equal(expected, result.Refusal);
        await AssertNothingWrittenAsync(order.Id, OrderStatus.Preparing, PaymentStatus.Paid);
    }

    public static TheoryData<CancellationReason?, string?, bool, int, TransitionRefusal> AdministratorCancellationInputs() => new()
    {
        { CancellationReason.OutOfStock, null, false, 1, TransitionRefusal.ConfirmationRequired },
        { null, null, true, 1, TransitionRefusal.InvalidInput },
        { CancellationReason.Other, null, true, 1, TransitionRefusal.InvalidInput },
        { CancellationReason.OutOfStock, "not for this reason", true, 1, TransitionRefusal.InvalidInput },
        { CancellationReason.OutOfStock, null, true, 7, TransitionRefusal.VersionConflict },
    };

    [Fact]
    public async Task TheSystemsCancellation_WithTheClosureAsItsReason_IsRefused_KK5()
    {
        (Order order, _) = await SeedAsync(OrderStatus.Placed, PaymentStatus.Pending, Physical(1));

        OrderTransitionResult result = await RunAsync((unit, token) => unit.CancelOrderAsync(CancelOrderRequest.BySystem(order.Id, TransitionReason.Closure), token));

        Assert.Equal(TransitionRefusal.ReasonNotAllowed, result.Refusal);
        await AssertNothingWrittenAsync(order.Id, OrderStatus.Placed, PaymentStatus.Pending);
    }

    // ---- KK6: the closures take no reason and make no B-7 ----

    [Theory]
    [InlineData(OrderStatus.Placed, TransitionCode.S3)]
    [InlineData(OrderStatus.Preparing, TransitionCode.S4)]
    public async Task TheClosure_OfPlacedAndPreparing_IsWrittenWithoutAReasonOfTheFirm_AndWithoutB7_KK6(OrderStatus status, TransitionCode expected)
    {
        (Order order, List<OrderItem> items) = await SeedAsync(status, PaymentStatus.Paid, new ItemSpec(status == OrderStatus.Placed ? ProductType.Digital : ProductType.Physical, 1));
        ClosureResult closure = ClosureResult.None;

        await RunCoreAsync(async (core, token) =>
        {
            await ScopeLocks!.LockAsync([order.Id], token);
            Order locked = await ScopeDb!.Orders.SingleAsync(o => o.Id == order.Id, token);
            ScopeDb.Cancellations.Add(Cancellation.ByBuyer(items[0].Id, 1, Amounts, null, Start));
            locked.BumpVersion();
            closure = await core.EvaluateClosureAsync(locked, TransitionActor.Buyer, token);
            return new TransitionOutcome(null, [], closure);
        });

        Assert.Equal(expected, closure.Code);
        await using AppDbContext reader = NewContext();
        TransitionRecord record = await reader.TransitionRecords.SingleAsync(t => t.OrderId == order.Id, Ct);
        Assert.Equal((ActorKind.System, (TransitionReason?)TransitionReason.Closure, (CancellationReason?)null), (record.ActorKind, record.TransitionReason, record.CancellationReason));
        Assert.Empty(await reader.DispatchRecords.Where(d => d.OrderId == order.Id).ToListAsync(Ct));
    }

    [Fact]
    public async Task S9_AsTheClosing_AfterS7_IsTheAdministratorsReasonlessClosure_AndWritesNoB7_KK6()
    {
        (Order order, List<OrderItem> items) = await SeedAsync(OrderStatus.Shipped, PaymentStatus.Paid, Physical(1));
        await SaveAsync(c => c.Cancellations.Add(Cancellation.ByBuyer(items[0].Id, 1, Amounts, null, Start)));
        OrderTransitionResult returned = await RunAsync((unit, token) => unit.MarkDeliveryFailedAsync(new AdminOrderRequest(order.Id, AdminCaller, 1), token));
        Assert.Null(returned.ClosureCode);

        OrderTransitionResult closed = await RunAsync((unit, token) => unit.CloseReturnedOrderAsync(new AdminOrderRequest(order.Id, AdminCaller, 2), token));

        Assert.False(closed.IsRejected);
        await using AppDbContext reader = NewContext();
        TransitionRecord record = await reader.TransitionRecords.SingleAsync(t => t.Id == closed.TransitionRecordIds.Single(), Ct);
        Assert.Equal((TransitionCode.S9, ActorKind.Admin, (TransitionReason?)TransitionReason.Closure, (CancellationReason?)null), (record.TransitionCode, record.ActorKind, record.TransitionReason, record.CancellationReason));
        Assert.Equal(OrderStatus.Cancelled, (await reader.Orders.SingleAsync(o => o.Id == order.Id, Ct)).OrderStatus);
        Assert.Equal([DispatchType.B6], await reader.DispatchRecords.Where(d => d.OrderId == order.Id).Select(d => d.DispatchType).ToListAsync(Ct));
    }

    [Fact]
    public async Task S9_AsTheClosing_IsRefused_WhileAnItemIsOpen_OrOneWasDelivered_KK6()
    {
        (Order open, _) = await SeedAsync(OrderStatus.DeliveryFailed, PaymentStatus.Paid, Physical(1));
        (Order delivered, List<OrderItem> items) = await SeedAsync(OrderStatus.DeliveryFailed, PaymentStatus.Paid, Physical(1), new ItemSpec(ProductType.Digital, 1, true));
        await SaveAsync(c => c.Cancellations.Add(Cancellation.ByBuyer(items[0].Id, 1, Amounts, null, Start)));

        OrderTransitionResult withOpenItem = await RunAsync((unit, token) => unit.CloseReturnedOrderAsync(new AdminOrderRequest(open.Id, AdminCaller, 1), token));
        OrderTransitionResult withDeliveredItem = await RunAsync((unit, token) => unit.CloseReturnedOrderAsync(new AdminOrderRequest(delivered.Id, AdminCaller, 1), token));

        Assert.Equal(TransitionRefusal.ConditionNotMet, withOpenItem.Refusal);
        Assert.Equal(TransitionRefusal.ConditionNotMet, withDeliveredItem.Refusal);
        await AssertNothingWrittenAsync(open.Id, OrderStatus.DeliveryFailed, PaymentStatus.Paid);
        await AssertNothingWrittenAsync(delivered.Id, OrderStatus.DeliveryFailed, PaymentStatus.Paid);
    }

    [Fact]
    public async Task TheFirmsS9Cancellation_OfAReturnedOrderWithNoOpenItem_IsRefused_BecauseItIsAClosure_KK6()
    {
        (Order order, List<OrderItem> items) = await SeedAsync(OrderStatus.DeliveryFailed, PaymentStatus.Paid, Physical(1));
        await SaveAsync(c => c.Cancellations.Add(Cancellation.ByBuyer(items[0].Id, 1, Amounts, null, Start)));

        OrderTransitionResult result = await RunAsync((unit, token) => unit.CancelOrderAsync(
            CancelOrderRequest.ByAdministrator(order.Id, AdminCaller, 1, true, CancellationReason.AgreedWithBuyer, agreement: AgreementConfirmation.FromOrderEmail), token));

        Assert.Equal(TransitionRefusal.ConditionNotMet, result.Refusal);
        await AssertNothingWrittenAsync(order.Id, OrderStatus.DeliveryFailed, PaymentStatus.Paid);
    }

    // ---- KK7: S5 and S8 ----

    [Fact]
    public async Task S5_WithoutTheConfirmation_IsRefused_AndWritesNothing_KK7()
    {
        (Order order, _) = await SeedAsync(OrderStatus.Preparing, PaymentStatus.Paid, Physical(1));

        OrderTransitionResult result = await RunAsync((unit, token) => unit.ShipOrderAsync(new ShipOrderRequest(order.Id, AdminCaller, 1, false, Listed), token));

        Assert.Equal(TransitionRefusal.ConfirmationRequired, result.Refusal);
        await AssertNothingWrittenAsync(order.Id, OrderStatus.Preparing, PaymentStatus.Paid);
    }

    [Theory]
    [MemberData(nameof(InvalidCarrierData))]
    public async Task S5_WithInvalidCarrierData_IsRefused_AndWritesNothing_KK7(ShipmentInfo info)
    {
        (Order order, _) = await SeedAsync(OrderStatus.Preparing, PaymentStatus.Paid, Physical(1));

        OrderTransitionResult result = await RunAsync((unit, token) => unit.ShipOrderAsync(new ShipOrderRequest(order.Id, AdminCaller, 1, true, info), token));

        Assert.Equal(TransitionRefusal.InvalidInput, result.Refusal);
        await AssertNothingWrittenAsync(order.Id, OrderStatus.Preparing, PaymentStatus.Paid);
    }

    public static TheoryData<ShipmentInfo> InvalidCarrierData() => new()
    {
        new ShipmentInfo(null, null, null, false),
        new ShipmentInfo("YurticiKargo", null, null, false),
        new ShipmentInfo("YurticiKargo", "Başka", "TR-1", false),
        new ShipmentInfo("NotInTheList", null, "TR-1", false),
        new ShipmentInfo(null, "Başka", "  ", false),
        new ShipmentInfo(null, null, "TR-1", true),
        new ShipmentInfo("YurticiKargo", null, null, true),
    };

    [Fact]
    public async Task S5_WithAnAdministratorWhoseDataIsMissing_OrAnOlderScreenVersion_IsRefused_KK7()
    {
        (Order order, _) = await SeedAsync(OrderStatus.Preparing, PaymentStatus.Paid, Physical(1));

        OrderTransitionResult noName = await RunAsync((unit, token) => unit.ShipOrderAsync(new ShipOrderRequest(order.Id, new TransitionAdmin(_admin!.Id, " "), 1, true, Listed), token));
        OrderTransitionResult older = await RunAsync((unit, token) => unit.ShipOrderAsync(new ShipOrderRequest(order.Id, AdminCaller, 0, true, Listed), token));

        Assert.Equal(TransitionRefusal.ActorNotAllowed, noName.Refusal);
        Assert.Equal(TransitionRefusal.VersionConflict, older.Refusal);
        await AssertNothingWrittenAsync(order.Id, OrderStatus.Preparing, PaymentStatus.Paid);
    }

    [Fact]
    public async Task S5_SendsOnlyTheOpenPhysicalItems_WritesTheCarrierData_AndOneB5Row_KK7()
    {
        (Order order, List<OrderItem> items) = await SeedAsync(OrderStatus.Preparing, PaymentStatus.Paid, Physical(1), Physical(2), new ItemSpec(ProductType.Digital, 1));
        await SaveAsync(c => c.Cancellations.Add(Cancellation.ByBuyer(items[1].Id, 2, Amounts, null, Start)));

        OrderTransitionResult result = await RunAsync((unit, token) => unit.ShipOrderAsync(
            new ShipOrderRequest(order.Id, AdminCaller, 1, true, new ShipmentInfo(" YurticiKargo ", null, " TR-1001 ", false)), token));

        Assert.False(result.IsRejected);
        Assert.Equal([items[0].Id], result.ItemIds);
        await using AppDbContext reader = NewContext();
        Order saved = await reader.Orders.SingleAsync(o => o.Id == order.Id, Ct);
        Assert.Equal((OrderStatus.Shipped, 2, "YurticiKargo", "TR-1001", false), (saved.OrderStatus, saved.Version, saved.Carrier, saved.TrackingNumber, saved.IsOwnVehicleDelivery));
        TransitionRecord record = await reader.TransitionRecords.SingleAsync(t => t.OrderId == order.Id, Ct);
        Assert.Equal((TransitionCode.S5, ActorKind.Admin, _admin!.Id), (record.TransitionCode, record.ActorKind, record.AdminId));
        DispatchRecord row = Assert.Single(await reader.DispatchRecords.Where(d => d.OrderId == order.Id).ToListAsync(Ct));
        Assert.Equal(DispatchType.B5, row.DispatchType);
    }

    [Fact]
    public async Task S5_WithTheOwnVehicleDeclaration_WritesNoCompanyAndNoNumber_KK7()
    {
        (Order order, _) = await SeedAsync(OrderStatus.Preparing, PaymentStatus.Paid, Physical(1));

        OrderTransitionResult result = await RunAsync((unit, token) => unit.ShipOrderAsync(
            new ShipOrderRequest(order.Id, AdminCaller, 1, true, new ShipmentInfo(null, null, null, true)), token));

        Assert.False(result.IsRejected);
        await using AppDbContext reader = NewContext();
        Order saved = await reader.Orders.SingleAsync(o => o.Id == order.Id, Ct);
        Assert.Equal((null, null, null, true), (saved.Carrier, saved.OtherCarrierName, saved.TrackingNumber, saved.IsOwnVehicleDelivery));
    }

    [Fact]
    public async Task S5_WithAnUnlistedCompany_KeepsItsNameAndNumber_KK7()
    {
        (Order order, _) = await SeedAsync(OrderStatus.Preparing, PaymentStatus.Paid, Physical(1));

        OrderTransitionResult result = await RunAsync((unit, token) => unit.ShipOrderAsync(
            new ShipOrderRequest(order.Id, AdminCaller, 1, true, new ShipmentInfo(null, "Yerel Kurye", "K-9", false)), token));

        Assert.False(result.IsRejected);
        await using AppDbContext reader = NewContext();
        Order saved = await reader.Orders.SingleAsync(o => o.Id == order.Id, Ct);
        Assert.Equal((null, "Yerel Kurye", "K-9"), (saved.Carrier, saved.OtherCarrierName, saved.TrackingNumber));
    }

    [Fact]
    public async Task S5_WhenNoPhysicalItemIsOpen_IsRefused_AndWritesNothing_KK7()
    {
        (Order order, List<OrderItem> items) = await SeedAsync(OrderStatus.Preparing, PaymentStatus.Paid, Physical(1), new ItemSpec(ProductType.Digital, 1, true));
        await SaveAsync(c => c.Cancellations.Add(Cancellation.ByBuyer(items[0].Id, 1, Amounts, null, Start)));

        OrderTransitionResult result = await RunAsync((unit, token) => unit.ShipOrderAsync(new ShipOrderRequest(order.Id, AdminCaller, 1, true, Listed), token));

        Assert.Equal(TransitionRefusal.ConditionNotMet, result.Refusal);
        await AssertNothingWrittenAsync(order.Id, OrderStatus.Preparing, PaymentStatus.Paid);
    }

    [Fact]
    public async Task ByAnotherKindOfActor_TheTablesShipmentRows_AreRefused_KK7()
    {
        (Order preparing, _) = await SeedAsync(OrderStatus.Preparing, PaymentStatus.Paid, Physical(1));
        (Order returned, _) = await SeedAsync(OrderStatus.DeliveryFailed, PaymentStatus.Paid, Physical(1));

        TransitionOutcome s5 = await RunCoreAsync((core, token) => core.ApplyAsync(new TransitionRequest(preparing.Id, TransitionCode.S5, TransitionActor.Buyer), null, token));
        TransitionOutcome s8 = await RunCoreAsync((core, token) => core.ApplyAsync(new TransitionRequest(returned.Id, TransitionCode.S8, TransitionActor.System), null, token));

        Assert.Equal(TransitionRejection.ActorNotAllowed, s5.Rejection);
        Assert.Equal(TransitionRejection.ActorNotAllowed, s8.Rejection);
        await AssertNothingWrittenAsync(preparing.Id, OrderStatus.Preparing, PaymentStatus.Paid);
        await AssertNothingWrittenAsync(returned.Id, OrderStatus.DeliveryFailed, PaymentStatus.Paid);
    }

    [Fact]
    public async Task S8_SendsTheOpenPhysicalItemsAgain_WithNewCarrierData_AndAnotherB5Row_KK7()
    {
        (Order order, List<OrderItem> items) = await SeedAsync(OrderStatus.Preparing, PaymentStatus.Paid, Physical(1), Physical(1));
        await RunAsync((unit, token) => unit.ShipOrderAsync(new ShipOrderRequest(order.Id, AdminCaller, 1, true, Listed), token));
        await RunAsync((unit, token) => unit.MarkDeliveryFailedAsync(new AdminOrderRequest(order.Id, AdminCaller, 2), token));
        await SaveAsync(c => c.Cancellations.Add(Cancellation.ByBuyer(items[1].Id, 1, Amounts, null, Start)));

        OrderTransitionResult result = await RunAsync((unit, token) => unit.ResendOrderAsync(
            new ShipOrderRequest(order.Id, AdminCaller, 3, true, new ShipmentInfo("ArasKargo", null, "AR-77", false)), token));

        Assert.False(result.IsRejected);
        Assert.Equal([items[0].Id], result.ItemIds);
        await using AppDbContext reader = NewContext();
        Order saved = await reader.Orders.SingleAsync(o => o.Id == order.Id, Ct);
        Assert.Equal((OrderStatus.Shipped, "ArasKargo", "AR-77", 4), (saved.OrderStatus, saved.Carrier, saved.TrackingNumber, saved.Version));
        Assert.Equivalent(
            new[] { TransitionCode.S5, TransitionCode.S7, TransitionCode.S8 },
            await reader.TransitionRecords.Where(t => t.OrderId == order.Id).Select(t => t.TransitionCode).ToListAsync(Ct));
        Assert.Equivalent(
            new[] { DispatchType.B5, DispatchType.B5, DispatchType.B6 },
            await reader.DispatchRecords.Where(d => d.OrderId == order.Id).Select(d => d.DispatchType).ToListAsync(Ct));
    }

    [Fact]
    public async Task S8_WhenNoPhysicalItemIsOpen_IsRefused_KK7()
    {
        (Order order, List<OrderItem> items) = await SeedAsync(OrderStatus.DeliveryFailed, PaymentStatus.Paid, Physical(1), new ItemSpec(ProductType.Digital, 1));
        await SaveAsync(c => c.Cancellations.Add(Cancellation.ByBuyer(items[0].Id, 1, Amounts, null, Start)));

        OrderTransitionResult result = await RunAsync((unit, token) => unit.ResendOrderAsync(new ShipOrderRequest(order.Id, AdminCaller, 1, true, Listed), token));

        Assert.Equal(TransitionRefusal.ConditionNotMet, result.Refusal);
        await AssertNothingWrittenAsync(order.Id, OrderStatus.DeliveryFailed, PaymentStatus.Paid);
    }

    [Fact]
    public async Task S8_WithoutTheConfirmation_IsRefused_KK7()
    {
        (Order order, _) = await SeedAsync(OrderStatus.DeliveryFailed, PaymentStatus.Paid, Physical(1));

        OrderTransitionResult result = await RunAsync((unit, token) => unit.ResendOrderAsync(new ShipOrderRequest(order.Id, AdminCaller, 1, false, Listed), token));

        Assert.Equal(TransitionRefusal.ConfirmationRequired, result.Refusal);
        await AssertNothingWrittenAsync(order.Id, OrderStatus.DeliveryFailed, PaymentStatus.Paid);
    }

    // ---- T350 KK7, KK8, KK9: the promise counts from the payment approval; the shipped mark shows the company and number ----

    [Fact]
    public async Task ThePromise_RunsFromTheLastUncorrectedPaymentApproval_AndNeverFromTheOrdersBirth_T350KK7()
    {
        (Order order, _) = await SeedAsync(OrderStatus.Placed, PaymentStatus.Paid, Physical(1));
        await ExecuteAsync($"UPDATE orders SET shipping_period = 3 WHERE id = '{order.Id}'");
        DateTimeOffset first = Start.AddDays(1);
        DateTimeOffset second = Start.AddDays(3);
        TransitionRecord firstApproval = TransitionRecord.Create(order.Id, TransitionCode.O1, ActorKind.System, null, null, null, null, null, null, null, first);
        await SaveAsync(c => c.TransitionRecords.Add(firstApproval));

        OrderShippingView? viaFirst = await ReadViewAsync(order.Id);
        await SaveAsync(c =>
        {
            c.TransitionRecords.Add(TransitionRecord.Create(order.Id, TransitionCode.Correction, ActorKind.Admin, _admin!.Id, _admin.Name, null, null, CorrectionReason.Other, null, firstApproval.Id, first.AddHours(1)));
            c.TransitionRecords.Add(TransitionRecord.Create(order.Id, TransitionCode.O1, ActorKind.System, null, null, null, null, null, null, null, second));
        });
        OrderShippingView? viaSecond = await ReadViewAsync(order.Id);

        Assert.Equal(first, viaFirst!.PaymentConfirmedAt);
        Assert.Equal(PeriodCounting.EndOf(CountedPeriod.ShippingPeriod, first, 3, BusinessCalendar.Product), viaFirst.PromiseEndsAt);
        Assert.Equal(second, viaSecond!.PaymentConfirmedAt);
        Assert.Equal(PeriodCounting.EndOf(CountedPeriod.ShippingPeriod, second, 3, BusinessCalendar.Product), viaSecond.PromiseEndsAt);
        Assert.Equal(3, viaSecond.ShippingPeriod);
        Assert.Equal("3 iş günü içinde kargoya verilir", viaSecond.PromiseText);
    }

    [Fact]
    public async Task AnOrderWithNoPromise_AndAnUnpaidOne_ShowNoPromiseEnd_T350KK7()
    {
        (Order digital, _) = await SeedAsync(OrderStatus.Placed, PaymentStatus.Paid, new ItemSpec(ProductType.Digital, 1));
        (Order unpaid, _) = await SeedAsync(OrderStatus.Placed, PaymentStatus.Pending, Physical(1));
        await ExecuteAsync($"UPDATE orders SET shipping_period = 2 WHERE id = '{unpaid.Id}'");
        await SaveAsync(c => c.TransitionRecords.Add(TransitionRecord.Create(digital.Id, TransitionCode.O1, ActorKind.System, null, null, null, null, null, null, null, Start)));

        OrderShippingView? digitalView = await ReadViewAsync(digital.Id);
        OrderShippingView? unpaidView = await ReadViewAsync(unpaid.Id);

        Assert.Null(digitalView!.ShippingPeriod);
        Assert.Null(digitalView.PromiseText);
        Assert.Null(digitalView.PromiseEndsAt);
        Assert.Equal(2, unpaidView!.ShippingPeriod);
        Assert.Null(unpaidView.PaymentConfirmedAt);
        Assert.Null(unpaidView.PromiseEndsAt);
        Assert.Null(await ReadViewAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task TheShippedMark_ShowsAListedCompanyWithItsTrackingLink_AnUnlistedOneWithoutIt_AndTheOwnVehicleAlone_T350KK8KK9()
    {
        (Order listed, _) = await SeedAsync(OrderStatus.Preparing, PaymentStatus.Paid, Physical(1));
        (Order other, _) = await SeedAsync(OrderStatus.Preparing, PaymentStatus.Paid, Physical(1));
        (Order own, _) = await SeedAsync(OrderStatus.Preparing, PaymentStatus.Paid, Physical(1));
        OrderShippingView? before = await ReadViewAsync(listed.Id);

        await RunAsync((unit, token) => unit.ShipOrderAsync(new ShipOrderRequest(listed.Id, AdminCaller, 1, true, new ShipmentInfo("YurticiKargo", null, "TR 1001", false)), token));
        await RunAsync((unit, token) => unit.ShipOrderAsync(new ShipOrderRequest(other.Id, AdminCaller, 1, true, new ShipmentInfo(null, "Yerel Kurye", "K-9", false)), token));
        await RunAsync((unit, token) => unit.ShipOrderAsync(new ShipOrderRequest(own.Id, AdminCaller, 1, true, new ShipmentInfo(null, null, null, true)), token));
        OrderShippingView? listedView = await ReadViewAsync(listed.Id);
        OrderShippingView? otherView = await ReadViewAsync(other.Id);
        OrderShippingView? ownView = await ReadViewAsync(own.Id);

        Assert.Null(before!.ShippedAt);
        Assert.Null(before.Display);
        Assert.Equal(Start, listedView!.ShippedAt);
        Assert.Equal("Yurtiçi Kargo", listedView.Display!.CarrierName);
        Assert.Equal("TR 1001", listedView.Display.TrackingNumber);
        Assert.Equal(CarrierCatalog.Default.BuildLink("YurticiKargo", "TR 1001"), listedView.Display.Link);
        Assert.NotNull(listedView.Display.Link);
        Assert.Equal("Yerel Kurye", otherView!.Display!.CarrierName);
        Assert.Equal("K-9", otherView.Display.TrackingNumber);
        Assert.Null(otherView.Display.Link);
        Assert.True(ownView!.Display!.IsOwnVehicleDeclaration);
        Assert.Null(ownView.Display.CarrierName);
        Assert.Null(ownView.Display.TrackingNumber);
        Assert.Null(ownView.Display.Link);
    }

    [Fact]
    public void TheShippingPeriodOfAnOrder_IsWrittenOnce_AndNeverNegative_T350KK6()
    {
        Order order = Order.Create("2026-000001", "access-placeholder-x", "confirm-placeholder-x", PaymentMethod.Card, true, "ali@example.com", 120m, 0m, Start.AddHours(1), Start);

        Assert.Throws<ArgumentOutOfRangeException>(() => order.FreezeShippingPromise(-1));
        order.FreezeShippingPromise(4);
        Assert.Equal(4, order.ShippingPeriod);
        Assert.Throws<InvalidOperationException>(() => order.FreezeShippingPromise(5));
        Assert.Equal(4, order.ShippingPeriod);
    }

    private async Task<OrderShippingView?> ReadViewAsync(Guid orderId)
    {
        await using ServiceProvider provider = BuildProvider();
        await using AsyncServiceScope scope = provider.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<IOrderingModule>().GetShippingViewAsync(orderId, Ct);
    }

    // ---- KK8: S6 ----

    [Fact]
    public async Task S6_MarksTheOpenPhysicalItemsWithTheDate_WritesNoNotice_AndNothingFollowsByItself_KK8()
    {
        (Order order, List<OrderItem> items) = await SeedAsync(OrderStatus.Preparing, PaymentStatus.Paid, Physical(1), Physical(1), new ItemSpec(ProductType.Service, 1));
        await RunAsync((unit, token) => unit.ShipOrderAsync(new ShipOrderRequest(order.Id, AdminCaller, 1, true, Listed), token));
        await SaveAsync(c => c.Cancellations.Add(Cancellation.ByBuyer(items[1].Id, 1, Amounts, null, Start)));
        int dispatchBefore = await CountDispatchAsync(order.Id);
        _clock.Set(Start.AddDays(2));
        DateOnly day = DateOnly.FromDateTime(Start.UtcDateTime).AddDays(1);

        OrderTransitionResult result = await RunAsync((unit, token) => unit.MarkDeliveredAsync(new MarkDeliveredRequest(order.Id, AdminCaller, 2, day), token));

        Assert.False(result.IsRejected);
        Assert.Equal([items[0].Id], result.ItemIds);
        await using AppDbContext reader = NewContext();
        List<OrderItem> saved = await reader.OrderItems.Where(i => i.OrderId == order.Id).OrderBy(i => i.Id).ToListAsync(Ct);
        OrderItem marked = saved.Single(i => i.Id == items[0].Id);
        Assert.Equal((Start.AddDays(2), (DateOnly?)day), (marked.DeliveryMarkedAt, marked.DeliveryDate));
        Assert.All(saved.Where(i => i.Id != items[0].Id), i => Assert.Null(i.DeliveryMarkedAt));
        Assert.Equal(OrderStatus.Delivered, (await reader.Orders.SingleAsync(o => o.Id == order.Id, Ct)).OrderStatus);
        Assert.Equal(dispatchBefore, await CountDispatchAsync(order.Id));
        Assert.Equivalent(
            new[] { TransitionCode.S5, TransitionCode.S6 },
            await reader.TransitionRecords.Where(t => t.OrderId == order.Id).Select(t => t.TransitionCode).ToListAsync(Ct));
        Assert.Null(result.ClosureCode);
    }

    [Fact]
    public async Task S6_AfterwardsNoClosureOrOperationMovesTheDeliveredOrder_KK8()
    {
        (Order order, _) = await SeedAsync(OrderStatus.Shipped, PaymentStatus.Paid, Physical(1));
        await RunAsync((unit, token) => unit.MarkDeliveredAsync(new MarkDeliveredRequest(order.Id, AdminCaller, 1, DateOnly.FromDateTime(Start.UtcDateTime)), token));
        ClosureResult closure = ClosureResult.None;

        await RunCoreAsync(async (core, token) =>
        {
            await ScopeLocks!.LockAsync([order.Id], token);
            Order locked = await ScopeDb!.Orders.SingleAsync(o => o.Id == order.Id, token);
            closure = await core.EvaluateClosureAsync(locked, TransitionActor.System, token);
            return new TransitionOutcome(null, [], closure);
        });

        Assert.False(closure.Closed);
        await using AppDbContext reader = NewContext();
        Assert.Equal(1, await reader.TransitionRecords.CountAsync(t => t.OrderId == order.Id, Ct));
    }

    [Fact]
    public async Task S6_WithADateInTheFuture_OrBeforeTheDayOfTheShipment_IsRefused_AndTheDayOfTheShipmentIsAccepted_KK8()
    {
        (Order order, _) = await SeedAsync(OrderStatus.Preparing, PaymentStatus.Paid, Physical(1));
        await RunAsync((unit, token) => unit.ShipOrderAsync(new ShipOrderRequest(order.Id, AdminCaller, 1, true, Listed), token));
        DateOnly shippingDay = DateOnly.FromDateTime(Start.UtcDateTime);

        OrderTransitionResult future = await RunAsync((unit, token) => unit.MarkDeliveredAsync(new MarkDeliveredRequest(order.Id, AdminCaller, 2, shippingDay.AddDays(1)), token));
        OrderTransitionResult before = await RunAsync((unit, token) => unit.MarkDeliveredAsync(new MarkDeliveredRequest(order.Id, AdminCaller, 2, shippingDay.AddDays(-1)), token));
        OrderTransitionResult sameDay = await RunAsync((unit, token) => unit.MarkDeliveredAsync(new MarkDeliveredRequest(order.Id, AdminCaller, 2, shippingDay), token));

        Assert.Equal(TransitionRefusal.InvalidInput, future.Refusal);
        Assert.Equal(TransitionRefusal.InvalidInput, before.Refusal);
        Assert.False(sameDay.IsRejected);
    }

    [Fact]
    public async Task S6_WhenNoPhysicalItemIsOpen_IsRefused_AndAnOlderScreenVersionToo_KK8()
    {
        (Order closed, List<OrderItem> items) = await SeedAsync(OrderStatus.Shipped, PaymentStatus.Paid, Physical(1), new ItemSpec(ProductType.Digital, 1));
        await SaveAsync(c => c.Cancellations.Add(Cancellation.ByBuyer(items[0].Id, 1, Amounts, null, Start)));
        (Order open, _) = await SeedAsync(OrderStatus.Shipped, PaymentStatus.Paid, Physical(1));
        DateOnly today = DateOnly.FromDateTime(Start.UtcDateTime);

        OrderTransitionResult noOpen = await RunAsync((unit, token) => unit.MarkDeliveredAsync(new MarkDeliveredRequest(closed.Id, AdminCaller, 1, today), token));
        OrderTransitionResult older = await RunAsync((unit, token) => unit.MarkDeliveredAsync(new MarkDeliveredRequest(open.Id, AdminCaller, 5, today), token));

        Assert.Equal(TransitionRefusal.ConditionNotMet, noOpen.Refusal);
        Assert.Equal(TransitionRefusal.VersionConflict, older.Refusal);
        await AssertNothingWrittenAsync(closed.Id, OrderStatus.Shipped, PaymentStatus.Paid);
        await AssertNothingWrittenAsync(open.Id, OrderStatus.Shipped, PaymentStatus.Paid);
    }

    // ---- KK9: S7 ----

    [Fact]
    public async Task S7_MarksTheReturnedShipment_AndWritesOneB6Row_KK9()
    {
        (Order order, _) = await SeedAsync(OrderStatus.Shipped, PaymentStatus.Paid, Physical(1));

        OrderTransitionResult result = await RunAsync((unit, token) => unit.MarkDeliveryFailedAsync(new AdminOrderRequest(order.Id, AdminCaller, 1), token));

        Assert.False(result.IsRejected);
        Assert.Null(result.ClosureCode);
        await using AppDbContext reader = NewContext();
        Order saved = await reader.Orders.SingleAsync(o => o.Id == order.Id, Ct);
        Assert.Equal((OrderStatus.DeliveryFailed, 2), (saved.OrderStatus, saved.Version));
        TransitionRecord record = await reader.TransitionRecords.SingleAsync(t => t.OrderId == order.Id, Ct);
        Assert.Equal((TransitionCode.S7, ActorKind.Admin), (record.TransitionCode, record.ActorKind));
        DispatchRecord row = Assert.Single(await reader.DispatchRecords.Where(d => d.OrderId == order.Id).ToListAsync(Ct));
        Assert.Equal((DispatchType.B6, "Ali@Example.com"), (row.DispatchType, row.RecipientEmail));
    }

    [Fact]
    public async Task S7_WithAnOlderScreenVersion_IsRefused_AndWritesNeitherARecordNorARow_KK9()
    {
        (Order order, _) = await SeedAsync(OrderStatus.Shipped, PaymentStatus.Paid, Physical(1));

        OrderTransitionResult result = await RunAsync((unit, token) => unit.MarkDeliveryFailedAsync(new AdminOrderRequest(order.Id, AdminCaller, 9), token));

        Assert.Equal(TransitionRefusal.VersionConflict, result.Refusal);
        await AssertNothingWrittenAsync(order.Id, OrderStatus.Shipped, PaymentStatus.Paid);
    }

    [Fact]
    public async Task AnOperationOfAnOrderThatDoesNotExist_IsRefusedAsNotFound()
    {
        OrderTransitionResult result = await RunAsync((unit, token) => unit.MarkDeliveryFailedAsync(new AdminOrderRequest(EntityIds.New(), AdminCaller, 1), token));

        Assert.Equal(TransitionRefusal.OrderNotFound, result.Refusal);
    }

    // ---- helpers ----

    public enum Op
    {
        CancelByCustomer,
        CancelByAdmin,
        CancelBySystem,
        CloseReturned,
        Ship,
        MarkDelivered,
        MarkDeliveryFailed,
        Resend,
    }

    private AppDbContext? ScopeDb { get; set; }

    private OrderLocks? ScopeLocks { get; set; }

    private static ItemSpec Physical(int quantity) => new(ProductType.Physical, quantity);

    private Task<OrderTransitionResult> CallAsync(IOrderTransitions unit, Op op, Guid orderId, int version, CancellationToken token)
    {
        return op switch
        {
            Op.CancelByCustomer => unit.CancelOrderAsync(CancelOrderRequest.ByCustomer(orderId), token),
            Op.CancelByAdmin => unit.CancelOrderAsync(CancelOrderRequest.ByAdministrator(orderId, AdminCaller, version, true, CancellationReason.OutOfStock), token),
            Op.CancelBySystem => unit.CancelOrderAsync(CancelOrderRequest.BySystem(orderId, TransitionReason.PaymentWindowExpired), token),
            Op.CloseReturned => unit.CloseReturnedOrderAsync(new AdminOrderRequest(orderId, AdminCaller, version), token),
            Op.Ship => unit.ShipOrderAsync(new ShipOrderRequest(orderId, AdminCaller, version, true, Listed), token),
            Op.MarkDelivered => unit.MarkDeliveredAsync(new MarkDeliveredRequest(orderId, AdminCaller, version, DateOnly.FromDateTime(Start.UtcDateTime)), token),
            Op.MarkDeliveryFailed => unit.MarkDeliveryFailedAsync(new AdminOrderRequest(orderId, AdminCaller, version), token),
            Op.Resend => unit.ResendOrderAsync(new ShipOrderRequest(orderId, AdminCaller, version, true, Listed), token),
            _ => throw new ArgumentOutOfRangeException(nameof(op)),
        };
    }

    private async Task<T> RunAsync<T>(Func<IOrderTransitions, CancellationToken, Task<T>> work)
        where T : IOperationResult
    {
        await using ServiceProvider provider = BuildProvider();
        await using AsyncServiceScope scope = provider.CreateAsyncScope();
        IOrderTransitions unit = scope.ServiceProvider.GetRequiredService<IOrderTransitions>();
        return await scope.ServiceProvider.GetRequiredService<ITransactionRunner>().RunAsync(token => work(unit, token), Ct);
    }

    private async Task<T> RunCoreAsync<T>(Func<OrderTransitions, CancellationToken, Task<T>> work)
        where T : IOperationResult
    {
        await using ServiceProvider provider = BuildProvider();
        await using AsyncServiceScope scope = provider.CreateAsyncScope();
        OrderTransitions core = scope.ServiceProvider.GetRequiredService<OrderTransitions>();
        ScopeDb = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        ScopeLocks = scope.ServiceProvider.GetRequiredService<OrderLocks>();
        return await scope.ServiceProvider.GetRequiredService<ITransactionRunner>().RunAsync(token => work(core, token), Ct);
    }

    private async Task AssertNothingWrittenAsync(Guid orderId, OrderStatus status, PaymentStatus payment)
    {
        await using AppDbContext reader = NewContext();
        Order saved = await reader.Orders.SingleAsync(o => o.Id == orderId, Ct);
        Assert.Equal((status, payment, 1), (saved.OrderStatus, saved.PaymentStatus, saved.Version));
        Assert.Empty(await reader.TransitionRecords.Where(t => t.OrderId == orderId).ToListAsync(Ct));
        Assert.Empty(await reader.DispatchRecords.Where(d => d.OrderId == orderId).ToListAsync(Ct));
        Assert.Null(saved.Carrier);
        Assert.Null(saved.TrackingNumber);
    }

    private async Task<int> CountDispatchAsync(Guid orderId)
    {
        await using AppDbContext reader = NewContext();
        return await reader.DispatchRecords.CountAsync(d => d.OrderId == orderId, Ct);
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
                item.DeliveryDate = DateOnly.FromDateTime(Start.UtcDateTime);
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

        // The states are set around the entity (it takes them only through the unit), the pair rule of the database still applies.
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
