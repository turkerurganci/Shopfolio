using Shopfolio.Modules.Payment.Contracts;

namespace Shopfolio.Modules.Ordering.Contracts;

/// <summary>
/// What the order confirmation and waiting screen shows and starts from (07 UC-28, UC-29; 04 5.14; T510). <see cref="IsAwaitingPayment"/> is the one state the screen opens
/// in - Alındı + Bekliyor, whatever the method (K-1209 (1)). The transfer's view shows the IBAN frozen on the order, the total and the end of the payment period
/// (<see cref="PaymentWindowEndsAt"/>; 02 4.2 Z-8 - after an outage it is the deferred end); the card's start sends the contact and billing values with the firm's frozen
/// values (08 2.1.4) and the access key of the return address.
/// </summary>
public sealed record ConfirmationScreenFacts(
    Guid OrderId,
    string OrderNumber,
    bool IsAwaitingPayment,
    PaymentMethod PaymentMethod,
    decimal Total,
    string? TransferIban,
    DateTimeOffset PaymentWindowEndsAt,
    string? ContactEmail,
    string CompanyEmail,
    string? BillingRecipientName,
    string? BillingAddressLine,
    string? ShippingPhone,
    string CompanyPhone,
    string? OrderAccessKey);

/// <summary>The cancellation's answer as the transaction runner takes it: everything but <see cref="UnopenedPaymentCancellation.Cancelled"/> is rolled back.</summary>
public sealed record UnopenedPaymentCancellationResult(UnopenedPaymentCancellation Outcome) : Infrastructure.IOperationResult
{
    public bool IsRejected => Outcome != UnopenedPaymentCancellation.Cancelled;
}

/// <summary>The confirmation screen's two calls into the order (07 UC-28, UC-29; T510). Both join the caller's transaction (09 6 item 3).</summary>
public interface IOrderConfirmationScreen
{
    /// <summary>The screen's values of an order the caller has already opened by its root; null when there is no such order. A plain read.</summary>
    Task<ConfirmationScreenFacts?> ReadAsync(Guid orderId, CancellationToken ct);

    /// <summary>
    /// The provider could not be reached for a confirmed order (07 UC-29; 03 10.1.1.2; K-1183, K-1200): the system cancels the order with <paramref name="reason"/> - its
    /// reservation goes free and L-7 does not count it - and the requesting browser's cart takes the once-told mark (the member's account cart, the guest's
    /// <c>__Host-cart</c> cart; none when <paramref name="cartOwner"/> is null or has no cart - K-1212 (2)). An order no longer Alındı + Bekliyor is not cancelled.
    /// </summary>
    Task<UnopenedPaymentCancellationResult> CancelUnopenedAsync(Guid orderId, TransitionReason reason, CartOwner? cartOwner, CancellationToken ct);
}
