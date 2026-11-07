using Shopfolio.Infrastructure;
using Shopfolio.UnitTests.Fakes;

namespace Shopfolio.UnitTests.Infrastructure;

/// <summary>
/// Counting of periods set in days (T378 criteria 2, 4, 5, 6, 9; KK2, KK4, KK5, KK6; 02 4.1.2 to 4.1.4; 09 7.1 items 3, 4). The start day is
/// not counted and the last day is included as a whole; two periods count business days, the rest calendar days; a span in minutes or hours
/// runs from the instant. Every test is free of the server's time zone and of real time.
/// </summary>
public sealed class PeriodCountingTests
{
    private static readonly BusinessCalendar _calendar = BusinessCalendar.Product;

    // --- KK2: weekend, holiday and the eve of a holiday ---

    [Fact]
    public void LastDay_BusinessPeriodOverAWeekend_SkipsSaturdayAndSunday()
    {
        // Friday 13 March 2026 plus one business day is Monday 16 March.
        Assert.Equal(new DateOnly(2026, 3, 16), _calendar.LastDayOfBusinessPeriod(new DateOnly(2026, 3, 13), 1));
    }

    [Fact]
    public void LastDay_BusinessPeriodOverAHoliday_SkipsTheHolidayDays()
    {
        // Ramazan Bayramı 2026 is Friday 20 to Sunday 22 March: from Thursday 19 the next business day is Monday 23.
        Assert.Equal(new DateOnly(2026, 3, 23), _calendar.LastDayOfBusinessPeriod(new DateOnly(2026, 3, 19), 1));
    }

    [Fact]
    public void LastDay_TheEveOfAHoliday_IsAFullBusinessDay()
    {
        // Wednesday 18 March plus one business day is the eve, Thursday 19 March (02 4.1.2).
        Assert.Equal(new DateOnly(2026, 3, 19), _calendar.LastDayOfBusinessPeriod(new DateOnly(2026, 3, 18), 1));
        // The eve of 29 October 2026 (Cumhuriyet Bayramı, Thursday) is Wednesday 28 October, a business day.
        Assert.Equal(new DateOnly(2026, 10, 28), _calendar.LastDayOfBusinessPeriod(new DateOnly(2026, 10, 27), 1));
    }

    [Fact]
    public void LastDay_BusinessPeriodOfSeveralDays_CountsOnlyBusinessDays()
    {
        // Ten business days from Wednesday 18 March 2026, the bayram days out:
        // 19(1) 23(2) 24(3) 25(4) 26(5) 27(6) 30(7) 31(8) 1 April(9) 2 April(10).
        Assert.Equal(new DateOnly(2026, 4, 2), _calendar.LastDayOfBusinessPeriod(new DateOnly(2026, 3, 18), 10));
    }

