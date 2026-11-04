using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Shopfolio.Modules.CorporateContent;

/// <summary>
/// Refuses the deletion the corporate content never allows (06 4.46, 4.54; 06 2.2.1): the about page is only put back to draft and the
/// brand row is permanent. The announcement, the service presentation, the reference work, the question, the general page and the social
/// link are deleted by their events and are not covered. The save is refused before any SQL is sent. A bulk statement
/// (<c>ExecuteDelete</c>) does not pass through a save and is not covered here.
/// </summary>
internal sealed class CorporateContentRecordsGuard : SaveChangesInterceptor
{
    public static CorporateContentRecordsGuard Instance { get; } = new();

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
            if (entry.State == EntityState.Deleted && entry.Entity is AboutPage or Branding)
            {
                throw new InvalidOperationException($"A {entry.Metadata.ClrType.Name} row is permanent and is never deleted (06 4.46, 4.54).");
            }
        }
    }
}
