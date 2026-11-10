using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Errors;
using Shopfolio.IntegrationTests.Database;
using Shopfolio.Modules.Administration;
using Shopfolio.Modules.Administration.Contracts;
using Shopfolio.Modules.AfterSales;
using Shopfolio.Modules.Catalog;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Notifications;
using Shopfolio.Modules.Notifications.Contracts;
using Shopfolio.Modules.Ordering;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Modules.Payment;
using Shopfolio.Modules.Payment.Contracts;

namespace Shopfolio.IntegrationTests.Ordering;

/// <summary>
/// The administrator's interventions of T386 on a real PostgreSQL (criteria 1-10, KK1-KK10; 02 10.4.3-10.4.5, 10.4.7, 7.1.7, 7.2.2, 7.2.8, 7.2.9; K-712, K-1474, K-608, K-1095):
/// the removal follows the cancellation's regime without a reason, with B-11 and a trail row; the carrier correction sends B-12; the order-level delivery date and the return's
/// arrival date are corrected within their bounds without a notice; the internal note never reaches the customer and its trail row carries no text.
/// </summary>
public sealed partial class OrderCorrectionTests
{
    // ---- KK2: the removal's boundaries ----

    [Fact]
    public async Task BeforeThePaymentsApproval_NoItemIsRemoved_TheOrderIsCancelledWhole_KK2()
    {
        Order order = await SeedAsync(OrderStatus.Placed, PaymentStatus.Pending, ProductType.Physical);
        Guid item = await ItemIdAsync(order.Id, ProductType.Physical);

        OrderTransitionResult result = await RunAsync(c => c.RemoveItemAsync(Removal(order, item, 1), Ct));

        Assert.Equal(TransitionRefusal.ConditionNotMet, result.Refusal);
        Assert.Equal(0, await CountAsync(c => c.ItemRemovals));
    }

    [Theory]
    [InlineData(OrderStatus.Shipped)]
    [InlineData(OrderStatus.Delivered)]
    [InlineData(OrderStatus.DeliveryFailed)]
    public async Task APhysicalItem_IsNotRemovedOnceTheOrderWasShipped_KK2(OrderStatus status)
    {
        Order order = await SeedAsync(status, PaymentStatus.Paid, ProductType.Physical);
        Guid item = await ItemIdAsync(order.Id, ProductType.Physical);

        OrderTransitionResult result = await RunAsync(c => c.RemoveItemAsync(Removal(order, item, 1), Ct));

        Assert.Equal(TransitionRefusal.ConditionNotMet, result.Refusal);
        Assert.Equal(0, await CountAsync(c => c.ItemRemovals));
    }

    [Fact]
    public async Task ADeliveredDigitalItem_IsNotRemoved_WhileAnUndeliveredOneIs_EvenOnAShippedOrder_KK2()
    {
        Order order = await SeedAsync(OrderStatus.Shipped, PaymentStatus.Paid, ProductType.Physical, ProductType.Digital);
        Guid digital = await ItemIdAsync(order.Id, ProductType.Digital);

        OrderTransitionResult open = await RunAsync(c => c.RemoveItemAsync(Removal(order, digital, 1), Ct));
        Assert.Null(open.Refusal);

        Order other = await SeedAsync(OrderStatus.Preparing, PaymentStatus.Paid, ProductType.Digital);
        Guid delivered = await ItemIdAsync(other.Id, ProductType.Digital);
        await ExecuteAsync($"UPDATE order_items SET delivery_marked_at = now(), delivery_date = current_date WHERE id = '{delivered}'");

        Order reloaded = await ReloadAsync(other.Id);
        OrderTransitionResult closed = await RunAsync(c => c.RemoveItemAsync(Removal(reloaded, delivered, 1), Ct));
        Assert.Equal(TransitionRefusal.ConditionNotMet, closed.Refusal);
    }

