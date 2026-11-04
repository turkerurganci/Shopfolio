using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Shopfolio.Modules.AfterSales;

/// <summary>
/// Refuses the deletion the after-sales side never allows (06 4.22-4.28; 06 2.2.3): the cancellation, the removal, the late delivery
/// termination, the withdrawal declaration, the receipt and the not-returned closure are the order's commercial record and are never
/// deleted. The defect claim (deleted by its retention) and the refund IBAN (deleted by its event) are not covered: they are deleted
/// explicitly (06 4.30, 4.31). The save is refused before any SQL is sent. A bulk statement (<c>ExecuteDelete</c>) does not pass through a
/// save and is not covered here; the database's restrictive item link still stops the item's removal.
/// </summary>
internal sealed class AfterSalesRecordsGuard : SaveChangesInterceptor
{
    public static AfterSalesRecordsGuard Instance { get; } = new();

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
            if (entry.State == EntityState.Deleted && IsPermanent(entry.Entity))
            {
                throw new InvalidOperationException(
                    $"A {entry.Metadata.ClrType.Name} is the order's commercial record and is never deleted (06 2.2.3; 4.22-4.28).");
            }
        }
    }

    private static bool IsPermanent(object entity)
    {
        return entity is Cancellation or ItemRemoval or LateDeliveryTermination or Withdrawal or Return or NotReturnedClosure;
    }
}
