using Microsoft.EntityFrameworkCore;

using Npgsql;

using Shopfolio.Infrastructure;
using Shopfolio.IntegrationTests.Database;
using Shopfolio.Modules.Catalog;
using Shopfolio.Modules.Catalog.Contracts;

namespace Shopfolio.IntegrationTests.Model;

/// <summary>The collection of the product gallery tests (T339): one database copied from the template, made by the real migrations.</summary>
[CollectionDefinition(ProductGalleryDefinition.Name)]
public sealed class ProductGalleryDefinition : ICollectionFixture<CollectionDatabaseFixture>
{
    public const string Name = "ProductGallery";
}

/// <summary>
/// The product's image and text through the real Catalog code on a real PostgreSQL (T339 criteria 1-9; KK1-KK9; 02 3.11.1-3.11.6; 06 4.6, 4.71): the main image is a mark apart from the order and
/// a gallery with images has exactly one; the limit counts the gallery's and the variants' images together, each once; the alternative text is the firm's or a generated one that is never empty;
/// the name has its ceiling and the description none and is cleaned; no table or column carries a warranty. A refusal writes nothing. The format and the size of an uploaded file (KK4) and its
/// metadata and orientation (KK5) are T223's tests: Unit <c>ImageProcessorTests.TheOrientationIsAppliedToThePixels_AndNoMetadataIsLeft</c> and
/// <c>AMovingWebP_IsEncodedFromItsFirstFrame_AsOneFrame</c>; Integration <c>Files/ImageFilesTests</c> (the size ceiling) and <c>ImageUploadResult.UnsupportedType</c>.
/// </summary>
[Collection(ProductGalleryDefinition.Name)]
public sealed class ProductGalleryTests : IAsyncLifetime
{
    private static readonly DateTimeOffset Now = new(2026, 11, 7, 9, 0, 0, TimeSpan.Zero);

    private static readonly string[] WarrantyWords = ["warranty", "guarantee", "garanti"];

    private readonly CollectionDatabaseFixture _collection;

