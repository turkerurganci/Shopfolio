using System.Text.RegularExpressions;

using Microsoft.Extensions.Options;

namespace Shopfolio.Infrastructure.InstallationSettings;

/// <summary>
/// The domain name of the installation (05 8.4.3; 09 3.5 item 2): a required installation setting from the settings file. Compose gives the
/// application the same value the reverse proxy reads (<c>SHOPFOLIO_DOMAIN</c> in the settings template), so the name is written once
/// (D-32 (a)).
/// </summary>
internal sealed class SiteOptions
{
    public const string SectionName = "Site";

    public const string DomainKey = SectionName + ":" + nameof(Domain);

    /// <summary>The host name only: no scheme, port or path (for example <c>shop.example.com</c>).</summary>
    public string Domain { get; set; } = string.Empty;
}

internal sealed partial class SiteOptionsValidator : IValidateOptions<SiteOptions>
{
    public ValidateOptionsResult Validate(string? name, SiteOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.Domain))
        {
            return ValidateOptionsResult.Fail(SettingFailures.Missing(SiteOptions.DomainKey));
        }

        return IsHostName(options.Domain)
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(SettingFailures.Malformed(SiteOptions.DomainKey, "expected a host name such as shop.example.com, without scheme, port or path"));
    }

    private static bool IsHostName(string value)
    {
        return value.Length <= 253 && (value == "localhost" || HostName().IsMatch(value));
    }

    // Two or more dot-separated labels of letters, digits and inner hyphens (punycode for non-Latin names).
    [GeneratedRegex(@"^(?=.{1,253}$)([A-Za-z0-9]([A-Za-z0-9-]{0,61}[A-Za-z0-9])?\.)+[A-Za-z]([A-Za-z0-9-]{0,61}[A-Za-z0-9])?$", RegexOptions.CultureInvariant)]
    private static partial Regex HostName();
}
