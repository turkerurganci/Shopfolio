using Shopfolio.UnitTests.Support;

namespace Shopfolio.UnitTests.Architecture;

/// <summary>
/// Runs the page source scan over the application source and shows on deliberate violation samples that each rule goes red (T206; 07 2.2.1, 2.10;
/// 09 6 item 2; 09 19.2 item 3; D-73). The scan reads files and never writes one.
/// </summary>
public sealed class PageSourceScanTests
{
    private static readonly string[] _scannedExtensions = ["*.cs", "*.cshtml"];

    [Fact]
    public void ApplicationSource_HasNoOutwardApiNoVersion_AndItsViewsTouchNeitherTheContextNorModuleInternalsAndNameEveryPostHandler()
    {
        string root = RepositoryRoot.Find();
        List<string> scanned = [];
        List<string> findings = [];
        foreach (string pattern in _scannedExtensions)
        {
            foreach (string file in Directory.EnumerateFiles(Path.Combine(root, "src"), pattern, SearchOption.AllDirectories))
            {
                string relative = Path.GetRelativePath(root, file);
                string path = relative.Replace('\\', '/');
                if (path.Contains("/obj/", StringComparison.Ordinal) || path.Contains("/bin/", StringComparison.Ordinal))
                {
                    continue;
                }

                scanned.Add(path);
                foreach ((string rule, int line) in PageSourceScan.Scan(relative, File.ReadAllText(file)))
                {
                    findings.Add($"{path}:{line} {rule}");
                }
            }
        }

        Assert.Contains(scanned, f => f.EndsWith("Program.cs", StringComparison.Ordinal));
        Assert.Contains(scanned, f => f.EndsWith("Routing/AddressRules.cs", StringComparison.Ordinal));
        Assert.Empty(findings);
    }

    [Fact]
    public void Scan_OnTheApiSample_ReportsEachFormOfTheApiAndOfTheVersionAndNotTheCommentOrTheFineAddress()
    {
        string sample = ReadSample("ApiMisuse.cs");

        IReadOnlyList<(string Rule, int Line)> findings = PageSourceScan.Scan("src/Shopfolio.Web/Endpoints/Misuse.cs", sample);

        Assert.Equal(
            [
                (PageSourceScan.OutwardApi, 3),
                (PageSourceScan.OutwardApi, 4),
                (PageSourceScan.OutwardApi, 8),
                (PageSourceScan.OutwardApi, 9),
                (PageSourceScan.OutwardApi, 10),
                (PageSourceScan.OutwardApi, 11),
                (PageSourceScan.AddressVersion, 12),
                (PageSourceScan.AddressVersion, 13),
                (PageSourceScan.OutwardApi, 14),
                (PageSourceScan.AddressVersion, 17),
                (PageSourceScan.AddressVersion, 18),
                (PageSourceScan.AddressVersion, 19),
            ],
            findings);
    }

    [Fact]
    public void Scan_OnTheViewSample_ReportsTheContextTheModuleInternalsAndEveryPostFormWithoutAHandler()
    {
        string sample = ReadSample("ViewMisuse.cshtml");

        IReadOnlyList<(string Rule, int Line)> findings = PageSourceScan.Scan(@"src\Shopfolio.Web\Pages\Misuse.cshtml", sample);

        Assert.Equal(
            [
                (PageSourceScan.ViewTouchesModuleInternals, 6),
                (PageSourceScan.ViewTouchesContext, 7),
                (PageSourceScan.ViewTouchesContext, 8),
                (PageSourceScan.PostFormWithoutHandler, 12),
                (PageSourceScan.PostFormWithoutHandler, 13),
                (PageSourceScan.PostFormWithoutHandler, 19),
            ],
            findings);
    }

