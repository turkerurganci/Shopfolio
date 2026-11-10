using System.Text.RegularExpressions;

namespace Shopfolio.UnitTests.Architecture;

/// <summary>
/// Source scan of the administrator's version comparison on the order side (T364 KK3, KK4; 09 10.2 item 3; 05 7.3.2; 02 3.31.2): the version the
/// panel showed is compared with the order's version, after the row lock and the load, only by <c>ShownVersion.Matches</c> - one place that says what
/// "the order changed since the screen" means. Over the order-side modules (<c>Modules/Ordering</c>, <c>Modules/AfterSales</c>,
/// <c>Modules/Payment</c>), with comment lines blanked and the whole text read, a comparison written by hand is a finding:
/// <list type="bullet">
/// <item>an equality or relational operator next to a property named exactly <c>Version</c>, on either side (<c>order.Version != shown</c>, <c>shown == order.Version</c>,
/// <c>request.ExpectedVersion != order.Version</c>, <c>(order.Version) != shown</c>, <c>order.Version &lt; shown</c>), also over a line break;</item>
/// <item>a pattern test of it (<c>order.Version is not 3</c>) and a property pattern on it (<c>order is { Version: 3 }</c>, <c>case { Version: 3 }</c>);</item>
/// <item>an <c>Equals</c>, <c>CompareTo</c> or <c>Compare</c> call on it or with it among the arguments (<c>order.Version.Equals(shown)</c>, <c>Equals(order.Version, shown)</c>,
/// <c>shown.Equals(order.Version)</c>, <c>EqualityComparer&lt;int&gt;.Default.Equals(order.Version, shown)</c>, <c>order.Version.CompareTo(shown)</c>).</item>
/// </list>
/// A comparison skipped when the version is missing is a finding too (T364 B1; KK4): a null test joined to <c>ShownVersion.Matches</c>
/// (<c>expected is { } shown &amp;&amp; !ShownVersion.Matches(order, shown)</c>, <c>expected is null || ShownVersion.Matches(...)</c>, <c>.HasValue</c>,
/// <c>== null</c>, <c>!= null</c>, <c>is not null</c>), a conditional on it (<c>expected is int v ? ... : false</c>), a null-conditional before it
/// (<c>expected?.Let(v =&gt; ShownVersion.Matches(order, v))</c>) and a stand-in for the missing version among its arguments (<c>expected ?? 0</c>). Whether an
/// operation is the administrator's - and so must carry the version - is said once, by <c>ShownVersion.Check</c>.
/// Copying the version into a local first (<c>int current = order.Version;</c>) and comparing the local is a known limit of a text scan: reading the
/// version (to show it, to return it) is allowed, so the copy alone is not a finding.
/// Other names that end in "Version" (a legal text's <c>TemplateVersion</c>, a settings section's version) are not the order's and are not matched. There
/// is no allowed file in the scanned folders: <c>ShownVersion</c> lives in <c>Infrastructure/Forms/RecordSave.cs</c>, outside them. The test projects are not
/// scanned: they write violations to prove the rule.
/// </summary>
internal static partial class ShownVersionSourceScan
{
    public const string HandWrittenComparison =
        "compares the order's version by hand (09 10.2 item 3: the administrator's shown version is compared only by ShownVersion.Matches, after the lock and the load)";

    public const string SkippedComparison =
        "skips the comparison when the version is missing (T364 KK4; 09 10.2 item 3: an administrator's request without the shown version applies nothing - ShownVersion.Check decides, not a null test)";

    /// <summary>The folders whose files are scanned, relative to the repository root.</summary>
    public static IReadOnlyList<string> ScannedFolders { get; } =
    [
        "src/Shopfolio.Web/Modules/Ordering/",
        "src/Shopfolio.Web/Modules/AfterSales/",
        "src/Shopfolio.Web/Modules/Payment/",
    ];

    public static bool IsScanned(string relativePath)
    {
        string path = relativePath.Replace('\\', '/');
        return path.EndsWith(".cs", StringComparison.OrdinalIgnoreCase) && ScannedFolders.Any(folder => path.StartsWith(folder, StringComparison.Ordinal));
    }

