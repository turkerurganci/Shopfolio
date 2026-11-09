using Shopfolio.Modules.Administration.Contracts;
using Shopfolio.Modules.AfterSales.Contracts;
using Shopfolio.Modules.CorporateContent.Contracts;
using Shopfolio.Modules.Notifications.Contracts;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Modules.Payment.Contracts;
using Shopfolio.Modules.Settings.Contracts;

namespace Shopfolio.Modules.Administration;

/// <summary>
/// The panel home (02 10.6.1, 10.8.2; 11 T382; KP-50, KP-65): the Administration module owns it (05 3.3.9) and reads the records only through the modules' surfaces. It has no table, no
/// database context and no request: the six counters, the two warnings and the setup checklist are computed again at every call and nothing is stored (K-405, K-467). The item sets grow with
/// the shop, so they are read a page of orders at a time (09 20 item 2).
/// </summary>
internal sealed class PanelHomeService : IPanelHome
{
    private const int OrdersPerPage = 200;

    private readonly IOrderingModule _ordering;
    private readonly IAfterSalesModule _afterSales;
    private readonly IPaymentModule _payment;
    private readonly ICorporateContentModule _content;
    private readonly IPanelWarnings _warnings;
    private readonly ISettingsModule _settings;

    public PanelHomeService(
        IOrderingModule ordering,
        IAfterSalesModule afterSales,
        IPaymentModule payment,
        ICorporateContentModule content,
        IPanelWarnings warnings,
        ISettingsModule settings)
    {
        ArgumentNullException.ThrowIfNull(ordering);
        ArgumentNullException.ThrowIfNull(afterSales);
        ArgumentNullException.ThrowIfNull(payment);
        ArgumentNullException.ThrowIfNull(content);
        ArgumentNullException.ThrowIfNull(warnings);
        ArgumentNullException.ThrowIfNull(settings);
        _ordering = ordering;
        _afterSales = afterSales;
        _payment = payment;
        _content = content;
        _warnings = warnings;
        _settings = settings;
    }

    public async Task<PanelHome> GetAsync(CancellationToken ct)
    {
        PendingOrderCounts orderCounts = await _ordering.CountPendingOrdersAsync(ct);

        // Kargoya verilecek: the Teslim edilemedi orders, unconditionally (K-590), and the Hazırlanıyor orders with a physical item that still has an open quantity - an item whose
        // every unit was cancelled, removed, terminated or withdrawn is closed and its order is not counted (02 10.6.1).
        int preparing = await CountOrdersWithOpenItemsAsync(PendingWorkSet.PreparingPhysical, ct);
        int shipped = await CountOrdersWithOpenItemsAsync(PendingWorkSet.ShippedPhysical, ct);
        int services = await CountOrdersWithOpenItemsAsync(PendingWorkSet.ServiceUnmarked, ct);

        int contactRequests = await _content.CountOpenContactRequestsAsync(ct);
        int defectClaims = await _afterSales.CountOpenDefectClaimsAsync(ct);
        int refundAndReturn = await CountRefundAndReturnOrdersAsync(ct);

        IReadOnlyDictionary<PanelCounterKind, int> counts = new Dictionary<PanelCounterKind, int>
        {
            [PanelCounterKind.PaymentApproval] = orderCounts.PaymentApproval,
            [PanelCounterKind.ToShip] = orderCounts.DeliveryFailed + preparing,
            [PanelCounterKind.DeliveryMarkPending] = shipped,
            [PanelCounterKind.ServicePending] = services,
            [PanelCounterKind.OpenRequest] = contactRequests + defectClaims,
            [PanelCounterKind.RefundAndReturnPending] = refundAndReturn,
        };

        CompanyIdentity identity = await _settings.GetCompanyIdentityAsync(ct);
        PanelWarnings warnings = await _warnings.ReadAsync(identity.Email, ct);
        SalesGate gate = await _settings.GetSalesGateAsync(ct);

        return new PanelHome(
            PanelHomeRules.Counters(counts),
            warnings.ChannelWarningOpen,
            warnings.FirmNoticesWarningOpen,
            PanelHomeRules.SetupChecklist(gate.UnmetConditions, gate.MissingIdentityFields));
    }

    private async Task<int> CountOrdersWithOpenItemsAsync(PendingWorkSet workSet, CancellationToken ct)
    {
        int total = 0;
        Guid? after = null;
        while (true)
        {
            IReadOnlyList<PendingWorkItem> page = await _ordering.ListPendingWorkItemsAsync(workSet, after, OrdersPerPage, ct);
            if (page.Count == 0)
            {
                return total;
            }

            IReadOnlyDictionary<Guid, int> closed = await _afterSales.ClosedQuantitiesAsync([.. page.Select(i => i.OrderItemId)], ct);
            total += PanelHomeRules.CountOrdersWithOpenItems(page, closed);
            after = page[^1].OrderId;
        }
    }

    // The union of the three order sets (an order with two kinds of work counts once): the returned goods the firm awaits, the card refunds that failed or await their result, and the refunds
    // the firm has yet to process (the transfer line, and the card line on a withdrawal). A customer-owed "IBAN bekleniyor" item is in none of them (02 10.6.1; K-621).
    private async Task<int> CountRefundAndReturnOrdersAsync(CancellationToken ct)
    {
        HashSet<Guid> orders = [.. await _afterSales.ListOrdersAwaitingReturnGoodsAsync(ct)];
        orders.UnionWith(await _payment.ListOrdersWithOpenRefundWorkAsync(ct));

        Guid? after = null;
        while (true)
        {
            RefundProcessingPage page = await _afterSales.ListOrdersAwaitingRefundProcessingAsync(after, OrdersPerPage, ct);
            orders.UnionWith(page.OrderIds);
            if (page.LastId is not { } last)
            {
                return orders.Count;
            }

            after = last;
        }
    }
}
