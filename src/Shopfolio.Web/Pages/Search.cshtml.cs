using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Shopfolio.Infrastructure.Lists;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Catalog.Contracts.Categories;
using Shopfolio.Modules.Catalog.Contracts.Lists;
using Shopfolio.Pages.Frame;
using Shopfolio.Pages.Lists;
using Shopfolio.Pages.Storefront;

namespace Shopfolio.Pages;

/// <summary>
/// The search page (E-03; T503; 07 UC-03; 04 5.3): <c>GET /arama</c> looks for the search word in the product names - capital and small letters and the Turkish letters alike, the word cut at 150
/// characters, <c>%</c> and <c>_</c> as the letters they are (<see cref="SearchProductsAsync"/> through <c>SearchTerm</c>) -, lists the published products with the same filters, page and order as the category
/// page, and is closed to the search engines always (07 2.6.7). A blank word shows only the search field; a result that is empty is the empty-state line with the way back to the categories. An archived
/// product is never in the result. A page past the last shows the last; a wrong price range is a field message and the filter is not applied (04 7.1.1.1).
/// </summary>
public sealed class SearchModel : PageModel, IListPage
{
    private readonly ICatalogModule _catalog;

    public SearchModel(ICatalogModule catalog)
    {
        ArgumentNullException.ThrowIfNull(catalog);
        _catalog = catalog;
    }

    public bool IsSearchPage => true;

    public bool ClosedToSearchEngines { get; set; }

    /// <summary>The screen's model.</summary>
    public SearchPage? View { get; private set; }

    public async Task<IActionResult> OnGetAsync(ProductQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        ListSearch search = new(ListKind.StorefrontProducts, Frame.FramePaths.Search, SearchPage.SearchLabel, query.Search);
        bool applied = query.HasAppliedFilter;
        string? rangeMessage = PriceRangeMessage();
        ProductListPage? list = await _catalog.SearchProductsAsync(query.Search, query.ToFilter(), query.RequestedPage(), ct);
        if (list is null)
        {
            // No word: no result list (07 2.6.3).
            View = new SearchPage(search, false, new ProductFilterForm(Frame.FramePaths.Search, query.Search, query.MinPrice, query.MaxPrice, ListFilters.ReadInStock(query.InStock), rangeMessage, applied), [], Pager.Of(Frame.FramePaths.Search, query, 1, 1), null, []);
            return Page();
        }

        List<ProductCard> cards = [];
        foreach (ProductListItem item in list.Items)
        {
            cards.Add(ProductCards.Of(item, await ImageOfAsync(item, ct)));
        }

        string clearAddress = ListAddress.Of(Frame.FramePaths.Search, new ProductQuery { Search = query.Search }, 1);
        EmptyState? empty = SearchPage.EmptyStateOf(cards.Count, applied, clearAddress);
        IReadOnlyList<SearchCategoryLink> categories = empty is null ? [] : await RootCategoriesAsync(ct);
        View = new SearchPage(
            search,
            true,
            new ProductFilterForm(Frame.FramePaths.Search, query.Search, query.MinPrice, query.MaxPrice, ListFilters.ReadInStock(query.InStock), rangeMessage, applied),
            cards,
            Pager.Of(Frame.FramePaths.Search, query, list.PageNumber, list.PageCount),
            empty,
            categories);
        return Page();
    }

    // The card's main image (D-332): the gallery's main image and its text; none for a product without a gallery.
    private async Task<CardImage?> ImageOfAsync(ProductListItem item, CancellationToken ct)
    {
        IReadOnlyList<GalleryImage> gallery = await _catalog.GetProductGalleryAsync(item.Id, ct);
        GalleryImage? main = gallery.FirstOrDefault(i => i.IsMain) ?? gallery.FirstOrDefault();
        return main is null ? null : ProductCards.ImageOf(main.ImageId, string.IsNullOrWhiteSpace(main.EffectiveAltText) ? item.Name : main.EffectiveAltText);
    }

    private async Task<IReadOnlyList<SearchCategoryLink>> RootCategoriesAsync(CancellationToken ct)
    {
        IReadOnlyList<CategoryNode> tree = await _catalog.GetCategoryTreeAsync(ct);
        return [.. tree.Where(n => n.ParentCategoryId is null)
            .Select(n => new SearchCategoryLink(n.Name, Shopfolio.Infrastructure.Routing.AddressRules.RecordAddress(Shopfolio.Infrastructure.Routing.AddressRules.CategoryPrefix, n.Slug)))];
    }

    // The message of the price range (04 7.1.1.1) is the model state's: the lower bound's field carries it.
    private string? PriceRangeMessage() =>
        ModelState.FirstOrDefault(e => e.Key.EndsWith(nameof(ProductQuery.MinPrice), StringComparison.OrdinalIgnoreCase) && e.Value?.Errors.Count > 0).Value?.Errors[0].ErrorMessage;
}
