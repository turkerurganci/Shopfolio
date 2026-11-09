using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Modules.AfterSales.Contracts;

namespace Shopfolio.Modules.AfterSales;

/// <summary>
/// The customer's cancellation record (T367; 06 4.22; 02 7.2.2, 7.1.7): the writer and the link to the transition that closed the order with it. The amounts are
/// set aside here, from the item's frozen values and from what the item's earlier closing records already set aside, so that the shares of all the item's records
/// add up to the item's own share to the kuruş (<see cref="ItemShares"/>).
/// </summary>
internal sealed partial class AfterSalesModule
{
    public async Task<ItemRecordWriteResult> AddCancellationAsync(CustomerCancellationEntry entry, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(entry);
        ArgumentNullException.ThrowIfNull(entry.Frozen);
        ItemRecordSubject subject = entry.Subject;
        RequireOrderLock(subject);
        int open = await OpenQuantityAsync(subject, ct);
        if (!ItemQuantities.IsSelectable(entry.Quantity, open))
        {
            return ItemRecordWriteResult.Rejected(ItemRecordRefusal.QuantityExceedsOpen);
        }

        (decimal couponSetAside, decimal vatSetAside) = await SetAsideAsync(subject.OrderItemId, ct);
        ItemAmounts amounts = ItemShares.SetAside(entry.Frozen, subject.ItemQuantity, entry.Quantity, entry.Quantity == open, couponSetAside, vatSetAside);
        Cancellation cancellation = Cancellation.ByBuyer(subject.OrderItemId, entry.Quantity, amounts, null, entry.At);
        _db.Cancellations.Add(cancellation);
        return new ItemRecordWriteResult(null, cancellation.Id, null);
    }

    public async Task LinkClosingTransitionAsync(Guid cancellationId, Guid transitionRecordId, CancellationToken ct)
    {
        Cancellation cancellation = await _db.Cancellations.FindAsync([cancellationId], ct)
            ?? throw new InvalidOperationException("The cancellation to link is not in the caller's context.");
        if (cancellation.TransitionRecordId is not null)
        {
            throw new InvalidOperationException("The cancellation is already linked to the transition that closed the order.");
        }

        cancellation.TransitionRecordId = transitionRecordId;
    }

    /// <summary>
    /// The coupon share and the VAT the item's closing records set aside so far (02 7.1.7 (4)): its cancellations, removals, late delivery terminations and withdrawals,
    /// saved or added in the caller's context. A cancellation undone by the correction of its closing transition gave its units back and sets nothing aside - the same
    /// rule as the closed quantity (<see cref="ClosedQuantitiesAsync"/>).
    /// </summary>
    private async Task<(decimal Coupon, decimal Vat)> SetAsideAsync(Guid orderItemId, CancellationToken ct)
    {
        List<SetAsideShare> shares = [];
        List<Cancellation> cancellations = await _db.Cancellations.Where(c => c.OrderItemId == orderItemId).ToListAsync(ct);
        Guid[] closing = [.. cancellations.Select(c => c.TransitionRecordId).OfType<Guid>().Distinct()];
        IReadOnlySet<Guid> corrected = closing.Length == 0 ? new HashSet<Guid>() : await _ordering.FindCorrectedTransitionRecordsAsync(closing, ct);
        shares.AddRange(cancellations
            .Where(c => c.TransitionRecordId is not { } closedBy || !corrected.Contains(closedBy))
            .Select(c => new SetAsideShare(c.CouponShare, c.VatAmount)));
        shares.AddRange(await _db.ItemRemovals.Where(r => r.OrderItemId == orderItemId).Select(r => new SetAsideShare(r.CouponShare, r.VatAmount)).ToListAsync(ct));
        shares.AddRange(await _db.LateDeliveryTerminations.Where(t => t.OrderItemId == orderItemId).Select(t => new SetAsideShare(t.CouponShare, t.VatAmount)).ToListAsync(ct));
        shares.AddRange(await _db.Withdrawals.Where(w => w.OrderItemId == orderItemId).Select(w => new SetAsideShare(w.CouponShare, w.VatAmount)).ToListAsync(ct));

        // The queries above read the saved rows; the rows the caller added a moment ago (an unsaved cancellation of the same operation included) are in the context only.
        shares.AddRange(AddedShares<ItemRemoval>(r => r.OrderItemId == orderItemId, r => new SetAsideShare(r.CouponShare, r.VatAmount)));
        shares.AddRange(AddedShares<LateDeliveryTermination>(t => t.OrderItemId == orderItemId, t => new SetAsideShare(t.CouponShare, t.VatAmount)));
        shares.AddRange(AddedShares<Withdrawal>(w => w.OrderItemId == orderItemId, w => new SetAsideShare(w.CouponShare, w.VatAmount)));
        shares.AddRange(AddedShares<Cancellation>(c => c.OrderItemId == orderItemId, c => new SetAsideShare(c.CouponShare, c.VatAmount)));
        return (shares.Sum(s => s.Coupon), shares.Sum(s => s.Vat));
    }

    private IEnumerable<SetAsideShare> AddedShares<TRecord>(Func<TRecord, bool> filter, Func<TRecord, SetAsideShare> shareOf)
        where TRecord : class
    {
        return [.. _db.ChangeTracker.Entries<TRecord>().Where(e => e.State == EntityState.Added && filter(e.Entity)).Select(e => shareOf(e.Entity))];
    }

    private sealed record SetAsideShare(decimal Coupon, decimal Vat);
}
