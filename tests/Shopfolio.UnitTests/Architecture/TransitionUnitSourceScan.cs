using System.Text;
using System.Text.RegularExpressions;

namespace Shopfolio.UnitTests.Architecture;

/// <summary>
/// Source scan of the transition unit's single place (T315 KK2; 09 9.1 items 4, 6, 8; 05 4.1.1, MK-35). The status fields of the order
/// (<c>OrderStatus</c>, <c>PaymentStatus</c>) and of the defect claim (<c>DefectClaimStatus</c>) are written by the unit only:
/// <list type="number">
/// <item>any mention of <c>ApplyTransition</c> or <c>ApplyCorrection</c> outside <c>OrderTransitions.cs</c> (a call, a method group, <c>nameof</c>,
/// a reflection string) is a finding, except the declaration itself (<c>void ApplyTransition(</c>); any mention of the defect claim's <c>ApplyStatus</c>
/// outside its one caller <c>DefectClaimStatusWriter.cs</c> is a finding (T320; D-274), and so is any mention of the module method that reaches that writer
/// (<c>ChangeDefectClaimStatusAsync</c>) outside the transition unit's item record file and the AfterSales surface that declares and implements it - the
/// claim's status is changed only from the transition unit (09 9.1 item 4), through the module that owns the claim (09 6 item 2);</item>
/// <item>a property named like a status field with a setter that is not <c>private</c> (<c>set</c>, <c>init</c>, <c>internal set</c>, <c>protected set</c>,
/// <c>private protected set</c>), or a field of that name that is not private; a query model's repeated text filter of that name (<c>string[]</c>) holds no status and is left out;</item>
/// <item>a bulk update that names a status field: the argument text of <c>ExecuteUpdate</c>, the property argument of <c>SetProperty</c>, the argument
/// text of the raw SQL calls (<c>ExecuteSql*</c>, <c>FromSql*</c>, <c>SqlQuery*</c>) naming <c>order_status</c>, <c>payment_status</c>, <c>defect_claim_status</c>
/// or the property names, and an <c>UPDATE ... SET</c> text of those columns anywhere (a statement held in a variable is no way around it); the
/// migrations are left out, a filter or rule text (<c>HasFilter</c>, <c>HasRule</c>) is not an update and stays free;</item>
/// <item>the property of the context's entry set by name (<c>Property("OrderStatus")</c> and the like);</item>
/// <item>in <c>OrderTransitions.cs</c>, any <c>HttpClient</c>, <c>IHttpClientFactory</c> or <c>System.Net.Http</c> text: the unit holds no outside
/// service (09 9.1 item 6).</item>
/// </list>
/// The places are full paths, not file names: a file of the same name elsewhere is scanned. The scan reads the whole text, so a statement split over
/// lines is found; full-line comments are blanked first and keep their line; string contents are read, because a raw statement is a string.
/// </summary>
internal static partial class TransitionUnitSourceScan
{
    public const string OutsideCall = "calls or names the unit's status writer outside OrderTransitions.cs (09 9.1 item 4)";

    public const string OpenSetter = "has a status field whose setter is not private (09 9.1 item 4)";

    public const string BulkStatusWrite = "writes a status column by a bulk update or a raw statement (09 9.1 item 8)";

    public const string PropertyByName = "sets a status property of the entry by its name (09 9.1 item 4)";

    public const string UnitHttp = "binds the transition unit to HttpClient or an outside HTTP type (09 9.1 item 6)";

    public const string UnitFile = "src/Shopfolio.Web/Modules/Ordering/OrderTransitions.cs";

    /// <summary>The one place that calls the defect claim's status writer (T320; D-274): the claim's writer in the AfterSales module, by its full path.</summary>
    public const string DefectClaimWriterFile = "src/Shopfolio.Web/Modules/AfterSales/DefectClaimStatusWriter.cs";

