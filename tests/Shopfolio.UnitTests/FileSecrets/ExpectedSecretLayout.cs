namespace Shopfolio.UnitTests.FileSecrets;

/// <summary>The secret layout of 05 6.x.2, written out once for the Compose tests and the runbook consistency checks.</summary>
internal static class ExpectedSecretLayout
{
    // 05 6.x.2 for the continuously running installation and the one-off services: the secrets each component receives.
    // A component with no secret receives none (the reverse proxy: 6.x.7).
    public static readonly IReadOnlyDictionary<string, string[]> PerService = new Dictionary<string, string[]>(StringComparer.Ordinal)
    {
        ["app"] = ["db_app_connection", "payment_provider_keys", "email_credential", "google_client_secret", "link_derivation_secret"],
        ["migrate"] = ["db_migrator_password"],
        ["backup"] = ["file_backup_storage_key", "file_backup_cipher_key"],
        ["database"] = ["db_backup_storage_key", "db_backup_cipher_key"],
        ["proxy"] = [],
        ["read-command"] = ["db_reader_connection"],
        ["command"] = ["db_app_connection"],
    };

    // 05 6.x.6: what the database receives only at the first install (deploy/compose.install.yaml).
    public static readonly string[] FirstInstallOnly = ["db_superuser_password", "db_setup_migrator_password", "db_setup_app_password", "db_setup_reader_password"];

    // 05 6.x.12: the only secrets that are never rotated.
    public static readonly string[] NeverRotated = ["db_backup_cipher_key", "file_backup_cipher_key"];

    public static string[] AllSecretNames() =>
        PerService.Values.SelectMany(s => s).Concat(FirstInstallOnly).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToArray();
}
