namespace Shopfolio.Infrastructure.Jobs;

/// <summary>
/// What a handler tells the runner about one row (09 13.1 item 4). The family is closed: the row is done, or it is not done yet and nothing went
/// wrong (a query that repeats; the attempt count does not grow). A failure is not an outcome: the handler throws and the runner applies the
/// retry rule of the job type (<see cref="JobRetryPolicy"/>).
/// </summary>
internal abstract record JobOutcome
{
    private JobOutcome()
    {
    }

    /// <summary>The row is done: it becomes completed.</summary>
    public static JobOutcome Completed { get; } = new DoneOutcome();

    /// <summary>The row is not done and no error happened: it stays pending and is taken again after <paramref name="delay"/>; the attempt count is not raised.</summary>
    public static JobOutcome RescheduleAfter(TimeSpan delay)
    {
        if (delay <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(delay), delay, "A rescheduled row waits for a positive time.");
        }

        return new RescheduledOutcome(delay);
    }

    /// <summary>The row is done.</summary>
    public sealed record DoneOutcome : JobOutcome;

    /// <summary>The row is taken again after <see cref="Delay"/>.</summary>
    public sealed record RescheduledOutcome(TimeSpan Delay) : JobOutcome;
}

/// <summary>What the retry rule decides after a handler failure (09 13.1 item 5).</summary>
internal abstract record JobFailureDecision
{
    private JobFailureDecision()
    {
    }

    /// <summary>The row stays pending, is taken again at <see cref="At"/> and carries <see cref="NewAttemptCount"/>.</summary>
    public sealed record RetryAt(DateTimeOffset At, int NewAttemptCount) : JobFailureDecision;

    /// <summary>The row becomes failed (05 4.3.4); it is never deleted and does not hold the rows behind it.</summary>
    public sealed record GiveUp : JobFailureDecision;
}
