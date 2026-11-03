namespace Shopfolio.Infrastructure.FileSecrets;

/// <summary>
/// File names of the secrets the application code reads (05 6.x.3, 6.x.18, 6.x.19; <c>DEPLOY_RUNBOOK</c> section B) and the
/// database users they belong to (09 10.8). The names equal the secret names in <c>deploy/compose.yaml</c>; a test keeps the
/// two equal. Only the secrets that code reads are listed: the task that introduces a secret's setting adds its name here.
/// </summary>
internal static class SecretNames
{
    /// <summary>Database user of the application and of the commands that write data (09 10.8).</summary>
    public const string DatabaseAppUser = "shopfolio_app";

    /// <summary>Database user of the read command, the return check and the maintenance inspection (09 10.8).</summary>
    public const string DatabaseReaderUser = "shopfolio_reader";

    /// <summary>Password of <see cref="DatabaseAppUser"/> (05 6.x.3, 6.x.19).</summary>
    public const string DatabaseAppPassword = "db_app_password";

    /// <summary>Password of <see cref="DatabaseReaderUser"/> (05 6.x.18).</summary>
    public const string DatabaseReaderPassword = "db_reader_password";

    /// <summary>The secret file that holds the password of a database user the code connects as, or <see langword="null"/> for any other user.</summary>
    public static string? PasswordFileOf(string? databaseUser) => databaseUser switch
    {
        DatabaseAppUser => DatabaseAppPassword,
        DatabaseReaderUser => DatabaseReaderPassword,
        _ => null,
    };
}
