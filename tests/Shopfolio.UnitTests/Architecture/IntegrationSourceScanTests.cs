using Shopfolio.UnitTests.Support;

namespace Shopfolio.UnitTests.Architecture;

/// <summary>
/// Runs the link layer scan over the application source and the fakes, and shows on a deliberate violation sample that each rule goes red
/// (T202 KK1, KK2, KK4, KK9; 09 19.2 item 3). The scan reads files and never writes one.
/// </summary>
public sealed class IntegrationSourceScanTests
{
    private const string SamplePath = "Architecture/ViolationSamples/Integrations/ProviderLeak.cs";

    private static string Sample() => File.ReadAllText(Path.Combine(RepositoryRoot.Find(), "tests", "Shopfolio.UnitTests", SamplePath));

    [Fact]
    public void ApplicationSourceAndFakes_KeepTheProviderNamesTheClientsAndTheRetriesInsideTheLayer()
    {
        string root = RepositoryRoot.Find();
        List<string> scanned = [];
        List<string> findings = [];
        IEnumerable<string> files = Directory.EnumerateFiles(Path.Combine(root, "src"), "*.*", SearchOption.AllDirectories)
            .Concat(Directory.EnumerateFiles(Path.Combine(root, "tests", "Shopfolio.UnitTests", "Fakes"), "*.cs", SearchOption.AllDirectories))
            .Where(file => file.EndsWith(".cs", StringComparison.Ordinal) || file.EndsWith(".cshtml", StringComparison.Ordinal));
        foreach (string file in files)
        {
            string path = Path.GetRelativePath(root, file).Replace('\\', '/');
            if (path.Contains("/obj/", StringComparison.Ordinal) || path.Contains("/bin/", StringComparison.Ordinal))
            {
                continue;
            }

            scanned.Add(path);
            foreach ((string rule, int line) in IntegrationSourceScan.Scan(path, File.ReadAllText(file)))
            {
                findings.Add($"{path}:{line} {rule}");
            }
        }

        Assert.Contains(IntegrationSourceScan.LayerFolder + "IPaymentGateway.cs", scanned);
        Assert.Contains(IntegrationSourceScan.FakesFolder + "FakePaymentGateway.cs", scanned);
        Assert.Contains(scanned, f => f.StartsWith(IntegrationSourceScan.CallerFolders[0], StringComparison.Ordinal));
        Assert.Empty(findings);
    }

    [Fact]
    public void NamedFolders_Exist()
    {
        string root = RepositoryRoot.Find();

        Assert.True(Directory.Exists(Path.Combine(root, IntegrationSourceScan.LayerFolder)));
        Assert.True(Directory.Exists(Path.Combine(root, IntegrationSourceScan.FakesFolder)));
        Assert.All(IntegrationSourceScan.CallerFolders, folder => Assert.True(Directory.Exists(Path.Combine(root, folder)), folder));
    }

    [Fact]
    public void Scan_OnViolationSampleInAModule_ReportsEveryRuleOnItsLine()
    {
        IReadOnlyList<(string Rule, int Line)> findings = IntegrationSourceScan.Scan("src/Shopfolio.Web/Modules/Payment/Leak.cs", Sample());

        Assert.Equal(
            [
                (IntegrationSourceScan.ProviderNameOutsideLayer, 5),
                (IntegrationSourceScan.ProviderNameOutsideLayer, 6),
                (IntegrationSourceScan.ProviderNameOutsideLayer, 7),
                (IntegrationSourceScan.ProviderNameOutsideLayer, 8),
                (IntegrationSourceScan.ProviderNameOutsideLayer, 9),
                (IntegrationSourceScan.ProviderNameOutsideLayer, 10),
                (IntegrationSourceScan.ClientTypeOutsideLayer, 11),
                (IntegrationSourceScan.NewHttpClient, 12),
                (IntegrationSourceScan.ClientTypeOutsideLayer, 12),
                (IntegrationSourceScan.NewHttpClient, 13),
                (IntegrationSourceScan.ClientTypeOutsideLayer, 13),
                (IntegrationSourceScan.ClientTypeOutsideLayer, 14),
                (IntegrationSourceScan.RetryHandler, 15),
                (IntegrationSourceScan.RetryHandler, 16),
            ],
            findings);
    }

