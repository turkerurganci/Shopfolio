using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Routing;
using Shopfolio.Modules.Catalog.Contracts.Categories;
using Shopfolio.Modules.CorporateContent;
using Shopfolio.Modules.CorporateContent.Contracts;
using Shopfolio.Modules.Settings.Contracts;

namespace Shopfolio.UnitTests.CorporateContent;

/// <summary>
/// The rules of the home page and the top menu that need no database (T361 criteria 1-6; KK1-KK6; 02 3.28.1-3.28.6; 04 2.2.3, 5.1): the order each layout gives, the hiding of the types
/// and of the products part, the names, the general pages, the numbers of the blocks, and the shape of the data model that has no banner, no campaign picture and no home page mark on a product.
/// </summary>
public sealed class StorefrontMenuAndHomepageTests
{
    private static readonly MenuNames Names = new("Ürünler", "Hizmetlerimiz", "Referanslarımız", "Hakkımızda", "SSS", "İletişim");

    private static readonly CatalogMenu NoProducts = new(false, []);

    private static MenuStructure Structure(bool offerings = false, bool portfolio = false, bool about = false, bool faq = false, CatalogMenu? catalog = null, params GeneralPageLink[] pages) =>
        new(offerings, portfolio, about, faq, catalog ?? NoProducts, pages);

    private static StorefrontMenu Compose(HomepageLayout layout, MenuStructure structure, MenuNames? names = null) =>
        StorefrontMenuReader.Compose(new HomepageSection(layout, names ?? Names, 1), structure);

    private static CatalogMenu WithProducts(params MenuCategory[] categories) => new(true, categories);

    // ---- KK1: two layouts, the same blocks ----

    [Theory]
    [InlineData(HomepageLayout.ContentFirst, true, true, true, new[] { HomepageBlockKind.CorporateIntro, HomepageBlockKind.Offerings, HomepageBlockKind.PortfolioItems, HomepageBlockKind.ProductShowcase })]
    [InlineData(HomepageLayout.StoreFirst, true, true, true, new[] { HomepageBlockKind.ProductShowcase, HomepageBlockKind.CorporateIntro, HomepageBlockKind.Offerings, HomepageBlockKind.PortfolioItems })]
    [InlineData(HomepageLayout.ContentFirst, false, true, true, new[] { HomepageBlockKind.CorporateIntro, HomepageBlockKind.PortfolioItems, HomepageBlockKind.ProductShowcase })]
    [InlineData(HomepageLayout.StoreFirst, true, false, false, new[] { HomepageBlockKind.CorporateIntro, HomepageBlockKind.Offerings })]
    [InlineData(HomepageLayout.ContentFirst, false, false, false, new[] { HomepageBlockKind.CorporateIntro })]
    [InlineData(HomepageLayout.StoreFirst, false, false, false, new[] { HomepageBlockKind.CorporateIntro })]
    public void TheBlocks_FollowTheLayout_AndABlockWithNothingToShowIsLeftOut_AndTheCorporateBlockStays_KK1(
        HomepageLayout layout,
        bool offerings,
        bool portfolio,
        bool showcase,
        HomepageBlockKind[] expected)
    {
        Assert.Equal(expected, HomepageReader.OrderOf(layout, offerings, portfolio, showcase));
    }

    [Fact]
    public void TheShowcase_ShowsFourProductsInTheContentFirstLayout_AndEightInTheStoreFirstLayout_KK1()
    {
        Assert.Equal(4, HomepageRules.ShowcaseProducts(HomepageLayout.ContentFirst));
        Assert.Equal(8, HomepageRules.ShowcaseProducts(HomepageLayout.StoreFirst));
        Assert.Equal(3, HomepageRules.CardsPerBlock);
        foreach (HomepageLayout layout in Enum.GetValues<HomepageLayout>())
        {
            Assert.InRange(HomepageRules.ShowcaseProducts(layout), 1, 8);
        }
    }

    [Fact]
    public void TheLayoutHasTwoValues_AndTheBlockListIsClosed_KK1()
    {
        Assert.Equal(["ContentFirst", "StoreFirst"], Enum.GetNames<HomepageLayout>());
        Assert.Equal(["CorporateIntro", "Offerings", "PortfolioItems", "ProductShowcase"], Enum.GetNames<HomepageBlockKind>());
    }

