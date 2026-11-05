using System.Text.RegularExpressions;

namespace Shopfolio.UnitTests.Architecture;

/// <summary>
/// Source scan of the cookie rules (T224 KK3; 09 15.1 item 6; 09 5.3 item 5; 05 6.7.10; 09 19.2 item 3). Four rules over the application source:
/// (1) a cookie is appended to a response, or deleted from one, only in <see cref="BrowserCookiesFile"/>; (2) <c>CookieOptions</c>, <c>CookieBuilder</c> and
/// <c>SameSiteMode</c> are created or named only in <see cref="DefaultsFile"/> - the attributes of a cookie are written in one place; (3) the <c>__Host-</c> prefix is
/// written in a string only in <see cref="NamesFile"/> - a cookie name is a constant of that class; (4) the framework's consent banner support
/// (<c>CheckConsentNeeded</c>, <c>UseCookiePolicy</c>, <c>CookiePolicyOptions</c>) is used nowhere: the storefront has no consent banner (02 12.2.5).
/// Each of the three files is allowed for its one rule only. The scan reads the whole text, so a call split over several lines is found; full-line
/// comments are blanked first and keep their line. The test projects are not scanned: they write cookies to prove the rules.
/// </summary>
internal static partial class CookieSourceScan
{
    public const string ResponseCookieWrite = "appends or deletes a response cookie outside BrowserCookies (09 15.1 item 6: the one place a cookie is written)";
    public const string CookieAttributes = "creates CookieOptions or CookieBuilder, or names SameSiteMode, outside CookieDefaults (09 15.1 item 6: the attributes are written in one place)";
    public const string HostPrefixLiteral = "writes the __Host- prefix in a string outside CookieNames (09 5.3 item 5: a cookie name is a constant of the closed list)";
    public const string ConsentBanner = "uses the framework's consent banner support (02 12.2.5: the storefront has no cookie consent banner)";

    /// <summary>The one file that may append and delete response cookies.</summary>
    public const string BrowserCookiesFile = "src/Shopfolio.Web/Infrastructure/Security/BrowserCookies.cs";

    /// <summary>The one file that may name the cookie option types and the same-site modes.</summary>
    public const string DefaultsFile = "src/Shopfolio.Web/Infrastructure/Security/CookieDefaults.cs";

    /// <summary>The one file that may write the prefix in a string.</summary>
    public const string NamesFile = "src/Shopfolio.Web/Infrastructure/Security/CookieNames.cs";

    public static IReadOnlyList<(string Rule, int Line)> Scan(string relativePath, string text)
    {
        string path = relativePath.Replace('\\', '/');
        string clean = BlankCommentLines(text.ReplaceLineEndings("\n"));
        List<(string Rule, int Line)> findings = [];
        if (!string.Equals(path, BrowserCookiesFile, StringComparison.Ordinal))
        {
            Add(findings, ResponseCookieWrite, CookieWrite(), clean);
        }

        if (!string.Equals(path, DefaultsFile, StringComparison.Ordinal))
        {
            Add(findings, CookieAttributes, AttributeTypes(), clean);
        }

        if (!string.Equals(path, NamesFile, StringComparison.Ordinal))
        {
            Add(findings, HostPrefixLiteral, PrefixInString(), clean);
        }

        Add(findings, ConsentBanner, ConsentSupport(), clean);
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

    // Cookies.Append( / Cookies.Delete( on a response (the manager's AppendResponseCookie and DeleteCookie too); the request's cookie collection has neither.
    [GeneratedRegex(@"\bCookies\s*\.\s*(?:Append|Delete)\s*\(|\bAppendResponseCookie\s*\(|\bDeleteCookie\s*\(", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex CookieWrite();

    [GeneratedRegex(@"\bnew\s+(?:CookieOptions|CookieBuilder)\b|\bSameSiteMode\b", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex AttributeTypes();

    // The prefix inside a string literal (ordinary, verbatim or raw): a quote, then anything without a quote or a line end, then the prefix.
    [GeneratedRegex(@"""[^""\n]*__Host-", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex PrefixInString();

    [GeneratedRegex(@"\b(?:CheckConsentNeeded|UseCookiePolicy|CookiePolicyOptions|ConsentCookie)\b", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex ConsentSupport();
}
