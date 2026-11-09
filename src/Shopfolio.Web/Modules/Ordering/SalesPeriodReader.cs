using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Ordering.Contracts;

namespace Shopfolio.Modules.Ordering;

/// <summary>
/// The plain read behind the sales summary (02 10.6.2, 10.6.3; 11 T383): the facts of one period from the orders, their items and their transition records. Nothing is written,
/// no lock is taken and nothing is kept - the summary is computed at every call (KP-53). The counts and sums are made by the database; the lists are bounded by the period (the
/// orders born in it, the carts of those orders, the promises that ran out in it), not by the whole order table, and the summary is one aggregate, not a paged list (09 20 item 2).
/// </summary>
internal sealed class SalesPeriodReader
{
    private static readonly PaymentStatus[] Approved = [PaymentStatus.Paid, PaymentStatus.PartiallyRefunded, PaymentStatus.Refunded];
    private static readonly TransitionCode[] PromiseMarks = [TransitionCode.S5, TransitionCode.S3, TransitionCode.S4];

    private readonly AppDbContext _db;

    public SalesPeriodReader(AppDbContext db)
    {
        _db = db;
    }

    public async Task<SalesPeriodFacts> ReadAsync(DateTimeOffset from, DateTimeOffset to, DateTimeOffset now, CancellationToken ct)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(to, from);
        IQueryable<Order> born = _db.Orders.AsNoTracking().Where(o => o.CreatedAt >= from && o.CreatedAt < to);
        IQueryable<Order> paid = born.Where(o => Approved.Contains(o.PaymentStatus));

        int orderCount = await paid.CountAsync(ct);
        decimal revenue = await paid.SumAsync(o => o.Total, ct);
        List<PaymentMethodCount> methods = await paid
            .GroupBy(o => o.PaymentMethod)
            .Select(g => new PaymentMethodCount(g.Key, g.Count()))
            .ToListAsync(ct);

        IQueryable<OrderItem> paidItems = _db.OrderItems.AsNoTracking().Where(i => paid.Select(o => o.Id).Contains(i.OrderId));
        var productRows = await paidItems
            .GroupBy(i => new { i.ProductId, i.ProductName })
            .Select(g => new
            {
                g.Key.ProductId,
                g.Key.ProductName,
                Quantity = g.Sum(i => i.Quantity),
                Amount = g.Sum(i => ((i.DiscountedUnitPrice ?? i.UnitPrice) * i.Quantity) - i.CouponShare),
                LastAt = g.Max(i => i.CreatedAt),
            })
            .ToListAsync(ct);

        // A product renamed between two orders has two rows; they are one product and the latest name is shown. An item whose product was deleted is keyed by its name.
        List<ProductSalesFact> products =
        [
            .. productRows
                .GroupBy(r => r.ProductId is { } id ? id.ToString() : "name:" + r.ProductName, StringComparer.Ordinal)
                .Select(g => new ProductSalesFact(
                    g.First().ProductId,
                    g.OrderByDescending(r => r.LastAt).First().ProductName,
                    g.Sum(r => r.Quantity),
                    g.Sum(r => r.Amount))),
        ];

        List<OrderOfItem> items = await paidItems.Select(i => new OrderOfItem(i.Id, i.OrderId)).ToListAsync(ct);

        IQueryable<Guid?> cartsOfPeriod = born.Where(o => o.CartId != null).Select(o => o.CartId);
        List<PaymentAttemptOrder> attempts = await _db.Orders.AsNoTracking()
            .Where(o => (o.CartId != null && cartsOfPeriod.Contains(o.CartId)) || (o.CartId == null && o.CreatedAt >= from && o.CreatedAt < to))
            .Select(o => new PaymentAttemptOrder(o.Id, o.CartId, o.CreatedAt, o.PaymentStatus, o.PaymentWindowEndsAt))
            .ToListAsync(ct);

