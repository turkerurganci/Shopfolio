using Shopfolio.Infrastructure;

namespace Shopfolio.UnitTests.Lists;

/// <summary>
/// The page number and the page count (T210 criteria 2, 3; KK2, KK3; 07 2.6.2; 09 10.5 items 2, 3, 6): the table of 09 10.5 item 6 - empty, non-numeric or
/// smaller-than-one is the first page, a value past the <c>int</c> range is <see cref="int.MaxValue"/>, a number is ASCII digits only.
/// </summary>
public sealed class PageNumberTests
{
    [Theory]
    [InlineData(null, 1)]
    [InlineData("", 1)]
    [InlineData("1", 1)]
    [InlineData("2", 2)]
    [InlineData("abc", 1)]
    [InlineData("0", 1)]
    [InlineData("00", 1)]
    [InlineData("-3", 1)]
    [InlineData("+5", 1)]
    [InlineData("5.0", 1)]
    [InlineData("5,0", 1)]
    [InlineData("٣", 1)]
    [InlineData(" 5", 1)]
    [InlineData("5 ", 1)]
    [InlineData("1e3", 1)]
    [InlineData("007", 7)]
    [InlineData("2147483647", int.MaxValue)]
    [InlineData("2147483648", int.MaxValue)]
    [InlineData("99999999999", int.MaxValue)]
    [InlineData("999999999999999999999999999999", int.MaxValue)]
    public void Parse_ReadsDigitsOnly_AndNeverFails(string? value, int expected)
    {
        Assert.Equal(expected, PageNumber.Parse(value));
    }

    [Theory]
    [InlineData(0, 25, 1)]
    [InlineData(1, 25, 1)]
    [InlineData(25, 25, 1)]
    [InlineData(26, 25, 2)]
    [InlineData(50, 25, 2)]
    [InlineData(51, 25, 3)]
    [InlineData(int.MaxValue, 1, int.MaxValue)]
    [InlineData(int.MaxValue, 24, 89478486)]
    public void PageCountOf_IsAtLeastOne_AndRoundsUp(int total, int pageSize, int expected)
    {
        Assert.Equal(expected, Paging.PageCountOf(total, pageSize));
    }

    [Fact]
    public void PageCountOf_OnAPageSizeUnderOne_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Paging.PageCountOf(10, 0));
    }
}
