using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Files;
using Shopfolio.Infrastructure.Imaging;
using Shopfolio.Infrastructure.InstallationSettings;
using Shopfolio.Infrastructure.Status;
using Shopfolio.Modules.Catalog;
using Shopfolio.Modules.Ordering;

namespace Shopfolio.IntegrationTests.Ordering;

/// <summary>
/// The cart's services (T346) for the tests that build the Ordering module by hand: <c>OrderingModule</c> takes <c>Carts</c>, which reads the live
/// variants through the Catalog module, so the harness registers the cart with the Catalog module and what that module needs. Whatever the harness
/// registered already (the time provider, the file store) is kept.
/// </summary>
internal static class CartTestServices
{
    public static IServiceCollection AddCartServices(this IServiceCollection services)
    {
        services.TryAddSingleton(TimeProvider.System);
        services.TryAddScoped<AdvisoryLocks>();
        services.AddFileStore();
        services.AddOptions<StatusThresholdOptions>();
        services.TryAddSingleton<IDiskUsageReader, DriveDiskUsageReader>();
        services.TryAddSingleton<DiskUsageGuard>();
        services.TryAddSingleton<ImageProcessor>();
        services.AddCatalogModule();
        services.TryAddScoped<CartStatements>();
        services.TryAddScoped<Carts>();
        return services;
    }
}
