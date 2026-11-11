using Shopfolio.Modules.Administration.Contracts;
using Shopfolio.Pages.Frame;

namespace Shopfolio.Pages.Components;

/// <summary>One counter as the panel home draws it: its name, its count - zero is written - and the filtered list it leads to (04 2.6.1).</summary>
public sealed record PendingCounterItem(PanelCounterKind Kind, string Name, int Count, string ListPath);

/// <summary>
/// The pending work counters of the panel home (T542; 04 2.6.1, 3.4.2; 02 10.6.1; K-750): six, in the document's order and under its names; a zero counter is drawn and says zero; every counter
/// leads to its own filtered list (07 5.3.2: the first five by <c>OrderQuery.pendingWork</c>, the open request by <c>RequestQuery.requestStatus=Open</c>). There is no seventh counter and the
/// menu does not repeat the number (04 2.2.8). The "IBAN bekleniyor" list is not a counter (<see cref="AwaitingCustomerList"/>).
/// </summary>
internal static class PendingCounters
{
    /// <summary>The counters' names, as 02 10.6.1 writes them.</summary>
    public static string NameOf(PanelCounterKind kind) => kind switch
    {
        PanelCounterKind.PaymentApproval => "Ödeme onayı bekleyen",
        PanelCounterKind.ToShip => "Kargoya verilecek",
        PanelCounterKind.DeliveryMarkPending => "Teslim işareti bekleyen",
        PanelCounterKind.ServicePending => "Tamamlanmayı bekleyen hizmet",
        PanelCounterKind.OpenRequest => "Açık talep",
        PanelCounterKind.RefundAndReturnPending => "İade ve geri ödeme bekleyen",
        _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "Not a panel home counter."),
    };

    /// <summary>The filtered list a counter's target is (07 5.3.2): the order list's <c>pendingWork</c> value, or the request list's open filter.</summary>
    public static string ListPathOf(PanelListTarget target) => target switch
    {
        PanelListTarget.OrdersAwaitingPaymentApproval => OrdersWith("AwaitingPayment"),
        PanelListTarget.OrdersToShip => OrdersWith("AwaitingShipment"),
        PanelListTarget.OrdersAwaitingDeliveryMark => OrdersWith("AwaitingDeliveryMark"),
        PanelListTarget.OrdersWithServicePending => OrdersWith("AwaitingServiceCompletion"),
        PanelListTarget.OpenRequests => PanelFramePaths.Requests + "?requestStatus=Open",
        PanelListTarget.RefundAndReturnWork => OrdersWith("AwaitingReturnOrRefund"),
        _ => throw new ArgumentOutOfRangeException(nameof(target), target, "Not a panel list target."),
    };

    /// <summary>The order list filtered to the orders with the IBAN mark: where the whole "IBAN bekleniyor" list is reached from (04 2.6.2).</summary>
    public static string IbanPendingListPath => OrdersWith("IbanPendingMark");

    /// <summary>
    /// The six counters of the home: the home's own counters, each with its name and list. The home gives exactly six, one per kind (<see cref="IPanelHome"/>); anything else is a programming
    /// error and throws - a seventh counter is never opened (02 10.6.1; K-535, K-467).
    /// </summary>
    public static IReadOnlyList<PendingCounterItem> Of(IReadOnlyList<PanelCounter> counters)
    {
        ArgumentNullException.ThrowIfNull(counters);
        PanelCounterKind[] kinds = Enum.GetValues<PanelCounterKind>();
        if (counters.Count != kinds.Length || kinds.Any(k => counters.Count(c => c.Kind == k) != 1))
        {
            throw new ArgumentException("The pending work block has exactly six counters, one of each kind (02 10.6.1).", nameof(counters));
        }

        return [.. kinds.Select(k => counters.Single(c => c.Kind == k)).Select(c => new PendingCounterItem(c.Kind, NameOf(c.Kind), c.Count, ListPathOf(c.ListTarget)))];
    }

    private static string OrdersWith(string pendingWork) => PanelFramePaths.Orders + "?pendingWork=" + pendingWork;
}
