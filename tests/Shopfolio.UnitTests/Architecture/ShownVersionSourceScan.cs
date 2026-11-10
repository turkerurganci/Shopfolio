using System.Text.RegularExpressions;

namespace Shopfolio.UnitTests.Architecture;

/// <summary>
/// Source scan of the administrator's version comparison on the order side (T364 KK3, KK4; 09 10.2 item 3; 05 7.3.2; 02 3.31.2): the version the
/// panel showed is compared with the order's version, after the row lock and the load, only by <c>ShownVersion.Matches</c> - one place that says what
/// "the order changed since the screen" means. Over the order-side modules (<c>Modules/Ordering</c>, <c>Modules/AfterSales</c>,
/// <c>Modules/Payment</c>), with comment lines blanked and the whole text read, a comparison written by hand is a finding:
/// <list type="bullet">
/// <item>an equality operator next to a property named exactly <c>Version</c>, on either side (<c>order.Version != shown</c>, <c>shown == order.Version</c>,
/// <c>request.ExpectedVersion != order.Version</c>), also over a line break;</item>
/// <item>a pattern test of it (<c>order.Version is not 3</c>);</item>
/// <item>an <c>Equals</c> call on it or with it among the arguments (<c>order.Version.Equals(shown)</c>, <c>Equals(order.Version, shown)</c>, <c>shown.Equals(order.Version)</c>).</item>
/// </list>
/// Other names that end in "Version" (a legal text's <c>TemplateVersion</c>, a settings section's version) are not the order's and are not matched. There
/// is no allowed file in the scanned folders: <c>ShownVersion</c> lives in <c>Infrastructure/Forms/RecordSave.cs</c>, outside them. The test projects are not
/// scanned: they write violations to prove the rule.
/// </summary>
internal static partial class ShownVersionSourceScan
{
    public const string HandWrittenComparison =
        "compares the order's version by hand (09 10.2 item 3: the administrator's shown version is compared only by ShownVersion.Matches, after the lock and the load)";

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
        foreach (Regex pattern in new[] { VersionThenOperator(), OperatorThenVersion(), EqualsOnVersion(), EqualsWithVersion() })
        {
            foreach (Match match in pattern.Matches(clean))
            {
                findings.Add((HandWrittenComparison, clean.AsSpan(0, match.Index).Count('\n') + 1));
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

    // order.Version != shown, order.Version == shown, order!.Version is not 3 - the operator after the property (whitespace and line breaks between).
    [GeneratedRegex(@"\.Version\b\s*(?:!=|==|\bis\b)", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex VersionThenOperator();

    // shown != order.Version, request.ExpectedVersion == order!.Version, x != (order).Version - the operator before the property.
    [GeneratedRegex(@"(?:!=|==)\s*[\w.!?()\[\]]*\.Version\b", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex OperatorThenVersion();

    // order.Version.Equals(shown).
    [GeneratedRegex(@"\.Version\s*\.\s*Equals\s*\(", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex EqualsOnVersion();

    // Equals(order.Version, shown), object.Equals(shown, order.Version), shown.Equals(order.Version) - the property among the call's arguments.
    [GeneratedRegex(@"\bEquals\s*\([^;)]*\.Version\b", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex EqualsWithVersion();
}
