using Shopfolio.Infrastructure.Migrations;
using Shopfolio.UnitTests.Support;

namespace Shopfolio.UnitTests.Architecture;

/// <summary>
/// Runs the third-party content scan over the application source and package files and shows on deliberate violation samples that each rule goes red
/// (T224 KK9; 02 12.2.5; 10 KD-19, KD-41, KD-55; 09 19.2 item 3). Also keeps the placeholder for the payment provider's frame cookies in the cookie
/// policy draft. The scan reads files and never writes one.
/// </summary>
public sealed class ThirdPartyContentScanTests
{
    private static readonly string[] _contentExtensions = [".cshtml", ".razor", ".html", ".htm", ".js", ".css", ".svg", ".cs"];

    [Fact]
    public void ApplicationSource_HasNoAnalyticsTagBannerEmbedOrForeignResource()
    {
        string root = RepositoryRoot.Find();
        List<string> scanned = [];
        List<string> findings = [];
        foreach (string file in Directory.EnumerateFiles(Path.Combine(root, "src"), "*", SearchOption.AllDirectories))
        {
            string relative = Path.GetRelativePath(root, file);
            string path = relative.Replace('\\', '/');
            if (path.Contains("/obj/", StringComparison.Ordinal) || path.Contains("/bin/", StringComparison.Ordinal)
                || !_contentExtensions.Contains(Path.GetExtension(path), StringComparer.OrdinalIgnoreCase))
            {
                continue;
            }

            scanned.Add(path);
            foreach ((string rule, int line) in ThirdPartyContentScan.Scan(relative, File.ReadAllText(file)))
            {
                findings.Add($"{path}:{line} {rule}");
            }
        }

        Assert.Contains("src/Shopfolio.Web/wwwroot/js/common.js", scanned);
        Assert.Contains("src/Shopfolio.Web/wwwroot/css/base.css", scanned);
        Assert.Contains(scanned, f => f.EndsWith("Program.cs", StringComparison.Ordinal));
        Assert.Empty(findings);
    }

    [Fact]
    public void PackageFiles_NameNoAnalyticsAdvertisingOrTagPackage()
    {
        string root = RepositoryRoot.Find();
        List<string> scanned = [];
        List<string> findings = [];
        foreach (string file in PackageFiles(root))
        {
            string path = Path.GetRelativePath(root, file).Replace('\\', '/');
            scanned.Add(path);
            foreach ((string rule, int line) in ThirdPartyContentScan.ScanPackages(File.ReadAllText(file)))
            {
                findings.Add($"{path}:{line} {rule}");
            }
        }

        Assert.Contains("Directory.Packages.props", scanned);
        Assert.Contains("src/Shopfolio.Web/Shopfolio.Web.csproj", scanned);
        Assert.Contains("src/Shopfolio.Web/packages.lock.json", scanned);
        Assert.Empty(findings);
    }

    [Fact]
    public void PaymentFramePages_IsClosedAndEmptyUntilThePaymentPageTaskAddsItsPage()
    {
        Assert.Empty(ThirdPartyContentScan.PaymentFramePages);
    }

    [Fact]
    public void Scan_OnViolationSamplePage_ReportsEachRuleAndNotTheAnchorOrTheOwnResources()
    {
        string sample = ReadSample("Page.html");

        IReadOnlyList<(string Rule, int Line)> findings = ThirdPartyContentScan.Scan("src/Shopfolio.Web/Pages/Misuse.cshtml", sample);

        Assert.Equal(
            [
                (ThirdPartyContentScan.ExternalResource, 2),
                (ThirdPartyContentScan.TrackingName, 2),
                (ThirdPartyContentScan.TrackingName, 3),
                (ThirdPartyContentScan.ExternalResource, 4),
                (ThirdPartyContentScan.ExternalResource, 5),
                (ThirdPartyContentScan.ConsentBannerName, 6),
                (ThirdPartyContentScan.EmbeddedContent, 7),
                (ThirdPartyContentScan.EmbeddedContent, 8),
                (ThirdPartyContentScan.EmbeddedContent, 10),
                (ThirdPartyContentScan.EmbeddedContent, 11),
                (ThirdPartyContentScan.EmbeddedContent, 12),
                (ThirdPartyContentScan.EmbeddedContent, 13),
                (ThirdPartyContentScan.EmbeddedContent, 14),
                (ThirdPartyContentScan.EmbeddedContent, 15),
            ],
            findings);
    }

    [Theory]
    [InlineData("<object data=\"x\"></object>")]
    [InlineData("<embed src=\"x\">")]
    [InlineData("<frameset rows=\"100%\"></frameset>")]
    [InlineData("<frame src=\"x\">")]
    [InlineData("el = document.createElement('object');")]
    [InlineData("el = document.createElement('embed');")]
    public void Scan_OnAPaymentFramePage_StillFindsAnObjectEmbedFrameOrFramesetElement(string text)
    {
        const string page = "src/Shopfolio.Web/Pages/Payment.cshtml";

        Assert.Equal([(ThirdPartyContentScan.EmbeddedContent, 1)], ThirdPartyContentScan.Scan(page, text, [page]));
        Assert.Equal([(ThirdPartyContentScan.EmbeddedContent, 1)], ThirdPartyContentScan.Scan("src/Shopfolio.Web/Pages/Other.cshtml", text, [page]));
    }

