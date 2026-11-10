using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Disposal;

namespace Shopfolio.UnitTests.Disposal;

/// <summary>
/// The periods of the system logs as pure functions (T201 criterion 9, KK9; 05 9.2.2; P-45): six calendar months in Turkey time, the newest
/// expired day, and the names of the files the disposal may delete. Every date is built from <see cref="ProductConstants.SystemLogRetentionMonths"/>.
/// </summary>
public sealed class LogFileRetentionTests
{
    private static readonly DateTimeOffset Night = new(2026, 11, 6, 0, 0, 0, TimeSpan.Zero); // 03:00 in Turkey

    [Fact]
    public void Cutoff_IsSixCalendarMonthsBack_InTurkeyTime()
    {
        DateTimeOffset cutoff = LogFileRetention.Cutoff(Night);

        Assert.Equal(Night.ToOffset(TurkeyTime.Offset).AddMonths(-ProductConstants.SystemLogRetentionMonths), cutoff);
        Assert.Equal(new DateTimeOffset(2026, 5, 6, 3, 0, 0, TurkeyTime.Offset), cutoff);
        Assert.Equal(TimeSpan.Zero, cutoff.Offset);
    }

    [Fact]
    public void NewestExpiredDay_IsTheDayBeforeTheDayTheCutoffFallsOn()
    {
        DateOnly cutoffDay = TurkeyTime.DateOf(LogFileRetention.Cutoff(Night));

        Assert.Equal(cutoffDay.AddDays(-1), LogFileRetention.NewestExpiredDay(Night));
        Assert.Equal(new DateOnly(2026, 5, 5), LogFileRetention.NewestExpiredDay(Night));
    }

    [Fact]
    public void NewestExpiredDay_ADayWholeIsExpired_OnlyWhenItsLastInstantIsSixMonthsOld()
    {
        DateOnly day = LogFileRetention.NewestExpiredDay(Night);
        DateTimeOffset endOfDay = TurkeyTime.StartOf(day.AddDays(1));

        Assert.True(endOfDay.ToOffset(TurkeyTime.Offset).AddMonths(ProductConstants.SystemLogRetentionMonths) <= Night);
        Assert.True(TurkeyTime.StartOf(day.AddDays(2)).ToOffset(TurkeyTime.Offset).AddMonths(ProductConstants.SystemLogRetentionMonths) > Night);
    }

    [Fact]
    public void NewestExpiredDay_AtTheEndOfALongMonth_ClampsLikeTheCalendar()
    {
        DateTimeOffset thirtyFirst = new(2026, 8, 31, 0, 0, 0, TimeSpan.Zero);

        Assert.Equal(new DateOnly(2026, 2, 27), LogFileRetention.NewestExpiredDay(thirtyFirst));
    }

    [Theory]
    [InlineData("app-20261105.jsonl", true)]
    [InlineData("audit-20261105.jsonl", true)]
    [InlineData("command-20261105.jsonl", true)]
    [InlineData("app-20261105.jsonl.bak", false)]
    [InlineData("app-2026110.jsonl", false)]
    [InlineData("unrelated.txt", false)]
    [InlineData("access.log", false)]
    public void IsApplicationFile_OnlyTheThreeKindsOfTheApplication(string name, bool expected)
    {
        Assert.Equal(expected, LogFileRetention.IsApplicationFile(name));
    }

    [Theory]
    [InlineData("access.log-20261105", true)]
    [InlineData("access.log", false)]
    [InlineData("access.log-20261105.gz", false)]
    [InlineData("app-20261105.jsonl", false)]
    public void IsRotatedAccessFile_NeverTheFileBeingWritten(string name, bool expected)
    {
        Assert.Equal(expected, LogFileRetention.IsRotatedAccessFile(name));
    }
}
