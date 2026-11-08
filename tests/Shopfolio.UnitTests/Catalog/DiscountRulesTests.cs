using System.Globalization;

using Shopfolio.Infrastructure;
using Shopfolio.Modules.Catalog;
using Shopfolio.Modules.Catalog.Contracts.Pricing;

namespace Shopfolio.UnitTests.Catalog;

/// <summary>
/// The discount rules as pure functions (T337 criteria 1-3, 8; KK1-KK3, KK8; 02 3.9.1, 3.9.2, 3.9.5, 4.1.4, 5.11.1; 06 2.5.2, 4.11, 4.12; K-600, K-1023 (4), K-1041 (2)):
/// every variant is discounted on its own price, rounded once to the kuruş with an exact half up; a rate of one hundred cannot be entered and a rounded discounted
/// price of zero is refused, for a discount and for a price entered while it has not ended; the range starts at 00.00 Turkey time of its start day and holds its end
/// day up to 23.59.59; a discounted price that is not below the reference is refused; the bounds a range owes the price history are its own moments. The expected
/// amounts are worked out in the comments; the moments are derived from <see cref="TurkeyTime"/>, Turkey being UTC+3.
/// </summary>
public sealed class DiscountRulesTests
{
    private static readonly DateOnly Start = new(2026, 11, 10);
    private static readonly DateOnly End = new(2026, 11, 12);
    private static readonly DiscountTerms Terms = new(20m, Start, End);

    // ---- KK1: every variant on its own price ----

    [Theory]
    [InlineData("1000.00", "20", "800.00")] // 1000 x 0,80
    [InlineData("1250.00", "20", "1000.00")] // another variant of the same product, its own price
    [InlineData("333.33", "15", "283.33")] // 333,33 x 0,85 = 283,3305
    [InlineData("0.05", "10", "0.05")] // 0,045: an exact half kuruş goes up
    [InlineData("99.99", "33.33", "66.66")] // 99,99 x 0,6667 = 66,663333
    public void TheDiscountedPrice_IsTheVariantsOwnPriceLessTheRate_RoundedOnceHalfUp(string price, string percentage, string expected)
    {
        Assert.Equal(Dec(expected), DiscountRules.DiscountedPrice(Dec(price), Dec(percentage)));
    }

    // ---- KK2: one hundred percent and a zero result are refused ----

    [Theory]
    [InlineData("0.01", true)]
    [InlineData("99.99", true)]
    [InlineData("50", true)]
    [InlineData("0", false)]
    [InlineData("100", false)]
    [InlineData("100.01", false)]
    [InlineData("-5", false)]
    [InlineData("10.125", false)]
    public void TheRate_IsAboveZeroAndBelowOneHundred_WithAtMostTwoDecimals(string percentage, bool valid)
    {
        Assert.Equal(valid, DiscountRules.IsValidPercentage(Dec(percentage)));
        Assert.Equal(valid ? DiscountProblem.None : DiscountProblem.InvalidPercentage, DiscountRules.CheckTerms(new DiscountTerms(Dec(percentage), Start, End)));
    }

