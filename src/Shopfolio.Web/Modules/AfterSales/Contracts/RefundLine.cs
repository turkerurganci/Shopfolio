namespace Shopfolio.Modules.AfterSales.Contracts;

/// <summary>
/// An IBAN request the refund line opens (T370; 06 4.29; 02 7.2.8, 7.4.5): on an item, or - on the refund of a late payment to a cancelled order - on the
/// refund alone; the refund is named on a failed card refund's transfer path. The administrator's step gives birth to it.
/// </summary>
public sealed record RefundIbanRequestEntry(
    Guid? OrderItemId,
    Guid? RefundId,
    RefundIbanRequestReason Reason,
    Guid AdminId,
    string AdminName,
    DateTimeOffset At);

/// <summary>The request the item or the refund now has open, and whether this call opened it (a second event uses the open request - no second B-14).</summary>
public sealed record RefundIbanRequestOpened(Guid RequestId, bool IsNew);

/// <summary>An open IBAN request as the order module checks it before the customer's entry: its item or refund and its reason.</summary>
public sealed record OpenRefundIbanRequest(Guid RequestId, Guid? OrderItemId, Guid? RefundId, RefundIbanRequestReason Reason);

/// <summary>
/// One IBAN field of the order page (T512; 04 5.16.12): an open request to answer (<see cref="RequestId"/>, with its reason) or an IBAN on file to correct
/// (<see cref="RefundIbanId"/>, with the value). <see cref="OrderItemId"/> is the item whose row draws it - null on the late payment's itemless refund (K-1075)
/// and on an IBAN whose records name no item. <see cref="RefundId"/> is the refund the request names - on an IBAN on file, the refund whose fulfilled request brought it.
/// </summary>
public sealed record RefundIbanField(Guid? OrderItemId, Guid? RequestId, Guid? RefundId, RefundIbanRequestReason? Reason, Guid? RefundIbanId, string? Iban);

/// <summary>The customer's IBAN entered on the order page for an open request (02 7.4.5; 03 1.11.33): the order, the request and the text as typed.</summary>
public sealed record RefundIbanEntry(Guid OrderId, Guid RequestId, string Iban, DateTimeOffset At);

/// <summary>The customer's correction of an IBAN in place, until the refund is processed (02 7.4.5; K-575).</summary>
public sealed record RefundIbanCorrection(Guid OrderId, Guid RefundIbanId, string Iban, DateTimeOffset At);

/// <summary>
/// The outcome of the refund line's IBAN write: the IBAN written, or - with no id - the refusal: <see cref="IsInvalidIban"/> when the text is not a Turkish IBAN
/// with valid check digits (04 7.3.2.7; the one rule of <c>InvalidIbanAttribute</c>), otherwise no such IBAN or open request on the order.
/// </summary>
public sealed record RefundIbanWriteResult(Guid? RefundIbanId, bool IsInvalidIban)
{
    public static RefundIbanWriteResult InvalidIban { get; } = new(null, true);

    public static RefundIbanWriteResult NotFound { get; } = new(null, false);
}

/// <summary>The firm's "havale gerçekleşmedi" on an item, or on the itemless refund of a late payment (02 7.4.5; K-575, K-1131).</summary>
public sealed record TransferFailure(Guid OrderId, Guid? OrderItemId, Guid? RefundId, Guid AdminId, string AdminName, DateTimeOffset At);
