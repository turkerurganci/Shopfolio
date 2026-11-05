using System.Text.Json;

using Serilog;
using Serilog.Core;
using Serilog.Events;

using Shopfolio.Infrastructure.Logging;

namespace Shopfolio.UnitTests.Support;

/// <summary>Keeps the events of a library logger in memory, so a test formats them without a file (09 19.2 item 2: the unit tests write no file).</summary>
internal sealed class CollectingSink : ILogEventSink
{
    public List<LogEvent> Events { get; } = [];

    public void Emit(LogEvent logEvent) => Events.Add(logEvent);
}

internal static class LogCapture
{
    /// <summary>Runs <paramref name="write"/> against a logger that keeps everything and returns the one event it wrote.</summary>
    public static LogEvent One(Action<Serilog.ILogger> write, ILogEventEnricher? enricher = null)
    {
        CollectingSink sink = new();
        LoggerConfiguration configuration = new LoggerConfiguration().MinimumLevel.Verbose();
        if (enricher is not null)
        {
            configuration = configuration.Enrich.With(enricher);
        }

        using Logger logger = configuration.WriteTo.Sink(sink).CreateLogger();
        write(logger);
        return Assert.Single(sink.Events);
    }

    /// <summary>The text the application formatter makes of an event: one line, ending with a newline.</summary>
    public static string Line(LogEvent logEvent)
    {
        using StringWriter writer = new(System.Globalization.CultureInfo.InvariantCulture);
        new ApplicationLogFormatter().Format(logEvent, writer);
        return writer.ToString();
    }

    public static JsonDocument Json(LogEvent logEvent) => JsonDocument.Parse(Line(logEvent));
}
