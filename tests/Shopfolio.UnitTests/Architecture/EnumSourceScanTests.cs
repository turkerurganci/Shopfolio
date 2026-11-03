using Shopfolio.UnitTests.Support;

namespace Shopfolio.UnitTests.Architecture;

/// <summary>
/// Runs the enum file scan over the application source (T102 KK6; 06 5.1.3) and shows on deliberate violation samples that the scan
/// goes red (09 19.2 item 3).
/// </summary>
public sealed class EnumSourceScanTests
{
    [Fact]
    public void EnumFiles_OfTheApplication_HoldNothingButTheCodeNames()
    {
        string root = RepositoryRoot.Find();
        string[] files = Directory.EnumerateFiles(Path.Combine(root, "src"), "*.cs", SearchOption.AllDirectories)
            .Where(f => !IsBuildOutput(f))
            .Where(f => EnumSourceScan.Applies(Path.GetRelativePath(root, f), File.ReadAllText(f)))
            .ToArray();

        Assert.True(files.Length >= 40, "The scan found fewer enum files than the data model defines.");
        List<string> findings = [];
        foreach (string file in files)
        {
            foreach ((string rule, int line) in EnumSourceScan.Scan(Path.GetFileName(file), File.ReadAllText(file)))
            {
                findings.Add($"{Path.GetRelativePath(root, file)}:{line} {rule}");
            }
        }

        Assert.Empty(findings);
    }

    [Theory]
    [InlineData("EnumWithMethod.cs", EnumSourceScan.ForeignLine, 11)]
    [InlineData("EnumWithValues.cs", EnumSourceScan.ForeignLine, 8)]
    [InlineData("EnumWithAttribute.cs", EnumSourceScan.ForeignLine, 6)]
    [InlineData("EnumWithBaseType.cs", EnumSourceScan.ForeignLine, 9)]
    public void Scan_OnViolationSample_ReportsTheRuleOnItsLine(string sample, string rule, int line)
    {
        IReadOnlyList<(string Rule, int Line)> findings = EnumSourceScan.Scan(sample, File.ReadAllText(SamplePath(sample)));

        Assert.Contains((rule, line), findings);
    }

    [Fact]
    public void Applies_ToAnEnumDeclaredWithABaseType()
    {
        string text = File.ReadAllText(SamplePath("EnumWithBaseType.cs"));

        Assert.True(EnumSourceScan.Applies("src/Shopfolio.Web/Modules/Sample/Contracts/EnumWithBaseType.cs", text));
    }

    [Fact]
    public void Scan_OnAFileNamedDifferentlyFromItsEnum_ReportsIt()
    {
        IReadOnlyList<(string Rule, int Line)> findings = EnumSourceScan.Scan("Other.cs", File.ReadAllText(SamplePath("Clean.cs")));

        Assert.Equal((EnumSourceScan.WrongFileName, 1), Assert.Single(findings));
    }

    [Fact]
    public void Scan_OnCleanSample_ReportsNothing()
    {
        Assert.Empty(EnumSourceScan.Scan("Clean.cs", File.ReadAllText(SamplePath("Clean.cs"))));
    }

    private static string SamplePath(string fileName)
    {
        return Path.Combine(RepositoryRoot.Find(), "tests", "Shopfolio.UnitTests", "Architecture", "ViolationSamples", "EnumFiles", fileName);
    }

    private static bool IsBuildOutput(string path) =>
        path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal)
        || path.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.Ordinal);
}
