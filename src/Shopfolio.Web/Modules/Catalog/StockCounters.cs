using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Catalog.Contracts.Variants;

namespace Shopfolio.Modules.Catalog;

/// <summary>
/// The one writer of the counters (09 10.2 item 4; 05 7.3.3; 06 2.6.5): the variant's stock or capacity field, its reserved quantity and the
/// coupon's reserved and used rights are written here and nowhere else, each by one conditional update - the condition and the arithmetic are in
/// the statement, nothing is read, counted and written back. A reservation is written only if what is left is enough; the panel's stock write
/// only if the field still holds the shown value and the new value is not below the reserved quantity. The writes take the rows' locks in the
/// order of 05 7.3.5 - the variants by id, the coupon last - and report each to <see cref="LockSequence"/>. None of them touches the product's
/// version (K-980). A freeing or a final decrease that finds fewer reserved units than it frees is a broken invariant (06 2.7.1) and throws.
/// </summary>
internal sealed class StockCounters
{
    private readonly AppDbContext _db;

    public StockCounters(AppDbContext db)
    {
        _db = db;
    }

    /// <summary>Applies the movements in the lock order; false when a reservation did not fit (the rows already written fall with the transaction).</summary>
    public async Task<bool> ApplyAsync(IReadOnlyList<(StockClaim Claim, CounterMove Move)> parts, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(parts);
        if (_db.Database.CurrentTransaction is null)
        {
            throw new InvalidOperationException("The counters are written only inside the caller's transaction (09 9.1 item 3).");
        }

        foreach ((StockClaim claim, _) in parts)
        {
            if (claim.Variants.Any(v => v.Quantity <= 0) || claim.Variants.Select(v => v.VariantId).Distinct().Count() != claim.Variants.Count)
            {
                throw new ArgumentException("A stock claim names each variant once with a unit count above zero (06 4.14).", nameof(parts));
            }
        }

        Comparer<Guid> byId = Comparer<Guid>.Create(LockSequence.CompareKeys);
        List<(Guid VariantId, CounterMove Move, int Quantity)> variantSteps = [.. parts
            .SelectMany(p => p.Claim.Variants.Select(v => (v.VariantId, p.Move, v.Quantity)))
            .OrderBy(s => s.VariantId, byId)
            .ThenBy(s => s.Move)];
        foreach ((Guid variantId, CounterMove move, int quantity) in variantSteps)
        {
            LockSequence.Enter(_db, LockKind.Variant, variantId);
            int written = await WriteVariantAsync(variantId, move, quantity, ct);
            if (written == 0 && !Settle(move, isCoupon: false, $"variant {variantId}"))
            {
                return false;
            }
        }

        List<(Guid CouponId, CounterMove Move)> couponSteps = [.. parts
            .Where(p => p.Claim.CouponId is not null)
            .Select(p => (CouponId: p.Claim.CouponId.GetValueOrDefault(), p.Move))
            .OrderBy(s => s.CouponId, byId)
            .ThenBy(s => s.Move)];
        foreach ((Guid couponId, CounterMove move) in couponSteps)
        {
            LockSequence.Enter(_db, LockKind.Coupon, couponId);
            int written = await WriteCouponAsync(couponId, move, ct);
            if (written == 0 && !Settle(move, isCoupon: true, $"coupon {couponId}"))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>The panel's write of the stock or capacity field: one conditional update against the shown value and the reserved quantity.</summary>
    public async Task<StockQuantityWriteResult> SaveStockQuantityAsync(Guid variantId, int? shownValue, int? newValue, CancellationToken ct)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(newValue ?? 0, nameof(newValue));
        if (_db.Database.CurrentTransaction is null)
        {
            throw new InvalidOperationException("The stock field is written only inside the caller's transaction (09 9.1 item 3).");
        }

        LockSequence.Enter(_db, LockKind.Variant, variantId);

        // The type decides what the field may hold (02 3.6.2): a physical variant needs a number, a digital variant has no field. The type is the product's, read here; a
        // variant that is gone has none and falls to the conditional update's own answer below.
        ProductType? type = await _db.Variants.AsNoTracking()
            .Where(v => v.Id == variantId)
            .Join(_db.Products, v => v.ProductId, p => p.Id, (v, p) => (ProductType?)p.ProductType)
            .SingleOrDefaultAsync(ct);
        if (type is { } known)
        {
            IReadOnlyList<StockProblem> problems = StockRules.Check(known, newValue);
            if (problems.Contains(StockProblem.PhysicalWithoutStock))
            {
                return StockQuantityWriteResult.StockRequired;
            }

            if (problems.Contains(StockProblem.DigitalWithStock))
            {
                return StockQuantityWriteResult.NotTracked;
            }
        }

        int written = await _db.Variants
            .Where(v => v.Id == variantId && v.StockQuantity == shownValue && (newValue == null || newValue >= v.ReservedQuantity))
            .ExecuteUpdateAsync(s => s.SetProperty(v => v.StockQuantity, newValue), ct);
        if (written == 1)
        {
            return StockQuantityWriteResult.Written;
        }

        // Nothing was written: the reason is read in the same transaction, under the row lock the update took (09 10.2 item 4).
        var now = await _db.Variants.AsNoTracking()
            .Where(v => v.Id == variantId)
            .Select(v => new { v.StockQuantity, v.ReservedQuantity })
            .SingleOrDefaultAsync(ct);
        return now is null || now.StockQuantity != shownValue
            ? StockQuantityWriteResult.ShownValueChanged
            : StockQuantityWriteResult.BelowReservedBy(now.ReservedQuantity);
    }

    /// <summary>The panel's read of the stock field and the reserved quantity beside it (02 3.6.7); null when the variant is gone. A plain read.</summary>
    public async Task<VariantStockState?> GetStateAsync(Guid variantId, CancellationToken ct)
    {
        return await _db.Variants.AsNoTracking()
            .Where(v => v.Id == variantId)
            .Select(v => new VariantStockState(v.StockQuantity, v.ReservedQuantity))
            .SingleOrDefaultAsync(ct);
    }

    /// <summary>
    /// The sold-out state of a product's variants as the visitor sees it (02 3.6.3-3.6.5): the published variants, each only sold out or not - never a count - and the product
    /// sold out when every visible variant is. A draft or archived variant is not shown to the visitor, so it neither counts nor appears. A plain read.
    /// </summary>
    public async Task<ProductAvailability> GetAvailabilityAsync(Guid productId, CancellationToken ct)
    {
        var rows = await _db.Variants.AsNoTracking()
            .Where(v => v.ProductId == productId && v.PublishStatus == PublishStatus.Published)
            .OrderBy(v => v.CreatedAt).ThenBy(v => v.Id)
            .Select(v => new { v.Id, v.StockQuantity, v.ReservedQuantity })
            .ToListAsync(ct);
        List<VariantAvailability> variants = [.. rows.Select(r => new VariantAvailability(r.Id, StockRules.IsSoldOut(r.StockQuantity, r.ReservedQuantity)))];
        return new ProductAvailability(variants, StockRules.IsProductSoldOut([.. variants.Select(v => v.IsSoldOut)]));
    }

    // A reservation that did not fit is the caller's refusal; a return to a deleted or unlimited variant has nothing to add; a freeing, a final
    // decrease or a coupon return that wrote nothing means the counts no longer match the reservation rows and the records.
    private static bool Settle(CounterMove move, bool isCoupon, string row)
    {
        return move switch
        {
            CounterMove.Reserve => false,
            CounterMove.Return when !isCoupon => true,
            _ => throw new InvalidOperationException($"The {move} of {row} found fewer reserved units than it frees (06 2.7.1, 4.14)."),
        };
    }

    private Task<int> WriteVariantAsync(Guid variantId, CounterMove move, int quantity, CancellationToken ct)
    {
        IQueryable<Variant> row = _db.Variants.Where(v => v.Id == variantId);
        return move switch
        {
            CounterMove.Reserve => row
                .Where(v => v.StockQuantity == null || v.StockQuantity - v.ReservedQuantity >= quantity)
                .ExecuteUpdateAsync(s => s.SetProperty(v => v.ReservedQuantity, v => v.ReservedQuantity + quantity), ct),
            CounterMove.Release => row
                .Where(v => v.ReservedQuantity >= quantity)
                .ExecuteUpdateAsync(s => s.SetProperty(v => v.ReservedQuantity, v => v.ReservedQuantity - quantity), ct),
            CounterMove.Commit => row
                .Where(v => v.ReservedQuantity >= quantity)
                .ExecuteUpdateAsync(
                    s => s
                        .SetProperty(v => v.StockQuantity, v => v.StockQuantity - quantity)
                        .SetProperty(v => v.ReservedQuantity, v => v.ReservedQuantity - quantity),
                    ct),
            CounterMove.Return => row
                .Where(v => v.StockQuantity != null)
                .ExecuteUpdateAsync(s => s.SetProperty(v => v.StockQuantity, v => v.StockQuantity + quantity), ct),
            _ => throw new ArgumentOutOfRangeException(nameof(move)),
        };
    }

    private Task<int> WriteCouponAsync(Guid couponId, CounterMove move, CancellationToken ct)
    {
        IQueryable<Coupon> row = _db.Coupons.Where(c => c.Id == couponId);
        return move switch
        {
            CounterMove.Reserve => row
                .Where(c => c.UsedCount + c.ReservedCount < c.TotalUsageLimit)
                .ExecuteUpdateAsync(s => s.SetProperty(c => c.ReservedCount, c => c.ReservedCount + 1), ct),
            CounterMove.Release => row
                .Where(c => c.ReservedCount >= 1)
                .ExecuteUpdateAsync(s => s.SetProperty(c => c.ReservedCount, c => c.ReservedCount - 1), ct),
            CounterMove.Commit => row
                .Where(c => c.ReservedCount >= 1)
                .ExecuteUpdateAsync(
                    s => s
                        .SetProperty(c => c.ReservedCount, c => c.ReservedCount - 1)
                        .SetProperty(c => c.UsedCount, c => c.UsedCount + 1),
                    ct),
            CounterMove.Return => row
                .Where(c => c.UsedCount >= 1)
                .ExecuteUpdateAsync(s => s.SetProperty(c => c.UsedCount, c => c.UsedCount - 1), ct),
            _ => throw new ArgumentOutOfRangeException(nameof(move)),
        };
    }
}
