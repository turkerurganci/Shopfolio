using Shopfolio.UnitTests.Support;

namespace Shopfolio.UnitTests.Architecture;

/// <summary>
/// Runs the transition unit's source scan over the application source (T315 KK2; 09 9.1 items 4, 6, 8) and shows on deliberate violation samples
/// that each rule goes red, that the unit's own file is the one allowed place of the writers and that the allowed texts stay free (09 19.2 item 3).
/// The scan reads files and never writes one.
/// </summary>
public sealed class TransitionUnitSourceScanTests
{
    private const string AnotherFile = "src/Shopfolio.Web/Modules/Ordering/Sample.cs";

    [Fact]
    public void ApplicationSource_WritesTheStatusFieldsOnlyInTheTransitionUnit()
    {
        string root = RepositoryRoot.Find();
        List<string> scanned = [];
        List<string> findings = [];
        foreach (string file in Directory.EnumerateFiles(Path.Combine(root, "src"), "*.cs", SearchOption.AllDirectories))
        {
            string relative = Path.GetRelativePath(root, file).Replace('\\', '/');
            if (relative.Contains("/obj/", StringComparison.Ordinal) || relative.Contains("/bin/", StringComparison.Ordinal) || !TransitionUnitSourceScan.Applies(relative))
            {
                continue;
            }

            scanned.Add(relative);
            foreach ((string rule, int line) in TransitionUnitSourceScan.Scan(relative, File.ReadAllText(file)))
            {
                findings.Add($"{relative}:{line} {rule}");
            }
        }

        Assert.Contains(TransitionUnitSourceScan.UnitFile, scanned);
        Assert.Contains("src/Shopfolio.Web/Modules/Ordering/Order.cs", scanned);
        Assert.Contains("src/Shopfolio.Web/Modules/AfterSales/DefectClaim.cs", scanned);
        Assert.DoesNotContain(scanned, p => p.Contains("/Migrations/", StringComparison.Ordinal));
        Assert.Empty(findings);
    }

    [Fact]
    public void Scan_OnTheWriterSample_ReportsEachNameOutsideTheUnit_AndEachOpenStatusField()
    {
        IReadOnlyList<(string Rule, int Line)> findings = TransitionUnitSourceScan.Scan(AnotherFile, ReadSample("WriterAndSetters.cs"));

        Assert.Equal(
            [
                (TransitionUnitSourceScan.OpenSetter, 5),
                (TransitionUnitSourceScan.OpenSetter, 6),
                (TransitionUnitSourceScan.OpenSetter, 7),
                (TransitionUnitSourceScan.OpenSetter, 8),
                (TransitionUnitSourceScan.OpenSetter, 10),
                (TransitionUnitSourceScan.OpenSetter, 11),
                (TransitionUnitSourceScan.OpenSetter, 18),
                (TransitionUnitSourceScan.OpenSetter, 19),
                (TransitionUnitSourceScan.OutsideCall, 27),
                (TransitionUnitSourceScan.OutsideCall, 28),
                (TransitionUnitSourceScan.OutsideCall, 29),
                (TransitionUnitSourceScan.OutsideCall, 30),
                (TransitionUnitSourceScan.OutsideCall, 31),
                (TransitionUnitSourceScan.OutsideCall, 32),
            ],
            findings);
    }

    [Fact]
    public void Scan_OnTheWriterSample_DoesNotReportThePrivateSetters_TheGetOnlyOnes_TheDeclarationsOrTheComment()
    {
        IReadOnlyList<(string Rule, int Line)> findings = TransitionUnitSourceScan.Scan(AnotherFile, ReadSample("WriterAndSetters.cs"));

        // 2 comment; 9 private set; 16, 17 get only; 20 private field; 21 other name; 22-24 the declarations of the writers.
        Assert.DoesNotContain(findings, f => f.Line is 1 or 2 or 9 or 16 or 17 or 20 or 21 or 22 or 23 or 24);
    }

    [Fact]
    public void Scan_InTheUnitFile_AllowsTheCallsOfThisUnitsWriters_ButNotTheDefectClaimsOne()
    {
        IReadOnlyList<(string Rule, int Line)> findings = TransitionUnitSourceScan.Scan(TransitionUnitSourceScan.UnitFile, ReadSample("WriterAndSetters.cs"));

        Assert.DoesNotContain(findings, f => f.Line is >= 27 and <= 30);
        Assert.Contains((TransitionUnitSourceScan.OutsideCall, 31), findings);
        Assert.Contains((TransitionUnitSourceScan.OutsideCall, 32), findings);
        Assert.Contains((TransitionUnitSourceScan.OpenSetter, 5), findings);
    }