    [Fact]
    public void LastDay_BusinessPeriodWithNoDays_IsRefused()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => _calendar.LastDayOfBusinessPeriod(new DateOnly(2026, 3, 18), 0));
    }

    // --- KK3 of T227 kept for the period count: a year the list does not cover leaves only the weekend out ---

    [Fact]
    public void LastDay_YearOutsideTheList_SkipsOnlyTheWeekend()
    {
        // 1 January 2030 is a Tuesday and 2030 is not in the list: Monday 31 December 2029 plus one business day is that Tuesday.
        Assert.False(_calendar.Covers(2030));
        Assert.Equal(new DateOnly(2030, 1, 1), _calendar.LastDayOfBusinessPeriod(new DateOnly(2029, 12, 31), 1));
        Assert.Equal(new DateOnly(2030, 4, 23), _calendar.LastDayOfBusinessPeriod(new DateOnly(2030, 4, 22), 1));
        Assert.Equal(new DateOnly(2030, 1, 7), _calendar.LastDayOfBusinessPeriod(new DateOnly(2030, 1, 4), 1));
    }

    // --- KK4: two periods count business days, the rest calendar days ---

    [Theory]
    [InlineData(CountedPeriod.ShippingPeriod, DayCounting.BusinessDay)]
    [InlineData(CountedPeriod.BankTransferPaymentWindow, DayCounting.BusinessDay)]
    [InlineData(CountedPeriod.ServiceFulfilmentPeriod, DayCounting.CalendarDay)]
    [InlineData(CountedPeriod.WithdrawalWindow, DayCounting.CalendarDay)]
    [InlineData(CountedPeriod.ReturnShipmentPeriod, DayCounting.CalendarDay)]
    [InlineData(CountedPeriod.RefundPeriod, DayCounting.CalendarDay)]
    [InlineData(CountedPeriod.DefectClaimPeriod, DayCounting.CalendarDay)]
    [InlineData(CountedPeriod.DataRequestAnswerPeriod, DayCounting.CalendarDay)]
    [InlineData(CountedPeriod.ImpossibilityNoticePeriod, DayCounting.CalendarDay)]
    [InlineData(CountedPeriod.LegalDeliveryLimit, DayCounting.CalendarDay)]
    public void CountingOf_EachPeriod_IsAsTheRuleSays(CountedPeriod period, DayCounting expected)
    {
        Assert.Equal(expected, PeriodCounting.CountingOf(period));
    }

    [Fact]
    public void CountingOf_EveryPeriodOfTheList_IsDecidedAndOnlyTwoCountBusinessDays()
    {
        CountedPeriod[] all = Enum.GetValues<CountedPeriod>();

        CountedPeriod[] business = [.. all.Where(p => PeriodCounting.CountingOf(p) == DayCounting.BusinessDay)];

        Assert.Equal([CountedPeriod.ShippingPeriod, CountedPeriod.BankTransferPaymentWindow], business);
    }

    [Fact]
    public void CountingOf_ValueOutsideTheList_IsRefused()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => PeriodCounting.CountingOf((CountedPeriod)999));
    }

    [Fact]
    public void LastDay_LegalPeriodOverAHoliday_CountsTheHolidayDays()
    {
        // A withdrawal window of 14 days from Wednesday 18 March 2026 is Wednesday 1 April; the bayram holiday does not lengthen it (02 4.1.3).
        Assert.Equal(new DateOnly(2026, 4, 1), PeriodCounting.LastDay(CountedPeriod.WithdrawalWindow, new DateOnly(2026, 3, 18), 14, _calendar));
        // The same days as a shipping period count business days only.
        Assert.Equal(new DateOnly(2026, 4, 8), PeriodCounting.LastDay(CountedPeriod.ShippingPeriod, new DateOnly(2026, 3, 18), 14, _calendar));
    }

    // --- KK5: a fake clock, the last moment of the last day and 00:00 of the day after ---

    [Fact]
    public void IsWithin_CalendarPeriod_LastMomentOfLastDayIsInsideAndNextMidnightIsOutside()
    {
        // Started on 10 March 2026 (Turkey) for 3 days: it begins 11 March 00:00 and its last day is 13 March.
        DateTimeOffset start = new(2026, 3, 10, 15, 0, 0, TimeSpan.Zero);
        DateTimeOffset nextMidnight = new(2026, 3, 13, 21, 0, 0, TimeSpan.Zero);
        ManualTimeProvider time = new(nextMidnight.AddTicks(-1));

        Assert.True(PeriodCounting.IsWithin(CountedPeriod.WithdrawalWindow, start, 3, time.GetUtcNow(), _calendar));
        time.Advance(TimeSpan.FromTicks(1));
        Assert.False(PeriodCounting.IsWithin(CountedPeriod.WithdrawalWindow, start, 3, time.GetUtcNow(), _calendar));
        Assert.Equal(nextMidnight, PeriodCounting.EndOf(CountedPeriod.WithdrawalWindow, start, 3, _calendar));
    }

    [Fact]
    public void IsWithin_BusinessPeriod_LastMomentOfLastBusinessDayIsInsideAndNextMidnightIsOutside()
    {
        // Started Thursday 19 March 2026 (the eve) for 1 business day: the last day is Monday 23 March, so it ends at 00:00 of Tuesday 24 March.
        DateTimeOffset start = new(2026, 3, 19, 9, 0, 0, TimeSpan.Zero);
        DateTimeOffset end = new(2026, 3, 23, 21, 0, 0, TimeSpan.Zero);
        ManualTimeProvider time = new(end.AddTicks(-1));

        Assert.True(PeriodCounting.IsWithin(CountedPeriod.ShippingPeriod, start, 1, time.GetUtcNow(), _calendar));
        time.Advance(TimeSpan.FromTicks(1));
        Assert.False(PeriodCounting.IsWithin(CountedPeriod.ShippingPeriod, start, 1, time.GetUtcNow(), _calendar));
        Assert.Equal(end, PeriodCounting.EndOf(CountedPeriod.ShippingPeriod, start, 1, _calendar));
    }

    [Fact]
    public void IsWithin_StartDay_IsNotCountedAndTheNextDayBeginsThePeriod()
    {
        DateTimeOffset start = new(2026, 3, 10, 15, 0, 0, TimeSpan.Zero);
        DateTimeOffset nextDayBegins = new(2026, 3, 10, 21, 0, 0, TimeSpan.Zero);

        Assert.False(PeriodCounting.IsWithin(CountedPeriod.RefundPeriod, start, 14, nextDayBegins.AddTicks(-1), _calendar));
        Assert.True(PeriodCounting.IsWithin(CountedPeriod.RefundPeriod, start, 14, nextDayBegins, _calendar));
    }

    [Fact]
    public void EndOf_CalendarPeriod_IsTheSameAsTheTurkeyTimeHelper()
    {
        DateTimeOffset start = new(2026, 7, 1, 23, 30, 0, TimeSpan.Zero);

        Assert.Equal(
            TurkeyTime.EndOfPeriod(TurkeyTime.DateOf(start), 14),
            PeriodCounting.EndOf(CountedPeriod.DataRequestAnswerPeriod, start, 14, _calendar));
    }

    // --- KK6: a span in minutes or hours is outside the day count ---

    [Fact]
    public void SpanEnd_HourSpanStartedLateInTheDay_RunsFromTheInstantAndNotToTheEndOfTheDay()
    {
        // 23:30 Turkey time on 10 March 2026 plus one hour is 00:30 on 11 March - not the end of 10 March.
        DateTimeOffset start = new(2026, 3, 10, 20, 30, 0, TimeSpan.Zero);

        DateTimeOffset end = PeriodCounting.SpanEnd(start, TimeSpan.FromHours(1));

        Assert.Equal(new DateTimeOffset(2026, 3, 10, 21, 30, 0, TimeSpan.Zero), end);
        Assert.Equal(new DateOnly(2026, 3, 11), TurkeyTime.DateOf(end));
        Assert.NotEqual(PeriodCounting.EndOf(CountedPeriod.WithdrawalWindow, start, 1, _calendar), end);
    }

    [Fact]
    public void SpanEnd_MinuteSpan_RunsFromTheInstantWithFakeClock()
    {
        ManualTimeProvider time = new(new DateTimeOffset(2026, 3, 10, 12, 0, 0, TimeSpan.Zero));
        DateTimeOffset end = PeriodCounting.SpanEnd(time.GetUtcNow(), TimeSpan.FromMinutes(30));

        time.Advance(TimeSpan.FromMinutes(30).Subtract(TimeSpan.FromTicks(1)));
        Assert.True(time.GetUtcNow() < end);
        time.Advance(TimeSpan.FromTicks(1));
        Assert.Equal(end, time.GetUtcNow());
    }
}
