using Shopfolio.UnitTests.Support;

namespace Shopfolio.UnitTests.Architecture;

/// <summary>
/// Runs the concurrency source scan over the application source (T228; 09 10.2 items 4-6) and shows on a deliberate violation sample that each
/// rule goes red, and that the allowed places are full paths per rule (09 19.2 item 3). The scan reads files and never writes one.
/// </summary>
public sealed class ConcurrencySourceScanTests
{
    [Fact]
    public void ApplicationSource_TakesLocksAndWritesCountersOnlyInTheirPlaces()
    {
        string root = RepositoryRoot.Find();
        List<string> scanned = [];
        List<string> findings = [];
        foreach (string file in Directory.EnumerateFiles(Path.Combine(root, "src"), "*.cs", SearchOption.AllDirectories))
        {
            string relative = Path.GetRelativePath(root, file).Replace('\\', '/');
            if (relative.Contains("/obj/", StringComparison.Ordinal) || relative.Contains("/bin/", StringComparison.Ordinal) || !ConcurrencySourceScan.Applies(relative))
            {
                continue;
            }

            scanned.Add(relative);
            foreach ((string rule, int line) in ConcurrencySourceScan.Scan(relative, File.ReadAllText(file)))
            {
                findings.Add($"{relative}:{line} {rule}");
            }
        }

        Assert.Contains(ConcurrencySourceScan.AdvisoryLocksFile, scanned);
        Assert.Contains(ConcurrencySourceScan.JobClaimFile, scanned);
        Assert.Contains(ConcurrencySourceScan.DigitalFileLocksFile, scanned);
        Assert.Contains(ConcurrencySourceScan.CouponLocksFile, scanned);
        Assert.Contains(ConcurrencySourceScan.ProductLocksFile, scanned);
        Assert.Contains(ConcurrencySourceScan.OrderLocksFile, scanned);
        Assert.Contains(ConcurrencySourceScan.StockCountersFile, scanned);
        Assert.Contains(ConcurrencySourceScan.DownloadCountersFile, scanned);
        Assert.Contains(ConcurrencySourceScan.CompanySettingsWriterFile, scanned);
        Assert.Empty(findings);
    }

    [Fact]
    public void Scan_OnViolationSample_ReportsEachLockTextAndCounterWrite_AndNotTheCommentOrTheReads()
    {
        IReadOnlyList<(string Rule, int Line)> findings = ConcurrencySourceScan.Scan("src/Shopfolio.Web/Modules/Ordering/Sample.cs", ReadSample());

        Assert.Equal(
            [
                (ConcurrencySourceScan.RowLockText, 5),
                (ConcurrencySourceScan.RowLockText, 6),
                (ConcurrencySourceScan.RowLockText, 7),
                (ConcurrencySourceScan.ShareLockText, 8),
                (ConcurrencySourceScan.ShareLockText, 9),
                (ConcurrencySourceScan.CounterWrite, 10),
                (ConcurrencySourceScan.CounterWrite, 12),
                (ConcurrencySourceScan.CounterWrite, 16),
                (ConcurrencySourceScan.CounterWrite, 17),
                (ConcurrencySourceScan.RowLockText, 22),
                (ConcurrencySourceScan.CounterWrite, 23),
                (ConcurrencySourceScan.CounterWrite, 24),
                (ConcurrencySourceScan.CounterWrite, 25),
            ],
            findings);
    }

    [Fact]
    public void Scan_InTheDownloadCountersFile_AllowsTheDownloadCounterWrites_AndStillFindsTheStockCounterWrites()
    {
        IReadOnlyList<(string Rule, int Line)> findings = ConcurrencySourceScan.Scan(ConcurrencySourceScan.DownloadCountersFile, ReadSample());

        Assert.DoesNotContain(findings, f => f.Line is 23 or 24 or 25);
        Assert.Contains((ConcurrencySourceScan.CounterWrite, 10), findings);
        Assert.Contains((ConcurrencySourceScan.CounterWrite, 17), findings);
    }

