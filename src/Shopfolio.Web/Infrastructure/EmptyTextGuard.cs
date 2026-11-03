using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Shopfolio.Infrastructure;

/// <summary>
/// Makes the one form of an empty value (06 2.8.5): an empty text is never stored; the empty value of an optional field is NULL.
/// Before a save, an empty string in a new or changed record becomes null: an optional column then gets NULL, and a required
/// column is refused by the database's NOT NULL constraint (a DbUpdateException carrying SQLSTATE 23502; no row is written). Whitespace is not touched; trimming belongs to the input (09 3.4).
/// </summary>
internal sealed class EmptyTextGuard : SaveChangesInterceptor
{
    public static EmptyTextGuard Instance { get; } = new();

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        EmptyToNull(eventData.Context);
        return result;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        EmptyToNull(eventData.Context);
        return ValueTask.FromResult(result);
    }

    private static void EmptyToNull(DbContext? context)
    {
        if (context is null)
        {
            return;
        }

        foreach (EntityEntry entry in context.ChangeTracker.Entries().Where(e => e.State is EntityState.Added or EntityState.Modified))
        {
            foreach (PropertyEntry property in entry.Properties)
            {
                if (property.Metadata.ClrType == typeof(string) && property.CurrentValue is string { Length: 0 })
                {
                    property.CurrentValue = null;
                }
            }
        }
    }
}
