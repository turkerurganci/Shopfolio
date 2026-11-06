using System.ComponentModel.DataAnnotations;

using Shopfolio.Infrastructure.Validation;

namespace Shopfolio.Infrastructure.Lists;

/// <summary>The price filter of a list as read from <c>minPrice</c> and <c>maxPrice</c> (07 2.6.3): a bound that is not an amount is absent; a lower bound above the upper one makes the whole filter void.</summary>
internal readonly record struct PriceFilter(decimal? Min, decimal? Max, bool Inverted)
{
    /// <summary>The lower bound the query applies: none when the range is inverted (07 2.6.3: the price filter is not applied).</summary>
    public decimal? AppliedMin => Inverted ? null : Min;

    /// <summary>The upper bound the query applies: none when the range is inverted.</summary>
    public decimal? AppliedMax => Inverted ? null : Max;
}

/// <summary>The date range of a list as read from two dates (07 2.6.6; 04 7.3.2.12): a date that is not <c>yyyy-AA-GG</c> is absent; an end before the start voids the range.</summary>
internal readonly record struct DateFilter(DateOnly? Start, DateOnly? End, bool Inverted)
{
    public DateOnly? AppliedStart => Inverted ? null : Start;

    public DateOnly? AppliedEnd => Inverted ? null : End;
}

/// <summary>
/// The reading of the filter values of a list (07 2.6.6): an invalid value - outside the closed list, a number or a date that is not in the format of 07 2.9 -
/// applies no filter and is no error, and the rules of two values together are field messages. The pair rules are the model's <c>IValidatableObject</c>
/// path (09 3.4 item 2): <see cref="PriceRangeResults"/> (<c>InvalidPriceRange</c>, 04 7.1.1.1) and <see cref="DateRangeResults"/> (<c>InvalidDateRange</c>,
/// 04 7.3.2.12) give the results with the code; the query reads the same value through <see cref="ReadPrice"/> and <see cref="ReadDates"/>, so the message and
/// the missing filter come from one reading. A future date, and the code of a malformed date (<c>InvalidDate</c>), are the date field's rule (T216).
/// </summary>
internal static class ListFilters
{
    /// <summary>The value of <c>inStock</c> that applies the filter (07 2.6.3); any other value does not.</summary>
    public const string InStockValue = "true";

    public static PriceFilter ReadPrice(string? min, string? max)
    {
        decimal? lower = RequestFormat.TryParseDecimal(min, out decimal low) ? low : null;
        decimal? upper = RequestFormat.TryParseDecimal(max, out decimal high) ? high : null;
        return new PriceFilter(lower, upper, lower is { } l && upper is { } u && l > u);
    }

    public static DateFilter ReadDates(string? start, string? end)
    {
        DateOnly? from = RequestFormat.TryParseDate(start, out DateOnly first) ? first : null;
        DateOnly? to = RequestFormat.TryParseDate(end, out DateOnly last) ? last : null;
        return new DateFilter(from, to, from is { } f && to is { } t && t < f);
    }

    /// <summary>Whether the stock filter applies: the one value <c>true</c>, exactly (07 2.6.3).</summary>
    public static bool ReadInStock(string? value) => string.Equals(value, InStockValue, StringComparison.Ordinal);

    /// <summary>
    /// A value of a closed list: the code name of 06 5 (PascalCase), exactly. A number, another spelling or a name outside the enum is no value
    /// (<c>Enum.TryParse</c> would accept <c>"1"</c>, a number, and ignore case).
    /// </summary>
    public static TEnum? ReadCode<TEnum>(string? value)
        where TEnum : struct, Enum
    {
        if (value is null || !Array.Exists(Enum.GetNames<TEnum>(), name => string.Equals(name, value, StringComparison.Ordinal)))
        {
            return null;
        }

        return Enum.Parse<TEnum>(value);
    }

    /// <summary>The values of a parameter that repeats (07 2.6.4): the valid ones, once each, in the order they came; the invalid ones are dropped.</summary>
    public static IReadOnlyList<TEnum> ReadCodes<TEnum>(IEnumerable<string?>? values)
        where TEnum : struct, Enum
    {
        if (values is null)
        {
            return [];
        }

        List<TEnum> codes = [];
        foreach (string? value in values)
        {
            if (ReadCode<TEnum>(value) is { } code && !codes.Contains(code))
            {
                codes.Add(code);
            }
        }

        return codes;
    }

    /// <summary>The <c>InvalidPriceRange</c> result for a model's <c>Validate</c>: it belongs to the lower bound's field, which "cannot be above the upper value" (04 7.1.1.1).</summary>
    public static IEnumerable<ValidationResult> PriceRangeResults(string? min, string? max, string lowerMember)
    {
        if (ReadPrice(min, max).Inverted)
        {
            yield return FieldError.Result(FieldCodes.InvalidPriceRange, lowerMember);
        }
    }

    /// <summary>The <c>InvalidDateRange</c> result for a model's <c>Validate</c>: it belongs to the end date's field, which "cannot be before the start" (04 7.3.2.12).</summary>
    public static IEnumerable<ValidationResult> DateRangeResults(string? start, string? end, string endMember)
    {
        if (ReadDates(start, end).Inverted)
        {
            yield return FieldError.Result(FieldCodes.InvalidDateRange, endMember);
        }
    }
}
