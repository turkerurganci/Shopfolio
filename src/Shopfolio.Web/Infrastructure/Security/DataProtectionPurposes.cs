namespace Shopfolio.Infrastructure.Security;

/// <summary>
/// The purpose strings of the framework's data protection (09 15.5 item 2): the one place that writes them. Each use of a protected value has its
/// own constant, shaped <c>Shopfolio.&lt;Use&gt;.v1</c>; the list is closed, and a new use enters it with a decision record. A value protected
/// for one purpose is not read under another. When the shape of a value changes the suffix goes up (<c>v2</c>) and the old value can no longer be
/// read; it is rejected without harm (09 15.1 item 9, 09 12 item 6). The framework's own purposes (anti-forgery, cookie chunking) are not in the list.
/// </summary>
internal static class DataProtectionPurposes
{
    /// <summary>The download link: the item's id and the end of its life (09 15.5 item 4; 05 7.7.4).</summary>
    public const string DownloadLink = "Shopfolio.DownloadLink.v1";

    /// <summary>The value of a pending registration: the pending record's inner key (09 15.5 item 5).</summary>
    public const string PendingRegistration = "Shopfolio.PendingRegistration.v1";

    /// <summary>The value of an invitation: the invitation's inner key (09 15.5 item 5).</summary>
    public const string AcceptedInvitation = "Shopfolio.AcceptedInvitation.v1";

    /// <summary>The flow cookie of the external sign-in: state, nonce, the PKCE verifier and the kind of the flow (09 15.1 item 7).</summary>
    public const string ExternalLoginFlow = "Shopfolio.ExternalLoginFlow.v1";

    /// <summary>The state value of the external sign-in: the return path, the reauthentication operation and the start moment (09 11.3).</summary>
    public const string ExternalLoginState = "Shopfolio.ExternalLoginState.v1";

    /// <summary>The cookie of the checkout step (05 6.7.9; 09 15.1 item 6).</summary>
    public const string CheckoutCookie = "Shopfolio.CheckoutCookie.v1";

    /// <summary>The closed list of the six purposes, in the order of the table in 09 15.5 item 2.</summary>
    public static readonly IReadOnlyList<string> All =
    [
        DownloadLink,
        PendingRegistration,
        AcceptedInvitation,
        ExternalLoginFlow,
        ExternalLoginState,
        CheckoutCookie,
    ];
}
