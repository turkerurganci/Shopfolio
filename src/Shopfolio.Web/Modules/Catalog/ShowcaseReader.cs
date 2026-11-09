using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Modules.Catalog.Contracts;

namespace Shopfolio.Modules.Catalog;

/// <summary>
/// The one place that says what the visitor sees by publish status (02 5.1; 03 1.8.1): a product is in the lists, the search and the site map only when it is
/// Published, and a variant is in the option list only when it is Published and its product is. A Draft and an Archived record are in none of them - the
/// archived record is kept, only the showcase does not show it. Sold out is not a status: a sold-out Published product and variant stay in (02 3.6.3, 3.6.4).
/// </summary>
internal static class ShowcaseVisibility
{
    /// <summary>The products the lists, the search and the site map show.</summary>
    public static IQueryable<Product> Listed(this IQueryable<Product> products)
    {
        ArgumentNullException.ThrowIfNull(products);
        return products.Where(p => p.PublishStatus == PublishStatus.Published);
    }

    /// <summary>The variants the product page's option list shows: Published, under a Published product.</summary>
    public static IQueryable<Variant> InOptionList(this IQueryable<Variant> variants, IQueryable<Product> products)
    {
        ArgumentNullException.ThrowIfNull(variants);
        ArgumentNullException.ThrowIfNull(products);
        return variants.Where(v => v.PublishStatus == PublishStatus.Published
            && products.Any(p => p.Id == v.ProductId && p.PublishStatus == PublishStatus.Published));
    }
}

/// <summary>
/// The storefront's read of the catalog by publish status (T321; 02 5.1; 09 10.5): the product list and the search in the fixed order, and a product's option
/// list. The search word goes through the one function of 09 10.5 item 5. A plain read; it writes nothing.
/// </summary>
internal sealed class ShowcaseReader
{
    private readonly AppDbContext _db;

    public ShowcaseReader(AppDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// A page of the listed products, newest first (09 10.5 item 1). A <paramref name="search"/> that is not null filters by the name's searched form; a word that makes no
    /// pattern (empty, blanks) makes no list: an empty page (07 2.6.3).
    /// </summary>
    public async Task<ShowcasePage> ListAsync(string? search, int page, CancellationToken ct)
    {
        IQueryable<Product> products = _db.Products.AsNoTracking().Listed();
        if (search is not null)
        {
            string? pattern = SearchTerm.ToLikePattern(search);
            if (pattern is null)
            {
                return new ShowcasePage([], 1, PageSizes.StorefrontProducts, 0, 1);
            }

            products = products.Where(p => EF.Functions.Like(p.NameSearchForm, pattern, SearchTerm.EscapeCharacter));
        }

        PagedList<Product> result = await products
            .OrderByDescending(p => p.CreatedAt).ThenByDescending(p => p.Id)
            .ToPageAsync(page, PageSizes.StorefrontProducts, ct);

        return new ShowcasePage(await CardsOfAsync(result.Items, ct), result.PageNumber, result.PageSize, result.TotalCount, result.PageCount);
    }

    /// <summary>
    /// The cards of the listed products among <paramref name="productIds"/> (T359; 02 3.27.10): a Published product only - a draft and an archived product are left out, and come back when
    /// it is published again - in the showcase's order (newest first); the caller puts them in its own order. A sold-out product is listed and marked. A plain read.
    /// </summary>
    public async Task<IReadOnlyList<ShowcaseProduct>> ListListedByIdsAsync(IReadOnlyCollection<Guid> productIds, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(productIds);
        if (productIds.Count == 0)
        {
            return [];
        }

        List<Product> products = await _db.Products.AsNoTracking().Listed()
            .Where(p => productIds.Contains(p.Id))
            .OrderByDescending(p => p.CreatedAt).ThenByDescending(p => p.Id)
            .ToListAsync(ct);
        return await CardsOfAsync(products, ct);
    }

    /// <summary>The ids among <paramref name="productIds"/> that are products, whatever their status (a link is made to a draft product too); a plain read.</summary>
    public async Task<IReadOnlyList<Guid>> FindProductIdsAsync(IReadOnlyCollection<Guid> productIds, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(productIds);
        if (productIds.Count == 0)
        {
            return [];
        }

        return await _db.Products.AsNoTracking().Where(p => productIds.Contains(p.Id)).Select(p => p.Id).ToListAsync(ct);
    }

    private async Task<IReadOnlyList<ShowcaseProduct>> CardsOfAsync(IReadOnlyList<Product> products, CancellationToken ct)
    {
        // A product is sold out when none of its Published variants can still be bought (02 3.6.3, 3.6.4): the stock left is the stock less the reserved.
        Guid[] ids = [.. products.Select(p => p.Id)];
        List<Guid> available = await _db.Variants.AsNoTracking()
            .Where(v => ids.Contains(v.ProductId)
                && v.PublishStatus == PublishStatus.Published
                && (v.StockQuantity == null || v.StockQuantity - v.ReservedQuantity > 0))
            .Select(v => v.ProductId)
            .Distinct()
            .ToListAsync(ct);
        HashSet<Guid> buyable = [.. available];
        return [.. products.Select(p => new ShowcaseProduct(p.Id, p.Name, p.Slug, !buyable.Contains(p.Id)))];
    }

    /// <summary>The option list of a product: its Published variants when the product is Published, otherwise none.</summary>
    public async Task<IReadOnlyList<ShowcaseVariant>> ListOptionsAsync(Guid productId, CancellationToken ct)
    {
        return await _db.Variants.AsNoTracking()
            .InOptionList(_db.Products.AsNoTracking())
            .Where(v => v.ProductId == productId)
            .OrderBy(v => v.CreatedAt).ThenBy(v => v.Id)
            .Select(v => new ShowcaseVariant(v.Id, v.SKU, v.Price, v.StockQuantity != null && v.StockQuantity - v.ReservedQuantity <= 0))
            .ToListAsync(ct);
    }
}
