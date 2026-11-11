using Shopfolio.Infrastructure;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Pages.Components;

namespace Shopfolio.UnitTests.Pages;

/// <summary>
/// The cart page's pure parts (T508; 04 5.12; K-772): the texts the page and the frame read, and the state of the way to the payment step - a closed sale first, then the cart's own block.
/// </summary>
public sealed class CartPageModelTests
{
    private static readonly ShippingAssessment NoShipping = new(0m, false, 0m, false, null, null);

    [Fact]
    public void TheMergeMessage_CountsAndNamesTheProducts_AndRefusesAnEmptyList()
    {
        Assert.Equal("Önceki ziyaretinden sepetine 2 ürün eklendi: Lamba, Kupa", CartTexts.MergeAdded(["Lamba", "Kupa"]));
        Assert.Equal("Önceki ziyaretinden sepetine 1 ürün eklendi: Lamba", CartTexts.MergeAdded(["Lamba"]));
        Assert.Throws<ArgumentException>(() => CartTexts.MergeAdded([]));
    }

    [Fact]
    public void TheRemovedLineMessage_NamesTheProduct()
    {
        Assert.Equal("Lamba artık satışta olmadığı için sepetinden çıkarıldı", CartTexts.RemovedFromShowcase("Lamba"));
        Assert.Throws<ArgumentException>(() => CartTexts.RemovedFromShowcase(" "));
    }

    [Fact]
    public void TheQuantityProblem_TellsTheStockWithoutANumber_AndTheLimitAsANumber()
    {
        Assert.Null(CartTexts.QuantityProblem(CartQuantityProblem.None, null));
        Assert.Equal(CartTexts.NoStock, CartTexts.QuantityProblem(CartQuantityProblem.NoStock, null));
        Assert.Equal("Bu üründen bir siparişte en fazla " + DisplayFormat.Number(1200) + " adet alınabilir", CartTexts.QuantityProblem(CartQuantityProblem.OverOrderLimit, 1200));
        Assert.Throws<ArgumentNullException>(() => CartTexts.QuantityProblem(CartQuantityProblem.OverOrderLimit, null));
    }

    [Theory]
    [InlineData(CartPaymentBlock.None, true, CartPaymentBlock.None)]
    [InlineData(CartPaymentBlock.None, false, CartPaymentBlock.SalesClosed)]
    [InlineData(CartPaymentBlock.NoOrderableItem, true, CartPaymentBlock.NoOrderableItem)]
    [InlineData(CartPaymentBlock.NoOrderableItem, false, CartPaymentBlock.SalesClosed)]
    [InlineData(CartPaymentBlock.MinimumOrderAmountNotMet, true, CartPaymentBlock.MinimumOrderAmountNotMet)]
    [InlineData(CartPaymentBlock.MinimumOrderAmountNotMet, false, CartPaymentBlock.SalesClosed)]
    public void ThePaymentStep_TellsAClosedSaleFirst_ThenTheCartsOwnBlock(CartPaymentBlock own, bool salesOpen, CartPaymentBlock expected)
    {
        CartView view = ViewWith(own, lines: 1);

        Assert.Equal(expected, view.PaymentStepBlock(salesOpen));
    }

    [Fact]
    public void AnEmptyCart_HasNoClosedSaleToTell_AndIsEmpty()
    {
        CartView empty = ViewWith(CartPaymentBlock.NoOrderableItem, lines: 0);

        Assert.True(empty.IsEmpty);
        Assert.Equal(CartPaymentBlock.NoOrderableItem, empty.PaymentStepBlock(salesPermitCheckout: false));
        Assert.False(ViewWith(CartPaymentBlock.None, lines: 1).IsEmpty);
    }

    private static CartView ViewWith(CartPaymentBlock block, int lines)
    {
        List<CartLine> list = [.. Enumerable.Range(0, lines).Select(_ => new CartLine(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Lamba", ProductType.Physical, 1, 100m, false, false, true))];
        return new CartView(Guid.NewGuid(), list, [], [], block, 100m * lines, NoShipping);
    }
}
