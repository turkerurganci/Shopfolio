using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Shopfolio.Infrastructure.Lists;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Catalog.Contracts.Categories;
using Shopfolio.Modules.Catalog.Contracts.Lists;
using Shopfolio.Pages.Lists;
using Shopfolio.Pages.Storefront;

namespace Shopfolio.Pages;

/// <summary>
/// The category page (E-02; T502; 07 UC-02, UC-06; 04 5.2): the published products of the category and of every branch below it in one list, newest first, 24 a page, filtered on the price range and
/// the stock state only. A slug that is no category's is a 404; a category without a published product is an empty page, not a 404 (02 3.28.6). A page past the last shows the last; a wrong price
/// range is a field message, the filter is not applied and the answer is 200 (04 7.1.1.1). An address with any parameter besides <c>page</c> is closed to the search engines (07 2.6.7) - the list pattern
/// sets the mark, the head writes the tag (<see cref="StorefrontHeads"/>).
/// </summary>
public sealed class CategoryModel : PageModel, IListPage
{
    private readonly ICatalogModule _catalog;

    public CategoryModel(ICatalogModule catalog)
    {
        ArgumentNullException.ThrowIfNull(catalog);
        _catalog = catalog;
    }

    public bool IsSearchPage => false;

    public bool ClosedToSearchEngines { get; set; }

    /// <summary>The screen's model; set when the category exists.</summary>
    public CategoryPage? View { get; private set; }

    public async Task<IActionResult> OnGetAsync(string slug, CategoryQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        ProductListPage? list = await _catalog.ListCategoryProductsAsync(slug, query.ToFilter(), query.RequestedPage(), ct);
        if (list is null)
        {
            return NotFound();
        }

        // The name and the path come from the tree; a category that vanished between the two reads is the same 404.
        IReadOnlyList<CategoryNode> tree = await _catalog.GetCategoryTreeAsync(ct);
        CategoryNode? category = tree.FirstOrDefault(n => string.Equals(n.Slug, slug, StringComparison.Ordinal));
        if (category is null)
        {
            return NotFound();
        }

        string path = CategoryPaths.Of(category.Slug);
        IReadOnlyList<ProductCard> cards = ProductCards.Of(list.Items);
        bool applied = query.HasAppliedFilter;
        View = new CategoryPage(
            category.Name,
            category.Slug,
            CategoryPage.TrailOf(category, tree),
            new CategoryFilterForm(path, query.MinPrice, query.MaxPrice, ListFilters.ReadInStock(query.InStock), PriceRangeMessage(), applied),
            cards,
            Pager.Of(path, query, list.PageNumber, list.PageCount),
            CategoryPage.EmptyStateOf(cards.Count, applied, path));
        return Page();
    }

    // The message of the price range (04 7.1.1.1) is the model state's: the lower bound's field carries it.
    private string? PriceRangeMessage() =>
        ModelState.FirstOrDefault(e => e.Key.EndsWith(nameof(CategoryQuery.MinPrice), StringComparison.OrdinalIgnoreCase) && e.Value?.Errors.Count > 0).Value?.Errors[0].ErrorMessage;
}
