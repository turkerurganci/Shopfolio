using Shopfolio.Infrastructure.Integrations;
using Shopfolio.Infrastructure.Jobs;
using Shopfolio.Modules.Payment;
using Shopfolio.Modules.Payment.Contracts;

namespace Shopfolio.UnitTests.Jobs;

/// <summary>
/// The schedule of the unknown card refund's query (T405 criterion 7, KK7; 08 2.1.9; K-1238 (3)): the first query a minute after the timeout, the next ones 1, 5, 15 and 60
/// minutes after the one before, then hourly with no upper limit - Z-17's fourteen days running out does not stop it. The expected moments are built from
/// <see cref="JobTimings"/>, which <c>JobTimingsConsistencyTests</c> ties to the 09 3.5 table; the answer classes of the request map to the record's states.
/// </summary>
public sealed class RefundOutcomeQueryRuleTests
{
    private static readonly DateTimeOffset Timeout = new(2026, 11, 10, 9, 0, 0, TimeSpan.Zero);

    /// <summary>The query moments the rule gives when every query is taken <paramref name="lateBy"/> after its moment and finds no result.</summary>
    private static List<DateTimeOffset> Moments(int count, TimeSpan lateBy)
    {
        List<DateTimeOffset> moments = [];
        DateTimeOffset moment = Timeout + JobTimings.RefundOutcomeQueryFirstDelay;
        for (int i = 0; i < count; i++)
        {
            moments.Add(moment);
            DateTimeOffset taken = moment + lateBy;
            moment = taken + RefundOutcomeQueryRule.NextDelay(Timeout, taken);
        }

        return moments;
    }

    [Fact]
    public void Schedule_IsTheFirstDelay_ThenTheFourIntervals_ThenHourly_KK7()
    {
        List<DateTimeOffset> expected = [Timeout + JobTimings.RefundOutcomeQueryFirstDelay];
        foreach (TimeSpan interval in JobTimings.RefundOutcomeQueryIntervals)
        {
            expected.Add(expected[^1] + interval);
        }

        for (int i = 0; i < 3; i++)
        {
            expected.Add(expected[^1] + JobTimings.RefundOutcomeQueryHourly);
        }

        Assert.Equal(expected, Moments(expected.Count, TimeSpan.Zero));
        Assert.Equal(5, JobTimings.RefundOutcomeQueryIntervals.Count + 1);
    }

    [Fact]
    public void Schedule_ThatRunsAFewSecondsLate_KeepsTheSameIntervals_KK7()
    {
        List<DateTimeOffset> onTime = Moments(8, TimeSpan.Zero);
        List<DateTimeOffset> late = Moments(8, TimeSpan.FromSeconds(3));

        // Each late query moves the next one by its lateness only; the interval chosen is the same.
        TimeSpan[] onTimeGaps = [.. onTime.Zip(onTime.Skip(1), (a, b) => b - a)];
        TimeSpan[] lateGaps = [.. late.Zip(late.Skip(1), (a, b) => b - a - TimeSpan.FromSeconds(3))];
        Assert.Equal(onTimeGaps, lateGaps);
    }

    [Fact]
    public void Query_AfterZ17sFourteenDays_StillRepeatsHourly_WithNoUpperLimit_KK7()
    {
        foreach (TimeSpan elapsed in new[] { TimeSpan.FromDays(14), TimeSpan.FromDays(14) + TimeSpan.FromMinutes(1), TimeSpan.FromDays(60), TimeSpan.FromDays(400) })
        {
            Assert.Equal(JobTimings.RefundOutcomeQueryHourly, RefundOutcomeQueryRule.NextDelay(Timeout, Timeout + elapsed));
        }
    }

    [Fact]
    public void RequestAnswer_MapsToTheRecordsOutcome_KK5()
    {
        Assert.Equal(RefundRecordState.Succeeded, CardRefundJobRule.OutcomeOf(new RefundRequestResult.Accepted()));
        Assert.Equal(RefundRecordState.Failed, CardRefundJobRule.OutcomeOf(new RefundRequestResult.NotCompleted("err:005")));
        Assert.Equal(RefundRecordState.OutcomePending, CardRefundJobRule.OutcomeOf(new RefundRequestResult.Unknown(ExternalCallFailure.TimeoutReason)));
    }
}
