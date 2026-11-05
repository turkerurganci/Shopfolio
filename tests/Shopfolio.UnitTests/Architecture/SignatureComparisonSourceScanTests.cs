using Shopfolio.UnitTests.Support;

namespace Shopfolio.UnitTests.Architecture;

/// <summary>
/// Runs the signature comparison scan over the link layer and the signed endpoints' pipeline, and shows on a deliberate violation sample that
/// every early-exit form goes red and the fixed-time call does not (T203 KK2; 09 19.2 item 3). The scan reads files and never writes one.
/// </summary>
public sealed class SignatureComparisonSourceScanTests
{
    private const string SamplePath = "Architecture/ViolationSamples/SignatureComparison/EarlyExitComparisons.cs";

    [Fact]
    public void LinkLayerAndSignedEndpoints_CompareSignaturesOnlyInFixedTime()
    {
        string root = RepositoryRoot.Find();
        List<string> scanned = [];
        List<string> findings = [];
        foreach (string file in Directory.EnumerateFiles(Path.Combine(root, "src"), "*.cs", SearchOption.AllDirectories))
        {
            string path = Path.GetRelativePath(root, file).Replace('\\', '/');
            if (!SignatureComparisonSourceScan.Applies(path))
            {
                continue;
            }

            scanned.Add(path);
            findings.AddRange(SignatureComparisonSourceScan.Scan(File.ReadAllText(file)).Select(f => $"{path}:{f.Line} {f.Rule}"));
        }

        Assert.Contains(SignatureComparisonSourceScan.ScannedFolders[0] + "NotificationVerification.cs", scanned);
        Assert.Contains(SignatureComparisonSourceScan.ScannedFolders[1] + "SignedEndpointHandler.cs", scanned);
        Assert.DoesNotContain(SignatureComparisonSourceScan.ComparisonFile, scanned);
        Assert.Empty(findings);
    }

    [Fact]
    public void TheComparisonFile_IsTheOneThatCallsTheFixedTimeFunction()
    {
        string text = File.ReadAllText(Path.Combine(RepositoryRoot.Find(), SignatureComparisonSourceScan.ComparisonFile));

        Assert.Contains("CryptographicOperations.FixedTimeEquals(", text, StringComparison.Ordinal);
    }

    [Fact]
    public void Scan_OnTheViolationSample_ReportsEveryEarlyExitForm_AndNotTheFixedTimeCallOrTheComments()
    {
        string sample = File.ReadAllText(Path.Combine(RepositoryRoot.Find(), "tests", "Shopfolio.UnitTests", SamplePath));

        IReadOnlyList<(string Rule, int Line)> findings = SignatureComparisonSourceScan.Scan(sample);

        Assert.All(findings, f => Assert.Equal(SignatureComparisonSourceScan.EarlyExitComparison, f.Rule));
        Assert.Equal([8, 10, 12, 14, 16, 18, 20, 22, 26], findings.Select(f => f.Line));
    }

    [Theory]
    [InlineData("src/Shopfolio.Web/Infrastructure/Integrations/PayTrPaymentGateway.cs", true)]
    [InlineData("src/Shopfolio.Web/Infrastructure/SignedEndpoints/SignedEndpointHandler.cs", true)]
    [InlineData("src/Shopfolio.Web/Infrastructure/Integrations/SignatureComparison.cs", false)]
    [InlineData("src/Shopfolio.Web/Infrastructure/Integrations/Sub/SignatureComparison.cs", true)]
    [InlineData("src/Shopfolio.Web/Infrastructure/Security/LinkRecords.cs", false)]
    public void Applies_ToTheTwoFolders_ExceptTheComparisonFileItself(string path, bool applies)
    {
        Assert.Equal(applies, SignatureComparisonSourceScan.Applies(path));
    }
}
