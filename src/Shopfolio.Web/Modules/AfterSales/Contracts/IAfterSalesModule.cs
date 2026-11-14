using Shopfolio.Modules.Catalog.Contracts;

namespace Shopfolio.Modules.AfterSales.Contracts;

/// <summary>
/// The open surface of the AfterSales module (09 6 item 1; 05 3.4.1): what other modules, the pages and the commands may call. The module owns cancellation, withdrawal and refund (05 3.3.5).
/// The members arrive with the tasks that own the module's rules, as methods that take and give immutable records and join the caller's
/// database transaction (09 6 item 3).
/// </summary>
public interface IAfterSalesModule
{
    /// <summary>
    /// The order's disposal (06 9.2.1, 9.2.2; K-1030; K-1069): empties the administrator link and name of the item records (the cancellation's and the late
    /// delivery termination's and the withdrawal's included), the cancellation's free note and the IBAN links of the cancellation, the late delivery termination and
    /// the withdrawal, and deletes the refund IBAN requests and the refund IBANs of the orders. The item records themselves, their amounts and their dates stay.
    /// It writes through the caller's change tracker: the caller holds the orders' row locks inside its transaction, has loaded the order items, raises the orders'
    /// versions and saves; the method opens no transaction and saves nothing (09 6 item 3).
    /// </summary>
    Task ErasePersonalDataAsync(OrderPersonalDataScope scope, CancellationToken ct);

    /// <summary>
    /// The quantity each order item has closed by its item records (02 5.8; 06 2.7.2; K-790): the sum of the quantities of its cancellation,
    /// removal, late delivery termination and withdrawal records, saved or added in the caller's context and not saved yet. A cancellation whose closing
    /// transition (S3, S4, S9) was corrected is undone and does not count (06 4.22; 02 10.4.6). An item with no such record is not in the result. The order module takes the item's open quantity as its own quantity minus this (03 1.4.3: the closure rule). A plain read;
    /// it writes nothing and takes no lock - the caller holds the order's lock (05 7.3.2).
    /// </summary>
    Task<IReadOnlyDictionary<Guid, int>> ClosedQuantitiesAsync(IReadOnlyCollection<Guid> orderItemIds, CancellationToken ct);

    /// <summary>
    /// The units of each order item that no operation takes (02 7.1.6; K-1077): the closed quantity of <see cref="ClosedQuantitiesAsync"/> plus the units rescinded on a
    /// defect claim - they do not close the item (the closure rule keeps reading <see cref="ClosedQuantitiesAsync"/>) but are no other operation's subject. The units open
    /// to an operation are the item's quantity minus this; the caller's unsaved rows count. A plain read under the caller's order lock.
    /// </summary>
    Task<IReadOnlyDictionary<Guid, int>> UnavailableQuantitiesAsync(IReadOnlyCollection<Guid> orderItemIds, CancellationToken ct);

    /// <summary>
    /// The check of the firm's rescission on a defect claim (02 7.5.3; 03 2.9.4; K-788, K-1077): the item's claim is open, the units are at least one and at most both the
    /// claim's defective units not rescinded yet and the units open to an operation, and the refund's amounts are split from the item's frozen values against what its
    /// earlier records set aside - the rescission that takes the last open units takes the rest (02 7.1.7 (4)). It writes nothing: the refund the payment module starts
    /// is the rescission's record. Requires the caller's transaction and order row lock.
    /// </summary>
    Task<DefectRemedyCheck> CheckDefectClaimRescissionAsync(DefectClaimRescissionEntry entry, CancellationToken ct);

    /// <summary>
    /// The check of the firm's price reduction on a defect claim (02 7.5.3; 03 8.3.3.7; K-228, K-539): the item's claim is open. The item is not returned and no unit
    /// leaves the open quantity; the amount's ceiling is the payment's. Requires the caller's transaction and order row lock.
    /// </summary>
    Task<DefectRemedyCheck> CheckDefectClaimPriceReductionAsync(ItemRecordSubject subject, CancellationToken ct);

    /// <summary>
    /// The IBAN request of a defect remedy's refund on the bank transfer line (02 7.5.3, 7.4.5; 03 8.3.3.6; K-686): when the order has no IBAN on file and the item has no
    /// open request, the administrator's request is written on the item for the refund; true when one was written - the caller then sends B-14. Joins the caller's
    /// transaction under the order's row lock and saves nothing.
    /// </summary>
    Task<bool> RequestDefectRemedyIbanAsync(DefectRemedyIbanRequest request, CancellationToken ct);

