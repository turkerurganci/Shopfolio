// Deliberate violation sample of the cookie scan (09 15.1 item 6; 09 5.3 item 5; 02 12.2.5). The scan reads this file as text; it is not compiled.
// Line numbers are part of the test. A full-line comment that says Cookies.Append(, CookieOptions, "__Host-x" or UseCookiePolicy is not a finding.
internal static class CookieMisuse
{
    public static void Raw(HttpContext context) => context.Response.Cookies.Append("sid", "x");
    public static void Name(HttpContext context) => context.Response.Cookies.Append(
        "__Host-cart", "x");
    public static CookieOptions Options() => new CookieOptions { SameSite = SameSiteMode.None };
    public static void Banner(IApplicationBuilder app, CookiePolicyOptions options) => app.UseCookiePolicy(options);
    public static void Delete(HttpContext context, ICookieManager manager) => manager.DeleteCookie(context, "a", new CookieOptions());
    public static string Constant => CookieNames.Cart;
    public static string Request(HttpContext context) => context.Request.Cookies["a"] ?? string.Empty;
}
