namespace Shopfolio.Modules.Ordering.Contracts;

/// <summary>
/// The Ordering module's reads for the panel's order screens (T527; 04 9.8, 9.9; 07 UC-91, UC-92) that need the item records' rule as well as the order's own rows - the module surface
/// <see cref="IOrderingModule"/> is a dependency of the AfterSales module, so the reads that call back into AfterSales stand on a surface of their own. Plain reads; nothing is written or locked.
/// </summary>
public interface IPanelOrderReader
{
    /// <summary>
    /// The firm's running delivery periods of the given orders (04 9.8.5, 9.8.7, 2.6.3): Z-10 of a preparing order with a physical item whose payment is approved, Z-39 of each service item that waits
    /// to be completed and still has an open quantity. One read for all the orders, whatever their number (09 20 item 4).
    /// </summary>
    Task<IReadOnlyList<OrderRunningPeriod>> FindRunningPeriodsAsync(IReadOnlyCollection<Guid> orderIds, CancellationToken ct);
}
