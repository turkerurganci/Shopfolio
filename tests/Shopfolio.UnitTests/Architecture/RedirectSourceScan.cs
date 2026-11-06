using System.Text.RegularExpressions;

namespace Shopfolio.UnitTests.Architecture;

/// <summary>
/// Source scan of the redirect rule (T207 KK6; 05 6.5.6; 07 2.3.2; 09 15.2 item 6; D-118). A redirect whose target came from the request must pass the local-path
/// check, so the application writes a redirect in two files only: <see cref="FormResultsFile"/> (the 303 of the page forms, which asks <c>ReturnPath.IsLocalPath</c>)
/// and <see cref="HandlerFile"/> (the 302 and 303 of the access denial, whose target is built by <c>ReturnPath.Resolve</c> or is a constant sign-in address). Every
/// other form is a finding: <c>Redirect(</c>, <c>RedirectPermanent(</c>, <c>RedirectPreserveMethod(</c>, <c>LocalRedirect(</c> and the other <c>*Redirect*(</c> calls,
/// <c>new RedirectResult(</c>, <c>Response.Redirect(</c>, <c>Results.Redirect(</c>, <c>TypedResults.Redirect(</c>, <c>RedirectToPage(</c>, <c>RedirectToAction(</c>,
/// <c>RedirectToRoute(</c>, <c>RedirectUri =</c>, and the <c>Location</c> response header in any form (<c>Headers.Location</c>, <c>HeaderNames.Location</c>,
/// the string <c>"Location"</c>). The scan reads the whole text, so a call split over several lines is found; full-line comments are blanked first and keep their
/// line. The allowed files are exactly two (the path is compared whole), never a folder or a pattern. The test projects are not scanned: they write redirects
/// to prove the rules.
/// </summary>
internal static partial class RedirectSourceScan
{
    public const string RedirectCall = "writes a redirect outside FormResults and SessionAuthenticationHandler (05 6.5.6: a target that came from the request goes through ReturnPath.Resolve)";
    public const string LocationHeader = "names the Location response header outside FormResults and SessionAuthenticationHandler (05 6.5.6: a redirect is written in two files only)";

    /// <summary>The page forms' 303 result; it asks <c>ReturnPath.IsLocalPath</c> before it writes the address.</summary>
    public const string FormResultsFile = "src/Shopfolio.Web/Infrastructure/Forms/FormResults.cs";

    /// <summary>The access denial of the two session schemes; its target is built by <c>ReturnPath.Resolve</c> or is a constant sign-in address.</summary>
    public const string HandlerFile = "src/Shopfolio.Web/Infrastructure/Security/SessionAuthenticationHandler.cs";

    public static IReadOnlyList<(string Rule, int Line)> Scan(string relativePath, string text)
    {
        string path = relativePath.Replace('\\', '/');
        if (string.Equals(path, FormResultsFile, StringComparison.Ordinal) || string.Equals(path, HandlerFile, StringComparison.Ordinal))
        {
            return [];
        }

        string clean = BlankCommentLines(text.ReplaceLineEndings("\n"));
        List<(string Rule, int Line)> findings = [];
        Add(findings, RedirectCall, Calls(), clean);
        Add(findings, LocationHeader, Location(), clean);
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
            if (lines[i].TrimStart().StartsWith("//", StringComparison.Ordinal))
            {
                lines[i] = string.Empty;
            }
        }

        return string.Join('\n', lines);
    }

    // Any call or construction whose name holds "Redirect": Redirect(, RedirectPermanent(, LocalRedirectPermanentPreserveMethod(, new RedirectResult(, new LocalRedirectResult(,
    // Response.Redirect(, Results.Redirect(, TypedResults.LocalRedirect(, RedirectToPage(, RedirectToAction(, RedirectToRoute(, RedirectToFile(; the property RedirectUri.
    // AllowAutoRedirect (a property without a call) is not matched.
    [GeneratedRegex(@"\b\w*Redirect\w*\s*(?:<[^>\n]*>\s*)?\(|\bRedirectUri\s*=", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex Calls();

    // The Location response header in any form: Headers.Location, Headers["Location"], HeaderNames.Location, Append("Location", ...), the string "Location".
    [GeneratedRegex(@"\bHeaders\s*\.\s*Location\b|\bHeaderNames\s*\.\s*Location\b|""Location""", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase, matchTimeoutMilliseconds: 5000)]
    private static partial Regex Location();
}
