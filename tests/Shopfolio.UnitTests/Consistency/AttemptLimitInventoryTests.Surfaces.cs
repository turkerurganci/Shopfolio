using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Security;
using Shopfolio.Infrastructure.Values;

namespace Shopfolio.UnitTests.Consistency;

/// <summary>
/// T377 criterion 7, KK7 (02 8.2.5): the cart's item count and the stock lookup have no attempt limit of their own. The nine limits of the document are the nine of the code,
/// none of them is named for a cart item or the stock, the cart axis belongs to the coupon limit alone, and no attempt constant is named for the stock or the item count.
/// </summary>
public sealed partial class AttemptLimitInventoryTests
{
    private static readonly string[] _forbiddenWords = ["stok", "kalem", "stock", "cartitem", "itemcount", "quantity"];

    [Fact]
    public void NoRowOfTheDocumentsTable_IsAboutTheCartsItemCountOrTheStockLookup_KK7()
    {
        List<(int Number, string Name, string Axis)> rows = TableRows();

        Assert.Equal(9, rows.Count);
        Assert.All(rows, r => Assert.DoesNotContain(_forbiddenWords, w => r.Name.Contains(w, StringComparison.OrdinalIgnoreCase)));
    }

    [Fact]
    public void NoLimitOfTheInventory_IsNamedForTheCartsItemCountOrTheStockLookup_AndTheCartAxisIsTheCouponLimitsAlone_KK7()
    {
        Assert.Equal(9, AttemptLimitInventory.All.Count);
        Assert.All(AttemptLimitInventory.All, e => Assert.DoesNotContain(_forbiddenWords, w => e.Name.Contains(w, StringComparison.OrdinalIgnoreCase)));
        AttemptLimitEntry cartAxis = Assert.Single(AttemptLimitInventory.All, e => e.Axes.Contains(AttemptAxis.Cart));
        Assert.Equal(AttemptLimit.L6, cartAxis.Limit);
    }

    [Fact]
    public void NoAttemptConstantOfTheProduct_IsNamedForTheCartsItemCountOrTheStockLookup_KK7()
    {
        IEnumerable<string> names = typeof(ProductConstants).GetFields().Select(f => f.Name)
            .Where(n => n.Contains("Attempt", StringComparison.Ordinal) || n.Contains("Limit", StringComparison.Ordinal));

        Assert.All(names, n => Assert.DoesNotContain(_forbiddenWords, w => n.Contains(w, StringComparison.OrdinalIgnoreCase)));
    }
}