    [Fact]
    public void Scan_InTheStockCountersFile_StillFindsTheDownloadCounterWrites_AndNotTheReadOrTheOtherColumn()
    {
        IReadOnlyList<(string Rule, int Line)> findings = ConcurrencySourceScan.Scan(ConcurrencySourceScan.StockCountersFile, ReadSample());

        Assert.Contains((ConcurrencySourceScan.CounterWrite, 23), findings);
        Assert.Contains((ConcurrencySourceScan.CounterWrite, 24), findings);
        Assert.Contains((ConcurrencySourceScan.CounterWrite, 25), findings);
        Assert.DoesNotContain(findings, f => f.Line is 10 or 12 or 16 or 17 or 28 or 29);
    }

    [Fact]
    public void Scan_AFileOfTheSameNameAsDownloadCountersElsewhere_IsNotAnAllowedPlace()
    {
        IReadOnlyList<(string Rule, int Line)> findings = ConcurrencySourceScan.Scan("src/Shopfolio.Web/Modules/Catalog/DownloadCounters.cs", ReadSample());

        Assert.Contains((ConcurrencySourceScan.CounterWrite, 23), findings);
    }

    [Fact]
    public void Scan_InTheAllowedPlaces_LeavesOutOnlyThatPlacesRule()
    {
        string sample = ReadSample();

        IReadOnlyList<(string Rule, int Line)> inOrderLocks = ConcurrencySourceScan.Scan(ConcurrencySourceScan.OrderLocksFile, sample);
        IReadOnlyList<(string Rule, int Line)> inAdvisoryLocks = ConcurrencySourceScan.Scan(ConcurrencySourceScan.AdvisoryLocksFile, sample);
        IReadOnlyList<(string Rule, int Line)> inStockCounters = ConcurrencySourceScan.Scan(ConcurrencySourceScan.StockCountersFile, sample);

        Assert.Equal([ConcurrencySourceScan.CounterWrite], inOrderLocks.Select(f => f.Rule).Distinct());
        Assert.Equal([ConcurrencySourceScan.ShareLockText, ConcurrencySourceScan.CounterWrite], inAdvisoryLocks.Select(f => f.Rule).Distinct());
        Assert.Equal([ConcurrencySourceScan.RowLockText, ConcurrencySourceScan.ShareLockText, ConcurrencySourceScan.CounterWrite], inStockCounters.Select(f => f.Rule).Distinct());
    }

    [Fact]
    public void Scan_InTheJobClaimFile_AllowsOnlyThePlainRowLockText()
    {
        IReadOnlyList<(string Rule, int Line)> findings = ConcurrencySourceScan.Scan(ConcurrencySourceScan.JobClaimFile, ReadSample());

        // Line 5 is the advisory lock, line 6 the plain row lock (allowed here), line 7 the weaker form (not allowed), lines 8-9 the shared locks.
        Assert.Contains((ConcurrencySourceScan.RowLockText, 5), findings);
        Assert.DoesNotContain((ConcurrencySourceScan.RowLockText, 6), findings);
        Assert.Contains((ConcurrencySourceScan.RowLockText, 7), findings);
        Assert.Contains((ConcurrencySourceScan.ShareLockText, 8), findings);
        Assert.Contains((ConcurrencySourceScan.CounterWrite, 10), findings);
    }

    [Fact]
    public void Scan_InTheDigitalFileLocksFile_AllowsOnlyThePlainRowLockText()
    {
        IReadOnlyList<(string Rule, int Line)> findings = ConcurrencySourceScan.Scan(ConcurrencySourceScan.DigitalFileLocksFile, ReadSample());

        Assert.Contains((ConcurrencySourceScan.RowLockText, 5), findings);
        Assert.DoesNotContain((ConcurrencySourceScan.RowLockText, 6), findings);
        Assert.Contains((ConcurrencySourceScan.RowLockText, 7), findings);
        Assert.Contains((ConcurrencySourceScan.ShareLockText, 8), findings);
        Assert.Contains((ConcurrencySourceScan.CounterWrite, 10), findings);
    }

