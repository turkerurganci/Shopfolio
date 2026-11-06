using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.UnitTests.Support;

namespace Shopfolio.UnitTests.Architecture;

/// <summary>
/// Runs the operation source scan over the application source and shows on deliberate violation samples that each rule goes red (T217 criteria 2, 3, 4, 8, KK2, KK4, KK8;
/// 07 8.1; 09 8.3, 19.2 item 3). The scan reads files and never writes one.
/// </summary>
public sealed class OperationSourceScanTests
{
    // The closed lists come from the enums, not from a copy of their members (09 19.2 item 2).
    private static readonly string[] _statusTypes = [nameof(OrderStatus), nameof(PaymentStatus)];

    private static readonly string[] _statusMembers = [.. Enum.GetNames<OrderStatus>(), .. Enum.GetNames<PaymentStatus>()];

    [Fact]
    public void ApplicationSource_EveryGatedHandlerReachesTheGate()
    {
        string root = RepositoryRoot.Find();
        List<string> scanned = [];
        List<string> findings = [];
        foreach (string file in SourceFiles(root, "src", "*.cs"))
        {
            scanned.Add(file);
            foreach ((string rule, int line) in OperationSourceScan.ScanHandlers(File.ReadAllText(Path.Combine(root, file))))
            {
                findings.Add($"{file}:{line} {rule}");
            }
        }

        Assert.Contains(scanned, f => f.EndsWith("Infrastructure/Operations/OperationGate.cs", StringComparison.Ordinal));
        Assert.Empty(findings);
    }

    [Fact]
    public void ApplicationSource_ViewsAndScriptsDecideNoOperation()
    {
        string root = RepositoryRoot.Find();
        List<string> scanned = [];
        List<string> findings = [];
        foreach (string pattern in new[] { "*.cshtml", "*.js" })
        {
            foreach (string file in SourceFiles(root, "src", pattern))
            {
                scanned.Add(file);
                foreach ((string rule, int line) in OperationSourceScan.ScanClient(file, File.ReadAllText(Path.Combine(root, file)), _statusTypes, _statusMembers))
                {
                    findings.Add($"{file}:{line} {rule}");
                }
            }
        }

        Assert.Contains(scanned, f => f.EndsWith("wwwroot/js/common.js", StringComparison.Ordinal));
        Assert.Empty(findings);
    }

    [Fact]
    public void TheSamplePage_MarksItsPostHandlers_ReachesTheGate_AndItsViewDecidesNothing()
    {
        string root = RepositoryRoot.Find();
        string folder = Path.Combine("tests", "Shopfolio.IntegrationTests", "Pages", "Panel");
        string handlers = File.ReadAllText(Path.Combine(root, folder, "OperationSample.cshtml.cs"));
        string view = File.ReadAllText(Path.Combine(root, folder, "OperationSample.cshtml"));

        Assert.Equal(3, System.Text.RegularExpressions.Regex.Matches(handlers, @"\[GatedOperation\]").Count);
        Assert.Empty(OperationSourceScan.ScanHandlers(handlers));
        Assert.Empty(OperationSourceScan.ScanClient("OperationSample.cshtml", view, _statusTypes, _statusMembers));
    }

    [Fact]
    public void ScanHandlers_OnTheCodeSample_ReportsTheHandlerThatNeverReachesTheGateAndTheMarkOnAGet_AndNotTheCommentTheStringOrTheFineHandlers()
    {
        IReadOnlyList<(string Rule, int Line)> findings = OperationSourceScan.ScanHandlers(ReadSample("GatedHandlers.cs"));

        Assert.Equal(
            [
                (OperationSourceScan.GatedWithoutCheck, 5),
                (OperationSourceScan.GatedWithoutCheck, 14),
                (OperationSourceScan.GatedNotAPostHandler, 22),
            ],
            findings);
    }

