using System.Reflection;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.InstallationSettings;

namespace Shopfolio.UnitTests.InstallationSettings;

/// <summary>
/// The start-up check of the settings (T226; 05 8.4.6; 09 3.5 item 2) as a pure function of the configuration: required settings, malformed
/// values, conditional lines, tunable design values and product constant names. No application and no file is needed; a failure names the
/// setting and never carries its value (KK5).
/// </summary>
public sealed class SettingsAuditTests
{
    private const string MadeUpMarker = "value-that-must-never-be-printed";

    private static readonly string ClientSecretValue = "made-up-" + "client-part";

    private static readonly string MadeUpConnection = "Host=localhost;Database=made_up;Username=made_up";

    public static TheoryData<string> RequiredKeys() =>
        [
            SiteOptions.DomainKey,
            EmailOptions.SenderAddressKey,
            EmailOptions.RegionKey,
            EmailOptions.CredentialKey,
            DatabaseOptions.ConnectionStringKey,
            LinkKeyOptions.DerivationSecretKey,
        ];

    public static TheoryData<string> ProductConstantNames()
    {
        TheoryData<string> names = [];
        foreach (FieldInfo field in typeof(ProductConstants).GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static).Where(f => f.IsLiteral))
        {
            names.Add(field.Name);
        }

