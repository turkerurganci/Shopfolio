using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Modules.AfterSales.Contracts;
using Shopfolio.Modules.Payment.Contracts;

namespace Shopfolio.Modules.Ordering;

/// <summary>
/// The amount of an item record's refund the firm processes (T529; 07 UC-131; 02 7.2.9, 10.4.9; 06 4.28, 4.33; K-1105): the record's product amount less its coupon share (the AfterSales module's
/// <see cref="IAfterSalesModule.RefundAmountAsync"/>), the shipping fee when the fee rule includes it, and - on a withdrawal's goods receipt - less what is left of the amount based refunds bound to the
/// item's "mal dönmedi" closures, deducted from the receipts after them in order until it is used up. The one home of the amount: the screen's line and the POST read it; the page works out none.
/// </summary>
internal static class FirmRefundAmounts
{
    /// <summary>
    /// The amount and the fee flag of the record's refund now. A defect claim's goods receipt (K-1629 (3)) pays the rescission's 02 7.1.7 amount of its units, at most what the item's refunds
    /// left (D-893: <see cref="ItemRefundLeftAsync"/> without the receipt's own owed amount), and carries no shipping fee - the amount the rescission writer pays.
    /// </summary>
    public static async Task<(decimal Amount, bool IsShippingFeeIncluded)> OfAsync(
        AppDbContext db, IAfterSalesModule afterSales, IPaymentModule payment, Order order, OwedRefundRecord record, IReadOnlyList<PanelRefundFacts> refunds, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(db);
        ArgumentNullException.ThrowIfNull(record);
        if (record.Type == RefundType.DefectClaimRescission)
        {
            OrderItem item = await db.OrderItems.AsNoTracking().SingleAsync(i => i.Id == record.OrderItemId, ct);
            decimal gross = (await RescissionAsync(afterSales, order, item, record.Quantity, ct)).Amount;
            return (Math.Min(gross, await ItemRefundLeftAsync(db, afterSales, payment, order, item, record.CauseId, ct)), false);
        }

        return await UncappedAsync(db, afterSales, order, record, refunds, ct);
    }

    /// <summary>
    /// What the item's refunds may still pay back (D-893; 12 P5.5): the item's paid amount - its frozen unit price times its units less its coupon share (02 7.1.7) - less its refunds that are
    /// not voided, started or sent, and less the refunds its records owe but have not born yet (D-906), each without the shipping fee it carries. <paramref name="exceptCause"/> is the owed record
    /// being paid, whose own amount is not deducted. Never below zero. The one home of the cap: the rescission writer and the order screen's line read it.
    /// </summary>
    public static async Task<decimal> ItemRefundLeftAsync(
        AppDbContext db, IAfterSalesModule afterSales, IPaymentModule payment, Order order, OrderItem item, Guid? exceptCause, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(afterSales);
        ArgumentNullException.ThrowIfNull(payment);
        ArgumentNullException.ThrowIfNull(order);
        ArgumentNullException.ThrowIfNull(item);
        decimal paid = ((item.DiscountedUnitPrice ?? item.UnitPrice) * item.Quantity) - item.CouponShare;
        decimal committed = await payment.CommittedItemRefundTotalAsync(item.Id, order.ShippingFee, ct);
        List<OwedRefundRecord> owed = [.. (await afterSales.ListOwedRefundRecordsAsync([item.Id], ct)).Where(r => exceptCause is not { } except || r.CauseId != except)];
        decimal owedTotal = 0m;
        if (owed.Count > 0)
        {
            IReadOnlyList<PanelRefundFacts> refunds = await payment.ListPanelRefundsAsync(order.Id, ct);
            foreach (OwedRefundRecord record in owed)
            {
                // The other owed records count at their own amount before any cap: a cap that read the others' caps would read itself.
                (decimal amount, bool fee) = await UncappedAsync(db, afterSales, order, record, refunds, ct);
                owedTotal += amount - (fee ? order.ShippingFee : 0m);
            }
        }

        return Math.Max(0m, paid - committed - owedTotal);
    }

    /// <summary>The rescission's split of <paramref name="quantity"/> units of the item now (02 7.1.7): the After-sales module's one split, from the item's frozen values.</summary>
    public static Task<RefundAmount> RescissionAsync(IAfterSalesModule afterSales, Order order, OrderItem item, int quantity, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(afterSales);
        ArgumentNullException.ThrowIfNull(item);
        ItemFrozenAmounts frozen = new(item.DiscountedUnitPrice ?? item.UnitPrice, item.CouponShare, item.VatAmount);
        return afterSales.DefectClaimRescissionAmountAsync(new DefectClaimRescissionEntry(OrderTransitions.Subject(order, item), quantity, frozen), ct);
    }