    [Fact]
    public async Task MoreUnitsThanOpen_OrNone_AreRefused_AndARefundToTheCardAsksTheConfirmation_KK2()
    {
        Order order = await SeedAsync(OrderStatus.Preparing, PaymentStatus.Paid, ProductType.Physical);
        Guid item = await ItemIdAsync(order.Id, ProductType.Physical);

        Assert.Equal(TransitionRefusal.InvalidInput, (await RunAsync(c => c.RemoveItemAsync(Removal(order, item, 3), Ct))).Refusal);
        Assert.Equal(TransitionRefusal.InvalidInput, (await RunAsync(c => c.RemoveItemAsync(Removal(order, item, 0), Ct))).Refusal);
        Assert.Equal(TransitionRefusal.ConfirmationRequired, (await RunAsync(c => c.RemoveItemAsync(Removal(order, item, 1) with { Confirmed = false }, Ct))).Refusal);
        Assert.Equal(TransitionRefusal.VersionConflict, (await RunAsync(c => c.RemoveItemAsync(Removal(order, item, 1) with { ExpectedVersion = order.Version + 5 }, Ct))).Refusal);
        Assert.Equal(TransitionRefusal.ActorNotAllowed, (await RunAsync(c => c.RemoveItemAsync(Removal(order, item, 1) with { Admin = null }, Ct))).Refusal);
        Assert.Equal(0, await CountAsync(c => c.ItemRemovals));
    }

    // ---- T364 KK3, KK5: two administrators remove the same item from the same screen at once (02 3.31.2) ----

    [Fact]
    public async Task TwoAdministrators_RemoveTheSameItemAtOnce_OnlyTheFirstCompletedIsApplied_OneRemoval_OneRefund_OneNotice_T364_KK3_KK5()
    {
        Order order = await SeedAsync(OrderStatus.Preparing, PaymentStatus.Paid, ProductType.Physical);
        Guid item = await ItemIdAsync(order.Id, ProductType.Physical);

        OrderTransitionResult[] results = await Task.WhenAll(
            RunAsync(c => c.RemoveItemAsync(Removal(order, item, 1), Ct)),
            RunAsync(c => c.RemoveItemAsync(Removal(order, item, 1), Ct)));

        Assert.Single(results, r => r.Refusal is null);
        Assert.Single(results, r => r.Refusal == TransitionRefusal.VersionConflict);
        Assert.Equal(1, await CountAsync(c => c.ItemRemovals.Where(r => r.OrderItemId == item)));
        Assert.Equal(1, await CountAsync(c => c.Refunds.Where(r => r.OrderId == order.Id)));
        Assert.Equal(1, await CountAsync(c => c.AuditLogs.Where(a => a.TargetRecordId == order.Id)));
        Assert.Equal(1, (await DispatchesAsync(order.Id)).Count(d => d.Type == DispatchType.B11));
        Assert.Equal(1, await CountAsync(c => c.ItemRemovals.Where(r => r.OrderItemId == item && r.Quantity == 1)));
        Assert.Equal(OrderStatus.Preparing, (await ReloadAsync(order.Id)).OrderStatus);
    }

    // ---- KK3, KK4: the amount from the frozen values, the line and the shipping fee ----

    [Fact]
    public async Task TheReduction_RefundsTheUnitsFrozenAmount_ToTheCard_AndTheItemLivesOn_KK3()
    {
        Order order = await SeedAsync(OrderStatus.Preparing, PaymentStatus.Paid, ProductType.Physical);
        Guid item = await ItemIdAsync(order.Id, ProductType.Physical);

        OrderTransitionResult result = await RunAsync(c => c.RemoveItemAsync(Removal(order, item, 1), Ct));

        Assert.Null(result.Refusal);
        Assert.False(result.ItemFullyRemoved);
        await using AppDbContext reader = NewContext();
        ItemRemoval removal = await reader.ItemRemovals.AsNoTracking().SingleAsync(r => r.OrderItemId == item, Ct);
        Assert.Equal((1, 300m, 10m, _admin!.Id), (removal.Quantity, removal.ProductAmount, removal.CouponShare, removal.AdminId!.Value));
        Refund refund = await reader.Refunds.AsNoTracking().SingleAsync(r => r.OrderId == order.Id, Ct);
        Assert.Equal((RefundType.ItemRemoval, removal.Id, removal.ProductAmount - removal.CouponShare, false), (refund.RefundType, refund.ItemRemovalId, refund.Amount, refund.IsShippingFeeIncluded));
        Assert.Equal(OrderStatus.Preparing, (await ReloadAsync(order.Id)).OrderStatus);
    }

