using Shopfolio.Infrastructure;
using Shopfolio.UnitTests.Fakes;

namespace Shopfolio.UnitTests.Infrastructure;

/// <summary>
/// Turkey time and periods set in days (T227 criteria 1, 3; KK1, KK3; 09 7.1 items 3, 4; 05 7.2.1, 7.2.2; 02 4.1.4). Time is a UTC instant;
/// the day is counted with the fixed +03:00 offset. The results do not depend on the server's time zone: the helper reads no local zone,
/// and the architecture test <c>TimeAndMoneyRulesTests</c> keeps every server-zone read out of the application.
/// </summary>
public sealed class TurkeyTimeTests
{
    private static readonly DateTimeOffset _turkeyMidnightOf2March = new(2026, 3, 1, 21, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Offset_IsFixedPlusThreeHours()
    {
        Assert.Equal(TimeSpan.FromHours(3), TurkeyTime.Offset);
    }

    [Fact]
    public void DateOf_OneTickBeforeTurkeyMidnight_IsPreviousDay()
    {
        // Turkey's 2 March 00:00 is 1 March 21:00 UTC (+03:00, 09 7.1 item 3).
        ManualTimeProvider time = new(_turkeyMidnightOf2March.AddTicks(-1));

        Assert.Equal(new DateOnly(2026, 3, 1), TurkeyTime.DateOf(time.GetUtcNow()));
    }

    [Fact]
    public void DateOf_AtTurkeyMidnight_IsTheNewDay()
    {
        Assert.Equal(new DateOnly(2026, 3, 2), TurkeyTime.DateOf(_turkeyMidnightOf2March));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(3)]
    [InlineData(-8)]
    [InlineData(14)]
    public void DateOf_SameInstantWrittenWithAnyOffset_IsTheSameTurkeyDay(int offsetHours)
    {
        DateTimeOffset written = _turkeyMidnightOf2March.AddMinutes(-1).ToOffset(TimeSpan.FromHours(offsetHours));

        Assert.Equal(new DateOnly(2026, 3, 1), TurkeyTime.DateOf(written));
    }

    [Fact]
    public void StartOf_Day_IsTurkeyMidnightAsUtcInstant()
    {
        DateTimeOffset start = TurkeyTime.StartOf(new DateOnly(2026, 3, 2));

        Assert.Equal(_turkeyMidnightOf2March, start);
        Assert.Equal(TimeSpan.Zero, start.Offset);
    }

    [Fact]
    public void StartOf_AndDateOf_TurnAnInstantToTurkeyTimeAndBack()
    {
        // An instant goes to its Turkey day and the day's start comes back as UTC, the instant inside that day (KK1).
        DateTimeOffset instant = new(2026, 12, 31, 22, 30, 0, TimeSpan.Zero);

        DateOnly day = TurkeyTime.DateOf(instant);

        Assert.Equal(new DateOnly(2027, 1, 1), day);
        Assert.Equal(new DateTimeOffset(2026, 12, 31, 21, 0, 0, TimeSpan.Zero), TurkeyTime.StartOf(day));
        Assert.True(TurkeyTime.StartOf(day) <= instant && instant < TurkeyTime.StartOf(day.AddDays(1)));
    }

    [Fact]
    public void LastDayOfPeriod_StartDayIsNotCounted()
    {
        // "14 days" started on 1 March: the period begins on 2 March and its last day is 15 March (02 4.1.4).
        Assert.Equal(new DateOnly(2026, 3, 15), TurkeyTime.LastDayOfPeriod(new DateOnly(2026, 3, 1), 14));
    }

    [Fact]
    public void EndOfPeriod_IsTheNextDaysTurkeyMidnight()
    {
        // The last day is 15 March; the period runs out at 16 March 00:00 Turkey time = 15 March 21:00 UTC.
        Assert.Equal(new DateTimeOffset(2026, 3, 15, 21, 0, 0, TimeSpan.Zero), TurkeyTime.EndOfPeriod(new DateOnly(2026, 3, 1), 14));
    }

    [Fact]
    public void IsWithinPeriod_LastMomentOfLastDay_IsInsideAndNextMidnightIsOutside()
    {
        // KK3: the last moment of the last day is inside, the next day's 00:00 is outside; no 23:59 is compared.
        DateOnly startDay = new(2026, 3, 1);
        DateTimeOffset end = TurkeyTime.EndOfPeriod(startDay, 14);
        ManualTimeProvider time = new(end.AddTicks(-1));

        Assert.True(TurkeyTime.IsWithinPeriod(time.GetUtcNow(), startDay, 14));

        time.Advance(TimeSpan.FromTicks(1));
        Assert.False(TurkeyTime.IsWithinPeriod(time.GetUtcNow(), startDay, 14));
    }

    [Fact]
    public void IsWithinPeriod_LastMinuteAfter2359_IsStillInside()
    {
        // 23:59:30 on the last day is past a written 23:59 but still inside the day.
        DateTimeOffset lastMinute = new(2026, 3, 15, 23, 59, 30, TurkeyTime.Offset);

        Assert.True(TurkeyTime.IsWithinPeriod(lastMinute, new DateOnly(2026, 3, 1), 14));
    }

    [Fact]
    public void IsWithinPeriod_StartDay_IsNotCounted()
    {
        DateOnly startDay = new(2026, 3, 1);

        Assert.False(TurkeyTime.IsWithinPeriod(TurkeyTime.StartOf(startDay.AddDays(1)).AddTicks(-1), startDay, 14));
        Assert.True(TurkeyTime.IsWithinPeriod(TurkeyTime.StartOf(startDay.AddDays(1)), startDay, 14));
    }

    [Fact]
    public void Period_OfZeroDays_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => TurkeyTime.EndOfPeriod(new DateOnly(2026, 3, 1), 0));
    }

    [Fact]
    public void PeriodTest_WithFakeClock_MovesTimeWithoutWaiting()
    {
        // KK2: a period test with the fake clock. A 3-day period starts on Friday 6 March at 18:00 Turkey time; the clock is moved, not waited.
        ManualTimeProvider time = new(new DateTimeOffset(2026, 3, 6, 18, 0, 0, TurkeyTime.Offset));
        DateOnly startDay = TurkeyTime.DateOf(time.GetUtcNow());
        DateTimeOffset end = TurkeyTime.EndOfPeriod(startDay, 3);

        time.Advance(TimeSpan.FromDays(3));
        Assert.True(time.GetUtcNow() < end);

        time.SetUtcNow(end);
        Assert.False(TurkeyTime.IsWithinPeriod(time.GetUtcNow(), startDay, 3));
        Assert.Equal(new DateOnly(2026, 3, 10), TurkeyTime.DateOf(end));
    }
}
