using Shopfolio.Infrastructure.Errors;
using Shopfolio.Modules.Catalog;
using Shopfolio.Modules.Catalog.Contracts.Coupons;
using Shopfolio.Modules.Catalog.Contracts.Pricing;

namespace Shopfolio.UnitTests.Catalog;

/// <summary>
/// The coupon's rules as pure functions (T338 criteria 2-6, 8, 9; KK2-KK6, KK8, KK9; 02 3.10.2-3.10.7, 5.11.3; 09 14.1): the definition's field rules, the item-by-item share - a
/// percentage rounded in each item, a fixed amount spread in proportion with the parts rounded down and the left-over kuruş on the largest item -, the VAT after the share,
/// the refusals (dates, rights, a total of zero), the minimum basket amount, the threshold base and the return of the right. The expected values are worked out in the
/// comments from the rule, not read back from the code.
/// </summary>
public sealed class CouponRulesTests
{
    // 15 November 2026, 12:00 in Turkey; the coupons run from 1 to 30 November.
    private static readonly DateTimeOffset Now = new(2026, 11, 15, 9, 0, 0, TimeSpan.Zero);
    private static readonly DateOnly Start = new(2026, 11, 1);
    private static readonly DateOnly End = new(2026, 11, 30);

    // ---- KK2: both definitions ----

    [Fact]
    public void APercentageCoupon_AndAFixedAmountCoupon_AreBothDefinitionsThatSave()
    {
        Assert.Empty(CouponRules.CheckDefinition(Percent(10m)));
        Assert.Empty(CouponRules.CheckDefinition(Percent(99.99m) with { MinimumBasketAmount = 0m }));
        Assert.Empty(CouponRules.CheckDefinition(Fixed(50m, 200m)));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(100)]
    [InlineData(-5)]
    [InlineData(10.005)]
    public void APercentageOutsideAboveZeroAndUnderOneHundred_IsTheInvalidPercentageMessage(double value)
    {
        Assert.Equal([new CouponProblem("Value", ErrorCode.InvalidPercentage)], CouponRules.CheckDefinition(Percent((decimal)value)));
    }

    [Fact]
    public void AFixedAmountOfZero_OrAMinimumNotLargerThanIt_OrNoMinimum_IsTheCouponValueMessage()
    {
        Assert.Contains(new CouponProblem("Value", ErrorCode.CouponValueInvalid), CouponRules.CheckDefinition(Fixed(0m, 100m)));
        Assert.Equal([new CouponProblem("MinimumBasketAmount", ErrorCode.CouponValueInvalid)], CouponRules.CheckDefinition(Fixed(50m, 50m)));
        Assert.Equal([new CouponProblem("MinimumBasketAmount", ErrorCode.CouponValueInvalid)], CouponRules.CheckDefinition(Fixed(50m, 49.99m)));
        Assert.Equal([new CouponProblem("MinimumBasketAmount", ErrorCode.CouponValueInvalid)], CouponRules.CheckDefinition(Fixed(50m, null)));
    }

    [Fact]
    public void AnAmountThatIsNoKurusAmount_IsTheAmountFieldsMessage()
    {
        Assert.Equal([new CouponProblem("Value", ErrorCode.InvalidAmount)], CouponRules.CheckDefinition(Fixed(10.005m, 100m)));
        Assert.Equal([new CouponProblem("MinimumBasketAmount", ErrorCode.InvalidAmount)], CouponRules.CheckDefinition(Fixed(10m, 100.001m)));
        Assert.Equal([new CouponProblem("MinimumBasketAmount", ErrorCode.InvalidAmount)], CouponRules.CheckDefinition(Percent(10m) with { MinimumBasketAmount = -1m }));
    }

    [Fact]
    public void ABlankCode_AnEndBeforeTheStart_AndACountUnderZero_AreEachTheirFieldsMessage_AllTogether()
    {
        CouponDefinition definition = Percent(10m) with { Code = "  ", StartDate = End, EndDate = Start, TotalUsageLimit = -1 };

        Assert.Equal(
            [
                new CouponProblem("Code", ErrorCode.FieldRequired),
                new CouponProblem("EndDate", ErrorCode.InvalidDateRange),
                new CouponProblem("TotalUsageLimit", ErrorCode.CouponUsageBelowUsed),
            ],
            CouponRules.CheckDefinition(definition));
    }

    [Fact]
    public void AOneDayRange_AndAPersonalCodeWithACountOfOne_Save()
    {
        Assert.Empty(CouponRules.CheckDefinition(Percent(10m) with { StartDate = Start, EndDate = Start, TotalUsageLimit = 1 }));
    }

    // ---- KK3: the share item by item, the left-over kuruş on the largest item ----

