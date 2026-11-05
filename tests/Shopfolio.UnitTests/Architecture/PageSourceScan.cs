using System.Text.RegularExpressions;

namespace Shopfolio.UnitTests.Architecture;

/// <summary>
/// Source scan of the page and address rules (T206; 07 2.2, 2.2.1, 2.10.1, 2.10.3; 09 6 item 2; 09 19.2 item 3; D-73). Over the application source:
/// (1) <c>.cs</c> and <c>.cshtml</c>: there is no outward API - no controller base or <c>[ApiController]</c>, no controller registration, no <c>/api</c> address -
/// and no version in the address, the header or the parameter (<c>Asp.Versioning</c>, <c>ApiVersion</c>, <c>api-version</c>, a <c>/vN</c> address, a versioned
/// vendor media type); (2) <c>.cshtml</c> only: a view touches no database context (<c>@inject AppDbContext</c>, an <c>EntityFrameworkCore</c> name) and no
/// internal type of a module (any name under <c>Shopfolio.Modules.&lt;Module&gt;</c> other than <c>Contracts</c>) - the source side of the page rule
/// whose compiled side is the <c>AspNetCoreGeneratedDocument</c> class (D-73); a <c>&lt;form&gt;</c> whose method is <c>post</c> names its handler by
/// <c>asp-page-handler</c> or by a <c>handler</c> query parameter in its action (07 2.2.1). The scan reads the whole text, so a tag split over several lines
/// is found and the case of the words does not matter; full-line <c>//</c> comments, Razor comments (<c>@* *@</c>) and HTML comments are blanked first and keep
/// their lines. There is no allowed file. The test projects are not scanned: they write violations to prove the rules.
/// </summary>
internal static partial class PageSourceScan
{
    public const string OutwardApi = "builds an outward API (07 2.10.1: pages and forms are the only interface; no controller, no /api address)";
    public const string AddressVersion = "puts a version in an address, a header or a parameter (07 2.10.3: there is no version)";
    public const string ViewTouchesContext = "a view touches the database context (09 6 item 2; 05 3.3.12: a page reads and writes through a module's open surface)";
    public const string ViewTouchesModuleInternals = "a view touches an internal type of a module (09 6 item 2: only Contracts is open)";
    public const string PostFormWithoutHandler = "a post form does not name its handler (07 2.2.1: every POST is a named handler that comes with the handler parameter)";

    public static IReadOnlyList<(string Rule, int Line)> Scan(string relativePath, string text)
    {
        string path = relativePath.Replace('\\', '/');
        bool view = path.EndsWith(".cshtml", StringComparison.OrdinalIgnoreCase);
        string clean = view ? BlankViewComments(text.ReplaceLineEndings("\n")) : BlankCommentLines(text.ReplaceLineEndings("\n"));
        List<(string Rule, int Line)> findings = [];
        Add(findings, OutwardApi, ControllerApi(), clean);
        Add(findings, OutwardApi, ApiAddress(), clean);
        Add(findings, AddressVersion, VersionNames(), clean);
        Add(findings, AddressVersion, VersionAddress(), clean);
        if (view)
        {
            Add(findings, ViewTouchesContext, ContextNames(), clean);
            Add(findings, ViewTouchesModuleInternals, ModuleInternals(), clean);
            foreach (Match tag in FormTag().Matches(clean))
            {
                string tagText = tag.Value;
                if (PostMethod().IsMatch(tagText) && !NamedHandler().IsMatch(tagText))
                {
                    findings.Add((PostFormWithoutHandler, clean.AsSpan(0, tag.Index).Count('\n') + 1));
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

    // [ApiController], a controller base class, a controller registration.
    [GeneratedRegex(@"\[\s*ApiController\b|\bControllerBase\b|:\s*Controller\b|\bMapControllers?\b|\bAddControllers(?:WithViews)?\b|\bAddMvc\b", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex ControllerApi();

    // An address that starts with /api: a string literal of any kind, or a route template of a Map call, [Route], an Http attribute or @page.
    [GeneratedRegex(@"""\$?@?/api(?:[/?""{]|\z)|\b(?:Map\w*|Route|Http(?:Get|Post|Put|Delete|Patch)|AddPageRoute\w*)\s*\(\s*\$?@?""/?api\b|@page\s+""/?api\b", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase, matchTimeoutMilliseconds: 5000)]
    private static partial Regex ApiAddress();

    // The versioning library and its names; the api-version parameter and header in any case; a versioned vendor media type.
    [GeneratedRegex(@"Asp\.Versioning|\bApiVersion\w*|\bAddApiVersioning\b|api-?version|application/vnd\.", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase, matchTimeoutMilliseconds: 5000)]
    private static partial Regex VersionNames();

    // An address whose first segment is /v1, /v2/...: a string literal, a Map call, an Http attribute, @page.
    [GeneratedRegex(@"""\$?@?/v\d+(?:[/?""{]|\z)|\b(?:Map\w*|Route|Http(?:Get|Post|Put|Delete|Patch)|AddPageRoute\w*)\s*\(\s*\$?@?""/?v\d+\b|@page\s+""/?v\d+\b", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase, matchTimeoutMilliseconds: 5000)]
    private static partial Regex VersionAddress();

    [GeneratedRegex(@"\bAppDbContext\b|\bDbContext\b|\bMicrosoft\.EntityFrameworkCore\b", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex ContextNames();

    // Shopfolio.Modules.<Module> followed by anything but .Contracts (or by nothing: the namespace of a module itself is not open either).
    [GeneratedRegex(@"\bShopfolio\.Modules\.[A-Za-z0-9_]+(?!\.Contracts\b)(?=[.\s;""<>(),]|$)", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex ModuleInternals();

    [GeneratedRegex(@"<form\b[^>]*>", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase | RegexOptions.Singleline, matchTimeoutMilliseconds: 5000)]
    private static partial Regex FormTag();

    [GeneratedRegex(@"\bmethod\s*=\s*[""']?\s*post\b", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase, matchTimeoutMilliseconds: 5000)]
    private static partial Regex PostMethod();

    // asp-page-handler="X" (the tag helper) or asp-route-handler="X" (the same query parameter) or a handler=X query parameter in the action; an empty asp-page-handler="" names nothing.
    [GeneratedRegex(@"\basp-(?:page|route)-handler\s*=\s*[""']?\s*[^""'\s>]|[?&]handler=[^&""'\s>]", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase, matchTimeoutMilliseconds: 5000)]
    private static partial Regex NamedHandler();
}
