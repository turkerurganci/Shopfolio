using Shopfolio.UnitTests.Support;

namespace Shopfolio.UnitTests.Architecture;

/// <summary>
/// Runs the form version scan over the application source and shows on deliberate violation samples that each rule goes red (T208 criterion 7, KK7;
/// 07 2.4.5, 2.4.7; 09 19.2 item 3). The scan reads files and never writes one.
/// </summary>
public sealed class FormVersionSourceScanTests
{
    private static readonly string[] _scannedExtensions = ["*.cs", "*.cshtml"];

    [Fact]
    public void ApplicationSource_CarriesNoVersionNorShownValueInAnAddressOrAHeader()
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
                foreach ((string rule, int line) in FormVersionSourceScan.Scan(relative, File.ReadAllText(file)))
                {
                    findings.Add($"{path}:{line} {rule}");
                }
            }
        }

        Assert.Contains(scanned, f => f.EndsWith("Infrastructure/Forms/VersionFields.cs", StringComparison.Ordinal));
        Assert.Contains(scanned, f => f.EndsWith("Program.cs", StringComparison.Ordinal));
        Assert.Empty(findings);
    }

    [Fact]
    public void Scan_OnTheCodeSample_ReportsEachAddressAndHeaderFormAndNotTheCommentOrTheFineAddress()
    {
        string sample = ReadSample("VersionCarriage.cs");

        IReadOnlyList<(string Rule, int Line)> findings = FormVersionSourceScan.Scan("src/Shopfolio.Web/Endpoints/Misuse.cs", sample);

        Assert.Equal(
            [
                (FormVersionSourceScan.VersionInAddress, 7),
                (FormVersionSourceScan.VersionInAddress, 8),
                (FormVersionSourceScan.VersionInAddress, 9),
                (FormVersionSourceScan.VersionInAddress, 10),
                (FormVersionSourceScan.VersionInAddress, 11),
                (FormVersionSourceScan.VersionInAddress, 13),
                (FormVersionSourceScan.VersionInHeader, 19),
                (FormVersionSourceScan.VersionInHeader, 20),
                (FormVersionSourceScan.VersionInHeader, 21),
                (FormVersionSourceScan.VersionInHeader, 22),
                (FormVersionSourceScan.VersionInHeader, 23),
                (FormVersionSourceScan.VersionInHeader, 24),
                (FormVersionSourceScan.VersionInHeader, 25),
                (FormVersionSourceScan.VersionInAddress, 31),
            ],
            findings);
    }

    [Fact]
    public void Scan_OnTheViewSample_ReportsTheRouteValuesTheQueryPairTheModelStateFieldsAndEveryNonPostFormWithAField()
    {
        string sample = ReadSample("VersionFields.cshtml");

        IReadOnlyList<(string Rule, int Line)> findings = FormVersionSourceScan.Scan(@"src\Shopfolio.Web\Pages\Misuse.cshtml", sample);

        Assert.Equal(
            [
                (FormVersionSourceScan.VersionInAddress, 5),
                (FormVersionSourceScan.VersionInAddress, 6),
                (FormVersionSourceScan.VersionInAddress, 7),
                (FormVersionSourceScan.VersionFieldFromModelState, 9),
                (FormVersionSourceScan.VersionFieldFromModelState, 10),
                (FormVersionSourceScan.VersionFieldInNonPostForm, 15),
                (FormVersionSourceScan.VersionFieldInNonPostForm, 19),
            ],
            findings);
    }

    [Theory]
    [InlineData("<a asp-route-version=\"1\">")]
    [InlineData("<a asp-route-Version=\"1\">")]
    [InlineData("<a asp-route-paymentMethodsVersion=\"1\">")]
    [InlineData("<a asp-route-originalText=\"x\">")]
    [InlineData("<a href=\"/panel/x?Version=1\">")]
    [InlineData("<a href=\"/panel/x?a=1&PaymentMethodsVersion=1\">")]
    [InlineData("<a href=\"/panel/x?originalStock=1\">")]
    [InlineData("<a href=\"/panel/x?version=1\">")]
    [InlineData("@page \"/panel/x/{version:int}\"")]
    [InlineData("@page \"/panel/x/{id:guid}/{paymentMethodsVersion?}\"")]
    public void Scan_OfAnAddressThatCarriesAVersionOrAShownValue_IsAFinding(string line)
    {
        Assert.Equal([(FormVersionSourceScan.VersionInAddress, 1)], FormVersionSourceScan.Scan("src/Shopfolio.Web/Pages/A.cshtml", line));
    }

    [Theory]
    [InlineData("<a asp-route-id=\"1\" asp-route-handler=\"Open\">")]
    [InlineData("<a href=\"/panel/x?conversion=1\">")]
    [InlineData("<a href=\"/panel/x?page=2&search=a\">")]
    [InlineData("@page \"/panel/x/{id:guid}\"")]
    [InlineData("<form method=\"post\" asp-page-handler=\"Save\"><input type=\"hidden\" name=\"Version\" value=\"1\" /></form>")]
    [InlineData("<form\n  method='POST'\n  asp-page-handler=\"Save\">@VersionFields.Version(Model.V)</form>")]
    public void Scan_OfAnAddressWithoutAVersionAndOfAPostFormWithTheFields_IsQuiet(string line)
    {
        Assert.Empty(FormVersionSourceScan.Scan("src/Shopfolio.Web/Pages/A.cshtml", line));
    }

    [Theory]
    [InlineData("context.Response.Headers.ETag = \"x\";")]
    [InlineData("var m = context.Request.Headers.IfMatch;")]
    [InlineData("var m = context.Request.Headers[\"if-match\"];")]
    [InlineData("var m = context.Request.Headers[\"If-None-Match\"];")]
    [InlineData("var m = context.Request.Headers[\"If-Unmodified-Since\"];")]
    [InlineData("var m = HeaderNames.ETag;")]
    [InlineData("var m = HeaderNames.IfNoneMatch;")]
    [InlineData("var t = new EntityTagHeaderValue(\"1\");")]
    [InlineData("context.Response.Headers.Append(\"X-Version\", \"1\");")]
    [InlineData("context.Response.Headers.TryAdd(\"OriginalStock\", \"1\");")]
    [InlineData("var m = context.Request.Headers[$\"x-original-{name}\"];")]
    public void Scan_OfAHeaderThatCarriesAVersionOrIsAConditionalRequestHeader_IsAFinding(string line)
    {
        Assert.Equal([(FormVersionSourceScan.VersionInHeader, 1)], FormVersionSourceScan.Scan("src/Shopfolio.Web/Endpoints/A.cs", line));
    }

    [Fact]
    public void Scan_OfALinkCallThatNamesAVersionInItsArguments_IsAFindingWhateverTheSplit()
    {
        const string text = "return RedirectToPage(\n    \"Record\",\n    new { id = input.Id,\n    version = input.Version });";

        Assert.Equal([(FormVersionSourceScan.VersionInAddress, 1)], FormVersionSourceScan.Scan("src/Shopfolio.Web/Endpoints/A.cs", text));
    }

    [Theory]
    [InlineData("<input type=\"hidden\" asp-for=\"Input.Version\" />")]
    [InlineData("<input type=\"hidden\" asp-for=\"Input.OriginalStockQuantity\" />")]
    [InlineData("@Html.HiddenFor(m => m.Input.PaymentMethodsVersion)")]
    [InlineData("@Html.HiddenFor(\n  m => m.Input.OriginalText)")]
    public void Scan_OfAFieldMadeFromTheModelState_IsAFinding(string line)
    {
        Assert.Contains(
            (FormVersionSourceScan.VersionFieldFromModelState, 1),
            FormVersionSourceScan.Scan("src/Shopfolio.Web/Pages/A.cshtml", "<form method=\"post\" asp-page-handler=\"Save\">" + line + "</form>"));
    }

    [Fact]
    public void Scan_OfAFormWithoutAMethod_IsAGetForm()
    {
        string text = "<form action=\"/panel/ara\">\n<input name=\"OriginalText\" />\n</form>";

        Assert.Equal([(FormVersionSourceScan.VersionFieldInNonPostForm, 1)], FormVersionSourceScan.Scan("src/Shopfolio.Web/Pages/A.cshtml", text));
    }

    [Fact]
    public void Scan_OfFullLineCommentsAndRazorComments_IsQuiet()
    {
        Assert.Empty(FormVersionSourceScan.Scan("src/A.cs", "// Headers.ETag If-Match ?Version=1"));
        Assert.Empty(FormVersionSourceScan.Scan("src/A.cshtml", "@* asp-route-version=\"1\" *@"));
        Assert.Equal([(FormVersionSourceScan.VersionInHeader, 3)], FormVersionSourceScan.Scan("src/A.cshtml", "@* a\nb *@\n@{ var h = Headers.ETag; }"));
    }

    [Fact]
    public void Scan_OfTheViewRules_DoesNotApplyToCodeFiles()
    {
        const string text = "<form method=\"get\"><input name=\"Version\" /></form> asp-for=\"Input.Version\"";

        Assert.Empty(FormVersionSourceScan.Scan("src/Shopfolio.Web/Pages/A.cshtml.cs", text));
    }

    // The two rules must not lock each other (the page scan wants a named handler and no module internals, this scan wants the helper and no model-state field):
    // the sample pages of the integration tests are written in the shape a real page has, and both scans read them as application pages.
    [Theory]
    [InlineData("VersionedProduct.cshtml")]
    [InlineData("VersionedProduct.cshtml.cs")]
    [InlineData("VersionedStock.cshtml")]
    [InlineData("VersionedStock.cshtml.cs")]
    [InlineData("VersionedOrder.cshtml")]
    [InlineData("VersionedOrder.cshtml.cs")]
    public void TheSamplePagesOfTheVersionedForms_PassTheFormVersionScanAndThePageScan(string file)
    {
        string text = File.ReadAllText(Path.Combine(RepositoryRoot.Find(), "tests", "Shopfolio.IntegrationTests", "Pages", "Panel", file));
        string asApplicationPage = "src/Shopfolio.Web/Pages/Panel/" + file;

        Assert.Empty(FormVersionSourceScan.Scan(asApplicationPage, text));
        // The sample handlers read the context and the module internals to stay small; those two rules are the sample's own and not the shape under test.
        IReadOnlyList<(string Rule, int Line)> pageFindings = PageSourceScan.Scan(asApplicationPage, text);
        Assert.DoesNotContain(pageFindings, f => f.Rule != PageSourceScan.ViewTouchesContext && f.Rule != PageSourceScan.ViewTouchesModuleInternals);
    }

    private static string ReadSample(string name) =>
        File.ReadAllText(Path.Combine(RepositoryRoot.Find(), "tests", "Shopfolio.UnitTests", "Architecture", "ViolationSamples", "Forms", name));
}
