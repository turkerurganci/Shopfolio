using System.Text.RegularExpressions;

using Microsoft.Extensions.Options;

using Shopfolio.Infrastructure.Integrations;
using Shopfolio.Infrastructure.Logging;

namespace Shopfolio.Infrastructure.InstallationSettings;

/// <summary>What the start-up check found: the failures (the application does not open) and the state of the conditional lines.</summary>
internal sealed record SettingsAuditResult(IReadOnlyList<string> Failures, IReadOnlyList<ConditionalLine> Lines)
{
    public bool Succeeded => Failures.Count == 0;
}

/// <summary>
/// The start-up check of all the settings (05 8.4.6; 09 3.5 item 2): binds every settings class from the configuration the application
/// really has (settings file, secret files and the rest), runs its validator, and looks for keys that name a product constant. It is a pure
/// function of the configuration, so a test needs no application. A failure names the setting and never prints its value (T226 KK5).
/// </summary>
internal static partial class SettingsAudit
{
    public static SettingsAuditResult Run(IConfiguration configuration)
    {
        List<string> failures = [.. ProductConstantNameGuard.FindKeys(configuration).Select(SettingFailures.ProductConstant)];

        AddFailures(configuration, DatabaseOptions.SectionName, new DatabaseOptionsValidator(), failures);
        AddFailures(configuration, SiteOptions.SectionName, new SiteOptionsValidator(), failures);
        AddFailures(configuration, EmailOptions.SectionName, new EmailOptionsValidator(), failures);
        AddCredentialShapeFailure(configuration, failures);
        AddFailures(configuration, LinkKeyOptions.SectionName, new LinkKeyOptionsValidator(), failures);
        AddFailures(configuration, RateLimitOptions.SectionName, new RateLimitOptionsValidator(), failures);
        AddFailures(configuration, StatusThresholdOptions.SectionName, new StatusThresholdOptionsValidator(), failures);
        AddFailures(configuration, LoggingOptions.SectionName, new LoggingOptionsValidator(), failures);

        PaymentOptions? payment = Bind<PaymentOptions>(configuration, PaymentOptions.SectionName, failures);
        GoogleOptions? google = Bind<GoogleOptions>(configuration, GoogleOptions.SectionName, failures);
        IReadOnlyList<ConditionalLine> lines = ConditionalLines.Evaluate(payment ?? new PaymentOptions(), google ?? new GoogleOptions());

        return new SettingsAuditResult(failures, lines);
    }

    // The e-mail credential's two-line shape (D-79) is checked where the e-mail service's gateway is the application's: with the development capture
    // setting the gateway never reads the credential, and the development file holds any non-empty text (09 4.3 item 2). A missing value is the
    // validator's failure above; this one names the key and never the value.
    private static void AddCredentialShapeFailure(IConfiguration configuration, List<string> failures)
    {
        string? credential = configuration[EmailOptions.CredentialKey];
        bool capture = !string.IsNullOrWhiteSpace(configuration[EmailCaptureOptions.HostKey]);
        if (!capture && !string.IsNullOrWhiteSpace(credential) && EmailCredential.Parse(credential) is null)
        {
            failures.Add(SettingFailures.Malformed(EmailOptions.CredentialKey, EmailCredential.Expectation));
        }
    }

    private static void AddFailures<T>(IConfiguration configuration, string section, IValidateOptions<T> validator, List<string> failures)
        where T : class, new()
    {
        T? options = Bind<T>(configuration, section, failures);
        if (options is null)
        {
            return;
        }

        ValidateOptionsResult result = validator.Validate(null, options);
        if (result.Failed)
        {
            failures.AddRange(result.Failures);
        }
    }

    // The binder fails when a value cannot be converted (a word where a number belongs). Its message names the key but its inner
    // exception quotes the value, so only the key is taken from it.
    private static T? Bind<T>(IConfiguration configuration, string section, List<string> failures)
        where T : class, new()
    {
        T options = new();
        try
        {
            configuration.GetSection(section).Bind(options);
            return options;
        }
        catch (InvalidOperationException exception)
        {
            Match key = ConvertedKey().Match(exception.Message);
            failures.Add(SettingFailures.Malformed(key.Success ? key.Groups[1].Value : section, "the value has the wrong type"));
            return null;
        }
    }

    [GeneratedRegex(@"at '([^']+)'", RegexOptions.CultureInvariant)]
    private static partial Regex ConvertedKey();
}
