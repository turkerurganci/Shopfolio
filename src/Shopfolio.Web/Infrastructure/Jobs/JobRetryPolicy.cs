using Shopfolio.Infrastructure.Values;

namespace Shopfolio.Infrastructure.Jobs;

/// <summary>
/// The retry rule of a job type, one pure function (09 13.1 item 5; 05 4.3.4; MK-42). The default rule: after a failure the row is taken again 1,
/// 5, 15, 60 and 240 minutes later - at most five retries - and the sixth failure makes it failed. Two types have a rule of their own:
/// the card refund and the returned-address query. The other types - the final query and the refund outcome query repeat through
/// <see cref="JobOutcome.RescheduleAfter"/> when they have nothing to report, and a failure of theirs is a code error - take the default rule.
/// </summary>
internal static class JobRetryPolicy
{
    /// <summary>
    /// What happens to a row of <paramref name="jobType"/> whose handler failed. <paramref name="attemptCount"/> is the row's count before this
    /// failure, <paramref name="now"/> the instant read from the time provider.
    /// </summary>
    public static JobFailureDecision AfterFailure(JobType jobType, int attemptCount, DateTimeOffset now)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(attemptCount);
        return jobType switch
        {
            // The returned-address query has no related record and its failed round is not retried and does not fail the row: the round
            // passes empty and the next one is written 15 minutes later (08 2.2.3; K-1241). The attempt count stays.
            JobType.BounceQuery => new JobFailureDecision.RetryAt(now + JobTimings.BounceQueryInterval, attemptCount),

            // The card refund: a sent request is never retried by the runner. The record is written before the request goes, so a retry of
            // the row finds it in the "sending" state, does not send again and asks for the result (05 4.3.3; CardRefundJobRule). A failure
            // before the record was written sent nothing and is retried by the default rule (05 4.3.4; K-1111, K-1132).
            JobType.CardRefund => Default(attemptCount, now),

            _ => Default(attemptCount, now),
        };
    }

    private static JobFailureDecision Default(int attemptCount, DateTimeOffset now)
    {
        return attemptCount < JobTimings.DefaultRetrySchedule.Count
            ? new JobFailureDecision.RetryAt(now + JobTimings.DefaultRetrySchedule[attemptCount], attemptCount + 1)
            : new JobFailureDecision.GiveUp();
    }
}
