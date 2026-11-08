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
        return new ProductPage(kind, product.Id, product.Name, product.Description, image, await ListVariantsAsync(product, kind, isAdministrator, ct));
    }

    private async Task<IReadOnlyList<ProductPageVariant>> ListVariantsAsync(Product product, ProductPageKind kind, bool isAdministrator, CancellationToken ct)
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
        return await variants
            .OrderBy(v => v.CreatedAt).ThenBy(v => v.Id)
            .Select(v => new ProductPageVariant(v.Id, v.SKU, v.Price, v.StockQuantity != null && v.StockQuantity - v.ReservedQuantity <= 0, v.PublishStatus))
            .ToListAsync(ct);
    }
}
