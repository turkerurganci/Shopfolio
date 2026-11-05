using Microsoft.EntityFrameworkCore;

namespace Shopfolio.Infrastructure.Jobs;

/// <summary>
/// The one method that writes a heartbeat (05 5.4.3; 09 13.1 item 8; K-975). The beat, the outage record of a gap longer than two minutes and
/// the changes the return makes (<see cref="IOutageEffect"/>: the transfer orders whose window ran out in the gap are put off) are one
/// transaction: all of them commit or none does, so a heartbeat can never be fresh while its gap's effects are missing - and the scheduler's
/// rule "no transfer cancellation while the last heartbeat is older than two minutes" holds the cancellation back until they are there.
/// The beat is a conditional write on the value that was read: two instances beating together (an update's old and new process) write once and
/// the second does nothing.
/// </summary>
internal static class OutageRecovery
{
    /// <summary>Writes the beat of <paramref name="now"/>; returns whether it recorded an outage.</summary>
    public static async Task<bool> ApplyAsync(IServiceProvider services, DateTimeOffset now, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(services);
        ITransactionRunner runner = services.GetRequiredService<ITransactionRunner>();
        AppDbContext db = services.GetRequiredService<AppDbContext>();
        IReadOnlyList<IOutageEffect> effects = [.. services.GetServices<IOutageEffect>()];
        bool recorded = false;
        await runner.RunAsync(
            async cancel =>
            {
                DateTimeOffset? previous = await db.HeartbeatRecords.AsNoTracking().Select(h => h.LastHeartbeatAt).SingleAsync(cancel);

                // The beat is written first, on the value just read: a row another beat changed meanwhile is left alone and nothing else is written.
                int written = await db.HeartbeatRecords
                    .Where(h => h.LastHeartbeatAt == previous)
                    .ExecuteUpdateAsync(s => s.SetProperty(h => h.LastHeartbeatAt, now), cancel);
                if (written == 0)
                {
                    return new SchedulerStepResult();
                }

                if (previous is { } last && IsOutage(last, now))
                {
                    db.OutageRecords.Add(OutageRecord.Create(last, now, now));
                    foreach (IOutageEffect effect in effects)
                    {
                        await effect.ApplyAsync(last, now, cancel);
                    }

                    recorded = true;
                }

                return new SchedulerStepResult();
            },
            ct);
        return recorded;
    }

    /// <summary>A gap longer than two minutes is an outage; two minutes or less is not (05 5.4.1).</summary>
    public static bool IsOutage(DateTimeOffset lastHeartbeatAt, DateTimeOffset now)
    {
        return now - lastHeartbeatAt > SchedulerTimings.OutageThreshold;
    }

    /// <summary>Whether the last heartbeat is recent enough for a transfer order's cancellation to run (05 5.4.3: not older than two minutes; none yet is not recent).</summary>
    public static bool IsFresh(DateTimeOffset? lastHeartbeatAt, DateTimeOffset now)
    {
        return lastHeartbeatAt is { } last && now - last <= SchedulerTimings.OutageThreshold;
    }
}
