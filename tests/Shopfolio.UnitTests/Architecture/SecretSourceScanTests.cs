using Shopfolio.UnitTests.Support;

namespace Shopfolio.UnitTests.Architecture;

/// <summary>
/// Runs the secret source scan over the application source (05 6.x.1: no secret from the environment) and shows on deliberate
/// violation samples that the scan goes red (09 19.2 item 3).
/// </summary>
public sealed class SecretSourceScanTests
{
    [Fact]
    public void ApplicationSource_ReadsNoSecretFromTheEnvironment()
    {
        string root = RepositoryRoot.Find();
        string[] files = Directory.EnumerateFiles(Path.Combine(root, "src"), "*.cs", SearchOption.AllDirectories)
            .Where(f => !IsBuildOutput(f))
            .ToArray();

        Assert.Contains(files, f => f.EndsWith("Program.cs", StringComparison.Ordinal));
        List<string> findings = [];
        foreach (string file in files)
        {
            foreach ((string rule, int line) in SecretSourceScan.Scan(File.ReadAllText(file)))
            {
                findings.Add($"{Path.GetRelativePath(root, file)}:{line} {rule}");
            }
        }

        Assert.Empty(findings);
    }

    [Theory]
    [InlineData("EnvironmentRead.cs", SecretSourceScan.EnvironmentRead)]
    [InlineData("EnvironmentReadAll.cs", SecretSourceScan.EnvironmentRead)]
    [InlineData("EnvironmentConfiguration.cs", SecretSourceScan.EnvironmentConfiguration)]
    [InlineData("DatabasePasswordVariable.cs", SecretSourceScan.DatabasePasswordVariable)]
    public void Scan_OnViolationSample_ReportsTheRuleOnItsLine(string sample, string rule)
    {
        string path = SamplePath(sample);

        IReadOnlyList<(string Rule, int Line)> findings = SecretSourceScan.Scan(File.ReadAllText(path));

        Assert.Equal((rule, 6), Assert.Single(findings));
    }

    [Fact]
    public void Scan_OnCleanSample_ReportsNothing()
    {
        string path = SamplePath("Clean.cs");

        Assert.Empty(SecretSourceScan.Scan(File.ReadAllText(path)));
    }

    private static string SamplePath(string fileName)
    {
        return Path.Combine(RepositoryRoot.Find(), "tests", "Shopfolio.UnitTests", "Architecture", "ViolationSamples", "SecretSources", fileName);
    }

    private static bool IsBuildOutput(string path) =>
        path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal)
        || path.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.Ordinal);
}
