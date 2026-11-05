using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Jobs;
using Shopfolio.Infrastructure.Values;

namespace Shopfolio.UnitTests.Jobs;

/// <summary>
/// When the nightly job starts (T221 criterion 3, KK1, KK5; 05 5.1.1, 5.3.2; MK-43). Every clock time is derived from
/// <see cref="SchedulerTimings"/> and <see cref="TurkeyTime"/>, not written again as a number.
/// </summary>
public sealed class NightlyJobScheduleTests
{
    private static readonly DateOnly Day = new(2026, 11, 5);

    private static DateTimeOffset NightlyOn(DateOnly day) => TurkeyTime.StartOf(day).AddHours(SchedulerTimings.NightlyHour);

    [Fact]
    public void LatestDueMoment_OneMinuteBeforeTheHour_IsYesterdays()
    {
        DateTimeOffset now = NightlyOn(Day) - TimeSpan.FromMinutes(1);

        Assert.Equal(NightlyOn(Day.AddDays(-1)), NightlyJobSchedule.LatestDueMoment(now));
    }

    [Fact]
    public void LatestDueMoment_AtTheHour_IsToday()
    {
        Assert.Equal(NightlyOn(Day), NightlyJobSchedule.LatestDueMoment(NightlyOn(Day)));
    }

    [Fact]
    public void IsDue_OneMinuteBeforeTheHour_WhenYesterdayWasDone_IsFalse_AndAtTheHour_IsTrue()
    {
        DateTimeOffset yesterdaysSuccess = NightlyOn(Day.AddDays(-1)) + TimeSpan.FromMinutes(5);

        Assert.False(NightlyJobSchedule.IsDue(NightlyOn(Day) - TimeSpan.FromMinutes(1), yesterdaysSuccess, yesterdaysSuccess));
        Assert.True(NightlyJobSchedule.IsDue(NightlyOn(Day), yesterdaysSuccess, yesterdaysSuccess));
    }

    [Fact]
    public void IsDue_AfterTodaysSuccess_IsFalse_UntilTomorrowsHour()
    {
        DateTimeOffset success = NightlyOn(Day) + TimeSpan.FromMinutes(5);

        Assert.False(NightlyJobSchedule.IsDue(NightlyOn(Day) + TimeSpan.FromHours(10), success, success));
        Assert.True(NightlyJobSchedule.IsDue(NightlyOn(Day.AddDays(1)), success, success));
    }

    [Fact]
    public void IsDue_ANightThatWasMissed_IsMadeUpAtTheFirstLookAfterIt()
    {
        DateTimeOffset lastSuccess = NightlyOn(Day.AddDays(-3)) + TimeSpan.FromMinutes(5);
        DateTimeOffset firstLook = NightlyOn(Day) + TimeSpan.FromHours(6);

        Assert.True(NightlyJobSchedule.IsDue(firstLook, lastSuccess, lastSuccess));
    }

    [Fact]
    public void IsDue_AFailedAttempt_IsNotTriedBeforeTheRetryWait_AndIsTriedAtIt()
    {
        DateTimeOffset attempt = NightlyOn(Day) + TimeSpan.FromMinutes(1);
        DateTimeOffset yesterdaysSuccess = NightlyOn(Day.AddDays(-1)) + TimeSpan.FromMinutes(5);

        Assert.False(NightlyJobSchedule.IsDue(attempt + SchedulerTimings.NightlyRetryWait - TimeSpan.FromMinutes(1), yesterdaysSuccess, attempt));
        Assert.True(NightlyJobSchedule.IsDue(attempt + SchedulerTimings.NightlyRetryWait, yesterdaysSuccess, attempt));
    }

    [Fact]
    public void IsDue_NeverRun_IsDue()
    {
        Assert.True(NightlyJobSchedule.IsDue(NightlyOn(Day), null, null));
    }

    [Fact]
    public void NightlyHour_InUtc_IsThreeHoursEarlier()
    {
        // 03:00 Turkey time is 00:00 UTC: the clock times are Turkey times and the stored instants are UTC (09 7.1 item 3).
        Assert.Equal(TimeSpan.Zero, NightlyOn(Day).TimeOfDay - TimeSpan.FromHours(SchedulerTimings.NightlyHour) + TurkeyTime.Offset);
        Assert.Equal(TimeSpan.Zero, NightlyOn(Day).UtcDateTime.TimeOfDay);
    }
}
