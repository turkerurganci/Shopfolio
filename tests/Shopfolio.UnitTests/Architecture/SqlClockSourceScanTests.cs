using Shopfolio.UnitTests.Support;

namespace Shopfolio.UnitTests.Architecture;

/// <summary>
/// Runs the SQL clock scan over the application's source outside the migrations (T227; 09 7.1 items 1, 5) and shows on a deliberate
/// violation sample that it goes red (09 19.2 item 3).
/// </summary>
public sealed class SqlClockSourceScanTests
{
    [Fact]
    public void ApplicationSource_OutsideMigrations_ReadsNoDatabaseClock()
    {
        string root = RepositoryRoot.Find();
        List<string> scanned = [];
        List<string> findings = [];
        foreach (string file in Directory.EnumerateFiles(Path.Combine(root, "src"), "*.cs", SearchOption.AllDirectories))
        {
            string relative = Path.GetRelativePath(root, file);
            if (IsBuildOutput(relative) || !SqlClockSourceScan.Applies(relative))
            {
                continue;
            }

            scanned.Add(relative);
            foreach ((string rule, int line) in SqlClockSourceScan.Scan(File.ReadAllText(file)))
            {
                findings.Add($"{relative}:{line} {rule}");
            }
        }

        Assert.Contains(scanned, f => f.EndsWith("Program.cs", StringComparison.Ordinal));
        Assert.Empty(findings);
    }

    [Fact]
    public void Applies_LeavesOnlyTheMigrationsToTheirOwnScan()
    {
        Assert.False(SqlClockSourceScan.Applies("src/Shopfolio.Web/Infrastructure/Migrations/20261104011340_CreateAdminInvitationAndAuditLogTables.cs"));
        Assert.True(SqlClockSourceScan.Applies("src/Shopfolio.Web/Infrastructure/Jobs/Job.cs"));
        Assert.True(SqlClockSourceScan.Applies("src/Shopfolio.Web/Modules/Ordering/OrderingModule.cs"));
    }

    [Fact]
    public void Scan_OnViolationSample_ReportsEachClockReadAndNotTheCommentOrParameter()
    {
        string sample = File.ReadAllText(Path.Combine(
            RepositoryRoot.Find(), "tests", "Shopfolio.UnitTests", "Architecture", "ViolationSamples", "SqlClock", "SqlClockQuery.cs"));

        IReadOnlyList<(string Rule, int Line)> findings = SqlClockSourceScan.Scan(sample);

        Assert.Equal(
            [(SqlClockSourceScan.DatabaseClock, 5), (SqlClockSourceScan.DatabaseClock, 6), (SqlClockSourceScan.DatabaseClock, 7)],
            findings);
    }

    private static bool IsBuildOutput(string relativePath)
    {
        string path = relativePath.Replace('\\', '/');
        return path.Contains("/obj/", StringComparison.Ordinal) || path.Contains("/bin/", StringComparison.Ordinal);
    }
}