        return names;
    }

    [Fact]
    public void Run_OnTheRequiredSettings_Succeeds()
    {
        SettingsAuditResult result = SettingsAudit.Run(Configuration(Valid()));

        Assert.Empty(result.Failures);
        Assert.True(result.Succeeded);
    }

    [Theory]
    [MemberData(nameof(RequiredKeys))]
    public void Run_WithARequiredSettingMissing_FailsAndNamesTheSetting(string key)
    {
        Dictionary<string, string?> settings = Valid();
        settings.Remove(key);

        SettingsAuditResult result = SettingsAudit.Run(Configuration(settings));

        string failure = Assert.Single(result.Failures);
        Assert.Contains(key, failure, StringComparison.Ordinal);
        Assert.False(result.Succeeded);
    }

    [Theory]
    [MemberData(nameof(RequiredKeys))]
    public void Run_WithARequiredSettingBlank_FailsAndNamesTheSetting(string key)
    {
        Dictionary<string, string?> settings = Valid();
        settings[key] = "   ";

        SettingsAuditResult result = SettingsAudit.Run(Configuration(settings));

        Assert.Contains(result.Failures, f => f.Contains(key, StringComparison.Ordinal));
    }

    [Theory]
    [InlineData("Site:Domain", "https://shop.example.com")]
    [InlineData("Site:Domain", "shop.example.com:8443")]
    [InlineData("Site:Domain", "shop.example.com/path")]
    [InlineData("Site:Domain", "shopnodot")]
    [InlineData("Email:SenderAddress", "not-an-address")]
    [InlineData("Email:SenderAddress", "Shop <info@example.com>")]
    [InlineData("Email:Region", "Frankfurt")]
    [InlineData("LinkKey:DerivationSecret", "not base64 at all!")]
    [InlineData("LinkKey:DerivationSecret", "c2hvcnQ=")]
    public void Run_WithAMalformedRequiredSetting_FailsNamesTheSettingAndNeverPrintsTheValue(string key, string value)
    {
        Dictionary<string, string?> settings = Valid();
        settings[key] = value;

        SettingsAuditResult result = SettingsAudit.Run(Configuration(settings));

        string failure = Assert.Single(result.Failures);
        Assert.Contains(key, failure, StringComparison.Ordinal);
        Assert.DoesNotContain(value, failure, StringComparison.Ordinal);
    }

    [Fact]
    public void Run_WithNoTunableValue_KeepsTheCodeDefaults()
    {
        RateLimitOptions rate = new();
        StatusThresholdOptions status = new();
        LoggingOptions logging = new();

        Assert.Equal(300, rate.PageRequestsPerMinute);
        Assert.Equal(30, rate.FormSubmissionsPerMinute);
        Assert.Equal(0.85, status.DiskUsageFraction);
        Assert.Equal(LogLevel.Information, logging.Level);
        Assert.Empty(SettingsAudit.Run(Configuration(Valid())).Failures);
    }

    [Fact]
    public void Run_WithATunableValueChanged_Succeeds()
    {
        Dictionary<string, string?> settings = Valid();
        settings[RateLimitOptions.PageRequestsPerMinuteKey] = "120";
        settings["StatusThreshold:BackupAgeHours"] = "30";
        settings[LoggingOptions.LevelKey] = "Warning";

        Assert.Empty(SettingsAudit.Run(Configuration(settings)).Failures);
    }

    [Theory]
    [InlineData("RateLimit:PageRequestsPerMinute", "0")]
    [InlineData("RateLimit:FormSubmissionsPerMinute", "-1")]
    [InlineData("StatusThreshold:DiskUsageFraction", "1")]
    [InlineData("StatusThreshold:DiskUsageFraction", "0")]
    [InlineData("StatusThreshold:HeartbeatAgeMinutes", "0")]
    [InlineData("StatusThreshold:NightlyJobAgeHours", "-5")]
    [InlineData("Logging:Level", "None")]
    [InlineData("Logging:Level", "Loud")]
    [InlineData("RateLimit:PageRequestsPerMinute", MadeUpMarker)]
    [InlineData("StatusThreshold:BackupAgeHours", MadeUpMarker)]
    public void Run_WithATunableValueOutOfRange_FailsNamesTheSettingAndNeverPrintsTheValue(string key, string value)
    {
        Dictionary<string, string?> settings = Valid();
        settings[key] = value;

        SettingsAuditResult result = SettingsAudit.Run(Configuration(settings));

        string failure = Assert.Single(result.Failures);
        Assert.Contains(key, failure, StringComparison.Ordinal);
        if (value.Length > 3)
        {
            Assert.DoesNotContain(value, failure, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void Run_WithNoPaymentKeysAndNoGoogle_OpensWithBothLinesClosed()
    {
        SettingsAuditResult result = SettingsAudit.Run(Configuration(Valid()));

        Assert.True(result.Succeeded);
        ConditionalLine card = Assert.Single(result.Lines, l => l.Name == ConditionalLine.CardPayment);
        ConditionalLine google = Assert.Single(result.Lines, l => l.Name == ConditionalLine.GoogleSignIn);
        Assert.Equal([PaymentOptions.ProviderKeysKey], card.MissingKeys);
        Assert.Equal([GoogleOptions.ClientIdKey, GoogleOptions.ClientSecretKey], google.MissingKeys);
    }

    [Fact]
    public void Run_WithPaymentKeysAndBothGoogleSettings_OpensWithBothLinesOpen()
    {
        Dictionary<string, string?> settings = Valid();
        settings.Add(PaymentOptions.ProviderKeysKey, "made-up-provider-keys");
        settings[GoogleOptions.ClientIdKey] = "made-up-client-id";
        settings.Add(GoogleOptions.ClientSecretKey, ClientSecretValue);

        SettingsAuditResult result = SettingsAudit.Run(Configuration(settings));

        Assert.True(result.Succeeded);
        Assert.All(result.Lines, line => Assert.True(line.IsOpen));
    }

    [Fact]
    public void Run_WithOnlyTheGoogleClientId_KeepsTheGoogleLineClosedAndNamesTheClientSecret()
    {
        Dictionary<string, string?> settings = Valid();
        settings[GoogleOptions.ClientIdKey] = "made-up-client-id";

        SettingsAuditResult result = SettingsAudit.Run(Configuration(settings));

        Assert.True(result.Succeeded);
        ConditionalLine google = Assert.Single(result.Lines, l => l.Name == ConditionalLine.GoogleSignIn);
        Assert.False(google.IsOpen);
        Assert.Equal([GoogleOptions.ClientSecretKey], google.MissingKeys);
    }

    [Theory]
    [MemberData(nameof(ProductConstantNames))]
    public void Run_WithAKeyThatNamesAProductConstant_RefusesAndNamesTheKeyNotTheValue(string constantName)
    {
        Dictionary<string, string?> settings = Valid();
        settings["Catalog:" + constantName] = MadeUpMarker;

        SettingsAuditResult result = SettingsAudit.Run(Configuration(settings));

        string failure = Assert.Single(result.Failures);
        Assert.Contains("Catalog:" + constantName, failure, StringComparison.Ordinal);
        Assert.DoesNotContain(MadeUpMarker, failure, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("CategoryLevelMax")]
    [InlineData("CATEGORY_LEVEL_MAX")]
    [InlineData("Category-Level-Max")]
    [InlineData("Catalog:ProductNameMaxLength")]
    [InlineData("ProductNameMaxLength")]
    public void Run_WithAProductConstantNameInAnyWriting_Refuses(string key)
    {
        Dictionary<string, string?> settings = Valid();
        settings[key] = "5";

        SettingsAuditResult result = SettingsAudit.Run(Configuration(settings));

        Assert.Single(result.Failures);
    }

    [Fact]
    public void FindKeys_WithASampleClass_FindsOnlyItsConstantNames()
    {
        IConfiguration configuration = Configuration(new Dictionary<string, string?>
        {
            ["Sample:WidgetLimit"] = "1",
            ["Sample:Other"] = "2",
            ["SAMPLE_WIDGET_LIMIT"] = "3",
        });

        IReadOnlyList<string> found = ProductConstantNameGuard.FindKeys(configuration, typeof(SampleConstants));

        Assert.Equal(2, found.Count);
        Assert.Contains("Sample:WidgetLimit", found);
        Assert.Contains("SAMPLE_WIDGET_LIMIT", found);
    }

    [Fact]
    public void FindKeys_OnTheRequiredAndTunableSettings_FindsNothing()
    {
        Dictionary<string, string?> settings = Valid();
        settings[RateLimitOptions.PageRequestsPerMinuteKey] = "120";

        Assert.Empty(ProductConstantNameGuard.FindKeys(Configuration(settings)));
    }

    private static Dictionary<string, string?> Valid() => new(StringComparer.OrdinalIgnoreCase)
    {
        { DatabaseOptions.ConnectionStringKey, MadeUpConnection },
        { SiteOptions.DomainKey, "shop.example.com" },
        { EmailOptions.SenderAddressKey, "info@example.com" },
        { EmailOptions.RegionKey, "eu-central-1" },
        { EmailOptions.CredentialKey, "made-up-credential" },
        { LinkKeyOptions.DerivationSecretKey, Convert.ToBase64String(new byte[LinkKeyOptions.DerivationSecretMinBytes]) },
    };

    private static IConfiguration Configuration(Dictionary<string, string?> settings) =>
        new ConfigurationBuilder().AddInMemoryCollection(settings).Build();

    private static class SampleConstants
    {
        public const int WidgetLimit = 4;

        public const int SampleWidgetLimit = 4;
    }
}