    [Fact]
    public async Task TheLastPhysicalUnits_CarryTheShippingFee_AndCloseTheOrderAsACancellationWould_KK4()
    {
        Order order = await SeedAsync(OrderStatus.Preparing, PaymentStatus.Paid, ProductType.Physical);
        await ExecuteAsync($"UPDATE orders SET shipping_fee = 40, shipping_fee_vat_breakdown = '[]', total = total + 40 WHERE id = '{order.Id}'");
        order = await ReloadAsync(order.Id);
        Guid item = await ItemIdAsync(order.Id, ProductType.Physical);

        OrderTransitionResult result = await RunAsync(c => c.RemoveItemAsync(Removal(order, item, 2), Ct));

        Assert.Null(result.Refusal);
        Assert.True(result.ItemFullyRemoved);
        await using AppDbContext reader = NewContext();
        Refund refund = await reader.Refunds.AsNoTracking().SingleAsync(r => r.OrderId == order.Id, Ct);
        ItemRemoval removal = await reader.ItemRemovals.AsNoTracking().SingleAsync(r => r.OrderItemId == item, Ct);
        Assert.True(refund.IsShippingFeeIncluded);
        Assert.Equal(removal.ProductAmount - removal.CouponShare + 40m, refund.Amount);
        Assert.Equal(OrderStatus.Cancelled, (await ReloadAsync(order.Id)).OrderStatus);
        Assert.NotNull(result.ClosureCode);
    }

    [Fact]
    public async Task TheRemovedUnits_GoBackToStock_AndTheCouponRightOnlyWithTheOrdersLastUnit_AsOnACancellation_KK4()
    {
        int n = Interlocked.Increment(ref _counter);
        Product product = Product.Create("Ürün", $"cikarma-urun-{n}", ProductType.Physical, 20m, Start).WithSeededOrigin();
        Variant variant = Variant.Create(product.Id, $"CKR-{n}", 10m, Start, 5);
        Coupon coupon = Coupon.CreateAmount($"CIKAR{n}", 50m, 100m, TurkeyTime.DateOf(Start).AddDays(-7), TurkeyTime.DateOf(Start).AddDays(30), 3, Start);
        await SaveAsync(c =>
        {
            c.Products.Add(product);
            c.Variants.Add(variant);
            c.Coupons.Add(coupon);
        });
        await ExecuteAsync($"UPDATE coupons SET used_count = 1 WHERE id = '{coupon.Id}'");
        Order order = await SeedAsync(OrderStatus.Preparing, PaymentStatus.Paid, ProductType.Physical);
        Guid item = await ItemIdAsync(order.Id, ProductType.Physical);
        await ExecuteAsync($"UPDATE order_items SET variant_id = '{variant.Id}' WHERE id = '{item}'");
        await ExecuteAsync($"UPDATE orders SET coupon_id = '{coupon.Id}', coupon_code = '{coupon.Code}' WHERE id = '{order.Id}'");
        order = await ReloadAsync(order.Id);

        Assert.Null((await RunAsync(c => c.RemoveItemAsync(Removal(order, item, 1), Ct))).Refusal);
        Assert.Equal((6, 1), await CountersAsync(variant.Id, coupon.Id));

        Order reloaded = await ReloadAsync(order.Id);
        Assert.Null((await RunAsync(c => c.RemoveItemAsync(Removal(reloaded, item, 1), Ct))).Refusal);
        Assert.Equal((7, 0), await CountersAsync(variant.Id, coupon.Id));
    }

    [Fact]
    public async Task OnTheTransferLine_TheRemovalOpensTheIbanRequest_AndSendsB14_KK3()
    {
        Order order = await SeedAsync(OrderStatus.Preparing, PaymentStatus.Paid, ProductType.Physical);
        await ExecuteAsync($"UPDATE orders SET payment_method = 'BankTransfer', bank_transfer_iban = 'TR000000000000000000000001' WHERE id = '{order.Id}'");
        order = await ReloadAsync(order.Id);
        Guid item = await ItemIdAsync(order.Id, ProductType.Physical);

        OrderTransitionResult result = await RunAsync(c => c.RemoveItemAsync(Removal(order, item, 1) with { Confirmed = false }, Ct));

        Assert.Null(result.Refusal);
        Assert.Equal(1, await CountAsync(c => c.RefundIbanRequests.Where(r => r.OrderItemId == item)));
        Assert.Contains(await DispatchesAsync(order.Id), d => d.Type == DispatchType.B14);
    }