    [Fact]
    public void ScanClient_OnTheViewSample_ReportsTheStatusAndTheDateArithmetic_AndNotTheCommentOrTheServersValues()
    {
        IReadOnlyList<(string Rule, int Line)> findings = OperationSourceScan.ScanClient("src/Shopfolio.Web/Pages/Misuse.cshtml", ReadSample("Decides.cshtml"), _statusTypes, _statusMembers);

        Assert.Equal(
            [
                (OperationSourceScan.ViewDecides, 3),
                (OperationSourceScan.ViewDecides, 7),
                (OperationSourceScan.ViewDecides, 9),
                (OperationSourceScan.ViewDecides, 10),
            ],
            findings);
    }

    [Fact]
    public void ScanClient_OnTheScriptSample_ReportsTheStatusAndTheClock_AndNotTheCommentsOrTheSum()
    {
        IReadOnlyList<(string Rule, int Line)> findings = OperationSourceScan.ScanClient(@"src\Shopfolio.Web\wwwroot\js\misuse.js", ReadSample("decides.js"), _statusTypes, _statusMembers);

        Assert.Equal(
            [
                (OperationSourceScan.ScriptDecides, 2),
                (OperationSourceScan.ScriptDecides, 3),
                (OperationSourceScan.ScriptDecides, 5),
            ],
            findings);
    }

    [Theory]
    [InlineData("[GatedOperation] public IActionResult OnPostA() { return Page(); }")]
    [InlineData("[GatedOperationAttribute] public IActionResult OnPostA() { return Page(); }")]
    [InlineData("[Shopfolio.Infrastructure.Operations.GatedOperation] public IActionResult OnPostA() { return Page(); }")]
    [InlineData("[HttpPost, GatedOperation] public IActionResult OnPostA() { return Page(); }")]
    [InlineData("[GatedOperation, HttpPost] public IActionResult OnPostA() { return Page(); }")]
    [InlineData("[GatedOperation()] [HttpPost] public IActionResult OnPostA() { return Page(); }")]
    [InlineData("[GatedOperation]\n    public async Task<IActionResult> OnPostA(Guid id,\n        Input input) { return Page(); }")]
    [InlineData("[GatedOperation] public IActionResult OnPostA() => Page();")]
    public void ScanHandlers_OfAMarkedHandlerWithoutTheGate_IsAFinding_InEverySpellingOfTheMark(string source)
    {
        Assert.Equal([(OperationSourceScan.GatedWithoutCheck, 1)], OperationSourceScan.ScanHandlers(source));
    }

    [Theory]
    [InlineData("[GatedOperation] public IActionResult OnPostA() { OperationGate.Decide(r, c, 1); return Page(); }")]
    [InlineData("[GatedOperation] public IActionResult OnPostA() { OperationGate . Decide(r, c, 1); return Page(); }")]
    [InlineData("[GatedOperation] public IActionResult OnPostA() => Infrastructure.Operations.OperationGate.Decide(r, c, 1).IsAllowed ? Page() : Page();")]
    [InlineData("[GatedOperation] public async Task<IActionResult> OnPostA() { var o = Services.GetRequiredService<GatedOrderOperation>(); await o.RunAsync(); return Page(); }")]
    [InlineData("[GatedOperation] public IActionResult OnPostA() { return Go(); } private IActionResult Go() { return Next(); } private IActionResult Next() { OperationGate.Decide(r, c, 1); return Page(); }")]
    public void ScanHandlers_OfAMarkedHandlerThatReachesTheGate_DirectlyOrThroughMethodsOfTheFile_IsClean(string source)
    {
        Assert.Empty(OperationSourceScan.ScanHandlers(source));
    }

