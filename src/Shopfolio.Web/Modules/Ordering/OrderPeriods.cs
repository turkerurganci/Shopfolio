using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Modules.AfterSales.Contracts;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Ordering.Contracts;

namespace Shopfolio.Modules.Ordering;

/// <summary>
/// The delivery periods read from an order's own records (03 4.1.10-4.1.13; 06 4.17): <see cref="DeliveryPeriodRules"/> over the payment's approval,
/// the order's frozen shipping promise, the items' fulfilment periods, delivery dates and open quantities. The payment's approval is not stored - it is
/// the moment of the order's last Ö1 record that no correction took back, so a transfer mark corrected and given again counts from the new mark
/// (03 4.1.10; K-685). Plain reads: nothing here writes, locks or acts when a period runs out, and the instant is the caller's.
/// </summary>
internal sealed class OrderPeriods
{
    private readonly AppDbContext _db;
    private readonly IAfterSalesModule _afterSales;

    public OrderPeriods(AppDbContext db, IAfterSalesModule afterSales)
    {
        _db = db;
        _afterSales = afterSales;
    }

    /// <summary>
    /// The payment's approval: the moment of the last Ö1 record of the order that no correction record points to - the card's success notice or the
    /// transfer's "paid" mark; null while the payment is not approved (none written, or the transfer mark corrected and not given again).
    /// </summary>
    public async Task<DateTimeOffset?> PaymentApprovedAtAsync(Guid orderId, CancellationToken ct)
    {
        return await _db.TransitionRecords
            .Where(t => t.OrderId == orderId && t.TransitionCode == TransitionCode.O1
                && !_db.TransitionRecords.Any(c => c.TransitionCode == TransitionCode.Correction && c.CorrectedTransitionRecordId == t.Id))
            .MaxAsync(t => (DateTimeOffset?)t.CreatedAt, ct);
    }

    /// <summary>
    /// Z-10 - where the order's shipping promise stands at <paramref name="now"/> while the order waits to be shipped (Preparing; 03 4.1.10): null when
    /// the order has no physical item, its payment is not approved, or it is no longer preparing - the period ends when the order is shipped.
    /// </summary>
    public async Task<PeriodStanding?> ShippingStandingAsync(Guid orderId, DateTimeOffset now, CancellationToken ct)
    {
        Order order = await _db.Orders.AsNoTracking().SingleAsync(o => o.Id == orderId, ct);
        if (order.OrderStatus != OrderStatus.Preparing || order.ShippingPeriod is not { } businessDays
            || await PaymentApprovedAtAsync(orderId, ct) is not { } approvedAt)
        {
            return null;
        }

        return DeliveryPeriodRules.ShippingStanding(approvedAt, businessDays, now, BusinessCalendar.Product);
    }

    /// <summary>
    /// Z-39 - where a service item's fulfilment period stands at <paramref name="now"/> while it waits to be completed (03 4.1.13): null for another type,
    /// a completed item (the "tamamlandı" mark), an item closed by its records, or an order whose payment is not approved.
    /// </summary>
    public async Task<PeriodStanding?> FulfilmentStandingAsync(Guid orderItemId, DateTimeOffset now, CancellationToken ct)
    {
        OrderItem item = await _db.OrderItems.AsNoTracking().SingleAsync(i => i.Id == orderItemId, ct);
        if (item.ProductType != ProductType.Service || item.FulfilmentPeriod is not { } days || item.DeliveryMarkedAt is not null
            || await OpenQuantityAsync(item, ct) <= 0 || await PaymentApprovedAtAsync(item.OrderId, ct) is not { } approvedAt)
        {
            return null;
        }

        return DeliveryPeriodRules.FulfilmentStanding(approvedAt, days, now);
    }

    /// <summary>Z-11 - whether the order's late delivery termination is open at <paramref name="now"/> (<see cref="DeliveryPeriodRules.IsLateDeliveryTerminationOpen"/>).</summary>
    public async Task<bool> IsLateDeliveryTerminationOpenAsync(Guid orderId, DateTimeOffset now, CancellationToken ct)
    {
        Order order = await _db.Orders.AsNoTracking().SingleAsync(o => o.Id == orderId, ct);
        List<OrderItem> items = await _db.OrderItems.AsNoTracking().Where(i => i.OrderId == orderId).ToListAsync(ct);
        IReadOnlyDictionary<Guid, int> closed = await _afterSales.ClosedQuantitiesAsync([.. items.Select(i => i.Id)], ct);
        IEnumerable<ItemDeliveryState> states = items.Select(i => new ItemDeliveryState(i.ProductType, i.Quantity - closed.GetValueOrDefault(i.Id), i.DeliveryDate));
        return DeliveryPeriodRules.IsLateDeliveryTerminationOpen(order.OrderStatus, order.PaymentStatus, order.CreatedAt, states, now);
    }

    /// <summary>
    /// Whether the item's cancellation at <paramref name="cancelledAt"/> carries the legal interest warning (03 4.1.10, 4.1.13): the order's shipping
    /// promise or the service item's fulfilment period had run out (<see cref="DeliveryPeriodRules.IsLegalInterestWarningDue"/>).
    /// </summary>
    public async Task<bool> IsLegalInterestWarningDueAsync(Guid orderItemId, DateTimeOffset cancelledAt, CancellationToken ct)
    {
        OrderItem item = await _db.OrderItems.AsNoTracking().SingleAsync(i => i.Id == orderItemId, ct);
        int? shippingPeriod = await _db.Orders.Where(o => o.Id == item.OrderId).Select(o => o.ShippingPeriod).SingleAsync(ct);
        DateTimeOffset? approvedAt = await PaymentApprovedAtAsync(item.OrderId, ct);
        return DeliveryPeriodRules.IsLegalInterestWarningDue(item.ProductType, approvedAt, shippingPeriod, item.FulfilmentPeriod, cancelledAt);
    }

    private async Task<int> OpenQuantityAsync(OrderItem item, CancellationToken ct)
    {
        IReadOnlyDictionary<Guid, int> closed = await _afterSales.ClosedQuantitiesAsync([item.Id], ct);
        return item.Quantity - closed.GetValueOrDefault(item.Id);
    }
}
