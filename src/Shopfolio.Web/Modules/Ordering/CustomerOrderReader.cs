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
        PagedList<CustomerOrderRow> paged = await _db.Orders.AsNoTracking()
            .Where(o => o.CustomerId == customerId)
            .Select(o => new CustomerOrderRow(o.Id, o.OrderNumber, o.CreatedAt, o.Total, o.OrderStatus, o.PaymentStatus))
            .OrderByDescending(r => r.CreatedAt)
            .ThenByDescending(r => r.OrderId)
            .ToPageAsync(page, PageSizes.Panel, ct);

        return new CustomerOrderPage([.. paged.Items], paged.PageNumber, paged.PageSize, paged.TotalCount, paged.PageCount);
    }
}
