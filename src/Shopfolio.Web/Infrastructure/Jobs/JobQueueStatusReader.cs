using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure.Values;

namespace Shopfolio.Infrastructure.Jobs;

/// <summary>What the queue says for the server's status command (05 8.5.6): the "failed" rows, the rows that are due and the moment the oldest due row was due.</summary>
internal sealed record JobQueueDetails(int FailedCount, int DueCount, DateTimeOffset? OldestDueAt);

/// <summary>
/// The job queue's numbers for the status command (05 8.5.6, 9.4.16): read from the queue itself, so it stands with the queue - the job table is touched only under
/// <c>Infrastructure/Jobs/</c>. The due rule is the status condition's own (<see cref="JobQueueStatusCondition"/>): a pending row is due when its next attempt is empty or has come.
/// A plain read; it writes and locks nothing.
/// </summary>
internal sealed class JobQueueStatusReader
{
    private readonly AppDbContext _db;
    private readonly TimeProvider _time;

    public JobQueueStatusReader(AppDbContext db, TimeProvider time)
    {
        ArgumentNullException.ThrowIfNull(db);
        ArgumentNullException.ThrowIfNull(time);
        _db = db;
        _time = time;
    }

    public async Task<JobQueueDetails> ReadAsync(CancellationToken ct)
    {
        DateTimeOffset now = _time.GetUtcNow();
        int failed = await _db.Jobs.AsNoTracking().CountAsync(j => j.JobState == JobState.Failed, ct);
        IQueryable<Job> due = _db.Jobs.AsNoTracking()
            .Where(j => j.JobState == JobState.Pending && (j.NextAttemptAt == null || j.NextAttemptAt <= now));
        int dueCount = await due.CountAsync(ct);
        if (dueCount == 0)
        {
            return new JobQueueDetails(failed, 0, null);
        }

        DateTimeOffset oldest = await due.MinAsync(j => j.NextAttemptAt ?? j.CreatedAt, ct);
        return new JobQueueDetails(failed, dueCount, oldest);
    }
}
