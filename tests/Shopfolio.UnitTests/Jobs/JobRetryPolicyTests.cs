using Shopfolio.Infrastructure.Jobs;
using Shopfolio.Infrastructure.Values;

namespace Shopfolio.UnitTests.Jobs;

/// <summary>
/// The retry rule of a job type (T220 criterion 5, KK5; 09 13.1 items 5 and 11; 05 4.3.4; MK-42). Every expected moment and count is derived from
/// <see cref="JobTimings.DefaultRetrySchedule"/>, the one place of the table's values, and not written again as a number.
/// </summary>
public sealed class JobRetryPolicyTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 5, 9, 0, 0, TimeSpan.Zero);

    [Fact]
    public void DefaultSchedule_IsTheFiveIntervalsOfTheTable()
    {
        Assert.Equal([1, 5, 15, 60, 240], JobTimings.DefaultRetrySchedule.Select(s => (int)s.TotalMinutes));
    }

    [Theory]
    [InlineData(JobType.ImageDerivative)]
    [InlineData(JobType.InboundNotificationProcessing)]
    [InlineData(JobType.FinalPaymentQuery)]
    [InlineData(JobType.RefundOutcomeQuery)]
    [InlineData(JobType.CardRefund)]
    public void AfterFailure_DefaultRule_RetriesAtTheScheduledMomentWithTheCountRaisedByOne_ForEachOfTheFirstFiveFailures(JobType jobType)
    {
        for (int attempts = 0; attempts < JobTimings.DefaultRetrySchedule.Count; attempts++)
        {
            JobFailureDecision decision = JobRetryPolicy.AfterFailure(jobType, attempts, Now);

            JobFailureDecision.RetryAt retry = Assert.IsType<JobFailureDecision.RetryAt>(decision);
            Assert.Equal(Now + JobTimings.DefaultRetrySchedule[attempts], retry.At);
            Assert.Equal(attempts + 1, retry.NewAttemptCount);
        }
    }

    [Theory]
    [InlineData(JobType.ImageDerivative)]
    [InlineData(JobType.InboundNotificationProcessing)]
    [InlineData(JobType.FinalPaymentQuery)]
    [InlineData(JobType.RefundOutcomeQuery)]
    [InlineData(JobType.CardRefund)]
    public void AfterFailure_DefaultRule_TheSixthFailureGivesUp(JobType jobType)
    {
        JobFailureDecision decision = JobRetryPolicy.AfterFailure(jobType, JobTimings.DefaultRetrySchedule.Count, Now);

        Assert.IsType<JobFailureDecision.GiveUp>(decision);
    }

    [Fact]
    public void AfterFailure_BounceQuery_NeverGivesUp_RepeatsAtTheQueryIntervalAndKeepsTheCount()
    {
        foreach (int attempts in new[] { 0, 1, JobTimings.DefaultRetrySchedule.Count, 50 })
        {
            JobFailureDecision.RetryAt retry = Assert.IsType<JobFailureDecision.RetryAt>(JobRetryPolicy.AfterFailure(JobType.BounceQuery, attempts, Now));

            Assert.Equal(Now + JobTimings.BounceQueryInterval, retry.At);
            Assert.Equal(attempts, retry.NewAttemptCount);
        }
    }

    [Fact]
    public void AfterFailure_NegativeCount_IsRefused()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => JobRetryPolicy.AfterFailure(JobType.ImageDerivative, -1, Now));
    }

    [Fact]
    public void AfterFailure_EveryJobType_HasADecision()
    {
        foreach (JobType jobType in Enum.GetValues<JobType>())
        {
            Assert.NotNull(JobRetryPolicy.AfterFailure(jobType, 0, Now));
        }
    }

    [Fact]
    public void RescheduleAfter_NeedsAPositiveDelay_AndIsNotAFailure()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => JobOutcome.RescheduleAfter(TimeSpan.Zero));
        JobOutcome.RescheduledOutcome later = Assert.IsType<JobOutcome.RescheduledOutcome>(JobOutcome.RescheduleAfter(JobTimings.FinalPaymentQueryInterval));
        Assert.Equal(JobTimings.FinalPaymentQueryInterval, later.Delay);
        Assert.IsType<JobOutcome.DoneOutcome>(JobOutcome.Completed);
    }

    [Fact]
    public void JobTypes_HaveNoEmailMember_TheDispatchRecordIsTheEmailsQueue()
    {
        // T220 criterion 8, KK8 (05 4.4.1; MK-38): an e-mail never gets a row of its own in the queue, so no job type names one.
        string[] names = Enum.GetNames<JobType>();

        Assert.Equal(
            ["BounceQuery", "CardRefund", "FinalPaymentQuery", "ImageDerivative", "InboundNotificationProcessing", "RefundOutcomeQuery"],
            names.Order(StringComparer.Ordinal));
        Assert.DoesNotContain(names, n => n.Contains("Mail", StringComparison.OrdinalIgnoreCase) || n.Contains("Dispatch", StringComparison.OrdinalIgnoreCase));
    }
}
