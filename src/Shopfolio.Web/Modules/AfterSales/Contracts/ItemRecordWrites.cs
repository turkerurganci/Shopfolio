using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Ordering.Contracts;

namespace Shopfolio.Modules.AfterSales.Contracts;

/// <summary>
/// The item a record is written on, as the caller - the transition unit, which holds the order's lock (05 7.3.2) - read it from the order: the order and the item,
/// the item's quantity and type, the order's shipment status, the item's delivery date - the delivery mark is written with it (02 5.8) - and its frozen withdrawal
/// exemption (06 4.17). The AfterSales writers read the item's records themselves; what belongs to the order is handed in, because the order is not theirs (09 6 item 2).
/// </summary>
public sealed record ItemRecordSubject(
    Guid OrderId,
    Guid OrderItemId,
    int ItemQuantity,
    ProductType ProductType,
    OrderStatus OrderStatus,
    DateOnly? DeliveryDate,
    WithdrawalExemptionReason? WithdrawalExemptionReason);

/// <summary>An administrator who writes an item record (06 4.22-4.27): the id and the name the record keeps as a value.</summary>
public sealed record ItemRecordAdmin(Guid AdminId, string AdminName);

/// <summary>The customer opens a defect claim (03 1.9.1, 2.9; 02 7.5): the defective units and the description.</summary>
public sealed record DefectClaimOpening(ItemRecordSubject Subject, int Quantity, string Description, DateTimeOffset At);

/// <summary>
/// A change of a defect claim's status (03 1.9.1): <see cref="DefectClaimStatus.Resolved"/> by an administrator, or <see cref="DefectClaimStatus.Open"/> - the reopening -
/// by the customer or an administrator. <paramref name="Quantity"/> is the customer's raised quantity on a reopening; null keeps the claim's quantity.
/// </summary>
public sealed record DefectClaimStatusChange(ItemRecordSubject Subject, DefectClaimStatus Status, ActorKind Actor, int? Quantity, DateTimeOffset At);

/// <summary>
/// A withdrawal declaration (03 1.7.1.5; 02 7.3; K-790): every call is a record of its own. The amounts are the caller's - the withdrawal operation computes them with
/// the one rounding function (02 7.1.7; T372) - and <paramref name="RefundIbanId"/> is the customer's standing IBAN, if any.
/// </summary>
public sealed record WithdrawalDeclaration(
    ItemRecordSubject Subject,
    int Quantity,
    ItemRecordAdmin? Admin,
    DateOnly DeclarationDate,
    bool? IsDeliveredBeforeDeclaration,
    decimal ProductAmount,
    decimal CouponShare,
    decimal VatAmount,
    Guid? RefundIbanId,
    DateTimeOffset At);

/// <summary>The receipt of returned goods of a withdrawal (03 1.7.1.6; K-794): every partial arrival is a record of its own, with the arrival date and the units.</summary>
public sealed record ReturnReceiptEntry(ItemRecordSubject Subject, Guid WithdrawalId, int Quantity, DateOnly ArrivalDate, ItemRecordAdmin Admin, DateTimeOffset At);

/// <summary>The rejection of returned goods at the receipt step (03 1.7.1.7; 02 7.4.9; K-656): only on an item frozen with a conditional exemption.</summary>
public sealed record ReturnRejectionEntry(ItemRecordSubject Subject, Guid WithdrawalId, int Quantity, DateOnly ArrivalDate, ItemRecordAdmin Admin, DateTimeOffset At);

/// <summary>The firm's return address a claim copied (06 4.30; K-817), as the notice writes it.</summary>
public sealed record ItemReturnAddress(string RecipientName, Guid ProvinceId, string District, string AddressLine, string? Phone);

/// <summary>An item's defect claim as it stands (06 4.30): its id, status, defective units and the return address it copied.</summary>
public sealed record DefectClaimInfo(Guid Id, Guid OrderItemId, DefectClaimStatus Status, int Quantity, DateTimeOffset CreatedAt, ItemReturnAddress? ReturnAddress);

/// <summary>The outcome of an AfterSales writer: a refusal, or the written record's id (and the claim as it now stands, on a claim's write).</summary>
public sealed record ItemRecordWriteResult(ItemRecordRefusal? Refusal, Guid? RecordId, DefectClaimInfo? Claim)
{
    public bool IsRejected => Refusal is not null;

    public static ItemRecordWriteResult Rejected(ItemRecordRefusal refusal) => new(refusal, null, null);
}

/// <summary>
/// An item's type and delivery date as the panel marks read them (03 1.7.3.5: a withdrawal after the delivery starts the return - only a physical item's, as the one
/// rule of 02 7.3.5 reads it).
/// </summary>
public sealed record ItemDelivery(Guid OrderItemId, ProductType ProductType, DateOnly? DeliveryDate);

/// <summary>
/// An item's panel marks (03 1.7.3; K-675): not a status and never stored - read from the records while the condition lasts. <paramref name="AwaitingReturnGoods"/> -
/// a withdrawal declared after the delivery has units neither received, nor rejected, nor closed as not come back (1.7.3.5); <paramref name="AwaitingIban"/> - an IBAN
/// request on the item is neither fulfilled nor closed (1.7.3.4).
/// </summary>
public sealed record ItemMarks(bool AwaitingReturnGoods, bool AwaitingIban);
