using System.Globalization;

namespace Shopfolio.Infrastructure.Validation;

/// <summary>The field is required (04 7.3.1): not null, not a text of white space only, not an empty list, not an empty key.</summary>
public sealed class FieldRequiredAttribute : FieldRuleAttribute
{
    public override string Code => FieldCodes.FieldRequired;

    protected override bool Accepts(object? value) => value switch
    {
        null => false,
        string text => !string.IsNullOrWhiteSpace(text),
        Guid key => key != Guid.Empty,
        System.Collections.IEnumerable list => list.GetEnumerator().MoveNext(),
        _ => true,
    };
}

/// <summary>An e-mail address (04 7.3.2.1): one address - a local part, one "@" and a domain with a dot; no white space.</summary>
public sealed class InvalidEmailAttribute : FieldRuleAttribute
{
    private const int MaxLength = 254;
    private const int MaxLocalLength = 64;

    public override string Code => FieldCodes.InvalidEmail;

    protected override bool Accepts(object? value)
    {
        if (IsEmpty(value))
        {
            return true;
        }

        if (value is not string address || address.Length > MaxLength || address.Any(c => char.IsWhiteSpace(c) || char.IsControl(c)))
        {
            return false;
        }

        int at = address.IndexOf('@', StringComparison.Ordinal);
        if (at < 1 || at != address.LastIndexOf('@') || at > MaxLocalLength)
        {
            return false;
        }

        string[] labels = address[(at + 1)..].Split('.');
        return labels.Length >= 2
            && labels.All(label => label.Length > 0 && label[0] != '-' && label[^1] != '-' && label.All(c => char.IsLetterOrDigit(c) || c == '-'));
    }
}

/// <summary>A Turkish telephone number (04 7.3.2.4): ten digits with the area code; a leading "0" or "+90" is accepted, spaces, hyphens and brackets are ignored.</summary>
public sealed class InvalidPhoneAttribute : FieldRuleAttribute
{
    private const int DigitCount = 10;

    public override string Code => FieldCodes.InvalidPhone;

    protected override bool Accepts(object? value)
    {
        if (IsEmpty(value))
        {
            return true;
        }

        if (value is not string text)
        {
            return false;
        }

        // The same compaction as the stored form (07 2.9.5): "0532 123 45 67" and "5321234567" are one number.
        string digits = RequestFormat.CompactPhone(text);
        return digits.Length == DigitCount && digits[0] != '0' && digits.All(char.IsAsciiDigit);
    }
}

/// <summary>A Turkish IBAN (04 7.3.2.7): "TR" and 24 digits, the check digits of ISO 13616 (modulo 97); spaces are ignored.</summary>
public sealed class InvalidIbanAttribute : FieldRuleAttribute
{
    private const int Length = 26;
    private const int Modulus = 97;

    public override string Code => FieldCodes.InvalidIban;

    protected override bool Accepts(object? value)
    {
        if (IsEmpty(value))
        {
            return true;
        }

        if (value is not string text)
        {
            return false;
        }

        // The same compaction as the stored form (07 2.9.5): the blanks are ignored and the letters are upper case.
        string iban = RequestFormat.CompactIban(text);
        if (iban.Length != Length || !iban.StartsWith("TR", StringComparison.Ordinal) || !iban[2..].All(char.IsAsciiDigit))
        {
            return false;
        }

        // The first four characters move to the end, letters become two digits (A = 10), and the number is checked modulo 97.
        int remainder = 0;
        foreach (char c in string.Concat(iban.AsSpan(4), iban.AsSpan(0, 4)))
        {
            int number = char.IsAsciiDigit(c) ? c - '0' : char.ToUpperInvariant(c) - 'A' + 10;
            remainder = (number > 9 ? remainder * 100 + number : remainder * 10 + number) % Modulus;
        }

        return remainder == 1;
    }
}

/// <summary>
/// An amount (04 7.3.2.8): not negative, at most two decimals, not above <see cref="Limit"/> - the limit of the money type (06 2.5.1; K-1214), or a
/// lower <see cref="Max"/> - and above zero unless <see cref="AllowZero"/>. A text that is no amount (07 2.9.4) is this rule's failure too. The message
/// names the limit in the display format of an amount (04 7.3.1, 8.2.4): "en çok 9.999.999.999,99 TL".
/// </summary>
public sealed class InvalidAmountAttribute : FieldRuleAttribute
{
    /// <summary>A lower limit than the money type's; a larger value is the money type's limit (an amount never exceeds what the column holds).</summary>
    public double Max { get; init; } = double.MaxValue;

    /// <summary>Settings amounts may be zero; a price may not (04 7.3.2.8).</summary>
    public bool AllowZero { get; init; }

    /// <summary>The largest amount the field takes: <see cref="Max"/>, at most <see cref="Money.MaxAmount"/>.</summary>
    public decimal Limit => Max >= (double)Money.MaxAmount ? Money.MaxAmount : (decimal)Max;

    public override string Code => FieldCodes.InvalidAmount;

    public override string? ParameterText => DisplayFormat.Amount(Limit);

    public override bool OwnsConversionFailure => true;

    protected override bool Accepts(object? value)
    {
        if (value is null)
        {
            return true;
        }

        decimal amount = Convert.ToDecimal(value, CultureInfo.InvariantCulture);
        return (amount > 0 || (AllowZero && amount == 0)) && HasAtMostTwoDecimals(amount) && amount <= Limit;
    }
}

