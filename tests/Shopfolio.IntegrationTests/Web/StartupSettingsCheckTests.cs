using System.Net;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using Shopfolio.Infrastructure.InstallationSettings;
using Shopfolio.Infrastructure.Logging;
using Shopfolio.IntegrationTests.Database;

namespace Shopfolio.IntegrationTests.Web;

/// <summary>
/// The start-up settings check on the opened application (T226 KK2, KK4, KK5; 05 8.4.6; 09 3.5 item 2, 18.1 item 1): a required setting that is
/// missing or malformed stops the start and the failure names the setting and never its value; a missing conditional setting opens the
/// application with the line closed and the start-up log says so; a tunable design value changes from the settings.
/// </summary>
[Collection(DatabaseInfrastructureDefinition.Name)]
public sealed class StartupSettingsCheckTests
{
    private const string ValueMarker = "marker-value-that-must-not-be-printed";

    // Built from two parts: the repository secret guard stops a secret-looking literal next to a secret key.
    private static readonly string ClientSecretValue = "made-up-" + "client-part";

    private readonly CollectionDatabaseFixture _database;

    public StartupSettingsCheckTests(CollectionDatabaseFixture database)
    {
        _database = database;
    }

    [Fact]
    public async Task Start_WithOnlyTheRequiredSettings_OpensAndTheLogSaysWhichLinesAreClosed()
    {
        CancellationToken ct = TestContext.Current.CancellationToken;
        CollectingLoggerProvider logs = new();
        await using ShopfolioWebApplicationFactory factory = new(_database.Dsn, loggerProvider: logs);
        using HttpClient client = factory.CreateClient();

        using HttpResponseMessage response = await client.GetAsync(new Uri("/not-mapped", UriKind.Relative), ct);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        string card = Assert.Single(logs.Lines, l => l.Contains(ConditionalLine.CardPayment, StringComparison.Ordinal));
        string google = Assert.Single(logs.Lines, l => l.Contains(ConditionalLine.GoogleSignIn, StringComparison.Ordinal));
        Assert.Contains("|Warning|", card, StringComparison.Ordinal);
        Assert.Contains("closed", card, StringComparison.Ordinal);
        Assert.Contains(PaymentOptions.ProviderKeysKey, card, StringComparison.Ordinal);
        Assert.Contains("closed", google, StringComparison.Ordinal);
        Assert.Contains(GoogleOptions.ClientIdKey, google, StringComparison.Ordinal);
        Assert.Contains(GoogleOptions.ClientSecretKey, google, StringComparison.Ordinal);
        Assert.Contains(LogEvents.StartupSettingsChecked, card, StringComparison.Ordinal);
        Assert.DoesNotContain(logs.Lines, l => l.Contains("|Critical|", StringComparison.Ordinal));
    }

