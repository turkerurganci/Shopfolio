using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Ordering;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Modules.Settings.Contracts;

namespace Shopfolio.UnitTests.Ordering;

/// <summary>The cart's pure display and order-entry rules (T346; 02 3.16.3-3.16.7) and its shipping terms (T349; 02 3.18, 3.19).</summary>
public sealed class CartRulesTests
{
    private static readonly ShippingFeeSection NoTerms = new(0m, null, null, 1);

    private static CartLine Line(int quantity, decimal unitPrice, bool soldOut = false, bool onShowcase = true, ProductType type = ProductType.Physical)
        => new(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Masa", type, quantity, unitPrice, false, soldOut, CartRules.EntersOrder(onShowcase, soldOut));

    private static CartPaymentBlock BlockOf(CartLine[] lines, ShippingFeeSection? terms = null)
        => CartRules.PaymentBlockOf(lines, CartRules.AssessShipping(lines, terms ?? NoTerms));

    [Fact]
    public void UnderTheMinimum_ThePaymentStepIsClosed_WithTheMissingAmount_T349_KK1()
    {
        ShippingFeeSection terms = new(50m, null, 500m, 1);
        CartLine[] lines = [Line(1, 370m)];

        Assert.Equal(CartPaymentBlock.MinimumOrderAmountNotMet, BlockOf(lines, terms));
        Assert.Equal("Sipariş verebilmek için 130 TL'lik daha ürün ekleyin", CartRules.AssessShipping(lines, terms).MinimumOrderMessage);
        Assert.Equal(CartPaymentBlock.None, BlockOf([Line(1, 370m), Line(1, 130m)], terms));

        // An empty cart says why first: there is nothing to order.
        Assert.Equal(CartPaymentBlock.NoOrderableItem, BlockOf([], terms));
    }

    [Fact]
    public void ASoldOutPhysicalLine_CarriesNeitherTheMinimumNorTheFee_NorEntersTheThresholdBasis_T349_KK3_KK6()
    {
        ShippingFeeSection terms = new(50m, 500m, 400m, 1);
        CartLine[] lines = [Line(1, 1000m, soldOut: true), Line(1, 100m, type: ProductType.Digital)];

        ShippingAssessment assessment = CartRules.AssessShipping(lines, terms);

        Assert.False(assessment.HasPhysicalItem);
        Assert.Equal(100m, assessment.Basis);
        Assert.Equal(0m, assessment.Fee);
        Assert.Null(assessment.MinimumOrderShortfall);
        Assert.Null(assessment.FreeShippingMessage);
        Assert.Equal(CartPaymentBlock.None, BlockOf(lines, terms));

        // With an orderable physical line the sold-out one still stays out of the basis.
        ShippingAssessment mixed = CartRules.AssessShipping([.. lines, Line(1, 250m)], terms);
        Assert.Equal(350m, mixed.Basis);
        Assert.Equal(50m, mixed.Fee);
        Assert.Equal(50m, mixed.MinimumOrderShortfall);
        Assert.Equal(150m, mixed.RemainingToFreeShipping);
    }

    [Theory]
    [InlineData(100, 100, false)]
    [InlineData(100, 120, true)]
    [InlineData(100, 80, true)]
    public void PriceChanged_IsTrueForAnyDifference_InEitherDirection_KK5(double first, double current, bool expected)
    {
        Assert.Equal(expected, CartRules.PriceChanged((decimal)first, (decimal)current));
    }

    [Fact]
    public void PriceChanged_GoesWhenThePriceReturnsToTheReference_KK5()
    {
        Assert.True(CartRules.PriceChanged(100m, 90m));
        Assert.False(CartRules.PriceChanged(100m, 100m));
    }

    [Theory]
    [InlineData(true, false, true)]
    [InlineData(true, true, false)]
    [InlineData(false, false, false)]
    [InlineData(false, true, false)]
    public void EntersOrder_NeedsTheShowcaseAndStock_KK6(bool onShowcase, bool soldOut, bool expected)
    {
        Assert.Equal(expected, CartRules.EntersOrder(onShowcase, soldOut));
    }

    [Theory]
    [InlineData(CartQuantityProblem.None, true)]
    [InlineData(CartQuantityProblem.NoStock, false)]
    [InlineData(CartQuantityProblem.OverOrderLimit, false)]
    public void EntersOrder_NeedsAQuantityTheStockAndTheLimitHold_T347KK2(CartQuantityProblem problem, bool expected)
    {
        Assert.Equal(expected, CartRules.EntersOrder(true, false, problem));
    }

    [Fact]
    public void ALineOverItsStockOrLimit_DoesNotOpenThePaymentStep_AndIsOutOfTheAmounts_T347KK2()
    {
        CartLine fine = new(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Masa", ProductType.Physical, 1, 50m, false, false, true);
        CartLine overStock = new(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Sandalye", ProductType.Physical, 9, 10m, false, false, CartRules.EntersOrder(true, false, CartQuantityProblem.NoStock), CartQuantityProblem.NoStock);
        CartLine overLimit = new(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Lamba", ProductType.Physical, 3, 10m, false, false, CartRules.EntersOrder(true, false, CartQuantityProblem.OverOrderLimit), CartQuantityProblem.OverOrderLimit, 2);

        Assert.Equal(CartPaymentBlock.NoOrderableItem, BlockOf([overStock, overLimit]));
        Assert.Equal(CartPaymentBlock.None, BlockOf([overStock, fine]));
        Assert.Equal(50m, CartRules.OrderableSubtotal([fine, overStock, overLimit]));
    }

    [Fact]
    public void PaymentBlock_IsNoOrderableItem_ForAnEmptyCart_AndForACartOfSoldOutLines_KK7()
    {
        Assert.Equal(CartPaymentBlock.NoOrderableItem, BlockOf([]));
        Assert.Equal(CartPaymentBlock.NoOrderableItem, BlockOf([Line(1, 50m, soldOut: true), Line(2, 20m, soldOut: true)]));
        Assert.Equal(CartPaymentBlock.None, BlockOf([Line(1, 50m, soldOut: true), Line(2, 20m)]));
    }

    [Fact]
    public void OrderableSubtotal_DoesNotCountTheLinesThatDoNotEnterTheOrder_KK7()
    {
        CartLine[] lines = [Line(2, 50m), Line(3, 10m), Line(4, 1000m, soldOut: true), Line(1, 500m, onShowcase: false)];

        Assert.Equal(130m, CartRules.OrderableSubtotal(lines));
        Assert.Equal(0m, CartRules.OrderableSubtotal([]));
    }

    [Theory]
    [InlineData(null, 1, true)]
    [InlineData(2, 3, true)]
    [InlineData(3, 3, false)]
    [InlineData(5, 3, false)]
    public void MergeShowsNew_OnlyForAVariantOrAQuantityTheGuestCartDidNotHold_KK4(int? guestQuantity, int memberQuantity, bool expected)
    {
        Assert.Equal(expected, CartRules.MergeShowsNew(guestQuantity, memberQuantity));
    }

    [Fact]
    public void CartOwner_DoesNotPrintTheGuestKey_KK1()
    {
        CartOwner guest = CartOwner.ForGuest("secret-cookie-value");
        CartOwner member = CartOwner.ForMember(Guid.NewGuid());

        Assert.DoesNotContain("secret-cookie-value", guest.ToString(), StringComparison.Ordinal);
        Assert.Null(guest.CustomerId);
        Assert.Null(member.GuestCartKey);
        Assert.Throws<ArgumentException>(() => CartOwner.ForMember(Guid.Empty));
        Assert.Throws<ArgumentException>(() => CartOwner.ForGuest(" "));
    }

    [Fact]
    public void CartView_OrderingLines_AreTheLinesThatEnterTheOrder_KK6()
    {
        CartLine sold = Line(1, 10m, soldOut: true);
        CartLine open = Line(1, 10m);
        CartView view = new(Guid.NewGuid(), [sold, open], [], [], BlockOf([sold, open]), CartRules.OrderableSubtotal([sold, open]), CartRules.AssessShipping([sold, open], NoTerms));

        Assert.Equal([open], view.OrderingLines);
        Assert.True(view.CanOpenPayment);
    }
}
