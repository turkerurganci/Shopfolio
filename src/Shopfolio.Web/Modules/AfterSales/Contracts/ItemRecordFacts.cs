namespace Shopfolio.Modules.AfterSales.Contracts;

/// <summary>The kinds of record the customer's order page shows on an item's row (04 5.16.5): the delivery mark is the order's and the IBAN field is the page's, so neither is here.</summary>
public enum ItemRecordFactKind
{
    Cancellation,
    ItemRemoval,
    LateDeliveryTermination,
    Withdrawal,
    ReturnReceipt,
    ReturnRejection,
    NotReturnedClosure,
    DefectClaim,
}

/// <summary>One record on an item's row (04 5.16.5; K-790, K-795): its item, its kind, the units it covers and the moment it was written.</summary>
public sealed record ItemRecordFact(Guid OrderItemId, ItemRecordFactKind Kind, int Quantity, DateTimeOffset At);
