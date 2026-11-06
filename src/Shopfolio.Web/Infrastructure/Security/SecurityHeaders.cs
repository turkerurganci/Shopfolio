using System.Security.Cryptography;

namespace Shopfolio.Infrastructure.Security;

/// <summary>
/// The values of the security headers every response carries (09 15.2 item 3; 05 6.5.4; 02 8.3.2): written in this class and in the middleware that
/// uses it, and nowhere else (<c>SecurityHeadersSourceScan</c>). HSTS is not among them - the reverse proxy writes it for a year without sub-domains
/// and without the preload list, because it ends the TLS and answers while the application is down (05 6.8.1; MK-58); the application never calls
/// <c>UseHsts</c>.
/// </summary>
internal static class SecurityHeaders
{
    /// <summary>The key of the request's nonce in <c>HttpContext.Items</c> (09 15.2 item 4): the one inline style block of the common layout carries it.</summary>
    public const string NonceKey = "Shopfolio.CspNonce";

    public const string NoSniff = "nosniff";

    /// <summary>The default: a request to another site carries the site's root only (05 6.5.4).</summary>
    public const string ReferrerPolicyDefault = "strict-origin-when-cross-origin";

    /// <summary>A page whose address holds a key sends no referrer at all (05 6.4.8): the account links and the access-key order page.</summary>
    public const string ReferrerPolicyKeyedPage = "no-referrer";

    /// <summary>Camera, microphone and location are off; the site uses none (05 6.5.4).</summary>
    public const string PermissionsPolicy = "camera=(), microphone=(), geolocation=()";

    /// <summary>The value of <c>Cache-Control</c> of every response that carries personal data, a session, a cart or an access key (09 15.2 item 5; 05 6.5.4).</summary>
    public const string NoStore = "no-store";

    /// <summary>A new nonce: 16 random bytes, Base64 (09 15.2 item 4). One per response.</summary>
    public static string NewNonce() => Convert.ToBase64String(RandomNumberGenerator.GetBytes(16));

    /// <summary>The nonce of the response being written; the layout's style block reads it. Throws when the middleware did not run.</summary>
    public static string NonceOf(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        return context.Items[NonceKey] as string
            ?? throw new InvalidOperationException("The security headers middleware did not run: there is no nonce for this response (09 15.2 item 3).");
    }
}
