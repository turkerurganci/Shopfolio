// Deliberate violation sample of the rich text scan (09 15.3 items 1, 7). The scan reads this file as text; it is not compiled.
// Line numbers are part of the test. A full-line comment that says Html.Raw, HtmlSanitizer or HtmlString is not a finding.
using Ganss.Xss;
internal sealed class RawMarkupLeak
{
    private readonly HtmlSanitizer _cleaner = new();
    public object Page(string text) => new Microsoft.AspNetCore.Html.HtmlString(text);
    public object Razor(IHtmlHelper html, string text) => html.Raw(text);
    public object Razor2(IHtmlHelper html, string text) => Html . Raw (text);
    public void Builder(object builder, string text) => builder.AppendHtml($"<b>{text}</b>");
    public object Blazor(string text) => (MarkupString)text;
    public string Clean => "text";
}
