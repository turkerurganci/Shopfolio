using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Shopfolio.Infrastructure.Lists;

/// <summary>
/// A storefront list page the search engines may not index when it carries a filter (07 2.6.7; 05 3.7.4). The page model says whether it is the search page
/// and receives the mark; the list pattern sets the mark and <b>nothing else</b>: the <c>noindex</c> tag itself is written by the search engine data pattern
/// (T219, KK8 - the tag's appearance in the HTML is tested there).
/// </summary>
public interface IListPage
{
    /// <summary>Whether the page is the search page, which is always closed (07 2.6.7: "ve arama sayfası").</summary>
    bool IsSearchPage { get; }

    /// <summary>The mark the list pattern sets before the handler runs: the page is closed to the search engines.</summary>
    bool ClosedToSearchEngines { get; set; }
}

/// <summary>
/// The rule of the mark (07 2.6.7): a storefront list address that carries any parameter other than <c>page</c> is closed, and so is the search page; an
/// address with <c>page</c> alone - the category's pages - is open. The rule reads the parameters the request <i>carries</i>, not the ones the model uses: an
/// invalid or unknown parameter makes another address for the same list, and an address that can be made without limit is not for an index.
/// </summary>
internal static class ListIndexing
{
    public static bool IsClosed(IQueryCollection query, bool isSearchPage)
    {
        ArgumentNullException.ThrowIfNull(query);
        return isSearchPage || query.Keys.Any(key => !string.Equals(key, QueryParameterNames.Of(nameof(PagedQuery.Page)), StringComparison.OrdinalIgnoreCase));
    }
}

/// <summary>Sets the mark of <see cref="IListPage"/> on the page model before its handler runs (07 2.6.7); a page that is not a list page is left alone.</summary>
internal sealed class ListIndexingFilter : IPageFilter
{
    public void OnPageHandlerSelected(PageHandlerSelectedContext context)
    {
    }

    public void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        if (context.HandlerInstance is IListPage page)
        {
            page.ClosedToSearchEngines = ListIndexing.IsClosed(context.HttpContext.Request.Query, page.IsSearchPage);
        }
    }

    public void OnPageHandlerExecuted(PageHandlerExecutedContext context)
    {
    }
}
