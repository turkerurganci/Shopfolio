using System.Linq.Expressions;

using Microsoft.EntityFrameworkCore;

using Shopfolio.Modules.AfterSales.Contracts;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Payment.Contracts;

namespace Shopfolio.Modules.AfterSales;

/// <summary>
/// The amounts a record's refund pays back and whether it carries the shipping fee (T369; 02 7.1.7, 7.2.9, 7.4.6; 06 4.33): plain reads over the item records, the
/// caller's unsaved ones included, that apply the two rule homes - <see cref="ItemShares"/> for the shares and <see cref="ShippingFeeRefunds"/> for the fee.
/// </summary>
internal sealed partial class AfterSalesModule
{
    public async Task<RefundAmount?> RefundAmountAsync(RefundSubject subject, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(subject);
        Guid id = subject.RecordId;
        return subject switch
        {
            { Type: RefundType.Cancellation } =>
                await _db.Cancellations.FindAsync([id], ct) is { } c ? new RefundAmount(c.Quantity, c.ProductAmount, c.CouponShare, c.VatAmount) : null,
            { Type: RefundType.ItemRemoval } =>
                await _db.ItemRemovals.FindAsync([id], ct) is { } r ? new RefundAmount(r.Quantity, r.ProductAmount, r.CouponShare, r.VatAmount) : null,
            { Type: RefundType.LateDeliveryTermination } =>
                await _db.LateDeliveryTerminations.FindAsync([id], ct) is { } t ? new RefundAmount(t.Quantity, t.ProductAmount, t.CouponShare, t.VatAmount) : null,
            { Type: RefundType.Withdrawal, ReturnId: { } returnId } => await ReceiptRefundAmountAsync(id, returnId, ct),
            { Type: RefundType.Withdrawal } =>
                await _db.Withdrawals.FindAsync([id], ct) is { } w ? new RefundAmount(w.Quantity, w.ProductAmount, w.CouponShare, w.VatAmount) : null,
            _ => throw new ArgumentOutOfRangeException(nameof(subject), subject.Type, "Not the refund of an item record this rule splits."),
        };
    }

    public async Task<bool> IsShippingFeeIncludedAsync(ShippingFeeQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);
        ArgumentNullException.ThrowIfNull(query.PhysicalItems);
        ArgumentNullException.ThrowIfNull(query.Subject);
        Dictionary<Guid, ShippedItem> items = query.PhysicalItems.GroupBy(i => i.OrderItemId).ToDictionary(g => g.Key, g => g.First());
        Guid[] ids = [.. items.Keys];
        if (ids.Length == 0)
        {
            return false;
        }

        DateTimeOffset? handed = query.HandedToCarrierAt;
        bool BeforeShipment(DateTimeOffset at) => handed is not { } carrier || at < carrier;

        List<Cancellation> cancellations = await StandingCancellationsAsync(ids, ct);
        List<ItemRemoval> removals = await LocalAsync(_db.ItemRemovals, r => ids.Contains(r.OrderItemId), ct);
        List<UnitMove> cancelledBefore =
        [
            .. cancellations.Where(c => BeforeShipment(c.CreatedAt)).Select(c => new UnitMove(c.OrderItemId, c.Id, c.CreatedAt, c.Quantity)),
            .. removals.Where(r => BeforeShipment(r.CreatedAt)).Select(r => new UnitMove(r.OrderItemId, r.Id, r.CreatedAt, r.Quantity)),
        ];
        Dictionary<Guid, int> physicalUnits = items.ToDictionary(i => i.Key, i => i.Value.Quantity);
        Guid subjectId = query.Subject.RecordId;

