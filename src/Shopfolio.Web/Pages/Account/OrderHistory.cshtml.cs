using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Errors;
using Shopfolio.Infrastructure.Lists;
using Shopfolio.Infrastructure.Routing;
using Shopfolio.Infrastructure.Security;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Pages.Components;
using Shopfolio.Pages.Frame;
using Shopfolio.Pages.Lists;

namespace Shopfolio.Pages.Account;

/// <summary>One row of the order history (04 5.27.4): the number and the address of the order's member root, the day, the first item and the count of the others, the total and the two badges.</summary>
public sealed record OrderHistoryRow(string Number, string Path, string Date, string ItemsLine, string Total, OrderBadges Badges);

/// <summary>
/// The member's order history (E-27; T518; 07 UC-74; 04 5.27; 02 3.17.8): inside the account folder, so the customer's session is required. The orders of the account, newest first, 25 to a page
/// (<see cref="PageSizes.OrderHistory"/>; K-784); a page past the last shows the last. The list is the module's <c>ListMemberOrdersAsync</c> - a guest order that fell to the account is in it, a never-paid
/// order the system cancelled by itself is not - and has no search, no filter and no sort (04 5.27.6); the row links to the order's member root, which returns to the first page (04 5.27.7; K-1208), so no
/// return address is carried. A member without an order sees the empty line with the way back to the products (04 5.27.10). A GET only: nothing is written. The response is personal and not stored.
/// </summary>
[NoStore]
internal sealed class OrderHistoryModel : PageModel
{
    private readonly IOrderingModule _ordering;

    public OrderHistoryModel(IOrderingModule ordering)
    {
        ArgumentNullException.ThrowIfNull(ordering);
        _ordering = ordering;
    }

    /// <summary>The rows of the page shown, newest first.</summary>
    public IReadOnlyList<OrderHistoryRow> Rows { get; private set; } = [];

    /// <summary>The page navigation (04 5.27.3); not shown when the list fits one page.</summary>
    public Pager Pager { get; private set; } = Pager.Of(FramePaths.AccountOrders, new OrderHistoryQuery(), 1, 1);

    /// <summary>The empty line of a member without an order; null when there is an order.</summary>
    public EmptyStateView? Empty { get; private set; }

    public async Task<IActionResult> OnGetAsync(OrderHistoryQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);
        if (User.AccountId() is not Guid customerId)
        {
            HttpContext.RequestServices.GetRequiredService<RejectionLog>().Rejected(ErrorCode.NotFound);
            return AccessDenial.NotFound();
        }

        MemberOrderPage page = await _ordering.ListMemberOrdersAsync(customerId, query.RequestedPage(), ct);
        Rows =
        [
            .. page.Items.Select(o => new OrderHistoryRow(
                o.OrderNumber,
                OrderRoutes.MemberPath(o.OrderNumber),
                DisplayFormat.Date(o.CreatedAt),
                OrderHistoryTexts.ItemsLine(o.FirstItemName, o.OtherItemCount),
                DisplayFormat.Amount(o.Total),
                StatusBadges.ForOrder(o.OrderStatus, o.PaymentStatus))),
        ];
        Pager = Pager.Of(FramePaths.AccountOrders, query, page.PageNumber, page.PageCount);
        if (page.TotalCount == 0)
        {
            Empty = EmptyStates.WithExit(OrderHistoryTexts.EmptySentence, OrderHistoryTexts.EmptyExitLabel, "/");
        }

        return Page();
    }
}

/// <summary>The query of UC-74 (07 5.3.2): <c>page</c> alone - the history has no search, no filter and no sort parameter (04 5.27.6).</summary>
public sealed class OrderHistoryQuery : PagedQuery
{
}
