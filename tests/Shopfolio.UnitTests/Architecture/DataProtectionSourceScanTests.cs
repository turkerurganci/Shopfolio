using Shopfolio.UnitTests.Support;

namespace Shopfolio.UnitTests.Architecture;

/// <summary>
/// Runs the data protection scan over the application source and shows on a deliberate violation sample that each rule goes red (T232 KK1, KK3;
/// 09 15.5 items 1 and 2; 09 19.2 item 3). The scan reads files and never writes one.
/// </summary>
public sealed class DataProtectionSourceScanTests
{
    [Fact]
    public void ApplicationSource_SetsUpTheRingInOneFileAndWritesNoPurposeStringElsewhere()
    {
        string root = RepositoryRoot.Find();
        List<string> scanned = [];
        List<string> findings = [];
        foreach (string file in Directory.EnumerateFiles(Path.Combine(root, "src"), "*.cs", SearchOption.AllDirectories))
        {
            string relative = Path.GetRelativePath(root, file);
            string path = relative.Replace('\\', '/');
            if (path.Contains("/obj/", StringComparison.Ordinal) || path.Contains("/bin/", StringComparison.Ordinal))
            {
                continue;
            }

            scanned.Add(path);
            foreach ((string rule, int line) in DataProtectionSourceScan.Scan(relative, File.ReadAllText(file)))
            {
                findings.Add($"{path}:{line} {rule}");
            }
        }

        Assert.Contains(scanned, f => f.EndsWith("Program.cs", StringComparison.Ordinal));
        Assert.Contains(DataProtectionSourceScan.SetupFile, scanned);
        Assert.Contains(DataProtectionSourceScan.PurposesFile, scanned);
        Assert.Empty(findings);
    }

    [Fact]
    public void AllowedFiles_ExistAndAreTheOnesTheApplicationSourceHolds()
    {
        string root = RepositoryRoot.Find();

        Assert.True(File.Exists(Path.Combine(root, DataProtectionSourceScan.SetupFile)));
        Assert.True(File.Exists(Path.Combine(root, DataProtectionSourceScan.PurposesFile)));
    }

    [Fact]
    public void Scan_OnViolationSample_ReportsEachRuleAndNotTheCommentTheConstantOrAnotherString()
    {
        string sample = File.ReadAllText(Path.Combine(
            RepositoryRoot.Find(), "tests", "Shopfolio.UnitTests", "Architecture", "ViolationSamples", "DataProtection", "RingMisuse.cs"));

        IReadOnlyList<(string Rule, int Line)> findings = DataProtectionSourceScan.Scan("src/Shopfolio.Web/Modules/Catalog/Misuse.cs", sample);

        Assert.Equal(
            [
                (DataProtectionSourceScan.KeyEncryptor, 5),
                (DataProtectionSourceScan.KeyLifetime, 5),
                (DataProtectionSourceScan.SecondSetup, 5),
                (DataProtectionSourceScan.ProtectorPurpose, 6),
                (DataProtectionSourceScan.PurposeLiteral, 6),
                (DataProtectionSourceScan.ProtectorPurpose, 7),
                (DataProtectionSourceScan.ProtectorPurpose, 8),
                (DataProtectionSourceScan.PurposeLiteral, 13),
            ],
            findings);
    }

    [Fact]
    public void Scan_InTheSetupFile_AllowsTheSetupCallAndNothingElse()
    {
        const string text = "services.AddDataProtection().ProtectKeysWithDpapi();";

        IReadOnlyList<(string Rule, int Line)> findings = DataProtectionSourceScan.Scan(DataProtectionSourceScan.SetupFile, text);

        Assert.Equal([(DataProtectionSourceScan.KeyEncryptor, 1)], findings);
    }

    [Fact]
    public void Scan_InThePurposesFile_AllowsThePurposeStringsAndNothingElse()
    {
        const string text = "const string A = \"Shopfolio.DownloadLink.v1\"; p.CreateProtector(\"Shopfolio.DownloadLink.v1\");";

        IReadOnlyList<(string Rule, int Line)> findings = DataProtectionSourceScan.Scan(@"src\Shopfolio.Web\Infrastructure\Security\DataProtectionPurposes.cs", text);

        Assert.Equal([(DataProtectionSourceScan.ProtectorPurpose, 1)], findings);
    }
}
