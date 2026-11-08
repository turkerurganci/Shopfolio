namespace Shopfolio.Modules.Ordering.Contracts;

/// <summary>
/// The answer of the guest's lookup by number and e-mail (07 UC-32; 02 3.22.3): the order and the access key of its address. The caller answers with a 303 to the
/// access-key root - the guest's only way (07 2.3.4) - and never shows the key as text.
/// </summary>
public sealed record OrderAccessLookup(Guid OrderId, string AccessKey);

/// <summary>
/// One download button of the order page (02 3.12.5, 3.22.4): a digital item of an order whose payment is confirmed. <see cref="IsAvailable"/> is false when the item's
/// right is used up - the page then says so and the firm renews the right (02 3.12.6).
/// </summary>
public sealed record OrderDownloadButton(Guid OrderItemId, string ProductName, bool IsAvailable);

/// <summary>
/// What the order page carries besides the lists of other tasks (02 3.22.4; 04 5.16): the two status axes, the delivery area (the carrier, the tracking number and the
/// "Takip et" link), the download buttons and the bank transfer IBAN frozen on the order (02 3.21.5). Read from the order alone: no e-mail, no notice and no queue
/// stands between the buyer and any of the four (02 6.1.1). <see cref="TransferPaymentEndsAt"/> is the last moment of a bank transfer order still waiting for its
/// payment - after an outage it is the deferred one (03 4.2.12; 02 4.2 Z-8: the order page shows the new last day, no e-mail tells it); empty on every other order.
/// </summary>
public sealed record OrderPageSummary(
    Guid OrderId,
    string OrderNumber,
    OrderStatus OrderStatus,
    PaymentStatus PaymentStatus,
    ShippingDisplay Shipping,
    IReadOnlyList<OrderDownloadButton> Downloads,
    string? TransferIban,
    DateTimeOffset? TransferPaymentEndsAt);
