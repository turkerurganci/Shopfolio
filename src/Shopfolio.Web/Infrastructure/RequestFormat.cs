using System.Globalization;

namespace Shopfolio.Infrastructure;

/// <summary>
/// The format of the values a request carries (07 2.9; 09 7.2 items 1-3): the culture is explicit and lives here once, the thread's culture and the browser's
/// <c>Accept-Language</c> never change a parse (07 2.9.9). T210 reads the filter values of a list with it (07 2.6.6); T216 owns the rest of the format - the model
/// binders of the form fields, the display of values and the table of 09 7.2 item 6 - and extends this class.
/// </summary>
internal static class RequestFormat
{
    /// <summary>The culture of an amount in a request: Turkish, the decimal separator is the comma (07 2.9.4).</summary>
    public static readonly CultureInfo Culture = CultureInfo.GetCultureInfo("tr-TR");

    /// <summary>The one format of a date in a request (07 2.9.3).</summary>
    public const string DateFormat = "yyyy-MM-dd";

    /// <summary>
    /// An amount: digits and at most one comma, nothing else - no thousands separator, sign, blank, exponent or currency (07 2.9.4; 09 7.2 item 3). In
    /// <c>tr-TR</c> the point is the group separator; the style does not accept it, so <c>12.50</c> is not read and never becomes 1250.
    /// </summary>
    public static bool TryParseDecimal(string? value, out decimal result)
    {
        result = 0m;
        return !string.IsNullOrEmpty(value) && decimal.TryParse(value, NumberStyles.AllowDecimalPoint, Culture, out result);
    }

    /// <summary>A date in the one format <c>yyyy-AA-GG</c> (07 2.9.3; 09 7.2 item 2); the format is fixed, so the culture is the invariant one.</summary>
    public static bool TryParseDate(string? value, out DateOnly result)
    {
        result = default;
        return !string.IsNullOrEmpty(value)
            && DateOnly.TryParseExact(value, DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out result);
    }
}
