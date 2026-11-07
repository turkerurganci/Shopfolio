namespace Shopfolio.Modules.Catalog.Contracts;

/// <summary>
/// A product card of the storefront's list and search (02 5.1): only a Published product is listed. A product whose every Published variant is sold out
/// stays listed and is marked (02 3.6.4).
/// </summary>
public sealed record ShowcaseProduct(Guid Id, string Name, string Slug, bool IsSoldOut);

/// <summary>
/// A variant of the product page's option list (02 5.1): only a Published variant of a Published product is listed; a sold-out one is listed and marked
/// (02 3.6.3).
/// </summary>
public sealed record ShowcaseVariant(Guid Id, string Sku, decimal Price, bool IsSoldOut);

/// <summary>One page of the storefront's product list or search result.</summary>
public sealed record ShowcasePage(IReadOnlyList<ShowcaseProduct> Items, int PageNumber, int PageSize, int TotalCount, int PageCount);
