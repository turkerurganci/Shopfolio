using System.Globalization;

using Microsoft.Extensions.Options;

using Npgsql;

namespace Shopfolio.Infrastructure;

/// <summary>
/// The installation settings of the database (09 3.5 item 2, 10.7 item 6): bound from the <c>Database</c> section and
/// checked when the application starts.
/// </summary>
internal sealed class DatabaseOptions
{
    public const string SectionName = "Database";

    public const string ConnectionStringKey = SectionName + ":" + nameof(ConnectionString);

    public const string LivenessTimeoutKey = SectionName + ":" + nameof(LivenessTimeoutSeconds);

    /// <summary>
    /// The connection string of the application's database user. It carries a credential, so it lives in the secret file of the
    /// installation (never in the settings template) and is a required setting: the application does not open without it.
    /// </summary>
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>The time the liveness probe waits for the database, in seconds (09 3.5 table: 2).</summary>
    public int LivenessTimeoutSeconds { get; set; } = 2;

    public TimeSpan LivenessTimeout => TimeSpan.FromSeconds(LivenessTimeoutSeconds);
}

/// <summary>
/// Start-up check of <see cref="DatabaseOptions"/>. A failure names the setting by its key and never prints the connection
/// string (it holds a credential).
/// </summary>
internal sealed class DatabaseOptionsValidator : IValidateOptions<DatabaseOptions>
{
    public ValidateOptionsResult Validate(string? name, DatabaseOptions options)
    {
        List<string> failures = [];

        if (string.IsNullOrWhiteSpace(options.ConnectionString))
        {
            failures.Add(string.Create(CultureInfo.InvariantCulture, $"{DatabaseOptions.ConnectionStringKey} is missing."));
        }
        else if (!IsParsable(options.ConnectionString))
        {
            failures.Add(string.Create(CultureInfo.InvariantCulture, $"{DatabaseOptions.ConnectionStringKey} is not a valid connection string."));
        }

        if (options.LivenessTimeoutSeconds <= 0 || options.LivenessTimeout >= DatabaseTimeouts.CommandTimeout)
        {
            failures.Add(string.Create(
                CultureInfo.InvariantCulture,
                $"{DatabaseOptions.LivenessTimeoutKey} must be greater than zero and shorter than the client command time ({DatabaseTimeouts.CommandTimeout.TotalSeconds} seconds)."));
        }

        return failures.Count == 0 ? ValidateOptionsResult.Success : ValidateOptionsResult.Fail(failures);
    }

    private static bool IsParsable(string connectionString)
    {
        try
        {
            _ = new NpgsqlConnectionStringBuilder(connectionString);
            return true;
        }
        catch (ArgumentException)
        {
            return false;
        }
    }
}
