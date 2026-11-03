using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Shopfolio.Modules.Settings;

/// <summary>
/// Refuses the writes the settings tables never allow (06 4.1…4.4, 4.56; 02 3.1.1): the company, its settings, the province list and the
/// country list are permanent and never deleted, and a published legal text version is never updated or deleted. The save is refused before any
/// SQL is sent, so there is no code path that changes or removes such a row. The deletion of a delivery province row is an ordinary event and is
/// not guarded. A bulk statement (<c>ExecuteDelete</c>, <c>ExecuteUpdate</c>) does not pass through a save and is not covered here.
/// </summary>
internal sealed class ProtectedRecordsGuard : SaveChangesInterceptor
{
    public static ProtectedRecordsGuard Instance { get; } = new();

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
            if (entry.State == EntityState.Deleted && entry.Entity is Company or CompanySettings or Province or Country or LegalTextVersion)
            {
                throw new InvalidOperationException($"A {entry.Metadata.ClrType.Name} row is permanent and is never deleted (06 4.1…4.4, 4.56).");
            }

            if (entry.State == EntityState.Modified
                && entry.Entity is LegalTextVersion
                && entry.Property(nameof(LegalTextVersion.PublishedAt)).OriginalValue is not null)
            {
                throw new InvalidOperationException("A published legal text version is never updated; a change is a new version (06 4.56).");
            }
        }
    }
}
