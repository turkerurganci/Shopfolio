using Shopfolio.Infrastructure;
using Shopfolio.Modules.Ordering;

namespace Shopfolio.UnitTests.Ordering;

/// <summary>The two legal periods of the order's disposal part (T301; 06 9.1 Z-28, Z-41): the edges are counted in Turkey time and handed over as UTC instants.</summary>
public sealed class OrderRetentionTests
{
    [Fact]
    public void PaidCreatedBefore_IsTheStartOfJanuaryOfTheYearTenYearsAgo_InTurkeyTime()
    {
        DateTimeOffset now = new(2027, 6, 15, 12, 0, 0, TimeSpan.Zero);

        DateTimeOffset edge = OrderRetention.PaidCreatedBefore(now);

        Assert.Equal(new DateTimeOffset(2016, 12, 31, 21, 0, 0, TimeSpan.Zero), edge);
        Assert.Equal(TimeSpan.Zero, edge.Offset);
    }

    [Fact]
    public void PaidCreatedBefore_MovesAtTheTurnOfTheYearInTurkeyTime_NotInUtc()
    {
        DateTimeOffset beforeTurn = new(2026, 12, 31, 20, 59, 59, TimeSpan.Zero);
        DateTimeOffset atTurn = new(2026, 12, 31, 21, 0, 0, TimeSpan.Zero);

        Assert.Equal(new DateTimeOffset(2015, 12, 31, 21, 0, 0, TimeSpan.Zero), OrderRetention.PaidCreatedBefore(beforeTurn));
        Assert.Equal(new DateTimeOffset(2016, 12, 31, 21, 0, 0, TimeSpan.Zero), OrderRetention.PaidCreatedBefore(atTurn));
    }

    [Fact]
    public void PaidCreatedBefore_AnOrderOfTheLastSecondOfItsYear_IsNotPastTheEdgeUntilTheNextTurn()
    {
        DateTimeOffset now = new(2027, 1, 1, 0, 0, 0, TimeSpan.Zero);
        DateTimeOffset createdLastSecond = TurkeyTime.StartOf(new DateOnly(2017, 1, 1)).AddSeconds(-1);

        Assert.True(createdLastSecond < OrderRetention.PaidCreatedBefore(now));
        Assert.False(createdLastSecond < OrderRetention.PaidCreatedBefore(now.AddDays(-2)));
    }

    [Fact]
    public void FailedBefore_IsThreeYearsBack_AsAnUtcInstant()
    {
        DateTimeOffset now = new(2027, 3, 10, 12, 0, 0, TimeSpan.Zero);

        DateTimeOffset edge = OrderRetention.FailedBefore(now);

        Assert.Equal(new DateTimeOffset(2024, 3, 10, 12, 0, 0, TimeSpan.Zero), edge);
        Assert.Equal(TimeSpan.Zero, edge.Offset);
    }

    [Fact]
    public void FailedBefore_CountsTheThreeYearsInTurkeyCalendar_OverALeapDay()
    {
        DateTimeOffset now = new(2028, 2, 29, 12, 0, 0, TimeSpan.Zero);

        Assert.Equal(new DateTimeOffset(2025, 2, 28, 12, 0, 0, TimeSpan.Zero), OrderRetention.FailedBefore(now));
    }
}
