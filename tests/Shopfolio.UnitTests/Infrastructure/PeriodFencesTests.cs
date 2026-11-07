using Shopfolio.Infrastructure;

namespace Shopfolio.UnitTests.Infrastructure;

/// <summary>
/// The three fenced periods (T378 criterion 8; KK8; 02 4.1.5, 11.1): the shipping period has an upper fence, the bank transfer payment window a
/// lower and an upper one, the service fulfilment period a lower one. A value outside is refused with the side and the fence it broke.
/// </summary>
public sealed class PeriodFencesTests
{
    private const int LegalDeliveryDays = 30;

    [Theory]
    [InlineData(0, FenceSide.Within, 0)]
    [InlineData(2, FenceSide.Within, 0)]
    [InlineData(10, FenceSide.Within, 0)]
    [InlineData(11, FenceSide.AboveUpper, 10)]
    [InlineData(-1, FenceSide.BelowLower, 0)]
    public void CheckShippingPeriod_UpperFenceIsTenBusinessDays(int days, FenceSide side, int fence)
    {
        PeriodFences.Verdict verdict = PeriodFences.CheckShippingPeriod(days);

        Assert.Equal(side, verdict.Side);
        Assert.Equal(fence, verdict.Fence);
        Assert.Equal(side == FenceSide.Within, verdict.IsWithin);
    }

    [Theory]
    [InlineData(0, FenceSide.BelowLower, 1)]
    [InlineData(1, FenceSide.Within, 0)]
    [InlineData(2, FenceSide.Within, 0)]
    [InlineData(3, FenceSide.Within, 0)]
    [InlineData(4, FenceSide.AboveUpper, 3)]
    public void CheckBankTransferPaymentWindow_HasALowerAndAnUpperFence(int days, FenceSide side, int fence)
    {
        PeriodFences.Verdict verdict = PeriodFences.CheckBankTransferPaymentWindow(days);

        Assert.Equal(side, verdict.Side);
        Assert.Equal(fence, verdict.Fence);
    }

    [Theory]
    [InlineData(0, FenceSide.BelowLower, 1)]
    [InlineData(-5, FenceSide.BelowLower, 1)]
    [InlineData(1, FenceSide.Within, 0)]
    [InlineData(365, FenceSide.Within, 0)]
    [InlineData(int.MaxValue, FenceSide.Within, 0)]
    public void CheckFulfilmentPeriod_HasALowerFenceOnly(int days, FenceSide side, int fence)
    {
        PeriodFences.Verdict verdict = PeriodFences.CheckFulfilmentPeriod(days);

        Assert.Equal(side, verdict.Side);
        Assert.Equal(fence, verdict.Fence);
    }

    [Fact]
    public void FenceValues_AreTheOnesOfTheSingleDefinition()
    {
        Assert.Equal(10, ParameterFences.ShippingPeriodUpper);
        Assert.Equal(0, ParameterFences.ShippingPeriodLower);
        Assert.Equal(1, ParameterFences.BankTransferPaymentWindowLower);
        Assert.Equal(3, ParameterFences.BankTransferPaymentWindowUpper);
        Assert.Equal(1, ParameterFences.FulfilmentPeriodLower);
    }

    [Fact]
    public void UpperFences_AtTheirLongest_LeaveAtLeastFourDaysOfTheLegalThirtyInEveryStartDay()
    {
        // 02 4.1.5: three business days of payment window then ten of shipping is 13 business days; over the shipped calendar the longest
        // stretch, a four to five day holiday included, is at most 26 calendar days, so four or more days are left for carrying the goods.
        BusinessCalendar calendar = BusinessCalendar.Product;
        DateOnly first = calendar.Holidays[0];
        DateOnly last = calendar.Holidays[^1];
        int longest = 0;
        for (DateOnly start = new(first.Year, 1, 1); start <= last; start = start.AddDays(1))
        {
            DateOnly paymentEnd = calendar.LastDayOfBusinessPeriod(start, ParameterFences.BankTransferPaymentWindowUpper);
            DateOnly shippingEnd = calendar.LastDayOfBusinessPeriod(paymentEnd, ParameterFences.ShippingPeriodUpper);
            longest = Math.Max(longest, shippingEnd.DayNumber - start.DayNumber);
        }

        Assert.True(longest <= LegalDeliveryDays - 4, $"the longest stretch is {longest} calendar days");
        Assert.True(longest >= 19, $"the longest stretch is {longest} calendar days");
    }
}
