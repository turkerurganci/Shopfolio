using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Modules.Catalog.Contracts;

namespace Shopfolio.Modules.Catalog;

/// <summary>
/// The plain read of the shipping periods of a set of variants (02 3.20.5; 11 T350 criterion 6): each variant answers with its product's type and the product's own period, so the order
/// can let the product's period override the firm default and take the longest of its items. A variant that does not exist is not in the answer. Nothing is written and no lock is taken.
/// </summary>
internal sealed class ShippingPeriodReader
{
    private readonly AppDbContext _db;

    public ShippingPeriodReader(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<VariantShippingPeriod>> GetAsync(IReadOnlyCollection<Guid> variantIds, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(variantIds);
        if (variantIds.Count == 0)
        {
            return [];
        }

        Guid[] ids = [.. variantIds.Distinct()];
        return await _db.Variants.AsNoTracking()
            .Where(v => ids.Contains(v.Id))
            .Join(
                _db.Products.AsNoTracking(),
                v => v.ProductId,
                p => p.Id,
                (v, p) => new VariantShippingPeriod(v.Id, p.Id, p.ProductType, p.ShippingPeriod))
            .ToListAsync(ct);
    }
}
