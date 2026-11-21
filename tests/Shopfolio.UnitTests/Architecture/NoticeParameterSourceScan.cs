using System.Text.RegularExpressions;

namespace Shopfolio.UnitTests.Architecture;

/// <summary>
/// The scan of the <c>notice</c> parameter (T214 criteria 2 and 3, KK2; 07 2.2.3, 6.3; 09 5.3 item 6, 8.2 item 4): the parameter is written and read in one
/// place - <see cref="WriterFile"/> - so an address that carries free text or a value outside the closed list cannot be built anywhere else. Every source
/// file of the application (<c>.cs</c> and <c>.cshtml</c>) is read line by line, comments included, and these forms are findings outside that file: the
/// parameter name as a whole string in any quoting (plain, verbatim, raw, char-quoted, case-insensitive: a bound name, a query index, a route value, an
/// attribute's <c>Name</c>), the parameter in an address literal (<c>?notice=</c>, <c>&amp;notice=</c>, interpolated or joined, with or without an encoded
/// separator), and the tag helper form of a view (<c>asp-route-notice</c>, <c>href="...notice="</c>). The file is named by its exact path, never by a pattern, and the real file
/// is checked to still hold the forms (the scan is not empty).
/// </summary>
internal static partial class NoticeParameterSourceScan
{
    public const string ParameterNameLiteral = "NoticeParameterNameOutsideTheWriter";

    public const string AddressLiteral = "NoticeInAnAddressOutsideTheWriter";

    public const string RouteValue = "NoticeRouteValueInAView";

    public const string HandlerParameterRule = "NoticeHandlerParameterOutsideTheWriter";

    public const string BoundPropertyRule = "NoticeBoundPropertyOutsideTheWriter";

    public const string NameConstantRule = "NoticeNameConstantReadOutsideTheWriter";

    public const string WriterFile = "src/Shopfolio.Web/Infrastructure/Notices/NoticeParameter.cs";

    public static bool AppliesTo(string relativePath) => Normalize(relativePath).StartsWith("src/", StringComparison.Ordinal);

    public static IReadOnlyList<(string Rule, int Line)> Scan(string relativePath, string text)
    {
        if (string.Equals(Normalize(relativePath), WriterFile, StringComparison.Ordinal))
        {
            return [];
        }

        bool view = Normalize(relativePath).EndsWith(".cshtml", StringComparison.OrdinalIgnoreCase);
        string[] lines = text.ReplaceLineEndings("\n").Split('\n');
        List<(string Rule, int Line)> findings = [];
        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i];
            if (ParameterName().IsMatch(line))
            {
                findings.Add((ParameterNameLiteral, i + 1));
            }

            if (InAddress().IsMatch(line))
            {
                findings.Add((AddressLiteral, i + 1));
            }

            if (view && ViewRoute().IsMatch(line))
            {
                findings.Add((RouteValue, i + 1));
            }
        }

        if (!view)
        {
            AddUnquotedReads(text, findings);
        }

        return [.. findings.OrderBy(static f => f.Line)];
    }

    // The reads that carry no quoted name (D-146): a page handler parameter named notice (the framework binds it by name), a bound property named Notice
    // (query binding by name) and the writer's name constant used to read the value outside the writer (a presence check is no read of the value).
    private static void AddUnquotedReads(string text, List<(string Rule, int Line)> findings)
    {
        string normalized = text.ReplaceLineEndings("\n");
        foreach (Match match in HandlerParameter().Matches(normalized))
        {
            findings.Add((HandlerParameterRule, LineOf(normalized, match.Index)));
        }

        foreach (Match match in BoundProperty().Matches(normalized))
        {
            findings.Add((BoundPropertyRule, LineOf(normalized, match.Index)));
        }

        string[] lines = normalized.Split('\n');
        for (int i = 0; i < lines.Length; i++)
        {
            if (NameConstant().IsMatch(lines[i]) && !PresenceCheck().IsMatch(lines[i]))
            {
                findings.Add((NameConstantRule, i + 1));
            }
        }
    }

    private static int LineOf(string text, int index)
    {
        int line = 1;
        for (int i = 0; i < index; i++)
        {
            if (text[i] == '\n')
            {
                line++;
            }
        }

        return line;
    }

    private static string Normalize(string path) => path.Replace('\\', '/');

    // "notice", @"notice", """notice""", 'notice', "Notice " is a whole string: the quote, the word and the quote (whitespace inside a quote counts).
    [GeneratedRegex(@"[""']\s*notice\s*[""']|[""']notice=", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex ParameterName();

    // ?notice= &notice= &amp;notice= %3Fnotice= %26notice=: the parameter in the text of an address, wherever the line breaks the literal.
    [GeneratedRegex(@"(?:[?&]|&amp;|%3F|%26)notice=", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex InAddress();

    // OnGet(string? notice), OnPostSaveAsync(int id, [FromQuery] string? Notice, CancellationToken ct): a handler parameter named notice, the parameter list read to its closing bracket
    // (nested brackets of an attribute included); the name matches without case, as the binder does.
    [GeneratedRegex(@"\bOn(?:Get|Post|Put|Delete|Patch|Head)\w*\s*\((?:[^()]|\([^()]*\))*?\bnotice\s*[,)=]", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex HandlerParameter();

    // [BindProperty(SupportsGet = true)] public string? Notice { get; set; } and [FromQuery] public string? Notice { get; set; }: a query-bound property named Notice.
    [GeneratedRegex(@"\[\s*(?:BindProperty\s*\([^\]]*SupportsGet\s*=\s*true[^\]]*|FromQuery[^\]]*)\]\s*(?:\[[^\]]*\]\s*)*(?:(?:public|internal|protected|private|required)\s+)*[\w<>?.,\[\]]+\s+Notice\s*[{;=]", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex BoundProperty();

    // NoticeParameter.ParameterName outside the writer: reading the value through the name constant (Request.Query[NoticeParameter.ParameterName], TryGetValue) skips the closed list.
    [GeneratedRegex(@"\bNoticeParameter\s*\.\s*ParameterName\b", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex NameConstant();

    // The one use that is no read of the value: asking whether the query carries the parameter (a page that closes to search engines when it does).
    [GeneratedRegex(@"\.\s*ContainsKey\s*\(\s*NoticeParameter\s*\.\s*ParameterName\s*\)", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex PresenceCheck();

    // asp-route-notice="..." and data of a route value named notice in a view.
    [GeneratedRegex(@"asp-route-notice\b|\bnotice\s*=\s*[""']", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex ViewRoute();
}
