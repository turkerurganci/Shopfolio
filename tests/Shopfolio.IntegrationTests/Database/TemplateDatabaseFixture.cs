using System.Security.Cryptography;

using Npgsql;

using DsnBuilder = Npgsql.NpgsqlConnectionStringBuilder;

[assembly: AssemblyFixture(typeof(Shopfolio.IntegrationTests.Database.TemplateDatabaseFixture))]

namespace Shopfolio.IntegrationTests.Database;

/// <summary>
/// The one assembly fixture of the integration tests (09 19.1 item 4): before the first test that asks for it, the migration bundle
/// is applied once to a freshly created <c>shopfolio_test_template</c> database; collections then copy it. The template is rebuilt
/// on every run and dropped at the end; nothing is kept between runs.
/// </summary>
public sealed class TemplateDatabaseFixture : IAsyncLifetime
{
    public const string TemplateName = "shopfolio_test_template";

    private string _adminDsn = string.Empty;

    public string TemplateDsn { get; private set; } = string.Empty;

    public async ValueTask InitializeAsync()
    {
        _adminDsn = TestDatabaseSettings.GetRequired();
        TemplateDsn = WithDatabase(_adminDsn, TemplateName);

        CancellationToken ct = TestContext.Current.CancellationToken;
        await DropDatabaseAsync(TemplateName, ct);
        await ExecuteAdminAsync($"CREATE DATABASE {TemplateName}", ct);

        using MigrationBundle bundle = await MigrationBundle.BuildAsync(ct);
        await bundle.ApplyAsync(TemplateDsn, ct);

        // A database cannot be copied while another session is connected to it (09 19.1 item 4): empty the template's pool.
        using NpgsqlConnection pooled = new(TemplateDsn);
        NpgsqlConnection.ClearPool(pooled);
    }

    public async ValueTask DisposeAsync()
    {
        if (_adminDsn.Length == 0)
        {
            return;
        }

        await DropDatabaseAsync(TemplateName, CancellationToken.None);
    }

    /// <summary>
    /// Creates a new database from the template under a random name that starts with <c>shopfolio_test_</c> and returns its name.
    /// </summary>
    public async Task<string> CreateDatabaseFromTemplateAsync(CancellationToken ct)
    {
        string name = "shopfolio_test_" + Convert.ToHexStringLower(RandomNumberGenerator.GetBytes(6));
        await ExecuteAdminAsync($"CREATE DATABASE {name} TEMPLATE {TemplateName}", ct);
        return name;
    }

    public Task DropDatabaseAsync(string name, CancellationToken ct)
    {
        return ExecuteAdminAsync($"DROP DATABASE IF EXISTS {name} WITH (FORCE)", ct);
    }

    public string DsnFor(string databaseName)
    {
        return WithDatabase(_adminDsn, databaseName);
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