    public ProductGalleryTests(CollectionDatabaseFixture collection)
    {
        _collection = collection;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public async ValueTask InitializeAsync()
    {
        await ExecuteAsync("TRUNCATE product_images, variants, option_values, options, products, images CASCADE");
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    // ---- KK1: the variant shows its own image, or the product's main image ----

    [Fact]
    public async Task AVariantWithoutItsOwnImage_ShowsTheProductsMainImage_AndAVariantWithOneShowsItsOwn()
    {
        (Product product, Variant variant) = await SeedProductWithVariantAsync("kk1");
        List<Image> images = await SeedImagesAsync(3);
        Assert.Equal(GallerySaveResult.Saved, await InTransactionAsync(g => g.SaveAsync(product.Id, Entries(images, mainIndex: 1), Ct)));

        VariantImage? inherited = await ReadAsync(g => g.GetVariantImageAsync(variant.Id, Ct));
        Assert.NotNull(inherited);
        Assert.False(inherited.IsOwn);
        Assert.Equal(images[1].Id, inherited.ImageId);

        Assert.Equal(VariantImageResult.Set, await InTransactionAsync(g => g.SetVariantImageAsync(variant.Id, images[2].Id, Ct)));
        VariantImage? own = await ReadAsync(g => g.GetVariantImageAsync(variant.Id, Ct));
        Assert.NotNull(own);
        Assert.True(own.IsOwn);
        Assert.Equal(images[2].Id, own.ImageId);

        Assert.Equal(VariantImageResult.Set, await InTransactionAsync(g => g.SetVariantImageAsync(variant.Id, null, Ct)));
        Assert.False((await ReadAsync(g => g.GetVariantImageAsync(variant.Id, Ct)))!.IsOwn);
    }

    [Fact]
    public async Task AVariantOfAProductWithoutImages_ShowsNone()
    {
        (_, Variant variant) = await SeedProductWithVariantAsync("kk1-bos");

        Assert.Null(await ReadAsync(g => g.GetVariantImageAsync(variant.Id, Ct)));
    }

    // ---- KK2: the main image ----

    [Fact]
    public async Task TheFirstImageAdded_BecomesMain_WhenTheListMarksNone()
    {
        (Product product, _) = await SeedProductWithVariantAsync("kk2-ilk");
        List<Image> images = await SeedImagesAsync(3);

        await InTransactionAsync(g => g.SaveAsync(product.Id, Entries(images, mainIndex: null), Ct));

        Assert.Equal(images[0].Id, await MainOfAsync(product.Id));
        Assert.Equal(1L, await MainCountAsync(product.Id));
    }

    [Fact]
    public async Task WhenTheMainImageLeaves_TheFirstImageInTheOrderBecomesMain()
    {
        (Product product, _) = await SeedProductWithVariantAsync("kk2-ayrilir");
        List<Image> images = await SeedImagesAsync(4);
        await InTransactionAsync(g => g.SaveAsync(product.Id, Entries(images, mainIndex: 2), Ct));
        Assert.Equal(images[2].Id, await MainOfAsync(product.Id));

        List<GalleryEntry> without = [.. Entries(images, mainIndex: null).Where(e => e.ImageId != images[2].Id)];
        await InTransactionAsync(g => g.SaveAsync(product.Id, without, Ct));

        Assert.Equal(images[0].Id, await MainOfAsync(product.Id));
        Assert.Equal(1L, await MainCountAsync(product.Id));
    }

    [Fact]
    public async Task TheMainMark_IsIndependentOfTheOrder_AndMovingItNeverBreaksTheOneMainIndex()
    {
        (Product product, _) = await SeedProductWithVariantAsync("kk2-sira");
        List<Image> images = await SeedImagesAsync(3);
        await InTransactionAsync(g => g.SaveAsync(product.Id, Entries(images, mainIndex: 0), Ct));

        // The main mark moves to the last image while the order is reversed: the mark follows the image, not the place.
        List<Image> reversed = [.. images.AsEnumerable().Reverse()];
        await InTransactionAsync(g => g.SaveAsync(product.Id, Entries(reversed, mainIndex: 0), Ct));
        Assert.Equal(images[2].Id, await MainOfAsync(product.Id));

        await InTransactionAsync(g => g.SaveAsync(product.Id, Entries(reversed, mainIndex: 2), Ct));
        Assert.Equal(images[0].Id, await MainOfAsync(product.Id));
        Assert.Equal(1L, await MainCountAsync(product.Id));

        // A list that marks none keeps the current main image where it is.
        await InTransactionAsync(g => g.SaveAsync(product.Id, Entries(images, mainIndex: null), Ct));
        Assert.Equal(images[0].Id, await MainOfAsync(product.Id));
    }

    [Fact]
    public async Task AListThatMarksSeveralImages_TakesTheFirstMarkedOne()
    {
        (Product product, _) = await SeedProductWithVariantAsync("kk2-cok");
        List<Image> images = await SeedImagesAsync(3);
        List<GalleryEntry> entries = [new(images[0].Id, null, false), new(images[1].Id, null, true), new(images[2].Id, null, true)];

        await InTransactionAsync(g => g.SaveAsync(product.Id, entries, Ct));

        Assert.Equal(images[1].Id, await MainOfAsync(product.Id));
        Assert.Equal(1L, await MainCountAsync(product.Id));
    }

    // ---- KK3: the order ----

    [Fact]
    public async Task TheOrderOfTheList_IsTheOrderOfTheGallery()
    {
        (Product product, _) = await SeedProductWithVariantAsync("kk3");
        List<Image> images = await SeedImagesAsync(4);
        await InTransactionAsync(g => g.SaveAsync(product.Id, Entries(images, mainIndex: 0), Ct));

        List<Image> reordered = [images[3], images[1], images[0], images[2]];
        await InTransactionAsync(g => g.SaveAsync(product.Id, Entries(reordered, mainIndex: null), Ct));

        IReadOnlyList<GalleryImage> gallery = await ReadAsync(g => g.ReadAsync(product.Id, Ct));
        Assert.Equal(reordered.Select(i => i.Id), gallery.Select(i => i.ImageId));
        Assert.Equal([1, 2, 3, 4], gallery.Select(i => i.Position));
        Assert.Equal(images[0].Id, gallery.Single(i => i.IsMain).ImageId);
    }

    // ---- KK4: the count ceiling (P-24) ----

    [Fact]
    public async Task TheGallery_TakesTheCeilingAndRefusesOneMore_AndWritesNothingOnTheRefusal()
    {
        (Product product, _) = await SeedProductWithVariantAsync("kk4-galeri");
        List<Image> images = await SeedImagesAsync(ProductConstants.ImagesPerProductMax + 1);
        List<Image> ceiling = images.Take(ProductConstants.ImagesPerProductMax).ToList();

        Assert.Equal(GallerySaveResult.Saved, await InTransactionAsync(g => g.SaveAsync(product.Id, Entries(ceiling, mainIndex: null), Ct)));
        Assert.Equal(GallerySaveResult.LimitExceeded, await InTransactionAsync(g => g.SaveAsync(product.Id, Entries(images, mainIndex: 5), Ct)));

        Assert.Equal((long)ProductConstants.ImagesPerProductMax, await ScalarAsync<long>($"SELECT count(*) FROM product_images WHERE product_id = '{product.Id}'"));
        Assert.Equal(ceiling[0].Id, await MainOfAsync(product.Id));
    }

    [Fact]
    public async Task TheVariantsImages_CountWithTheGalleryOnes_AndAnImageUsedTwiceCountsOnce()
    {
        (Product product, OptionValue red, _, Variant variant) = await SeedVariantWithValuesAsync("kk4-varyant");
        List<Image> images = await SeedImagesAsync(ProductConstants.ImagesPerProductMax + 1);
        List<Image> galleryImages = images.Take(ProductConstants.ImagesPerProductMax - 1).ToList();
        Assert.Equal(GallerySaveResult.Saved, await InTransactionAsync(g => g.SaveAsync(product.Id, Entries(galleryImages, mainIndex: null), Ct)));

        // The same image in the gallery and on the variant: one image, so the ceiling is not reached.
        Assert.Equal(VariantImageResult.Set, await InTransactionAsync(g => g.SetVariantImageAsync(variant.Id, galleryImages[0].Id, Ct)));
        // A new image brings the count to the ceiling exactly.
        Assert.Equal(VariantImageResult.Set, await InTransactionAsync(g => g.SetVariantImageAsync(variant.Id, images[ProductConstants.ImagesPerProductMax - 1].Id, Ct)));

        // The variant's own image now counts: one more gallery image is over the ceiling.
        List<Image> oneMore = [.. galleryImages, images[ProductConstants.ImagesPerProductMax]];
        Assert.Equal(GallerySaveResult.LimitExceeded, await InTransactionAsync(g => g.SaveAsync(product.Id, Entries(oneMore, mainIndex: null), Ct)));

        // A second variant cannot take a further new image either.
        Variant other = await AddVariantAsync(product, red.OptionId, "KK4-02");
        Assert.Equal(VariantImageResult.LimitExceeded, await InTransactionAsync(g => g.SetVariantImageAsync(other.Id, images[ProductConstants.ImagesPerProductMax].Id, Ct)));
        Assert.Null(await ScalarOrNullAsync<Guid>($"SELECT image_id FROM variants WHERE id = '{other.Id}'"));

        // ... but it can take one that is already counted.
        Assert.Equal(VariantImageResult.Set, await InTransactionAsync(g => g.SetVariantImageAsync(other.Id, galleryImages[1].Id, Ct)));
    }

    [Fact]
    public async Task ARefusedWrite_NamesItsReason_AndWritesNothing()
    {
        (Product product, Variant variant) = await SeedProductWithVariantAsync("kk4-ret");
        List<Image> images = await SeedImagesAsync(2);
        Guid missing = EntityIds.New();

        Assert.Equal(GallerySaveResult.ProductNotFound, await InTransactionAsync(g => g.SaveAsync(missing, Entries(images, mainIndex: null), Ct)));
        Assert.Equal(GallerySaveResult.DuplicateImage, await InTransactionAsync(g => g.SaveAsync(product.Id, [new(images[0].Id, null, false), new(images[0].Id, null, true)], Ct)));
        Assert.Equal(GallerySaveResult.ImageNotFound, await InTransactionAsync(g => g.SaveAsync(product.Id, [new(images[0].Id, null, false), new(missing, null, false)], Ct)));
        Assert.Equal(VariantImageResult.VariantNotFound, await InTransactionAsync(g => g.SetVariantImageAsync(missing, images[0].Id, Ct)));
        Assert.Equal(VariantImageResult.ImageNotFound, await InTransactionAsync(g => g.SetVariantImageAsync(variant.Id, missing, Ct)));

        Assert.Equal(0L, await ScalarAsync<long>("SELECT count(*) FROM product_images"));
        Assert.Null(await ScalarOrNullAsync<Guid>($"SELECT image_id FROM variants WHERE id = '{variant.Id}'"));
    }

    [Fact]
    public async Task TheGalleryWrites_AreRefusedOutsideATransaction()
    {
        (Product product, Variant variant) = await SeedProductWithVariantAsync("kk4-islem");
        List<Image> images = await SeedImagesAsync(1);
        await using AppDbContext context = NewContext();
        ProductGallery gallery = new(context, TimeProvider.System);

        await Assert.ThrowsAsync<InvalidOperationException>(() => gallery.SaveAsync(product.Id, Entries(images, mainIndex: null), Ct));
        await Assert.ThrowsAsync<InvalidOperationException>(() => gallery.SetVariantImageAsync(variant.Id, images[0].Id, Ct));
    }

    // ---- KK6: the alternative text ----

    [Fact]
    public async Task TheAlternativeText_IsTheFirmsWhenWritten_AndGeneratedWhenEmpty()
    {
        (Product product, _) = await SeedProductWithVariantAsync("kk6-galeri", name: "Pamuk Tişört");
        List<Image> images = await SeedImagesAsync(3);
        List<GalleryEntry> entries = [new(images[0].Id, "  Önden görünüm  ", true), new(images[1].Id, "   ", false), new(images[2].Id, null, false)];

        await InTransactionAsync(g => g.SaveAsync(product.Id, entries, Ct));

        IReadOnlyList<GalleryImage> gallery = await ReadAsync(g => g.ReadAsync(product.Id, Ct));
        Assert.Equal("Önden görünüm", gallery[0].AltText);
        Assert.Equal("Önden görünüm", gallery[0].EffectiveAltText);
        Assert.Null(gallery[1].AltText);
        Assert.Equal("Pamuk Tişört — görsel 2 / 3", gallery[1].EffectiveAltText);
        Assert.Equal("Pamuk Tişört — görsel 3 / 3", gallery[2].EffectiveAltText);
        Assert.DoesNotContain(gallery, i => string.IsNullOrWhiteSpace(i.EffectiveAltText));
    }

    [Fact]
    public async Task TheCardText_IsTheMainImagesFirmText_ElseTheProductName()
    {
        (Product product, _) = await SeedProductWithVariantAsync("kk6-kart", name: "Pamuk Tişört");
        List<Image> images = await SeedImagesAsync(2);
        await InTransactionAsync(g => g.SaveAsync(product.Id, [new(images[0].Id, null, true), new(images[1].Id, "İkinci", false)], Ct));
        Assert.Equal("Pamuk Tişört", await ReadAsync(g => g.GetCardAltTextAsync(product.Id, Ct)));

        await InTransactionAsync(g => g.SaveAsync(product.Id, [new(images[0].Id, "Ana görsel metni", true), new(images[1].Id, "İkinci", false)], Ct));
        Assert.Equal("Ana görsel metni", await ReadAsync(g => g.GetCardAltTextAsync(product.Id, Ct)));

        Assert.Null(await ReadAsync(g => g.GetCardAltTextAsync(EntityIds.New(), Ct)));
    }

    [Fact]
    public async Task AVariantsOwnImageText_NamesTheProductAndTheValues_UnlessTheGalleryHoldsTheSameImageWithAText()
    {
        (Product product, OptionValue red, OptionValue sizeM, Variant variant) = await SeedVariantWithValuesAsync("kk6-varyant");
        List<Image> images = await SeedImagesAsync(2);
        await InTransactionAsync(g => g.SetVariantImageAsync(variant.Id, images[0].Id, Ct));

        VariantImage? generated = await ReadAsync(g => g.GetVariantImageAsync(variant.Id, Ct));
        Assert.Equal($"{product.Name} — {red.Value} / {sizeM.Value}", generated!.AltText);

        await InTransactionAsync(g => g.SaveAsync(product.Id, [new(images[0].Id, "Kırmızı M, önden", true), new(images[1].Id, null, false)], Ct));
        Assert.Equal("Kırmızı M, önden", (await ReadAsync(g => g.GetVariantImageAsync(variant.Id, Ct)))!.AltText);
    }

    // ---- KK7: the name has its ceiling, the description has none and is cleaned ----

    [Fact]
    public async Task TheDescription_HasNoCeiling_AndIsCleanedOnItsWayThroughTheDatabase()
    {
        string paragraphs = string.Concat(Enumerable.Repeat("<p>Satır <strong>kalın</strong></p>", 4000));
        Assert.True(paragraphs.Length > 100_000);
        Product product = NewProduct("kk7");
        product.SetDescription(paragraphs + "<script>alert(1)</script><p>Son<br>satır</p>");
        await SaveAsync(c => c.Products.Add(product));

        await using AppDbContext context = NewContext();
        string stored = (await context.Products.AsNoTracking().SingleAsync(p => p.Id == product.Id, Ct)).Description!;

        Assert.True(stored.Length > 100_000);
        Assert.DoesNotContain("script", stored, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("alert", stored, StringComparison.OrdinalIgnoreCase);
        Assert.EndsWith("<p>Son<br>satır</p>", stored, StringComparison.Ordinal);
        Assert.Contains("<strong>kalın</strong>", stored, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ADescriptionOfEmptyOrWhiteSpace_IsStoredAsNone()
    {
        Product product = NewProduct("kk7-bos");
        product.SetDescription("  \n ");
        await SaveAsync(c => c.Products.Add(product));

        Assert.Null(await ScalarOrNullAsync($"SELECT description FROM products WHERE id = '{product.Id}'"));
    }

    [Fact]
    public async Task TheNameTakesItsCeiling_AndOneMoreCharacterIsRefusedByTheDatabase()
    {
        // The ceiling itself is proven by CatalogCoreTablesTests.ProductNameLongerThanTheCeiling_IsRefused; here the full length is accepted.
        Product product = NewProduct("kk7-ad");
        product.SetName(new string('a', ProductConstants.ProductNameMaxLength));
        await SaveAsync(c => c.Products.Add(product));

        Assert.Equal(1L, await ScalarAsync<long>($"SELECT count(*) FROM products WHERE id = '{product.Id}' AND length(name) = {ProductConstants.ProductNameMaxLength}"));

        Product longer = NewProduct("kk7-ad-uzun");
        longer.SetName(new string('a', ProductConstants.ProductNameMaxLength + 1));
        DbUpdateException error = await Assert.ThrowsAsync<DbUpdateException>(() => SaveAsync(c => c.Products.Add(longer)));
        Assert.Equal(PostgresErrorCodes.StringDataRightTruncation, Assert.IsType<PostgresException>(error.InnerException).SqlState);
    }

    // ---- KK9: no warranty ----

    [Fact]
    public async Task NoTableOrColumn_CarriesAWarrantyOrAGuarantee()
    {
        List<string> names = await QueryAsync(
            "SELECT table_name || '.' || column_name FROM information_schema.columns WHERE table_schema = 'public' UNION ALL SELECT table_name FROM information_schema.tables WHERE table_schema = 'public'");

        Assert.NotEmpty(names);
        Assert.DoesNotContain(names, n => WarrantyWords.Any(w => n.Contains(w, StringComparison.OrdinalIgnoreCase)));
    }

    // ---- helpers ----

    private static List<GalleryEntry> Entries(IReadOnlyList<Image> images, int? mainIndex)
    {
        return [.. images.Select((image, i) => new GalleryEntry(image.Id, null, mainIndex == i))];
    }

    private static Product NewProduct(string slug, string? name = null)
    {
        return Product.Create(name ?? "Ürün " + slug, slug, ProductType.Physical, 20m, Now);
    }

    private async Task<(Product Product, Variant Variant)> SeedProductWithVariantAsync(string slug, string? name = null)
    {
        Product product = NewProduct(slug, name);
        Variant variant = Variant.Create(product.Id, slug.ToUpperInvariant(), 100m, Now);
        await SaveAsync(c =>
        {
            c.Products.Add(product);
            c.Variants.Add(variant);
        });
        return (product, variant);
    }

    private async Task<Variant> AddVariantAsync(Product product, Guid optionId, string sku)
    {
        OptionValue value = OptionValue.Create(optionId, "Mavi", 2, Now);
        Variant variant = Variant.Create(product.Id, sku, 100m, Now);
        variant.FirstOptionValueId = value.Id;
        await SaveAsync(c =>
        {
            c.OptionValues.Add(value);
            c.Variants.Add(variant);
        });
        return variant;
    }

    private async Task<(Product Product, OptionValue Red, OptionValue SizeM, Variant Variant)> SeedVariantWithValuesAsync(string slug)
    {
        Product product = NewProduct(slug, "Pamuk Tişört");
        Option color = Option.Create(product.Id, "Renk", 1, Now);
        Option size = Option.Create(product.Id, "Beden", 2, Now);
        OptionValue red = OptionValue.Create(color.Id, "Kırmızı", 1, Now);
        OptionValue sizeM = OptionValue.Create(size.Id, "M", 1, Now);
        Variant variant = Variant.Create(product.Id, "KK6-01", 100m, Now);
        variant.FirstOptionValueId = red.Id;
        variant.SecondOptionValueId = sizeM.Id;
        await SaveAsync(c =>
        {
            c.Products.Add(product);
            c.Options.AddRange(color, size);
            c.OptionValues.AddRange(red, sizeM);
            c.Variants.Add(variant);
        });
        return (product, red, sizeM, variant);
    }

    private async Task<List<Image>> SeedImagesAsync(int count)
    {
        List<Image> images = [.. Enumerable.Range(0, count).Select(i => Image.Create($"images/{EntityIds.New():N}", ImageFormat.Jpeg, 800, 600, Now.AddSeconds(i)))];
        await SaveAsync(c => c.Images.AddRange(images));
        return images;
    }

    /// <summary>Runs one gallery write inside the caller's transaction as the entry point's runner does: write, save, commit; a refusal is not committed.</summary>
    private async Task<T> InTransactionAsync<T>(Func<ProductGallery, Task<T>> write)
        where T : IOperationResult
    {
        await using AppDbContext context = NewContext();
        await using var transaction = await context.Database.BeginTransactionAsync(Ct);
        T result = await write(new ProductGallery(context, TimeProvider.System));
        if (result.IsRejected)
        {
            await transaction.RollbackAsync(Ct);
            return result;
        }

        await context.SaveChangesAsync(Ct);
        await transaction.CommitAsync(Ct);
        return result;
    }

    private async Task<T> ReadAsync<T>(Func<ProductGallery, Task<T>> read)
    {
        await using AppDbContext context = NewContext();
        return await read(new ProductGallery(context, TimeProvider.System));
    }

    private Task<Guid?> MainOfAsync(Guid productId) => ScalarOrNullAsync<Guid>($"SELECT image_id FROM product_images WHERE product_id = '{productId}' AND is_main");

    private Task<long> MainCountAsync(Guid productId) => ScalarAsync<long>($"SELECT count(*) FROM product_images WHERE product_id = '{productId}' AND is_main");

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

    private async Task<T?> ScalarOrNullAsync<T>(string sql)
        where T : struct
    {
        await using NpgsqlConnection connection = new(_collection.Dsn);
        await connection.OpenAsync(Ct);
        await using NpgsqlCommand command = new(sql, connection);
        object? value = await command.ExecuteScalarAsync(Ct);
        return value is null or DBNull ? null : (T)value;
    }

    private async Task<string?> ScalarOrNullAsync(string sql)
    {
        await using NpgsqlConnection connection = new(_collection.Dsn);
        await connection.OpenAsync(Ct);
        await using NpgsqlCommand command = new(sql, connection);
        object? value = await command.ExecuteScalarAsync(Ct);
        return value is null or DBNull ? null : (string)value;
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
