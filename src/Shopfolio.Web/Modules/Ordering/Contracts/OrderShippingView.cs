namespace Shopfolio.Modules.Ordering.Contracts;

/// <summary>
/// What the customer's order page shows of the delivery (02 3.20.4-3.20.7; 04 5.16.8; 11 T350 criteria 7, 8): the frozen promise in business days and its text, the moment the payment was
/// confirmed (when the period started), the moment the promise runs out, the moment the order was shipped and the carrier display.
/// <see cref="ShippingPeriod"/> is null on an order with no physical item and then the promise, its text and its end are null too. <see cref="ShippedAt"/> and <see cref="Display"/> are
/// null until the order is marked shipped.
/// </summary>
public sealed record OrderShippingView(
    int? ShippingPeriod,
    string? PromiseText,
    DateTimeOffset? PaymentConfirmedAt,
    DateTimeOffset? PromiseEndsAt,
    DateTimeOffset? ShippedAt,
    ShippingDisplay? Display);
