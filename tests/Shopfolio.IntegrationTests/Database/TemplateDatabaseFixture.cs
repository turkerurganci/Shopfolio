using System.Security.Cryptography;

using Npgsql;

using DsnBuilder = Npgsql.NpgsqlConnectionStringBuilder;

[assembly: AssemblyFixture(typeof(Shopfolio.IntegrationTests.Database.TemplateDatabaseFixture))]

namespace Shopfolio.IntegrationTests.Database;

/// <summary>
/// The four database users of one test run (09 10.8 item 1): names that are unique to the run, so a test run never touches the users of
/// a development database on the same server, and passwords made up for the run. In an installation the names are fixed.
/// </summary>
public sealed record TestDatabaseUsers(string Migrator, string App, string Reader, string BackupStatus, string MigratorPassword, string AppPassword, string ReaderPassword)
{
    /// <summary>The name prefix of every user the tests create; <see cref="TemplateDatabaseFixture"/> sweeps leftovers of a crashed run by it.</summary>
    public const string Prefix = "shopfolio_test_r";

    public static TestDatabaseUsers CreateForRun()
    {
        string run = Prefix + Convert.ToHexStringLower(RandomNumberGenerator.GetBytes(4));
        return new TestDatabaseUsers(
            run + "_migrator",
            run + "_app",
            run + "_reader",
            run + "_status",
            Convert.ToHexStringLower(RandomNumberGenerator.GetBytes(16)),
            Convert.ToHexStringLower(RandomNumberGenerator.GetBytes(16)),
            Convert.ToHexStringLower(RandomNumberGenerator.GetBytes(16)));
    }

    /// <summary>The variables of the setup SQL files (<c>deploy/images/database/bootstrap</c>).</summary>
    public Dictionary<string, string> Variables(string database)
    {
        return new Dictionary<string, string>
        {
            ["db"] = database,
            ["migrator"] = Migrator,
            ["app"] = App,
            ["reader"] = Reader,
            ["backup_status"] = BackupStatus,
        };
    }
}

/// <summary>
/// The one assembly fixture of the integration tests (09 19.1 item 4): before the first test that asks for it, the four database users
/// and the template database are made by the SQL files of the database image's setup script, and the migration bundle is applied once
/// to the template as the schema owner; collections then copy it. The template is rebuilt on every run and dropped at the end, with the
/// users; nothing is kept between runs.
/// </summary>
public sealed class TemplateDatabaseFixture : IAsyncLifetime
{
    public const string TemplateName = "shopfolio_test_template";

    private string _adminDsn = string.Empty;

    /// <summary>
    /// Connection string of the template for tests, as the test server's administrator. Not pooled: an idle pooled connection would
    /// block the next copy (55006).
    /// </summary>
    public string TemplateDsn { get; private set; } = string.Empty;

    /// <summary>The users of this run.</summary>
    public TestDatabaseUsers Users { get; private set; } = TestDatabaseUsers.CreateForRun();

    public async ValueTask InitializeAsync()
    {
        _adminDsn = TestDatabaseSettings.GetRequired();
        DsnBuilder unpooled = new(WithDatabase(_adminDsn, TemplateName)) { Pooling = false };
        TemplateDsn = unpooled.ConnectionString;

        CancellationToken ct = TestContext.Current.CancellationToken;
        await DropDatabaseAsync(TemplateName, ct);
        await SweepLeftoverUsersAsync(ct);

        // The users, the database, its privileges and the schema are made by the files of the setup script (09 10.8 item 2); the role
        // settings of the schema owner (09 10.8 item 5) are among them, so the migrations of the bundle read the user names from them.
        Dictionary<string, string> variables = Users.Variables(TemplateName);
        await BootstrapScript.RunAsync(_adminDsn, BootstrapScript.RolesFile, variables, ct);

        // The file makes the users without a password (the setup script gives them theirs as SCRAM verifiers, not through SQL text);
        // the tests need to log in as them, so they set theirs here. Made-up hex values; the run's own throwaway server.
        await ExecuteAdminAsync($"ALTER ROLE {Users.Migrator} PASSWORD '{Users.MigratorPassword}'", ct);
        await ExecuteAdminAsync($"ALTER ROLE {Users.App} PASSWORD '{Users.AppPassword}'", ct);
        await ExecuteAdminAsync($"ALTER ROLE {Users.Reader} PASSWORD '{Users.ReaderPassword}'", ct);
        await BootstrapScript.RunAsync(_adminDsn, BootstrapScript.DatabaseFile, variables, ct);
        await BootstrapScript.RunAsync(_adminDsn, BootstrapScript.DatabaseAccessFile, variables, ct);
        await BootstrapScript.RunAsync(TemplateDsn, BootstrapScript.SchemaFile, variables, ct);

        using MigrationBundle bundle = await MigrationBundle.BuildAsync(ct);
        await bundle.ApplyAsync(DsnFor(TemplateName, Users.Migrator, Users.MigratorPassword, pooling: false), ct);

        ClearTemplatePool();
    }

