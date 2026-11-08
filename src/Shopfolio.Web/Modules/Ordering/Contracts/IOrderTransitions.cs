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
/// The shipment axis (T316): S3/S4/S9 <see cref="CancelOrderAsync"/>, S5 <see cref="ShipOrderAsync"/>, S6 <see cref="MarkDeliveredAsync"/>,
/// S7 <see cref="MarkDeliveryFailedAsync"/>, S8 <see cref="ResendOrderAsync"/>, the reasonless S9 closing <see cref="CloseReturnedOrderAsync"/>; S1 has no
/// operation of its own - it is written with Ö1 by the approval (T318; 03 1.4.2.1; D-307) - and S2, S10 and S11 are the closure rule's, written at the end of
/// the operation that completes it (03 1.4.3).
/// The two axes' tie (T318; 03 1.4): the combination table (<c>TransitionTable.Combinations</c>), the simultaneous transitions - Ö1 with S1 or S2, the
/// digital items' delivery marks and the final decrease of the reservation; S3 with Ö2 and the freed reservation; the closure rule; the transfer mark's
/// correction moving both axes - and the counters' moments (<c>CounterMomentTable</c>).
/// The payment axis (T317): Ö1 <see cref="ApplyCardPaymentAsync"/> and <see cref="MarkTransferPaidAsync"/> - the one path of the approval, with the payment
/// record (09 24 item 4) - Ö3/Ö4/Ö5 <see cref="ApplyCardRefundOutcomeAsync"/> and <see cref="ProcessTransferRefundAsync"/>; Ö2 has no operation of its own:
/// an unpaid order's cancellation writes it with S3 in <see cref="CancelOrderAsync"/>.
/// The correction (T319): <see cref="CorrectTransitionAsync"/>, the one way back; no method undoes a transition and a withdrawal is never corrected (03 1.6.2).
/// </summary>
public interface IOrderTransitions
{
    /// <summary>
    /// Applies the provider's successful card payment (05 4.5.3: the one path of the notification and the final query). On a pending order: Ö1 by the
    /// system and, when the order carries an open physical item, S1 with it (03 1.2 S1); the reservation falls finally, the open digital items take their
    /// delivery mark and an order without an open physical item reads the closure rule (S2; 03 1.4.2.1); the payment record and B-4. On a cancelled order, and on a
    /// paid one when the transaction id is new (K-1211): the payment record without an Ö1 and the system's card refund queued in the same transaction;
    /// the axes stay. A transaction id already recorded does nothing (K-970).
    /// </summary>
    Task<PaymentTransitionResult> ApplyCardPaymentAsync(CardPaymentSucceeded payment, CancellationToken ct);

    /// <summary>
    /// The administrator's "paid" mark on a bank transfer order: Ö1 by the administrator, S1 by the system with it (D-291), the same simultaneous effects as the
    /// card's approval (03 1.4.2.1) with the closure as the administrator's event, the payment record and B-4.
    /// </summary>
    Task<PaymentTransitionResult> MarkTransferPaidAsync(TransferPaidMark mark, CancellationToken ct);

    /// <summary>
    /// Applies the outcome of a card refund request (05 4.6.2): on success Ö3, Ö4 or Ö5 by the refund's actor, B-8, and F-4 when the system refunded by
    /// itself; on failure or an unknown outcome the axis stays, no notice goes and the panel mark is read from the refund records.
    /// </summary>
    Task<PaymentTransitionResult> ApplyCardRefundOutcomeAsync(CardRefundOutcome outcome, CancellationToken ct);

    /// <summary>The administrator processes a bank transfer refund: the refund is sent, Ö3, Ö4 or Ö5 by the administrator, and B-8.</summary>
    Task<PaymentTransitionResult> ProcessTransferRefundAsync(TransferRefundProcessing processing, CancellationToken ct);

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

