using Shopfolio.UnitTests.Support;

namespace Shopfolio.UnitTests.Architecture;

/// <summary>
/// The <c>notice</c> parameter is built and read in one place (T214 criteria 2 and 3, KK2; 07 2.2.3; 09 5.3 item 6, 19.2 item 3), over the real files and on
/// deliberate violation samples: the scan finds every quoting and joining form of the parameter and spares exactly the one file, by exact path.
/// </summary>
public sealed class NoticeParameterSourceScanTests
{
    private const string AnyFile = "src/Shopfolio.Web/Infrastructure/Forms/Sample.cs";

    [Fact]
    public void TheApplication_BuildsAndReadsTheNoticeParameterInTheWriterOnly()
    {
        string root = RepositoryRoot.Find();
        List<string> scanned = [];
        List<string> findings = [];
        foreach (string pattern in new[] { "*.cs", "*.cshtml" })
        {
            foreach (string file in Directory.EnumerateFiles(Path.Combine(root, "src"), pattern, SearchOption.AllDirectories))
            {
                string relative = Path.GetRelativePath(root, file);
                scanned.Add(relative.Replace('\\', '/'));
                findings.AddRange(NoticeParameterSourceScan.Scan(relative, File.ReadAllText(file)).Select(f => $"{relative}:{f.Line} {f.Rule}"));
            }
        }

        Assert.Contains(NoticeParameterSourceScan.WriterFile, scanned);
        Assert.Empty(findings);
    }

    [Fact]
    public void TheWriter_StillHoldsTheFormsTheScanSpares_SoTheScanIsNotEmpty()
    {
        string writer = File.ReadAllText(Path.Combine(RepositoryRoot.Find(), NoticeParameterSourceScan.WriterFile));

        string[] findings = [.. NoticeParameterSourceScan.Scan("src/Shopfolio.Web/Infrastructure/Other/Copy.cs", writer).Select(f => f.Rule)];

        Assert.Contains(NoticeParameterSourceScan.ParameterNameLiteral, findings);
    }

    [Theory]
    [InlineData("src/Shopfolio.Web/Infrastructure/Other/NoticeParameter.cs")]
    [InlineData("src/Shopfolio.Web/Infrastructure/Notices/NoticeParameter.cs.bak")]
    [InlineData("src/Shopfolio.Web/Infrastructure/Notices/noticeparameter.cs")]
    [InlineData("src/Shopfolio.Web/Infrastructure/Notices/Sub/NoticeParameter.cs")]
    [InlineData("src/Shopfolio.Web/Infrastructure/Notices/NoticeParameters.cs")]
    public void TheExemption_IsTheExactPathOnly(string relativePath)
    {
        Assert.NotEmpty(NoticeParameterSourceScan.Scan(relativePath, "string a = request.Query[\"notice\"];"));
    }

    [Fact]
    public void OnTheCodeSample_EveryFormIsReported_AndTheNearNamesAreNot()
    {
        IReadOnlyList<(string Rule, int Line)> findings = NoticeParameterSourceScan.Scan(AnyFile, Sample("NoticeElsewhere.cs"));

        Assert.Equal(
            [
                (NoticeParameterSourceScan.ParameterNameLiteral, 7),
                (NoticeParameterSourceScan.ParameterNameLiteral, 8),
                (NoticeParameterSourceScan.ParameterNameLiteral, 9),
                (NoticeParameterSourceScan.AddressLiteral, 10),
                (NoticeParameterSourceScan.AddressLiteral, 11),
                (NoticeParameterSourceScan.ParameterNameLiteral, 12),
                (NoticeParameterSourceScan.AddressLiteral, 13),
                (NoticeParameterSourceScan.AddressLiteral, 14),
                (NoticeParameterSourceScan.ParameterNameLiteral, 18),
            ],
            findings);
    }

    [Fact]
    public void OnTheViewSample_TheTagHelperAndTheHrefFormsAreReported()
    {
        IReadOnlyList<(string Rule, int Line)> findings = NoticeParameterSourceScan.Scan("src/Shopfolio.Web/Pages/Misuse.cshtml", Sample("NoticeInAView.cshtml"));

        Assert.Equal(
            [
                (NoticeParameterSourceScan.RouteValue, 2),
                (NoticeParameterSourceScan.AddressLiteral, 3),
                (NoticeParameterSourceScan.RouteValue, 4),
            ],
            findings);
    }

    [Fact]
    public void AVariableNamedNotice_InCode_IsNoFinding_ButTheSameTextInAViewIsARouteValue()
    {
        Assert.Empty(Scan("string notice = \"hello\"; Show(notice);"));
        Assert.Single(NoticeParameterSourceScan.Scan("src/Shopfolio.Web/Pages/Misuse.cshtml", "<a href='x' notice=\"Saved\">x</a>"));
    }

    private static IReadOnlyList<string> Scan(string text) => [.. NoticeParameterSourceScan.Scan(AnyFile, text).Select(f => f.Rule)];

    private static string Sample(string name) =>
        File.ReadAllText(Path.Combine(RepositoryRoot.Find(), "tests", "Shopfolio.UnitTests", "Architecture", "ViolationSamples", "Notices", name));
}
