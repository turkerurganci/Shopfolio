using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Modules.Payment.Contracts;

namespace Shopfolio.Modules.Ordering;

/// <summary>
/// The plain reads behind the panel home's counters (T382; 02 10.6.1): the order sets each counter starts from. Nothing is written, no lock is taken and nothing is kept - the counters
/// are computed at every call (K-405, K-467). The sets grow with the shop, so the item sets are read one page of orders at a time (09 20 item 2); the open quantity of an item is the
/// item records' one rule and is decided by the caller, which holds the AfterSales surface (this module has no say in it).
/// </summary>
internal sealed class PendingWorkReader
{
    private static readonly TransitionCode[] ShipmentMarks = [TransitionCode.S5, TransitionCode.S8];

    private readonly AppDbContext _db;

    public PendingWorkReader(AppDbContext db)
    {
        _db = db;
    }

    public async Task<PendingOrderCounts> CountAsync(CancellationToken ct)
    {
        int approval = await _db.Orders.AsNoTracking().Where(PaymentWindowRules.Awaiting(PaymentMethod.BankTransfer)).CountAsync(ct);
        int failed = await _db.Orders.AsNoTracking().CountAsync(o => o.OrderStatus == OrderStatus.DeliveryFailed, ct);
        return new PendingOrderCounts(approval, failed);
    }

    public async Task<IReadOnlyList<PendingWorkItem>> ListItemsAsync(PendingWorkSet workSet, Guid? afterOrderId, int maxOrders, CancellationToken ct)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(maxOrders, 1);
        IQueryable<OrderItem> items = _db.OrderItems.AsNoTracking();
        IQueryable<Order> orders = _db.Orders.AsNoTracking();
        IQueryable<OrderItem> inSet = workSet switch
        {
            PendingWorkSet.PreparingPhysical => items.Where(i => i.ProductType == ProductType.Physical
                && orders.Any(o => o.Id == i.OrderId && o.OrderStatus == OrderStatus.Preparing)),
            PendingWorkSet.ShippedPhysical => items.Where(i => i.ProductType == ProductType.Physical
                && orders.Any(o => o.Id == i.OrderId && o.OrderStatus == OrderStatus.Shipped)),
            PendingWorkSet.ServiceUnmarked => items.Where(i => i.ProductType == ProductType.Service
                && i.DeliveryMarkedAt == null
                && orders.Any(o => o.Id == i.OrderId
                    && o.OrderStatus != OrderStatus.Cancelled
                    && o.PaymentStatus != PaymentStatus.Pending
                    && o.PaymentStatus != PaymentStatus.Failed)),
            _ => throw new ArgumentOutOfRangeException(nameof(workSet), workSet, "Not a pending work set."),
        };

        if (afterOrderId is { } after)
        {
            inSet = inSet.Where(i => i.OrderId.CompareTo(after) > 0);
        }

        // One page: the next orders by id and their items of the set; the caller walks the pages, so no read takes the growing set whole.
        Guid[] pageOrderIds = await inSet.Select(i => i.OrderId).Distinct().OrderBy(id => id).Take(maxOrders).ToArrayAsync(ct);
        if (pageOrderIds.Length == 0)
        {
            return [];
        }

        return await inSet
            .Where(i => pageOrderIds.Contains(i.OrderId))
            .OrderBy(i => i.OrderId).ThenBy(i => i.Id)
            .Select(i => new PendingWorkItem(i.OrderId, i.Id, i.Quantity))
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<RefundWorkItemFacts>> FindRefundFactsAsync(IReadOnlyCollection<Guid> orderItemIds, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(orderItemIds);
        if (orderItemIds.Count == 0)
        {
            return [];
        }

        Guid[] ids = [.. orderItemIds];
        var rows = await (
            from i in _db.OrderItems.AsNoTracking()
            join o in _db.Orders.AsNoTracking() on i.OrderId equals o.Id
            where ids.Contains(i.Id)
            select new { i.Id, i.OrderId, i.ProductType, i.DeliveryDate, o.PaymentMethod, o.PaymentStatus }).ToListAsync(ct);
        Guid[] orderIds = [.. rows.Select(r => r.OrderId).Distinct()];
        HashSet<Guid> shipped = [.. await _db.TransitionRecords.AsNoTracking()
            .Where(t => orderIds.Contains(t.OrderId) && ShipmentMarks.Contains(t.TransitionCode))
            .Select(t => t.OrderId).Distinct().ToListAsync(ct)];
        return [.. rows.Select(r => new RefundWorkItemFacts(r.Id, r.OrderId, r.ProductType, r.DeliveryDate, r.PaymentMethod, r.PaymentStatus, shipped.Contains(r.OrderId)))];
    }
}
