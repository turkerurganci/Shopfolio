// Deliberate violation sample of the order documents' scan (09 9.3 item 6, 11.4 item 4; D-167). The scan reads this file as text; it is not compiled.
// Line numbers are part of the test.
namespace Shopfolio.Modules.Ordering.Documents;

internal static class DocumentsMisuse
{
    public static object Plain(string text) => new HtmlString(text);
    public static MarkupString Cast(string text) => (MarkupString)text;
    public static object Raw(string text) => Html.Raw(text);
    public static string Host(HttpContext context) => context.Request.Host.Value;
    public static string Origin(HttpRequest request) => request.Headers["Host"];
}