        IReadOnlyList<ShippingPromiseOrder> promises = await ReadShippingPromisesAsync(from, to, now, ct);
        return new SalesPeriodFacts(orderCount, revenue, methods, products, items, attempts, promises);
    }

    /// <summary>The orders of <paramref name="orderItemIds"/> whose payment was approved (Paid, PartiallyRefunded or Refunded); the items of other orders are left out.</summary>
    public async Task<IReadOnlyList<OrderOfItem>> FindPaidOrdersOfItemsAsync(IReadOnlyCollection<Guid> orderItemIds, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(orderItemIds);
        Guid[] ids = [.. orderItemIds.Distinct()];
        if (ids.Length == 0)
        {
            return [];
        }

        return await _db.OrderItems.AsNoTracking()
            .Where(i => ids.Contains(i.Id))
            .Join(_db.Orders.AsNoTracking().Where(o => Approved.Contains(o.PaymentStatus)), i => i.OrderId, o => o.Id, (i, o) => new OrderOfItem(i.Id, o.Id))
            .ToListAsync(ct);
    }

    /// <summary>
    /// The orders with a physical item whose shipping promise ran out in <c>[from, to)</c> and by <paramref name="now"/> (02 10.6.3; K-535): the promise counts from the last payment
    /// approval (O1) that no correction took back (<see cref="ShippingViewReader"/>), and its end is <see cref="DeliveryPeriodRules.ShippingPeriodEnd"/> - the zero-day promise
    /// included. The first uncorrected shipment (S5), the uncorrected cancellation (S3 or S4) and the order's physical items with their quantities come with it.
    /// </summary>
    private async Task<IReadOnlyList<ShippingPromiseOrder>> ReadShippingPromisesAsync(DateTimeOffset from, DateTimeOffset to, DateTimeOffset now, CancellationToken ct)
    {
        DateTimeOffset lookFrom = EarliestApprovalReaching(from);
        var approvals = await _db.TransitionRecords.AsNoTracking()
            .Where(t => t.TransitionCode == TransitionCode.O1 && t.CreatedAt >= lookFrom && t.CreatedAt < to)
            .Where(t => !_db.TransitionRecords.Any(c => c.CorrectedTransitionRecordId == t.Id))
            .Join(
                _db.Orders.AsNoTracking().Where(o => o.ShippingPeriod != null),
                t => t.OrderId,
                o => o.Id,
                (t, o) => new { OrderId = o.Id, Period = o.ShippingPeriod!.Value, ApprovedAt = t.CreatedAt })
            .ToListAsync(ct);

        var ending = approvals
            .GroupBy(a => a.OrderId)
            .Select(g => g.OrderByDescending(a => a.ApprovedAt).First())
            .Select(a => new { a.OrderId, EndsAt = DeliveryPeriodRules.ShippingPeriodEnd(a.ApprovedAt, a.Period, BusinessCalendar.Product) })
            .Where(a => a.EndsAt >= from && a.EndsAt < to && a.EndsAt <= now)
            .ToList();
        if (ending.Count == 0)
        {
            return [];
        }

        Guid[] ids = [.. ending.Select(a => a.OrderId)];
        var marks = await _db.TransitionRecords.AsNoTracking()
            .Where(t => ids.Contains(t.OrderId) && PromiseMarks.Contains(t.TransitionCode))
            .Where(t => !_db.TransitionRecords.Any(c => c.CorrectedTransitionRecordId == t.Id))
            .Select(t => new { t.OrderId, t.TransitionCode, t.CreatedAt })
            .ToListAsync(ct);

        // The physical items with their own quantities: whether a cancellation or removal of items closed all of them before the promise ran out is read from the item
        // records' open quantity by the summary (K-535), not from the order's transitions.
        var physical = await _db.OrderItems.AsNoTracking()
            .Where(i => ids.Contains(i.OrderId) && i.ProductType == ProductType.Physical)
            .Select(i => new { i.OrderId, Item = new PromisedItem(i.Id, i.Quantity) })
            .ToListAsync(ct);

        return
        [
            .. ending.Select(a =>
            {
                DateTimeOffset? shippedAt = marks.Where(m => m.OrderId == a.OrderId && m.TransitionCode == TransitionCode.S5).Min(m => (DateTimeOffset?)m.CreatedAt);
                DateTimeOffset? cancelledAt = marks.Where(m => m.OrderId == a.OrderId && m.TransitionCode != TransitionCode.S5).Min(m => (DateTimeOffset?)m.CreatedAt);
                List<PromisedItem> items = [.. physical.Where(p => p.OrderId == a.OrderId).Select(p => p.Item)];
                return new ShippingPromiseOrder(a.OrderId, a.EndsAt, shippedAt, cancelledAt, items);
            }),
        ];
    }

    /// <summary>
    /// The start of the earliest Turkey day whose payment approval can still have a promise running out at or after <paramref name="from"/>: no frozen promise is longer than the
    /// upper fence of P-5 and the promise's end only moves forward with a later approval, so the days are walked back from <paramref name="from"/>'s day while the longest promise
    /// approved on them still reaches it. The read below starts there; every candidate's exact end is compared afterwards, so the walk only narrows the read.
    /// </summary>
    internal static DateTimeOffset EarliestApprovalReaching(DateTimeOffset from)
    {
        DateOnly day = TurkeyTime.DateOf(from);
        while (DeliveryPeriodRules.ShippingPeriodEnd(TurkeyTime.StartOf(day.AddDays(-1)), ParameterFences.ShippingPeriodUpper, BusinessCalendar.Product) >= from)
        {
            day = day.AddDays(-1);
        }

        return TurkeyTime.StartOf(day);
    }
}
