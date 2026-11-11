using Shopfolio.Infrastructure;
using Shopfolio.Modules.AfterSales.Contracts;
using Shopfolio.Modules.Notifications.Contracts;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Modules.Payment.Contracts;

namespace Shopfolio.Modules.Ordering;

/// <summary>
/// The money remedies of a defect claim (T374; 02 7.5.3; 03 2.9.4, 8.4.10): the system never runs the remedy, but when the firm's remedy needs money the rescission pays
/// the units taken back through the refund line with the amounts of 02 7.1.7, and the price reduction pays an amount back without the item coming back (02 5.5). Each
/// works in the unit's order (09 9.1 item 4): the order's row lock and the screen's version, the paid order, the AfterSales check of the open claim, the payment module's
/// refund (started; the firm processes it on its line - 03 1.11.31), the IBAN request on the bank transfer line, the order's version raised once, then B-14.
/// </summary>
internal sealed partial class OrderTransitions
{
    public async Task<ItemRecordResult> RescindDefectClaimAsync(RescindDefectClaimRequest request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (!TryAdminActor(request.Admin, out _))
        {
            return ItemRefused(TransitionRefusal.ActorNotAllowed);
        }

        (Order? order, OrderItem? item, TransitionRefusal? refusal) = await LockItemAsync(request.OrderId, request.OrderItemId, request.ExpectedVersion, ct);
        if (refusal is { } locked)
        {
            return ItemRefused(locked);
        }

        if (!IsMoneyTaken(order!)) // non-null: LockItemAsync returns the order and the item when it returns no refusal
        {
            return ItemRefused(TransitionRefusal.ConditionNotMet);
        }

        ItemRecordSubject subject = Subject(order!, item!); // non-null: LockItemAsync returns the order and the item when it returns no refusal
        ItemFrozenAmounts frozen = new(item!.DiscountedUnitPrice ?? item.UnitPrice, item.CouponShare, item.VatAmount); // non-null: LockItemAsync returns the order and the item when it returns no refusal
        DefectRemedyCheck check = await _afterSales.CheckDefectClaimRescissionAsync(new DefectClaimRescissionEntry(subject, request.Quantity, frozen), ct);
        if (check.Refusal is { } refused)
        {
            return ItemRefused(Map(refused));
        }

        // The item's refunds never pass its paid amount (D-893): after a price reduction the rescission pays at most what the item's started and sent refunds left.
        // A rescission with nothing to pay back (the coupon took the whole price, or the earlier refunds did) has no refund to carry its units (06 4.33: the amount
        // is above zero; D-881).
        RefundAmount amount = check.Amount ?? throw new InvalidOperationException("A rescission check without a refusal carries the amount.");
        decimal payable = Math.Min(amount.Amount, await ItemRefundLeftAsync(order!, item, ct)); // non-null: LockItemAsync returns the order and the item when it returns no refusal
        if (payable <= 0m)
        {
            return ItemRefused(TransitionRefusal.ConditionNotMet);
        }

        DefectRescissionShares shares = new(check.ClaimId ?? throw new InvalidOperationException("A rescission check without a refusal carries the claim."), amount.Quantity, amount.CouponShare, amount.VatAmount);
        return await StartRemedyRefundAsync(order!, subject, request.Admin, payable, shares, ct); // non-null: LockItemAsync returns the order and the item when it returns no refusal
    }

    public async Task<ItemRecordResult> ReduceDefectClaimPriceAsync(ReduceDefectClaimPriceRequest request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (!TryAdminActor(request.Admin, out _))
        {
            return ItemRefused(TransitionRefusal.ActorNotAllowed);
        }

        if (request.Amount <= 0m || request.Amount != Money.Round(request.Amount))
        {
            return ItemRefused(TransitionRefusal.InvalidInput);
        }

        (Order? order, OrderItem? item, TransitionRefusal? refusal) = await LockItemAsync(request.OrderId, request.OrderItemId, request.ExpectedVersion, ct);
        if (refusal is { } locked)
        {
            return ItemRefused(locked);
        }

        if (!IsMoneyTaken(order!)) // non-null: LockItemAsync returns the order and the item when it returns no refusal
        {
            return ItemRefused(TransitionRefusal.ConditionNotMet);
        }

        ItemRecordSubject subject = Subject(order!, item!); // non-null: LockItemAsync returns the order and the item when it returns no refusal
        DefectRemedyCheck check = await _afterSales.CheckDefectClaimPriceReductionAsync(subject, ct);
        if (check.Refusal is { } refused)
        {
            return ItemRefused(Map(refused));
        }

        // The amount-based refund pays back while paid money that is not refunded is left (03 1.11.31): the refunds already started count, so two never pass the total;
        // and the item's refunds never pass the item's paid amount (D-893).
        decimal left = Math.Min(order!.Total - await _payment.CommittedRefundTotalAsync(order.Id, ct), await ItemRefundLeftAsync(order, item!, ct)); // non-null: LockItemAsync returns the order and the item when it returns no refusal
        if (request.Amount > left)
        {
            return ItemRefused(TransitionRefusal.InvalidInput);
        }

        return await StartRemedyRefundAsync(order, subject, request.Admin, request.Amount, null, ct);
    }

    /// <summary>
    /// What the item's refunds may still pay back (D-893; 12 P5.5): the item's paid amount - its frozen unit price times its units less its coupon share (02 7.1.7) - less
    /// its refunds that are not voided, started or sent, without the shipping fee one of them carries. Never below zero.
    /// </summary>
    private async Task<decimal> ItemRefundLeftAsync(Order order, OrderItem item, CancellationToken ct)
    {
        decimal paid = ((item.DiscountedUnitPrice ?? item.UnitPrice) * item.Quantity) - item.CouponShare;
        return Math.Max(0m, paid - await _payment.CommittedItemRefundTotalAsync(item.Id, order.ShippingFee, ct));
    }

    /// <summary>The money was taken and not all paid back (02 5.5): Ödendi or Kısmen geri ödendi.</summary>
    private static bool IsMoneyTaken(Order order) => order.PaymentStatus is PaymentStatus.Paid or PaymentStatus.PartiallyRefunded;

    /// <summary>
    /// The refund of the remedy on the order's payment way, then - on the bank transfer line without an IBAN on file or an open request - the item's IBAN request and
    /// B-14 (02 7.5.3, 7.4.5; K-686). The order's version is raised once.
    /// </summary>
    private async Task<ItemRecordResult> StartRemedyRefundAsync(
        Order order, ItemRecordSubject subject, TransitionAdmin admin, decimal amount, DefectRescissionShares? rescission, CancellationToken ct)
    {
        string adminName = admin.AdminName.Trim();
        Guid refundId = await _payment.StartDefectRemedyRefundAsync(
            new DefectRemedyRefundStart(order.Id, order.PaymentMethod, subject.OrderItemId, amount, admin.AdminId, adminName, rescission), ct);

        bool ibanRequested = order.PaymentMethod == PaymentMethod.BankTransfer
            && await _afterSales.RequestDefectRemedyIbanAsync(
                new DefectRemedyIbanRequest(subject, new ItemRecordAdmin(admin.AdminId, adminName), _time.GetUtcNow()), ct);

        RaiseVersionOnce(order);
        if (ibanRequested)
        {
            await QueueNoticeAsync(order.Id, new RefundIbanRequestedNotice(order.Id, RefundIbanRequestReason.AdminRequest), ct);
        }

        return new ItemRecordResult(null, refundId);
    }
}
