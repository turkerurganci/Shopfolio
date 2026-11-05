using Microsoft.Extensions.Options;

using Shopfolio.Infrastructure.InstallationSettings;

namespace Shopfolio.Infrastructure.Integrations;

/// <summary>
/// The development setting that points the e-mail gateway at the capture tool's SMTP port (09 4.3 item 2, 19.1 item 7). Only the development
/// variant of Compose sets it (<c>deploy/compose.dev.yaml</c>); an installation never does, and it is not in the settings template. When
/// <see cref="Host"/> is set the application's <see cref="IEmailGateway"/> is <see cref="SmtpCaptureEmailGateway"/>, so no e-mail leaves the machine.
/// </summary>
internal sealed class EmailCaptureOptions
{
    public const string SectionName = "EmailCapture";

    public const string HostKey = SectionName + ":" + nameof(Host);

    public const string PortKey = SectionName + ":" + nameof(Port);

    /// <summary>The capture tool's host name inside the Compose network.</summary>
    public string Host { get; set; } = string.Empty;

    /// <summary>The capture tool's SMTP port; required when <see cref="Host"/> is set (no default in code: the Compose file names it).</summary>
    public int Port { get; set; }
}

/// <summary>A set host needs a port; without a host the setting is off and nothing is checked.</summary>
internal sealed class EmailCaptureOptionsValidator : IValidateOptions<EmailCaptureOptions>
{
    public ValidateOptionsResult Validate(string? name, EmailCaptureOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        if (string.IsNullOrWhiteSpace(options.Host) || options.Port is > 0 and <= 65535)
        {
            return ValidateOptionsResult.Success;
        }

        return ValidateOptionsResult.Fail(SettingFailures.Malformed(EmailCaptureOptions.PortKey, "expected a port number between 1 and 65535"));
    }
}
