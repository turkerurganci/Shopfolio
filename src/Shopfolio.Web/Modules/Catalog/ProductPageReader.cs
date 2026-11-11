using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Modules.Catalog.Contracts;

namespace Shopfolio.Modules.Catalog;

/// <summary>
/// The storefront's read of one product address (T335; 02 3.7.5, 3.7.6, 3.30.4; 09 10.5): what the address answers by the publish status and by who asks. A Published product is
/// its page; an Archived product is the "not sold any more" page without variants - so without price and cart; a Draft product is "page not found" for a visitor and the page with
/// its band for a signed-in administrator, who also sees the Draft variants; an address that holds no product (never given, deleted) is "page not found". The variant list of a
/// Published product for a visitor is <see cref="ShowcaseVisibility.InOptionList"/>, the same rule as the option list. A plain read; it writes nothing.
/// </summary>
internal sealed class ProductPageReader
{
    private readonly AppDbContext _db;

    public ProductPageReader(AppDbContext db)
    {
        _db = db;
    }

    public async Task<ProductPage> ReadAsync(string? slug, bool isAdministrator, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(slug))
        {
            return ProductPage.NotFound;
        }

        Product? product = await _db.Products.AsNoTracking().SingleOrDefaultAsync(p => p.Slug == slug, ct);
        if (product is null || (product.PublishStatus == PublishStatus.Draft && !isAdministrator))
        {
            return ProductPage.NotFound;
        }

        ProductPageKind kind = product.PublishStatus switch
        {
            PublishStatus.Published => ProductPageKind.Published,
            PublishStatus.Archived => ProductPageKind.Archived,
            _ => ProductPageKind.DraftPreview,
        };
        Guid? image = await _db.ProductImages.AsNoTracking()
            .Where(i => i.ProductId == product.Id)
            .OrderByDescending(i => i.IsMain).ThenBy(i => i.SortOrder).ThenBy(i => i.Id)
            .Select(i => (Guid?)i.ImageId)
            .FirstOrDefaultAsync(ct);
        Guid? primaryCategory = await _db.ProductCategories.AsNoTracking()
            .Where(l => l.ProductId == product.Id && l.IsPrimaryCategory)
            .Select(l => (Guid?)l.CategoryId)
            .FirstOrDefaultAsync(ct);
        (IReadOnlyList<ProductPageOption> options, Dictionary<Guid, (int Order, int Slot)> valueSlots) = kind == ProductPageKind.Archived
            ? ([], [])
            : await ReadOptionsAsync(product.Id, ct);
        IReadOnlyList<ProductPageVariant> variants = await ListVariantsAsync(product, kind, isAdministrator, valueSlots, ct);
        return new ProductPage(kind, product.Id, product.Name, product.Description, image, variants)
        {
            Type = product.ProductType,
            Slug = product.Slug,
            ShippingPeriod = product.ShippingPeriod,
            FulfilmentPeriod = product.FulfilmentPeriod,
            PrimaryCategoryId = primaryCategory,
            Options = options,
        };
    }

    // The option dimensions in the firm's order (Option.SortOrder 1 is the first, 2 the second) with their values in their own order; and the place of every value by its id, which the variants'
    // two bindings are turned into (07 4.4.1: the selection parameters are the values' orders, never their keys).
    private async Task<(IReadOnlyList<ProductPageOption> Options, Dictionary<Guid, (int Order, int Slot)> Slots)> ReadOptionsAsync(Guid productId, CancellationToken ct)
    {
        var options = await _db.Options.AsNoTracking()
            .Where(o => o.ProductId == productId)
            .OrderBy(o => o.SortOrder)
            .Select(o => new { o.Id, o.Name })
            .ToListAsync(ct);
        List<ProductPageOption> result = [];
        Dictionary<Guid, (int Order, int Slot)> slots = [];
        foreach (var option in options)
        {
            var values = await _db.OptionValues.AsNoTracking()
                .Where(v => v.OptionId == option.Id)
                .OrderBy(v => v.SortOrder).ThenBy(v => v.Id)
                .Select(v => new { v.Id, v.Value, v.SortOrder })
                .ToListAsync(ct);
            int slot = result.Count;
            foreach (var value in values)
            {
                slots[value.Id] = (value.SortOrder, slot);
            }

            result.Add(new ProductPageOption(option.Name, [.. values.Select(v => new ProductPageOptionValue(v.SortOrder, v.Value))]));
        }

        return (result, slots);
    }

    private async Task<IReadOnlyList<ProductPageVariant>> ListVariantsAsync(
        Product product, ProductPageKind kind, bool isAdministrator, Dictionary<Guid, (int Order, int Slot)> valueSlots, CancellationToken ct)
    {
        // The archived page is not a sales page: it carries no variant, so no price and nothing to add to the cart (02 3.7.6).
        if (kind == ProductPageKind.Archived)
        {
            return [];
        }

        IQueryable<Variant> variants = _db.Variants.AsNoTracking().Where(v => v.ProductId == product.Id);

        // A visitor sees the Published variants of a Published product; the administrator's preview adds the Draft ones, never the archived (02 3.7.5).
        variants = isAdministrator
            ? variants.Where(v => v.PublishStatus != PublishStatus.Archived)
            : variants.InOptionList(_db.Products.AsNoTracking());
        var rows = await variants
            .OrderBy(v => v.CreatedAt).ThenBy(v => v.Id)
            .Select(v => new
            {
                v.Id,
                v.SKU,
                v.Price,
                SoldOut = v.StockQuantity != null && v.StockQuantity - v.ReservedQuantity <= 0,
                v.PublishStatus,
                v.FirstOptionValueId,
                v.SecondOptionValueId,
            })
            .ToListAsync(ct);
        return
        [
            .. rows.Select(v => new ProductPageVariant(v.Id, v.SKU, v.Price, v.SoldOut, v.PublishStatus)
            {
                FirstValueOrder = OrderOf(valueSlots, v.FirstOptionValueId),
                SecondValueOrder = OrderOf(valueSlots, v.SecondOptionValueId),
            }),
        ];
    }

    private static int? OrderOf(Dictionary<Guid, (int Order, int Slot)> valueSlots, Guid? valueId) =>
        valueId is Guid id && valueSlots.TryGetValue(id, out (int Order, int Slot) place) ? place.Order : null;
}
