namespace Shopfolio.Modules.Payment.Contracts;

/// <summary>
/// The firm's refund step on the panel (T529; 07 UC-131, UC-133; 06 4.33): the refund row is born with the step. The type and its links - the record that causes it (<see cref="RecordId"/>: the
/// cancellation, the removal, the late delivery termination or the withdrawal; with <see cref="ReturnId"/> the withdrawal's goods receipt), or on the amount based refund the item and the "mal
/// dönmedi" closure it is bound to (K-1105) -, the amount the server worked out, the fee flag, the units, the order's payment method and the administrator.
/// </summary>
public sealed record FirmRefundStart(
    Guid OrderId,
    PaymentMethod OrderPaymentMethod,
    RefundType RefundType,
    decimal Amount,
    bool IsShippingFeeIncluded,
    Guid? OrderItemId,
    int? Quantity,
    Guid? RecordId,
    Guid? ReturnId,
    Guid? NotReturnedClosureId,
    Guid AdminId,
    string AdminName);

/// <summary>
/// One refund of the order as the panel's refund lines read it (T529; 04 9.9.6, 9.9.8; 06 4.33, 4.34): the links, the way, the amount, whether it was sent or voided, the latest card request's state,
/// the administrator's "gerçekleşmedi" mark and the success that came after it (K-978).
/// </summary>
public sealed record PanelRefundFacts(
    Guid RefundId,
    RefundType RefundType,
    PaymentMethod Way,
    decimal Amount,
    bool IsShippingFeeIncluded,
    Guid? OrderItemId,
    int? Quantity,
    DateTimeOffset CreatedAt,
    bool IsSent,
    bool IsVoided,
    RefundRecordState? LatestState,
    bool IsMarkedFailedByAdmin,
    DateTimeOffset? LateSuccessAt)
{
    public Guid? CancellationId { get; init; }

    public Guid? ItemRemovalId { get; init; }

    public Guid? LateDeliveryTerminationId { get; init; }

    public Guid? WithdrawalId { get; init; }

    public Guid? ReturnId { get; init; }

    public Guid? DefectClaimId { get; init; }

    public Guid? NotReturnedClosureId { get; init; }

    public Guid? PaymentRecordId { get; init; }

    /// <summary>Whether <paramref name="causeId"/> is one of the refund's seven cause links (07 UC-131: exactly one of them is the key the form carries).</summary>
    public bool IsCausedBy(Guid causeId) =>
        causeId == CancellationId || causeId == ItemRemovalId || causeId == LateDeliveryTerminationId || causeId == DefectClaimId || causeId == PaymentRecordId
        || causeId == ReturnId || (ReturnId is null && causeId == WithdrawalId);
}