    [Fact]
    public void Scan_InTheCouponLocksFile_AllowsOnlyThePlainRowLockText()
    {
        IReadOnlyList<(string Rule, int Line)> findings = ConcurrencySourceScan.Scan(ConcurrencySourceScan.CouponLocksFile, ReadSample());

        Assert.Contains((ConcurrencySourceScan.RowLockText, 5), findings);
        Assert.DoesNotContain((ConcurrencySourceScan.RowLockText, 6), findings);
        Assert.Contains((ConcurrencySourceScan.RowLockText, 7), findings);
        Assert.Contains((ConcurrencySourceScan.ShareLockText, 8), findings);
        Assert.Contains((ConcurrencySourceScan.CounterWrite, 10), findings);
    }

    [Fact]
    public void Scan_InTheProductLocksFile_AllowsOnlyTheNoKeyRowLockText_T525()
    {
        IReadOnlyList<(string Rule, int Line)> findings = ConcurrencySourceScan.Scan(ConcurrencySourceScan.ProductLocksFile, ReadSample());

        // The plain FOR UPDATE (line 6) would block the order lines' foreign key lock on the product row; only the NO KEY form (line 7) stands here.
        Assert.Contains((ConcurrencySourceScan.RowLockText, 5), findings);
        Assert.Contains((ConcurrencySourceScan.RowLockText, 6), findings);
        Assert.DoesNotContain((ConcurrencySourceScan.RowLockText, 7), findings);
        Assert.Contains((ConcurrencySourceScan.ShareLockText, 8), findings);
        Assert.Contains((ConcurrencySourceScan.CounterWrite, 10), findings);
    }

    [Theory]
    [InlineData("src/Shopfolio.Web/Modules/Catalog/Products.cs")]
    [InlineData("src/Shopfolio.Web/Modules/Ordering/ProductLocks.cs")]
    [InlineData("other/src/Shopfolio.Web/Modules/Catalog/ProductLocks.cs")]
    public void Scan_AFileOfTheSameNameAsProductLocksElsewhere_IsNotAnAllowedPlace_T525(string path)
    {
        IReadOnlyList<(string Rule, int Line)> findings = ConcurrencySourceScan.Scan(path, ReadSample());

        Assert.Contains((ConcurrencySourceScan.RowLockText, 6), findings);
        Assert.Contains((ConcurrencySourceScan.RowLockText, 7), findings);
    }

    [Theory]
    [InlineData("src/Shopfolio.Web/Modules/Catalog/Coupons.cs")]
    [InlineData("src/Shopfolio.Web/Modules/Ordering/CouponLocks.cs")]
    [InlineData("other/src/Shopfolio.Web/Modules/Catalog/CouponLocks.cs")]
    public void Scan_AFileOfTheSameNameAsCouponLocksElsewhere_OrTheCouponModule_IsNotAnAllowedPlace(string path)
    {
        IReadOnlyList<(string Rule, int Line)> findings = ConcurrencySourceScan.Scan(path, ReadSample());

        Assert.Contains((ConcurrencySourceScan.RowLockText, 6), findings);
    }

    [Theory]
    [InlineData("src/Shopfolio.Web/Modules/Ordering/DigitalFileLocks.cs")]
    [InlineData("other/src/Shopfolio.Web/Modules/Catalog/DigitalFileLocks.cs")]
    [InlineData("src/Shopfolio.Web/Endpoints/Files/UploadEndpoints.cs")]
    public void Scan_AFileOfTheSameNameAsDigitalFileLocksElsewhere_OrAnotherFile_IsNotAnAllowedPlace(string path)
    {
        IReadOnlyList<(string Rule, int Line)> findings = ConcurrencySourceScan.Scan(path, ReadSample());

        Assert.Contains((ConcurrencySourceScan.RowLockText, 6), findings);
    }

    [Fact]
    public void Scan_AFileOfTheSameNameAsJobClaimElsewhere_IsNotAnAllowedPlace()
    {
        IReadOnlyList<(string Rule, int Line)> findings = ConcurrencySourceScan.Scan("src/Shopfolio.Web/Modules/Payment/JobClaim.cs", ReadSample());

        Assert.Contains((ConcurrencySourceScan.RowLockText, 6), findings);
    }