    // ---- KK5: no reason, a trail row and B-11 ----

    [Fact]
    public async Task TheRemoval_AsksNoReason_WritesTheTrailWithTheItemAndTheUnits_AndSendsB11_KK5()
    {
        Order order = await SeedAsync(OrderStatus.Preparing, PaymentStatus.Paid, ProductType.Physical);
        Guid item = await ItemIdAsync(order.Id, ProductType.Physical);

        await RunAsync(c => c.RemoveItemAsync(Removal(order, item, 1), Ct));
        Order reloaded = await ReloadAsync(order.Id);
        await RunAsync(c => c.RemoveItemAsync(Removal(reloaded, item, 1), Ct));

        Assert.DoesNotContain(typeof(RemoveItemRequest).GetProperties(), p => p.Name.Contains("Reason", StringComparison.Ordinal));
        await using AppDbContext reader = NewContext();
        List<AuditLog> rows = await reader.AuditLogs.AsNoTracking().Where(a => a.TargetRecordId == order.Id).OrderBy(a => a.CreatedAt).ToListAsync(Ct);
        Assert.Equal([nameof(AuditOperation.ReduceItemQuantity), nameof(AuditOperation.RemoveItem)], rows.Select(r => r.Operation));
        Assert.All(rows, r => Assert.Contains(r.Details!.Values, v => v.Name == "ItemId" && v.Value == item.ToString("D")));
        Assert.All(rows, r => Assert.Contains(r.Details!.Values, v => v.Name == "Quantity" && v.Value == "1"));
        Assert.Equal(2, (await DispatchesAsync(order.Id)).Count(d => d.Type == DispatchType.B11));
    }

    // ---- KK6: the carrier correction sends B-12 ----

    [Fact]
    public async Task TheCarrierCorrection_ChangesTheShipmentsValues_SendsB12_AndTheTrailHasNoTrackingNumber_KK6()
    {
        Order order = await SeedAsync(OrderStatus.Shipped, PaymentStatus.Paid, ProductType.Physical);
        await ExecuteAsync($"UPDATE orders SET other_carrier_name = 'Eski Kargo', tracking_number = 'ESKI-1' WHERE id = '{order.Id}'");
        order = await ReloadAsync(order.Id);

        OrderCorrectionResult same = await RunAsync(c => c.CorrectShipmentAsync(new CorrectShipmentRequest(order.Id, AdminCaller, new ShipmentInfo(null, "Eski Kargo", "ESKI-1", false), order.Version), Ct));
        OrderCorrectionResult wrong = await RunAsync(c => c.CorrectShipmentAsync(new CorrectShipmentRequest(order.Id, AdminCaller, new ShipmentInfo(null, "Yeni Kargo", null, false), order.Version), Ct));
        OrderCorrectionResult result = await RunAsync(c => c.CorrectShipmentAsync(new CorrectShipmentRequest(order.Id, AdminCaller, new ShipmentInfo(null, "Yeni Kargo", "YENI-77", false), order.Version), Ct));

        Assert.Equal(TransitionRefusal.InvalidInput, same.Refusal);
        Assert.Equal(TransitionRefusal.InvalidInput, wrong.Refusal);
        Assert.Null(result.Refusal);
        Order saved = await ReloadAsync(order.Id);
        Assert.Equal(("Yeni Kargo", "YENI-77", order.Version + 1), (saved.OtherCarrierName, saved.TrackingNumber, saved.Version));
        Assert.Single(await DispatchesAsync(order.Id), d => d.Type == DispatchType.B12);
        await using AppDbContext reader = NewContext();
        AuditLog row = await reader.AuditLogs.AsNoTracking().SingleAsync(a => a.TargetRecordId == order.Id, Ct);
        Assert.Equal(nameof(AuditOperation.CorrectShippingInfo), row.Operation);
        Assert.DoesNotContain(row.Details!.Values, v => (v.Value ?? string.Empty).Contains("YENI", StringComparison.Ordinal) || (v.Value ?? string.Empty).Contains("Kargo", StringComparison.Ordinal));
    }