    [Theory]
    [InlineData("[GatedOperation] public IActionResult OnPostA() { return Go(); } private IActionResult Go() { // OperationGate.Decide\n return Page(); }")]
    [InlineData("[GatedOperation] public IActionResult OnPostA() { /* GatedOrderOperation */ return Page(); }")]
    [InlineData("[GatedOperation] public IActionResult OnPostA() { string s = \"OperationGate.Decide\"; return Page(); }")]
    [InlineData("[GatedOperation] public IActionResult OnPostA() { string s = @\"GatedOrderOperation\"; return Page(); }")]
    [InlineData("[GatedOperation] public IActionResult OnPostA() { return Go(); } private IActionResult Go() { return A(); } private IActionResult A() { return B(); } private IActionResult B() { return C(); } private IActionResult C() { return D(); } private IActionResult D() { OperationGate.Decide(r, c, 1); return Page(); }")]
    public void ScanHandlers_OfACheckThatIsOnlyInACommentAStringOrTooFarAway_IsAFinding(string source)
    {
        Assert.Equal([(OperationSourceScan.GatedWithoutCheck, 1)], OperationSourceScan.ScanHandlers(source));
    }

    [Theory]
    [InlineData("<p>@(Model.Order.OrderStatus)</p>")]
    [InlineData("@if (x == PaymentStatus.Paid) { }")]
    [InlineData("@{ var d = TurkeyTime.DateOf(Model.At); }")]
    [InlineData("<p>@Model.At.AddDays(3)</p>")]
    [InlineData("<p>@TimeProvider.System.GetUtcNow()</p>")]
    [InlineData("@{ var t = DateTimeOffset.UtcNow; }")]
    public void ScanClient_OfAViewThatDecides_IsAFinding(string line)
    {
        Assert.Equal([(OperationSourceScan.ViewDecides, 1)], OperationSourceScan.ScanClient("src/Shopfolio.Web/Pages/A.cshtml", line, _statusTypes, _statusMembers));
    }

    [Theory]
    [InlineData("<p>@Model.Cancel.MaxQuantity</p>")]
    [InlineData("<p>@DisplayFormat.Date(lastDay)</p>")]
    [InlineData("@* OrderStatus *@")]
    [InlineData("<!-- AddDays -->")]
    [InlineData("<p>@Model.OrderStatusText</p>")]
    public void ScanClient_OfAViewThatOnlyShowsTheServersValues_IsClean(string line)
    {
        Assert.Empty(OperationSourceScan.ScanClient("src/Shopfolio.Web/Pages/A.cshtml", line, _statusTypes, _statusMembers));
    }

    [Theory]
    [InlineData("if (row.status === 'Delivered') {}")]
    [InlineData("const s = \"Shipped\";")]
    [InlineData("const p = PartiallyRefunded;")]
    [InlineData("const d = new Date();")]
    [InlineData("const t = performance.now(); const x = d.getTime();")]
    [InlineData("const y = d.getFullYear();")]
    public void ScanClient_OfAScriptThatDecides_IsAFinding(string line)
    {
        Assert.Equal([(OperationSourceScan.ScriptDecides, 1)], OperationSourceScan.ScanClient("src/Shopfolio.Web/wwwroot/js/a.js", line, _statusTypes, _statusMembers));
    }

    [Fact]
    public void ScanClient_OfOtherFileTypes_IsEmpty()
    {
        Assert.Empty(OperationSourceScan.ScanClient("src/Shopfolio.Web/wwwroot/css/base.css", "Shipped Date", _statusTypes, _statusMembers));
    }

    private static IEnumerable<string> SourceFiles(string root, string folder, string pattern)
    {
        foreach (string file in Directory.EnumerateFiles(Path.Combine(root, folder), pattern, SearchOption.AllDirectories))
        {
            string path = Path.GetRelativePath(root, file).Replace('\\', '/');
            if (!path.Contains("/obj/", StringComparison.Ordinal) && !path.Contains("/bin/", StringComparison.Ordinal))
            {
                yield return path;
            }
        }
    }

    private static string ReadSample(string name) =>
        File.ReadAllText(Path.Combine(RepositoryRoot.Find(), "tests", "Shopfolio.UnitTests", "Architecture", "ViolationSamples", "Operations", name));
}
