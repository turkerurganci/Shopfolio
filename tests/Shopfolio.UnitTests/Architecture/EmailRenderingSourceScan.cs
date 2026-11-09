using System.Text.RegularExpressions;

namespace Shopfolio.UnitTests.Architecture;

/// <summary>
/// Source scan of the e-mail rendering rules (T305 KK2, KK5, KK6; 09 11.4 items 2 and 7; 09 25 "Kaçış gösterim kanalında mı"; 09 19.2 item 3). Two rules over
/// the e-mail namespace <c>Shopfolio.Infrastructure.EmailComposition</c> and, since T356 (D-167), the order documents' namespace <c>Shopfolio.Modules.Ordering.Documents</c> - the files of
/// their two folders, and any file that declares one of the namespaces elsewhere: (1) no
/// raw markup path - <c>MarkupString</c>, <c>HtmlString</c> and any other <c>Html*Content*</c> type, <c>IHtmlContent</c>, <c>HtmlContentBuilder</c>,
/// <c>Html.Raw</c> or a <c>Raw(</c> call, <c>AddMarkupContent</c>, <c>WriteLiteral</c> - so every value reaches the HTML through the renderer's encoding; (2) no request
/// header or request object - <c>HttpContext</c>, <c>IHttpContextAccessor</c>, <c>HttpRequest</c>, <c>Request.Host</c>, <c>HostString</c>, <c>X-Forwarded-Host</c>,
/// <c>ForwardedHost</c>, a <c>"Host"</c> header key - so the address root is the installation's domain setting and never a header a visitor writes. The scan reads the
/// whole text (a call split over several lines or with an extra or named argument is found); full-line <c>//</c> comments and Razor <c>@* *@</c> comments are
/// blanked first and keep their lines. It reads <c>.cs</c> and <c>.razor</c> files.
/// </summary>
internal static partial class EmailRenderingSourceScan
{
    public const string RawMarkup = "writes raw markup in the e-mail or order documents namespace (09 11.4 item 2: values reach the HTML only through the renderer's encoding)";
    public const string RequestHeaderOrObject = "reads the request or its Host header in the e-mail or order documents namespace (09 11.4 item 2: the address root is the domain setting)";

    public const string Folder = "src/Shopfolio.Web/Infrastructure/EmailComposition/";

    /// <summary>The order documents' folder (T356; 09 9.3 item 6, 11.4 item 4; D-167): the form and the contract are written as plain text and shown through the display channel, so the same two rules hold there.</summary>
    public const string DocumentsFolder = "src/Shopfolio.Web/Modules/Ordering/Documents/";

    public static bool InScope(string relativePath, string text)
    {
        string path = relativePath.Replace('\\', '/');
        return path.StartsWith(Folder, StringComparison.Ordinal) || path.StartsWith(DocumentsFolder, StringComparison.Ordinal) || EmailNamespace().IsMatch(text);
    }

    public static IReadOnlyList<(string Rule, int Line)> Scan(string relativePath, string text)
    {
        if (!InScope(relativePath, text))
        {
            return [];
        }

        string clean = BlankComments(text.ReplaceLineEndings("\n"));
        List<(string Rule, int Line)> findings = [];
        Add(findings, RawMarkup, Markup(), clean);
        Add(findings, RequestHeaderOrObject, Request(), clean);
        return [.. findings.OrderBy(f => f.Line).ThenBy(f => f.Rule, StringComparer.Ordinal)];
    }

    private static void Add(List<(string Rule, int Line)> findings, string rule, Regex pattern, string text)
    {
        foreach (Match match in pattern.Matches(text))
        {
            int line = text.AsSpan(0, match.Index).Count('\n') + 1;
            if (!findings.Contains((rule, line)))
            {
                findings.Add((rule, line));
            }
        }
    }

    private static string BlankComments(string text)
    {
        string withoutRazor = RazorComment().Replace(text, match => new string('\n', match.Value.Count(c => c == '\n')));
        string[] lines = withoutRazor.Split('\n');
        for (int i = 0; i < lines.Length; i++)
        {
            if (lines[i].TrimStart().StartsWith("//", StringComparison.Ordinal))
            {
                lines[i] = string.Empty;
            }
        }

        return string.Join('\n', lines);
    }

    [GeneratedRegex(@"@\*.*?\*@", RegexOptions.CultureInvariant | RegexOptions.Singleline, matchTimeoutMilliseconds: 5000)]
    private static partial Regex RazorComment();

    [GeneratedRegex(@"namespace\s+Shopfolio\s*\.\s*(?:Infrastructure\s*\.\s*EmailComposition|Modules\s*\.\s*Ordering\s*\.\s*Documents)\b", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex EmailNamespace();

    // The raw markup types and calls, in any spelling: MarkupString (also as a cast), HtmlString and the Html*Content* family, IHtmlContent, Html.Raw, Raw(, the
    // render tree's AddMarkupContent and the Razor writer's WriteLiteral.
    [GeneratedRegex(@"\bMarkupString\b|\b(?:Html|Raw)String\b|\bHtmlContent\w*|\bIHtmlContent\b|\bHtml\s*\.\s*Raw\b|\bRaw\s*\(|\bAddMarkupContent\b|\bWriteLiteral\b", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex Markup();

    // The request object and the headers that carry a host.
    [GeneratedRegex(@"\bHttpContext\w*|\bIHttpContextAccessor\b|\bHttpRequest\b|\bRequest\s*\.\s*Host\b|\bHostString\b|X-Forwarded-Host|\bForwardedHost\b|\[\s*""Host""\s*\]|HeaderNames\s*\.\s*Host\b", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase, matchTimeoutMilliseconds: 5000)]
    private static partial Regex Request();
}
