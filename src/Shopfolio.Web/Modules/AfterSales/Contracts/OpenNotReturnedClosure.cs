namespace Shopfolio.Modules.AfterSales.Contracts;

/// <summary>
/// A "mal dönmedi" closure that was not reopened (T529; 06 4.28; 07 UC-133 <c>NotReturnedClosureId</c>; 04 7.2.13.2; K-1105): the amount based refund on the item can be bound to it - its id, the
/// item, the closed units and the moment of the closure.
/// </summary>
public sealed record OpenNotReturnedClosure(Guid ClosureId, Guid OrderItemId, int Quantity, DateTimeOffset ClosedAt);
