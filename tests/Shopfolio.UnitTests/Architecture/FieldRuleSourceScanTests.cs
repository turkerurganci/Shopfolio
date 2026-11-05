using Shopfolio.UnitTests.Support;

namespace Shopfolio.UnitTests.Architecture;

/// <summary>
/// The scan of the input validation pattern over the application source and on its deliberate violation sample (T211 criterion 8, KK7; 09 3.4 items 1 and 6;
/// 09 19.2 item 3): no number written on a limit attribute, and no client-side validation output (<c>data-val-*</c>, the switch, the scripts).
/// </summary>
public sealed class FieldRuleSourceScanTests
{
    [Fact]
    public void ApplicationSource_WritesNoNumberOnALimitRule_AndNoClientValidationOutput()
    {
        string root = RepositoryRoot.Find();
        List<string> scanned = [];
        List<string> findings = [];
        foreach (string file in Directory.EnumerateFiles(Path.Combine(root, "src"), "*", SearchOption.AllDirectories)
                     .Where(f => f.EndsWith(".cs", StringComparison.Ordinal) || f.EndsWith(".cshtml", StringComparison.Ordinal)))
        {
            string path = Path.GetRelativePath(root, file).Replace('\\', '/');
            if (path.Contains("/obj/", StringComparison.Ordinal) || path.Contains("/bin/", StringComparison.Ordinal))
            {
                continue;
            }

            scanned.Add(path);
            foreach ((string rule, int line) in FieldRuleSourceScan.Scan(File.ReadAllText(file)))
            {
                findings.Add($"{path}:{line} {rule}");
            }
        }

        // The file that turns the framework's output off names the switch with false; it is scanned like the rest.
        Assert.Contains("src/Shopfolio.Web/Infrastructure/Validation/FieldValidationSetup.cs", scanned);
        Assert.Contains("src/Shopfolio.Web/Program.cs", scanned);
        Assert.Empty(findings);
    }

    [Fact]
    public void Scan_OnViolationSample_ReportsEachRuleAndNotTheConstantTheFalseSwitchOrTheCommentLines()
    {
        string sample = File.ReadAllText(Path.Combine(
            RepositoryRoot.Find(), "tests", "Shopfolio.UnitTests", "Architecture", "ViolationSamples", "Validation", "NumberOnRule.cs"));

        IReadOnlyList<(string Rule, int Line)> findings = FieldRuleSourceScan.Scan(sample);

        Assert.Equal(
            [
                (FieldRuleSourceScan.NumberOnLimitRule, 5),
                (FieldRuleSourceScan.NumberOnLimitRule, 11),
                (FieldRuleSourceScan.NumberOnLimitRule, 14),
                (FieldRuleSourceScan.ClientValidationOn, 22),
                (FieldRuleSourceScan.ClientValidationOn, 30),
                (FieldRuleSourceScan.ClientValidationOn, 31),
            ],
            findings);
    }
}
