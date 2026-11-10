namespace Shopfolio.Infrastructure.Jobs;

/// <summary>
/// The times of the job queue (09 3.5 table rows "İş kuyruğu çalıştırıcısının kuyruğa bakma aralığı" and "İş satırının varsayılan yeniden
/// denemesi"; 05 4.3.2, 4.3.4). Every value is a row of that table and has no other place: the runner, the retry rule and the tests read these
/// constants and never write a number of their own (<c>JobTimingsConsistencyTests</c> keeps them equal to the table).
/// </summary>
internal static class JobTimings
{
    /// <summary>How often the runner looks at the queue when nothing woke it (09 3.5 table: 5 seconds; 05 4.3.2). It is also the wait before the runner asks for its lock again.</summary>
    public static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(5);

    /// <summary>The waits of the default retry rule: 1, 5, 15, 60 and 240 minutes after the 1st to the 5th failure (09 3.5 table; 05 4.3.4).</summary>
    public static readonly IReadOnlyList<TimeSpan> DefaultRetrySchedule =
    [
        TimeSpan.FromMinutes(1),
        TimeSpan.FromMinutes(5),
        TimeSpan.FromMinutes(15),
        TimeSpan.FromMinutes(60),
        TimeSpan.FromMinutes(240),
    ];

    /// <summary>The unanswered final query is asked again after 5 minutes (09 3.5 table; 05 5.3.3). The handler returns it as <see cref="JobOutcome.RescheduleAfter"/>.</summary>
    public static readonly TimeSpan FinalPaymentQueryInterval = TimeSpan.FromMinutes(5);

    /// <summary>The first query of a card refund whose outcome is unknown: 1 minute after the request's timeout (09 3.5 table; 08 2.1.9; K-1238 (3)).</summary>
    public static readonly TimeSpan RefundOutcomeQueryFirstDelay = TimeSpan.FromMinutes(1);

    /// <summary>
    /// The waits of the next queries, each after the query before it: 1, 5, 15 and 60 minutes - 2, 7, 22 and 82 minutes after the timeout (09 3.5 table; 08 2.1.9;
    /// K-1238 (3)). After the last one the query repeats every <see cref="RefundOutcomeQueryHourly"/>, with no upper limit.
    /// </summary>
    public static readonly IReadOnlyList<TimeSpan> RefundOutcomeQueryIntervals =
    [
        TimeSpan.FromMinutes(1),
        TimeSpan.FromMinutes(5),
        TimeSpan.FromMinutes(15),
        TimeSpan.FromMinutes(60),
    ];

    /// <summary>The query of a card refund whose outcome is unknown repeats every hour after its four intervals, until a result or the firm's mark (09 3.5 table; 08 2.1.9).</summary>
    public static readonly TimeSpan RefundOutcomeQueryHourly = TimeSpan.FromHours(1);

    /// <summary>The returned-address query repeats every 15 minutes, a failed round included (09 3.5 table; 05 4.3.4; K-1241).</summary>
    public static readonly TimeSpan BounceQueryInterval = TimeSpan.FromMinutes(15);

    /// <summary>
    /// The window of the returned-address query: the service's list is read from 24 hours ago, and a "sent" row is marked only when its sending moment is inside the same window
    /// (08 2.2.3 table, "Pencerenin sınırı"; K-1241). The 24 hours is a row of the 09 3.5 table (and of 08 2.2.3).
    /// </summary>
    public static readonly TimeSpan BounceQueryWindow = TimeSpan.FromHours(24);

    /// <summary>The name of the runner's single-run lock (09 13.1 item 7).</summary>
    public const string RunnerLockName = "JobRunner";
}
