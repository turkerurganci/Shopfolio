namespace Shopfolio.Infrastructure.Jobs;

/// <summary>
/// When the nightly job starts (05 5.1.1, 5.3.2; 09 13.1 item 9): a pure function of the clock and the two instants of the heartbeat record.
/// The job starts when its last success is older than the latest 03:00 (Turkey time); a night that was missed is therefore made up at the first
/// wake-up after it, and a job that ended in failure starts again no sooner than an hour after its last attempt. The caller reads the clock.
/// </summary>
internal static class NightlyJobSchedule
{
    /// <summary>The latest 03:00 at or before <paramref name="now"/>: today's if it has passed, yesterday's otherwise.</summary>
    public static DateTimeOffset LatestDueMoment(DateTimeOffset now)
    {
        DateOnly today = TurkeyTime.DateOf(now);
        DateTimeOffset latest = TurkeyTime.StartOf(today).AddHours(SchedulerTimings.NightlyHour);
        return now < latest ? TurkeyTime.StartOf(today.AddDays(-1)).AddHours(SchedulerTimings.NightlyHour) : latest;
    }

    public static bool IsDue(DateTimeOffset now, DateTimeOffset? lastSucceededAt, DateTimeOffset? lastAttemptedAt)
    {
        DateTimeOffset latestDue = LatestDueMoment(now);
        if (lastSucceededAt is { } succeeded && succeeded >= latestDue)
        {
            return false;
        }

        return lastAttemptedAt is not { } attempted || now >= attempted + SchedulerTimings.NightlyRetryWait;
    }
}
