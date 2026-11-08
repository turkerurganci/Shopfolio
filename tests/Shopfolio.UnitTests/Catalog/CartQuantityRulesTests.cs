using Shopfolio.Modules.Catalog;
using Shopfolio.Modules.Catalog.Contracts;

namespace Shopfolio.UnitTests.Catalog;

/// <summary>
/// The cart's quantity rules as pure functions (T347 KK1-KK4; 02 3.16.8-3.16.10): a quantity above the stock less the reserved units is NoStock; the quantities of a product's variants
/// together are held to the firm's limit, which wins over the stock; a line that breaks the stock or the showcase does not count into the total; an empty limit leaves only the stock.
/// </summary>
public sealed class CartQuantityRulesTests
{
    private static readonly Guid ProductA = Guid.NewGuid();

    private static readonly Guid ProductB = Guid.NewGuid();

    private static QuantityInput Line(int quantity, int? stock = 10, int reserved = 0, int? limit = null, Guid? product = null, bool onShowcase = true)
        => new(Guid.NewGuid(), product ?? ProductA, quantity, stock, reserved, limit, onShowcase);

    [Theory]
    [InlineData(10, 0, 10, false)]
    [InlineData(10, 0, 11, true)]
    [InlineData(10, 4, 6, false)]
    [InlineData(10, 4, 7, true)]
    public void ExceedsStock_ReadsTheStockLessTheReservedUnits_KK1(int stock, int reserved, int quantity, bool expected)
    {
        Assert.Equal(expected, CartQuantityRules.ExceedsStock(stock, reserved, quantity));
    }

    [Fact]
    public void ExceedsStock_NeverHoldsBack_AnUntrackedCount_KK1()
    {
        Assert.False(CartQuantityRules.ExceedsStock(null, 0, int.MaxValue));
    }

    [Fact]
    public void Add_AboveTheStockLessTheReserved_IsNoStock_AndCarriesNoNumber_KK1()
    {
        QuantityInput candidate = Line(7, stock: 10, reserved: 4);

        CartQuantityVerdict verdict = CartQuantityRules.JudgeAdd(candidate, []);

        Assert.Equal(CartQuantityProblem.NoStock, verdict.Problem);
        Assert.Null(verdict.OrderLimit);
        Assert.Equal(CartQuantityProblem.None, CartQuantityRules.JudgeAdd(Line(6, stock: 10, reserved: 4), []).Problem);
    }

    [Fact]
    public void Add_TheServiceCapacity_FollowsTheSameRule_KK2()
    {
        // A service keeps its capacity in the same field; a service without capacity has none and is never held back.
        Assert.Equal(CartQuantityProblem.NoStock, CartQuantityRules.JudgeAdd(Line(3, stock: 2), []).Problem);
        Assert.Equal(CartQuantityProblem.None, CartQuantityRules.JudgeAdd(Line(300, stock: null), []).Problem);
    }

    [Fact]
    public void Judge_ALineAboveTheStock_StaysInTheCartAsNoStock_AndTheQuantityIsNotLowered_KK2()
    {
        QuantityInput merged = Line(8, stock: 5);
        QuantityInput fine = Line(2, stock: 5);

        IReadOnlyList<CartQuantityVerdict> verdicts = CartQuantityRules.Judge([merged, fine]);

        Assert.Equal(CartQuantityProblem.NoStock, verdicts[0].Problem);
        Assert.Equal(CartQuantityProblem.None, verdicts[1].Problem);
        Assert.Equal(8, merged.Quantity);
    }

    [Fact]
    public void Judge_ASoldOutOrOffShowcaseLine_HasItsOwnMark_NotAQuantityProblem_KK2()
    {
        IReadOnlyList<CartQuantityVerdict> verdicts = CartQuantityRules.Judge([Line(3, stock: 4, reserved: 4), Line(3, onShowcase: false)]);

        Assert.All(verdicts, v => Assert.Equal(CartQuantityProblem.None, v.Problem));
    }

    [Fact]
    public void Add_TheLimitCoversTheSumOfAllVariants_KK3()
    {
        QuantityInput red = Line(1, limit: 2);
        QuantityInput blue = Line(1, limit: 2);

        // 1 red + 1 blue is within 2; adding one more red makes 2 red + 1 blue = 3 > 2.
        Assert.Equal(CartQuantityProblem.None, CartQuantityRules.JudgeAdd(blue, [red]).Problem);
        QuantityInput twoRed = red with { Quantity = 2 };
        CartQuantityVerdict verdict = CartQuantityRules.JudgeAdd(twoRed, [blue]);
        Assert.Equal(CartQuantityProblem.OverOrderLimit, verdict.Problem);
        Assert.Equal(2, verdict.OrderLimit);
    }

