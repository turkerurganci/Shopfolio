namespace Shopfolio.Infrastructure.Integrations;

/// <summary>
/// The result of sending a message (08 3.1 table, 2.2.2; 05 10.2.2; 09 11.1 item 2): sent, a recipient error (the suspended recipient included)
/// or a channel error. The recipient error belongs to the one recipient; the channel error to the channel. <c>ReasonCode</c> is the service's
/// error code, never a request field.
/// </summary>
internal abstract record EmailSendResult : IExternalCallResult<EmailSendResult>
{
    private EmailSendResult()
    {
    }

    public sealed record Sent : EmailSendResult;

    public sealed record RecipientError(string ReasonCode) : EmailSendResult;

    public sealed record ChannelError(string ReasonCode) : EmailSendResult;

    /// <summary>A send without a classified answer is a channel error: the timeout, the unreachable service and the unlisted answer (08 2.2.2).</summary>
    public static EmailSendResult FromFailure(ExternalCallFailure failure)
    {
        ArgumentNullException.ThrowIfNull(failure);
        return new ChannelError(failure.ReasonCode);
    }
}
