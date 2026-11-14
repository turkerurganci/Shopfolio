using Microsoft.EntityFrameworkCore;

namespace Shopfolio.Modules.AfterSales;

/// <summary>The "mal dönmedi" closure an amount based refund is bound to (T529; 07 UC-133; 06 4.28, 4.33 <c>NotReturnedClosureId</c>; K-1105).</summary>
internal sealed partial class AfterSalesModule
{
    public async Task<Guid?> FindOpenNotReturnedClosureAsync(Guid orderItemId, CancellationToken ct)
    {
        return await _db.NotReturnedClosures.AsNoTracking()
            .Where(n => n.OrderItemId == orderItemId && n.ReopenedAt == null)
            .OrderByDescending(n => n.CreatedAt).ThenByDescending(n => n.Id)
            .Select(n => (Guid?)n.Id)
            .FirstOrDefaultAsync(ct);
    }
}
