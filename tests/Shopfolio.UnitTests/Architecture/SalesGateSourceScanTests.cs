using Shopfolio.UnitTests.Support;

namespace Shopfolio.UnitTests.Architecture;

/// <summary>
/// The sales gate's rules on the application source (T331 KK8; 11 T331 criterion 8; 09 19.2 item 3): the switch is named only by the entity, the section writer, the gate's
/// reader and the switch writer, and a gate is built only by its rule. Each rule goes red on a deliberate violation sample, in every form the scan claims to see.
/// </summary>
public sealed class SalesGateSourceScanTests
{
    private const string SamplePath = "src/Shopfolio.Web/Pages/Samples/SwitchReadAndGateConstruction.cs";

    private static string Sample() => File.ReadAllText(Path.Combine(RepositoryRoot.Find(), "tests", "Shopfolio.UnitTests", "Architecture", "ViolationSamples", "SalesGate", "SwitchReadAndGateConstruction.cs"));

    [Fact]
    public void ApplicationSource_NamesTheSwitchOnlyInTheGateFiles_AndBuildsAGateOnlyInTheRule()
    {
        string root = RepositoryRoot.Find();
        List<string> scanned = [];
        List<string> findings = [];
        foreach (string file in Directory.EnumerateFiles(Path.Combine(root, "src"), "*.*", SearchOption.AllDirectories))
        {
            string relative = Path.GetRelativePath(root, file).Replace('\\', '/');
            if (!SalesGateSourceScan.Applies(relative))
            {
                continue;
            }

            scanned.Add(relative);
            foreach ((string rule, int line) in SalesGateSourceScan.Scan(relative, File.ReadAllText(file)))
            {
                findings.Add($"{relative}:{line} {rule}");
            }
        }

        // The scan reached the files it names, the rule's file and a migration, so an empty result is not an empty scan.
        Assert.All(SalesGateSourceScan.SwitchFiles, f => Assert.Contains(f, scanned));
        Assert.Contains(SalesGateSourceScan.RuleFile, scanned);
        Assert.Contains(scanned, f => f.Contains("/Infrastructure/Migrations/", StringComparison.Ordinal));
        Assert.Empty(findings);
    }

    [Fact]
    public void TheSwitchFilesReallyNameTheSwitch_SoNoFileOfTheClosedListIsDead()
    {
        string root = RepositoryRoot.Find();

        foreach (string file in SalesGateSourceScan.SwitchFiles)
        {
            string text = File.ReadAllText(Path.Combine(root, file));
            Assert.Contains("IsSalesTemporarilyClosed", text, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void ViolationSample_EachRuleReportsItsLines_AndNotTheCommentsTheHiddenFieldOrTheNeighbourTypes()
    {
        IReadOnlyList<(string Rule, int Line)> findings = SalesGateSourceScan.Scan(SamplePath, Sample());

        Assert.Equal(
            [
                (SalesGateSourceScan.NamesTheSwitchOutsideTheGate, 5),
                (SalesGateSourceScan.NamesTheSwitchOutsideTheGate, 6),
                (SalesGateSourceScan.NamesTheSwitchOutsideTheGate, 7),
                (SalesGateSourceScan.NamesTheSwitchOutsideTheGate, 8),
                (SalesGateSourceScan.NamesTheSwitchOutsideTheGate, 9),
                (SalesGateSourceScan.BuildsAGateOutsideTheRule, 10),
                (SalesGateSourceScan.BuildsAGateOutsideTheRule, 11),
                (SalesGateSourceScan.BuildsAGateOutsideTheRule, 15),
            ],
            findings);
    }

    [Fact]
    public void TheGateFiles_MayNameTheSwitch_ButNotTheirNeighbours()
    {
        const string name = "var closed = row.IsSalesTemporarilyClosed;";

        foreach (string file in SalesGateSourceScan.SwitchFiles)
        {
            Assert.DoesNotContain(SalesGateSourceScan.Scan(file, name), f => f.Rule == SalesGateSourceScan.NamesTheSwitchOutsideTheGate);
        }

        Assert.Contains(SalesGateSourceScan.Scan("src/Shopfolio.Web/Modules/Settings/CompanyIdentities.cs", name), f => f.Rule == SalesGateSourceScan.NamesTheSwitchOutsideTheGate);
        Assert.Contains(SalesGateSourceScan.Scan("src/Shopfolio.Web/Modules/Settings/CompanySettingsWriter2.cs", name), f => f.Rule == SalesGateSourceScan.NamesTheSwitchOutsideTheGate);
        Assert.Contains(SalesGateSourceScan.Scan("src/Shopfolio.Web/Modules/Ordering/OrderingModule.cs", name), f => f.Rule == SalesGateSourceScan.NamesTheSwitchOutsideTheGate);
        Assert.Contains(SalesGateSourceScan.Scan("src/Shopfolio.Web/Pages/Panel/Sales.cshtml", name), f => f.Rule == SalesGateSourceScan.NamesTheSwitchOutsideTheGate);
        Assert.DoesNotContain(
            SalesGateSourceScan.Scan("src/Shopfolio.Web/Infrastructure/Migrations/20261101_Snapshot.cs", name),
            f => f.Rule == SalesGateSourceScan.NamesTheSwitchOutsideTheGate);
    }

    [Fact]
    public void TheRuleFile_MayBuildAGate_ButNoNeighbourMay()
    {
        const string build = "return new SalesGate(unmet, missing);";

        Assert.DoesNotContain(SalesGateSourceScan.Scan(SalesGateSourceScan.RuleFile, build), f => f.Rule == SalesGateSourceScan.BuildsAGateOutsideTheRule);
        Assert.Contains(SalesGateSourceScan.Scan("src/Shopfolio.Web/Modules/Settings/SalesGates.cs", build), f => f.Rule == SalesGateSourceScan.BuildsAGateOutsideTheRule);
        Assert.Contains(SalesGateSourceScan.Scan("src/Shopfolio.Web/Modules/Ordering/Cart.cs", build), f => f.Rule == SalesGateSourceScan.BuildsAGateOutsideTheRule);
    }

    [Theory]
    [InlineData("// var s = c.IsSalesTemporarilyClosed;")]
    [InlineData("/* c.IsSalesTemporarilyClosed */ var x = 1;")]
    [InlineData("var s = \"https://example.com\"; // IsSalesTemporarilyClosed")]
    [InlineData("var h = Original.OriginalIsSalesTemporarilyClosed;")]
    [InlineData("var s = new SalesGateFacts([], true, false, true, true, true);")]
    [InlineData("// var g = new SalesGate([], []);")]
    public void CommentsTheHiddenFieldAndTheNeighbourTypes_AreNoFindings(string line)
    {
        Assert.Empty(SalesGateSourceScan.Scan("src/Shopfolio.Web/Pages/Samples/Other.cs", line));
    }

    [Fact]
    public void ACallSplitOverLines_IsFound()
    {
        const string split = "var s = c.\n    IsSalesTemporarilyClosed;\nvar g = new\n    SalesGate\n    ([], []);";

        IReadOnlyList<(string Rule, int Line)> findings = SalesGateSourceScan.Scan("src/Shopfolio.Web/Pages/Samples/Split.cs", split);

        Assert.Contains((SalesGateSourceScan.NamesTheSwitchOutsideTheGate, 2), findings);
        Assert.Contains((SalesGateSourceScan.BuildsAGateOutsideTheRule, 3), findings);
    }
}
