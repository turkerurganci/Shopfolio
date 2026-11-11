using Microsoft.EntityFrameworkCore;

using Shopfolio.Modules.AfterSales.Contracts;

namespace Shopfolio.Modules.AfterSales;

/// <summary>
/// The records on the items' rows of the customer's order page (04 5.16.5; OB-05, 2.5.2; K-790, K-795; T511): each record by its kind, the units it covers and its moment, the same
/// kind as often as it was written. The closing records come from <see cref="ClosingRecordMomentsAsync"/>, so a cancellation undone by the correction of its closing transition is
/// not on the row (one reading, 09 25). A "mal dönmedi" closure reopened by a late return is not on the row either. A plain read.
/// </summary>
internal sealed partial class AfterSalesModule
{
    public async Task<IReadOnlyList<ItemRecordFact>> ListItemRecordsAsync(IReadOnlyCollection<Guid> orderItemIds, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(orderItemIds);
        Guid[] ids = [.. orderItemIds.Distinct()];
        if (ids.Length == 0)
        {
            return [];
        }

        List<ItemRecordFact> records =
        [
            .. (await ClosingRecordMomentsAsync(ids, ct)).Select(m => new ItemRecordFact(
                m.OrderItemId,
                m.IsRemoval ? ItemRecordFactKind.ItemRemoval : m.IsTermination ? ItemRecordFactKind.LateDeliveryTermination : ItemRecordFactKind.Cancellation,
                m.Quantity,
                m.At)),
        ];
        records.AddRange(await _db.Withdrawals.AsNoTracking()
            .Where(w => ids.Contains(w.OrderItemId))
            .Select(w => new ItemRecordFact(w.OrderItemId, ItemRecordFactKind.Withdrawal, w.Quantity, w.CreatedAt))
            .ToListAsync(ct));
        records.AddRange(await _db.Returns.AsNoTracking()
            .Where(r => ids.Contains(r.OrderItemId))
            .Select(r => new ItemRecordFact(r.OrderItemId, ItemRecordFactKind.ReturnReceipt, r.Quantity, r.CreatedAt))
            .ToListAsync(ct));
        records.AddRange(await _db.ReturnRejections.AsNoTracking()
            .Where(r => ids.Contains(r.OrderItemId))
            .Select(r => new ItemRecordFact(r.OrderItemId, ItemRecordFactKind.ReturnRejection, r.Quantity, r.CreatedAt))
            .ToListAsync(ct));
        records.AddRange(await _db.NotReturnedClosures.AsNoTracking()
            .Where(c => ids.Contains(c.OrderItemId) && c.ReopenedAt == null)
            .Select(c => new ItemRecordFact(c.OrderItemId, ItemRecordFactKind.NotReturnedClosure, c.Quantity, c.CreatedAt))
            .ToListAsync(ct));
        records.AddRange(await _db.DefectClaims.AsNoTracking()
            .Where(d => ids.Contains(d.OrderItemId))
            .Select(d => new ItemRecordFact(d.OrderItemId, ItemRecordFactKind.DefectClaim, d.Quantity, d.CreatedAt))
            .ToListAsync(ct));
        return [.. records.OrderBy(r => r.At).ThenBy(r => r.Kind)];
    }
}
