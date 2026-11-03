using System.Text.RegularExpressions;

using Shopfolio.UnitTests.FileSecrets;
using Shopfolio.UnitTests.Support;

namespace Shopfolio.UnitTests.Consistency;

/// <summary>
/// Keeps the database users and the setup script equal across the files that name them: the script, its client authentication templates,
/// the Compose files, the secret inventory of <c>DEPLOY_RUNBOOK</c> section B and <c>09 §10.8</c> (09 21.4 item 1; T10). Runs in the Lint
/// step, not in the Unit step.
/// </summary>
[Trait("Consistency", "true")]
public sealed partial class DatabaseUsersConsistencyTests
{
    private static readonly string[] _userNames = ["shopfolio_migrator", "shopfolio_app", "shopfolio_reader", "shopfolio_backup_status"];

    private static string ReadRepositoryFile(params string[] parts) => File.ReadAllText(Path.Combine([RepositoryRoot.Find(), .. parts]));

    private static string SetupScript() => ReadRepositoryFile("deploy", "images", "database", "initdb", "10-bootstrap.sh");

    [Fact]
    public void SetupScript_CreatesTheFourUsersTheCodingGuidelinesNameAndNoOther()
    {
        string script = SetupScript();
        string[] fromScript = UserAssignment().Matches(script).Select(m => m.Groups[1].Value).ToArray();
        string guidelines = ReadRepositoryFile("Docs", "09_CODING_GUIDELINES.md");
        string section = guidelines[guidelines.IndexOf("### 10.8 ", StringComparison.Ordinal)..guidelines.IndexOf("### 10.9 ", StringComparison.Ordinal)];

        Assert.Equal(_userNames.Order(StringComparer.Ordinal), fromScript.Order(StringComparer.Ordinal));
        Assert.All(_userNames, name => Assert.Contains($"`{name}`", section, StringComparison.Ordinal));
    }

    [Fact]
    public void ClientAuthenticationTemplates_NameOnlyTheUsersOfTheSetupScript()
    {
        string hba = ReadRepositoryFile("deploy", "images", "database", "bootstrap", "pg_hba.conf");
        string ident = ReadRepositoryFile("deploy", "images", "database", "bootstrap", "pg_ident.conf");

        string[] named = Regex.Matches(hba + ident, @"shopfolio_[a-z_]+").Select(m => m.Value).Distinct(StringComparer.Ordinal).ToArray();

        // The network users are named in the hba template, the backup status user in both files; no name outside the four appears.
        Assert.All(named, name => Assert.Contains(name, _userNames));
        Assert.Contains("shopfolio_backup_status", ident, StringComparison.Ordinal);
    }

    [Fact]
    public void SetupScript_ReadsExactlyTheSetupSecretsOfTheInstallOverlay()
    {
        IReadOnlyDictionary<string, IReadOnlyList<string>> install = ComposeSecrets.ReadServiceSecrets(ReadRepositoryFile("deploy", "compose.install.yaml"));
        string[] fromScript = SetupSecretName().Matches(SetupScript()).Select(m => m.Value).Distinct(StringComparer.Ordinal).ToArray();

        string[] setupSecrets = install["database"].Where(name => name.StartsWith("db_setup_", StringComparison.Ordinal)).ToArray();

        Assert.NotEmpty(setupSecrets);
        Assert.Equal(setupSecrets.Order(StringComparer.Ordinal), fromScript.Order(StringComparer.Ordinal));
    }

    [Fact]
    public void SetupScript_RunsEverySqlFileOfTheBootstrapFolderInOrder_AndTheDockerfileCarriesBoth()
    {
        string[] run = RunSql().Matches(SetupScript()).Select(m => m.Groups[1].Value).ToArray();
        string folder = Path.Combine(RepositoryRoot.Find(), "deploy", "images", "database", "bootstrap");
        string[] files = Directory.GetFiles(folder, "*.sql").Select(Path.GetFileName).Order(StringComparer.Ordinal).ToArray()!; // '!': GetFileName returns null only for an empty path.
        string dockerfile = ReadRepositoryFile("deploy", "images", "database", "Dockerfile");

        Assert.Equal(files, run);
        Assert.Contains("deploy/images/database/initdb/10-bootstrap.sh /docker-entrypoint-initdb.d/10-bootstrap.sh", dockerfile, StringComparison.Ordinal);
        Assert.Contains("deploy/images/database/bootstrap/ /usr/local/share/shopfolio/bootstrap/", dockerfile, StringComparison.Ordinal);
    }

    [Fact]
    public void ConnectionSettingsOfTheComponents_NameNoUserButTheFourOfTheSetupScript()
    {
        // 05 7.1.3: no component connects over the network as the founding user. Every connection setting in the deploy files and the
        // runbook names one of the four users; the compose file never renames the founding user.
        string[] files =
        [
            ReadRepositoryFile("deploy", "compose.yaml"),
            ReadRepositoryFile("deploy", "compose.dev.yaml"),
            ReadRepositoryFile("deploy", "compose.install.yaml"),
            ReadRepositoryFile("deploy", ".env.example"),
            ReadRepositoryFile("Docs", "DEPLOY_RUNBOOK.md"),
        ];
        string[] users = files.SelectMany(text => ConnectionUser().Matches(text).Select(m => m.Groups[1].Value)).ToArray();

        Assert.NotEmpty(users);
        Assert.All(users, user => Assert.Contains(user, _userNames));
        Assert.DoesNotContain("POSTGRES_USER", files[0] + files[1] + files[2], StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("compose.yaml", "Username=shopfolio_migrator")]
    [InlineData("compose.dev.yaml", "Username=shopfolio_app")]
    public void ComposeFile_ConnectsTheComponentWithItsOwnUser(string file, string expected)
    {
        Assert.Contains(expected, ReadRepositoryFile("deploy", file), StringComparison.Ordinal);
    }

    [Fact]
    public void Runbook_GivesTheApplicationAndTheReaderTheirOwnConnectionStrings()
    {
        string runbook = ReadRepositoryFile("Docs", "DEPLOY_RUNBOOK.md");

        Assert.Contains("Username=shopfolio_app;Password=<parola>", runbook, StringComparison.Ordinal);
        Assert.Contains("Username=shopfolio_reader;Password=<parola>", runbook, StringComparison.Ordinal);
    }

    [GeneratedRegex(@"^(?:MIGRATOR|APPLICATION|READER|BACKUP_STATUS)=(\w+)$", RegexOptions.Multiline)]
    private static partial Regex UserAssignment();

    [GeneratedRegex(@"db_setup_\w+_password")]
    private static partial Regex SetupSecretName();

    [GeneratedRegex(@"^run_sql (\S+\.sql) ", RegexOptions.Multiline)]
    private static partial Regex RunSql();

    [GeneratedRegex(@"Username=(\w+)")]
    private static partial Regex ConnectionUser();
}
