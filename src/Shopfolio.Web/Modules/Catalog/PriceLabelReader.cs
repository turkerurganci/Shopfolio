using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Modules.Catalog.Contracts.Pricing;

namespace Shopfolio.Modules.Catalog;

/// <summary>
/// The plain read of a variant's price label (02 3.8.5, 3.9.2, 3.9.3, 12.1.5, 12.1.6; 11 T336 criteria 1, 2, 8; 11 T337 criteria 4, 5): the price as stored - the
/// number the administrator typed -, the product's VAT rate, the origin line when the product shows it, the date the price applies since - the start of the
/// variant's last price history row that has started by now (02 3.9.3; "now" from <see cref="TimeProvider"/>) -, the unit price of a product sold by measure, and
/// the declaration of the product's running discount: the discounted price, the reference price the system computes from the variant's own history and the range's
/// dates. The unit price is of the price that applies - the discounted price while the discount runs (D-345). Nothing is written and no lock is taken. The
/// country's name is the Settings module's list (D-344).
/// </summary>
internal sealed class PriceLabelReader
{
    private readonly AppDbContext _db;
    private readonly TimeProvider _time;

    public PriceLabelReader(AppDbContext db, TimeProvider time)
    {
        _db = db;
        _time = time;
    }

    /// <summary>The label of <paramref name="variantId"/>; null when there is no such variant.</summary>
    public async Task<PriceLabel?> GetAsync(Guid variantId, CancellationToken ct)
    {
        var row = await _db.Variants.AsNoTracking()
            .Where(v => v.Id == variantId)
            .Join(
                _db.Products.AsNoTracking(),
                v => v.ProductId,
                p => p.Id,
                (v, p) => new { v.ProductId, v.Price, v.FirstOnSaleAt, v.NetQuantity, p.VatRate, p.ProductType, p.OriginCountryId, p.MeasurementUnit })
            .SingleOrDefaultAsync(ct);
        if (row is null)
        {
            return null;
        }

        DateTimeOffset now = _time.GetUtcNow();
        DateTimeOffset? since = await _db.PriceHistories.AsNoTracking()
            .Where(h => h.VariantId == variantId && h.StartedAt <= now)
            .OrderByDescending(h => h.StartedAt)
            .ThenByDescending(h => h.Id)
            .Select(h => (DateTimeOffset?)h.StartedAt)
            .FirstOrDefaultAsync(ct);

        DiscountLine? discount = null;
        if ((await PriceHistoryReads.RunningDiscountsAsync(_db, [row.ProductId], now, ct)).TryGetValue(row.ProductId, out RunningDiscount? running))
        {
            ReferenceInput variant = new(variantId, row.Price, row.FirstOnSaleAt);
            Dictionary<Guid, decimal> references = await PriceHistoryReads.ReferencesAsync(_db, running.Rule, DiscountRules.StartsAt(running.Terms), [variant], ct);
            discount = DiscountRules.LineOf(running.Terms, row.Price, references[variantId]);
        }

        bool sellsByMeasure = row.MeasurementUnit is not null;
        return new PriceLabel(
            row.Price,
            row.VatRate,
            PriceLabelRules.ShowsOrigin(row.ProductType, row.OriginCountryId) ? row.OriginCountryId : null,
            since,
            row.MeasurementUnit,
            sellsByMeasure ? row.NetQuantity : null,
            PriceLabelRules.UnitPrice(discount?.DiscountedPrice ?? row.Price, row.MeasurementUnit, row.NetQuantity),
            discount);
    }
}
