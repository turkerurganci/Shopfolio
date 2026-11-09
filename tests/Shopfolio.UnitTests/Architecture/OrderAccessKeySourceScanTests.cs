using Shopfolio.UnitTests.Support;

namespace Shopfolio.UnitTests.Architecture;

/// <summary>
/// Runs the access key scan over the application source (T375; D-567) and shows on a deliberate violation sample that each rule goes red and that the two allowed files are
/// full paths (09 19.2 item 3). The scan reads files and never writes one.
/// </summary>
public sealed class OrderAccessKeySourceScanTests
{
    [Fact]
    public void ApplicationSource_WritesTheAccessKeyInTheOrderOnly_AndRenewsItInTheCorrectionOnly()
    {
        string root = RepositoryRoot.Find();
        List<string> scanned = [];
        List<string> findings = [];
        foreach (string file in Directory.EnumerateFiles(Path.Combine(root, "src"), "*.cs", SearchOption.AllDirectories))
        {
            string relative = Path.GetRelativePath(root, file).Replace('\\', '/');
            if (!OrderAccessKeySourceScan.Applies(relative))
            {
                continue;
            }

            scanned.Add(relative);
            foreach ((string rule, int line) in OrderAccessKeySourceScan.Scan(relative, File.ReadAllText(file)))
            {
                findings.Add($"{relative}:{line} {rule}");
            }
        }

        Assert.Contains(OrderAccessKeySourceScan.OrderFile, scanned);
        Assert.Contains(OrderAccessKeySourceScan.CorrectionsFile, scanned);
        Assert.Empty(findings);
    }

    [Fact]
    public void Scan_OnViolationSample_ReportsTheWriteTheBulkUpdateAndTheRenewal_AndNotTheCommentTheComparisonOrTheRead()
    {
        IReadOnlyList<(string Rule, int Line)> findings = OrderAccessKeySourceScan.Scan("src/Shopfolio.Web/Modules/Ordering/KeyWriters.cs", ReadSample());

        Assert.Equal(
            [
                (OrderAccessKeySourceScan.KeyWrite, 8),
                (OrderAccessKeySourceScan.RenewalCall, 10),
                (OrderAccessKeySourceScan.KeyWrite, 11),
            ],
            findings);
    }

    [Fact]
    public void Scan_InTheCorrectionsFile_AllowsTheRenewal_ButNotTheWrite()
    {
        IReadOnlyList<(string Rule, int Line)> findings = OrderAccessKeySourceScan.Scan(OrderAccessKeySourceScan.CorrectionsFile, ReadSample());

        Assert.DoesNotContain(findings, f => f.Rule == OrderAccessKeySourceScan.RenewalCall);
        Assert.Contains((OrderAccessKeySourceScan.KeyWrite, 8), findings);
    }

    [Fact]
    public void Scan_InTheOrderFile_AllowsTheWrite_ButNotTheRenewalCall()
    {
        IReadOnlyList<(string Rule, int Line)> findings = OrderAccessKeySourceScan.Scan(OrderAccessKeySourceScan.OrderFile, ReadSample());

        Assert.DoesNotContain(findings, f => f.Rule == OrderAccessKeySourceScan.KeyWrite);
    }

    [Fact]
    public void Scan_AFileOfTheSameNameElsewhere_IsNotAnAllowedPlace()
    {
        IReadOnlyList<(string Rule, int Line)> findings = OrderAccessKeySourceScan.Scan("src/Shopfolio.Web/Modules/Catalog/OrderCorrections.cs", ReadSample());

        Assert.Contains((OrderAccessKeySourceScan.RenewalCall, 10), findings);
        Assert.Contains((OrderAccessKeySourceScan.KeyWrite, 8), findings);
    }

    private static string ReadSample() =>
        File.ReadAllText(Path.Combine(RepositoryRoot.Find(), "tests", "Shopfolio.UnitTests", "Architecture", "ViolationSamples", "OrderAccessKey", "KeyWriters.cs"));
}
