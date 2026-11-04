using Shopfolio.Infrastructure.InstallationSettings;

namespace Shopfolio.Infrastructure.FileSecrets;

/// <summary>A secret setting and the secret files that can hold its value (the first mounted file is the value).</summary>
internal sealed record SecretSetting(string Key, IReadOnlyList<string> Files);

/// <summary>
/// File names of the secrets the application code reads (05 6.x.3, 6.x.18, 6.x.19; <c>DEPLOY_RUNBOOK</c> section B) and the setting each
/// one is the value of. The names equal the secret names in <c>deploy/compose.yaml</c>; a test keeps the two equal. Only the secrets that
/// code reads are listed: the task that introduces a secret's setting adds its name here.
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

    /// <summary>The payment provider's keys; mounted into <c>app</c> (05 6.x.3). Value of <see cref="PaymentOptions.ProviderKeysKey"/>.</summary>
    public const string PaymentProviderKeys = "payment_provider_keys";

    /// <summary>The e-mail sending credential; mounted into <c>app</c> (05 6.x.3). Value of <see cref="EmailOptions.CredentialKey"/>.</summary>
    public const string EmailCredential = "email_credential";

    /// <summary>The Google application's client secret; mounted into <c>app</c> (05 6.x.3). Value of <see cref="GoogleOptions.ClientSecretKey"/>.</summary>
    public const string GoogleClientSecret = "google_client_secret";

    /// <summary>The derivation secret of the account links; mounted into <c>app</c> (05 6.x.20). Value of <see cref="LinkKeyOptions.DerivationSecretKey"/>.</summary>
    public const string LinkDerivationSecret = "link_derivation_secret";

    /// <summary>
    /// The files that can hold the connection string of a container. A container is given at most one of them; the first that
    /// exists is the setting's value.
    /// </summary>
    public static readonly IReadOnlyList<string> DatabaseConnectionFiles = [DatabaseAppConnection, DatabaseReaderConnection];

    /// <summary>
    /// Every secret setting and its files. These keys are filled from files only: the settings template carries none of them
    /// (05 8.4.3; a test keeps the template free of them).
    /// </summary>
    public static readonly IReadOnlyList<SecretSetting> Settings =
    [
        new(DatabaseOptions.ConnectionStringKey, DatabaseConnectionFiles),
        new(EmailOptions.CredentialKey, [EmailCredential]),
        new(LinkKeyOptions.DerivationSecretKey, [LinkDerivationSecret]),
        new(PaymentOptions.ProviderKeysKey, [PaymentProviderKeys]),
        new(GoogleOptions.ClientSecretKey, [GoogleClientSecret]),
    ];
}
