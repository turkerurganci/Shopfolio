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

    private readonly AdvisoryLocks _advisoryLocks;

    public OrderingModule(AppDbContext db, AdvisoryLocks advisoryLocks)
    {
        _db = db;
        _advisoryLocks = advisoryLocks;
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

    public async Task<int> RemoveFromCartsAsync(IReadOnlyCollection<Guid> variantIds, string productName, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(variantIds);
        ArgumentException.ThrowIfNullOrWhiteSpace(productName);
        if (variantIds.Count == 0)
        {
            return 0;
        }

        // Tracked rows saved by the transaction owner (09 6 item 3), not a bulk statement: a cart item is no order-side row, but the module's bulk writes are a closed list (09 10.2 item 4).
        Guid[] ids = [.. variantIds];
        List<CartItem> items = await _db.CartItems
            .Where(i => i.VariantId != null && ids.Contains(i.VariantId.Value) && i.RemovedProductName == null)
            .ToListAsync(ct);
        foreach (CartItem item in items)
        {
            item.RemovedProductName = productName;
        }

        return items.Count;
    }

    public async Task<IReadOnlyList<string>> TakeRemovedItemNoticesAsync(Guid cartId, CancellationToken ct)
    {
        // The cart's lock heads the transaction: a request that arrived together with this one waits here, and once the first has committed it reads no
        // marked item - it tells nothing (told once), with no exception on the way (09 10.2 items 2, 5).
        await _advisoryLocks.LockAsync(AdvisoryLockScope.Cart, cartId, ct);
        List<CartItem> marked = await _db.CartItems
            .Where(i => i.CartId == cartId && i.RemovedProductName != null)
            .OrderBy(i => i.CreatedAt).ThenBy(i => i.Id)
            .ToListAsync(ct);
        if (marked.Count == 0)
        {
            return [];
        }

        string[] names = [.. marked.Select(i => i.RemovedProductName!)];
        _db.CartItems.RemoveRange(marked);
        return names;
    }

    public async Task<IReadOnlyList<ReservationHolder>> ListReservationHoldersAsync(Guid variantId, CancellationToken ct)
    {
        return await _db.StockReservations.AsNoTracking()
            .Where(r => r.VariantId == variantId)
            .Join(_db.Orders, r => r.OrderId, o => o.Id, (r, o) => new { r.CreatedAt, r.Id, o.OrderNumber, OrderId = o.Id, r.Quantity })
            .OrderByDescending(x => x.CreatedAt).ThenBy(x => x.Id)
            .Select(x => new ReservationHolder(x.OrderId, x.OrderNumber, x.Quantity))
            .ToListAsync(ct);
    }

    public Task<int> CountOpenOrdersOfProductAsync(Guid productId, CancellationToken ct)
        => OpenOrders().CountAsync(o => _db.OrderItems.Any(i => i.OrderId == o.Id && i.ProductId == productId), ct);

    public Task<int> CountOpenOrdersOfVariantAsync(Guid variantId, CancellationToken ct)
        => OpenOrders().CountAsync(o => _db.OrderItems.Any(i => i.OrderId == o.Id && i.VariantId == variantId), ct);

    // An open order is one outside the terminal states of the shipment axis (02 3.7.7); the list is the transition table's own.
    private IQueryable<Order> OpenOrders()
    {
        OrderStatus[] terminal = [.. TransitionTable.TerminalStatuses];
        return _db.Orders.AsNoTracking().Where(o => !terminal.Contains(o.OrderStatus));
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
