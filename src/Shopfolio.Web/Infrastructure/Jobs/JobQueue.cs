using System.Threading.Channels;

using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure.Values;

namespace Shopfolio.Infrastructure.Jobs;

/// <summary>
/// The one writer of a job row (09 13.1 item 2; 05 4.3.1; MK-37): a side effect that goes outside is written as a row in the transaction of the
/// transition that causes it and runs after that transaction. Taking a row is the database's row lock; the row has no lock column (06 4.59).
/// </summary>
internal interface IJobQueue
{
    /// <summary>
    /// Adds a pending row to the caller's open transaction and does not save: the row is written by one statement that joins the transaction, so
    /// it commits and rolls back with the transition's own changes. A pending row of the same type and related record already there is the
    /// same row (the unique index of 06 4.59): the call leaves it as it is. <paramref name="notBefore"/> is the first attempt's moment; empty
    /// means at once. After the transaction commits the runner is woken (<see cref="ITransactionRunner"/>); a rolled back transaction wakes nobody.
    /// </summary>
    Task EnqueueAsync(JobType jobType, Guid? relatedRecordId, DateTimeOffset? notBefore, CancellationToken ct);
}

/// <summary>
/// Remembers, per scope, that a row was added in the open transaction, so the transaction runner can wake the job runner once the transaction
/// has committed and only then (09 13.1 item 2). Scoped: one scope is one unit of work.
/// </summary>
internal sealed class JobEnqueueTracker
{
    /// <summary>True when a row was added since the mark was last taken or cleared.</summary>
    public bool IsMarked { get; private set; }

    public void Mark() => IsMarked = true;

    public void Clear() => IsMarked = false;
}

/// <summary>The wake-up of the runner (09 13.1 item 2): a signal with room for one; repeated signals come down to one wake-up.</summary>
internal interface IJobSignal
{
    /// <summary>Wakes the runner if it is waiting, or makes its next wait end at once. Never blocks and never throws.</summary>
    void Notify();

    /// <summary>
    /// Waits until <see cref="Notify"/> is called or <paramref name="timeout"/> passes on <paramref name="time"/>, whichever comes first. The
    /// wait is a <c>Task.Delay</c> on the given time provider, so a test moves time instead of waiting (09 7.1).
    /// </summary>
    Task WaitAsync(TimeSpan timeout, TimeProvider time, CancellationToken ct);
}

internal sealed class JobSignal : IJobSignal
{
    private readonly Channel<bool> _channel = Channel.CreateBounded<bool>(new BoundedChannelOptions(1)
    {
        FullMode = BoundedChannelFullMode.DropWrite,
        SingleReader = true,
    });

    public void Notify()
    {
        _channel.Writer.TryWrite(true);
    }

    public async Task WaitAsync(TimeSpan timeout, TimeProvider time, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(time);
        using var stop = CancellationTokenSource.CreateLinkedTokenSource(ct);
        Task delay = Task.Delay(timeout, time, stop.Token);
        Task signalled = _channel.Reader.WaitToReadAsync(stop.Token).AsTask();
        Task first = await Task.WhenAny(delay, signalled);
        await stop.CancelAsync();
        ct.ThrowIfCancellationRequested();
        if (first == signalled)
        {
            _channel.Reader.TryRead(out _);
        }
    }
}

internal sealed class JobQueue : IJobQueue
{
    private readonly AppDbContext _db;
    private readonly TimeProvider _time;
    private readonly JobEnqueueTracker _tracker;

    public JobQueue(AppDbContext db, TimeProvider time, JobEnqueueTracker tracker)
    {
        _db = db;
        _time = time;
        _tracker = tracker;
    }

    public async Task EnqueueAsync(JobType jobType, Guid? relatedRecordId, DateTimeOffset? notBefore, CancellationToken ct)
    {
        if (_db.Database.CurrentTransaction is null)
        {
            throw new InvalidOperationException("A job row is written in the caller's transaction and never alone (09 13.1 item 2; 05 4.3.1).");
        }

        if (!Enum.IsDefined(jobType))
        {
            throw new ArgumentOutOfRangeException(nameof(jobType), jobType, "Unknown job type.");
        }

        if ((jobType == JobType.BounceQuery) == relatedRecordId.HasValue)
        {
            throw new ArgumentException(
                jobType == JobType.BounceQuery
                    ? "The returned-address query has no related record (06 4.59)."
                    : $"A {jobType} row points at its record (06 4.59).",
                nameof(relatedRecordId));
        }

        Job row = Job.Enqueue(jobType, relatedRecordId, notBefore, _time.GetUtcNow());
        string typeName = row.JobType.ToString();

        // The conflict target is the unique index of the pending rows (06 7.1), whose empty related record counts as equal: the repeated
        // returned-address query is the same row. Raw SQL, because the insert must not wait for a save and must not fail on the duplicate.
        await _db.Database.ExecuteSqlAsync(
            $"""
            INSERT INTO jobs (id, created_at, job_type, related_record_id, next_attempt_at)
            VALUES ({row.Id}, {row.CreatedAt}, {typeName}, {row.RelatedRecordId}, {row.NextAttemptAt})
            ON CONFLICT (job_type, related_record_id) WHERE job_state = 'Pending' DO NOTHING
            """,
            ct);
        _tracker.Mark();
    }
}
