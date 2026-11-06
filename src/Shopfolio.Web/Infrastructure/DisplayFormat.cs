using System.Globalization;

namespace Shopfolio.Infrastructure;

/// <summary>
/// The one display format of the values a page shows (04 8.2; 07 2.9.1; K-824): the date, the time, a number, a percentage, an amount, a period, a
/// telephone number and an IBAN look the same on the storefront and on the panel. The site has one language (04 8.1.1; 10 KD-20): the format is
/// Turkish and fixed - the culture is <see cref="RequestFormat.Culture"/>, written out on every call, so neither the thread's culture nor the browser's
/// <c>Accept-Language</c> changes a text (07 2.9.9). An instant is shown in Turkey time and no time zone is written (02 4.1.1; <see cref="TurkeyTime"/>).
/// A page writes a value through these functions, never through the framework's implicit formatting (which follows the thread's culture). The value a
/// form field carries is <see cref="RequestFormat"/>'s (07 2.9.3, 2.9.4); the format of an e-mail is the Integration Specification's.
/// </summary>
public static class DisplayFormat
{
    /// <summary>The currency written after an amount (04 8.2.4): "TL"; neither "₺" nor "TRY" is used.</summary>
    public const string Currency = "TL";

    private const string DateFormat = "d MMMM yyyy";
    private const string TimeFormat = "HH:mm";
    private const string WholeFormat = "#,0";
    private const string KurusFormat = "#,0.00";
    private const string DecimalFormat = "#,0.############################";
    private const string RangeSeparator = " – ";
    private const string DateTimeSeparator = ", ";
    private const string PercentSign = "%";
    private const string DaysUnit = "gün";
    private const string BusinessDaysUnit = "iş günü";
    private const int IbanGroup = 4;
    private const int PhoneDigits = 10;
    private const decimal KurusPerLira = 100m;

    private static CultureInfo Culture => RequestFormat.Culture;

    /// <summary>A date (04 8.2.1): the day without a leading zero, the Turkish name of the month and four digits of the year - "4 Ekim 2026"; no day name.</summary>
    public static string Date(DateOnly date) => date.ToString(DateFormat, Culture);

    /// <summary>The Turkey day of an instant (04 8.2.1; 02 4.1.1): 4 October 21:00 UTC is "5 Ekim 2026".</summary>
    public static string Date(DateTimeOffset instant) => Date(TurkeyTime.DateOf(instant));

    /// <summary>The Turkey time of an instant (04 8.2.2): twenty-four hours, no seconds - "14:30"; no time zone is written.</summary>
    public static string Time(DateTimeOffset instant) => instant.ToOffset(TurkeyTime.Offset).ToString(TimeFormat, Culture);

    /// <summary>The date and the time of an instant (04 8.2.2): "4 Ekim 2026, 14:30".</summary>
    public static string DateAndTime(DateTimeOffset instant) => Date(instant) + DateTimeSeparator + Time(instant);

    /// <summary>A range of dates (04 8.2.1): "1 Ekim 2026 – 31 Ekim 2026".</summary>
    public static string DateRange(DateOnly start, DateOnly end) => Date(start) + RangeSeparator + Date(end);

    /// <summary>A whole number (04 8.2.3): the point as the thousands separator - "1.250".</summary>
    public static string Number(long value) => value.ToString(WholeFormat, Culture);

    /// <summary>A decimal number (04 8.2.3): the point as the thousands separator, the comma as the decimal separator, no trailing zero - "0,5", "1.250".</summary>
    public static string Number(decimal value) => value.ToString(DecimalFormat, Culture);

    /// <summary>A percentage (04 8.2.3): the sign before the number, no space - "%20", "%12,5" (06 2.5.1: "20" is 20 percent).</summary>
    public static string Percent(decimal rate) => PercentSign + Number(rate);

    /// <summary>
    /// An amount (04 8.2.4): the number, one space and "TL"; an amount with kuruş has two decimals ("266,67 TL"), an amount without has none ("1.200 TL").
    /// A deducted amount (a discount, a coupon's share) is written with the minus sign: "-50 TL". An amount is kuruş precision (06 2.5.1); a value with
    /// more decimals is a programming error and throws - the display never rounds a value silently.
    /// </summary>
    public static string Amount(decimal amount)
    {
        // No rounding here (09 14.1: rounding lives in Money only): a value is checked, never changed.
        if (amount * KurusPerLira % 1 != 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), amount, "An amount has at most two decimals.");
        }

        return amount.ToString(amount % 1 == 0 ? WholeFormat : KurusFormat, Culture) + " " + Currency;
    }

    /// <summary>A period in days (04 8.2.5): "3 gün".</summary>
    public static string Days(int days) => Number(days) + " " + DaysUnit;

    /// <summary>A period in business days (04 8.2.5): "2 iş günü".</summary>
    public static string BusinessDays(int days) => Number(days) + " " + BusinessDaysUnit;

    /// <summary>
    /// A telephone number (04 8.2.6): grouped with the area code - "0532 123 45 67". The value is the stored form (06 2.5.1, ten digits); a value
    /// that is not ten digits after <see cref="RequestFormat.CompactPhone"/> is shown as it is.
    /// </summary>
    public static string Phone(string value)
    {
        ArgumentNullException.ThrowIfNull(value);
        string digits = RequestFormat.CompactPhone(value);
        if (digits.Length != PhoneDigits || !digits.All(char.IsAsciiDigit))
        {
            return value;
        }

        return $"0{digits[..3]} {digits[3..6]} {digits[6..8]} {digits[8..]}";
    }

    /// <summary>
    /// An IBAN (04 8.2.6): groups of four - "TR33 0006 1005 1978 6457 8413 26". The copy button copies <see cref="RequestFormat.CompactIban"/>, the value
    /// without blanks; that the groups do not break on a narrow screen is the page's style (04 8.3.5).
    /// </summary>
    public static string Iban(string value)
    {
        ArgumentNullException.ThrowIfNull(value);
        string compact = RequestFormat.CompactIban(value);
        return string.Join(' ', compact.Chunk(IbanGroup).Select(group => new string(group)));
    }
}
