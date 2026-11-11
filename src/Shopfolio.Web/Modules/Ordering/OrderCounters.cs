using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Modules.AfterSales.Contracts;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Catalog.Contracts.Coupons;

namespace Shopfolio.Modules.Ordering;

/// <summary>
/// The order's side of the counters (03 1.4.4; 06 4.14; 02 4.3): one method per row of <see cref="CounterMomentTable"/>, each writing the
/// reservation rows and asking the Catalog module - the one writer of the counters (05 7.3.3) - for the movement. The caller is the operation
/// that owns the moment, under the order's lock and in its transaction (09 9.1 item 6); nothing here takes a lock of its own before the
/// catalog's rows or opens a transaction. The variant's deletion (1.4.4.9) has no method: the reservation row cascades with the variant.
/// </summary>
internal sealed class OrderCounters
{
    private readonly AppDbContext _db;
    private readonly ICatalogModule _catalog;
    private readonly IAfterSalesModule _afterSales;
    private readonly TimeProvider _time;

    public OrderCounters(AppDbContext db, ICatalogModule catalog, IAfterSalesModule afterSales, TimeProvider time)
    {
        _db = db;
        _catalog = catalog;
        _afterSales = afterSales;
        _time = time;
    }

    /// <summary>
    /// 1.4.4.1 (path 1): a row for every physical and service item's variant and one for the coupon; the reserved quantities rise only if what is
    /// left is enough. A digital item reserves nothing. False when one reservation did not fit: the caller refuses the order and writes no row.
    /// </summary>
    public async Task<bool> ReserveAsync(Order order, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(order);
        List<VariantQuantity> units = await ReservableUnitsAsync(order, ct);
        if (units.Count == 0 && order.CouponId is null)
        {
            return true;
        }

        if (!await _catalog.ReserveAsync(new StockClaim(units, order.CouponId), null, ct))
        {
            return false;
        }

        WriteRows(order, units);
        return true;
    }

    /// <summary>
    /// 1.4.4.1 for the order being born (T348; 02 3.17.3): the same reservation as <see cref="ReserveAsync(Order, CancellationToken)"/>, read from the confirmation's frozen
    /// <paramref name="items"/> that are not saved yet. False when one reservation did not fit: no row is written and the caller refuses the order.
    /// </summary>
    public async Task<bool> ReserveAsync(Order order, IReadOnlyList<OrderItem> items, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(order);
        ArgumentNullException.ThrowIfNull(items);
        List<VariantQuantity> units = Merge([.. items
            .Where(i => i.ProductType != ProductType.Digital && i.VariantId != null)
            .Select(i => new VariantQuantity(i.VariantId!.Value, i.Quantity))]); // non-null: the Where above keeps only items with a variant
        if (units.Count == 0 && order.CouponId is null)
        {
            return true;
        }

        if (!await _catalog.ReserveAsync(new StockClaim(units, order.CouponId), null, ct))
        {
            return false;
        }

        WriteRows(order, units);
        return true;
    }

    /// <summary>1.4.4.2 (path 2): the reserved units fall finally and the reserved right becomes a used one; the rows are deleted.</summary>
    public async Task CommitAsync(Order order, CancellationToken ct)
    {
        StockClaim? claim = await TakeRowsAsync(order, ct);
        if (claim is not null)
        {
            await _catalog.CommitAsync(claim, ct);
        }
    }

    /// <summary>1.4.4.3 (path 3): the unpaid order's reservation is freed - the reserved units and the reserved right go down; the rows are deleted.</summary>
    public async Task ReleaseAsync(Order order, CancellationToken ct)
    {
        StockClaim? claim = await TakeRowsAsync(order, ct);
        if (claim is not null)
        {
            await _catalog.ReleaseAsync(claim, ct);
        }
    }

