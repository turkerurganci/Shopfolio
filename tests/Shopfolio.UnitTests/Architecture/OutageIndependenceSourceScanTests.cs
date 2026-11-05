using Shopfolio.UnitTests.Support;

namespace Shopfolio.UnitTests.Architecture;

/// <summary>
/// Runs the outage independence scan over the application source and shows on a deliberate violation sample that each rule goes red
/// (T204 KK7; 08 3.3; 09 19.2 item 3). The scan reads files and never writes one.
/// </summary>
public sealed class OutageIndependenceSourceScanTests
{
    private const string SamplePath = "Architecture/ViolationSamples/Outages/DeadlineWaitsForGateway.cs";

    private static string Sample() => File.ReadAllText(Path.Combine(RepositoryRoot.Find(), "tests", "Shopfolio.UnitTests", SamplePath));

    [Fact]
    public void ApplicationSource_NamesTheEmailAndGoogleGatewaysOnlyInTheirOwnFolders()
    {
        string root = RepositoryRoot.Find();
        List<string> scanned = [];
        List<string> findings = [];
        IEnumerable<string> files = Directory.EnumerateFiles(Path.Combine(root, "src"), "*.*", SearchOption.AllDirectories)
            .Where(file => file.EndsWith(".cs", StringComparison.Ordinal) || file.EndsWith(".cshtml", StringComparison.Ordinal));
        foreach (string file in files)
        {
            string path = Path.GetRelativePath(root, file).Replace('\\', '/');
            if (path.Contains("/obj/", StringComparison.Ordinal) || path.Contains("/bin/", StringComparison.Ordinal))
            {
                continue;
            }

            scanned.Add(path);
            foreach ((string rule, int line) in OutageIndependenceSourceScan.Scan(path, File.ReadAllText(file)))
            {
                findings.Add($"{path}:{line} {rule}");
            }
        }

        Assert.Contains(OutageIndependenceSourceScan.LayerFolder + "IEmailGateway.cs", scanned);
        Assert.Contains(OutageIndependenceSourceScan.NotificationsFolder + "DispatchAttempts.cs", scanned);
        Assert.Contains(scanned, f => f.StartsWith("src/Shopfolio.Web/Infrastructure/Jobs/", StringComparison.Ordinal));
        Assert.Empty(findings);
    }

    [Fact]
    public void NamedFolders_Exist()
    {
        string root = RepositoryRoot.Find();

        Assert.True(Directory.Exists(Path.Combine(root, OutageIndependenceSourceScan.LayerFolder)));
        Assert.True(Directory.Exists(Path.Combine(root, OutageIndependenceSourceScan.NotificationsFolder)));
        Assert.True(Directory.Exists(Path.Combine(root, OutageIndependenceSourceScan.MembershipFolder)));
    }

    [Theory]
    [InlineData("src/Shopfolio.Web/Modules/Orders/DeadlineWaitsForGateway.cs")]
    [InlineData("src/Shopfolio.Web/Infrastructure/Jobs/DeadlineWaitsForGateway.cs")]
    [InlineData("src/Shopfolio.Web/Pages/Checkout/Index.cshtml")]
    [InlineData("src/Shopfolio.Web/Modules/NotificationsArchive/DeadlineWaitsForGateway.cs")]
    public void Scan_OnViolationSampleOutsideBothFolders_ReportsEveryGatewayOnItsLine(string path)
    {
        Assert.Equal(
            [
                (OutageIndependenceSourceScan.EmailGatewayOutsideItsFolders, 5),
                (OutageIndependenceSourceScan.EmailGatewayOutsideItsFolders, 6),
                (OutageIndependenceSourceScan.GoogleGatewayOutsideItsFolders, 7),
                (OutageIndependenceSourceScan.GoogleGatewayOutsideItsFolders, 8),
                (OutageIndependenceSourceScan.EmailGatewayOutsideItsFolders, 8),
                (OutageIndependenceSourceScan.EmailGatewayOutsideItsFolders, 9),
            ],
            OutageIndependenceSourceScan.Scan(path, Sample()));
    }

    [Fact]
    public void Scan_InTheNotificationsModule_AllowsTheEmailGateway_ButNotGoogle()
    {
        Assert.Equal(
            [
                (OutageIndependenceSourceScan.GoogleGatewayOutsideItsFolders, 7),
                (OutageIndependenceSourceScan.GoogleGatewayOutsideItsFolders, 8),
            ],
            OutageIndependenceSourceScan.Scan(OutageIndependenceSourceScan.NotificationsFolder + "DispatchAttempts.cs", Sample()));
    }

    [Fact]
    public void Scan_InTheMembershipModule_AllowsGoogle_ButNotTheEmailGateway()
    {
        Assert.Equal(
            [
                (OutageIndependenceSourceScan.EmailGatewayOutsideItsFolders, 5),
                (OutageIndependenceSourceScan.EmailGatewayOutsideItsFolders, 6),
                (OutageIndependenceSourceScan.EmailGatewayOutsideItsFolders, 8),
                (OutageIndependenceSourceScan.EmailGatewayOutsideItsFolders, 9),
            ],
            OutageIndependenceSourceScan.Scan(OutageIndependenceSourceScan.MembershipFolder + "GoogleSignIn.cs", Sample()));
    }

    [Fact]
    public void Scan_InTheLinkLayer_AllowsBothGateways()
    {
        Assert.Empty(OutageIndependenceSourceScan.Scan(OutageIndependenceSourceScan.LayerFolder + "IntegrationRegistration.cs", Sample()));
    }

    [Theory]
    [InlineData("// waits for IEmailGateway")]
    [InlineData("/// <see cref=\"IGoogleIdentityGateway\"/>")]
    [InlineData("EmailGatewayOptions options; GoogleIdentityResult result; SendEmail(to);")]
    public void Scan_OnCommentsAndLookalikes_FindsNothing(string line)
    {
        Assert.Empty(OutageIndependenceSourceScan.Scan("src/Shopfolio.Web/Modules/Orders/X.cs", line));
    }
}
