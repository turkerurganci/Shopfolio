using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;

namespace Shopfolio.Modules.AfterSales;

/// <summary>
/// The item records the sales summary's cancellation and return count and ratio read (02 10.6.3; 07 9.22.1; 11 T383 criterion 8; K-580, K-790): the cancellation (customer and firm)
/// and the removal on the day they were written, the late delivery termination on its notice's date stamp and the withdrawal on its declaration's date - the withdrawal is counted
/// when it is declared, whatever happens to the goods later. A cancellation undone by the correction of its closing transition is not counted (06 4.22; K-1056). The
/// "goods did not come back" closure and the return rejection are not item records of a loss and are never read, so they cannot change the count (02 10.4.9, 7.4.9).
/// Plain reads; nothing is written.
/// </summary>
internal sealed partial class AfterSalesModule
{
    public async Task<IReadOnlyList<Guid>> ListItemsWithLossInRangeAsync(DateOnly startDay, DateOnly endDay, CancellationToken ct)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(endDay, startDay);
        DateTimeOffset from = TurkeyTime.StartOf(startDay);
        DateTimeOffset to = TurkeyTime.EndOfRange(endDay);

        var cancellations = await _db.Cancellations.AsNoTracking()
            .Where(c => c.CreatedAt >= from && c.CreatedAt < to)
            .Select(c => new { c.OrderItemId, c.TransitionRecordId })
            .ToListAsync(ct);
        HashSet<Guid> items = [.. await LiveCancellationItemsAsync(cancellations.Select(c => (c.OrderItemId, c.TransitionRecordId)), ct)];
        items.UnionWith(await _db.ItemRemovals.AsNoTracking().Where(r => r.CreatedAt >= from && r.CreatedAt < to).Select(r => r.OrderItemId).ToListAsync(ct));
        items.UnionWith(await _db.LateDeliveryTerminations.AsNoTracking()
            .Where(t => t.DeclarationDate >= startDay && t.DeclarationDate <= endDay).Select(t => t.OrderItemId).ToListAsync(ct));
        items.UnionWith(await _db.Withdrawals.AsNoTracking()
            .Where(w => w.DeclarationDate >= startDay && w.DeclarationDate <= endDay).Select(w => w.OrderItemId).ToListAsync(ct));
        return [.. items];
    }

    public async Task<IReadOnlySet<Guid>> FindItemsWithLossAsync(IReadOnlyCollection<Guid> orderItemIds, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(orderItemIds);
        Guid[] ids = [.. orderItemIds.Distinct()];
        if (ids.Length == 0)
        {
            return new HashSet<Guid>();
        }

        var cancellations = await _db.Cancellations.AsNoTracking()
            .Where(c => ids.Contains(c.OrderItemId))
            .Select(c => new { c.OrderItemId, c.TransitionRecordId })
            .ToListAsync(ct);
        HashSet<Guid> items = [.. await LiveCancellationItemsAsync(cancellations.Select(c => (c.OrderItemId, c.TransitionRecordId)), ct)];
        items.UnionWith(await _db.ItemRemovals.AsNoTracking().Where(r => ids.Contains(r.OrderItemId)).Select(r => r.OrderItemId).ToListAsync(ct));
        items.UnionWith(await _db.LateDeliveryTerminations.AsNoTracking().Where(t => ids.Contains(t.OrderItemId)).Select(t => t.OrderItemId).ToListAsync(ct));
        items.UnionWith(await _db.Withdrawals.AsNoTracking().Where(w => ids.Contains(w.OrderItemId)).Select(w => w.OrderItemId).ToListAsync(ct));
        return items;
    }

    /// <summary>The items of the cancellations that still stand: a cancellation whose closing transition was corrected gave its units back (06 4.22).</summary>
    private async Task<IEnumerable<Guid>> LiveCancellationItemsAsync(IEnumerable<(Guid OrderItemId, Guid? TransitionRecordId)> cancellations, CancellationToken ct)
    {
        List<(Guid OrderItemId, Guid? TransitionRecordId)> rows = [.. cancellations];
        Guid[] closing = [.. rows.Select(c => c.TransitionRecordId).OfType<Guid>().Distinct()];
        IReadOnlySet<Guid> corrected = closing.Length == 0 ? new HashSet<Guid>() : await _ordering.FindCorrectedTransitionRecordsAsync(closing, ct);
        return rows.Where(c => c.TransitionRecordId is not { } closedBy || !corrected.Contains(closedBy)).Select(c => c.OrderItemId);
    }
}
