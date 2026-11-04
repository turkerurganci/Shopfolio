using Microsoft.Extensions.Options;

namespace Shopfolio.Infrastructure.InstallationSettings;

/// <summary>
/// The general request rate limit (05 6.3.5, 8.4.5; 09 15.4 item 2): a design value that the settings file may change. The defaults are the
/// code's; the window and its segments are not here, they are constants of the limiter. Zero or a negative value does not open.
/// </summary>
internal sealed class RateLimitOptions
{
    public const string SectionName = "RateLimit";

    public const string PageRequestsPerMinuteKey = SectionName + ":" + nameof(PageRequestsPerMinute);

    public const string FormSubmissionsPerMinuteKey = SectionName + ":" + nameof(FormSubmissionsPerMinute);

    /// <summary>Page and form requests per minute and client address (09 3.5 table: 300).</summary>
    public int PageRequestsPerMinute { get; set; } = 300;

    /// <summary>Form submissions (POST) per minute and client address (09 3.5 table: 30).</summary>
    public int FormSubmissionsPerMinute { get; set; } = 30;
}

internal sealed class RateLimitOptionsValidator : IValidateOptions<RateLimitOptions>
{
    public ValidateOptionsResult Validate(string? name, RateLimitOptions options)
    {
        List<string> failures = [];
        Positive(options.PageRequestsPerMinute, RateLimitOptions.PageRequestsPerMinuteKey, failures);
        Positive(options.FormSubmissionsPerMinute, RateLimitOptions.FormSubmissionsPerMinuteKey, failures);
        return failures.Count == 0 ? ValidateOptionsResult.Success : ValidateOptionsResult.Fail(failures);
    }

    private static void Positive(int value, string key, List<string> failures)
    {
        if (value <= 0)
        {
            failures.Add(SettingFailures.Malformed(key, "expected a number greater than zero"));
        }
    }
}

/// <summary>
/// The thresholds of the status endpoint (05 9.4.2, 8.4.5): design values that the settings file may change. The defaults are the code's
/// and the table of 05 9.4.2. The e-mail channel warning (9.4.7) has no number and is not here.
/// </summary>
internal sealed class StatusThresholdOptions
{
    public const string SectionName = "StatusThreshold";

    /// <summary>9.4.3 — share of the disk in use above which the status is a warning; the upload check reads the same value (0.85).</summary>
    public double DiskUsageFraction { get; set; } = 0.85;

    /// <summary>9.4.4 — minutes the transaction log has not been shipped.</summary>
    public int LogShippingStallMinutes { get; set; } = 10;

    /// <summary>9.4.4 — most log files that may wait for shipping.</summary>
    public int LogFilesWaitingMax { get; set; } = 10;

    /// <summary>9.4.5 — hours since the last successful nightly full backup or file backup.</summary>
    public int BackupAgeHours { get; set; } = 26;

    /// <summary>9.4.6 — minutes since the last heartbeat.</summary>
    public int HeartbeatAgeMinutes { get; set; } = 5;

    /// <summary>9.4.8 — unexpected errors that raise the warning ...</summary>
    public int UnexpectedErrorCount { get; set; } = 10;

    /// <summary>9.4.8 — ... within this many minutes.</summary>
    public int UnexpectedErrorWindowMinutes { get; set; } = 15;

    /// <summary>9.4.18 — consecutive payment start connection failures.</summary>
    public int PaymentStartFailureCount { get; set; } = 5;

    /// <summary>9.4.18 — minutes the first of those failures must be older than.</summary>
    public int PaymentStartFailureAgeMinutes { get; set; } = 15;

    /// <summary>9.4.9 — days left to the expiry of the site's certificate below which the status warns.</summary>
    public int CertificateDaysLeft { get; set; } = 14;

    /// <summary>9.4.16 — minutes the oldest due job row may wait.</summary>
    public int JobQueueAgeMinutes { get; set; } = 15;

    /// <summary>9.4.17 — hours since the last successful nightly destruction and clean-up.</summary>
    public int NightlyJobAgeHours { get; set; } = 26;
}

internal sealed class StatusThresholdOptionsValidator : IValidateOptions<StatusThresholdOptions>
{
    public ValidateOptionsResult Validate(string? name, StatusThresholdOptions options)
    {
        List<string> failures = [];
        if (!(options.DiskUsageFraction > 0 && options.DiskUsageFraction < 1))
        {
            failures.Add(SettingFailures.Malformed(Key(nameof(options.DiskUsageFraction)), "expected a fraction greater than 0 and less than 1"));
        }

        Positive(options.LogShippingStallMinutes, nameof(options.LogShippingStallMinutes), failures);
        Positive(options.LogFilesWaitingMax, nameof(options.LogFilesWaitingMax), failures);
        Positive(options.BackupAgeHours, nameof(options.BackupAgeHours), failures);
        Positive(options.HeartbeatAgeMinutes, nameof(options.HeartbeatAgeMinutes), failures);
        Positive(options.UnexpectedErrorCount, nameof(options.UnexpectedErrorCount), failures);
        Positive(options.UnexpectedErrorWindowMinutes, nameof(options.UnexpectedErrorWindowMinutes), failures);
        Positive(options.PaymentStartFailureCount, nameof(options.PaymentStartFailureCount), failures);
        Positive(options.PaymentStartFailureAgeMinutes, nameof(options.PaymentStartFailureAgeMinutes), failures);
        Positive(options.CertificateDaysLeft, nameof(options.CertificateDaysLeft), failures);
        Positive(options.JobQueueAgeMinutes, nameof(options.JobQueueAgeMinutes), failures);
        Positive(options.NightlyJobAgeHours, nameof(options.NightlyJobAgeHours), failures);
        return failures.Count == 0 ? ValidateOptionsResult.Success : ValidateOptionsResult.Fail(failures);
    }

    private static string Key(string property) => StatusThresholdOptions.SectionName + ":" + property;

    private static void Positive(int value, string property, List<string> failures)
    {
        if (value <= 0)
        {
            failures.Add(SettingFailures.Malformed(Key(property), "expected a number greater than zero"));
        }
    }
}
