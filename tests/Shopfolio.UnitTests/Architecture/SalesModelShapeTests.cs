using System.Reflection;

using Shopfolio.Infrastructure;
using Shopfolio.Modules.AfterSales.Contracts;
using Shopfolio.Modules.Catalog;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Ordering;

namespace Shopfolio.UnitTests.Architecture;

/// <summary>
/// The shape of the sales model in the application's own types (T332; 02 3.2.1-3.2.3, 3.3.1, 3.3.5, 3.3.6, 7.3.4; 10 KD-6, KD-35; 11 T332 criteria 1-3, 7-9; KK1-KK3, KK7-KK9): no name of the
/// application means a quote, an "ask for the price", an appointment, a minimum quantity, a review or a recommendation; the type lives on the product and the variant has none; the
/// withdrawal exemption is one product field and the variant has no trace of it; and the cart takes the variant, never the product. The scan is shown to find each forbidden
/// form in a sample of names (09 19.2 item 3).
/// </summary>
public sealed class SalesModelShapeTests
{
    private static readonly Assembly Application = typeof(AppDbContext).Assembly;

    [Fact]
    public void NoTypeMemberEnumValueOrParameterOfTheApplication_NamesAQuoteAnAppointmentAMinimumQuantityAReviewOrARecommendation()
    {
        List<string> findings =
        [
            .. SalesModelShape.NamesOf(Application)
                .SelectMany(n => SalesModelShape.FindingsIn(n.Name).Select(phrase => $"{n.Owner}: \"{phrase}\"")),
        ];

        Assert.Empty(findings);
    }

    [Fact]
    public void TheScanReadsTheWholeApplication_NotAnEmptyOrPartialAssembly()
    {
        List<string> names = [.. SalesModelShape.NamesOf(Application).Select(n => n.Name)];

        Assert.Contains(nameof(Product), names);
        Assert.Contains(nameof(Variant), names);
        Assert.Contains(nameof(CartItem), names);
        Assert.Contains(nameof(ICatalogModule.SuggestStockCodeAsync), names);
        Assert.Contains(nameof(ProductType.Service), names);
        Assert.True(names.Count > 5000, "the application surface has thousands of names; a scan that reads few is not reading the application");
    }

    public static TheoryData<string, string> ForbiddenSamples => new()
    {
        { "QuoteRequest", "quote" },
        { "RequestQuoteAsync", "quote" },
        { "price_on_request", "price on request" },
        { "PriceOnRequest", "price on request" },
        { "AskPrice", "ask price" },
        { "IsAskForPrice", "ask for price" },
        { "Appointment", "appointment" },
        { "BookingSlot", "booking" },
        { "TimeSlotId", "time slot" },
        { "MinOrderQuantity", "min order quantity" },
        { "minimumQuantity", "minimum quantity" },
        { "MIN_QTY", "min qty" },
        { "MinQuantityPerProduct", "min quantity" },
        { "ProductReview", "review" },
        { "ReviewCount", "review" },
        { "RatingAverage", "rating" },
        { "Stars", "stars" },
        { "Recommendations", "recommendations" },
        { "RelatedProducts", "related products" },
        { "also_bought", "also bought" },
        { "PeopleAlsoViewed", "people also" },
        { "Rfq", "rfq" },
        { "RFQNumber", "rfq" },
    };

    [Theory]
    [MemberData(nameof(ForbiddenSamples))]
    public void TheScan_FindsEachForbiddenFormInItsSpelling(string name, string phrase)
    {
        Assert.Contains(phrase, SalesModelShape.FindingsIn(name));
    }

    [Theory]
    [InlineData("MaxQuantityPerOrder")]
    [InlineData("StockQuantity")]
    [InlineData("ReservedQuantity")]
    [InlineData("MinimumOrderAmount")]
    [InlineData("PriceHistory")]
    [InlineData("ReviewerOfTheRecord")]
    [InlineData("Starter")]
    [InlineData("BookmarkRequest")]
    [InlineData("Quoted")]
    public void TheScan_DoesNotMistakeAnOrdinaryNameForAForbiddenOne(string name)
    {
        Assert.Empty(SalesModelShape.FindingsIn(name));
    }

    [Fact]
    public void TheVariant_HasNoTypeNoExemptionAndNoFulfilmentPeriod_TheProductHasThem()
    {
        PropertyInfo[] variant = typeof(Variant).GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        PropertyInfo[] product = typeof(Product).GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        Assert.DoesNotContain(variant, p => p.PropertyType == typeof(ProductType) || p.PropertyType == typeof(WithdrawalExemptionReason));
        Assert.DoesNotContain(variant, p => p.Name.Contains("Type", StringComparison.Ordinal)
            || p.Name.Contains("Exemption", StringComparison.Ordinal)
            || p.Name.Contains("Fulfilment", StringComparison.Ordinal));
        Assert.Single(product, p => p.PropertyType == typeof(ProductType));
        Assert.Single(product, p => p.PropertyType == typeof(WithdrawalExemptionReason?));
        Assert.Single(product, p => p.Name.Contains("Exemption", StringComparison.Ordinal));
        Assert.Contains(product, p => p.Name == nameof(Product.FulfilmentPeriod));
    }

    [Fact]
    public void TheProduct_HasNoMinimumOrderQuantityAndTheVariantNoneEither_OnlyTheMaximumPerOrderExists()
    {
        string[] productAndVariant = [.. typeof(Product).GetProperties().Concat(typeof(Variant).GetProperties()).Select(p => p.Name)];

        Assert.Contains(nameof(Product.MaxQuantityPerOrder), productAndVariant);
        Assert.DoesNotContain(productAndVariant, n => SalesModelShape.Words(n).Contains("min") || SalesModelShape.Words(n).Contains("minimum"));
    }

    [Fact]
    public void TheCartItem_ReferencesTheVariant_AndNotTheProduct()
    {
        PropertyInfo[] properties = typeof(CartItem).GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        Assert.Contains(properties, p => p.Name == nameof(CartItem.VariantId));
        Assert.DoesNotContain(properties, p => p.Name.Contains("Product", StringComparison.Ordinal) && p.Name != nameof(CartItem.RemovedProductName));
    }

    [Fact]
    public void TheOptionDimensionLimit_ComesFromTheProductConstant_NotFromALiteralInTheRule()
    {
        // The rule has no copy of the number: a limit moved in the constant moves the rule and the model's check together (02 11.2 P-21).
        string rule = File.ReadAllText(Path.Combine(Support.RepositoryRoot.Find(), "src/Shopfolio.Web/Modules/Catalog/OptionStructureRules.cs"));
        string entity = File.ReadAllText(Path.Combine(Support.RepositoryRoot.Find(), "src/Shopfolio.Web/Modules/Catalog/Option.cs"));

        Assert.Contains("ProductConstants.OptionDimensionMax", rule, StringComparison.Ordinal);
        Assert.Contains("ProductConstants.OptionDimensionMax", entity, StringComparison.Ordinal);
        Assert.DoesNotContain("<= 2", rule, StringComparison.Ordinal);
        Assert.DoesNotContain("BETWEEN 1 AND 2", entity, StringComparison.Ordinal);
    }
}
