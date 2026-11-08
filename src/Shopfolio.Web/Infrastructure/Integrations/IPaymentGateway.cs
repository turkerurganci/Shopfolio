namespace Shopfolio.Infrastructure.Integrations;

/// <summary>
/// The payment provider behind the link layer (08 3.1 rule 1, 2.1; 05 3.6.1; MK-17; 09 11.1 item 1): the five jobs of the product, with names
/// that carry no provider's name. Card payment works with one provider and this interface has one implementation (KD-64; 05 3.6.4); the
/// provider's request, signature and answer codes stay inside it. Every call has its limit (<see cref="IntegrationOperations"/>) and is not
/// retried by the layer; every result is a class of its family, never an exception. The product's outcome - cancelling the order, the two
/// minutes of "unavailable" - is the calling module's (08 3.1 rule 4).
/// </summary>
internal interface IPaymentGateway
{
    /// <summary>Starts a payment: a call the user waits for (15 seconds).</summary>
    Task<PaymentStartResult> StartAsync(PaymentStartRequest request, CancellationToken ct);

    /// <summary>Asks the outcome of an order's payment: a job call (30 seconds).</summary>
    Task<PaymentQueryResult> QueryPaymentAsync(string orderNumber, CancellationToken ct);

    /// <summary>Asks for a card refund: a job call (30 seconds); the request goes once and is never sent again.</summary>
    Task<RefundRequestResult> RequestRefundAsync(CardRefund refund, CancellationToken ct);

    /// <summary>Asks the outcome of a sent card refund: a job call (30 seconds).</summary>
    Task<RefundQueryResult> QueryRefundAsync(CardRefund refund, CancellationToken ct);

    /// <summary>Verifies an incoming notification's signature over its form fields: a local computation, no call (08 3.2).</summary>
    NotificationVerification VerifyNotification(IReadOnlyDictionary<string, string> formFields);

    /// <summary>
    /// What a notification's event type and outcome, as they came and as the inbound record keeps them, mean to the product (08 2.1.5 "event type
    /// and outcome values"; 05 4.5.5): a payment that succeeded, a payment that failed, or a value the layer does not recognize. A local computation,
    /// no call: the notification endpoint reads it at the record and the processing job reads it again from the record, so the provider's values
    /// stay inside the layer.
    /// </summary>
    NotificationResult ReadNotificationResult(string eventType, string outcome);

    /// <summary>
    /// The amount check of a successful payment's notification against the order's paid total (08 2.1.5 "amount comparison"; 05 3.6.1, 4.5.5;
    /// 06 4.35 Constraints): the form of "the amount holds" is the provider's, so it is the layer's. A local computation, no call; a mismatch is a
    /// class of its own, never an exception.
    /// </summary>
    NotificationAmountCheck CheckNotificationAmount(PaymentNotice notice, decimal orderTotal);
}

/// <summary>What a recorded notification's event type and outcome mean (08 2.1.5; 06 5.6.6 <c>UnrecognizedNotification</c>).</summary>
internal enum NotificationResult
{
    /// <summary>The payment succeeded: applied through the one path of 05 4.5.3 when the amount holds.</summary>
    PaymentSucceeded,

    /// <summary>The payment failed: the unpaid order is cancelled by the system (02 6.2.1); no amount is checked.</summary>
    PaymentFailed,

    /// <summary>The event type or the outcome is none the layer knows: the notification is recorded marked and not processed.</summary>
    Unrecognized,
}

/// <summary>The class of a successful payment notification's amount check (08 2.1.5): the amount holds, or it does not and the notification is marked.</summary>
internal enum NotificationAmountCheck
{
    Holds,
    Mismatch,
}

/// <summary>
/// What starting a payment needs, at the product's level (08 2.1.4): the order's number and total, the contact and billing values with the
/// firm's values that stand in when the customer's do not fit, the request's client address, the end of the payment period and the order's
/// access key for the return address. The provider's own form of each value is made in the layer.
/// </summary>
internal sealed record PaymentStartRequest(
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

/// <summary>A card refund as the layer sends and asks it (08 2.1.9): the order's number, the refund's amount and the refund record's id, its reference.</summary>
internal sealed record CardRefund(string OrderNumber, decimal Amount, Guid RefundRecordId);

/// <summary>
/// The fields of a verified notification that the field mapping reads (08 2.1.5): the provider's transaction reference as it came (the order number
/// without dashes; kept in the record's transaction id and order reference), the order number the layer maps it back to - null when the mapping does
/// not hold -, the event type and the outcome as they came, the collected total and the started amount - null when the field could not be read (a body
/// that cannot be parsed; 05 4.5.5) -, and the three marks the core checks before it applies a notification.
/// </summary>
internal sealed record PaymentNotice(
    string ProviderTransactionId,
    string? OrderNumber,
    string EventType,
    string Outcome,
    decimal? TotalAmount,
    decimal? PaymentAmount,
    bool PaidByCard,
    bool InLira,
    bool TestMode);
