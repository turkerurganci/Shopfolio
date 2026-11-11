using System.Text.RegularExpressions;

namespace Shopfolio.UnitTests.Architecture;

/// <summary>
/// Source scan of the form and site security (T209; 07 2.5.4, 2.5.5, 2.5.6; 05 6.5; 09 15.2 items 1-5; 09 19.2 item 3). Over the application source
/// (<c>.cs</c> and <c>.cshtml</c>): (1) no way out of the forgery protection but the one switch of the signed endpoints - no <c>IgnoreAntiforgery…</c>
/// name anywhere, <c>DisableAntiforgery</c> only in <c>SignedEndpointRegistration.cs</c>, no <c>RequireAntiforgeryToken(false)</c>, every
/// <c>MapSignedEndpoint</c> call takes its path from <c>AntiforgeryExemptions</c> and never from a literal, and no other writing endpoint is mapped
/// (a minimal <c>MapPost</c> has no page filter - the browser POSTs are page handlers, 09 4.1; the header-token endpoints of the chunked upload bring
/// their own mechanism and relax this rule in one file only, D-131); (2) a post form in a view carries the token: the form tag adds it only when it has no
/// <c>action</c> of its own, so a post form with an <c>action</c> needs <c>asp-antiforgery="true"</c>, none may switch it off, and none posts to another
/// site; (3) the headers of 09 15.2 items 3 and 4 are written in their own files only, the policy has no <c>unsafe-inline</c>, no <c>unsafe-eval</c> and
/// no report-only form, and nothing in the application writes HSTS or calls <c>UseHsts</c> (the reverse proxy does, 05 6.8.1); (4) the markup of
/// the content security policy (09 15.2 item 4): a view has no <c>style</c> attribute, no inline event attribute, no <c>javascript:</c> address, a
/// <c>&lt;script&gt;</c> only with <c>src</c> or the structured-data type, and a <c>&lt;style&gt;</c> only as the nonce block of the common layout.
/// Full-line <c>//</c> comments, Razor comments and HTML comments are blanked first and keep their lines. The test projects are not scanned: they
/// write violations to prove the rules.
/// </summary>
internal static partial class ForgeryProtectionSourceScan
{
    public const string IgnoresProtection = "switches the forgery protection off (07 2.5.5: the closed list holds only the signed notification, and its one switch is MapSignedEndpoint)";
    public const string DisableOutsideRegistration = "calls DisableAntiforgery outside SignedEndpointRegistration.cs (09 15.2 item 2: MapSignedEndpoint is the one place)";
    public const string SignedEndpointPathNotListed = "maps a signed endpoint with a path that is not a member of AntiforgeryExemptions (09 15.2 item 2: the closed list is the only source)";
    public const string OtherWritingEndpoint = "maps a writing endpoint that is no page handler (09 4.1, 15.2 item 1: a browser POST is a page handler under the page filter; a machine POST is a signed endpoint)";
    public const string PostFormWithoutToken = "a post form with its own action carries no token (05 6.5.1: the form tag adds the token only without an action; add asp-antiforgery=\"true\" or drop the action)";
    public const string TokenSwitchedOff = "a form switches its token off (asp-antiforgery=\"false\"; 07 2.5.4)";
    public const string PostFormToAnotherSite = "a post form posts to another site (09 15.2 item 4: form-action is the site and Google's authorization address only)";
    public const string HeaderWrittenElsewhere = "writes a security header outside its own file (09 15.2 item 3: SecurityHeaders, ContentSecurityPolicy and the middleware are the places)";
    public const string UnsafePolicy = "weakens the content security policy (09 15.2 item 4: no unsafe-inline, no unsafe-eval, no report-only)";
    public const string HstsInApplication = "writes or configures HSTS (05 6.8.1, MK-58: the reverse proxy writes it; the application never calls UseHsts)";
    public const string StyleAttribute = "a view has a style attribute (09 15.2 item 4: a colour is a CSS class or the layout's nonce variable, never an element's style)";
    public const string InlineEventAttribute = "a view has an inline event attribute or a javascript: address (09 15.2 item 4: script is the site's own files)";
    public const string ScriptWithoutSource = "a view has a script tag with neither src nor the structured-data type (09 15.2 item 4)";
    public const string StyleBlockOutsideLayout = "a view has a style block that is not the nonce block of the common layout (09 15.2 item 4)";

