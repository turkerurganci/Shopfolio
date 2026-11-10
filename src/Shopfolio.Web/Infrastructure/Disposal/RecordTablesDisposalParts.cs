using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure.Security;
using Shopfolio.Infrastructure.Values;

namespace Shopfolio.Infrastructure.Disposal;

/// <summary>
/// The sign-in records whose period is over (06 9.3.1; Z-40; P-45): six calendar months from the moment a sign-in is recorded, in Turkey time - the same
/// period and the same edge as the log files (<see cref="LogFileRetention.Cutoff"/>), so the two never part. The rows are deleted by the database, never
/// updated; the sign-in record's guard refuses an update (06 2.2.3).
/// </summary>
internal sealed class LoginRecordDisposalPart : IDisposalPart
{
    public string Name => "LoginRecords";

    public Task RunAsync(DisposalRun run, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(run);
        DateTimeOffset cutoff = LogFileRetention.Cutoff(run.Now);
        return run.ChunksAsync(
            DisposalRecordType.LoginRecord,
            async (db, max, cancel) =>
            {
                List<Guid> ids = await db.LoginRecords
                    .Where(l => l.CreatedAt < cutoff)
                    .OrderBy(l => l.CreatedAt)
                    .ThenBy(l => l.Id)
                    .Select(l => l.Id)
                    .Take(max)
                    .ToListAsync(cancel);
                return await db.LoginRecords.Where(l => ids.Contains(l.Id)).ExecuteDeleteAsync(cancel);
            },
            ct);
    }
}

/// <summary>
/// The disposal records whose period is over (06 9.3.1; Z-44): three calendar years from the row's moment, in Turkey time (<see cref="DisposalRecordPeriod"/>).
/// The part's own deletion is a row like any other: the chunk that deletes writes one row of type <see cref="DisposalRecordType.DisposalRecord"/> with the
/// number it deleted, in the same transaction, and that row is made at the run's moment, so it is never inside the period it records.
/// </summary>
internal sealed class DisposalRecordDisposalPart : IDisposalPart
{
    public string Name => "DisposalRecords";

    public Task RunAsync(DisposalRun run, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(run);
        DateTimeOffset cutoff = DisposalRecordPeriod.Cutoff(run.Now);
        return run.ChunksAsync(
            DisposalRecordType.DisposalRecord,
            async (db, max, cancel) =>
            {
                List<Guid> ids = await db.DisposalRecords
                    .Where(d => d.CreatedAt < cutoff)
                    .OrderBy(d => d.CreatedAt)
                    .ThenBy(d => d.Id)
                    .Select(d => d.Id)
                    .Take(max)
                    .ToListAsync(cancel);
                return await db.DisposalRecords.Where(d => ids.Contains(d.Id)).ExecuteDeleteAsync(cancel);
            },
            ct);
    }
}

/// <summary>
/// The period of the disposal record (06 9.1 Z-44; 02 4.2 Z-44; 05 7.4.2, 9.2.4): at least three years, kept as a legal period - not a product constant - with
/// the part that applies it (09 3.5 item 1; 06 9.1.2).
/// </summary>
internal static class DisposalRecordPeriod
{
    /// <summary>Z-44: a disposal record is kept this many calendar years from its moment.</summary>
    public const int Years = 3;

    /// <summary>The edge at <paramref name="now"/>: a disposal record made before the returned instant is past its period.</summary>
    public static DateTimeOffset Cutoff(DateTimeOffset now) => RetentionCutoff.YearsBefore(now, Years);
}
