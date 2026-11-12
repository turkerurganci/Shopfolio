using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Modules.Catalog.Contracts;

namespace Shopfolio.Modules.Catalog;

/// <summary>
/// The panel's product form read (T525; 07 UC-88; 04 9.5): the product's fields and <c>Version</c>, its variants with the shown stock value and the reserved units, the publish gate's unmet
/// conditions - asked of the one gate (<see cref="IPublishGate"/>), not computed again here - and the address fixed at the first publish. A fixed number of reads whatever the variant count:
/// the product, its variants, their option values' names, its gallery, and the gate's own reads (09 20 item 4). A plain read; it writes nothing.
/// </summary>
internal sealed class PanelProductFormReader : IPanelProductForm
{
    private readonly AppDbContext _db;
    private readonly IPublishGate _gate;

    public PanelProductFormReader(AppDbContext db, IPublishGate gate)
    {
        _db = db;
        _gate = gate;
    }

    public async Task<int?> GetVersionAsync(Guid productId, Guid? variantId, CancellationToken ct)
    {
        if (variantId is { } variant && !await _db.Variants.AsNoTracking().AnyAsync(v => v.Id == variant && v.ProductId == productId, ct))
        {
            return null;
        }

        return await _db.Products.AsNoTracking().Where(p => p.Id == productId).Select(p => (int?)p.Version).SingleOrDefaultAsync(ct);
    }

    public async Task<PanelProductForm?> GetAsync(Guid productId, CancellationToken ct)
    {
        Product? product = await _db.Products.AsNoTracking().SingleOrDefaultAsync(p => p.Id == productId, ct);
        if (product is null)
        {
            return null;
        }

        List<Variant> variants = await _db.Variants.AsNoTracking()
            .Where(v => v.ProductId == productId)
            .OrderBy(v => v.CreatedAt)
            .ThenBy(v => v.Id)
            .ToListAsync(ct);
        Guid[] valueIds = [.. variants.SelectMany(v => new[] { v.FirstOptionValueId, v.SecondOptionValueId }).OfType<Guid>().Distinct()];
        Dictionary<Guid, string> valueNames = valueIds.Length == 0
            ? []
            : await _db.OptionValues.AsNoTracking().Where(o => valueIds.Contains(o.Id)).ToDictionaryAsync(o => o.Id, o => o.Value, ct);
        PanelImageRow[] images =
        [
            .. await _db.ProductImages.AsNoTracking()
                .Where(i => i.ProductId == productId)
                .OrderBy(i => i.SortOrder)
                .Select(i => new PanelImageRow(i.ImageId, i.SortOrder, i.IsMain, i.AltText))
                .ToListAsync(ct),
        ];
        PublishGateVerdict gate = await _gate.CheckProductAsync(productId, ct);

        PanelVariantRow[] rows =
        [
            .. variants.Select(v => new PanelVariantRow(
                v.Id,
                [.. new[] { v.FirstOptionValueId, v.SecondOptionValueId }.OfType<Guid>().Select(id => valueNames.GetValueOrDefault(id, string.Empty))],
                v.SKU,
                v.Price,
                v.NetQuantity,
                v.StockQuantity,
                v.ReservedQuantity,
                v.PublishStatus,
                v.ImageId)),
        ];

        return new PanelProductForm(
            product.Id,
            product.Name,
            product.Description,
            product.ProductType,
            product.PublishStatus,
            product.VatRate,
            product.OriginCountryId,
            product.MeasurementUnit,
            product.ShippingPeriod,
            product.FulfilmentPeriod,
            product.MaxQuantityPerOrder,
            product.WithdrawalExemptionReason,
            product.Version,
            product.Slug,
            product.SlugFixedAt is not null,
            rows,
            images,
            gate.UnmetConditions);
    }
}