    [Fact]
    public void AFixedAmount_IsSpreadInProportion_PartsRoundedDown_TheLeftOverKurusOnTheLargestItem()
    {
        // 10,00 over 50 / 150 / 100 (300): 1,666.. -> 1,66; 5,00; 3,333.. -> 3,33; together 9,99, the kuruş left over goes to the largest item (150): 5,01.
        Assert.Equal([1.66m, 5.01m, 3.33m], CouponRules.AmountShares(10m, [50m, 150m, 100m]));

        // Three equal items: 3,333.. -> 3,33 each, 9,99; the kuruş goes to the first of the equal largest.
        Assert.Equal([3.34m, 3.33m, 3.33m], CouponRules.AmountShares(10m, [100m, 100m, 100m]));
    }

    [Fact]
    public void APercentageCoupon_MakesEveryItemSmaller_ItsShareRoundedInTheItem_AHalfKurusUp()
    {
        // 10% of 33,35 is 3,335 -> 3,34 (a half up, 02 3.8.3); 10% of 12,34 is 1,234 -> 1,23; 10% of 0,05 is 0,005 -> 0,01.
        Assert.Equal([3.34m, 1.23m, 0.01m], CouponRules.PercentageShares(10m, [33.35m, 12.34m, 0.05m]));
    }

    [Fact]
    public void TheLeftOverKurus_ThatWouldTakeTheLargestItemPastItsAmount_GoesOnToTheNextLargest()
    {
        // 2,99 over three items of 1,00 (the minimum 3,00 lets it in): 0,99 each, 2,97; the 2 kuruş left over would make the first 1,01, more than the item. It keeps
        // 1,00 and the kuruş that does not fit goes to the next item: 1,00 / 1,00 / 0,99 - still 2,99, no item below zero (D-482).
        IReadOnlyList<decimal> shares = CouponRules.AmountShares(2.99m, [1m, 1m, 1m]);

        Assert.Equal([1.00m, 1.00m, 0.99m], shares);
    }

    [Fact]
    public void FixedAmountShares_AlwaysAddUpToTheAmount_AndNoShareIsNegativeOrPassesItsItem()
    {
        Random random = new(338);
        for (int round = 0; round < 1000; round++)
        {
            int count = random.Next(1, 7);
            List<decimal> amounts = [.. Enumerable.Range(0, count).Select(_ => random.Next(1, 100_000) / 100m)];
            decimal total = amounts.Sum();
            decimal amount = random.Next(1, (int)(total * 100)) / 100m;
            if (amount >= total)
            {
                continue;
            }

            IReadOnlyList<decimal> shares = CouponRules.AmountShares(amount, amounts);

            Assert.Equal(amount, shares.Sum());
            Assert.All(shares.Zip(amounts), pair => Assert.InRange(pair.First, 0m, pair.Second));
        }
    }

    // ---- KK4: the share is the item's and the VAT comes after it ----

    [Fact]
    public void TheAcceptedCoupon_GivesEveryItemItsShare_AndTheVatIsSeparatedAfterTheShare()
    {
        // Item 1: 100,00 x 2 = 200,00 at 20%; item 2: 60,00 x 1 = 60,00 at 10%. 10%: shares 20,00 and 6,00.
        // Item 1 pays 180,00, VAT 180 x 20 / 120 = 30,00, net 150,00; item 2 pays 54,00, VAT 54 x 10 / 110 = 4,909.. -> 4,91, net 49,09.
        CouponApplication application = CouponRules.Evaluate(PercentTerms(10m), [new CouponLine(100m, 2, 20m), new CouponLine(60m, 1, 10m)], Now);

        Assert.Equal(CouponOutcome.Accepted, application.Outcome);
        Assert.Equal(
            [new ItemAmounts(200m, 20m, 180m, 30.00m, 150.00m), new ItemAmounts(60m, 6m, 54m, 4.91m, 49.09m)],
            application.Items);
        Assert.Equal((26m, 234m), (application.Discount, application.PayableTotal));

        // The VAT without the share first would be 200 x 20 / 120 = 33,33 - not what the item freezes.
        Assert.NotEqual(33.33m, application.Items[0].Vat);
    }

    [Fact]
    public void AFixedCoupon_ComesDownFromTheDiscountedPrice_ItsSharesSpreadOverTheItems()
    {
        // A discounted unit price already rounded (83,33) x 3 = 249,99 and 50,01 x 1: basket 300,00; 30,00 spread: 24,999 -> 24,99 and 5,001 -> 5,00, 29,99, the
        // kuruş to the largest: 25,00 and 5,00.
        CouponApplication application = CouponRules.Evaluate(FixedTerms(30m, 100m), [new CouponLine(83.33m, 3, 20m), new CouponLine(50.01m, 1, 20m)], Now);

        Assert.Equal([25.00m, 5.00m], application.Items.Select(i => i.CouponShare));
        Assert.Equal(270.00m, application.PayableTotal);
    }

