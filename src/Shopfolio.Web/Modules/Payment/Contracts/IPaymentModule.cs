namespace Shopfolio.Modules.Payment.Contracts;

/// <summary>
/// The open surface of the Payment module (09 6 item 1; 05 3.4.1): what other modules, the pages and the commands may call. The module owns the payment records and the refund records (05 3.3.4).
/// The members arrive with the tasks that own the module's rules, as methods that take and give immutable records and join the caller's
/// database transaction (09 6 item 3).
/// The members of T317 are the payment axis's records (05 4.5.3, 4.6; 06 4.32-4.34): they are called by the transition unit under the order's row
/// lock, write into the caller's open transaction and save nothing; the unit raises the order's version once for the whole operation (06 2.6.4).
/// No member makes an amount: a refund's amount is the refund's own, and the late payment's refund is the paid total the caller hands in.
/// </summary>
public interface IPaymentModule
{
    /// <summary>True when a payment with the provider's transaction id is already in the payment records (05 4.5.3: the same payment is processed once; K-970).</summary>
    Task<bool> IsPaymentRecordedAsync(string providerTransactionId, CancellationToken ct);

    /// <summary>
    /// Writes the provider's card payment to the order's payment record (06 4.32): the transaction id, and the O1 that approved it - empty on a payment
    /// that came late to a cancelled order or a second payment to a paid one (02 6.2.16; K-1211). Returns the payment record's id.
    /// </summary>
    Task<Guid> RecordCardPaymentAsync(Guid orderId, string providerTransactionId, Guid? transitionRecordId, CancellationToken ct);

    /// <summary>Writes the firm's "paid" mark on a bank transfer to the order's payment record (06 4.32): no transaction id, the O1 that carries the administrator.</summary>
    Task<Guid> RecordTransferMarkAsync(Guid orderId, Guid transitionRecordId, CancellationToken ct);

    /// <summary>
    /// Starts the system's card refund of a payment that must not stay (02 6.2.16; 05 4.5.3): the refund of the payment record with the paid total, and
    /// its card refund job row in the same transaction (05 4.3.1). Returns the refund's id.
    /// </summary>
    Task<Guid> StartLatePaymentRefundAsync(Guid orderId, Guid paymentRecordId, decimal paidTotal, CancellationToken ct);

    /// <summary>
    /// Writes the outcome of one card refund request (05 4.6.2): <see cref="RefundRecordState.Succeeded"/> - the provider carried it out, the refund is
    /// sent; <see cref="RefundRecordState.Failed"/> - refused or not sent; <see cref="RefundRecordState.OutcomePending"/> - unknown, to be asked. The
    /// record must be the order's card refund's and still open (sending or outcome pending); otherwise nothing is written and the result is null.
    /// </summary>
    Task<RefundFacts?> WriteCardRefundOutcomeAsync(Guid orderId, Guid refundRecordId, RefundRecordState outcome, CancellationToken ct);

    /// <summary>
    /// Marks the order's bank transfer refund as sent: the administrator processed it (02 5.5; 03 1.3 Ö3-Ö5). The refund must be the order's, on the
    /// transfer way, neither sent nor voided; otherwise nothing is written and the result is null.
    /// </summary>
    Task<RefundFacts?> MarkTransferRefundSentAsync(Guid orderId, Guid refundId, CancellationToken ct);

    /// <summary>
    /// The total of the order's sent refunds that pay the order's money back - voided ones and the late payment's refund left out, which pays back a
    /// payment the order never took (02 6.2.16) - the changes of the open transaction included.
    /// </summary>
    Task<decimal> SentRefundTotalAsync(Guid orderId, CancellationToken ct);

    /// <summary>The panel marks of the order's card refunds (02 7.2.8; 05 4.6.2), the changes of the open transaction included.</summary>
    Task<RefundMarks> ReadRefundMarksAsync(Guid orderId, CancellationToken ct);

    /// <summary>
    /// The payment records of the given orders, as the values a caller needs to tell whether an order's payment was taken (06 4.32; 06 9.1 Z-28, Z-41, Z-43): the
    /// order and the transition (Ö1) that confirmed the payment, or null on a payment that came late to a cancelled order. A plain read; the caller reads the
    /// "corrected" mark from the transition records it owns - the payment record itself carries no such mark.
    /// </summary>
    Task<IReadOnlyList<PaymentSource>> FindPaymentSourcesAsync(IReadOnlyCollection<Guid> orderIds, CancellationToken ct);

