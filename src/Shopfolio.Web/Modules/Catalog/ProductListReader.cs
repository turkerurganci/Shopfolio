using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Catalog.Contracts.Lists;

namespace Shopfolio.Modules.Catalog;

/// <summary>
/// The storefront's two product lists - a category's page and the search result - read through one query shape (02 3.4.3, 3.5.1-3.5.3; 05 7.9; 07 2.6.3; 09 10.5). Only published
/// products are listed; the order is fixed, newest first, ending with the key (09 10.5 item 1) - there is no sort parameter and no hand order. A category's scope is the category and
/// its whole branch: a product hung on several categories of the scope is one row, and the count, the pages and the filters work on that scope (02 3.4.3). The search is the
/// name's searched form with a contains match through <see cref="SearchTerm"/>: the description, the category name and the option values are not columns of the query (02 3.5.1).
/// The two filters are the price range and the stock state only (02 3.5.2): a product passes when one of its published variants does, and when both filters are on, one and the
/// same variant must satisfy them together - a bought variant is the one in range and in stock. With no filter, sold-out products are listed too. A variant can be bought when it is
/// published and its stock or capacity is empty (unlimited, or not applicable to a digital variant) or exceeds the reserved quantity (02 3.6.3; 06 4.14). Nothing is written.
/// </summary>
internal sealed class ProductListReader
{
    private readonly AppDbContext _db;

    public ProductListReader(AppDbContext db)
    {
        _db = db;
    }

    /// <summary>A category's page by its address; null when no category has the address (the page answers 404). An empty category is an empty page, not a 404 (02 3.28.6).</summary>
    public async Task<ProductListPage?> ListCategoryAsync(string? categorySlug, ProductListFilter? filter, int page, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(categorySlug))
        {
            return null;
        }

        Guid? categoryId = await _db.Categories.AsNoTracking().Where(c => c.Slug == categorySlug).Select(c => (Guid?)c.Id).SingleOrDefaultAsync(ct);
        if (categoryId is not Guid root)
        {
            return null;
        }

        List<CategoryPlace> tree = await _db.Categories.AsNoTracking().Select(c => new CategoryPlace(c.Id, c.ParentCategoryId, c.Level)).ToListAsync(ct);
        List<Guid> scope = [.. CategoryLevels.SubtreeOf(root, tree)];

        IQueryable<Product> products = Published().Where(p => _db.ProductCategories.Any(l => l.ProductId == p.Id && scope.Contains(l.CategoryId)));
        return await PageOfAsync(products, filter, page, ct);
    }

    /// <summary>The search result for a word; null when the word is empty or only blanks - the page then shows the search field and no list (07 2.6.3).</summary>
    public async Task<ProductListPage?> SearchAsync(string? search, ProductListFilter? filter, int page, CancellationToken ct)
    {
        string? pattern = SearchTerm.ToLikePattern(search);
        if (pattern is null)
        {
            return null;
        }

        IQueryable<Product> products = Published().Where(p => EF.Functions.Like(p.NameSearchForm, pattern, SearchTerm.EscapeCharacter));
        return await PageOfAsync(products, filter, page, ct);
    }

    private IQueryable<Product> Published() => _db.Products.AsNoTracking().Where(p => p.PublishStatus == PublishStatus.Published);

    private async Task<ProductListPage> PageOfAsync(IQueryable<Product> products, ProductListFilter? filter, int page, CancellationToken ct)
    {
        ProductListFilter applied = filter ?? ProductListFilter.None;
        decimal? min = applied.IsPriceRangeInverted ? null : applied.MinPrice;
        decimal? max = applied.IsPriceRangeInverted ? null : applied.MaxPrice;
        bool inStockOnly = applied.InStockOnly;

        if (min is not null || max is not null || inStockOnly)
        {
            products = products.Where(p => _db.Variants.Any(v =>
                v.ProductId == p.Id
                && v.PublishStatus == PublishStatus.Published
                && (!inStockOnly || v.StockQuantity == null || v.StockQuantity - v.ReservedQuantity > 0)
                && (min == null || v.Price >= min)
                && (max == null || v.Price <= max)));
        }

        // The order is on the product itself (a projection to a record cannot be ordered by its member); the page's cards are read in a second step for the page's keys only.
        IOrderedQueryable<Product> ordered = products.OrderByDescending(p => p.CreatedAt).ThenByDescending(p => p.Id);
        PagedList<Product> paged = await ordered.ToPageAsync(page, PageSizes.StorefrontProducts, ct);

        List<Guid> keys = [.. paged.Items.Select(p => p.Id)];
        Dictionary<Guid, CardFigures> figures = await _db.Variants.AsNoTracking()
            .Where(v => keys.Contains(v.ProductId) && v.PublishStatus == PublishStatus.Published)
            .GroupBy(v => v.ProductId)
            .Select(g => new CardFigures(g.Key, g.Min(v => (decimal?)v.Price), g.Any(v => v.StockQuantity == null || v.StockQuantity - v.ReservedQuantity > 0)))
            .ToDictionaryAsync(f => f.ProductId, ct);

        return new ProductListPage(
            [.. paged.Items.Select(p => new ProductListItem(p.Id, p.Name, p.Slug, figures.TryGetValue(p.Id, out CardFigures? f) ? f.LowestPrice : null, f is not null && f.IsPurchasable))],
            paged.PageNumber,
            paged.PageSize,
            paged.TotalCount,
            paged.PageCount);
    }

    private sealed record CardFigures(Guid ProductId, decimal? LowestPrice, bool IsPurchasable);
}
