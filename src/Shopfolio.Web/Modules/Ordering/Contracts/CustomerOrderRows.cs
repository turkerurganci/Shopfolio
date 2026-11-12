namespace Shopfolio.Modules.Ordering.Contracts;

/// <summary>One order of a member's record (T533; 07 UC-95): the number, the day, the total and the two status axes.</summary>
public sealed record CustomerOrderRow(Guid OrderId, string OrderNumber, DateTimeOffset CreatedAt, decimal Total, OrderStatus OrderStatus, PaymentStatus PaymentStatus);

/// <summary>One page of the orders bound to a member account, newest first.</summary>
public sealed record CustomerOrderPage(IReadOnlyList<CustomerOrderRow> Items, int PageNumber, int PageSize, int TotalCount, int PageCount);
