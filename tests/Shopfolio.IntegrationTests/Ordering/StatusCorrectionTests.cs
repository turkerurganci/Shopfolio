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
/// The correction of a wrong transition on a real PostgreSQL and a fake clock (T319 criteria 3-9, KK3-KK9; 03 1.6.2-1.6.2.4; 02 10.4.6; 05 4.1.4): the
/// administrator's correction through <see cref="IOrderTransitions.CorrectTransitionAsync"/> - the wrong row and the correction stay side by side, the reason
/// is from the closed list and B-13 goes; the delivery mark's correction empties the marks, the firm's cancellation is corrected only while the payment is
/// Paid and no refund is under way and the units can be taken again, the shipment marks go back while what was recorded meanwhile stays, the transfer mark is
/// corrected only with none of its four obstacles and starts Z-8 again with a second B-3, and the card's payment and a withdrawal have no correction. Every
/// operation runs in the transaction runner like a page would call it; what it wrote is read back from the tables.
/// </summary>
[Collection(OrderTransitionsDefinition.Name)]
public sealed class StatusCorrectionTests : IAsyncLifetime
{
    private static readonly DateTimeOffset Start = new(2026, 11, 9, 9, 0, 0, TimeSpan.Zero);

    private static readonly DateOnly Today = DateOnly.FromDateTime(Start.UtcDateTime);

    private static readonly ItemAmounts Amounts = new(100m, 0m, 16.67m);

    private static readonly ShipmentInfo Listed = new("YurticiKargo", null, "TR-1001", false);

    private readonly CollectionDatabaseFixture _collection;

    private readonly ManualTimeProvider _clock = new(Start);

    private ServiceProvider _provider = null!;

    private Admin? _admin;

    private int _counter;

    public StatusCorrectionTests(CollectionDatabaseFixture collection)
    {
        _collection = collection;
    }

    /// <summary>The obstacles of the transfer mark's correction (03 1.6.2.4): the delivery mark, the shipment, the refund and the four item records.</summary>
    public enum TransferObstacle
    {
        DeliveryMark,
        Shipment,
        Refund,
        Cancellation,
        Removal,
        Termination,
        Withdrawal,
    }

    /// <summary>The state of the cancellation's refund when its correction is asked (K-940).</summary>
    public enum RefundState
    {
        TransferSent,
        CardSending,
        CardOutcomePending,
        CardSucceeded,
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private TransitionAdmin AdminCaller => new(_admin!.Id, _admin.Name);

    public async ValueTask InitializeAsync()
    {
        await ExecuteAsync(
            "TRUNCATE dispatch_records, jobs, refund_records, refund_iban_requests, refunds, payment_records, returns, return_rejections, defect_claims, withdrawals, "
            + "late_delivery_terminations, item_removals, cancellations, internal_notes, transition_records, stock_reservations, order_items, orders, coupons, "
            + "variants, products, admins, legal_text_versions, provinces CASCADE");
        _admin = Admin.Create("Firma Yöneticisi", "yonetici@example.com", "digest-placeholder", Start);
        await SaveAsync(c => c.Admins.Add(_admin));
        _clock.SetUtcNow(Start);
        _provider = BuildProvider();
    }

    public async ValueTask DisposeAsync() => await _provider.DisposeAsync();

    // ---- KK3, KK4: the reason, the two rows and B-13 ----

    [Theory]
    [InlineData(null, null)]
    [InlineData(CorrectionReason.Other, null)]
    [InlineData(CorrectionReason.Other, "   ")]
    [InlineData(CorrectionReason.WrongOrder, "not")]
    [InlineData((CorrectionReason)99, null)]
    public async Task ACorrection_WithoutAReasonFromTheClosedList_OrWithAMisplacedNote_IsRefused_AndWritesNothing_KK4(CorrectionReason? reason, string? note)
    {
        (Order order, _) = await SeedAsync(OrderStatus.Shipped, PaymentStatus.Paid, PaymentMethod.Card, new Line(ProductType.Physical, 1));
        Guid delivered = (await RunAsync((unit, token) => unit.MarkDeliveredAsync(new MarkDeliveredRequest(order.Id, AdminCaller, 1, Today), token))).TransitionRecordIds.Single();

        OrderTransitionResult result = await CorrectAsync(order.Id, delivered, 2, reason, note);

        Assert.Equal(TransitionRefusal.InvalidInput, result.Refusal);
        await AssertNotCorrectedAsync(order.Id, OrderStatus.Delivered, PaymentStatus.Paid, 2);
    }

    [Fact]
    public async Task ACorrection_ByAnAdministratorWhoseDataIsMissing_IsRefused_KK4()
    {
        (Order order, _) = await SeedAsync(OrderStatus.Shipped, PaymentStatus.Paid, PaymentMethod.Card, new Line(ProductType.Physical, 1));
        Guid delivered = (await RunAsync((unit, token) => unit.MarkDeliveredAsync(new MarkDeliveredRequest(order.Id, AdminCaller, 1, Today), token))).TransitionRecordIds.Single();

        OrderTransitionResult result = await RunAsync((unit, token) => unit.CorrectTransitionAsync(
            new CorrectTransitionRequest(order.Id, delivered, new TransitionAdmin(Guid.Empty, " "), 2, CorrectionReason.WrongOrder, null), token));

        Assert.Equal(TransitionRefusal.ActorNotAllowed, result.Refusal);
        await AssertNotCorrectedAsync(order.Id, OrderStatus.Delivered, PaymentStatus.Paid, 2);
    }

