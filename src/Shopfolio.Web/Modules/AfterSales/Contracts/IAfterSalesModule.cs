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
    /// The units of each order item as the coupon right's return reads them (06 4.14, the paragraph after path 6; 03 1.4.4; K-1104, K-1106): the closed
    /// quantity of <see cref="ClosedQuantitiesAsync"/>, the withdrawn quantity, and how much of the withdrawal the returned goods' receipt, the return
    /// rejection and the standing "goods did not come back" closing settled. With <paramref name="includeUnsaved"/> the records the caller added to its
    /// context and has not saved count too; without it only the saved ones - the order as it was before the caller's record. Every item is in the result.
    /// A plain read; the caller holds the order's lock.
    /// </summary>
    Task<IReadOnlyDictionary<Guid, ItemUnitFacts>> UnitFactsAsync(IReadOnlyCollection<Guid> orderItemIds, bool includeUnsaved, CancellationToken ct);

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

    /// <inheritdoc cref="OpenDefectClaimAsync" path="/summary"/>
    Task<ItemRecordWriteResult> AddWithdrawalAsync(WithdrawalDeclaration declaration, CancellationToken ct);

    /// <inheritdoc cref="OpenDefectClaimAsync" path="/summary"/>
    Task<ItemRecordWriteResult> AddReturnReceiptAsync(ReturnReceiptEntry entry, CancellationToken ct);

    /// <inheritdoc cref="OpenDefectClaimAsync" path="/summary"/>
    Task<ItemRecordWriteResult> AddReturnRejectionAsync(ReturnRejectionEntry entry, CancellationToken ct);

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
}

/// <summary>One cancellation record as the order module needs it: the record, the item and the units it closed.</summary>
public sealed record CancelledUnits(Guid CancellationId, Guid OrderItemId, int Quantity);

/// <summary>The orders whose personal data goes, with the ids that reach their records: their items and their refunds (the refund IBAN request is linked to either).</summary>
public sealed record OrderPersonalDataScope(
    IReadOnlyCollection<Guid> OrderIds,
    IReadOnlyCollection<Guid> OrderItemIds,
    IReadOnlyCollection<Guid> RefundIds);
