// Deliberate violation sample of the redirect scan (05 6.5.6; 07 2.3.2; D-118). The scan reads this file as text; it is not compiled.
// Line numbers are part of the test. A full-line comment that says Redirect(returnUrl) or Headers.Location is not a finding.
internal static class RedirectMisuse
{
    public static IActionResult Plain(string returnUrl) => Redirect(returnUrl);
    public static IActionResult Permanent(string returnUrl) => RedirectPermanent(returnUrl);
    public static IActionResult Local(string returnUrl) => LocalRedirect(returnUrl);
    public static IActionResult Built(string returnUrl) => new RedirectResult(returnUrl);
    public static void Direct(HttpContext context, string returnUrl) => context.Response.Redirect(returnUrl);
    public static IResult Minimal(string returnUrl) => Results.Redirect(returnUrl);
    public static IResult Typed(string returnUrl) => TypedResults.Redirect(returnUrl);
    public static IActionResult ToPage(string returnUrl) => RedirectToPage("/Account/Home", new { returnUrl });
    public static IActionResult Split(string returnUrl) => Redirect
        (returnUrl);
    public static void Header(HttpContext context, string returnUrl) => context.Response.Headers.Location = returnUrl;
    public static void Named(HttpContext context, string returnUrl) => context.Response.Headers[HeaderNames.Location] = returnUrl;
    public static void Text(HttpContext context, string returnUrl) => context.Response.Headers.Append("Location", returnUrl);
    public static AuthenticationProperties Challenge(string returnUrl) => new AuthenticationProperties { RedirectUri = returnUrl };
    public static bool Property(SocketsHttpHandler handler) => handler.AllowAutoRedirect;
}
