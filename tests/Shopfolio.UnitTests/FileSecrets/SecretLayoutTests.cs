using Shopfolio.Infrastructure.FileSecrets;
using Shopfolio.UnitTests.Support;

namespace Shopfolio.UnitTests.FileSecrets;

/// <summary>
/// Keeps the secret layout of the installation equal to its three descriptions (05 6.x.2, MK-59): the table in the architecture
/// document (written out below), the Compose files in <c>deploy/</c> and the inventory in <c>DEPLOY_RUNBOOK</c> section B.
/// </summary>
public sealed class SecretLayoutTests
{
    // 05 6.x.2 for the continuously running installation and the one-off services: the secrets each component receives.
    // A component with no secret receives none (the reverse proxy: 6.x.7).
    private static readonly Dictionary<string, string[]> _expectedPerService = new(StringComparer.Ordinal)
    {
        ["app"] = ["db_app_connection", "payment_provider_keys", "email_credential", "google_client_secret", "link_derivation_secret"],
        ["migrate"] = ["db_migrator_password"],
        ["backup"] = ["file_backup_storage_key", "file_backup_cipher_key"],
        ["database"] = ["db_backup_storage_key", "db_backup_cipher_key"],
        ["proxy"] = [],
        ["read-command"] = ["db_reader_connection"],
        ["command"] = ["db_app_connection"],
    };

    // 05 6.x.6: what the database receives only at the first install (deploy/compose.install.yaml).
    private static readonly string[] _expectedFirstInstallOnly = ["db_superuser_password", "db_setup_migrator_password", "db_setup_app_password", "db_setup_reader_password"];

    // 05 6.x.12: the only secrets that are never rotated.
    private static readonly string[] _neverRotated = ["db_backup_cipher_key", "file_backup_cipher_key"];

    private static string ReadRepositoryFile(params string[] parts) => File.ReadAllText(Path.Combine([RepositoryRoot.Find(), .. parts]));

    private static string ComposeText() => ReadRepositoryFile("deploy", "compose.yaml");

    private static string InstallText() => ReadRepositoryFile("deploy", "compose.install.yaml");

    private static string RunbookText() => ReadRepositoryFile("Docs", "DEPLOY_RUNBOOK.md");

    private static string[] AllSecretNames() =>
        _expectedPerService.Values.SelectMany(s => s).Concat(_expectedFirstInstallOnly).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToArray();

    [Fact]
    public void Compose_GivesEachServiceExactlyTheSecretsOfItsTableRow()
    {
        IReadOnlyDictionary<string, IReadOnlyList<string>> actual = ComposeSecrets.ReadServiceSecrets(ComposeText());

        Assert.Equal(_expectedPerService.Keys.Order(StringComparer.Ordinal), actual.Keys.Order(StringComparer.Ordinal));
        foreach ((string service, string[] expected) in _expectedPerService)
        {
            Assert.Equal(expected.Order(StringComparer.Ordinal), actual[service].Order(StringComparer.Ordinal));
        }
    }

    [Fact]
    public void InstallOverlay_GivesTheDatabaseTheFounderAndThreeUserPasswordsAndNothingElse()
    {
        IReadOnlyDictionary<string, IReadOnlyList<string>> actual = ComposeSecrets.ReadServiceSecrets(InstallText());

        Assert.Equal(["database"], actual.Keys);
        Assert.Equal(_expectedFirstInstallOnly.Order(StringComparer.Ordinal), actual["database"].Order(StringComparer.Ordinal));
        Assert.Contains("POSTGRES_PASSWORD_FILE=/run/secrets/db_superuser_password", InstallText(), StringComparison.Ordinal);
    }

    [Fact]
    public void Compose_DoesNotCarryTheFirstInstallSecretsInTheDatabaseForGood()
    {
        string compose = ComposeText();
        IReadOnlyDictionary<string, IReadOnlyList<string>> secrets = ComposeSecrets.ReadServiceSecrets(compose);

        foreach (string name in _expectedFirstInstallOnly)
        {
            Assert.DoesNotContain(name, secrets["database"]);
        }

        Assert.DoesNotContain("POSTGRES_PASSWORD", compose, StringComparison.Ordinal);
    }

    [Fact]
    public void Compose_DefinesEverySecretOnceAsAFileNamedLikeIt()
    {
        IReadOnlyDictionary<string, string> defined = ComposeSecrets.ReadDefinedSecrets(ComposeText());

        Assert.Equal(AllSecretNames(), defined.Keys.Order(StringComparer.Ordinal));
        Assert.All(defined, pair => Assert.Equal(pair.Key, pair.Value));
    }

    [Fact]
    public void Compose_GivesNoSecretToTheContinuouslyRunningApplicationBeyondItsRow()
    {
        IReadOnlyDictionary<string, IReadOnlyList<string>> actual = ComposeSecrets.ReadServiceSecrets(ComposeText());

        // 6.x.3 and 6.x.18: the running application has no schema owner password, no read-only password, no backup secret.
        Assert.DoesNotContain("db_migrator_password", actual["app"]);
        Assert.DoesNotContain("db_reader_connection", actual["app"]);
        Assert.DoesNotContain(actual["app"], s => s.Contains("backup", StringComparison.Ordinal));
    }

