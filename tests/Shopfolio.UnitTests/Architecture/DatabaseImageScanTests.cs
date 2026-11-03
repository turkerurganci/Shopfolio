using Shopfolio.UnitTests.Support;

namespace Shopfolio.UnitTests.Architecture;

/// <summary>
/// Runs the database image scans over the real setup files (09 10.8 item 1 and 7, 17 item 1 and 2; 05 6.x.16) and shows on deliberate
/// violation samples that each rule goes red (09 19.2 item 3). The scans read files; they touch no database and no network.
/// </summary>
public sealed class DatabaseImageScanTests
{
    private static string ImageFile(params string[] parts) => Path.Combine([RepositoryRoot.Find(), "deploy", "images", "database", .. parts]);

    private static string Sample(string name) => File.ReadAllText(Path.Combine(RepositoryRoot.Find(), "tests", "Shopfolio.UnitTests", "Architecture", "ViolationSamples", "DatabaseImage", name));

    [Fact]
    public void ClientAuthenticationTemplate_HasNoTrustLineAndNoLineOpenToEveryUser()
    {
        Assert.Empty(DatabaseImageScan.ScanHba(File.ReadAllText(ImageFile("bootstrap", "pg_hba.conf"))));
    }

    [Fact]
    public void ClientAuthenticationTemplate_NamesTheBackupStatusUserOnlyOnceAndOnlyWithTheDatabase()
    {
        string[] lines = File.ReadAllLines(ImageFile("bootstrap", "pg_hba.conf")).Where(line => !line.TrimStart().StartsWith('#')).ToArray();

        string statusLine = Assert.Single(lines, line => line.Contains("shopfolio_backup_status", StringComparison.Ordinal));
        string[] fields = statusLine.Split([' ', '\t'], StringSplitOptions.RemoveEmptyEntries);

        Assert.Equal(["local", "@DATABASE@", "shopfolio_backup_status"], fields[..3]);
    }

    [Theory]
    [InlineData("HbaTrust.conf", DatabaseImageScan.HbaTrust)]
    [InlineData("HbaAllUsers.conf", DatabaseImageScan.HbaAllUsers)]
    [InlineData("HbaStatusUserEverywhere.conf", DatabaseImageScan.HbaStatusUserScope)]
    [InlineData("HbaFounderOverNetwork.conf", DatabaseImageScan.HbaFounderOverNetwork)]
    public void ClientAuthenticationScan_OnViolationSample_ReportsTheRuleOnItsLine(string sample, string rule)
    {
        Assert.Equal((rule, 5), Assert.Single(DatabaseImageScan.ScanHba(Sample(sample))));
    }

    [Fact]
    public void ClientAuthenticationScan_OnCleanSample_ReportsNothing()
    {
        Assert.Empty(DatabaseImageScan.ScanHba(Sample("HbaClean.conf")));
    }

    [Fact]
    public void SetupScript_SwitchesNoShellTraceOnAndGivesNoPasswordOnACommandLine()
    {
        Assert.Empty(DatabaseImageScan.ScanScript(File.ReadAllText(ImageFile("initdb", "10-bootstrap.sh"))));
    }

    [Theory]
    [InlineData("ScriptShellTrace.sh", DatabaseImageScan.ScriptShellTrace, 5)]
    [InlineData("ScriptPasswordArgument.sh", DatabaseImageScan.ScriptPasswordOnCommandLine, 6)]
    public void SetupScriptScan_OnViolationSample_ReportsTheRuleOnItsLine(string sample, string rule, int line)
    {
        Assert.Equal((rule, line), Assert.Single(DatabaseImageScan.ScanScript(Sample(sample))));
    }

    [Fact]
    public void SetupScriptScan_OnCleanSample_ReportsNothing()
    {
        Assert.Empty(DatabaseImageScan.ScanScript(Sample("ScriptClean.sh")));
    }

    [Theory]
    [InlineData("10-roles.sql")]
    [InlineData("20-database.sql")]
    [InlineData("30-database-access.sql")]
    [InlineData("40-schema.sql")]
    public void SetupSql_WritesNoPasswordAsLiteralStatesTheFiveNegativesAndGrantsNothingToPublic(string file)
    {
        Assert.Empty(DatabaseImageScan.ScanSql(File.ReadAllText(ImageFile("bootstrap", file))));
    }

    [Theory]
    [InlineData("SqlLiteralPassword.sql", DatabaseImageScan.SqlLiteralPassword)]
    [InlineData("SqlDangerousRole.sql", DatabaseImageScan.SqlRoleAttributes)]
    [InlineData("SqlPublicGrant.sql", DatabaseImageScan.SqlPublicGrant)]
    public void SetupSqlScan_OnViolationSample_ReportsTheRuleOnItsLine(string sample, string rule)
    {
        Assert.Equal((rule, 4), Assert.Single(DatabaseImageScan.ScanSql(Sample(sample))));
    }
}