    [Fact]
    public async Task TheOtherReason_KeepsItsTrimmedNote_OnTheCorrectionRow_BesideTheWrongRow_AndB13Goes_KK3_KK4()
    {
        (Order order, _) = await SeedAsync(OrderStatus.Shipped, PaymentStatus.Paid, PaymentMethod.Card, new Line(ProductType.Physical, 1));
        Guid delivered = (await RunAsync((unit, token) => unit.MarkDeliveredAsync(new MarkDeliveredRequest(order.Id, AdminCaller, 1, Today), token))).TransitionRecordIds.Single();

        OrderTransitionResult result = await CorrectAsync(order.Id, delivered, 2, CorrectionReason.Other, "  Kargo firması yanlış bildirdi ");

        Assert.False(result.IsRejected);
        TransitionRecord correction = await AssertCorrectedAsync(order.Id, delivered, TransitionCode.S6, CorrectionReason.Other);
        Assert.Equal("Kargo firması yanlış bildirdi", correction.ReasonNote);
        Assert.Equal([correction.Id], result.TransitionRecordIds);
    }

    [Fact]
    public async Task ARowCorrectedOnce_IsNotCorrectedAgain_AndACorrectionRowHasNoCorrection_KK3()
    {
        (Order order, _) = await SeedAsync(OrderStatus.Shipped, PaymentStatus.Paid, PaymentMethod.Card, new Line(ProductType.Physical, 1));
        Guid delivered = (await RunAsync((unit, token) => unit.MarkDeliveredAsync(new MarkDeliveredRequest(order.Id, AdminCaller, 1, Today), token))).TransitionRecordIds.Single();
        Guid correction = (await CorrectAsync(order.Id, delivered, 2, CorrectionReason.MarkedBeforeOccurred, null)).TransitionRecordIds.Single();

        OrderTransitionResult again = await CorrectAsync(order.Id, delivered, 3, CorrectionReason.WrongOrder, null);
        OrderTransitionResult ofCorrection = await CorrectAsync(order.Id, correction, 3, CorrectionReason.WrongOrder, null);

        Assert.Equal((TransitionRefusal.CorrectionNotPossible, CorrectionObstacle.NotCorrectable), (again.Refusal, again.Obstacle));
        Assert.Equal((TransitionRefusal.CorrectionNotPossible, CorrectionObstacle.NotCorrectable), (ofCorrection.Refusal, ofCorrection.Obstacle));
        await AssertCorrectedAsync(order.Id, delivered, TransitionCode.S6, CorrectionReason.MarkedBeforeOccurred);
    }

    // ---- KK5: the delivery mark's correction ----

    [Fact]
    public async Task TheDeliveryMarksCorrection_TakesTheOrderBackToShipped_AndEmptiesThePhysicalItemsMarks_TheDigitalItemKeepsItsOwn_KK5()
    {
        (Order order, List<OrderItem> items) = await SeedAsync(
            OrderStatus.Shipped, PaymentStatus.Paid, PaymentMethod.Card, new Line(ProductType.Physical, 2), new Line(ProductType.Digital, 1, Delivered: true));
        Guid delivered = (await RunAsync((unit, token) => unit.MarkDeliveredAsync(new MarkDeliveredRequest(order.Id, AdminCaller, 1, Today), token))).TransitionRecordIds.Single();
        await using (AppDbContext before = NewContext())
        {
            Assert.NotNull((await before.OrderItems.SingleAsync(i => i.Id == items[0].Id, Ct)).DeliveryMarkedAt);
        }

        OrderTransitionResult result = await CorrectAsync(order.Id, delivered, 2, CorrectionReason.MarkedBeforeOccurred, null);

        // The withdrawal and the defect periods count from the delivery date: with the mark gone they wait for the new date (1.6.2.1).
        Assert.False(result.IsRejected);
        Assert.Equal([items[0].Id], result.ItemIds);
        await using AppDbContext reader = NewContext();
        Order saved = await reader.Orders.SingleAsync(o => o.Id == order.Id, Ct);
        Assert.Equal((OrderStatus.Shipped, PaymentStatus.Paid, 3), (saved.OrderStatus, saved.PaymentStatus, saved.Version));
        OrderItem physical = await reader.OrderItems.SingleAsync(i => i.Id == items[0].Id, Ct);
        Assert.Equal((null, null), (physical.DeliveryMarkedAt, physical.DeliveryDate));
        OrderItem digital = await reader.OrderItems.SingleAsync(i => i.Id == items[1].Id, Ct);
        Assert.Equal((Start, Today), (digital.DeliveryMarkedAt!.Value, digital.DeliveryDate!.Value));
        await AssertCorrectedAsync(order.Id, delivered, TransitionCode.S6, CorrectionReason.MarkedBeforeOccurred);
    }

    // ---- KK6: the firm's cancellation ----

    [Theory]
    [InlineData(PaymentMethod.BankTransfer, false)]
    [InlineData(PaymentMethod.Card, true)]
    public async Task TheFirmsCancellation_OfAPaidOrder_IsCorrected_TheUnitsAreTakenAgain_TheRefundVoided_TheIbanRequestClosed_KK3_KK6(PaymentMethod method, bool failedCardRequest)
    {
        Variant variant = await SeedVariantAsync(5);
        (Order order, List<OrderItem> items) = await SeedAsync(OrderStatus.Preparing, PaymentStatus.Paid, method, new Line(ProductType.Physical, 2, variant.Id));
        Guid s4 = await CancelByFirmAsync(order.Id);
        (Cancellation cancellation, Refund refund) = await SeedCancellationAsync(order, items[0], s4, method);
        RefundIbanRequest itemRequest;
        if (failedCardRequest)
        {
            // A failed card request alone does not stop the correction: the money did not leave (K-940), and the refund is voided with it; the IBAN
            // request the failure opened on the refund closes with it.
            RefundRecord failed = RefundRecord.Sending(refund.Id, Start);
            failed.State = RefundRecordState.Failed;
            failed.OutcomeAt = Start;
            itemRequest = RefundIbanRequest.ForItem(items[0].Id, RefundIbanRequestReason.CardRefundFailed, refund.Id, _admin!.Id, _admin.Name, Start);
            await SaveAsync(c => c.RefundRecords.Add(failed));
        }
        else
        {
            // The transfer line's cancellation asked for an IBAN on the item ("cancellation or removal"; 06 4.31).
            itemRequest = RefundIbanRequest.ForItem(items[0].Id, RefundIbanRequestReason.CancellationOrRemoval, null, _admin!.Id, _admin.Name, Start);
        }

        await SaveAsync(c => c.RefundIbanRequests.Add(itemRequest));
        Assert.Equal(2, (await ClosedQuantitiesAsync(items[0].Id)).GetValueOrDefault(items[0].Id));
        _clock.Advance(TimeSpan.FromHours(1));

        OrderTransitionResult result = await CorrectAsync(order.Id, s4, 2, CorrectionReason.WrongOrder, null);

        Assert.False(result.IsRejected);
        Assert.Equal([items[0].Id], result.ItemIds);
        await using AppDbContext reader = NewContext();
        Order saved = await reader.Orders.SingleAsync(o => o.Id == order.Id, Ct);
        Assert.Equal((OrderStatus.Preparing, PaymentStatus.Paid, 3), (saved.OrderStatus, saved.PaymentStatus, saved.Version));
        Assert.Equal((3, 0), await VariantAsync(variant.Id));
        Assert.Equal(_clock.GetUtcNow(), (await reader.Refunds.SingleAsync(r => r.Id == refund.Id, Ct)).VoidedAt);
        Assert.Equal(_clock.GetUtcNow(), (await reader.RefundIbanRequests.SingleAsync(r => r.Id == itemRequest.Id, Ct)).ClosedAt);

        // The cancellation record stays - the correction is a new transition - but it no longer closes the units (06 4.22).
        Assert.True(await reader.Cancellations.AnyAsync(c => c.Id == cancellation.Id, Ct));
        Assert.Equal(0, (await ClosedQuantitiesAsync(items[0].Id)).GetValueOrDefault(items[0].Id));
        await AssertCorrectedAsync(order.Id, s4, TransitionCode.S4, CorrectionReason.WrongOrder);
    }

