using Microsoft.Extensions.Options;

using Npgsql;

namespace Shopfolio.Infrastructure.FileSecrets;

/// <summary>
/// Puts the password of the database user the connection string names into <see cref="DatabaseOptions.ConnectionString"/> from
/// that user's secret file (<see cref="SecretNames.PasswordFileOf"/>; 05 6.x.1, 6.x.3, 6.x.18). The connection string of the
/// setting carries host, database and user only; the password never sits in a setting or an environment variable. When the file
/// is not mounted (a development or test setup that gives a complete connection string) the string stays as it is; when it is
/// mounted, the file wins. A string that cannot be parsed is left to <see cref="DatabaseOptionsValidator"/>, which names the setting.
/// </summary>
internal sealed class DatabasePasswordPostConfigure : IPostConfigureOptions<DatabaseOptions>
{
    private readonly SecretFiles _secrets;

    public DatabasePasswordPostConfigure(SecretFiles secrets)
    {
        _secrets = secrets;
    }

    public void PostConfigure(string? name, DatabaseOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.ConnectionString))
        {
            return;
        }

        NpgsqlConnectionStringBuilder builder;
        try
        {
            builder = new NpgsqlConnectionStringBuilder(options.ConnectionString);
        }
        catch (ArgumentException)
        {
            return;
        }

        string? passwordFile = SecretNames.PasswordFileOf(builder.Username);
        string? password = passwordFile is null ? null : _secrets.Read(passwordFile);
        if (password is null)
        {
            return;
        }

        builder.Password = password;
        options.ConnectionString = builder.ConnectionString;
    }
}
