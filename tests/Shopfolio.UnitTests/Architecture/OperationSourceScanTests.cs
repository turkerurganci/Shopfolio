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

    public static TheoryData<string> ScanHandlers_OfAMarkedHandlerWithoutTheGate_IsAFinding_InEverySpellingOfTheMarkRows =>
    [
        "[GatedOperation] public IActionResult OnPostA() { return Page(); }",
        "[GatedOperationAttribute] public IActionResult OnPostA() { return Page(); }",
        "[Shopfolio.Infrastructure.Operations.GatedOperation] public IActionResult OnPostA() { return Page(); }",
        "[HttpPost, GatedOperation] public IActionResult OnPostA() { return Page(); }",
        "[GatedOperation, HttpPost] public IActionResult OnPostA() { return Page(); }",
        "[GatedOperation()] [HttpPost] public IActionResult OnPostA() { return Page(); }",
        "[GatedOperation]\n    public async Task<IActionResult> OnPostA(Guid id,\n        Input input) { return Page(); }",
        "[GatedOperation] public IActionResult OnPostA() => Page();",
    ];

    public static TheoryData<string> ScanHandlers_OfAMarkedHandlerThatReachesTheGate_DirectlyOrThroughMethodsOfTheFile_IsCleanRows =>
    [
        "[GatedOperation] public IActionResult OnPostA() { OperationGate.Decide(r, c, 1); return Page(); }",
        "[GatedOperation] public IActionResult OnPostA() { OperationGate . Decide(r, c, 1); return Page(); }",
        "[GatedOperation] public IActionResult OnPostA() => Infrastructure.Operations.OperationGate.Decide(r, c, 1).IsAllowed ? Page() : Page();",
        "[GatedOperation] public async Task<IActionResult> OnPostA() { var o = Services.GetRequiredService<OperationRunner>(); await o.RunAsync(); return Page(); }",
        "[GatedOperation] public IActionResult OnPostA() { return Go(); } private IActionResult Go() { return Next(); } private IActionResult Next() { OperationGate.Decide(r, c, 1); return Page(); }",
    ];

    public static TheoryData<string> ScanHandlers_OfACheckThatIsOnlyInACommentAStringOrTooFarAway_IsAFindingRows =>
    [
        "[GatedOperation] public IActionResult OnPostA() { return Go(); } private IActionResult Go() { // OperationGate.Decide\n return Page(); }",
        "[GatedOperation] public IActionResult OnPostA() { /* OperationRunner */ return Page(); }",
        "[GatedOperation] public IActionResult OnPostA() { string s = \"OperationGate.Decide\"; return Page(); }",
        "[GatedOperation] public IActionResult OnPostA() { string s = @\"OperationRunner\"; return Page(); }",
        "[GatedOperation] public IActionResult OnPostA() { var o = Services.GetRequiredService<OperationRunner>(); return Page(); }",
        "[GatedOperation] public async Task<IActionResult> OnPostA() { var o = Services.GetRequiredService<OperationRunner>(); await Other.RunAsyncLater(); return Page(); }",
        "[GatedOperation] public IActionResult OnPostA() { return Go(); } private IActionResult Go() { return A(); } private IActionResult A() { return B(); } private IActionResult B() { return C(); } private IActionResult C() { return D(); } private IActionResult D() { OperationGate.Decide(r, c, 1); return Page(); }",
    ];

    public static TheoryData<string> ScanClient_OfAViewThatDecides_IsAFindingRows =>
    [
        "<p>@(Model.Order.OrderStatus)</p>",
        "@if (x == PaymentStatus.Paid) { }",
        "@{ var d = TurkeyTime.DateOf(Model.At); }",
        "<p>@Model.At.AddDays(3)</p>",
        "<p>@TimeProvider.System.GetUtcNow()</p>",
        "@{ var t = DateTimeOffset.UtcNow; }",
    ];

    public static TheoryData<string> ScanClient_OfAViewThatOnlyShowsTheServersValues_IsCleanRows =>
    [
        "<p>@Model.Cancel.MaxQuantity</p>",
        "<p>@DisplayFormat.Date(lastDay)</p>",
        "@* OrderStatus *@",
        "<!-- AddDays -->",
        "<p>@Model.OrderStatusText</p>",
    ];

    public static TheoryData<string> ScanClient_OfAScriptThatDecides_IsAFindingRows =>
    [
        "if (row.status === 'Delivered') {}",
        "const s = \"Shipped\";",
        "const p = PartiallyRefunded;",
        "const d = new Date();",
        "const t = performance.now(); const x = d.getTime();",
        "const y = d.getFullYear();",
    ];

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

        Assert.Equal(3, System.Text.RegularExpressions.Regex.Count(handlers, @"\[GatedOperation\]"));
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
                (OperationSourceScan.GatedWithoutCheck, 45),
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
    [MemberData(nameof(ScanHandlers_OfAMarkedHandlerWithoutTheGate_IsAFinding_InEverySpellingOfTheMarkRows))]
    public void ScanHandlers_OfAMarkedHandlerWithoutTheGate_IsAFinding_InEverySpellingOfTheMark(string source)
    {
        Assert.Equal([(OperationSourceScan.GatedWithoutCheck, 1)], OperationSourceScan.ScanHandlers(source));
    }

    [Theory]
    [MemberData(nameof(ScanHandlers_OfAMarkedHandlerThatReachesTheGate_DirectlyOrThroughMethodsOfTheFile_IsCleanRows))]
    public void ScanHandlers_OfAMarkedHandlerThatReachesTheGate_DirectlyOrThroughMethodsOfTheFile_IsClean(string source)
    {
        Assert.Empty(OperationSourceScan.ScanHandlers(source));
    }

    [Theory]
    [MemberData(nameof(ScanHandlers_OfACheckThatIsOnlyInACommentAStringOrTooFarAway_IsAFindingRows))]
    public void ScanHandlers_OfACheckThatIsOnlyInACommentAStringOrTooFarAway_IsAFinding(string source)
    {
        Assert.Equal([(OperationSourceScan.GatedWithoutCheck, 1)], OperationSourceScan.ScanHandlers(source));
    }

    [Theory]
    [MemberData(nameof(ScanClient_OfAViewThatDecides_IsAFindingRows))]
    public void ScanClient_OfAViewThatDecides_IsAFinding(string line)
    {
        Assert.Equal([(OperationSourceScan.ViewDecides, 1)], OperationSourceScan.ScanClient("src/Shopfolio.Web/Pages/A.cshtml", line, _statusTypes, _statusMembers));
    }

    [Theory]
    [MemberData(nameof(ScanClient_OfAViewThatOnlyShowsTheServersValues_IsCleanRows))]
    public void ScanClient_OfAViewThatOnlyShowsTheServersValues_IsClean(string line)
    {
        Assert.Empty(OperationSourceScan.ScanClient("src/Shopfolio.Web/Pages/A.cshtml", line, _statusTypes, _statusMembers));
    }

    [Theory]
    [MemberData(nameof(ScanClient_OfAScriptThatDecides_IsAFindingRows))]
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
