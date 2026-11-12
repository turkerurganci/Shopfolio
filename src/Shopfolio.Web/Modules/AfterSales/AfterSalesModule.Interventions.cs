using Microsoft.EntityFrameworkCore;

using Shopfolio.Modules.AfterSales.Contracts;

namespace Shopfolio.Modules.AfterSales;

/// <summary>
/// The after-sales side of the administrator's order interventions (T386; 02 10.4.3, 10.4.5): the removal record, the arrival dates and the withdrawal days the delivery
/// date's correction reads, the firm's answer to which came first on a declaration's day (K-608) and the arrival date's correction in place (K-712). The writers join the
/// caller's transaction under the order's row lock and save nothing; the reads are plain.
/// </summary>
internal sealed partial class AfterSalesModule
{
    public async Task<ItemRecordWriteResult> AddItemRemovalAsync(ItemRemovalEntry entry, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(entry);
        ArgumentNullException.ThrowIfNull(entry.Admin);
        ArgumentNullException.ThrowIfNull(entry.Frozen);
        ItemRecordSubject subject = entry.Subject;
        RequireOrderLock(subject);

        // The removed units are cancelled units on the shipping line (02 10.4.3 (1)): the cancellation's unit rule and its amounts (02 7.1.7).
        int open = await OpenQuantityAsync(subject, ct);
        if (!ItemQuantities.IsSelectable(entry.Quantity, open))
        {
            return ItemRecordWriteResult.Rejected(ItemRecordRefusal.QuantityExceedsOpen);
        }

        (decimal couponSetAside, decimal vatSetAside) = await SetAsideAsync(subject.OrderItemId, ct);
        ItemAmounts amounts = ItemShares.SetAside(entry.Frozen, subject.ItemQuantity, entry.Quantity, entry.Quantity == open, couponSetAside, vatSetAside);
        ItemRemoval removal = ItemRemoval.Create(subject.OrderItemId, entry.Quantity, entry.Admin.AdminId, entry.Admin.AdminName, amounts, entry.At);
        _db.ItemRemovals.Add(removal);
        return new ItemRecordWriteResult(null, removal.Id, null);
    }

    public async Task<IReadOnlyList<ArrivalRecordDate>> ArrivalRecordDatesAsync(IReadOnlyCollection<Guid> orderItemIds, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(orderItemIds);
        Guid[] ids = [.. orderItemIds.Distinct()];
        if (ids.Length == 0)
        {
            return [];
        }

        List<ArrivalRecordDate> dates = await _db.Returns.AsNoTracking()
            .Where(r => ids.Contains(r.OrderItemId))
            .Select(r => new ArrivalRecordDate(r.Id, r.OrderItemId, r.ArrivalDate))
            .ToListAsync(ct);
        dates.AddRange(await _db.ReturnRejections.AsNoTracking()
            .Where(r => ids.Contains(r.OrderItemId))
            .Select(r => new ArrivalRecordDate(r.Id, r.OrderItemId, r.ArrivalDate))
            .ToListAsync(ct));
        return dates;
    }

    public async Task<IReadOnlyList<WithdrawalDeclarationDay>> WithdrawalDeclarationDaysAsync(IReadOnlyCollection<Guid> orderItemIds, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(orderItemIds);
        Guid[] ids = [.. orderItemIds.Distinct()];
        if (ids.Length == 0)
        {
            return [];
        }

        return await _db.Withdrawals.AsNoTracking()
            .Where(w => ids.Contains(w.OrderItemId))
            .Select(w => new WithdrawalDeclarationDay(w.Id, w.OrderItemId, w.DeclarationDate, w.CreatedAt))
            .ToListAsync(ct);
    }

    public async Task SetDeliveryOrderAsync(IReadOnlyCollection<Guid> withdrawalIds, bool deliveredBeforeDeclaration, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(withdrawalIds);
        if (_db.Database.CurrentTransaction is null)
        {
            throw new InvalidOperationException("The delivery order is written inside the caller's transaction, under the order's row lock (05 7.3.2).");
        }

        Guid[] ids = [.. withdrawalIds.Distinct()];
        foreach (Withdrawal withdrawal in await _db.Withdrawals.Where(w => ids.Contains(w.Id)).ToListAsync(ct))
        {
            withdrawal.IsDeliveredBeforeDeclaration = deliveredBeforeDeclaration;
        }
    }

    public async Task<(ItemRecordRefusal? Refusal, DateOnly? OldDate)> CorrectArrivalDateAsync(ItemRecordSubject subject, Guid recordId, DateOnly arrivalDate, CancellationToken ct)
    {
        RequireOrderLock(subject);

        // The arrival is not before the item's delivery date - the receipt's own rule (02 7.4.7; 06 4.26) - and the record is the item's receipt or rejection.
        if (subject.DeliveryDate is { } delivered && arrivalDate < delivered)
        {
            return (ItemRecordRefusal.ArrivalDateOutOfRange, null);
        }

        if (await _db.Returns.SingleOrDefaultAsync(r => r.Id == recordId && r.OrderItemId == subject.OrderItemId, ct) is { } receipt)
        {
            DateOnly old = receipt.ArrivalDate;
            receipt.ArrivalDate = arrivalDate;
            return (null, old);
        }

        if (await _db.ReturnRejections.SingleOrDefaultAsync(r => r.Id == recordId && r.OrderItemId == subject.OrderItemId, ct) is { } rejection)
        {
            DateOnly old = rejection.ArrivalDate;
            rejection.ArrivalDate = arrivalDate;
            return (null, old);
        }

        return (ItemRecordRefusal.ReceiptNotFound, null);
    }
}
