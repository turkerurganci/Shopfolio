using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using Shopfolio.Infrastructure.Disposal;
using Shopfolio.Infrastructure.Jobs;
using Shopfolio.Infrastructure.Security;
using Shopfolio.Infrastructure.Values;

using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Catalog.Contracts.Categories;
using Shopfolio.Modules.Catalog.Contracts.Coupons;
using Shopfolio.Modules.Catalog.Contracts.Lists;
using Shopfolio.Modules.Catalog.Contracts.Pricing;
using Shopfolio.Modules.Catalog.Contracts.Variants;

namespace Shopfolio.Modules.Catalog;

/// <summary>The Catalog module's implementation of its open surface; internal, reached only through <see cref="ICatalogModule"/>.</summary>
internal sealed class CatalogModule : ICatalogModule
{
    private readonly StockCounters _counters;
    private readonly ImageUploads _imageUploads;
    private readonly CatalogSitemapReader _sitemap;
    private readonly DigitalFileReader _fileReader;
    private readonly DigitalFileUploads _fileUploads;
    private readonly ShowcaseReader _showcase;
    private readonly StockCodes _stockCodes;
    private readonly ProductStructureReader _structure;
    private readonly ProductPageReader _pages;
    private readonly PriceLabelReader _priceLabels;
    private readonly CategoryTree _categories;
    private readonly ProductCategoryLinks _links;
    private readonly ProductListReader _lists;
    private readonly ProductGallery _gallery;
    private readonly CartVariantReader _cartVariants;
    private readonly CartQuantityReader _cartQuantities;
    private readonly OrderItemSourceReader _orderItemSources;
    private readonly Discounts _discounts;
    private readonly VariantPrices _variantPrices;
    private readonly ShippingPeriodReader _shippingPeriods;
    private readonly CatalogMenuReader _menu;
    private readonly Coupons _coupons;

    public CatalogModule(
        StockCounters counters,
        ImageUploads imageUploads,
        CatalogSitemapReader sitemap,
        DigitalFileReader fileReader,
        DigitalFileUploads fileUploads,
        ShowcaseReader showcase,
        StockCodes stockCodes,
        ProductStructureReader structure,
        ProductPageReader pages,
        PriceLabelReader priceLabels,
        CategoryTree categories,
        ProductCategoryLinks links,
        ProductListReader lists,
        ProductGallery gallery,
        Discounts discounts,
        VariantPrices variantPrices,
        ShippingPeriodReader shippingPeriods,
        CatalogMenuReader menu,
        Coupons coupons,
        CartVariantReader cartVariants,
        CartQuantityReader cartQuantities,
        OrderItemSourceReader orderItemSources)
    {
        _cartVariants = cartVariants;
        _cartQuantities = cartQuantities;
        _orderItemSources = orderItemSources;
        _pages = pages;
        _discounts = discounts;
        _menu = menu;
        _variantPrices = variantPrices;
        _shippingPeriods = shippingPeriods;
        _coupons = coupons;
        _priceLabels = priceLabels;
        _categories = categories;
        _links = links;
        _lists = lists;
        _showcase = showcase;
        _counters = counters;
        _imageUploads = imageUploads;
        _sitemap = sitemap;
        _fileReader = fileReader;
        _fileUploads = fileUploads;
        _stockCodes = stockCodes;
        _structure = structure;
        _gallery = gallery;
    }

    public Task<string?> SuggestStockCodeAsync(Guid? productId, string? productName, CancellationToken ct)
        => _stockCodes.SuggestAsync(productId, productName, ct);

    public Task<StockCodeCheck> CheckStockCodeAsync(string? stockCode, Guid? exceptVariantId, CancellationToken ct)
        => _stockCodes.CheckAsync(stockCode, exceptVariantId, ct);

    public Task<VariantStructureCheck> CheckVariantStructureAsync(
        Guid productId,
        Guid? firstOptionValueId,
        Guid? secondOptionValueId,
        Guid? exceptVariantId,
        CancellationToken ct)
        => _structure.CheckVariantAsync(productId, firstOptionValueId, secondOptionValueId, exceptVariantId, ct);

    public Task<IReadOnlyList<OptionMatrixCell>> GetOptionMatrixAsync(Guid productId, CancellationToken ct)
        => _structure.GetMatrixAsync(productId, ct);

