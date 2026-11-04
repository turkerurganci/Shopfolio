using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Shopfolio.Modules.Payment;

/// <summary>
/// Refuses the deletion the payment side never allows (06 4.32-4.34; 06 2.2.3, 9.1.4): the payment record, the refund and the refund record
/// are the order's commercial record and are never deleted. The inbound notification record is not covered: the disposal deletes it
/// explicitly by its retention (06 4.35). The save is refused before any SQL is sent. A bulk statement (<c>ExecuteDelete</c>) does not pass
/// through a save and is not covered here; the database's restrictive links still stop the order's removal.
/// </summary>
internal sealed class PaymentRecordsGuard : SaveChangesInterceptor
{
    public static PaymentRecordsGuard Instance { get; } = new();

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
            if (entry.State == EntityState.Deleted && entry.Entity is PaymentRecord or Refund or RefundRecord)
            {
                throw new InvalidOperationException(
                    $"A {entry.Metadata.ClrType.Name} is the order's commercial record and is never deleted (06 2.2.3; 4.32-4.34).");
            }
        }
    }
}