    /// <summary>
    /// The card payments that came late and were paid back by the system (T384; 02 10.7.1, 6.2.16; K-589; 06 4.32, 4.33): for each of the given orders, the first payment record that
    /// carries no confirming transition (a payment to a cancelled order, or a second one to a paid order) with the date of the payment and of its refund of the type
    /// <see cref="RefundType.LatePayment"/> and the refund's amount; a refund the system voided does not count and an order with none is not in the result. The refund's date is null
    /// until it was sent. The order export writes the four values to its late payment columns. A plain read.
    /// </summary>
    Task<IReadOnlyList<LatePaymentFact>> ListLatePaymentsAsync(IReadOnlyCollection<Guid> orderIds, CancellationToken ct);

    /// <summary>
    /// True when the order has a refund that is not voided - started, sent or awaiting its outcome - the open transaction's changes included. The transfer's
    /// "paid" mark is not corrected after a refund (03 1.6.2.4; K-703).
    /// </summary>
    Task<bool> HasRefundAsync(Guid orderId, CancellationToken ct);

    /// <summary>
    /// True when a refund of the given cancellations that is not voided was sent, or has a card request that is being sent, whose outcome is awaited or that
    /// succeeded (06 4.22, 4.34; K-940): the money may have left, so the wrong cancellation is not corrected. A plain read under the caller's order lock.
    /// </summary>
    Task<bool> IsCancellationRefundUnderwayAsync(IReadOnlyCollection<Guid> cancellationIds, CancellationToken ct);

    /// <summary>
    /// The refund subjects among <paramref name="subjectIds"/> - cancellation, late delivery termination or goods' receipt ids - that have a refund that was
    /// sent or voided (06 4.33): their refund no longer uses an IBAN (03 4.2.8; K-1127). A plain read under the caller's order lock.
    /// </summary>
    Task<IReadOnlySet<Guid>> FindSettledRefundSubjectsAsync(IReadOnlyCollection<Guid> subjectIds, CancellationToken ct);

    /// <summary>
    /// The correction of the firm's wrong cancellation (06 4.22, 4.33; 02 10.4.6; K-1056): the cancellations' refunds that are neither sent nor voided are voided
    /// now, and a voided refund is never sent (the card refund handler reads the mark - T405; D-518). Writes through the caller's change tracker and saves
    /// nothing; returns the voided ids.
    /// </summary>
    Task<IReadOnlyList<Guid>> VoidCancellationRefundsAsync(IReadOnlyCollection<Guid> cancellationIds, CancellationToken ct);

    /// <summary>
    /// The order's disposal (06 9.2.1; K-1059): empties the administrator link and name of every refund of the given orders and returns the ids of all their
    /// refunds. It writes through the caller's change tracker - the caller's transaction and order row lock hold, and the caller raises the order's version and
    /// saves. The payment records, the refund records and the inbound notification records are not touched (06 9.2.2).
    /// </summary>
    Task<IReadOnlyList<Guid>> ErasePersonalDataAsync(IReadOnlyCollection<Guid> orderIds, CancellationToken ct);

    /// <summary>
    /// The number of open card refunds, for the server's status command (T389; 05 8.5.6; 08 2.1.2): the card refunds that are neither sent nor voided and whose latest request did not fail -
    /// the request is still to be sent (the job waits in the queue), is being sent, or its outcome is unknown. A refund whose latest request failed is the firm's to retry or to move to the
    /// bank transfer; it is not a running card job. A plain read; it counts and writes nothing.
    /// </summary>
    Task<int> CountOpenCardRefundsAsync(CancellationToken ct);
}

/// <summary>One payment record seen from another module (06 4.32): the order, and the transition that confirmed the payment - null on a late card payment.</summary>
public sealed record PaymentSource(Guid OrderId, Guid? TransitionRecordId);

/// <summary>A card payment that came late and was paid back by the system (T384; 02 10.7.1): the order, the moment of the payment, the moment the refund was sent (null until then) and the refund's amount.</summary>
public sealed record LatePaymentFact(Guid OrderId, DateTimeOffset PaidAt, DateTimeOffset? RefundSentAt, decimal RefundAmount);
