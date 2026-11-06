namespace Shopfolio.Infrastructure.Security;

/// <summary>
/// The content security policy, built in one place (09 15.2 item 4; 05 6.5.4; 02 8.3.2, 8.3.3). Script, style, font, image and connection come
/// from the site itself; the one inline style block the server writes - the brand colour variable (05 3.8.3) - is allowed by the response's nonce and
/// nothing else is; a form and the redirect after it go to the site and to Google's authorization address only; the site is framed by no one and
/// frames no one. The policy is enforced, not report-only, and has no <c>unsafe-inline</c> or <c>unsafe-eval</c>. The two pages that change it - the
/// payment page's frame permission and the hand-over page's <c>frame-ancestors 'self'</c> - belong to the payment screen's task (T510) and enter here as
/// parameters of <see cref="Build"/> then.
/// </summary>
internal static class ContentSecurityPolicy
{
    /// <summary>Google's authorization address, the one other target of a form (09 15.2 item 4; 05 6.1.4): the sign-in and the re-verification.</summary>
    public const string GoogleAuthorizationOrigin = "https://accounts.google.com";

    /// <summary>The header's name.</summary>
    public const string HeaderName = "Content-Security-Policy";

    public static string Build(string nonce)
    {
        ArgumentException.ThrowIfNullOrEmpty(nonce);
        return string.Join(
            "; ",
            "default-src 'self'",
            "script-src 'self'",
            $"style-src 'self' 'nonce-{nonce}'",
            "img-src 'self'",
            "font-src 'self'",
            "connect-src 'self'",
            "object-src 'none'",
            "base-uri 'self'",
            "frame-src 'none'",
            "frame-ancestors 'none'",
            $"form-action 'self' {GoogleAuthorizationOrigin}");
    }
}
