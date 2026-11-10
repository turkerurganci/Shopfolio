using Shopfolio.Infrastructure.Logging;

namespace Shopfolio.Infrastructure.Integrations;

/// <summary>
/// The common face of every outside call of the link layer (08 3.1 rules 2, 3 and 5; 05 10.1.1; MK-61; 09 11.1 items 3 and 4). It gives the
/// call its job's limit with a cancellation source bound to the time provider - a test moves the limit with a fake clock -, makes the call
/// once and never again, and turns a call without a classified answer into its family's class (<see cref="IExternalCallResult{TSelf}"/>): the
/// passed limit, the unreachable service, and - through <see cref="Failed{TResult}"/> - the answer the service's table does not list. There is
/// no retry and no circuit breaker. A failure writes one log line with the job, the kind and the reason code only: no request field, no
/// address, no key, no exception text and no field of the answer. A cancellation of the caller (the host stopping) is not a timeout and
/// passes up.
/// </summary>
internal sealed partial class ExternalCallRunner
{
    private readonly TimeProvider _time;
    private readonly ILogger<ExternalCallRunner> _logger;

    public ExternalCallRunner(TimeProvider time, ILogger<ExternalCallRunner> logger)
    {
        _time = time;
        _logger = logger;
    }

    /// <summary>The reason code of an unreachable service: the transport's error name (<see cref="HttpRequestError"/>), never its message.</summary>
    public static string ConnectionReason(HttpRequestError error) => "connection:" + error;

    /// <summary>Makes <paramref name="call"/> once within the limit of <paramref name="operation"/>.</summary>
    public async Task<TResult> RunAsync<TResult>(IntegrationOperation operation, Func<CancellationToken, Task<TResult>> call, CancellationToken ct)
        where TResult : IExternalCallResult<TResult>
    {
        ArgumentNullException.ThrowIfNull(call);
        using var limit = new CancellationTokenSource(IntegrationOperations.LimitOf(operation), _time);
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(limit.Token, ct);
        try
        {
            return await call(linked.Token);
        }
        catch (OperationCanceledException) when (limit.IsCancellationRequested && !ct.IsCancellationRequested)
        {
            return Failed<TResult>(operation, ExternalCallFailure.TimedOut());
        }
        catch (HttpRequestException exception)
        {
            return Failed<TResult>(operation, ExternalCallFailure.Unreachable(ConnectionReason(exception.HttpRequestError)));
        }
    }

    /// <summary>Writes the failure's log line and returns its family's class; an implementation calls it for an answer its table does not list.</summary>
    public TResult Failed<TResult>(IntegrationOperation operation, ExternalCallFailure failure)
        where TResult : IExternalCallResult<TResult>
    {
        WriteFailure(operation, failure);
        return TResult.FromFailure(failure);
    }

    /// <summary>
    /// Writes the failure's one log line only; an implementation calls it for an answer its table classes as "no answer" without the family's default class (08 2.1.6:
    /// the final query's request errors, whose interval is the code error rule's).
    /// </summary>
    public void WriteFailure(IntegrationOperation operation, ExternalCallFailure failure)
    {
        ArgumentNullException.ThrowIfNull(failure);
        LogFailed(_logger, LogEvents.ExternalCallFailed, operation, failure.Kind, failure.ReasonCode);
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "{Event} {Operation} {FailureKind} {Reason}")]
    private static partial void LogFailed(ILogger logger, string @event, IntegrationOperation operation, ExternalCallFailureKind failureKind, string reason);
}