    private static readonly string[] _headerFiles =
    [
        "Infrastructure/Security/SecurityHeaders.cs",
        "Infrastructure/Security/ContentSecurityPolicy.cs",
        "Infrastructure/Security/SecurityHeadersMiddleware.cs",
    ];

    private const string RegistrationFile = "Infrastructure/SignedEndpoints/SignedEndpointRegistration.cs";

    private static readonly string[] HeaderTokenPathArguments = ["AntiforgeryExemptions.UploadStartPath", "AntiforgeryExemptions.UploadChunkPath"];

    /// <summary>The one file whose POST mappings may carry the header token check instead of being page handlers or signed endpoints (09 15.7 item 4; D-131).</summary>
    public const string UploadEndpointsFile = "src/Shopfolio.Web/Endpoints/Files/UploadEndpoints.cs";

    public static IReadOnlyList<(string Rule, int Line)> Scan(string relativePath, string text)
    {
        string path = relativePath.Replace('\\', '/');
        bool view = path.EndsWith(".cshtml", StringComparison.OrdinalIgnoreCase);
        string clean = view ? BlankViewComments(text.ReplaceLineEndings("\n")) : BlankCommentLines(text.ReplaceLineEndings("\n"));
        List<(string Rule, int Line)> findings = [];

        Add(findings, IgnoresProtection, IgnoreNames(), clean);
        Add(findings, IgnoresProtection, RequireFalse(), clean);
        if (!path.EndsWith(RegistrationFile, StringComparison.Ordinal))
        {
            Add(findings, DisableOutsideRegistration, DisableName(), clean);
            foreach (Match map in WritingMap().Matches(clean))
            {
                // The one relaxation (D-131): the two POSTs of the chunked upload, mapped in the one file, from the header-token list, with the session and the header check.
                if (!(string.Equals(path, UploadEndpointsFile, StringComparison.Ordinal) && IsHeaderTokenPost(clean, map)))
                {
                    findings.Add((OtherWritingEndpoint, LineOf(clean, map.Index)));
                }
            }

            foreach (Match call in SignedMap().Matches(clean))
            {
                if (!PathComesFromTheList(clean, call))
                {
                    findings.Add((SignedEndpointPathNotListed, LineOf(clean, call.Index)));
                }
            }
        }

        if (!_headerFiles.Any(f => path.EndsWith(f, StringComparison.Ordinal)))
        {
            Add(findings, HeaderWrittenElsewhere, SecurityHeaderNames(), clean);
        }

        Add(findings, UnsafePolicy, UnsafeNames(), clean);
        Add(findings, HstsInApplication, HstsNames(), clean);

        if (view)
        {
            foreach (Match tag in AnyTag().Matches(clean))
            {
                CheckTag(findings, path, clean, tag);
            }
        }

        return [.. findings.Distinct().OrderBy(f => f.Line).ThenBy(f => f.Rule, StringComparer.Ordinal)];
    }

    // The common layout of the brand variables: the storefront frame's layout (T540; 09 16.1 item 6) or a layout named _Layout. The panel's layouts carry no style block at all.
    private static bool IsCommonLayout(string path)
    {
        string normalized = path.Replace('\\', '/');
        return normalized.Contains("/Pages/Shared/_Layout", StringComparison.Ordinal) || normalized.EndsWith("/Pages/Shared/_StorefrontLayout.cshtml", StringComparison.Ordinal);
    }

    private static void CheckTag(List<(string Rule, int Line)> findings, string path, string clean, Match tag)
    {
        string text = tag.Value;
        int line = LineOf(clean, tag.Index);
        string name = TagName().Match(text).Groups[1].Value.ToUpperInvariant();

        if (name == "FORM" && PostMethod().IsMatch(text))
        {
            if (TokenFalse().IsMatch(text))
            {
                findings.Add((TokenSwitchedOff, line));
            }

            if (OwnAction().IsMatch(text) && !TokenTrue().IsMatch(text))
            {
                findings.Add((PostFormWithoutToken, line));
            }

            if (ForeignAction().IsMatch(text))
            {
                findings.Add((PostFormToAnotherSite, line));
            }
        }

        if (StyleAttr().IsMatch(text))
        {
            findings.Add((StyleAttribute, line));
        }

        if (EventAttr().IsMatch(text) || JavascriptAddress().IsMatch(text))
        {
            findings.Add((InlineEventAttribute, line));
        }

        if (name == "SCRIPT" && !SourceAttr().IsMatch(text) && !StructuredData().IsMatch(text))
        {
            findings.Add((ScriptWithoutSource, line));
        }

        if (name == "STYLE" && !(IsCommonLayout(path) && NonceAttr().IsMatch(text)))
        {
            findings.Add((StyleBlockOutsideLayout, line));
        }
    }

