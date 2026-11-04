using Shopfolio.UnitTests.Support;

namespace Shopfolio.UnitTests.Architecture;

/// <summary>
/// Runs the module skeleton source scans over the application (T218 KK6, KK9; 09 6 item 3; 05 3.4.3, 3.4.6) and shows on deliberate violation
/// samples that each goes red (09 19.2 item 3).
/// </summary>
public sealed class ModuleSourceScanTests
{
    [Fact]
    public void Program_RegistersEachModuleWithOnePlainCall()
    {
        string program = File.ReadAllText(Path.Combine(RepositoryRoot.Find(), "src", "Shopfolio.Web", "Program.cs"));

        Assert.Empty(ModuleSourceScan.ScanProgram(program));
    }

    [Fact]
    public void ScanProgram_OnCleanSample_ReportsNothing()
    {
        Assert.Empty(ModuleSourceScan.ScanProgram(File.ReadAllText(SamplePath("ProgramClean.cs"))));
    }

    [Theory]
    [InlineData(ModuleSourceScan.NotPlain, "Catalog", 3)]
    [InlineData(ModuleSourceScan.NotPlain, "Ordering", 3)]
    [InlineData(ModuleSourceScan.NotPlain, "Settings", 9)]
    [InlineData(ModuleSourceScan.Twice, "Catalog", 10)]
    [InlineData(ModuleSourceScan.Missing, "Membership", 0)]
    public void ScanProgram_OnViolationSample_ReportsTheRuleForTheModule(string rule, string module, int line)
    {
        IReadOnlyList<(string Rule, string Module, int Line)> findings = ModuleSourceScan.ScanProgram(File.ReadAllText(SamplePath("ProgramTwiceAndChained.cs")));

        Assert.Contains((rule, module, line), findings);
    }

    [Fact]
    public void Application_HasNoEventPublisher()
    {
        string root = RepositoryRoot.Find();
        List<string> findings = [];
        foreach (string file in SourceFiles(root))
        {
            foreach ((string rule, int line) in ModuleSourceScan.ScanEventPublisher(File.ReadAllText(file)))
            {
                findings.Add($"{Path.GetRelativePath(root, file)}:{line} {rule}");
            }
        }

        Assert.Empty(findings);
    }

    [Theory]
    [InlineData(3)]
    [InlineData(5)]
    [InlineData(6)]
    public void ScanEventPublisher_OnViolationSample_ReportsTheLine(int line)
    {
        IReadOnlyList<(string Rule, int Line)> findings = ModuleSourceScan.ScanEventPublisher(File.ReadAllText(SamplePath("EventPublisher.cs")));

        Assert.Contains((ModuleSourceScan.EventPublisher, line), findings);
        Assert.DoesNotContain(findings, f => f.Line <= 2);
    }

    private static IEnumerable<string> SourceFiles(string root)
    {
        return Directory.EnumerateFiles(Path.Combine(root, "src"), "*.cs", SearchOption.AllDirectories)
            .Where(f => !f.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal)
                && !f.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.Ordinal));
    }

    private static string SamplePath(string fileName)
    {
        return Path.Combine(RepositoryRoot.Find(), "tests", "Shopfolio.UnitTests", "Architecture", "ViolationSamples", "ModuleSource", fileName);
    }
}
