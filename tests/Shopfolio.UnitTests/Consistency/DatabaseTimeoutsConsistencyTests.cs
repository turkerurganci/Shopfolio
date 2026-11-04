using System.Text.RegularExpressions;

using Shopfolio.Infrastructure;
using Shopfolio.UnitTests.Support;

namespace Shopfolio.UnitTests.Consistency;

/// <summary>
/// Keeps the database session times equal across the files that name them (T233; 09 10.7 items 1 and 2, 21.4 item 1): the constants of
/// <c>DatabaseTimeouts</c> against the two rows of the 09 3.5 table, and the migration service's connection string in
/// <c>deploy/compose.yaml</c> against what <c>DatabaseConnection</c> writes for the maintenance profile. Runs in the Lint step.
/// </summary>
[Trait("Consistency", "true")]
public sealed partial class DatabaseTimeoutsConsistencyTests
{
    private static string ReadRepositoryFile(params string[] parts) => File.ReadAllText(Path.Combine([RepositoryRoot.Find(), .. parts]));

    private static string TableRow(string start)
    {
        string guidelines = ReadRepositoryFile("Docs", "09_CODING_GUIDELINES.md");
        string row = guidelines.ReplaceLineEndings("\n").Split('\n').Single(line => line.StartsWith(start, StringComparison.Ordinal));
        return row;
    }

    [Fact]
    public void WebRowOfTheTable_EqualsTheWebConstants()
    {
        string row = TableRow("| Veritabanı oturumu (sunucu taraflı):");

        Match match = WebRow().Match(row);

        Assert.True(match.Success, "The web row of the 09 3.5 table has changed shape; update this check with it.");
        Assert.Equal(TimeSpan.FromSeconds(int.Parse(match.Groups[1].Value, System.Globalization.CultureInfo.InvariantCulture)), DatabaseTimeouts.LockTimeout);
        Assert.Equal(TimeSpan.FromSeconds(int.Parse(match.Groups[2].Value, System.Globalization.CultureInfo.InvariantCulture)), DatabaseTimeouts.StatementTimeout);
        Assert.Equal(TimeSpan.FromSeconds(int.Parse(match.Groups[3].Value, System.Globalization.CultureInfo.InvariantCulture)), DatabaseTimeouts.IdleInTransactionTimeout);
        Assert.Equal(TimeSpan.FromSeconds(int.Parse(match.Groups[4].Value, System.Globalization.CultureInfo.InvariantCulture)), DatabaseTimeouts.CommandTimeout);
    }

    [Fact]
    public void MaintenanceRowOfTheTable_EqualsTheMaintenanceConstants()
    {
        string row = TableRow("| Göç hizmeti ve yönetim komutları:");

        Match match = MaintenanceRow().Match(row);

        Assert.True(match.Success, "The maintenance row of the 09 3.5 table has changed shape; update this check with it.");
        Assert.Equal(TimeSpan.FromMinutes(int.Parse(match.Groups[1].Value, System.Globalization.CultureInfo.InvariantCulture)), DatabaseTimeouts.MaintenanceStatementTimeout);
        Assert.Equal(
            TimeSpan.FromMinutes(int.Parse(match.Groups[2].Value, System.Globalization.CultureInfo.InvariantCulture)) + TimeSpan.FromSeconds(int.Parse(match.Groups[3].Value, System.Globalization.CultureInfo.InvariantCulture)),
            DatabaseTimeouts.MaintenanceCommandTimeout);
    }

    [Fact]
    public void MigrationService_ConnectsWithTheMaintenanceProfile()
    {
        string compose = ReadRepositoryFile("deploy", "compose.yaml");
        Match entry = MigratorConnection().Match(compose);
        Assert.True(entry.Success, "The migration service's efbundle line was not found in deploy/compose.yaml.");

        string expected = DatabaseConnection.Create("Host=database;Database=shopfolio;Username=shopfolio_migrator", DatabaseSessionProfile.Maintenance);

        Assert.Equal(Pairs(expected), Pairs(entry.Groups[1].Value));
    }

    [Fact]
    public void MigrationServiceCheck_WithTheWebProfile_WouldDiffer()
    {
        // The check can go red: the web profile's connection string is not the maintenance one.
        string web = DatabaseConnection.Create("Host=database;Database=shopfolio;Username=shopfolio_migrator", DatabaseSessionProfile.Web);
        string maintenance = DatabaseConnection.Create("Host=database;Database=shopfolio;Username=shopfolio_migrator", DatabaseSessionProfile.Maintenance);

        Assert.NotEqual(Pairs(web), Pairs(maintenance));
    }

    private static string[] Pairs(string connectionString)
    {
        // key=value pairs; a value with spaces may come quoted from the builder and unquoted from the file, so the quotes are not part of the value.
        return connectionString
            .Split(';', StringSplitOptions.RemoveEmptyEntries)
            .Select(pair => pair.Split('=', 2))
            .Select(parts => $"{parts[0].Trim().ToUpperInvariant()}={parts[1].Trim().Trim('"')}")
            .Order(StringComparer.Ordinal)
            .ToArray();
    }

    [GeneratedRegex(@"`lock_timeout` (\d+), `statement_timeout` (\d+), `idle_in_transaction_session_timeout` (\d+) saniye; istemci `CommandTimeout` (\d+) saniye", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex WebRow();

    [GeneratedRegex(@"`statement_timeout` (\d+) dakika.*`CommandTimeout` (\d+) dakika (\d+) saniye", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex MaintenanceRow();

    [GeneratedRegex(@"efbundle --connection ""([^""]*Username=shopfolio_migrator[^""]*)""", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex MigratorConnection();
}
