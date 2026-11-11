using Shopfolio.Infrastructure.Routing;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Catalog.Contracts.Categories;
using Shopfolio.Pages.Frame;
using Shopfolio.Pages.Storefront;

namespace Shopfolio.UnitTests.Pages;

/// <summary>
/// The archived product's page rules that need no host (T549 KK2, KK3; 04 5.5.3, 5.5.4; 07 4.5.1): the fixed line, the ways out - home and the home page's own first-level categories - and the buy
/// area that stays empty. The page itself is checked on the opened application (<c>StorefrontSearchAndProductTests</c>).
/// </summary>
public sealed class ArchivedProductPageTests
{
    private static MenuCategory Category(string name, string slug, int level, Guid? parent = null) => new(Guid.NewGuid(), parent, name, slug, level);

    [Fact]
    public void TheLine_IsTheWordsOf04_5_5_3()
    {
        Assert.Equal("Bu ürün artık satılmıyor", ProductView.NoLongerSoldText);
    }

    [Fact]
    public void ReturnLinksOf_IsEmpty_WhenThereIsNoMenuOrNoCategory_T549_KK2()
    {
        Assert.Empty(ProductViews.ReturnLinksOf(null));
        Assert.Empty(ProductViews.ReturnLinksOf([]));
    }

    [Fact]
    public void ReturnLinksOf_AreTheFirstLevelCategories_WithTheirPageAddresses_AndNothingBelowThem_T549_KK2()
    {
        MenuCategory furniture = Category("Mobilya", "mobilya", 1);
        MenuCategory table = Category("Masalar", "masalar", 2, furniture.Id);
        MenuCategory lamps = Category("Aydınlatma", "aydinlatma", 1);

        IReadOnlyList<MenuCategoryNode> links = ProductViews.ReturnLinksOf([furniture, table, lamps]);

        Assert.Equal(["Mobilya", "Aydınlatma"], links.Select(l => l.Name));
        Assert.Equal(
            [AddressRules.RecordAddress(AddressRules.CategoryPrefix, "mobilya"), AddressRules.RecordAddress(AddressRules.CategoryPrefix, "aydinlatma")],
            links.Select(l => l.Path));
    }
}
