using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Integrations;
using Shopfolio.Modules.Notifications;
using Shopfolio.Modules.Notifications.Contracts;

namespace Shopfolio.UnitTests.Outages;

/// <summary>
/// The retry rule of the dispatch record's row (T204 criteria 4 and 5, KK4, KK5; 08 2.2 "Retry stratejisi", 2.2.2 "(sonuç)"; 05 4.4.3; P-38):
/// the three retries are counted from the first attempt, a late queue does not move them, a recipient error is retried like a channel error and
/// the attempt after the last retry makes the row "undelivered". The distances are read from the rule (which reads <c>ProductConstants</c>),
/// never written here; <c>OutageConsistencyTests.DispatchRetryConstants_AreTheP38Row</c> keeps the constants equal to P-38.
/// </summary>
public sealed class DispatchRetryScheduleTests
{
    private static readonly DateTimeOffset FirstAttempt = new(2026, 11, 5, 9, 0, 0, TimeSpan.Zero);

    private static IReadOnlyList<TimeSpan> Retries => DispatchRetrySchedule.RetriesFromFirstAttempt;

    private static DispatchRecord Row() => DispatchRecord.Queue(DispatchType.F5, "firma@example.com", null, null, null, FirstAttempt.AddMinutes(-1));

    public static TheoryData<DispatchOutcome> Errors() => [DispatchOutcome.ChannelError, DispatchOutcome.RecipientError];

    [Fact]
    public void Retries_AreTheThreeProductConstants_InOrder()
    {
        Assert.Equal(
            [
                TimeSpan.FromMinutes(ProductConstants.DispatchFirstRetryMinutes),
                TimeSpan.FromMinutes(ProductConstants.DispatchSecondRetryMinutes),
                TimeSpan.FromMinutes(ProductConstants.DispatchThirdRetryMinutes),
            ],
            Retries);
        Assert.Equal(Retries.Count + 1, DispatchRetrySchedule.AttemptLimit);
    }

    [Theory]
    [MemberData(nameof(Errors))]
    public void FailingEveryTime_SetsTheMomentsFromTheFirstAttempt_ThenUndelivered(DispatchOutcome error)
    {
        DispatchRecord row = Row();
        List<DateTimeOffset?> moments = [];
        DateTimeOffset start = FirstAttempt;
        for (int attempt = 1; attempt <= DispatchRetrySchedule.AttemptLimit; attempt++)
        {
            DispatchRetrySchedule.Apply(row, error, start, start.AddSeconds(2));
            moments.Add(row.NextAttemptAt);
            Assert.Equal(attempt, row.AttemptCount);
            Assert.Equal(error, row.LastOutcome);
            Assert.Null(row.SentAt);
            if (row.NextAttemptAt is { } next)
            {
                Assert.Equal(DispatchState.Queued, row.DispatchState);
                start = next;
            }
        }

        Assert.Equal([FirstAttempt + Retries[0], FirstAttempt + Retries[1], FirstAttempt + Retries[2], null], moments);
        Assert.Equal(DispatchState.Undelivered, row.DispatchState);
        Assert.Equal(DispatchRetrySchedule.AttemptLimit, row.AttemptCount);
    }

    [Fact]
    public void LateQueue_DoesNotMoveTheLaterMoments()
    {
        DispatchRecord row = Row();
        DispatchRetrySchedule.Apply(row, DispatchOutcome.ChannelError, FirstAttempt, FirstAttempt);

        // The queue takes the row 7 minutes after its moment; the next moment is still the second retry's distance from the first attempt.
        DateTimeOffset late = row.NextAttemptAt!.Value.AddMinutes(7);
        DispatchRetrySchedule.Apply(row, DispatchOutcome.ChannelError, late, late);
        Assert.Equal(FirstAttempt + Retries[1], row.NextAttemptAt);

        DateTimeOffset later = row.NextAttemptAt!.Value.AddMinutes(40);
        DispatchRetrySchedule.Apply(row, DispatchOutcome.RecipientError, later, later);
        Assert.Equal(FirstAttempt + Retries[2], row.NextAttemptAt);
    }

    [Fact]
    public void FirstFailure_IsCountedFromTheAttemptsStart_NotFromTheResultsMoment()
    {
        DispatchRecord row = Row();

        DispatchRetrySchedule.Apply(row, DispatchOutcome.ChannelError, FirstAttempt, FirstAttempt.AddSeconds(30));

        Assert.Equal(FirstAttempt + Retries[0], row.NextAttemptAt);
    }

    [Fact]
    public void Sending_AfterFailures_IsSent_WithItsMoment_AndNoNextAttempt()
    {
        DispatchRecord row = Row();
        DispatchRetrySchedule.Apply(row, DispatchOutcome.ChannelError, FirstAttempt, FirstAttempt);
        DispatchRetrySchedule.Apply(row, DispatchOutcome.RecipientError, row.NextAttemptAt!.Value, row.NextAttemptAt!.Value);
        DateTimeOffset third = row.NextAttemptAt!.Value;

        DispatchRetrySchedule.Apply(row, DispatchOutcome.Sent, third, third.AddSeconds(1));

        Assert.Equal(DispatchState.Sent, row.DispatchState);
        Assert.Equal(DispatchOutcome.Sent, row.LastOutcome);
        Assert.Equal(third.AddSeconds(1), row.SentAt);
        Assert.Null(row.NextAttemptAt);
        Assert.Equal(3, row.AttemptCount);
    }

    [Fact]
    public void FirstSending_CountsOneAttempt()
    {
        DispatchRecord row = Row();

        DispatchRetrySchedule.Apply(row, DispatchOutcome.Sent, FirstAttempt, FirstAttempt);

        Assert.Equal((DispatchState.Sent, 1, (DateTimeOffset?)FirstAttempt), (row.DispatchState, row.AttemptCount, row.SentAt));
    }

    [Theory]
    [InlineData(DispatchState.Sent)]
    [InlineData(DispatchState.Undelivered)]
    public void RowThatIsNotQueued_TakesNoResult(DispatchState state)
    {
        DispatchRecord row = Row();
        row.DispatchState = state;

        Assert.Throws<InvalidOperationException>(() => DispatchRetrySchedule.Apply(row, DispatchOutcome.Sent, FirstAttempt, FirstAttempt));
        Assert.Equal(0, row.AttemptCount);
    }

    [Fact]
    public void OutcomeOf_MapsEachClassOfTheFamily()
    {
        Assert.Equal(DispatchOutcome.Sent, DispatchRetrySchedule.OutcomeOf(new EmailSendResult.Sent()));
        Assert.Equal(DispatchOutcome.RecipientError, DispatchRetrySchedule.OutcomeOf(new EmailSendResult.RecipientError("bounce")));
        Assert.Equal(DispatchOutcome.ChannelError, DispatchRetrySchedule.OutcomeOf(new EmailSendResult.ChannelError("timeout")));
    }
}
