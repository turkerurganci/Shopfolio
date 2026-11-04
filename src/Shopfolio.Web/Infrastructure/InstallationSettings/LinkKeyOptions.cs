using Microsoft.Extensions.Options;

namespace Shopfolio.Infrastructure.InstallationSettings;

/// <summary>
/// The derivation secret of the account links (05 6.4.4, 6.x.20; 09 15.1 item 4): a required installation setting that is the whole content of
/// the secret file <c>link_derivation_secret</c>. At least 32 bytes, Base64; the application does not open with a missing or shorter value.
/// </summary>
internal sealed class LinkKeyOptions
{
    public const string SectionName = "LinkKey";

    public const string DerivationSecretKey = SectionName + ":" + nameof(DerivationSecret);

    /// <summary>The smallest accepted size of the decoded secret, in bytes.</summary>
    public const int DerivationSecretMinBytes = 32;

    /// <summary>The Base64 text of the secret; it comes from a secret file only.</summary>
    public string DerivationSecret { get; set; } = string.Empty;
}

internal sealed class LinkKeyOptionsValidator : IValidateOptions<LinkKeyOptions>
{
    public ValidateOptionsResult Validate(string? name, LinkKeyOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.DerivationSecret))
        {
            return ValidateOptionsResult.Fail(SettingFailures.Missing(LinkKeyOptions.DerivationSecretKey));
        }

        return DecodedLength(options.DerivationSecret) >= LinkKeyOptions.DerivationSecretMinBytes
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(SettingFailures.Malformed(
                LinkKeyOptions.DerivationSecretKey,
                "expected Base64 text of at least " + LinkKeyOptions.DerivationSecretMinBytes + " bytes"));
    }

    private static int DecodedLength(string value)
    {
        try
        {
            return Convert.FromBase64String(value.Trim()).Length;
        }
        catch (FormatException)
        {
            return -1;
        }
    }
}
