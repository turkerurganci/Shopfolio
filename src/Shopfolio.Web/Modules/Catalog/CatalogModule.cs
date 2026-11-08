using Microsoft.Extensions.DependencyInjection;

using Shopfolio.Infrastructure.Disposal;
using Shopfolio.Infrastructure.Jobs;
using Shopfolio.Infrastructure.Values;

using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Catalog.Contracts.Categories;
using Shopfolio.Modules.Catalog.Contracts.Lists;
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
    private readonly CategoryTree _categories;
    private readonly ProductCategoryLinks _links;
    private readonly ProductListReader _lists;
    private readonly ProductGallery _gallery;

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
        CategoryTree categories,
        ProductCategoryLinks links,
        ProductListReader lists,
        ProductGallery gallery)
    {
        _pages = pages;
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

    public Task<IReadOnlyList<CategoryNode>> GetCategoryTreeAsync(CancellationToken ct) => _categories.ReadAsync(ct);

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

    public Task ReleaseAsync(StockClaim claim, CancellationToken ct) => ApplyAsync(claim, CounterMove.Release, ct);

    public Task CommitAsync(StockClaim claim, CancellationToken ct) => ApplyAsync(claim, CounterMove.Commit, ct);

    public Task ReturnAsync(StockClaim claim, CancellationToken ct) => ApplyAsync(claim, CounterMove.Return, ct);

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
        services.AddScoped<CategoryTree>();
        services.AddScoped<ProductCategoryLinks>();
        services.AddScoped<ProductListReader>();
        services.AddScoped<ProductGallery>();

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
        services.AddScoped<ICatalogModule, CatalogModule>();
        return services;
    }
}