    [Fact]
    public async Task TheCarrierCorrection_IsClosedBeforeTheShipment_KK6()
    {
        Order order = await SeedAsync(OrderStatus.Preparing, PaymentStatus.Paid, ProductType.Physical);

        OrderCorrectionResult result = await RunAsync(c => c.CorrectShipmentAsync(new CorrectShipmentRequest(order.Id, AdminCaller, new ShipmentInfo(null, null, null, true), order.Version), Ct));

        Assert.Equal(TransitionRefusal.ConditionNotMet, result.Refusal);
    }

    // ---- KK7, KK8: the delivery date ----

    [Fact]
    public async Task TheDeliveryDate_IsNotInTheFuture_NotBeforeTheShipment_NorAfterAReturnsArrival_KK7()
    {
        (Order order, Guid item) = await DeliveredAsync(shippedDaysAgo: 5, deliveredDaysAgo: 3);
        DateOnly today = TurkeyTime.DateOf(Start);
        await SeedReturnAsync(item, declarationDaysAgo: 2, arrivalDaysAgo: 1);

        OrderCorrectionResult future = await RunAsync(c => c.CorrectDeliveryDateAsync(DateRequest(order, today.AddDays(1)), Ct));
        OrderCorrectionResult early = await RunAsync(c => c.CorrectDeliveryDateAsync(DateRequest(order, today.AddDays(-6)), Ct));
        OrderCorrectionResult late = await RunAsync(c => c.CorrectDeliveryDateAsync(DateRequest(order, today), Ct));

        Assert.Equal((TransitionRefusal.InvalidInput, DeliveryDateObstacle.InTheFuture), (future.Refusal!.Value, future.DateObstacle!.Value));
        Assert.Equal((TransitionRefusal.InvalidInput, DeliveryDateObstacle.BeforeShipment), (early.Refusal!.Value, early.DateObstacle!.Value));
        Assert.Equal((TransitionRefusal.InvalidInput, DeliveryDateObstacle.AfterReturnArrival), (late.Refusal!.Value, late.DateObstacle!.Value));
        Assert.Equal(today.AddDays(-3), (await ItemAsync(item)).DeliveryDate);
    }