    /// <summary>
    /// The units of each order item as the coupon right's return reads them (06 4.14, the paragraph after path 6; 03 1.4.4; K-1104, K-1106): the closed
    /// quantity of <see cref="ClosedQuantitiesAsync"/>, the withdrawn quantity, and how much of the withdrawal the returned goods' receipt, the return
    /// rejection and the standing "goods did not come back" closing settled. With <paramref name="includeUnsaved"/> the records the caller added to its
    /// context and has not saved count too, and a closing the caller's receipt opened and has not saved no longer stands; without it only the saved ones -
    /// the order as it was before the caller's record. Every item is in the result.
    /// A plain read; the caller holds the order's lock.
    /// </summary>
    Task<IReadOnlyDictionary<Guid, ItemUnitFacts>> UnitFactsAsync(IReadOnlyCollection<Guid> orderItemIds, bool includeUnsaved, CancellationToken ct);

    /// <summary>
    /// What the refund of an item record pays back (02 7.1.7; 06 4.33; T369): a cancellation's, removal's, termination's or withdrawal declaration's own set-aside amounts;
    /// a receipt of a withdrawal's goods (<see cref="RefundSubject.ReturnId"/>) - the refund of the withdrawal after the delivery, one per receipt (K-1057) - takes its part of the declaration's amounts, split by
    /// <c>ItemShares.SetAsideParts</c> over the declaration's receipts in their order (02 7.1.7 (5)). Null when the record is not there or the receipt is not a withdrawal's.
    /// The caller's unsaved records count. A plain read; the caller holds the order's lock.
    /// </summary>
    Task<RefundAmount?> RefundAmountAsync(RefundSubject subject, CancellationToken ct);

    /// <summary>
    /// Whether the record's refund carries the order's frozen shipping fee (02 7.2.9, 7.4.6; 06 4.33 <c>IsShippingFeeIncluded</c>): the rule of <c>ShippingFeeRefunds</c> over the
    /// records of the order's physical items, the caller's unsaved ones included. The fee is paid back once; the refunds' own unique rule keeps that (06 4.33). A plain
    /// read; the caller holds the order's lock.
    /// </summary>
    Task<bool> IsShippingFeeIncludedAsync(ShippingFeeQuery query, CancellationToken ct);

    /// <summary>The item's defect claim as it stands (06 4.30), or null when the item has none. A plain read.</summary>
    Task<DefectClaimInfo?> FindDefectClaimAsync(Guid orderItemId, CancellationToken ct);

    /// <summary>
    /// The item record writers (03 1.7.1, 1.7.4, 1.9.1; T320). Each adds its own row in the caller's context - two declarations are two rows, two partial
    /// arrivals two receipts (K-790, K-794) - after the unit rule (at least one, at most the quantity open to the operation; a larger value is refused). They join the
    /// caller's transaction and require the order's row lock in it (05 7.3.2): the caller is the transition unit, which checks the order's own conditions, raises the
    /// order's version and saves (09 9.1 item 4; 09 6 item 3). A refusal is a value and writes nothing.
    /// </summary>
    /// <remarks>
    /// <see cref="OpenDefectClaimAsync"/>: one claim per item (an existing claim, open or resolved, is refused), the channel of Z-18 and the units open to a claim.
    /// <see cref="ChangeDefectClaimStatusAsync"/>: the claim's own transitions (Open to Resolved by an administrator; Resolved to Open by the customer or an
    /// administrator inside Z-18, the customer may raise the units), the status and its history entry together - the only path to the claim's status.
    /// <see cref="AddReturnRejectionAsync"/>: only on an item frozen with a conditional exemption, at most the declaration's units still on the way back.
    /// </remarks>
    Task<ItemRecordWriteResult> OpenDefectClaimAsync(DefectClaimOpening opening, CancellationToken ct);

    /// <inheritdoc cref="OpenDefectClaimAsync" path="/summary"/>
    Task<ItemRecordWriteResult> ChangeDefectClaimStatusAsync(DefectClaimStatusChange change, CancellationToken ct);

    /// <summary>
    /// The customer's cancellation record (06 4.22; 02 7.2.2, 7.1.7; T367): the unit rule (at least one, at most the item's open units), then one row with the customer
    /// as the actor, the moment and the amounts it sets aside from the item - the coupon share and the VAT are the units' share rounded down, and the record that closes
    /// the item's last open units takes what the earlier records left (02 7.1.7 (4)). Whether the operation is open at all is the order module's decision (03 1.11);
    /// the writer joins the caller's transaction under the order's row lock and saves nothing.
    /// </summary>
    Task<ItemRecordWriteResult> AddCancellationAsync(CustomerCancellationEntry entry, CancellationToken ct);

