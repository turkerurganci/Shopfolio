using System.Diagnostics;

namespace Shopfolio.Infrastructure.Integrations;

/// <summary>
/// The result of verifying an identity with Google (08 3.1 table, 2.3; 09 11.1 item 2, 11.3 item 4): success, failure, or unavailable - which
/// only the re-authentication tells apart. Comparing <see cref="Succeeded.AuthTime"/> and <see cref="Succeeded.Nonce"/> and matching the e-mail
/// to an account are the core's rules (08 3.1 rule 4). The subject identifier is not taken (K-1061).
/// </summary>
internal abstract record GoogleIdentityResult : IExternalCallResult<GoogleIdentityResult>
{
    private GoogleIdentityResult()
    {
    }

    public sealed record Succeeded(string Email, bool EmailVerified, string? Name, string? HostedDomain, DateTimeOffset? AuthTime, string Nonce)
        : GoogleIdentityResult;

    public sealed record Failed(string ReasonCode) : GoogleIdentityResult;

    public sealed record Unavailable(string ReasonCode) : GoogleIdentityResult;

    /// <summary>
    /// The timeout and the unreachable service are "unavailable" (09 11.3 item 4: no connection, the user-waiting limit passed); an answer
    /// that is not in the table is a failure (08 3.1: "Google'da başarısızdır").
    /// </summary>
    public static GoogleIdentityResult FromFailure(ExternalCallFailure failure)
    {
        ArgumentNullException.ThrowIfNull(failure);
        return failure.Kind switch
        {
            ExternalCallFailureKind.TimedOut => new Unavailable(failure.ReasonCode),
            ExternalCallFailureKind.Unreachable => new Unavailable(failure.ReasonCode),
            ExternalCallFailureKind.Unrecognized => new Failed(failure.ReasonCode),
            _ => throw new UnreachableException(),
        };
    }
}