    [Fact]
    public void Add_WhenTheLimitAndTheStockAreBothExceeded_OnlyTheLimitIsTold_KK3()
    {
        QuantityInput candidate = Line(9, stock: 5, limit: 2);

        CartQuantityVerdict verdict = CartQuantityRules.JudgeAdd(candidate, []);

        Assert.Equal(CartQuantityProblem.OverOrderLimit, verdict.Problem);
        Assert.Equal(2, verdict.OrderLimit);
    }

    [Fact]
    public void Add_WithAnEmptyLimit_TheOnlyLimitIsTheStock_KK3()
    {
        Assert.Equal(CartQuantityProblem.None, CartQuantityRules.JudgeAdd(Line(10, stock: 10, limit: null), []).Problem);
        Assert.Equal(CartQuantityProblem.NoStock, CartQuantityRules.JudgeAdd(Line(11, stock: 10, limit: null), []).Problem);
    }

    [Fact]
    public void Add_TheLimitOfAnotherProduct_DoesNotCount_KK3()
    {
        QuantityInput other = Line(5, limit: 5, product: ProductB);

        Assert.Equal(CartQuantityProblem.None, CartQuantityRules.JudgeAdd(Line(2, limit: 2), [other]).Problem);
    }

    [Fact]
    public void Add_TheTotalDoesNotOverflow_WhenTheQuantitiesAreHuge_KK3()
    {
        QuantityInput other = Line(int.MaxValue, stock: null, limit: 5);

        Assert.Equal(CartQuantityProblem.OverOrderLimit, CartQuantityRules.JudgeAdd(Line(int.MaxValue, stock: null, limit: 5), [other]).Problem);
    }

    [Fact]
    public void Judge_ALimitLoweredLater_KeepsTheLinesInTheCart_AndOutOfTheOrder_KK4()
    {
        QuantityInput red = Line(2, limit: 2);
        QuantityInput blue = Line(1, limit: 2);

        IReadOnlyList<CartQuantityVerdict> verdicts = CartQuantityRules.Judge([red, blue]);

        Assert.All(verdicts, v =>
        {
            Assert.Equal(CartQuantityProblem.OverOrderLimit, v.Problem);
            Assert.Equal(2, v.OrderLimit);
        });
    }

    [Fact]
    public void Judge_ALineThatDoesNotEnterTheOrder_IsNotCounted_IntoTheLimitsTotal_KK4()
    {
        // The red line is over its stock and the blue one is sold out: neither counts, so the 2 that do enter are within the limit of 2.
        QuantityInput overStock = Line(9, stock: 3, limit: 2);
        QuantityInput soldOut = Line(4, stock: 1, reserved: 1, limit: 2);
        QuantityInput fine = Line(2, stock: 10, limit: 2);

        IReadOnlyList<CartQuantityVerdict> verdicts = CartQuantityRules.Judge([overStock, soldOut, fine]);

        Assert.Equal(CartQuantityProblem.NoStock, verdicts[0].Problem);
        Assert.Equal(CartQuantityProblem.None, verdicts[1].Problem);
        Assert.Equal(CartQuantityProblem.None, verdicts[2].Problem);
    }

    [Fact]
    public void Judge_TheTotalIsKeptPerProduct_KK4()
    {
        IReadOnlyList<CartQuantityVerdict> verdicts = CartQuantityRules.Judge([Line(3, limit: 2), Line(1, limit: 2), Line(4, limit: 5, product: ProductB)]);

        Assert.Equal(CartQuantityProblem.OverOrderLimit, verdicts[0].Problem);
        Assert.Equal(CartQuantityProblem.OverOrderLimit, verdicts[1].Problem);
        Assert.Equal(CartQuantityProblem.None, verdicts[2].Problem);
    }

    [Fact]
    public void TheVerdictsAndTheLines_CarryNoStockNumber_KK1()
    {
        string[] forbidden = ["Stock", "Available", "Remaining", "Reserved", "Capacity"];
        Type[] carriers = [typeof(CartQuantityVerdict), typeof(CartQuantityLine), typeof(Shopfolio.Modules.Ordering.Contracts.CartLine), typeof(Shopfolio.Modules.Ordering.Contracts.CartAddResult)];

        foreach (Type carrier in carriers)
        {
            Assert.DoesNotContain(carrier.GetProperties(), p => forbidden.Any(f => p.Name.Contains(f, StringComparison.Ordinal)));
        }
    }
}
