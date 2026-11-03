using Microsoft.EntityFrameworkCore;

using Npgsql;

using Shopfolio.IntegrationTests.Database;
using Shopfolio.UnitTests.Samples;

namespace Shopfolio.IntegrationTests.Model;

/// <summary>
/// A database of its own for the model rule tests (T101): copied from the template, with the tables of the sample model made by the
/// script the model produces. The application has no migration of its own that makes tables yet (T103 brings the first), so the sample
/// tables come from the model's create script; the database is dropped when the tests of the class end.
/// </summary>
public sealed class SampleDatabaseFixture : IAsyncLifetime
{
    private readonly TemplateDatabaseFixture _template;

    public SampleDatabaseFixture(TemplateDatabaseFixture template)
    {
        _template = template;
    }

    public string DatabaseName { get; private set; } = string.Empty;

    /// <summary>Connection string of the sample database as the test server's administrator.</summary>
    public string Dsn { get; private set; } = string.Empty;

    public async ValueTask InitializeAsync()
    {
        CancellationToken ct = TestContext.Current.CancellationToken;
        DatabaseName = await _template.CreateDatabaseFromTemplateAsync(ct);
        Dsn = _template.DsnFor(DatabaseName);

        string script;
        await using (Shopfolio.Infrastructure.AppDbContext context = SampleModel.CreateGoodContext(Dsn))
        {
            script = context.Database.GenerateCreateScript();
        }

        await using NpgsqlConnection connection = new(Dsn);
        await connection.OpenAsync(ct);
        await using NpgsqlCommand command = new(script, connection);
        await command.ExecuteNonQueryAsync(ct);
    }

    public async ValueTask DisposeAsync()
    {
        if (DatabaseName.Length == 0)
        {
            return;
        }

        using NpgsqlConnection pooled = new(Dsn);
        NpgsqlConnection.ClearPool(pooled);
        await _template.DropDatabaseAsync(DatabaseName, CancellationToken.None);
    }

    public async Task<T?> ScalarAsync<T>(string sql, CancellationToken ct, params (string Name, object Value)[] parameters)
    {
        await using NpgsqlConnection connection = new(Dsn);
        await connection.OpenAsync(ct);
        await using NpgsqlCommand command = new(sql, connection);
        foreach ((string name, object value) in parameters)
        {
            command.Parameters.AddWithValue(name, value);
        }

        object? result = await command.ExecuteScalarAsync(ct);
        return result is null or DBNull ? default : (T)Convert.ChangeType(result, Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T), System.Globalization.CultureInfo.InvariantCulture);
    }

    public async Task ExecuteAsync(string sql, CancellationToken ct, params (string Name, object Value)[] parameters)
    {
        await using NpgsqlConnection connection = new(Dsn);
        await connection.OpenAsync(ct);
        await using NpgsqlCommand command = new(sql, connection);
        foreach ((string name, object value) in parameters)
        {
            command.Parameters.AddWithValue(name, value);
        }

        await command.ExecuteNonQueryAsync(ct);
    }
}
