using Shopfolio.Modules.Administration.Contracts;
using Shopfolio.Modules.AfterSales.Contracts;
using Shopfolio.Modules.Notifications.Contracts;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Modules.Payment.Contracts;

namespace Shopfolio.Modules.Administration;

/// <summary>
/// The order sets of the pending work (02 10.6.1; T382, T527): the one rule the home's counters count and the order list's <c>pendingWork</c> filter filters with (04 9.8.4), so a counter and the
/// list it leads to never disagree. Read through the modules' surfaces; the item sets grow with the shop, so they are read a page of orders at a time (09 20 item 2). Nothing is written or kept.
/// </summary>
internal sealed class PendingWorkOrders
{
    private const int OrdersPerPage = 200;

    private readonly IOrderingModule _ordering;
    private readonly IAfterSalesModule _afterSales;
    private readonly IPaymentModule _payment;
    private readonly INotificationsModule _notifications;

    public PendingWorkOrders(IOrderingModule ordering, IAfterSalesModule afterSales, IPaymentModule payment, INotificationsModule notifications)
    {
        _ordering = ordering;
        _afterSales = afterSales;
        _payment = payment;
        _notifications = notifications;
    }

    /// <summary>The orders of a pending work filter now.</summary>
    public async Task<IReadOnlySet<Guid>> OrdersOfAsync(PanelOrderWork work, CancellationToken ct) => work switch
    {
        PanelOrderWork.AwaitingPayment => (await _ordering.ListPendingOrderIdsAsync(PendingOrderKind.TransferPaymentApproval, ct)).ToHashSet(),
        PanelOrderWork.AwaitingShipment => await ToShipAsync(ct),
        PanelOrderWork.AwaitingDeliveryMark => await OrdersWithOpenItemsAsync(PendingWorkSet.ShippedPhysical, ct),
        PanelOrderWork.AwaitingServiceCompletion => await OrdersWithOpenItemsAsync(PendingWorkSet.ServiceUnmarked, ct),
        PanelOrderWork.AwaitingReturnOrRefund => await RefundAndReturnAsync(ct),
        PanelOrderWork.EmailUndeliveredMark => await EmailUndeliveredAsync(ct),
        PanelOrderWork.IbanPendingMark => await IbanPendingAsync(ct),
        _ => throw new ArgumentOutOfRangeException(nameof(work), work, "Not a pending work filter."),
    };

    // Kargoya verilecek: the Teslim edilemedi orders, unconditionally (K-590), and the Hazırlanıyor orders with a physical item that still has an open quantity - an item whose
    // every unit was cancelled, removed, terminated or withdrawn is closed and its order is not counted (02 10.6.1).
    private async Task<IReadOnlySet<Guid>> ToShipAsync(CancellationToken ct)
    {
        HashSet<Guid> orders = [.. await _ordering.ListPendingOrderIdsAsync(PendingOrderKind.DeliveryFailed, ct)];
        orders.UnionWith(await OrdersWithOpenItemsAsync(PendingWorkSet.PreparingPhysical, ct));
        return orders;
    }

    private async Task<IReadOnlySet<Guid>> OrdersWithOpenItemsAsync(PendingWorkSet workSet, CancellationToken ct)
    {
        HashSet<Guid> orders = [];
        Guid? after = null;
        while (true)
        {
            IReadOnlyList<PendingWorkItem> page = await _ordering.ListPendingWorkItemsAsync(workSet, after, OrdersPerPage, ct);
            if (page.Count == 0)
            {
                return orders;
            }

            IReadOnlyDictionary<Guid, int> closed = await _afterSales.ClosedQuantitiesAsync([.. page.Select(i => i.OrderItemId)], ct);
            orders.UnionWith(PanelHomeRules.OrdersWithOpenItems(page, closed));
            after = page[^1].OrderId;
        }
    }

    // The union of the three order sets (an order with two kinds of work counts once): the returned goods the firm awaits, the open refunds (a card refund that failed or awaits its result, a
    // transfer refund born and not yet sent), and the refunds the firm has yet to process (the transfer line, and the card line on a withdrawal). A customer-owed "IBAN bekleniyor" item or
    // refund is in none of them (02 10.6.1; K-525, K-621).
    private async Task<IReadOnlySet<Guid>> RefundAndReturnAsync(CancellationToken ct)
    {
        HashSet<Guid> orders = [.. await _afterSales.ListOrdersAwaitingReturnGoodsAsync(ct)];
        IReadOnlyList<OpenRefundWork> openRefunds = await _payment.ListOpenRefundWorkAsync(ct);
        IReadOnlySet<Guid> awaitingIban = await _afterSales.FindRefundsAwaitingIbanAsync([.. openRefunds.Select(r => new RefundOfItem(r.RefundId, r.OrderItemId))], ct);
        orders.UnionWith(openRefunds.Where(r => !awaitingIban.Contains(r.RefundId)).Select(r => r.OrderId));

        Guid? after = null;
        while (true)
        {
            RefundProcessingPage page = await _afterSales.ListOrdersAwaitingRefundProcessingAsync(after, OrdersPerPage, ct);
            orders.UnionWith(page.OrderIds);
            if (page.LastId is not { } last)
            {
                return orders;
            }

            after = last;
        }
    }

    // The orders with the "e-posta ulaşmadı" mark (04 2.5.3.1): the Notifications module's own rule, walked a page at a time.
    private async Task<IReadOnlySet<Guid>> EmailUndeliveredAsync(CancellationToken ct)
    {
        HashSet<Guid> orders = [];
        Guid? after = null;
        while (true)
        {
            IReadOnlyList<Guid> page = await _notifications.FindMarkedOrderIdsAsync(after, OrdersPerPage, ct);
            if (page.Count == 0)
            {
                return orders;
            }

            orders.UnionWith(page);
            after = page[^1];
        }
    }

    // The orders with the "IBAN bekleniyor" mark (04 2.5.3.4): an open IBAN request - neither fulfilled nor closed - on one of the order's items, or on the refund of a late payment to the order (K-1075).
    private async Task<IReadOnlySet<Guid>> IbanPendingAsync(CancellationToken ct)
    {
        OpenIbanRequestSubjects subjects = await _afterSales.ListOpenIbanRequestSubjectsAsync(ct);
        HashSet<Guid> orders = [.. (await _ordering.FindOrdersOfItemsAsync(subjects.OrderItemIds, ct)).Select(o => o.OrderId)];
        orders.UnionWith(await _payment.FindOrdersOfRefundsAsync(subjects.RefundIds, ct));
        return orders;
    }
}
