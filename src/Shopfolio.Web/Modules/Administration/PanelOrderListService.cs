using Shopfolio.Infrastructure;
using Shopfolio.Modules.Administration.Contracts;
using Shopfolio.Modules.AfterSales.Contracts;
using Shopfolio.Modules.Notifications.Contracts;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Modules.Payment.Contracts;

namespace Shopfolio.Modules.Administration;

/// <summary>
/// The panel's order list (T527; 07 UC-91; 04 9.8; 05 3.3.9): the search and the status filters are the Ordering module's query; a pending work filter is the home counter's own set
/// (<see cref="PendingWorkOrders"/>), inside which the Ordering module filters. Without a timed filter the database pages the list newest first; under a timed one the kept orders are sorted
/// by their least remaining period here (04 9.8.7) and the page is cut from them. The rows' running periods are one read for the page (09 20 item 4).
/// </summary>
internal sealed class PanelOrderListService : IPanelOrderList
{
    /// <summary>The pending work filters whose list is ordered by the time left (04 9.8.7).</summary>
    public static readonly IReadOnlySet<PanelOrderWork> TimedFilters = new HashSet<PanelOrderWork>
    {
        PanelOrderWork.AwaitingShipment,
        PanelOrderWork.AwaitingServiceCompletion,
        PanelOrderWork.AwaitingReturnOrRefund,
        PanelOrderWork.IbanPendingMark,
    };

    private readonly IOrderingModule _ordering;
    private readonly IPanelOrderReader _reader;
    private readonly PendingWorkOrders _work;

    public PanelOrderListService(IOrderingModule ordering, IPanelOrderReader reader, IAfterSalesModule afterSales, IPaymentModule payment, INotificationsModule notifications)
    {
        ArgumentNullException.ThrowIfNull(ordering);
        ArgumentNullException.ThrowIfNull(reader);
        ArgumentNullException.ThrowIfNull(afterSales);
        ArgumentNullException.ThrowIfNull(payment);
        ArgumentNullException.ThrowIfNull(notifications);
        _ordering = ordering;
        _reader = reader;
        _work = new PendingWorkOrders(ordering, afterSales, payment, notifications);
    }

    public async Task<PanelOrderListPage> ListAsync(PanelOrderListQuery query, int requestedPage, int pageSize, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);
        ArgumentOutOfRangeException.ThrowIfLessThan(pageSize, 1);

        IReadOnlySet<Guid>? within = query.Work is { } work ? await _work.OrdersOfAsync(work, ct) : null;
        PanelOrderFilter filter = new(query.Search, query.OrderStatuses, query.PaymentStatuses, within);

        if (query.Work is not { } timed || !TimedFilters.Contains(timed))
        {
            PanelOrderPage paged = await _ordering.ListPanelOrdersAsync(filter, requestedPage, pageSize, ct);
            return new PanelOrderListPage(await WithPeriodsAsync(paged.Rows, ct), paged.PageNumber, paged.PageCount, paged.TotalCount);
        }

        IReadOnlyList<PanelOrderKey> keys = await _ordering.FindPanelOrderKeysAsync(filter, ct);
        IReadOnlyList<OrderRunningPeriod> periods = await _reader.FindRunningPeriodsAsync([.. keys.Select(k => k.OrderId)], ct);
        IReadOnlyList<Guid> ordered = OrderByTimeLeft(keys, periods);
        int pageCount = Paging.PageCountOf(ordered.Count, pageSize);
        int page = Math.Clamp(requestedPage, PageNumber.First, pageCount);
        int skip = checked((int)((long)(page - 1) * pageSize));
        Guid[] shown = [.. ordered.Skip(skip).Take(pageSize)];
        IReadOnlyList<PanelOrderRow> rows = await _ordering.ReadPanelOrderRowsAsync(shown, ct);
        return new PanelOrderListPage([.. rows.Select(r => new PanelOrderListRow(r, LeastOf(periods, r.OrderId)))], page, pageCount, ordered.Count);
    }

    /// <summary>
    /// The order of a timed filter (04 9.8.7): by the last day of the order's least remaining period - an overrun period's last day is in the past, so the overrun orders come first -; an order with
    /// no running period last; then newest first and the inner key, so the order is total.
    /// </summary>
    internal static IReadOnlyList<Guid> OrderByTimeLeft(IReadOnlyList<PanelOrderKey> keys, IReadOnlyList<OrderRunningPeriod> periods)
    {
        ArgumentNullException.ThrowIfNull(keys);
        ArgumentNullException.ThrowIfNull(periods);
        Dictionary<Guid, DateOnly> least = periods.GroupBy(p => p.OrderId).ToDictionary(g => g.Key, g => g.Min(p => p.LastDay));
        return
        [
            .. keys
                .OrderBy(k => least.ContainsKey(k.OrderId) ? 0 : 1)
                .ThenBy(k => least.TryGetValue(k.OrderId, out DateOnly day) ? day : DateOnly.MaxValue)
                .ThenByDescending(k => k.CreatedAt)
                .ThenByDescending(k => k.OrderId)
                .Select(k => k.OrderId),
        ];
    }

    /// <summary>The order's period with the least time left (04 9.8.5): the earliest last day; null when none runs.</summary>
    internal static OrderRunningPeriod? LeastOf(IReadOnlyList<OrderRunningPeriod> periods, Guid orderId) =>
        periods.Where(p => p.OrderId == orderId).OrderBy(p => p.LastDay).ThenBy(p => p.Period).FirstOrDefault();

    private async Task<IReadOnlyList<PanelOrderListRow>> WithPeriodsAsync(IReadOnlyList<PanelOrderRow> rows, CancellationToken ct)
    {
        IReadOnlyList<OrderRunningPeriod> periods = rows.Count == 0 ? [] : await _reader.FindRunningPeriodsAsync([.. rows.Select(r => r.OrderId)], ct);
        return [.. rows.Select(r => new PanelOrderListRow(r, LeastOf(periods, r.OrderId)))];
    }
}
