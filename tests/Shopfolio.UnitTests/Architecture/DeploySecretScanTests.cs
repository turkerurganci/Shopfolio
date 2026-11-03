using Shopfolio.UnitTests.Support;

namespace Shopfolio.UnitTests.Architecture;

/// <summary>
/// Runs the deploy file secret scans over the real files (05 6.x.1; 09 17 item 7) and shows on deliberate violation samples that
/// each scan goes red, for both forms of the Compose environment section (09 19.2 item 3).
/// </summary>
public sealed class DeploySecretScanTests
{
    [Theory]
    [InlineData("compose.yaml")]
    [InlineData("compose.install.yaml")]
    [InlineData("compose.dev.yaml")]
    public void ComposeFile_PutsNoSecretIntoAnEnvironmentSetting(string fileName)
    {
        string text = File.ReadAllText(Path.Combine(RepositoryRoot.Find(), "deploy", fileName));

        Assert.Empty(DeploySecretScan.ScanComposeEnvironment(fileName, text));
    }

    [Fact]
    public void ComposeEnvironmentScan_AllowsOnlyTheDevelopmentConnectionStringAsAnException()
    {
        // 09 4.3 item 2: the one deliberate exception, named with its file and its reason.
        (string File, string Name) only = Assert.Single(DeploySecretScan.AllowedEnvironmentSettings.Keys);

        Assert.Equal(("compose.dev.yaml", "Database__ConnectionString"), only);
    }

    [Fact]
    public void ComposeEnvironmentScan_OnTheConnectionStringOutsideTheDevelopmentOverlay_ReportsIt()
    {
        string text = File.ReadAllText(Path.Combine(RepositoryRoot.Find(), "deploy", "compose.dev.yaml"));

        // The development value is no exception in any other file: the same text under another file name goes red.
        Assert.NotEmpty(DeploySecretScan.ScanComposeEnvironment("compose.yaml", text));
    }

    [Theory]
    [InlineData("ComposeEnvironmentList.yaml")]
    [InlineData("ComposeEnvironmentMapping.yaml")]
    public void ComposeEnvironmentScan_OnViolationSample_ReportsTheRuleOnItsLine(string sample)
    {
        IReadOnlyList<(string Rule, int Line)> findings = DeploySecretScan.ScanComposeEnvironment(sample, File.ReadAllText(SamplePath(sample)));

        Assert.Equal((DeploySecretScan.SecretInEnvironment, 6), Assert.Single(findings));
    }

    [Fact]
    public void ComposeEnvironmentScan_OnCleanSample_ReportsNothing()
    {
        Assert.Empty(DeploySecretScan.ScanComposeEnvironment("ComposeClean.yaml", File.ReadAllText(SamplePath("ComposeClean.yaml"))));
    }

    [Fact]
    public void SettingsTemplate_CarriesNoSecretKeyAndNoSecretLikeValue()
    {
        string text = File.ReadAllText(Path.Combine(RepositoryRoot.Find(), "deploy", ".env.example"));

        Assert.Empty(DeploySecretScan.ScanSettingsTemplate(text));
        Assert.Contains("=", text, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("SettingsSecretKey.env", DeploySecretScan.SecretKey)]
    [InlineData("SettingsSecretValue.env", DeploySecretScan.SecretLikeValue)]
    public void SettingsTemplateScan_OnViolationSample_ReportsTheRuleOnItsLine(string sample, string rule)
    {
        IReadOnlyList<(string Rule, int Line)> findings = DeploySecretScan.ScanSettingsTemplate(File.ReadAllText(SamplePath(sample)));

        Assert.Equal((rule, 6), Assert.Single(findings));
    }

    [Fact]
    public void SettingsTemplateScan_OnCleanSample_ReportsNothing()
    {
        Assert.Empty(DeploySecretScan.ScanSettingsTemplate(File.ReadAllText(SamplePath("SettingsClean.env"))));
    }

    private static string SamplePath(string fileName)
    {
        return Path.Combine(RepositoryRoot.Find(), "tests", "Shopfolio.UnitTests", "Architecture", "ViolationSamples", "DeploySecrets", fileName);
    }
}
