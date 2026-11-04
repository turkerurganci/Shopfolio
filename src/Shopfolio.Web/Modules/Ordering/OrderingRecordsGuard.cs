using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Shopfolio.Modules.Ordering;

/// <summary>
/// Refuses the writes the order side never allows (06 4.16, 4.17, 4.19, 4.21; 06 2.2.3, 2.2.5): an order, its item, its transition record and
/// its checkbox acceptance (with the acceptance's item link) are permanent and never deleted, and a transition record is only appended. The one
/// change of a transition record is emptying the administrator's link, name and free note (the disposal; the database empties the link when
/// the administrator is removed). The save is refused before any SQL is sent. A bulk statement (<c>ExecuteDelete</c>, <c>ExecuteUpdate</c>)
/// does not pass through a save and is not covered here; the database's restrictive links still stop an order's removal.
/// </summary>
internal sealed class OrderingRecordsGuard : SaveChangesInterceptor
{
    private static readonly HashSet<string> ErasableTransitionFields =
    [
        nameof(TransitionRecord.AdminId),
        nameof(TransitionRecord.AdminName),
        nameof(TransitionRecord.ReasonNote),
    ];

    public static OrderingRecordsGuard Instance { get; } = new();

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
                    $"A {entry.Metadata.ClrType.Name} is permanent and is never deleted (06 2.2.3, 2.2.5); an order is closed by its status.");
            }

            if (entry.State == EntityState.Modified && entry.Entity is TransitionRecord && !IsErasure(entry))
            {
                throw new InvalidOperationException(
                    "A transition record is only appended (06 4.19); its one change is emptying the administrator's link, name and note.");
            }

            if (entry.State == EntityState.Modified && entry.Entity is OrderEmailHistory or OrderAddressHistory && !IsAdminLinkEmptying(entry))
            {
                throw new InvalidOperationException(
                    "An e-mail or address history row is only appended (06 4.61, 4.62; 06 2.2.3); its one change is emptying the administrator's link.");
            }
        }
    }

    private static bool IsPermanent(object entity)
    {
        return entity is Order or OrderItem or TransitionRecord or CheckboxAcceptance or CheckboxAcceptanceOrderItem;
    }

    // The one change of a history row is the administrator link emptied (the database does it when the administrator is removed; 06 2.2.3, 2.2.5).
    private static bool IsAdminLinkEmptying(EntityEntry entry)
    {
        foreach (PropertyEntry property in entry.Properties)
        {
            if (property.IsModified && (property.Metadata.Name != nameof(OrderEmailHistory.AdminId) || property.CurrentValue is not null))
            {
                return false;
            }
        }

        return true;
    }

    private static bool IsErasure(EntityEntry entry)
    {
        foreach (PropertyEntry property in entry.Properties)
        {
            if (!property.IsModified)
            {
                continue;
            }

            if (!ErasableTransitionFields.Contains(property.Metadata.Name) || property.CurrentValue is not null)
            {
                return false;
            }
        }

        return true;
    }
}
