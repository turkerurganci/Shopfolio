namespace Shopfolio.Infrastructure.Logging;

/// <summary>
/// The closed list of log event names (09 18.2 item 3). A free text never goes into the event field. Only the event the
/// liveness probe and the two start-up checks write exist so far; the rest of the list arrives with the log writers (T230).
/// </summary>
internal static class LogEvents
{
    /// <summary>The start-up data check refused the start for a missing record or did not run because the database was out of reach (06 8.2.1; 09 18.2 item 3).</summary>
    public const string StartupDatabaseChecked = "StartupDatabaseChecked";

    /// <summary>The start-up settings check refused the start or closed a conditional line (05 8.4.6; 09 18.2 item 3).</summary>
    public const string StartupSettingsChecked = "StartupSettingsChecked";

    /// <summary>The liveness probe's database check ran out of time (09 10.7 item 6).</summary>
    public const string DbTimeout = "DbTimeout";
}
