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

    // A line leads to its order: the item's order, or the order of the refund it names. Its item is the request's own or the one its refund is for; a request with no item at all is the refund of a late
    // payment (02 7.2.8; K-1108) and has no item name. A line whose item or order cannot be found (the record is gone) is left out.
    private async Task<IReadOnlyList<AwaitingIbanRow>> ReadAwaitingIbanAsync(CancellationToken ct)
    {
        IReadOnlyList<AwaitingIbanLine> lines = await _afterSales.ListAwaitingIbanLinesAsync(ct);
        if (lines.Count == 0)
        {
            return [];
        }

        Dictionary<Guid, OpenRefundWork> workOfRefund = lines.Any(l => l.OrderItemId is null && l.RefundId is not null)
            ? (await _payment.ListOpenRefundWorkAsync(ct)).ToDictionary(r => r.RefundId)
            : [];

        List<(AwaitingIbanLine Line, Guid? ItemId, Guid? RefundOrderId)> resolved = [];
        foreach (AwaitingIbanLine line in lines)
        {
            OpenRefundWork? work = line.RefundId is { } refund && workOfRefund.TryGetValue(refund, out OpenRefundWork? found) ? found : null;
            resolved.Add((line, line.OrderItemId ?? work?.OrderItemId, work?.OrderId));
        }

        Guid[] itemIds = [.. resolved.Select(r => r.ItemId).OfType<Guid>().Distinct()];
        Dictionary<Guid, OrderItemLabel> labels = itemIds.Length == 0
            ? []
            : (await _ordering.FindItemLabelsAsync(itemIds, ct)).ToDictionary(l => l.OrderItemId);

        Dictionary<Guid, string?> numberOfOrder = [];
        List<AwaitingIbanRow> rows = [];
        foreach ((AwaitingIbanLine line, Guid? itemId, Guid? refundOrderId) in resolved)
        {
            if (itemId is { } item)
            {
                if (labels.TryGetValue(item, out OrderItemLabel? label))
                {
                    rows.Add(new AwaitingIbanRow(label.OrderId, label.OrderNumber, PanelRequestListService.ItemNameOf(label), line.Reason, line.CreatedAt, line.RefundLastDay));
                }

                continue;
            }

            if (refundOrderId is not { } orderId)
            {
                continue;
            }

            if (!numberOfOrder.TryGetValue(orderId, out string? number))
            {
                number = await _ordering.GetOrderNumberAsync(orderId, ct);
                numberOfOrder[orderId] = number;
            }

            if (number is not null)
            {
                rows.Add(new AwaitingIbanRow(orderId, number, null, line.Reason, line.CreatedAt, line.RefundLastDay));
            }
        }

        return rows;
    }
}
