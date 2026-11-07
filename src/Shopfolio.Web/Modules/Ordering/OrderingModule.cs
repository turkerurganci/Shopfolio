using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Disposal;
using Shopfolio.Infrastructure.Jobs;
using Shopfolio.Infrastructure.Routing;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Ordering.Contracts;

namespace Shopfolio.Modules.Ordering;

/// <summary>The Ordering module's implementation of its open surface; internal, reached only through <see cref="IOrderingModule"/>.</summary>
internal sealed class OrderingModule : IOrderingModule
{
    private readonly AppDbContext _db;

    public OrderingModule(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Guid?> FindMemberOrderAsync(string orderNumber, Guid customerId, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(orderNumber);
        string number = OrderRoutes.NumberForLookup(orderNumber);

        // A guest order has no account link, so no customer id matches it (07 2.3.4).
        return await _db.Orders.AsNoTracking()
            .Where(o => o.OrderNumber == number && o.CustomerId == customerId)
            .Select(o => (Guid?)o.Id)
            .SingleOrDefaultAsync(ct);
    }

    public async Task<DownloadableItem?> FindDownloadableItemAsync(Guid orderItemId, CancellationToken ct)
    {
        return await _db.OrderItems.AsNoTracking()
            .Where(i => i.Id == orderItemId && i.ProductType == ProductType.Digital)
            .Select(i => new DownloadableItem(i.VariantId, i.DigitalFileId))
            .SingleOrDefaultAsync(ct);
    }

    public async Task<Guid?> FindOrderByAccessKeyAsync(string accessKey, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(accessKey);
        if (accessKey.Length == 0)
        {
            return null;
        }

        return await _db.Orders.AsNoTracking()
            .Where(o => o.OrderAccessKey == accessKey)
            .Select(o => (Guid?)o.Id)
            .SingleOrDefaultAsync(ct);
    }
}

/// <summary>The module's single registration (09 6 item 3): <c>Program.cs</c> calls <c>AddOrderingModule</c> once.</summary>
internal static class OrderingModuleRegistration
{
    public static IServiceCollection AddOrderingModule(this IServiceCollection services)
    {
        services.AddScoped<OrderLocks>();
        services.AddOutageEffect<TransferWindowDeferral>();
        services.AddScoped<IOrderingModule, OrderingModule>();

        // The transition unit (09 9.1 item 1): the class is what the module's own operations take; the open face is the same instance.
        services.AddScoped<OrderTransitions>();
        services.AddScoped<IOrderTransitions>(provider => provider.GetRequiredService<OrderTransitions>());
        services.AddDisposalPart<GuestCartDisposalPart>();
        services.AddDisposalPart<OrderDisposalPart>();
        return services;
    }
}