    [Theory]
    [InlineData(RefundState.TransferSent)]
    [InlineData(RefundState.CardSending)]
    [InlineData(RefundState.CardOutcomePending)]
    [InlineData(RefundState.CardSucceeded)]
    public async Task TheFirmsCancellation_WhoseRefundIsUnderway_IsNotCorrected_RefundPending_KK6(RefundState state)
    {
        PaymentMethod method = state == RefundState.TransferSent ? PaymentMethod.BankTransfer : PaymentMethod.Card;
        Variant variant = await SeedVariantAsync(5);
        (Order order, List<OrderItem> items) = await SeedAsync(OrderStatus.Preparing, PaymentStatus.Paid, method, new Line(ProductType.Physical, 2, variant.Id));
        Guid s4 = await CancelByFirmAsync(order.Id);
        (_, Refund refund) = await SeedCancellationAsync(order, items[0], s4, method);
        if (state == RefundState.TransferSent)
        {
            await ExecuteAsync($"UPDATE refunds SET sent_at = '{Start:O}' WHERE id = '{refund.Id}'");
        }
        else
        {
            RefundRecord record = RefundRecord.Sending(refund.Id, Start);
            record.State = state switch
            {
                RefundState.CardOutcomePending => RefundRecordState.OutcomePending,
                RefundState.CardSucceeded => RefundRecordState.Succeeded,
                _ => RefundRecordState.Sending,
            };
            record.OutcomeAt = state == RefundState.CardSucceeded ? Start : null;
            await SaveAsync(c => c.RefundRecords.Add(record));
        }

        OrderTransitionResult result = await CorrectAsync(order.Id, s4, 2, CorrectionReason.WrongOrder, null);

        Assert.Equal((TransitionRefusal.CorrectionNotPossible, CorrectionObstacle.RefundPending), (result.Refusal, result.Obstacle));
        await AssertNotCorrectedAsync(order.Id, OrderStatus.Cancelled, PaymentStatus.Paid, 2);
        Assert.Equal((5, 0), await VariantAsync(variant.Id));
        await using AppDbContext reader = NewContext();
        Assert.Null((await reader.Refunds.SingleAsync(r => r.Id == refund.Id, Ct)).VoidedAt);
    }

    [Fact]
    public async Task TheFirmsCancellation_WhoseUnitsAreNoLongerLeft_IsNotCorrected_StockShort_NamingTheItem_KK6()
    {
        Variant shortVariant = await SeedVariantAsync(1);
        Variant enough = await SeedVariantAsync(5);
        (Order order, List<OrderItem> items) = await SeedAsync(
            OrderStatus.Preparing, PaymentStatus.Paid, PaymentMethod.Card, new Line(ProductType.Physical, 2, shortVariant.Id), new Line(ProductType.Physical, 1, enough.Id));
        Guid s4 = await CancelByFirmAsync(order.Id);
        await SeedCancellationAsync(order, items[0], s4, PaymentMethod.Card);
        await SeedCancellationAsync(order, items[1], s4, PaymentMethod.Card);

        OrderTransitionResult result = await CorrectAsync(order.Id, s4, 2, CorrectionReason.WrongOrder, null);

        Assert.Equal((TransitionRefusal.CorrectionNotPossible, CorrectionObstacle.StockShort), (result.Refusal, result.Obstacle));
        Assert.Equal([items[0].Id], result.ItemIds);
        await AssertNotCorrectedAsync(order.Id, OrderStatus.Cancelled, PaymentStatus.Paid, 2);
        Assert.Equal((1, 0), await VariantAsync(shortVariant.Id));
        Assert.Equal((5, 0), await VariantAsync(enough.Id));
    }

    [Theory]
    [InlineData(PaymentStatus.PartiallyRefunded)]
    [InlineData(PaymentStatus.Refunded)]
    public async Task TheFirmsCancellation_WhenThePaymentIsNoLongerPaid_IsNotCorrected_PaymentNotPaid_KK6(PaymentStatus payment)
    {
        (Order order, List<OrderItem> items) = await SeedAsync(OrderStatus.Preparing, PaymentStatus.Paid, PaymentMethod.Card, new Line(ProductType.Physical, 1));
        Guid s4 = await CancelByFirmAsync(order.Id);
        await SeedCancellationAsync(order, items[0], s4, PaymentMethod.Card);
        await ExecuteAsync($"UPDATE orders SET payment_status = '{payment}' WHERE id = '{order.Id}'");

        OrderTransitionResult result = await CorrectAsync(order.Id, s4, 2, CorrectionReason.WrongOrder, null);

        Assert.Equal((TransitionRefusal.CorrectionNotPossible, CorrectionObstacle.PaymentNotPaid), (result.Refusal, result.Obstacle));
        await AssertNotCorrectedAsync(order.Id, OrderStatus.Cancelled, payment, 2);
    }

