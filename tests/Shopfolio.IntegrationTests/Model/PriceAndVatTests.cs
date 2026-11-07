using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Npgsql;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Migrations;
using Shopfolio.IntegrationTests.Database;
using Shopfolio.Modules.Administration;
using Shopfolio.Modules.Administration.Contracts;
using Shopfolio.Modules.Catalog;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Catalog.Contracts.Pricing;
using Shopfolio.Modules.Settings;
using Shopfolio.Modules.Settings.Contracts;

namespace Shopfolio.IntegrationTests.Model;

/// <summary>The collection of the price and VAT tests (T336): one database copied from the template, made by the real migrations.</summary>
[CollectionDefinition(PriceAndVatDefinition.Name)]
public sealed class PriceAndVatDefinition : ICollectionFixture<CollectionDatabaseFixture>
{
    public const string Name = "PriceAndVat";
}

/// <summary>
/// The price, the VAT and the price label through the real Catalog code on a real PostgreSQL (T336 criteria 1-3, 7, 8; KK1-KK3, KK7, KK8; 02 3.8.1-3.8.5; 06 4.5, 4.6,
/// 4.17, 4.18): the typed price comes back as typed; the VAT rate is a product column the variants inherit, its default is the firm setting; the order item keeps its VAT
/// amount and derives the net; a price with three decimals is refused before SQL and a price of zero by the database; the place of origin, the price date and the
/// measurement unit with its net quantity follow 02 3.8.5. The calculation order and the rounding (KK4-KK6) are pure and are the unit tests' (ItemPricingTests, MoneyTests).
/// </summary>
[Collection(PriceAndVatDefinition.Name)]
public sealed class PriceAndVatTests : IAsyncLifetime
{
    private static readonly DateTimeOffset Now = new(2026, 11, 7, 9, 0, 0, TimeSpan.Zero);

    private readonly CollectionDatabaseFixture _collection;

    private int _counter;

