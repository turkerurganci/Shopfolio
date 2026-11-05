using Microsoft.Extensions.Options;

using Serilog.Events;

using Shopfolio.Infrastructure.InstallationSettings;

namespace Shopfolio.Infrastructure.Logging;

/// <summary>
/// The log level of the application's own categories (05 9.1.3, 8.4.5; 09 18.1 item 4): a design value that the settings file may change.
/// The default is information; the type is the logging library's own level (09 18.2 item 1). The directory is where the three log files are
/// written (09 18.1 item 2; K-1354): the Compose volume of the application's log files, which the backup tool reads.
/// </summary>
internal sealed class LoggingOptions
{
    public const string SectionName = "Logging";

    public const string LevelKey = SectionName + ":" + nameof(Level);

    public const string DirectoryKey = SectionName + ":" + nameof(Directory);

    public const string DefaultDirectory = "/var/log/shopfolio/app";

    public LogEventLevel Level { get; set; } = LogEventLevel.Information;

    public string Directory { get; set; } = DefaultDirectory;

    /// <summary>
    /// The options as the log setup reads them: a value that does not validate falls back to the default, so the log can still be written
    /// and the start-up check can record which setting is wrong (the check, not the setup, refuses the start).
    /// </summary>
    public static LoggingOptions FromConfiguration(IConfiguration configuration)
    {
        LoggingOptions options = new();
        IConfigurationSection section = configuration.GetSection(SectionName);
        if (Enum.TryParse(section[nameof(Level)], ignoreCase: true, out LogEventLevel level) && Enum.IsDefined(level))
        {
            options.Level = level;
        }

        string? directory = section[nameof(Directory)];
        if (!string.IsNullOrWhiteSpace(directory))
        {
            options.Directory = directory;
        }

        return options;
    }
}

internal sealed class LoggingOptionsValidator : IValidateOptions<LoggingOptions>
{
    public ValidateOptionsResult Validate(string? name, LoggingOptions options)
    {
        List<string> failures = [];
        if (!Enum.IsDefined(options.Level))
        {
            failures.Add(SettingFailures.Malformed(LoggingOptions.LevelKey, "expected Verbose, Debug, Information, Warning, Error or Fatal"));
        }

        if (string.IsNullOrWhiteSpace(options.Directory))
        {
            failures.Add(SettingFailures.Malformed(LoggingOptions.DirectoryKey, "expected a directory path"));
        }

        return failures.Count == 0 ? ValidateOptionsResult.Success : ValidateOptionsResult.Fail(failures);
    }
}
