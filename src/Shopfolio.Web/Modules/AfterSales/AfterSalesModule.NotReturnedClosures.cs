using Microsoft.EntityFrameworkCore;

using Shopfolio.Modules.AfterSales.Contracts;

namespace Shopfolio.Modules.AfterSales;

/// <summary>The "mal dönmedi" closures an amount based refund can be bound to (T529; 07 UC-133; 06 4.28, 4.33 <c>NotReturnedClosureId</c>; 04 7.2.13.2; K-1105).</summary>
internal sealed partial class AfterSalesModule
{
    public async Task<IReadOnlyList<OpenNotReturnedClosure>> ListOpenNotReturnedClosuresAsync(IReadOnlyCollection<Guid> orderItemIds, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(orderItemIds);
        if (orderItemIds.Count == 0)
        {
            return [];
        }

        return await _db.NotReturnedClosures.AsNoTracking()
            .Where(n => orderItemIds.Contains(n.OrderItemId) && n.ReopenedAt == null)
            .OrderByDescending(n => n.CreatedAt).ThenByDescending(n => n.Id)
            .Select(n => new OpenNotReturnedClosure(n.Id, n.OrderItemId, n.Quantity, n.CreatedAt))
            .ToListAsync(ct);
    }
}