    [Fact]
    public void TheDataModel_HasNoBannerCampaignPictureOrHomepageMarkOnAProduct_AndTheSettingsHoldOnlyTheLayout_KK1()
    {
        DbContextOptionsBuilder<AppDbContext> builder = new();
        AppDbContext.Configure(builder, "Host=127.0.0.1;Database=never_opened");
        using AppDbContext context = new(builder.Options);
        string[] forbidden = ["banner", "slider", "campaign", "kampanya"];
        string[] productTables = ["products", "variants", "product_categories", "product_images", "options", "option_values", "categories"];

        IEnumerable<IEntityType> entities = context.Model.GetEntityTypes();
        foreach (IEntityType entity in entities)
        {
            string table = entity.GetTableName() ?? string.Empty;
            Assert.DoesNotContain(forbidden, word => table.Contains(word, StringComparison.OrdinalIgnoreCase) && !table.StartsWith("coupon", StringComparison.Ordinal));
        }

        List<IEntityType> product = [.. entities.Where(e => e.GetTableName() is { } t && productTables.Contains(t))];
        Assert.Equal(productTables.Length, product.Select(e => e.GetTableName()).Distinct().Count());
        foreach (IEntityType entity in product)
        {
            foreach (IProperty property in entity.GetProperties())
            {
                Assert.DoesNotContain("Homepage", property.Name, StringComparison.OrdinalIgnoreCase);
                Assert.DoesNotContain(forbidden, word => property.Name.Contains(word, StringComparison.OrdinalIgnoreCase));
            }
        }

        // The flag lives on the two content types only (02 3.28.3).
        List<string> flagged = [.. entities.Where(e => e.GetProperties().Any(p => p.Name == "IsShownOnHomepage")).Select(e => e.GetTableName()!)];
        Assert.Equal(["offerings", "portfolio_items"], flagged.Order(StringComparer.Ordinal));
    }

    // ---- KK4: the menu skeleton, the order, the names ----

    [Fact]
    public void TheContentFirstMenu_PutsTheContentTypesFirstWithTheAboutPageAtTheirHead_ThenProducts_ThenTheMenuPages_ContactLast_KK4()
    {
        GeneralPageLink page = new(Guid.NewGuid(), "Kampanya", "/sayfa/kampanya", IsInMenu: true);
        StorefrontMenu menu = Compose(HomepageLayout.ContentFirst, Structure(true, true, true, true, WithProducts(), page));

        Assert.Equal(
            [MenuItemKind.AboutPage, MenuItemKind.Offerings, MenuItemKind.PortfolioItems, MenuItemKind.FaqItems, MenuItemKind.Products, MenuItemKind.GeneralPage, MenuItemKind.Contact],
            menu.Items.Select(i => i.Kind));
        Assert.All(menu.Items, item => Assert.True(item.IsVisible));
    }

    [Fact]
    public void TheStoreFirstMenu_PutsProductsFirst_ThenTheContentTypes_ThenTheMenuPages_ContactLast_KK4()
    {
        GeneralPageLink page = new(Guid.NewGuid(), "Kampanya", "/sayfa/kampanya", IsInMenu: true);
        StorefrontMenu menu = Compose(HomepageLayout.StoreFirst, Structure(true, true, true, true, WithProducts(), page));

        Assert.Equal(
            [MenuItemKind.Products, MenuItemKind.Offerings, MenuItemKind.PortfolioItems, MenuItemKind.AboutPage, MenuItemKind.FaqItems, MenuItemKind.GeneralPage, MenuItemKind.Contact],
            menu.Items.Select(i => i.Kind));
    }

    [Theory]
    [InlineData(HomepageLayout.ContentFirst)]
    [InlineData(HomepageLayout.StoreFirst)]
    public void AFreshSite_ShowsOnlyContact_AndEveryOtherItemCarriesItsReason_KK4(HomepageLayout layout)
    {
        StorefrontMenu menu = Compose(layout, Structure());

        Assert.Equal([MenuItemKind.Contact], menu.VisibleItems.Select(i => i.Kind));
        MenuItem[] hidden = [.. menu.Items.Where(i => !i.IsVisible)];
        Assert.Equal(5, hidden.Length);
        Assert.All(hidden.Where(i => i.Kind != MenuItemKind.Products), i => Assert.Equal(MenuHiddenReason.NoPublishedRecord, i.HiddenReason));
        Assert.Equal(MenuHiddenReason.NoPublishedProduct, menu.Items.Single(i => i.Kind == MenuItemKind.Products).HiddenReason);
        Assert.Equal("/iletisim", menu.VisibleItems.Single().Path);
    }

