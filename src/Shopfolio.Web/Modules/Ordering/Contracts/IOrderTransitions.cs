namespace Shopfolio.Modules.Ordering.Contracts;

/// <summary>
/// The open face of the transition unit (09 9.1 item 1; 05 4.1.1, MK-35): what the panel, the storefront and the background jobs call to move an order
/// between states. Every transition is a method whose name is the operation's name (a verb and a dictionary object, 09 5.3) and whose
/// <c>TransitionCode</c> constant stands in one place inside it; the correction is a method of its own that only the panel's administrator calls.
/// The core of the unit - the white list, the order of work, the record and the closure rule - is the task that wrote this interface (T315); the
/// methods arrive with the tasks that own each transition's condition (T316 the shipment axis, T317 the payment axis, T319 the correction), as methods
/// that take and give immutable records and join the caller's database transaction (09 6 item 3): none of them opens a transaction, takes a caller's
/// lock before its own or calls an outside service. No method is open before its condition is written: a call that named a code and skipped the
/// condition would open the white list's rows without their rules.
/// The shipment axis (T316): S1 <see cref="StartPreparationAsync"/>, S3/S4/S9 <see cref="CancelOrderAsync"/>, S5 <see cref="ShipOrderAsync"/>,
/// S6 <see cref="MarkDeliveredAsync"/>, S7 <see cref="MarkDeliveryFailedAsync"/>, S8 <see cref="ResendOrderAsync"/>, the reasonless S9 closing
/// <see cref="CloseReturnedOrderAsync"/>; S2, S10 and S11 have no operation of their own - they are the closure rule's, written at the end of the operation
/// that completes it (03 1.4.3).
/// </summary>
public interface IOrderTransitions
{
    /// <summary>
    /// S1, Placed to Preparing (03 1.2; 02 5.4): the system, together with the payment's approval and only on an order that carries at least one physical item.
    /// With <paramref name="approvesPayment"/> the call writes the card approval Ö1 and S1 in one lock, one version raise and one B-4 row - B-4 is Ö1's notice and
    /// S1 produces none; without it the payment is already Paid (a bank transfer marked by the administrator, whose Ö1 wrote the B-4) and the call writes S1 alone.
    /// </summary>
    Task<OrderTransitionResult> StartPreparationAsync(Guid orderId, bool approvesPayment, CancellationToken ct);

    /// <summary>
    /// S3, S4 or S9 (03 1.2; 02 5.4): the order's cancellation by the customer, the administrator with a reason, or the system; B-7 goes to the customer. An unpaid
    /// order's cancellation also writes Ö2. A refusal writes nothing.
    /// </summary>
    Task<OrderTransitionResult> CancelOrderAsync(CancelOrderRequest request, CancellationToken ct);

    /// <summary>
    /// S9 as a closure (03 1.2, 1.4.3; 02 5.4 S9): after S7 the administrator closes a returned order whose open items are all gone and none of which was delivered.
    /// No reason is chosen, no B-7 goes - the items' own records are the reason.
    /// </summary>
    Task<OrderTransitionResult> CloseReturnedOrderAsync(AdminOrderRequest request, CancellationToken ct);

    /// <summary>S5, Preparing to Shipped (03 1.2): the administrator ships the open physical items, with the irreversible-action confirmation and the carrier data; B-5 goes to the customer.</summary>
    Task<OrderTransitionResult> ShipOrderAsync(ShipOrderRequest request, CancellationToken ct);

    /// <summary>S6, Shipped to Delivered (03 1.2): the administrator's delivery mark and delivery date, written on the open physical items; no notice goes and nothing follows by itself.</summary>
    Task<OrderTransitionResult> MarkDeliveredAsync(MarkDeliveredRequest request, CancellationToken ct);

    /// <summary>S7, Shipped to DeliveryFailed (03 1.2): the administrator marks the returned shipment; B-6 goes to the customer, and the closure rule is read at the end (S11).</summary>
    Task<OrderTransitionResult> MarkDeliveryFailedAsync(AdminOrderRequest request, CancellationToken ct);

    /// <summary>S8, DeliveryFailed to Shipped (03 1.2; K-677): the administrator sends the open physical items again, with the irreversible-action confirmation and new carrier data; B-5 goes to the customer again.</summary>
    Task<OrderTransitionResult> ResendOrderAsync(ShipOrderRequest request, CancellationToken ct);
}
