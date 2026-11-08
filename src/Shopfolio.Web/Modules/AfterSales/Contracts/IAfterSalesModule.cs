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
}

/// <summary>One cancellation record as the order module needs it: the record, the item and the units it closed.</summary>
public sealed record CancelledUnits(Guid CancellationId, Guid OrderItemId, int Quantity);

/// <summary>The orders whose personal data goes, with the ids that reach their records: their items and their refunds (the refund IBAN request is linked to either).</summary>
public sealed record OrderPersonalDataScope(
    IReadOnlyCollection<Guid> OrderIds,
    IReadOnlyCollection<Guid> OrderItemIds,
    IReadOnlyCollection<Guid> RefundIds);