    // ---- KK5: never down to zero ----

    [Fact]
    public void AFixedCouponEqualToOrAboveTheBasket_IsRejected()
    {
        // The stored rule (minimum larger than the amount) keeps such a coupon off the basket already; the verdict refuses it on its own as well.
        CouponLine[] basket = [new CouponLine(40m, 1, 20m), new CouponLine(60m, 1, 20m)];

        Assert.Equal(CouponOutcome.Rejected, CouponRules.Evaluate(FixedTerms(100m, null), basket, Now).Outcome);
        Assert.Equal(CouponOutcome.Rejected, CouponRules.Evaluate(FixedTerms(100.01m, null), basket, Now).Outcome);
        Assert.Equal(CouponOutcome.Accepted, CouponRules.Evaluate(FixedTerms(99.99m, null), basket, Now).Outcome);
    }

    [Fact]
    public void APercentageCoupon_ThatTheRoundingTakesToATotalOfZero_IsRejected()
    {
        // 50% of 0,01 is 0,005 -> 0,01: each item pays nothing, the total would be zero. 99,99% of 0,01 the same.
        CouponLine[] basket = [new CouponLine(0.01m, 1, 20m), new CouponLine(0.01m, 1, 20m)];

        Assert.Equal(CouponOutcome.Rejected, CouponRules.Evaluate(PercentTerms(50m), basket, Now).Outcome);
        Assert.Equal(CouponOutcome.Rejected, CouponRules.Evaluate(PercentTerms(99.99m), basket, Now).Outcome);

        // 40% of 0,01 is 0,004 -> 0,00: nothing comes off, the total stays above zero and the coupon is accepted.
        Assert.Equal(CouponOutcome.Accepted, CouponRules.Evaluate(PercentTerms(40m), basket, Now).Outcome);
    }

    [Fact]
    public void ABasketUnderTheMinimum_IsItsOwnOutcome_WithTheMinimum_AndABasketAtTheMinimumIsAccepted()
    {
        CouponApplication under = CouponRules.Evaluate(FixedTerms(50m, 200m), [new CouponLine(199.99m, 1, 20m)], Now);
        CouponApplication at = CouponRules.Evaluate(FixedTerms(50m, 200m), [new CouponLine(100m, 2, 20m)], Now);
        CouponApplication percentUnder = CouponRules.Evaluate(PercentTerms(10m) with { MinimumBasketAmount = 150m }, [new CouponLine(149m, 1, 20m)], Now);

        Assert.Equal((CouponOutcome.BelowMinimumBasket, 200m, 199.99m), (under.Outcome, under.MinimumBasketAmount, under.BasketTotal));
        Assert.Empty(under.Items);
        Assert.Equal(CouponOutcome.Accepted, at.Outcome);
        Assert.Equal(CouponOutcome.BelowMinimumBasket, percentUnder.Outcome);
    }

    // ---- KK6: the date range and the rights ----

    [Fact]
    public void TheRange_StartsAtMidnightOfTheStartDay_AndTheEndDayIsIncludedAsAWhole_InTurkeyTime()
    {
        CouponLine[] basket = [new CouponLine(100m, 1, 20m)];

        // 1 November 00:00 in Turkey is 31 October 21:00 UTC; 30 November 23:59 in Turkey is 20:59 UTC, 1 December 00:00 is 21:00 UTC (02 4.1.4; Z-22).
        Assert.Equal(CouponOutcome.Rejected, CouponRules.Evaluate(PercentTerms(10m), basket, new DateTimeOffset(2026, 10, 31, 20, 59, 59, TimeSpan.Zero)).Outcome);
        Assert.Equal(CouponOutcome.Accepted, CouponRules.Evaluate(PercentTerms(10m), basket, new DateTimeOffset(2026, 10, 31, 21, 0, 0, TimeSpan.Zero)).Outcome);
        Assert.Equal(CouponOutcome.Accepted, CouponRules.Evaluate(PercentTerms(10m), basket, new DateTimeOffset(2026, 11, 30, 20, 59, 59, TimeSpan.Zero)).Outcome);
        Assert.Equal(CouponOutcome.Rejected, CouponRules.Evaluate(PercentTerms(10m), basket, new DateTimeOffset(2026, 11, 30, 21, 0, 0, TimeSpan.Zero)).Outcome);
    }

