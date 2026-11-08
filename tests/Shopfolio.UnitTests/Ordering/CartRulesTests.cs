using Shopfolio.Modules.Ordering;
using Shopfolio.Modules.Ordering.Contracts;

namespace Shopfolio.UnitTests.Ordering;

/// <summary>The cart's pure display and order-entry rules (T346; 02 3.16.3-3.16.7).</summary>
public sealed class CartRulesTests
{
    private static CartLine Line(int quantity, decimal unitPrice, bool soldOut = false, bool onShowcase = true)
        => new(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Masa", quantity, unitPrice, false, soldOut, CartRules.EntersOrder(onShowcase, soldOut));

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

    [Fact]
    public void PaymentBlock_IsNoOrderableItem_ForAnEmptyCart_AndForACartOfSoldOutLines_KK7()
    {
        Assert.Equal(CartPaymentBlock.NoOrderableItem, CartRules.PaymentBlockOf([]));
        Assert.Equal(CartPaymentBlock.NoOrderableItem, CartRules.PaymentBlockOf([Line(1, 50m, soldOut: true), Line(2, 20m, soldOut: true)]));
        Assert.Equal(CartPaymentBlock.None, CartRules.PaymentBlockOf([Line(1, 50m, soldOut: true), Line(2, 20m)]));
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
        CartView view = new(Guid.NewGuid(), [sold, open], [], [], CartRules.PaymentBlockOf([sold, open]), CartRules.OrderableSubtotal([sold, open]));

        Assert.Equal([open], view.OrderingLines);
        Assert.True(view.CanOpenPayment);
    }
}
