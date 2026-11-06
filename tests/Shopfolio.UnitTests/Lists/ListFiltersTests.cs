using System.ComponentModel.DataAnnotations;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Lists;
using Shopfolio.Infrastructure.Validation;
using Shopfolio.Modules.Catalog.Contracts;

namespace Shopfolio.UnitTests.Lists;

/// <summary>
/// The reading of the filter values of a list (T210 criteria 5, 6; KK5, KK6; 07 2.6.3, 2.6.6; 04 7.1.1.1, 7.3.2.12): an invalid value applies no filter and
/// is no error; the two pair rules - the price range and the date range - are field messages with their codes; and the format the values are read in
/// (07 2.9.3, 2.9.4; 09 7.2).
/// </summary>
public sealed class ListFiltersTests
{
    [Theory]
    [InlineData("12,5", true, 12.5)]
    [InlineData("12", true, 12)]
    [InlineData("0", true, 0)]
    [InlineData("1234,50", true, 1234.5)]
    [InlineData("12.50", false, 0)]
    [InlineData("1.250,50", false, 0)]
    [InlineData("12,5,1", false, 0)]
    [InlineData("-5", false, 0)]
    [InlineData("12 TL", false, 0)]
    [InlineData("1e3", false, 0)]
    [InlineData(" 12", false, 0)]
    [InlineData("", false, 0)]
    [InlineData(null, false, 0)]
    [InlineData("abc", false, 0)]
    public void RequestFormat_TryParseDecimal_ReadsDigitsAndOneCommaOnly(string? value, bool parsed, double expected)
    {
        Assert.Equal(parsed, RequestFormat.TryParseDecimal(value, out decimal result));
        Assert.Equal((decimal)expected, result);
    }

    [Fact]
    public void RequestFormat_TryParseDecimal_IsNotChangedByTheThreadCulture()
    {
        CultureInfoScope.Run("en-US", () =>
        {
            Assert.True(RequestFormat.TryParseDecimal("12,5", out decimal comma));
            Assert.Equal(12.5m, comma);
            Assert.False(RequestFormat.TryParseDecimal("12.50", out _));
        });
    }

