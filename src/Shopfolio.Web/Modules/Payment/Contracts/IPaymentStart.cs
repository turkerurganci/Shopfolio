using Shopfolio.Modules.Ordering.Contracts;

namespace Shopfolio.Modules.Payment.Contracts;

/// <summary>
/// What the payment page does with a confirmed card order's start (07 UC-29; 08 2.1.8, 3.1 rule 4): the module's rule over the link layer's answer, in the page's words.
/// </summary>
public enum PaymentStartDecision
{
    /// <summary>The payment may start: the page shows the provider's form in its frame (<see cref="PaymentStartAnswer.FrameAddress"/>).</summary>
    OpenPaymentForm,

    /// <summary>The provider could not be reached: the order is cancelled with <see cref="IPaymentStart.UnopenedCancellationReason"/> and the customer goes back to the cart.</summary>
    CancelAsUnavailable,

    /// <summary>The provider refused a second token for the same order: the order is not cancelled and the confirmation screen opens again.</summary>
    StayOnConfirmation,
}

/// <summary>
/// The values a payment start needs from a confirmed card order (08 2.1.4): the number and total, the contact and billing values with the firm's frozen values that
/// stand in when the customer's do not fit, the request's client address, the end of the payment period and the access key of the return address.
/// </summary>
public sealed record PaymentStartOrder(
    string OrderNumber,
    decimal Total,
    string ContactEmail,
    string CompanyEmail,
    string BillingRecipientName,
    string BillingAddressLine,
    string? ShippingPhone,
    string CompanyPhone,
    string ClientIpAddress,
    DateTimeOffset PaymentDeadline,
    string OrderAccessKey);

/// <summary>The decision and, when the form may open, the address the page's frame loads; the address is not kept anywhere (08 2.1.4: the token is not stored).</summary>
public sealed record PaymentStartAnswer(PaymentStartDecision Decision, Uri? FrameAddress);

/// <summary>
/// The start of a confirmed card order's payment for the payment page (07 UC-29; T510): one token request, made outside any transaction and never repeated; every
/// opening asks for a new one. The cancellation of a <see cref="PaymentStartDecision.CancelAsUnavailable"/> order is the page's, in its own transaction, with
/// <see cref="UnopenedCancellationReason"/> - the reason L-7 does not count (K-1183).
/// </summary>
public interface IPaymentStart
{
    /// <summary>The reason of the cancellation of an order whose payment form could not be opened (08 2.1.8; 02 8.2 L-7).</summary>
    TransitionReason UnopenedCancellationReason { get; }

    Task<PaymentStartAnswer> StartAsync(PaymentStartOrder order, CancellationToken ct);
}
