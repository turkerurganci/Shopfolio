using Shopfolio.UnitTests.Support;

namespace Shopfolio.UnitTests.Architecture;

/// <summary>
/// Runs the data model source scan over the application source (09 10.1 items 1, 2; 5.2 item 3; 9.4 item 5) and shows on deliberate
/// violation samples that the scan goes red (09 19.2 item 3).
/// </summary>
public sealed class DataModelSourceScanTests
{
    [Fact]
    public void ApplicationSource_BreaksNoDataModelBan()
    {
        string root = RepositoryRoot.Find();
        string[] files = Directory.EnumerateFiles(Path.Combine(root, "src"), "*.cs", SearchOption.AllDirectories)
            .Where(f => !IsBuildOutput(f))
            .ToArray();

        Assert.Contains(files, f => Path.GetFileName(f) == DataModelSourceScan.EntityIdsFile);
        Assert.Contains(files, f => Path.GetFileName(f) == DataModelSourceScan.TextNormalizerFile);
        List<string> findings = [];
        foreach (string file in files)
        {
            foreach ((string rule, int line) in DataModelSourceScan.Scan(Path.GetFileName(file), File.ReadAllText(file)))
            {
                findings.Add($"{Path.GetRelativePath(root, file)}:{line} {rule}");
            }
        }

        Assert.Empty(findings);
    }

    [Theory]
    [InlineData("GuidFactory.cs", DataModelSourceScan.IdGeneration)]
    [InlineData("DatabaseIdDefault.cs", DataModelSourceScan.DatabaseIdDefault)]
    [InlineData("HandWrittenName.cs", DataModelSourceScan.HandWrittenName)]
    [InlineData("RestrictDelete.cs", DataModelSourceScan.ForbiddenDeleteBehavior)]
    [InlineData("TurkishLetter.cs", DataModelSourceScan.TurkishLetterConstant)]
    public void Scan_OnViolationSample_ReportsTheRuleOnItsLine(string sample, string rule)
    {
        string path = SamplePath(sample);

        IReadOnlyList<(string Rule, int Line)> findings = DataModelSourceScan.Scan(sample, File.ReadAllText(path));

        Assert.Equal((rule, 6), Assert.Single(findings));
    }

    [Fact]
    public void Scan_OnCleanSample_ReportsNothing()
    {
        Assert.Empty(DataModelSourceScan.Scan("Clean.cs", File.ReadAllText(SamplePath("Clean.cs"))));
    }

    [Fact]
    public void Scan_IdFactoryInEntityIdsFile_IsAllowed()
    {
        string text = File.ReadAllText(SamplePath("GuidFactory.cs"));

        Assert.Empty(DataModelSourceScan.Scan(DataModelSourceScan.EntityIdsFile, text));
    }

    [Fact]
    public void Scan_TurkishLetterInTextNormalizerFile_IsAllowed()
    {
        string text = File.ReadAllText(SamplePath("TurkishLetter.cs"));

        Assert.Empty(DataModelSourceScan.Scan(DataModelSourceScan.TextNormalizerFile, text));
    }

    private static string SamplePath(string fileName)
    {
        return Path.Combine(RepositoryRoot.Find(), "tests", "Shopfolio.UnitTests", "Architecture", "ViolationSamples", "DataModel", fileName);
    }

    private static bool IsBuildOutput(string path) =>
        path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal)
        || path.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.Ordinal);
}
