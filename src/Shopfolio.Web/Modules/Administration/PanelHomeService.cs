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
/// database context and no request: the six counters, the two warnings and the setup checklist are computed again at every call and nothing is stored (K-405, K-467). The order sets
/// of the counters are <see cref="PendingWorkOrders"/>' - the order list's pending work filter reads the same sets (T527).
/// </summary>
internal sealed class PanelHomeService : IPanelHome
{
    private readonly IOrderingModule _ordering;
    private readonly IAfterSalesModule _afterSales;
    private readonly ICorporateContentModule _content;
    private readonly IPanelWarnings _warnings;
    private readonly ISettingsModule _settings;
    private readonly PendingWorkOrders _work;

    public PanelHomeService(
        IOrderingModule ordering,
        IAfterSalesModule afterSales,
        IPaymentModule payment,
        ICorporateContentModule content,
        IPanelWarnings warnings,
        ISettingsModule settings,
        INotificationsModule notifications)
    {
        ArgumentNullException.ThrowIfNull(ordering);
        ArgumentNullException.ThrowIfNull(afterSales);
        ArgumentNullException.ThrowIfNull(payment);
        ArgumentNullException.ThrowIfNull(content);
        ArgumentNullException.ThrowIfNull(warnings);
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(notifications);
        _ordering = ordering;
        _afterSales = afterSales;
        _content = content;
        _warnings = warnings;
        _settings = settings;
        _work = new PendingWorkOrders(ordering, afterSales, payment, notifications);
    }

    public async Task<PanelHome> GetAsync(CancellationToken ct)
    {
        PendingOrderCounts orderCounts = await _ordering.CountPendingOrdersAsync(ct);
        int toShip = (await _work.OrdersOfAsync(PanelOrderWork.AwaitingShipment, ct)).Count;
        int shipped = (await _work.OrdersOfAsync(PanelOrderWork.AwaitingDeliveryMark, ct)).Count;
        int services = (await _work.OrdersOfAsync(PanelOrderWork.AwaitingServiceCompletion, ct)).Count;

        int contactRequests = await _content.CountOpenContactRequestsAsync(ct);
        int defectClaims = await _afterSales.CountOpenDefectClaimsAsync(ct);
        int refundAndReturn = (await _work.OrdersOfAsync(PanelOrderWork.AwaitingReturnOrRefund, ct)).Count;

        IReadOnlyDictionary<PanelCounterKind, int> counts = new Dictionary<PanelCounterKind, int>
        {
            [PanelCounterKind.PaymentApproval] = orderCounts.PaymentApproval,
            [PanelCounterKind.ToShip] = toShip,
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
}
