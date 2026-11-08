namespace Shopfolio.Modules.Catalog.Contracts.Lists;

/// <summary>
/// A page of a storefront product list (09 10.5 item 2): the products newest first, the page shown (a page past the last shows the last), the fixed page size, the count of
/// all the products in the scope after the filters, and the number of pages (at least one).
/// </summary>
public sealed record ProductListPage(IReadOnlyList<ProductListItem> Items, int PageNumber, int PageSize, int TotalCount, int PageCount);