    public async ValueTask DisposeAsync()
    {
        if (_adminDsn.Length == 0)
        {
            return;
        }

        CancellationToken ct = CancellationToken.None;
        await DropDatabaseAsync(TemplateName, ct);
        await DropUsersAsync(Users, ct);
    }

    /// <summary>
    /// Creates a new database from the template under a random name that starts with <c>shopfolio_test_</c> and returns its name. The
    /// copy is owned by the schema owner and gets the database privileges and the role settings again: a database copy takes neither along
    /// (09 10.8 items 2 and 5).
    /// </summary>
    public async Task<string> CreateDatabaseFromTemplateAsync(CancellationToken ct)
    {
        string name = "shopfolio_test_" + Convert.ToHexStringLower(RandomNumberGenerator.GetBytes(6));
        ClearTemplatePool();
        await ExecuteAdminAsync($"CREATE DATABASE {name} TEMPLATE {TemplateName} OWNER {Users.Migrator}", ct);
        await BootstrapScript.RunAsync(_adminDsn, BootstrapScript.DatabaseAccessFile, Users.Variables(name), ct);
        return name;
    }

    public Task DropDatabaseAsync(string name, CancellationToken ct)
    {
        return ExecuteAdminAsync($"DROP DATABASE IF EXISTS {name} WITH (FORCE)", ct);
    }

    /// <summary>Connection string of a database as the test server's administrator (the founding user).</summary>
    public string DsnFor(string databaseName)
    {
        return WithDatabase(_adminDsn, databaseName);
    }

    /// <summary>Connection string of a database as one of the four users: host and port of the test server, the user's name and password.</summary>
    public string DsnFor(string databaseName, string user, string? password, bool pooling = true)
    {
        DsnBuilder builder = new(WithDatabase(_adminDsn, databaseName))
        {
            Username = user,
            Password = password,
            Pooling = pooling,
        };
        return builder.ConnectionString;
    }

    public async Task<bool> DatabaseExistsAsync(string name, CancellationToken ct)
    {
        await using NpgsqlConnection connection = new(_adminDsn);
        await connection.OpenAsync(ct);
        await using NpgsqlCommand command = new("SELECT count(*) FROM pg_database WHERE datname = @name", connection);
        command.Parameters.AddWithValue("name", name);
        object? count = await command.ExecuteScalarAsync(ct);
        return Convert.ToInt64(count, System.Globalization.CultureInfo.InvariantCulture) > 0;
    }

    /// <summary>
    /// A database cannot be copied while another session is connected to it (09 19.1 item 4): empty the template's pool before every copy.
    /// </summary>
    private static void ClearTemplatePool()
    {
        // Every pool, not only the one of one exact connection string: a spelling difference gives another pool. Only idle connections are closed.
        NpgsqlConnection.ClearAllPools();
    }

    /// <summary>
    /// Drops the users of crashed earlier runs (their names start with <see cref="TestDatabaseUsers.Prefix"/>), as far as nothing depends
    /// on them any more. A user that still owns a database is left alone.
    /// </summary>
    private async Task SweepLeftoverUsersAsync(CancellationToken ct)
    {
        List<string> names = [];
        await using (NpgsqlConnection connection = new(_adminDsn))
        {
            await connection.OpenAsync(ct);
            await using NpgsqlCommand command = new("SELECT rolname FROM pg_roles WHERE rolname LIKE 'shopfolio\\_test\\_r%'", connection);
            await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                names.Add(reader.GetString(0));
            }
        }

        foreach (string name in names)
        {
            await TryDropUserAsync(name, ct);
        }
    }

    private async Task DropUsersAsync(TestDatabaseUsers users, CancellationToken ct)
    {
        ClearTemplatePool();
        foreach (string name in new[] { users.Migrator, users.App, users.Reader, users.BackupStatus })
        {
            await TryDropUserAsync(name, ct);
        }
    }

    private async Task TryDropUserAsync(string name, CancellationToken ct)
    {
        try
        {
            await ExecuteAdminAsync($"DROP ROLE IF EXISTS {name}", ct);
        }
        catch (PostgresException)
        {
            // Another database still depends on the user (a test database that was not dropped): leave it for the next sweep.
        }
    }

    private async Task ExecuteAdminAsync(string sql, CancellationToken ct)
    {
        await using NpgsqlConnection connection = new(_adminDsn);
        await connection.OpenAsync(ct);
        await using NpgsqlCommand command = new(sql, connection);
        await command.ExecuteNonQueryAsync(ct);
    }

    private static string WithDatabase(string dsn, string databaseName)
    {
        DsnBuilder builder = new(dsn);
        builder.Database = databaseName;
        return builder.ConnectionString;
    }
}
