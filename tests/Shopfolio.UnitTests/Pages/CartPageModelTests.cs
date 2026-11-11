using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Errors;
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

    [Fact]
    public void TheCartsRefusals_AreWrittenWith07sCodes_AndAResultThatIsNoRefusalHasNone_D638()
    {
        Assert.Equal(ErrorCode.StockInsufficient, CartRefusalCodes.CodeOf(CartQuantityProblem.NoStock));
        Assert.Equal(ErrorCode.MaxQuantityPerOrderExceeded, CartRefusalCodes.CodeOf(CartQuantityProblem.OverOrderLimit));
        Assert.Throws<ArgumentOutOfRangeException>(() => CartRefusalCodes.CodeOf(CartQuantityProblem.None));

        Assert.Equal(ErrorCode.StockInsufficient, CartRefusalCodes.CodeOf(CartItemUpdateOutcome.NoStock));
        Assert.Equal(ErrorCode.MaxQuantityPerOrderExceeded, CartRefusalCodes.CodeOf(CartItemUpdateOutcome.OverOrderLimit));
        Assert.Equal(ErrorCode.InvalidQuantity, CartRefusalCodes.CodeOf(CartItemUpdateOutcome.InvalidQuantity));
        Assert.Equal(ErrorCode.NotFound, CartRefusalCodes.CodeOf(CartItemUpdateOutcome.NotInCart));
        Assert.Throws<ArgumentOutOfRangeException>(() => CartRefusalCodes.CodeOf(CartItemUpdateOutcome.Updated));
        Assert.Throws<ArgumentOutOfRangeException>(() => CartRefusalCodes.CodeOf(CartItemUpdateOutcome.TakenOffShowcase));

        Assert.Equal(ErrorCode.StockInsufficient, CartRefusalCodes.CodeOf(CartAddOutcome.NoStock));
        Assert.Equal(ErrorCode.MaxQuantityPerOrderExceeded, CartRefusalCodes.CodeOf(CartAddOutcome.OverOrderLimit));
        Assert.Equal(ErrorCode.DigitalVariantAlreadyInCart, CartRefusalCodes.CodeOf(CartAddOutcome.AlreadyInCart));
        Assert.Equal(ErrorCode.VariantUnavailable, CartRefusalCodes.CodeOf(CartAddOutcome.SoldOut));
        Assert.Equal(ErrorCode.InvalidQuantity, CartRefusalCodes.CodeOf(CartAddOutcome.InvalidQuantity));
        Assert.Equal(ErrorCode.NotFound, CartRefusalCodes.CodeOf(CartAddOutcome.VariantNotFound));
        Assert.Equal(ErrorCode.NotFound, CartRefusalCodes.CodeOf(CartAddOutcome.NotOnSale));
        Assert.Throws<ArgumentOutOfRangeException>(() => CartRefusalCodes.CodeOf(CartAddOutcome.Added));

        // Every refusal of the three result codes has a code: a member added later fails here until it is mapped.
        Assert.All(Enum.GetValues<CartAddOutcome>().Where(o => o != CartAddOutcome.Added), o => _ = CartRefusalCodes.CodeOf(o));
        Assert.All(Enum.GetValues<CartItemUpdateOutcome>().Where(o => o is not (CartItemUpdateOutcome.Updated or CartItemUpdateOutcome.TakenOffShowcase)), o => _ = CartRefusalCodes.CodeOf(o));
    }

    private static CartView ViewWith(CartPaymentBlock block, int lines)
    {
        List<CartLine> list = [.. Enumerable.Range(0, lines).Select(_ => new CartLine(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Lamba", ProductType.Physical, 1, 100m, false, false, true))];
        return new CartView(Guid.NewGuid(), list, [], [], block, 100m * lines, NoShipping);
    }
}
