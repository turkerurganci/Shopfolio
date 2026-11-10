using Amazon.SimpleEmailV2;

using Microsoft.Extensions.DependencyInjection;

using Shopfolio.Infrastructure.InstallationSettings;
using Shopfolio.Infrastructure.Integrations;
using Shopfolio.Infrastructure.Jobs;
using Shopfolio.Infrastructure.Values;
using Shopfolio.IntegrationTests.Database;
using Shopfolio.Modules.Notifications;

namespace Shopfolio.IntegrationTests.Web;

/// <summary>
/// The application's e-mail gateway in the opened application (T202 criterion 9, KK9; T407 criteria 2 and 3, KK3; 09 4.3 item 2, 11.2 item 1, 19.1 item 7): the setting
/// that <c>deploy/compose.dev.yaml</c> gives (<c>EmailCapture:Host</c>, <c>EmailCapture:Port</c>) makes the capture gateway the application's e-mail gateway; without it
/// the e-mail service's gateway is the application's, on the one client of the application; a host without a port stops the start and names the port's key; and
/// an e-mail credential that is not the two lines stops the start where the service's gateway is used, and only there. No test sends: the gateway is only resolved.
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
    public async Task Application_WithTheDevelopmentSetting_UsesTheCaptureGateway_AndMakesNoClientOfTheService()
    {
        await using ShopfolioWebApplicationFactory factory = new(
            _database.Dsn,
            new Dictionary<string, string> { [EmailCaptureOptions.HostKey] = "mailpit", [EmailCaptureOptions.PortKey] = "1025" },
            keepEmailGateway: true);
        using HttpClient client = factory.CreateClient();

        Assert.IsType<SmtpCaptureEmailGateway>(factory.Services.GetRequiredService<IEmailGateway>());
        Assert.Null(factory.Services.GetService<IAmazonSimpleEmailServiceV2>());
    }

    [Fact]
    public async Task Application_WithoutTheDevelopmentSetting_UsesTheServicesGateway_OnOneClient()
    {
        await using ShopfolioWebApplicationFactory factory = new(_database.Dsn, keepEmailGateway: true);
        using HttpClient client = factory.CreateClient();

        IEmailGateway gateway = factory.Services.GetRequiredService<IEmailGateway>();

        Assert.IsType<SesEmailGateway>(gateway);
        Assert.Same(gateway, factory.Services.GetRequiredService<IEmailGateway>());
        Assert.Same(factory.Services.GetRequiredService<IAmazonSimpleEmailServiceV2>(), factory.Services.GetRequiredService<IAmazonSimpleEmailServiceV2>());
        Assert.IsType<ExternalCallRunner>(factory.Services.GetRequiredService<ExternalCallRunner>());
    }

    [Fact]
    public async Task Application_WithNoGateway_Opens_AndTheReturnedAddressQueryRunsWithoutOne()
    {
        await using ShopfolioWebApplicationFactory factory = new(_database.Dsn);
        using HttpClient client = factory.CreateClient();

        Assert.Null(factory.Services.GetService<IEmailGateway>());
        await using AsyncServiceScope scope = factory.Services.CreateAsyncScope();
        BounceQueryJobHandler handler = scope.ServiceProvider.GetRequiredService<BounceQueryJobHandler>();

        JobOutcome outcome = await handler.HandleAsync(new JobWork(Guid.NewGuid(), JobType.BounceQuery, null, DateTimeOffset.UtcNow, 0), TestContext.Current.CancellationToken);

        Assert.Equal(JobOutcome.RescheduleAfter(JobTimings.BounceQueryInterval), outcome);
    }

    [Fact]
    public async Task Application_WithACaptureHostButNoPort_RefusesToStart_NamingThePortKey()
    {
        await using ShopfolioWebApplicationFactory factory = new(_database.Dsn, new Dictionary<string, string> { [EmailCaptureOptions.HostKey] = "mailpit" });

        Exception failure = Assert.ThrowsAny<Exception>(() => factory.CreateClient());

        Assert.Contains(EmailCaptureOptions.PortKey, failure.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task Application_WithAOneLineCredentialAndNoCaptureHost_RefusesToStart_NamingTheCredentialKeyAndNotItsValue()
    {
        await using ShopfolioWebApplicationFactory factory = new(
            _database.Dsn, new Dictionary<string, string> { [EmailOptions.CredentialKey] = "one-line-made-up-value-1234" });

        Exception failure = Assert.ThrowsAny<Exception>(() => factory.CreateClient());

        Assert.Contains(EmailOptions.CredentialKey, failure.ToString(), StringComparison.Ordinal);
        Assert.DoesNotContain("one-line-made-up-value-1234", failure.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task Application_WithAOneLineCredentialAndTheCaptureHost_Opens_BecauseTheCaptureGatewayNeverReadsIt()
    {
        await using ShopfolioWebApplicationFactory factory = new(
            _database.Dsn,
            new Dictionary<string, string>
            {
                [EmailCaptureOptions.HostKey] = "mailpit",
                [EmailCaptureOptions.PortKey] = "1025",
                [EmailOptions.CredentialKey] = "any-non-empty-development-text",
            },
            keepEmailGateway: true);

        using HttpClient client = factory.CreateClient();

        Assert.IsType<SmtpCaptureEmailGateway>(factory.Services.GetRequiredService<IEmailGateway>());
        Assert.NotNull(client);
    }
}