    /// <summary>
    /// The firm's cancellation record (06 4.22; 02 7.2.4; T368): the unit rule of the customer's record, then one row with the administrator as the actor, the reason and
    /// - on "Other" only - its note; the amounts as on the customer's record. Whether the operation is open is the order module's decision (03 1.11.21); the writer joins
    /// the caller's transaction under the order's row lock and saves nothing.
    /// </summary>
    Task<ItemRecordWriteResult> AddFirmCancellationAsync(FirmCancellationEntry entry, CancellationToken ct);

    /// <summary>
    /// The late delivery termination record of one item (06 4.24; 02 7.2.3, 7.1.6; T368): only a physical item with no delivery date, and the quantity is exactly the item's
    /// open units (K-796, K-1077); the record that closes the item's last open units takes the rest of its coupon share and VAT (02 7.1.7 (4)). The customer's row carries no
    /// administrator; the record of a notice from another channel carries the administrator. Joins the caller's transaction under the order's row lock and saves nothing.
    /// </summary>
    Task<ItemRecordWriteResult> AddLateDeliveryTerminationAsync(LateDeliveryTerminationEntry entry, CancellationToken ct);

    /// <summary>
    /// The cancellations, removals (marked, T386; D-848) and late delivery terminations of the given items with their moments, for the panel's legal interest warning (02 7.2.3)
    /// and the export's "closed before the shipment" (09 10.10 item 5): a cancellation undone by the correction of its closing transition is left out, like its closed units
    /// (<see cref="ClosedQuantitiesAsync"/>). A plain read.
    /// </summary>
    Task<IReadOnlyList<ClosingRecordMoment>> ClosingRecordMomentsAsync(IReadOnlyCollection<Guid> orderItemIds, CancellationToken ct);

    /// <summary>
    /// The records on the items' rows of the customer's order page (04 5.16.5; T511), oldest first: cancellations, removals and late delivery terminations as
    /// <see cref="ClosingRecordMomentsAsync"/> reads them, withdrawals, return receipts, return rejections, "mal dönmedi" closures that were not reopened and defect claims. A plain read.
    /// </summary>
    Task<IReadOnlyList<ItemRecordFact>> ListItemRecordsAsync(IReadOnlyCollection<Guid> orderItemIds, CancellationToken ct);

    /// <summary>
    /// Links the cancellation the caller added a moment ago to the transition that closed the order with it (06 4.22 <c>TransitionRecordId</c>: the S3, S4 or S9 of
    /// the cancellation of the order's last open units), so the correction of a wrong cancellation finds it (<see cref="FindCancellationsOfTransitionAsync"/>).
    /// Writes through the caller's change tracker and saves nothing.
    /// </summary>
    Task LinkClosingTransitionAsync(Guid cancellationId, Guid transitionRecordId, CancellationToken ct);

    /// <summary>
    /// The withdrawal declaration's writer (03 1.7.1.5; 02 7.3.5, 10.4.10; T372): a record of its own with the units, the date, the firm's current return address
    /// copied on a physical item, the amounts set aside from the item's frozen values (02 7.1.7) and the refund IBAN of the transfer line - entered, the one on file, or
    /// none with an IBAN request on the item; after the delivery the IBAN gets its Z-38 deletion day. The rules of the other writers apply (lock, units, refusal as a value).
    /// </summary>
    Task<ItemRecordWriteResult> AddWithdrawalAsync(WithdrawalDeclaration declaration, CancellationToken ct);

    /// <summary>
    /// What the declaration screen shows for each of <paramref name="items"/>, in their order (02 7.3.5; <see cref="WithdrawalNotes"/>): the return address is the setting as it is
    /// now, read once for the screen; the refund's start is read for a declaration sent today against the item's delivery date. A plain read.
    /// </summary>
    Task<IReadOnlyList<WithdrawalNotes>> WithdrawalNotesAsync(IReadOnlyList<WithdrawalNoteQuery> items, CancellationToken ct);

    /// <inheritdoc cref="OpenDefectClaimAsync" path="/summary"/>
    Task<ItemRecordWriteResult> AddReturnReceiptAsync(ReturnReceiptEntry entry, CancellationToken ct);

