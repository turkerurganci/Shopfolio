using Shopfolio.Infrastructure;

namespace Shopfolio.UnitTests.Infrastructure;

/// <summary>
/// The business day calendar (T227 criterion 4; KK4; 05 7.2.3; 02 4.1.2): the holiday list is a data file inside the application; in a year
/// it does not cover only the weekend is excluded. The parser tests read text, not files.
/// </summary>
public sealed class BusinessCalendarTests
{
    /// <summary>The fixed national holidays (month, day) every covered year has.</summary>
    private static readonly (int Month, int Day)[] _fixedHolidays = [(1, 1), (4, 23), (5, 1), (5, 19), (7, 15), (8, 30), (10, 29)];

    [Fact]
    public void Product_LoadsTheShippedHolidayFile()
    {
        BusinessCalendar calendar = BusinessCalendar.Product;

        Assert.True(calendar.Covers(2026));
        Assert.True(calendar.Covers(2027));
        Assert.True(calendar.IsHoliday(new DateOnly(2026, 3, 20)));
    }

    [Fact]
    public void Product_EveryCoveredYear_HasTheFixedNationalHolidays()
    {
        BusinessCalendar calendar = BusinessCalendar.Product;

        foreach (int year in calendar.Holidays.Select(h => h.Year).Distinct())
        {
            foreach ((int month, int day) in _fixedHolidays)
            {
                Assert.True(calendar.IsHoliday(new DateOnly(year, month, day)), $"{year}-{month:00}-{day:00} is missing");
            }
        }
    }

    [Fact]
    public void Product_HalfDays_AreBusinessDays()
    {
        // The eve of Ramazan Bayramı 2026 (Thursday 19 March) and 28 October 2026 (Wednesday) are half days, full business days (02 4.1.2).
        Assert.True(BusinessCalendar.Product.IsBusinessDay(new DateOnly(2026, 3, 19)));
        Assert.True(BusinessCalendar.Product.IsBusinessDay(new DateOnly(2026, 10, 28)));
    }

    [Fact]
    public void IsBusinessDay_WeekdayHolidayAndWeekend_InACoveredYear()
    {
        BusinessCalendar calendar = BusinessCalendar.Product;

        Assert.True(calendar.IsBusinessDay(new DateOnly(2026, 3, 18)));   // Wednesday
        Assert.False(calendar.IsBusinessDay(new DateOnly(2026, 3, 20)));  // Friday, Ramazan Bayramı
        Assert.False(calendar.IsBusinessDay(new DateOnly(2026, 3, 21)));  // Saturday
        Assert.False(calendar.IsBusinessDay(new DateOnly(2026, 3, 22)));  // Sunday
        Assert.True(calendar.IsBusinessDay(new DateOnly(2026, 3, 23)));   // Monday
    }

    [Fact]
    public void IsBusinessDay_YearOutsideTheList_ExcludesOnlyTheWeekend()
    {
        // KK4: 2030 is not covered; its 1 January (a Tuesday) and 29 October (a Tuesday) are business days, the weekend is not.
        BusinessCalendar calendar = BusinessCalendar.Product;

        Assert.False(calendar.Covers(2030));
        Assert.True(calendar.IsBusinessDay(new DateOnly(2030, 1, 1)));
        Assert.True(calendar.IsBusinessDay(new DateOnly(2030, 10, 29)));
        Assert.False(calendar.IsBusinessDay(new DateOnly(2030, 1, 5)));
        Assert.False(calendar.IsBusinessDay(new DateOnly(2030, 1, 6)));
        Assert.DoesNotContain(calendar.Holidays, h => h.Year == 2030);
    }

    [Fact]
    public void IsBusinessDay_EveryDayOfAnUncoveredYear_IsBusinessExactlyOnWeekdays()
    {
        BusinessCalendar calendar = BusinessCalendar.Parse("2026-01-01 Yılbaşı");

        for (DateOnly day = new(2031, 1, 1); day.Year == 2031; day = day.AddDays(1))
        {
            bool weekend = day.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday;
            Assert.Equal(!weekend, calendar.IsBusinessDay(day));
        }
    }

    [Fact]
    public void Parse_SkipsCommentsAndBlankLinesAndReadsTheDays()
    {
        BusinessCalendar calendar = BusinessCalendar.Parse("# list\n\n2028-01-01 Yılbaşı\r\n2028-04-23 Ulusal Egemenlik ve Çocuk Bayramı\n");

        Assert.Equal([new DateOnly(2028, 1, 1), new DateOnly(2028, 4, 23)], calendar.Holidays);
        Assert.True(calendar.Covers(2028));
        Assert.False(calendar.Covers(2029));
    }

    [Theory]
    [InlineData("2028-13-01 Bad month", 1)]
    [InlineData("# head\n01.01.2028 Yılbaşı", 2)]
    [InlineData("2028-01-01 Yılbaşı\n2028-01-01 Yılbaşı", 2)]
    [InlineData("2028-04-23 A\n2028-01-01 B", 2)]
    public void Parse_BadDayDuplicateOrOrder_ThrowsNamingTheLine(string text, int line)
    {
        FormatException error = Assert.Throws<FormatException>(() => BusinessCalendar.Parse(text));

        Assert.Contains($"line {line}:", error.Message, StringComparison.Ordinal);
    }
}