    [Fact]
    public async Task TheClosuresCancellation_AndTheCustomersCancellation_AreNotCorrected_KK6()
    {
        (Order closed, List<OrderItem> closedItems) = await SeedAsync(OrderStatus.Preparing, PaymentStatus.Paid, PaymentMethod.Card, new Line(ProductType.Physical, 1));
        ClosureResult closure = await CloseWithAsync(closed.Id, db => db.Cancellations.Add(Cancellation.ByBuyer(closedItems[0].Id, 1, Amounts, null, Start)));
        Assert.Equal(TransitionCode.S4, closure.Code);
        (Order byCustomer, List<OrderItem> customerItems) = await SeedAsync(OrderStatus.Preparing, PaymentStatus.Paid, PaymentMethod.Card, new Line(ProductType.Physical, 1));
        await SaveAsync(c => c.Cancellations.Add(Cancellation.ByBuyer(customerItems[0].Id, 1, Amounts, null, Start)));
        Guid customerS4 = (await RunAsync((unit, token) => unit.CancelOrderAsync(
            CancelOrderRequest.ByCustomer(byCustomer.Id, new CancelledItem("Ürün", null, null, 1)), token))).TransitionRecordIds.Single();

        OrderTransitionResult ofClosure = await CorrectAsync(closed.Id, closure.TransitionRecordId!.Value, await VersionAsync(closed.Id), CorrectionReason.WrongOrder, null);
        OrderTransitionResult ofCustomer = await CorrectAsync(byCustomer.Id, customerS4, await VersionAsync(byCustomer.Id), CorrectionReason.WrongOrder, null);

        Assert.Equal((TransitionRefusal.CorrectionNotPossible, CorrectionObstacle.NotCorrectable), (ofClosure.Refusal, ofClosure.Obstacle));
        Assert.Equal((TransitionRefusal.CorrectionNotPossible, CorrectionObstacle.NotCorrectable), (ofCustomer.Refusal, ofCustomer.Obstacle));
        await AssertNotCorrectedAsync(closed.Id, OrderStatus.Cancelled, PaymentStatus.Paid, await VersionAsync(closed.Id));
        await AssertNotCorrectedAsync(byCustomer.Id, OrderStatus.Cancelled, PaymentStatus.Paid, await VersionAsync(byCustomer.Id));
    }

    // ---- KK7: the shipment marks ----

    [Fact]
    public async Task TheShipmentMarksCorrection_KeepsTheWithdrawalAndTheDefectClaim_AndClosesTheOrderWithNoOpenItem_InTheSameOperation_KK7()
    {
        (Order order, List<OrderItem> items) = await SeedAsync(OrderStatus.Preparing, PaymentStatus.Paid, PaymentMethod.Card, new Line(ProductType.Physical, 2));
        Guid s5 = (await RunAsync((unit, token) => unit.ShipOrderAsync(new ShipOrderRequest(order.Id, AdminCaller, 1, true, Listed), token))).TransitionRecordIds.Single();
        Withdrawal withdrawal = Withdrawal.ByBuyer(items[0].Id, 2, Today, null, Amounts, null, Start);
        DefectClaim claim = DefectClaim.Create(items[0].Id, 1, "Kutusu ezik geldi", null, Start);
        await SaveAsync(c =>
        {
            c.Withdrawals.Add(withdrawal);
            c.DefectClaims.Add(claim);
        });

        OrderTransitionResult result = await CorrectAsync(order.Id, s5, 2, CorrectionReason.WrongOrder, null);

        // Back on Preparing with no open item and no delivery mark, the closure rule cancels the order by closure (1.6.2.3; 1.4.3).
        Assert.False(result.IsRejected);
        Assert.Equal(TransitionCode.S4, result.ClosureCode);
        await using AppDbContext reader = NewContext();
        Order saved = await reader.Orders.SingleAsync(o => o.Id == order.Id, Ct);
        Assert.Equal((OrderStatus.Cancelled, PaymentStatus.Paid), (saved.OrderStatus, saved.PaymentStatus));
        TransitionRecord closure = await reader.TransitionRecords.SingleAsync(t => t.OrderId == order.Id && t.TransitionCode == TransitionCode.S4, Ct);
        Assert.Equal((ActorKind.System, TransitionReason.Closure), (closure.ActorKind, closure.TransitionReason!.Value));
        Assert.True(await reader.Withdrawals.AnyAsync(w => w.Id == withdrawal.Id, Ct));
        Assert.True(await reader.DefectClaims.AnyAsync(d => d.Id == claim.Id, Ct));
        await AssertCorrectedAsync(order.Id, s5, TransitionCode.S5, CorrectionReason.WrongOrder);
    }

    [Fact]
    public async Task TheShipmentMarksCorrection_OfAnOrderWithAnOpenItem_GoesBackToPreparing_AndKeepsTheCarrierData_KK7()
    {
        (Order order, _) = await SeedAsync(OrderStatus.Preparing, PaymentStatus.Paid, PaymentMethod.Card, new Line(ProductType.Physical, 1));
        Guid s5 = (await RunAsync((unit, token) => unit.ShipOrderAsync(new ShipOrderRequest(order.Id, AdminCaller, 1, true, Listed), token))).TransitionRecordIds.Single();

        OrderTransitionResult result = await CorrectAsync(order.Id, s5, 2, CorrectionReason.WrongOrder, null);

        Assert.False(result.IsRejected);
        Assert.Null(result.ClosureCode);
        await using AppDbContext reader = NewContext();
        Order saved = await reader.Orders.SingleAsync(o => o.Id == order.Id, Ct);
        Assert.Equal((OrderStatus.Preparing, PaymentStatus.Paid, 3), (saved.OrderStatus, saved.PaymentStatus, saved.Version));
        Assert.Equal(Listed.TrackingNumber, saved.TrackingNumber);
        await AssertCorrectedAsync(order.Id, s5, TransitionCode.S5, CorrectionReason.WrongOrder);
    }

