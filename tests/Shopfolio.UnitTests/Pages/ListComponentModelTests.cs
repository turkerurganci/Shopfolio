using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Lists;
using Shopfolio.Modules.Catalog.Contracts.Lists;
using Shopfolio.Modules.Catalog.Contracts.Pricing;
using Shopfolio.Modules.CorporateContent.Contracts;
using Shopfolio.Pages.Lists;

namespace Shopfolio.UnitTests.Pages;

/// <summary>
/// The models of the list components (T545 KK1-KK10; 04 2.11, 2.13): the product card and the content card carry exactly the fields the table gives, the discount line declares the reference price and the
/// dates, the page navigation is numbered and absent for one page, the page sizes are the table's, the search belongs to the listed lists only, and the panel table is never uneven.
/// </summary>
public sealed class ListComponentModelTests
{
    private static readonly Guid ImageId = Guid.Parse("0a1b2c3d-0000-7000-8000-000000000001");

    private static ProductListItem Item(string name = "Ampul", string slug = "ampul", decimal? price = 120m, bool purchasable = true, DiscountLine? discount = null) =>
        new(Guid.NewGuid(), name, slug, price, purchasable, discount);

    // ---- KK1: the fields of a product card, and the two it does not have ----

    [Fact]
    public void AProductCard_CarriesTheMainImageTheNameAndTheVatIncludedPrice_AndTheProductPageAddress_KK1()
    {
        ProductCard card = ProductCards.Of(Item(price: 1200m), ProductCards.ImageOf(ImageId, "Ampul — görsel 1 / 2"));

        Assert.Equal("Ampul", card.Name);
        Assert.Equal("/urun/ampul", card.Path);
        Assert.Equal("/gorsel/" + ImageId.ToString("D"), card.Image!.Path);
        Assert.Equal("Ampul — görsel 1 / 2", card.Image.AltText);
        Assert.Equal("1.200 TL", card.PriceText);
        Assert.False(card.IsSoldOut);
        Assert.Null(card.Discount);
    }

    [Fact]
    public void TheCardRecords_HaveNoCartAndNoStockCountAndNoRatingMember_KK1()
    {
        string[] members = [.. typeof(ProductCard).GetProperties().Select(p => p.Name), .. typeof(ProductCardDiscount).GetProperties().Select(p => p.Name), .. typeof(ContentCard).GetProperties().Select(p => p.Name)];

        Assert.All(members, name =>
        {
            Assert.DoesNotContain("Cart", name, StringComparison.Ordinal);
            Assert.DoesNotContain("Stock", name, StringComparison.Ordinal);
            Assert.DoesNotContain("Quantity", name, StringComparison.Ordinal);
            Assert.DoesNotContain("Rating", name, StringComparison.Ordinal);
            Assert.DoesNotContain("Review", name, StringComparison.Ordinal);
        });
    }

    [Fact]
    public void AProductWithoutAPublishedVariant_HasACardWithoutAPrice_KK1() =>
        Assert.Null(ProductCards.Of(Item(price: null), null).PriceText);

    [Theory]
    [InlineData("a b", "a%20b")]
    [InlineData("çay", "%C3%A7ay")]
    public void TheProductPath_EscapesTheSlugOnce_KK1(string slug, string expected) => Assert.Equal("/urun/" + expected, ProductCards.ProductPath(slug));

    // ---- KK2: one card per product, whatever its variants ----

    [Fact]
    public void TheCards_AreOnePerListItem_InTheListsOrder_KK2()
    {
        ProductListItem[] items = [Item("Bir", "bir"), Item("İki", "iki"), Item("Üç", "uc")];

        IReadOnlyList<ProductCard> cards = ProductCards.Of(items, _ => null);

        Assert.Equal(["Bir", "İki", "Üç"], cards.Select(c => c.Name).ToArray());
    }

    // ---- KK3: the discount line and the sold-out mark ----

    [Fact]
    public void ADiscountedCard_DeclaresTheReferencePriceBesideThePriceAndTheRangeAsOneLine_KK3()
    {
        DiscountLine line = new(80m, 100m, 20m, new DateOnly(2026, 11, 1), new DateOnly(2026, 11, 30));

        ProductCard card = ProductCards.Of(Item(price: 100m, discount: line), null);

        Assert.Equal("80 TL", card.PriceText);
        Assert.Equal("100 TL", card.Discount!.ReferencePriceText);
        Assert.Equal("80 TL", card.Discount.DiscountedPriceText);
        Assert.Equal("İndirim %20", card.Discount.MarkText);
        Assert.Equal("1 Kasım 2026 – 30 Kasım 2026", card.Discount.DateRangeText);
        Assert.DoesNotContain('\n', card.Discount.DateRangeText);
    }