    /// <summary>
    /// 1.4.4.4, 1.4.4.5 and 1.4.4.6: the paid order's units the record returns - <paramref name="units"/>, chosen by the operation by its row: the
    /// cancelled or removed units; the terminated units of an item not shipped; the received goods the firm adds to stock and the withdrawn service
    /// capacity - go back; the used coupon right goes back when <see cref="IsCouponReturnDueAsync"/> says this record finishes the order.
    /// </summary>
    public async Task ReturnAsync(Order order, IReadOnlyList<VariantQuantity> units, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(order);
        ArgumentNullException.ThrowIfNull(units);
        Guid? coupon = await IsCouponReturnDueAsync(order, ct) ? order.CouponId : null;
        List<VariantQuantity> merged = Merge(units);
        if (merged.Count > 0 || coupon is not null)
        {
            await _catalog.ReturnAsync(new StockClaim(merged, coupon), ct);
        }
    }

    /// <summary>
    /// 1.4.4.7 (path 6): the wrong cancellation's correction on a paid order takes again the units the cancellation freed - <paramref name="units"/> -
    /// and they fall at once; no row is written. The right counts as used again when the cancellation had given it back. False when a variant has not
    /// enough left: the caller refuses the correction.
    /// </summary>
    public async Task<bool> RetakeAsync(Order order, IReadOnlyList<VariantQuantity> units, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(order);
        ArgumentNullException.ThrowIfNull(units);
        Guid? coupon = order.CouponId is not null && await IsCouponReturnedAsync(order, ct) ? order.CouponId : null;
        List<VariantQuantity> merged = Merge(units);
        return (merged.Count == 0 && coupon is null) || await _catalog.RetakeAsync(new StockClaim(merged, coupon), ct);
    }

    /// <summary>
    /// The variants among <paramref name="units"/> that have not enough left for <see cref="RetakeAsync"/> now (1.4.4.7; 03 1.6.2.2): what is left is the stock
    /// minus the reserved quantity; an unlimited variant and a deleted one (K-1091) are never short. A plain read before the correction's first write, so the
    /// refusal can name the items; the conditional write of <see cref="RetakeAsync"/> stays the answer that counts.
    /// </summary>
    public async Task<IReadOnlyList<Guid>> ShortVariantsAsync(IReadOnlyList<VariantQuantity> units, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(units);
        List<Guid> shortVariants = [];
        foreach (VariantQuantity unit in Merge(units))
        {
            if (await _catalog.GetStockStateAsync(unit.VariantId, ct) is { StockQuantity: { } stock } state && stock - state.ReservedQuantity < unit.Quantity)
            {
                shortVariants.Add(unit.VariantId);
            }
        }

        return shortVariants;
    }

    /// <summary>
    /// 1.4.4.8 (path 5): the transfer mark's correction keeps the reservation - the finally decreased units return to it, the used right becomes a
    /// reserved one and the rows are written again; a deleted variant's row is not.
    /// </summary>
    public async Task RestoreAsync(Order order, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(order);
        List<VariantQuantity> units = await ReservableUnitsAsync(order, ct);
        if (units.Count == 0 && order.CouponId is null)
        {
            return;
        }

        await _catalog.RestoreAsync(new StockClaim(units, order.CouponId), ct);
        List<VariantQuantity> kept = [];
        foreach (VariantQuantity unit in units)
        {
            if (await _catalog.GetStockStateAsync(unit.VariantId, ct) is not null)
            {
                kept.Add(unit);
            }
        }

        WriteRows(order, kept);
    }

    /// <summary>
    /// The coupon right's return (06 4.14, the paragraph after path 6; 03 1.4.4.4-1.4.4.6; K-791, K-1104, K-1106): the right goes back once, with the
    /// record that brings every unit of the order to its last state - no open quantity is left and no physical withdrawn unit awaits its receipt - and
    /// never when a unit ended in a return rejection or a standing "goods did not come back" closing. The order before the caller's unsaved record had not
    /// yet given the right back, so a second call does not give it back twice; a closing the caller's receipt opens (06 4.28; K-1055) had ended the order
    /// with a loss before it, so the receipt that brings the late goods' last unit gives the right back once. An unpaid order's right is the reservation's,
    /// freed by <see cref="ReleaseAsync"/>.
    /// </summary>
    public async Task<bool> IsCouponReturnDueAsync(Order order, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(order);
        if (order.CouponId is null || await _db.StockReservations.AnyAsync(r => r.OrderId == order.Id, ct))
        {
            return false;
        }

        List<OrderItem> items = await _db.OrderItems.Where(i => i.OrderId == order.Id).ToListAsync(ct);
        IReadOnlyDictionary<Guid, ItemUnitFacts> after = await _afterSales.UnitFactsAsync([.. items.Select(i => i.Id)], true, ct);
        IReadOnlyDictionary<Guid, ItemUnitFacts> before = await _afterSales.UnitFactsAsync([.. items.Select(i => i.Id)], false, ct);
        return ReturnsRight(items, after) && !ReturnsRight(items, before);
    }

