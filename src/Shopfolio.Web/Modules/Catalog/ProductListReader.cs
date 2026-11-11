using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Catalog.Contracts.Lists;
using Shopfolio.Modules.Catalog.Contracts.Pricing;

namespace Shopfolio.Modules.Catalog;

/// <summary>
/// The storefront's two product lists - a category's page and the search result - read through one query shape (02 3.4.3, 3.5.1-3.5.3; 05 7.9; 07 2.6.3; 09 10.5). Only published
/// products are listed; the order is fixed, newest first, ending with the key (09 10.5 item 1) - there is no sort parameter and no hand order. A category's scope is the category and
/// its whole branch: a product hung on several categories of the scope is one row, and the count, the pages and the filters work on that scope (02 3.4.3). The search is the
/// name's searched form with a contains match through <see cref="SearchTerm"/>: the description, the category name and the option values are not columns of the query (02 3.5.1).
/// The two filters are the price range and the stock state only (02 3.5.2): a product passes when one of its published variants does, and when both filters are on, one and the
/// same variant must satisfy them together - a bought variant is the one in range and in stock. With no filter, sold-out products are listed too. A variant can be bought when it is
/// published and its stock or capacity is empty (unlimited, or not applicable to a digital variant) or exceeds the reserved quantity (02 3.6.3; 06 4.14). The price filter and the
/// card's lowest price read the list price; a running discount is declared on the card beside it with its dates (02 3.9.2; K-1578), and filtering on the discounted price is the
/// storefront's task (T502; D-330). Nothing is written.
/// </summary>
internal sealed class ProductListReader
{
    private readonly AppDbContext _db;
    private readonly TimeProvider _time;

    public ProductListReader(AppDbContext db, TimeProvider time)
    {
        _db = db;
        _time = time;
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

    /// <summary>
    /// The newest published products, at most <paramref name="count"/> of them, as the same cards the lists use (T502; 02 3.28.1; 04 5.1.6): the home page's showcase. No filter, no scope,
    /// the list's order. The count is the layout's; a count below one gives no card.
    /// </summary>
    public async Task<IReadOnlyList<ProductListItem>> ListNewestAsync(int count, CancellationToken ct)
    {
        if (count < 1)
        {
            return [];
        }

        ProductListPage page = await PageOfAsync(Published(), null, PageNumber.First, count, ct);
        return page.Items;
    }

    // The product's visibility is the one rule of ShowcaseVisibility (09 24 item 4), not a condition written here.
    private IQueryable<Product> Published() => _db.Products.AsNoTracking().Listed();

    private Task<ProductListPage> PageOfAsync(IQueryable<Product> products, ProductListFilter? filter, int page, CancellationToken ct) =>
        PageOfAsync(products, filter, page, PageSizes.StorefrontProducts, ct);

    private async Task<ProductListPage> PageOfAsync(IQueryable<Product> products, ProductListFilter? filter, int page, int pageSize, CancellationToken ct)
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

        // Only the columns the card shows (and the order's two) are read - the description and the rest of the product are not (09 20 item 3). The projection is a class with
        // init members, so the order can be put on its members; the page's figures are read in a second step for the page's keys only.
        IOrderedQueryable<CardRow> ordered = products
            .Select(p => new CardRow { Id = p.Id, Name = p.Name, Slug = p.Slug, CreatedAt = p.CreatedAt })
            .OrderByDescending(r => r.CreatedAt)
            .ThenByDescending(r => r.Id);
        PagedList<CardRow> paged = await ordered.ToPageAsync(page, pageSize, ct);

        List<Guid> keys = [.. paged.Items.Select(p => p.Id)];
        Dictionary<Guid, CardFigures> figures = await _db.Variants.AsNoTracking()
            .Where(v => keys.Contains(v.ProductId) && v.PublishStatus == PublishStatus.Published)
            .GroupBy(v => v.ProductId)
            .Select(g => new CardFigures(g.Key, g.Min(v => (decimal?)v.Price), g.Any(v => v.StockQuantity == null || v.StockQuantity - v.ReservedQuantity > 0)))
            .ToDictionaryAsync(f => f.ProductId, ct);

        Dictionary<Guid, DiscountLine> discounts = await DiscountLinesAsync(keys, ct);

        // The main image of each card (D-332): one more read for the page's keys; the firm's text, else the product's name alone (02 3.11.4).
        Dictionary<Guid, MainImageRow> mainImages = await _db.ProductImages.AsNoTracking()
            .Where(i => keys.Contains(i.ProductId) && i.IsMain)
            .Select(i => new MainImageRow(i.ProductId, i.ImageId, i.AltText))
            .ToDictionaryAsync(i => i.ProductId, ct);
        return new ProductListPage(
            [.. paged.Items.Select(p =>
            {
                mainImages.TryGetValue(p.Id, out MainImageRow? image);
                return new ProductListItem(
                    p.Id,
                    p.Name,
                    p.Slug,
                    figures.TryGetValue(p.Id, out CardFigures? f) ? f.LowestPrice : null,
                    f is not null && f.IsPurchasable,
                    discounts.GetValueOrDefault(p.Id),
                    image?.ImageId,
                    image is null ? null : ImageAltText.Choose(image.AltText, () => ImageAltText.ForCard(p.Name)));
            })],
            paged.PageNumber,
            paged.PageSize,
            paged.TotalCount,
            paged.PageCount);
    }

