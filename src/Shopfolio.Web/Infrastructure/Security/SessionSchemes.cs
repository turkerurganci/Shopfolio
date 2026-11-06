using System.Security.Claims;

namespace Shopfolio.Infrastructure.Security;

/// <summary>
/// The two identity schemes, their policies and the page folders they close (05 3.5.2, 6.1.3; 07 2.3.1; 09 15.1 item 2): the storefront's scheme knows only
/// customer accounts and reads <c>__Host-session</c>, the panel's scheme knows only administrator accounts and reads <c>__Host-admin-session</c>. The names
/// are written here once.
/// </summary>
internal static class SessionSchemes
{
    /// <summary>The customer scheme: the default scheme of the application (the open storefront pages read the customer identity, 09 15.1 item 2).</summary>
    public const string CustomerScheme = "Customer";

    /// <summary>The administrator scheme: the panel's.</summary>
    public const string AdminScheme = "Admin";

    /// <summary>The policy of the customer's pages: the customer scheme and an authenticated user.</summary>
    public const string CustomerPolicy = "CustomerPolicy";

    /// <summary>The policy of the panel: the administrator scheme and an authenticated user.</summary>
    public const string AdminPolicy = "AdminPolicy";

    /// <summary>The page folder of the panel; closed by default (05 6.2.2). Its pages' addresses start with <c>/panel</c>.</summary>
    public const string PanelFolder = "/Panel";

    /// <summary>The page folder of the customer's account; closed by default (05 6.2.3). Its pages' addresses start with <c>/hesap</c>.</summary>
    public const string AccountFolder = "/Account";

    /// <summary>The first path segment of the panel's pages (07 2.1.2).</summary>
    public const string PanelSegment = "panel";

    /// <summary>The first path segment of the account's pages (07 2.1.2).</summary>
    public const string AccountSegment = "hesap";

    /// <summary>The sign-in page of the storefront (07 2.3.2).</summary>
    public const string CustomerSignInPath = "/giris";

    /// <summary>The sign-in page of the panel (07 2.3.2).</summary>
    public const string AdminSignInPath = "/panel/giris";

    /// <summary>The claim that carries the id of the session row (09 15.1 item 2: the identity carries the account's internal key and the session's id, no e-mail or name).</summary>
    public const string SessionIdClaim = "session_id";

    /// <summary>
    /// The open pages of the panel (07 2.3.1), by address, with the parameter names left out: <c>/panel/sifre-sifirlama[/{key}]</c>, <c>/panel/davet/{key}</c>,
    /// <c>/panel/dogrulama/{key}</c> and <c>/panel/geri-al/{key}</c>. Every other page under <c>/panel</c> asks for the panel's session
    /// (<c>/panel/yeniden-dogrulama</c> too, 05 6.2.2). The list equals the one of 07 2.3.1 (a test compares them).
    /// </summary>
    public static readonly IReadOnlyList<string> OpenPanelAddresses =
        ["/panel/giris", "/panel/sifre-sifirlama", "/panel/sifre-sifirlama/{}", "/panel/davet/{}", "/panel/dogrulama/{}", "/panel/geri-al/{}"];

    /// <summary>The one open page under <c>/hesap</c> (07 2.3.1): the result state of an account deletion (07 4.25.7; K-1184).</summary>
    public static readonly IReadOnlyList<string> OpenAccountAddresses = ["/hesap/silindi"];

    /// <summary>The account's internal key the identity carries, or null when the principal holds no session identity.</summary>
    public static Guid? AccountId(this ClaimsPrincipal principal)
    {
        ArgumentNullException.ThrowIfNull(principal);
        string? value = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(value, out Guid id) ? id : null;
    }
}