    [Theory]
    [InlineData("src/Shopfolio.Web/Infrastructure/Integrations/IPaymentGateway.cs")]
    [InlineData("src/Shopfolio.Web/Infrastructure/Integrations/PaymentStartResult.cs")]
    [InlineData("src/Shopfolio.Web/Infrastructure/Integrations/Payment/PayTrHelper.cs")]
    [InlineData("src/Shopfolio.Web/Infrastructure/PayTrPaymentGateway.cs")]
    [InlineData("src/Shopfolio.Web/Modules/Payment/PayTrPaymentGateway.cs")]
    public void Scan_OutsideTheImplementationFiles_ReportsTheProviderNames_ButNotTheClientTypesOutsideTheCallerFolders(string path)
    {
        IReadOnlyList<(string Rule, int Line)> findings = IntegrationSourceScan.Scan(path, Sample());

        Assert.Equal([5, 6, 7, 8, 9, 10], findings.Where(f => f.Rule == IntegrationSourceScan.ProviderNameOutsideLayer).Select(f => f.Line));
        Assert.Equal(path.Contains("/Modules/", StringComparison.Ordinal), findings.Any(f => f.Rule == IntegrationSourceScan.ClientTypeOutsideLayer));
        Assert.Equal([12, 13], findings.Where(f => f.Rule == IntegrationSourceScan.NewHttpClient).Select(f => f.Line));
    }

    [Theory]
    [InlineData("src/Shopfolio.Web/Infrastructure/Integrations/PayTrPaymentGateway.cs")]
    [InlineData("src/Shopfolio.Web/Infrastructure/Integrations/SesEmailGateway.cs")]
    public void Scan_InAnImplementationFile_AllowsTheProviderNames_ButNotANewClientNorARetry(string path)
    {
        IReadOnlyList<(string Rule, int Line)> findings = IntegrationSourceScan.Scan(path, Sample());

        Assert.Equal(
            [
                (IntegrationSourceScan.NewHttpClient, 12),
                (IntegrationSourceScan.NewHttpClient, 13),
                (IntegrationSourceScan.RetryHandler, 15),
                (IntegrationSourceScan.RetryHandler, 16),
            ],
            findings);
    }

    [Fact]
    public void Scan_OnViolationSampleAsAFake_ReportsTheNetworkTypes()
    {
        IReadOnlyList<(string Rule, int Line)> findings = IntegrationSourceScan.Scan(IntegrationSourceScan.FakesFolder + "Leak.cs", Sample());

        Assert.Equal(
            [
                (IntegrationSourceScan.FakeUsesNetwork, 11),
                (IntegrationSourceScan.FakeUsesNetwork, 12),
                (IntegrationSourceScan.FakeUsesNetwork, 13),
                (IntegrationSourceScan.FakeUsesNetwork, 14),
            ],
            findings);
    }

    [Theory]
    [InlineData("var p = \"paytr_token\";")]
    [InlineData("// the Paytr form")]
    [InlineData("/// <summary>Pay TR</summary>")]
    [InlineData("string s = \"Amazon\";")]
    [InlineData("SesClientFactory f;")]
    [InlineData("// SES rejects it")]
    [InlineData("services.UsePayTrGateway();")]
    [InlineData("const string Key = \"PAY-TR\";")]
    public void Scan_FindsAProviderNameInAnyCaseAndInComments(string line)
    {
        Assert.Equal([(IntegrationSourceScan.ProviderNameOutsideLayer, 1)], IntegrationSourceScan.Scan("src/Shopfolio.Web/Modules/Notifications/X.cs", line));
    }

    [Theory]
    [InlineData("Session session; PaymentTransaction t; string ses = \"x\";")]
    [InlineData("// display tree, repay track, the payment trail")]
    [InlineData("services.AddHttpClient(name).ConfigureHttpClient(c => c.Timeout = Timeout.InfiniteTimeSpan);")]
    [InlineData("var config = new { MaxErrorRetry = 0 };")]
    public void Scan_OnLookalikesOutsideTheCallerFolders_FindsNothing(string line)
    {
        Assert.Empty(IntegrationSourceScan.Scan("src/Shopfolio.Web/Infrastructure/Integrations/IntegrationHttpClients.cs", line));
    }
}
