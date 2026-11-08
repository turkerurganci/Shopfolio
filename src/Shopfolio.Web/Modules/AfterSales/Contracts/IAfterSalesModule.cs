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
    /// removal, late delivery termination and withdrawal records, saved or added in the caller's context and not saved yet. An item with no such record is
    /// not in the result. The order module takes the item's open quantity as its own quantity minus this (03 1.4.3: the closure rule). A plain read;
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
}

/// <summary>The orders whose personal data goes, with the ids that reach their records: their items and their refunds (the refund IBAN request is linked to either).</summary>
public sealed record OrderPersonalDataScope(
    IReadOnlyCollection<Guid> OrderIds,
    IReadOnlyCollection<Guid> OrderItemIds,
    IReadOnlyCollection<Guid> RefundIds);