    /// <summary>
    /// The files that may name the module method reaching the claim's writer, by full path: the transition unit's item record operations (the only caller), the
    /// AfterSales surface that declares it and the module that implements it.
    /// </summary>
    public static readonly IReadOnlyList<string> DefectClaimChangeFiles =
    [
        "src/Shopfolio.Web/Modules/Ordering/OrderTransitions.ItemRecords.cs",
        "src/Shopfolio.Web/Modules/AfterSales/Contracts/IAfterSalesModule.cs",
        "src/Shopfolio.Web/Modules/AfterSales/AfterSalesModule.ItemRecords.cs",
    ];

    public static readonly IReadOnlyList<string> StatusProperties = ["OrderStatus", "PaymentStatus", "DefectClaimStatus"];

    public static readonly IReadOnlyList<string> StatusColumns = ["order_status", "payment_status", "defect_claim_status"];

    /// <summary>The scan applies to every application source file except the migrations (the filter text of a model snapshot names the columns).</summary>
    public static bool Applies(string relativePath)
    {
        string path = relativePath.Replace('\\', '/');
        return path.EndsWith(".cs", StringComparison.Ordinal) && !path.Contains("/Migrations/", StringComparison.Ordinal);
    }

    public static IReadOnlyList<(string Rule, int Line)> Scan(string relativePath, string text)
    {
        string path = relativePath.Replace('\\', '/');
        string clean = BlankCommentLines(text.ReplaceLineEndings("\n"));
        List<(string Rule, int Line)> findings = [];

        if (path != UnitFile)
        {
            Add(findings, OutsideCall, WriterMention(), clean);
        }

        if (path != DefectClaimWriterFile)
        {
            Add(findings, OutsideCall, StatusApplierMention(), clean);
        }

        if (!DefectClaimChangeFiles.Contains(path))
        {
            Add(findings, OutsideCall, DefectClaimChangeMention(), clean);
        }
        Add(findings, OpenSetter, OpenStatusSetter(), clean);
        Add(findings, OpenSetter, OpenStatusField(), clean);
        Add(findings, PropertyByName, PropertyByNameCall(), clean);
        Add(findings, BulkStatusWrite, StatusUpdateStatement(), clean);

        foreach (Match call in BulkCall().Matches(clean))
        {
            int open = call.Index + call.Length - 1;
            string argument = call.Value.StartsWith("SetProperty", StringComparison.Ordinal) ? FirstArgument(clean, open) : ArgumentText(clean, open);
            if (NamesAStatus(argument))
            {
                AddAt(findings, BulkStatusWrite, clean, call.Index);
            }
        }

        if (path == UnitFile)
        {
            Add(findings, UnitHttp, HttpText(), clean);
        }

        findings.Sort((a, b) => a.Line.CompareTo(b.Line));
        return findings;
    }

