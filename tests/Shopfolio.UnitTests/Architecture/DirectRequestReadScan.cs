using System.Text.RegularExpressions;

namespace Shopfolio.UnitTests.Architecture;

/// <summary>
/// The scan of the direct request reads of the page models (D-1614; 07 5.3.1, 09 3.4): a value reaches a page through the handler's parameters and the bound input,
/// where the endpoint binding check (<c>EndpointBindingCheck</c>) sees it; a page model that reads <c>Request.Form</c> or <c>Request.Query</c> itself takes a value
/// past that check. Every such read (also <c>HttpContext.Request.Form</c>, the indexer, <c>ContainsKey</c>, <c>TryGetValue</c>, <c>ReadFormAsync</c>) in a page file
/// (<c>src/Shopfolio.Web/Pages</c>, <c>.cs</c>) is a finding unless it is on <see cref="Allowed"/>: the reads named by file, each with the reason - a common parameter of
/// 07 5.3.1, or the page model's own field read again to draw the form back. The list is held both ways (a read not on it is a finding; an entry that matches no read is a finding).
/// </summary>
internal static partial class DirectRequestReadScan
{
    public const string UnlistedRead = "ADirectRequestReadThatIsNotOnTheAllowedList";

    public const string StaleEntry = "AnAllowedReadThatNoPageReadsAnyMore";

    public const string PagesFolder = "src/Shopfolio.Web/Pages/";

    /// <summary>The allowed reads: the page file, the normalised read, the number of places, the reason.</summary>
    public static readonly IReadOnlyList<(string File, string Read, int Count, string Reason)> Allowed =
    [
        // The common parameters of 07 5.3.1.
        ("src/Shopfolio.Web/Pages/Panel/OrderDetail.cshtml.cs", "Query[ReturnPath.ParameterName]", 1, "returnUrl of the order screen (UC-88): ListReturn.Target keeps the local, listed addresses only"),
        ("src/Shopfolio.Web/Pages/Panel/RequestDetail.cshtml.cs", "Query[ReturnPath.ParameterName]", 1, "returnUrl of the request screen (UC-92): ListReturn.Target keeps the local, listed addresses only"),
        ("src/Shopfolio.Web/Pages/Product.cshtml.cs", "Query.ContainsKey", 3, "presence of the variant and notice parameters: a page that carries one closes to search engines; the value is never read"),
        ("src/Shopfolio.Web/Pages/Orders/AfterSalesScreenModels.cs", "Query.ContainsKey", 1, "presence of the defect claim parameter: the value is bound by the handler"),
        ("src/Shopfolio.Web/Pages/Orders/AfterSalesScreenModels.cs", "Query[\"handler\"]", 1, "the handler name of the reopen form: the page tells its own POST handler apart before the binding"),

        // The page model's own field, read again to draw the form back after a refusal.
        ("src/Shopfolio.Web/Pages/Orders/AfterSalesScreenModels.cs", "Form[nameof(ReopenDefectClaimInput.OrderItemId)]", 1, "the reopen form's own item, read to find the claim the refusal is drawn on"),
        ("src/Shopfolio.Web/Pages/Panel/Coupons.cshtml.cs", "Form[\"Input.Version\"]", 1, "the sent version drawn back in the coupon form"),
        ("src/Shopfolio.Web/Pages/Panel/Coupons.cshtml.cs", "Form[\"Input.\" + name]", 1, "the sent value of a coupon field drawn back"),
        ("src/Shopfolio.Web/Pages/Storefront/Cart.cshtml.cs", "Form[nameof(UpdateCartItemInput.Quantity)]", 2, "the sent quantity drawn back on a refusal"),
        ("src/Shopfolio.Web/Pages/Storefront/Checkout.cshtml.cs", "Form.ContainsKey", 1, "which of the two checkout forms was posted"),
        ("src/Shopfolio.Web/Pages/Storefront/Register.cshtml.cs", "Form[PendingParameter]", 1, "the pending registration's key drawn back"),
    ];

    public static bool AppliesTo(string relativePath)
    {
        string path = Normalize(relativePath);
        return path.StartsWith(PagesFolder, StringComparison.Ordinal) && path.EndsWith(".cs", StringComparison.Ordinal);
    }

    /// <summary>The reads of one file, normalised, with their lines; comments are blanked.</summary>
    public static IReadOnlyList<(string Read, int Line)> Reads(string text)
    {
        string clean = BlankComments(text.ReplaceLineEndings("\n"));
        List<(string Read, int Line)> reads = [];
        foreach (Match match in Read().Matches(clean))
        {
            string tail = match.Groups["key"].Success
                ? "[" + Regex.Replace(match.Groups["key"].Value, @"\s+", " ").Trim() + "]"
                : match.Groups["member"].Success ? "." + match.Groups["member"].Value : string.Empty;
            reads.Add((match.Groups["source"].Value + tail, clean.AsSpan(0, match.Index).Count('\n') + 1));
        }

        return reads;
    }

    /// <summary>The findings of one page file: every read that the allowed list does not cover (the list covers as many reads of a form as its count says).</summary>
    public static IReadOnlyList<(string Rule, int Line, string Read)> Scan(string relativePath, string text)
    {
        string path = Normalize(relativePath);
        List<(string Rule, int Line, string Read)> findings = [];
        foreach (IGrouping<string, (string Read, int Line)> group in Reads(text).GroupBy(r => r.Read, StringComparer.Ordinal))
        {
            int allowed = Allowed.Where(a => a.File == path && a.Read == group.Key).Sum(a => a.Count);
            foreach ((string read, int line) in group.Skip(allowed))
            {
                findings.Add((UnlistedRead, line, read));
            }
        }

        return [.. findings.OrderBy(f => f.Line)];
    }

    private static string Normalize(string path) => path.Replace('\\', '/');

    private static string BlankComments(string text) => LineComment().Replace(BlockComment().Replace(text, match => Regex.Replace(match.Value, @"[^\n]", " ")), string.Empty);

    // Request.Form["x"], HttpContext.Request.Query[Name], Request.Form.ContainsKey(...), Request.Form.TryGetValue(...), Request.ReadFormAsync(...).
    [GeneratedRegex(@"\b(?:HttpContext\s*\.\s*)?Request\s*\.\s*(?<source>Form|Query)\b(?:\s*\[\s*(?<key>[^\]]+?)\s*\]|\s*\.\s*(?<member>\w+))|\b(?:HttpContext\s*\.\s*)?Request\s*\.\s*(?<source>ReadFormAsync)\b(?:\s*\.\s*(?<member>\w+))?", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex Read();

    [GeneratedRegex(@"/\*.*?\*/", RegexOptions.CultureInvariant | RegexOptions.Singleline, matchTimeoutMilliseconds: 5000)]
    private static partial Regex BlockComment();

    [GeneratedRegex(@"//[^\n]*", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex LineComment();
}
