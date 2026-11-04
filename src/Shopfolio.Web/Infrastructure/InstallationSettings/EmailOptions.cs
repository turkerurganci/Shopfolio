using System.Net.Mail;
using System.Text.RegularExpressions;

using Microsoft.Extensions.Options;

namespace Shopfolio.Infrastructure.InstallationSettings;

/// <summary>
/// The e-mail sending settings (05 8.4.3, 8.4.6; 08 2.2): the sender address and the region of the sending service come from the settings
/// file; the sending credential (access key id and secret key) is the whole content of the secret file <c>email_credential</c> and is the
/// value of <see cref="CredentialKey"/>. All three are required: e-mail is not a line that can be closed (ÖK-3). The start-up check tests
/// that each exists and, where the documents give a shape, that it has it; it does not test the credential with the service (08 2.2).
/// </summary>
internal sealed class EmailOptions
{
    public const string SectionName = "Email";

    public const string SenderAddressKey = SectionName + ":" + nameof(SenderAddress);

    public const string RegionKey = SectionName + ":" + nameof(Region);

    public const string CredentialKey = SectionName + ":" + nameof(Credential);

    /// <summary>The address the mails are sent from: a plain address without a display name.</summary>
    public string SenderAddress { get; set; } = string.Empty;

    /// <summary>The region of the sending service (for example <c>eu-central-1</c>).</summary>
    public string Region { get; set; } = string.Empty;

    /// <summary>The sending credential; it comes from a secret file only.</summary>
    public string Credential { get; set; } = string.Empty;
}

internal sealed partial class EmailOptionsValidator : IValidateOptions<EmailOptions>
{
    public ValidateOptionsResult Validate(string? name, EmailOptions options)
    {
        List<string> failures = [];

        if (string.IsNullOrWhiteSpace(options.SenderAddress))
        {
            failures.Add(SettingFailures.Missing(EmailOptions.SenderAddressKey));
        }
        else if (!IsPlainAddress(options.SenderAddress))
        {
            failures.Add(SettingFailures.Malformed(EmailOptions.SenderAddressKey, "expected a plain e-mail address such as info@example.com"));
        }

        if (string.IsNullOrWhiteSpace(options.Region))
        {
            failures.Add(SettingFailures.Missing(EmailOptions.RegionKey));
        }
        else if (!RegionName().IsMatch(options.Region))
        {
            failures.Add(SettingFailures.Malformed(EmailOptions.RegionKey, "expected a region name such as eu-central-1"));
        }

        if (string.IsNullOrWhiteSpace(options.Credential))
        {
            failures.Add(SettingFailures.Missing(EmailOptions.CredentialKey));
        }

        return failures.Count == 0 ? ValidateOptionsResult.Success : ValidateOptionsResult.Fail(failures);
    }

    private static bool IsPlainAddress(string value)
    {
        return MailAddress.TryCreate(value, out MailAddress? address)
            && string.Equals(address.Address, value, StringComparison.Ordinal)
            && address.Host.Contains('.', StringComparison.Ordinal);
    }

    [GeneratedRegex(@"^[a-z]{2}(-[a-z]+)+-[0-9]$", RegexOptions.CultureInvariant)]
    private static partial Regex RegionName();
}