    // The discount declarations of the page's products whose discount runs now (02 3.9.2; 04 2.11.2; K-1578): the card's price is the lowest list price, so the
    // declaration is that lowest-priced published variant's - one rate on every variant keeps it the lowest discounted price too. Three reads whatever the page holds:
    // the running discounts, their products' published variants, and the chosen variants' history before their start for the reference (02 3.9.3).
    private async Task<Dictionary<Guid, DiscountLine>> DiscountLinesAsync(List<Guid> productIds, CancellationToken ct)
    {
        DateTimeOffset now = _time.GetUtcNow();
        Dictionary<Guid, RunningDiscount> running = await PriceHistoryReads.RunningDiscountsAsync(_db, productIds, now, ct);
        if (running.Count == 0)
        {
            return [];
        }

        List<Guid> discounted = [.. running.Keys];
        var lowest = (await _db.Variants.AsNoTracking()
                .Where(v => discounted.Contains(v.ProductId) && v.PublishStatus == PublishStatus.Published)
                .Select(v => new { v.Id, v.ProductId, v.Price, v.FirstOnSaleAt })
                .ToListAsync(ct))
            .GroupBy(v => v.ProductId)
            .Select(g => g.OrderBy(v => v.Price).ThenBy(v => v.Id).First())
            .Select(v => (Discount: running[v.ProductId], Variant: new ReferenceInput(v.Id, v.Price, v.FirstOnSaleAt)))
            .ToList();
        Dictionary<Guid, decimal> references = await PriceHistoryReads.ReferencesAsync(
            _db,
            [.. lowest.Select(l => new ReferenceAsk(l.Variant, l.Discount.Rule, DiscountRules.StartsAt(l.Discount.Terms)))],
            ct);
        return lowest.ToDictionary(
            l => l.Discount.ProductId,
            l => DiscountRules.LineOf(l.Discount.Terms, l.Variant.Price, references[l.Variant.VariantId]));
    }

    private sealed class CardRow
    {
        public required Guid Id { get; init; }

        public required string Name { get; init; }

        public required string Slug { get; init; }

        public required DateTimeOffset CreatedAt { get; init; }
    }

    private sealed record MainImageRow(Guid ProductId, Guid ImageId, string? AltText);

    private sealed record CardFigures(Guid ProductId, decimal? LowestPrice, bool IsPurchasable);
}