    public Task<ProductPage> GetProductPageAsync(string? slug, bool isAdministrator, CancellationToken ct) => _pages.ReadAsync(slug, isAdministrator, ct);

    public Task<IReadOnlyList<Guid>> ListVariantsWithoutFileAsync(Guid productId, CancellationToken ct) => _fileReader.ListPublishedWithoutFileAsync(productId, ct);

    public Task<PriceLabel?> GetPriceLabelAsync(Guid variantId, CancellationToken ct) => _priceLabels.GetAsync(variantId, ct);

    public Task<IReadOnlyList<VariantShippingPeriod>> GetShippingPeriodsAsync(IReadOnlyCollection<Guid> variantIds, CancellationToken ct) => _shippingPeriods.GetAsync(variantIds, ct);

    public Task<DiscountSaveResult> SaveDiscountAsync(Guid productId, decimal percentage, DateOnly startDate, DateOnly endDate, CancellationToken ct)
        => _discounts.SaveAsync(productId, percentage, startDate, endDate, ct);

    public Task<DiscountRemoveResult> RemoveDiscountAsync(Guid productId, CancellationToken ct) => _discounts.RemoveAsync(productId, ct);

    public Task<DiscountView?> GetDiscountAsync(Guid productId, CancellationToken ct) => _discounts.GetAsync(productId, ct);

    public Task<VariantPriceResult> ChangeVariantPriceAsync(Guid variantId, decimal price, CancellationToken ct) => _variantPrices.ChangeAsync(variantId, price, ct);

    public ItemAmounts PriceItem(ItemPriceInput input) => ItemPricing.Price(input);

    public IReadOnlyList<ShippingVatPart> SplitShippingFee(decimal shippingFee, IReadOnlyList<ShippingBasisLine> lines)
        => ItemPricing.SplitShippingFee(shippingFee, lines);

    public decimal SumOrderTotal(IReadOnlyList<ItemAmounts> items, decimal shippingFee) => ItemPricing.SumOrderTotal(items, shippingFee);

    public Task<CouponWriteResult> CreateCouponAsync(CouponDefinition definition, CancellationToken ct) => _coupons.CreateAsync(definition, ct);

    public Task<CouponApplication> EvaluateCouponAsync(string? code, IReadOnlyList<CouponLine> lines, CancellationToken ct) => _coupons.EvaluateAsync(code, lines, ct);

    public Task<CouponApplication> ApplyCouponAsync(Guid cartId, string? code, IReadOnlyList<CouponLine> lines, CancellationToken ct)
        => _coupons.ApplyAsync(cartId, code, lines, ct);

    public bool ReturnsCouponRight(IReadOnlyList<ItemClosure> items) => CouponRules.ReturnsRight(items);

    public Task<IReadOnlyList<CategoryNode>> GetCategoryTreeAsync(CancellationToken ct) => _categories.ReadAsync(ct);

    public Task<CatalogMenu> GetMenuAsync(CancellationToken ct) => _menu.ReadAsync(ct);

    public Task<CategoryWriteResult> CreateCategoryAsync(string? name, Guid? parentCategoryId, CancellationToken ct)
        => _categories.CreateAsync(name, parentCategoryId, ct);

    public Task<CategoryWriteResult> RenameCategoryAsync(Guid categoryId, string? name, CancellationToken ct)
        => _categories.RenameAsync(categoryId, name, ct);

    public Task<CategoryWriteResult> MoveCategoryAsync(Guid categoryId, Guid? newParentCategoryId, CancellationToken ct)
        => _categories.MoveAsync(categoryId, newParentCategoryId, ct);

    public Task<CategoryWriteResult> DeleteCategoryAsync(Guid categoryId, CancellationToken ct) => _categories.DeleteAsync(categoryId, ct);

    public Task<ProductCategoriesResult> SetProductCategoriesAsync(Guid productId, IReadOnlyCollection<Guid> categoryIds, Guid? primaryCategoryId, CancellationToken ct)
        => _links.SetAsync(productId, categoryIds, primaryCategoryId, ct);

    public Task<ProductListPage?> ListCategoryProductsAsync(string? categorySlug, ProductListFilter? filter, int page, CancellationToken ct)
        => _lists.ListCategoryAsync(categorySlug, filter, page, ct);

