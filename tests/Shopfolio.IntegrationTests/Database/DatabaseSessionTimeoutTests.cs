using System.Diagnostics;

using Npgsql;

using Shopfolio.Infrastructure;

using DsnBuilder = Npgsql.NpgsqlConnectionStringBuilder;

namespace Shopfolio.IntegrationTests.Database;

/// <summary>The collection of the session time tests (09 10.7 item 7; T233): one database copied from the template.</summary>
[CollectionDefinition(Name)]
public sealed class DatabaseSessionTimeoutsDefinition : ICollectionFixture<CollectionDatabaseFixture>
{
    public const string Name = "DatabaseSessionTimeouts";
}

/// <summary>
/// The session times of 09 10.7 against a real PostgreSQL 18, through the connection strings that <c>DatabaseConnection</c> writes: the web and
/// maintenance profiles show their times, a lock wait, a long query and an idle open transaction each end at the server's time (earlier than
/// the client time) with the server's own error, and nothing in the path catches or retries it.
/// </summary>
[Collection(DatabaseSessionTimeoutsDefinition.Name)]
public sealed class DatabaseSessionTimeoutTests
{
    private const string Probe = "public.session_time_probe";
    private const string LockTimeoutState = "55P03";
    private const string QueryCancelledState = "57014";

    // The wait is measured on the client around a round trip, so it may start a little after the server's clock: a lower bound with some slack.
    private static readonly TimeSpan Slack = TimeSpan.FromSeconds(1);

    private readonly CollectionDatabaseFixture _collection;

    public DatabaseSessionTimeoutTests(CollectionDatabaseFixture collection)
    {
        _collection = collection;
    }

    [Theory]
    [InlineData(DatabaseSessionProfile.Web)]
    [InlineData(DatabaseSessionProfile.Maintenance)]
    public async Task Show_EachProfile_GivesTheServerTimesOfThatProfile(DatabaseSessionProfile profile)
    {
        CancellationToken ct = TestContext.Current.CancellationToken;
        TimeSpan statementTime = profile == DatabaseSessionProfile.Maintenance
            ? DatabaseTimeouts.MaintenanceStatementTimeout
            : DatabaseTimeouts.StatementTimeout;
        await using NpgsqlConnection connection = new(Dsn(profile));
        await connection.OpenAsync(ct);

        Assert.Equal(DatabaseTimeouts.LockTimeout, await ServerTimeAsync(connection, "lock_timeout", ct));
        Assert.Equal(statementTime, await ServerTimeAsync(connection, "statement_timeout", ct));
        Assert.Equal(DatabaseTimeouts.IdleInTransactionTimeout, await ServerTimeAsync(connection, "idle_in_transaction_session_timeout", ct));
    }

    [Fact]
    public async Task Show_TheTwoProfiles_AreNotOneSessionSetting()
    {
        CancellationToken ct = TestContext.Current.CancellationToken;
        await using NpgsqlConnection web = new(Dsn(DatabaseSessionProfile.Web));
        await using NpgsqlConnection maintenance = new(Dsn(DatabaseSessionProfile.Maintenance));
        await web.OpenAsync(ct);
        await maintenance.OpenAsync(ct);

        // Both sessions are open at the same time: neither changed the other's time.
        Assert.Equal(DatabaseTimeouts.StatementTimeout, await ServerTimeAsync(web, "statement_timeout", ct));
        Assert.Equal(DatabaseTimeouts.MaintenanceStatementTimeout, await ServerTimeAsync(maintenance, "statement_timeout", ct));
    }

