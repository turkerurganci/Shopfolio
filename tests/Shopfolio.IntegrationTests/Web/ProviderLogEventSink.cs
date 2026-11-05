using System.Text;

using Microsoft.Extensions.Logging;

using Serilog.Core;
using Serilog.Events;
using Serilog.Parsing;

namespace Shopfolio.IntegrationTests.Web;

/// <summary>
/// Hands the lines of the application's logger to a test's <see cref="ILoggerProvider"/> (T230; 05 9.2.3). The host does not forward its lines to
/// the framework's providers (the console would carry them into the container's output), so a test that counts lines cannot add a provider to the
/// host; it registers this sink, which the logging setup reads from the services, and the sink writes to the provider the test passed to the factory.
/// </summary>
public sealed class ProviderLogEventSink : ILogEventSink
{
    private readonly ILoggerFactory _factory;

    public ProviderLogEventSink(ILoggerProvider provider)
    {
        ArgumentNullException.ThrowIfNull(provider);
        _factory = LoggerFactory.Create(builder => builder.AddProvider(provider));
    }

    public void Emit(LogEvent logEvent)
    {
        ArgumentNullException.ThrowIfNull(logEvent);

        string category = logEvent.Properties.TryGetValue("SourceContext", out LogEventPropertyValue? context) && context is ScalarValue { Value: string name }
            ? name
            : string.Empty;
        ILogger logger = _factory.CreateLogger(category);
        LogLevel level = LevelOf(logEvent.Level);
        if (logger.IsEnabled(level))
        {
            logger.Log(level, default, Render(logEvent), logEvent.Exception, static (text, _) => text);
        }
    }

    // A string value is written as it is (the library's own rendering puts quotes round it), the way the framework's formatter writes it.
    private static string Render(LogEvent logEvent)
    {
        StringBuilder text = new();
        foreach (MessageTemplateToken token in logEvent.MessageTemplate.Tokens)
        {
            if (token is PropertyToken property && logEvent.Properties.TryGetValue(property.PropertyName, out LogEventPropertyValue? value))
            {
                text.Append(value is ScalarValue { Value: string literal } ? literal : value.ToString());
            }
            else
            {
                text.Append(token.ToString());
            }
        }

        return text.ToString();
    }

    private static LogLevel LevelOf(LogEventLevel level) => level switch
    {
        LogEventLevel.Verbose => LogLevel.Trace,
        LogEventLevel.Debug => LogLevel.Debug,
        LogEventLevel.Information => LogLevel.Information,
        LogEventLevel.Warning => LogLevel.Warning,
        LogEventLevel.Error => LogLevel.Error,
        _ => LogLevel.Critical,
    };
}
