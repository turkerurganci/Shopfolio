using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Disposal;
using Shopfolio.Infrastructure.Jobs;
using Shopfolio.Infrastructure.Routing;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Modules.Payment.Contracts;

namespace Shopfolio.Modules.Ordering;

/// <summary>The Ordering module's implementation of its open surface; internal, reached only through <see cref="IOrderingModule"/>.</summary>
internal sealed class OrderingModule : IOrderingModule
{
    private readonly AppDbContext _db;
    private readonly OrderPaymentArms _arms;
    private readonly ShippingViewReader _shippingViews;

    private readonly OrderLocks _locks;

    private readonly AdvisoryLocks _advisoryLocks;

    private readonly DownloadCounters _downloads;

    private readonly Carts _carts;

    private readonly OrderAccess _access;

    public OrderingModule(AppDbContext db, OrderPaymentArms arms, OrderLocks locks, AdvisoryLocks advisoryLocks, DownloadCounters downloads, Carts carts, OrderAccess access)
    {
        _carts = carts;
        _db = db;
        _arms = arms;
        _shippingViews = new ShippingViewReader(db);
        _locks = locks;
        _advisoryLocks = advisoryLocks;
        _downloads = downloads;
        _access = access;
    }

    public Task<OrderAccessLookup?> FindOrderByNumberAndEmailAsync(string orderNumber, string email, CancellationToken ct) => _access.FindByNumberAndEmailAsync(orderNumber, email, ct);

    public Task<OrderPageSummary?> GetOrderPageSummaryAsync(Guid orderId, CancellationToken ct) => _access.GetSummaryAsync(orderId, ct);

    public Task<OrderShippingView?> GetShippingViewAsync(Guid orderId, CancellationToken ct) => _shippingViews.GetAsync(orderId, ct);

    public Task<IReadOnlyList<OrderPaymentArm>> FindPaymentArmsAsync(IReadOnlyCollection<Guid> orderIds, CancellationToken ct) => _arms.ReadAsync(orderIds, ct);

    public Task<IReadOnlyList<Guid>> FindUnpaidFailedOrdersAsync(DateTimeOffset failedBefore, IReadOnlyCollection<Guid> exclude, int max, CancellationToken ct) =>
        _arms.FindUnpaidFailedBeforeAsync(failedBefore, exclude, max, ct);

    public async Task<IReadOnlyList<OrderOfItem>> FindOrdersOfItemsAsync(IReadOnlyCollection<Guid> orderItemIds, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(orderItemIds);
        Guid[] ids = [.. orderItemIds.Distinct()];
        if (ids.Length == 0)
        {
            return [];
        }

        return await _db.OrderItems.AsNoTracking()
            .Where(i => ids.Contains(i.Id))
            .Select(i => new OrderOfItem(i.Id, i.OrderId))
            .ToListAsync(ct);
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
        // Delivery follows the confirmed payment (02 3.12.1): an item of an order whose payment is not confirmed is not downloadable.
        return await _db.OrderItems.AsNoTracking()
            .Where(i => i.Id == orderItemId
                && i.ProductType == ProductType.Digital
                && _db.Orders.Where(DownloadRules.PaymentConfirmed).Any(o => o.Id == i.OrderId))
            .Select(i => new DownloadableItem(i.VariantId, i.DigitalFileId))
            .SingleOrDefaultAsync(ct);
    }

    public Task<DownloadTakeResult> TakeDownloadAsync(Guid orderItemId, CancellationToken ct) => _downloads.TakeAsync(orderItemId, ct);

    public Task<DownloadRenewalResult> RenewDownloadAllowanceAsync(Guid orderItemId, int expectedVersion, CancellationToken ct)
        => _downloads.RenewAsync(orderItemId, expectedVersion, ct);

    public async Task<DigitalAddCheck> CheckDigitalAddAsync(DigitalAddRequest request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (request.ProductType != ProductType.Digital)
        {
            return new DigitalAddCheck(true, false, false);
        }

        // A second add of the same variant is refused (02 3.12.4); the marked items of a cart are no items of it (06 4.16).
        bool isInCart = request.CartId is Guid cartId
            && await _db.CartItems.AsNoTracking().AnyAsync(i => i.CartId == cartId && i.VariantId == request.VariantId && i.RemovedProductName == null, ct);

        // A member's past is read from the member's orders with a confirmed payment - the ones the order page can download from (02 3.12.9); a guest has none to read.
        bool wasBought = request.CustomerId is Guid customerId
            && await _db.OrderItems.AsNoTracking()
                .Where(i => i.VariantId == request.VariantId)
                .AnyAsync(i => _db.Orders.Where(DownloadRules.PaymentConfirmed).Any(o => o.Id == i.OrderId && o.CustomerId == customerId), ct);

        return new DigitalAddCheck(DownloadRules.IsQuantityAllowed(request.ProductType, request.Quantity), isInCart, wasBought);
    }

