using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure.Values;

namespace Shopfolio.Infrastructure.Jobs;

/// <summary>What the installer does with a failed job row (05 8.5.9; 09 13.2 item 3).</summary>
internal enum FailedJobAction
{
    /// <summary>Back to the queue: pending, the attempt count reset and the next attempt at once (06 4.59).</summary>
    Requeue,

    /// <summary>Closed: the row counts as completed and the nightly clean-up takes it in time (06 4.59, 7.5).</summary>
    Close,
}

/// <summary>Why the installer's step on a job row did not happen; <see cref="None"/> when it did.</summary>
internal enum FailedJobRefusal
{
    None,

    /// <summary>No row has the id.</summary>
    NotFound,

    /// <summary>The row is not failed: only a failed row is requeued or closed (05 8.5.9).</summary>
    NotFailed,

    /// <summary>
    /// A pending row of the same type and related record is already in the queue (the unique index of 06 4.59): the same work waits there, so the failed row is closed, not requeued.
    /// </summary>
    SameWorkPending,
}

/// <summary>The outcome of the installer's step on a job row; a refused one rolls back and writes nothing.</summary>
internal sealed record FailedJobResult(FailedJobRefusal Refusal) : IOperationResult
{
    public bool IsRejected => Refusal != FailedJobRefusal.None;
}

/// <summary>
/// The installer's step on a "failed" job row (05 8.5.9, 4.3.4, 9.4.16; 06 4.59; 09 13.2 items 3, 7): the <c>job</c> command's one writer. The row is locked first
/// (<see cref="JobClaim.LockAsync"/>, the runner's own lock) so that the step and a running attempt never write the same row; only a failed row is touched. <b>Requeue</b> makes it pending
/// again with the attempt count reset and the next attempt empty - taken at once by the running application's runner, never by the command (05 8.5.7); <b>close</b> marks it completed with
/// the moment. Either way no failed row is left, so the job queue condition of the status endpoint goes once the last one is handled (05 9.4.16). Joins the caller's transaction and saves
/// nothing.
/// </summary>
internal sealed class FailedJobRows
{
    private readonly AppDbContext _db;
    private readonly TimeProvider _time;

    public FailedJobRows(AppDbContext db, TimeProvider time)
    {
        _db = db;
        _time = time;
    }

    public async Task<FailedJobResult> ApplyAsync(Guid jobId, FailedJobAction action, CancellationToken ct)
    {
        Job? row = await JobClaim.LockAsync(_db, jobId, ct);
        if (row is null)
        {
            return new FailedJobResult(FailedJobRefusal.NotFound);
        }

        if (row.JobState != JobState.Failed)
        {
            return new FailedJobResult(FailedJobRefusal.NotFailed);
        }

        switch (action)
        {
            case FailedJobAction.Requeue:
                bool samePending = await _db.Jobs.AsNoTracking()
                    .AnyAsync(j => j.Id != row.Id && j.JobState == JobState.Pending && j.JobType == row.JobType && j.RelatedRecordId == row.RelatedRecordId, ct);
                if (samePending)
                {
                    return new FailedJobResult(FailedJobRefusal.SameWorkPending);
                }

                row.JobState = JobState.Pending;
                row.AttemptCount = 0;
                row.NextAttemptAt = null;
                break;
            case FailedJobAction.Close:
                row.JobState = JobState.Completed;
                row.CompletedAt = _time.GetUtcNow();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(action));
        }

        return new FailedJobResult(FailedJobRefusal.None);
    }
}
