namespace Shopfolio.Modules.Catalog.Contracts.Lists;

/// <summary>
/// The two axes a storefront product list is filtered on (02 3.5.2; 07 2.6.3) - the price range and the stock state - and nothing else: there is no option-value axis and no
/// sort (02 3.5.3). The bounds are the amounts already read from the request (<c>minPrice</c>, <c>maxPrice</c>); a lower bound above the upper one voids the whole price
/// filter (07 2.6.3). <see cref="InStockOnly"/> is the <c>inStock=true</c> filter; without it sold-out products are listed too (02 3.5.2; K-52).
/// </summary>
public sealed record ProductListFilter(decimal? MinPrice = null, decimal? MaxPrice = null, bool InStockOnly = false)
{
    /// <summary>The filter that applies nothing.</summary>
    public static ProductListFilter None { get; } = new();

    /// <summary>Whether the price range is void: both bounds given and the lower above the upper.</summary>
    public bool IsPriceRangeInverted => MinPrice is { } low && MaxPrice is { } high && low > high;
}