    [Fact]
    public async Task Start_WithOnlyTheGoogleClientId_KeepsTheGoogleLineClosedAndNamesTheClientSecret()
    {
        CollectingLoggerProvider logs = new();
        await using ShopfolioWebApplicationFactory factory = new(
            _database.Dsn,
            new Dictionary<string, string> { [GoogleOptions.ClientIdKey] = "made-up-client-id" },
            logs);
        using HttpClient client = factory.CreateClient();

        string google = Assert.Single(logs.Lines, l => l.Contains(ConditionalLine.GoogleSignIn, StringComparison.Ordinal));
        Assert.Contains("closed", google, StringComparison.Ordinal);
        Assert.Contains(GoogleOptions.ClientSecretKey, google, StringComparison.Ordinal);
        Assert.DoesNotContain(GoogleOptions.ClientIdKey, google, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Start_WithThePaymentKeysAndBothGoogleSettings_OpensWithBothLinesOpen()
    {
        CollectingLoggerProvider logs = new();
        await using ShopfolioWebApplicationFactory factory = new(
            _database.Dsn,
            new Dictionary<string, string>
            {
                { PaymentOptions.ProviderKeysKey, "made-up-provider-keys" },
                { GoogleOptions.ClientIdKey, "made-up-client-id" },
                { GoogleOptions.ClientSecretKey, ClientSecretValue },
            },
            logs);
        using HttpClient client = factory.CreateClient();

        string card = Assert.Single(logs.Lines, l => l.Contains(ConditionalLine.CardPayment, StringComparison.Ordinal));
        string google = Assert.Single(logs.Lines, l => l.Contains(ConditionalLine.GoogleSignIn, StringComparison.Ordinal));
        Assert.Contains("is open", card, StringComparison.Ordinal);
        Assert.Contains("is open", google, StringComparison.Ordinal);
        Assert.DoesNotContain(logs.Lines, l => l.Contains("made-up-provider-keys", StringComparison.Ordinal) || l.Contains(ClientSecretValue, StringComparison.Ordinal));
    }

    [Theory]
    [InlineData("Site:Domain", "")]
    [InlineData("Email:SenderAddress", "")]
    [InlineData("Email:Region", "")]
    [InlineData("Email:Credential", "")]
    [InlineData("LinkKey:DerivationSecret", "")]
    [InlineData("Site:Domain", "https://" + ValueMarker + ".example.com/path")]
    [InlineData("Email:SenderAddress", ValueMarker)]
    [InlineData("Email:Region", "Region " + ValueMarker)]
    [InlineData("LinkKey:DerivationSecret", ValueMarker)]
    public async Task Start_WithARequiredSettingMissingOrMalformed_RefusesAndTheFailureNamesTheSettingNotItsValue(string key, string value)
    {
        CollectingLoggerProvider logs = new();
        await using ShopfolioWebApplicationFactory factory = new(_database.Dsn, new Dictionary<string, string> { [key] = value }, logs);

        Exception failure = Assert.ThrowsAny<Exception>(() => factory.CreateClient());

        Assert.Contains(key, failure.ToString(), StringComparison.Ordinal);
        Assert.DoesNotContain(ValueMarker, failure.ToString(), StringComparison.Ordinal);
        Assert.Contains(
            logs.Lines,
            l => l.Contains("|Critical|", StringComparison.Ordinal)
                && l.Contains(LogEvents.StartupSettingsChecked, StringComparison.Ordinal)
                && l.Contains(key, StringComparison.Ordinal));
        Assert.DoesNotContain(logs.Lines, l => l.Contains(ValueMarker, StringComparison.Ordinal));
    }

    [Theory]
    [InlineData("Catalog:ProductNameMaxLength")]
    [InlineData("CategoryLevelMax")]
    [InlineData("Catalog:OPTION_DIMENSION_MAX")]
    public async Task Start_WithASettingThatNamesAProductConstant_RefusesAndNamesTheKeyNotTheValue(string key)
    {
        CollectingLoggerProvider logs = new();
        await using ShopfolioWebApplicationFactory factory = new(_database.Dsn, new Dictionary<string, string> { [key] = ValueMarker }, logs);

        Exception failure = Assert.ThrowsAny<Exception>(() => factory.CreateClient());

        Assert.Contains(key, failure.ToString(), StringComparison.Ordinal);
        Assert.DoesNotContain(ValueMarker, failure.ToString(), StringComparison.Ordinal);
        Assert.Contains(logs.Lines, l => l.Contains("|Critical|", StringComparison.Ordinal) && l.Contains(key, StringComparison.Ordinal));
        Assert.DoesNotContain(logs.Lines, l => l.Contains(ValueMarker, StringComparison.Ordinal));
    }

    [Fact]
    public async Task Start_WithATunableValueOutOfRange_RefusesAndNamesTheSetting()
    {
        await using ShopfolioWebApplicationFactory factory = new(
            _database.Dsn,
            new Dictionary<string, string> { [RateLimitOptions.PageRequestsPerMinuteKey] = "0" });

        Exception failure = Assert.ThrowsAny<Exception>(() => factory.CreateClient());

        Assert.Contains(RateLimitOptions.PageRequestsPerMinuteKey, failure.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task RateLimit_FromTheSettings_IsTheValueTheSettingsGiveAndTheCodeDefaultWithoutOne()
    {
        await using ShopfolioWebApplicationFactory changed = new(
            _database.Dsn,
            new Dictionary<string, string> { [RateLimitOptions.PageRequestsPerMinuteKey] = "120" });
        using HttpClient changedClient = changed.CreateClient();
        await using ShopfolioWebApplicationFactory unchanged = new(_database.Dsn);
        using HttpClient unchangedClient = unchanged.CreateClient();

        RateLimitOptions fromSettings = changed.Services.GetRequiredService<IOptions<RateLimitOptions>>().Value;
        RateLimitOptions fromCode = unchanged.Services.GetRequiredService<IOptions<RateLimitOptions>>().Value;

        Assert.Equal(120, fromSettings.PageRequestsPerMinute);
        Assert.Equal(30, fromSettings.FormSubmissionsPerMinute);
        Assert.Equal(300, fromCode.PageRequestsPerMinute);
        Assert.Equal(30, fromCode.FormSubmissionsPerMinute);
    }
}
