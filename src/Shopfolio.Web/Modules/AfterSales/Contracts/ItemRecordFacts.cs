namespace Shopfolio.Modules.AfterSales.Contracts;

/// <summary>One record on an item's row (04 5.16.5; K-790, K-795): its item, its kind, the units it covers and the moment it was written.</summary>
public sealed record ItemRecordFact(Guid OrderItemId, ItemRecordFactKind Kind, int Quantity, DateTimeOffset At);