    [Theory]
    [InlineData("<form method=\"post\">")]
    [InlineData("<form method=post>")]
    [InlineData("<form method = 'POST'>")]
    [InlineData("<form\n  method=\"Post\"\n  class=\"a\">")]
    [InlineData("<form method=\"post\" asp-page-handler=\"\">")]
    [InlineData("<form method=\"post\" action=\"/panel/x\" asp-route-other=\"Add\">")]
    public void Scan_OfAPostFormInAnyWriting_IsAFindingWhenTheHandlerIsNotNamed(string tag)
    {
        Assert.Contains((PageSourceScan.PostFormWithoutHandler, 1), PageSourceScan.Scan("src/Shopfolio.Web/Pages/A.cshtml", tag + "</form>"));
    }

    [Theory]
    [InlineData("<form method=\"post\" asp-page-handler=\"Add\">")]
    [InlineData("<form method=\"post\" action=\"/panel/x?handler=Add\">")]
    [InlineData("<form\n  method=\"post\"\n  asp-page-handler='Add'>")]
    [InlineData("<form method=\"post\" asp-route-handler=\"Add\">")]
    [InlineData("<form method=\"get\">")]
    [InlineData("<form>")]
    [InlineData("<formal method=\"post\">")]
    public void Scan_OfAFormThatNamesItsHandlerOrIsNotAPost_IsQuiet(string tag)
    {
        Assert.Empty(PageSourceScan.Scan("src/Shopfolio.Web/Pages/A.cshtml", tag + "</form>"));
    }

    [Theory]
    [InlineData("@using Shopfolio.Modules.Catalog.Contracts")]
    [InlineData("@model Shopfolio.Modules.Catalog.Contracts.ProductCard")]
    public void Scan_OfAViewThatUsesAModulesContracts_IsQuiet(string line)
    {
        Assert.Empty(PageSourceScan.Scan("src/Shopfolio.Web/Pages/A.cshtml", line));
    }

    [Theory]
    [InlineData("@using Shopfolio.Modules.Catalog")]
    [InlineData("@using Shopfolio.Modules.Catalog.Products")]
    [InlineData("@model Shopfolio.Modules.Ordering.OrderEntity")]
    [InlineData("@{ var x = new Shopfolio.Modules.Catalog.ProductEntity(); }")]
    public void Scan_OfAViewThatUsesAModulesInternals_IsAFinding(string line)
    {
        Assert.Equal([(PageSourceScan.ViewTouchesModuleInternals, 1)], PageSourceScan.Scan("src/Shopfolio.Web/Pages/A.cshtml", line));
    }

    [Fact]
    public void Scan_OfTheViewRules_DoesNotApplyToCodeFiles()
    {
        const string text = "using Shopfolio.Modules.Catalog; <form method=\"post\"> AppDbContext";

        Assert.Empty(PageSourceScan.Scan("src/Shopfolio.Web/Pages/A.cshtml.cs", text));
    }

    [Theory]
    [InlineData("@page \"/api/orders\"")]
    [InlineData("@page \"/v1/orders\"")]
    [InlineData("@page \"api/orders\"")]
    public void Scan_OfAPageTemplateWithAnApiOrAVersion_IsAFinding(string line)
    {
        Assert.Single(PageSourceScan.Scan("src/Shopfolio.Web/Pages/A.cshtml", line));
    }

    [Fact]
    public void Scan_OfAFullLineCommentAndOfARazorComment_IsQuiet()
    {
        Assert.Empty(PageSourceScan.Scan("src/A.cs", "// [ApiController] \"/api/x\" ApiVersion"));
        Assert.Empty(PageSourceScan.Scan("src/A.cshtml", "@* @inject AppDbContext <form method=\"post\"> *@"));
        Assert.Equal([(PageSourceScan.ViewTouchesContext, 3)], PageSourceScan.Scan("src/A.cshtml", "@* a\nb *@\n@inject AppDbContext c"));
    }

    private static string ReadSample(string name) =>
        File.ReadAllText(Path.Combine(RepositoryRoot.Find(), "tests", "Shopfolio.UnitTests", "Architecture", "ViolationSamples", "Pages", name));
}