    private static bool NamesAStatus(string argument)
    {
        return StatusProperties.Any(p => Regex.IsMatch(argument, $@"\b{p}\b", RegexOptions.CultureInvariant, TimeSpan.FromSeconds(5)))
            || StatusColumns.Any(c => argument.Contains(c, StringComparison.OrdinalIgnoreCase));
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

    /// <summary>The text between the parenthesis at <paramref name="open"/> and its match; parentheses inside string literals are not counted.</summary>
    private static string ArgumentText(string text, int open)
    {
        int depth = 0;
        for (int i = open; i < text.Length; i++)
        {
            i = SkipString(text, i);
            char c = text[i];
            if (c == '(')
            {
                depth++;
            }
            else if (c == ')' && --depth == 0)
            {
                return text[(open + 1)..i];
            }
        }

        return text[(open + 1)..];
    }

    /// <summary>The first argument of the call whose parenthesis is at <paramref name="open"/> (the property a <c>SetProperty</c> writes).</summary>
    private static string FirstArgument(string text, int open)
    {
        int depth = 0;
        for (int i = open; i < text.Length; i++)
        {
            i = SkipString(text, i);
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

    /// <summary>When a string literal starts at <paramref name="i"/>, the index of its last character; otherwise <paramref name="i"/> (regular, verbatim, interpolated and raw strings).</summary>
    private static int SkipString(string text, int i)
    {
        int start = i;
        bool verbatim = false;
        while (i < text.Length && text[i] is '$' or '@')
        {
            verbatim |= text[i] == '@';
            i++;
        }

        if (i >= text.Length || text[i] != '"')
        {
            return start;
        }

        int quotes = 0;
        while (i + quotes < text.Length && text[i + quotes] == '"')
        {
            quotes++;
        }

        if (quotes >= 3)
        {
            string closing = new('"', quotes);
            int end = text.IndexOf(closing, i + quotes, StringComparison.Ordinal);
            return end < 0 ? text.Length - 1 : end + quotes - 1;
        }

        for (int j = i + 1; j < text.Length; j++)
        {
            if (verbatim && text[j] == '"' && j + 1 < text.Length && text[j + 1] == '"')
            {
                j++;
            }
            else if (!verbatim && text[j] == '\\')
            {
                j++;
            }
            else if (text[j] == '"')
            {
                return j;
            }
        }

        return text.Length - 1;
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

    // The mention of the writers, except the declaration (`void ApplyTransition(`).
    [GeneratedRegex(@"(?<!\bvoid\s+)\b(?:ApplyTransition|ApplyCorrection)\b", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex WriterMention();

    // The defect claim's status writer: any mention but the declaration is a finding outside DefectClaimStatusWriter.cs.
    [GeneratedRegex(@"(?<!\bvoid\s+)\bApplyStatus\b", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex StatusApplierMention();

    // The module method that reaches the claim's status writer: any mention (a call, a method group, nameof, a string) outside its three files.
    [GeneratedRegex(@"\bChangeDefectClaimStatusAsync\b", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex DefectClaimChangeMention();

    // A property named like a status field, with an accessor `set`/`init` whose modifiers are not exactly `private`. A query model's repeated text filter
    // (`string[]`; 07 2.6.4, 5.3.2 `orderStatus`, `paymentStatus`) holds no status and is no finding: only the array of texts is left out, every other type of that name is still read.
    [GeneratedRegex(@"(?<!\bstring\[\]\??\s+)\b(?:OrderStatus|PaymentStatus|DefectClaimStatus)\s*\{[^{}]*?(?<!\bprivate\s)\b(?:set|init)\b\s*(?:;|\{|=>)", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex OpenStatusSetter();

    // `private protected set` is open to a derived class: the lookbehind above reads `protected ` before `set`, not `private `, so it is a finding too.
    // A field of that name that is not private (public, internal, protected, or a record parameter of the entity is not a field).
    [GeneratedRegex(@"\b(?:public|internal|protected)\s+(?:static\s+)?(?:volatile\s+)?[\w<>?.]+\s+(?:OrderStatus|PaymentStatus|DefectClaimStatus)\s*(?:;|=(?!>))", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex OpenStatusField();

    [GeneratedRegex(@"\b(?:ExecuteUpdate|ExecuteUpdateAsync|SetProperty|ExecuteSql\w*|FromSql\w*|SqlQuery\w*)(?<generic>\s*<[^()]*?>)?\s*\(", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex BulkCall();

    [GeneratedRegex(@"\.(?:Property|Member)\s*(?:<[^()]*?>)?\s*\(\s*(?:""|nameof\s*\([^)]*?)(?:OrderStatus|PaymentStatus|DefectClaimStatus)\b", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex PropertyByNameCall();

    [GeneratedRegex(@"\bUPDATE\s+(?:ONLY\s+)?[\w.""\\]+\s+SET\b[^;]*?\b(?:order_status|payment_status|defect_claim_status)\b\s*=", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex StatusUpdateStatement();

    [GeneratedRegex(@"\bHttpClient\b|\bIHttpClientFactory\b|\bHttpMessageHandler\b|\bSystem\.Net\.Http\b", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex HttpText();
}
