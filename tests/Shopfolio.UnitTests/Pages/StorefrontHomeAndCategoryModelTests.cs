using Shopfolio.Modules.Catalog.Contracts.Categories;
using Shopfolio.Modules.Catalog.Contracts.Lists;
using Shopfolio.Modules.CorporateContent.Contracts;
using Shopfolio.Pages.Lists;
using Shopfolio.Pages.Storefront;

namespace Shopfolio.UnitTests.Pages;

/// <summary>
/// The models of the home page and the category page (T502 KK1-KK8; 04 5.1, 5.2): the home page's model keeps the module's blocks in the layout's order and the rows of the layout, the corporate block
/// falls back to the brand, the category's path runs from the root, the filter reads the request as the pattern does and an inverted range is no filter, the empty-state line names the way out, the
/// card carries the main image the list item carries, and the style sheet holds the medium class's incomplete-row rule.
/// </summary>
public sealed class StorefrontHomeAndCategoryModelTests
{
    private static readonly Guid ImageId = Guid.Parse("0a1b2c3d-0000-7000-8000-000000000002");

    // The counts of the showcase the medium class cuts: 4 and 7 lose the last card, 5 and 8 lose the last two.
    private static readonly string[] _hiddenCases = ["(4):last-child", "(7):last-child", "(5):last-child", "(8):last-child", "(4):nth-last-child(2)", "(7):nth-last-child(2)"];

    private static ProductListItem Item(string name, Guid? imageId = null, string? alt = null) =>
        new(Guid.NewGuid(), name, name.ToLowerInvariant(), 10m, true, null, imageId, alt);

    private static HomepageView View(HomepageLayout layout, params HomepageBlockKind[] blocks) => new(layout, blocks, null, null, null, null);

    // ---- KK1: the layout ----

    [Fact]
    public void TheHomePage_KeepsTheModulesBlockOrder_AndTheShowcaseRowsFollowTheLayout_KK1()
    {
        HomepageBlockKind[] contentFirst = [HomepageBlockKind.CorporateIntro, HomepageBlockKind.Offerings, HomepageBlockKind.PortfolioItems, HomepageBlockKind.ProductShowcase];
        HomepageBlockKind[] storeFirst = [HomepageBlockKind.ProductShowcase, HomepageBlockKind.CorporateIntro, HomepageBlockKind.Offerings, HomepageBlockKind.PortfolioItems];

        HomePage content = HomePage.Of(View(HomepageLayout.ContentFirst, contentFirst));
        HomePage store = HomePage.Of(View(HomepageLayout.StoreFirst, storeFirst));

        Assert.Equal(contentFirst, content.Blocks);
        Assert.Equal(storeFirst, store.Blocks);
        Assert.True(content.IsCorporateBlockWide);
        Assert.False(store.IsCorporateBlockWide);
        Assert.Equal(1, content.ShowcaseRows);
        Assert.Equal(2, store.ShowcaseRows);
    }

    [Fact]
    public void TheEmptySitePage_IsTheCorporateBlockAlone_WithoutAnIntroduction_KK1()
    {
        Assert.Equal([HomepageBlockKind.CorporateIntro], HomePage.Empty.Blocks);
        Assert.Null(HomePage.Empty.Intro);
        Assert.Null(HomePage.Empty.Showcase);
    }

    // ---- KK2, KK3: the cards of a block and of the showcase, the category links ----

    [Fact]
    public void TheShowcase_BecomesProductCardsInTheListsOrder_WithTheCategoryLinksAsCategoryAddresses_KK2_KK3()
    {
        HomepageShowcase showcase = new(
            "Ürünler",
            [Item("B", ImageId, "Ön yüz"), Item("A")],
            [new MenuCategory(Guid.NewGuid(), null, "Mobilya", "mobilya", 1)]);

        HomePage page = HomePage.Of(new HomepageView(HomepageLayout.ContentFirst, [HomepageBlockKind.ProductShowcase], null, null, null, showcase));

        Assert.Equal(["B", "A"], page.Showcase!.Cards.Select(c => c.Name));
        Assert.Equal("/gorsel/" + ImageId.ToString("D"), page.Showcase.Cards[0].Image!.Path);
        Assert.Equal("Ön yüz", page.Showcase.Cards[0].Image!.AltText);
        Assert.Null(page.Showcase.Cards[1].Image);
        Assert.Equal([new HomeCategoryLink("Mobilya", "/kategori/mobilya")], page.Showcase.CategoryLinks);
    }

