using Shopfolio.Infrastructure.Integrations;

namespace Shopfolio.UnitTests.Integrations;

/// <summary>
/// The two functions the notification brings to the one home of the provider's amount conversions (T402 KK4; 09 14.2 items 1, 3 and 5): <see cref="PaymentAmounts.FromKurus"/>
/// and its round trip with <see cref="PaymentAmounts.ToKurus"/>, and the four cases of <see cref="PaymentAmounts.NotificationMatches"/>.
/// </summary>
public sealed class PaymentAmountsNotificationTests
{
    [Theory]
    [InlineData(3456L, "34.56")]
    [InlineData(10L, "0.10")]
    [InlineData(1L, "0.01")]
    [InlineData(0L, "0")]
    [InlineData(123456789L, "1234567.89")]
    public void FromKurus_DividesByAHundred_WithoutRounding(long kurus, string expected)
    {
        Assert.Equal(decimal.Parse(expected, System.Globalization.CultureInfo.InvariantCulture), PaymentAmounts.FromKurus(kurus));
    }

    [Fact]
    public void FromKurusThenToKurus_IsTheSameValue_ForAThousandValues()
    {
        for (long kurus = 0; kurus < 1000; kurus++)
        {
            long value = kurus * 7919;
            Assert.Equal(value, PaymentAmounts.ToKurus(PaymentAmounts.FromKurus(value)));
        }
    }

    [Theory]
    [InlineData(12000L, 12000L, true)]
    [InlineData(12000L, 12450L, true)]
    [InlineData(12000L, 11999L, false)]
    [InlineData(11999L, 12000L, false)]
    public void NotificationMatches_EqualAmount_InstalmentDifference_ShortTotal_OtherStartedAmount(long paymentAmount, long totalAmount, bool expected)
    {
        Assert.Equal(expected, PaymentAmounts.NotificationMatches(paymentAmount, totalAmount, 120m));
    }
}
