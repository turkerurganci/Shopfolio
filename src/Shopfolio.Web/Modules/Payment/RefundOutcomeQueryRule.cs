using Shopfolio.Infrastructure.Jobs;

namespace Shopfolio.Modules.Payment;

/// <summary>
/// The schedule of the query of a card refund whose outcome is unknown (08 2.1.9; K-1238 (3); 05 4.3.4): the query row is written at the request's timeout and first
/// taken <see cref="JobTimings.RefundOutcomeQueryFirstDelay"/> later; each query without a result waits the next of <see cref="JobTimings.RefundOutcomeQueryIntervals"/>
/// after it - 2, 7, 22 and 82 minutes after the timeout -, then <see cref="JobTimings.RefundOutcomeQueryHourly"/> for ever: the query has no upper limit, the legal
/// period running out (Z-16, Z-17) does not stop it - only a result or the firm's mark does. One pure function of the row's creation and the moment of the query, so the
/// row needs no counter of its own (the rescheduled row's attempt count does not grow).
/// </summary>
internal static class RefundOutcomeQueryRule
{
    /// <summary>
    /// The wait after a query without a result, made at <paramref name="now"/> by the row written at <paramref name="rowCreatedAt"/>: the scheduled moments are counted
    /// up to <paramref name="now"/> and the wait is the interval that follows the last of them.
    /// </summary>
    public static TimeSpan NextDelay(DateTimeOffset rowCreatedAt, DateTimeOffset now)
    {
        TimeSpan elapsed = now - rowCreatedAt;
        TimeSpan moment = JobTimings.RefundOutcomeQueryFirstDelay;
        int passed = 0;
        foreach (TimeSpan interval in JobTimings.RefundOutcomeQueryIntervals)
        {
            if (elapsed < moment)
            {
                break;
            }

            passed++;
            moment += interval;
        }

        // Every scheduled moment up to the last interval's has passed: the query is hourly from here on, with no limit.
        if (passed == JobTimings.RefundOutcomeQueryIntervals.Count && elapsed >= moment)
        {
            return JobTimings.RefundOutcomeQueryHourly;
        }

        return JobTimings.RefundOutcomeQueryIntervals[Math.Max(passed, 1) - 1];
    }
}
