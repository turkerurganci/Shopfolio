using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure.Values;

namespace Shopfolio.Infrastructure.Jobs;

/// <summary>
/// The clean-up of the queue's rows (05 4.3.4, 5.2.9; 06 9.1.4, 9.4.4): the nightly clean-up deletes the completed rows - a closed row counts as
/// completed - and only them. A failed row is never deleted: it is the status endpoint's condition and leaves only by the installer's command
/// (requeue or close). A pending row is the queue's work and is not touched either. The rows carry no personal data, so no disposal record is
/// written. The nightly job (T201, T221) calls this; it is the one place a job row is deleted.
/// </summary>
internal static class JobCleanup
{
    /// <summary>Deletes every completed row and returns how many went.</summary>
    public static Task<int> DeleteCompletedAsync(AppDbContext db, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(db);
        return db.Jobs.Where(j => j.JobState == JobState.Completed).ExecuteDeleteAsync(ct);
    }
}
