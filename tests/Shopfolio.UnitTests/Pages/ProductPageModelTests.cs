using System.Text.Json;

using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Catalog.Contracts.Pricing;
using Shopfolio.Infrastructure;
using Shopfolio.Modules.Settings.Contracts;
using Shopfolio.Pages.Lists;
using Shopfolio.Pages.Storefront;

namespace Shopfolio.UnitTests.Pages;

/// <summary>
/// The product page's own rules (T503 KK4-KK6, KK8; 07 UC-04, UC-05; 04 5.4): which variant the two parameters pick, the selection fields, the quantity, the buy area's mode, the price block, the
/// delivery line by type and the script data. The rows are plain records, so no host is needed.
/// </summary>
public sealed class ProductPageModelTests
{
    private static readonly IReadOnlyList<ProductPageOption> TwoOptions =
    [
        new ProductPageOption("Renk", [new ProductPageOptionValue(0, "Kırmızı"), new ProductPageOptionValue(1, "Mavi")]),
        new ProductPageOption("Beden", [new ProductPageOptionValue(0, "S"), new ProductPageOptionValue(1, "M")]),
    ];

    private static ProductPageVariant Variant(int? first, int? second, bool soldOut = false, PublishStatus status = PublishStatus.Published) =>
        new(Guid.NewGuid(), $"SKU-{first}-{second}", 10m, soldOut, status) { FirstValueOrder = first, SecondValueOrder = second };

    private static ProductPage PageOf(IReadOnlyList<ProductPageVariant> variants, IReadOnlyList<ProductPageOption>? options = null, ProductPageKind kind = ProductPageKind.Published) =>
        new(kind, Guid.NewGuid(), "Masa", null, null, variants) { Slug = "masa", Type = ProductType.Physical, Options = options ?? [] };

    // ---- KK4: the parameters; a wrong value falls back to the default ----

    [Theory]
    [InlineData("0", 0)]
    [InlineData("12", 12)]
    [InlineData("007", 7)]
    [InlineData("", null)]
    [InlineData(null, null)]
    [InlineData("-1", null)]
    [InlineData("1.5", null)]
    [InlineData("abc", null)]
    [InlineData("1234567890", null)]
    [InlineData("+1", null)]
    public void ParseOrder_ReadsAWholeNonNegativeNumberAndNothingElse_T503_KK4(string? raw, int? expected)
    {
        Assert.Equal(expected, VariantSelection.ParseOrder(raw));
    }

    [Fact]
    public void Resolve_GivesTheNamedVariant_WhenBothPlacesNameAnOpenInStockVariant_T503_KK4()
    {
        ProductPageVariant a = Variant(0, 0);
        ProductPageVariant b = Variant(1, 1);
        ProductPage page = PageOf([a, b], TwoOptions);

        Assert.Same(b, VariantSelection.Resolve(page, 1, 1));
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData(1, null)]
    [InlineData(null, 1)]
    [InlineData(7, 7)]
    [InlineData(0, 1)]
    public void Resolve_FallsBackToTheDefault_ForAMissingUnknownOrUnopenedPlace_T503_KK4(int? first, int? second)
    {
        ProductPageVariant a = Variant(0, 0);
        ProductPageVariant b = Variant(1, 1);
        ProductPage page = PageOf([a, b], TwoOptions);

        Assert.Same(a, VariantSelection.Resolve(page, first, second));
    }

    [Fact]
    public void Resolve_FallsBackToTheDefault_WhenTheNamedVariantIsSoldOut_T503_KK4()
    {
        ProductPageVariant a = Variant(0, 0);
        ProductPageVariant soldOut = Variant(1, 1, soldOut: true);
        ProductPage page = PageOf([a, soldOut], TwoOptions);

        Assert.Same(a, VariantSelection.Resolve(page, 1, 1));
    }

    [Fact]
    public void Resolve_ForAProductWithOneOption_NeedsOnlyTheFirstPlace_T503_KK4()
    {
        ProductPageVariant a = Variant(0, null);
        ProductPageVariant b = Variant(1, null);
        ProductPage page = PageOf([a, b], [TwoOptions[0]]);

        Assert.Same(b, VariantSelection.Resolve(page, 1, null));
    }

