using System.Globalization;
using System.Text;

using Serilog.Events;
using Serilog.Parsing;

namespace Shopfolio.Infrastructure.Logging;

/// <summary>
/// The audit line writer and the command line writer (09 18.1 item 2, 5, 6): for each line it opens the day's file in append mode, writes the
/// whole line in one call, flushes the stream and closes it; every failure — an unwritable directory, a full disk, a locked file — <b>propagates to
/// the caller</b>. "Written" means handed to the operating system when <see cref="Write"/> returns; the disk's own write cache is out of scope
/// (09 18.1 item 5). The file name is <c>&lt;prefix&gt;-&lt;yyyyMMdd&gt;.jsonl</c>, the day in Turkey time (<see cref="TurkeyTime"/>; the library's
/// rolling writer names the normal file by the server zone, 09 18.1 item 2). Nothing here deletes or limits a file (09 18.1 item 3). The line format is the application's formatter.
/// </summary>
internal sealed class AuditLogWriter : IAuditLineWriter
{
    private const string DayFormat = "yyyyMMdd";

    private static readonly MessageTemplateParser _parser = new();

    private static readonly ApplicationLogFormatter _formatter = new();

    private static readonly UTF8Encoding _utf8 = new(encoderShouldEmitUTF8Identifier: false);

    private readonly string _directory;
    private readonly string _prefix;
    private readonly TimeProvider _time;
    private readonly Lock _gate = new();

    public AuditLogWriter(string directory, string prefix, TimeProvider time)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        ArgumentException.ThrowIfNullOrWhiteSpace(prefix);
        ArgumentNullException.ThrowIfNull(time);

        _directory = directory;
        _prefix = prefix;
        _time = time;
    }

    public void Write(AuditLine line)
    {
        ArgumentNullException.ThrowIfNull(line);

        // The clock is read once: the line's time and the file's day come from the same instant, so a line written at the Turkey midnight is
        // never in the file of one day with the time of the next.
        DateTimeOffset now = _time.GetUtcNow();
        byte[] bytes = _utf8.GetBytes(Format(line, now));

        lock (_gate)
        {
            Directory.CreateDirectory(_directory);
            string path = Path.Combine(_directory, $"{_prefix}-{TurkeyTime.DateOf(now).ToString(DayFormat, CultureInfo.InvariantCulture)}.{LoggingSetup.FileExtension}");

            using FileStream stream = new(path, FileMode.Append, FileAccess.Write, FileShare.Read);
            stream.Write(bytes);
            stream.Flush(flushToDisk: line.FlushToDisk);
        }
    }

    private static string Format(AuditLine line, DateTimeOffset now)
    {
        LogEvent logEvent = new(
            now,
            LogEventLevel.Information,
            exception: null,
            _parser.Parse(line.Template),
            [
                new LogEventProperty("SourceContext", new ScalarValue(typeof(AuditLogWriter).FullName)),
                new LogEventProperty("Event", new ScalarValue(line.EventName)),
                new LogEventProperty(line.NameKey, new ScalarValue(line.Name)),
                new LogEventProperty(line.IdKey, new ScalarValue(line.Id)),
                .. line.ClosedValues.Select(value => new LogEventProperty(value.Key, new ScalarValue(value.Value))),
            ]);

        using StringWriter text = new(CultureInfo.InvariantCulture);
        _formatter.Format(logEvent, text);
        return text.ToString();
    }
}
