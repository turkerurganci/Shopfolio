namespace Shopfolio.Infrastructure.Security;

/// <summary>
/// The content security policy, built in one place (09 15.2 item 4; 05 6.5.4; 02 8.3.2, 8.3.3). Script, style, font, image and connection come
/// from the site itself; the one inline style block the server writes - the brand colour variable (05 3.8.3) - is allowed by the response's nonce and
/// nothing else is; a form and the redirect after it go to the site and to Google's authorization address only; the site is framed by no one and
/// frames no one. The policy is enforced, not report-only, and has no <c>unsafe-inline</c> or <c>unsafe-eval</c>. Two responses change it (T510;
/// <see cref="ContentSecurityPolicyOptions"/>): the payment page lets the site and the provider's form origin into its frame (<c>frame-src</c>;
/// <see cref="AllowPaymentFrameAttribute"/>), and the payment return's hand-over page lets the site frame it (<c>frame-ancestors 'self'</c>;
/// <see cref="AllowFramingAttribute"/>). Neither widens <c>script-src</c> or <c>form-action</c>.
/// </summary>
internal static class ContentSecurityPolicy
{
    /// <summary>Google's authorization address, the one other target of a form (09 15.2 item 4; 05 6.1.4): the sign-in and the re-verification.</summary>
    public const string GoogleAuthorizationOrigin = "https://accounts.google.com";

    /// <summary>The header's name.</summary>
    public const string HeaderName = "Content-Security-Policy";

    /// <summary>The policy of every response that is neither the payment page nor the hand-over page.</summary>
    public static string Build(string nonce) => Build(nonce, ContentSecurityPolicyOptions.Default);

    public static string Build(string nonce, ContentSecurityPolicyOptions options)
    {
        ArgumentException.ThrowIfNullOrEmpty(nonce);
        ArgumentNullException.ThrowIfNull(options);
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
            options.PaymentFrameOrigin is string origin ? $"frame-src 'self' {origin}" : "frame-src 'none'",
            options.AllowsSelfFraming ? "frame-ancestors 'self'" : "frame-ancestors 'none'",
            $"form-action 'self' {GoogleAuthorizationOrigin}");
    }
}

/// <summary>
/// What a response changes in the policy (09 15.2 item 4; 05 6.5.4; 08 2.1.4 items 2 and 3). <see cref="PaymentFrameOrigin"/> is set only on the payment page's response
/// that holds the provider's frame: <c>frame-src 'self' {origin}</c> - <c>'self'</c> because the provider sends the frame to the site's return address, whose hand-over
/// page loads in it (K-1274). <see cref="AllowsSelfFraming"/> is set only on the hand-over page's response: <c>frame-ancestors 'self'</c>, so the site's own payment
/// page may hold it; the order page stays unframeable.
/// </summary>
internal sealed record ContentSecurityPolicyOptions(string? PaymentFrameOrigin, bool AllowsSelfFraming)
{
    public static ContentSecurityPolicyOptions Default { get; } = new(null, false);
}

/// <summary>
/// Marks the page that may hold the payment provider's frame (09 15.2 item 4; 07 UC-29): the confirmation screen, whose <c>StartPayment</c> answer is the payment page.
/// The mark alone widens nothing: the frame permission is written only on the response the page marks as showing the frame (<see cref="PaymentFrameResponse.Mark"/>),
/// so the confirmation screen's own GET and every other answer of the page keep <c>frame-src 'none'</c>.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
internal sealed class AllowPaymentFrameAttribute : Attribute
{
}

/// <summary>
/// Marks the payment return's hand-over page (09 15.2 item 4; 07 UC-30): its response carries <c>frame-ancestors 'self'</c>, the one response the site's own page may frame.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
internal sealed class AllowFramingAttribute : Attribute
{
}

/// <summary>
/// The payment page's response mark (07 UC-29): the page sets it when its answer draws the provider's frame, and the security headers read it together with the page's
/// <see cref="AllowPaymentFrameAttribute"/>. Kept in <c>HttpContext.Items</c> for the one response.
/// </summary>
internal static class PaymentFrameResponse
{
    private const string Key = "Shopfolio.PaymentFrameResponse";

    public static void Mark(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        context.Items[Key] = true;
    }

    public static bool IsMarked(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        return context.Items.ContainsKey(Key);
    }
}
