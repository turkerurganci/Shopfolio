using Shopfolio.Infrastructure;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Catalog.Contracts.Categories;
using Shopfolio.Modules.Catalog.Contracts.Coupons;
using Shopfolio.Modules.Catalog.Contracts.Lists;
using Shopfolio.Modules.Catalog.Contracts.Pricing;
using Shopfolio.Modules.Catalog.Contracts.Variants;
using Shopfolio.Modules.CorporateContent.Contracts;
using Shopfolio.Modules.Settings;
using Shopfolio.Modules.Settings.Contracts;

namespace Shopfolio.UnitTests.Infrastructure;

/// <summary>
/// The site map's cache owner without a database (T313 criterion 9, KK9; 09 10.4 items 1, 2 and 6; 05 3.7.3, 7.10.1): the two modules are read once and the second
/// call comes from the cache; an invalidation of the site map area makes the next call read again; another area does not touch it; the read goes through the
/// modules' Contracts and the fixed list pages come with it.
/// </summary>
public sealed class SitemapSourceTests
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task TheSecondCall_ComesFromTheCache_AndReadsNeitherModule()
    {
        AppCache cache = new();
        FakeCatalog catalog = new(["/urun/masa"]);
        FakeContent content = new(["/hizmet/montaj"]);
        SitemapSource source = new(cache, catalog, content);

        SitemapView first = await source.GetAsync(Ct);
        SitemapView second = await source.GetAsync(Ct);

        Assert.Same(first, second);
        Assert.Equal(1, catalog.Reads);
        Assert.Equal(1, content.Reads);
        Assert.Equal([.. Shopfolio.Infrastructure.SearchEngine.SiteMapFiles.FixedListPaths, "/hizmet/montaj", "/urun/masa"], first.Paths);
    }

    [Fact]
    public async Task AnInvalidationOfTheSiteMapArea_MakesTheNextCallReadAgain_AndShowsTheNewRecord()
    {
        AppCache cache = new();
        FakeCatalog catalog = new(["/urun/masa"]);
        SitemapSource source = new(cache, catalog, new FakeContent([]));
        await source.GetAsync(Ct);

        catalog.Paths = ["/urun/masa", "/urun/sandalye"];
        cache.Invalidate(CacheArea.Sitemap);
        SitemapView after = await source.GetAsync(Ct);

        Assert.Equal(2, catalog.Reads);
        Assert.Contains("/urun/sandalye", after.Paths);
    }

    [Fact]
    public async Task AnInvalidationOfAnotherArea_LeavesTheSiteMapInTheCache()
    {
        AppCache cache = new();
        FakeCatalog catalog = new(["/urun/masa"]);
        SitemapSource source = new(cache, catalog, new FakeContent([]));
        await source.GetAsync(Ct);

        foreach (CacheArea area in Enum.GetValues<CacheArea>().Where(a => a != CacheArea.Sitemap))
        {
            cache.Invalidate(area);
        }

        await source.GetAsync(Ct);

        Assert.Equal(1, catalog.Reads);
    }

    [Fact]
    public async Task AClosedPathFromAModule_NeverReachesTheMap()
    {
        SitemapSource source = new(new AppCache(), new FakeCatalog(["/panel/urunler", "/arama", "/urun/masa"]), new FakeContent(["/sepet"]));

        SitemapView view = await source.GetAsync(Ct);

        Assert.DoesNotContain("/panel/urunler", view.Paths);
        Assert.DoesNotContain("/arama", view.Paths);
        Assert.DoesNotContain("/sepet", view.Paths);
        Assert.Contains("/urun/masa", view.Paths);
    }

    private sealed class FakeCatalog(IReadOnlyList<string> paths) : ICatalogModule
    {
        public IReadOnlyList<string> Paths { get; set; } = paths;

        public int Reads { get; private set; }

        public Task<IReadOnlyList<string>> ListSitemapPathsAsync(CancellationToken ct)
        {
            Reads++;
            return Task.FromResult(Paths);
        }

        public Task<bool> ReserveAsync(StockClaim claim, StockClaim? releasedFirst, CancellationToken ct) => throw new NotSupportedException();

        public Task ReleaseAsync(StockClaim claim, CancellationToken ct) => throw new NotSupportedException();

        public Task CommitAsync(StockClaim claim, CancellationToken ct) => throw new NotSupportedException();

        public Task ReturnAsync(StockClaim claim, CancellationToken ct) => throw new NotSupportedException();

        public Task<bool> RetakeAsync(StockClaim claim, CancellationToken ct) => throw new NotSupportedException();

        public Task RestoreAsync(StockClaim claim, CancellationToken ct) => throw new NotSupportedException();

        public Task<StockQuantityWriteResult> SaveStockQuantityAsync(Guid variantId, int? shownValue, int? newValue, CancellationToken ct) => throw new NotSupportedException();

        public Task<VariantStockState?> GetStockStateAsync(Guid variantId, CancellationToken ct) => throw new NotSupportedException();

        public Task<ProductAvailability> GetAvailabilityAsync(Guid productId, CancellationToken ct) => throw new NotSupportedException();

        public Task<ImageUploadResult> UploadImageAsync(byte[] content, CancellationToken ct) => throw new NotSupportedException();

        public Task<bool> ImageExistsAsync(Guid imageId, CancellationToken ct) => throw new NotSupportedException();

        public Task<DigitalFileLocation?> FindDownloadFileAsync(Guid? variantId, Guid? lastKnownFileId, CancellationToken ct) => throw new NotSupportedException();

        public Task<UploadStartResult> StartUploadAsync(string? fileName, long sizeBytes, CancellationToken ct) => throw new NotSupportedException();

        public Task<UploadChunkResult> ReceiveChunkAsync(Guid id, long start, Stream body, long length, CancellationToken ct) => throw new NotSupportedException();

        public Task<UploadStatus?> GetUploadStatusAsync(Guid id, CancellationToken ct) => throw new NotSupportedException();

        public Task<ShowcasePage> ListShowcaseAsync(string? search, int page, CancellationToken ct) => throw new NotSupportedException();

        public Task<IReadOnlyList<ShowcaseVariant>> ListOptionVariantsAsync(Guid productId, CancellationToken ct) => throw new NotSupportedException();

        public Task<ProductPage> GetProductPageAsync(string? slug, bool isAdministrator, CancellationToken ct) => throw new NotSupportedException();

        public Task<IReadOnlyList<Guid>> ListVariantsWithoutFileAsync(Guid productId, CancellationToken ct) => throw new NotSupportedException();

        public Task<string?> SuggestStockCodeAsync(Guid? productId, string? productName, CancellationToken ct) => throw new NotSupportedException();

        public Task<StockCodeCheck> CheckStockCodeAsync(string? stockCode, Guid? exceptVariantId, CancellationToken ct) => throw new NotSupportedException();

        public Task<VariantStructureCheck> CheckVariantStructureAsync(Guid productId, Guid? firstOptionValueId, Guid? secondOptionValueId, Guid? exceptVariantId, CancellationToken ct) => throw new NotSupportedException();

        public Task<IReadOnlyList<OptionMatrixCell>> GetOptionMatrixAsync(Guid productId, CancellationToken ct) => throw new NotSupportedException();

        public Task<PriceLabel?> GetPriceLabelAsync(Guid variantId, CancellationToken ct) => throw new NotSupportedException();

        public Task<DiscountSaveResult> SaveDiscountAsync(Guid productId, decimal percentage, DateOnly startDate, DateOnly endDate, CancellationToken ct) => throw new NotSupportedException();

        public Task<DiscountRemoveResult> RemoveDiscountAsync(Guid productId, CancellationToken ct) => throw new NotSupportedException();

        public Task<DiscountView?> GetDiscountAsync(Guid productId, CancellationToken ct) => throw new NotSupportedException();

        public Task<VariantPriceResult> ChangeVariantPriceAsync(Guid variantId, decimal price, CancellationToken ct) => throw new NotSupportedException();

        public ItemAmounts PriceItem(ItemPriceInput input) => throw new NotSupportedException();

        public IReadOnlyList<ShippingVatPart> SplitShippingFee(decimal shippingFee, IReadOnlyList<ShippingBasisLine> lines) => throw new NotSupportedException();

        public decimal SumOrderTotal(IReadOnlyList<ItemAmounts> items, decimal shippingFee) => throw new NotSupportedException();

        public Task<CouponWriteResult> CreateCouponAsync(CouponDefinition definition, CancellationToken ct) => throw new NotSupportedException();

        public Task<CouponApplication> EvaluateCouponAsync(string? code, IReadOnlyList<CouponLine> lines, CancellationToken ct) => throw new NotSupportedException();

        public Task<CouponApplication> ApplyCouponAsync(Guid cartId, string? code, IReadOnlyList<CouponLine> lines, CancellationToken ct) => throw new NotSupportedException();

        public bool ReturnsCouponRight(IReadOnlyList<ItemClosure> items) => throw new NotSupportedException();

        public Task<IReadOnlyList<CategoryNode>> GetCategoryTreeAsync(CancellationToken ct) => throw new NotSupportedException();

        public Task<CategoryWriteResult> CreateCategoryAsync(string? name, Guid? parentCategoryId, CancellationToken ct) => throw new NotSupportedException();

        public Task<CategoryWriteResult> RenameCategoryAsync(Guid categoryId, string? name, CancellationToken ct) => throw new NotSupportedException();

        public Task<CategoryWriteResult> MoveCategoryAsync(Guid categoryId, Guid? newParentCategoryId, CancellationToken ct) => throw new NotSupportedException();

        public Task<CategoryWriteResult> DeleteCategoryAsync(Guid categoryId, CancellationToken ct) => throw new NotSupportedException();

        public Task<ProductCategoriesResult> SetProductCategoriesAsync(Guid productId, IReadOnlyCollection<Guid> categoryIds, Guid? primaryCategoryId, CancellationToken ct) => throw new NotSupportedException();

        public Task<ProductListPage?> ListCategoryProductsAsync(string? categorySlug, ProductListFilter? filter, int page, CancellationToken ct) => throw new NotSupportedException();

        public Task<ProductListPage?> SearchProductsAsync(string? search, ProductListFilter? filter, int page, CancellationToken ct) => throw new NotSupportedException();

        public Task<GallerySaveResult> SaveProductGalleryAsync(Guid productId, IReadOnlyList<GalleryEntry> entries, CancellationToken ct) => throw new NotSupportedException();

        public Task<VariantImageResult> SetVariantImageAsync(Guid variantId, Guid? imageId, CancellationToken ct) => throw new NotSupportedException();

        public Task<IReadOnlyList<GalleryImage>> GetProductGalleryAsync(Guid productId, CancellationToken ct) => throw new NotSupportedException();

        public Task<string?> GetCardImageAltTextAsync(Guid productId, CancellationToken ct) => throw new NotSupportedException();

        public Task<VariantImage?> GetVariantImageAsync(Guid variantId, CancellationToken ct) => throw new NotSupportedException();
    }

    private sealed class FakeContent(IReadOnlyList<string> paths) : ICorporateContentModule
    {
        public int Reads { get; private set; }

        public Task<IReadOnlyList<string>> ListSitemapPathsAsync(CancellationToken ct)
        {
            Reads++;
            return Task.FromResult(paths);
        }

        public Task<BrandingSection> GetBrandingAsync(CancellationToken ct) => throw new NotSupportedException();

        public Task<BrandingSaveResult> SaveBrandingAsync(BrandingInput input, int formVersion, CancellationToken ct) => throw new NotSupportedException();

        public Task<BrandIdentity> GetBrandIdentityAsync(CancellationToken ct) => throw new NotSupportedException();
    }
}