    private async Task<bool> IsCouponReturnedAsync(Order order, CancellationToken ct)
    {
        List<OrderItem> items = await _db.OrderItems.Where(i => i.OrderId == order.Id).ToListAsync(ct);
        IReadOnlyDictionary<Guid, ItemUnitFacts> facts = await _afterSales.UnitFactsAsync([.. items.Select(i => i.Id)], false, ct);
        return ReturnsRight(items, facts);
    }

    // The order whose units all reached their last state with no loss has its right back.
    private bool ReturnsRight(List<OrderItem> items, IReadOnlyDictionary<Guid, ItemUnitFacts> facts) => IsFinal(items, facts) && !HasLoss(facts);

    // "Every unit of every item closed" is the catalog's one rule of the right (CouponRules.ReturnsRight; D-483); the physical withdrawn unit awaiting its receipt
    // is the order's own condition on top of it (K-1104).
    private bool IsFinal(List<OrderItem> items, IReadOnlyDictionary<Guid, ItemUnitFacts> facts)
    {
        return items.Count > 0
            && _catalog.ReturnsCouponRight([.. items.Select(i => new ItemClosure(i.Quantity, Math.Clamp(facts[i.Id].Closed, 0, i.Quantity)))])
            && items.All(i => i.ProductType != ProductType.Physical || facts[i.Id].AwaitingReceipt <= 0);
    }

    private static bool HasLoss(IReadOnlyDictionary<Guid, ItemUnitFacts> facts) => facts.Values.Any(f => f.Rejected > 0 || f.NotReturned > 0);

    /// <summary>The units an order reserves: its physical and service items' variants, summed per variant; a digital item and an item whose variant is gone reserve nothing.</summary>
    private async Task<List<VariantQuantity>> ReservableUnitsAsync(Order order, CancellationToken ct)
    {
        List<OrderItem> items = await _db.OrderItems
            .Where(i => i.OrderId == order.Id && i.ProductType != ProductType.Digital && i.VariantId != null)
            .ToListAsync(ct);
        return Merge([.. items.Select(i => new VariantQuantity(i.VariantId!.Value, i.Quantity))]); // non-null: the Where above keeps only items with a variant
    }

    private static List<VariantQuantity> Merge(IReadOnlyList<VariantQuantity> units)
    {
        return [.. units.Where(u => u.Quantity > 0).GroupBy(u => u.VariantId).Select(g => new VariantQuantity(g.Key, g.Sum(u => u.Quantity)))];
    }

    private void WriteRows(Order order, List<VariantQuantity> units)
    {
        DateTimeOffset now = _time.GetUtcNow();
        foreach (VariantQuantity unit in units)
        {
            _db.StockReservations.Add(StockReservation.ForVariant(order.Id, unit.VariantId, unit.Quantity, now));
        }

        if (order.CouponId is { } couponId)
        {
            _db.StockReservations.Add(StockReservation.ForCoupon(order.Id, couponId, now));
        }
    }

    /// <summary>The order's reservation rows as one claim, the rows removed from the context; null when the order holds none.</summary>
    private async Task<StockClaim?> TakeRowsAsync(Order order, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(order);
        List<StockReservation> rows = await _db.StockReservations.Where(r => r.OrderId == order.Id).ToListAsync(ct);
        if (rows.Count == 0)
        {
            return null;
        }

        _db.StockReservations.RemoveRange(rows);
        List<VariantQuantity> units = [.. rows.Where(r => r.VariantId is not null).Select(r => new VariantQuantity(r.VariantId!.Value, r.Quantity))]; // non-null: the Where before it keeps only rows with a variant
        return new StockClaim(units, rows.FirstOrDefault(r => r.CouponId is not null)?.CouponId);
    }
}