    public static IReadOnlyList<(string Rule, int Line)> Scan(string relativePath, string text)
    {
        if (!IsScanned(relativePath))
        {
            return [];
        }

        string clean = BlankCommentLines(text.ReplaceLineEndings("\n"));
        List<(string Rule, int Line)> findings = [];
        foreach (Regex pattern in new[] { VersionThenOperator(), OperatorThenVersion(), EqualsOnVersion(), EqualsWithVersion(), PropertyPatternOnVersion() })
        {
            foreach (Match match in pattern.Matches(clean))
            {
                findings.Add((HandWrittenComparison, clean.AsSpan(0, match.Index).Count('\n') + 1));
            }
        }

        foreach (Regex pattern in new[] { NullGuardThenMatches(), NullOrMatches(), ConditionalAroundMatches(), NullConditionalAroundMatches() })
        {
            foreach (Match match in pattern.Matches(clean))
            {
                findings.Add((SkippedComparison, clean.AsSpan(0, match.Index).Count('\n') + 1));
            }
        }

        return [.. findings.Distinct().OrderBy(f => f.Line)];
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

    // order.Version != shown, (order.Version) == shown, order!.Version is not 3, order.Version < shown - the operator after the property (whitespace,
    // closing parentheses and line breaks between). The lambda arrow (=>) is not an operator here.
    [GeneratedRegex(@"\.Version\b[\s)]*(?:!=|==|<=|>=|<|>|\bis\b)", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex VersionThenOperator();

    // shown != order.Version, request.ExpectedVersion == order!.Version, x != (order).Version, shown > order.Version - the operator before the property;
    // the '>' of a lambda arrow (o => o.Version) is not one.
    [GeneratedRegex(@"(?:!=|==|<=|>=|<|(?<![=\-])>)\s*[\w.!?()\[\]]*\.Version\b", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex OperatorThenVersion();

    // order.Version.Equals(shown), order.Version.CompareTo(shown).
    [GeneratedRegex(@"\.Version\s*\.\s*(?:Equals|CompareTo)\s*\(", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex EqualsOnVersion();

    // Equals(order.Version, shown), object.Equals(Shown(form), order.Version), shown.Equals(order.Version), EqualityComparer<int>.Default.Equals(...),
    // Comparer<int>.Default.Compare(...), shown.CompareTo(order.Version) - the property among the call's arguments (one level of nested call allowed).
    [GeneratedRegex(@"\b(?:Equals|CompareTo|Compare)\s*\((?:[^;()]|\([^;()]*\))*\.Version\b", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex EqualsWithVersion();

    // order is { Version: 3 }, order is not { Version: var v } and v != shown, case { Version: 3 }, a switch arm { Version: 3 } => - a property pattern
    // on Version. A conditional's ':' (flag ? order.Version : 0) is not one: the pattern starts after 'is', 'case', 'and', 'or', 'not' or right after '{'.
    [GeneratedRegex(@"(?:\b(?:is|case|and|or|not)\s+\{[^{};]*\bVersion|\{\s*Version)\s*:", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex PropertyPatternOnVersion();

    // expected is { } shown && !ShownVersion.Matches(order, shown), expected is int v && ..., expected is not null && ..., expected != null && ...,
    // expected.HasValue && ... - a null test that lets a request without a version past the comparison (the T364 B1 shape), also over a line break.
    [GeneratedRegex(@"(?:\bis\s+(?:not\s+null\b|\{\s*\}|int\b)[\w\s]*|!=\s*null\b|\.HasValue\b)\s*\)*\s*&&\s*!?\s*\(*\s*ShownVersion\s*\.\s*Matches\b", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex NullGuardThenMatches();

    // expected is null || ShownVersion.Matches(...), expected == null || ..., !expected.HasValue || ... - "no version, or it matches" lets the empty one through.
    [GeneratedRegex(@"(?:\bis\s+null\b|==\s*null\b|!\s*[\w.?!]*\.HasValue\b)\s*\)*\s*\|\|\s*!?\s*\(*\s*ShownVersion\s*\.\s*Matches\b", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex NullOrMatches();

    // expected is int v ? !ShownVersion.Matches(order, v) : false - a conditional that compares only when there is a version.
    [GeneratedRegex(@"\bis\s+(?:\{\s*\}|int)\s+\w+\s*\)*\s*\?\s*!?\s*\(*\s*ShownVersion\s*\.\s*Matches\b", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex ConditionalAroundMatches();

    // expected?.Let(v => ShownVersion.Matches(order, v)) - the comparison behind a null-conditional; ShownVersion.Matches(order, expected ?? 0) - a stand-in
    // for the missing version among the call's arguments (one level of nested call allowed).
    [GeneratedRegex(@"\?\.[^;{}]*ShownVersion\s*\.\s*Matches\b|ShownVersion\s*\.\s*Matches\s*\((?:[^;(){}]|\([^;(){}]*\))*\?\?", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex NullConditionalAroundMatches();
}
