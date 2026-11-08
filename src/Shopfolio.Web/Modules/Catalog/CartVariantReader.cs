using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Modules.Catalog.Contracts;

namespace Shopfolio.Modules.Catalog;

/// <summary>
/// The plain read of the live facts a cart item shows (T346; 02 3.16.5-3.16.7): per variant the product's name, the price that applies now, whether the variant and its product are
/// Published and whether the variant is sold out. The price is the variant's own price or - while the product's discount runs - the discounted price (<see cref="DiscountRules.AppliedPrice"/>),
/// the same value the price label's unit price is taken from. A variant that does not exist is not in the answer. Nothing is written and no lock is taken.
/// </summary>
internal sealed class CartVariantReader
{
    private readonly AppDbContext _db;
    private readonly TimeProvider _time;

    public CartVariantReader(AppDbContext db, TimeProvider time)
    {
        _db = db;
        _time = time;
    }

    public async Task<IReadOnlyList<CartVariantFact>> ListAsync(IReadOnlyCollection<Guid> variantIds, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(variantIds);
        Guid[] ids = [.. variantIds.Distinct()];
        if (ids.Length == 0)
        {
            return [];
        }

        var rows = await _db.Variants.AsNoTracking()
            .Where(v => ids.Contains(v.Id))
            .Join(
                _db.Products.AsNoTracking(),
                v => v.ProductId,
                p => p.Id,
                (v, p) => new
                {
                    VariantId = v.Id,
                    v.ProductId,
                    ProductName = p.Name,
                    p.ProductType,
                    ProductStatus = p.PublishStatus,
                    VariantStatus = v.PublishStatus,
                    v.Price,
                    v.StockQuantity,
                    v.ReservedQuantity,
                })
            .ToListAsync(ct);

        DateTimeOffset now = _time.GetUtcNow();
        Dictionary<Guid, RunningDiscount> running = await PriceHistoryReads.RunningDiscountsAsync(_db, [.. rows.Select(r => r.ProductId).Distinct()], now, ct);
        return [.. rows.Select(r => new CartVariantFact(
            r.VariantId,
            r.ProductId,
            r.ProductName,
            r.ProductType,
            r.ProductStatus == PublishStatus.Published && r.VariantStatus == PublishStatus.Published,
            StockRules.IsSoldOut(r.StockQuantity, r.ReservedQuantity),
            DiscountRules.AppliedPrice(r.Price, running.TryGetValue(r.ProductId, out RunningDiscount? discount) ? discount.Terms : null, now)))];
    }
}