    /// <inheritdoc cref="OpenDefectClaimAsync" path="/summary"/>
    Task<ItemRecordWriteResult> AddReturnRejectionAsync(ReturnRejectionEntry entry, CancellationToken ct);

    /// <inheritdoc cref="OpenDefectClaimAsync" path="/summary"/>
    Task<ItemRecordWriteResult> AddNotReturnedClosureAsync(NotReturnedClosureEntry entry, CancellationToken ct);

    /// <summary>
    /// The "Stoğa ekle" step's mark on a receipt (02 7.4.7; 06 4.26 <c>StockAddedAt</c>): once per receipt, the receipt's units as the result; the caller returns them to
    /// the variant (03 1.4.4.6). Runs in the caller's transaction under the order's row lock and saves nothing.
    /// </summary>
    Task<ReturnStockResult> MarkReturnStockAddedAsync(ReturnStockEntry entry, CancellationToken ct);

    /// <summary>
    /// The items' panel marks (03 1.7.3; K-675): read from the records while the condition lasts, never stored - "awaiting the returned goods" and "awaiting the IBAN".
    /// The caller hands in the items' delivery dates (the item is the order's). Every item is in the result. A plain read.
    /// </summary>
    Task<IReadOnlyDictionary<Guid, ItemMarks>> ItemMarksAsync(IReadOnlyCollection<ItemDelivery> items, CancellationToken ct);

    /// <summary>
    /// The cancellation records that closed the order with the given transition (06 4.22 <c>TransitionRecordId</c>: the S3, S4 or S9 written with the
    /// cancellation of the last open units) - what the correction of the firm's wrong cancellation takes back (03 1.6.2.2). A plain read.
    /// </summary>
    Task<IReadOnlyList<CancelledUnits>> FindCancellationsOfTransitionAsync(Guid transitionRecordId, CancellationToken ct);

    /// <summary>
    /// True when one of the items has a cancellation, removal, late delivery termination or withdrawal record, saved or added in the caller's context - a
    /// cancellation undone by a correction included, since the record stays (03 1.6.2.4; K-703). A plain read.
    /// </summary>
    Task<bool> HasClosingRecordAsync(IReadOnlyCollection<Guid> orderItemIds, CancellationToken ct);

    /// <summary>
    /// The correction of the firm's wrong cancellation closes its open IBAN requests (06 4.22, 4.31; 02 10.4.6; K-1056): the open requests of the voided refunds
    /// and the open "cancellation or removal" requests of the cancelled items that have no removal record. Writes through the caller's change tracker and saves
    /// nothing.
    /// </summary>
    Task CloseCancellationIbanRequestsAsync(IReadOnlyCollection<Guid> orderItemIds, IReadOnlyCollection<Guid> refundIds, CancellationToken ct);

    /// <summary>
    /// The number of orders paid by card that could still cause a card refund, for the server's status command (T389; 05 8.5.6; 08 2.1.2; K-1287): an order paid by card, not fully refunded,
    /// that has an open item (an item with units no record has closed, not yet delivered) or an item delivered whose withdrawal window of fourteen days has not run out (Z-13; the service
    /// item's mark and the digital item close the right, Z-14, Z-15). Each order is counted once. The number is the installer's alone: it is in no counter of the panel (02 10.6.1). A plain
    /// read; it counts and writes nothing.
    /// </summary>
    Task<int> CountCardOrdersWithRefundExposureAsync(CancellationToken ct);

    /// <summary>
    /// The refund IBANs whose deletion day (Z-38) is at or before <paramref name="now"/>, earliest first, all of them (05 5.2.5; 09 13.1 item 6). A plain
    /// read for the scheduler's scan; it writes nothing.
    /// </summary>
    Task<IReadOnlyList<DueRefundIbanDeletion>> FindDueRefundIbanDeletionsAsync(DateTimeOffset now, CancellationToken ct);

    /// <summary>
    /// What the refund IBAN is used by (<see cref="RefundIbanUse"/>), or null when it is gone. A plain read; the deletion day reads it once to find the
    /// order and once more under the order's row lock.
    /// </summary>
    Task<RefundIbanUse?> FindRefundIbanUseAsync(Guid refundIbanId, CancellationToken ct);

