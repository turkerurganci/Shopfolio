using System.Collections.Concurrent;

using Microsoft.Extensions.Logging;

namespace Shopfolio.IntegrationTests.Web;

/// <summary>Keeps the formatted log lines of a test application so a test can count what was written.</summary>
public sealed class CollectingLoggerProvider : ILoggerProvider
{
    private readonly ConcurrentQueue<string> _lines = new();

    public IReadOnlyList<string> Lines => [.. _lines];

    public ILogger CreateLogger(string categoryName) => new CollectingLogger(categoryName, _lines);

    public void Dispose()
    {
    }

    private sealed class CollectingLogger : ILogger
    {
        private readonly string _category;
        private readonly ConcurrentQueue<string> _lines;

        public CollectingLogger(string category, ConcurrentQueue<string> lines)
        {
            _category = category;
            _lines = lines;
        }

        public IDisposable? BeginScope<TState>(TState state)
            where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            _lines.Enqueue($"{_category}|{logLevel}|{formatter(state, exception)}");
        }
    }
}
