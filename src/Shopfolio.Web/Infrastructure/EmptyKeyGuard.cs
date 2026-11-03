using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Shopfolio.Infrastructure;

/// <summary>
/// Stops a new record whose <see cref="Guid"/> key was never assigned (09 10.1 item 2). The model never lets EF make a key, so an
/// unassigned key would be sent as the empty id and the first row would be written with it. The guard throws before any SQL is
/// sent. It does only this check: events and cache invalidation are not attached to it.
/// </summary>
internal sealed class EmptyKeyGuard : SaveChangesInterceptor
{
    public static EmptyKeyGuard Instance { get; } = new();

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

        foreach (EntityEntry entry in context.ChangeTracker.Entries().Where(e => e.State == EntityState.Added))
        {
            IKey? key = entry.Metadata.FindPrimaryKey();
            if (key is null)
            {
                continue;
            }

            foreach (IProperty property in key.Properties)
            {
                if (property.ClrType == typeof(Guid) && entry.Property(property.Name).CurrentValue is Guid id && id == Guid.Empty)
                {
                    throw new InvalidOperationException(
                        $"The key '{property.Name}' of the new '{entry.Metadata.DisplayName()}' record was not assigned; take it from EntityIds.New().");
                }
            }
        }
    }
}
