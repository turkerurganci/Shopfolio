using Microsoft.EntityFrameworkCore;

using Npgsql;

using Shopfolio.Infrastructure.Logging;

namespace Shopfolio.Infrastructure.Jobs;

/// <summary>
/// The nightly job (05 5.1.1, 5.2.8, 5.3.2; 09 13.1 items 7, 9). When <see cref="NightlyJobSchedule.IsDue"/> says so from the heartbeat
/// record, it holds the nightly single-run lock (a second instance gets nothing and leaves), reads the record again under the lock, writes the
/// attempt, runs the registered steps one after the other - each in a scope of its own - and writes the success only when all of them ended
/// without an exception. A failure is logged and not rethrown: the job is tried again an hour after the attempt at the earliest. This is the
/// third place of a broad catch (09 8.1 item 6).
/// </summary>
internal sealed partial class NightlyJobRunner
{
    private readonly NpgsqlDataSource _dataSource;
    private readonly IServiceScopeFactory _scopes;
    private readonly IReadOnlyList<INightlyStep> _steps;
    private readonly TimeProvider _time;
    private readonly ILogger<NightlyJobRunner> _logger;

    public NightlyJobRunner(
        NpgsqlDataSource dataSource,
        IServiceScopeFactory scopes,
        IEnumerable<INightlyStep> steps,
        TimeProvider time,
        ILogger<NightlyJobRunner> logger)
    {
        _dataSource = dataSource;
        _scopes = scopes;
        _steps = [.. steps];
        _time = time;
        _logger = logger;
    }

    /// <summary>Runs the job when its time has come; returns whether it ran to its end.</summary>
    public async Task<bool> RunIfDueAsync(CancellationToken ct)
    {
        if (!await IsDueAsync(ct))
        {
            return false;
        }

        SessionAdvisoryLock? held = await AdvisoryLocks.TryHoldAsync(_dataSource, SchedulerTimings.NightlyLockName, ct);
        if (held is null)
        {
            return false;
        }

        await using (held)
        {
            // Another instance may have finished the job between the first look and the lock.
            if (!await IsDueAsync(ct))
            {
                return false;
            }

            DateTimeOffset startedAt = _time.GetUtcNow();
            await WriteAsync(h => h.SetProperty(r => r.NightlyJobLastAttemptedAt, startedAt), ct);
            LogStarted(_logger, LogEvents.JobStarted, SchedulerTimings.NightlyLockName);
            try
            {
                foreach (INightlyStep step in _steps)
                {
                    await held.EnsureAliveAsync(ct);
                    await using AsyncServiceScope scope = _scopes.CreateAsyncScope();
                    await step.RunAsync(scope.ServiceProvider, _time.GetUtcNow(), ct);
                }
            }
#pragma warning disable CA1031 // 09 8.1 item 6: the nightly job's wrapper is a place a broad catch is allowed; the failure is logged and the job is tried again an hour later.
            catch (Exception exception) when (!ct.IsCancellationRequested)
#pragma warning restore CA1031
            {
                LogFailed(_logger, exception, LogEvents.UnexpectedError, SchedulerTimings.NightlyLockName);
                return false;
            }

            DateTimeOffset finishedAt = _time.GetUtcNow();
            await WriteAsync(h => h.SetProperty(r => r.NightlyJobLastSucceededAt, finishedAt), ct);
            LogFinished(_logger, LogEvents.JobFinished, SchedulerTimings.NightlyLockName, "Completed");
            return true;
        }
    }

    private async Task<bool> IsDueAsync(CancellationToken ct)
    {
        await using AsyncServiceScope scope = _scopes.CreateAsyncScope();
        AppDbContext db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var times = await db.HeartbeatRecords.AsNoTracking()
            .Select(h => new { h.NightlyJobLastSucceededAt, h.NightlyJobLastAttemptedAt })
            .SingleOrDefaultAsync(ct);
        return times is not null
            && NightlyJobSchedule.IsDue(_time.GetUtcNow(), times.NightlyJobLastSucceededAt, times.NightlyJobLastAttemptedAt);
    }

    private async Task WriteAsync(
        Action<Microsoft.EntityFrameworkCore.Query.UpdateSettersBuilder<HeartbeatRecord>> set,
        CancellationToken ct)
    {
        await using AsyncServiceScope scope = _scopes.CreateAsyncScope();
        AppDbContext db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.HeartbeatRecords.ExecuteUpdateAsync(set, ct);
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "{Event} {JobName}")]
    private static partial void LogStarted(ILogger logger, string @event, string jobName);

    [LoggerMessage(Level = LogLevel.Information, Message = "{Event} {JobName} {Result}")]
    private static partial void LogFinished(ILogger logger, string @event, string jobName, string result);

    [LoggerMessage(Level = LogLevel.Error, Message = "{Event} {JobName}: the nightly job failed; it is tried again an hour after the attempt at the earliest.")]
    private static partial void LogFailed(ILogger logger, Exception exception, string @event, string jobName);
}
