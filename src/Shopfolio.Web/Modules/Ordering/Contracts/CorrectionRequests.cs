namespace Shopfolio.Modules.Ordering.Contracts;

/// <summary>
/// The administrator's correction of a wrong status transition (03 1.6.2; 02 10.4.6; 05 4.1.1, 4.1.4): a new transition that sets the order back to where the
/// corrected transition left it from. <paramref name="TransitionRecordId"/> is the wrong transition's record; <paramref name="Reason"/> is from the closed list
/// (06 5.3.9) and <paramref name="ReasonNote"/> is the free note of "Other" - no other reason takes one. <paramref name="ExpectedVersion"/> is the version the
/// screen showed (05 7.3.2). A correction asks no irreversible-action confirmation (04 9.9.31): it is the path that corrects, not a result that goes outside.
/// </summary>
public sealed record CorrectTransitionRequest(
    Guid OrderId,
    Guid TransitionRecordId,
    TransitionAdmin Admin,
    int ExpectedVersion,
    CorrectionReason? Reason,
    string? ReasonNote);

/// <summary>
/// Why a correction can not be made (07 6.2.7 <c>CorrectionNotPossible</c>; 02 6.7.5; K-703; 03 1.6.2): the panel shows the obstacle's message. It is the
/// <see cref="OrderTransitionResult.Obstacle"/> of a <see cref="TransitionRefusal.CorrectionNotPossible"/> refusal.
/// </summary>
public enum CorrectionObstacle
{
    /// <summary>
    /// The record has no correction: another order's, a correction itself, corrected already, its result is no longer the state, or a transition that is
    /// not corrected - S1, the closure's S2, S10, S11, the closure's or the customer's or the system's cancellation, Ö2-Ö5 (03 1.6.2).
    /// </summary>
    NotCorrectable,

    /// <summary>The card line's payment axis has no correction (03 1.6.2).</summary>
    CardPayment,

    /// <summary>The wrong cancellation is corrected only while the payment is Paid (03 1.6.2.2).</summary>
    PaymentNotPaid,

    /// <summary>The cancellation's card refund is being sent, its outcome is awaited or it succeeded: the refund may have happened at the provider (K-940).</summary>
    RefundPending,

    /// <summary>A variant has not enough left to take the cancelled units again (03 1.6.2.2; 1.4.4.7); <see cref="OrderTransitionResult.ItemIds"/> names the items.</summary>
    StockShort,

    /// <summary>
    /// The transfer's "paid" mark is corrected only while there is no shipment, no refund, no delivery mark and no item record - cancellation, removal,
    /// late delivery termination or withdrawal (03 1.6.2.4; K-703).
    /// </summary>
    ItemRecordExists,

    /// <summary>The "paid" mark of an order with a digital item opened the download and is not corrected (02 10.4.6; K-798).</summary>
    DigitalItem,
}
