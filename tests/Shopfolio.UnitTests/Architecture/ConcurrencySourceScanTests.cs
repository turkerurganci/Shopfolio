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
        Assert.Contains(ConcurrencySourceScan.OrderLocksFile, scanned);
        Assert.Contains(ConcurrencySourceScan.StockCountersFile, scanned);
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
            ],
            findings);
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
        Assert.Equal([ConcurrencySourceScan.RowLockText, ConcurrencySourceScan.ShareLockText], inStockCounters.Select(f => f.Rule).Distinct());
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

    private static string ReadSample()
    {
        return File.ReadAllText(Path.Combine(RepositoryRoot.Find(), "tests", "Shopfolio.UnitTests", "Architecture", "ViolationSamples", "Concurrency", "LockAndCounterWrites.cs"));
    }
}
