namespace Shopfolio.Infrastructure.FileSecrets;

/// <summary>
/// File names of the secrets the application code reads (05 6.x.3, 6.x.18, 6.x.19; <c>DEPLOY_RUNBOOK</c> section B). The names
/// equal the secret names in <c>deploy/compose.yaml</c>; a test keeps the two equal. Only the secrets that code reads are listed:
/// the task that introduces a secret's setting adds its name here.
/// </summary>
internal static class SecretNames
{
    /// <summary>
    /// Whole connection string (password included) of the application's database user; mounted into <c>app</c> and <c>command</c>
    /// (05 6.x.3, 6.x.19). It is the value of the setting <see cref="DatabaseOptions.ConnectionStringKey"/> (09 3.5 item 2).
    /// </summary>
    public const string DatabaseAppConnection = "db_app_connection";

    /// <summary>Whole connection string of the read-only database user; mounted into <c>read-command</c> only (05 6.x.18).</summary>
    public const string DatabaseReaderConnection = "db_reader_connection";

    /// <summary>
    /// The files that can hold the connection string of a container. A container is given at most one of them; the first that
    /// exists is the setting's value.
    /// </summary>
    public static readonly IReadOnlyList<string> DatabaseConnectionFiles = [DatabaseAppConnection, DatabaseReaderConnection];
}
