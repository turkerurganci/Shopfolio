using Shopfolio.Infrastructure;
using Shopfolio.Modules.AfterSales.Contracts;
using Shopfolio.Modules.Payment.Contracts;

namespace Shopfolio.Modules.Ordering.Contracts;

/// <summary>
/// The provider's successful card payment (03 1.3 Ö1; 02 6.1.2; 05 4.5.3): the success notification and the final query's positive answer bring the
/// same record to the same method. <paramref name="ProviderTransactionId"/> is the provider's payment transaction id, unique in the payment records.
/// </summary>
public sealed record CardPaymentSucceeded(Guid OrderId, string ProviderTransactionId);

/// <summary>
/// The administrator's "paid" mark on a bank transfer order (03 1.3 Ö1; 02 5.5, 5.9): irreversible, so it is applied only with the confirmation;
/// <paramref name="ExpectedVersion"/> is the version the screen showed (05 7.3.2).
/// </summary>
public sealed record TransferPaidMark(Guid OrderId, Guid AdminId, string AdminName, int ExpectedVersion, bool IsIrreversibleConfirmed);

/// <summary>
/// The cancellation of an order whose payment never came (03 1.3 Ö2; 02 5.5): S3 and Ö2 together, whoever cancels. The customer gives no reason; the
/// administrator gives the firm's reason (the note of "Other") and the screen's version; the system gives the kind of its automatic cancellation.
/// </summary>
public sealed record UnpaidOrderCancellation(
    Guid OrderId,
    ActorKind Actor,
    Guid? AdminId,
    string? AdminName,
    CancellationReason? FirmReason,
    string? ReasonNote,
    TransitionReason? SystemReason,
    int? ExpectedVersion);

/// <summary>
/// The outcome of one card refund request (05 4.6.2), from the refund job or the outcome query: <see cref="RefundRecordState.Succeeded"/>,
/// <see cref="RefundRecordState.Failed"/> or <see cref="RefundRecordState.OutcomePending"/> (the outcome is unknown).
/// </summary>
public sealed record CardRefundOutcome(Guid OrderId, Guid RefundRecordId, RefundRecordState Outcome);

/// <summary>
/// The administrator processes a bank transfer refund (03 1.3 Ö3-Ö5; 02 5.5, 5.9): the money went to the customer's IBAN. Irreversible, so it is
/// applied only with the confirmation; <paramref name="ExpectedVersion"/> is the version the screen showed.
/// </summary>
public sealed record TransferRefundProcessing(Guid OrderId, Guid RefundId, Guid AdminId, string AdminName, int ExpectedVersion, bool IsIrreversibleConfirmed);

/// <summary>
/// The result of a payment-axis operation (09 9.1 item 3): the transitions it applied, in order, or why it applied nothing. A refusal is a value and
/// the transaction runner rolls back on it; <see cref="IsAlreadyApplied"/> is not a refusal - the same payment came again and nothing was done (K-970).
/// <see cref="LatePaymentRefundId"/> is the refund a payment that must not stay started (02 6.2.16; K-1211).
/// </summary>
public sealed class PaymentTransitionResult : IOperationResult
{
    internal PaymentTransitionResult(
        IReadOnlyList<TransitionCode> applied,
        bool isOrderNotFound = false,
        bool isNotOpen = false,
        bool isVersionConflict = false,
        bool isConfirmationRequired = false,
        bool isAlreadyApplied = false,
        Guid? latePaymentRefundId = null)
    {
        Applied = applied;
        IsOrderNotFound = isOrderNotFound;
        IsNotOpen = isNotOpen;
        IsVersionConflict = isVersionConflict;
        IsConfirmationRequired = isConfirmationRequired;
        IsAlreadyApplied = isAlreadyApplied;
        LatePaymentRefundId = latePaymentRefundId;
    }

    /// <summary>The transitions applied, in the order they were applied; empty when the operation moved no axis.</summary>
    public IReadOnlyList<TransitionCode> Applied { get; }

    public bool IsOrderNotFound { get; }

    /// <summary>The operation is not open for the order as it is now: outside the white list, the actor or the reason, or the operation's own condition.</summary>
    public bool IsNotOpen { get; }

    /// <summary>The administrator's screen showed another version of the order (05 7.3.2).</summary>
    public bool IsVersionConflict { get; }

    /// <summary>The irreversible operation came without the administrator's confirmation (02 5.9).</summary>
    public bool IsConfirmationRequired { get; }

    public bool IsAlreadyApplied { get; }

    public Guid? LatePaymentRefundId { get; }

    public bool IsRejected => IsOrderNotFound || IsNotOpen || IsVersionConflict || IsConfirmationRequired;
}
