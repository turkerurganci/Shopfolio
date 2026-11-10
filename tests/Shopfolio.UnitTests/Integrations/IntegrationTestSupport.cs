using System.Diagnostics;

using Serilog;
using Serilog.Core;
using Serilog.Extensions.Logging;

using Shopfolio.Infrastructure.Integrations;
using Shopfolio.UnitTests.Fakes;
using Shopfolio.UnitTests.Support;

namespace Shopfolio.UnitTests.Integrations;

/// <summary>A runner on a fake clock whose log lines are kept in memory (09 19.2 item 2: no file is written).</summary>
internal sealed class RunnerHarness : IDisposable
{
    private readonly Logger _root;
    private readonly SerilogLoggerFactory _factory;

    public RunnerHarness()
    {
        _root = new LoggerConfiguration().MinimumLevel.Verbose().WriteTo.Sink(Sink).CreateLogger();
        _factory = new SerilogLoggerFactory(_root);
        Runner = new ExternalCallRunner(Time, new Microsoft.Extensions.Logging.Logger<ExternalCallRunner>(_factory));
    }

    public static readonly DateTimeOffset Now = new(2026, 11, 5, 9, 0, 0, TimeSpan.Zero);

    public ManualTimeProvider Time { get; } = new(Now);

    public CollectingSink Sink { get; } = new();

    public ExternalCallRunner Runner { get; }

    /// <summary>The application formatter's lines of everything written so far.</summary>
    public IReadOnlyList<string> Lines => [.. Sink.Events.Select(LogCapture.Line)];

    /// <summary>A logger of the same in-memory sink, for a class of the layer that writes its own line (T402: the failed payment's reason).</summary>
    public Microsoft.Extensions.Logging.ILogger<T> LoggerFor<T>() => new Microsoft.Extensions.Logging.Logger<T>(_factory);

    public void Dispose()
    {
        _factory.Dispose();
        _root.Dispose();
    }
}

/// <summary>
/// A hand-written HTTP handler for the layer tests: it counts the requests and answers each one as told - hangs until cancelled, refuses the
/// connection, or answers with a status. It never opens a socket.
/// </summary>
internal sealed class ScriptedHandler : HttpMessageHandler
{
    private int _requests;

    public enum Behavior
    {
        Hang,
        Refuse,
        Answer,
    }

    public Behavior Mode { get; set; } = Behavior.Answer;

    public System.Net.HttpStatusCode Status { get; set; } = System.Net.HttpStatusCode.OK;

    public string? Body { get; set; }

    public int Requests => Volatile.Read(ref _requests);

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Interlocked.Increment(ref _requests);
        switch (Mode)
        {
            case Behavior.Hang:
                TaskCompletionSource never = new(TaskCreationOptions.RunContinuationsAsynchronously);
                await using (cancellationToken.Register(() => never.TrySetCanceled(cancellationToken)))
                {
                    await never.Task;
                }

                throw new UnreachableException();
            case Behavior.Refuse:
                throw new HttpRequestException(HttpRequestError.ConnectionError, "refused");
            case Behavior.Answer:
                return new HttpResponseMessage(Status) { Content = new StringContent(Body ?? string.Empty) };
            default:
                throw new UnreachableException();
        }
    }
}
