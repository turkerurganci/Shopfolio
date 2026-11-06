namespace Shopfolio.Infrastructure;

/// <summary>
/// One page of a list (09 10.5 item 2): the items of the page, the page shown (after the clamp to the last page), the page size, the number of all
/// records and the number of pages. A list that fits one page has <see cref="PageCount"/> 1 and shows no navigation (04 2.13.3).
/// </summary>
internal sealed record PagedList<T>(IReadOnlyList<T> Items, int PageNumber, int PageSize, int TotalCount, int PageCount)
{
    /// <summary>Whether the navigation (previous, next, page numbers) is shown: not for a list of one page (04 2.13.3).</summary>
    public bool ShowsNavigation => PageCount > 1;
}
