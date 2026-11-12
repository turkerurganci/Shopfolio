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
/// A withdrawal declaration (03 1.7.1.5; 02 7.3; K-790): every call is a record of its own. <paramref name="Frozen"/> are the item's frozen values the declaration's
/// amounts are split from (02 7.1.7; <see cref="ItemFrozenAmounts"/>); the writer splits them against the shares the item's earlier records already took.
/// <paramref name="Iban"/> says where the refund IBAN comes from on the bank transfer line (02 7.3.5, 10.4.10): entered with the declaration, the one on the order's
/// file, or none - then the item gets an IBAN request (B-14 is the caller's notice).
/// </summary>
public sealed record WithdrawalDeclaration(
    ItemRecordSubject Subject,
    int Quantity,
    ItemRecordAdmin? Admin,
    DateOnly DeclarationDate,
    bool? IsDeliveredBeforeDeclaration,
    ItemFrozenAmounts Frozen,
    WithdrawalIban Iban,
    DateTimeOffset At);

/// <summary>
/// The item's frozen values an item record sets its amounts aside from (02 7.1.7; 06 4.18): the unit price the customer paid - the discounted one, rounded to the
/// kuruş, while a discount ran (02 3.9.1) - and the item's coupon share and VAT. The order module hands them in; the item is not this module's (09 6 item 2).
/// </summary>
public sealed record ItemFrozenAmounts(decimal UnitPrice, decimal CouponShare, decimal VatAmount);

/// <summary>
/// The refund IBAN of a withdrawal on the bank transfer line (02 7.3.5, 7.4.5, 10.4.10; K-607): <paramref name="Entered"/> - entered with the declaration, or carried
/// over from a notice that came from the order's contact e-mail (already compact and checked by the caller); <paramref name="UseOnFile"/> - the IBAN already on the
/// order's file is used (none on file refuses the declaration); <paramref name="Request"/> - the record is made without an IBAN and an IBAN request opens on the item
/// (the administrator's record of a notice that brought none). <see cref="None"/> is the card line's.
/// </summary>
public sealed record WithdrawalIban(string? Entered, bool UseOnFile, bool Request)
{
    public static WithdrawalIban None { get; } = new(null, false, false);
}

/// <summary>
/// The customer's cancellation of a paid item's units from the order page (03 1.11.2, 1.11.3; 02 7.2.1, 7.2.2; T367): every call is a record of its own. The amounts
/// are worked out by the writer from <paramref name="Frozen"/> and what the item's earlier records set aside (02 7.1.7): the product amount is the unit price times the
/// units, the coupon share and the VAT are the units' share rounded down, and the record that closes the item's last open units takes what is left.
/// </summary>
public sealed record CustomerCancellationEntry(ItemRecordSubject Subject, int Quantity, ItemFrozenAmounts Frozen, DateTimeOffset At, string? Iban = null);

/// <summary>
/// The firm's cancellation of a paid item's units from the panel (03 1.11.21, 8.3.1.1; 02 7.2.4; T368): every call is a record of its own, with the administrator and a
/// reason from the closed list (06 5.3.6) - the note only on "Other". The amounts are worked out as on the customer's record (<see cref="CustomerCancellationEntry"/>).
/// </summary>
public sealed record FirmCancellationEntry(
    ItemRecordSubject Subject,
    int Quantity,
    ItemFrozenAmounts Frozen,
    ItemRecordAdmin Admin,
    CancellationReason Reason,
    string? ReasonNote,
    DateTimeOffset At);

/// <summary>
/// The late delivery termination of one undelivered physical item (06 4.24; 02 7.2.3; T368): the customer's from the order page (<paramref name="Admin"/> null) or the
/// firm's record of a notice that came by another channel (02 10.4.10). <paramref name="Quantity"/> is the item's whole open quantity - units are never chosen (02 7.1.6;
/// K-796) and the writer refuses any other number. <paramref name="DeclarationDate"/> is the termination's date stamp: the day of the button, or the day the notice reached
/// the firm; the refund's fourteen days run from it. <paramref name="Iban"/> is the IBAN the customer gave with the termination on the transfer line (T512; 02 7.2.3,
/// 7.4.5; D-873), already checked by the order module; null on the card line and on the firm's record.
/// </summary>
public sealed record LateDeliveryTerminationEntry(
    ItemRecordSubject Subject,
    int Quantity,
    ItemFrozenAmounts Frozen,
    ItemRecordAdmin? Admin,
    DateOnly DeclarationDate,
    DateTimeOffset At,
    string? Iban = null);

/// <summary>
/// The firm's removal of a paid item's units from the panel, or the reduction of its quantity (06 4.23; 02 10.4.3; T386): a correction with the administrator and no reason.
/// The amounts are worked out as on the cancellation (<see cref="CustomerCancellationEntry"/>; 02 7.1.7): the removed units are cancelled units on the shipping line.
/// </summary>
public sealed record ItemRemovalEntry(ItemRecordSubject Subject, int Quantity, ItemFrozenAmounts Frozen, ItemRecordAdmin Admin, DateTimeOffset At);

/// <summary>
/// A cancellation, a removal or a late delivery termination with its moment (02 7.2.3; 09 10.10 item 5): the record, its item, whether it is a late delivery
/// termination (<paramref name="IsTermination"/>), whether it is the firm's removal (<paramref name="IsRemoval"/>; 02 10.4.3 - a correction, not a cancellation, so the legal
/// interest warning does not read it), its moment and the quantity it closed.
/// </summary>
public sealed record ClosingRecordMoment(Guid RecordId, Guid OrderItemId, bool IsTermination, DateTimeOffset At, int Quantity, bool IsRemoval = false);