    /// <summary>
    /// The deletion day's outcome on the IBAN (03 4.2.8; 02 4.3; K-1110, K-1127): with <paramref name="deleteIban"/> the IBAN is deleted - the links of the
    /// cancellation, the late delivery termination, the withdrawal and the IBAN request are emptied by the database - and the deletion is written to the
    /// disposal record in the same transaction (Z-44; K-708); the withdrawal and its item stay as they are. Without it the IBAN stays for the refund that
    /// still uses it and its deletion day is emptied: the deletion that comes with that refund's completion is the refund line's (T370). It joins the
    /// caller's transaction under the order's row lock and saves nothing (09 6 item 3).
    /// </summary>
    Task ApplyRefundIbanDeletionDayAsync(Guid refundIbanId, bool deleteIban, CancellationToken ct);

    /// <summary>
    /// The items' records and periods at <paramref name="now"/> (T323; 03 1.11; 02 4.2 Z-13, Z-14, Z-18, Z-42): the one place the withdrawal window, the defect claim channel and the
    /// "goods did not come back" period are worked out, read by the order module's operation decision so the screen and the POST share one answer. Every queried item is in the
    /// result. A plain read; it writes nothing and takes no lock.
    /// </summary>
    Task<IReadOnlyDictionary<Guid, ItemPeriodState>> ItemPeriodStatesAsync(IReadOnlyCollection<ItemPeriodQuery> items, DateTimeOffset now, CancellationToken ct);

    /// <summary>The order's refund IBAN as the operation decision reads it (03 1.11.11, 1.11.12, 1.11.39): the IBAN on file, and an open request on one of the given items. A plain read.</summary>
    Task<RefundIbanState> RefundIbanStateAsync(Guid orderId, IReadOnlyCollection<Guid> orderItemIds, CancellationToken ct);

    /// <summary>
    /// Opens the IBAN request of the refund line (T370; 06 4.29; 02 7.2.8, 7.4.5; K-569, K-690): on the item - or, itemless, on the late payment's refund - unless
    /// one is already open there, which the event then uses. The caller sends B-14 when <see cref="RefundIbanRequestOpened.IsNew"/>. Writes through the caller's
    /// change tracker under the order's row lock and saves nothing.
    /// </summary>
    Task<RefundIbanRequestOpened> OpenRefundIbanRequestAsync(RefundIbanRequestEntry entry, CancellationToken ct);

    /// <summary>The open IBAN request with the given id, or null when it is fulfilled, closed or gone. A plain read; the caller checks that its item or refund is the order's.</summary>
    Task<OpenRefundIbanRequest?> FindOpenRefundIbanRequestAsync(Guid requestId, CancellationToken ct);

    /// <summary>
    /// The order page's IBAN fields (T512; 04 5.16.12; 02 7.4.5): the open requests of <paramref name="orderItemIds"/>, the open itemless requests of a late payment's
    /// refund (K-1075; the caller keeps those whose refund is the order's) and the order's IBANs still on file - an IBAN lives only while a pending refund uses it, so
    /// one on file is not processed yet and is corrected in place (K-575). A plain read.
    /// </summary>
    Task<IReadOnlyList<RefundIbanField>> ListRefundIbanFieldsAsync(Guid orderId, IReadOnlyCollection<Guid> orderItemIds, CancellationToken ct);

    /// <summary>
    /// Whether a cancellation of <paramref name="selectedUnits"/> before the carrier took the order pays the shipping fee back (T512; 02 7.2.9; 04 5.17.5; K-789): the
    /// shipping fee rule's trigger (1) over the order's physical items' open units, read for the screen before the last step so the page works it out not itself (09 3.4
    /// item 1). A pure answer; reads and writes nothing.
    /// </summary>
    bool CancellationRefundsShippingFee(IReadOnlyDictionary<Guid, int> openPhysicalUnits, IReadOnlyDictionary<Guid, int> selectedUnits);

    /// <summary>
    /// The withdrawal's shipping fee trigger as the declaration screen reads it (T513; 02 7.4.6; 04 5.18.6; K-789, K-1076): per physical item the units the carrier took - its units
    /// less those cancelled or removed before <paramref name="handedToCarrierAt"/> - that no declaration withdrew yet. Null when no such unit is left, or a unit was rejected or
    /// closed as not come back - it stayed with the customer, so no withdrawal brings the fee back. The caller hands no items on an order whose frozen fee is zero. A plain read.
    /// </summary>
    Task<IReadOnlyDictionary<Guid, int>?> WithdrawalFeeUnitsAsync(IReadOnlyCollection<ShippedItem> physicalItems, DateTimeOffset handedToCarrierAt, CancellationToken ct);