    [Fact]
    public void ACouponWhoseUsedAndReservedRightsFillItsTotal_IsRejected()
    {
        CouponLine[] basket = [new CouponLine(100m, 1, 20m)];

        Assert.Equal(CouponOutcome.Rejected, CouponRules.Evaluate(PercentTerms(10m) with { TotalUsageLimit = 3, UsedCount = 2, ReservedCount = 1 }, basket, Now).Outcome);
        Assert.Equal(CouponOutcome.Rejected, CouponRules.Evaluate(PercentTerms(10m) with { TotalUsageLimit = 0 }, basket, Now).Outcome);
        Assert.Equal(CouponOutcome.Accepted, CouponRules.Evaluate(PercentTerms(10m) with { TotalUsageLimit = 3, UsedCount = 1, ReservedCount = 1 }, basket, Now).Outcome);

        // A personal code: a count of 1, once taken nobody else gets it.
        Assert.Equal(CouponOutcome.Rejected, CouponRules.Evaluate(PercentTerms(10m) with { TotalUsageLimit = 1, ReservedCount = 1 }, basket, Now).Outcome);
    }

    [Fact]
    public void EveryRefusal_CarriesNoCouponAndNoShare_SoTheReasonIsNotTold()
    {
        CouponLine[] basket = [new CouponLine(100m, 1, 20m)];
        CouponApplication expired = CouponRules.Evaluate(PercentTerms(10m), basket, Now.AddMonths(1));
        CouponApplication unknown = CouponRules.RejectUnknown(basket);

        Assert.Equal((CouponOutcome.Rejected, null, null, 100m), (expired.Outcome, expired.CouponId, expired.Code, expired.BasketTotal));
        Assert.Equal((CouponOutcome.Rejected, null, null, 100m), (unknown.Outcome, unknown.CouponId, unknown.Code, unknown.BasketTotal));
        Assert.Empty(expired.Items);
        Assert.Empty(unknown.Items);
    }

    // ---- KK8: the right comes back only when the whole order closes ----

    [Fact]
    public void TheRight_ComesBack_OnlyWhenEveryUnitOfEveryItemIsClosed()
    {
        Assert.True(CouponRules.ReturnsRight([new ItemClosure(2, 2), new ItemClosure(1, 1)]));
        Assert.False(CouponRules.ReturnsRight([new ItemClosure(2, 1), new ItemClosure(1, 1)]));
        Assert.False(CouponRules.ReturnsRight([new ItemClosure(2, 2), new ItemClosure(1, 0)]));
        Assert.False(CouponRules.ReturnsRight([new ItemClosure(3, 0)]));
    }

    [Fact]
    public void TheRightsRule_RefusesAnOrderWithoutItems_AndAClosureOutsideTheUnits()
    {
        Assert.Throws<ArgumentException>(() => CouponRules.ReturnsRight([]));
        Assert.Throws<ArgumentOutOfRangeException>(() => CouponRules.ReturnsRight([new ItemClosure(1, 2)]));
        Assert.Throws<ArgumentOutOfRangeException>(() => CouponRules.ReturnsRight([new ItemClosure(1, -1)]));
        Assert.Throws<ArgumentOutOfRangeException>(() => CouponRules.ReturnsRight([new ItemClosure(0, 0)]));
    }

    // ---- KK9: the coupon does not lower the threshold's base ----

    [Fact]
    public void TheBasketTotal_TheThresholdsAreComparedWith_IsTheTotalBeforeTheCoupon()
    {
        // A 500,00 basket with a 10% coupon pays 450,00 for its items; the free shipping threshold and the minimum order amount still see 500,00 (02 3.10.7).
        CouponApplication application = CouponRules.Evaluate(PercentTerms(10m), [new CouponLine(250m, 2, 20m)], Now);

        Assert.Equal((500m, 450m), (application.BasketTotal, application.PayableTotal));
    }

    // ---- The lines are items ----

    [Fact]
    public void AnEmptyBasket_OrALineThatIsNoItem_IsAProgrammingError()
    {
        Assert.Throws<ArgumentException>(() => CouponRules.Evaluate(PercentTerms(10m), [], Now));
        Assert.Throws<ArgumentOutOfRangeException>(() => CouponRules.Evaluate(PercentTerms(10m), [new CouponLine(0m, 1, 20m)], Now));
        Assert.Throws<ArgumentOutOfRangeException>(() => CouponRules.Evaluate(PercentTerms(10m), [new CouponLine(10m, 0, 20m)], Now));
    }

    private static CouponDefinition Percent(decimal value) => new("YAZ10", CouponKind.Percentage, value, null, Start, End, 100);

    private static CouponDefinition Fixed(decimal value, decimal? minimum) => new("YAZ50", CouponKind.Amount, value, minimum, Start, End, 100);

    private static CouponTerms PercentTerms(decimal percentage) => new(Guid.NewGuid(), "YAZ10", percentage, null, null, Start, End, 100, 0, 0);

    private static CouponTerms FixedTerms(decimal amount, decimal? minimum) => new(Guid.NewGuid(), "YAZ50", null, amount, minimum, Start, End, 100, 0, 0);
}
