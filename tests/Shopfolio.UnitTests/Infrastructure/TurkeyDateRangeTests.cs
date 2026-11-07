using Shopfolio.Infrastructure;
using Shopfolio.UnitTests.Fakes;

namespace Shopfolio.UnitTests.Infrastructure;

/// <summary>
/// A date range entered as two dates (T378 criterion 7; KK7; 02 4.1.4; 09 7.1 item 3): it begins at 00:00 of the start day and ends at the
/// end of the end day, so the end day is included as a whole and the range runs out at 00:00 of the day after.
/// </summary>
public sealed class TurkeyDateRangeTests
{
    private static readonly DateOnly _start = new(2026, 3, 10);

    private static readonly DateOnly _end = new(2026, 3, 12);

    [Fact]
    public void IsWithinRange_StartDayMidnight_IsInsideAndOneTickBeforeIsOutside()
    {
        DateTimeOffset startMidnight = TurkeyTime.StartOf(_start);
        ManualTimeProvider time = new(startMidnight.AddTicks(-1));

        Assert.False(TurkeyTime.IsWithinRange(time.GetUtcNow(), _start, _end));
        time.Advance(TimeSpan.FromTicks(1));
        Assert.True(TurkeyTime.IsWithinRange(time.GetUtcNow(), _start, _end));
        Assert.Equal(new DateTimeOffset(2026, 3, 9, 21, 0, 0, TimeSpan.Zero), startMidnight);
    }

    [Fact]
    public void IsWithinRange_EndDay_IsIncludedAsAWholeAndTheNextMidnightIsOutside()
    {
        DateTimeOffset endsAt = TurkeyTime.EndOfRange(_end);
        ManualTimeProvider time = new(endsAt.AddTicks(-1));

        Assert.True(TurkeyTime.IsWithinRange(time.GetUtcNow(), _start, _end));
        time.Advance(TimeSpan.FromTicks(1));
        Assert.False(TurkeyTime.IsWithinRange(time.GetUtcNow(), _start, _end));
        Assert.Equal(new DateTimeOffset(2026, 3, 12, 21, 0, 0, TimeSpan.Zero), endsAt);
    }

    [Fact]
    public void IsWithinRange_OneDayRange_HoldsExactlyThatDay()
    {
        Assert.True(TurkeyTime.IsWithinRange(TurkeyTime.StartOf(_start), _start, _start));
        Assert.True(TurkeyTime.IsWithinRange(TurkeyTime.StartOf(_start.AddDays(1)).AddTicks(-1), _start, _start));
        Assert.False(TurkeyTime.IsWithinRange(TurkeyTime.StartOf(_start.AddDays(1)), _start, _start));
    }

    [Fact]
    public void IsWithinRange_EndBeforeStart_IsACallerError()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => TurkeyTime.IsWithinRange(TurkeyTime.StartOf(_start), _end, _start));
    }
}
