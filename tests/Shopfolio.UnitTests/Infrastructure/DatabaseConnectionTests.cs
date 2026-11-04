using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

using Npgsql;

using Shopfolio.Infrastructure;
using Shopfolio.UnitTests.Support;

using DsnBuilder = Npgsql.NpgsqlConnectionStringBuilder;

namespace Shopfolio.UnitTests.Infrastructure;

/// <summary>
/// The session times in the connection string (T233; 09 10.7 items 1-3, 7): both profiles, the contradiction scan of the 09 3.5 table
/// and the closed retry strategy. Nothing here opens a connection.
/// </summary>
public sealed class DatabaseConnectionTests
{
    private const string Installation = "Host=database;Database=shopfolio;Username=shopfolio_app";

    [Fact]
    public void Configure_WebProfile_WritesTheThreeServerTimesAndTheClientTime()
    {
        DsnBuilder builder = new(Installation);

        DatabaseConnection.Configure(builder, DatabaseSessionProfile.Web);

        Assert.Equal("-c lock_timeout=5000 -c statement_timeout=15000 -c idle_in_transaction_session_timeout=30000", builder.Options);
        Assert.Equal(20, builder.CommandTimeout);
    }

    [Fact]
    public void Configure_MaintenanceProfile_WritesTheLongStatementTimeAndTheLongClientTime()
    {
        DsnBuilder builder = new(Installation);

        DatabaseConnection.Configure(builder, DatabaseSessionProfile.Maintenance);

        Assert.Equal("-c lock_timeout=5000 -c statement_timeout=600000 -c idle_in_transaction_session_timeout=30000", builder.Options);
        Assert.Equal(605, builder.CommandTimeout);
    }

    [Fact]
    public void Configure_MaintenanceProfile_KeepsTheLockAndIdleTimesOfTheWebProfile()
    {
        DsnBuilder web = new(Installation);
        DsnBuilder maintenance = new(Installation);

        DatabaseConnection.Configure(web, DatabaseSessionProfile.Web);
        DatabaseConnection.Configure(maintenance, DatabaseSessionProfile.Maintenance);

        Assert.Equal(DatabaseTimeouts.StatementTimeout, TimeSpan.FromMilliseconds(Server(web.Options!, "statement_timeout")));
        Assert.Equal(DatabaseTimeouts.MaintenanceStatementTimeout, TimeSpan.FromMilliseconds(Server(maintenance.Options!, "statement_timeout")));
        Assert.Equal(Server(web.Options!, "lock_timeout"), Server(maintenance.Options!, "lock_timeout"));
        Assert.Equal(Server(web.Options!, "idle_in_transaction_session_timeout"), Server(maintenance.Options!, "idle_in_transaction_session_timeout"));
        Assert.True(DatabaseTimeouts.MaintenanceStatementTimeout > DatabaseTimeouts.StatementTimeout);
    }

    [Theory]
    [InlineData(DatabaseSessionProfile.Web)]
    [InlineData(DatabaseSessionProfile.Maintenance)]
    public void Configure_EachProfile_LeavesTheClientTimeLongerThanTheLockAndStatementTimes(DatabaseSessionProfile profile)
    {
        DsnBuilder builder = new(Installation);

        DatabaseConnection.Configure(builder, profile);

        double lockTime = Server(builder.Options!, "lock_timeout");
        double statementTime = Server(builder.Options!, "statement_timeout");
        double clientTime = TimeSpan.FromSeconds(builder.CommandTimeout).TotalMilliseconds;
        Assert.True(lockTime < statementTime, "lock_timeout must be shorter than statement_timeout.");
        Assert.True(statementTime < clientTime, "statement_timeout must be shorter than the client command time: the server cancels first.");
    }

    [Fact]
    public void Configure_WithAnOptionsOrCommandTimeoutInTheInstallationString_ReplacesThem()
    {
        DsnBuilder builder = new(Installation + ";Options=-c statement_timeout=0;Command Timeout=1");

        DatabaseConnection.Configure(builder, DatabaseSessionProfile.Web);

        Assert.DoesNotContain("statement_timeout=0", builder.Options, StringComparison.Ordinal);
        Assert.Equal(20, builder.CommandTimeout);
    }

    [Fact]
    public void Create_TheTwoProfiles_AreTwoConnectionStrings()
    {
        string web = DatabaseConnection.Create(Installation, DatabaseSessionProfile.Web);
        string maintenance = DatabaseConnection.Create(Installation, DatabaseSessionProfile.Maintenance);

        Assert.NotEqual(web, maintenance);
        DsnBuilder webBuilder = new(web);
        DsnBuilder maintenanceBuilder = new(maintenance);
        Assert.Equal("shopfolio_app", webBuilder.Username);
        Assert.Equal(webBuilder.Host, maintenanceBuilder.Host);
        Assert.NotEqual(webBuilder.Options, maintenanceBuilder.Options);
        Assert.NotEqual(webBuilder.CommandTimeout, maintenanceBuilder.CommandTimeout);
    }

