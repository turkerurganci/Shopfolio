using Shopfolio.Infrastructure.Integrations;

namespace Shopfolio.UnitTests.Fakes;

/// <summary>
/// The three ways an outside call ends without a classified answer, as the fakes produce them (T204 criterion 1; 09 19.1 items 3 and 6; 08 3.1
/// rules 3 and 4): the passed limit, the unreachable service and the answer the service's table does not list. A fake never waits and never
/// leaves the process; it answers the class its family's <c>FromFailure</c> gives that failure - the same function the layer's runner uses
/// (<see cref="ExternalCallRunner"/>) - with the runner's reason codes, so a module test sees what the real gateway would hand it.
/// </summary>
internal static class FakeFailures
{
    /// <summary>The reason code of the fakes' unlisted answer: a short code, never a field of an answer (08 3.1 rule 5).</summary>
    public const string UnrecognizedReason = "unrecognized";

    /// <summary>Every kind, in the order of the enum: the timeout, the unreachable service, the unlisted answer.</summary>
    public static IReadOnlyList<ExternalCallFailureKind> AllKinds { get; } = Enum.GetValues<ExternalCallFailureKind>();

    /// <summary>The failure of <paramref name="kind"/> with the reason code the layer's runner writes for it.</summary>
    public static ExternalCallFailure Of(ExternalCallFailureKind kind)
    {
        return kind switch
        {
            ExternalCallFailureKind.TimedOut => ExternalCallFailure.TimedOut(),
            ExternalCallFailureKind.Unreachable => ExternalCallFailure.Unreachable(ExternalCallRunner.ConnectionReason(HttpRequestError.ConnectionError)),
            ExternalCallFailureKind.Unrecognized => ExternalCallFailure.Unrecognized(UnrecognizedReason),
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "Unknown failure kind."),
        };
    }

    /// <summary>The class of <typeparamref name="TResult"/>'s family that a call ending in <paramref name="kind"/> becomes.</summary>
    public static TResult ResultOf<TResult>(ExternalCallFailureKind kind)
        where TResult : IExternalCallResult<TResult>
    {
        return TResult.FromFailure(Of(kind));
    }
}
