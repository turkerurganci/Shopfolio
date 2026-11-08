using Shopfolio.Infrastructure;
using Shopfolio.Modules.AfterSales.Contracts;
using Shopfolio.Modules.Catalog;
using Shopfolio.Modules.Catalog.Contracts;

namespace Shopfolio.UnitTests.Catalog;

/// <summary>
/// The reference price as a pure function (T337 criteria 5-7; KK6, KK7; 02 3.9.3, 3.9.4, 4.2 Z-21; K-1023 (2), K-563, K-1483, K-1493): on goods the lowest price
/// applied in the <see cref="ReferencePriceRules.LookbackDays"/> days before the discount's start - the price in force when the window opens included, an earlier
/// discounted price included -, never reaching back before the variant first went on sale; on a service and on a perishable product the price right before the start;
/// a product exempt for an expiry date follows the goods' rule. The window is derived from the constant, not written as a number.
/// </summary>
public sealed class ReferencePriceRulesTests
{
    private static readonly DateTimeOffset Start = TurkeyTime.StartOf(new DateOnly(2026, 11, 20));
    private static readonly DateTimeOffset WindowStart = Start.AddDays(-ReferencePriceRules.LookbackDays);
    private static readonly DateTimeOffset LongOnSale = Start.AddDays(-60);

    // ---- KK6: the rule of each product ----

    [Theory]
    [InlineData(ProductType.Physical, null, "LowestInWindow")]
    [InlineData(ProductType.Digital, null, "LowestInWindow")]
    [InlineData(ProductType.Service, null, "ImmediatelyBefore")]
    [InlineData(ProductType.Physical, WithdrawalExemptionReason.Perishable, "ImmediatelyBefore")]
    [InlineData(ProductType.Physical, WithdrawalExemptionReason.ExpiryDated, "LowestInWindow")]
    [InlineData(ProductType.Physical, WithdrawalExemptionReason.MadeToOrder, "LowestInWindow")]
    [InlineData(ProductType.Physical, WithdrawalExemptionReason.HygieneSensitive, "LowestInWindow")]
    [InlineData(ProductType.Digital, WithdrawalExemptionReason.SealedMedia, "LowestInWindow")]
    public void TheRule_IsImmediatelyBeforeOnlyOnAServiceAndAPerishableProduct(ProductType type, WithdrawalExemptionReason? reason, string expected)
    {
        Assert.Equal(Enum.Parse<ReferenceRule>(expected), ReferencePriceRules.RuleOf(type, reason));
    }

    // ---- KK6: the lowest price of the window on goods ----

    [Fact]
    public void OnGoods_TheReference_IsTheLowestPriceAppliedInTheWindow_ThePriceInForceAtItsOpeningIncluded()
    {
        PricePoint[] history =
        [
            new(LongOnSale, 120m), // in force when the window opens
            new(WindowStart.AddDays(2), 90m),
            new(WindowStart.AddDays(5), 110m),
        ];

        Assert.Equal(90m, Reference(ReferenceRule.LowestInWindow, history, 110m));
        Assert.Equal(110m, Reference(ReferenceRule.LowestInWindow, [new(LongOnSale, 120m), new(WindowStart.AddDays(5), 110m)], 110m));
    }

    [Fact]
    public void APriceThatEndedBeforeTheWindow_DoesNotCount_ARowAtTheWindowsOpeningDoes()
    {
        PricePoint[] ended = [new(LongOnSale, 70m), new(WindowStart.AddTicks(-1), 100m), new(WindowStart.AddDays(4), 95m)];
        Assert.Equal(95m, Reference(ReferenceRule.LowestInWindow, ended, 95m));

        PricePoint[] atOpening = [new(LongOnSale, 70m), new(WindowStart, 90m), new(WindowStart.AddDays(4), 95m)];
        Assert.Equal(90m, Reference(ReferenceRule.LowestInWindow, atOpening, 95m));

        // The window holds its last moment before the start (Start - 10 days, up to the start).
        PricePoint[] lastMoment = [new(LongOnSale, 100m), new(Start.AddTicks(-1), 60m)];
        Assert.Equal(60m, Reference(ReferenceRule.LowestInWindow, lastMoment, 100m));
    }

