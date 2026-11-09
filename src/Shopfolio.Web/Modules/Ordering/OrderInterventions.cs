using System.Collections.Frozen;

using Shopfolio.Modules.Administration.Contracts;
using Shopfolio.Modules.Ordering.Contracts;

namespace Shopfolio.Modules.Ordering;

/// <summary>One of the administrator's nine order interventions: its number in 02 10.4.1, the order operations of 03 1.11 that carry it and the trail operations each writes.</summary>
internal sealed record OrderIntervention(int Number, IReadOnlyList<OrderOperation> Operations, IReadOnlyList<AuditOperation> Trail);

/// <summary>
/// The closed list of the administrator's interventions in an order (T375; 02 10.4.1, 7.6.6; K-369, K-370): nine, each written to the operation trail; there is no manual order creation
/// (the phone or face-to-face order goes to the storefront, whose approval, pre-information and 3-D Secure steps a manual order would skip), no way to add an item or change a price (the
/// approved amount binds), and no intervention that takes a customer's declaration back (02 7.6.7: the withdrawal is the buyer's legal statement and is neither undone nor corrected). A
/// panel operation of the order that is in no row here is not an intervention - it is a transition (03 1.6) or a customer's own operation - and an intervention that is in no row
/// does not exist. The rows only name what other code already writes; this list is the one place the nine are counted.
/// </summary>
internal static class OrderInterventions
{
    public static readonly IReadOnlyList<OrderIntervention> All =
    [
        new(1, [OrderOperation.CorrectAddress], [AuditOperation.CorrectAddress]),
        new(2, [OrderOperation.RemoveOrReduceItem], [AuditOperation.ReduceItemQuantity, AuditOperation.RemoveItem]),
        new(3, [OrderOperation.CorrectCarrier], [AuditOperation.CorrectShippingInfo]),
        new(4, [OrderOperation.CorrectDeliveryOrArrivalDate], [AuditOperation.CorrectDeliveryDate, AuditOperation.CorrectReturnArrivalDate]),
        new(5, [OrderOperation.CorrectTransition], [AuditOperation.CorrectTransition]),
        new(6, [OrderOperation.WriteInternalNote], [AuditOperation.SaveInternalNote]),
        new(7, [OrderOperation.CloseAsNotReturned], [AuditOperation.CloseNotReturned]),
        new(8, [OrderOperation.RecordWithdrawalFromOtherChannel, OrderOperation.RecordLateTerminationFromOtherChannel], [AuditOperation.RecordWithdrawalFromOtherChannel, AuditOperation.RecordLateDeliveryTerminationFromOtherChannel]),
        new(9, [OrderOperation.CorrectGuestEmail], [AuditOperation.CorrectContactEmail]),
    ];

    private static readonly FrozenSet<OrderOperation> InterventionOperations = All.SelectMany(i => i.Operations).ToFrozenSet();

    /// <summary>Whether the order operation is one of the nine interventions' (and so writes a trail row).</summary>
    public static bool IsIntervention(OrderOperation operation) => InterventionOperations.Contains(operation);
}
