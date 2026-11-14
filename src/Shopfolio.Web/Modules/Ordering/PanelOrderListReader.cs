using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Routing;
using Shopfolio.Modules.Ordering.Contracts;

namespace Shopfolio.Modules.Ordering;

/// <summary>
/// The plain reads behind the panel's order list (T527; 07 UC-91; 04 9.8): the filter of the search and the two status axes over every order - an unpaid order the system cancelled by itself
/// included (04 9.8.12; 03 3.2.2.4) -, newest first and ended with the inner key (09 10.5 item 1). The count and the page are made by the database; the items of a page are one read, so the
/// number of reads does not grow with the page size (09 20 item 4). Nothing is written.
/// </summary>
internal sealed class PanelOrderListReader
{
    private readonly AppDbContext _db;

    public PanelOrderListReader(AppDbContext db)
    {
        _db = db;
    }

    public async Task<PanelOrderPage> ListAsync(PanelOrderFilter filter, int page, int pageSize, CancellationToken ct)
    {
        PagedList<Order> paged = await Filtered(filter)
            .OrderByDescending(o => o.CreatedAt)
            .ThenByDescending(o => o.Id)
            .ToPageAsync(page, pageSize, ct);
        IReadOnlyList<PanelOrderRow> rows = await ReadRowsAsync([.. paged.Items.Select(o => o.Id)], ct);
        return new PanelOrderPage(rows, paged.PageNumber, paged.PageCount, paged.TotalCount);
    }

    public async Task<IReadOnlyList<PanelOrderKey>> FindKeysAsync(PanelOrderFilter filter, CancellationToken ct)
    {
        if (filter.Within is null)
        {
            throw new ArgumentException("The keys are read only inside a pending work set; the whole list is paged by the database.", nameof(filter));
        }

        return await Filtered(filter)
            .OrderByDescending(o => o.CreatedAt)
            .ThenByDescending(o => o.Id)
            .Select(o => new PanelOrderKey(o.Id, o.CreatedAt))
            .ToListAsync(ct);
    }

    /// <summary>The rows of the given orders in the order of <paramref name="orderIds"/>; an id with no order is not in the answer. Two reads whatever the number of ids.</summary>
    public async Task<IReadOnlyList<PanelOrderRow>> ReadRowsAsync(IReadOnlyList<Guid> orderIds, CancellationToken ct)
    {
        if (orderIds.Count == 0)
        {
            return [];
        }

        Guid[] ids = [.. orderIds.Distinct()];
        var orders = await _db.Orders.AsNoTracking()
            .Where(o => ids.Contains(o.Id))
            .Select(o => new
            {
                o.Id,
                o.OrderNumber,
                o.CreatedAt,
                o.ShippingRecipientName,
                o.BillingRecipientName,
                o.ContactEmail,
                o.Total,
                o.PaymentMethod,
                o.OrderStatus,
                o.PaymentStatus,
            })
            .ToDictionaryAsync(o => o.Id, ct);
        var items = await _db.OrderItems.AsNoTracking()
            .Where(i => ids.Contains(i.OrderId))
            .OrderBy(i => i.CreatedAt)
            .ThenBy(i => i.Id)
            .Select(i => new { i.OrderId, i.ProductName, i.FirstOptionValue, i.SecondOptionValue })
            .ToListAsync(ct);
        ILookup<Guid, string> names = items.ToLookup(i => i.OrderId, i => ItemNameOf(i.ProductName, i.FirstOptionValue, i.SecondOptionValue));

        List<PanelOrderRow> rows = [];
        foreach (Guid id in orderIds)
        {
            if (!orders.TryGetValue(id, out var o))
            {
                continue;
            }

            string[] itemNames = [.. names[id]];
            rows.Add(new PanelOrderRow(
                o.Id,
                o.OrderNumber,
                o.CreatedAt,
                o.ShippingRecipientName ?? o.BillingRecipientName,
                o.ContactEmail,
                itemNames.Length == 0 ? string.Empty : itemNames[0],
                Math.Max(0, itemNames.Length - 1),
                o.Total,
                o.PaymentMethod,
                o.OrderStatus,
                o.PaymentStatus));
        }

        return rows;
    }

    /// <summary>The item's name as the panel writes it: the product name the order recorded, then the option values the item was bought with in parentheses.</summary>
    internal static string ItemNameOf(string productName, string? firstOption, string? secondOption)
    {
        string[] options = [.. new[] { firstOption, secondOption }.OfType<string>().Where(v => !string.IsNullOrWhiteSpace(v))];
        return options.Length == 0 ? productName : $"{productName} ({string.Join(", ", options)})";
    }

    private IQueryable<Order> Filtered(PanelOrderFilter filter)
    {
        ArgumentNullException.ThrowIfNull(filter);
        IQueryable<Order> query = _db.Orders.AsNoTracking();

        // The search is matched whole: the number without regard to letter case (07 2.1.7), the e-mail in its normalized form (06 4.17); a value that is neither finds nothing (04 9.8.11).
        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            string trimmed = filter.Search.Trim();
            string number = OrderRoutes.NumberForLookup(trimmed);
            string email = TextNormalizer.NormalizeIdentity(trimmed);
            query = query.Where(o => o.OrderNumber == number || o.NormalizedContactEmail == email);
        }

        if (filter.OrderStatuses.Count > 0)
        {
            OrderStatus[] statuses = [.. filter.OrderStatuses.Distinct()];
            query = query.Where(o => statuses.Contains(o.OrderStatus));
        }

        if (filter.PaymentStatuses.Count > 0)
        {
            PaymentStatus[] statuses = [.. filter.PaymentStatuses.Distinct()];
            query = query.Where(o => statuses.Contains(o.PaymentStatus));
        }

        if (filter.Within is { } within)
        {
            Guid[] ids = [.. within.Distinct()];
            query = query.Where(o => ids.Contains(o.Id));
        }

        return query;
    }
}
