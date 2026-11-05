namespace Shopfolio.Infrastructure.Integrations;

/// <summary>
/// The result of starting a payment (08 3.1 table, 2.1.8; 09 11.1 item 2). The connection error (the payment method is unavailable; a set-up or
/// code error is counted in it) is told apart from the second token refusal, which is not a connection error; a failed payment is the
/// notification's result, never this call's. What the order does with each class is the core's rule (08 3.1 rule 4).
/// </summary>
internal abstract record PaymentStartResult : IExternalCallResult<PaymentStartResult>
{
    private PaymentStartResult()
    {
    }

    /// <summary>The payment page may open: the address of the provider's form, which the page shows in its frame. The token is not kept.</summary>
    public sealed record Started(Uri FrameAddress) : PaymentStartResult;

    /// <summary>The payment method is unavailable: no connection, the 15 second limit, a server error, a set-up or code error, or an answer not in the table.</summary>
    public sealed record ConnectionFailed(string ReasonCode) : PaymentStartResult;

    /// <summary>The provider refused a second token for the same order reference (08 2.1.7 (1)): not a connection error.</summary>
    public sealed record SecondTokenRejected(string ReasonCode) : PaymentStartResult;

    /// <summary>A start without a classified answer is a connection error, whatever its kind (08 2.1.8: the timeout and the unlisted answer included).</summary>
    public static PaymentStartResult FromFailure(ExternalCallFailure failure)
    {
        ArgumentNullException.ThrowIfNull(failure);
        return new ConnectionFailed(failure.ReasonCode);
    }
}