    public Task<ProductListPage?> SearchProductsAsync(string? search, ProductListFilter? filter, int page, CancellationToken ct)
        => _lists.SearchAsync(search, filter, page, ct);

    public Task<GallerySaveResult> SaveProductGalleryAsync(Guid productId, IReadOnlyList<GalleryEntry> entries, CancellationToken ct)
        => _gallery.SaveAsync(productId, entries, ct);

    public Task<VariantImageResult> SetVariantImageAsync(Guid variantId, Guid? imageId, CancellationToken ct)
        => _gallery.SetVariantImageAsync(variantId, imageId, ct);

    public Task<IReadOnlyList<GalleryImage>> GetProductGalleryAsync(Guid productId, CancellationToken ct)
        => _gallery.ReadAsync(productId, ct);

    public Task<string?> GetCardImageAltTextAsync(Guid productId, CancellationToken ct)
        => _gallery.GetCardAltTextAsync(productId, ct);

    public Task<VariantImage?> GetVariantImageAsync(Guid variantId, CancellationToken ct)
        => _gallery.GetVariantImageAsync(variantId, ct);

    public Task<IReadOnlyList<CartVariantFact>> ListCartVariantFactsAsync(IReadOnlyCollection<Guid> variantIds, CancellationToken ct)
        => _cartVariants.ListAsync(variantIds, ct);

    public Task<IReadOnlyList<OrderItemSource>> ListOrderItemSourcesAsync(IReadOnlyCollection<Guid> variantIds, CancellationToken ct)
        => _orderItemSources.ListAsync(variantIds, ct);

    public Task<IReadOnlyList<CartQuantityVerdict>> JudgeCartQuantitiesAsync(IReadOnlyList<CartQuantityLine> lines, CancellationToken ct)
        => _cartQuantities.JudgeAsync(lines, ct);

    public Task<CartQuantityVerdict> JudgeCartAddAsync(CartQuantityLine candidate, IReadOnlyList<CartQuantityLine> others, CancellationToken ct)
        => _cartQuantities.JudgeAddAsync(candidate, others, ct);

    public Task<DigitalFileLocation?> FindDownloadFileAsync(Guid? variantId, Guid? lastKnownFileId, CancellationToken ct)
        => _fileReader.FindAsync(variantId, lastKnownFileId, ct);

    public Task<UploadStartResult> StartUploadAsync(string? fileName, long sizeBytes, CancellationToken ct)
        => _fileUploads.StartAsync(fileName, sizeBytes, ct);

    public Task<UploadChunkResult> ReceiveChunkAsync(Guid id, long start, Stream body, long length, CancellationToken ct)
        => _fileUploads.ReceiveAsync(id, start, body, length, ct);

    public Task<UploadStatus?> GetUploadStatusAsync(Guid id, CancellationToken ct) => _fileUploads.StatusAsync(id, ct);

    public Task<IReadOnlyList<string>> ListSitemapPathsAsync(CancellationToken ct) => _sitemap.ListAsync(ct);

    public Task<ShowcasePage> ListShowcaseAsync(string? search, int page, CancellationToken ct) => _showcase.ListAsync(search, page, ct);

    public Task<IReadOnlyList<ShowcaseVariant>> ListOptionVariantsAsync(Guid productId, CancellationToken ct) => _showcase.ListOptionsAsync(productId, ct);

    public Task<bool> ReserveAsync(StockClaim claim, StockClaim? releasedFirst, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(claim);
        List<(StockClaim, CounterMove)> parts = releasedFirst is null
            ? [(claim, CounterMove.Reserve)]
            : [(releasedFirst, CounterMove.Release), (claim, CounterMove.Reserve)];
        return _counters.ApplyAsync(parts, ct);
    }

    public Task<ImageUploadResult> UploadImageAsync(byte[] content, CancellationToken ct) => _imageUploads.UploadAsync(content, ct);

    public Task<bool> ImageExistsAsync(Guid imageId, CancellationToken ct) => _imageUploads.ExistsAsync(imageId, ct);

    public Task ReleaseAsync(StockClaim claim, CancellationToken ct) => ApplyAsync(claim, CounterMove.Release, ct);

    public Task CommitAsync(StockClaim claim, CancellationToken ct) => ApplyAsync(claim, CounterMove.Commit, ct);

