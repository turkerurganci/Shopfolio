using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure.Logging;

namespace Shopfolio.Infrastructure.Jobs;

/// <summary>
/// One wake-up of the scheduler (05 5.1.1, 5.4.3; 09 13.1 items 6, 7). It reads the clock once, asks every registered source for the records
/// whose time has come, puts the records of all sources in order of their due moments (ties by source order, then id), and has each processed
/// in a scope and a transaction of its own. A source that waits for a fresh heartbeat is passed over while the last heartbeat is older than
/// two minutes, and its freshness is read again before each of its records. A record's failure is logged and the record is left for the next
/// wake-up (the second place of a broad catch, 09 8.1 item 6); the scan goes on with the next one. At the end the nightly job is asked.
/// </summary>
internal sealed partial class SchedulerScan
{
    private readonly IServiceScopeFactory _scopes;
    private readonly IReadOnlyList<IDueRecordSource> _sources;
    private readonly NightlyJobRunner _nightly;
    private readonly TimeProvider _time;
    private readonly ILogger<SchedulerScan> _logger;

    public SchedulerScan(
        IServiceScopeFactory scopes,
        IEnumerable<IDueRecordSource> sources,
        NightlyJobRunner nightly,
        TimeProvider time,
        ILogger<SchedulerScan> logger)
    {
        _scopes = scopes;
        _sources = [.. sources];
        _nightly = nightly;
        _time = time;
        _logger = logger;
    }

    public async Task RunAsync(CancellationToken ct)
    {
        DateTimeOffset now = _time.GetUtcNow();
        bool fresh = await HeartbeatIsFreshAsync(now, ct);
        List<(int Order, IDueRecordSource Source, DueRecord Record)> due = [];
        for (int order = 0; order < _sources.Count; order++)
        {
            IDueRecordSource source = _sources[order];
            if (source.WaitsForFreshHeartbeat && !fresh)
            {
                continue;
            }

            await using AsyncServiceScope scope = _scopes.CreateAsyncScope();
            IReadOnlyList<DueRecord> found = await source.FindDueAsync(scope.ServiceProvider, now, ct);
            due.AddRange(found.Select(record => (order, source, record)));
        }

        foreach ((int _, IDueRecordSource source, DueRecord record) in due
            .OrderBy(d => d.Record.DueAt)
            .ThenBy(d => d.Order)
            .ThenBy(d => d.Record.Id))
        {
            await ProcessOneAsync(source, record, ct);
        }

        await _nightly.RunIfDueAsync(ct);
    }

    private async Task ProcessOneAsync(IDueRecordSource source, DueRecord record, CancellationToken ct)
    {
        try
        {
            if (source.WaitsForFreshHeartbeat && !await HeartbeatIsFreshAsync(_time.GetUtcNow(), ct))
            {
                return;
            }

            await using AsyncServiceScope scope = _scopes.CreateAsyncScope();
            await source.ProcessAsync(scope.ServiceProvider, record.Id, ct);
        }
#pragma warning disable CA1031 // 09 8.1 item 6: the scheduler's record loop is a place a broad catch is allowed; a record's failure is logged and the record waits for the next wake-up.
        catch (Exception exception) when (!ct.IsCancellationRequested)
#pragma warning restore CA1031
        {
            LogRecordFailed(_logger, exception, LogEvents.UnexpectedError, source.Name, record.Id);
        }
    }

    private async Task<bool> HeartbeatIsFreshAsync(DateTimeOffset now, CancellationToken ct)
    {
        await using AsyncServiceScope scope = _scopes.CreateAsyncScope();
        AppDbContext db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        DateTimeOffset? last = await db.HeartbeatRecords.AsNoTracking().Select(h => h.LastHeartbeatAt).SingleOrDefaultAsync(ct);
        return OutageRecovery.IsFresh(last, now);
    }

    [LoggerMessage(Level = LogLevel.Error, Message = "{Event} {Source} {RecordId}: the due record failed; it waits for the next wake-up.")]
    private static partial void LogRecordFailed(ILogger logger, Exception exception, string @event, string source, Guid recordId);
}
