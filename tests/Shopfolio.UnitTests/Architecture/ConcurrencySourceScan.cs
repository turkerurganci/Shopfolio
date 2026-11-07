using System.Text;
using System.Text.RegularExpressions;

namespace Shopfolio.UnitTests.Architecture;

/// <summary>
/// Source scan of the concurrency code's single places (T228 KK9 and criterion 5; 09 10.2 items 4-6, 9.1 item 5, 10.9 item 2): the advisory lock
/// and the row lock text (<c>pg_advisory</c>, <c>pg_try_advisory</c>, <c>FOR UPDATE</c>, <c>FOR NO KEY UPDATE</c>) stands only in <c>AdvisoryLocks.cs</c> and
/// <c>OrderLocks.cs</c> (the settings writer's lock of 09 10.3 joins them with that writer's file - no path is allowed before its file exists); the two more places are the digital file's row lock (<c>DigitalFileLocks.cs</c>, T312) and the job queue's
/// <c>JobClaim.cs</c>, and only for the plain <c>FOR UPDATE</c> text (the row lock of taking a row, T220; no advisory text, no <c>NO KEY</c> form there); the shared row lock text (<c>FOR SHARE</c>, <c>FOR KEY SHARE</c>) only in
/// <c>OrderLocks.cs</c>; and a counter - the stock or capacity field, the reserved quantity, the coupon's used and reserved rights - is written
/// by a bulk update (<c>SetProperty</c>) or a raw <c>UPDATE ... SET</c> only in <c>StockCounters.cs</c>. The places are full paths, not file
/// names: a file of the same name elsewhere is scanned. The scan reads the whole text, so a statement split over lines is found; full-line
/// comments are blanked first and keep their line. A <c>SetProperty</c> is a finding only when its first argument - the property it writes - names
/// a counter; a counter read in the value argument is not a write.
/// </summary>
internal static partial class ConcurrencySourceScan
{
    public const string RowLockText = "takes an advisory or row lock outside AdvisoryLocks.cs and OrderLocks.cs (09 10.2 items 5, 6)";

    public const string ShareLockText = "takes a shared row lock outside OrderLocks.cs (09 10.2 item 6, 10.9 item 2)";

    public const string CounterWrite = "writes a counter outside StockCounters.cs (09 10.2 item 4: the counters' one writer)";

    public const string AdvisoryLocksFile = "src/Shopfolio.Web/Infrastructure/AdvisoryLocks.cs";

    public const string JobClaimFile = "src/Shopfolio.Web/Infrastructure/Jobs/JobClaim.cs";

    public const string OrderLocksFile = "src/Shopfolio.Web/Modules/Ordering/OrderLocks.cs";

    public const string DigitalFileLocksFile = "src/Shopfolio.Web/Modules/Catalog/DigitalFileLocks.cs";

    public const string StockCountersFile = "src/Shopfolio.Web/Modules/Catalog/StockCounters.cs";

    /// <summary>The counter properties (06 2.6.5) and their columns.</summary>
    public static readonly IReadOnlyList<string> CounterProperties = ["StockQuantity", "ReservedQuantity", "UsedCount", "ReservedCount"];

    /// <summary>The scan applies to every application source file; the allowed places are per rule.</summary>
    public static bool Applies(string relativePath)
    {
        return relativePath.Replace('\\', '/').EndsWith(".cs", StringComparison.Ordinal);
    }

    public static IReadOnlyList<(string Rule, int Line)> Scan(string relativePath, string text)
    {
        string path = relativePath.Replace('\\', '/');
        string clean = BlankCommentLines(text.ReplaceLineEndings("\n"));
        List<(string Rule, int Line)> findings = [];
        if (path is not (AdvisoryLocksFile or OrderLocksFile))
        {
            Add(findings, RowLockText, AdvisoryText(), clean);
            Add(findings, RowLockText, WeakRowLockText(), clean);
            if (path != JobClaimFile && path != DigitalFileLocksFile)
            {
                // The job queue's taking file and the digital file row lock (T312; 09 15.7 item 6) are the places with the plain row lock text, and only that text (T220; 09 13.1 item 3).
                Add(findings, RowLockText, PlainRowLockText(), clean);
            }
        }

        if (path != OrderLocksFile)
        {
            Add(findings, ShareLockText, ShareLock(), clean);
        }

        if (path != StockCountersFile)
        {
            Add(findings, CounterWrite, CounterColumnUpdate(), clean);
            foreach (Match call in SetPropertyCall().Matches(clean))
            {
                int open = call.Index + call.Length - 1;
                string first = FirstArgument(clean, open);
                if (CounterProperties.Any(p => Regex.IsMatch(first, $@"\b{p}\b", RegexOptions.CultureInvariant, TimeSpan.FromSeconds(5))))
                {
                    AddAt(findings, CounterWrite, clean, call.Index);
                }
            }
        }

        findings.Sort((a, b) => a.Line.CompareTo(b.Line));
        return findings;
    }

    private static void Add(List<(string Rule, int Line)> findings, string rule, Regex pattern, string text)
    {
        foreach (Match match in pattern.Matches(text))
        {
            AddAt(findings, rule, text, match.Index);
        }
    }

    private static void AddAt(List<(string Rule, int Line)> findings, string rule, string text, int index)
    {
        int line = text.AsSpan(0, index).Count('\n') + 1;
        if (!findings.Contains((rule, line)))
        {
            findings.Add((rule, line));
        }
    }

    private static string FirstArgument(string text, int open)
    {
        int depth = 0;
        for (int i = open; i < text.Length; i++)
        {
            char c = text[i];
            if (c is '(' or '<' or '[' or '{')
            {
                depth++;
            }
            else if (c is ')' or ']' or '}' || (c == '>' && text[i - 1] != '='))
            {
                if (--depth == 0)
                {
                    return text[(open + 1)..i];
                }
            }
            else if (c == ',' && depth == 1)
            {
                return text[(open + 1)..i];
            }
        }

        return text[(open + 1)..];
    }

    private static string BlankCommentLines(string text)
    {
        StringBuilder builder = new();
        foreach (string line in text.Split('\n'))
        {
            builder.Append(line.TrimStart().StartsWith("//", StringComparison.Ordinal) ? string.Empty : line).Append('\n');
        }

        return builder.ToString(0, builder.Length - 1);
    }

    [GeneratedRegex(@"\bpg_(?:try_)?advisory\w*", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex AdvisoryText();

    [GeneratedRegex(@"\bFOR\s+NO\s+KEY\s+UPDATE\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex WeakRowLockText();

    [GeneratedRegex(@"\bFOR\s+UPDATE\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex PlainRowLockText();

    [GeneratedRegex(@"\bFOR\s+(?:KEY\s+)?SHARE\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex ShareLock();

    [GeneratedRegex(@"\bUPDATE\s+[\w.""]+\s+SET\b[^;]*?\b(?:stock_quantity|reserved_quantity|used_count|reserved_count)\s*=", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex CounterColumnUpdate();

    [GeneratedRegex(@"\bSetProperty\s*(?:<[^()]*?>)?\s*\(", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex SetPropertyCall();
}
