using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Modules.Catalog.Contracts;
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

        return LabelOf(row.Price, row.VatRate, row.ProductType, row.OriginCountryId, since, row.MeasurementUnit, row.NetQuantity, discount);
    }

    /// <summary>
    /// The labels of all the variants of <paramref name="productId"/>, by variant (09 20 item 4): the product, its variants, the last started price row of each and the product's running discount with
    /// the reference prices are each one read, whatever the number of variants. Each label is the one <see cref="GetAsync"/> gives; empty when there is no such product.
    /// </summary>
    public async Task<IReadOnlyDictionary<Guid, PriceLabel>> GetForProductAsync(Guid productId, CancellationToken ct)
    {
        var product = await _db.Products.AsNoTracking()
            .Where(p => p.Id == productId)
            .Select(p => new { p.VatRate, p.ProductType, p.OriginCountryId, p.MeasurementUnit })
            .SingleOrDefaultAsync(ct);
        if (product is null)
        {
            return new Dictionary<Guid, PriceLabel>();
        }

        var variants = await _db.Variants.AsNoTracking()
            .Where(v => v.ProductId == productId)
            .Select(v => new { v.Id, v.Price, v.FirstOnSaleAt, v.NetQuantity })
            .ToListAsync(ct);
        DateTimeOffset now = _time.GetUtcNow();
        Dictionary<Guid, DateTimeOffset> started = await _db.PriceHistories.AsNoTracking()
            .Where(h => h.StartedAt <= now && _db.Variants.Any(v => v.Id == h.VariantId && v.ProductId == productId))
            .GroupBy(h => h.VariantId)
            .Select(g => new { VariantId = g.Key, Since = g.Max(h => h.StartedAt) })
            .ToDictionaryAsync(r => r.VariantId, r => r.Since, ct);

        Dictionary<Guid, decimal> references = [];
        RunningDiscount? running = null;
        if ((await PriceHistoryReads.RunningDiscountsAsync(_db, [productId], now, ct)).TryGetValue(productId, out RunningDiscount? found))
        {
            running = found;
            references = await PriceHistoryReads.ReferencesAsync(
                _db, found.Rule, DiscountRules.StartsAt(found.Terms), [.. variants.Select(v => new ReferenceInput(v.Id, v.Price, v.FirstOnSaleAt))], ct);
        }

        Dictionary<Guid, PriceLabel> labels = [];
        foreach (var variant in variants)
        {
            DiscountLine? discount = running is null ? null : DiscountRules.LineOf(running.Terms, variant.Price, references[variant.Id]);
            labels[variant.Id] = LabelOf(
                variant.Price, product.VatRate, product.ProductType, product.OriginCountryId, started.TryGetValue(variant.Id, out DateTimeOffset since) ? since : null,
                product.MeasurementUnit, variant.NetQuantity, discount);
        }

        return labels;
    }

    private static PriceLabel LabelOf(
        decimal price, decimal vatRate, ProductType productType, Guid? originCountryId, DateTimeOffset? since, MeasurementUnit? measurementUnit, decimal? netQuantity, DiscountLine? discount)
    {
        bool sellsByMeasure = measurementUnit is not null;
        return new PriceLabel(
            price,
            vatRate,
            PriceLabelRules.ShowsOrigin(productType, originCountryId) ? originCountryId : null,
            since,
            measurementUnit,
            sellsByMeasure ? netQuantity : null,
            PriceLabelRules.UnitPrice(discount?.DiscountedPrice ?? price, measurementUnit, netQuantity),
            discount);
    }
}
