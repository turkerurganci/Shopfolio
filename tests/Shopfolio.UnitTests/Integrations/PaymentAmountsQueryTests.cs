using System.Globalization;

using Shopfolio.Infrastructure.Integrations;

namespace Shopfolio.UnitTests.Integrations;

/// <summary>
/// The two functions the final query brings to the one home of the provider's amount conversions (T403 KK5, KK6; 09 14.2 items 1, 3 and 5; 08 2.1.6, DV-12):
/// <see cref="PaymentAmounts.TryParseProviderText"/> - one separator of either kind is the decimal separator with at most two places, digits only is whole lira,
/// anything else is not read - under two server cultures, and the four cases of <see cref="PaymentAmounts.QueryMatches"/>.
/// </summary>
public sealed class PaymentAmountsQueryTests
{
    private static readonly string[] CultureTexts = ["10,8", "9.76", "10", "10,123", "1.234,56"];

    // KK6: the criterion's table (10, 10,8, 9.76, 10,123, 1.234,56) and 09 14.2 item 5's list.
    [Theory]
    [InlineData("10", "10")]
    [InlineData("10,8", "10.80")]
    [InlineData("9.76", "9.76")]
    [InlineData("0,05", "0.05")]
    [InlineData("120", "120")]
    [InlineData("124,5", "124.50")]
    public void TryParseProviderText_OneSeparatorOrNone_IsReadWithoutRounding_KK6(string text, string expected)
    {
        Assert.Equal(decimal.Parse(expected, CultureInfo.InvariantCulture), PaymentAmounts.TryParseProviderText(text));
    }

    [Theory]
    [InlineData("10,123")]
    [InlineData("10.123")]
    [InlineData("1.234,56")]
    [InlineData("1,234.56")]
    [InlineData("10,,5")]
    [InlineData("10,")]
    [InlineData(",5")]
    [InlineData("")]
    [InlineData("abc")]
    [InlineData("-10")]
    [InlineData("+10")]
    [InlineData(" 10")]
    [InlineData("10 TL")]
    [InlineData("1e3")]
    [InlineData("١٠")]
    [InlineData("99999999999999999999,99")]
    public void TryParseProviderText_ATextOutsideTheRule_IsNotRead_KK6(string text)
    {
        Assert.Null(PaymentAmounts.TryParseProviderText(text));
    }

    [Fact]
    public void TryParseProviderText_Null_IsNotRead()
    {
        Assert.Null(PaymentAmounts.TryParseProviderText(null));
    }

    [Theory]
    [InlineData("tr-TR")]
    [InlineData("en-US")]
    public void TryParseProviderText_IsTheSameUnderEitherServerCulture_KK6(string culture)
    {
        CultureInfo before = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = new CultureInfo(culture);
            Assert.Equal([10.80m, 9.76m, 10m, null, null], CultureTexts.Select(PaymentAmounts.TryParseProviderText).ToArray());
        }
        finally
        {
            CultureInfo.CurrentCulture = before;
        }
    }

    // KK5: payment_amount equal, payment_total equal or more (the instalment difference); either short does not hold.
    [Theory]
    [InlineData("120", "120", true)]
    [InlineData("120", "124.50", true)]
    [InlineData("120", "119.99", false)]
    [InlineData("119.99", "120", false)]
    [InlineData("120.01", "124.50", false)]
    public void QueryMatches_EqualAmount_InstalmentDifference_ShortTotal_OtherAmount_KK5(string paymentAmount, string paymentTotal, bool expected)
    {
        Assert.Equal(
            expected,
            PaymentAmounts.QueryMatches(decimal.Parse(paymentAmount, CultureInfo.InvariantCulture), decimal.Parse(paymentTotal, CultureInfo.InvariantCulture), 120m));
    }
}
