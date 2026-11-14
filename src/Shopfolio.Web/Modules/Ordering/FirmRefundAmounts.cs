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
    /// <summary>The amount and the fee flag of the record's refund now.</summary>
    public static async Task<(decimal Amount, bool IsShippingFeeIncluded)> OfAsync(
        AppDbContext db, IAfterSalesModule afterSales, Order order, OwedRefundRecord record, IReadOnlyList<PanelRefundFacts> refunds, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(order);
        ArgumentNullException.ThrowIfNull(record);
        ArgumentNullException.ThrowIfNull(refunds);
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
