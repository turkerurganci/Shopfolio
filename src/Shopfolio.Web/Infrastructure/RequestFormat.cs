using System.Globalization;

namespace Shopfolio.Infrastructure;

/// <summary>
/// The format of the values a request carries (07 2.9; 09 7.2 items 1-3): the culture is explicit and lives here once, the thread's culture and the browser's
/// <c>Accept-Language</c> never change a parse (07 2.9.9). The list filters read their values with it (T210; 07 2.6.6) and the model binders of the form
/// and query fields go through it (T216; <see cref="Validation.RequestValueBinder"/>). A value a form field shows is written with <see cref="FormatDecimal"/>
/// and <see cref="FormatDate"/>, the inverse of the parse, so what the browser sends back is read as the same value. The display of a value on a page is
/// <see cref="DisplayFormat"/>'s (04 8.2).
/// </summary>
internal static class RequestFormat
{
    /// <summary>The culture of an amount in a request: Turkish, the decimal separator is the comma (07 2.9.4).</summary>
    public static readonly CultureInfo Culture = CultureInfo.GetCultureInfo("tr-TR");

    /// <summary>The one format of a date in a request (07 2.9.3).</summary>
    public const string DateFormat = "yyyy-MM-dd";

    // The characters 04 7.3.2.4 ignores in a telephone number, and the two prefixes it accepts before the ten digits.
    private static readonly char[] _phoneSeparators = [' ', '-', '(', ')'];
    private const string PhoneCountryPrefix = "+90";
    private const char PhoneTrunkPrefix = '0';

    /// <summary>
    /// An amount: digits and at most one comma, nothing else - no thousands separator, sign, blank, exponent or currency (07 2.9.4; 09 7.2 item 3). In
    /// <c>tr-TR</c> the point is the group separator; the style does not accept it, so <c>12.50</c> is not read and never becomes 1250. A text with more
    /// digits than a <see cref="decimal"/> holds is not read either: the framework's parser would round it ("1,999...9" would become 2, "12,000...01" would
    /// become 12 and pass the rule of two decimals), so the parsed value must write back as the typed number, up to the zeros that change nothing.
    /// </summary>
    public static bool TryParseDecimal(string? value, out decimal result)
    {
        result = 0m;
        if (string.IsNullOrEmpty(value) || !decimal.TryParse(value, NumberStyles.AllowDecimalPoint, Culture, out decimal parsed)
            || !string.Equals(WithoutIdleZeros(value), FormatDecimal(parsed), StringComparison.Ordinal))
        {
            return false;
        }

        result = parsed;
        return true;
    }

    /// <summary>
    /// A number the decimal style has read - digits and at most one comma - without the zeros that change nothing: the leading zeros of the whole part, the
    /// trailing zeros of the fraction and a comma with no digit after it. It is the form <see cref="FormatDecimal"/> writes.
    /// </summary>
    private static string WithoutIdleZeros(string number)
    {
        string separator = Culture.NumberFormat.NumberDecimalSeparator;
        int comma = number.IndexOf(separator, StringComparison.Ordinal);
        string whole = (comma < 0 ? number : number[..comma]).TrimStart('0');
        string fraction = comma < 0 ? string.Empty : number[(comma + separator.Length)..].TrimEnd('0');
        if (whole.Length == 0)
        {
            whole = "0";
        }

        return fraction.Length == 0 ? whole : whole + separator + fraction;
    }

    /// <summary>A date in the one format <c>yyyy-AA-GG</c> (07 2.9.3; 09 7.2 item 2); the format is fixed, so the culture is the invariant one.</summary>
    public static bool TryParseDate(string? value, out DateOnly result)
    {
        result = default;
        return !string.IsNullOrEmpty(value)
            && DateOnly.TryParseExact(value, DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out result);
    }

    /// <summary>
    /// A whole number - a quantity, a number of days (09 7.2 item 3): ASCII digits only, read with <see cref="NumberStyles.None"/> and the invariant culture.
    /// No sign, blank, separator, hexadecimal prefix (<c>0x10</c>, <c>#10</c>) or exponent is read, and a number beyond the type's range is no number.
    /// </summary>
    public static bool TryParseWholeNumber(string? value, out long result)
    {
        result = 0;
        return !string.IsNullOrEmpty(value) && long.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out result);
    }

    /// <summary>The text a form field shows for an amount or a decimal number (04 8.2.3): no group separator, the comma as the decimal separator.</summary>
    public static string FormatDecimal(decimal value) => value.ToString("0.############################", Culture);

    /// <summary>The value a date field carries (07 2.9.3): <c>yyyy-AA-GG</c>.</summary>
    public static string FormatDate(DateOnly value) => value.ToString(DateFormat, CultureInfo.InvariantCulture);

    /// <summary>
    /// The stored form of a telephone number (06 2.5.1; 04 7.3.2.4; 07 2.9.5): blanks, hyphens and brackets are ignored, a leading <c>+90</c> or <c>0</c> is
    /// dropped - "0532 123 45 67", "+90 (532) 123-45-67" and "5321234567" are the same number, "5321234567". Whether the result is a number of ten digits is
    /// the rule's question (<see cref="Validation.InvalidPhoneAttribute"/>).
    /// </summary>
    public static string CompactPhone(string value)
    {
        ArgumentNullException.ThrowIfNull(value);
        string compact = string.Concat(value.Split(_phoneSeparators));
        return compact.StartsWith(PhoneCountryPrefix, StringComparison.Ordinal) ? compact[PhoneCountryPrefix.Length..]
            : compact.StartsWith(PhoneTrunkPrefix) ? compact[1..]
            : compact;
    }

    /// <summary>
    /// The stored form of an optional telephone field (06 2.5.1, 2.8.5): nothing for an empty text; the ten digits when the text is a number of the accepted forms
    /// (<see cref="CompactPhone"/>), else the trimmed text, which <see cref="Validation.InvalidPhoneAttribute"/> then refuses. The one place of this decision: the
    /// company identity and the branch both store through it.
    /// </summary>
    public static string? StoredPhone(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        string text = value.Trim();
        string compact = CompactPhone(text);
        return compact.Length == 10 && compact.All(char.IsAsciiDigit) && compact[0] != '0' ? compact : text;
    }

    /// <summary>
    /// The stored form of an IBAN (06 2.5.1; 04 7.3.2.7; 07 2.9.5): the blanks are ignored and the letters are upper case - "TR33 0006 1005 1978 6457 8413 26"
    /// and "tr330006100519786457841326" are the same IBAN. Whether it is a valid IBAN is the rule's question (<see cref="Validation.InvalidIbanAttribute"/>).
    /// </summary>
    public static string CompactIban(string value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return value.Replace(" ", string.Empty, StringComparison.Ordinal).ToUpperInvariant();
    }
}
