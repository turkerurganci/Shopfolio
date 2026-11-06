using Shopfolio.Infrastructure.Binding;

namespace Shopfolio.Infrastructure.Lists;

/// <summary>
/// The base of the query model of a paged list (07 2.6, 5.3.1; 09 10.5 item 3). A list is a GET; its parameters are the camelCase names of the model's
/// fields (<see cref="QueryParameterNames"/>), a model's fields are text so that a wrong value is never a binding error (07 2.6.6), and a name the model does
/// not have is ignored. <c>page</c> starts at one, the first page is written without it, and the page size is no parameter (07 2.6.2). There is no sort
/// parameter: the order is the server's (07 2.6.5).
/// </summary>
public abstract class PagedQuery : IQueryModel
{
    /// <summary>The page asked for, as text: <see cref="PageNumber.Parse"/> reads it (09 10.5 item 3). An <c>int</c> field could not carry a value past the range, which is the last page.</summary>
    [SpecAssignedName]
    public string? Page { get; init; }

    /// <summary>The page asked for as a number; the paging helper clamps it to the last page.</summary>
    public int RequestedPage() => PageNumber.Parse(Page);
}
