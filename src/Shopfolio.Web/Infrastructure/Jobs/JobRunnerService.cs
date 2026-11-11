using Microsoft.EntityFrameworkCore;

using Npgsql;

using Shopfolio.Infrastructure.Logging;
using Shopfolio.Infrastructure.Values;

namespace Shopfolio.Infrastructure.Jobs;

/// <summary>The result of the taking step (09 13.1 item 4); never refused, so the transaction always commits what the step did.</summary>
internal sealed record JobStepResult(JobWork? Work, bool IsFinished) : IOperationResult
{
    public bool IsRejected => false;

    public static JobStepResult Empty { get; } = new(null, false);
}

/// <summary>
/// The job queue's runner (05 4.3.2; 09 13.1 items 1, 3, 4, 7): one background loop in the application. It holds the single-run lock of the
/// queue, takes the oldest due row of a known type, runs its handler, writes the outcome on the row, and goes on with the next; when there is
/// no row it waits for the wake-up that the transaction runner gives after a commit that wrote a row, or for five seconds, whichever comes first.
/// Rows are handled one at a time, in order. A row's failure is written on the row and the loop goes on; a failure of the loop itself - a lost
/// lock connection, a database that fails the taking - ends the service, the host stops (the default behavior is kept) and the container
/// restarts (05 4.3.2, 9.4.13).
/// </summary>
internal sealed partial class JobRunnerService : BackgroundService
{
    private readonly NpgsqlDataSource _dataSource;
    private readonly IServiceScopeFactory _scopes;
    private readonly JobHandlerRegistry _handlers;
    private readonly IReadOnlyList<IJobStartGate> _gates;
    private readonly IJobSignal _signal;
    private readonly TimeProvider _time;
    private readonly ILogger<JobRunnerService> _logger;

    public JobRunnerService(
        NpgsqlDataSource dataSource,
        IServiceScopeFactory scopes,
        JobHandlerRegistry handlers,
        IEnumerable<IJobStartGate> gates,
        IJobSignal signal,
        TimeProvider time,
        ILogger<JobRunnerService> logger)
    {
        _dataSource = dataSource;
        _scopes = scopes;
        _handlers = handlers;
        _gates = [.. gates];
        _signal = signal;
        _time = time;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // The start of the host does not wait for the loop.
        await Task.Yield();
        try
        {
            await LoopAsync(stoppingToken);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // The host is stopping: the loop ends cleanly. A row being handled stays pending and is taken by the next runner.
        }
    }

    /// <summary>
    /// Takes at most one due row and handles it; returns whether there was a row. Public to the tests so that one round can be driven; the
    /// loop calls nothing else for the rows.
    /// </summary>
    internal async Task<bool> RunOneAsync(CancellationToken ct)
    {
        await using AsyncServiceScope scope = _scopes.CreateAsyncScope();
        IServiceProvider services = scope.ServiceProvider;
        ITransactionRunner runner = services.GetRequiredService<ITransactionRunner>();
        AppDbContext db = services.GetRequiredService<AppDbContext>();
        IReadOnlyList<string> known = _handlers.KnownTypeNames;
        JobWork? current = null;
        try
        {
            // Step 1, one transaction: the row is taken (its row lock is held until the commit). A handler that only writes to the database
            // runs here too, so its effect and the row's completion are one commit (05 4.3.3).
            JobStepResult step = await runner.RunAsync(
                async cancel =>
                {
                    DateTimeOffset now = _time.GetUtcNow();
                    Job? row = await JobClaim.TakeNextAsync(db, known, now, cancel);
                    if (row is null)
                    {
                        return JobStepResult.Empty;
                    }

                    JobWork work = new(row.Id, row.JobType, row.RelatedRecordId, row.CreatedAt, row.AttemptCount);
                    current = work;
                    JobHandlerRegistration registration = Registration(work.JobType);
                    if (!registration.IsTransactional)
                    {
                        return new JobStepResult(work, false);
                    }

                    LogStarted(_logger, LogEvents.JobStarted, work.Id, work.JobType);
                    await ((ITransactionalJobHandler)services.GetRequiredService(registration.HandlerType)).HandleAsync(work, cancel);
                    Complete(row, _time.GetUtcNow());
                    return new JobStepResult(work, true);
                },
                ct);

            if (step.Work is null)
            {
                return false;
            }

            if (step.IsFinished)
            {
                LogFinished(_logger, LogEvents.JobFinished, step.Work.Id, step.Work.JobType, "Completed");
                return true;
            }

            // Step 2, no transaction open: the handler may call outside; no lock is held while it waits (05 3.6.3).
            JobWork taken = step.Work;
            LogStarted(_logger, LogEvents.JobStarted, taken.Id, taken.JobType);
            IJobHandler handler = (IJobHandler)services.GetRequiredService(Registration(taken.JobType).HandlerType);
            JobOutcome outcome = await handler.HandleAsync(taken, ct);

            // Step 3, one short transaction: the outcome is written on the row.
            await WriteOutcomeAsync(runner, db, taken, outcome, ct);
            LogFinished(_logger, LogEvents.JobFinished, taken.Id, taken.JobType, outcome is JobOutcome.RescheduledOutcome ? "Rescheduled" : "Completed");
            return true;
        }
#pragma warning disable CA1031 // 09 8.1 item 6: the job queue's row loop is the second place a broad catch is allowed; a row's failure is written on the row.
        catch (Exception exception) when (current is not null && !ct.IsCancellationRequested)
#pragma warning restore CA1031
        {
            // A cancelled host is not a failure of the row (the filter): the row stays pending with its count and the next runner takes it.
            JobWork failed = current;
            LogFailed(_logger, exception, LogEvents.UnexpectedError, failed.Id, failed.JobType);
            await WriteFailureAsync(runner, db, failed, exception, ct);
            return true;
        }
    }

