using System.Text.RegularExpressions;

namespace Shopfolio.UnitTests.Architecture;

/// <summary>
/// Source scan of the search engine data pattern (T219; 09 9.4 item 3, 09 19.2 item 3; 05 3.7): (1) the one folding of the name (decomposition, diacritic removal,
/// hand-made Turkish letter replacement) is written only in <c>TextNormalizer.cs</c>; (2) the address pattern (a run of characters other than <c>a-z</c> and <c>0-9</c>)
/// is written only in <c>Slug.cs</c>; (3) no identifier, column name or form field is a search engine field the firm could fill (<c>MetaTitle</c>, <c>SeoTitle</c>,
/// <c>OgImage</c>, <c>Canonical...</c> and the like - 02 3.30.5); (4) <c>sitemap.xml</c> and <c>robots.txt</c> are named only in <c>AddressRules.cs</c> (the reserved first
/// segments): their endpoints are T313's; (5) the robots meta tag and the <c>X-Robots-Tag</c> header are written only in <c>SearchEngine/</c> and, with its reason,
/// in the CSV result. The scan reads whole texts (a call split over lines is found), blanks comments first and keeps their lines; <c>.cs</c> and <c>.cshtml</c> files
/// of <c>src</c> are scanned, the test projects are not.
/// </summary>
internal static partial class SearchEngineSourceScan
{
    public const string FoldOutsideNormalizer = "folds a name outside TextNormalizer.cs (Normalize, NormalizationForm, diacritic removal or a hand-made Turkish letter replacement; 09 9.4 items 2 and 3: one function)";
    public const string AddressPatternOutsideSlug = "writes the address pattern [^a-z0-9] outside Slug.cs (09 9.4 item 3: Slug.From is the one function)";
    public const string SearchEngineField = "names a search engine field the firm could fill (MetaTitle, SeoTitle, OgImage, Canonical... ; 02 3.30.5: there is no SEO field)";
    public const string SlugTyped = "has a form field named Slug (02 3.30.2: the address is made on the server from the name; the firm never types it)";
    public const string ProtocolFileOutsideRules = "names sitemap.xml or robots.txt outside AddressRules.cs (T219 criterion 7: the endpoints and contents are T313's)";
    public const string RobotsOutsideSearchEngine = "writes the robots meta tag or the X-Robots-Tag header outside Infrastructure/SearchEngine (05 3.7.4: the closing is the search engine data pattern's)";

    public const string NormalizerFile = "src/Shopfolio.Web/Infrastructure/TextNormalizer.cs";

    public const string SlugFile = "src/Shopfolio.Web/Infrastructure/Slug.cs";

    public const string AddressRulesFile = "src/Shopfolio.Web/Infrastructure/Routing/AddressRules.cs";

    public const string SearchEngineFolder = "src/Shopfolio.Web/Infrastructure/SearchEngine/";

    /// <summary>The closed list of other files that may write the header, each with its reason: the CSV result sets <c>X-Robots-Tag: noindex</c> itself, as a set and not an add (07 4.32.2), so the closing middleware's set leaves one value.</summary>
    public static readonly string[] RobotsExceptions = ["src/Shopfolio.Web/Infrastructure/Forms/CsvFileResult.cs"];

    /// <summary>The files whose code the three users of the common function stand in (T219 KK2): the address, the search term, the product's searched name.</summary>
    public static readonly string[] SimplifyUsers =
    [
        SlugFile,
        "src/Shopfolio.Web/Infrastructure/SearchTerm.cs",
        "src/Shopfolio.Web/Modules/Catalog/Product.cs",
    ];

    public static bool Applies(string relativePath)
    {
        string path = relativePath.Replace('\\', '/');
        return (path.EndsWith(".cs", StringComparison.Ordinal) || path.EndsWith(".cshtml", StringComparison.Ordinal))
            && path.StartsWith("src/", StringComparison.Ordinal)
            && !path.Contains("/obj/", StringComparison.Ordinal)
            && !path.Contains("/bin/", StringComparison.Ordinal);
    }

    public static IReadOnlyList<(string Rule, int Line)> Scan(string relativePath, string text)
    {
        string path = relativePath.Replace('\\', '/');
        string clean = BlankComments(text.ReplaceLineEndings("\n"));
        List<(string Rule, int Line)> findings = [];

        if (!string.Equals(path, NormalizerFile, StringComparison.Ordinal))
        {
            Add(findings, FoldOutsideNormalizer, Folding(), clean);
        }

        if (!string.Equals(path, SlugFile, StringComparison.Ordinal))
        {
            Add(findings, AddressPatternOutsideSlug, AddressPattern(), clean);
        }

        Add(findings, SearchEngineField, SearchEngineFieldName(), clean);

        if (!string.Equals(path, AddressRulesFile, StringComparison.Ordinal))
        {
            Add(findings, ProtocolFileOutsideRules, ProtocolFile(), clean);
        }

        if (!path.StartsWith(SearchEngineFolder, StringComparison.Ordinal) && !RobotsExceptions.Contains(path, StringComparer.Ordinal))
        {
            Add(findings, RobotsOutsideSearchEngine, RobotsMark(), clean);
        }

        if (path.EndsWith(".cshtml", StringComparison.Ordinal))
        {
            Add(findings, SlugTyped, SlugFormField(), clean);
        }

        return [.. findings.Distinct().OrderBy(f => f.Line).ThenBy(f => f.Rule, StringComparer.Ordinal)];
    }

