using System.Globalization;

using Shopfolio.Infrastructure;

namespace Shopfolio.UnitTests.Infrastructure;

/// <summary>
/// The format of the values a request carries (T216 criteria 2-5; KK3, KK4, KK5; 07 2.9.3-2.9.5; 09 7.2 items 1-3, 6): the culture is Turkish and
/// explicit, an amount is digits and at most one comma, a date is <c>yyyy-AA-GG</c>, a whole number is ASCII digits, and a telephone number and an IBAN
/// are the same value with or without blanks. The table of 09 7.2 item 6 is here; the list filters read the same functions (<c>ListFiltersTests</c>).
/// </summary>
public sealed class RequestFormatTests
{
    [Fact]
    public void Culture_IsTurkish_TheCommaIsTheDecimalSeparator_AndThePointTheGroupSeparator()
    {
        // 09 7.2 item 1: the application does not run in the invariant globalization mode; tr-TR needs ICU.
        Assert.Equal("tr-TR", RequestFormat.Culture.Name);
        Assert.Equal(",", RequestFormat.Culture.NumberFormat.NumberDecimalSeparator);
        Assert.Equal(".", RequestFormat.Culture.NumberFormat.NumberGroupSeparator);
        Assert.Equal("Ekim", RequestFormat.Culture.DateTimeFormat.GetMonthName(10));
    }

    // 09 7.2 item 6: "12," and ",5" are digits and one comma; the framework's parser reads them as 12 and 0,5 (measured in T216). Neither is made larger.
    [Theory]
    [InlineData("12,5", true, "12.5")]
    [InlineData("12", true, "12")]
    [InlineData("1234,50", true, "1234.50")]
    [InlineData("0,01", true, "0.01")]
    [InlineData("12,", true, "12")]
    [InlineData(",5", true, "0.5")]
    [InlineData("12.50", false, "0")]
    [InlineData("1.250,50", false, "0")]
    [InlineData("1.250", false, "0")]
    [InlineData("12,5,1", false, "0")]
    [InlineData("-5", false, "0")]
    [InlineData("+5", false, "0")]
    [InlineData("12 TL", false, "0")]
    [InlineData("12TL", false, "0")]
    [InlineData("1e3", false, "0")]
    [InlineData("1 250", false, "0")]
    [InlineData("(5)", false, "0")]
    [InlineData("₺12", false, "0")]
    [InlineData("١٢", false, "0")]
    [InlineData(",", false, "0")]
    [InlineData("", false, "0")]
    [InlineData(null, false, "0")]
    public void TryParseDecimal_TheTableOf09_ReadsDigitsAndOneCommaOnly_AndNeverEnlargesAValue(string? value, bool parsed, string expected)
    {
        Assert.Equal(parsed, RequestFormat.TryParseDecimal(value, out decimal result));
        Assert.Equal(decimal.Parse(expected, CultureInfo.InvariantCulture), result);
    }

    /// <summary>
    /// A text with more digits than a <see cref="decimal"/> holds would be rounded by the framework's parser - "12,000...0001" would become 12 and pass the
    /// rule of two decimals, "0,000...0001" would become 0. Such a text is not read: the value is never made larger or smaller (07 2.9.4).
    /// </summary>
    [Theory]
    [InlineData("12,00000000000000000000000000001")]
    [InlineData("1,99999999999999999999999999999")]
    [InlineData("0,00000000000000000000000000000001")]
    [InlineData("99999999999999999999999999999")]
    [InlineData("999999999999999999999999999999")]
    [InlineData("7922816251426433759354395033,51")]
    public void TryParseDecimal_ATextBeyondThePrecisionOfADecimal_IsNotRead_NothingIsRounded(string value)
    {
        Assert.False(RequestFormat.TryParseDecimal(value, out decimal result));
        Assert.Equal(0m, result);
    }

    /// <summary>Zeros that change nothing are not a loss: leading zeros and trailing decimal zeros, however many, still read the typed value.</summary>
    [Theory]
    [InlineData("012,50", "12.5")]
    [InlineData("0000000000000000000000000000000012,5", "12.5")]
    [InlineData("12,500000000000000000000000000000000", "12.5")]
    [InlineData("0,000", "0")]
    [InlineData("000", "0")]
    [InlineData("9999999999,99", "9999999999.99")]
    [InlineData("0,0000000000000000000000000001", "0.0000000000000000000000000001")]
    public void TryParseDecimal_ZerosThatChangeNothing_ReadTheTypedValue(string value, string expected)
    {
        Assert.True(RequestFormat.TryParseDecimal(value, out decimal result));
        Assert.Equal(decimal.Parse(expected, CultureInfo.InvariantCulture), result);
    }