    public Task ReturnAsync(StockClaim claim, CancellationToken ct) => ApplyAsync(claim, CounterMove.Return, ct);

    public Task<bool> RetakeAsync(StockClaim claim, CancellationToken ct) => ApplyAsync(claim, CounterMove.Retake, ct);

    public Task RestoreAsync(StockClaim claim, CancellationToken ct) => ApplyAsync(claim, CounterMove.Restore, ct);

    public Task<StockQuantityWriteResult> SaveStockQuantityAsync(Guid variantId, int? shownValue, int? newValue, CancellationToken ct)
    {
        return _counters.SaveStockQuantityAsync(variantId, shownValue, newValue, ct);
    }

    public Task<VariantStockState?> GetStockStateAsync(Guid variantId, CancellationToken ct) => _counters.GetStateAsync(variantId, ct);

    public Task<ProductAvailability> GetAvailabilityAsync(Guid productId, CancellationToken ct) => _counters.GetAvailabilityAsync(productId, ct);

    private Task<bool> ApplyAsync(StockClaim claim, CounterMove move, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(claim);
        return _counters.ApplyAsync([(claim, move)], ct);
    }
}

/// <summary>The module's single registration (09 6 item 3): <c>Program.cs</c> calls <c>AddCatalogModule</c> once.</summary>
internal static class CatalogModuleRegistration
{
    public static IServiceCollection AddCatalogModule(this IServiceCollection services)
    {
        services.AddScoped<StockCounters>();
        services.AddScoped<ImageUploads>();
        services.AddScoped<CatalogSitemapReader>();
        services.AddScoped<DigitalFileReader>();
        services.AddScoped<DigitalFileLocks>();
        services.AddScoped<DigitalFileUploads>();
        services.AddScoped<StockCodes>();
        services.AddScoped<ProductStructureReader>();
        services.AddScoped<PriceLabelReader>();
        services.AddScoped<ShippingPeriodReader>();
        services.AddScoped<CartVariantReader>();
        services.AddScoped<CartQuantityReader>();
        services.AddScoped<OrderItemSourceReader>();
        services.AddScoped<CategoryTree>();
        services.AddScoped<CatalogMenuReader>();
        services.AddScoped<ProductCategoryLinks>();
        services.AddScoped<ProductListReader>();
        services.AddScoped<ProductGallery>();

        // The discount and the price history (T337): one writer of the history, the two writers that call it, and the scheduler's source of the discounts' bounds.
        services.AddScoped<PriceHistoryRecorder>();
        services.AddScoped<Discounts>();
        services.AddScoped<VariantPrices>();
        services.AddDueRecordSource<DiscountBoundarySource>();

        // The coupon (T338): its application counts the cart's invalid attempts (L-6). The counter is infrastructure that Program.cs registers first; a host that builds
        // the module alone (the tests) gets it here, and the application's own registration is never doubled.
        services.AddScoped<Coupons>();
        services.TryAddScoped<AttemptCounter>();

        // The image derivative job (D-97) and the image disposal (D-102): the rows first, the files they leave behind after (06 9.3.4).
        services.AddJobHandler<ImageDerivativeJobHandler>(JobType.ImageDerivative);
        services.AddDisposalPart<ImageDisposalPart>();
        services.AddDisposalPart<OrphanImageFilePart>();
        services.AddDisposalPart<IncompleteDigitalUploadPart>();
        services.AddDisposalPart<DigitalFileDisposalPart>();
        services.AddDisposalPart<OrphanDigitalFilePart>();

        // The publish status transitions (T321; 09 9.1 item 1): the class is what the module takes, the open face is the same instance. The gate is T335's (D-300).
        services.AddScoped<ShowcaseReader>();
        services.AddScoped<ProductPageReader>();
        services.AddScoped<IPublishGate, CatalogPublishGate>();
        services.AddScoped<ProductTransitions>();
        services.AddScoped<IProductTransitions>(provider => provider.GetRequiredService<ProductTransitions>());
        services.AddScoped<IProductRemoval, ProductRemoval>();
        services.AddScoped<IDigitalDelivery, DigitalDelivery>();
        services.AddScoped<ICatalogModule, CatalogModule>();
        return services;
    }
}