    [Fact]
    public void AnEarlierDiscountedPrice_IsAnAppliedPriceAndLowersTheReference()
    {
        // The previous campaign's discounted price (80) started and ended inside the window.
        PricePoint[] history = [new(LongOnSale, 100m), new(WindowStart.AddDays(1), 80m), new(WindowStart.AddDays(4), 100m)];

        Assert.Equal(80m, Reference(ReferenceRule.LowestInWindow, history, 100m));
    }

    [Fact]
    public void ThePriceOfAStretchOffSale_Counts_TheRuleDoesNotLookAtThePublicationStatus()
    {
        // The variant was taken off sale between the second and the sixth day of the window and its price was lowered meanwhile; the row stays in the window.
        PricePoint[] history = [new(LongOnSale, 100m), new(WindowStart.AddDays(2), 75m), new(WindowStart.AddDays(6), 100m)];

        Assert.Equal(75m, Reference(ReferenceRule.LowestInWindow, history, 100m));
    }

    [Fact]
    public void AnExpiryDatedProduct_TakesTheWindowsLowest_APerishableOneThePriceRightBefore()
    {
        PricePoint[] history = [new(LongOnSale, 100m), new(WindowStart.AddDays(2), 80m), new(WindowStart.AddDays(6), 100m)];

        Assert.Equal(80m, Reference(ReferencePriceRules.RuleOf(ProductType.Physical, WithdrawalExemptionReason.ExpiryDated), history, 100m));
        Assert.Equal(100m, Reference(ReferencePriceRules.RuleOf(ProductType.Physical, WithdrawalExemptionReason.Perishable), history, 100m));
        Assert.Equal(100m, Reference(ReferencePriceRules.RuleOf(ProductType.Service, null), history, 100m));
    }

    [Fact]
    public void ImmediatelyBefore_IsTheLastPriceBeforeTheStart_AndRowsFromTheStartOnAreIgnored()
    {
        PricePoint[] history = [new(LongOnSale, 100m), new(WindowStart.AddDays(2), 80m), new(WindowStart.AddDays(6), 90m), new(Start, 72m), new(Start.AddDays(1), 50m)];

        Assert.Equal(90m, Reference(ReferenceRule.ImmediatelyBefore, history, 90m));
        Assert.Equal(80m, Reference(ReferenceRule.LowestInWindow, history, 90m));
    }

    [Fact]
    public void AVariantWithoutARowBeforeTheStart_TakesItsOwnPrice()
    {
        Assert.Equal(100m, Reference(ReferenceRule.LowestInWindow, [], 100m));
        Assert.Equal(100m, Reference(ReferenceRule.ImmediatelyBefore, [new(Start, 80m)], 100m));
    }

    // ---- KK7: a shorter history begins with the variant's own going on sale ----

    [Fact]
    public void AVariantOnSaleForLessThanTheWindow_TakesTheLowestSinceItWentOnSale()
    {
        DateTimeOffset onSale = WindowStart.AddDays(6);

        // Created at 70 before it went on sale; 70 was the price in force when it went on sale, so it applied then.
        PricePoint[] history = [new(onSale.AddDays(-1), 70m), new(onSale.AddDays(1), 90m)];
        Assert.Equal(70m, ReferencePriceRules.Reference(ReferenceRule.LowestInWindow, Start, onSale, history, 90m));

        // A price set and replaced before it went on sale never applied on sale.
        PricePoint[] beforeSale = [new(onSale.AddDays(-2), 40m), new(onSale.AddDays(-1), 100m), new(onSale.AddDays(1), 120m)];
        Assert.Equal(100m, ReferencePriceRules.Reference(ReferenceRule.LowestInWindow, Start, onSale, beforeSale, 120m));
    }

    [Fact]
    public void AVariantNeverOnSaleBeforeTheStart_TakesThePriceRightBefore()
    {
        PricePoint[] history = [new(WindowStart.AddDays(1), 60m), new(WindowStart.AddDays(3), 100m)];

        Assert.Equal(100m, ReferencePriceRules.Reference(ReferenceRule.LowestInWindow, Start, null, history, 100m));
        Assert.Equal(100m, ReferencePriceRules.Reference(ReferenceRule.LowestInWindow, Start, Start, history, 100m));
    }

    private static decimal Reference(ReferenceRule rule, IEnumerable<PricePoint> history, decimal price)
        => ReferencePriceRules.Reference(rule, Start, LongOnSale, history, price);
}
