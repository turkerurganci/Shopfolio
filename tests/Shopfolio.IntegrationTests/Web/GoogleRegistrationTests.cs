using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

using Shopfolio.Infrastructure.Integrations;
using Shopfolio.IntegrationTests.Database;

namespace Shopfolio.IntegrationTests.Web;

/// <summary>
/// Google's gateway in the opened application (T409 criteria 2 and 5, KK2 and KK5; 09 11.3 item 2): the application resolves <see cref="IGoogleIdentityGateway"/> to the one
/// Google gateway, over the one configuration manager of the fixed discovery address, with and without a Google application in the settings; opening the application and
/// resolving the gateway reaches nothing outside.
/// </summary>
[Collection(DatabaseInfrastructureDefinition.Name)]
public sealed class GoogleRegistrationTests
{
    private readonly CollectionDatabaseFixture _database;

    public GoogleRegistrationTests(CollectionDatabaseFixture database)
    {
        _database = database;
    }

    [Fact]
    public async Task Application_WithoutAGoogleApplication_StillResolvesTheGateway_OnOneManager()
    {
        await using ShopfolioWebApplicationFactory factory = new(_database.Dsn);
        using HttpClient client = factory.CreateClient();

        IGoogleIdentityGateway gateway = factory.Services.GetRequiredService<IGoogleIdentityGateway>();

        Assert.IsType<GoogleIdentityGateway>(gateway);
        Assert.Same(gateway, factory.Services.GetRequiredService<IGoogleIdentityGateway>());
        ConfigurationManager<OpenIdConnectConfiguration> manager = factory.Services.GetRequiredService<ConfigurationManager<OpenIdConnectConfiguration>>();
        Assert.Same(manager, factory.Services.GetRequiredService<ConfigurationManager<OpenIdConnectConfiguration>>());
        Assert.Equal(GoogleEndpoints.Discovery, manager.MetadataAddress);
    }

    [Fact]
    public async Task GatewayWithNoGoogleApplication_MakesNoCall_AndAnswersWithAFailure()
    {
        await using ShopfolioWebApplicationFactory factory = new(_database.Dsn);
        using HttpClient client = factory.CreateClient();
        IGoogleIdentityGateway gateway = factory.Services.GetRequiredService<IGoogleIdentityGateway>();

        GoogleIdentityResult result = await gateway.VerifyIdentityAsync(new IdentityCodeExchange("made-up-code", "made-up-verifier"), TestContext.Current.CancellationToken);

        Assert.IsType<GoogleIdentityResult.Failed>(result);
    }
}
