using System.Globalization;
using System.Text.Json;

namespace Shopfolio.Infrastructure.Logging;

/// <summary>
/// The closed list of the kinds an instant deletion line carries (05 7.4.5; 09 18.2 item 5): the deletions that are not bound to a time and are written to the application log after the
/// deleting transaction commits - the customer's account (the member's own deletion and the firm's deletion on request, one kind: the same removal) and the administrator's removal.
/// <see cref="InstantDeletionLog"/> reads them back and the <c>reapply-deletions</c> command applies each one by the same removal path (05 8.5.10).
/// </summary>
internal static class InstantDeletionKinds
{
    /// <summary>A customer account removed by the member or by the firm on request (02 3.15.1, 3.15.5).</summary>
    public const string CustomerAccount = "CustomerAccount";

    /// <summary>An administrator account removed by another administrator (02 10.2.2).</summary>
    public const string AdminAccount = "AdminAccount";

    public static IReadOnlyList<string> All { get; } = [CustomerAccount, AdminAccount];
}

/// <summary>One instant deletion line: the moment it was written, its kind and the internal id of the deleted record (no field value, 09 18.2 item 5).</summary>
internal sealed record InstantDeletionEntry(DateTimeOffset At, string Kind, Guid RecordId);

/// <summary>What a read of the application log found: the deletion lines from the moment on, in the order they were written, and the number of lines that could not be read.</summary>
internal sealed record InstantDeletionReading(IReadOnlyList<InstantDeletionEntry> Entries, int UnreadableLines);

/// <summary>
/// The reader of the application log's instant deletion lines (05 8.5.10; 09 18.1 item 2). The application log lives in the log directory outside the database, in one file per Turkey day
/// (<c>audit-yyyyMMdd.jsonl</c>), so a database restore does not take it back. The read takes the files of the days from the restore moment's day to the last day asked for and keeps the
/// <c>InstantDeletion</c> lines written at or after the moment. A line that is not a JSON object with the line's fields is counted, never skipped silently, and its text is never reported.
/// </summary>
internal sealed class InstantDeletionLog
{
    private const string DayFormat = "yyyyMMdd";

    private readonly string _directory;
    private readonly TimeProvider _time;

    public InstantDeletionLog(IConfiguration configuration, TimeProvider time)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(time);
        _directory = LoggingOptions.FromConfiguration(configuration).Directory;
        _time = time;
    }

    /// <summary>The deletion lines from <paramref name="from"/> to this moment.</summary>
    public InstantDeletionReading ReadSince(DateTimeOffset from) => Read(_directory, from, _time.GetUtcNow());

    public static InstantDeletionReading Read(string directory, DateTimeOffset from, DateTimeOffset to)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        List<InstantDeletionEntry> entries = [];
        int unreadable = 0;

        // A day's file is named by the Turkey day of the line's moment (AuditLogWriter); the days are read oldest first so the lines come back in the order they were written.
        for (DateOnly day = TurkeyTime.DateOf(from); day <= TurkeyTime.DateOf(to); day = day.AddDays(1))
        {
            string path = Path.Combine(directory, $"{LoggingSetup.Prefix(AuditFile.Audit)}-{day.ToString(DayFormat, CultureInfo.InvariantCulture)}.{LoggingSetup.FileExtension}");
            if (!File.Exists(path))
            {
                continue;
            }

            foreach (string line in File.ReadLines(path))
            {
                if (line.Length == 0)
                {
                    continue;
                }

                switch (TryParse(line))
                {
                    case (false, _):
                        unreadable++;
                        break;
                    case (true, { } entry) when entry.At >= from:
                        entries.Add(entry);
                        break;
                }
            }
        }

        return new InstantDeletionReading(entries, unreadable);
    }

    // (false, null): not a line of the log's shape. (true, null): a line of another event. (true, entry): a deletion line.
    private static (bool Readable, InstantDeletionEntry? Entry) TryParse(string line)
    {
        try
        {
            using JsonDocument document = JsonDocument.Parse(line);
            JsonElement root = document.RootElement;
            if (root.ValueKind != JsonValueKind.Object
                || !root.TryGetProperty("event", out JsonElement eventName)
                || eventName.ValueKind != JsonValueKind.String)
            {
                return (false, null);
            }

            if (!string.Equals(eventName.GetString(), LogEvents.InstantDeletion, StringComparison.Ordinal))
            {
                return (true, null);
            }

            if (!root.TryGetProperty("time", out JsonElement time) || time.ValueKind != JsonValueKind.String
                || !DateTimeOffset.TryParse(time.GetString(), CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out DateTimeOffset at)
                || !root.TryGetProperty("kind", out JsonElement kind) || kind.ValueKind != JsonValueKind.String
                || !root.TryGetProperty("recordId", out JsonElement recordId) || recordId.ValueKind != JsonValueKind.String || !Guid.TryParse(recordId.GetString(), out Guid id))
            {
                return (false, null);
            }

            return (true, new InstantDeletionEntry(at.ToUniversalTime(), kind.GetString()!, id)); // non-null: the condition above makes kind a String element, whose GetString is never null
        }
        catch (JsonException)
        {
            return (false, null);
        }
    }
}
