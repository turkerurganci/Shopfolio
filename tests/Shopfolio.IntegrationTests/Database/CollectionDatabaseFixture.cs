using Npgsql;

namespace Shopfolio.IntegrationTests.Database;

/// <summary>
/// Collection fixture (09 19.1 item 4): the collection gets its own database copied from the template and drops it when the
/// collection ends. Tests in one collection run in sequence and set up their own data; collections never see each other's data.
/// </summary>
public sealed class CollectionDatabaseFixture : IAsyncLifetime
{
    private readonly TemplateDatabaseFixture _template;

    public CollectionDatabaseFixture(TemplateDatabaseFixture template)
    {
        _template = template;
    }

    public string DatabaseName { get; private set; } = string.Empty;

    public string Dsn { get; private set; } = string.Empty;

    public async ValueTask InitializeAsync()
    {
        DatabaseName = await _template.CreateDatabaseFromTemplateAsync(TestContext.Current.CancellationToken);
        Dsn = _template.DsnFor(DatabaseName);
    }

    public async ValueTask DisposeAsync()
    {
        if (DatabaseName.Length == 0)
        {
            return;
        }

        // Pooled connections would keep the database busy; the drop also forces remaining sessions off.
        using NpgsqlConnection pooled = new(Dsn);
        NpgsqlConnection.ClearPool(pooled);
        await _template.DropDatabaseAsync(DatabaseName, CancellationToken.None);
    }
}
