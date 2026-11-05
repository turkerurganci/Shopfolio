namespace Shopfolio.Infrastructure.Security;

/// <summary>
/// The closed list of the cookie names (09 5.3 item 5; 07 2; 02 12.2.5): the one place that writes them. Every name carries the <c>__Host-</c>
/// prefix, which the browser accepts only for a cookie that is Secure, has no Domain and has the path <c>/</c> (05 6.7.1; <see cref="CookieDefaults"/>).
/// A name is written as a string nowhere else; a new name enters the list with the inventory of 02 12.2.5 and 07 2 (09 15.1 item 9).
/// </summary>
internal static class CookieNames
{
    /// <summary>The storefront's session cookie: the random session key (05 6.7.2).</summary>
    public const string Session = "__Host-session";

    /// <summary>The panel's session cookie: the same shape under its own name (05 6.7.3).</summary>
    public const string AdminSession = "__Host-admin-session";

    /// <summary>The form security cookie of the framework's anti-forgery protection (05 6.7.4, 6.5.1).</summary>
    public const string Antiforgery = "__Host-antiforgery";

    /// <summary>The guest cart cookie: the random cart key (05 6.7.5).</summary>
    public const string Cart = "__Host-cart";

    /// <summary>The recognized browser mark (05 6.7.6).</summary>
    public const string KnownDevice = "__Host-known-device";

    /// <summary>What the checkout step wrote, encrypted and signed (05 6.7.7, 6.7.9).</summary>
    public const string Checkout = "__Host-checkout";

    /// <summary>The flow cookie of the sign-in with Google (05 6.7.12).</summary>
    public const string ExternalLogin = "__Host-external-login";

    /// <summary>The required prefix of every name in the list.</summary>
    public const string Prefix = "__Host-";

    /// <summary>The seven cookies of the inventory, in the order of the tables in 05 6.7.</summary>
    public static readonly IReadOnlyList<string> All = [Session, AdminSession, Antiforgery, Cart, KnownDevice, Checkout, ExternalLogin];
}