    [Fact]
    public void ADiscountedCard_ShowsTheDiscountedPriceAsTheMainPriceAndTheListPriceOnlyAsTheReference_KK3()
    {
        // The reader's shape: LowestPrice is the list price (100), the discount line carries the discounted price (80) and the reference (100).
        DiscountLine line = new(80m, 100m, 20m, new DateOnly(2026, 11, 1), new DateOnly(2026, 11, 30));

        ProductCard card = ProductCards.Of(Item(price: 100m, discount: line), null);

        Assert.Equal("80 TL", card.PriceText);
        Assert.Equal("100 TL", card.Discount!.ReferencePriceText);
        Assert.NotEqual(card.PriceText, card.Discount.ReferencePriceText);
    }

    [Fact]
    public void AProductWithoutADiscount_ShowsTheSinglePriceAndNoReference_KK3()
    {
        ProductCard card = ProductCards.Of(Item(price: 100m), null);

        Assert.Equal("100 TL", card.PriceText);
        Assert.Null(card.Discount);
    }

    [Fact]
    public void ASoldOutProduct_KeepsItsCardAndCarriesTheMark_KK3()
    {
        ProductCard card = ProductCards.Of(Item(purchasable: false), null);

        Assert.True(card.IsSoldOut);
        Assert.Equal("/urun/ampul", card.Path);
    }

    // ---- KK4: the content card, with and without an image ----

    [Fact]
    public void AContentCard_CarriesTheImageTheTitleAndTheShortDescription_NoPriceMember_KK4()
    {
        HomepageCard record = new(Guid.NewGuid(), "Bakım hizmeti", "Yılda iki bakım.", "/sayfa/bakim", new ContentImage(ImageId, " Teknisyen "));

        ContentCard card = ContentCards.Of(record);

        Assert.Equal("Bakım hizmeti", card.Title);
        Assert.Equal("/sayfa/bakim", card.Path);
        Assert.Equal("Yılda iki bakım.", card.ShortDescription);
        Assert.Equal("Teknisyen", card.Image!.AltText);
        Assert.DoesNotContain(typeof(ContentCard).GetProperties(), p => p.Name.Contains("Price", StringComparison.Ordinal));
    }

    [Fact]
    public void ARecordWithoutAMainImage_IsACardWithoutOne_AndAnImageWithoutTextTakesTheTitle_KK4()
    {
        ContentCard plain = ContentCards.Of(new HomepageCard(Guid.NewGuid(), "Referans", null, "/sayfa/ref", null));
        ContentCard unnamed = ContentCards.Of(new HomepageCard(Guid.NewGuid(), "Referans", "  ", "/sayfa/ref", new ContentImage(ImageId, null)));

        Assert.Null(plain.Image);
        Assert.Null(plain.ShortDescription);
        Assert.Equal("Referans", unnamed.Image!.AltText);
        Assert.Null(unnamed.ShortDescription);
    }

    // ---- KK8: the page sizes and the numbered navigation ----

    [Theory]
    [InlineData(ListKind.StorefrontProducts, 24)]
    [InlineData(ListKind.ContentList, 24)]
    [InlineData(ListKind.MemberOrders, 25)]
    [InlineData(ListKind.PanelOrders, 25)]
    [InlineData(ListKind.PanelProducts, 25)]
    [InlineData(ListKind.PanelCoupons, 25)]
    [InlineData(ListKind.PanelOther, 25)]
    public void ThePageSize_IsTheTablesNumber_KK8(ListKind kind, int expected) => Assert.Equal(expected, ListCatalog.PageSizeOf(kind));

    private sealed class Query : PagedQuery
    {
        public string? Search { get; init; }
    }

    private const string Root = "/kategori/lamba";

    [Fact]
    public void AListOfOnePage_ShowsNoNavigation_KK8()
    {
        Pager one = Pager.Of(Root, new Query(), 1, 1);
        Pager none = Pager.Of(Root, new Query(), 1, 0);

        Assert.False(one.IsShown);
        Assert.False(none.IsShown);
        Assert.Empty(one.Items);
    }

    [Fact]
    public void ThePager_HasPreviousNextAndNumberedPages_TheFirstPageWithoutTheParameter_KK8()
    {
        Pager pager = Pager.Of(Root, new Query { Search = "ampul" }, 2, 3);

        Assert.True(pager.IsShown);
        Assert.Equal("/kategori/lamba?search=ampul", pager.PrevPagePath);
        Assert.Equal("/kategori/lamba?search=ampul&page=3", pager.NextPath);
        Assert.Equal([1, 2, 3], pager.Items.Select(i => i.Number!.Value).ToArray());
        Assert.Equal("/kategori/lamba?search=ampul", pager.Items[0].Path);
        Assert.True(pager.Items[1].IsCurrent);
        Assert.Null(pager.Items[1].Path);
    }

