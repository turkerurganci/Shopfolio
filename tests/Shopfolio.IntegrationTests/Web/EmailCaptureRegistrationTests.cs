using Microsoft.Extensions.DependencyInjection;

using Shopfolio.Infrastructure.Integrations;
using Shopfolio.IntegrationTests.Database;

namespace Shopfolio.IntegrationTests.Web;

/// <summary>
/// The development e-mail gateway in the opened application (T202 criterion 9, KK9; 09 4.3 item 2, 19.1 item 7): the setting that
/// <c>deploy/compose.dev.yaml</c> gives (<c>EmailCapture:Host</c>, <c>EmailCapture:Port</c>) makes the capture gateway the application's e-mail
/// gateway; without it the application opens with no e-mail gateway of the layer; a host without a port stops the start and names the port's key.
/// No test sends: the gateway is only resolved.
/// </summary>
[Collection(DatabaseInfrastructureDefinition.Name)]
public sealed class EmailCaptureRegistrationTests
{
    private readonly CollectionDatabaseFixture _database;

    public EmailCaptureRegistrationTests(CollectionDatabaseFixture database)
    {
        _database = database;
    }

    [Fact]
    public async Task Application_WithTheDevelopmentSetting_UsesTheCaptureGateway()
    {
        await using ShopfolioWebApplicationFactory factory = new(
            _database.Dsn, new Dictionary<string, string> { [EmailCaptureOptions.HostKey] = "mailpit", [EmailCaptureOptions.PortKey] = "1025" });
        using HttpClient client = factory.CreateClient();

        Assert.IsType<SmtpCaptureEmailGateway>(factory.Services.GetRequiredService<IEmailGateway>());
    }

    [Fact]
    public async Task Application_WithoutTheDevelopmentSetting_OpensWithNoEmailGatewayOfTheLayer()
    {
        await using ShopfolioWebApplicationFactory factory = new(_database.Dsn);
        using HttpClient client = factory.CreateClient();

        Assert.Null(factory.Services.GetService<IEmailGateway>());
        Assert.IsType<ExternalCallRunner>(factory.Services.GetRequiredService<ExternalCallRunner>());
    }

    [Fact]
    public async Task Application_WithACaptureHostButNoPort_RefusesToStart_NamingThePortKey()
    {
        await using ShopfolioWebApplicationFactory factory = new(_database.Dsn, new Dictionary<string, string> { [EmailCaptureOptions.HostKey] = "mailpit" });

        Exception failure = Assert.ThrowsAny<Exception>(() => factory.CreateClient());

        Assert.Contains(EmailCaptureOptions.PortKey, failure.ToString(), StringComparison.Ordinal);
    }
}
