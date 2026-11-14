using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;

using Npgsql;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Errors;
using Shopfolio.Infrastructure.Files;
using Shopfolio.Infrastructure.Imaging;
using Shopfolio.Infrastructure.Jobs;
using Shopfolio.IntegrationTests.Database;
using Shopfolio.Modules.Administration;
using Shopfolio.Modules.Administration.Contracts;
using Shopfolio.Modules.AfterSales;
using Shopfolio.Modules.Catalog;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Ordering;
using Shopfolio.Modules.Payment;
using Shopfolio.Modules.Settings;

namespace Shopfolio.IntegrationTests.Catalog;

/// <summary>The collection of the product form's save tests (T525): one database copied from the template.</summary>
[CollectionDefinition(ProductSavesDefinition.Name)]
public sealed class ProductSavesDefinition : ICollectionFixture<CollectionDatabaseFixture>
{
    public const string Name = "ProductSaves";
}

/// <summary>
/// The product form's save on a real PostgreSQL (11 T525 KK3-KK6; 07 UC-114): one transaction and the Draft birth, the stock against its shown value and the price history, every 422
/// reason in its own test, the version's 409 - and a refusal that writes nothing. The page handler and the order of the refusals on the wire are the page tests'.
/// </summary>
[Collection(ProductSavesDefinition.Name)]
public sealed class ProductSavesTests : IAsyncLifetime
{
    private static readonly DateOnly Today = new(2026, 11, 20);
    private static readonly DateTimeOffset Now = TurkeyTime.StartOf(Today).AddHours(9);
    private static readonly DateTimeOffset LongAgo = Now.AddDays(-60);

    private readonly CollectionDatabaseFixture _collection;
    private readonly FixedTime _time = new(Now);

    private int _counter;

    private Guid _countryId;

    private Admin _admin = null!;

    private ServiceProvider _provider = null!;

