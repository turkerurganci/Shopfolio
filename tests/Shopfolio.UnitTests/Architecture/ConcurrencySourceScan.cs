using System.Text;
using System.Text.RegularExpressions;

namespace Shopfolio.UnitTests.Architecture;

/// <summary>
/// Source scan of the concurrency code's single places (T228 KK9 and criterion 5; 09 10.2 items 4-6, 9.1 item 5, 10.9 item 2): the advisory lock
/// and the row lock text (<c>pg_advisory</c>, <c>pg_try_advisory</c>, <c>FOR UPDATE</c>, <c>FOR NO KEY UPDATE</c>) stands only in <c>AdvisoryLocks.cs</c> and
/// <c>OrderLocks.cs</c>; the plain <c>FOR UPDATE</c> text is also allowed in <c>CompanySettingsWriter.cs</c> (the settings row lock of 09 10.3 item 2, T330), which is also the one place that bulk-writes the settings row (09 10.3 item 4: <c>ExecuteUpdate</c>, <c>ExecuteDelete</c>, <c>SaveChanges</c> in a file that names the row's type, or a raw <c>UPDATE</c>/<c>DELETE</c> of <c>company_settings</c> elsewhere is a finding); the more places are the digital file's row lock (<c>DigitalFileLocks.cs</c>, T312), the account row lock (<c>AccountLocks.cs</c>, T342), the coupon row lock of the panel change (<c>CouponLocks.cs</c>, T526) and the job queue's
/// <c>JobClaim.cs</c>, and only for the plain <c>FOR UPDATE</c> text (the row lock of taking a row, T220; no advisory text, no <c>NO KEY</c> form there); the <c>FOR NO KEY UPDATE</c> text, and only it, also stands in the product row lock (<c>ProductLocks.cs</c>, T525: the order lines' foreign key asks the product row for <c>FOR KEY SHARE</c>, which the plain form blocks); the shared row lock text (<c>FOR SHARE</c>, <c>FOR KEY SHARE</c>) only in
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

    public const string CounterWrite = "writes a counter outside its one writer: StockCounters.cs, or DownloadCounters.cs for the download counter (09 10.2 item 4)";

    public const string CompanySettingsWrite = "writes the company settings row outside CompanySettingsWriter.cs (09 10.3 item 4: the row's one writer)";

    public const string CompanySettingsWriterFile = "src/Shopfolio.Web/Modules/Settings/CompanySettingsWriter.cs";

    public const string AdvisoryLocksFile = "src/Shopfolio.Web/Infrastructure/AdvisoryLocks.cs";

    public const string JobClaimFile = "src/Shopfolio.Web/Infrastructure/Jobs/JobClaim.cs";

    public const string OrderLocksFile = "src/Shopfolio.Web/Modules/Ordering/OrderLocks.cs";

    public const string AccountLocksFile = "src/Shopfolio.Web/Infrastructure/Security/AccountLocks.cs";

    public const string DigitalFileLocksFile = "src/Shopfolio.Web/Modules/Catalog/DigitalFileLocks.cs";

    public const string CouponLocksFile = "src/Shopfolio.Web/Modules/Catalog/CouponLocks.cs";

    public const string ProductLocksFile = "src/Shopfolio.Web/Modules/Catalog/ProductLocks.cs";

    public const string StockCountersFile = "src/Shopfolio.Web/Modules/Catalog/StockCounters.cs";

    public const string DownloadCountersFile = "src/Shopfolio.Web/Modules/Ordering/DownloadCounters.cs";

    /// <summary>The counter properties (06 2.6.5) and their columns.</summary>
    public static readonly IReadOnlyList<string> StockCounterProperties = ["StockQuantity", "ReservedQuantity", "UsedCount", "ReservedCount"];

    /// <summary>The download counter (06 4.18; T340): written only by <c>DownloadCounters.cs</c>.</summary>
    public static readonly IReadOnlyList<string> DownloadCounterProperties = ["DownloadCount"];

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
            if (path != ProductLocksFile)
            {
                // The product row lock (T525; 09 10.2 item 5) is the one place with the NO KEY form, and only that form: the order's lines' foreign key asks the product row
                // for a KEY SHARE lock, which a plain FOR UPDATE would block (a deadlock with the order confirmation that has already written the variant).
                Add(findings, RowLockText, WeakRowLockText(), clean);
            }

            if (path is not (JobClaimFile or CompanySettingsWriterFile or DigitalFileLocksFile or AccountLocksFile or CouponLocksFile))
            {
                // The job queue's taking file, the settings writer and the digital file row lock (T312; 09 15.7 item 6), the account row lock (T342; 09 15.1 item 3) and the
                // coupon row lock of the panel change (T526; 05 7.3.5, 02 3.10.6) are the places with the plain row lock text,
                // and only that text (T220; 09 13.1 item 3; T330, 09 10.3 item 2 (a)).
                Add(findings, RowLockText, PlainRowLockText(), clean);
            }
        }

        if (path != CompanySettingsWriterFile)
        {
            // Bulk writes of the settings row (09 10.3 item 4): a file that names the row's type or table is a finding for each bulk update/delete call
            // and each SaveChanges call in it (a variable holding the query is no way around it; 09 10.3 item 4: no code saves the row), and a raw UPDATE/DELETE of the table is a finding in any file.
            if (CompanySettingsName().IsMatch(clean))
            {
                Add(findings, CompanySettingsWrite, BulkWriteCall(), clean);
            }

            Add(findings, CompanySettingsWrite, CompanySettingsRawWrite(), clean);
        }

        if (path != OrderLocksFile)
        {
            Add(findings, ShareLockText, ShareLock(), clean);
        }

        // Each counter group has its one writer file: the stock and coupon counters, and the download counter (T340; D-94 (1)). A write of a group's counter anywhere else is
        // a finding, whichever file the other group's writer is.
        bool inStockWriter = path == StockCountersFile;
        bool inDownloadWriter = path == DownloadCountersFile;
        if (!inStockWriter)
        {
            Add(findings, CounterWrite, StockCounterColumnUpdate(), clean);
        }

        if (!inDownloadWriter)
        {
            Add(findings, CounterWrite, DownloadCounterColumnUpdate(), clean);
        }

        if (!inStockWriter || !inDownloadWriter)
        {
            foreach (Match call in SetPropertyCall().Matches(clean))
            {
                int open = call.Index + call.Length - 1;
                string first = FirstArgument(clean, open);
                IEnumerable<string> guarded = (inStockWriter ? Array.Empty<string>() : StockCounterProperties).Concat(inDownloadWriter ? Array.Empty<string>() : DownloadCounterProperties);
                if (guarded.Any(p => Regex.IsMatch(first, $@"\b{p}\b", RegexOptions.CultureInvariant, TimeSpan.FromSeconds(5))))
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
    private static partial Regex StockCounterColumnUpdate();

    [GeneratedRegex(@"\bUPDATE\s+[\w.""]+\s+SET\b[^;]*?\bdownload_count\s*=", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex DownloadCounterColumnUpdate();

    [GeneratedRegex(@"\bCompanySettings\b|\bcompany_settings\b", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex CompanySettingsName();

    [GeneratedRegex(@"\b(?:Execute(?:Update|Delete)|SaveChanges)(?:Async)?\s*\(", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex BulkWriteCall();

    [GeneratedRegex(@"\b(?:UPDATE|DELETE\s+FROM)\s+(?:ONLY\s+)?(?:[\\""]*\w+[\\""]*\.)?[\\""]*company_settings\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex CompanySettingsRawWrite();

    [GeneratedRegex(@"\bSetProperty\s*(?:<[^()]*?>)?\s*\(", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex SetPropertyCall();
}