    public PriceAndVatTests(CollectionDatabaseFixture collection)
    {
        _collection = collection;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public async ValueTask InitializeAsync()
    {
        await ExecuteAsync("TRUNCATE price_histories, variants, option_values, options, products CASCADE");

        // The setting one test changes goes back to its start value before every test.
        decimal shipped = InsertStartupData.SettingDefaults.Single(s => s.Parameter == "P-10").Value!.Value;
        await ExecuteAsync($"UPDATE company_settings SET default_vat_rate = {shipped.ToString(System.Globalization.CultureInfo.InvariantCulture)}");
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    // ---- KK1: the typed price is the shown price ----

    [Fact]
    public async Task ThePriceTyped_IsThePriceTheLabelShows_VatIncluded()
    {
        (_, Variant variant) = await SeedAsync(ProductType.Physical, 1234.56m);

        PriceLabel label = await LabelAsync(variant.Id);

        Assert.Equal(1234.56m, label.Price);
        Assert.Equal("1234.56", await ScalarAsync<string>($"SELECT price::text FROM variants WHERE id = '{variant.Id}'"));
    }

    [Fact]
    public async Task AVariantThatDoesNotExist_HasNoLabel()
    {
        await using AppDbContext db = NewContext();

        Assert.Null(await new PriceLabelReader(db, new FixedTime(Now)).GetAsync(Guid.NewGuid(), Ct));
    }

    // ---- KK2: the rate is the product's, the variants inherit it, its default is the setting ----

    [Fact]
    public async Task TheVatRate_IsAProductColumn_AndNoVariantColumn()
    {
        Assert.Equal(["vat_rate"], await QueryAsync("SELECT column_name FROM information_schema.columns WHERE table_name = 'products' AND column_name LIKE '%vat%' ORDER BY 1"));
        Assert.Empty(await QueryAsync("SELECT column_name FROM information_schema.columns WHERE table_name = 'variants' AND column_name LIKE '%vat%'"));
    }

    [Fact]
    public async Task TheDefaultRate_ComesFromTheSetting_AndEveryVariantInheritsTheProductsRate()
    {
        decimal shipped = InsertStartupData.SettingDefaults.Single(s => s.Parameter == "P-10").Value!.Value;
        Assert.Equal(shipped, (await ProductDefaultsAsync()).DefaultVatRate);

        await ExecuteAsync("UPDATE company_settings SET default_vat_rate = 10");
        decimal rate = (await ProductDefaultsAsync()).DefaultVatRate;
        Product product = NewProduct(ProductType.Physical, rate);
        (Variant first, Variant second) = await SeedTwoVariantsAsync(product, ("ORAN-1", 50m, null), ("ORAN-2", 70m, null));

        Assert.Equal(10m, rate);
        Assert.Equal(10m, (await LabelAsync(first.Id)).VatRate);
        Assert.Equal(10m, (await LabelAsync(second.Id)).VatRate);
    }

    // ---- KK3: the order keeps the VAT apart; the net is derived ----

    [Fact]
    public async Task TheOrderItem_KeepsItsVatAmount_AndNoNetColumn_AndTheOrderKeepsTheShippingFeesSplit()
    {
        List<string> itemColumns = await QueryAsync("SELECT column_name FROM information_schema.columns WHERE table_name = 'order_items' ORDER BY 1");

        Assert.Contains("vat_rate", itemColumns);
        Assert.Contains("vat_amount", itemColumns);
        Assert.Contains("coupon_share", itemColumns);
        Assert.DoesNotContain(itemColumns, c => c.StartsWith("net", StringComparison.Ordinal) || c.Contains("matrah", StringComparison.Ordinal) || c.Contains("base_amount", StringComparison.Ordinal));
        Assert.Equal("jsonb", await ScalarAsync<string>("SELECT data_type FROM information_schema.columns WHERE table_name = 'orders' AND column_name = 'shipping_fee_vat_breakdown'"));

        // The amounts an item would freeze: the VAT is separated and rounded in the item, the net is the rest (02 3.8.3).
        ItemAmounts amounts = ItemPricing.Price(new ItemPriceInput(100m, 1, 0m, 20m));
        Assert.Equal((16.67m, 83.33m), (amounts.Vat, amounts.Net));
    }

    // ---- KK7: three decimals and zero are refused ----

    [Fact]
    public async Task APriceWithThreeDecimals_IsRefusedBeforeTheColumnRoundsIt()
    {
        Product product = NewProduct(ProductType.Physical);
        await SaveAsync(c => c.Products.Add(product));

        InvalidOperationException variant = await Assert.ThrowsAsync<InvalidOperationException>(
            () => SaveAsync(c => c.Variants.Add(Variant.Create(product.Id, "UC-ONDALIK", 12.345m, Now))));
        Assert.Contains("two decimals", variant.Message, StringComparison.Ordinal);
        Assert.Equal(0L, await ScalarAsync<long>("SELECT count(*) FROM variants"));

        Variant stored = Variant.Create(product.Id, "IKI-ONDALIK", 12.35m, Now);
        await SaveAsync(c => c.Variants.Add(stored));
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => SaveAsync(c => c.PriceHistories.Add(PriceHistory.Create(stored.Id, 12.345m, Now, Now))));
        Assert.Equal(0L, await ScalarAsync<long>("SELECT count(*) FROM price_histories"));
    }

    [Fact]
    public async Task APriceOfZero_IsRefusedByTheDatabase()
    {
        Product product = NewProduct(ProductType.Physical);
        await SaveAsync(c => c.Products.Add(product));

        DbUpdateException error = await Assert.ThrowsAsync<DbUpdateException>(
            () => SaveAsync(c => c.Variants.Add(Variant.Create(product.Id, "SIFIR", 0.00m, Now))));

        PostgresException postgres = Assert.IsType<PostgresException>(error.InnerException);
        Assert.Equal((PostgresErrorCodes.CheckViolation, "ck_variants_price_above_zero"), (postgres.SqlState, postgres.ConstraintName));
    }

    // ---- KK8: the place of origin ----

    [Fact]
    public async Task APhysicalProductWithoutItsOrigin_IsRefused_EvenAsADraft()
    {
        Product product = Product.Create("Kapı", "ulkesiz", ProductType.Physical, 20m, Now);

        InvalidOperationException error = await Assert.ThrowsAsync<InvalidOperationException>(() => SaveAsync(c => c.Products.Add(product)));

        Assert.Contains("place of origin", error.Message, StringComparison.Ordinal);
        Assert.Equal(0L, await ScalarAsync<long>("SELECT count(*) FROM products"));
    }

    [Fact]
    public async Task APhysicalProduct_ShowsItsOrigin()
    {
        (_, Variant variant) = await SeedAsync(ProductType.Physical, 10m);

        Assert.Equal(SeededCountry.Turkey, (await LabelAsync(variant.Id)).OriginCountryId);
    }

    [Fact]
    public async Task ADigitalProduct_IsSavedWithoutAnOrigin_WithNoLine_AndShowsAFilledOne()
    {
        (_, Variant empty) = await SeedAsync(ProductType.Digital, 10m);
        (_, Variant filled) = await SeedAsync(ProductType.Digital, 10m, origin: SeededCountry.IdOf("DE"));

        Assert.Null((await LabelAsync(empty.Id)).OriginCountryId);
        Assert.Equal(SeededCountry.IdOf("DE"), (await LabelAsync(filled.Id)).OriginCountryId);
    }

    [Fact]
    public async Task AServiceCarryingAnOrigin_IsRefused_AndWithoutOneHasNoLine()
    {
        Product carrying = NewProduct(ProductType.Service);
        carrying.OriginCountryId = SeededCountry.Turkey;

        InvalidOperationException error = await Assert.ThrowsAsync<InvalidOperationException>(() => SaveAsync(c => c.Products.Add(carrying)));
        (_, Variant service) = await SeedAsync(ProductType.Service, 10m);

        Assert.Contains("service carrying a place of origin", error.Message, StringComparison.Ordinal);
        Assert.Null((await LabelAsync(service.Id)).OriginCountryId);
    }

    // ---- KK8: the date the price applies since ----

    [Fact]
    public async Task ThePriceDate_IsTheLastHistoryRowThatHasStarted_NotAFutureOne()
    {
        (_, Variant variant) = await SeedAsync(ProductType.Physical, 100m);
        await SaveAsync(c => c.PriceHistories.AddRange(
            PriceHistory.Create(variant.Id, 90m, Now.AddDays(-10), Now),
            PriceHistory.Create(variant.Id, 100m, Now.AddDays(-2), Now),
            PriceHistory.Create(variant.Id, 110m, Now.AddDays(3), Now)));

        Assert.Equal(Now.AddDays(-2), (await LabelAsync(variant.Id)).PriceAppliesSince);
    }

    [Fact]
    public async Task WithoutAHistoryRow_ThePriceDateIsEmpty()
    {
        (_, Variant variant) = await SeedAsync(ProductType.Physical, 100m);

        Assert.Null((await LabelAsync(variant.Id)).PriceAppliesSince);
    }

    // ---- KK8: the measurement unit and the net quantity ----

    [Fact]
    public async Task WithAUnit_AVariantWithoutANetQuantity_IsRefused_AndZeroIsTheDatabasesRefusal()
    {
        Product product = NewProduct(ProductType.Physical);
        product.MeasurementUnit = MeasurementUnit.Gram;
        await SaveAsync(c => c.Products.Add(product));

        InvalidOperationException missing = await Assert.ThrowsAsync<InvalidOperationException>(
            () => SaveAsync(c => c.Variants.Add(Variant.Create(product.Id, "BOS-MIKTAR", 50m, Now))));
        Variant zero = Variant.Create(product.Id, "SIFIR-MIKTAR", 50m, Now);
        zero.NetQuantity = 0m;
        DbUpdateException zeroError = await Assert.ThrowsAsync<DbUpdateException>(() => SaveAsync(c => c.Variants.Add(zero)));

        Assert.Contains("no net quantity", missing.Message, StringComparison.Ordinal);
        Assert.Equal("ck_variants_net_quantity_above_zero", Assert.IsType<PostgresException>(zeroError.InnerException).ConstraintName);
        Assert.Equal(0L, await ScalarAsync<long>("SELECT count(*) FROM variants"));
    }

    [Fact]
    public async Task WithoutAUnit_AVariantCarryingANetQuantity_IsRefused()
    {
        Product product = NewProduct(ProductType.Physical);
        await SaveAsync(c => c.Products.Add(product));
        Variant variant = Variant.Create(product.Id, "BIRIMSIZ", 50m, Now);
        variant.NetQuantity = 250m;

        await Assert.ThrowsAsync<InvalidOperationException>(() => SaveAsync(c => c.Variants.Add(variant)));
    }

    [Fact]
    public async Task ANetQuantityWithFourDecimals_IsRefused()
    {
        Product product = NewProduct(ProductType.Physical);
        product.MeasurementUnit = MeasurementUnit.Kilogram;
        await SaveAsync(c => c.Products.Add(product));
        Variant variant = Variant.Create(product.Id, "DORT-ONDALIK", 50m, Now);
        variant.NetQuantity = 0.1234m;

        await Assert.ThrowsAsync<InvalidOperationException>(() => SaveAsync(c => c.Variants.Add(variant)));
    }

    [Fact]
    public async Task AUnitGivenLater_IsRefusedWhileAStoredVariantHasNoNetQuantity_AndSavedWithIt()
    {
        (Product product, Variant variant) = await SeedAsync(ProductType.Physical, 50m);

        await using (AppDbContext context = NewContext())
        {
            Product loaded = await context.Products.SingleAsync(p => p.Id == product.Id, Ct);
            loaded.MeasurementUnit = MeasurementUnit.Gram;
            loaded.BumpVersion();
            InvalidOperationException error = await Assert.ThrowsAsync<InvalidOperationException>(() => context.SaveChangesAsync(Ct));
            Assert.Contains("no net quantity", error.Message, StringComparison.Ordinal);
        }

        await using (AppDbContext context = NewContext())
        {
            Product loaded = await context.Products.SingleAsync(p => p.Id == product.Id, Ct);
            Variant stored = await context.Variants.SingleAsync(v => v.Id == variant.Id, Ct);
            loaded.MeasurementUnit = MeasurementUnit.Gram;
            loaded.BumpVersion();
            stored.NetQuantity = 250m;
            await context.SaveChangesAsync(Ct);
        }

        Assert.Equal("Gram", await ScalarAsync<string>($"SELECT measurement_unit FROM products WHERE id = '{product.Id}'"));
    }

    [Fact]
    public async Task TheUnitPrice_IsDerivedFromEachVariantsOwnPriceAndNetQuantity()
    {
        Product product = NewProduct(ProductType.Physical);
        product.MeasurementUnit = MeasurementUnit.Gram;
        (Variant small, Variant large) = await SeedTwoVariantsAsync(product, ("KAHVE-250", 50m, 250m), ("KAHVE-1000", 180m, 1000m));

        PriceLabel smallLabel = await LabelAsync(small.Id);
        PriceLabel largeLabel = await LabelAsync(large.Id);

        Assert.Equal((MeasurementUnit.Gram, 250m), (smallLabel.MeasurementUnit, smallLabel.NetQuantity));
        Assert.Equal(new UnitPriceLine(MeasurementUnit.Kilogram, 200.00m), smallLabel.UnitPrice);

        // 1000 grams is one kilogram: the unit price equals the sale price and is not shown (K-1018).
        Assert.Equal((MeasurementUnit.Gram, 1000m), (largeLabel.MeasurementUnit, largeLabel.NetQuantity));
        Assert.Null(largeLabel.UnitPrice);
    }

    [Fact]
    public async Task WithoutAUnit_TheLabelHasNoUnitLine()
    {
        (_, Variant variant) = await SeedAsync(ProductType.Physical, 50m);

        PriceLabel label = await LabelAsync(variant.Id);

        Assert.Null(label.MeasurementUnit);
        Assert.Null(label.NetQuantity);
        Assert.Null(label.UnitPrice);
    }

    // ---- helpers ----

    private Product NewProduct(ProductType type, decimal vatRate = 20m)
    {
        int n = Interlocked.Increment(ref _counter);
        Product product = Product.Create("Ürün " + n, "urun-" + n, type, vatRate, Now);
        return type == ProductType.Physical ? product.WithSeededOrigin() : product;
    }

    private async Task<(Product Product, Variant Variant)> SeedAsync(ProductType type, decimal price, Guid? origin = null)
    {
        Product product = NewProduct(type);
        if (origin is not null)
        {
            product.OriginCountryId = origin;
        }

        Variant variant = Variant.Create(product.Id, "SKU-" + _counter, price, Now);
        await SaveAsync(c =>
        {
            c.Products.Add(product);
            c.Variants.Add(variant);
        });
        return (product, variant);
    }

    // Two variants of one product are told apart by an option value (06 4.6 unique index), so the product gets one option with two values.
    private async Task<(Variant First, Variant Second)> SeedTwoVariantsAsync(
        Product product,
        (string Sku, decimal Price, decimal? NetQuantity) first,
        (string Sku, decimal Price, decimal? NetQuantity) second)
    {
        Option option = Option.Create(product.Id, "Boy", 1, Now);
        OptionValue one = OptionValue.Create(option.Id, "Küçük", 1, Now);
        OptionValue two = OptionValue.Create(option.Id, "Büyük", 2, Now);
        Variant a = Variant.Create(product.Id, first.Sku, first.Price, Now);
        a.FirstOptionValueId = one.Id;
        a.NetQuantity = first.NetQuantity;
        Variant b = Variant.Create(product.Id, second.Sku, second.Price, Now);
        b.FirstOptionValueId = two.Id;
        b.NetQuantity = second.NetQuantity;
        await SaveAsync(c =>
        {
            c.Products.Add(product);
            c.Options.Add(option);
            c.OptionValues.AddRange(one, two);
            c.Variants.AddRange(a, b);
        });
        return (a, b);
    }

    private async Task<PriceLabel> LabelAsync(Guid variantId)
    {
        await using AppDbContext db = NewContext();
        PriceLabel? label = await new PriceLabelReader(db, new FixedTime(Now)).GetAsync(variantId, Ct);
        return Assert.IsType<PriceLabel>(label);
    }

    private async Task<ProductDefaultsSection> ProductDefaultsAsync()
    {
        ServiceCollection services = new();
        services.AddDbContext<AppDbContext>(options => AppDbContext.Configure(options, _collection.Dsn));
        services.AddScoped<CacheInvalidations>();
        services.AddSingleton(TimeProvider.System);
        services.AddScoped<IAuditActorSource, NoActor>();
        services.AddScoped<IAuditTrail, AuditTrail>();
        services.AddSettingsModule();
        await using ServiceProvider provider = services.BuildServiceProvider();
        await using AsyncServiceScope scope = provider.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<ISettingsModule>().GetProductDefaultsAsync(Ct);
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

    private sealed class FixedTime(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }

    private sealed class NoActor : IAuditActorSource
    {
        public Task<AuditActor?> CurrentAsync(CancellationToken ct) => Task.FromResult<AuditActor?>(null);
    }
}