    [Fact]
    public void Resolve_ForAProductWithoutOptions_IgnoresTheParameters_T503_KK4()
    {
        ProductPageVariant only = Variant(null, null);
        ProductPage page = PageOf([only]);

        Assert.Same(only, VariantSelection.Resolve(page, 3, 4));
    }

    [Fact]
    public void Resolve_ForADraftVariantOfAPreview_ShowsTheNamedDraftVariant_T503_KK3()
    {
        ProductPageVariant published = Variant(0, 0);
        ProductPageVariant draft = Variant(1, 1, status: PublishStatus.Draft);
        ProductPage page = PageOf([published, draft], TwoOptions, ProductPageKind.DraftPreview);

        Assert.Same(draft, VariantSelection.Resolve(page, 1, 1));
    }

    [Fact]
    public void Default_IsTheFirstPublishedOneThatCanBeBought_ElseTheFirstPublished_ElseTheFirstOfTheList_T503_KK4()
    {
        ProductPageVariant soldOut = Variant(0, 0, soldOut: true);
        ProductPageVariant buyable = Variant(1, 0);
        ProductPageVariant draft = Variant(0, 1, status: PublishStatus.Draft);

        Assert.Same(buyable, VariantSelection.Default([soldOut, buyable]));
        Assert.Same(soldOut, VariantSelection.Default([draft, soldOut]));
        Assert.Same(draft, VariantSelection.Default([draft]));
        Assert.Null(VariantSelection.Default([]));
    }

    [Fact]
    public void Find_GivesTheVariantOfTheIdentifier_AndNullForAnyOther_T503_KK8()
    {
        ProductPageVariant a = Variant(0, 0);
        ProductPage page = PageOf([a], TwoOptions);

        Assert.Same(a, VariantSelection.Find(page, a.Id));
        Assert.Null(VariantSelection.Find(page, Guid.NewGuid()));
    }

    // ---- KK5: the fields; "Tükendi" and the combination the firm did not open ----

    [Fact]
    public void Fields_MarkASoldOutValueAndALackingCombination_AndKeepTheShownValueSelectable_T503_KK5()
    {
        ProductPageVariant redS = Variant(0, 0);
        ProductPageVariant redM = Variant(0, 1, soldOut: true);
        ProductPageVariant blueS = Variant(1, 0);
        ProductPage page = PageOf([redS, redM, blueS], TwoOptions);

        IReadOnlyList<OptionField> fields = VariantSelection.Fields(page, redS);

        OptionField size = fields[1];
        Assert.Equal(VariantQuery.SecondParameter, size.FieldName);
        OptionChoice m = size.Choices[1];
        Assert.True(m.IsSoldOut);
        Assert.False(m.IsSelectable);
        Assert.True(size.Choices[0].IsSelected);

        OptionField color = fields[0];
        Assert.Equal(VariantQuery.FirstParameter, color.FieldName);
        Assert.All(color.Choices, c => Assert.True(c.IsSelectable));

        // From the blue variant the red-and-S neighbour exists but "blue, M" was never opened: the value is listed and cannot be chosen.
        OptionChoice blueM = VariantSelection.Fields(page, blueS)[1].Choices[1];
        Assert.False(blueM.IsSelectable);
        Assert.False(blueM.IsSoldOut);
    }

    [Fact]
    public void Fields_AreEmpty_ForAProductWithoutOptions_T503_KK5()
    {
        Assert.Empty(VariantSelection.Fields(PageOf([Variant(null, null)]), null));
    }

    [Theory]
    [InlineData(0, null, "/urun/masa")]
    [InlineData(1, null, "/urun/masa?firstOptionValue=1")]
    [InlineData(1, 2, "/urun/masa?firstOptionValue=1&secondOptionValue=2")]
    public void PathFor_NamesTheVariantByItsTwoPlaces_T503_KK7(int first, int? second, string expected)
    {
        ProductPageVariant variant = first == 0 ? Variant(null, null) : Variant(first, second);

        Assert.Equal(expected, VariantSelection.PathFor("/urun/masa", variant));
    }