    /// <summary>
    /// Whether withdrawing <paramref name="selectedUnits"/> now meets the withdrawal's shipping fee trigger over <paramref name="undeclaredUnits"/> (<see cref="WithdrawalFeeUnitsAsync"/>):
    /// the rule's answer, so the screen works out nothing itself (09 3.4 item 1). A pure answer.
    /// </summary>
    bool WithdrawalRefundsShippingFee(IReadOnlyDictionary<Guid, int> undeclaredUnits, IReadOnlyDictionary<Guid, int> selectedUnits);

    /// <summary>The refund period of a cancellation and of a late delivery termination in calendar days (T512; 02 4.2 Z-17; 02 7.2.3): the period rule's own value.</summary>
    int RefundPeriodDays { get; }

    /// <summary>True when an IBAN request on the refund was fulfilled: the customer chose the transfer path, so the card refund is not retried (02 7.2.8; K-569).</summary>
    Task<bool> IsRefundIbanEnteredForRefundAsync(Guid refundId, CancellationToken ct);

    /// <summary>
    /// The customer's IBAN for an open request (02 7.4.5; 03 1.11.33): checked for format and check digits, stored without blanks and upper case, the request
    /// fulfilled with it. The firm never enters it (02 8.4.2). Writes through the caller's change tracker under the order's row lock and saves nothing.
    /// </summary>
    Task<RefundIbanWriteResult> EnterRefundIbanAsync(RefundIbanEntry entry, CancellationToken ct);

    /// <summary>
    /// The customer's correction of the order's IBAN in place until the refund is processed (02 7.4.5; K-575): the same check as the entry; the firm's step uses
    /// the IBAN in the field at that moment. Writes through the caller's change tracker under the order's row lock and saves nothing.
    /// </summary>
    Task<RefundIbanWriteResult> ChangeRefundIbanAsync(RefundIbanCorrection correction, CancellationToken ct);

    /// <summary>
    /// The card refund succeeded: the open IBAN request of its transfer path closes unfulfilled, the IBAN field closes (02 7.2.8; 06 4.29 <c>ClosedAt</c>). Writes
    /// through the caller's change tracker and saves nothing.
    /// </summary>
    Task CloseRefundIbanRequestsOfRefundAsync(Guid refundId, CancellationToken ct);

    /// <summary>
    /// The IBAN's end (T370; 02 7.4.5; Z-44; K-356, K-1110): every IBAN of the order that no pending refund uses any more is deleted and each deletion is written
    /// to the disposal record in the same transaction. An IBAN stays while another refund still uses it - a record linked to it whose refund is not settled
    /// (a withdrawal by its units still to be paid back), or a request fulfilled with it whose refund is not settled. Returns the deleted ids. Under the order's
    /// row lock in the caller's transaction; saves nothing.
    /// </summary>
    Task<IReadOnlyList<Guid>> DeleteUnusedRefundIbansAsync(Guid orderId, CancellationToken ct);

    /// <summary>
    /// The firm's "havale gerçekleşmedi" (02 7.4.5; K-575, K-1131): the IBAN the item's refund - or the itemless late payment refund - would use is deleted with
    /// its disposal record, and a "transfer failed" request opens the IBAN field again; the period does not stop. Returns the request, or null when there is no
    /// IBAN to report. The caller sends B-14. Under the order's row lock in the caller's transaction; saves nothing.
    /// </summary>
    Task<RefundIbanRequestOpened?> ReportTransferFailedAsync(TransferFailure failure, CancellationToken ct);

    /// <summary>
    /// The items with a cancellation, removal, late delivery termination or withdrawal whose event day falls in <paramref name="startDay"/>…<paramref name="endDay"/> (Turkey days)
    /// (T383; 02 10.6.3; 07 9.22.1; K-580): the cancellation and the removal on their record's day, the termination on its notice's date stamp, the withdrawal on its declaration's
    /// date. A cancellation undone by a correction is left out. A plain read.
    /// </summary>
    Task<IReadOnlyList<Guid>> ListItemsWithLossInRangeAsync(DateOnly startDay, DateOnly endDay, CancellationToken ct);

    /// <summary>
    /// Which of the given items have a cancellation, removal, late delivery termination or withdrawal at any time (T383; 02 10.6.3; K-790); a cancellation undone by a correction
    /// does not count, the "goods did not come back" closure and the return rejection are not read. A plain read.
    /// </summary>
    Task<IReadOnlySet<Guid>> FindItemsWithLossAsync(IReadOnlyCollection<Guid> orderItemIds, CancellationToken ct);

