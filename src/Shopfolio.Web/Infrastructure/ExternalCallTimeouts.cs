using System.Diagnostics;

namespace Shopfolio.Infrastructure;

/// <summary>
/// The time limits of the calls that leave the application (09 11.1 item 4; the 09 3.5 table row "Dış çağrı süresi"): two constants, and nothing
/// else sets a call's limit. A call's limit is set with a cancellation source bound to the time provider,
/// <c>new CancellationTokenSource(limit, TimeProvider)</c>, so a test moves the limit with a fake clock: the calls of the outside services'
/// link layer (<c>Integrations/ExternalCallRunner</c>) and the scheduler's heartbeat request (09 13.1 item 8) alike. <c>HttpClient.Timeout</c>
/// is infinite. Passing the limit is the timeout result, never a retry.
/// </summary>
internal static class ExternalCallTimeouts
{
    /// <summary>A call a user waits for: 15 seconds (09 3.5 table). The heartbeat request uses it too.</summary>
    public static readonly TimeSpan UserWaiting = TimeSpan.FromSeconds(15);

    /// <summary>A call that leaves the job queue: 30 seconds (09 3.5 table).</summary>
    public static readonly TimeSpan Job = TimeSpan.FromSeconds(30);

    /// <summary>The limit of a kind of call: one of the two constants above.</summary>
    public static TimeSpan For(ExternalCallKind kind)
    {
        return kind switch
        {
            ExternalCallKind.UserWaiting => UserWaiting,
            ExternalCallKind.Job => Job,
            _ => throw new UnreachableException(),
        };
    }
}