    [Fact]
    public void TheCardBlocks_KeepTheirTitleListPathAndMoreMark_AndTheIntroImageNeverHasEmptyText_KK2()
    {
        HomepageCard card = new(Guid.NewGuid(), "Montaj", "Kısa", "/hizmet/montaj", null);
        HomepageView view = new(
            HomepageLayout.ContentFirst,
            [HomepageBlockKind.CorporateIntro, HomepageBlockKind.Offerings],
            new HomepageIntro("Atölyeyiz.", "/hakkimizda", new ContentImage(ImageId, "  ")),
            new HomepageCardBlock("Hizmetlerimiz", "/hizmetler", [card], true),
            null,
            null);

        HomePage page = HomePage.Of(view);

        Assert.Equal("Hizmetlerimiz", page.Offerings!.Title);
        Assert.Equal("/hizmetler", page.Offerings.ListPath);
        Assert.True(page.Offerings.HasMore);
        Assert.Equal(["Montaj"], page.Offerings.Cards.Select(c => c.Title));
        Assert.Null(page.PortfolioItems);
        Assert.Equal(HomePage.IntroImageFallbackText, page.Intro!.Image!.AltText);
    }

    [Fact]
    public void AProductCard_TakesTheMainImageFromTheListItem_AndTheNameWhenTheImageHasNoText_D332()
    {
        ProductCard withText = ProductCards.Of(Item("Masa", ImageId, "Masa görseli"));
        ProductCard withoutText = ProductCards.Of(Item("Masa", ImageId));
        ProductCard none = ProductCards.Of(Item("Masa"));

        Assert.Equal("Masa görseli", withText.Image!.AltText);
        Assert.Equal("Masa", withoutText.Image!.AltText);
        Assert.Null(none.Image);
    }

    // ---- KK5: the path from the root ----

    [Fact]
    public void TheTrail_RunsFromTheRootToTheCategory_InTheTreesOrder_KK5()
    {
        CategoryNode root = new(Guid.NewGuid(), null, "Mobilya", "mobilya", 1, 1, 0);
        CategoryNode middle = new(Guid.NewGuid(), root.Id, "Oturma", "oturma", 2, 1, 0);
        CategoryNode leaf = new(Guid.NewGuid(), middle.Id, "Sandalye", "sandalye", 3, 0, 4);

        IReadOnlyList<CategoryCrumb> trail = CategoryPage.TrailOf(leaf, [leaf, root, middle]);

        Assert.Equal(
            [new CategoryCrumb("Mobilya", "/kategori/mobilya"), new CategoryCrumb("Oturma", "/kategori/oturma"), new CategoryCrumb("Sandalye", "/kategori/sandalye")],
            trail);
        Assert.Equal([new CategoryCrumb("Mobilya", "/kategori/mobilya")], CategoryPage.TrailOf(root, [root]));
    }

    // ---- KK6, KK7: the query ----

    [Fact]
    public void TheQuery_ReadsThePriceBoundsAndTheStockState_AsThePatternDoes_KK6()
    {
        CategoryQuery query = new() { MinPrice = "10", MaxPrice = "20,5", InStock = "true" };

        ProductListFilter filter = query.ToFilter();

        Assert.Equal(10m, filter.MinPrice);
        Assert.Equal(20.5m, filter.MaxPrice);
        Assert.True(filter.InStockOnly);
        Assert.True(query.IsFilterApplied());
    }

    [Theory]
    [InlineData(null, null, null, false)]
    [InlineData("abc", "", "yes", false)]
    [InlineData("5", null, null, true)]
    [InlineData(null, "5", null, true)]
    [InlineData(null, null, "true", true)]
    [InlineData("30", "10", null, false)]
    public void AFilterApplies_OnlyWhenABoundIsAnAmountAndTheRangeIsNotInverted_OrTheStockStateIsOn_KK6_KK7(string? min, string? max, string? stock, bool applied)
    {
        Assert.Equal(applied, new CategoryQuery { MinPrice = min, MaxPrice = max, InStock = stock }.IsFilterApplied());
    }