/// <summary>
/// The arrival records of returned goods on the given items as the delivery date's correction reads them (02 10.4.5; K-1474): the receipt or the rejection, its item and
/// its arrival date.
/// </summary>
public sealed record ArrivalRecordDate(Guid RecordId, Guid OrderItemId, DateOnly ArrivalDate);

/// <summary>A withdrawal declaration's day on one of the given items (02 7.4.1; K-608): the corrected delivery date that falls on it asks the panel which came first.</summary>
public sealed record WithdrawalDeclarationDay(Guid WithdrawalId, Guid OrderItemId, DateOnly DeclarationDate);

/// <summary>An order item, its own quantity and the moment its open quantity is asked at (T383; 02 10.6.3: the end of the order's shipping promise).</summary>
public sealed record ItemQuantityAt(Guid OrderItemId, int Quantity, DateTimeOffset At);

/// <summary>
/// The receipt of returned goods of a withdrawal (03 1.7.1.6; K-794): every partial arrival is a record of its own, with the arrival date and the units.
/// <paramref name="IsTransferLine"/> - the order's money came by bank transfer: a receipt whose declaration's IBAN was deleted asks the customer for one again (02 7.4.5; K-497).
/// </summary>
public sealed record ReturnReceiptEntry(
    ItemRecordSubject Subject, Guid WithdrawalId, int Quantity, DateOnly ArrivalDate, ItemRecordAdmin Admin, DateTimeOffset At, bool IsTransferLine = false);

/// <summary>The "mal dönmedi" closure of a withdrawal's units not come back (03 1.7.1.8; 02 10.4.9; K-496, K-794): only after the sending period (Z-42), at most the units awaited.</summary>
public sealed record NotReturnedClosureEntry(ItemRecordSubject Subject, Guid WithdrawalId, int Quantity, ItemRecordAdmin Admin, DateTimeOffset At);

/// <summary>The "Stoğa ekle" step of a receipt (02 7.4.7, 3.6.6; 06 4.26 <c>StockAddedAt</c>; K-215): the receipt's units go back to the stock once, after the firm's check.</summary>
public sealed record ReturnStockEntry(ItemRecordSubject Subject, Guid ReturnId, DateTimeOffset At);

/// <summary>The outcome of the "Stoğa ekle" step: a refusal, or the receipt's units the caller returns to the item's variant.</summary>
public sealed record ReturnStockResult(ItemRecordRefusal? Refusal, int Quantity)
{
    public bool IsRejected => Refusal is not null;
}

/// <summary>The rejection of returned goods at the receipt step (03 1.7.1.7; 02 7.4.9; K-656): only on an item frozen with a conditional exemption.</summary>
public sealed record ReturnRejectionEntry(ItemRecordSubject Subject, Guid WithdrawalId, int Quantity, DateOnly ArrivalDate, ItemRecordAdmin Admin, DateTimeOffset At);

/// <summary>
/// What a withdrawal declaration's screen tells the customer (02 7.3.5): the firm's current return address and the days to send the goods back - a physical item's only
/// (K-530); whether the carrier brings back goods not taken in - a shipped physical item not yet marked delivered (K-838); and the conditional exemption whose protective
/// packaging condition is shown (K-509), null on any other item. <paramref name="RefundPeriodDays"/> is Z-16 and <paramref name="RefundFromGoodsArrival"/> says where it starts
/// for a declaration sent now (02 7.4.1; 04 5.18.6): from the goods' arrival at the firm after the delivery, from the declaration before it and on a service item.
/// </summary>
public sealed record WithdrawalNotes(
    ItemReturnAddress? ReturnAddress,
    int? SendingPeriodDays,
    DateOnly? SendingLastDay,
    bool CarrierReturnsUnreceivedGoods,
    WithdrawalExemptionReason? PackagingCondition,
    int RefundPeriodDays,
    bool RefundFromGoodsArrival);

/// <summary>One item of the declaration screen as <see cref="IAfterSalesModule.WithdrawalNotesAsync"/> reads it: its type, whether it is shipped and not marked delivered, its frozen exemption and its delivery date.</summary>
public sealed record WithdrawalNoteQuery(ProductType ProductType, bool ShippedNotDelivered, WithdrawalExemptionReason? Exemption, DateOnly? DeliveryDate);

/// <summary>The firm's return address a claim copied (06 4.30; K-817), as the notice writes it.</summary>
public sealed record ItemReturnAddress(string RecipientName, Guid ProvinceId, string District, string AddressLine, string? Phone);

/// <summary>An item's defect claim as it stands (06 4.30): its id, status, defective units, the return address it copied and the customer's first description (04 5.19.4).</summary>
public sealed record DefectClaimInfo(Guid Id, Guid OrderItemId, DefectClaimStatus Status, int Quantity, DateTimeOffset CreatedAt, ItemReturnAddress? ReturnAddress, string Description);

/// <summary>
/// The outcome of an AfterSales writer: a refusal, or the written record's id (and the claim as it now stands, on a claim's write). <paramref name="ReturnAddress"/> is the
/// return address a withdrawal copied - what B-9 writes.
/// </summary>
public sealed record ItemRecordWriteResult(
    ItemRecordRefusal? Refusal, Guid? RecordId, DefectClaimInfo? Claim, ItemReturnAddress? ReturnAddress = null, bool IsIbanRequested = false)
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