    [Theory]
    [InlineData(true, false, false, false, MenuItemKind.Offerings)]
    [InlineData(false, true, false, false, MenuItemKind.PortfolioItems)]
    [InlineData(false, false, true, false, MenuItemKind.AboutPage)]
    [InlineData(false, false, false, true, MenuItemKind.FaqItems)]
    public void ATypeShowsExactlyWhenItHasAPublishedRecord_KK4(bool offerings, bool portfolio, bool about, bool faq, MenuItemKind shown)
    {
        StorefrontMenu menu = Compose(HomepageLayout.ContentFirst, Structure(offerings, portfolio, about, faq));

        Assert.Equal([shown, MenuItemKind.Contact], menu.VisibleItems.Select(i => i.Kind));
        Assert.Null(menu.Items.Single(i => i.Kind == shown).HiddenReason);
    }

    [Fact]
    public void TheFirmsNames_AreTheNamesOfTheItems_AndTheContactNameChangesToo_KK4()
    {
        MenuNames renamed = new("Mağaza", "Hizmetler", "Projelerimiz", "Biz kimiz", "Sık sorulanlar", "Bize yazın");
        StorefrontMenu menu = Compose(HomepageLayout.StoreFirst, Structure(true, true, true, true, WithProducts()), renamed);

        Assert.Equal(["Mağaza", "Hizmetler", "Projelerimiz", "Biz kimiz", "Sık sorulanlar", "Bize yazın"], menu.Items.Select(i => i.Name));
    }

    [Fact]
    public void TheFixedItemsPaths_AreTheFixedPagesOfTheAddressRules_AndProductsHasNoPage_KK4()
    {
        StorefrontMenu menu = Compose(HomepageLayout.StoreFirst, Structure(true, true, true, true, WithProducts()));

        Assert.Null(menu.Items.Single(i => i.Kind == MenuItemKind.Products).Path);
        foreach (MenuItem item in menu.Items.Where(i => i.Kind != MenuItemKind.Products))
        {
            Assert.Contains(item.Path!.TrimStart('/'), AddressRules.FixedPages);
        }
    }

    [Fact]
    public void TheMenuHasNoWayToAddRemoveOrReorderAnItem_KK4()
    {
        Assert.Equal(
            ["Products", "Offerings", "PortfolioItems", "AboutPage", "FaqItems", "GeneralPage", "Contact"],
            Enum.GetNames<MenuItemKind>());
        string[] editorWords = ["Add", "Remove", "Reorder", "Move", "Editor", "Save"];
        foreach (System.Reflection.MethodInfo method in typeof(ICorporateContentModule).GetMethods())
        {
            Assert.DoesNotContain(editorWords, word => method.Name.Contains("Menu", StringComparison.Ordinal) && method.Name.Contains(word, StringComparison.Ordinal));
        }
    }

    // ---- KK5: general pages ----

    [Fact]
    public void AGeneralPage_IsInTheFooterAlways_AndInTheTopMenuOnlyWhenMarked_KK5()
    {
        GeneralPageLink inMenu = new(Guid.NewGuid(), "Kampanya", "/sayfa/kampanya", IsInMenu: true);
        GeneralPageLink footerOnly = new(Guid.NewGuid(), "Kargo", "/sayfa/kargo", IsInMenu: false);
        StorefrontMenu menu = Compose(HomepageLayout.ContentFirst, Structure(pages: [inMenu, footerOnly]));

        Assert.Equal([inMenu, footerOnly], menu.FooterPages);
        Assert.Equal(["Kampanya", "İletişim"], menu.VisibleItems.Select(i => i.Name));
        Assert.Equal("/sayfa/kampanya", menu.VisibleItems.First().Path);
    }

    // ---- KK6: the categories ----

    [Fact]
    public void TheCategories_AreTheCatalogsList_WhileProductsShow_AndNoneWhileTheyAreHidden_KK6()
    {
        MenuCategory root = new(Guid.NewGuid(), null, "Mobilya", "mobilya", 1);
        MenuCategory child = new(Guid.NewGuid(), root.Id, "Masalar", "masalar", 2);

        StorefrontMenu shown = Compose(HomepageLayout.ContentFirst, Structure(catalog: WithProducts(root, child)));
        StorefrontMenu hidden = Compose(HomepageLayout.ContentFirst, Structure(catalog: new CatalogMenu(false, [root, child])));

        Assert.Equal([root, child], shown.Categories);
        Assert.Empty(hidden.Categories);
    }
}