    [Fact]
    public void Compose_PutsNoSecretIntoAnEnvironmentSetting()
    {
        // Passwords reach containers as files; the only password-like value in an environment line is a path to a file. The one
        // exception is the one-off migration process, which hands the file's content to its own process (see `migrate` in the file).
        foreach (string text in new[] { ComposeText(), InstallText() })
        {
            foreach (string line in text.ReplaceLineEndings("\n").Split('\n'))
            {
                string trimmed = line.Trim();
                if (trimmed.StartsWith('#') || !trimmed.StartsWith("- ", StringComparison.Ordinal) || !trimmed.Contains('=', StringComparison.Ordinal))
                {
                    continue;
                }

                if (trimmed.Contains("PASSWORD", StringComparison.OrdinalIgnoreCase))
                {
                    Assert.True(
                        trimmed.Contains("_FILE=/run/secrets/", StringComparison.Ordinal) || trimmed.Contains("$$(cat /run/secrets/", StringComparison.Ordinal),
                        trimmed);
                }
            }
        }
    }

    [Fact]
    public void Compose_GivesTheConnectionStringFromTheFileAndFromNoEnvironmentSetting()
    {
        string compose = ComposeText();
        IReadOnlyDictionary<string, IReadOnlyList<string>> secrets = ComposeSecrets.ReadServiceSecrets(compose);
        IReadOnlyDictionary<string, string> defined = ComposeSecrets.ReadDefinedSecrets(compose);

        // 09 3.5 item 2: the whole value of Database:ConnectionString is the content of a secret file; no environment line carries it.
        Assert.DoesNotContain("Database__ConnectionString", compose, StringComparison.Ordinal);
        Assert.DoesNotContain("Username=shopfolio_app", compose, StringComparison.Ordinal);
        Assert.DoesNotContain("Username=shopfolio_reader", compose, StringComparison.Ordinal);
        Assert.Equal(2, SecretNames.DatabaseConnectionFiles.Count);
        Assert.Contains(SecretNames.DatabaseAppConnection, defined.Keys);
        Assert.Contains(SecretNames.DatabaseReaderConnection, defined.Keys);
        Assert.Contains(SecretNames.DatabaseAppConnection, secrets["app"]);
        Assert.Contains(SecretNames.DatabaseAppConnection, secrets["command"]);
        Assert.Contains(SecretNames.DatabaseReaderConnection, secrets["read-command"]);
        foreach (string service in secrets.Keys)
        {
            Assert.True(secrets[service].Count(SecretNames.DatabaseConnectionFiles.Contains) <= 1, $"{service} gets more than one connection string file");
        }
    }

    [Fact]
    public void Compose_GivesEverySecretToOneContainerUserOnly()
    {
        // 05 6.x.1: every secret file belongs to the user of the container that reads it, and only that user reads it. The
        // application image runs as one user (app, migrate, command, read-command); the other images have their own users.
        // The first install overlay (database) is part of the picture: a password the database reads at the first install has its own file.
        Dictionary<string, string> userOfService = new(StringComparer.Ordinal)
        {
            ["app"] = "application image",
            ["migrate"] = "application image",
            ["command"] = "application image",
            ["read-command"] = "application image",
            ["database"] = "database image",
            ["backup"] = "backup image",
            ["proxy"] = "proxy image",
        };

        Dictionary<string, HashSet<string>> usersOfSecret = new(StringComparer.Ordinal);
        foreach (IReadOnlyDictionary<string, IReadOnlyList<string>> file in new[] { ComposeSecrets.ReadServiceSecrets(ComposeText()), ComposeSecrets.ReadServiceSecrets(InstallText()) })
        {
            foreach ((string service, IReadOnlyList<string> names) in file)
            {
                foreach (string name in names)
                {
                    if (!usersOfSecret.TryGetValue(name, out HashSet<string>? users))
                    {
                        users = new HashSet<string>(StringComparer.Ordinal);
                        usersOfSecret[name] = users;
                    }

                    users.Add(userOfService[service]);
                }
            }
        }

        Assert.Equal(AllSecretNames(), usersOfSecret.Keys.Order(StringComparer.Ordinal));
        Assert.All(usersOfSecret, pair => Assert.True(pair.Value.Count == 1, $"{pair.Key} is read by {string.Join(", ", pair.Value)}"));
    }

    [Fact]
    public void Runbook_ListsEverySecretWithTheComponentsThatReceiveIt()
    {
        IReadOnlyList<RunbookSecretRow> rows = ComposeSecrets.ReadRunbookSecrets(RunbookText());
        List<IReadOnlyDictionary<string, IReadOnlyList<string>>> compose =
            [ComposeSecrets.ReadServiceSecrets(ComposeText()), ComposeSecrets.ReadServiceSecrets(InstallText())];

        Assert.Equal(AllSecretNames(), rows.Select(r => r.Secret).Order(StringComparer.Ordinal));
        foreach (RunbookSecretRow row in rows)
        {
            string[] mounted = compose.SelectMany(file => file).Where(pair => pair.Value.Contains(row.Secret)).Select(pair => pair.Key)
                .Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToArray();
            Assert.Equal(mounted, row.Receivers.Order(StringComparer.Ordinal));
        }
    }

    [Fact]
    public void Runbook_RotatesEverySecretExceptTheTwoBackupCipherKeys()
    {
        IReadOnlyList<RunbookSecretRow> rows = ComposeSecrets.ReadRunbookSecrets(RunbookText());

        Assert.NotEmpty(rows);
        foreach (RunbookSecretRow row in rows)
        {
            bool neverRotated = row.Rotation.StartsWith("Döndürülmez", StringComparison.Ordinal);
            Assert.Equal(_neverRotated.Contains(row.Secret, StringComparer.Ordinal), neverRotated);
        }
    }
}