    private static async Task<(decimal Amount, bool IsShippingFeeIncluded)> UncappedAsync(
        AppDbContext db, IAfterSalesModule afterSales, Order order, OwedRefundRecord record, IReadOnlyList<PanelRefundFacts> refunds, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(order);
        ArgumentNullException.ThrowIfNull(refunds);
        if (record.Type == RefundType.DefectClaimRescission)
        {
            OrderItem item = await db.OrderItems.AsNoTracking().SingleAsync(i => i.Id == record.OrderItemId, ct);
            return ((await RescissionAsync(afterSales, order, item, record.Quantity, ct)).Amount, false);
        }

        RefundSubject subject = new(record.Type, record.RecordId, record.ReturnId);
        RefundAmount gross = await afterSales.RefundAmountAsync(subject, ct)
            ?? throw new InvalidOperationException("The owed record is read from the same context.");
        bool fee = await IsShippingFeeIncludedAsync(db, afterSales, order.Id, subject, ct);
        decimal deduction = 0m;
        if (record.ReturnId is not null)
        {
            deduction = await ReceiptDeductionAsync(afterSales, order, record.OrderItemId, gross.Amount, refunds, ct);
        }

        return (gross.Amount - deduction + (fee ? order.ShippingFee : 0m), fee);
    }

    /// <summary>The shipping fee rule's question for the record (02 7.2.9, 7.4.6): the order's physical items and the moment the carrier first took the order.</summary>
    public static async Task<bool> IsShippingFeeIncludedAsync(AppDbContext db, IAfterSalesModule afterSales, Guid orderId, RefundSubject subject, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(afterSales);
        List<ShippedItem> physical = await OrderPeriods.ShippedItemsAsync(db, orderId, ct);
        if (physical.Count == 0)
        {
            return false;
        }

        DateTimeOffset? handed = await OrderPeriods.HandedToCarrierAtAsync(db, orderId, ct);
        return await afterSales.IsShippingFeeIncludedAsync(new ShippingFeeQuery(physical, handed, subject), ct);
    }

    /// <summary>
    /// K-1105: the amount based refunds bound to the item's "mal dönmedi" closures are deducted from the receipts' refunds after them, in order, until used up. What the earlier receipts already bore is
    /// their product amount less what they paid (the fee they carried aside).
    /// </summary>
    private static async Task<decimal> ReceiptDeductionAsync(
        IAfterSalesModule afterSales, Order order, Guid orderItemId, decimal gross, IReadOnlyList<PanelRefundFacts> refunds, CancellationToken ct)
    {
        List<PanelRefundFacts> bound = [.. refunds.Where(r => !r.IsVoided && r.OrderItemId == orderItemId && r.NotReturnedClosureId is not null)];
        if (bound.Count == 0)
        {
            return 0m;
        }

        DateTimeOffset from = bound.Min(r => r.CreatedAt);
        decimal earlierGross = 0m;
        decimal earlierPaid = 0m;
        foreach (PanelRefundFacts receipt in refunds.Where(r => !r.IsVoided && r.OrderItemId == orderItemId && r.ReturnId is not null && r.WithdrawalId is not null && r.CreatedAt > from))
        {
            RefundAmount? amount = await afterSales.RefundAmountAsync(new RefundSubject(RefundType.Withdrawal, receipt.WithdrawalId!.Value, receipt.ReturnId), ct); // non-null: the Where keeps receipts with both links
            earlierGross += amount?.Amount ?? 0m;
            earlierPaid += receipt.Amount - (receipt.IsShippingFeeIncluded ? order.ShippingFee : 0m);
        }

        return NotReturnedDeduction.Of(bound.Sum(r => r.Amount), earlierGross, earlierPaid, gross);
    }
}

/// <summary>The deduction's arithmetic (K-1105; 02 10.4.9): pure, so its edges - nothing bound, partly used, used up, larger than the receipt - are unit tested.</summary>
internal static class NotReturnedDeduction
{
    /// <summary>What this receipt's refund bears of the bound total: what is left after the earlier receipts bore theirs, at most the receipt's own product amount.</summary>
    public static decimal Of(decimal boundTotal, decimal earlierGross, decimal earlierPaid, decimal gross)
    {
        decimal used = Math.Max(0m, earlierGross - earlierPaid);
        decimal left = Math.Max(0m, boundTotal - used);
        return Math.Min(left, Math.Max(0m, gross));
    }
}
