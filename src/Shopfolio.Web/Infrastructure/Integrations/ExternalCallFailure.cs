namespace Shopfolio.Infrastructure.Integrations;

/// <summary>
/// A call that ended without a classified answer, with its reason (08 3.1 rule 5): the reason is a short code - <c>timeout</c>, the transport's
/// error name, or the service's own error code - and never a request field's value, an address, a key or a field of the answer that the
/// service's field mapping does not read.
/// </summary>
internal sealed record ExternalCallFailure(ExternalCallFailureKind Kind, string ReasonCode)
{
    /// <summary>The reason code of a call whose limit passed.</summary>
    public const string TimeoutReason = "timeout";

    public static ExternalCallFailure TimedOut() => new(ExternalCallFailureKind.TimedOut, TimeoutReason);

    public static ExternalCallFailure Unreachable(string reasonCode) => new(ExternalCallFailureKind.Unreachable, reasonCode);

    public static ExternalCallFailure Unrecognized(string reasonCode) => new(ExternalCallFailureKind.Unrecognized, reasonCode);
}
