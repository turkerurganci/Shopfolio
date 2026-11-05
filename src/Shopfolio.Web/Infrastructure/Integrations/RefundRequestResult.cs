namespace Shopfolio.Infrastructure.Integrations;

/// <summary>
/// The result of asking for a card refund (08 3.1 table, 2.1.9; 09 11.1 item 2). A sent request is never sent again: a request without an
/// answer is <see cref="Unknown"/> and its result is asked (08 3.1 rule 3; 05 10.1.1).
/// </summary>
internal abstract record RefundRequestResult : IExternalCallResult<RefundRequestResult>
{
    private RefundRequestResult()
    {
    }

    /// <summary>The provider accepted the refund.</summary>
    public sealed record Accepted : RefundRequestResult;

    /// <summary>The provider refused the refund; the system does not try again by itself.</summary>
    public sealed record NotCompleted(string ReasonCode) : RefundRequestResult;

    /// <summary>The result is unknown: no answer, the limit passed or the connection broke - the request is not sent again, the result is asked.</summary>
    public sealed record Unknown(string ReasonCode) : RefundRequestResult;

    /// <summary>
    /// A refund request without a classified answer has an unknown result: the timeout and the broken connection (08 2.1.9), and the answer that
    /// is not in the table too (08 3.1: a connection error in payment) - money may have moved, so the request is not sent again.
    /// </summary>
    public static RefundRequestResult FromFailure(ExternalCallFailure failure)
    {
        ArgumentNullException.ThrowIfNull(failure);
        return new Unknown(failure.ReasonCode);
    }
}
