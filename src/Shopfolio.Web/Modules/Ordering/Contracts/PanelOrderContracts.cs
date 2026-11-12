using Shopfolio.Modules.Payment.Contracts;

namespace Shopfolio.Modules.Ordering.Contracts;

/// <summary>
/// The order side of the panel's order list filter (T527; 07 UC-91; 04 9.8.3, 9.8.4): the search - the order number or the order's contact e-mail, matched whole; the e-mail without regard to
/// letter case (06 4.17 <c>NormalizedContactEmail</c>) -, the order and payment statuses (none named: no filter on that axis) and, for a pending work filter, the set of orders the work names
/// (<see cref="Within"/>; null: every order). The parts apply together (04 9.8.4).
/// </summary>
public sealed record PanelOrderFilter(
    string? Search,
    IReadOnlyCollection<OrderStatus> OrderStatuses,
    IReadOnlyCollection<PaymentStatus> PaymentStatuses,
    IReadOnlyCollection<Guid>? Within);

/// <summary>An order the filter keeps, with the moment it was placed: the key a list sorted outside the database (the timed pending work filters, 04 9.8.7) cuts its page from.</summary>
public sealed record PanelOrderKey(Guid OrderId, DateTimeOffset CreatedAt);

/// <summary>
/// One row of the panel's order list as the order gives it (T527; 04 9.8.5): the number and the moment, the recipient and the contact e-mail - empty once the order's personal data is disposed -,
/// the first item's name with its option values and the number of the other items, the total, the payment method and the two status axes.
/// </summary>
public sealed record PanelOrderRow(
    Guid OrderId,
    string OrderNumber,
    DateTimeOffset CreatedAt,
    string? RecipientName,
    string? ContactEmail,
    string FirstItemName,
    int OtherItemCount,
    decimal Total,
    PaymentMethod PaymentMethod,
    OrderStatus OrderStatus,
    PaymentStatus PaymentStatus);

/// <summary>
/// One running period of an order the firm owes (T527; 04 9.8.5, 2.6.3): the shipping promise (Z-10) of a preparing order or a service item's fulfilment period (Z-39), with its last day; the caller
/// places the day against its own instant (<c>PeriodStanding.Of</c>). An order may carry several.
/// </summary>
public sealed record OrderRunningPeriod(Guid OrderId, Shopfolio.Infrastructure.CountedPeriod Period, DateOnly LastDay);

/// <summary>One page of the panel's order list: the rows, the page shown (a page past the last is the last, 07 2.6.2), the number of pages and the number of orders of the filter.</summary>
public sealed record PanelOrderPage(IReadOnlyList<PanelOrderRow> Rows, int PageNumber, int PageCount, int TotalCount);
