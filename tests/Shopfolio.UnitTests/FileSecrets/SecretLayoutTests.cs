using Shopfolio.Infrastructure.FileSecrets;
using Shopfolio.UnitTests.Support;

namespace Shopfolio.UnitTests.FileSecrets;

/// <summary>
/// Keeps the secret layout of the installation equal to its three descriptions (05 6.x.2, MK-59): the table in the architecture
/// document (ExpectedSecretLayout) and the Compose files in <c>deploy/</c>. The runbook inventory is checked in Consistency/.
/// </summary>
public sealed class SecretLayoutTests
{

    private static string ReadRepositoryFile(params string[] parts) => File.ReadAllText(Path.Combine([RepositoryRoot.Find(), .. parts]));

    private static string ComposeText() => ReadRepositoryFile("deploy", "compose.yaml");

    private static string InstallText() => ReadRepositoryFile("deploy", "compose.install.yaml");

    [Fact]
    public void Compose_GivesEachServiceExactlyTheSecretsOfItsTableRow()
    {
        IReadOnlyDictionary<string, IReadOnlyList<string>> actual = ComposeSecrets.ReadServiceSecrets(ComposeText());

        Assert.Equal(ExpectedSecretLayout.PerService.Keys.Order(StringComparer.Ordinal), actual.Keys.Order(StringComparer.Ordinal));
        foreach ((string service, string[] expected) in ExpectedSecretLayout.PerService)
        {
            Assert.Equal(expected.Order(StringComparer.Ordinal), actual[service].Order(StringComparer.Ordinal));
        }
    }

    [Fact]
    public void InstallOverlay_GivesTheDatabaseTheFounderAndThreeUserPasswordsAndNothingElse()
    {
        IReadOnlyDictionary<string, IReadOnlyList<string>> actual = ComposeSecrets.ReadServiceSecrets(InstallText());

        Assert.Equal(["database"], actual.Keys);
        Assert.Equal(ExpectedSecretLayout.FirstInstallOnly.Order(StringComparer.Ordinal), actual["database"].Order(StringComparer.Ordinal));
        Assert.Contains("POSTGRES_PASSWORD_FILE=/run/secrets/db_superuser_password", InstallText(), StringComparison.Ordinal);
    }

    [Fact]
    public void Compose_DoesNotCarryTheFirstInstallSecretsInTheDatabaseForGood()
    {
        string compose = ComposeText();
        IReadOnlyDictionary<string, IReadOnlyList<string>> secrets = ComposeSecrets.ReadServiceSecrets(compose);

        foreach (string name in ExpectedSecretLayout.FirstInstallOnly)
        {
            Assert.DoesNotContain(name, secrets["database"]);
        }

        Assert.DoesNotContain("POSTGRES_PASSWORD", compose, StringComparison.Ordinal);
    }

    [Fact]
    public void Compose_DefinesEverySecretOnceAsAFileNamedLikeIt()
    {
        IReadOnlyDictionary<string, string> defined = ComposeSecrets.ReadDefinedSecrets(ComposeText());

        Assert.Equal(ExpectedSecretLayout.AllSecretNames(), defined.Keys.Order(StringComparer.Ordinal));
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

        Assert.Equal(ExpectedSecretLayout.AllSecretNames(), usersOfSecret.Keys.Order(StringComparer.Ordinal));
        Assert.All(usersOfSecret, pair => Assert.True(pair.Value.Count == 1, $"{pair.Key} is read by {string.Join(", ", pair.Value)}"));
    }
}
