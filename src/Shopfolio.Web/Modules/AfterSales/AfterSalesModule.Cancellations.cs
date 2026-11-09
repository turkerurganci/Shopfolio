using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Modules.AfterSales.Contracts;
using Shopfolio.Modules.Catalog.Contracts;

namespace Shopfolio.Modules.AfterSales;

/// <summary>
/// The cancellation records of the customer (T367) and of the firm (T368) and the late delivery termination record (T368; 06 4.22, 4.24; 02 7.2.2-7.2.4, 7.1.7): the
/// writers, the link to the transition that closed the order with a cancellation and the records' moments for the legal interest warning. The amounts are
/// set aside here, from the item's frozen values and from what the item's earlier closing records already set aside, so that the shares of all the item's records
/// add up to the item's own share to the kuruş (<see cref="ItemShares"/>).
/// </summary>
internal sealed partial class AfterSalesModule
{
    public async Task<ItemRecordWriteResult> AddCancellationAsync(CustomerCancellationEntry entry, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(entry);
        return await AddCancellationRowAsync(
            entry.Subject,
            entry.Quantity,
            entry.Frozen,
            amounts => Cancellation.ByBuyer(entry.Subject.OrderItemId, entry.Quantity, amounts, null, entry.At),
            ct);
    }

    public async Task<ItemRecordWriteResult> AddFirmCancellationAsync(FirmCancellationEntry entry, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(entry);
        ArgumentNullException.ThrowIfNull(entry.Admin);

        // The note belongs to "Other" alone and "Other" needs one (02 7.2.4; 06 4.22); the order module checks it first, the writer does not take a row that breaks it.
        string? note = string.IsNullOrWhiteSpace(entry.ReasonNote) ? null : entry.ReasonNote.Trim();
        if ((entry.Reason == CancellationReason.Other) != (note is not null))
        {
            throw new ArgumentException("The firm's cancellation carries a note exactly on the reason \"Other\".", nameof(entry));
        }

        return await AddCancellationRowAsync(
            entry.Subject,
            entry.Quantity,
            entry.Frozen,
            amounts => Cancellation.ByAdmin(entry.Subject.OrderItemId, entry.Quantity, entry.Admin.AdminId, entry.Admin.AdminName, entry.Reason, note, amounts, entry.At),
            ct);
    }

    public async Task<ItemRecordWriteResult> AddLateDeliveryTerminationAsync(LateDeliveryTerminationEntry entry, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(entry);
        ArgumentNullException.ThrowIfNull(entry.Frozen);
        ItemRecordSubject subject = entry.Subject;
        RequireOrderLock(subject);

        // Only an undelivered physical item is terminated (02 7.2.3: digital and service items go on; an item with a delivery date no longer opens it, 03 4.1.12).
        if (subject.ProductType != ProductType.Physical || subject.DeliveryDate is not null)
        {
            return ItemRecordWriteResult.Rejected(ItemRecordRefusal.StatusNotAllowed);
        }

        // All the open units, never a choice (02 7.1.6; K-796, K-1077).
        int open = await OpenQuantityAsync(subject, ct);
        if (open < 1 || entry.Quantity != open)
        {
            return ItemRecordWriteResult.Rejected(ItemRecordRefusal.QuantityExceedsOpen);
        }

        (decimal couponSetAside, decimal vatSetAside) = await SetAsideAsync(subject.OrderItemId, ct);
        ItemAmounts amounts = ItemShares.SetAside(entry.Frozen, subject.ItemQuantity, open, closesTheItem: true, couponSetAside, vatSetAside);
        LateDeliveryTermination termination = entry.Admin is { } admin
            ? LateDeliveryTermination.ByAdmin(subject.OrderItemId, open, admin.AdminId, admin.AdminName, entry.DeclarationDate, amounts, null, entry.At)
            : LateDeliveryTermination.ByBuyer(subject.OrderItemId, open, entry.DeclarationDate, amounts, null, entry.At);
        _db.LateDeliveryTerminations.Add(termination);
        return new ItemRecordWriteResult(null, termination.Id, null);
    }

    public async Task<IReadOnlyList<ClosingRecordMoment>> ClosingRecordMomentsAsync(IReadOnlyCollection<Guid> orderItemIds, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(orderItemIds);
        Guid[] ids = [.. orderItemIds.Distinct()];
        if (ids.Length == 0)
        {
            return [];
        }

        List<Cancellation> cancellations = await _db.Cancellations.AsNoTracking().Where(c => ids.Contains(c.OrderItemId)).ToListAsync(ct);
        Guid[] closing = [.. cancellations.Select(c => c.TransitionRecordId).OfType<Guid>().Distinct()];
        IReadOnlySet<Guid> corrected = closing.Length == 0 ? new HashSet<Guid>() : await _ordering.FindCorrectedTransitionRecordsAsync(closing, ct);
        List<ClosingRecordMoment> moments =
        [
            .. cancellations
                .Where(c => c.TransitionRecordId is not { } closedBy || !corrected.Contains(closedBy))
                .Select(c => new ClosingRecordMoment(c.Id, c.OrderItemId, ClosingRecordKind.Cancellation, c.CreatedAt)),
        ];
        moments.AddRange(await _db.LateDeliveryTerminations.AsNoTracking()
            .Where(t => ids.Contains(t.OrderItemId))
            .Select(t => new ClosingRecordMoment(t.Id, t.OrderItemId, ClosingRecordKind.LateDeliveryTermination, t.CreatedAt))
            .ToListAsync(ct));
        return [.. moments.OrderBy(m => m.At).ThenBy(m => m.RecordId)];
    }

    /// <summary>The cancellation row of either actor (06 4.22): the unit rule under the order's lock, the amounts set aside, then the row the caller's factory makes.</summary>
    private async Task<ItemRecordWriteResult> AddCancellationRowAsync(
        ItemRecordSubject subject, int quantity, ItemFrozenAmounts frozen, Func<ItemAmounts, Cancellation> create, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(frozen);
        RequireOrderLock(subject);
        int open = await OpenQuantityAsync(subject, ct);
        if (!ItemQuantities.IsSelectable(quantity, open))
        {
            return ItemRecordWriteResult.Rejected(ItemRecordRefusal.QuantityExceedsOpen);
        }

        (decimal couponSetAside, decimal vatSetAside) = await SetAsideAsync(subject.OrderItemId, ct);
        ItemAmounts amounts = ItemShares.SetAside(frozen, subject.ItemQuantity, quantity, quantity == open, couponSetAside, vatSetAside);
        Cancellation cancellation = create(amounts);
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