    /// <summary>
    /// The open quantity each given item had at its moment (T383; 02 10.6.3; K-535): the item's quantity minus the units of its saved closing records - the records of
    /// <see cref="ClosedQuantitiesAsync"/> (cancellation, removal, late delivery termination, withdrawal; a cancellation undone by a correction does not count) - written before
    /// <see cref="ItemQuantityAt.At"/>, by the one open quantity rule. Every item is in the result. A plain read; it writes nothing and takes no lock.
    /// </summary>
    Task<IReadOnlyDictionary<Guid, int>> OpenQuantitiesAtAsync(IReadOnlyCollection<ItemQuantityAt> items, CancellationToken ct);

    /// <summary>The open defect claims (T382; 02 10.6.1): one per claim, whatever the order's state. A plain read that counts; nothing is kept.</summary>
    Task<int> CountOpenDefectClaimsAsync(CancellationToken ct);

    /// <summary>
    /// The number of defect claims the panel's requests list takes (T532; 07 UC-93): the claims in one of <paramref name="statuses"/>, none named meaning any status. A claim is kept until the disposal deletes it
    /// with its order's data (06 9.1 Z-29), so every row is a kept one. A plain read that counts.
    /// </summary>
    Task<int> CountPanelDefectClaimsAsync(IReadOnlyCollection<DefectClaimStatus> statuses, CancellationToken ct);

    /// <summary>
    /// The first <paramref name="take"/> defect claims of the panel's requests list (T532; 07 UC-93), newest opening first, in one of <paramref name="statuses"/> (none named: any status): a claim is ordered by
    /// its last "Open" entry of the status history - a reopened claim by its last reopening -, or by its creation moment when it was never reopened; equal moments by the id as the database sorts it. The caller
    /// merges them with the contact requests. A plain read; the whole set is never taken.
    /// </summary>
    Task<IReadOnlyList<PanelDefectClaimRow>> ListPanelDefectClaimsAsync(IReadOnlyCollection<DefectClaimStatus> statuses, int take, CancellationToken ct);

    /// <summary>
    /// The orders whose returned goods the firm still awaits (T382; 02 10.6.1, 10.4.9, 7.4.9): a shipped physical item with units of a withdrawal or a late delivery termination that
    /// are neither received, rejected nor closed as "mal dönmedi". A closure that was reopened (the goods arrived) counts again. A plain read.
    /// </summary>
    Task<IReadOnlyList<Guid>> ListOrdersAwaitingReturnGoodsAsync(CancellationToken ct);

    /// <summary>
    /// The number of items marked "iade malı bekleniyor" (T536; 03 1.7.3.5, 8.7.4.5; K-665, K-814): the items of <see cref="ListOrdersAwaitingReturnGoodsAsync"/> whose <see cref="ItemMarksAsync"/> mark
    /// is set, counted one by one - a withdrawal before the delivery and a late delivery termination await goods but carry no mark. The return address screen's confirmation says this number. A plain read.
    /// </summary>
    Task<int> CountItemsAwaitingReturnGoodsAsync(CancellationToken ct);

    /// <summary>
    /// One page of the orders that owe the firm a refund step (T382; 02 10.6.1; K-621): the refund of a paid order's item record - a cancellation that was not undone, a removal, a late
    /// delivery termination, a withdrawal owed at its declaration, a withdrawal's goods receipt - that has no refund row yet, on the bank transfer line and, for the withdrawal, on the
    /// card line; the system's own card refunds always have their row. An item that waits for the customer's IBAN is not counted. The page reads the next <paramref name="maxSubjects"/>
    /// record ids after <paramref name="afterId"/> (null starts from the first); an empty page is the end. A plain read.
    /// </summary>
    Task<RefundProcessingPage> ListOrdersAwaitingRefundProcessingAsync(Guid? afterId, int maxSubjects, CancellationToken ct);

    /// <summary>
    /// The refunds among <paramref name="refunds"/> that wait for the customer's IBAN (T382; 02 10.6.1; K-525, K-686): an open IBAN request - neither fulfilled nor closed - is linked to the
    /// refund itself (the transfer path opened after a failed card refund, or an itemless request) or to the refund's item with no refund yet (the request the defect remedy's or the
    /// notice's item record opened when the file had no IBAN). Such a refund is the customer's to move, not the firm's; the rule is here alone. A plain read.
    /// </summary>
    Task<IReadOnlySet<Guid>> FindRefundsAwaitingIbanAsync(IReadOnlyCollection<RefundOfItem> refunds, CancellationToken ct);