    [Fact]
    public void Scan_AFileOfTheSameNameAsTheUnitElsewhere_IsNotTheAllowedPlace()
    {
        foreach (string path in new[] { "src/Shopfolio.Web/Modules/AfterSales/OrderTransitions.cs", "other/src/Shopfolio.Web/Modules/Ordering/OrderTransitions.cs" })
        {
            IReadOnlyList<(string Rule, int Line)> findings = TransitionUnitSourceScan.Scan(path, ReadSample("WriterAndSetters.cs"));

            Assert.Contains((TransitionUnitSourceScan.OutsideCall, 27), findings);
            Assert.Contains((TransitionUnitSourceScan.OutsideCall, 28), findings);
        }
    }

    [Fact]
    public void Scan_OnTheBulkSample_ReportsEachBulkAndRawWriteOfAStatusColumn()
    {
        IReadOnlyList<(string Rule, int Line)> findings = TransitionUnitSourceScan.Scan(AnotherFile, ReadSample("BulkWrites.cs"));

        Assert.Equal(
            [
                (TransitionUnitSourceScan.BulkStatusWrite, 7),
                (TransitionUnitSourceScan.BulkStatusWrite, 8),
                (TransitionUnitSourceScan.BulkStatusWrite, 11),
                (TransitionUnitSourceScan.BulkStatusWrite, 13),
                (TransitionUnitSourceScan.BulkStatusWrite, 14),
                (TransitionUnitSourceScan.BulkStatusWrite, 15),
                (TransitionUnitSourceScan.BulkStatusWrite, 16),
                (TransitionUnitSourceScan.BulkStatusWrite, 18),
                (TransitionUnitSourceScan.BulkStatusWrite, 24),
                (TransitionUnitSourceScan.PropertyByName, 25),
                (TransitionUnitSourceScan.PropertyByName, 26),
            ],
            findings);
    }

    [Fact]
    public void Scan_OnTheBulkSample_LeavesTheFilterTheRuleTheReadAndTheOtherColumnFree()
    {
        IReadOnlyList<(string Rule, int Line)> findings = TransitionUnitSourceScan.Scan(AnotherFile, ReadSample("BulkWrites.cs"));

        // 2 comment; 10 the version setter of the same call; 31 version only; 32 filter; 33 rule; 34 read; 35 raw update of another column.
        Assert.DoesNotContain(findings, f => f.Line is 2 or 10 or 31 or 32 or 33 or 34 or 35);
    }

    [Fact]
    public void Scan_OnTheUnitHttpSample_ReportsTheHttpTypesOnlyInTheUnitFile()
    {
        IReadOnlyList<(string Rule, int Line)> inUnit = TransitionUnitSourceScan.Scan(TransitionUnitSourceScan.UnitFile, ReadSample("UnitHttp.cs"));
        IReadOnlyList<(string Rule, int Line)> elsewhere = TransitionUnitSourceScan.Scan(AnotherFile, ReadSample("UnitHttp.cs"));

        Assert.Equal(
            [
                (TransitionUnitSourceScan.UnitHttp, 3),
                (TransitionUnitSourceScan.UnitHttp, 7),
                (TransitionUnitSourceScan.UnitHttp, 8),
            ],
            inUnit);
        Assert.Empty(elsewhere);
    }

    [Theory]
    [InlineData("src/Shopfolio.Web/Infrastructure/Migrations/20261104051528_CreateCartAndOrderCoreTables.cs", false)]
    [InlineData("src/Shopfolio.Web/Infrastructure/AppDbContext.cs", true)]
    [InlineData("src/Shopfolio.Web/Modules/Ordering/Order.cs", true)]
    public void Applies_LeavesOutTheMigrations(string path, bool expected)
    {
        Assert.Equal(expected, TransitionUnitSourceScan.Applies(path));
    }

    private static string ReadSample(string fileName)
    {
        return File.ReadAllText(Path.Combine(RepositoryRoot.Find(), "tests", "Shopfolio.UnitTests", "Architecture", "ViolationSamples", "TransitionUnit", fileName));
    }
}
