using Shopfolio.Infrastructure.Operations;

namespace Shopfolio.Modules.Ordering.Contracts;

/// <summary>One open operation of an order as the server decided it: the operation, the item it is open for (null for an operation on the order), and its values (open quantity, upper limit, last day).</summary>
public sealed record OrderOperationEntry(OrderOperation Operation, Guid? OrderItemId, OperationAvailability Availability);

/// <summary>The operations open to one side of an order at one moment (03 1.11): only the open ones are in the list - an operation outside its interval is not offered at all (03 1.11; T323 criterion 3).</summary>
public sealed record OrderOperationsView(Guid OrderId, ActorKind Actor, int Version, IReadOnlyList<OrderOperationEntry> Open)
{
    /// <summary>True when the operation is open for the item (or for the order when <paramref name="orderItemId"/> is null).</summary>
    public bool IsOpen(OrderOperation operation, Guid? orderItemId = null) => Open.Any(e => e.Operation == operation && e.OrderItemId == orderItemId);
}

/// <summary>
/// The one decision of the forty-one operations of 03 1.11 (T323; 09 8.3 item 1; 07 8.1.5): from the order's status, the payment status, the item's type and its open quantity the
/// decision says which operations are open now. The order page and the panel draw their buttons from <see cref="ReadAsync"/>; every POST asks <see cref="CheckAsync"/> - the same
/// decision, worked out again from the records, so a button that was hidden, stale or never drawn is refused all the same (05 MK-47, 6.2.1). There is no role model: the customer's
/// side is the buyer and the panel's side is the firm's administrator (02 10.1.2; K-912). Both methods only read; <see cref="CheckAsync"/> is called inside the writer's
/// transaction after the order's row lock (05 7.3.2), so the answer is the answer of the state the write sees.
/// </summary>
public interface IOrderOperations
{
    /// <summary>
    /// The operations open to <paramref name="actor"/> (Buyer or Admin) on the order, or null when there is no such order. The customer's view is the order page's, the
    /// administrator's is the panel's; an operation of the other side is never in the list.
    /// </summary>
    Task<OrderOperationsView?> ReadAsync(Guid orderId, ActorKind actor, CancellationToken ct);

    /// <summary>
    /// The verdict of one request: closed when the operation is not in the table, is not open now for the order (or the item), or the item is not the order's; the quantity
    /// above the open quantity is refused whole. A request for a missing order is closed.
    /// </summary>
    Task<OperationVerdict> CheckAsync(Guid orderId, OrderOperation operation, Guid? orderItemId, int? quantity, CancellationToken ct);

    /// <summary>
    /// The withdrawal declaration's screen of one item on the order page (02 7.3.5; T372), or null when the customer's withdrawal button is not open for it now - no such
    /// order or item, a digital item, a physical item not shipped yet (its way is the cancellation, 02 7.3.7), the window closed, an absolute exemption, no open units.
    /// </summary>
    Task<WithdrawalScreen?> ReadWithdrawalScreenAsync(Guid orderId, Guid orderItemId, CancellationToken ct);

    /// <summary>
    /// E-18's screen at once (T513; 07 UC-42; 09 20 item 4): the declaration screen of every item the customer's withdrawal button is open for now - in the order's item
    /// order, each as <see cref="ReadWithdrawalScreenAsync"/> reads it - and the shipping fee trigger's units still to withdraw (<see cref="WithdrawalScreens.ShippingFeeUnits"/>),
    /// from one load of the order's facts; the number of queries does not grow with the items. Null when there is no such order.
    /// </summary>
    Task<WithdrawalScreens?> ReadWithdrawalScreensAsync(Guid orderId, CancellationToken ct);

    /// <summary>
    /// The last step of E-18 (04 5.18.6; 02 7.4.6; K-789, K-1076): whether withdrawing <paramref name="selectedUnits"/> pays the shipping fee back - the AfterSales shipping fee
    /// rule's answer over <paramref name="screens"/>' units, so the page works out nothing itself (09 3.4 item 1). A pure answer.
    /// </summary>
    bool WithdrawalRefundsShippingFee(WithdrawalScreens screens, IReadOnlyDictionary<Guid, int> selectedUnits);
}