    [Fact]
    public async Task TheDeliveryFailedMarksCorrection_TakesTheOrderBackToShipped_KK7()
    {
        (Order order, _) = await SeedAsync(OrderStatus.Shipped, PaymentStatus.Paid, PaymentMethod.Card, new Line(ProductType.Physical, 1));
        Guid s7 = (await RunAsync((unit, token) => unit.MarkDeliveryFailedAsync(new AdminOrderRequest(order.Id, AdminCaller, 1), token))).TransitionRecordIds.Single();

        OrderTransitionResult result = await CorrectAsync(order.Id, s7, 2, CorrectionReason.MarkedBeforeOccurred, null);

        Assert.False(result.IsRejected);
        await using AppDbContext reader = NewContext();
        Assert.Equal(OrderStatus.Shipped, (await reader.Orders.SingleAsync(o => o.Id == order.Id, Ct)).OrderStatus);
        await AssertCorrectedAsync(order.Id, s7, TransitionCode.S7, CorrectionReason.MarkedBeforeOccurred);
    }

    // ---- KK8: the transfer mark ----

    [Theory]
    [InlineData(TransferObstacle.DeliveryMark)]
    [InlineData(TransferObstacle.Shipment)]
    [InlineData(TransferObstacle.Refund)]
    [InlineData(TransferObstacle.Cancellation)]
    [InlineData(TransferObstacle.Removal)]
    [InlineData(TransferObstacle.Termination)]
    [InlineData(TransferObstacle.Withdrawal)]
    public async Task TheTransferMarksCorrection_WithAnyOfItsObstacles_IsRefused_ItemRecordExists_AndWritesNothing_KK8(TransferObstacle obstacle)
    {
        (Order order, List<OrderItem> items) = await SeedAsync(
            OrderStatus.Placed, PaymentStatus.Pending, PaymentMethod.BankTransfer, new Line(ProductType.Physical, 1), new Line(ProductType.Service, 1));
        PaymentTransitionResult marked = await RunAsync((unit, token) => unit.MarkTransferPaidAsync(Mark(order.Id, 1), token));
        Assert.Equal([TransitionCode.O1, TransitionCode.S1], marked.Applied);
        Guid o1 = await RecordIdAsync(order.Id, TransitionCode.O1);
        OrderStatus status = OrderStatus.Preparing;
        switch (obstacle)
        {
            case TransferObstacle.DeliveryMark:
                await ExecuteAsync($"UPDATE order_items SET delivery_marked_at = '{Start:O}', delivery_date = '{Today:yyyy-MM-dd}' WHERE id = '{items[1].Id}'");
                break;
            case TransferObstacle.Shipment:
                Assert.False((await RunAsync((unit, token) => unit.ShipOrderAsync(new ShipOrderRequest(order.Id, AdminCaller, 2, true, Listed), token))).IsRejected);
                status = OrderStatus.Shipped;
                break;
            case TransferObstacle.Refund:
                await SaveAsync(c => c.Refunds.Add(Refund.ByAdmin(order.Id, RefundType.AmountBased, PaymentMethod.BankTransfer, 10m, _admin!.Id, _admin.Name, Start)));
                break;
            case TransferObstacle.Cancellation:
                await SaveAsync(c => c.Cancellations.Add(Cancellation.ByBuyer(items[1].Id, 1, Amounts, null, Start)));
                break;
            case TransferObstacle.Removal:
                await SaveAsync(c => c.ItemRemovals.Add(ItemRemoval.Create(items[1].Id, 1, _admin!.Id, _admin.Name, Amounts, Start)));
                break;
            case TransferObstacle.Termination:
                await SaveAsync(c => c.LateDeliveryTerminations.Add(LateDeliveryTermination.ByBuyer(items[1].Id, 1, Today, Amounts, null, Start)));
                break;
            case TransferObstacle.Withdrawal:
                await SaveAsync(c => c.Withdrawals.Add(Withdrawal.ByBuyer(items[1].Id, 1, Today, null, Amounts, null, Start)));
                break;
        }

        int version = await VersionAsync(order.Id);
        OrderTransitionResult result = await CorrectAsync(order.Id, o1, version, CorrectionReason.WrongOrder, null);

        Assert.Equal((TransitionRefusal.CorrectionNotPossible, CorrectionObstacle.ItemRecordExists), (result.Refusal, result.Obstacle));
        await AssertNotCorrectedAsync(order.Id, status, PaymentStatus.Paid, version);
    }

