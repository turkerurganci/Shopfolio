using System.Globalization;

using Shopfolio.Infrastructure;

namespace Shopfolio.UnitTests.Infrastructure;

/// <summary>
/// The one display format of a page (T216 criteria 1, 2; KK1, KK2; 04 8.2; 07 2.9.1, 2.9.2): the examples of 04 8.2 one by one, the Turkey day of an
/// instant near midnight, and the same text whatever the thread's culture (the browser's language never reaches the format; 07 2.9.9).
/// </summary>
public sealed class DisplayFormatTests
{
    // 04 8.2.1: the day without a leading zero, the Turkish month name, four digits of the year; no day name.
    [Theory]
    [InlineData(2026, 10, 4, "4 Ekim 2026")]
    [InlineData(2026, 1, 31, "31 Ocak 2026")]
    [InlineData(2026, 2, 1, "1 Şubat 2026")]
    [InlineData(2026, 8, 15, "15 Ağustos 2026")]
    [InlineData(2026, 12, 9, "9 Aralık 2026")]
    public void Date_IsDayMonthNameAndYear(int year, int month, int day, string expected)
    {
        Assert.Equal(expected, DisplayFormat.Date(new DateOnly(year, month, day)));
    }

    [Fact]
    public void DateRange_IsTwoDatesWithTheDash()
    {
        Assert.Equal("1 Ekim 2026 – 31 Ekim 2026", DisplayFormat.DateRange(new DateOnly(2026, 10, 1), new DateOnly(2026, 10, 31)));
    }

    // KK2 (02 4.1.1; 07 2.9.2): an instant is shown on its Turkey day; 20:59:59 UTC is still the 4th, 21:00 UTC is the 5th.
    [Fact]
    public void Date_OfAnInstantNearMidnight_FallsOnItsTurkeyDay()
    {
        DateTimeOffset lastTick = new(2026, 10, 4, 20, 59, 59, TimeSpan.Zero);
        DateTimeOffset midnight = TurkeyTime.StartOf(new DateOnly(2026, 10, 5));

        Assert.Equal("4 Ekim 2026", DisplayFormat.Date(lastTick));
        Assert.Equal("4 Ekim 2026", DisplayFormat.Date(midnight.AddTicks(-1)));
        Assert.Equal("5 Ekim 2026", DisplayFormat.Date(midnight));
        Assert.Equal("5 Ekim 2026, 00:00", DisplayFormat.DateAndTime(midnight));
        Assert.Equal("4 Ekim 2026, 23:59", DisplayFormat.DateAndTime(lastTick));
    }

    // 04 8.2.2: twenty-four hours, no seconds, no time zone; an instant with another offset is the same Turkey time.
    [Fact]
    public void Time_IsTurkeyTime_TwentyFourHours_WithoutSecondsOrZone()
    {
        DateTimeOffset utc = new(2026, 10, 4, 11, 30, 45, TimeSpan.Zero);

        Assert.Equal("14:30", DisplayFormat.Time(utc));
        Assert.Equal("14:30", DisplayFormat.Time(utc.ToOffset(TimeSpan.FromHours(-5))));
        Assert.Equal("4 Ekim 2026, 14:30", DisplayFormat.DateAndTime(utc));
        Assert.Equal("4 Ekim 2026, 23:05", DisplayFormat.DateAndTime(new DateTimeOffset(2026, 10, 4, 20, 5, 0, TimeSpan.Zero)));
    }

    // 04 8.2.3: the point groups thousands, the comma separates decimals; the percent sign comes first, without a blank.
    [Fact]
    public void NumberAndPercent_AreTheTurkishForms()
    {
        Assert.Equal("1.250", DisplayFormat.Number(1250L));
        Assert.Equal("1.000.000", DisplayFormat.Number(1_000_000L));
        Assert.Equal("7", DisplayFormat.Number(7L));
        Assert.Equal("0,5", DisplayFormat.Number(0.5m));
        Assert.Equal("1.250", DisplayFormat.Number(1250.00m));
        Assert.Equal("1.250,75", DisplayFormat.Number(1250.75m));
        Assert.Equal("%20", DisplayFormat.Percent(20m));
        Assert.Equal("%12,5", DisplayFormat.Percent(12.50m));
        Assert.Equal("%100", DisplayFormat.Percent(100m));
    }