    /// <summary>Whether a text reaches the common folding function in any call form: a call, a method group, a <c>using static</c> alias.</summary>
    public static bool CallsSimplify(string text)
    {
        string clean = BlankComments(text.ReplaceLineEndings("\n"));
        return SimplifyReference().IsMatch(clean);
    }

    private static void Add(List<(string Rule, int Line)> findings, string rule, Regex pattern, string text)
    {
        foreach (Match match in pattern.Matches(text))
        {
            findings.Add((rule, text.AsSpan(0, match.Index).Count('\n') + 1));
        }
    }

    // Comments are blanked and keep their lines: block comments (and the Razor comment) by their newlines, full-line and trailing line comments by their text.
    private static string BlankComments(string text)
    {
        string withoutBlocks = BlockComment().Replace(text, m => new string('\n', m.Value.Count(c => c == '\n')));
        return string.Join('\n', withoutBlocks.Split('\n').Select(StripLineComment));
    }

    // A line comment starts at "//" that is not inside a string or character literal of the line (a URL such as "https://" is kept).
    private static string StripLineComment(string line)
    {
        bool inString = false;
        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];
            if (inString)
            {
                if (c == '\\')
                {
                    i++;
                }
                else if (c == '"')
                {
                    inString = false;
                }
            }
            else if (c == '"')
            {
                inString = true;
            }
            else if (c == '\'' && i + 2 < line.Length)
            {
                i = line[i + 1] == '\\' ? line.IndexOf('\'', i + 2) : i + 2;
                if (i < 0)
                {
                    return line;
                }
            }
            else if (c == '/' && i + 1 < line.Length && line[i + 1] == '/')
            {
                return line[..i];
            }
        }

        return line;
    }

    [GeneratedRegex(@"/\*.*?\*/|@\*.*?\*@", RegexOptions.CultureInvariant | RegexOptions.Singleline, matchTimeoutMilliseconds: 5000)]
    private static partial Regex BlockComment();

    [GeneratedRegex(@"\bNormalizationForm\b|\.\s*Normalize\s*\(|\bRemoveDiacritics\b|\bIgnoreNonSpace\b|\bReplace\s*\(\s*['""][çğıöşüÇĞİÖŞÜ]['""]", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex Folding();

    [GeneratedRegex(@"\[\^\s*a-z\s*0-9", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase, matchTimeoutMilliseconds: 5000)]
    private static partial Regex AddressPattern();

    // The names as identifiers or as parts of a column or form field name (meta_title, seo-title, "ogImage"); the Open Graph type of the page head data is not one.
    [GeneratedRegex(
        @"(?<![A-Za-z0-9])(?:Meta(?:Title|Description|Keywords)|Seo[A-Z_]\w*|Og(?:Image|Title|Description|Url)|Canonical\w*|RobotsMeta\w*)|(?<![a-z0-9])(?:meta[_-](?:title|description|keywords)|seo[_-]\w+|og[_-](?:image|title|description)|canonical[_-]\w+)",
        RegexOptions.CultureInvariant,
        matchTimeoutMilliseconds: 5000)]
    private static partial Regex SearchEngineFieldName();

    // A page's form field named Slug: the address is made on the server from the name and is never typed (02 3.30.2: K-280).
    [GeneratedRegex(@"\bname\s*=\s*""Slug""|\basp-for\s*=\s*""[\w.]*Slug""", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase, matchTimeoutMilliseconds: 5000)]
    private static partial Regex SlugFormField();

    [GeneratedRegex(@"sitemap\.xml|robots\.txt", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase, matchTimeoutMilliseconds: 5000)]
    private static partial Regex ProtocolFile();

    [GeneratedRegex(@"x-robots-tag|""noindex|noindex""|name\s*=\s*\\?""robots", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase, matchTimeoutMilliseconds: 5000)]
    private static partial Regex RobotsMark();

    [GeneratedRegex(@"\bTextNormalizer\s*\.\s*Simplify\b|\busing\s+static\s+[\w.]*TextNormalizer\b", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex SimplifyReference();
}
