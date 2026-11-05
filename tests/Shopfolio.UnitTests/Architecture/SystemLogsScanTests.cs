using Shopfolio.UnitTests.Support;

namespace Shopfolio.UnitTests.Architecture;

/// <summary>
/// The two scans of the system logs task (T225; 05 9.1.7, 9.2.1, 9.2.2), each over the real files and on a deliberate violation sample (09 19.2
/// item 3). KK6: the code of the logging block deletes no log file and writes no disposal record (the disposal job's deletion and its record are
/// tested in T201). KK7: no metric package or collection and dashboard stack in the dependencies and the deployment files.
/// </summary>
public sealed class SystemLogsScanTests
{
    // --- KK6 ---

    [Fact]
    public void LoggingFolder_DeletesNoLogFileAndWritesNoDisposalRecord()
    {
        string root = RepositoryRoot.Find();
        List<string> scanned = [];
        List<string> findings = [];
        foreach (string file in Directory.EnumerateFiles(Path.Combine(root, "src"), "*.cs", SearchOption.AllDirectories))
        {
            string relative = Path.GetRelativePath(root, file);
            if (!LogFileDeletionSourceScan.AppliesTo(relative))
            {
                continue;
            }

            scanned.Add(relative);
            findings.AddRange(LogFileDeletionSourceScan.Scan(File.ReadAllText(file)).Select(f => $"{relative}:{f.Line} {f.Rule}"));
        }

        Assert.Contains(scanned, f => f.EndsWith("LoggingSetup.cs", StringComparison.Ordinal));
        Assert.Contains(scanned, f => f.EndsWith("AuditLogWriter.cs", StringComparison.Ordinal));
        Assert.Empty(findings);
    }

    [Fact]
    public void LogFileDeletionScan_OnViolationSample_ReportsEachFormAndNotTheClosedLimitsOrTheComments()
    {
        IReadOnlyList<(string Rule, int Line)> findings = LogFileDeletionSourceScan.Scan(Sample("LogFileDeletion.cs"));

        Assert.Equal(
            [
                (LogFileDeletionSourceScan.FileDeleted, 8),
                (LogFileDeletionSourceScan.FileDeleted, 9),
                (LogFileDeletionSourceScan.FileDeleted, 10),
                (LogFileDeletionSourceScan.FileOverwritten, 11),
                (LogFileDeletionSourceScan.FileOverwritten, 12),
                (LogFileDeletionSourceScan.LibraryLimitOpen, 13),
                (LogFileDeletionSourceScan.DisposalRecordWritten, 15),
            ],
            findings);
    }

    [Fact]
    public void LogFileDeletionScan_AppliesToTheLoggingFolderOnly()
    {
        Assert.True(LogFileDeletionSourceScan.AppliesTo("src/Shopfolio.Web/Infrastructure/Logging/AuditLogWriter.cs"));
        Assert.True(LogFileDeletionSourceScan.AppliesTo(@"src\Shopfolio.Web\Infrastructure\Logging\AuditLogWriter.cs"));
        Assert.False(LogFileDeletionSourceScan.AppliesTo("src/Shopfolio.Web/Infrastructure/Disposal/DisposalRecord.cs"));
    }

    // --- KK7 ---

    [Fact]
    public void DependenciesAndDeploymentFiles_NameNoMetricCollectionOrDashboard()
    {
        string root = RepositoryRoot.Find();
        List<string> files =
        [
            Path.Combine(root, "Directory.Packages.props"),
            Path.Combine(root, "src", "Shopfolio.Web", "Shopfolio.Web.csproj"),
            Path.Combine(root, "src", "Shopfolio.Web", "packages.lock.json"),
            .. Directory.EnumerateFiles(Path.Combine(root, "deploy"), "compose*.yaml"),
            .. Directory.EnumerateFiles(Path.Combine(root, "deploy", "images"), "Dockerfile", SearchOption.AllDirectories),
            Path.Combine(root, "deploy", "caddy", "Caddyfile"),
        ];
        List<string> findings = [];
        foreach (string file in files)
        {
            Assert.True(File.Exists(file), $"{file} was not found.");
            findings.AddRange(MetricsPackageScan.Scan(File.ReadAllText(file)).Select(f => $"{Path.GetRelativePath(root, file)}:{f.Line} {f.Rule}"));
        }

        Assert.True(files.Count >= 8, "The scan lost files.");
        Assert.Empty(findings);
    }

    public static TheoryData<string> RefusedNames => [.. MetricsPackageScan.Names];

    [Theory]
    [MemberData(nameof(RefusedNames))]
    public void MetricsScan_OnAPackageLineOfEachRefusedName_Reports(string name)
    {
        string line = $"    <PackageVersion Include=\"{name}\" Version=\"1.0.0\" />";

        IReadOnlyList<(string Rule, int Line)> findings = MetricsPackageScan.Scan("<Project>\n" + line + "\n</Project>");

        Assert.Equal([(MetricsPackageScan.MetricsOrDashboardName, 2)], findings);
    }

    [Fact]
    public void MetricsScan_OnCommentsAndOnWordsThatOnlyContainAName_ReportsNothing()
    {
        string text = "# prometheus is not added\n<!-- Grafana is not added -->\n    image: caddy:2\n    <PackageVersion Include=\"Lokinet\" Version=\"1\" />";

        Assert.Empty(MetricsPackageScan.Scan(text));
    }

    [Fact]
    public void MetricsScan_OnComposeImagesOfTheCollectionStack_Reports()
    {
        IReadOnlyList<(string Rule, int Line)> findings = MetricsPackageScan.Scan("services:\n  pm:\n    image: prom/prometheus:v3\n  dash:\n    image: grafana/grafana:11\n");

        Assert.Equal([(MetricsPackageScan.MetricsOrDashboardName, 3), (MetricsPackageScan.MetricsOrDashboardName, 5)], findings);
    }

    private static string Sample(string name) =>
        File.ReadAllText(Path.Combine(RepositoryRoot.Find(), "tests", "Shopfolio.UnitTests", "Architecture", "ViolationSamples", "Logging", name));
}
