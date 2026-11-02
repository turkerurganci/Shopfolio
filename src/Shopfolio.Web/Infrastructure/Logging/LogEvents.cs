namespace Shopfolio.Infrastructure.Logging;

/// <summary>
/// The closed list of log event names (09 18.2 item 3). A free text never goes into the event field. Only the event the
/// liveness probe writes exists so far; the rest of the list arrives with the log writers (T230).
/// </summary>
internal static class LogEvents
{
    /// <summary>The liveness probe's database check ran out of time (09 10.7 item 6).</summary>
    public const string DbTimeout = "DbTimeout";
}
