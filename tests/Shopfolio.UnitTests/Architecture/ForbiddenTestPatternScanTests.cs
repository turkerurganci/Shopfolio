using Shopfolio.UnitTests.Support;

namespace Shopfolio.UnitTests.Architecture;

/// <summary>
/// Runs the forbidden test pattern scan over the test projects (09 19.2 item 6) and shows on deliberate violation samples
/// that the scan goes red (09 19.2 item 3).
/// </summary>
public sealed class ForbiddenTestPatternScanTests
{
    private static readonly string[] _scannedExtensions = [".cs", ".csproj", ".props", ".targets", ".json", ".runsettings", ".config", ".xml"];

    [Fact]
    public void TestProjects_Sources_HaveNoForbiddenPatterns()
    {
        List<string> findings = [];
        foreach (string file in TestProjectFiles())
        {
            foreach ((string rule, int line) in ForbiddenTestPatterns.Scan(file, File.ReadAllText(file)))
            {
                findings.Add($"{Path.GetRelativePath(RepositoryRoot.Find(), file)}:{line} {rule}");
            }
        }

        Assert.Empty(findings);
    }

    [Fact]
    public void TestProjectFiles_AreFoundInBothTestProjects()
    {
        List<string> files = TestProjectFiles().ToList();

        Assert.Contains(files, f => f.Contains("Shopfolio.UnitTests", StringComparison.Ordinal) && f.EndsWith(".cs", StringComparison.Ordinal));
        Assert.Contains(files, f => f.Contains("Shopfolio.IntegrationTests", StringComparison.Ordinal) && f.EndsWith(".cs", StringComparison.Ordinal));
        Assert.DoesNotContain(files, f => f.Contains("ViolationSamples", StringComparison.Ordinal));
    }

    [Fact]
    public void Scan_OnViolationSample_ReportsEveryRuleOnce()
    {
        string path = SamplePath("ForbiddenTests.cs");

        IReadOnlyList<(string Rule, int Line)> findings = ForbiddenTestPatterns.Scan(path, File.ReadAllText(path));

        string[] rules = findings.Select(f => f.Rule).Order(StringComparer.Ordinal).ToArray();
        string[] expected =
        [
            ForbiddenTestPatterns.AlwaysTrue,
            ForbiddenTestPatterns.ExplicitTest,
            ForbiddenTestPatterns.NoAssertion,
            ForbiddenTestPatterns.OnlyNotNull,
            ForbiddenTestPatterns.RuntimeSkip,
            ForbiddenTestPatterns.SkippedTest,
        ];
        Assert.Equal(expected.Order(StringComparer.Ordinal).ToArray(), rules);
    }

    [Fact]
    public void Scan_OnSkipSample_PointsAtTheSkipLine()
    {
        string path = SamplePath("ForbiddenTests.cs");

        IReadOnlyList<(string Rule, int Line)> findings = ForbiddenTestPatterns.Scan(path, File.ReadAllText(path));

        (string Rule, int Line) skipped = Assert.Single(findings, f => f.Rule == ForbiddenTestPatterns.SkippedTest);
        Assert.Equal(8, skipped.Line);
    }

    [Fact]
    public void Scan_OnCleanSample_ReportsNothing()
    {
        string path = SamplePath("CleanTests.cs");

        IReadOnlyList<(string Rule, int Line)> findings = ForbiddenTestPatterns.Scan(path, File.ReadAllText(path));

        Assert.Empty(findings);
    }

    [Fact]
    public void Scan_OnRunnerFilterSample_ReportsTheFilter()
    {
        string path = SamplePath("RunnerFilter.runsettings");

        IReadOnlyList<(string Rule, int Line)> findings = ForbiddenTestPatterns.Scan(path, File.ReadAllText(path));

        (string Rule, int Line) finding = Assert.Single(findings);
        Assert.Equal(ForbiddenTestPatterns.RunnerFilter, finding.Rule);
    }

    private static string SamplePath(string fileName)
    {
        return Path.Combine(RepositoryRoot.Find(), "tests", "Shopfolio.UnitTests", "Architecture", "ViolationSamples", "ForbiddenPatterns", fileName);
    }

    private static IEnumerable<string> TestProjectFiles()
    {
        string tests = Path.Combine(RepositoryRoot.Find(), "tests");
        return Directory.EnumerateFiles(tests, "*", SearchOption.AllDirectories)
            .Where(file => _scannedExtensions.Contains(Path.GetExtension(file), StringComparer.OrdinalIgnoreCase))
            .Where(file => !IsUnder(file, "bin") && !IsUnder(file, "obj") && !IsUnder(file, "ViolationSamples"))
            .Where(file => !file.EndsWith("packages.lock.json", StringComparison.Ordinal));
    }

    private static bool IsUnder(string file, string directoryName)
    {
        string separator = Path.DirectorySeparatorChar.ToString();
        return file.Contains($"{separator}{directoryName}{separator}", StringComparison.Ordinal);
    }
}