    /// <summary>
    /// S6, Shipped to Delivered (03 1.2): the administrator's delivery mark and delivery date, written on the open physical items; no notice goes and nothing follows by itself. The date
    /// is not in the future and not before the day of the shipment; a refused date names its reason in <see cref="OrderTransitionResult.DateObstacle"/> (02 3.20.11).
    /// </summary>
    Task<OrderTransitionResult> MarkDeliveredAsync(MarkDeliveredRequest request, CancellationToken ct);

    /// <summary>
    /// The service item's "completed" mark (T351; 02 3.20.9, 7.3.3; 03 8.2.9): the administrator delivers the item's whole open quantity. It is set only while the payment is not Pending or
    /// Failed (a partial refund does not close it), only on a service item that carries no mark and has an open quantity. It is an item mark, not a transition: the item has no state of its
    /// own, and when it was the last open item the closure rule ends the order's line in the same operation (S2, S10 or S11, the administrator's event). No notice goes.
    /// </summary>
    Task<OrderTransitionResult> MarkServiceCompletedAsync(MarkServiceCompletedRequest request, CancellationToken ct);

    /// <summary>S7, Shipped to DeliveryFailed (03 1.2): the administrator marks the returned shipment; B-6 goes to the customer, and the closure rule is read at the end (S11).</summary>
    Task<OrderTransitionResult> MarkDeliveryFailedAsync(AdminOrderRequest request, CancellationToken ct);

    /// <summary>S8, DeliveryFailed to Shipped (03 1.2; K-677): the administrator sends the open physical items again, with the irreversible-action confirmation and new carrier data; B-5 goes to the customer again.</summary>
    Task<OrderTransitionResult> ResendOrderAsync(ShipOrderRequest request, CancellationToken ct);

    /// <summary>
    /// The customer opens a defect claim on an item (03 1.9.1, 2.9; 02 7.5; T320): one claim per item, inside the channel of Z-18, with the defective units (at most the
    /// units open to a claim). B-9 goes to the customer and F-3 to the firm. The order's statuses do not change.
    /// </summary>
    Task<ItemRecordResult> OpenDefectClaimAsync(OpenDefectClaimRequest request, CancellationToken ct);

    /// <summary>The administrator marks an open defect claim resolved (03 1.9.1); no notice goes.</summary>
    Task<ItemRecordResult> ResolveDefectClaimAsync(ResolveDefectClaimRequest request, CancellationToken ct);

    /// <summary>
    /// A resolved defect claim is reopened inside Z-18 (03 1.9.1; K-681) - the same record, never a second one. The customer's reopening may raise the units and sends
    /// B-9 and F-3; the administrator's sends nothing.
    /// </summary>
    Task<ItemRecordResult> ReopenDefectClaimAsync(ReopenDefectClaimRequest request, CancellationToken ct);

    /// <summary>
    /// The administrator rejects returned goods at the receipt step (03 1.7.1.7; 02 7.4.9; K-656): only on an item frozen with a conditional exemption, at most the
    /// declaration's units still on the way back, with the irreversible-action confirmation. B-16 goes to the customer; no refund is paid.
    /// </summary>
    Task<ItemRecordResult> RejectReturnAsync(RejectReturnRequest request, CancellationToken ct);

    /// <summary>
    /// The administrator's correction of a wrong transition (T319; 03 1.6.2; 02 10.4.6; 05 4.1.4): a new transition with a reason from the closed list that
    /// sets the axis back; the wrong row and the correction both stay in the record, and B-13 goes. There is no "undo": the panel corrects by this one path.
    /// What is corrected, and on what condition: the early delivery mark S6 (the physical items' marks go, the withdrawal and defect periods wait for the new
    /// date); the firm's cancellation S3, S4, S9 of a paid order (the reservations are taken again, the unsent refund is voided, the open IBAN request closes);
    /// the wrong S5, S7 or S8 (the closure rule is read in the same operation); the transfer's "paid" mark Ö1 with its S1 (Z-8 starts again, B-3 goes once more).
    /// A refusal names the <see cref="OrderTransitionResult.Obstacle"/>.
    /// </summary>
    Task<OrderTransitionResult> CorrectTransitionAsync(CorrectTransitionRequest request, CancellationToken ct);
}