    // A MapPost call of the upload endpoints' file that is allowed: it is MapPost (no other writing method), its first argument (the path) is made of AntiforgeryExemptions
    // members and holds no string literal, and its statement (up to the semicolon) carries both RequireAdminSession( and RequireHeaderForgeryToken(.
    private static bool IsHeaderTokenPost(string clean, Match map)
    {
        if (!map.Value.StartsWith("MapPost", StringComparison.Ordinal))
        {
            return false;
        }

        int open = map.Index + map.Length - 1;
        int end = clean.IndexOf(';', open);
        if (end < 0)
        {
            return false;
        }

        string statement = clean[map.Index..end];
        int depth = 0;
        int comma = -1;
        for (int i = open; i < end; i++)
        {
            if (clean[i] is '(' or '[' or '{')
            {
                depth++;
            }
            else if (clean[i] is ')' or ']' or '}')
            {
                depth--;
            }
            else if (clean[i] == ',' && depth == 1)
            {
                comma = i;
                break;
            }
        }

        if (comma < 0)
        {
            return false;
        }

        string pathArgument = clean[(open + 1)..comma];
        return HeaderTokenPathArguments.Contains(pathArgument.Trim(), StringComparer.Ordinal)
            && !pathArgument.Contains('"', StringComparison.Ordinal)
            && statement.Contains(".RequireAdminSession(", StringComparison.Ordinal)
            && statement.Contains(".RequireHeaderForgeryToken(", StringComparison.Ordinal);
    }

    // The path argument of a MapSignedEndpoint call: the call's whole argument list holds AntiforgeryExemptions. and no string literal.
    private static bool PathComesFromTheList(string clean, Match call)
    {
        int open = call.Index + call.Length - 1;
        int depth = 0;
        int end = open;
        for (; end < clean.Length; end++)
        {
            if (clean[end] == '(')
            {
                depth++;
            }
            else if (clean[end] == ')' && --depth == 0)
            {
                break;
            }
        }

        string args = clean[open..Math.Min(end + 1, clean.Length)];
        return args.Contains("AntiforgeryExemptions.", StringComparison.Ordinal) && !args.Contains('"', StringComparison.Ordinal);
    }

    private static int LineOf(string text, int index) => text.AsSpan(0, index).Count('\n') + 1;

