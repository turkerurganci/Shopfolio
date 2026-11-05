using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure.Values;

namespace Shopfolio.Infrastructure.Jobs;

/// <summary>
/// Taking a row (05 4.3.2; 06 4.59): the database's row lock, <c>FOR UPDATE SKIP LOCKED</c>, in the open transaction. The row has no lock
/// column: a row that is taken and not finished - the process died - is still pending and is taken again by the next runner. A row another
/// transaction holds is skipped, never waited for, so two takers never take the same row. This file is the one place outside
/// <see cref="AdvisoryLocks"/> and <c>OrderLocks</c> that writes a row lock (the concurrency source scan names it by its full path).
/// </summary>
internal static class JobClaim
{
    /// <summary>
    /// The oldest pending row that is due and of a known type, locked, or <c>null</c>. Due: the next attempt's moment is empty or has come;
    /// the order is that moment, or the creation moment when empty (09 13.1 item 3). A type without a handler is not read.
    /// </summary>
    public static async Task<Job?> TakeNextAsync(AppDbContext db, IReadOnlyCollection<string> knownTypes, DateTimeOffset now, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(db);
        ArgumentNullException.ThrowIfNull(knownTypes);
        if (knownTypes.Count == 0)
        {
            return null;
        }

        string pending = nameof(JobState.Pending);
        string[] types = [.. knownTypes];
        List<Job> rows = await db.Jobs
            .FromSql($"""
                SELECT * FROM jobs
                WHERE job_state = {pending} AND job_type = ANY({types}) AND (next_attempt_at IS NULL OR next_attempt_at <= {now})
                ORDER BY COALESCE(next_attempt_at, created_at), id
                LIMIT 1
                FOR UPDATE SKIP LOCKED
                """)
            .AsTracking()
            .ToListAsync(ct);
        return rows.SingleOrDefault();
    }

    /// <summary>The row of <paramref name="id"/>, locked and tracked, or <c>null</c> when it is gone; the caller looks at its state (a row the installer closed meanwhile is not written over).</summary>
    public static async Task<Job?> LockAsync(AppDbContext db, Guid id, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(db);

        // The context of the scope may still track the row from the taking step; the row is read again so that its present state is seen.
        db.ChangeTracker.Clear();
        List<Job> rows = await db.Jobs
            .FromSql($"SELECT * FROM jobs WHERE id = {id} FOR UPDATE")
            .AsTracking()
            .ToListAsync(ct);
        return rows.SingleOrDefault();
    }
}