    [Fact]
    public void Create_ParsedBack_GivesTheSameTimesThatConfigureWrote()
    {
        DsnBuilder written = new(Installation);
        DatabaseConnection.Configure(written, DatabaseSessionProfile.Maintenance);

        DsnBuilder parsed = new(DatabaseConnection.Create(Installation, DatabaseSessionProfile.Maintenance));

        Assert.Equal(written.Options, parsed.Options);
        Assert.Equal(written.CommandTimeout, parsed.CommandTimeout);
    }

    [Fact]
    public void MigrationServiceConnectionInTheComposeFile_ParsesToTheMaintenanceTimes()
    {
        // The Compose file carries the string unquoted; the driver must read the same Options and Command Timeout that Configure writes.
        string compose = File.ReadAllText(Path.Combine(RepositoryRoot.Find(), "deploy", "compose.yaml"));
        System.Text.RegularExpressions.Match entry = System.Text.RegularExpressions.Regex.Match(compose, "efbundle --connection \"([^\"]*)\"");
        Assert.True(entry.Success);

        DsnBuilder fromFile = new(entry.Groups[1].Value);
        DsnBuilder expected = new(Installation);
        DatabaseConnection.Configure(expected, DatabaseSessionProfile.Maintenance);

        Assert.Equal(expected.Options, fromFile.Options);
        Assert.Equal(expected.CommandTimeout, fromFile.CommandTimeout);
        Assert.Equal("shopfolio_migrator", fromFile.Username);
    }

    [Fact]
    public void Configure_UnknownProfile_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => DatabaseConnection.Configure(new DsnBuilder(Installation), (DatabaseSessionProfile)99));
    }

    [Fact]
    public void Timeouts_AreTheRowsOfTheTable()
    {
        Assert.Equal(TimeSpan.FromSeconds(5), DatabaseTimeouts.LockTimeout);
        Assert.Equal(TimeSpan.FromSeconds(15), DatabaseTimeouts.StatementTimeout);
        Assert.Equal(TimeSpan.FromMinutes(10), DatabaseTimeouts.MaintenanceStatementTimeout);
        Assert.Equal(TimeSpan.FromSeconds(30), DatabaseTimeouts.IdleInTransactionTimeout);
        Assert.Equal(TimeSpan.FromSeconds(20), DatabaseTimeouts.CommandTimeout);
        Assert.Equal(TimeSpan.FromSeconds(605), DatabaseTimeouts.MaintenanceCommandTimeout);
    }

    [Fact]
    public void Timeouts_PassTheContradictionScanOfTheTable()
    {
        // 09 3.5 contradiction scan: lock < statement < client time in both profiles; the 15 second user call is shorter than the 20 second command time.
        Assert.True(DatabaseTimeouts.LockTimeout < DatabaseTimeouts.StatementTimeout);
        Assert.True(DatabaseTimeouts.StatementTimeout < DatabaseTimeouts.CommandTimeout);
        Assert.True(DatabaseTimeouts.LockTimeout < DatabaseTimeouts.MaintenanceStatementTimeout);
        Assert.True(DatabaseTimeouts.MaintenanceStatementTimeout < DatabaseTimeouts.MaintenanceCommandTimeout);
        Assert.True(DatabaseTimeouts.MaintenanceCommandTimeout > DatabaseTimeouts.CommandTimeout);
    }

    [Fact]
    public void AppDbContext_AsConfigured_HasNoRetryStrategy()
    {
        DbContextOptionsBuilder<AppDbContext> options = new();
        AppDbContext.Configure(options, DatabaseConnection.Create(Installation, DatabaseSessionProfile.Web));
        using AppDbContext context = new(options.Options);

        IExecutionStrategy strategy = context.Database.CreateExecutionStrategy();

        Assert.False(strategy.RetriesOnFailure);
    }

    [Fact]
    public void AppDbContext_WithRetryOnFailureTurnedOn_ShowsTheCheckCanGoRed()
    {
        DbContextOptionsBuilder<AppDbContext> options = new();
        options.UseNpgsql(DatabaseConnection.Create(Installation, DatabaseSessionProfile.Web), npgsql => npgsql.EnableRetryOnFailure());
        using AppDbContext context = new(options.Options);

        Assert.True(context.Database.CreateExecutionStrategy().RetriesOnFailure);
    }

    private static double Server(string options, string name)
    {
        string[] parts = options.Split(' ');
        for (int i = 0; i < parts.Length - 1; i++)
        {
            if (parts[i] == "-c" && parts[i + 1].StartsWith(name + "=", StringComparison.Ordinal))
            {
                return double.Parse(parts[i + 1][(name.Length + 1)..], System.Globalization.CultureInfo.InvariantCulture);
            }
        }

        throw new InvalidOperationException($"{name} is not in the options.");
    }
}
