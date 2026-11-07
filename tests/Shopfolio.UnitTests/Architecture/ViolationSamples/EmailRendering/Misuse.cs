// Deliberate violation sample of the e-mail rendering scan (09 11.4 item 2). The scan reads this file as text; it is not compiled.
// Line numbers are part of the test. A full-line comment that says MarkupString or Request.Host is not a finding.
namespace Shopfolio.Infrastructure.EmailComposition;

internal static class EmailMisuse
{
    public static MarkupString Cast(string name) => (MarkupString)name;
    public static object Plain(string name) => new HtmlString(name);
    public static object Raw1(IHtmlHelper html, string name) => html.Raw(name);
    public static object Raw2(string name) => Html.Raw(name);
    public static object Raw3(string name) => Html
        .Raw(name);
    public static void Builder(RenderTreeBuilder builder, string name) => builder.AddMarkupContent(0, name);
    public static object Content(string name) => new HtmlContentBuilder().AppendHtml(name);
    public static object Interface(IHtmlContent content) => content;
    public static string Host1(HttpContext context) => context.Request.Host.Value;
    public static string Host2(IHttpContextAccessor accessor) => accessor.HttpContext!.Request.Host.Value;
    public static string Host3(HttpRequest request) => request.Headers["X-Forwarded-Host"];
    public static string Host4(HttpRequest request) => request.Headers[HeaderNames.Host];
    public static string Host5(HttpRequest request) => request.Headers["Host"];
    public static HostString Host6 => default;
}
