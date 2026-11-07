using Microsoft.EntityFrameworkCore;

using Npgsql;

using Shopfolio.Infrastructure;
using Shopfolio.IntegrationTests.Database;
using Shopfolio.Modules.AfterSales.Contracts;
using Shopfolio.Modules.Catalog;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Catalog.Contracts.Variants;
using Shopfolio.Modules.Ordering;

namespace Shopfolio.IntegrationTests.Model;

/// <summary>The collection of the sales model tests (T332): one database copied from the template, made by the real migrations.</summary>
[CollectionDefinition(SalesModelDefinition.Name)]
public sealed class SalesModelDefinition : ICollectionFixture<CollectionDatabaseFixture>
{
    public const string Name = "SalesModel";
}

/// <summary>
/// The sales model through the real Catalog code on a real PostgreSQL (T332 criteria 1-10; KK1-KK10; 02 3.2, 3.3, 7.3.4; 05 7.8; 06 4.5-4.8): the type is a closed three-valued field of the
/// product that the variant does not carry; the stock code is required, compared without letter case and kept as typed, and is suggested from the product's address; a product
/// takes at most two option dimensions and a variant is bound to the product's own values; the matrix need not be full; the withdrawal exemption is a product field only for a physical
/// product and stays outside the gate; a service is not published without its fulfilment period; and no table or column carries a quote line, a minimum quantity or a review.
/// </summary>
[Collection(SalesModelDefinition.Name)]
public sealed class SalesModelTests : IAsyncLifetime
{
    private static readonly DateTimeOffset Now = new(2026, 11, 7, 9, 0, 0, TimeSpan.Zero);

    private readonly CollectionDatabaseFixture _collection;

