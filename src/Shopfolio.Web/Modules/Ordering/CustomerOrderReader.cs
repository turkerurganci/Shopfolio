using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Modules.Ordering.Contracts;

namespace Shopfolio.Modules.Ordering;

/// <summary>
/// The plain read behind the member record's order list (T533; 07 UC-95; 04 2.13.3): the orders bound to one member account, newest first, ended with the inner key (09 10.5 item 1),
/// one page at a time. The count and the page are made by the database; nothing is written.
/// </summary>
internal sealed class CustomerOrderReader
{
    private readonly AppDbContext _db;

    public CustomerOrderReader(AppDbContext db)
    {
        _db = db;
    }

    public async Task<CustomerOrderPage> ListAsync(Guid customerId, int page, CancellationToken ct)
    {
        // The page is cut on the entity query ordered ending with the inner key (09 10.5 item 1); the narrow rows are made from the page's records.
        PagedList<Order> paged = await _db.Orders.AsNoTracking()
            .Where(o => o.CustomerId == customerId)
            .OrderByDescending(o => o.CreatedAt)
            .ThenByDescending(o => o.Id)
            .ToPageAsync(page, PageSizes.Panel, ct);

        return new CustomerOrderPage(
            [.. paged.Items.Select(o => new CustomerOrderRow(o.Id, o.OrderNumber, o.CreatedAt, o.Total, o.OrderStatus, o.PaymentStatus))],
            paged.PageNumber,
            paged.PageSize,
            paged.TotalCount,
            paged.PageCount);
    }

    /// <summary>
    /// The member's own history (T518; 07 UC-74; 04 5.27; 02 3.17.8): the orders bound to the account through <see cref="OrderVisibility.ShownToCustomer"/> - a guest order that fell to the account
    /// included, a never-paid order the system cancelled by itself left out -, newest first and ended with the inner key, <see cref="PageSizes.OrderHistory"/> to a page. The first item's name and the
    /// count of the others come from one read for the whole page (09 20 item 4), never one read per order.
    /// </summary>
    public async Task<MemberOrderPage> ListShownAsync(Guid customerId, int page, CancellationToken ct)
    {
        IQueryable<Order> shown = OrderVisibility.ShownToCustomer(_db.Orders.AsNoTracking().Where(o => o.CustomerId == customerId), _db.TransitionRecords);
        PagedList<Order> paged = await shown
            .OrderByDescending(o => o.CreatedAt)
            .ThenByDescending(o => o.Id)
            .ToPageAsync(page, PageSizes.OrderHistory, ct);

        Guid[] ids = [.. paged.Items.Select(o => o.Id)];
        Dictionary<Guid, List<string>> names = [];
        if (ids.Length > 0)
        {
            List<LineName> lines = await _db.OrderItems.AsNoTracking()
                .Where(i => ids.Contains(i.OrderId))
                .OrderBy(i => i.CreatedAt).ThenBy(i => i.Id)
                .Select(i => new LineName(i.OrderId, i.ProductName))
                .ToListAsync(ct);
            foreach (LineName line in lines)
            {
                if (!names.TryGetValue(line.OrderId, out List<string>? list))
                {
                    list = [];
                    names[line.OrderId] = list;
                }

                list.Add(line.ProductName);
            }
        }

        return new MemberOrderPage(
            [.. paged.Items.Select(o =>
            {
                List<string>? own = names.GetValueOrDefault(o.Id);
                return new MemberOrderRow(o.Id, o.OrderNumber, o.CreatedAt, o.Total, o.OrderStatus, o.PaymentStatus, own?[0], own is null ? 0 : own.Count - 1);
            })],
            paged.PageNumber,
            paged.PageSize,
            paged.TotalCount,
            paged.PageCount);
    }

    private sealed record LineName(Guid OrderId, string ProductName);
}