    // ---- KK8: the quantity ----

    [Theory]
    [InlineData("1", 1)]
    [InlineData(" 12 ", 12)]
    [InlineData("0", null)]
    [InlineData("-3", null)]
    [InlineData("", null)]
    [InlineData(null, null)]
    [InlineData("2.5", null)]
    [InlineData("1e3", null)]
    [InlineData("x", null)]
    [InlineData("2147483648", null)]
    public void CartQuantity_ForAPhysicalProduct_IsAWholeNumberFromOneUp_T503_KK8(string? raw, int? expected)
    {
        Assert.Equal(expected, CartQuantity.Read(ProductType.Physical, raw));
        Assert.Equal(expected, CartQuantity.Read(ProductType.Service, raw));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("7")]
    [InlineData("abc")]
    public void CartQuantity_ForADigitalProduct_IsOneWhateverTheFormSent_T503_KK8(string? raw)
    {
        Assert.Equal(1, CartQuantity.Read(ProductType.Digital, raw));
    }

    [Fact]
    public void AddCartItemInput_ReadsTheVariantIdentifier_AndNullForAnythingElse_T503_KK8()
    {
        Guid id = Guid.NewGuid();

        Assert.Equal(id, new AddCartItemInput { VariantId = id.ToString("D") }.ReadVariantId());
        Assert.Null(new AddCartItemInput { VariantId = Guid.Empty.ToString("D") }.ReadVariantId());
        Assert.Null(new AddCartItemInput { VariantId = "x" }.ReadVariantId());
        Assert.Null(new AddCartItemInput().ReadVariantId());
    }

    [Fact]
    public void OrderLimitText_NamesTheLimit_ButNoMessageTellsTheStockAsANumber_T503_KK8()
    {
        Assert.Equal("Bu üründen bir siparişte en fazla 3 adet alınabilir", CartAddMessages.OrderLimitText(3));
        Assert.DoesNotContain(CartAddMessages.NoStockText, "0123456789", StringComparison.Ordinal);
    }

    // ---- KK5: the buy area's mode ----

    [Fact]
    public void ModeOf_IsTheFormWhileAnyVariantCanBeBought_AndSoldOutWhenNoneCan_T503_KK5()
    {
        ProductPageVariant buyable = Variant(0, 0);
        ProductPageVariant soldOut = Variant(1, 0, soldOut: true);

        Assert.Equal(BuyMode.Form, ProductViews.ModeOf(PageOf([buyable, soldOut], TwoOptions), buyable, false));
        Assert.Equal(BuyMode.SoldOut, ProductViews.ModeOf(PageOf([soldOut], TwoOptions), soldOut, false));
    }

    [Fact]
    public void ModeOf_SalesClosedBeatsTheForm_AndAPreviewBeatsBoth_AndTheArchivedPageSellsNothing_T503_KK5()
    {
        ProductPageVariant buyable = Variant(0, 0);
        ProductPageVariant draft = Variant(1, 0, status: PublishStatus.Draft);

        Assert.Equal(BuyMode.SalesClosed, ProductViews.ModeOf(PageOf([buyable]), buyable, true));
        Assert.Equal(BuyMode.Preview, ProductViews.ModeOf(PageOf([buyable], null, ProductPageKind.DraftPreview), buyable, true));
        Assert.Equal(BuyMode.Preview, ProductViews.ModeOf(PageOf([buyable, draft]), draft, false));
        Assert.Equal(BuyMode.None, ProductViews.ModeOf(PageOf([], null, ProductPageKind.Archived), null, false));
    }

    // ---- KK5: the price block ----

    [Fact]
    public void PriceBlockOf_LeavesTheUnitPriceOut_WhenItIsThePriceItself_T503_KK5()
    {
        PriceLabel same = new(100m, 20m, null, null, null, null, new UnitPriceLine(MeasurementUnit.Piece, 100m), null);
        PriceLabel different = new(100m, 20m, null, null, MeasurementUnit.Gram, 500m, new UnitPriceLine(MeasurementUnit.Kilogram, 200m), null);

        Assert.Null(ProductViews.PriceBlockOf(same, null).UnitPriceText);
        Assert.EndsWith("/ kg", ProductViews.PriceBlockOf(different, null).UnitPriceText, StringComparison.Ordinal);
    }