    [Theory]
    [InlineData("2026-10-04", true)]
    [InlineData("2026-02-28", true)]
    [InlineData("2026-02-30", false)]
    [InlineData("04.10.2026", false)]
    [InlineData("2026-4-1", false)]
    [InlineData("2026-10-04T10:00", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void RequestFormat_TryParseDate_ReadsTheOneFormat(string? value, bool parsed)
    {
        Assert.Equal(parsed, RequestFormat.TryParseDate(value, out _));
    }

    [Theory]
    [InlineData("10", "20", "10", "20", false)]
    [InlineData("10", "10", "10", "10", false)]
    [InlineData("10", null, "10", null, false)]
    [InlineData(null, "20", null, "20", false)]
    [InlineData("20", "10", "20", "10", true)]
    [InlineData("12,5", "12,4", "12.5", "12.4", true)]
    [InlineData("abc", "10", null, "10", false)]
    [InlineData("10", "abc", "10", null, false)]
    [InlineData("20", "10.5", "20", null, false)]
    [InlineData("", "", null, null, false)]
    public void ReadPrice_ReadsEachBound_AndMarksAnInvertedRange(string? min, string? max, string? lower, string? upper, bool inverted)
    {
        PriceFilter filter = ListFilters.ReadPrice(min, max);

        Assert.Equal(lower is null ? null : decimal.Parse(lower, System.Globalization.CultureInfo.InvariantCulture), filter.Min);
        Assert.Equal(upper is null ? null : decimal.Parse(upper, System.Globalization.CultureInfo.InvariantCulture), filter.Max);
        Assert.Equal(inverted, filter.Inverted);
    }

    [Fact]
    public void ReadPrice_OnAnInvertedRange_AppliesNoBound()
    {
        PriceFilter filter = ListFilters.ReadPrice("20", "10");

        Assert.Null(filter.AppliedMin);
        Assert.Null(filter.AppliedMax);
    }

    [Fact]
    public void ReadPrice_OnAValidRange_AppliesBothBounds()
    {
        PriceFilter filter = ListFilters.ReadPrice("10", "20");

        Assert.Equal(10m, filter.AppliedMin);
        Assert.Equal(20m, filter.AppliedMax);
    }

    [Fact]
    public void PriceRangeResults_OnAnInvertedRange_IsTheCodeOfTheLowerField_AndOtherwiseEmpty()
    {
        ValidationResult result = Assert.Single(ListFilters.PriceRangeResults("20", "10", "MinPrice"));

        Assert.Equal(FieldCodes.InvalidPriceRange, result.ErrorMessage);
        Assert.Equal(["MinPrice"], result.MemberNames);
        Assert.Empty(ListFilters.PriceRangeResults("10", "20", "MinPrice"));
        Assert.Empty(ListFilters.PriceRangeResults("abc", "10", "MinPrice"));
    }

    [Theory]
    [InlineData("2026-10-01", "2026-10-31", false)]
    [InlineData("2026-10-01", "2026-10-01", false)]
    [InlineData("2026-10-31", "2026-10-01", true)]
    [InlineData("2026-10-31", "not a date", false)]
    [InlineData(null, "2026-10-01", false)]
    public void ReadDates_MarksAnEndBeforeTheStart(string? start, string? end, bool inverted)
    {
        Assert.Equal(inverted, ListFilters.ReadDates(start, end).Inverted);
    }

    [Fact]
    public void ReadDates_OnAnInvertedRange_AppliesNeitherDate()
    {
        DateFilter filter = ListFilters.ReadDates("2026-10-31", "2026-10-01");

        Assert.Null(filter.AppliedStart);
        Assert.Null(filter.AppliedEnd);
        Assert.Equal(new DateOnly(2026, 10, 31), filter.Start);
    }

    [Fact]
    public void DateRangeResults_OnAnInvertedRange_IsTheCodeOfTheEndField()
    {
        ValidationResult result = Assert.Single(ListFilters.DateRangeResults("2026-10-31", "2026-10-01", "EndDate"));

        Assert.Equal(FieldCodes.InvalidDateRange, result.ErrorMessage);
        Assert.Equal(["EndDate"], result.MemberNames);
        Assert.Empty(ListFilters.DateRangeResults("2026-10-01", "2026-10-31", "EndDate"));
    }

    [Theory]
    [InlineData("true", true)]
    [InlineData("True", false)]
    [InlineData("TRUE", false)]
    [InlineData("1", false)]
    [InlineData("false", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void ReadInStock_AppliesOnTheOneValueTrue(string? value, bool applied)
    {
        Assert.Equal(applied, ListFilters.ReadInStock(value));
    }

    [Theory]
    [InlineData("Published", PublishStatus.Published)]
    [InlineData("Archived", PublishStatus.Archived)]
    [InlineData("Draft", PublishStatus.Draft)]
    public void ReadCode_OnACodeNameOfTheEnum_IsTheValue(string value, PublishStatus expected)
    {
        Assert.Equal(expected, ListFilters.ReadCode<PublishStatus>(value));
    }

    [Theory]
    [InlineData("published")]
    [InlineData("PUBLISHED")]
    [InlineData("1")]
    [InlineData("0")]
    [InlineData("-1")]
    [InlineData("99")]
    [InlineData("Draft,Published")]
    [InlineData(" Draft")]
    [InlineData("Deleted")]
    [InlineData("")]
    [InlineData(null)]
    public void ReadCode_OnAnythingElse_IsNoValue(string? value)
    {
        Assert.Null(ListFilters.ReadCode<PublishStatus>(value));
    }

    [Fact]
    public void ReadCodes_KeepsTheValidValuesOnce_InTheOrderTheyCame()
    {
        IReadOnlyList<PublishStatus> codes = ListFilters.ReadCodes<PublishStatus>(["Published", "bogus", "Draft", "Published", null, "2"]);

        Assert.Equal([PublishStatus.Published, PublishStatus.Draft], codes);
        Assert.Empty(ListFilters.ReadCodes<PublishStatus>(null));
    }

    private static class CultureInfoScope
    {
        public static void Run(string culture, Action action)
        {
            System.Globalization.CultureInfo before = System.Globalization.CultureInfo.CurrentCulture;
            try
            {
                System.Globalization.CultureInfo.CurrentCulture = System.Globalization.CultureInfo.GetCultureInfo(culture);
                action();
            }
            finally
            {
                System.Globalization.CultureInfo.CurrentCulture = before;
            }
        }
    }
}
