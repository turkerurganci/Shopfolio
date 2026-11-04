using Shopfolio.UnitTests.Support;

namespace Shopfolio.UnitTests.Architecture;

/// <summary>
/// Runs the migration source scan over the application's migrations (T103; 09 10.6 item 11, 10.8 item 7) and shows on deliberate
/// violation samples that the scan goes red (09 19.2 item 3).
/// </summary>
public sealed class MigrationSourceScanTests
{
    [Fact]
    public void Migrations_OfTheApplication_BreakNoRule()
    {
        string root = RepositoryRoot.Find();
        string[] files = Directory.EnumerateFiles(Path.Combine(root, "src"), "*.cs", SearchOption.AllDirectories)
            .Where(f => !IsBuildOutput(f))
            .Where(f => MigrationSourceScan.Applies(Path.GetRelativePath(root, f)))
            .ToArray();

        Assert.Contains(files, f => f.EndsWith("_CreateSettingsTables.cs", StringComparison.Ordinal));
        List<string> findings = [];
        foreach (string file in files)
        {
            foreach ((string rule, int line) in MigrationSourceScan.Scan(File.ReadAllText(file)))
            {
                findings.Add($"{Path.GetRelativePath(root, file)}:{line} {rule}");
            }
        }

        Assert.Empty(findings);
    }

    [Theory]
    [InlineData("DownAndTransaction.cs", MigrationSourceScan.SuppressedTransaction, 5)]
    [InlineData("DownAndTransaction.cs", MigrationSourceScan.DownMethod, 7)]
    [InlineData("DangerousSql.cs", MigrationSourceScan.NonTransactionalStatement, 7)]
    [InlineData("DangerousSql.cs", MigrationSourceScan.NonTransactionalStatement, 8)]
    [InlineData("DangerousSql.cs", MigrationSourceScan.NonTransactionalStatement, 9)]
    [InlineData("DangerousSql.cs", MigrationSourceScan.NonTransactionalStatement, 10)]
    [InlineData("DangerousSql.cs", MigrationSourceScan.NonTransactionalStatement, 11)]
    [InlineData("DangerousSql.cs", MigrationSourceScan.DatabaseClock, 12)]
    [InlineData("Privileges.cs", MigrationSourceScan.FixedUserName, 7)]
    [InlineData("Privileges.cs", MigrationSourceScan.UnscopedPrivilege, 7)]
    [InlineData("Privileges.cs", MigrationSourceScan.UnscopedPrivilege, 8)]
    [InlineData("Privileges.cs", MigrationSourceScan.PublicPrivilege, 8)]
    [InlineData("AuditTrail.cs", MigrationSourceScan.AuditLogRowWrite, 7)]
    [InlineData("AuditTrail.cs", MigrationSourceScan.AuditLogRowWrite, 8)]
    [InlineData("AuditTrail.cs", MigrationSourceScan.AuditLogStructureChange, 9)]
    [InlineData("AuditTrail.cs", MigrationSourceScan.AuditLogStructureChange, 10)]
    [InlineData("AuditTrail.cs", MigrationSourceScan.AuditLogStructureChange, 11)]
    [InlineData("AuditTrail.cs", MigrationSourceScan.DataWriteOutsideAllowedTables, 14)]
    [InlineData("AuditTrail.cs", MigrationSourceScan.DataWriteOutsideAllowedTables, 15)]
    [InlineData("AuditTrail.cs", MigrationSourceScan.DatabaseClock, 16)]
    [InlineData("AuditTrail.cs", MigrationSourceScan.PublicPrivilege, 17)]
    [InlineData("AuditTrail.cs", MigrationSourceScan.PublicPrivilege, 18)]
    public void Scan_OnViolationSample_ReportsTheRuleOnItsLine(string sample, string rule, int line)
    {
        IReadOnlyList<(string Rule, int Line)> findings = MigrationSourceScan.Scan(File.ReadAllText(SamplePath(sample)));

        Assert.Contains((rule, line), findings);
    }

    [Fact]
    public void Scan_OnCleanSample_ReportsNothing()
    {
        Assert.Empty(MigrationSourceScan.Scan(File.ReadAllText(SamplePath("Clean.cs"))));
    }

    private static string SamplePath(string fileName)
    {
        return Path.Combine(RepositoryRoot.Find(), "tests", "Shopfolio.UnitTests", "Architecture", "ViolationSamples", "Migrations", fileName);
    }

    private static bool IsBuildOutput(string path) =>
        path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal)
        || path.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.Ordinal);
}
