namespace Shopfolio.Modules.Ordering.Contracts;

/// <summary>
/// The firm's retry of a failed card refund from the panel (T370; 02 7.2.8; 06 4.34; K-569): the order, the refund, the administrator and the version the screen showed.
/// </summary>
public sealed record RetryCardRefundRequest(Guid OrderId, Guid RefundId, TransitionAdmin Admin, int ExpectedVersion);

/// <summary>
/// The firm opens the transfer path of a failed card refund (T370; 02 7.2.8; K-569): an IBAN request on the refund and B-14 to the customer.
/// </summary>
public sealed record OpenRefundTransferPathRequest(Guid OrderId, Guid RefundId, TransitionAdmin Admin, int ExpectedVersion);

/// <summary>The customer's IBAN for an open request on the order page (T370; 02 7.4.5; 03 1.11.33): the order, the request and the text as typed.</summary>
public sealed record EnterRefundIbanRequest(Guid OrderId, Guid RequestId, string Iban);

/// <summary>The customer's correction of the order's IBAN until the refund is processed (T370; 02 7.4.5; K-575).</summary>
public sealed record CorrectRefundIbanRequest(Guid OrderId, Guid RefundIbanId, string Iban);

/// <summary>
/// The firm's "havale gerçekleşmedi" (T370; 02 7.4.5; K-575, K-1131): on an item, or on the itemless refund of a late payment; the screen's version.
/// </summary>
public sealed record ReportTransferFailedRequest(Guid OrderId, Guid? OrderItemId, Guid? RefundId, TransitionAdmin Admin, int ExpectedVersion);
