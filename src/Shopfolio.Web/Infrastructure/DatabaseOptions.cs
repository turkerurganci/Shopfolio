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
