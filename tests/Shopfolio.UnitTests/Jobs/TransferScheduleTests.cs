using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Jobs;
using Shopfolio.Infrastructure.Values;

namespace Shopfolio.UnitTests.Jobs;

/// <summary>
/// The clock times of a bank transfer order (T221 criterion 4, KK5; 05 5.3.1, 5.4.3; 02 4.3, 4.1.4; MK-43): the reminder at 08:00 Turkey time
/// of the window's last day, and the end of a window an outage put off. Hours come from <see cref="SchedulerTimings"/>.
/// </summary>
public sealed class TransferScheduleTests
{
    private static readonly BusinessCalendar Calendar = BusinessCalendar.Product;

    [Fact]
    public void ReminderAt_IsTheReminderHourOfTheLastDay_InTurkeyTime()
    {
        DateOnly lastDay = new(2026, 11, 12);
        DateTimeOffset windowEnd = TurkeyTime.StartOf(lastDay.AddDays(1));

        DateTimeOffset reminder = TransferSchedule.ReminderAt(windowEnd);

        Assert.Equal(TurkeyTime.StartOf(lastDay).AddHours(SchedulerTimings.TransferReminderHour), reminder);
        Assert.Equal(lastDay, TurkeyTime.DateOf(reminder));
    }

    [Fact]
    public void ReminderAt_InUtc_IsThreeHoursBeforeTheTurkeyClock()
    {
        DateTimeOffset windowEnd = TurkeyTime.StartOf(new DateOnly(2026, 11, 13));

        DateTimeOffset reminder = TransferSchedule.ReminderAt(windowEnd);

        Assert.Equal(TimeSpan.FromHours(SchedulerTimings.TransferReminderHour) - TurkeyTime.Offset, reminder.UtcDateTime.TimeOfDay);
    }

    [Fact]
    public void DeferredWindowEnd_ReturnOnAWeekday_IsTheEndOfTheNextBusinessDay()
    {
        DateOnly returnDay = new(2026, 11, 4);
        DateTimeOffset returned = TurkeyTime.StartOf(returnDay).AddHours(SchedulerTimings.NightlyHour);

        DateTimeOffset end = TransferSchedule.DeferredWindowEnd(returned, Calendar);

        Assert.Equal(TurkeyTime.StartOf(returnDay.AddDays(2)), end);
    }

    [Fact]
    public void DeferredWindowEnd_ReturnOnAFriday_SkipsTheWeekend()
    {
        DateOnly friday = new(2026, 11, 6);
        DateTimeOffset returned = TurkeyTime.StartOf(friday).AddHours(SchedulerTimings.NightlyHour);

        DateTimeOffset end = TransferSchedule.DeferredWindowEnd(returned, Calendar);

        Assert.Equal(TurkeyTime.StartOf(Calendar.NextBusinessDay(friday).AddDays(1)), end);
        Assert.Equal(DayOfWeek.Monday, Calendar.NextBusinessDay(friday).DayOfWeek);
    }

    [Fact]
    public void DeferredWindowEnd_ReturnBeforeAHoliday_SkipsTheHoliday()
    {
        // 29 October is a national holiday (Thursday in 2026): a return on the 28th puts the end after the 30th.
        DateOnly eve = new(2026, 10, 28);
        DateTimeOffset returned = TurkeyTime.StartOf(eve).AddHours(SchedulerTimings.NightlyHour);

        DateTimeOffset end = TransferSchedule.DeferredWindowEnd(returned, Calendar);

        Assert.Equal(TurkeyTime.StartOf(new DateOnly(2026, 10, 31)), end);
    }

    [Fact]
    public void DeferredWindowEnd_ReturnNearMidnight_UsesTheTurkeyDayNotTheUtcDay()
    {
        // 22:00 UTC on Wednesday is 01:00 on Thursday in Turkey: the day of the return is Thursday.
        DateTimeOffset returned = new(2026, 11, 4, 22, 0, 0, TimeSpan.Zero);

        DateTimeOffset end = TransferSchedule.DeferredWindowEnd(returned, Calendar);

        Assert.Equal(new DateOnly(2026, 11, 5), TurkeyTime.DateOf(returned));
        Assert.Equal(TurkeyTime.StartOf(new DateOnly(2026, 11, 7)), end);
    }
}
