using System.Globalization;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure.Disposal;
using Shopfolio.Infrastructure.Values;

namespace Shopfolio.Infrastructure.LogRead;

/// <summary>The three sources of the read command (05 9.3.1, 8.5.5; 09 13.2 item 3): the sign-in record, the system logs and the disposal record.</summary>
internal enum LogSource
{
    Login,
    System,
    Disposal,
}

/// <summary>
/// The filter of the read command (05 9.3.1; 09 13.2 item 3): the event type, the request id, the account's internal id, the IP or the account's e-mail. The IP and the e-mail are
/// personal data: they never come as an argument and are read from the terminal; the e-mail is turned into the account's internal id where the source carries ids (the system logs carry
/// no e-mail - 05 9.1.3) and into its normalized form where the source keeps the entered e-mail (the sign-in record - 06 4.44).
/// </summary>
internal enum LogFilterKind
{
    None,
    Event,
    Request,
    Account,
    Ip,
    Email,
}

/// <summary>
/// What the read command reads: the source, the Turkey days from <see cref="From"/> to <see cref="To"/> (both whole; 09 13.2 item 3) and one filter. <see cref="Value"/> is the filter's value
/// as the reader compares it: the event type, the request id, the account id (<c>D</c> format), the IP's text or the normalized e-mail; empty when there is no filter.
/// </summary>
internal sealed record LogReadRequest(LogSource Source, DateOnly From, DateOnly To, LogFilterKind Kind, string? Value)
{
    public DateTimeOffset Start => TurkeyTime.StartOf(From);

    public DateTimeOffset End => TurkeyTime.EndOfRange(To);
}

/// <summary>The outcome of a read: the rows written to the file and, for the system logs, the lines that could not be read (no JSON, no time).</summary>
internal sealed record LogReadSummary(int Rows, int UnreadableLines);

/// <summary>
/// The reader of the three sources (05 9.3.1, 9.3.3, 8.5.5; 06 4.44, 4.64; MK-66): the records in the database are read with the connection the command container is given - the read-only
/// user's in an installation (05 7.1.3; DEPLOY_RUNBOOK B) -, the log files line by line with no change to them (opened for reading, shared with their writer). The result is a CSV file
/// (<see cref="LogCsv"/>) written row by row, so no source is taken whole into memory (09 20 item 2). The panel has no screen of these sources; this reader is used only by the
/// <c>read-logs</c> command (05 9.3.1; a source scan keeps it so).
/// </summary>
internal sealed partial class LogReader
{
    private readonly AppDbContext _db;
    private readonly LogDirectories _directories;
    private readonly ServerLogDirectories _serverDirectories;

    public LogReader(AppDbContext db, LogDirectories directories, ServerLogDirectories serverDirectories)
    {
        _db = db;
        _directories = directories;
        _serverDirectories = serverDirectories;
    }

    public async Task<LogReadSummary> WriteCsvAsync(LogReadRequest request, TextWriter output, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(output);
        return request.Source switch
        {
            LogSource.Login => await WriteLoginRecordsAsync(request, output, ct),
            LogSource.Disposal => await WriteDisposalRecordsAsync(request, output, ct),
            LogSource.System => await WriteSystemLogsAsync(request, output, ct),
            _ => throw new ArgumentOutOfRangeException(nameof(request)),
        };
    }

    private async Task<LogReadSummary> WriteLoginRecordsAsync(LogReadRequest request, TextWriter output, CancellationToken ct)
    {
        DateTimeOffset start = request.Start;
        DateTimeOffset end = request.End;
        // One filter at most; the others stay null and drop out of the query. The record's type is not named as a type argument here: the sign-in record has one writer (D-64).
        IPAddress? ip = null;
        string? email = null;
        LoginOutcome? outcome = null;
        switch (request.Kind)
        {
            case LogFilterKind.None:
                break;
            case LogFilterKind.Ip:
                ip = IPAddress.Parse(request.Value!); // non-null: ReadLogsCommand, the one caller, sends the IP kind only with the address it read and parsed
                break;
            case LogFilterKind.Email:
                email = request.Value!; // non-null: ReadLogsCommand, the one caller, sends the e-mail kind only with the normalized address it read
                break;
            case LogFilterKind.Event:
                outcome = Enum.Parse<LoginOutcome>(request.Value!); // non-null: ReadLogsCommand.ParseFilter gives the event kind only with a plain token as its value
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(request), "The sign-in record has no such filter.");
        }

