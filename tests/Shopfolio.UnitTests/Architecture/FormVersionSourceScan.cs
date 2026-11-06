using System.Text.RegularExpressions;

namespace Shopfolio.UnitTests.Architecture;

/// <summary>
/// Source scan of how a version and a shown value are carried (T208 criterion 7, KK7; 07 2.4.7; 05 2.1 MK-05): they are fields of the POST body, never part
/// of an address and never a header. Over the application source (<c>.cs</c> and <c>.cshtml</c>), after the comments are blanked and with the whole
/// text read - a call split over lines is found, the case of the header words does not matter:
/// <list type="number">
/// <item>An address that carries one: a route value of a tag helper (<c>asp-route-version</c>, <c>asp-route-originalStock</c>), a query pair in a string
/// (<c>?Version=</c>, <c>&amp;PaymentMethodsVersion=</c>, <c>?originalStock=</c>), a path parameter in a page or Map template (<c>{version}</c>), a redirect, link or
/// query-string call (<c>SeeOther</c>, <c>RedirectToPage</c>, <c>Url.Page</c>, <c>AddQueryString</c>, ...) that names one of them in its arguments.</item>
/// <item>A header: the conditional request headers (<c>If-Match</c>, <c>If-None-Match</c>, <c>If-Unmodified-Since</c>, <c>ETag</c>, their <c>HeaderNames</c> constants and
/// <c>EntityTagHeaderValue</c>) and any header that is set or read with a name that holds <c>Version</c> or <c>Original</c>.</item>
/// <item>A view: a <c>&lt;form&gt;</c> whose method is not post (a GET form sends its fields in the address) that holds a version or shown-value field.</item>
/// <item>A view: a version or shown-value field made from the posted model state (<c>asp-for</c>, <c>HiddenFor</c>). The framework restores the posted value there, so a
/// re-displayed form (07 2.4.5) would carry the old version and "Yine de kaydet" could never succeed; <c>VersionFields</c> writes the value the handler chose.</item>
/// </list>
/// The helper that writes the fields (<c>Infrastructure/Forms/VersionFields.cs</c>) contains the names as strings and matches none of the rules. There is no
/// allowed file. The test projects are not scanned: they write violations to prove the rules.
/// </summary>
internal static partial class FormVersionSourceScan
{
    public const string VersionInAddress = "carries a version or a shown value in an address (07 2.4.7: they are fields of the POST body)";
    public const string VersionInHeader = "carries a version or a shown value in a header, or uses a conditional request header (07 2.4.7: no If-Match, no ETag)";
    public const string VersionFieldInNonPostForm = "a form that is not a post form holds a version or shown-value field (07 2.4.7: the field would go into the address)";
    public const string VersionFieldFromModelState = "makes a version or shown-value field from the posted model state (07 2.4.5: the form carries the record's current version; use VersionFields)";

    public static IReadOnlyList<(string Rule, int Line)> Scan(string relativePath, string text)
    {
        string path = relativePath.Replace('\\', '/');
        bool view = path.EndsWith(".cshtml", StringComparison.OrdinalIgnoreCase);
        string clean = view ? BlankViewComments(text.ReplaceLineEndings("\n")) : BlankCommentLines(text.ReplaceLineEndings("\n"));
        List<(string Rule, int Line)> findings = [];
        Add(findings, VersionInAddress, RouteValueAttribute(), clean);
        Add(findings, VersionInAddress, QueryPair(), clean);
        Add(findings, VersionInAddress, PathParameter(), clean);
        Add(findings, VersionInAddress, LinkCallWithField(), clean);
        Add(findings, VersionInHeader, ConditionalHeader(), clean);
        Add(findings, VersionInHeader, HeaderWithField(), clean);
        if (view)
        {
            Add(findings, VersionFieldFromModelState, ModelStateField(), clean);
            foreach (Match tag in FormTag().Matches(clean))
            {
                if (PostMethod().IsMatch(tag.Value))
                {
                    continue;
                }

                int bodyStart = tag.Index + tag.Length;
                int end = clean.IndexOf("</form", bodyStart, StringComparison.OrdinalIgnoreCase);
                string body = clean[bodyStart..(end < 0 ? clean.Length : end)];
                if (FieldInForm().IsMatch(body))
                {
                    findings.Add((VersionFieldInNonPostForm, clean.AsSpan(0, tag.Index).Count('\n') + 1));
                }
            }
        }

        return [.. findings.Distinct().OrderBy(f => f.Line).ThenBy(f => f.Rule, StringComparer.Ordinal)];
    }

    private static void Add(List<(string Rule, int Line)> findings, string rule, Regex pattern, string text)
    {
        foreach (Match match in pattern.Matches(text))
        {
            findings.Add((rule, text.AsSpan(0, match.Index).Count('\n') + 1));
        }
    }