    private static void Add(List<(string Rule, int Line)> findings, string rule, Regex pattern, string text)
    {
        foreach (Match match in pattern.Matches(text))
        {
            findings.Add((rule, LineOf(text, match.Index)));
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

    private static string BlankViewComments(string text) =>
        ViewComment().Replace(text, m => string.Concat(m.Value.Select(c => c == '\n' ? '\n' : ' ')));

    [GeneratedRegex(@"@\*.*?\*@|<!--.*?-->", RegexOptions.CultureInvariant | RegexOptions.Singleline, matchTimeoutMilliseconds: 5000)]
    private static partial Regex ViewComment();

    // IgnoreAntiforgeryToken, IgnoreAntiforgeryTokenAttribute, IIgnoreAntiforgeryToken, and the suffix forms of any qualification.
    [GeneratedRegex(@"\bI?IgnoreAntiforgery\w*", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex IgnoreNames();

    // [RequireAntiforgeryToken(false)], [RequireAntiforgeryToken(required: false)], RequireAntiforgeryTokenAttribute(false).
    [GeneratedRegex(@"\bRequireAntiforgeryToken\w*\s*\(\s*(?:required\s*:\s*)?false\b", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex RequireFalse();

    [GeneratedRegex(@"\bDisableAntiforgery\b", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex DisableName();

    // MapPost, MapPut, MapDelete, MapPatch, MapMethods - extension call, static call, with or without type arguments.
    [GeneratedRegex(@"\bMap(?:Post|Put|Delete|Patch|Methods)\b\s*(?:<[^>(]*>)?\s*\(", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex WritingMap();

    // MapSignedEndpoint<A, B>( ... ) - the match ends at the opening parenthesis.
    [GeneratedRegex(@"\bMapSignedEndpoint\b[^(;]*\(", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex SignedMap();

    // The header names and the framework's names for them, in any qualification.
    [GeneratedRegex(@"Content-Security-Policy|Referrer-Policy|Permissions-Policy|\.\s*(?:ContentSecurityPolicy|ReferrerPolicy|PermissionsPolicy)\b|\[\s*""(?:ContentSecurityPolicy|ReferrerPolicy|PermissionsPolicy)""", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase, matchTimeoutMilliseconds: 5000)]
    private static partial Regex SecurityHeaderNames();

    [GeneratedRegex(@"unsafe-inline|unsafe-eval|Content-Security-Policy-Report-Only|ContentSecurityPolicyReportOnly", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase, matchTimeoutMilliseconds: 5000)]
    private static partial Regex UnsafeNames();

    [GeneratedRegex(@"\bUseHsts\b|\bAddHsts\b|\bHstsOptions\b|\bHstsMiddleware\b|Strict-Transport-Security|\.\s*StrictTransportSecurity\b|\[\s*""StrictTransportSecurity""", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase, matchTimeoutMilliseconds: 5000)]
    private static partial Regex HstsNames();

    // Every start tag, over several lines; a Razor expression inside the tag may hold quotes but not a closing angle bracket in the cases the views use.
    [GeneratedRegex(@"<[a-zA-Z][^>]*>", RegexOptions.CultureInvariant | RegexOptions.Singleline, matchTimeoutMilliseconds: 5000)]
    private static partial Regex AnyTag();

    [GeneratedRegex(@"^<([a-zA-Z][a-zA-Z0-9:-]*)", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex TagName();

    [GeneratedRegex(@"\bmethod\s*=\s*[""']?\s*post\b", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase, matchTimeoutMilliseconds: 5000)]
    private static partial Regex PostMethod();

    [GeneratedRegex(@"(?<![\w:-])action\s*=", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase, matchTimeoutMilliseconds: 5000)]
    private static partial Regex OwnAction();

    [GeneratedRegex(@"(?<![\w:-])action\s*=\s*[""']?\s*(?:https?:)?//", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase, matchTimeoutMilliseconds: 5000)]
    private static partial Regex ForeignAction();

    [GeneratedRegex(@"\basp-antiforgery\s*=\s*[""']?\s*false\b", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase, matchTimeoutMilliseconds: 5000)]
    private static partial Regex TokenFalse();

    [GeneratedRegex(@"\basp-antiforgery\s*=\s*[""']?\s*true\b", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase, matchTimeoutMilliseconds: 5000)]
    private static partial Regex TokenTrue();

    [GeneratedRegex(@"(?<![\w:-])style\s*=", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase, matchTimeoutMilliseconds: 5000)]
    private static partial Regex StyleAttr();

    [GeneratedRegex(@"(?<![\w:-])on[a-z]+\s*=", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase, matchTimeoutMilliseconds: 5000)]
    private static partial Regex EventAttr();

    [GeneratedRegex(@"(?:href|src|action|formaction|xlink:href)\s*=\s*[""']?\s*javascript:", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase, matchTimeoutMilliseconds: 5000)]
    private static partial Regex JavascriptAddress();

    [GeneratedRegex(@"(?<![\w:-])src\s*=", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase, matchTimeoutMilliseconds: 5000)]
    private static partial Regex SourceAttr();

    [GeneratedRegex(@"\btype\s*=\s*[""']?\s*application/ld\+json", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase, matchTimeoutMilliseconds: 5000)]
    private static partial Regex StructuredData();

    [GeneratedRegex(@"(?<![\w:-])nonce\s*=", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase, matchTimeoutMilliseconds: 5000)]
    private static partial Regex NonceAttr();
}