    private async Task LoopAsync(CancellationToken ct)
    {
        while (true)
        {
            ct.ThrowIfCancellationRequested();
            if (await GatesAreOpenAsync(ct))
            {
                SessionAdvisoryLock? held = await AdvisoryLocks.TryHoldAsync(_dataSource, JobTimings.RunnerLockName, ct);
                if (held is not null)
                {
                    await using (held)
                    {
                        await ServeWhileHoldingAsync(held, ct);
                    }
                }
            }

            await Task.Delay(JobTimings.PollInterval, _time, ct);
        }
    }

    private async Task ServeWhileHoldingAsync(SessionAdvisoryLock held, CancellationToken ct)
    {
        await WarnAboutUnknownTypesAsync(ct);
        while (true)
        {
            ct.ThrowIfCancellationRequested();

            // A broken lock connection means the lock is gone: the exception ends the service (09 13.1 item 7).
            await held.EnsureAliveAsync(ct);
            if (!await RunOneAsync(ct))
            {
                await _signal.WaitAsync(JobTimings.PollInterval, _time, ct);
            }
        }
    }

    private async Task<bool> GatesAreOpenAsync(CancellationToken ct)
    {
        foreach (IJobStartGate gate in _gates)
        {
            if (!await gate.IsOpenAsync(ct))
            {
                return false;
            }
        }

        return true;
    }

    private async Task WarnAboutUnknownTypesAsync(CancellationToken ct)
    {
        await using AsyncServiceScope scope = _scopes.CreateAsyncScope();
        AppDbContext db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        List<JobType> known = [.. _handlers.KnownTypes];
        int waiting = await db.Jobs.CountAsync(j => j.JobState == JobState.Pending && !known.Contains(j.JobType), ct);
        if (waiting > 0)
        {
            LogUnknownTypes(_logger, LogEvents.JobTypeUnknown, waiting);
        }
    }

    private JobHandlerRegistration Registration(JobType jobType)
    {
        return _handlers.TryGet(jobType, out JobHandlerRegistration registration)
            ? registration
            : throw new InvalidOperationException($"A row of the type {jobType} was taken but the type has no handler.");
    }

    private async Task WriteOutcomeAsync(ITransactionRunner runner, AppDbContext db, JobWork work, JobOutcome outcome, CancellationToken ct)
    {
        await runner.RunAsync(
            async cancel =>
            {
                Job? row = await JobClaim.LockAsync(db, work.Id, cancel);

                // A row the installer closed meanwhile (06 4.59, 05 8.5.9) is not written over.
                if (row is { JobState: JobState.Pending })
                {
                    DateTimeOffset now = _time.GetUtcNow();
                    if (outcome is JobOutcome.RescheduledOutcome later)
                    {
                        row.NextAttemptAt = now + later.Delay;
                    }
                    else
                    {
                        Complete(row, now);
                    }
                }

                return JobStepResult.Empty;
            },
            ct);
    }

    private async Task WriteFailureAsync(ITransactionRunner runner, AppDbContext db, JobWork work, Exception exception, CancellationToken ct)
    {
        string summary = Summarize(exception);
        await runner.RunAsync(
            async cancel =>
            {
                Job? row = await JobClaim.LockAsync(db, work.Id, cancel);
                if (row is { JobState: JobState.Pending })
                {
                    DateTimeOffset now = _time.GetUtcNow();
                    row.LastErrorSummary = summary;
                    switch (JobRetryPolicy.AfterFailure(row.JobType, row.AttemptCount, now))
                    {
                        case JobFailureDecision.RetryAt retry:
                            row.NextAttemptAt = retry.At;
                            row.AttemptCount = retry.NewAttemptCount;
                            break;
                        default:
                            // The row is failed: it stays, is not taken again, and holds nobody back (05 4.3.4).
                            row.AttemptCount++;
                            row.NextAttemptAt = null;
                            row.JobState = JobState.Failed;
                            break;
                    }
                }

                return JobStepResult.Empty;
            },
            ct);
    }

    private static void Complete(Job row, DateTimeOffset now)
    {
        row.JobState = JobState.Completed;
        row.CompletedAt = now;
    }

    /// <summary>The row's last-error summary (06 4.59): the exception's type name and, when a database error is in its chain, the SQLSTATE; never the message, which can carry personal data or parameter values.</summary>
    internal static string Summarize(Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);
        string? sqlState = null;
        for (Exception? next = exception; next is not null && sqlState is null; next = next.InnerException)
        {
            sqlState = (next as PostgresException)?.SqlState;
        }

        return sqlState is null ? exception.GetType().Name : $"{exception.GetType().Name} {sqlState}";
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "{Event} {JobId} {JobType}")]
    private static partial void LogStarted(ILogger logger, string @event, Guid jobId, JobType jobType);

    [LoggerMessage(Level = LogLevel.Information, Message = "{Event} {JobId} {JobType} {Result}")]
    private static partial void LogFinished(ILogger logger, string @event, Guid jobId, JobType jobType, string result);

    [LoggerMessage(Level = LogLevel.Error, Message = "{Event} {JobId} {JobType}: the handler failed; the failure is written on the row.")]
    private static partial void LogFailed(ILogger logger, Exception exception, string @event, Guid jobId, JobType jobType);

    [LoggerMessage(Level = LogLevel.Warning, Message = "{Event}: {Count} pending rows are of a type this version has no handler for; they are left as they are.")]
    private static partial void LogUnknownTypes(ILogger logger, string @event, int count);
}
