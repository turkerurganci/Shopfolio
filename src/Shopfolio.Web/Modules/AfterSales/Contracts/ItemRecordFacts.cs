namespace Shopfolio.Modules.AfterSales.Contracts;

/// <summary>
/// One record on an item's row (04 5.16.5; K-790, K-795): its item, its kind, the units it covers and the moment it was written; a withdrawal of a physical item carries the return
/// address written into it (K-665; it does not follow a later change of the firm's address), a defect claim its status (Açık · Çözüldü; 04 2.5.1.5).
/// </summary>
public sealed record ItemRecordFact(
    Guid OrderItemId,
    ItemRecordFactKind Kind,
    int Quantity,
    DateTimeOffset At,
    ItemReturnAddress? ReturnAddress = null,
    DefectClaimStatus? ClaimStatus = null);
