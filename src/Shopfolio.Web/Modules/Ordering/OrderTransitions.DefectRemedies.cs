using Shopfolio.Modules.AfterSales.Contracts;
using Shopfolio.Modules.Notifications.Contracts;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Modules.Payment.Contracts;

namespace Shopfolio.Modules.Ordering;

/// <summary>
/// The money remedy of a defect claim's rescission (T374; 02 7.5.3; 03 2.9.4, 8.4.10; K-808): the system never runs the remedy, but when the firm rescinds, the units taken back are paid
/// through the refund line with the amounts of 02 7.1.7 - UC-131 starts it on the claim's goods receipt (K-1629 (3)). The price reduction is UC-133's amount-based refund (03 8.3.3.7), not a
/// writer of its own (09 25). The rescission works in the unit's order (09 9.1 item 4): the order's row lock and the screen's version, the paid order, the AfterSales check of the open claim,
/// the payment module's refund (started), the IBAN request on the bank transfer line, the order's version raised once, then B-14.
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

        // The item's refunds never pass its paid amount (D-893): after a price reduction the rescission pays at most what the item's started and sent refunds left - the receipt it
        // pays is not its own deduction (K-1629 (3)). A rescission with nothing to pay back (the coupon took the whole price, or the earlier refunds did) has no refund to carry its
        // units (06 4.33: the amount is above zero; D-881).
        RefundAmount amount = check.Amount ?? throw new InvalidOperationException("A rescission check without a refusal carries the amount.");
        decimal left = await FirmRefundAmounts.ItemRefundLeftAsync(_db, _afterSales, _payment, order!, item, request.ReturnId, ct); // non-null: LockItemAsync returns the order and the item when it returns no refusal
        decimal payable = Math.Min(amount.Amount, left);
        if (payable <= 0m)
        {
            return ItemRefused(TransitionRefusal.ConditionNotMet);
        }

        DefectRescissionShares shares = new(check.ClaimId ?? throw new InvalidOperationException("A rescission check without a refusal carries the claim."), amount.Quantity, amount.CouponShare, amount.VatAmount)
        {
            ReturnId = request.ReturnId,
        };
        return await StartRemedyRefundAsync(order!, subject, request.Admin, payable, shares, ct); // non-null: LockItemAsync returns the order and the item when it returns no refusal
    }

    /// <summary>The money was taken and not all paid back (02 5.5): Ödendi or Kısmen geri ödendi.</summary>
    private static bool IsMoneyTaken(Order order) => order.PaymentStatus is PaymentStatus.Paid or PaymentStatus.PartiallyRefunded;

    /// <summary>
    /// The rescission's refund on the order's payment way, then - on the bank transfer line without an IBAN on file or an open request - the item's IBAN request and
    /// B-14 (02 7.5.3, 7.4.5; K-686). The order's version is raised once.
    /// </summary>
    private async Task<ItemRecordResult> StartRemedyRefundAsync(
        Order order, ItemRecordSubject subject, TransitionAdmin admin, decimal amount, DefectRescissionShares rescission, CancellationToken ct)
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
