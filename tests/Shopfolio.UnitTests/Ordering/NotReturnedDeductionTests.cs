using Shopfolio.Modules.Ordering;

namespace Shopfolio.UnitTests.Ordering;

/// <summary>
/// K-1105 (T529 KK5; 07 UC-131, UC-133; 02 10.4.9): the amount based refunds bound to the item's "mal dönmedi" closures are deducted from the later receipts' refunds in order, until used up - the
/// arithmetic of <see cref="NotReturnedDeduction"/>.
/// </summary>
public sealed class NotReturnedDeductionTests
{
    [Theory]
    // Nothing bound: the receipt pays its whole amount.
    [InlineData(0, 0, 0, 80, 0)]
    // The first receipt bears the bound total when it is smaller than the receipt.
    [InlineData(30, 0, 0, 80, 30)]
    // The bound total larger than the receipt: the receipt bears all of it, the rest waits for the next one.
    [InlineData(100, 0, 0, 80, 80)]
    // The second receipt bears what is left after the first bore its 80.
    [InlineData(100, 80, 0, 80, 20)]
    // Used up: the earlier receipts already bore the whole bound total.
    [InlineData(100, 160, 60, 80, 0)]
    // An earlier receipt that paid in full bore nothing: the whole bound total is still left.
    [InlineData(50, 80, 80, 80, 50)]
    public void TheDeduction_IsWhatIsLeftOfTheBoundTotal_AtMostTheReceiptsAmount_K1105(double bound, double earlierGross, double earlierPaid, double gross, double expected)
    {
        decimal deduction = NotReturnedDeduction.Of((decimal)bound, (decimal)earlierGross, (decimal)earlierPaid, (decimal)gross);

        Assert.Equal((decimal)expected, deduction);
    }

    [Fact]
    public void ThreeReceiptsInOrder_BearTheBoundTotalUntilItIsUsedUp_K1105()
    {
        const decimal Bound = 130m;
        decimal[] grosses = [50m, 50m, 50m];
        decimal earlierGross = 0m;
        decimal earlierPaid = 0m;
        List<decimal> paid = [];
        foreach (decimal gross in grosses)
        {
            decimal amount = gross - NotReturnedDeduction.Of(Bound, earlierGross, earlierPaid, gross);
            paid.Add(amount);
            earlierGross += gross;
            earlierPaid += amount;
        }

        Assert.Equal([0m, 0m, 20m], paid);
    }
}
