using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Modules.Payment.Contracts;

namespace Shopfolio.Modules.Administration.Contracts;

/// <summary>
/// The pending work and mark filter of the panel's order list (T527; 07 5.3.2 <c>OrderQuery.pendingWork</c>; 04 9.8.4): one value at a time. The first five filter the scope of the home's counters of
/// the same name (02 10.6.1) - the counter and the list share one rule -; the last two are the order's marks. The names are the parameter's values (07 5.3.2).
/// </summary>
public enum PanelOrderWork
{
    AwaitingPayment,
    AwaitingShipment,
    AwaitingDeliveryMark,
    AwaitingServiceCompletion,
    AwaitingReturnOrRefund,
    IbanPendingMark,
    EmailUndeliveredMark,
}

/// <summary>The panel's order list query (T527; 07 UC-91): the search, the two status axes (none named: no filter on that axis) and at most one pending work filter, applied together.</summary>
public sealed record PanelOrderListQuery(
    string? Search,
    IReadOnlyCollection<OrderStatus> OrderStatuses,
    IReadOnlyCollection<PaymentStatus> PaymentStatuses,
    PanelOrderWork? Work);

/// <summary>One row of the list (04 9.8.5): the order's own columns and the running period with the least time left (null when none runs).</summary>
public sealed record PanelOrderListRow(PanelOrderRow Order, OrderRunningPeriod? LeastPeriod);

/// <summary>One page of the list: the rows, the page shown (a page past the last is the last), the number of pages and the number of orders of the filter.</summary>
public sealed record PanelOrderListPage(IReadOnlyList<PanelOrderListRow> Rows, int PageNumber, int PageCount, int TotalCount);

/// <summary>
/// The panel's order list (T527; 07 UC-91; 04 9.8): every order - the unpaid one the system cancelled by itself included (04 9.8.12) -, newest first. The Administration module owns the list because the pending
/// work filters are the home counters' sets (05 3.3.9): each set is read through the modules' surfaces with the counter's own rule and the Ordering module pages the orders inside it.
/// </summary>
public interface IPanelOrderList
{
    /// <summary>
    /// One page of the list; <paramref name="requestedPage"/> is clamped into the range (07 2.6.2). Newest first; under a timed pending work filter - kargoya verilecek, tamamlanmayı bekleyen hizmet,
    /// iade ve geri ödeme bekleyen, "IBAN bekleniyor" - the order with the least time left first, an overrun one at the very top, an order with no running period last (04 9.8.7). A plain read.
    /// </summary>
    Task<PanelOrderListPage> ListAsync(PanelOrderListQuery query, int requestedPage, int pageSize, CancellationToken ct);
}
