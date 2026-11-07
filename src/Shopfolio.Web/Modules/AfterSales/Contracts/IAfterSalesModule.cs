namespace Shopfolio.Modules.AfterSales.Contracts;

/// <summary>
/// The open surface of the AfterSales module (09 6 item 1; 05 3.4.1): what other modules, the pages and the commands may call. The module owns cancellation, withdrawal and refund (05 3.3.5).
/// The members arrive with the tasks that own the module's rules, as methods that take and give immutable records and join the caller's
/// database transaction (09 6 item 3).
/// </summary>
public interface IAfterSalesModule
{
    /// <summary>
    /// The quantity each order item has closed by its item records (02 5.8; 06 2.7.2; K-790): the sum of the quantities of its cancellation,
    /// removal, late delivery termination and withdrawal records, saved or added in the caller's context and not saved yet. An item with no such record is
    /// not in the result. The order module takes the item's open quantity as its own quantity minus this (03 1.4.3: the closure rule). A plain read;
    /// it writes nothing and takes no lock - the caller holds the order's lock (05 7.3.2).
    /// </summary>
    Task<IReadOnlyDictionary<Guid, int>> ClosedQuantitiesAsync(IReadOnlyCollection<Guid> orderItemIds, CancellationToken ct);
}