        switch (query.Subject.Type)
        {
            case RefundType.Cancellation:
                {
                    if (cancellations.SingleOrDefault(c => c.Id == subjectId) is not { } cancellation)
                    {
                        return false;
                    }

                    UnitMove move = new(cancellation.OrderItemId, cancellation.Id, cancellation.CreatedAt, cancellation.Quantity);
                    if (BeforeShipment(cancellation.CreatedAt))
                    {
                        return ShippingFeeRefunds.OnCancellationBeforeShipment(physicalUnits, cancelledBefore, move);
                    }

                    List<UnitMove> afterShipment = [.. cancellations.Where(c => !BeforeShipment(c.CreatedAt)).Select(c => new UnitMove(c.OrderItemId, c.Id, c.CreatedAt, c.Quantity))];
                    return ShippingFeeRefunds.OnCancellationAfterShipment(cancellation.CancellationReason, afterShipment, move);
                }

            case RefundType.ItemRemoval:
                {
                    // The administrator's removal follows the cancellation's trigger (02 7.2.9; 10.4.3); after the shipment it carries no fee.
                    return removals.SingleOrDefault(r => r.Id == subjectId) is { } removal && BeforeShipment(removal.CreatedAt)
                        && ShippingFeeRefunds.OnCancellationBeforeShipment(physicalUnits, cancelledBefore, new UnitMove(removal.OrderItemId, removal.Id, removal.CreatedAt, removal.Quantity));
                }

            case RefundType.LateDeliveryTermination:
                {
                    List<UnitMove> terminations = [.. (await LocalAsync(_db.LateDeliveryTerminations, t => ids.Contains(t.OrderItemId), ct))
                    .Select(t => new UnitMove(t.OrderItemId, t.Id, t.CreatedAt, t.Quantity))];
                    UnitMove? move = terminations.Where(t => t.RecordId == subjectId).Cast<UnitMove?>().SingleOrDefault();
                    return move is { } termination && ShippingFeeRefunds.OnTermination(terminations, termination);
                }

            case RefundType.Withdrawal:
                return handed is not null && await IsWithdrawalTriggerAsync(items, ids, cancelledBefore, query.Subject, ct);

            default:
                // The rescission, the amount-based and the late payment's refunds never carry the fee (06 4.33: only the four item record types may).
                return false;
        }
    }

    public async Task<IReadOnlyDictionary<Guid, int>?> WithdrawalFeeUnitsAsync(IReadOnlyCollection<ShippedItem> physicalItems, DateTimeOffset handedToCarrierAt, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(physicalItems);
        Dictionary<Guid, int> units = physicalItems.GroupBy(i => i.OrderItemId).ToDictionary(g => g.Key, g => g.First().Quantity);
        Guid[] ids = [.. units.Keys];
        if (ids.Length == 0)
        {
            return null;
        }

        // A unit rejected or closed as not come back stayed with the customer: no withdrawal brings the fee back any more (02 7.4.6; K-1076).
        int kept = (await LocalAsync(_db.ReturnRejections, r => ids.Contains(r.OrderItemId), ct)).Sum(r => r.Quantity)
            + (await LocalAsync(_db.NotReturnedClosures, n => ids.Contains(n.OrderItemId), ct)).Where(n => n.ReopenedAt == null).Sum(n => n.Quantity);
        if (kept > 0)
        {
            return null;
        }

        // The units the carrier took (IsWithdrawalTriggerAsync's count), less the ones every earlier declaration withdrew - the receipt after the delivery follows its declaration.
        List<(Guid ItemId, int Quantity)> moved =
        [
            .. (await StandingCancellationsAsync(ids, ct)).Where(c => c.CreatedAt < handedToCarrierAt).Select(c => (c.OrderItemId, c.Quantity)),
            .. (await LocalAsync(_db.ItemRemovals, r => ids.Contains(r.OrderItemId), ct)).Where(r => r.CreatedAt < handedToCarrierAt).Select(r => (r.OrderItemId, r.Quantity)),
            .. (await LocalAsync(_db.Withdrawals, w => ids.Contains(w.OrderItemId), ct)).Select(w => (w.OrderItemId, w.Quantity)),
        ];
        Dictionary<Guid, int> undeclared = units
            .Select(u => (u.Key, Units: u.Value - moved.Where(m => m.ItemId == u.Key).Sum(m => m.Quantity)))
            .Where(u => u.Units > 0)
            .ToDictionary(u => u.Key, u => u.Units);
        return undeclared.Count == 0 ? null : undeclared;
    }

    /// <summary>
    /// 02 7.4.6: the units the carrier took - each physical item's units less those cancelled or removed before it - all back with the firm, a declaration before the
    /// delivery counting at its moment and a receipt after it at its own; no unit rejected or closed as not come back.
    /// </summary>
    private async Task<bool> IsWithdrawalTriggerAsync(
        Dictionary<Guid, ShippedItem> items, Guid[] ids, List<UnitMove> cancelledBefore, RefundSubject subject, CancellationToken ct)
    {
        Dictionary<Guid, int> shipped = items.ToDictionary(
            i => i.Key, i => Math.Max(0, i.Value.Quantity - cancelledBefore.Where(m => m.OrderItemId == i.Key).Sum(m => m.Quantity)));
        List<Withdrawal> withdrawals = await LocalAsync(_db.Withdrawals, w => ids.Contains(w.OrderItemId), ct);
        List<Return> receipts = await LocalAsync(_db.Returns, r => ids.Contains(r.OrderItemId) && r.WithdrawalId != null, ct);
        int kept = (await LocalAsync(_db.ReturnRejections, r => ids.Contains(r.OrderItemId), ct)).Sum(r => r.Quantity)
            + (await LocalAsync(_db.NotReturnedClosures, n => ids.Contains(n.OrderItemId), ct)).Where(n => n.ReopenedAt == null).Sum(n => n.Quantity);

        bool AfterDelivery(Withdrawal w) =>
            AfterSalesPeriodRules.IsAfterDelivery(ProductType.Physical, items[w.OrderItemId].DeliveryDate, w.DeclarationDate, w.IsDeliveredBeforeDeclaration);

        HashSet<Guid> afterDelivery = [.. withdrawals.Where(AfterDelivery).Select(w => w.Id)];
        List<UnitMove> back =
        [
            .. withdrawals.Where(w => !afterDelivery.Contains(w.Id)).Select(w => new UnitMove(w.OrderItemId, w.Id, w.CreatedAt, w.Quantity)),
            .. receipts.Where(r => afterDelivery.Contains(r.WithdrawalId!.Value)).Select(r => new UnitMove(r.OrderItemId, r.Id, r.CreatedAt, r.Quantity)), // non-null: receipts holds only returns with a withdrawal (its query keeps WithdrawalId != null)
        ];

        // The withdrawal after the delivery is refunded per receipt (K-1057), the one before it at its declaration: the subject is the record that refunds.
        Guid refundedBy = subject.ReturnId ?? subject.RecordId;
        UnitMove? move = back.Where(m => m.RecordId == refundedBy).Cast<UnitMove?>().SingleOrDefault();
        bool ofTheWithdrawal = subject.ReturnId is not { } returnId
            ? withdrawals.Any(w => w.Id == subject.RecordId)
            : receipts.Any(r => r.Id == returnId && r.WithdrawalId == subject.RecordId);
        return ofTheWithdrawal && move is { } refunded && ShippingFeeRefunds.OnWithdrawal(shipped, back, kept, refunded);
    }

    /// <summary>A receipt's part of its withdrawal's amounts (02 7.1.7 (5)): the declaration's receipts in their order - the moment, then the uuid order of the database.</summary>
    private async Task<RefundAmount?> ReceiptRefundAmountAsync(Guid withdrawalId, Guid returnId, CancellationToken ct)
    {
        if (await _db.Returns.FindAsync([returnId], ct) is not { } receipt || receipt.WithdrawalId != withdrawalId
            || await _db.Withdrawals.FindAsync([withdrawalId], ct) is not { } withdrawal)
        {
            return null;
        }

        List<Return> parts = [.. (await LocalAsync(_db.Returns, r => r.WithdrawalId == withdrawalId, ct))
            .OrderBy(r => r.CreatedAt).ThenBy(r => r.Id.ToString("N"), StringComparer.Ordinal)];
        IReadOnlyList<ItemAmounts> amounts = ItemShares.SetAsideParts(
            new ItemAmounts(withdrawal.ProductAmount, withdrawal.CouponShare, withdrawal.VatAmount), withdrawal.Quantity, [.. parts.Select(p => p.Quantity)]);
        ItemAmounts part = amounts[parts.IndexOf(receipt)];
        return new RefundAmount(receipt.Quantity, part.ProductAmount, part.CouponShare, part.VatAmount);
    }

    /// <summary>The item cancellations that stand: one whose closing transition was corrected gave its units back (06 4.22; <see cref="AddSavedClosedAsync"/>'s rule).</summary>
    private async Task<List<Cancellation>> StandingCancellationsAsync(Guid[] ids, CancellationToken ct)
    {
        List<Cancellation> cancellations = await LocalAsync(_db.Cancellations, c => ids.Contains(c.OrderItemId), ct);
        Guid[] closing = [.. cancellations.Select(c => c.TransitionRecordId).OfType<Guid>().Distinct()];
        IReadOnlySet<Guid> corrected = closing.Length == 0 ? new HashSet<Guid>() : await _ordering.FindCorrectedTransitionRecordsAsync(closing, ct);
        return [.. cancellations.Where(c => c.TransitionRecordId is not { } closedBy || !corrected.Contains(closedBy))];
    }

    /// <summary>The saved rows loaded into the context with the ones the caller added: the context's local view answers both.</summary>
    private static async Task<List<TRecord>> LocalAsync<TRecord>(DbSet<TRecord> set, Expression<Func<TRecord, bool>> filter, CancellationToken ct)
        where TRecord : class
    {
        await set.Where(filter).LoadAsync(ct);
        Func<TRecord, bool> matches = filter.Compile();
        return [.. set.Local.Where(matches)];
    }
}
