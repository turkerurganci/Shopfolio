using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Modules.Catalog.Contracts.Pricing;

namespace Shopfolio.Modules.Catalog;

/// <summary>
/// The plain read of a variant's price label (02 3.8.5, 12.1.5; 11 T336 criteria 1, 2, 8): the price as stored - the number the administrator typed -, the
/// product's VAT rate, the origin line when the product shows it, the date the price applies since - the start of the variant's last price history row that has
/// started by now (02 3.9.3; "now" from <see cref="TimeProvider"/>) - and the unit price of a product sold by measure. Nothing is written and no lock is taken.
/// The unit price comes from the variant's price; in a discount the discounted price is the one that applies, which the discount's task passes through
/// <see cref="PriceLabelRules.UnitPrice"/> (D-345). The country's name is the Settings module's list (D-344).
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
                (v, p) => new { v.Price, v.NetQuantity, p.VatRate, p.ProductType, p.OriginCountryId, p.MeasurementUnit })
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

        bool sellsByMeasure = row.MeasurementUnit is not null;
        return new PriceLabel(
            row.Price,
            row.VatRate,
            PriceLabelRules.ShowsOrigin(row.ProductType, row.OriginCountryId) ? row.OriginCountryId : null,
            since,
            row.MeasurementUnit,
            sellsByMeasure ? row.NetQuantity : null,
            PriceLabelRules.UnitPrice(row.Price, row.MeasurementUnit, row.NetQuantity));
    }
}
