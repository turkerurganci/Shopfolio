using Microsoft.Extensions.Options;

using Shopfolio.Infrastructure.Logging;

namespace Shopfolio.Infrastructure.InstallationSettings;

/// <summary>
/// Registers the settings classes and the start-up check (05 8.4.6; 09 3.5 item 2). The check is the first hosted service, so it runs before
/// the web server starts listening: a required setting that is missing or malformed stops the application before it answers a request.
/// </summary>
internal static class InstallationSettingsRegistration
{
    public static IServiceCollection AddInstallationSettings(this IServiceCollection services)
    {
        // First hosted service of the application (the web server is added after the application's own services at Build).
        services.AddHostedService<StartupSettingsCheck>();

        services.AddOptions<SiteOptions>().BindConfiguration(SiteOptions.SectionName);
        services.AddOptions<EmailOptions>().BindConfiguration(EmailOptions.SectionName);
        services.AddOptions<LinkKeyOptions>().BindConfiguration(LinkKeyOptions.SectionName);
        services.AddOptions<PaymentOptions>().BindConfiguration(PaymentOptions.SectionName);
        services.AddOptions<GoogleOptions>().BindConfiguration(GoogleOptions.SectionName);
        services.AddOptions<RateLimitOptions>().BindConfiguration(RateLimitOptions.SectionName);
        services.AddOptions<StatusThresholdOptions>().BindConfiguration(StatusThresholdOptions.SectionName);
        services.AddOptions<LoggingOptions>().BindConfiguration(LoggingOptions.SectionName);

        services.AddSingleton<IValidateOptions<SiteOptions>, SiteOptionsValidator>();
        services.AddSingleton<IValidateOptions<EmailOptions>, EmailOptionsValidator>();
        services.AddSingleton<IValidateOptions<LinkKeyOptions>, LinkKeyOptionsValidator>();
        services.AddSingleton<IValidateOptions<RateLimitOptions>, RateLimitOptionsValidator>();
        services.AddSingleton<IValidateOptions<StatusThresholdOptions>, StatusThresholdOptionsValidator>();
        services.AddSingleton<IValidateOptions<LoggingOptions>, LoggingOptionsValidator>();

        // What the conditional settings opened or closed, worked out once at start-up (05 8.4.6); the status command reads it (T389).
        services.AddSingleton<IReadOnlyList<ConditionalLine>>(provider =>
            ConditionalLines.Evaluate(
                provider.GetRequiredService<IOptions<PaymentOptions>>().Value,
                provider.GetRequiredService<IOptions<GoogleOptions>>().Value));
        return services;
    }
}

/// <summary>
/// Checks all the settings when the application starts (05 8.4.6): a failure is written to the log by the setting's name, then the start is
/// refused; a closed conditional line is written to the log and the application opens. Lines go through <c>ILogger&lt;T&gt;</c>
/// (the file is T230's).
/// </summary>
internal sealed partial class StartupSettingsCheck : IHostedService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<StartupSettingsCheck> _logger;

    public StartupSettingsCheck(IConfiguration configuration, ILogger<StartupSettingsCheck> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        SettingsAuditResult result = SettingsAudit.Run(_configuration);

        foreach (string failure in result.Failures)
        {
            LogFailure(_logger, LogEvents.StartupSettingsChecked, failure);
        }

        foreach (ConditionalLine line in result.Lines)
        {
            if (line.IsOpen)
            {
                LogLineOpen(_logger, LogEvents.StartupSettingsChecked, line.Name);
            }
            else
            {
                LogLineClosed(_logger, LogEvents.StartupSettingsChecked, line.Name, string.Join(", ", line.MissingKeys));
            }
        }

        if (!result.Succeeded)
        {
            throw new InvalidOperationException("The application does not open: " + string.Join(" ", result.Failures));
        }

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    [LoggerMessage(Level = LogLevel.Critical, Message = "{Event}: refused. {Failure}")]
    private static partial void LogFailure(ILogger logger, string @event, string failure);

    [LoggerMessage(Level = LogLevel.Information, Message = "{Event}: line {Line} is open.")]
    private static partial void LogLineOpen(ILogger logger, string @event, string line);

    [LoggerMessage(Level = LogLevel.Warning, Message = "{Event}: line {Line} is closed; missing settings: {MissingKeys}.")]
    private static partial void LogLineClosed(ILogger logger, string @event, string line, string missingKeys);
}