    [Fact]
    public void Scan_AFileOfTheSameNameElsewhere_IsNotAnAllowedPlace()
    {
        IReadOnlyList<(string Rule, int Line)> findings = ConcurrencySourceScan.Scan("src/Shopfolio.Web/Modules/Catalog/OrderLocks.cs", ReadSample());

        Assert.Contains(findings, f => f.Rule == ConcurrencySourceScan.RowLockText);
        Assert.Contains(findings, f => f.Rule == ConcurrencySourceScan.ShareLockText);
    }

    [Fact]
    public void Scan_InTheCompanySettingsWriterFile_AllowsOnlyThePlainRowLockText()
    {
        IReadOnlyList<(string Rule, int Line)> findings = ConcurrencySourceScan.Scan(ConcurrencySourceScan.CompanySettingsWriterFile, ReadSample());

        Assert.Contains((ConcurrencySourceScan.RowLockText, 5), findings);
        Assert.DoesNotContain((ConcurrencySourceScan.RowLockText, 6), findings);
        Assert.Contains((ConcurrencySourceScan.RowLockText, 7), findings);
        Assert.Contains((ConcurrencySourceScan.ShareLockText, 8), findings);
    }

    [Fact]
    public void Scan_AFileOfTheSameNameAsTheSettingsWriterElsewhere_IsNotAnAllowedPlace()
    {
        string path = "src/Shopfolio.Web/Modules/Catalog/CompanySettingsWriter.cs";

        Assert.Contains((ConcurrencySourceScan.RowLockText, 6), ConcurrencySourceScan.Scan(path, ReadSample()));
        Assert.NotEmpty(ConcurrencySourceScan.Scan(path, ReadSettingsSample()));
    }

    [Fact]
    public void Scan_OnTheSettingsViolationSample_ReportsEveryBulkAndRawWriteOfTheRow_AndNotTheCommentOrTheReads()
    {
        IReadOnlyList<(string Rule, int Line)> findings = ConcurrencySourceScan.Scan("src/Shopfolio.Web/Modules/Ordering/Sample.cs", ReadSettingsSample());

        Assert.Equal(
            [
                (ConcurrencySourceScan.CompanySettingsWrite, 5),
                (ConcurrencySourceScan.CompanySettingsWrite, 6),
                (ConcurrencySourceScan.CompanySettingsWrite, 11),
                (ConcurrencySourceScan.CompanySettingsWrite, 14),
                (ConcurrencySourceScan.CompanySettingsWrite, 15),
                (ConcurrencySourceScan.CompanySettingsWrite, 16),
                (ConcurrencySourceScan.CompanySettingsWrite, 17),
            ],
            findings);
    }

    [Fact]
    public void Scan_TheSettingsWriterFile_MayWriteTheRow()
    {
        Assert.Empty(ConcurrencySourceScan.Scan(ConcurrencySourceScan.CompanySettingsWriterFile, ReadSettingsSample()));
    }

    [Fact]
    public void Scan_BulkWriteInAFileThatDoesNotNameTheSettingsRow_IsNotASettingsFinding()
    {
        // The counters' sample writes other tables in bulk; none of those lines is a settings finding.
        Assert.DoesNotContain(ConcurrencySourceScan.Scan("src/Shopfolio.Web/Modules/Ordering/Sample.cs", ReadSample()), f => f.Rule == ConcurrencySourceScan.CompanySettingsWrite);
    }

    private static string ReadSettingsSample()
    {
        return File.ReadAllText(Path.Combine(RepositoryRoot.Find(), "tests", "Shopfolio.UnitTests", "Architecture", "ViolationSamples", "Concurrency", "CompanySettingsWrites.cs"));
    }

    private static string ReadSample()
    {
        return File.ReadAllText(Path.Combine(RepositoryRoot.Find(), "tests", "Shopfolio.UnitTests", "Architecture", "ViolationSamples", "Concurrency", "LockAndCounterWrites.cs"));
    }
}
