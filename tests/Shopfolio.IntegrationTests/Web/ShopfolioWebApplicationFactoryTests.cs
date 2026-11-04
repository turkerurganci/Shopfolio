using System.Net;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Npgsql;

using Shopfolio.Infrastructure;
using Shopfolio.IntegrationTests.Database;

using DsnBuilder = Npgsql.NpgsqlConnectionStringBuilder;

namespace Shopfolio.IntegrationTests.Web;

[Collection(DatabaseInfrastructureDefinition.Name)]
public sealed class ShopfolioWebApplicationFactoryTests
{
    private readonly CollectionDatabaseFixture _database;

    public ShopfolioWebApplicationFactoryTests(CollectionDatabaseFixture database)
    {
        _database = database;
    }

    [Fact]
    public async Task Request_ToUnmappedAddress_IsAnsweredByTheApplicationWith404()
    {
        // The first real endpoint test (the liveness endpoint) is T01's; this one proves the application opens and answers.
        CancellationToken ct = TestContext.Current.CancellationToken;
        await using ShopfolioWebApplicationFactory factory = new(_database.Dsn);
        using HttpClient client = factory.CreateClient();

        using HttpResponseMessage response = await client.GetAsync(new Uri("/not-mapped", UriKind.Relative), ct);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DatabaseConnections_OfTheWebHost_CarryTheWebSessionProfile()
    {
        // 09 10.7 item 2: the web host chooses the Web profile for the data source and the context alike; a raw installation string would carry no time.
        CancellationToken ct = TestContext.Current.CancellationToken;
        await using ShopfolioWebApplicationFactory factory = new(_database.Dsn);
        using HttpClient client = factory.CreateClient();
        await using AsyncServiceScope scope = factory.Services.CreateAsyncScope();
        NpgsqlDataSource dataSource = factory.Services.GetRequiredService<NpgsqlDataSource>();
        AppDbContext context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        DsnBuilder fromDataSource = new(dataSource.ConnectionString);
        DsnBuilder fromContext = new(context.Database.GetConnectionString());
        DsnBuilder expected = new(_database.Dsn);
        DatabaseConnection.Configure(expected, DatabaseSessionProfile.Web);

        Assert.Equal(expected.Options, fromDataSource.Options);
        Assert.Equal(expected.CommandTimeout, fromDataSource.CommandTimeout);
        Assert.Equal(expected.Options, fromContext.Options);
        Assert.Equal(expected.CommandTimeout, fromContext.CommandTimeout);
        Assert.Contains($"statement_timeout={(int)DatabaseTimeouts.StatementTimeout.TotalMilliseconds}", fromDataSource.Options, StringComparison.Ordinal);
        await using NpgsqlConnection connection = await dataSource.OpenConnectionAsync(ct);
        await using NpgsqlCommand command = new("SELECT setting FROM pg_settings WHERE name = 'statement_timeout'", connection);
        Assert.Equal(((int)DatabaseTimeouts.StatementTimeout.TotalMilliseconds).ToString(System.Globalization.CultureInfo.InvariantCulture), (string?)await command.ExecuteScalarAsync(ct));
    }

    [Fact]
    public async Task DatabaseContext_FromTheOpenedApplication_PointsAtTheCollectionDatabase()
    {
        CancellationToken ct = TestContext.Current.CancellationToken;
        await using ShopfolioWebApplicationFactory factory = new(_database.Dsn);
        using HttpClient client = factory.CreateClient();
        await using AsyncServiceScope scope = factory.Services.CreateAsyncScope();
        AppDbContext context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        bool canConnect = await context.Database.CanConnectAsync(ct);

        Assert.True(canConnect);
        Assert.Equal(_database.DatabaseName, context.Database.GetDbConnection().Database);
    }
}
