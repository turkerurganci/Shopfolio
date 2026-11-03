using Shopfolio.UnitTests.FileSecrets;
using Shopfolio.UnitTests.Support;

namespace Shopfolio.UnitTests.Consistency;

/// <summary>
/// Keeps the secret inventory of <c>DEPLOY_RUNBOOK</c> section B equal to the Compose files (05 6.x.2, 6.x.9, 6.x.12; 09 21.4 item 1).
/// Runs in the Lint step, not in the Unit step.
/// </summary>
[Trait("Consistency", "true")]
public sealed class SecretRunbookConsistencyTests
{
    private static string ReadRepositoryFile(params string[] parts) => File.ReadAllText(Path.Combine([RepositoryRoot.Find(), .. parts]));

    [Fact]
    public void Runbook_ListsEverySecretWithTheComponentsThatReceiveIt()
    {
        IReadOnlyList<RunbookSecretRow> rows = ComposeSecrets.ReadRunbookSecrets(ReadRepositoryFile("Docs", "DEPLOY_RUNBOOK.md"));
        List<IReadOnlyDictionary<string, IReadOnlyList<string>>> compose =
        [
            ComposeSecrets.ReadServiceSecrets(ReadRepositoryFile("deploy", "compose.yaml")),
            ComposeSecrets.ReadServiceSecrets(ReadRepositoryFile("deploy", "compose.install.yaml")),
        ];

        Assert.Equal(ExpectedSecretLayout.AllSecretNames(), rows.Select(r => r.Secret).Order(StringComparer.Ordinal));
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
        IReadOnlyList<RunbookSecretRow> rows = ComposeSecrets.ReadRunbookSecrets(ReadRepositoryFile("Docs", "DEPLOY_RUNBOOK.md"));

        Assert.NotEmpty(rows);
        foreach (RunbookSecretRow row in rows)
        {
            bool neverRotated = row.Rotation.StartsWith("Döndürülmez", StringComparison.Ordinal);
            Assert.Equal(ExpectedSecretLayout.NeverRotated.Contains(row.Secret, StringComparer.Ordinal), neverRotated);
        }
    }
}