    // 04 8.2.4: the number, a blank and "TL"; two decimals with kuruş, none without; a deducted amount with the minus sign.
    [Theory]
    [InlineData("266.67", "266,67 TL")]
    [InlineData("1200", "1.200 TL")]
    [InlineData("1200.00", "1.200 TL")]
    [InlineData("1200.5", "1.200,50 TL")]
    [InlineData("0.05", "0,05 TL")]
    [InlineData("0", "0 TL")]
    [InlineData("-50", "-50 TL")]
    [InlineData("-12.5", "-12,50 TL")]
    [InlineData("9999999999.99", "9.999.999.999,99 TL")]
    public void Amount_IsTheNumberAndTL(string value, string expected)
    {
        Assert.Equal(expected, DisplayFormat.Amount(decimal.Parse(value, CultureInfo.InvariantCulture)));
    }

    [Fact]
    public void Amount_NeverRoundsAValueSilently_AndUsesNeitherTheSymbolNorTheCode()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => DisplayFormat.Amount(12.345m));
        Assert.Equal("12,30 TL", DisplayFormat.Amount(12.300m));
        Assert.Equal("TL", DisplayFormat.Currency);
        Assert.DoesNotContain("₺", DisplayFormat.Amount(10m), StringComparison.Ordinal);
        Assert.DoesNotContain("TRY", DisplayFormat.Amount(10m), StringComparison.Ordinal);
    }

    // 04 8.2.5: a period in days or business days.
    [Fact]
    public void DaysAndBusinessDays_AreTheNumberAndTheUnit()
    {
        Assert.Equal("3 gün", DisplayFormat.Days(3));
        Assert.Equal("2 iş günü", DisplayFormat.BusinessDays(2));
        Assert.Equal("1.000 gün", DisplayFormat.Days(1000));
    }

    // 04 8.2.6: the telephone number grouped with the area code, the IBAN in groups of four; the stored and the typed forms show the same.
    [Theory]
    [InlineData("5321234567")]
    [InlineData("0532 123 45 67")]
    [InlineData("+90 (532) 123-45-67")]
    public void Phone_IsGroupedWithTheAreaCode(string value)
    {
        Assert.Equal("0532 123 45 67", DisplayFormat.Phone(value));
    }

    [Fact]
    public void Phone_ThatIsNotTenDigits_IsShownAsItIs()
    {
        Assert.Equal("12345", DisplayFormat.Phone("12345"));
    }

    [Theory]
    [InlineData("TR330006100519786457841326")]
    [InlineData("TR33 0006 1005 1978 6457 8413 26")]
    [InlineData("tr33 0006100519786457841326")]
    public void Iban_IsInGroupsOfFour(string value)
    {
        Assert.Equal("TR33 0006 1005 1978 6457 8413 26", DisplayFormat.Iban(value));
    }

    // KK1 (07 2.9.9; 10 KD-20): the thread's culture - which a browser's Accept-Language could set - changes no text.
    [Theory]
    [InlineData("en-US")]
    [InlineData("de-DE")]
    [InlineData("ar-SA")]
    [InlineData("")]
    public void EveryFormat_IsTheSame_WhateverTheThreadCulture(string culture)
    {
        DateTimeOffset instant = new(2026, 10, 4, 11, 30, 0, TimeSpan.Zero);
        string[] turkish = Texts(instant);

        CultureInfo before = CultureInfo.CurrentCulture;
        CultureInfo beforeUi = CultureInfo.CurrentUICulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture);
            CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo(culture);

            Assert.Equal(turkish, Texts(instant));
        }
        finally
        {
            CultureInfo.CurrentCulture = before;
            CultureInfo.CurrentUICulture = beforeUi;
        }

        Assert.Equal(
            ["4 Ekim 2026", "14:30", "4 Ekim 2026, 14:30", "1.250", "0,5", "%12,5", "1.200 TL", "266,67 TL", "3 gün", "0532 123 45 67", "TR33 0006 1005 1978 6457 8413 26"],
            turkish);
    }

    private static string[] Texts(DateTimeOffset instant) =>
    [
        DisplayFormat.Date(instant), DisplayFormat.Time(instant), DisplayFormat.DateAndTime(instant), DisplayFormat.Number(1250L),
        DisplayFormat.Number(0.5m), DisplayFormat.Percent(12.5m), DisplayFormat.Amount(1200m), DisplayFormat.Amount(266.67m), DisplayFormat.Days(3),
        DisplayFormat.Phone("5321234567"), DisplayFormat.Iban("TR330006100519786457841326"),
    ];
}
