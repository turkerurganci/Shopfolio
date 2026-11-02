using Npgsql;

using DsnBuilder = Npgsql.NpgsqlConnectionStringBuilder;

namespace Shopfolio.IntegrationTests.Database;

[Collection(DatabaseInfrastructureDefinition.Name)]
public sealed class TestDatabaseInfrastructureTests
{
    private readonly TemplateDatabaseFixture _template;
    private readonly CollectionDatabaseFixture _collection;

    public TestDatabaseInfrastructureTests(TemplateDatabaseFixture template, CollectionDatabaseFixture collection)
    {
        _template = template;
        _collection = collection;
    }

    [Fact]
    public async Task Template_AfterBundleRun_ExistsWithMigrationHistoryTable()
    {
        CancellationToken ct = TestContext.Current.CancellationToken;

        bool exists = await _template.DatabaseExistsAsync(TemplateDatabaseFixture.TemplateName, ct);
        bool hasHistory = await TableExistsAsync(_template.TemplateDsn, "__EFMigrationsHistory", ct);

        Assert.True(exists);
        Assert.True(hasHistory);
    }

    [Fact]
    public async Task CollectionDatabase_CopiedFromTemplate_HasTemplateSchemaAndRandomName()
    {
        CancellationToken ct = TestContext.Current.CancellationToken;

        bool hasHistory = await TableExistsAsync(_collection.Dsn, "__EFMigrationsHistory", ct);

        Assert.StartsWith("shopfolio_test_", _collection.DatabaseName, StringComparison.Ordinal);
        Assert.NotEqual(TemplateDatabaseFixture.TemplateName, _collection.DatabaseName);
        Assert.True(hasHistory);
    }

    [Fact]
    public async Task CollectionDatabase_AfterFixtureDispose_IsDropped()
    {
        CancellationToken ct = TestContext.Current.CancellationToken;
        CollectionDatabaseFixture own = new(_template);

        await own.InitializeAsync();
        bool existedBefore = await _template.DatabaseExistsAsync(own.DatabaseName, ct);
        await own.DisposeAsync();
        bool existsAfter = await _template.DatabaseExistsAsync(own.DatabaseName, ct);

        Assert.True(existedBefore);
        Assert.False(existsAfter);
    }

    [Fact]
    public async Task CollectionDatabases_OfTwoFixtures_AreIsolatedFromEachOther()
    {
        CancellationToken ct = TestContext.Current.CancellationToken;
        CollectionDatabaseFixture other = new(_template);
        await other.InitializeAsync();
        try
        {
            await ExecuteAsync(_collection.Dsn, "CREATE TABLE isolation_probe (id integer)", ct);

            bool inOwn = await TableExistsAsync(_collection.Dsn, "isolation_probe", ct);
            bool inOther = await TableExistsAsync(other.Dsn, "isolation_probe", ct);
            bool inTemplate = await TableExistsAsync(_template.TemplateDsn, "isolation_probe", ct);

            Assert.True(inOwn);
            Assert.False(inOther);
            Assert.False(inTemplate);
        }
        finally
        {
            await ExecuteAsync(_collection.Dsn, "DROP TABLE IF EXISTS isolation_probe", ct);
            await other.DisposeAsync();
        }
    }

    [Fact]
    public async Task CreateDatabaseFromTemplate_AfterPooledConnectionToTemplate_Succeeds()
    {
        CancellationToken ct = TestContext.Current.CancellationToken;
        DsnBuilder poolingTarget = new(_template.TemplateDsn);
        poolingTarget.Pooling = true;
        await using (NpgsqlConnection idle = new(poolingTarget.ConnectionString))
        {
            await idle.OpenAsync(ct);
        }

        // The connection is now idle in the pool of the template; the copy must still work whatever order the tests run in.
        string name = await _template.CreateDatabaseFromTemplateAsync(ct);
        bool exists = await _template.DatabaseExistsAsync(name, ct);
        await _template.DropDatabaseAsync(name, ct);

        Assert.True(exists);
    }

    private static async Task<bool> TableExistsAsync(string connectionString, string table, CancellationToken ct)
    {
        await using NpgsqlConnection connection = new(connectionString);
        await connection.OpenAsync(ct);
        await using NpgsqlCommand command = new("SELECT count(*) FROM information_schema.tables WHERE table_schema = 'public' AND table_name = @table", connection);
        command.Parameters.AddWithValue("table", table);
        object? count = await command.ExecuteScalarAsync(ct);
        return Convert.ToInt64(count, System.Globalization.CultureInfo.InvariantCulture) > 0;
    }

    private static async Task ExecuteAsync(string connectionString, string sql, CancellationToken ct)
    {
        await using NpgsqlConnection connection = new(connectionString);
        await connection.OpenAsync(ct);
        await using NpgsqlCommand command = new(sql, connection);
        await command.ExecuteNonQueryAsync(ct);
    }
}