    [Fact]
    public async Task TheTransferMarksCorrection_TakesBothAxesBack_StartsTheWindowAgain_AndTheReminderGoesASecondTime_KK3_KK8()
    {
        await SeedCompanySettingsAsync(bankTransferWindow: 2);
        try
        {
            (Order order, _) = await SeedAsync(OrderStatus.Placed, PaymentStatus.Pending, PaymentMethod.BankTransfer, new Line(ProductType.Physical, 1));
            await ExecuteAsync(
                $"UPDATE orders SET payment_window_ends_at = '{Start.AddDays(2):O}', payment_reminder_at = '{Start.AddMinutes(1):O}' WHERE id = '{order.Id}'");

            // The first B-3 goes at its moment while the window runs.
            _clock.Advance(TimeSpan.FromMinutes(1));
            await ProcessReminderAsync(order.Id);
            Assert.Equal(1, await CountDispatchesAsync(order.Id, DispatchType.B3));
            int marking = await VersionAsync(order.Id);
            PaymentTransitionResult marked = await RunAsync((unit, token) => unit.MarkTransferPaidAsync(Mark(order.Id, marking), token));
            Assert.Equal([TransitionCode.O1, TransitionCode.S1], marked.Applied);
            Guid o1 = await RecordIdAsync(order.Id, TransitionCode.O1);
            Guid s1 = await RecordIdAsync(order.Id, TransitionCode.S1);
            _clock.Advance(TimeSpan.FromHours(2));
            DateTimeOffset correctedAt = _clock.GetUtcNow();

            OrderTransitionResult result = await CorrectAsync(order.Id, o1, marking + 1, CorrectionReason.WrongOrder, null);

            // Z-8 starts again from the correction's moment (1.6.2.4): the window's end and the reminder's moment are the new window's.
            Assert.False(result.IsRejected);
            DateTimeOffset windowEnd = PeriodCounting.EndOf(CountedPeriod.BankTransferPaymentWindow, correctedAt, 2, BusinessCalendar.Product);
            DateTimeOffset reminderAt = TransferSchedule.ReminderAt(windowEnd);
            await using (AppDbContext reader = NewContext())
            {
                Order saved = await reader.Orders.SingleAsync(o => o.Id == order.Id, Ct);
                Assert.Equal((OrderStatus.Placed, PaymentStatus.Pending, marking + 2), (saved.OrderStatus, saved.PaymentStatus, saved.Version));
                Assert.Equal((windowEnd, reminderAt), (saved.PaymentWindowEndsAt, saved.PaymentReminderAt!.Value));
                Assert.Equivalent(new[] { o1, s1 }, await reader.TransitionRecords
                    .Where(t => t.OrderId == order.Id && t.TransitionCode == TransitionCode.Correction)
                    .Select(t => t.CorrectedTransitionRecordId!.Value).ToListAsync(Ct));
                Assert.Equal(2, await reader.TransitionRecords.CountAsync(t => t.OrderId == order.Id && (t.Id == o1 || t.Id == s1), Ct));
            }

            Assert.Equal(1, await CountDispatchesAsync(order.Id, DispatchType.B13));

            // B-3 goes once more on the new window's last business day.
            _clock.SetUtcNow(reminderAt);
            await ProcessReminderAsync(order.Id);
            await ProcessReminderAsync(order.Id);
            Assert.Equal(2, await CountDispatchesAsync(order.Id, DispatchType.B3));
        }
        finally
        {
            // The collection's other tests start without the firm's settings row; it is taken away again.
            await ExecuteAsync("TRUNCATE company_settings CASCADE");
        }
    }

    [Fact]
    public async Task ACorrectedShipmentMark_IsNoShipment_TheTransferMarkIsCorrectedAfterIt_KK8()
    {
        await SeedCompanySettingsAsync(bankTransferWindow: 1);
        try
        {
            (Order order, _) = await SeedAsync(OrderStatus.Placed, PaymentStatus.Pending, PaymentMethod.BankTransfer, new Line(ProductType.Physical, 1));
            await RunAsync((unit, token) => unit.MarkTransferPaidAsync(Mark(order.Id, 1), token));
            Guid o1 = await RecordIdAsync(order.Id, TransitionCode.O1);
            Guid s5 = (await RunAsync((unit, token) => unit.ShipOrderAsync(new ShipOrderRequest(order.Id, AdminCaller, 2, true, Listed), token))).TransitionRecordIds.Single();
            Assert.False((await CorrectAsync(order.Id, s5, 3, CorrectionReason.WrongOrder, null)).IsRejected);

            OrderTransitionResult result = await CorrectAsync(order.Id, o1, 4, CorrectionReason.WrongOrder, null);

            Assert.False(result.IsRejected);
            await using AppDbContext reader = NewContext();
            Order saved = await reader.Orders.SingleAsync(o => o.Id == order.Id, Ct);
            Assert.Equal((OrderStatus.Placed, PaymentStatus.Pending), (saved.OrderStatus, saved.PaymentStatus));
        }
        finally
        {
            await ExecuteAsync("TRUNCATE company_settings CASCADE");
        }
    }

    [Fact]
    public async Task TheTransferMark_OfAnOrderWithADigitalItem_IsNotCorrected_DigitalItem_KK8()
    {
        (Order order, _) = await SeedAsync(
            OrderStatus.Placed, PaymentStatus.Pending, PaymentMethod.BankTransfer, new Line(ProductType.Service, 1), new Line(ProductType.Digital, 1));
        await RunAsync((unit, token) => unit.MarkTransferPaidAsync(Mark(order.Id, 1), token));
        Guid o1 = await RecordIdAsync(order.Id, TransitionCode.O1);

        OrderTransitionResult result = await CorrectAsync(order.Id, o1, 2, CorrectionReason.WrongOrder, null);

        Assert.Equal((TransitionRefusal.CorrectionNotPossible, CorrectionObstacle.DigitalItem), (result.Refusal, result.Obstacle));
        await AssertNotCorrectedAsync(order.Id, OrderStatus.Placed, PaymentStatus.Paid, 2);
    }

    // ---- KK9: the card's payment and the withdrawal ----

    [Fact]
    public async Task TheCardPayment_HasNoCorrection_CardPayment_KK9()
    {
        (Order order, _) = await SeedAsync(OrderStatus.Placed, PaymentStatus.Pending, PaymentMethod.Card, new Line(ProductType.Physical, 1));
        await RunAsync((unit, token) => unit.ApplyCardPaymentAsync(new CardPaymentSucceeded(order.Id, "trx-t319"), token));
        Guid o1 = await RecordIdAsync(order.Id, TransitionCode.O1);
        Guid s1 = await RecordIdAsync(order.Id, TransitionCode.S1);

        OrderTransitionResult ofO1 = await CorrectAsync(order.Id, o1, 2, CorrectionReason.WrongOrder, null);
        OrderTransitionResult ofS1 = await CorrectAsync(order.Id, s1, 2, CorrectionReason.WrongOrder, null);

        Assert.Equal((TransitionRefusal.CorrectionNotPossible, CorrectionObstacle.CardPayment), (ofO1.Refusal, ofO1.Obstacle));
        Assert.Equal((TransitionRefusal.CorrectionNotPossible, CorrectionObstacle.NotCorrectable), (ofS1.Refusal, ofS1.Obstacle));
        await AssertNotCorrectedAsync(order.Id, OrderStatus.Preparing, PaymentStatus.Paid, 2);
    }