    [Fact]
    public async Task TheDeliveryDate_IsCorrectedForTheWholeOrder_WithoutANotice_AndTheTrailKeepsTheOldAndTheNewDate_KK7_KK9()
    {
        (Order order, Guid item) = await DeliveredAsync(shippedDaysAgo: 6, deliveredDaysAgo: 4);
        DateOnly today = TurkeyTime.DateOf(Start);
        int dispatches = (await DispatchesAsync(order.Id)).Count;

        OrderCorrectionResult result = await RunAsync(c => c.CorrectDeliveryDateAsync(DateRequest(order, today.AddDays(-2)), Ct));

        Assert.Null(result.Refusal);
        Assert.Equal(today.AddDays(-2), (await ItemAsync(item)).DeliveryDate);
        Assert.Equal(dispatches, (await DispatchesAsync(order.Id)).Count);
        await using AppDbContext reader = NewContext();
        AuditLog row = await reader.AuditLogs.AsNoTracking().SingleAsync(a => a.TargetRecordId == order.Id, Ct);
        Assert.Equal(nameof(AuditOperation.CorrectDeliveryDate), row.Operation);
        AuditValueChange change = Assert.Single(row.Details!.Changes);
        Assert.Equal(("DeliveryDate", today.AddDays(-4).ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture), today.AddDays(-2).ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture)), (change.Field, change.OldValue, change.NewValue));
    }

    [Fact]
    public async Task OnADeclarationsDay_TheCorrectionAsksWhichCameFirst_KeepsTheDeclaration_AndWritesTheAnswer_KK8()
    {
        (Order order, Guid item) = await DeliveredAsync(shippedDaysAgo: 6, deliveredDaysAgo: 4);
        DateOnly today = TurkeyTime.DateOf(Start);
        Guid withdrawal = await SeedWithdrawalAsync(item, declarationDaysAgo: 2);

        OrderCorrectionResult unanswered = await RunAsync(c => c.CorrectDeliveryDateAsync(DateRequest(order, today.AddDays(-2)), Ct));
        Assert.Equal(TransitionRefusal.InvalidInput, unanswered.Refusal);
        Assert.True(unanswered.AsksDeliveryOrder);
        Assert.Equal(today.AddDays(-4), (await ItemAsync(item)).DeliveryDate);

        OrderCorrectionResult answered = await RunAsync(c => c.CorrectDeliveryDateAsync(DateRequest(order, today.AddDays(-2)) with { IsDeliveredBeforeDeclaration = true }, Ct));

        Assert.Null(answered.Refusal);
        await using AppDbContext reader = NewContext();
        Withdrawal saved = await reader.Withdrawals.AsNoTracking().SingleAsync(w => w.Id == withdrawal, Ct);
        Assert.Equal((true, today.AddDays(-2)), (saved.IsDeliveredBeforeDeclaration, (await ItemAsync(item)).DeliveryDate!.Value));
    }

    [Fact]
    public async Task TheWithdrawalWindow_RunsFromTheCorrectedDate_KK8()
    {
        // Delivered 20 days ago: the 14-day window is over; the corrected date 3 days ago opens it again (02 10.4.5).
        (Order order, Guid item) = await DeliveredAsync(shippedDaysAgo: 21, deliveredDaysAgo: 20);
        DateOnly today = TurkeyTime.DateOf(Start);
        Assert.True(AfterSalesPeriodRules.WithdrawalWindowEnd((await ItemAsync(item)).DeliveryDate!.Value) <= Start);

        OrderCorrectionResult result = await RunAsync(c => c.CorrectDeliveryDateAsync(DateRequest(order, today.AddDays(-3)), Ct));

        // The window and the defect period are read from the item's delivery date (AfterSalesPeriodRules): the corrected date is the one they run from.
        Assert.Null(result.Refusal);
        Assert.True(AfterSalesPeriodRules.WithdrawalWindowEnd((await ItemAsync(item)).DeliveryDate!.Value) > Start);
    }

    // ---- KK9: the return's arrival date ----

    [Fact]
    public async Task TheArrivalDate_IsCorrectedInPlace_NotIntoTheFuture_NorBeforeTheDelivery_WithoutANotice_KK9()
    {
        (Order order, Guid item) = await DeliveredAsync(shippedDaysAgo: 8, deliveredDaysAgo: 6);
        DateOnly today = TurkeyTime.DateOf(Start);
        Guid receipt = await SeedReturnAsync(item, declarationDaysAgo: 4, arrivalDaysAgo: 1);
        int dispatches = (await DispatchesAsync(order.Id)).Count;

        OrderCorrectionResult future = await RunAsync(c => c.CorrectReturnArrivalDateAsync(ArrivalRequest(order, item, receipt, today.AddDays(1)), Ct));
        OrderCorrectionResult early = await RunAsync(c => c.CorrectReturnArrivalDateAsync(ArrivalRequest(order, item, receipt, today.AddDays(-7)), Ct));
        OrderCorrectionResult result = await RunAsync(c => c.CorrectReturnArrivalDateAsync(ArrivalRequest(order, item, receipt, today.AddDays(-3)), Ct));

        Assert.Equal(TransitionRefusal.InvalidInput, future.Refusal);
        Assert.Equal(TransitionRefusal.InvalidInput, early.Refusal);
        Assert.Null(result.Refusal);
        await using AppDbContext reader = NewContext();
        DateOnly? arrival = (await reader.Returns.AsNoTracking().SingleAsync(r => r.Id == receipt, Ct)).ArrivalDate;
        Assert.Equal(today.AddDays(-3), arrival);

        // The refund's fourteen days run from the corrected date (02 10.4.5; Z-16): the start is read from the stored arrival, never stored itself (K-712).
        DateOnly declaration = today.AddDays(-4);
        Assert.Equal(today.AddDays(-1), AfterSalesPeriodRules.WithdrawalRefundStartDay(true, declaration, today.AddDays(-1)));
        Assert.Equal(today.AddDays(-3), AfterSalesPeriodRules.WithdrawalRefundStartDay(true, declaration, arrival));
        Assert.Equal(dispatches, (await DispatchesAsync(order.Id)).Count);
        AuditLog row = await reader.AuditLogs.AsNoTracking().SingleAsync(a => a.TargetRecordId == order.Id, Ct);
        Assert.Equal(nameof(AuditOperation.CorrectReturnArrivalDate), row.Operation);
        Assert.Equal("ReturnArrivalDate", Assert.Single(row.Details!.Changes).Field);
    }

    // ---- KK10: the internal note ----

    [Fact]
    public async Task TheInternalNote_IsWrittenChangedAndEmptied_ReachesNoCustomerOutput_AndItsTrailHasNoText_KK10()
    {
        Order order = await SeedAsync(OrderStatus.Preparing, PaymentStatus.Paid, ProductType.Physical);
        int dispatches = (await DispatchesAsync(order.Id)).Count;

        SaveInternalNoteResult written = await NoteAsync(order.Id, "Müşteri aradı, gizli not", null);
        SaveInternalNoteResult changed = await NoteAsync(order.Id, "İkinci gizli not", "Müşteri aradı, gizli not");
        SaveInternalNoteResult same = await NoteAsync(order.Id, "İkinci gizli not", "İkinci gizli not");

        Assert.True(written.IsSaved);
        Assert.True(changed.IsSaved);
        Assert.False(same.IsSaved);
        Assert.Null(same.Code);
        await using (AppDbContext reader = NewContext())
        {
            Assert.Equal("İkinci gizli not", (await reader.InternalNotes.AsNoTracking().SingleAsync(n => n.OrderId == order.Id, Ct)).Text);
            List<AuditLog> rows = await reader.AuditLogs.AsNoTracking().Where(a => a.TargetRecordId == order.Id).ToListAsync(Ct);
            Assert.Equal(2, rows.Count);
            Assert.All(rows, r => Assert.Equal(nameof(AuditOperation.SaveInternalNote), r.Operation));
            Assert.All(rows, r => Assert.Empty(r.Details!.Changes));
            Assert.All(rows, r => Assert.DoesNotContain(r.Details!.Values, v => (v.Value ?? string.Empty).Contains("gizli", StringComparison.Ordinal)));
            List<DispatchRecord> sent = await reader.DispatchRecords.AsNoTracking().Where(d => d.OrderId == order.Id).ToListAsync(Ct);
            Assert.Equal(dispatches, sent.Count);
        }

        // The note does not raise the order's version (09 10.9).
        Assert.Equal(order.Version, (await ReloadAsync(order.Id)).Version);

        // (f) An emptied note is deleted (K-1051); the version still does not move (g).
        SaveInternalNoteResult emptied = await NoteAsync(order.Id, "  ", "İkinci gizli not");
        Assert.True(emptied.IsSaved);
        Assert.Equal(0, await CountAsync(c => c.InternalNotes.Where(n => n.OrderId == order.Id)));
        Assert.Equal(order.Version, (await ReloadAsync(order.Id)).Version);
    }

    [Fact]
    public async Task ADisposedOrder_TakesNoInternalNote_KK10()
    {
        Order order = await SeedAsync(OrderStatus.Delivered, PaymentStatus.Paid, ProductType.Digital);
        await ExecuteAsync(
            "UPDATE orders SET contact_email = NULL, normalized_contact_email = NULL, order_access_key = NULL, ip_address = NULL, cart_id = NULL, customer_id = NULL, "
            + "billing_recipient_name = NULL, billing_province_id = NULL, billing_district = NULL, billing_address_line = NULL, shipping_recipient_name = NULL, shipping_phone = NULL, "
            + $"shipping_province_id = NULL, shipping_district = NULL, shipping_address_line = NULL WHERE id = '{order.Id}'");

        SaveInternalNoteResult result = await NoteAsync(order.Id, "not", null);
        SaveInternalNoteResult missing = await NoteAsync(Guid.NewGuid(), "not", null);

        Assert.Equal(ErrorCode.OperationNotAvailable, result.Code);
        Assert.Equal(ErrorCode.OperationNotAvailable, missing.Code);
        Assert.Equal(0, await CountAsync(c => c.InternalNotes.Where(n => n.OrderId == order.Id)));
    }

    // ---- helpers ----

    private RemoveItemRequest Removal(Order order, Guid item, int quantity) => new(order.Id, item, AdminCaller, order.Version, quantity, true);

    private CorrectDeliveryDateRequest DateRequest(Order order, DateOnly date) => new(order.Id, AdminCaller, date, null, ShownVersionOf(order.Id));

    private CorrectReturnArrivalDateRequest ArrivalRequest(Order order, Guid item, Guid record, DateOnly date) => new(order.Id, item, record, AdminCaller, date, ShownVersionOf(order.Id));

    private async Task<Guid> ItemIdAsync(Guid orderId, ProductType type)
    {
        await using AppDbContext reader = NewContext();
        return await reader.OrderItems.AsNoTracking().Where(i => i.OrderId == orderId && i.ProductType == type).Select(i => i.Id).FirstAsync(Ct);
    }

    private async Task<(int? Stock, int CouponUsed)> CountersAsync(Guid variantId, Guid couponId)
    {
        await using AppDbContext reader = NewContext();
        int? stock = await reader.Variants.Where(v => v.Id == variantId).Select(v => v.StockQuantity).SingleAsync(Ct);
        int used = await reader.Coupons.Where(c => c.Id == couponId).Select(c => c.UsedCount).SingleAsync(Ct);
        return (stock, used);
    }

    private async Task<OrderItem> ItemAsync(Guid itemId)
    {
        await using AppDbContext reader = NewContext();
        return await reader.OrderItems.AsNoTracking().SingleAsync(i => i.Id == itemId, Ct);
    }

    /// <summary>A delivered order with one physical item: its S5 record a number of days ago and its delivery date a number of days ago.</summary>
    private async Task<(Order Order, Guid Item)> DeliveredAsync(int shippedDaysAgo, int deliveredDaysAgo)
    {
        Order order = await SeedAsync(OrderStatus.Delivered, PaymentStatus.Paid, ProductType.Physical);
        Guid item = await ItemIdAsync(order.Id, ProductType.Physical);
        DateOnly today = TurkeyTime.DateOf(Start);
        await SaveAsync(c => c.TransitionRecords.Add(
            TransitionRecord.Create(order.Id, TransitionCode.S5, ActorKind.Admin, _admin!.Id, _admin.Name, null, null, null, null, null, Start.AddDays(-shippedDaysAgo))));
        await ExecuteAsync(
            $"UPDATE order_items SET delivery_marked_at = '{Start.AddDays(-deliveredDaysAgo):O}', delivery_date = '{today.AddDays(-deliveredDaysAgo):yyyy-MM-dd}' WHERE id = '{item}'");
        return (await ReloadAsync(order.Id), item);
    }

    private async Task<Guid> SeedWithdrawalAsync(Guid item, int declarationDaysAgo)
    {
        Withdrawal withdrawal = Withdrawal.ByBuyer(item, 1, TurkeyTime.DateOf(Start).AddDays(-declarationDaysAgo), null, new ItemAmounts(300m, 10m, 50m), null, Start.AddDays(-declarationDaysAgo));
        await SaveAsync(c => c.Withdrawals.Add(withdrawal));
        return withdrawal.Id;
    }

    private async Task<Guid> SeedReturnAsync(Guid item, int declarationDaysAgo, int arrivalDaysAgo)
    {
        Guid withdrawal = await SeedWithdrawalAsync(item, declarationDaysAgo);
        Return receipt = Return.ForWithdrawal(item, withdrawal, 1, TurkeyTime.DateOf(Start).AddDays(-arrivalDaysAgo), _admin!.Id, _admin.Name, Start.AddDays(-arrivalDaysAgo));
        await SaveAsync(c => c.Returns.Add(receipt));
        return receipt.Id;
    }
}
