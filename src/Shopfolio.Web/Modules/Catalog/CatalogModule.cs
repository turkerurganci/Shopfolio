using Microsoft.Extensions.DependencyInjection;

using Shopfolio.Infrastructure.Disposal;
using Shopfolio.Infrastructure.Jobs;
using Shopfolio.Infrastructure.Values;

using Shopfolio.Modules.Catalog.Contracts;

namespace Shopfolio.Modules.Catalog;

/// <summary>The Catalog module's implementation of its open surface; internal, reached only through <see cref="ICatalogModule"/>.</summary>
internal sealed class CatalogModule : ICatalogModule
{
    private readonly StockCounters _counters;
    private readonly ImageUploads _imageUploads;

    public CatalogModule(StockCounters counters, ImageUploads imageUploads)
    {
        _counters = counters;
        _imageUploads = imageUploads;
    }

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

        // The image derivative job (D-97) and the image disposal (D-102): the rows first, the files they leave behind after (06 9.3.4).
        services.AddJobHandler<ImageDerivativeJobHandler>(JobType.ImageDerivative);
        services.AddDisposalPart<ImageDisposalPart>();
        services.AddDisposalPart<OrphanImageFilePart>();
        services.AddScoped<ICatalogModule, CatalogModule>();
        return services;
    }
}