    [Fact]
    public async Task AWithdrawal_IsNotATransition_AndIsNeverCorrected_KK9()
    {
        (Order order, List<OrderItem> items) = await SeedAsync(OrderStatus.Delivered, PaymentStatus.Paid, PaymentMethod.Card, new Line(ProductType.Physical, 1, Delivered: true));
        Withdrawal withdrawal = Withdrawal.ByBuyer(items[0].Id, 1, Today, null, Amounts, null, Start);
        await SaveAsync(c => c.Withdrawals.Add(withdrawal));

        OrderTransitionResult result = await CorrectAsync(order.Id, withdrawal.Id, 1, CorrectionReason.WrongOrder, null);

        Assert.Equal((TransitionRefusal.CorrectionNotPossible, CorrectionObstacle.NotCorrectable), (result.Refusal, result.Obstacle));
        await AssertNotCorrectedAsync(order.Id, OrderStatus.Delivered, PaymentStatus.Paid, 1);
        await using AppDbContext reader = NewContext();
        Assert.True(await reader.Withdrawals.AnyAsync(w => w.Id == withdrawal.Id, Ct));
    }

    // ---- helpers ----

    private TransferPaidMark Mark(Guid orderId, int version) => new(orderId, _admin!.Id, _admin.Name, version, true);

    private Task<OrderTransitionResult> CorrectAsync(Guid orderId, Guid recordId, int version, CorrectionReason? reason, string? note) =>
        RunAsync((unit, token) => unit.CorrectTransitionAsync(new CorrectTransitionRequest(orderId, recordId, AdminCaller, version, reason, note), token));

    /// <summary>The firm's cancellation of the whole order (S4 from Preparing), with the confirmation and a reason from the closed list.</summary>
    private async Task<Guid> CancelByFirmAsync(Guid orderId)
    {
        OrderTransitionResult cancelled = await RunAsync((unit, token) => unit.CancelOrderAsync(
            CancelOrderRequest.ByAdministrator(orderId, AdminCaller, 1, true, CancellationReason.OutOfStock), token));
        Assert.False(cancelled.IsRejected);
        return await RecordIdAsync(orderId, TransitionCode.S4);
    }

    /// <summary>The item record the firm's cancellation closed with its transition (06 4.22) and the refund it opened; the units went back to stock.</summary>
    private async Task<(Cancellation Cancellation, Refund Refund)> SeedCancellationAsync(Order order, OrderItem item, Guid transitionRecordId, PaymentMethod method)
    {
        Cancellation cancellation = Cancellation.ByAdmin(item.Id, item.Quantity, _admin!.Id, _admin.Name, CancellationReason.OutOfStock, null, Amounts, Start);
        cancellation.TransitionRecordId = transitionRecordId;
        Refund refund = Refund.ByAdmin(order.Id, RefundType.Cancellation, method, 100m * item.Quantity, _admin.Id, _admin.Name, Start);
        refund.CancellationId = cancellation.Id;
        refund.OrderItemId = item.Id;
        refund.Quantity = item.Quantity;
        await SaveAsync(c =>
        {
            c.Cancellations.Add(cancellation);
            c.Refunds.Add(refund);
        });
        return (cancellation, refund);
    }

    /// <summary>Writes an item record under the order's lock and evaluates the closure rule in the same transaction, like the record's task.</summary>
    private async Task<ClosureResult> CloseWithAsync(Guid orderId, Action<AppDbContext> record)
    {
        ClosureResult closure = ClosureResult.None;
        await using AsyncServiceScope scope = _provider.CreateAsyncScope();
        OrderTransitions core = scope.ServiceProvider.GetRequiredService<OrderTransitions>();
        AppDbContext db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        OrderLocks locks = scope.ServiceProvider.GetRequiredService<OrderLocks>();
        await scope.ServiceProvider.GetRequiredService<ITransactionRunner>().RunAsync(
            async token =>
            {
                await locks.LockAsync([orderId], token);
                Order locked = await db.Orders.SingleAsync(o => o.Id == orderId, token);
                record(db);
                locked.BumpVersion();
                closure = await core.EvaluateClosureAsync(locked, TransitionActor.Buyer, token);
                return new TransitionOutcome(null, [], closure);
            },
            Ct);
        return closure;
    }

    private async Task<T> RunAsync<T>(Func<IOrderTransitions, CancellationToken, Task<T>> work)
        where T : IOperationResult
    {
        await using AsyncServiceScope scope = _provider.CreateAsyncScope();
        IOrderTransitions unit = scope.ServiceProvider.GetRequiredService<IOrderTransitions>();
        return await scope.ServiceProvider.GetRequiredService<ITransactionRunner>().RunAsync(token => work(unit, token), Ct);
    }

    private async Task<IReadOnlyDictionary<Guid, int>> ClosedQuantitiesAsync(Guid itemId)
    {
        await using AsyncServiceScope scope = _provider.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<IAfterSalesModule>().ClosedQuantitiesAsync([itemId], Ct);
    }

    private async Task ProcessReminderAsync(Guid orderId)
    {
        await using AsyncServiceScope scope = _provider.CreateAsyncScope();
        TransferPaymentReminder source = _provider.GetServices<IDueRecordSource>().OfType<TransferPaymentReminder>().Single();
        await source.ProcessAsync(scope.ServiceProvider, orderId, Ct);
    }