    public SalesModelTests(CollectionDatabaseFixture collection)
    {
        _collection = collection;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public static TheoryData<WithdrawalExemptionReason> EveryExemptionReason => [.. Enum.GetValues<WithdrawalExemptionReason>()];

    public async ValueTask InitializeAsync()
    {
        await ExecuteAsync("TRUNCATE cart_items, carts, variants, option_values, options, products CASCADE");
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    // ---- KK1: no quote line, no "ask for the price", no appointment; KK7: no minimum quantity; KK8: no review ----

    [Fact]
    public async Task NoTableOrColumn_CarriesAQuoteAnAppointmentAMinimumQuantityAReviewOrARecommendation()
    {
        List<string> names = await QueryAsync(
            "SELECT table_name || '.' || column_name FROM information_schema.columns WHERE table_schema = 'public' ORDER BY 1");
        List<string> tables = await QueryAsync(
            "SELECT table_name FROM information_schema.tables WHERE table_schema = 'public' ORDER BY 1");

        string[] forbidden =
        [
            "quote", "offer_request", "price_on_request", "ask_price", "appointment", "booking",
            "review", "rating", "_star_", "recommend", "related_product", "also_bought", "similar_product",
        ];
        // "_star_" is a whole word ("start_date" is not a star); the others are word stems.
        Assert.DoesNotContain(names.Concat(tables), n => forbidden.Any(f => ("_" + n.Replace('.', '_') + "_").Contains(f, StringComparison.Ordinal)));

        // A minimum quantity of any spelling: "min" followed by a quantity word in one name (the minimum order amount of 02 3.18.3 is a money setting, not a quantity).
        Assert.DoesNotContain(names, n => System.Text.RegularExpressions.Regex.IsMatch(
            n.Split('.')[1], "^(min|minimum)_.*(quantity|qty|count|units|pieces)", System.Text.RegularExpressions.RegexOptions.CultureInvariant, TimeSpan.FromSeconds(5)));
    }

    [Fact]
    public async Task AVariantWithAnyUnitCount_IsSoldAsOne_AndTheCartTakesTheVariantNotTheProduct()
    {
        List<string> cartItemColumns = await QueryAsync(
            "SELECT column_name FROM information_schema.columns WHERE table_name = 'cart_items' ORDER BY 1");
        Assert.Contains("variant_id", cartItemColumns);
        Assert.DoesNotContain("product_id", cartItemColumns);

        (_, Variant variant) = await SeedPhysicalProductWithOneVariantAsync("tek-adet", "TEK-01");
        Cart cart = Cart.ForGuest("hash-tek-adet", Now);
        await SaveAsync(c =>
        {
            c.Carts.Add(cart);
            c.CartItems.Add(CartItem.Create(cart.Id, variant.Id, 1, variant.Price, Now));
        });

        Assert.Equal(1L, await ScalarAsync<long>("SELECT count(*) FROM cart_items WHERE quantity = 1"));
    }

    // ---- KK2: the type is closed, three-valued, on the product only ----

    [Fact]
    public async Task ProductType_IsTheThreeValuesOfTheProduct_AndTheVariantHasNoTypeColumn()
    {
        Assert.Equal(["Physical", "Digital", "Service"], Enum.GetNames<ProductType>());

        List<string> variantColumns = await QueryAsync("SELECT column_name FROM information_schema.columns WHERE table_name = 'variants' ORDER BY 1");
        Assert.DoesNotContain(variantColumns, c => c.Contains("type", StringComparison.Ordinal));
        Assert.DoesNotContain(variantColumns, c => c.Contains("withdrawal", StringComparison.Ordinal));
        Assert.DoesNotContain(variantColumns, c => c.Contains("fulfilment", StringComparison.Ordinal));
        Assert.Contains("product_type", await QueryAsync("SELECT column_name FROM information_schema.columns WHERE table_name = 'products' ORDER BY 1"));
    }

    [Fact]
    public async Task AFourthProductType_IsRefusedByTheDatabase_AndEachOfTheThreeIsAccepted()
    {
        (Product product, _) = await SeedPhysicalProductWithOneVariantAsync("tip-kapali", "TIP-01");

        PostgresException refused = await Assert.ThrowsAsync<PostgresException>(() => ExecuteAsync(
            $"UPDATE products SET product_type = 'Subscription' WHERE id = '{product.Id}'"));
        Assert.Equal(PostgresErrorCodes.CheckViolation, refused.SqlState);

        foreach (ProductType type in Enum.GetValues<ProductType>())
        {
            await ExecuteAsync($"UPDATE products SET product_type = '{type}' WHERE id = '{product.Id}'");
        }
    }

    // ---- KK4, KK5: at most two dimensions; a combination matrix that need not be full ----

    [Fact]
    public async Task AThirdOptionDimension_IsRefused_ByTheRuleAndByTheDatabase()
    {
        Product product = NewProduct("uc-boyut", ProductType.Physical);
        Option color = Option.Create(product.Id, "Renk", 1, Now);
        Option size = Option.Create(product.Id, "Beden", 2, Now);
        await SaveAsync(c =>
        {
            c.Products.Add(product);
            c.Options.AddRange(color, size);
        });

        Assert.Null(OptionStructureRules.NextOptionOrder(Enumerable.Range(1, ProductConstants.OptionDimensionMax)));

        DbUpdateException refused = await Assert.ThrowsAsync<DbUpdateException>(() => SaveAsync(c => c.Options.Add(Option.Create(product.Id, "Malzeme", ProductConstants.OptionDimensionMax + 1, Now))));
        Assert.Equal(PostgresErrorCodes.CheckViolation, Assert.IsType<PostgresException>(refused.InnerException).SqlState);
    }

    [Fact]
    public async Task AVariantIsBoundToTheProductsOwnValues_EachRequiredWhereTheProductHasTheOption()
    {
        (Product product, OptionValue red, OptionValue blue, OptionValue sizeM, OptionValue sizeL) = await SeedTwoDimensionsAsync();
        (_, OptionValue otherValue) = await SeedOtherProductValueAsync();

        await using AppDbContext context = NewContext();
        ProductStructureReader reader = new(context);

        Assert.Equal(VariantStructureCheck.Valid, await reader.CheckVariantAsync(product.Id, red.Id, sizeM.Id, null, Ct));
        Assert.Equal(VariantStructureCheck.ValueMissing, await reader.CheckVariantAsync(product.Id, red.Id, null, null, Ct));
        Assert.Equal(VariantStructureCheck.ValueMissing, await reader.CheckVariantAsync(product.Id, null, null, null, Ct));
        Assert.Equal(VariantStructureCheck.ValueMissing, await reader.CheckVariantAsync(product.Id, null, sizeM.Id, null, Ct));
        Assert.Equal(VariantStructureCheck.ValueNotOfTheProduct, await reader.CheckVariantAsync(product.Id, sizeM.Id, red.Id, null, Ct));
        Assert.Equal(VariantStructureCheck.ValueNotOfTheProduct, await reader.CheckVariantAsync(product.Id, otherValue.Id, sizeL.Id, null, Ct));
        Assert.Equal(VariantStructureCheck.ValueNotOfTheProduct, await reader.CheckVariantAsync(product.Id, blue.Id, otherValue.Id, null, Ct));
        Assert.Equal(VariantStructureCheck.ValueNotOfTheProduct, await reader.CheckVariantAsync(product.Id, EntityIds.New(), sizeL.Id, null, Ct));
    }

    [Fact]
    public async Task AProductWithoutOptions_TakesOneVariantWithNoValues_AndAOneDimensionProductOnlyTheFirstValue()
    {
        Product plain = NewProduct("secenek-yok", ProductType.Physical);
        Product oneDimension = NewProduct("tek-boyut", ProductType.Physical);
        Option color = Option.Create(oneDimension.Id, "Renk", 1, Now);
        OptionValue red = OptionValue.Create(color.Id, "Kırmızı", 1, Now);
        OptionValue green = OptionValue.Create(color.Id, "Yeşil", 2, Now);
        Variant plainVariant = Variant.Create(plain.Id, "SADE-01", 10m, Now, 100);
        await SaveAsync(c =>
        {
            c.Products.AddRange(plain, oneDimension);
            c.Options.Add(color);
            c.OptionValues.AddRange(red, green);
        });

        await using (AppDbContext context = NewContext())
        {
            ProductStructureReader reader = new(context);
            Assert.Equal(VariantStructureCheck.Valid, await reader.CheckVariantAsync(plain.Id, null, null, null, Ct));
            Assert.Equal(VariantStructureCheck.ValueForAbsentOption, await reader.CheckVariantAsync(plain.Id, red.Id, null, null, Ct));
            Assert.Equal(VariantStructureCheck.Valid, await reader.CheckVariantAsync(oneDimension.Id, red.Id, null, null, Ct));
            Assert.Equal(VariantStructureCheck.ValueMissing, await reader.CheckVariantAsync(oneDimension.Id, null, null, null, Ct));
            Assert.Equal(VariantStructureCheck.ValueForAbsentOption, await reader.CheckVariantAsync(oneDimension.Id, red.Id, green.Id, null, Ct));
        }

        await SaveAsync(c => c.Variants.Add(plainVariant));
        await using AppDbContext after = NewContext();
        ProductStructureReader next = new(after);
        Assert.Equal(VariantStructureCheck.CombinationTaken, await next.CheckVariantAsync(plain.Id, null, null, null, Ct));
        Assert.Equal(VariantStructureCheck.Valid, await next.CheckVariantAsync(plain.Id, null, null, plainVariant.Id, Ct));
    }

    [Fact]
    public async Task ACombinationHeldByAnotherVariant_IsTaken_ButNotByTheVariantBeingEdited()
    {
        (Product product, OptionValue red, OptionValue blue, OptionValue sizeM, OptionValue sizeL) = await SeedTwoDimensionsAsync();
        Variant held = Variant.Create(product.Id, "KOMB-01", 10m, Now, 100);
        held.FirstOptionValueId = red.Id;
        held.SecondOptionValueId = sizeM.Id;
        await SaveAsync(c => c.Variants.Add(held));

        await using AppDbContext context = NewContext();
        ProductStructureReader reader = new(context);

        Assert.Equal(VariantStructureCheck.CombinationTaken, await reader.CheckVariantAsync(product.Id, red.Id, sizeM.Id, null, Ct));
        Assert.Equal(VariantStructureCheck.Valid, await reader.CheckVariantAsync(product.Id, red.Id, sizeM.Id, held.Id, Ct));
        Assert.Equal(VariantStructureCheck.Valid, await reader.CheckVariantAsync(product.Id, red.Id, sizeL.Id, null, Ct));
        Assert.Equal(VariantStructureCheck.Valid, await reader.CheckVariantAsync(product.Id, blue.Id, sizeM.Id, null, Ct));
    }

    [Fact]
    public async Task TheMatrixNeedNotBeFull_AMissingCombinationIsACellNoVariantOpens()
    {
        (Product product, OptionValue red, OptionValue blue, OptionValue sizeM, OptionValue sizeL) = await SeedTwoDimensionsAsync();
        Variant redM = Variant.Create(product.Id, "MX-RM", 10m, Now, 100);
        redM.FirstOptionValueId = red.Id;
        redM.SecondOptionValueId = sizeM.Id;
        Variant redL = Variant.Create(product.Id, "MX-RL", 10m, Now, 100);
        redL.FirstOptionValueId = red.Id;
        redL.SecondOptionValueId = sizeL.Id;
        Variant blueL = Variant.Create(product.Id, "MX-BL", 10m, Now, 100);
        blueL.FirstOptionValueId = blue.Id;
        blueL.SecondOptionValueId = sizeL.Id;
        await SaveAsync(c => c.Variants.AddRange(redM, redL, blueL));

        await using AppDbContext context = NewContext();
        IReadOnlyList<OptionMatrixCell> cells = await new ProductStructureReader(context).GetMatrixAsync(product.Id, Ct);

        Assert.Equal(4, cells.Count);
        Assert.Equal(
            [(red.Id, sizeM.Id, redM.Id), (red.Id, sizeL.Id, redL.Id), (blue.Id, sizeM.Id, (Guid?)null), (blue.Id, sizeL.Id, blueL.Id)],
            cells.Select(c => (c.FirstOptionValueId.GetValueOrDefault(), c.SecondOptionValueId.GetValueOrDefault(), c.VariantId)));
        Assert.Equal(3, cells.Count(c => c.IsOpened));
        Assert.False(cells.Single(c => c.FirstOptionValueId == blue.Id && c.SecondOptionValueId == sizeM.Id).IsOpened);
    }

    [Fact]
    public async Task TheMatrixOfAProductWithoutOptions_IsOneCellWithNoValues_OpenedByTheOnlyVariant()
    {
        Product plain = NewProduct("matris-sade", ProductType.Physical);
        await SaveAsync(c => c.Products.Add(plain));

        await using (AppDbContext before = NewContext())
        {
            OptionMatrixCell closed = Assert.Single(await new ProductStructureReader(before).GetMatrixAsync(plain.Id, Ct));
            Assert.Null(closed.FirstOptionValueId);
            Assert.Null(closed.SecondOptionValueId);
            Assert.False(closed.IsOpened);
        }

        Variant only = Variant.Create(plain.Id, "MS-01", 10m, Now, 100);
        await SaveAsync(c => c.Variants.Add(only));
        await using AppDbContext after = NewContext();
        OptionMatrixCell opened = Assert.Single(await new ProductStructureReader(after).GetMatrixAsync(plain.Id, Ct));
        Assert.Equal(only.Id, opened.VariantId);
    }

    // ---- KK6: the stock code is required, unique without letter case, kept as typed; the suggestion comes from the address ----

    [Fact]
    public async Task AStockCode_IsMissingWhenBlank_TakenInItsIdentityForm_AndFreeOtherwise()
    {
        (Product _, Variant variant) = await SeedPhysicalProductWithOneVariantAsync("kod-denetimi", "AB-İ1");

        await using AppDbContext context = NewContext();
        StockCodes codes = new(context);

        Assert.Equal(StockCodeCheck.Missing, await codes.CheckAsync(null, null, Ct));
        Assert.Equal(StockCodeCheck.Missing, await codes.CheckAsync(string.Empty, null, Ct));
        Assert.Equal(StockCodeCheck.Missing, await codes.CheckAsync("   ", null, Ct));
        Assert.Equal(StockCodeCheck.Taken, await codes.CheckAsync("AB-İ1", null, Ct));
        Assert.Equal(StockCodeCheck.Taken, await codes.CheckAsync("ab-ı1", null, Ct));
        Assert.Equal(StockCodeCheck.Taken, await codes.CheckAsync("Ab-I1", null, Ct));
        Assert.Equal(StockCodeCheck.Taken, await codes.CheckAsync(" ab-i1 ", null, Ct));
        Assert.Equal(StockCodeCheck.Free, await codes.CheckAsync("AB-İ2", null, Ct));
        Assert.Equal(StockCodeCheck.Free, await codes.CheckAsync("AB-İ1", variant.Id, Ct));
    }

    [Fact]
    public async Task ASecondCodeInAnotherLetterCase_IsRefused_AndTheFirstStaysAsItWasTyped()
    {
        (Product product, Variant first) = await SeedPhysicalProductWithOneVariantAsync("kod-ilk", "Ahşap-I01");
        Option option = Option.Create(product.Id, "Renk", 1, Now);
        OptionValue value = OptionValue.Create(option.Id, "Kırmızı", 1, Now);
        Variant second = Variant.Create(product.Id, "AHŞAP-ı01", 10m, Now, 100);
        second.FirstOptionValueId = value.Id;

        await SaveAsync(c =>
        {
            c.Options.Add(option);
            c.OptionValues.Add(value);
        });
        DbUpdateException refused = await Assert.ThrowsAsync<DbUpdateException>(() => SaveAsync(c => c.Variants.Add(second)));

        Assert.Equal(PostgresErrorCodes.UniqueViolation, Assert.IsType<PostgresException>(refused.InnerException).SqlState);
        Assert.Equal("Ahşap-I01", await ScalarAsync<string>($"SELECT sku FROM variants WHERE id = '{first.Id}'"));
        Assert.Equal(1L, await ScalarAsync<long>("SELECT count(*) FROM variants"));
    }

    [Fact]
    public async Task TheSuggestion_IsTheAddressInCapitalsAndTheOrder_RaisedUntilTheCodeIsFree()
    {
        Product product = NewProduct("ahsap-masa", ProductType.Physical, "Ahşap Masa");
        Product another = NewProduct("baska-urun", ProductType.Physical);
        await SaveAsync(c => c.Products.AddRange(product, another));

        Assert.Equal("AHSAP-MASA-01", await SuggestAsync(product.Id, null));

        Variant first = Variant.Create(product.Id, "AHSAP-MASA-01", 10m, Now, 100);
        await SaveAsync(c => c.Variants.Add(first));
        Assert.Equal("AHSAP-MASA-02", await SuggestAsync(product.Id, "Tamamen Başka Ad"));

        // Another product holds the next code: the order goes up until the code is free; the comparison is the identity one.
        await SaveAsync(c => c.Variants.Add(Variant.Create(another.Id, "ahsap-masa-02", 10m, Now, 100)));
        Assert.Equal("AHSAP-MASA-03", await SuggestAsync(product.Id, null));
    }

    [Fact]
    public async Task TheSuggestion_TakesTheFirstTwentyCharactersOfTheAddress_AndDropsAHyphenTheCutLeaves()
    {
        Product longAddress = NewProduct("cok-uzun-bir-urun-adi-ile-test", ProductType.Physical);
        Product hyphenCut = NewProduct("abcdefghijklmnopqrs-tuvw", ProductType.Physical);
        await SaveAsync(c => c.Products.AddRange(longAddress, hyphenCut));

        Assert.Equal("COK-UZUN-BIR-URUN-AD-01", await SuggestAsync(longAddress.Id, null));
        Assert.Equal("ABCDEFGHIJKLMNOPQRS-01", await SuggestAsync(hyphenCut.Id, null));
    }

    [Fact]
    public async Task TheSuggestion_ForAProductNotSavedYet_ComesFromTheNameNow_AndIsNullWhenThereIsNone()
    {
        Assert.Equal("AHSAP-MASA-01", await SuggestAsync(null, "Ahşap Masa"));
        Assert.Equal("AHSAP-MASA-01", await SuggestAsync(EntityIds.New(), "Ahşap Masa"));
        Assert.Null(await SuggestAsync(null, null));
        Assert.Null(await SuggestAsync(null, string.Empty));
        Assert.Null(await SuggestAsync(null, "   "));
        Assert.Null(await SuggestAsync(EntityIds.New(), "  "));

        // An unsaved product's suggestion also steps over a code that is held (the code of a saved variant of another product).
        Product holder = NewProduct("tutan", ProductType.Physical);
        await SaveAsync(c =>
        {
            c.Products.Add(holder);
            c.Variants.Add(Variant.Create(holder.Id, "AHSAP-MASA-01", 10m, Now, 100));
        });
        Assert.Equal("AHSAP-MASA-02", await SuggestAsync(null, "Ahşap Masa"));
    }

    [Fact]
    public async Task ARenamedProduct_KeepsItsCodes_AndTheNextSuggestionStillComesFromItsAddress()
    {
        Product product = NewProduct("eski-ad", ProductType.Physical, "Eski Ad");
        Variant first = Variant.Create(product.Id, "ESKI-AD-01", 10m, Now, 100);
        await SaveAsync(c =>
        {
            c.Products.Add(product);
            c.Variants.Add(first);
        });
        await using (AppDbContext rename = NewContext())
        {
            Product loaded = await rename.Products.SingleAsync(p => p.Id == product.Id, Ct);
            loaded.SetName("Yepyeni Ad");
            loaded.BumpVersion();
            await rename.SaveChangesAsync(Ct);
        }

        Assert.Equal("ESKI-AD-01", await ScalarAsync<string>($"SELECT sku FROM variants WHERE id = '{first.Id}'"));
        Assert.Equal("ESKI-AD-02", await SuggestAsync(product.Id, "Yepyeni Ad"));
    }

    // ---- KK9: the withdrawal exemption is a product field, only for a physical product, outside the gate ----

    [Theory]
    [MemberData(nameof(EveryExemptionReason))]
    public async Task APhysicalProduct_CarriesAnyExemptionReason_AndIsPublishedWithOrWithoutIt(WithdrawalExemptionReason reason)
    {
        Product marked = NewProduct("isaretli-" + reason.ToString().ToLowerInvariant(), ProductType.Physical);
        marked.WithdrawalExemptionReason = reason;
        marked.PublishStatus = PublishStatus.Published;
        Product unmarked = NewProduct("isaretsiz-" + reason.ToString().ToLowerInvariant(), ProductType.Physical);
        unmarked.PublishStatus = PublishStatus.Published;

        await SaveAsync(c => c.Products.AddRange(marked, unmarked));

        Assert.Equal(1L, await ScalarAsync<long>($"SELECT count(*) FROM products WHERE id = '{marked.Id}' AND withdrawal_exemption_reason IS NOT NULL AND publish_status = 'Published'"));
        Assert.Equal(1L, await ScalarAsync<long>($"SELECT count(*) FROM products WHERE id = '{unmarked.Id}' AND withdrawal_exemption_reason IS NULL AND publish_status = 'Published'"));
    }

    [Theory]
    [InlineData(ProductType.Digital)]
    [InlineData(ProductType.Service)]
    public async Task AnExemptionReasonOnANonPhysicalProduct_IsRefusedBeforeAnySql(ProductType type)
    {
        Product product = NewProduct("istisna-yasak", type);
        product.WithdrawalExemptionReason = WithdrawalExemptionReason.MadeToOrder;

        await Assert.ThrowsAsync<InvalidOperationException>(() => SaveAsync(c => c.Products.Add(product)));

        Assert.Equal(0L, await ScalarAsync<long>("SELECT count(*) FROM products"));
    }

    [Fact]
    public async Task ChangingAMarkedPhysicalProductToDigital_IsRefusedUntilTheReasonIsCleared()
    {
        Product product = NewProduct("tip-degisimi", ProductType.Physical);
        product.WithdrawalExemptionReason = WithdrawalExemptionReason.MadeToOrder;
        await SaveAsync(c => c.Products.Add(product));

        await using AppDbContext context = NewContext();
        Product loaded = await context.Products.SingleAsync(p => p.Id == product.Id, Ct);
        loaded.ProductType = ProductType.Digital;
        loaded.BumpVersion();
        await Assert.ThrowsAsync<InvalidOperationException>(() => context.SaveChangesAsync(Ct));

        loaded.WithdrawalExemptionReason = null;
        await context.SaveChangesAsync(Ct);
        Assert.Equal("Digital", await ScalarAsync<string>($"SELECT product_type FROM products WHERE id = '{product.Id}'"));
    }

    [Fact]
    public async Task TheExemption_IsTheReasonFieldBeingFilled_WithNoFlagAndNothingOnTheVariant()
    {
        List<string> productColumns = await QueryAsync("SELECT column_name FROM information_schema.columns WHERE table_name = 'products' ORDER BY 1");

        Assert.Equal(["withdrawal_exemption_reason"], productColumns.Where(c => c.Contains("exemption", StringComparison.Ordinal)).ToList());
        Assert.DoesNotContain(
            await QueryAsync("SELECT column_name FROM information_schema.columns WHERE table_name = 'variants' ORDER BY 1"),
            c => c.Contains("exemption", StringComparison.Ordinal));
    }

    // ---- KK10: a service is not published without its fulfilment period; zero days is refused ----

    [Fact]
    public async Task AServiceWithoutAFulfilmentPeriod_CannotBePublished_ButCanBeADraft()
    {
        Product draft = NewProduct("hizmet-taslak", ProductType.Service);
        await SaveAsync(c => c.Products.Add(draft));

        Product published = NewProduct("hizmet-yayin", ProductType.Service);
        published.PublishStatus = PublishStatus.Published;
        await Assert.ThrowsAsync<InvalidOperationException>(() => SaveAsync(c => c.Products.Add(published)));

        await using AppDbContext context = NewContext();
        Product loaded = await context.Products.SingleAsync(p => p.Id == draft.Id, Ct);
        loaded.PublishStatus = PublishStatus.Published;
        await Assert.ThrowsAsync<InvalidOperationException>(() => context.SaveChangesAsync(Ct));

        Assert.Equal(1L, await ScalarAsync<long>("SELECT count(*) FROM products"));
        Assert.Equal("Draft", await ScalarAsync<string>($"SELECT publish_status FROM products WHERE id = '{draft.Id}'"));
    }

    [Fact]
    public async Task AServiceWithAFulfilmentPeriodOfOneDayOrMore_IsPublished_AndZeroDaysIsRefused()
    {
        Product oneDay = NewProduct("hizmet-bir-gun", ProductType.Service);
        oneDay.FulfilmentPeriod = 1;
        oneDay.PublishStatus = PublishStatus.Published;
        await SaveAsync(c => c.Products.Add(oneDay));

        Product thirty = NewProduct("hizmet-otuz-gun", ProductType.Service);
        thirty.FulfilmentPeriod = 30;
        thirty.PublishStatus = PublishStatus.Published;
        await SaveAsync(c => c.Products.Add(thirty));

        Product zero = NewProduct("hizmet-sifir-gun", ProductType.Service);
        zero.FulfilmentPeriod = 0;
        DbUpdateException refused = await Assert.ThrowsAsync<DbUpdateException>(() => SaveAsync(c => c.Products.Add(zero)));

        Assert.Equal(PostgresErrorCodes.CheckViolation, Assert.IsType<PostgresException>(refused.InnerException).SqlState);
        Assert.Equal(2L, await ScalarAsync<long>("SELECT count(*) FROM products"));
    }

    [Fact]
    public async Task APhysicalOrDigitalProduct_IsPublishedWithoutAFulfilmentPeriod()
    {
        Product physical = NewProduct("fiziksel-yayin", ProductType.Physical);
        physical.PublishStatus = PublishStatus.Published;
        Product digital = NewProduct("dijital-yayin", ProductType.Digital);
        digital.PublishStatus = PublishStatus.Published;

        await SaveAsync(c => c.Products.AddRange(physical, digital));

        Assert.Equal(2L, await ScalarAsync<long>("SELECT count(*) FROM products WHERE publish_status = 'Published' AND fulfilment_period IS NULL"));
    }

    // ---- helpers ----

    private static Product NewProduct(string slug, ProductType type, string? name = null)
    {
        return Product.Create(name ?? "Ürün " + slug, slug, type, 20m, Now);
    }

    private async Task<(Product Product, Variant Variant)> SeedPhysicalProductWithOneVariantAsync(string slug, string sku)
    {
        Product product = NewProduct(slug, ProductType.Physical);
        Variant variant = Variant.Create(product.Id, sku, 100m, Now, 100);
        await SaveAsync(c =>
        {
            c.Products.Add(product);
            c.Variants.Add(variant);
        });
        return (product, variant);
    }

    private async Task<(Product Product, OptionValue Red, OptionValue Blue, OptionValue M, OptionValue L)> SeedTwoDimensionsAsync()
    {
        Product product = NewProduct("iki-boyut", ProductType.Physical);
        Option color = Option.Create(product.Id, "Renk", 1, Now);
        Option size = Option.Create(product.Id, "Beden", 2, Now);
        OptionValue red = OptionValue.Create(color.Id, "Kırmızı", 1, Now);
        OptionValue blue = OptionValue.Create(color.Id, "Mavi", 2, Now);
        OptionValue sizeM = OptionValue.Create(size.Id, "M", 1, Now);
        OptionValue sizeL = OptionValue.Create(size.Id, "L", 2, Now);
        await SaveAsync(c =>
        {
            c.Products.Add(product);
            c.Options.AddRange(color, size);
            c.OptionValues.AddRange(red, blue, sizeM, sizeL);
        });
        return (product, red, blue, sizeM, sizeL);
    }

    private async Task<(Product Product, OptionValue Value)> SeedOtherProductValueAsync()
    {
        Product other = NewProduct("baska-urun-secenegi", ProductType.Physical);
        Option option = Option.Create(other.Id, "Renk", 1, Now);
        OptionValue value = OptionValue.Create(option.Id, "Siyah", 1, Now);
        await SaveAsync(c =>
        {
            c.Products.Add(other);
            c.Options.Add(option);
            c.OptionValues.Add(value);
        });
        return (other, value);
    }

    private async Task<string?> SuggestAsync(Guid? productId, string? name)
    {
        await using AppDbContext context = NewContext();
        return await new StockCodes(context).SuggestAsync(productId, name, Ct);
    }

    private AppDbContext NewContext()
    {
        DbContextOptionsBuilder<AppDbContext> builder = new();
        AppDbContext.Configure(builder, _collection.Dsn);
        return new AppDbContext(builder.Options);
    }

    private async Task SaveAsync(Action<AppDbContext> change)
    {
        await using AppDbContext context = NewContext();
        change(context);
        await context.SaveChangesAsync(Ct);
    }

    private async Task ExecuteAsync(string sql)
    {
        await using NpgsqlConnection connection = new(_collection.Dsn);
        await connection.OpenAsync(Ct);
        await using NpgsqlCommand command = new(sql, connection);
        await command.ExecuteNonQueryAsync(Ct);
    }

    private async Task<T> ScalarAsync<T>(string sql)
    {
        await using NpgsqlConnection connection = new(_collection.Dsn);
        await connection.OpenAsync(Ct);
        await using NpgsqlCommand command = new(sql, connection);
        return (T)(await command.ExecuteScalarAsync(Ct))!;
    }

    private async Task<List<string>> QueryAsync(string sql)
    {
        await using NpgsqlConnection connection = new(_collection.Dsn);
        await connection.OpenAsync(Ct);
        await using NpgsqlCommand command = new(sql, connection);
        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(Ct);
        List<string> rows = [];
        while (await reader.ReadAsync(Ct))
        {
            rows.Add(reader.GetString(0));
        }

        return rows;
    }
}
