using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure.Values;

namespace Shopfolio.Infrastructure.Jobs;

/// <summary>What the queue says for the server's status command (05 8.5.6): the "failed" rows, the rows that are due and the moment the oldest due row was due.</summary>
internal sealed record JobQueueDetails(int FailedCount, int DueCount, DateTimeOffset? OldestDueAt);

/// <summary>One "failed" row as the status command lists it (T387; 05 8.5.6, 8.5.9): its id for the <c>job</c> command, its type, attempts, creation and error summary.</summary>
internal sealed record FailedJobLine(Guid Id, JobType JobType, int AttemptCount, DateTimeOffset CreatedAt, string? LastErrorSummary);

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

    /// <summary>
    /// The "failed" rows, oldest first, streamed row by row (09 20 item 2): the <c>job</c> command takes a row by its id (T387; 05 8.5.9). The error summary carries no personal data
    /// (06 4.59).
    /// </summary>
    public IAsyncEnumerable<FailedJobLine> ReadFailed()
    {
        return _db.Jobs.AsNoTracking()
            .Where(j => j.JobState == JobState.Failed)
            .OrderBy(j => j.CreatedAt).ThenBy(j => j.Id)
            .Select(j => new FailedJobLine(j.Id, j.JobType, j.AttemptCount, j.CreatedAt, j.LastErrorSummary))
            .AsAsyncEnumerable();
    }
}