        LogCsv.WriteHeader(output, "Zaman", "IP", "Hesap türü", "E-posta", "Sonuç");
        int count = 0;
        await foreach (Security.LoginRecord row in _db.LoginRecords.AsNoTracking()
            .Where(l => l.CreatedAt >= start && l.CreatedAt < end)
            .Where(l => ip == null || l.IpAddress == ip)
            .Where(l => email == null || l.NormalizedEmail == email)
            .Where(l => outcome == null || l.Outcome == outcome)
            .OrderBy(l => l.CreatedAt).ThenBy(l => l.Id).AsAsyncEnumerable().WithCancellation(ct))
        {
            LogCsv.WriteRow(output, LogCsv.Moment(row.CreatedAt), row.IpAddress.ToString(), row.AccountType.ToString(), row.NormalizedEmail, row.Outcome.ToString());
            count++;
        }

        return new LogReadSummary(count, 0);
    }

    private async Task<LogReadSummary> WriteDisposalRecordsAsync(LogReadRequest request, TextWriter output, CancellationToken ct)
    {
        DateTimeOffset start = request.Start;
        DateTimeOffset end = request.End;
        IQueryable<DisposalRecord> rows = _db.DisposalRecords.AsNoTracking().Where(d => d.CreatedAt >= start && d.CreatedAt < end);
        switch (request.Kind)
        {
            case LogFilterKind.None:
                break;
            case LogFilterKind.Event:
                DisposalRecordType type = Enum.Parse<DisposalRecordType>(request.Value!); // non-null: ReadLogsCommand.ParseFilter gives the event kind only with a plain token as its value
                rows = rows.Where(d => d.DisposalRecordType == type);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(request), "The disposal record has no such filter.");
        }

        LogCsv.WriteHeader(output, "Zaman", "Kayıt türü", "Sayı", "Aralık başı", "Aralık sonu");
        int count = 0;
        await foreach (DisposalRecord row in rows.OrderBy(d => d.CreatedAt).ThenBy(d => d.Id).AsAsyncEnumerable().WithCancellation(ct))
        {
            LogCsv.WriteRow(
                output,
                LogCsv.Moment(row.CreatedAt),
                row.DisposalRecordType.ToString(),
                row.Count?.ToString(CultureInfo.InvariantCulture),
                row.RangeStartDate?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                row.RangeEndDate?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
            count++;
        }

        return new LogReadSummary(count, 0);
    }

    // The three sources of the system logs (05 9.1.1): the log files - the application's (app-, audit- and command-, each line a JSON object with "time") and the reverse proxy's
    // access log (each line a JSON object with "ts", seconds since 1970) - and the server's logs - the database's and the backup tool's text files (ServerTextLine) and the operating
    // system's journal (JournalFile). A line is in the range by its own time; a line with no time is counted as unreadable and not written.
    private async Task<LogReadSummary> WriteSystemLogsAsync(LogReadRequest request, TextWriter output, CancellationToken ct)
    {
        LogCsv.WriteHeader(output, "Dosya", "Zaman", "Satır");
        (int count, int unreadable) = await WriteLogFilesAsync(request, output, ct);
        (int serverCount, int serverUnreadable) = await WriteServerTextFilesAsync(request, output, ct);
        (int journalCount, int journalUnreadable) = WriteJournal(request, output, ct);
        return new LogReadSummary(count + serverCount + journalCount, unreadable + serverUnreadable + journalUnreadable);
    }

    private async Task<(int Rows, int Unreadable)> WriteServerTextFilesAsync(LogReadRequest request, TextWriter output, CancellationToken ct)
    {
        int count = 0;
        int unreadable = 0;
        foreach ((string label, string path) in ServerTextFiles())
        {
            await using FileStream stream = new(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
            using StreamReader reader = new(stream, Encoding.UTF8);
            DateTimeOffset? time = null;
            while (await reader.ReadLineAsync(ct) is { } line)
            {
                if (line.Length == 0)
                {
                    continue;
                }

                time = ServerTextLine.TimeOf(line, time);
                if (time is not { } moment)
                {
                    unreadable++;
                    continue;
                }

                if (moment >= request.Start && moment < request.End && MatchesServerLine(request, line, null, null))
                {
                    LogCsv.WriteRow(output, label, LogCsv.Moment(moment), line);
                    count++;
                }
            }
        }

        return (count, unreadable);
    }

    private (int Rows, int Unreadable) WriteJournal(LogReadRequest request, TextWriter output, CancellationToken ct)
    {
        int count = 0;
        int unreadable = 0;
        foreach (string path in JournalFiles())
        {
            string label = "journal/" + Path.GetRelativePath(_serverDirectories.Journal, path).Replace('\\', '/');
            foreach (JournalEntry? entry in JournalFile.ReadEntries(path))
            {
                ct.ThrowIfCancellationRequested();
                if (entry is null)
                {
                    unreadable++;
                    continue;
                }

                if (entry.Time >= request.Start && entry.Time < request.End && MatchesServerLine(request, entry.Line, entry.Identifier, entry.Unit))
                {
                    LogCsv.WriteRow(output, label, LogCsv.Moment(entry.Time), entry.Line);
                    count++;
                }
            }
        }

        return (count, unreadable);
    }

    // A server line has no event field: the event filter is the journal's program (SYSLOG_IDENTIFIER, e.g. sshd) or unit (docker.service); the request id, the account id and the IP
    // match a whole token of the line.
    private static bool MatchesServerLine(LogReadRequest request, string line, string? identifier, string? unit) => request.Kind switch
    {
        LogFilterKind.None => true,
        LogFilterKind.Event => request.Value is not null
            && (string.Equals(identifier, request.Value, StringComparison.Ordinal) || string.Equals(unit, request.Value, StringComparison.Ordinal)),
        LogFilterKind.Request or LogFilterKind.Account or LogFilterKind.Ip => request.Value is not null && ServerTextLine.ContainsToken(line, request.Value),
        _ => throw new ArgumentOutOfRangeException(nameof(request), "The system logs are filtered by an account id, not an e-mail."),
    };

    // The database's volume: the server's log and pgBackRest's (DEPLOY_RUNBOOK L.1 rows 4, 5); the backup tool's volume (row 6). The live file and the rotated ones (-yyyyMMdd).
    private IEnumerable<(string Label, string Path)> ServerTextFiles()
    {
        IEnumerable<(string, string)> database = Directory.Exists(_serverDirectories.Database)
            ? Directory.EnumerateFiles(_serverDirectories.Database, "*", SearchOption.AllDirectories)
                .Where(p => ServerFileName().IsMatch(Path.GetFileName(p)))
                .Select(p => ("db/" + Path.GetRelativePath(_serverDirectories.Database, p).Replace('\\', '/'), p))
            : [];
        IEnumerable<(string, string)> backup = Directory.Exists(_serverDirectories.Backup)
            ? Directory.EnumerateFiles(_serverDirectories.Backup, "*", SearchOption.AllDirectories)
                .Where(p => ServerFileName().IsMatch(Path.GetFileName(p)))
                .Select(p => ("backup/" + Path.GetRelativePath(_serverDirectories.Backup, p).Replace('\\', '/'), p))
            : [];
        return database.Concat(backup).OrderBy(f => f.Item1, StringComparer.Ordinal);
    }

    // The journal's files under the machine's directory: the active ones (*.journal) and the archived ones journald renamed after an unclean stop (*.journal~).
    private IEnumerable<string> JournalFiles() => Directory.Exists(_serverDirectories.Journal)
        ? Directory.EnumerateFiles(_serverDirectories.Journal, "*", SearchOption.AllDirectories)
            .Where(p => p.EndsWith(".journal", StringComparison.Ordinal) || p.EndsWith(".journal~", StringComparison.Ordinal))
            .Order(StringComparer.Ordinal)
        : [];

    private async Task<(int Rows, int Unreadable)> WriteLogFilesAsync(LogReadRequest request, TextWriter output, CancellationToken ct)
    {
        int count = 0;
        int unreadable = 0;
        foreach (string path in SystemLogFiles())
        {
            await using FileStream stream = new(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
            using StreamReader reader = new(stream, Encoding.UTF8);
            string fileName = Path.GetFileName(path);
            while (await reader.ReadLineAsync(ct) is { } line)
            {
                if (line.Length == 0)
                {
                    continue;
                }

                SystemLine? parsed = SystemLine.TryParse(line);
                if (parsed is null)
                {
                    unreadable++;
                    continue;
                }

                if (parsed.Time >= request.Start && parsed.Time < request.End && parsed.Matches(request))
                {
                    LogCsv.WriteRow(output, fileName, LogCsv.Moment(parsed.Time), line);
                    count++;
                }
            }
        }

        return (count, unreadable);
    }

    private IEnumerable<string> SystemLogFiles()
    {
        IEnumerable<string> application = Directory.Exists(_directories.Application)
            ? Directory.EnumerateFiles(_directories.Application).Where(p => ApplicationFileName().IsMatch(Path.GetFileName(p)))
            : [];
        IEnumerable<string> access = Directory.Exists(_directories.Access)
            ? Directory.EnumerateFiles(_directories.Access).Where(p => AccessFileName().IsMatch(Path.GetFileName(p)))
            : [];
        return application.Concat(access).Order(StringComparer.Ordinal);
    }

    // The names the logging setup and the proxy's rotation give (LogFileRetention; DEPLOY_RUNBOOK L.1, L.3), the live access.log included.
    [GeneratedRegex(@"^(?:app|audit|command)-[0-9]{8}\.jsonl$", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 1000)]
    private static partial Regex ApplicationFileName();

    [GeneratedRegex(@"^access\.log(?:-[0-9]{8})?$", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 1000)]
    private static partial Regex AccessFileName();

    // postgresql.log, pgBackRest's <stanza>-<command>.log, the backup tool's file-backup.log and their rotated names (DEPLOY_RUNBOOK L.3: dateext -yyyyMMdd, nocompress).
    [GeneratedRegex(@"^[A-Za-z0-9._-]+\.log(?:-[0-9]{8})?$", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 1000)]
    private static partial Regex ServerFileName();

    /// <summary>
    /// One line of a system log: its time and its string values. The event filter is the application's <c>event</c> field; the request id, the account id and the IP match any string
    /// value of the line (the application writes them under their own names - <c>requestId</c>, <c>accountId</c>, <c>adminId</c>… -, the proxy as <c>request_id</c> and <c>remote_ip</c>).
    /// </summary>
    internal sealed class SystemLine
    {
        private readonly HashSet<string> _values;

        private SystemLine(DateTimeOffset time, string? eventName, HashSet<string> values)
        {
            Time = time;
            EventName = eventName;
            _values = values;
        }

        public DateTimeOffset Time { get; }

        public string? EventName { get; }

        public static SystemLine? TryParse(string line)
        {
            try
            {
                using JsonDocument document = JsonDocument.Parse(line);
                JsonElement root = document.RootElement;
                if (root.ValueKind != JsonValueKind.Object)
                {
                    return null;
                }

                DateTimeOffset? time = ReadTime(root);
                if (time is null)
                {
                    return null;
                }

                HashSet<string> values = new(StringComparer.Ordinal);
                Collect(root, values);
                string? eventName = root.TryGetProperty("event", out JsonElement e) && e.ValueKind == JsonValueKind.String ? e.GetString() : null;
                return new SystemLine(time.Value, eventName, values);
            }
            catch (JsonException)
            {
                return null;
            }
        }

        public bool Matches(LogReadRequest request)
        {
            return request.Kind switch
            {
                LogFilterKind.None => true,
                LogFilterKind.Event => string.Equals(EventName, request.Value, StringComparison.Ordinal),
                LogFilterKind.Request or LogFilterKind.Account or LogFilterKind.Ip => request.Value is not null && _values.Contains(request.Value),
                _ => throw new ArgumentOutOfRangeException(nameof(request), "The system logs are filtered by an account id, not an e-mail."),
            };
        }

        private static DateTimeOffset? ReadTime(JsonElement root)
        {
            if (root.TryGetProperty("time", out JsonElement time) && time.ValueKind == JsonValueKind.String
                && DateTimeOffset.TryParse(time.GetString(), CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out DateTimeOffset moment))
            {
                return moment;
            }

            if (root.TryGetProperty("ts", out JsonElement ts) && ts.ValueKind == JsonValueKind.Number && ts.TryGetDouble(out double seconds)
                && seconds is >= 0 and < 253402300799)
            {
                return DateTimeOffset.UnixEpoch.AddTicks((long)(seconds * TimeSpan.TicksPerSecond));
            }

            return null;
        }

        private static void Collect(JsonElement element, HashSet<string> values)
        {
            switch (element.ValueKind)
            {
                case JsonValueKind.Object:
                    foreach (JsonProperty property in element.EnumerateObject())
                    {
                        Collect(property.Value, values);
                    }

                    break;
                case JsonValueKind.Array:
                    foreach (JsonElement item in element.EnumerateArray())
                    {
                        Collect(item, values);
                    }

                    break;
                case JsonValueKind.String:
                    values.Add(element.GetString()!); // non-null: GetString of a String element is never null
                    break;
                default:
                    break;
            }
        }
    }
}

/// <summary>
/// The CSV form of the read command's file (05 9.3.3), the same form as the panel's exports (09 21 items 2, 3): UTF-8 with a BOM, a semicolon between cells, CRLF at the end of a row;
/// a cell with a semicolon, a quote, CR or LF is quoted and its quotes doubled; a cell that starts with <c>=</c>, <c>+</c>, <c>-</c>, <c>@</c>, a tab or CR gets a leading apostrophe first,
/// so a spreadsheet does not run it as a formula. A moment is written in Turkey time with its seconds (<c>yyyy-MM-dd HH:mm:ss</c>): a log is read to the second.
/// </summary>
internal static class LogCsv
{
    public static readonly UTF8Encoding Encoding = new(encoderShouldEmitUTF8Identifier: true);

    public static void WriteHeader(TextWriter output, params string?[] cells) => WriteRow(output, cells);

    public static void WriteRow(TextWriter output, params string?[] cells)
    {
        ArgumentNullException.ThrowIfNull(output);
        ArgumentNullException.ThrowIfNull(cells);
        for (int i = 0; i < cells.Length; i++)
        {
            if (i > 0)
            {
                output.Write(';');
            }

            output.Write(Cell(cells[i]));
        }

        output.Write("\r\n");
    }

    public static string Moment(DateTimeOffset moment) => moment.ToOffset(TurkeyTime.Offset).ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);

    public static string Cell(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        string text = value[0] is '=' or '+' or '-' or '@' or '\t' or '\r' ? "'" + value : value;
        return text.AsSpan().IndexOfAny(";\"\r\n") >= 0 ? "\"" + text.Replace("\"", "\"\"", StringComparison.Ordinal) + "\"" : text;
    }
}
