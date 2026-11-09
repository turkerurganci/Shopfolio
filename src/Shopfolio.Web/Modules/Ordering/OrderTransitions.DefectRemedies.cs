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

        if (!IsMoneyTaken(order!))
        {
            return ItemRefused(TransitionRefusal.ConditionNotMet);
        }

        ItemRecordSubject subject = Subject(order!, item!);
        ItemFrozenAmounts frozen = new(item!.DiscountedUnitPrice ?? item.UnitPrice, item.CouponShare, item.VatAmount);
        DefectRemedyCheck check = await _afterSales.CheckDefectClaimRescissionAsync(new DefectClaimRescissionEntry(subject, request.Quantity, frozen), ct);
        if (check.Refusal is { } refused)
        {
            return ItemRefused(Map(refused));
        }

        // A rescission with nothing to pay back (the coupon took the whole price) has no refund to carry its units (06 4.33: the amount is above zero).
        RefundAmount amount = check.Amount!;
        if (amount.Amount <= 0m)
        {
            return ItemRefused(TransitionRefusal.ConditionNotMet);
        }

        DefectRescissionShares shares = new(check.ClaimId!.Value, amount.Quantity, amount.CouponShare, amount.VatAmount);
        return await StartRemedyRefundAsync(order!, subject, request.Admin, amount.Amount, shares, ct);
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

        if (!IsMoneyTaken(order!))
        {
            return ItemRefused(TransitionRefusal.ConditionNotMet);
        }

        ItemRecordSubject subject = Subject(order!, item!);
        DefectRemedyCheck check = await _afterSales.CheckDefectClaimPriceReductionAsync(subject, ct);
        if (check.Refusal is { } refused)
        {
            return ItemRefused(Map(refused));
        }

        // The amount-based refund pays back while paid money that is not refunded is left (03 1.11.31): the refunds already started count, so two never pass the total.
        decimal left = order!.Total - await _payment.CommittedRefundTotalAsync(order.Id, ct);
        if (request.Amount > left)
        {
            return ItemRefused(TransitionRefusal.InvalidInput);
        }

        return await StartRemedyRefundAsync(order, subject, request.Admin, request.Amount, null, ct);
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