    [Theory]
    [InlineData("en-US")]
    [InlineData("de-DE")]
    [InlineData("")]
    public void TryParseDecimal_AndTryParseWholeNumber_AreNotChangedByTheThreadCulture(string culture)
    {
        CultureInfo before = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture);

            Assert.True(RequestFormat.TryParseDecimal("1234,50", out decimal amount));
            Assert.Equal(1234.50m, amount);
            Assert.False(RequestFormat.TryParseDecimal("12.50", out _));
            Assert.False(RequestFormat.TryParseWholeNumber("1,000", out _));
        }
        finally
        {
            CultureInfo.CurrentCulture = before;
        }
    }

    [Theory]
    [InlineData("2026-10-04", true)]
    [InlineData("2028-02-29", true)]
    [InlineData("2026-02-29", false)]
    [InlineData("2026-02-30", false)]
    [InlineData("04.10.2026", false)]
    [InlineData("10/04/2026", false)]
    [InlineData("2026-4-1", false)]
    [InlineData("2026-10-04T10:00", false)]
    [InlineData("2026-10-04T00:00:00+03:00", false)]
    [InlineData("2026-10-04 ", false)]
    [InlineData("20261004", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void TryParseDate_ReadsTheOneFormat_AndNoTime(string? value, bool parsed)
    {
        Assert.Equal(parsed, RequestFormat.TryParseDate(value, out DateOnly day));
        if (parsed)
        {
            Assert.Equal(value, RequestFormat.FormatDate(day));
        }
    }

    [Fact]
    public void TryParseDate_IsTheDayWritten_WhateverTheThreadCulture()
    {
        CultureInfo before = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en-US");

            Assert.True(RequestFormat.TryParseDate("2026-10-04", out DateOnly day));
            Assert.Equal(new DateOnly(2026, 10, 4), day);
        }
        finally
        {
            CultureInfo.CurrentCulture = before;
        }
    }

    [Theory]
    [InlineData("5", true, 5L)]
    [InlineData("1000", true, 1000L)]
    [InlineData("007", true, 7L)]
    [InlineData("9223372036854775807", true, long.MaxValue)]
    [InlineData("9223372036854775808", false, 0L)]
    [InlineData("+5", false, 0L)]
    [InlineData("-5", false, 0L)]
    [InlineData(" 5", false, 0L)]
    [InlineData("5 ", false, 0L)]
    [InlineData("0x10", false, 0L)]
    [InlineData("#10", false, 0L)]
    [InlineData("1.000", false, 0L)]
    [InlineData("1,000", false, 0L)]
    [InlineData("1,5", false, 0L)]
    [InlineData("1e3", false, 0L)]
    [InlineData("١٢", false, 0L)]
    [InlineData("", false, 0L)]
    [InlineData(null, false, 0L)]
    public void TryParseWholeNumber_ReadsAsciiDigitsOnly(string? value, bool parsed, long expected)
    {
        Assert.Equal(parsed, RequestFormat.TryParseWholeNumber(value, out long result));
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("12.5", "12,5")]
    [InlineData("1234.50", "1234,5")]
    [InlineData("1250", "1250")]
    [InlineData("0.01", "0,01")]
    [InlineData("9999999999.99", "9999999999,99")]
    public void FormatDecimal_IsTheInverseOfTheParse_WithoutAGroupSeparator(string value, string expected)
    {
        decimal number = decimal.Parse(value, CultureInfo.InvariantCulture);

        string text = RequestFormat.FormatDecimal(number);

        Assert.Equal(expected, text);
        Assert.True(RequestFormat.TryParseDecimal(text, out decimal back));
        Assert.Equal(number, back);
    }

    // KK5 (07 2.9.5; 04 8.2.6): a telephone number and an IBAN are the same with or without blanks.
    [Theory]
    [InlineData("0532 123 45 67")]
    [InlineData("05321234567")]
    [InlineData("+90 (532) 123-45-67")]
    [InlineData("+905321234567")]
    [InlineData("5321234567")]
    public void CompactPhone_IgnoresBlanksHyphensBracketsAndThePrefix(string value)
    {
        Assert.Equal("5321234567", RequestFormat.CompactPhone(value));
    }

    [Theory]
    [InlineData("TR33 0006 1005 1978 6457 8413 26")]
    [InlineData("TR330006100519786457841326")]
    [InlineData("tr33 0006 1005 1978 6457 8413 26")]
    [InlineData(" TR33  0006 1005 1978 6457 8413 26 ")]
    public void CompactIban_AnIbanWithBlanks_IsEqualToTheOneWithout(string value)
    {
        Assert.Equal("TR330006100519786457841326", RequestFormat.CompactIban(value));
    }
}