    public async Task<int> LinkGuestOrdersAsync(Guid customerId, string email, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(email);
        if (_db.Database.CurrentTransaction is null)
        {
            throw new InvalidOperationException("The guest orders are linked only inside the transaction of the verification (02 3.13.2; 09 6 item 3).");
        }

        string normalized = TextNormalizer.NormalizeIdentity(email);

        // The orders are locked first and read after the lock (05 7.3.2): a guest order another request is changing at this moment is waited for, not overwritten. An order that
        // has an account, or whose account was deleted, is never taken (K-1116).
        Guid[] candidates = await _db.Orders.AsNoTracking()
            .Where(o => o.CustomerId == null && !o.IsAccountDeleted && o.NormalizedContactEmail == normalized)
            .Select(o => o.Id)
            .ToArrayAsync(ct);
        if (candidates.Length == 0)
        {
            return 0;
        }

        await _locks.LockAsync(candidates, ct);
        List<Order> orders = await _db.Orders
            .Where(o => candidates.Contains(o.Id) && o.CustomerId == null && !o.IsAccountDeleted && o.NormalizedContactEmail == normalized)
            .ToListAsync(ct);

        // The account link is a non-raising field (06 2.6.4): the order's version does not move, and the save of the caller's unit of work writes this column only.
        foreach (Order order in orders)
        {
            order.CustomerId = customerId;
        }

        return orders.Count;
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
        return await _carts.TakeRemovedNamesAsync(cartId, ct);
    }

    public Task<CartView> OpenCartAsync(CartOwner owner, CancellationToken ct) => _carts.OpenAsync(owner, ct);

    public Task<CartAddResult> AddToCartAsync(CartOwner owner, Guid variantId, int quantity, CancellationToken ct) => _carts.AddAsync(owner, variantId, quantity, ct);

    public Task<CartMergeResult> MergeGuestCartAsync(Guid customerId, string guestCartKey, CancellationToken ct) => _carts.MergeAsync(customerId, guestCartKey, ct);

    public async Task<IReadOnlyList<ReservationHolder>> ListReservationHoldersAsync(Guid variantId, CancellationToken ct)
    {
        return await _db.StockReservations.AsNoTracking()
            .Where(r => r.VariantId == variantId)
            .Join(_db.Orders, r => r.OrderId, o => o.Id, (r, o) => new { r.CreatedAt, r.Id, o.OrderNumber, OrderId = o.Id, r.Quantity })
            .OrderByDescending(x => x.CreatedAt).ThenBy(x => x.Id)
            .Select(x => new ReservationHolder(x.OrderId, x.OrderNumber, x.Quantity))
            .ToListAsync(ct);
    }

    public async Task<OrderForPayment?> FindOrderForPaymentAsync(string orderNumber, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(orderNumber);
        if (orderNumber.Length == 0)
        {
            return null;
        }

        return await _db.Orders.AsNoTracking()
            .Where(o => o.OrderNumber == orderNumber)
            .Select(o => new OrderForPayment(o.Id, o.Total, o.PaymentMethod))
            .SingleOrDefaultAsync(ct);
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

    public async Task<CardPaymentQueryTarget?> GetCardPaymentQueryTargetAsync(Guid orderId, DateTimeOffset now, CancellationToken ct)
    {
        Order? order = await _db.Orders.AsNoTracking()
            .Where(o => o.Id == orderId)
            .Where(PaymentWindowRules.Awaiting(PaymentMethod.Card))
            .SingleOrDefaultAsync(ct);
        return order is null ? null : new CardPaymentQueryTarget(order.OrderNumber, PaymentWindowRules.IsPaymentOutcomeUnknown(order, now));
    }

    public async Task<IReadOnlySet<Guid>> FindCorrectedTransitionRecordsAsync(IReadOnlyCollection<Guid> transitionRecordIds, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(transitionRecordIds);
        Guid[] ids = [.. transitionRecordIds.Distinct()];
        HashSet<Guid> corrected = [];
        if (ids.Length == 0)
        {
            return corrected;
        }

        List<Guid?> saved = await _db.TransitionRecords.AsNoTracking()
            .Where(t => t.CorrectedTransitionRecordId != null && ids.Contains(t.CorrectedTransitionRecordId.Value))
            .Select(t => t.CorrectedTransitionRecordId)
            .ToListAsync(ct);
        corrected.UnionWith(saved.OfType<Guid>());
        corrected.UnionWith(_db.TransitionRecords.Local.Select(t => t.CorrectedTransitionRecordId).OfType<Guid>().Where(id => ids.Contains(id)));
        return corrected;
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
        services.AddScoped<OrderPaymentArms>();
        services.AddScoped<DownloadCounters>();
        services.AddScoped<CartStatements>();
        services.AddScoped<Carts>();

        // The order number, the access key and the ways into the order page (T354; 02 3.22).
        services.AddScoped<OrderIdentifiers>();
        services.AddScoped<OrderAccess>();
        services.AddOutageEffect<TransferWindowDeferral>();
        services.AddScoped<IOrderingModule, OrderingModule>();

        // The transition unit (09 9.1 item 1): the class is what the module's own operations take; the open face is the same instance.
        services.AddScoped<OrderCounters>();
        services.AddScoped<OrderTransitions>();
        services.AddScoped<IOrderTransitions>(provider => provider.GetRequiredService<OrderTransitions>());
        services.AddDisposalPart<GuestCartDisposalPart>();
        services.AddDisposalPart<OrderDisposalPart>();

        // The payment window (T324; 03 4.1.7-4.1.9): its end for the order's confirmation and the correction, and the three sources of its due moments.
        // The cart has no source: it never empties by itself (03 4.1.6; Z-6).
        services.AddScoped<PaymentWindows>();
        services.AddDueRecordSource<TransferPaymentExpiry>();
        services.AddDueRecordSource<TransferPaymentReminder>();
        services.AddDueRecordSource<CardPaymentExpiry>();

        // The refund IBAN's deletion day on a withdrawal whose goods did not come back (T327; Z-38; 03 4.2.8).
        services.AddDueRecordSource<RefundIbanDeletionDay>();

        // The delivery periods (T325; 03 4.1.10-4.1.13): read from the order's records, never a source - Z-10, Z-11 and Z-39 run out without an action.
        services.AddScoped<OrderPeriods>();
        return services;
    }
}
