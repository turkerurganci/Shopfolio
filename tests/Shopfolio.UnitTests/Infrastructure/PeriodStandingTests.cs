using Shopfolio.Infrastructure;

namespace Shopfolio.UnitTests.Infrastructure;

/// <summary>
/// Where a running period stands (T325 criteria 1, 6; 04 2.6.3, 2.6.6; K-767): the days left up to and including the last day, zero on the last day,
/// the days over from the day after it, business days for a business-day period, and the edge of the last moment and the next day's 00:00 in Turkey time.
/// </summary>
public sealed class PeriodStandingTests
{
    // Monday 16 November 2026 and the week before carry no public holiday.
    private static readonly DateOnly LastDay = new(2026, 11, 16);

    private static DateTimeOffset Turkey(DateOnly day, int hour) => TurkeyTime.StartOf(day).AddHours(hour);

    [Fact]
    public void CalendarPeriod_CountsTheDaysAfterToday_UpToTheLastDay()
    {
        PeriodStanding standing = PeriodStanding.Of(CountedPeriod.RefundPeriod, LastDay, Turkey(LastDay.AddDays(-3), 10), BusinessCalendar.Product);

        Assert.Equal((3, 0, false), (standing.DaysLeft, standing.DaysOver, standing.IsOver));
        Assert.Equal(TurkeyTime.StartOf(LastDay.AddDays(1)), standing.EndsAt);
        Assert.Equal(LastDay, standing.LastDay);
    }

    [Fact]
    public void LastMomentOfTheLastDay_IsInside_AndTheNextDaysMidnight_IsOneDayOver()
    {
        DateTimeOffset end = TurkeyTime.StartOf(LastDay.AddDays(1));

        PeriodStanding last = PeriodStanding.Of(CountedPeriod.RefundPeriod, LastDay, end.AddTicks(-1), BusinessCalendar.Product);
        PeriodStanding over = PeriodStanding.Of(CountedPeriod.RefundPeriod, LastDay, end, BusinessCalendar.Product);
        PeriodStanding later = PeriodStanding.Of(CountedPeriod.RefundPeriod, LastDay, end.AddDays(4), BusinessCalendar.Product);

        Assert.Equal((0, 0, false), (last.DaysLeft, last.DaysOver, last.IsOver));
        Assert.Equal((0, 1, true), (over.DaysLeft, over.DaysOver, over.IsOver));
        Assert.Equal(5, later.DaysOver);
    }

    [Fact]
    public void TheDayOfTheInstant_IsItsTurkeyDay()
    {
        // 21:30 UTC on the day before the last day is 00:30 of the last day in Turkey: no day is left.
        DateTimeOffset utcEvening = new(2026, 11, 15, 21, 30, 0, TimeSpan.Zero);

        Assert.Equal(0, PeriodStanding.Of(CountedPeriod.RefundPeriod, LastDay, utcEvening, BusinessCalendar.Product).DaysLeft);
    }

    [Fact]
    public void BusinessDayPeriod_LeavesTheWeekendOutOfTheDaysLeft()
    {
        // Friday 13 November: Saturday and Sunday are not counted, Monday the last day is one business day left.
        PeriodStanding shipping = PeriodStanding.Of(CountedPeriod.ShippingPeriod, LastDay, Turkey(new DateOnly(2026, 11, 13), 10), BusinessCalendar.Product);
        PeriodStanding refund = PeriodStanding.Of(CountedPeriod.RefundPeriod, LastDay, Turkey(new DateOnly(2026, 11, 13), 10), BusinessCalendar.Product);

        Assert.Equal(1, shipping.DaysLeft);
        Assert.Equal(3, refund.DaysLeft);
    }
}
