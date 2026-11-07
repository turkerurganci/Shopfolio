using Shopfolio.Modules.Ordering.Contracts;

namespace Shopfolio.Modules.Payment.Contracts;

/// <summary>
/// The outcome of one card refund request (05 4.6.2): sent - the provider carried the refund out; not sent - the provider refused it or the request
/// could not go; unknown - a timeout, a broken connection or a crash after the request went, so it is not known whether the money left.
/// </summary>
public enum CardRefundResult
{
    Succeeded,
    Failed,
    Unknown,
}

/// <summary>
/// What the payment axis needs to know of one refund (06 4.33; 02 5.5): whose order it is, its reason, amount and way, who started it, whether it is
/// sent or voided, and whether the request that carried it out was the firm's retry after a failed one. The record is a copy; the refund stays the
/// Payment module's.
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