    /// <summary>The correction row beside the wrong row (KK3): the wrong row is unchanged, one correction names it with the administrator and the reason, and B-13 went once.</summary>
    private async Task<TransitionRecord> AssertCorrectedAsync(Guid orderId, Guid correctedId, TransitionCode correctedCode, CorrectionReason reason)
    {
        await using AppDbContext reader = NewContext();
        TransitionRecord wrong = await reader.TransitionRecords.SingleAsync(t => t.Id == correctedId, Ct);
        Assert.Equal((orderId, correctedCode, (Guid?)null), (wrong.OrderId, wrong.TransitionCode, wrong.CorrectedTransitionRecordId));
        TransitionRecord correction = await reader.TransitionRecords.SingleAsync(t => t.CorrectedTransitionRecordId == correctedId, Ct);
        Assert.Equal(
            (orderId, TransitionCode.Correction, ActorKind.Admin, _admin!.Id, _admin.Name, (CorrectionReason?)reason),
            (correction.OrderId, correction.TransitionCode, correction.ActorKind, correction.AdminId!.Value, correction.AdminName, correction.CorrectionReason));
        Assert.Equal(1, await CountDispatchesAsync(orderId, DispatchType.B13));
        return correction;
    }

    /// <summary>A refused correction writes nothing: the states and the version stay, no correction row and no B-13.</summary>
    private async Task AssertNotCorrectedAsync(Guid orderId, OrderStatus status, PaymentStatus payment, int version)
    {
        await using AppDbContext reader = NewContext();
        Order saved = await reader.Orders.SingleAsync(o => o.Id == orderId, Ct);
        Assert.Equal((status, payment, version), (saved.OrderStatus, saved.PaymentStatus, saved.Version));
        Assert.Equal(0, await reader.TransitionRecords.CountAsync(t => t.OrderId == orderId && t.TransitionCode == TransitionCode.Correction, Ct));
        Assert.Equal(0, await CountDispatchesAsync(orderId, DispatchType.B13));
    }

    private async Task<int> CountDispatchesAsync(Guid orderId, DispatchType type)
    {
        await using AppDbContext reader = NewContext();
        return await reader.DispatchRecords.CountAsync(d => d.OrderId == orderId && d.DispatchType == type, Ct);
    }

    private async Task<Guid> RecordIdAsync(Guid orderId, TransitionCode code)
    {
        await using AppDbContext reader = NewContext();
        return await reader.TransitionRecords.Where(t => t.OrderId == orderId && t.TransitionCode == code).Select(t => t.Id).SingleAsync(Ct);
    }

    private async Task<int> VersionAsync(Guid orderId)
    {
        await using AppDbContext reader = NewContext();
        return await reader.Orders.Where(o => o.Id == orderId).Select(o => o.Version).SingleAsync(Ct);
    }

    private async Task<(int? Stock, int Reserved)> VariantAsync(Guid variantId)
    {
        await using AppDbContext reader = NewContext();
        Variant variant = await reader.Variants.SingleAsync(v => v.Id == variantId, Ct);
        return (variant.StockQuantity, variant.ReservedQuantity);
    }

    private async Task<Variant> SeedVariantAsync(int stock)
    {
        int n = Interlocked.Increment(ref _counter);
        Product product = Product.Create("Ürün", $"duzeltme-urun-{n}", ProductType.Physical, 20m, Start).WithSeededOrigin();
        Variant variant = Variant.Create(product.Id, $"DZT-{n}", 10m, Start, stock);
        await SaveAsync(c =>
        {
            c.Products.Add(product);
            c.Variants.Add(variant);
        });
        return variant;
    }

    private async Task SeedCompanySettingsAsync(int bankTransferWindow)
    {
        await ExecuteAsync("TRUNCATE company_settings CASCADE");
        CompanySettings settings = CompanySettings.Create(Start);
        settings.BankTransferPaymentWindow = bankTransferWindow;
        settings.DownloadAllowance = 1;
        settings.HomepageLayout = Shopfolio.Modules.CorporateContent.Contracts.HomepageLayout.StoreFirst;
        settings.MenuNameProducts = "Ürünler";
        settings.MenuNameOfferings = "Hizmetler";
        settings.MenuNamePortfolioItems = "Çalışmalar";
        settings.MenuNameAboutPage = "Hakkımızda";
        settings.MenuNameFaqItems = "Sıkça Sorulanlar";
        settings.MenuNameContact = "İletişim";
        await SaveAsync(c => c.CompanySettings.Add(settings));
    }

    private async Task<(Order Order, List<OrderItem> Items)> SeedAsync(OrderStatus orderStatus, PaymentStatus paymentStatus, PaymentMethod method, params Line[] lines)
    {
        int n = Interlocked.Increment(ref _counter);
        Province province = Province.Create($"{n % 100:D2}", $"İl {n}", Start);
        LegalTextVersion form = LegalTextVersion.CreatePublished(LegalTextType.PreInformationForm, n, Start, "metin", Start);
        LegalTextVersion contract = LegalTextVersion.CreatePublished(LegalTextType.DistanceSalesContract, n, Start, "metin", Start);
        LegalTextVersion notice = LegalTextVersion.CreatePublished(LegalTextType.PrivacyNotice, n, Start, "metin", Start).WithHosting();
        Order order = Order.Create(
            $"2026-{n + 319000:D6}", $"access-placeholder-{n}", $"confirm-placeholder-{n}", method, true, "Ali@Example.com", 120m, 0m, Start.AddHours(1), Start);
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
        order.BankTransferIban = method == PaymentMethod.BankTransfer ? "TR330006100519786457841326" : null;
        List<OrderItem> items = [];
        foreach (Line line in lines)
        {
            OrderItem item = OrderItem.Create(order.Id, null, line.VariantId, line.Type, "Ürün", line.Quantity, 100m, 20m, 16.67m, Start);
            if (line.Type == ProductType.Digital)
            {
                item.DownloadAllowance = 5;
            }

            if (line.Type == ProductType.Service)
            {
                item.FulfilmentPeriod = 3;
            }

            if (line.Delivered)
            {
                item.DeliveryMarkedAt = Start;
                item.DeliveryDate = Today;
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

    /// <summary>One item of a seeded order: its type, quantity, variant and whether it already carries the delivery mark.</summary>
    public sealed record Line(ProductType Type, int Quantity, Guid? VariantId = null, bool Delivered = false);
}
