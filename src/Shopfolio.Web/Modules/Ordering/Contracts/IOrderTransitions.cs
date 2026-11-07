namespace Shopfolio.Modules.Ordering.Contracts;

/// <summary>
/// The open face of the transition unit (09 9.1 item 1; 05 4.1.1, MK-35): what the panel, the storefront and the background jobs call to move an order
/// between states. Every transition is a method whose name is the operation's name (a verb and a dictionary object, 09 5.3) and whose
/// <c>TransitionCode</c> constant stands in one place inside it; the correction is a method of its own that only the panel's administrator calls.
/// The core of the unit - the white list, the order of work, the record and the closure rule - is the task that wrote this interface (T315); the
/// methods arrive with the tasks that own each transition's condition (T316 the shipment axis, T317 the payment axis, T319 the correction), as methods
/// that take and give immutable records and join the caller's database transaction (09 6 item 3). No method is open before its condition is
/// written: a call that named a code and skipped the condition would open the white list's rows without their rules.
/// </summary>
public interface IOrderTransitions
{
    /// <summary>
    /// Applies the provider's successful card payment (05 4.5.3: the one path of the notification and the final query). On a pending order: Ö1 by the
    /// system and, when the order carries an open physical item, S1 with it (03 1.2 S1); the payment record and B-4. On a cancelled order, and on a
    /// paid one when the transaction id is new (K-1211): the payment record without an Ö1 and the system's card refund queued in the same transaction;
    /// the axes stay. A transaction id already recorded does nothing (K-970).
    /// </summary>
    Task<PaymentTransitionResult> ApplyCardPaymentAsync(CardPaymentSucceeded payment, CancellationToken ct);

    /// <summary>The administrator's "paid" mark on a bank transfer order: Ö1 by the administrator, S1 by the system with it (D-291), the payment record and B-4.</summary>
    Task<PaymentTransitionResult> MarkTransferPaidAsync(TransferPaidMark mark, CancellationToken ct);

    /// <summary>The cancellation of an unpaid order: S3 and Ö2 in one request, whoever cancels, and B-7. A paid order is refused: Ö2 is not open from Ödendi.</summary>
    Task<PaymentTransitionResult> CancelUnpaidOrderAsync(UnpaidOrderCancellation cancellation, CancellationToken ct);

    /// <summary>
    /// Applies the outcome of a card refund request (05 4.6.2): on success Ö3, Ö4 or Ö5 by the refund's actor, B-8, and F-4 when the system refunded by
    /// itself; on failure or an unknown outcome the axis stays, no notice goes and the panel mark is read from the refund records.
    /// </summary>
    Task<PaymentTransitionResult> ApplyCardRefundOutcomeAsync(CardRefundOutcome outcome, CancellationToken ct);

    /// <summary>The administrator processes a bank transfer refund: the refund is sent, Ö3, Ö4 or Ö5 by the administrator, and B-8.</summary>
    Task<PaymentTransitionResult> ProcessTransferRefundAsync(TransferRefundProcessing processing, CancellationToken ct);
}