    [Fact]
    public void PriceBlockOf_ShowsTheDiscountedPriceWithTheReferenceMarkAndDates_WhileADiscountRuns_T503_KK5()
    {
        DiscountLine discount = new(80m, 100m, 20m, new DateOnly(2026, 11, 1), new DateOnly(2026, 11, 30));
        PriceLabel label = new(80m, 20m, null, null, null, null, null, discount);

        ProductPriceBlock block = ProductViews.PriceBlockOf(label, null);

        Assert.Equal(DisplayFormat.Amount(80m), block.PriceText);
        Assert.Equal(DisplayFormat.Amount(100m), block.ReferencePriceText);
        Assert.StartsWith("İndirim", block.DiscountMark, StringComparison.Ordinal);
        Assert.NotNull(block.DiscountDates);
    }

    // ---- KK5: the delivery line by type ----

    [Fact]
    public void DeliveryOf_IsByTheProductsType_AndOnlyAPhysicalProductCarriesTheRestrictionLine_T503_KK5()
    {
        DeliveryArea everywhere = new([], []);

        ProductDelivery digital = ProductViews.DeliveryOf(PageOf([]) with { Type = ProductType.Digital }, 3, everywhere);
        ProductDelivery service = ProductViews.DeliveryOf(PageOf([]) with { Type = ProductType.Service, FulfilmentPeriod = 5 }, 3, everywhere);
        ProductDelivery physical = ProductViews.DeliveryOf(PageOf([]) with { Type = ProductType.Physical, ShippingPeriod = 2 }, 3, everywhere);

        Assert.Equal("İndirme, ödeme onaylandığında açılır", digital.Line);
        Assert.Null(digital.Restriction);
        Assert.StartsWith("Hizmet", service.Line, StringComparison.Ordinal);
        Assert.Null(service.Restriction);
        Assert.False(string.IsNullOrEmpty(physical.Line));
    }

    // ---- KK6: the script data has no stock count ----

    [Fact]
    public void ScriptDataOf_CarriesThePurchasableFlagAndNeverAStockCount_T503_KK6()
    {
        VariantScriptData data = new(Guid.NewGuid(), 0, 1, "10,00 TL", null, null, null, null, "/gorsel/a", "Ad", true);

        using JsonDocument json = JsonDocument.Parse(ProductViews.ScriptDataOf([data]));
        JsonElement item = json.RootElement[0];

        Assert.True(item.GetProperty("purchasable").GetBoolean());
        Assert.False(item.TryGetProperty("stock", out _));
        Assert.False(item.TryGetProperty("stockQuantity", out _));
        Assert.DoesNotContain("stock", json.RootElement.GetRawText(), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ProductBuyAreaAndScriptRecords_HaveNoStockCountMember_T503_KK6()
    {
        foreach (Type type in new[] { typeof(VariantScriptData), typeof(ProductBuyArea), typeof(ProductView), typeof(ProductPriceBlock) })
        {
            Assert.DoesNotContain(type.GetProperties(), p => p.Name.Contains("Stock", StringComparison.OrdinalIgnoreCase) && p.PropertyType != typeof(bool));
        }
    }

    // ---- KK2: the empty states ----

    [Fact]
    public void EmptyStateOf_IsNullWithCards_TheSearchLineWithout_AndTheFilterLineWhenAFilterApplied_T503_KK2()
    {
        Assert.Null(SearchPage.EmptyStateOf(3, false, "/arama"));

        EmptyState plain = SearchPage.EmptyStateOf(0, false, "/arama")!;
        Assert.Equal(SearchPage.EmptySearchText, plain.Message);

        EmptyState filtered = SearchPage.EmptyStateOf(0, true, "/arama?search=masa")!;
        Assert.Equal(SearchPage.EmptyFilterText, filtered.Message);
    }
}
