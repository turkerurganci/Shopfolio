using Microsoft.Extensions.Configuration;

using Shopfolio.Infrastructure.InstallationSettings;
using Shopfolio.Infrastructure.Integrations;

namespace Shopfolio.UnitTests.InstallationSettings;

/// <summary>
/// The e-mail credential's content (T407 criterion 3, KK3; D-79; 08 2.2 "Kimlik doğrulama"): two lines - the access key id and the secret key. The start-up check refuses
/// the application when the e-mail service's gateway is the application's and the content has another shape, names the setting and never prints the value;
/// with the development capture setting the credential is never read, so any non-empty text opens the application. A missing sender address, region or
/// credential stops the start as before.
/// </summary>
public sealed class EmailCredentialTests
{
    public static TheoryData<string> Valid => new()
    {
        "AKIAMADEUPKEYID0001\nmade-up-secret-key-0002",
        "AKIAMADEUPKEYID0001\nmade-up-secret-key-0002\n",
        "AKIAMADEUPKEYID0001\r\nmade-up-secret-key-0002\r\n",
        "AKIAMADEUPKEYID0001\nmade-up/secret+key=0002\n\n",
    };

    [Theory]
    [MemberData(nameof(Valid))]
    public void Parse_TwoLines_GivesTheKeyIdAndTheSecretKey(string content)
    {
        EmailCredential? credential = EmailCredential.Parse(content);

        Assert.NotNull(credential);
        Assert.Equal("AKIAMADEUPKEYID0001", credential.AccessKeyId);
        Assert.StartsWith("made-up", credential.SecretKey, StringComparison.Ordinal);
        Assert.DoesNotContain('\n', credential.SecretKey);
        Assert.DoesNotContain('\r', credential.SecretKey);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\n")]
    [InlineData("only-one-line")]
    [InlineData("only-one-line\n")]
    [InlineData("keyid:secret")]
    [InlineData("keyid\n\nsecret")]
    [InlineData("\nkeyid\nsecret")]
    [InlineData("keyid\nsecret\nthird")]
    [InlineData("key id\nsecret")]
    [InlineData("keyid\nsec ret")]
    [InlineData("keyid\n \n")]
    public void Parse_AnyOtherContent_IsNull(string? content)
    {
        Assert.Null(EmailCredential.Parse(content));
    }

    [Fact]
    public void ToString_DoesNotPrintTheSecret()
    {
        EmailCredential credential = new("AKIAMADEUPKEYID0001", "made-up-secret-key-0002");

        Assert.DoesNotContain("made-up-secret-key-0002", credential.ToString(), StringComparison.Ordinal);
        Assert.DoesNotContain("AKIAMADEUPKEYID0001", credential.ToString(), StringComparison.Ordinal);
    }

    private static IConfiguration Configuration(string? credential, string? captureHost = null)
    {
        Dictionary<string, string?> settings = new(StringComparer.OrdinalIgnoreCase)
        {
            { DatabaseOptions.ConnectionStringKey, "Host=127.0.0.1;Database=shopfolio;Username=shopfolio_app;Password=made-up" },
            { SiteOptions.DomainKey, "shop.example.com" },
            { EmailOptions.SenderAddressKey, "info@example.com" },
            { EmailOptions.RegionKey, "eu-central-1" },
            { LinkKeyOptions.DerivationSecretKey, Convert.ToBase64String(new byte[LinkKeyOptions.DerivationSecretMinBytes]) },
        };
        if (credential is not null)
        {
            settings[EmailOptions.CredentialKey] = credential;
        }

        if (captureHost is not null)
        {
            settings[EmailCaptureOptions.HostKey] = captureHost;
            settings[EmailCaptureOptions.PortKey] = "1025";
        }

        return new ConfigurationBuilder().AddInMemoryCollection(settings).Build();
    }

    [Fact]
    public void Audit_WithTheTwoLines_Succeeds()
    {
        Assert.Empty(SettingsAudit.Run(Configuration("AKIAMADEUPKEYID0001\nmade-up-secret-key-0002")).Failures);
    }

    [Theory]
    [InlineData("one-line-made-up-value-7788")]
    [InlineData("a:b")]
    [InlineData("a\nb\nc")]
    public void Audit_WithAnotherShape_Fails_NamingTheKey_AndNotThePrintedValue(string content)
    {
        IReadOnlyList<string> failures = SettingsAudit.Run(Configuration(content)).Failures;

        string failure = Assert.Single(failures);
        Assert.Contains(EmailOptions.CredentialKey, failure, StringComparison.Ordinal);
        Assert.Contains(EmailCredential.Expectation, failure, StringComparison.Ordinal);
        Assert.DoesNotContain("made-up-value-7788", failure, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("one-line-made-up-value-7788")]
    [InlineData("any non-empty text")]
    public void Audit_WithTheDevelopmentCaptureHost_AcceptsAnyNonEmptyText(string content)
    {
        Assert.Empty(SettingsAudit.Run(Configuration(content, captureHost: "mailpit")).Failures);
    }

    [Fact]
    public void Audit_WithoutTheCredential_Fails_AsMissing_NotAsMalformed()
    {
        string failure = Assert.Single(SettingsAudit.Run(Configuration(null)).Failures);

        Assert.Equal(SettingFailures.Missing(EmailOptions.CredentialKey), failure);
    }

    [Fact]
    public void Audit_WithoutTheCredential_WithTheCaptureHost_StillFails_AsMissing()
    {
        string failure = Assert.Single(SettingsAudit.Run(Configuration(null, captureHost: "mailpit")).Failures);

        Assert.Equal(SettingFailures.Missing(EmailOptions.CredentialKey), failure);
    }

    [Theory]
    [InlineData(EmailOptions.SenderAddressKey)]
    [InlineData(EmailOptions.RegionKey)]
    [InlineData(EmailOptions.CredentialKey)]
    public void Audit_WithoutAnyOfTheThreeRequiredEmailSettings_Fails_NamingIt(string key)
    {
        Dictionary<string, string?> settings = new(StringComparer.OrdinalIgnoreCase)
        {
            { DatabaseOptions.ConnectionStringKey, "Host=127.0.0.1;Database=shopfolio;Username=shopfolio_app;Password=made-up" },
            { SiteOptions.DomainKey, "shop.example.com" },
            { EmailOptions.SenderAddressKey, "info@example.com" },
            { EmailOptions.RegionKey, "eu-central-1" },
            { EmailOptions.CredentialKey, "AKIAMADEUPKEYID0001\nmade-up-secret-key-0002" },
            { LinkKeyOptions.DerivationSecretKey, Convert.ToBase64String(new byte[LinkKeyOptions.DerivationSecretMinBytes]) },
        };
        settings.Remove(key);

        IReadOnlyList<string> failures = SettingsAudit.Run(new ConfigurationBuilder().AddInMemoryCollection(settings).Build()).Failures;

        Assert.Contains(SettingFailures.Missing(key), failures);
    }
}
