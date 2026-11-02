using System.Globalization;

using Microsoft.Extensions.Options;

using Npgsql;

namespace Shopfolio.Infrastructure;

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