    [Fact]
    public void TheFirstPage_HasNoPrevious_AndTheLastHasNoNext_KK8()
    {
        Assert.Null(Pager.Of(Root, new Query(), 1, 5).PrevPagePath);
        Assert.NotNull(Pager.Of(Root, new Query(), 1, 5).NextPath);
        Assert.Null(Pager.Of(Root, new Query(), 5, 5).NextPath);
        Assert.NotNull(Pager.Of(Root, new Query(), 5, 5).PrevPagePath);
    }

    [Theory]
    [InlineData(1, 10, "1 2 … 10")]
    [InlineData(5, 10, "1 … 4 5 6 … 10")]
    [InlineData(4, 10, "1 2 3 4 5 … 10")]
    [InlineData(10, 10, "1 … 9 10")]
    [InlineData(99, 10, "1 … 9 10")]
    [InlineData(0, 4, "1 2 3 4")]
    public void ALongList_ShowsTheFirstTheLastAndTheNeighbours_AGapOfOneIsThePageItself_AndAPageOutOfRangeIsClamped_KK8(int page, int count, string expected)
    {
        Pager pager = Pager.Of(Root, new Query(), page, count);

        Assert.Equal(expected, string.Join(' ', pager.Items.Select(i => i.IsGap ? "…" : i.Number!.Value.ToString(System.Globalization.CultureInfo.InvariantCulture))));
    }

    // ---- KK7: the search and the filter belong to the listed lists ----

    [Theory]
    [InlineData(ListKind.StorefrontProducts, true, true)]
    [InlineData(ListKind.PanelOrders, true, true)]
    [InlineData(ListKind.PanelProducts, true, true)]
    [InlineData(ListKind.PanelMembers, true, false)]
    [InlineData(ListKind.PanelAuditTrail, false, true)]
    [InlineData(ListKind.PanelRequests, false, true)]
    [InlineData(ListKind.PanelCoupons, false, false)]
    [InlineData(ListKind.PanelContent, false, false)]
    [InlineData(ListKind.ContentList, false, false)]
    [InlineData(ListKind.MemberOrders, false, false)]
    [InlineData(ListKind.PanelOther, false, false)]
    public void TheSearchAndTheFilter_ExistInTheListsOfTheTableOnly_KK7(ListKind kind, bool search, bool filter)
    {
        Assert.Equal(search, ListCatalog.HasSearch(kind));
        Assert.Equal(filter, ListCatalog.HasFilter(kind));
        Assert.Equal(search, new ListSearch(kind, "/x", "Ara", null).IsShown);
    }

    // ---- KK9: the panel table is never uneven; no selection column ----

    private static readonly PanelTableColumn[] Columns = [new("Sipariş", true), new("Durum", true), new("E-posta", false)];

    [Fact]
    public void ARowOfAnotherLength_IsRefused_AndATableWithoutAPrimaryColumnIsRefused_KK9()
    {
        PanelTableRow[] uneven = [new PanelTableRow(["A", "B"], null)];
        PanelTableColumn[] secondaryOnly = [new("E-posta", false)];

        Assert.Throws<ArgumentException>(() => new PanelTable("Siparişler", Columns, uneven));
        Assert.Throws<ArgumentException>(() => new PanelTable("Siparişler", secondaryOnly, []));
    }

    [Fact]
    public void TheLinkColumn_IsTheFirstPrimaryOne_KK9()
    {
        PanelTableColumn[] columns = [new("E-posta", false), new("Sipariş", true), new("Durum", true)];

        Assert.Equal(1, new PanelTable("Siparişler", columns, []).LinkColumn);
    }

    [Fact]
    public void ThePanelTableRecords_HaveNoSelectionAndNoBulkMember_KK9()
    {
        string[] members = [.. typeof(PanelTable).GetProperties().Select(p => p.Name), .. typeof(PanelTableRow).GetProperties().Select(p => p.Name), .. typeof(PanelTableColumn).GetProperties().Select(p => p.Name)];

        Assert.All(members, name =>
        {
            Assert.DoesNotContain("Select", name, StringComparison.Ordinal);
            Assert.DoesNotContain("Bulk", name, StringComparison.Ordinal);
            Assert.DoesNotContain("Checkbox", name, StringComparison.Ordinal);
        });
    }

    // ---- KK10: the empty-state line ----

    [Fact]
    public void AnEmptyState_IsASentenceAndAWayOut_TheTwoPartsOfTheWayOutGoTogether_KK10()
    {
        EmptyState home = new("Bu kategoride ürün yok.", "Ana sayfaya dön", "/");
        EmptyState none = new("Henüz kayıt yok.");

        Assert.Equal("/", home.ActionPath);
        Assert.Null(none.ActionPath);
        Assert.Throws<ArgumentException>(() => new EmptyState("x", "Dön", null));
        Assert.Throws<ArgumentException>(() => new EmptyState("x", null, "/"));
        Assert.Throws<ArgumentException>(() => new EmptyState(" "));
    }
}
