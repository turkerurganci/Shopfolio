using Shopfolio.Modules.Administration.Contracts;
using Shopfolio.Modules.AfterSales.Contracts;
using Shopfolio.Modules.Notifications.Contracts;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Modules.Payment.Contracts;

namespace Shopfolio.Modules.Administration;

/// <summary>
/// The panel home's own extra reads (T521; 04 2.6.2, 3.4.3; 02 10.6.1): the order counts behind the e-mail and card refund warnings and the "IBAN bekleniyor" lines. Like <see cref="PanelHomeService"/> it
/// reads only through the modules' surfaces, has no table and stores nothing; the marked orders are read a page at a time (09 20 item 2).
/// </summary>
internal sealed class PanelHomeDetailsService : IPanelHomeDetails
{
    private const int OrdersPerPage = 200;

    private readonly INotificationsModule _notifications;
    private readonly IPaymentModule _payment;
    private readonly IAfterSalesModule _afterSales;
    private readonly IOrderingModule _ordering;

    public PanelHomeDetailsService(INotificationsModule notifications, IPaymentModule payment, IAfterSalesModule afterSales, IOrderingModule ordering)
    {
        ArgumentNullException.ThrowIfNull(notifications);
        ArgumentNullException.ThrowIfNull(payment);
        ArgumentNullException.ThrowIfNull(afterSales);
        ArgumentNullException.ThrowIfNull(ordering);
        _notifications = notifications;
        _payment = payment;
        _afterSales = afterSales;
        _ordering = ordering;
    }

    public async Task<PanelHomeDetails> GetAsync(CancellationToken ct)
    {
        int notDelivered = await CountMarkedOrdersAsync(ct);
        CardRefundProblems card = await _payment.CountCardRefundProblemsAsync(ct);
        IReadOnlyList<AwaitingIbanRow> awaiting = await ReadAwaitingIbanAsync(ct);
        return new PanelHomeDetails(notDelivered, card.FailedOrders, card.OutcomePendingOrders, awaiting);
    }

    private async Task<int> CountMarkedOrdersAsync(CancellationToken ct)
    {
        int total = 0;
        Guid? after = null;
        while (true)
        {
            IReadOnlyList<Guid> page = await _notifications.FindMarkedOrderIdsAsync(after, OrdersPerPage, ct);
            if (page.Count == 0)
            {
                return total;
            }

            total += page.Count;
            after = page[^1];
        }
    }

    // A line leads to its order: the item's order, or the order of the refund an itemless request names. A line whose order cannot be found (the record is gone) is left out.
    private async Task<IReadOnlyList<AwaitingIbanRow>> ReadAwaitingIbanAsync(CancellationToken ct)
    {
        IReadOnlyList<AwaitingIbanLine> lines = await _afterSales.ListAwaitingIbanLinesAsync(ct);
        if (lines.Count == 0)
        {
            return [];
        }

        IReadOnlyList<OrderOfItem> ofItems = await _ordering.FindOrdersOfItemsAsync([.. lines.Where(l => l.OrderItemId is not null).Select(l => l.OrderItemId!.Value).Distinct()], ct);
        Dictionary<Guid, Guid> orderOfItem = ofItems.ToDictionary(o => o.OrderItemId, o => o.OrderId);
        Dictionary<Guid, Guid> orderOfRefund = (await _payment.ListOpenRefundWorkAsync(ct)).ToDictionary(r => r.RefundId, r => r.OrderId);

        List<(Guid OrderId, DateOnly? LastDay)> resolved = [];
        foreach (AwaitingIbanLine line in lines)
        {
            Guid? orderId = line.OrderItemId is { } item && orderOfItem.TryGetValue(item, out Guid byItem) ? byItem
                : line.RefundId is { } refund && orderOfRefund.TryGetValue(refund, out Guid byRefund) ? byRefund
                : null;
            if (orderId is { } found)
            {
                resolved.Add((found, line.RefundLastDay));
            }
        }

        Dictionary<Guid, string> numbers = [];
        foreach (Guid orderId in resolved.Select(r => r.OrderId).Distinct())
        {
            if (await _ordering.GetOrderNumberAsync(orderId, ct) is { } number)
            {
                numbers[orderId] = number;
            }
        }

        return [.. resolved.Where(r => numbers.ContainsKey(r.OrderId)).Select(r => new AwaitingIbanRow(r.OrderId, numbers[r.OrderId], r.LastDay))];
    }
}