    /// <summary>
    /// The open IBAN requests - neither fulfilled nor closed - as the panel home's "Müşteriden beklenenler" list reads them (T521; 02 10.6.1; 04 2.6.2; K-716): the item or the refund the request is on
    /// and the last day of the refund period when the line has one. The period belongs to the cancellation, the removal and the withdrawal lines; the defect remedy, the amount-based and the late payment's
    /// lines run without one. A plain read; nothing is written.
    /// </summary>
    Task<IReadOnlyList<AwaitingIbanLine>> ListAwaitingIbanLinesAsync(CancellationToken ct);

    /// <summary>
    /// The firm's removal record (06 4.23; 02 10.4.3; T386): the unit rule of the cancellation (at most the units open to an operation), then one row with the administrator
    /// and no reason; the amounts set aside as on the cancellation (02 7.1.7). Whether the operation is open is the order module's decision (03 1.11.22); the writer joins the
    /// caller's transaction under the order's row lock and saves nothing.
    /// </summary>
    Task<ItemRecordWriteResult> AddItemRemovalAsync(ItemRemovalEntry entry, CancellationToken ct);

    /// <summary>The receipts and rejections of returned goods on the given items, with their arrival dates (02 10.4.5; K-1474, K-712). A plain read.</summary>
    Task<IReadOnlyList<ArrivalRecordDate>> ArrivalRecordDatesAsync(IReadOnlyCollection<Guid> orderItemIds, CancellationToken ct);

    /// <summary>The withdrawal declarations of the given items with their days (02 7.4.1; K-608). A plain read.</summary>
    Task<IReadOnlyList<WithdrawalDeclarationDay>> WithdrawalDeclarationDaysAsync(IReadOnlyCollection<Guid> orderItemIds, CancellationToken ct);

    /// <summary>
    /// The firm's answer, on the corrected delivery date, to whether the delivery came before the declaration of the given withdrawals (02 10.4.5, 7.4.1; K-608): written
    /// on each declaration whose day is the corrected date. The declaration itself stays (02 7.6.7). Joins the caller's transaction under the order's row lock and saves nothing.
    /// </summary>
    Task SetDeliveryOrderAsync(IReadOnlyCollection<Guid> withdrawalIds, bool deliveredBeforeDeclaration, CancellationToken ct);

    /// <summary>
    /// The correction of a receipt's or a rejection's arrival date in place (02 10.4.5; K-712): the record must be one of the item's, and the date not before the item's
    /// delivery date (<see cref="ItemRecordRefusal.ArrivalDateOutOfRange"/>, the receipt's rule); a future date is the caller's refusal. Returns the old date on success; the
    /// refund's fourteen days run from the new date because they are read from it. Joins the caller's transaction under the order's row lock and saves nothing.
    /// </summary>
    Task<(ItemRecordRefusal? Refusal, DateOnly? OldDate)> CorrectArrivalDateAsync(ItemRecordSubject subject, Guid recordId, DateOnly arrivalDate, CancellationToken ct);
}

/// <summary>One open IBAN request of the panel home's list (T521; 04 9.3.6): the item it is on (null on the refund of a late payment), the refund it names (null before a refund is linked), the reason it was opened for, the instant it was opened, the last day of its refund period (null when it has none).</summary>
public sealed record AwaitingIbanLine(Guid? OrderItemId, Guid? RefundId, RefundIbanRequestReason Reason, DateTimeOffset CreatedAt, DateOnly? RefundLastDay);

/// <summary>A refund and the item it is for, null when it is for no one item (T382): the pair the IBAN wait is read by.</summary>
public sealed record RefundOfItem(Guid RefundId, Guid? OrderItemId);

/// <summary>One cancellation record as the order module needs it: the record, the item and the units it closed.</summary>
public sealed record CancelledUnits(Guid CancellationId, Guid OrderItemId, int Quantity);

/// <summary>The orders whose personal data goes, with the ids that reach their records: their items and their refunds (the refund IBAN request is linked to either).</summary>
public sealed record OrderPersonalDataScope(
    IReadOnlyCollection<Guid> OrderIds,
    IReadOnlyCollection<Guid> OrderItemIds,
    IReadOnlyCollection<Guid> RefundIds);
