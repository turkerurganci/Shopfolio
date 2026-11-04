using Shopfolio.UnitTests.Support;

namespace Shopfolio.UnitTests.Architecture;

/// <summary>
/// Runs the start-up data source scan over the application (T119 KK6, KK8; 06 8.1.1, 8.4.2) and shows on deliberate violation samples that
/// the scan goes red (09 19.2 item 3).
/// </summary>
public sealed class StartupDataSourceScanTests
{
    [Fact]
    public void ApplicationCode_HasNoPathThatCreatesAStartupRow()
    {
        string root = RepositoryRoot.Find();
        string[] files = SourceFiles(root).Where(f => !StartupDataSourceScan.IsMigration(Path.GetRelativePath(root, f))).ToArray();

        Assert.Contains(files, f => f.EndsWith("Company.cs", StringComparison.Ordinal));
        Assert.Empty(Findings(root, files, file => StartupDataSourceScan.ScanCode(Path.GetFileName(file), File.ReadAllText(file))));
    }

    [Fact]
    public void Migrations_WriteNoKeyRingRowAndNoGeneratedTextVersion()
    {
        string root = RepositoryRoot.Find();
        string[] files = SourceFiles(root).Where(f => StartupDataSourceScan.IsMigration(Path.GetRelativePath(root, f))).ToArray();

        Assert.Contains(files, f => f.EndsWith("_InsertStartupData.cs", StringComparison.Ordinal));
        Assert.Empty(Findings(root, files, file => StartupDataSourceScan.ScanMigration(File.ReadAllText(file))));
    }

    [Theory]
    [InlineData(StartupDataSourceScan.FactoryCall, 7)]
    [InlineData(StartupDataSourceScan.SetAdd, 7)]
    [InlineData(StartupDataSourceScan.FactoryCall, 8)]
    [InlineData(StartupDataSourceScan.SetAdd, 8)]
    [InlineData(StartupDataSourceScan.Constructor, 9)]
    [InlineData(StartupDataSourceScan.SetAdd, 9)]
    [InlineData(StartupDataSourceScan.FactoryCall, 10)]
    [InlineData(StartupDataSourceScan.SqlInsert, 11)]
    [InlineData(StartupDataSourceScan.SqlInsert, 12)]
    [InlineData(StartupDataSourceScan.FactoryCall, 13)]
    [InlineData(StartupDataSourceScan.SetAdd, 13)]
    [InlineData(StartupDataSourceScan.Constructor, 14)]
    [InlineData(StartupDataSourceScan.SetAdd, 14)]
    public void ScanCode_OnViolationSample_ReportsTheRuleOnItsLine(string rule, int line)
    {
        IReadOnlyList<(string Rule, int Line)> findings = StartupDataSourceScan.ScanCode("CodeViolations.cs", File.ReadAllText(SamplePath("CodeViolations.cs")));

        Assert.Contains((rule, line), findings);
    }

    [Theory]
    [InlineData(StartupDataSourceScan.KeyRingRow, 7)]
    [InlineData(StartupDataSourceScan.KeyRingRow, 8)]
    [InlineData(StartupDataSourceScan.GeneratedTextVersion, 9)]
    public void ScanMigration_OnViolationSample_ReportsTheRuleOnItsLine(string rule, int line)
    {
        IReadOnlyList<(string Rule, int Line)> findings = StartupDataSourceScan.ScanMigration(File.ReadAllText(SamplePath("MigrationViolations.cs")));

        Assert.Contains((rule, line), findings);
    }

    [Fact]
    public void Scan_OnCleanSample_ReportsNothing()
    {
        string text = File.ReadAllText(SamplePath("Clean.cs"));

        Assert.Empty(StartupDataSourceScan.ScanCode("Clean.cs", text));
        Assert.Empty(StartupDataSourceScan.ScanMigration(text));
    }

    /// <summary>An entity's own file may construct it (its factory); the same line elsewhere is a finding.</summary>
    [Fact]
    public void ScanCode_ExemptsOnlyTheEntitysOwnFileFromTheConstructorRule()
    {
        const string Line = "        return new Company { Id = EntityIds.New(), CreatedAt = now };";

        Assert.Empty(StartupDataSourceScan.ScanCode("Company.cs", Line));
        Assert.Contains((StartupDataSourceScan.Constructor, 1), StartupDataSourceScan.ScanCode("CompanyService.cs", Line));
    }

    private static List<string> Findings(string root, string[] files, Func<string, IReadOnlyList<(string Rule, int Line)>> scan)
    {
        List<string> findings = [];
        foreach (string file in files)
        {
            foreach ((string rule, int line) in scan(file))
            {
                findings.Add($"{Path.GetRelativePath(root, file)}:{line} {rule}");
            }
        }

        return findings;
    }

    private static IEnumerable<string> SourceFiles(string root)
    {
        return Directory.EnumerateFiles(Path.Combine(root, "src"), "*.cs", SearchOption.AllDirectories)
            .Where(f => !f.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal)
                && !f.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.Ordinal));
    }

    private static string SamplePath(string fileName)
    {
        return Path.Combine(RepositoryRoot.Find(), "tests", "Shopfolio.UnitTests", "Architecture", "ViolationSamples", "StartupData", fileName);
    }
}
