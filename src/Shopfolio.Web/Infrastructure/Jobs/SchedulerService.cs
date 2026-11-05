namespace Shopfolio.Infrastructure.Jobs;

/// <summary>
/// The scheduler's background service (05 5.1.1, 5.4.1; 09 13.1 items 1, 7, 8): two loops that run side by side, so a slow scan never delays a
/// beat. The scan loop holds the scheduler's single-run lock, asks whether the lock connection is alive at each round, and runs
/// <see cref="SchedulerScan"/> once a minute. The heartbeat loop beats at once and then every 30 seconds, on every instance, because the
/// beat is a conditional write and two beats write once. A loop that ends with an exception ends the service; the host stops (the default
/// behavior is kept) and the container restarts.
/// </summary>
internal sealed class SchedulerService : BackgroundService
{
    private readonly Npgsql.NpgsqlDataSource _dataSource;
    private readonly SchedulerScan _scan;
    private readonly HeartbeatPulse _pulse;
    private readonly IReadOnlyList<IJobStartGate> _gates;
    private readonly TimeProvider _time;

    public SchedulerService(
        Npgsql.NpgsqlDataSource dataSource,
        SchedulerScan scan,
        HeartbeatPulse pulse,
        IEnumerable<IJobStartGate> gates,
        TimeProvider time)
    {
        _dataSource = dataSource;
        _scan = scan;
        _pulse = pulse;
        _gates = [.. gates];
        _time = time;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // The start of the host does not wait for the loops.
        await Task.Yield();
        try
        {
            Task scanning = ScanLoopAsync(stoppingToken);
            Task beating = HeartbeatLoopAsync(stoppingToken);
            Task ended = await Task.WhenAny(scanning, beating);

            // A loop ends only by an exception or by the host stopping; the exception is thrown here.
            await ended;
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // The host is stopping: the loops end cleanly.
        }
    }

    private async Task HeartbeatLoopAsync(CancellationToken ct)
    {
        while (true)
        {
            ct.ThrowIfCancellationRequested();
            await _pulse.BeatAsync(ct);
            await Task.Delay(SchedulerTimings.HeartbeatInterval, _time, ct);
        }
    }

    private async Task ScanLoopAsync(CancellationToken ct)
    {
        while (true)
        {
            ct.ThrowIfCancellationRequested();
            if (await GatesAreOpenAsync(ct))
            {
                SessionAdvisoryLock? held = await AdvisoryLocks.TryHoldAsync(_dataSource, SchedulerTimings.SchedulerLockName, ct);
                if (held is not null)
                {
                    await using (held)
                    {
                        await ScanWhileHoldingAsync(held, ct);
                    }
                }
            }

            await Task.Delay(SchedulerTimings.ScanInterval, _time, ct);
        }
    }

    private async Task ScanWhileHoldingAsync(SessionAdvisoryLock held, CancellationToken ct)
    {
        while (true)
        {
            ct.ThrowIfCancellationRequested();

            // A broken lock connection means the lock is gone: the exception ends the service (09 13.1 item 7).
            await held.EnsureAliveAsync(ct);
            await _scan.RunAsync(ct);
            await Task.Delay(SchedulerTimings.ScanInterval, _time, ct);
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
}