    [Fact]
    public void AnInvertedRange_IsPassedAsItIs_ForTheModuleToIgnore_AndFailsTheQueryValidation_KK7()
    {
        CategoryQuery query = new() { MinPrice = "40", MaxPrice = "20" };

        Assert.True(query.ToFilter().IsPriceRangeInverted);
        System.ComponentModel.DataAnnotations.ValidationResult result = Assert.Single(query.Validate(new System.ComponentModel.DataAnnotations.ValidationContext(query)));
        Assert.Equal([nameof(CategoryQuery.MinPrice)], result.MemberNames);
        Assert.Empty(new CategoryQuery { MinPrice = "20", MaxPrice = "40" }.Validate(new System.ComponentModel.DataAnnotations.ValidationContext(query)));
    }

    // ---- KK8: the empty-state line ----

    [Fact]
    public void TheEmptyState_IsNoneWithCards_TheHomePageForAnEmptyCategory_AndTheFiltersWayOutForAnEmptyResult_KK8()
    {
        Assert.Null(CategoryPage.EmptyStateOf(3, true, "/kategori/mobilya"));

        EmptyState emptyCategory = CategoryPage.EmptyStateOf(0, false, "/kategori/mobilya")!;
        Assert.Equal(CategoryPage.EmptyCategoryText, emptyCategory.Message);
        Assert.Equal("Ana sayfaya dön", emptyCategory.ActionText);
        Assert.Equal("/", emptyCategory.ActionPath);

        EmptyState emptyFilter = CategoryPage.EmptyStateOf(0, true, "/kategori/mobilya")!;
        Assert.Equal(CategoryPage.EmptyFilterText, emptyFilter.Message);
        Assert.Equal("Süzgeci kaldır", emptyFilter.ActionText);
        Assert.Equal("/kategori/mobilya", emptyFilter.ActionPath);
    }

    // ---- KK2: the style sheet's rule of the medium class ----

    [Fact]
    public void TheStyleSheet_HidesTheIncompleteShowcaseRow_OnTheMediumClassOnly_KK2()
    {
        string css = File.ReadAllText(StyleSheetPath("storefront.css"));
        string[] selectors = [.. _hiddenCases.Select(s => ".home-showcase .card-grid__item:nth-child" + s)];

        // The medium limit starts the rule (09 16.1: only min-width conditions) and the wide limit shows the cards again.
        string hide = BlockOf(css, "@media (min-width: 768px) {\n    .home-showcase");
        string show = BlockOf(css, "@media (min-width: 1024px) {\n    .home-showcase");

        foreach (string selector in selectors)
        {
            Assert.Contains(selector, hide, StringComparison.Ordinal);
            Assert.Contains(selector, show, StringComparison.Ordinal);
        }

        Assert.Contains("display: none;", hide, StringComparison.Ordinal);
        Assert.Contains("display: list-item;", show, StringComparison.Ordinal);

        // Only the showcase, never the content blocks' or the category list's grid (04 5.1.13).
        Assert.DoesNotContain(".home-cards", hide + show, StringComparison.Ordinal);
        Assert.DoesNotContain(".category-layout", hide + show, StringComparison.Ordinal);
    }

    [Fact]
    public void TheStyleSheet_PutsTheBandLayoutOfTheCorporateBlockSideBySide_FromTheMediumClassUp_KK1()
    {
        string css = File.ReadAllText(StyleSheetPath("storefront.css"));

        // Only a min-width condition starts the side-by-side rule (09 16.1); outside it the grid is the single narrow column (04 5.1.13).
        string medium = BlockOf(css, "@media (min-width: 768px) {\n    .home-intro--wide");

        Assert.Contains(".home-intro--band", medium, StringComparison.Ordinal);
        Assert.Contains(".home-intro--wide", medium, StringComparison.Ordinal);
        Assert.Contains("grid-template-columns: repeat(2, minmax(0, 1fr));", medium, StringComparison.Ordinal);
    }

    private static string BlockOf(string css, string start)
    {
        string text = css.Replace("\r\n", "\n", StringComparison.Ordinal);
        int from = text.IndexOf(start, StringComparison.Ordinal);
        Assert.True(from >= 0, "The rule is not in the style sheet: " + start);
        return text[from..text.IndexOf("\n}\n", from, StringComparison.Ordinal)];
    }

    private static string StyleSheetPath(string name)
    {
        DirectoryInfo? directory = new(AppContext.BaseDirectory);
        while (directory is not null)
        {
            string candidate = Path.Combine(directory.FullName, "src", "Shopfolio.Web", "wwwroot", "css", name);
            if (File.Exists(candidate))
            {
                return candidate;
            }

            directory = directory.Parent;
        }

        throw new FileNotFoundException("The style sheet is not found above the test output: " + name);
    }
}
