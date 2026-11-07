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
}

/// <summary>The orders whose personal data goes, with the ids that reach their records: their items and their refunds (the refund IBAN request is linked to either).</summary>
public sealed record OrderPersonalDataScope(
    IReadOnlyCollection<Guid> OrderIds,
    IReadOnlyCollection<Guid> OrderItemIds,
    IReadOnlyCollection<Guid> RefundIds);
