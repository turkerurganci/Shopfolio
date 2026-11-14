using System.Text.RegularExpressions;

namespace Shopfolio.UnitTests.Architecture;

/// <summary>
/// Source scan of the page address rules (T363; 02 3.30.2, 3.30.3, 3.30.7; 09 9.4 item 3): (1) there is no redirect from an old address - no table, column, type or call that
/// keeps a record's former address or maps one address to another (a slug history, an old-slug column, a redirect map, a rewrite rule); (2) no platform's share address or
/// share script is written in the source (the one "Paylaş" button uses the device's menu or the clipboard); (3) a record's address is written to its entity in one place only -
/// the corporate content entry (<c>ContentAuthoring.cs</c>) - through <c>x.Slug = ...</c>; the product writes it through <c>Product.SetAddress</c> and a new record takes it in its
/// <c>Create</c>. <c>.cs</c>, <c>.cshtml</c> and <c>.js</c> files of <c>src</c> are scanned (the migrations included); comments are blanked and keep their lines.
/// </summary>
internal static partial class AddressSourceScan
{
    public const string RedirectFromOldAddress = "keeps a former address or redirects one address to another (02 3.30.3: there is no redirect system - the address does not change, so none is needed)";
    public const string PlatformShare = "writes a platform's share address or share script (02 3.30.7: one \"Paylaş\" button; no Facebook, X, WhatsApp or other platform code)";
    public const string SlugAssignedOutsideItsPlace = "assigns a record's Slug outside ContentAuthoring.cs (02 3.30.2; 06 4.72.3: the address is written by the entry that made it, and a fixed one is never rewritten)";

    public const string ContentEntryFile = "src/Shopfolio.Web/Modules/CorporateContent/ContentAuthoring.cs";

    public static bool Applies(string relativePath)
    {
        string path = relativePath.Replace('\\', '/');
        bool extension = path.EndsWith(".cs", StringComparison.Ordinal) || path.EndsWith(".cshtml", StringComparison.Ordinal) || path.EndsWith(".js", StringComparison.Ordinal);
        return extension && path.StartsWith("src/", StringComparison.Ordinal) && !path.Contains("/obj/", StringComparison.Ordinal) && !path.Contains("/bin/", StringComparison.Ordinal);
    }

    public static IReadOnlyList<(string Rule, int Line)> Scan(string relativePath, string text)
    {
        string path = relativePath.Replace('\\', '/');
        string clean = BlankComments(text.ReplaceLineEndings("\n"));
        List<(string Rule, int Line)> findings = [];
        Add(findings, RedirectFromOldAddress, OldAddress(), clean);
        Add(findings, PlatformShare, PlatformAddress(), clean);
        if (!string.Equals(path, ContentEntryFile, StringComparison.Ordinal))
        {
            Add(findings, SlugAssignedOutsideItsPlace, SlugAssignment(), clean);
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

    // Full-line comments (C#, script, Razor) are blanked and keep their line; so are block comments, by their newlines.
    private static string BlankComments(string text)
    {
        string withoutBlocks = BlockComment().Replace(text, m => new string('\n', m.Value.Count(c => c == '\n')));
        return string.Join('\n', withoutBlocks.Split('\n').Select(l => l.TrimStart().StartsWith("//", StringComparison.Ordinal) ? string.Empty : l));
    }

    // An identifier, table or column that keeps a former address or maps an address: slug history, old/previous/former slug, redirect map or rule or table, rewrite rule, the rewrite middleware.
    [GeneratedRegex(
        @"(?<![A-Za-z0-9])(?:Slug|Url|Path)History\w*|(?:Old|Previous|Former|Legacy|Prior)(?:Slug|Url|Path)\w*|\bSlugRedirect\w*|\bRedirect(?:Map|Rule|Table|Entry|s)\b|\bUrlRewrite\w*|\bAddRewrite\b|\bUseRewriter\b|\bAddRedirect\w*\b|slug_history|(?:old|previous|former)_(?:slug|url)|\bredirects?\b(?=\s*[(=:])(?!\s*:\s*[""']manual[""'])",
        RegexOptions.CultureInvariant,
        matchTimeoutMilliseconds: 5000)]
    private static partial Regex OldAddress();

    // The share addresses and scripts of the platforms and of the share widgets. A WhatsApp chat link to the firm's own number (wa.me/ then digits; 04 2.2.3) is not a share address.
    [GeneratedRegex(
        @"sharer\.php|facebook\.com/(?:sharer|dialog/share)|connect\.facebook\.net|twitter\.com/(?:intent|share)|x\.com/(?:intent|share)|platform\.twitter\.com|api\.whatsapp\.com|web\.whatsapp\.com|\bwa\.me\b(?!/[0-9])|linkedin\.com/(?:sharing|shareArticle)|t\.me/share|telegram\.me/share|pinterest\.com/pin/create|reddit\.com/submit|addthis|sharethis|addtoany",
        RegexOptions.CultureInvariant | RegexOptions.IgnoreCase,
        matchTimeoutMilliseconds: 5000)]
    private static partial Regex PlatformAddress();

    // A member assignment of Slug (record.Slug = value); an initializer (Slug = value inside new X { }) and a comparison are not matched.
    [GeneratedRegex(@"[A-Za-z0-9_\)\]]\s*\.\s*Slug\s*=(?![=>])", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex SlugAssignment();

    [GeneratedRegex(@"/\*.*?\*/|@\*.*?\*@", RegexOptions.CultureInvariant | RegexOptions.Singleline, matchTimeoutMilliseconds: 5000)]
    private static partial Regex BlockComment();
}
