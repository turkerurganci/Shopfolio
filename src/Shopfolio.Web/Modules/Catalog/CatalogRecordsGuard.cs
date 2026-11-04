using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Shopfolio.Modules.Catalog;

/// <summary>
/// Refuses the writes the catalog never allows (06 4.11, 4.13; 06 2.2.3): a price history row is only appended and never updated, and a coupon
/// is permanent and never deleted. The save is refused before any SQL is sent, so there is no code path that changes a price history row or
/// removes a coupon. A price history row still goes with its variant: the database cascades it, and a tracked row the context deletes with its
/// variant is not an update. A bulk statement (<c>ExecuteDelete</c>, <c>ExecuteUpdate</c>) does not pass through a save and is not covered here.
/// </summary>
internal sealed class CatalogRecordsGuard : SaveChangesInterceptor
{
    public static CatalogRecordsGuard Instance { get; } = new();

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        Check(eventData.Context);
        return result;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        Check(eventData.Context);
        return ValueTask.FromResult(result);
    }

    private static void Check(DbContext? context)
    {
        if (context is null)
        {
            return;
        }

        foreach (EntityEntry entry in context.ChangeTracker.Entries())
        {
            if (entry.State == EntityState.Deleted && entry.Entity is Coupon)
            {
                throw new InvalidOperationException("A coupon is permanent and is never deleted (06 4.13); a campaign is stopped by its end date.");
            }

            if (entry.State == EntityState.Modified && entry.Entity is PriceHistory)
            {
                throw new InvalidOperationException("A price history row is only appended and never updated (06 4.11, 2.2.3); a new price is a new row.");
            }
        }
    }
}
