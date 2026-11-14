using Shopfolio.Modules.Administration.Contracts;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Modules.Settings.Contracts.Gates;

namespace Shopfolio.Modules.Administration;

/// <summary>
/// The panel home's pure rules (T382; 02 10.6.1, 10.8.2): the open-quantity count of an order set, the union of the refund and return work's order sets, the six counters' fixed shape and the
/// setup checklist from the sales gate. It reads nothing: <see cref="PanelHomeService"/> reads the module surfaces and hands the facts here.
/// </summary>
internal static class PanelHomeRules
{
    /// <summary>The counters' names in the order the page lists them (02 10.6.1): six, and a seventh is never opened (K-535, K-467).</summary>
    public static readonly IReadOnlyList<PanelCounterKind> CounterKinds =
    [
        PanelCounterKind.PaymentApproval,
        PanelCounterKind.ToShip,
        PanelCounterKind.DeliveryMarkPending,
        PanelCounterKind.ServicePending,
        PanelCounterKind.OpenRequest,
        PanelCounterKind.RefundAndReturnPending,
    ];

    /// <summary>The list each counter leads to (10 KP-50): its own filtered list.</summary>
    public static PanelListTarget ListOf(PanelCounterKind kind) => kind switch
    {
        PanelCounterKind.PaymentApproval => PanelListTarget.OrdersAwaitingPaymentApproval,
        PanelCounterKind.ToShip => PanelListTarget.OrdersToShip,
        PanelCounterKind.DeliveryMarkPending => PanelListTarget.OrdersAwaitingDeliveryMark,
        PanelCounterKind.ServicePending => PanelListTarget.OrdersWithServicePending,
        PanelCounterKind.OpenRequest => PanelListTarget.OpenRequests,
        PanelCounterKind.RefundAndReturnPending => PanelListTarget.RefundAndReturnWork,
        _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "Not a panel home counter."),
    };

    /// <summary>The six counters from their counts: every kind exactly once, in <see cref="CounterKinds"/>' order.</summary>
    public static IReadOnlyList<PanelCounter> Counters(IReadOnlyDictionary<PanelCounterKind, int> counts)
    {
        ArgumentNullException.ThrowIfNull(counts);
        return [.. CounterKinds.Select(k => new PanelCounter(k, counts.GetValueOrDefault(k), ListOf(k)))];
    }

    /// <summary>
    /// The orders of <paramref name="items"/> that have at least one item with an open quantity (02 10.6.1): the item's open quantity is its ordered quantity minus what its item records
    /// closed - the one rule of the closure (03 1.4.3). An order whose every item is closed - cancelled, removed, terminated or withdrawn - is not counted; a service or physical item
    /// with one open unit counts its order once.
    /// </summary>
    public static int CountOrdersWithOpenItems(IEnumerable<PendingWorkItem> items, IReadOnlyDictionary<Guid, int> closedQuantities)
    {
        ArgumentNullException.ThrowIfNull(items);
        ArgumentNullException.ThrowIfNull(closedQuantities);
        return OrdersWithOpenItems(items, closedQuantities).Count();
    }

    /// <summary>The orders of <see cref="CountOrdersWithOpenItems"/>: the same rule, as a set (the order list's filter, T527).</summary>
    public static IEnumerable<Guid> OrdersWithOpenItems(IEnumerable<PendingWorkItem> items, IReadOnlyDictionary<Guid, int> closedQuantities)
    {
        ArgumentNullException.ThrowIfNull(items);
        ArgumentNullException.ThrowIfNull(closedQuantities);
        return items.Where(i => OpenQuantity(i, closedQuantities) > 0).Select(i => i.OrderId).Distinct();
    }

    /// <summary>The open quantity of one item (the ordered quantity minus the closed one).</summary>
    public static int OpenQuantity(PendingWorkItem item, IReadOnlyDictionary<Guid, int> closedQuantities)
    {
        ArgumentNullException.ThrowIfNull(item);
        ArgumentNullException.ThrowIfNull(closedQuantities);
        return item.Quantity - closedQuantities.GetValueOrDefault(item.OrderItemId);
    }

    /// <summary>
    /// The setup checklist (02 10.8.2) from the sales gate's unmet conditions: five items, each done unless its condition is unmet; the temporary closure is the firm's own switch and is
    /// not an item. Null when every item is done - the list disappears by itself, with no wizard and no step order.
    /// </summary>
    public static IReadOnlyList<SetupChecklistItem>? SetupChecklist(IReadOnlyCollection<SalesGateCondition> unmetConditions, IReadOnlyList<string> missingIdentityFields)
    {
        ArgumentNullException.ThrowIfNull(unmetConditions);
        ArgumentNullException.ThrowIfNull(missingIdentityFields);
        IReadOnlyList<SetupChecklistItem> items =
        [
            Item(SetupChecklistItemKind.CompanyIdentity, SalesGateCondition.CompanyIdentity, unmetConditions, missingIdentityFields),
            Item(SetupChecklistItemKind.PaymentMethod, SalesGateCondition.PaymentMethod, unmetConditions, []),
            Item(SetupChecklistItemKind.PrivacyNotice, SalesGateCondition.PrivacyNotice, unmetConditions, []),
            Item(SetupChecklistItemKind.CookiePolicy, SalesGateCondition.CookiePolicy, unmetConditions, []),
            Item(SetupChecklistItemKind.ReturnAddress, SalesGateCondition.ReturnAddress, unmetConditions, []),
        ];
        return items.All(i => i.IsDone) ? null : items;
    }

    private static SetupChecklistItem Item(SetupChecklistItemKind kind, SalesGateCondition condition, IReadOnlyCollection<SalesGateCondition> unmet, IReadOnlyList<string> missing)
    {
        bool done = !unmet.Contains(condition);
        return new SetupChecklistItem(kind, done, done ? [] : missing);
    }
}