    [Theory]
    [InlineData("<iframe src=\"https://www.paytr.com/odeme/guvenli/tok\"></iframe>", false)]
    [InlineData("<iframe src=\"/odeme/gecis\"></iframe>", false)]
    [InlineData("<iframe title=\"x\" src='https://www.paytr.com/x'></iframe>", false)]
    [InlineData("<iframe src=\"https://www.paytr.com.example.net/odeme\"></iframe>", true)]
    [InlineData("<iframe src=\"https://www.paytr.com@example.net/odeme\"></iframe>", true)]
    [InlineData("<iframe src=\"https://example.net/odeme\"></iframe>", true)]
    [InlineData("<iframe src=\"//example.net/odeme\"></iframe>", true)]
    [InlineData("<iframe srcdoc=\"&lt;p&gt;\"></iframe>", true)]
    [InlineData("<iframe src=\"data:text/html,x\"></iframe>", true)]
    public void Scan_OnAPaymentFramePage_ChecksTheFrameAddress(string text, bool finding)
    {
        const string page = "src/Shopfolio.Web/Pages/Payment.cshtml";

        IReadOnlyList<(string Rule, int Line)> findings = ThirdPartyContentScan.Scan(page, text, [page]);

        Assert.Equal(finding ? [(ThirdPartyContentScan.FrameSource, 1)] : [], findings);
    }

    [Fact]
    public void Scan_OnAPaymentFramePage_AllowsTheFrameElementAndNothingElse()
    {
        string sample = ReadSample("Page.html");
        const string page = "src/Shopfolio.Web/Pages/Payment.cshtml";

        IReadOnlyList<(string Rule, int Line)> onPage = ThirdPartyContentScan.Scan(page, sample, [page]);
        IReadOnlyList<(string Rule, int Line)> elsewhere = ThirdPartyContentScan.Scan("src/Shopfolio.Web/Pages/Other.cshtml", sample, [page]);

        // The frames of lines 8 (no address) and 14 (the provider's address) are no finding on the page; object, embed and frameset (lines 10 to 13) are, and so
        // are the frame with a video address (line 7: an embedded video and a foreign address) and the look-alike provider address (line 15).
        Assert.DoesNotContain(onPage, f => f.Line is 8 or 14);
        Assert.Contains((ThirdPartyContentScan.EmbeddedContent, 7), onPage);
        Assert.Contains((ThirdPartyContentScan.FrameSource, 7), onPage);
        Assert.Contains((ThirdPartyContentScan.FrameSource, 15), onPage);
        Assert.DoesNotContain((ThirdPartyContentScan.EmbeddedContent, 15), onPage);
        foreach (int line in new[] { 10, 11, 12, 13 })
        {
            Assert.Contains((ThirdPartyContentScan.EmbeddedContent, line), onPage);
        }

        Assert.Contains((ThirdPartyContentScan.TrackingName, 2), onPage);
        Assert.Contains((ThirdPartyContentScan.ExternalResource, 4), onPage);
        Assert.Contains((ThirdPartyContentScan.EmbeddedContent, 8), elsewhere);
    }

    [Fact]
    public void ScanPackages_OnViolationSample_ReportsTheAnalyticsPackagesAndNotAnotherOne()
    {
        IReadOnlyList<(string Rule, int Line)> findings = ThirdPartyContentScan.ScanPackages(ReadSample("Packages.props"));

        Assert.Equal([(ThirdPartyContentScan.TrackingPackage, 3), (ThirdPartyContentScan.TrackingPackage, 5)], findings);
    }

    [Fact]
    public void CookiePolicyDraft_CarriesThePlaceholderForThePaymentFrameCookiesAndTheSevenCookies()
    {
        string draft = InsertStartupData.CookiePolicyDraft.ReplaceLineEndings("\n");

        Assert.Contains("<h3>Ödeme sağlayıcısının çerezleri</h3>", draft, StringComparison.Ordinal);
        Assert.Contains("<p>[Ödeme sağlayıcınızın çerçevesi", draft, StringComparison.Ordinal);
        int list = draft.IndexOf("<ul>", StringComparison.Ordinal);
        int listEnd = draft.IndexOf("</ul>", StringComparison.Ordinal);
        Assert.InRange(list, 0, listEnd);
        Assert.Equal(7, draft[list..listEnd].Split("<li>").Length - 1);
        Assert.Contains("Üçüncü taraf çerezi, izleme pikseli ve reklam etiketi kullanılmaz.", draft, StringComparison.Ordinal);
    }

    private static string ReadSample(string name) =>
        File.ReadAllText(Path.Combine(RepositoryRoot.Find(), "tests", "Shopfolio.UnitTests", "Architecture", "ViolationSamples", "ThirdParty", name));

    private static IEnumerable<string> PackageFiles(string root)
    {
        yield return Path.Combine(root, "Directory.Packages.props");
        yield return Path.Combine(root, "Directory.Build.props");
        foreach (string file in Directory.EnumerateFiles(Path.Combine(root, "src"), "*", SearchOption.AllDirectories))
        {
            string path = file.Replace('\\', '/');
            if (path.Contains("/obj/", StringComparison.Ordinal) || path.Contains("/bin/", StringComparison.Ordinal))
            {
                continue;
            }

            if (path.EndsWith(".csproj", StringComparison.Ordinal) || path.EndsWith("/packages.lock.json", StringComparison.Ordinal))
            {
                yield return file;
            }
        }
    }
}