/// <summary>
/// A percentage (04 7.3.2.9): at most two decimals, from zero (or above it unless <see cref="AllowZero"/>) up to but not including one hundred. A text
/// that is no number (07 2.9.4) is this rule's failure too. The message names the largest value in the display format of a percentage: "en çok %99,99".
/// </summary>
public sealed class InvalidPercentageAttribute : FieldRuleAttribute
{
    private const decimal Hundred = 100m;

    /// <summary>A VAT rate may be zero; a discount may not (04 7.3.2.9).</summary>
    public bool AllowZero { get; init; }

    public override string Code => FieldCodes.InvalidPercentage;

    public override string? ParameterText => DisplayFormat.Percent(Money.MaxPercentage);

    public override bool OwnsConversionFailure => true;

    protected override bool Accepts(object? value)
    {
        if (value is null)
        {
            return true;
        }

        decimal rate = Convert.ToDecimal(value, CultureInfo.InvariantCulture);
        return (rate > 0 || (AllowZero && rate == 0)) && rate < Hundred && HasAtMostTwoDecimals(rate);
    }
}

/// <summary>A quantity (04 7.3.2.10): a whole number from one up to <paramref name="max"/>, the limit the message names.</summary>
public sealed class InvalidQuantityAttribute : FieldRuleAttribute
{
    private readonly long _max;

    public InvalidQuantityAttribute(long max)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(max, 1);
        _max = max;
    }

    public override string Code => FieldCodes.InvalidQuantity;

    public override string? ParameterText => DisplayFormat.Number(_max);

    public override bool OwnsConversionFailure => true;

    protected override bool Accepts(object? value)
    {
        if (value is null)
        {
            return true;
        }

        if (value is decimal or double or float)
        {
            decimal fractional = Convert.ToDecimal(value, CultureInfo.InvariantCulture);
            return fractional % 1m == 0m && fractional >= 1 && fractional <= _max;
        }

        long quantity = Convert.ToInt64(value, CultureInfo.InvariantCulture);
        return quantity >= 1 && quantity <= _max;
    }
}

/// <summary>
/// A date field (04 7.3.2.12; 07 2.9.3): a value that is not a day in the one format <c>yyyy-AA-GG</c> is this rule's failure - the date binder reports
/// it with this code (09 7.2 item 2). A date field without the attribute gets the same code. The limits a single field adds - a future date in a report
/// filter, a lower limit - belong to the task that builds the field and are added here as properties; the order of two dates is the model's
/// <c>InvalidDateRange</c> (09 3.4 item 2).
/// </summary>
public sealed class InvalidDateAttribute : FieldRuleAttribute
{
    public override string Code => FieldCodes.InvalidDate;

    public override bool OwnsConversionFailure => true;

    protected override bool Accepts(object? value) => value is null or DateOnly;
}

/// <summary>A link (04 7.3.2.13): a full address that starts with "http://" or "https://".</summary>
public sealed class InvalidUrlAttribute : FieldRuleAttribute
{
    public override string Code => FieldCodes.InvalidUrl;

    protected override bool Accepts(object? value)
    {
        if (IsEmpty(value))
        {
            return true;
        }

        return value is string text
            && !text.Any(char.IsWhiteSpace)
            && Uri.TryCreate(text, UriKind.Absolute, out Uri? uri)
            && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps)
            && uri.Host.Length > 0
            && (text.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || text.StartsWith("https://", StringComparison.OrdinalIgnoreCase));
    }
}

/// <summary>A text with a character limit (04 7.3.2.14); <paramref name="max"/> is a product constant (P-27 ... P-30), never a number written on the attribute.</summary>
public sealed class TextTooLongAttribute : FieldRuleAttribute
{
    private readonly int _max;

    public TextTooLongAttribute(int max)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(max, 1);
        _max = max;
    }

    public override string Code => FieldCodes.TextTooLong;

    public override string? ParameterText => DisplayFormat.Number(_max);

    protected override bool Accepts(object? value) =>
        value is not string text || text.EnumerateRunes().Count() <= _max;
}

/// <summary>A value of a closed list (04 7.3.2.16): the names of a 06 enum, or the listed texts; an empty value passes (the required rule decides).</summary>
public sealed class InvalidListValueAttribute : FieldRuleAttribute
{
    private readonly string[] _allowed;

    public InvalidListValueAttribute(params string[] allowed)
    {
        ArgumentNullException.ThrowIfNull(allowed);
        _allowed = allowed;
    }

    public InvalidListValueAttribute(Type enumType)
        : this(Enum.GetNames(enumType ?? throw new ArgumentNullException(nameof(enumType))))
    {
    }

    public override string Code => FieldCodes.InvalidListValue;

    protected override bool Accepts(object? value) =>
        IsEmpty(value) || (value?.ToString() is { } text && _allowed.Contains(text, StringComparer.Ordinal));
}

/// <summary>A colour (06 4.54): "#" and six hexadecimal digits.</summary>
public sealed class InvalidColorAttribute : FieldRuleAttribute
{
    private const int Length = 7;

    public override string Code => FieldCodes.InvalidColor;

    protected override bool Accepts(object? value) =>
        IsEmpty(value) || (value is string text && text.Length == Length && text[0] == '#' && text.Skip(1).All(char.IsAsciiHexDigit));
}

/// <summary>A confirmation box that must be ticked (04 7.1.5.9).</summary>
public sealed class CheckboxNotAcceptedAttribute : FieldRuleAttribute
{
    public override string Code => FieldCodes.CheckboxNotAccepted;

    protected override bool Accepts(object? value) => value is true;
}