    private static string BlankCommentLines(string text)
    {
        string[] lines = text.Split('\n');
        for (int i = 0; i < lines.Length; i++)
        {
            if (lines[i].TrimStart().StartsWith("//", StringComparison.Ordinal))
            {
                lines[i] = string.Empty;
            }
        }

        return string.Join('\n', lines);
    }

    // A Razor comment and an HTML comment become blanks of the same line count, so a finding after them keeps its line number.
    private static string BlankViewComments(string text) =>
        ViewComment().Replace(text, m => string.Concat(m.Value.Select(c => c == '\n' ? '\n' : ' ')));

    [GeneratedRegex(@"@\*.*?\*@|<!--.*?-->", RegexOptions.CultureInvariant | RegexOptions.Singleline, matchTimeoutMilliseconds: 5000)]
    private static partial Regex ViewComment();

    // asp-route-version, asp-route-paymentMethodsVersion, asp-route-originalStockQuantity: a route value the tag helper puts into the address.
    [GeneratedRegex(@"\basp-route-(?:[a-z0-9-]*version|original[a-z0-9-]*)\b", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase, matchTimeoutMilliseconds: 5000)]
    private static partial Regex RouteValueAttribute();

    // ?Version=, &PaymentMethodsVersion=, ?version=, ?OriginalText=, &originalStock=: a query pair in a string. "conversion=" is no version column.
    [GeneratedRegex(@"[?&](?:version|[A-Za-z0-9]*Version|Original[A-Za-z0-9]+|original[A-Z][A-Za-z0-9]*)=", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex QueryPair();

    // {version}, {Version:int}, {id}/{paymentMethodsVersion?}: a path parameter in a page, Map or route template.
    [GeneratedRegex(@"(?:@page|\bMap\w*\s*\(|\bAddPageRoute\w*\s*\(|\bRoute\s*\(|\bHttp(?:Get|Post|Put|Delete|Patch)\s*\()[^\n]*\{\s*(?:[A-Za-z0-9]*[Vv]ersion|[Oo]riginal[A-Za-z0-9]*)\s*(?:[:?][^}]*)?\}", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex PathParameter();

    // A call that builds an address or a redirect and names a version or shown-value field among its arguments (to the end of the statement).
    [GeneratedRegex(@"\b(?:SeeOther|Redirect\w*|LocalRedirect\w*|RedirectToPage\w*|RedirectToAction\w*|Url\.(?:Page|Action|RouteUrl|Content)|AddQueryString|QueryString\.Create|GetPathByPage|GetUriByPage|RouteValueDictionary)\s*\([^;]*?\b(?:[A-Za-z0-9]*Version|version|Original[A-Za-z0-9]+|original[A-Z][A-Za-z0-9]*)\b", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex LinkCallWithField();

    // If-Match, If-None-Match, If-Unmodified-Since, ETag; HeaderNames.IfMatch/IfNoneMatch/IfUnmodifiedSince/ETag; EntityTagHeaderValue; the typed header properties.
    [GeneratedRegex(@"\bIf-?(?:None-?)?Match\b|\bIf-?Unmodified-?Since\b|\bE-?Tag\b|\bEntityTag\w*|\bHeaderNames\s*\.\s*(?:IfMatch|IfNoneMatch|IfUnmodifiedSince|ETag)\b|\bIf(?:None)?Match\s*(?:=|\.)", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase, matchTimeoutMilliseconds: 5000)]
    private static partial Regex ConditionalHeader();

    // Headers["X-Version"], Headers.Append("Version", ...), Headers.TryGetValue("OriginalStock", ...): a header whose name holds Version or Original.
    [GeneratedRegex(@"\bHeaders\s*(?:\[|\.\s*(?:Append|Add|TryAdd|TryGetValue|ContainsKey|Remove|Set\w*)\s*\()\s*\$?@?""[^""]*(?:version|original)", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase, matchTimeoutMilliseconds: 5000)]
    private static partial Regex HeaderWithField();

    // asp-for="Input.Version", asp-for="Original...", @Html.HiddenFor(m => m.Input.Version): the framework restores the posted value for these.
    [GeneratedRegex(@"\basp-for\s*=\s*[""'][^""']*(?:Version|Original[A-Za-z0-9]+)[""']|\bHiddenFor\s*\([^)]*?(?:Version|Original[A-Za-z0-9]+)", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex ModelStateField();

    [GeneratedRegex(@"<form\b[^>]*>", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase | RegexOptions.Singleline, matchTimeoutMilliseconds: 5000)]
    private static partial Regex FormTag();

    [GeneratedRegex(@"\bmethod\s*=\s*[""']?\s*post\b", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase, matchTimeoutMilliseconds: 5000)]
    private static partial Regex PostMethod();

    // name="Version", name="Input.PaymentMethodsVersion", name="OriginalStockQuantity", the VersionFields helper.
    [GeneratedRegex(@"\bname\s*=\s*[""'][^""']*(?:Version|Original[A-Za-z0-9]+)[""']|\bVersionFields\s*\.", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex FieldInForm();
}
