using System.ComponentModel.DataAnnotations;

using Shopfolio.Infrastructure.Binding;
using Shopfolio.Infrastructure.Lists;
using Shopfolio.Modules.Catalog.Contracts.Lists;
using Shopfolio.Pages.Lists;

namespace Shopfolio.Pages.Storefront;

/// <summary>
/// The query model of the search page (E-03; 07 5.3.2 <c>ProductQuery</c>, 2.6.1-2.6.3): the search word, the two filter axes - the price range and the stock state - and <c>page</c>; there is no sort
/// and no option-value axis (04 2.13.1). Every field is text so that a wrong value is never a binding error (07 2.6.6); the price range's own rule is the one pattern's
/// (<see cref="ListFilters.PriceRangeResults"/>, 04 7.1.1.1).
/// </summary>
public sealed class ProductQuery : PagedQuery, IValidatableObject
{
    [SpecAssignedName]
    [Display(Name = "Arama")]
    public string? Search { get; init; }

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

/// <summary>The filter form of the storefront's product lists (04 5.2.5, 5.3): the values the request carried, put back in the fields, the search word carried in a hidden field, and the field message of the price range.</summary>
public sealed record ProductFilterForm(string ActionPath, string? Search, string? MinPrice, string? MaxPrice, bool InStockOnly, string? PriceRangeMessage, bool IsApplied)
{
    public const string MinPriceFieldId = "minPrice";

    public const string MaxPriceFieldId = "maxPrice";

    public const string InStockFieldId = "inStock";
}

/// <summary>A category link of the empty result's way out (04 5.3; D-1178): the root categories lead back to the list.</summary>
public sealed record SearchCategoryLink(string Name, string Path);

/// <summary>
/// The search page as the screen draws it (E-03; 04 5.3; 07 UC-03): the search field always; below it - only when a word was searched - the filter form, the cards of the page, the page navigation or, for
/// a result that is empty, the empty-state line and the category links. A blank word makes no result list (07 2.6.3).
/// </summary>
public sealed record SearchPage(
    ListSearch Search,
    bool HasResultList,
    ProductFilterForm Filter,
    IReadOnlyList<ProductCard> Cards,
    Pager Pager,
    EmptyState? Empty,
    IReadOnlyList<SearchCategoryLink> Categories)
{
    /// <summary>The label of the search field.</summary>
    public const string SearchLabel = "Ürün ara";

    /// <summary>The line of a search that finds no product (04 5.3).</summary>
    public const string EmptySearchText = "Aramanıza uyan ürün bulunamadı.";

    /// <summary>The line of a search whose filter leaves no product.</summary>
    public const string EmptyFilterText = "Seçtiğiniz süzgece uyan ürün yok.";

    /// <summary>The empty-state line of a result without cards; the way out is the home page - or the removal of the filter when one applied.</summary>
    public static EmptyState? EmptyStateOf(int cardCount, bool filterApplied, string searchAddress)
    {
        if (cardCount > 0)
        {
            return null;
        }

        return filterApplied
            ? new EmptyState(EmptyFilterText, "Süzgeci kaldır", searchAddress)
            : new EmptyState(EmptySearchText, "Ana sayfaya dön", "/");
    }
}
