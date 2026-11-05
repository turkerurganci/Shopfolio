using System.Diagnostics;

using Shopfolio.Infrastructure.Integrations;

namespace Shopfolio.Modules.Membership;

/// <summary>The two flows that verify an identity with Google (08 2.3): sign-in and registration share one, the re-authentication is the other.</summary>
internal enum GoogleIdentityFlow
{
    SignInOrRegistration,

    Reauthentication,
}

/// <summary>
/// The notice codes a failed Google verification leads to (07 6.3.2), named exactly as there. They join the closed <c>NoticeCode</c> list when
/// T214 adds it (D-109); until then this enum is the rule's own answer and carries no other code.
/// </summary>
internal enum GoogleIdentityFailure
{
    SocialLoginFailed,

    SocialReauthenticationUnavailable,

    SocialReauthenticationFailed,
}

/// <summary>
/// The core's rule over the layer's answer when Google cannot be used (08 2.3 error table, 3.1, 3.3; 05 10.1.5; 03 10.1.3): Google is
/// unavailable - no connection, the 15 second limit, an error answer - so sign-in and registration end with <c>SocialLoginFailed</c> and the
/// re-authentication with <c>SocialReauthenticationUnavailable</c>; a failed verification (an answer not in the table included, 08 3.1) ends
/// sign-in with <c>SocialLoginFailed</c> and the re-authentication with <c>SocialReauthenticationFailed</c>. Nothing is retried: the user
/// starts the flow again, and a spent authorization code is never sent twice (08 3.1 table, "yok"). A success goes on to the core's own checks
/// (the verified e-mail, <c>auth_time</c>, the account match) - those are the sign-in tasks' (T343, T409) and are not decided here.
/// </summary>
internal static class GoogleIdentityFailureRule
{
    /// <summary>The notice code of a failed verification, or <c>null</c> when Google verified the identity.</summary>
    public static GoogleIdentityFailure? FailureOf(GoogleIdentityFlow flow, GoogleIdentityResult answer)
    {
        ArgumentNullException.ThrowIfNull(answer);
        bool reauthentication = flow switch
        {
            GoogleIdentityFlow.SignInOrRegistration => false,
            GoogleIdentityFlow.Reauthentication => true,
            _ => throw new ArgumentOutOfRangeException(nameof(flow), flow, "Unknown Google flow."),
        };

        return answer switch
        {
            GoogleIdentityResult.Succeeded => null,
            GoogleIdentityResult.Unavailable => reauthentication ? GoogleIdentityFailure.SocialReauthenticationUnavailable : GoogleIdentityFailure.SocialLoginFailed,
            GoogleIdentityResult.Failed => reauthentication ? GoogleIdentityFailure.SocialReauthenticationFailed : GoogleIdentityFailure.SocialLoginFailed,
            _ => throw new UnreachableException(),
        };
    }
}
