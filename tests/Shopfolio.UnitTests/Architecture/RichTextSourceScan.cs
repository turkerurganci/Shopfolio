using System.Text.RegularExpressions;

namespace Shopfolio.UnitTests.Architecture;

/// <summary>
/// Source scan of the rich text path (T339 KK8; 09 15.3 items 1, 7). Three rules over the application source: (1) the cleaner library's namespace (<c>Ganss.Xss</c>) and its
/// type (<c>HtmlSanitizer</c>) are named only in <c>Infrastructure/Security/RichTextSanitizer.cs</c> - the allow list is built in one class; (2) the raw markup calls
/// (<c>Html.Raw</c>, <c>Raw(</c>, <c>AddMarkupContent</c>, <c>WriteLiteral</c>, <c>AppendHtml(</c> with a string argument) and the Blazor <c>MarkupString</c> are named only in
/// <c>Infrastructure/Security/RichText.cs</c> - a page prints no other text unescaped; (3) <c>HtmlString</c>, in any spelling (<c>new HtmlString(</c>, target-typed
/// <c>new(</c> is not a spelling of the type's name and so is covered by the two lists above), is named only in <c>RichText.cs</c> and in the one file that builds the search
/// engine head from values it encodes itself (<c>SearchEngine/SearchEngineData.cs</c>; its move into the structured data helper of 09 15.3 item 1 is D-357). Full-line comments
/// are blanked first and keep their line. The test projects are not scanned. The scan reads text and writes nothing.
/// </summary>
internal static partial class RichTextSourceScan
{
    public const string CleanerOutsideItsClass = "names the cleaner library outside Infrastructure/Security/RichTextSanitizer.cs (09 15.3 item 1: the allow list is built in one class)";
    public const string RawMarkupOutsideTheDisplay = "prints or builds raw markup outside Infrastructure/Security/RichText.cs (09 15.3 item 1: Html.Raw and its kin live only in RichText.Render)";
    public const string HtmlStringOutsideItsPlaces = "names HtmlString outside RichText.cs and the search engine head (09 15.3 item 7)";

    public const string SanitizerFile = "src/Shopfolio.Web/Infrastructure/Security/RichTextSanitizer.cs";
    public const string DisplayFile = "src/Shopfolio.Web/Infrastructure/Security/RichText.cs";
    public const string SearchHeadFile = "src/Shopfolio.Web/Infrastructure/SearchEngine/SearchEngineData.cs";

    public static IReadOnlyList<(string Rule, int Line)> Scan(string relativePath, string text)
    {
        string path = relativePath.Replace('\\', '/');
        string code = BlankCommentLines(text.ReplaceLineEndings("\n"));
        List<(string Rule, int Line)> findings = [];
        if (!string.Equals(path, SanitizerFile, StringComparison.Ordinal))
        {
            Add(findings, CleanerOutsideItsClass, Cleaner(), code);
        }

        if (!string.Equals(path, DisplayFile, StringComparison.Ordinal))
        {
            Add(findings, RawMarkupOutsideTheDisplay, RawMarkup(), code);
            if (!string.Equals(path, SearchHeadFile, StringComparison.Ordinal))
            {
                Add(findings, HtmlStringOutsideItsPlaces, HtmlStringName(), code);
            }
        }

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

    private static string BlankCommentLines(string text)
    {
        string[] lines = text.Split('\n');
        for (int i = 0; i < lines.Length; i++)
        {
            string trimmed = lines[i].TrimStart();
            if (trimmed.StartsWith("//", StringComparison.Ordinal) || trimmed.StartsWith("///", StringComparison.Ordinal) || trimmed.StartsWith("@*", StringComparison.Ordinal))
            {
                lines[i] = string.Empty;
            }
        }

        return string.Join('\n', lines);
    }

    // The namespace and the type, also as an attribute-less alias or a using static: any word-bounded naming.
    [GeneratedRegex(@"\bGanss\s*\.\s*Xss\b|\bHtmlSanitizer\w*\b", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex Cleaner();

    // The raw markup calls and types: Html.Raw (also as @Html.Raw), any Raw( call, AddMarkupContent, WriteLiteral, MarkupString (also as a cast), and AppendHtml( whose first argument is a
    // string: a literal (plain, interpolated, verbatim), a ToString() call or a string.* call. AppendHtml of a tag builder (the composition of the form controls) is not raw.
    [GeneratedRegex(
        @"\bHtml\s*\.\s*Raw\b|\bRaw\s*\(|\bAddMarkupContent\b|\bWriteLiteral\w*\b|\bMarkupString\b|\bAppendHtml\s*\(\s*(?:\$@?""|@\$?""|""|[^()]*\.ToString\s*\(|string\b)",
        RegexOptions.CultureInvariant,
        matchTimeoutMilliseconds: 5000)]
    private static partial Regex RawMarkup();

    [GeneratedRegex(@"\bHtmlString\b", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex HtmlStringName();
}
