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

/// <summary>The customer's IBAN entered on the order page for an open request (02 7.4.5; 03 1.11.33): the order, the request and the text as typed.</summary>
public sealed record RefundIbanEntry(Guid OrderId, Guid RequestId, string Iban, DateTimeOffset At);

/// <summary>The customer's correction of an IBAN in place, until the refund is processed (02 7.4.5; K-575).</summary>
public sealed record RefundIbanCorrection(Guid OrderId, Guid RefundIbanId, string Iban, DateTimeOffset At);

/// <summary>Why the refund line's IBAN write did not happen.</summary>
public enum RefundIbanRefusal
{
    /// <summary>The text is not a Turkish IBAN with valid check digits (04 7.3.2.7; the one rule of <c>InvalidIbanAttribute</c>).</summary>
    InvalidIban,

    /// <summary>No such IBAN or open request on the order.</summary>
    NotFound,
}

/// <summary>The outcome of the refund line's IBAN write: the IBAN written, or the refusal.</summary>
public sealed record RefundIbanWriteResult(Guid? RefundIbanId, RefundIbanRefusal? Refusal)
{
    public static RefundIbanWriteResult Rejected(RefundIbanRefusal refusal) => new(null, refusal);
}

/// <summary>The firm's "havale gerçekleşmedi" on an item, or on the itemless refund of a late payment (02 7.4.5; K-575, K-1131).</summary>
public sealed record TransferFailure(Guid OrderId, Guid? OrderItemId, Guid? RefundId, Guid AdminId, string AdminName, DateTimeOffset At);
