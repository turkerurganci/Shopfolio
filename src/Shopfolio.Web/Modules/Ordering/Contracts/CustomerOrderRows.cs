namespace Shopfolio.Modules.Ordering.Contracts;

/// <summary>One order of a member's record (T533; 07 UC-95): the number, the day, the total and the two status axes.</summary>
public sealed record CustomerOrderRow(Guid OrderId, string OrderNumber, DateTimeOffset CreatedAt, decimal Total, OrderStatus OrderStatus, PaymentStatus PaymentStatus);

/// <summary>One page of the orders bound to a member account, newest first.</summary>
public sealed record CustomerOrderPage(IReadOnlyList<CustomerOrderRow> Items, int PageNumber, int PageSize, int TotalCount, int PageCount);

/// <summary>
/// One order of the member's own history (T518; 07 UC-74; 04 5.27.4): the number, the day, the total, the first item's name with the count of the other items, and the two status axes. The first
/// item is the first one of the order's lines (the order the order page lists them in); <paramref name="FirstItemName"/> is empty only for an order with no line.
/// </summary>
public sealed record MemberOrderRow(
    Guid OrderId,
    string OrderNumber,
    DateTimeOffset CreatedAt,
    decimal Total,
    OrderStatus OrderStatus,
    PaymentStatus PaymentStatus,
    string? FirstItemName,
    int OtherItemCount);

/// <summary>One page of the member's own order history (T518; 07 UC-74): the orders the customer's list shows, newest first, <c>PageSizes.OrderHistory</c> to a page.</summary>
public sealed record MemberOrderPage(IReadOnlyList<MemberOrderRow> Items, int PageNumber, int PageSize, int TotalCount, int PageCount);
