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
/// The outcome of one card refund request (05 4.6.2), from the refund job or the outcome query: <see cref="RefundRecordState.Succeeded"/>,
/// <see cref="RefundRecordState.Failed"/> or <see cref="RefundRecordState.OutcomePending"/> (the outcome is unknown).
/// </summary>
public sealed record CardRefundOutcome(Guid OrderId, Guid RefundRecordId, RefundRecordState Outcome);

/// <summary>
/// The card refund job's start of one request (T405; 05 4.6.1, 4.3.3; MK-40): the refund record is written in the "sending" state under the order's lock and committed
/// before the request goes. <paramref name="JobCreatedAt"/> is the job row's creation: a record written at or after it is this row's.
/// </summary>
public sealed record CardRefundRequestBegin(Guid OrderId, Guid RefundId, DateTimeOffset JobCreatedAt);

/// <summary>
/// The result of <see cref="IOrderTransitions.BeginCardRefundRequestAsync"/>: what the request sends - the order's number, the refund's amount and the record's id, its
/// reference - or <see cref="NotStarted"/> when no request may go (the refund is voided, turned to the transfer, sent, or has an open request). Never a refusal: when
/// nothing may go nothing was written, and the job row completes.
/// </summary>
public sealed class CardRefundRequestStart : IOperationResult
{
    private CardRefundRequestStart(string? orderNumber, decimal amount, Guid recordId)
    {
        OrderNumber = orderNumber;
        Amount = amount;
        RecordId = recordId;
    }

    public static CardRefundRequestStart NotStarted { get; } = new(null, 0m, Guid.Empty);

    /// <summary>The order's number; null when no request goes.</summary>
    public string? OrderNumber { get; }

    public decimal Amount { get; }

    public Guid RecordId { get; }

    public bool IsStarted => OrderNumber is not null;

    public bool IsRejected => false;

    internal static CardRefundRequestStart Started(string orderNumber, decimal amount, Guid recordId) => new(orderNumber, amount, recordId);
}

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
        Guid? latePaymentRefundId = null,
        bool isLateRefundSuccess = false)
    {
        IsLateRefundSuccess = isLateRefundSuccess;
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

    /// <summary>
    /// A card refund's success came for a request the firm had marked failed (T405; 05 4.6.2): it is kept on the record and not applied to the order. Not a refusal -
    /// the record's moment is committed.
    /// </summary>
    public bool IsLateRefundSuccess { get; }

    public bool IsRejected => IsOrderNotFound || IsNotOpen || IsVersionConflict || IsConfirmationRequired;
}
