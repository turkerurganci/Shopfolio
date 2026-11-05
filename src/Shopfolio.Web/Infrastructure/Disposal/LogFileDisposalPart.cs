using System.Text.RegularExpressions;

using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure.Values;

namespace Shopfolio.Infrastructure.Disposal;

/// <summary>The two directories of the log files the disposal deletes: the application's three files and the reverse proxy's rotated access log (DEPLOY_RUNBOOK L.1).</summary>
internal sealed record LogDirectories(string Application, string Access)
{
    /// <summary>The reverse proxy's log directory as the application container mounts it (deploy/compose.yaml).</summary>
    public const string DefaultAccessDirectory = "/var/log/shopfolio/access";
}

/// <summary>
/// The periods of the system logs (05 9.2.1, 9.2.2; MK-65; Z-40; P-45): six calendar months from the moment a record is made, in Turkey time.
/// A file holds records until its last write, so it is expired when its last write is that old; the day in a file's name is not read (the
/// rotated access log carries the previous day's lines under the rotation day's name, D-91 K1).
/// </summary>
internal static partial class LogFileRetention
{
    // app-, audit- and command- files: the three prefixes of the logging setup and its extension (DEPLOY_RUNBOOK L.1 row 2).
    [GeneratedRegex(@"^(?:app|audit|command)-[0-9]{8}\.jsonl$", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 1000)]
    private static partial Regex ApplicationFileName();

    // The reverse proxy's rotated file: access.log-yyyyMMdd (DEPLOY_RUNBOOK L.3).
    [GeneratedRegex(@"^access\.log-[0-9]{8}$", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 1000)]
    private static partial Regex AccessFileName();

    /// <summary>The moment before which a record is expired at <paramref name="now"/>: six calendar months back in Turkey time.</summary>
    public static DateTimeOffset Cutoff(DateTimeOffset now) => now.ToOffset(TurkeyTime.Offset).AddMonths(-ProductConstants.SystemLogRetentionMonths);

    /// <summary>
    /// The newest Turkey day all of whose records are expired at <paramref name="now"/>: the day before the day the cutoff falls on. The server's
    /// own rotation deletes the logs of this day (and the older ones), and the disposal writes it down (05 9.2.2).
    /// </summary>
    public static DateOnly NewestExpiredDay(DateTimeOffset now) => TurkeyTime.DateOf(Cutoff(now)).AddDays(-1);

    /// <summary>Whether a file of the application's directory is one the application wrote (app, audit, command).</summary>
    public static bool IsApplicationFile(string fileName) => ApplicationFileName().IsMatch(fileName);

    /// <summary>Whether a file of the access log directory is a rotated one; the file being written (<c>access.log</c>) is never deleted.</summary>
    public static bool IsRotatedAccessFile(string fileName) => AccessFileName().IsMatch(fileName);

    /// <summary>The paths of the expired log files of both directories, in the order of their names.</summary>
    public static IReadOnlyList<string> ExpiredFiles(LogDirectories directories, DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(directories);
        DateTimeOffset cutoff = Cutoff(now);
        List<string> expired = [];
        Collect(directories.Application, IsApplicationFile, cutoff, expired);
        Collect(directories.Access, IsRotatedAccessFile, cutoff, expired);
        expired.Sort(StringComparer.Ordinal);
        return expired;
    }

    private static void Collect(string directory, Func<string, bool> isLogFile, DateTimeOffset cutoff, List<string> into)
    {
        if (!Directory.Exists(directory))
        {
            return;
        }

        foreach (string path in Directory.EnumerateFiles(directory))
        {
            string name = Path.GetFileName(path);
            if (isLogFile(name) && new DateTimeOffset(File.GetLastWriteTimeUtc(path), TimeSpan.Zero) < cutoff)
            {
                into.Add(path);
            }
        }
    }
}

/// <summary>
/// The system log files whose six months are over, and the server's own rotation (05 7.4.4, 9.2.2; 06 9.3.4; MK-65). Each chunk of files is
/// written to the disposal record first, with the record type and the number of files, and deleted after: a crash in between leaves a count
/// without its deletion and the next night deletes the file and writes it again - a surplus count is preferred to a missing one (K-985). The
/// server's logs (the operating system's journal, the database and backup logs) are deleted by their own rotation; every night the part writes
/// the days that rotation has deleted since the last row as one "deleted by the rotation" row with the range of dates (05 9.2.2).
/// </summary>
internal sealed class LogFileDisposalPart : IDisposalPart
{
    private readonly LogDirectories _directories;

    public LogFileDisposalPart(LogDirectories directories)
    {
        _directories = directories;
    }

    public string Name => "LogFiles";

    public async Task RunAsync(DisposalRun run, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(run);
        IReadOnlyList<string> expired = LogFileRetention.ExpiredFiles(_directories, run.Now);
        for (int start = 0; start < expired.Count; start += DisposalTimings.PartSize)
        {
            string[] chunk = [.. expired.Skip(start).Take(DisposalTimings.PartSize)];
            await run.RecordAsync(DisposalRecord.Counted(DisposalRecordType.LogFile, chunk.Length, run.Now), ct);
            foreach (string path in chunk)
            {
                ct.ThrowIfCancellationRequested();
                File.Delete(path);
            }
        }

        await WriteServerRotationAsync(run, ct);
    }

    private static async Task WriteServerRotationAsync(DisposalRun run, CancellationToken ct)
    {
        DateOnly newest = LogFileRetention.NewestExpiredDay(run.Now);
        DateOnly? last = await run.Db.DisposalRecords.AsNoTracking()
            .Where(r => r.DisposalRecordType == DisposalRecordType.ServerLogRotation)
            .MaxAsync(r => r.RangeEndDate, ct);
        if (last is { } writtenUpTo && writtenUpTo >= newest)
        {
            return;
        }

        DateOnly first = last is { } previous ? previous.AddDays(1) : newest;
        await run.RecordAsync(DisposalRecord.ServerLogRotation(first, newest, run.Now), ct);
    }
}
