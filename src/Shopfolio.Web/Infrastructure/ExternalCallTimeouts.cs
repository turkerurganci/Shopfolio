namespace Shopfolio.Infrastructure;

/// <summary>
/// The time limits of the calls that leave the application (09 11.2 item 4; the 09 3.5 table row "Dış çağrı süresi"): two constants, and nothing
/// else sets a call's limit. A call's limit is given to its own cancellation source with <c>CancelAfter</c>; <c>HttpClient.Timeout</c> is
/// infinite. The scheduler's heartbeat request takes the first (09 13.1 item 8). Passing the limit is the timeout result, never a retry.
/// </summary>
internal static class ExternalCallTimeouts
{
    /// <summary>A call a user waits for: 15 seconds (09 3.5 table). The heartbeat request uses it too.</summary>
    public static readonly TimeSpan UserWaiting = TimeSpan.FromSeconds(15);

    /// <summary>A call that leaves the job queue: 30 seconds (09 3.5 table).</summary>
    public static readonly TimeSpan Job = TimeSpan.FromSeconds(30);
}
