using Shopfolio.Modules.Ordering.Contracts;

namespace Shopfolio.Modules.Payment.Contracts;

/// <summary>
/// What the payment axis needs to know of one refund (06 4.33; 02 5.5): whose order it is, its reason, amount and way, who started it, whether it is
/// sent or voided, and whether the request that carried it out was the firm's retry after a failed one (03 7.1.18: the retry's success is B-8 only).
/// The record is a copy; the refund stays the Payment module's.
/// </summary>
public sealed record RefundFacts(
    Guid RefundId,
    Guid OrderId,
    RefundType RefundType,
    decimal Amount,
    PaymentMethod PaymentMethod,
    ActorKind ActorKind,
    Guid? AdminId,
    string? AdminName,
    bool IsSent,
    bool IsVoided,
    bool IsFirmRetry);

/// <summary>
/// The two panel marks of an order's card refunds (03 1.7.3.3, 1.7.3.7; 02 5.5, 7.2.8): "geri ödeme gerçekleşmedi" while a refund that is neither sent
/// nor voided has a failed latest request, "geri ödeme sonucu bekleniyor" while one has a latest request whose outcome is pending. A mark is not a
/// state: it is read from the refund records and goes when its condition goes (K-675).
/// </summary>
public sealed record RefundMarks(bool IsNotRealized, bool IsOutcomePending);

/// <summary>One carried-out refund as the order page shows it (04 5.16.7): the moment it was sent, its amount and way, and the item and units it covered (empty when amount based).</summary>
public sealed record SentRefund(DateTimeOffset SentAt, decimal Amount, PaymentMethod Way, Guid? OrderItemId, int? Quantity);

/// <summary>
/// A refund that has started and is neither sent nor voided (T527; 04 9.9.6 "bekleyen geri ödeme"): the item and the units it covers (empty when amount based), the amount the system worked
/// out when the refund started (02 7.1.7; K-788), the way and the moment it started.
/// </summary>
public sealed record PendingRefund(Guid RefundId, Guid? OrderItemId, int? Quantity, decimal Amount, PaymentMethod Way, DateTimeOffset StartedAt);

/// <summary>The units, coupon share and VAT an item's defect claim rescissions set aside (06 4.33 <c>Quantity</c>, <c>CouponShare</c>, <c>VatAmount</c>; K-1024 (3)).</summary>
public sealed record RescindedShares(int Quantity, decimal CouponShare, decimal VatAmount);

/// <summary>
/// The refund an item record causes (T370; 06 4.33; 02 7.2.8, 7.4.5): the order's payment method, the record - a cancellation, a removal or a late delivery
/// termination -, its item and units, the amount (product amount less the coupon share, the shipping fee added when <see cref="IsShippingFeeIncluded"/>),
/// the fee flag, and whether an IBAN came with the record.
/// </summary>
public sealed record RecordRefundStart(
    Guid OrderId,
    PaymentMethod OrderPaymentMethod,
    RefundType RefundType,
    decimal Amount,
    bool IsShippingFeeIncluded,
    Guid OrderItemId,
    int Quantity,
    Guid RecordId,
    bool HasIban);

/// <summary>
/// What the refund line did with a record's refund (T370; 02 7.2.8): the refund the system started on the card - null when the refund is the firm's step -,
/// its way, and whether the customer is to be asked for an IBAN (the transfer line without an IBAN; never on the card line).
/// </summary>
public sealed record RecordRefundStarted(Guid? SystemRefundId, PaymentMethod Way, bool NeedsIbanField);

/// <summary>
/// One refund as the refund line reads it (T370; 02 7.2.8; 06 4.33, 4.34): its order and item, its way, whether it is sent or voided, and the state of its
/// latest card request - null when no request was written yet.
/// </summary>
public sealed record CardRefundLine(Guid RefundId, Guid OrderId, Guid? OrderItemId, PaymentMethod Way, bool IsSent, bool IsVoided, RefundRecordState? LatestState);

/// <summary>
/// The refund record written in the "sending" state before a card refund request goes (T405; 05 4.6.1; MK-40): the record's id - the request's reference - and the
/// refund's amount, the amount the request sends.
/// </summary>
public sealed record CardRefundSending(Guid RecordId, decimal Amount);

/// <summary>
/// A refund that touches one of the asked records (T370; 06 4.33): its item, its links to the cancellation, removal, termination, withdrawal and receipt, its
/// units and the moment it settled - sent, or voided and so never to go; null while it is pending.
/// </summary>
public sealed record RefundOfRecord(
    Guid RefundId,
    Guid? OrderItemId,
    Guid? CancellationId,
    Guid? ItemRemovalId,
    Guid? LateDeliveryTerminationId,
    Guid? WithdrawalId,
    Guid? ReturnId,
    int? Quantity,
    DateTimeOffset? SettledAt)
{
    public bool IsSettled => SettledAt is not null;
}

/// <summary>What the refund line opens on a refund (02 7.2.8; K-569, K-940): the firm's retry, the firm's transfer path, and turning the way to the transfer with the customer's IBAN.</summary>
public readonly record struct CardRefundActions(bool CanRetry, bool CanOpenTransferPath, bool CanTurnToTransfer);

/// <summary>
/// The refund of a defect claim's remedy the firm starts (02 7.5.3; 06 4.33; K-788, K-228): the order, the way (the order's payment method), the item, the amount and
/// the administrator. <paramref name="Rescission"/> makes it the rescission's refund (<see cref="RefundType.DefectClaimRescission"/>, with the claim, the units, the coupon
/// share and the VAT); null makes it the price reduction's amount-based refund of the item, which takes nothing back.
/// </summary>
public sealed record DefectRemedyRefundStart(
    Guid OrderId,
    PaymentMethod PaymentMethod,
    Guid OrderItemId,
    decimal Amount,
    Guid AdminId,
    string AdminName,
    DefectRescissionShares? Rescission);

/// <summary>What a defect claim's rescission sets aside on its refund (06 4.33 <c>DefectClaimId</c>, <c>Quantity</c>, <c>CouponShare</c>, <c>VatAmount</c>).</summary>
public sealed record DefectRescissionShares(Guid DefectClaimId, int Quantity, decimal CouponShare, decimal VatAmount);
