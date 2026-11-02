using System.Net;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Shopfolio.Infrastructure;
using Shopfolio.IntegrationTests.Database;

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
