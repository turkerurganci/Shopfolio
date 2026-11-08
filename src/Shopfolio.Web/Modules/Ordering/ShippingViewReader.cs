using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Modules.Ordering.Contracts;

namespace Shopfolio.Modules.Ordering;

/// <summary>
/// The plain read behind the customer's delivery area (02 3.20.4-3.20.7; 04 5.16.8; 11 T350 criteria 7-9): the order's frozen promise, when its period started - the last payment
/// confirmation (Ö1) that was not corrected - when it runs out, when the order was last shipped (S5 or S8) and how the carrier shows. Nothing is written and no lock is taken. The
/// carrier text is <see cref="CarrierCatalog"/>'s: a listed company with a number gets its "Takip et" link, "Diğer" and a retired company get name and number only.
/// </summary>
internal sealed class ShippingViewReader
{
    private readonly AppDbContext _db;

    public ShippingViewReader(AppDbContext db)
    {
        _db = db;
    }

    /// <summary>The view of <paramref name="orderId"/>; null when there is no such order.</summary>
    public async Task<OrderShippingView?> GetAsync(Guid orderId, CancellationToken ct)
    {
        var order = await _db.Orders.AsNoTracking()
            .Where(o => o.Id == orderId)
            .Select(o => new { o.ShippingPeriod, o.Carrier, o.OtherCarrierName, o.TrackingNumber, o.IsOwnVehicleDelivery })
            .SingleOrDefaultAsync(ct);
        if (order is null)
        {
            return null;
        }

        DateTimeOffset? confirmedAt = await _db.TransitionRecords.AsNoTracking()
            .Where(t => t.OrderId == orderId && t.TransitionCode == TransitionCode.O1)
            .Where(t => !_db.TransitionRecords.Any(c => c.CorrectedTransitionRecordId == t.Id))
            .MaxAsync(t => (DateTimeOffset?)t.CreatedAt, ct);

        DateTimeOffset? shippedAt = await _db.TransitionRecords.AsNoTracking()
            .Where(t => t.OrderId == orderId && (t.TransitionCode == TransitionCode.S5 || t.TransitionCode == TransitionCode.S8))
            .MaxAsync(t => (DateTimeOffset?)t.CreatedAt, ct);

        bool hasCarrierData = order.IsOwnVehicleDelivery || order.Carrier is not null || order.OtherCarrierName is not null;
        ShippingDisplay? display = shippedAt is not null && hasCarrierData
            ? CarrierCatalog.Default.Describe(order.Carrier, order.OtherCarrierName, order.TrackingNumber, order.IsOwnVehicleDelivery)
            : null;

        int? period = order.ShippingPeriod;
        return new OrderShippingView(
            period,
            period is { } days ? ShippingPromise.Text(days) : null,
            confirmedAt,
            period is { } p && confirmedAt is { } at ? ShippingPromise.PromiseEnds(at, p) : null,
            shippedAt,
            display);
    }
}