    [Fact]
    public async Task LockWait_LongerThanLockTimeout_FailsWithTheServersLockError()
    {
        CancellationToken ct = TestContext.Current.CancellationToken;
        await ExecuteAsync(_collection.Dsn, $"DROP TABLE IF EXISTS {Probe}; CREATE TABLE {Probe} (id integer)", ct);
        try
        {
            await using NpgsqlConnection holder = new(Dsn(DatabaseSessionProfile.Web));
            await holder.OpenAsync(ct);
            await using NpgsqlTransaction holding = await holder.BeginTransactionAsync(ct);
            await ExecuteAsync(holder, $"LOCK TABLE {Probe} IN ACCESS EXCLUSIVE MODE", ct);

            await using NpgsqlConnection waiter = new(Dsn(DatabaseSessionProfile.Web));
            await waiter.OpenAsync(ct);
            Stopwatch watch = Stopwatch.StartNew();
            PostgresException error = await Assert.ThrowsAsync<PostgresException>(() => ExecuteAsync(waiter, $"SELECT count(*) FROM {Probe}", ct));
            watch.Stop();

            Assert.Equal(LockTimeoutState, error.SqlState);
            Assert.InRange(watch.Elapsed, DatabaseTimeouts.LockTimeout - Slack, DatabaseTimeouts.CommandTimeout);
        }
        finally
        {
            await ExecuteAsync(_collection.Dsn, $"DROP TABLE IF EXISTS {Probe}", CancellationToken.None);
        }
    }

    [Fact]
    public async Task LongQuery_LongerThanStatementTimeout_IsCancelledByTheServerBeforeTheClientTime()
    {
        CancellationToken ct = TestContext.Current.CancellationToken;
        await using NpgsqlConnection connection = new(Dsn(DatabaseSessionProfile.Web));
        await connection.OpenAsync(ct);

        Stopwatch watch = Stopwatch.StartNew();
        PostgresException error = await Assert.ThrowsAsync<PostgresException>(() => ExecuteAsync(connection, "SELECT pg_sleep(60)", ct));
        watch.Stop();

        Assert.Equal(QueryCancelledState, error.SqlState);
        Assert.InRange(watch.Elapsed, DatabaseTimeouts.StatementTimeout - Slack, DatabaseTimeouts.CommandTimeout);
    }

    [Fact]
    public async Task IdleOpenTransaction_LongerThanIdleTimeout_LosesItsConnection()
    {
        CancellationToken ct = TestContext.Current.CancellationToken;
        await using NpgsqlConnection connection = new(Dsn(DatabaseSessionProfile.Web));
        await connection.OpenAsync(ct);
        await using NpgsqlTransaction transaction = await connection.BeginTransactionAsync(ct);
        await ExecuteAsync(connection, "SELECT 1", ct);

        await Task.Delay(DatabaseTimeouts.IdleInTransactionTimeout + (2 * Slack), ct);

        await Assert.ThrowsAnyAsync<NpgsqlException>(() => ExecuteAsync(connection, "SELECT 1", ct));
        Assert.NotEqual(System.Data.ConnectionState.Open, connection.State);
    }

    private string Dsn(DatabaseSessionProfile profile)
    {
        DsnBuilder builder = new(DatabaseConnection.Create(_collection.Dsn, profile)) { Pooling = false };
        return builder.ConnectionString;
    }

    // The three settings are kept in milliseconds in pg_settings, whatever unit SHOW prints.
    private static async Task<TimeSpan> ServerTimeAsync(NpgsqlConnection connection, string setting, CancellationToken ct)
    {
        await using NpgsqlCommand command = new("SELECT setting FROM pg_settings WHERE name = @name", connection);
        command.Parameters.AddWithValue("name", setting);
        string milliseconds = (string)(await command.ExecuteScalarAsync(ct))!;
        return TimeSpan.FromMilliseconds(long.Parse(milliseconds, System.Globalization.CultureInfo.InvariantCulture));
    }

    private static async Task ExecuteAsync(string dsn, string sql, CancellationToken ct)
    {
        await using NpgsqlConnection connection = new(dsn);
        await connection.OpenAsync(ct);
        await ExecuteAsync(connection, sql, ct);
    }

    private static async Task ExecuteAsync(NpgsqlConnection connection, string sql, CancellationToken ct)
    {
        await using NpgsqlCommand command = new(sql, connection);
        await command.ExecuteNonQueryAsync(ct);
    }
}
