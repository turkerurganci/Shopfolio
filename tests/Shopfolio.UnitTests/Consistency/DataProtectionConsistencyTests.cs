using System.Text.RegularExpressions;

using Shopfolio.Infrastructure.Security;
using Shopfolio.UnitTests.Support;

namespace Shopfolio.UnitTests.Consistency;

/// <summary>
/// Keeps the purpose list equal to the document and the key ring inside the backup (T232 KK2, KK8; 09 15.5 items 2 and 7, 21.4 item 1; 05 7.6.3).
/// Reads repository files only. Runs in the Lint step.
/// </summary>
[Trait("Consistency", "true")]
public sealed partial class DataProtectionConsistencyTests
{
    private static string ReadRepositoryFile(params string[] parts) => File.ReadAllText(Path.Combine([RepositoryRoot.Find(), .. parts]));

    [Fact]
    public void PurposeTableOf09_EqualsTheClosedListOfTheCode_InBothDirections()
    {
        string[] lines = ReadRepositoryFile("Docs", "09_CODING_GUIDELINES.md").ReplaceLineEndings("\n").Split('\n');
        int start = Array.FindIndex(lines, l => l.StartsWith("### 15.5 ", StringComparison.Ordinal));
        int end = Array.FindIndex(lines, start + 1, l => l.StartsWith("### ", StringComparison.Ordinal));
        Assert.True(start >= 0 && end > start, "The 15.5 section of 09 was not found; update this check with the document.");

        string[] documented =
        [
            .. lines[start..end]
                .Where(l => l.TrimStart().StartsWith('|'))
                .SelectMany(l => PurposeInBackticks().Matches(l).Select(m => m.Groups[1].Value)),
        ];

        Assert.NotEmpty(documented);
        Assert.Equal(documented.Order(StringComparer.Ordinal), DataProtectionPurposes.All.Order(StringComparer.Ordinal));
    }

    [Fact]
    public void DatabaseBackup_IsOfTheWholeClusterWithNoTableOrDatabaseFilter()
    {
        // The key ring is a table of the application's database (06 4.68) and the database backup is the whole cluster (05 7.6.3): no line of the
        // pgBackRest configuration, of the entrypoint or of the job scripts includes or excludes a database, a tablespace or a table.
        string[] files =
        [
            ReadRepositoryFile("deploy", "images", "database", "pgbackrest.conf"),
            ReadRepositoryFile("deploy", "images", "database", "database-entrypoint.sh"),
            ReadRepositoryFile("deploy", "images", "database", "database-job.sh"),
        ];

        foreach (string text in files)
        {
            string active = string.Join('\n', text.ReplaceLineEndings("\n").Split('\n').Where(l => !l.TrimStart().StartsWith('#')));
            Assert.DoesNotMatch(BackupFilter(), active);
        }
    }

    [Fact]
    public void KeyRingTable_IsInTheMigrationOfTheApplicationDatabase()
    {
        string migration = ReadRepositoryFile("src", "Shopfolio.Web", "Infrastructure", "Migrations", "20261104124425_CreateSchedulerBackupDisposalAndKeyTables.cs");

        Assert.Contains("\"data_protection_keys\"", migration, StringComparison.Ordinal);
    }

    [GeneratedRegex(@"`(Shopfolio\.\w+\.v\d+)`", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex PurposeInBackticks();

    [GeneratedRegex(@"(?:--|\b)(?:db-include|db-exclude|tablespace-map|exclude|include)\b(?!_dir)", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex BackupFilter();
}