    public ProductSavesTests(CollectionDatabaseFixture collection)
    {
        _collection = collection;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public async ValueTask InitializeAsync()
    {
        await ExecuteAsync(
            "TRUNCATE audit_logs, admins, price_histories, discounts, product_images, product_categories, categories, digital_files, images, variants, option_values, options, products, countries CASCADE");
        Country country = Country.Create("TR", "Türkiye", Now);
        _admin = Admin.Create("Firma Yöneticisi", "yonetici@example.com", "digest-placeholder", Now);
        await SaveAsync(c =>
        {
            c.Countries.Add(country);
            c.Admins.Add(_admin);
        });
        _countryId = country.Id;
        _provider = BuildProvider();
    }

    public async ValueTask DisposeAsync() => await _provider.DisposeAsync();

    // ---- KK3: one transaction; the Draft birth; the closed format set ----

    [Fact]
    public async Task ANewProduct_AndItsVariant_AreBornDraft_InOneSave_WithTheFirstPriceInTheHistory_KK3()
    {
        ProductSaveResult result = await RunAsync(Command(variants: [Row(sku: "KUPA-1", price: 120m)]));

        Guid productId = Assert.IsType<Guid>(result.ProductId);
        await using AppDbContext db = NewContext();
        Product product = await db.Products.SingleAsync(p => p.Id == productId, Ct);
        Variant variant = await db.Variants.SingleAsync(v => v.ProductId == productId, Ct);
        Assert.Equal((PublishStatus.Draft, PublishStatus.Draft, 1), (product.PublishStatus, variant.PublishStatus, product.Version));
        Assert.Equal(120m, Assert.Single(await db.PriceHistories.Where(h => h.VariantId == variant.Id).Select(h => h.Price).ToListAsync(Ct)));
    }

    [Fact]
    public async Task TheDescription_IsCleanedByTheClosedFormatSet_KK3()
    {
        ProductSaveResult result = await RunAsync(Command(description: "<p>Kupa</p><script>alert(1)</script>", variants: [Row(sku: "KUPA-2")]));

        await using AppDbContext db = NewContext();
        string? description = await db.Products.Where(p => p.Id == result.ProductId).Select(p => p.Description).SingleAsync(Ct);
        Assert.Contains("Kupa", description, StringComparison.Ordinal);
        Assert.DoesNotContain("script", description, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ANewVariantWithABlankStockCode_TakesTheSuggestedCode_KK3()
    {
        ProductSaveResult result = await RunAsync(Command(name: "Seramik Kupa", variants: [Row(sku: null)]));

        await using AppDbContext db = NewContext();
        string sku = await db.Variants.Where(v => v.ProductId == result.ProductId).Select(v => v.SKU).SingleAsync(Ct);
        Assert.False(string.IsNullOrWhiteSpace(sku));
    }

    // ---- KK4: the stock against its shown value; the price history; the version ----

    [Fact]
    public async Task AnExistingProduct_WritesItsStockAgainstTheShownValue_AndItsNewPriceToTheHistory_AndRaisesTheVersionOnce_KK4()
    {
        (Product product, Variant variant) = await SeedAsync(ProductType.Physical, stock: 5);

        ProductSaveResult result = await RunAsync(Existing(product, variant, price: 150m, stock: 8, shown: 5));

        Assert.False(result.IsRejected);
        await using AppDbContext db = NewContext();
        Variant saved = await db.Variants.SingleAsync(v => v.Id == variant.Id, Ct);
        Assert.Equal((8, 150m), (saved.StockQuantity, saved.Price));
        Assert.Equal(product.Version + 1, await db.Products.Where(p => p.Id == product.Id).Select(p => p.Version).SingleAsync(Ct));
        Assert.Contains(150m, await db.PriceHistories.Where(h => h.VariantId == variant.Id).Select(h => h.Price).ToListAsync(Ct));

        // Neither the stock nor the price enters the trail (criterion 4).
        Assert.Equal(0, await db.AuditLogs.CountAsync(Ct));
    }

    [Fact]
    public async Task AnExistingProductsShippingPeriod_EntersTheTrailWithItsOldAndNewValue_AndAnUnchangedSettingDoesNot_KK4()
    {
        (Product product, Variant variant) = await SeedAsync(ProductType.Physical, stock: 5);
        await ExecuteAsync($"UPDATE products SET shipping_period = 2, max_quantity_per_order = 5 WHERE id = '{product.Id}'");

        ProductSaveResult result = await RunAsync(Existing(product, variant, price: 100m, stock: 5, shown: 5) with { ShippingPeriod = 4, MaxQuantityPerOrder = 5 });

        Assert.False(result.IsRejected);
        await using AppDbContext db = NewContext();
        AuditLog row = Assert.Single(await db.AuditLogs.AsNoTracking().ToListAsync(Ct));
        Assert.Equal((nameof(AuditOperation.UpdateSettings), nameof(Product), (Guid?)product.Id), (row.Operation, row.TargetRecordType, row.TargetRecordId));
        Assert.Equal(new AuditValueChange(nameof(Product.ShippingPeriod), "2", "4"), Assert.Single(row.Details!.Changes));
    }

    [Fact]
    public async Task AServicesFulfilmentPeriodAndMostPerOrder_EnterTheTrailFromEmptyToTheirValue_KK4()
    {
        (Product product, Variant variant) = await SeedAsync(ProductType.Service, stock: null);

        ProductSaveResult result = await RunAsync(Existing(product, variant, price: 100m, stock: null, shown: null) with { FulfilmentPeriod = 3, MaxQuantityPerOrder = 2 });

        Assert.False(result.IsRejected);
        await using AppDbContext db = NewContext();
        AuditLog row = Assert.Single(await db.AuditLogs.AsNoTracking().ToListAsync(Ct));
        Assert.Equal(
            [new AuditValueChange(nameof(Product.MaxQuantityPerOrder), null, "2"), new AuditValueChange(nameof(Product.FulfilmentPeriod), null, "3")],
            row.Details!.Changes);
    }

    [Fact]
    public async Task ANewProductsBirth_WithItsSettingsGiven_WritesNoTrailRow_KK4()
    {
        ProductSaveResult result = await RunAsync(Command(variants: [Row(sku: "KUPA-T")]) with { MaxQuantityPerOrder = 2 });

        Assert.False(result.IsRejected);
        await using AppDbContext db = NewContext();
        Assert.Equal(0, await db.AuditLogs.CountAsync(Ct));
    }

    [Fact]
    public async Task AMainImageMovedByTheGallerysBulkMark_RaisesTheVersion_AndASecondSaveOfTheOldVersionIsTheConflict_D361()
    {
        (Product product, Variant variant) = await SeedAsync(ProductType.Physical, stock: 5);
        Image first = Image.Create("gorsel-ana-1", ImageFormat.Jpeg, 800, 600, LongAgo);
        Image second = Image.Create("gorsel-ana-2", ImageFormat.Jpeg, 800, 600, LongAgo);
        await SaveAsync(c =>
        {
            c.Images.AddRange(first, second);
            c.ProductImages.Add(ProductImage.Create(product.Id, first.Id, 1, true, LongAgo));
            c.ProductImages.Add(ProductImage.Create(product.Id, second.Id, 2, false, LongAgo));
        });
        IReadOnlyList<GalleryEntry> mainMoved = [new GalleryEntry(first.Id, null, false), new GalleryEntry(second.Id, null, true)];

        ProductSaveResult moved = await RunAsync(Existing(product, variant, price: 100m, stock: 5, shown: 5) with { Gallery = mainMoved });
        ProductSaveResult stale = await RunAsync(Existing(product, variant, price: 100m, stock: 5, shown: 5) with { Gallery = [new GalleryEntry(first.Id, null, true), new GalleryEntry(second.Id, null, false)] });

        Assert.False(moved.IsRejected);
        Assert.Equal(ErrorCode.RecordVersionConflict, stale.Code);
        await using AppDbContext db = NewContext();
        Assert.Equal(product.Version + 1, await db.Products.Where(p => p.Id == product.Id).Select(p => p.Version).SingleAsync(Ct));
        Assert.Equal(second.Id, await db.ProductImages.Where(i => i.ProductId == product.Id && i.IsMain).Select(i => i.ImageId).SingleAsync(Ct));
    }

    // ---- D-279: the fences of P-6, P-9, P-44 are the firm settings' one home ----

    [Fact]
    public async Task TheMostPerOrderAndTheFulfilmentPeriod_BelowTheirFences_AreValueOutOfBounds_D279()
    {
        ProductSaveResult result = await RunAsync(Command(variants: [Row(sku: "KUPA-F")]) with
        {
            ProductType = ProductType.Service,
            MaxQuantityPerOrder = FirmSettingFences.BoundsOf("P-9").Lower - 1,
            FulfilmentPeriod = FirmSettingFences.BoundsOf("P-44").Lower - 1,
        });

        Assert.True(result.IsRejected);
        AssertField(result, nameof(ProductSaveCommand.MaxQuantityPerOrder), ErrorCode.ValueOutOfBounds);
        AssertField(result, nameof(ProductSaveCommand.FulfilmentPeriod), ErrorCode.ValueOutOfBounds);
    }

    [Fact]
    public async Task AShippingPeriodAboveItsFence_IsValueOutOfBounds_D279_D496()
    {
        ProductSaveResult result = await RunAsync(Command(variants: [Row(sku: "KUPA-S")]) with
        {
            ProductType = ProductType.Physical,
            OriginCountryId = _countryId,
            ShippingPeriod = FirmSettingFences.BoundsOf("P-6").Upper + 1,
            Variants = [Row(sku: "KUPA-S") with { StockQuantity = 1 }],
        });

        Assert.True(result.IsRejected);
        AssertField(result, nameof(ProductSaveCommand.ShippingPeriod), ErrorCode.ValueOutOfBounds);
    }

    // ---- D-362, D-381, D-401: the product's row lock orders the product's writes ----

    [Fact]
    public async Task TheFormsSave_AndAStockWriteOfTheSameProduct_TakeTheProductsRowInTurn_D362()
    {
        (Product product, Variant variant) = await SeedAsync(ProductType.Physical, stock: 5);
        await using AsyncServiceScope first = _provider.CreateAsyncScope();
        AppDbContext firstDb = first.ServiceProvider.GetRequiredService<AppDbContext>();
        await using IDbContextTransaction firstTransaction = await firstDb.Database.BeginTransactionAsync(Ct);
        ProductSaveResult saved = await first.ServiceProvider.GetRequiredService<ProductSaves>().SaveAsync(Existing(product, variant, price: 100m, stock: 5, shown: 5), Ct);
        Assert.False(saved.IsRejected);

        Task<StockQuantityWriteResult> stock = Task.Run(
            async () =>
            {
                await using AsyncServiceScope second = _provider.CreateAsyncScope();
                ICatalogModule catalog = second.ServiceProvider.GetRequiredService<ICatalogModule>();
                return await second.ServiceProvider.GetRequiredService<ITransactionRunner>().RunAsync(cancel => catalog.SaveStockQuantityAsync(variant.Id, 5, 9, cancel), Ct);
            },
            Ct);
        await WaitUntilATransactionWaitsOnALockAsync();
        Assert.False(stock.IsCompleted);

        await firstDb.SaveChangesAsync(Ct);
        await firstTransaction.CommitAsync(Ct);
        StockQuantityWriteResult written = await stock;

        Assert.False(written.IsShownValueChanged);
        await using AppDbContext db = NewContext();
        Assert.Equal(9, await db.Variants.Where(v => v.Id == variant.Id).Select(v => v.StockQuantity).SingleAsync(Ct));
    }

    // ---- KK5: the 422 reasons, each in its own test ----

    [Fact]
    public async Task APriceThatIsNoAmount_IsTheFieldsRule_AndNothingIsWritten_KK5()
    {
        ProductSaveResult result = await RunAsync(Command(variants: [Row(sku: "KUPA-3", price: 0.001m)]));

        Assert.Contains(result.FieldErrors, e => e.Field == "Variants[0].Price" && e.Code == ErrorCode.InvalidAmount);
        Assert.Equal(0L, await CountAsync("products"));
    }

    [Fact]
    public async Task ABlankNameAndABlankStockCode_BothTakeTheRequiredMessage_KK5()
    {
        ProductSaveResult result = await RunAsync(Command(name: "", variants: [Row(sku: null)]));

        Assert.Contains(result.FieldErrors, e => e.Field == "Name" && e.Code == ErrorCode.FieldRequired);
        Assert.Contains(result.FieldErrors, e => e.Field == "Variants[0].SKU" && e.Code == ErrorCode.FieldRequired);
    }

    [Fact]
    public async Task AStockCodeAnotherVariantHolds_IsRefused_KK5()
    {
        await SeedAsync(ProductType.Digital, stock: null, sku: "AYNI-KOD");

        ProductSaveResult result = await RunAsync(Command(variants: [Row(sku: "ayni-kod")]));

        Assert.Contains(result.FieldErrors, e => e.Field == "Variants[0].SKU" && e.Code == ErrorCode.DuplicateSku);
    }

    [Fact]
    public async Task AGalleryImageWhoseRowIsGone_IsTheUploadsField_KK5()
    {
        ProductSaveResult result = await RunAsync(Command(variants: [Row(sku: "KUPA-4")], gallery: [new GalleryEntry(Guid.NewGuid(), null, true)]));

        Assert.Contains(result.FieldErrors, e => e.Field == "Images" && e.Code == ErrorCode.UploadMissing);
    }

    [Fact]
    public async Task ACategoryDeletedMeanwhile_IsTheCategoryField_KK5()
    {
        Guid gone = Guid.NewGuid();

        ProductSaveResult result = await RunAsync(Command(variants: [Row(sku: "KUPA-5")], categories: [gone], primary: gone));

        Assert.Contains(result.FieldErrors, e => e.Field == "Categories" && e.Code == ErrorCode.InvalidListValue);
    }

    [Fact]
    public async Task AFileWhoseUploadHasNotCompleted_IsTheFilesField_KK5()
    {
        DigitalFile file = DigitalFile.Create(EntityIds.NewStorageKey(), "kitap.pdf", 1000, Now);
        await SaveAsync(c => c.DigitalFiles.Add(file));

        ProductSaveResult result = await RunAsync(Command(variants: [Row(sku: "KUPA-6")], digitalFileId: file.Id));

        Assert.Contains(result.FieldErrors, e => e.Field == "DigitalFileId" && e.Code == ErrorCode.UploadMissing);
    }

    [Fact]
    public async Task ADiscountWhoseRoundedPriceReachesZero_IsRefusedAtTheVariant_KK5()
    {
        (Product product, Variant variant) = await SeedAsync(ProductType.Digital, stock: null, price: 0.01m);

        ProductSaveResult result = await RunAsync(Existing(product, variant, price: 0.01m, stock: null, shown: null) with
        {
            Discount = new ProductSaveDiscount(60m, Today, Today.AddDays(2)),
        });

        Assert.Contains(result.FieldErrors, e => e.Field == "Variants[0].Price" && e.Code == ErrorCode.DiscountedPriceZero);
        Assert.Equal(0L, await CountAsync("discounts"));
    }

    [Fact]
    public async Task APublishedProductsSave_ThatEmptiesItsName_BreaksItsGate_KK5()
    {
        (Product product, Variant variant) = await SeedAsync(ProductType.Digital, stock: null, published: true);
        await using AppDbContext read = NewContext();
        Guid fileId = await read.DigitalFiles.Where(f => f.ProductId == product.Id).Select(f => f.Id).SingleAsync(Ct);

        ProductSaveResult result = await RunAsync(Existing(product, variant, price: 100m, stock: null, shown: null) with { Name = "", DigitalFileId = fileId });

        Assert.Contains(result.FieldErrors, e => e.Field == "Name" && e.Code == ErrorCode.PublishedRecordGateBroken);
    }

    [Fact]
    public async Task AStockBelowTheReservedQuantity_IsRefusedWithTheReservedQuantity_KK5()
    {
        (Product product, Variant variant) = await SeedAsync(ProductType.Physical, stock: 5);
        await ExecuteAsync($"UPDATE variants SET reserved_quantity = 3 WHERE id = '{variant.Id}'");

        ProductSaveResult result = await RunAsync(Existing(product, variant, price: 100m, stock: 2, shown: 5));

        ProductSaveFieldError error = AssertField(result, "Variants[0].StockQuantity", ErrorCode.StockBelowReserved);
        Assert.Equal(3m, error.Amount);
        await using AppDbContext db = NewContext();
        Assert.Equal(5, await db.Variants.Where(v => v.Id == variant.Id).Select(v => v.StockQuantity).SingleAsync(Ct));
    }

    [Theory]
    [InlineData("1.250", false)]
    [InlineData("1.2505", true)]
    public async Task TheNetQuantity_TakesThreeDecimals_FromItsOneRule_KK5(string netQuantity, bool refused)
    {
        decimal quantity = decimal.Parse(netQuantity, System.Globalization.CultureInfo.InvariantCulture);

        ProductSaveResult result = await RunAsync(Command(unit: MeasurementUnit.Kilogram, variants: [Row(sku: "KUPA-7") with { NetQuantity = quantity }]));

        Assert.Equal(refused, result.IsRejected);
        if (refused)
        {
            Assert.Contains(result.FieldErrors, e => e.Field == "Variants[0].NetQuantity" && e.Code == ErrorCode.InvalidAmount);
        }
    }

    [Fact]
    public async Task RemovingTheFileOfAPublishedDigitalVariant_IsRefused_KK5()
    {
        (Product product, Variant variant) = await SeedAsync(ProductType.Digital, stock: null, published: true);

        ProductSaveResult result = await RunAsync(Existing(product, variant, price: 100m, stock: null, shown: null));

        Assert.Contains(result.FieldErrors, e => e.Field == "Variants[0].DigitalFileId" && e.Code == ErrorCode.DigitalFileRequired);
        await using AppDbContext db = NewContext();
        Assert.True(await db.DigitalFiles.AnyAsync(f => f.ProductId == product.Id && f.DetachedAt == null, Ct));
    }

    // ---- KK6: the version's 409 writes nothing ----

    [Fact]
    public async Task AStaleVersion_IsTheConflict_AndNothingIsWritten_KK6()
    {
        (Product product, Variant variant) = await SeedAsync(ProductType.Physical, stock: 5);

        ProductSaveResult result = await RunAsync(Existing(product, variant, price: 140m, stock: 5, shown: 5) with { Version = product.Version + 7 });

        Assert.Equal(ErrorCode.RecordVersionConflict, result.Code);
        await using AppDbContext db = NewContext();
        Assert.Equal(100m, await db.Variants.Where(v => v.Id == variant.Id).Select(v => v.Price).SingleAsync(Ct));
    }

    private static ProductSaveFieldError AssertField(ProductSaveResult result, string field, ErrorCode code)
    {
        Assert.True(result.IsRejected);
        return Assert.Single(result.FieldErrors, e => e.Field == field && e.Code == code);
    }

    private static ProductSaveCommand Command(
        string? name = "Seramik Kupa",
        string? description = null,
        MeasurementUnit? unit = null,
        IReadOnlyList<ProductSaveVariant>? variants = null,
        IReadOnlyList<GalleryEntry>? gallery = null,
        IReadOnlyList<Guid>? categories = null,
        Guid? primary = null,
        Guid? digitalFileId = null)
        => new(
            null, null, ProductType.Digital, name, description, 20m, null, unit, null, null, null, null,
            categories ?? [], primary, [], variants ?? [], gallery ?? [], digitalFileId, null);

    private static ProductSaveCommand Existing(Product product, Variant variant, decimal price, int? stock, int? shown)
        => new(
            product.Id, product.Version, product.ProductType, product.Name, null, product.VatRate, product.OriginCountryId, null, null, null, null, null,
            [], null, [], [Row(sku: variant.SKU, price: price) with { VariantId = variant.Id, StockQuantity = stock, ShownStockQuantity = shown }], [], null, null);

    private static ProductSaveVariant Row(string? sku, decimal price = 100m) => new(null, null, null, sku, price, null, null, null, null, null);

    private async Task<(Product Product, Variant Variant)> SeedAsync(ProductType type, int? stock, string? sku = null, decimal price = 100m, bool published = false)
    {
        int n = ++_counter;
        Product product = Product.Create($"Urun {n}", $"urun-{n}", type, 20m, LongAgo);
        Variant variant = Variant.Create(product.Id, sku ?? $"SKU-{n}", price, LongAgo, stock);
        if (type == ProductType.Physical)
        {
            product.OriginCountryId = _countryId;
        }

        Category? category = null;
        if (published)
        {
            category = Category.Create($"Kategori {n}", $"kategori-{n}", null, 1, LongAgo);
            product.PublishStatus = PublishStatus.Published;
            variant.PublishStatus = PublishStatus.Published;
        }

        DigitalFile? file = null;
        if (type == ProductType.Digital && published)
        {
            file = DigitalFile.Create(EntityIds.NewStorageKey(), "dosya.pdf", 100, LongAgo);
            file.UploadCompletedAt = LongAgo;
            file.ProductId = product.Id;
            file.AttachedAt = LongAgo;
        }

        await SaveAsync(c =>
        {
            c.Products.Add(product);
            c.Variants.Add(variant);
            c.PriceHistories.Add(PriceHistory.Create(variant.Id, price, LongAgo, LongAgo));
            if (category is not null)
            {
                c.Categories.Add(category);
                c.ProductCategories.Add(ProductCategory.Create(product.Id, category.Id, true, LongAgo));
            }

            if (file is not null)
            {
                c.DigitalFiles.Add(file);
            }
        });
        return (product, variant);
    }

    private async Task<ProductSaveResult> RunAsync(ProductSaveCommand command)
    {
        await using AsyncServiceScope scope = _provider.CreateAsyncScope();
        AppDbContext db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await using IDbContextTransaction transaction = await db.Database.BeginTransactionAsync(Ct);
        ProductSaveResult result = await scope.ServiceProvider.GetRequiredService<ProductSaves>().SaveAsync(command, Ct);
        if (result.IsRejected)
        {
            await transaction.RollbackAsync(Ct);
            return result;
        }

        await db.SaveChangesAsync(Ct);
        await transaction.CommitAsync(Ct);
        return result;
    }

    /// <summary>The module's services as the application registers them (the save's lock, the items' file links and the trail are its dependencies), over the collection's database.</summary>
    private ServiceProvider BuildProvider()
    {
        ServiceCollection services = new();
        services.AddDbContext<AppDbContext>(options => AppDbContext.Configure(options, _collection.Dsn));
        services.AddScoped<JobEnqueueTracker>();
        services.AddSingleton<IJobSignal, JobSignal>();
        services.AddScoped<ITransactionRunner, TransactionRunner>();
        services.AddAppCache();
        services.AddScoped<AdvisoryLocks>();
        services.AddSingleton<TimeProvider>(_time);
        services.AddScoped<IJobQueue, JobQueue>();
        services.AddFileStore();
        services.AddOptions<Shopfolio.Infrastructure.InstallationSettings.StatusThresholdOptions>();
        services.AddSingleton<Shopfolio.Infrastructure.Status.IDiskUsageReader, Shopfolio.Infrastructure.Status.DriveDiskUsageReader>();
        services.AddSingleton<Shopfolio.Infrastructure.Status.DiskUsageGuard>();
        services.AddSingleton<ImageProcessor>();
        services.AddCatalogModule();
        services.AddAfterSalesModule();
        services.AddPaymentModule();
        services.AddOrderingModule();
        services.AddScoped<IAuditActorSource>(_ => new FixedActor(_admin));
        services.AddScoped<IAuditTrail, AuditTrail>();
        return services.BuildServiceProvider();
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

    /// <summary>Waits until a transaction of this collection's database waits on a row lock (the second writer behind the product's row lock).</summary>
    private async Task WaitUntilATransactionWaitsOnALockAsync()
    {
        for (int attempt = 0; attempt < 400; attempt++)
        {
            await using NpgsqlConnection connection = new(_collection.Dsn);
            await connection.OpenAsync(Ct);
            await using NpgsqlCommand command = new(
                "SELECT count(*) FROM pg_stat_activity WHERE datname = current_database() AND wait_event_type = 'Lock' AND query ILIKE '%FOR UPDATE%'", connection);
            if ((long)(await command.ExecuteScalarAsync(Ct))! > 0)
            {
                return;
            }

            await Task.Delay(25, Ct);
        }

        Assert.Fail("No transaction waited on the product's row lock.");
    }

    private async Task<long> CountAsync(string table)
    {
        await using NpgsqlConnection connection = new(_collection.Dsn);
        await connection.OpenAsync(Ct);
        await using NpgsqlCommand command = new($"SELECT count(*) FROM {table}", connection);
        return (long)(await command.ExecuteScalarAsync(Ct))!;
    }

    private sealed class FixedTime(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }

    private sealed class FixedActor(Admin admin) : IAuditActorSource
    {
        public Task<AuditActor?> CurrentAsync(CancellationToken ct) => Task.FromResult<AuditActor?>(new AuditActor(admin.Id, admin.Name, admin.Email));
    }
}