    [Fact]
    public void ARateOutsideTheRule_IsAProgrammingErrorInTheCalculation()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => DiscountRules.DiscountedPrice(100m, 100m));
    }

    [Fact]
    public void ADiscountedPriceRoundedToZero_IsRefusedBeforeTheReference()
    {
        // 0,01 x 0,40 = 0,004 -> 0,00; the reference (1,00) would let it pass, the zero rule comes first.
        Assert.Equal(DiscountProblem.DiscountedPriceZero, DiscountRules.CheckVariant(0.01m, 1m, 60m));

        // 0,01 x 0,50 = 0,005 -> 0,01: not zero, and below a reference of 0,02.
        Assert.Equal(DiscountProblem.None, DiscountRules.CheckVariant(0.01m, 0.02m, 50m));
    }

    [Fact]
    public void APriceThatTheDiscountWouldBringToZero_IsTold()
    {
        Assert.True(DiscountRules.ReachesZero(0.01m, 60m)); // 0,004 -> 0,00
        Assert.False(DiscountRules.ReachesZero(0.02m, 60m)); // 0,008 -> 0,01
    }

    [Fact]
    public void AnEndBeforeTheStart_IsRefused_AOneDayRangeIsNot()
    {
        Assert.Equal(DiscountProblem.EndBeforeStart, DiscountRules.CheckTerms(new DiscountTerms(10m, End, Start)));
        Assert.Equal(DiscountProblem.None, DiscountRules.CheckTerms(new DiscountTerms(10m, Start, Start)));
    }

    // ---- KK3: 00.00 of the start day to 23.59 of the end day ----

    [Fact]
    public void TheRange_StartsAtMidnightOfTheStartDay_AndEndsAfterTheWholeEndDay()
    {
        Assert.Equal(new DateTimeOffset(2026, 11, 9, 21, 0, 0, TimeSpan.Zero), DiscountRules.StartsAt(Terms)); // 10 November 00.00 in Turkey
        Assert.Equal(new DateTimeOffset(2026, 11, 12, 21, 0, 0, TimeSpan.Zero), DiscountRules.EndsAt(Terms)); // 13 November 00.00 in Turkey
    }

    [Fact]
    public void TheState_IsReadFromTheDates_AtTheTwoBounds()
    {
        DateTimeOffset startDay = TurkeyTime.StartOf(Start);
        DateTimeOffset lastMinute = new(2026, 11, 12, 23, 59, 59, TurkeyTime.Offset);

        Assert.Equal(DiscountState.NotStarted, DiscountRules.StateAt(Terms, startDay.AddTicks(-1)));
        Assert.Equal(DiscountState.Running, DiscountRules.StateAt(Terms, startDay));
        Assert.Equal(DiscountState.Running, DiscountRules.StateAt(Terms, new DateTimeOffset(2026, 11, 12, 23, 59, 0, TurkeyTime.Offset)));
        Assert.Equal(DiscountState.Running, DiscountRules.StateAt(Terms, lastMinute));
        Assert.Equal(DiscountState.Ended, DiscountRules.StateAt(Terms, TurkeyTime.StartOf(End.AddDays(1))));
    }

    [Fact]
    public void TheAppliedPrice_IsTheDiscountedPriceOnlyWhileTheDiscountRuns()
    {
        Assert.Equal(100m, DiscountRules.AppliedPrice(100m, Terms, DiscountRules.StartsAt(Terms).AddTicks(-1)));
        Assert.Equal(80m, DiscountRules.AppliedPrice(100m, Terms, DiscountRules.StartsAt(Terms)));
        Assert.Equal(100m, DiscountRules.AppliedPrice(100m, Terms, DiscountRules.EndsAt(Terms)));
        Assert.Equal(100m, DiscountRules.AppliedPrice(100m, null, DiscountRules.StartsAt(Terms)));
    }

    // ---- KK8: a discounted price that is not below the reference ----

    [Theory]
    [InlineData("100.00", "80.00", "20", "NotBelowReference")] // 80,00 is not below 80,00
    [InlineData("100.00", "80.00", "20.01", "None")] // 79,99
    [InlineData("100.00", "70.00", "20", "NotBelowReference")] // 80,00 above 70,00
    [InlineData("100.00", "100.00", "0.01", "None")] // 99,99
    public void ADiscountedPriceNotBelowTheReference_IsRefused(string price, string reference, string percentage, string expected)
    {
        Assert.Equal(Enum.Parse<DiscountProblem>(expected), DiscountRules.CheckVariant(Dec(price), Dec(reference), Dec(percentage)));
    }

    [Fact]
    public void TheDeclaration_CarriesTheDiscountedPriceTheReferenceTheRateAndTheDates()
    {
        Assert.Equal(new DiscountLine(80m, 90m, 20m, Start, End), DiscountRules.LineOf(Terms, 100m, 90m));
    }

    // ---- the bounds a range owes the price history (06 4.11; K-1023 (4), K-1041 (2)) ----

    [Fact]
    public void TheStartBound_IsOwedOnceItCame_AtItsOwnMomentWithTheDiscountedPrice()
    {
        DateTimeOffset start = DiscountRules.StartsAt(Terms);
        DateTimeOffset defined = start.AddDays(-3);

        Assert.Empty(DiscountRules.OwedBounds(Terms, defined, 100m, defined, start.AddTicks(-1)));
        Assert.Equal(new PricePoint[] { new PricePoint(start, 80m) }, DiscountRules.OwedBounds(Terms, defined, 100m, defined, start));

        // The scheduler comes six hours late after an outage: the row still carries the range's bound.
        Assert.Equal(new PricePoint[] { new PricePoint(start, 80m) }, DiscountRules.OwedBounds(Terms, defined, 100m, defined, start.AddHours(6)));
    }

    [Fact]
    public void BothBounds_AreOwedAfterTheEnd_AndAWrittenBoundIsNotOwedAgain()
    {
        DateTimeOffset start = DiscountRules.StartsAt(Terms);
        DateTimeOffset end = DiscountRules.EndsAt(Terms);
        DateTimeOffset defined = start.AddDays(-3);

        Assert.Equal(new PricePoint[] { new PricePoint(start, 80m), new PricePoint(end, 100m) }, DiscountRules.OwedBounds(Terms, defined, 100m, null, end));
        Assert.Equal(new PricePoint[] { new PricePoint(end, 100m) }, DiscountRules.OwedBounds(Terms, defined, 100m, start, end));
        Assert.Empty(DiscountRules.OwedBounds(Terms, defined, 100m, end, end.AddDays(1)));
    }

    [Fact]
    public void ARangeEnteredInThePast_OwesOnlyTheBoundsAfterItsDefinition()
    {
        DateTimeOffset start = DiscountRules.StartsAt(Terms);
        DateTimeOffset end = DiscountRules.EndsAt(Terms);

        // Defined on its second day: the start had passed and is not written back; the end is owed.
        DateTimeOffset defined = start.AddDays(1);
        Assert.Equal(new PricePoint[] { new PricePoint(end, 100m) }, DiscountRules.OwedBounds(Terms, defined, 100m, defined, end));

        // Defined after its end: nothing is owed.
        Assert.Empty(DiscountRules.OwedBounds(Terms, end.AddHours(1), 100m, null, end.AddDays(1)));
    }

    // ---- T326 KK1: the discount that ends reverts the price; ending at the confirmation is a summary difference (Z-20; 03 4.1.23) ----

    [Fact]
    public void TheAppliedPrice_RevertsToTheOwnPrice_AtTheExactMomentTheRangeEnds_Z20_KK1()
    {
        DateTimeOffset end = DiscountRules.EndsAt(Terms);

        Assert.Equal(80m, DiscountRules.AppliedPrice(100m, Terms, end.AddTicks(-1)));
        Assert.Equal(100m, DiscountRules.AppliedPrice(100m, Terms, end));
        Assert.Equal(100m, DiscountRules.AppliedPrice(100m, Terms, end.AddDays(30)));
    }

    [Fact]
    public void ADiscountThatEndsBetweenTheSummaryAndTheConfirmation_IsASummaryDifference_Z20_KK1()
    {
        DateTimeOffset end = DiscountRules.EndsAt(Terms);

        Assert.True(DiscountRules.IsSummaryDifference(Terms, end.AddMinutes(-5), end));
        Assert.True(DiscountRules.IsSummaryDifference(Terms, end.AddMinutes(-5), end.AddHours(2)));
        Assert.False(DiscountRules.IsSummaryDifference(Terms, end.AddMinutes(-5), end.AddTicks(-1)));
    }

    [Fact]
    public void ADiscountThatStartsBetweenTheSummaryAndTheConfirmation_IsASummaryDifference_AndAnUnchangedStateIsNot_Z20_KK1()
    {
        DateTimeOffset start = DiscountRules.StartsAt(Terms);
        DateTimeOffset end = DiscountRules.EndsAt(Terms);

        Assert.True(DiscountRules.IsSummaryDifference(Terms, start.AddMinutes(-1), start));
        Assert.False(DiscountRules.IsSummaryDifference(Terms, start.AddHours(1), end.AddTicks(-1)));
        Assert.False(DiscountRules.IsSummaryDifference(Terms, end, end.AddDays(3)));
        Assert.False(DiscountRules.IsSummaryDifference(Terms, start.AddDays(-9), start.AddDays(-8)));
        Assert.False(DiscountRules.IsSummaryDifference(null, start.AddMinutes(-1), start));
    }

    private static decimal Dec(string text) => decimal.Parse(text, CultureInfo.InvariantCulture);
}
