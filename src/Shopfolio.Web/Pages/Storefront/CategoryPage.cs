using System.ComponentModel.DataAnnotations;

using Shopfolio.Infrastructure.Binding;
using Shopfolio.Infrastructure.Lists;
using Shopfolio.Modules.Catalog.Contracts.Categories;
using Shopfolio.Modules.Catalog.Contracts.Lists;
using Shopfolio.Pages.Lists;

namespace Shopfolio.Pages.Storefront;

/// <summary>
/// The query model of the category page (E-02; 07 5.3.2, 2.6.1-2.6.3): <c>page</c> and the two filter axes - the price range and the stock state - and nothing else; there is no sort and no option-value
/// axis (04 5.2.4). Every field is text so that a wrong value is never a binding error (07 2.6.6); the price range's own rule is the one pattern's (<see cref="ListFilters.PriceRangeResults"/>, 04 7.1.1.1).
/// </summary>
public sealed class CategoryQuery : PagedQuery, IValidatableObject
{
    [SpecAssignedName]
    [Display(Name = "En düşük fiyat")]
    public string? MinPrice { get; init; }

    [SpecAssignedName]
    [Display(Name = "En yüksek fiyat")]
    public string? MaxPrice { get; init; }

    [SpecAssignedName]
    public string? InStock { get; init; }

    /// <summary>The filter the module takes: the amounts read from the text (a value that is no amount is no bound) and the stock state. An inverted range is passed as it is - the module does not apply it (07 2.6.3).</summary>
    public ProductListFilter ToFilter()
    {
        PriceFilter price = ListFilters.ReadPrice(MinPrice, MaxPrice);
        return new ProductListFilter(price.Min, price.Max, ListFilters.ReadInStock(InStock));
    }

    /// <summary>True when the request carries a filter that applies: a price bound that is an amount (and the range is not inverted) or the stock state.</summary>
    public bool HasAppliedFilter
    {
        get
        {
            ProductListFilter filter = ToFilter();
            return filter.InStockOnly || (!filter.IsPriceRangeInverted && (filter.MinPrice is not null || filter.MaxPrice is not null));
        }
    }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext) => ListFilters.PriceRangeResults(MinPrice, MaxPrice, nameof(MinPrice));
}

/// <summary>The category page's filter form (04 5.2.3, 5.2.5): the values the request carried, put back in the fields, and the field message of the price range when the range is wrong (04 5.2.7).</summary>
public sealed record CategoryFilterForm(string ActionPath, string? MinPrice, string? MaxPrice, bool InStockOnly, string? PriceRangeMessage, bool IsApplied)
{
    public const string MinPriceFieldId = "minPrice";

    public const string MaxPriceFieldId = "maxPrice";

    public const string InStockFieldId = "inStock";
}

/// <summary>
/// The category page as the screen draws it (E-02; 04 5.2): the category's name, the path from the root, the filter form, the cards of the page, the page navigation and - for an empty list - the
/// empty-state line. A category without a published product is an empty page, not a 404 (04 5.2.9); a filter that leaves nothing says so and carries the way to remove it.
/// </summary>
public sealed record CategoryPage(
    string Name,
    string Slug,
    IReadOnlyList<CategoryCrumb> Trail,
    CategoryFilterForm Filter,
    IReadOnlyList<ProductCard> Cards,
    Pager Pager,
    EmptyState? Empty)
{
    /// <summary>The line of an empty category (04 5.2.9): the way out is the home page.</summary>
    public const string EmptyCategoryText = "Bu kategoride henüz ürün yok.";

    /// <summary>The line of a filter that leaves no product (04 5.2.9): the way out removes the filter.</summary>
    public const string EmptyFilterText = "Seçtiğiniz süzgece uyan ürün yok.";

    /// <summary>The path from the root to the category: every ancestor and the category itself, in the tree's order.</summary>
    public static IReadOnlyList<CategoryCrumb> TrailOf(CategoryNode category, IReadOnlyList<CategoryNode> tree)
    {
        ArgumentNullException.ThrowIfNull(category);
        ArgumentNullException.ThrowIfNull(tree);

        List<CategoryCrumb> trail = [];
        CategoryNode? current = category;
        int guard = tree.Count + 1;
        while (current is not null && guard-- > 0)
        {
            trail.Insert(0, new CategoryCrumb(current.Name, CategoryPaths.Of(current.Slug)));
            Guid? parentId = current.ParentCategoryId;
            current = parentId is null ? null : tree.FirstOrDefault(n => n.Id == parentId);
        }

        return trail;
    }

    /// <summary>The empty-state line a page without cards shows: none when there are cards, the filter's way out when a filter applied, the home page otherwise.</summary>
    public static EmptyState? EmptyStateOf(int cardCount, bool filterApplied, string categoryPath)
    {
        if (cardCount > 0)
        {
            return null;
        }

        return filterApplied
            ? new EmptyState(EmptyFilterText, "Süzgeci kaldır", categoryPath)
            : new EmptyState(EmptyCategoryText, "Ana sayfaya dön", "/");
    }
}
