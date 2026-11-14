using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Modules.Catalog.Contracts;

namespace Shopfolio.Modules.Catalog;

/// <summary>
/// The panel's product form read (T525; 07 UC-88; 04 9.5): the product's fields and <c>Version</c>, its variants with the shown stock value and the reserved units, the publish gate's unmet
/// conditions - asked of the one gate (<see cref="IPublishGate"/>), not computed again here - and the address fixed at the first publish. A fixed number of reads whatever the variant count:
/// the product, its variants, its options and their values, its categories, its discount, its attached files, its gallery, and the gate's own reads (09 20 item 4). The form carries the
/// categories, options, discount and files it does not edit itself, so a save of the form keeps them (UC-114 writes the form whole). A plain read; it writes nothing.
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

    public async Task<PanelProductRecord?> GetAsync(Guid productId, CancellationToken ct)
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
        List<Option> options = await _db.Options.AsNoTracking().Where(o => o.ProductId == productId).OrderBy(o => o.SortOrder).ToListAsync(ct);
        List<OptionValue> values = await _db.OptionValues.AsNoTracking()
            .Where(v => _db.Options.Any(o => o.Id == v.OptionId && o.ProductId == productId))
            .OrderBy(v => v.SortOrder)
            .ToListAsync(ct);
        Dictionary<Guid, string> valueNames = values.ToDictionary(v => v.Id, v => v.Value);
        PanelCategoryRow[] categories =
        [
            .. await _db.ProductCategories.AsNoTracking()
                .Where(l => l.ProductId == productId)
                .OrderByDescending(l => l.IsPrimaryCategory)
                .ThenBy(l => l.CreatedAt)
                .Select(l => new PanelCategoryRow(l.CategoryId, l.IsPrimaryCategory))
                .ToListAsync(ct),
        ];
        PanelDiscountRow? discount = await _db.Discounts.AsNoTracking()
            .Where(d => d.ProductId == productId)
            .Select(d => new PanelDiscountRow(d.Percentage, d.StartDate, d.EndDate))
            .SingleOrDefaultAsync(ct);

        // The attached files of the product and of its variants (06 4.70): the form carries them, so a save keeps them (UC-114).
        Guid[] variantIds = [.. variants.Select(v => v.Id)];
        List<DigitalFile> files = await _db.DigitalFiles.AsNoTracking()
            .Where(f => f.DetachedAt == null && f.AttachedAt != null && (f.ProductId == productId || (f.VariantId != null && variantIds.Contains(f.VariantId.Value))))
            .ToListAsync(ct);
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
                v.ImageId,
                v.FirstOptionValueId,
                v.SecondOptionValueId,
                files.FirstOrDefault(f => f.VariantId == v.Id)?.Id)),
        ];

        return new PanelProductRecord(
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
            gate.UnmetConditions)
        {
            Categories = categories,
            Options = [.. options.Select(o => new PanelOptionRow(o.Id, o.Name, [.. values.Where(v => v.OptionId == o.Id).Select(v => new PanelOptionValueRow(v.Id, v.Value))]))],
            Discount = discount,
            DigitalFileId = files.FirstOrDefault(f => f.ProductId == productId)?.Id,
        };
    }
}
