using Shopfolio.Infrastructure.Operations;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Payment.Contracts;

namespace Shopfolio.Modules.Ordering.Contracts;

/// <summary>
/// The lists of the customer's order page (07 UC-33; 04 5.16.3, 5.16.5, 5.16.7, 5.16.9; 02 3.23): the items as they were frozen at the confirmation, the frozen breakdown, the refunds that
/// were carried out, the order's addresses and contact e-mail, and the record of the ticked boxes. The order's statuses, the delivery area, the download buttons and the frozen IBAN are
/// <see cref="IOrderingModule.GetOrderPageSummaryAsync"/>'s, the open operations <see cref="IOrderOperations.ReadAsync"/>'s and the two documents <see cref="IOrderDocuments"/>'s: the page puts
/// the answers side by side and derives no status of its own (04 5.16.17). <see cref="ReadAsync"/> is a plain read; <see cref="TakeDownloadAsync"/> is the customer's one write of the page
/// (UC-36).
/// </summary>
public interface IOrderPage
{
    /// <summary>The order page's lists, or null when there is no such order. The order is already opened by one of the three ways (07 2.3.4); nothing here asks who is asking.</summary>
    Task<OrderPageDetail?> ReadAsync(Guid orderId, CancellationToken ct);

    /// <summary>
    /// UC-36 — the download of a digital item (07 4.16.3; 03 1.11.14; 05 7.7.4; 09 8.3 item 3). Joins the caller's transaction (<c>OperationRunner</c>) and runs the operation table's
    /// <see cref="OrderOperation.DownloadDigitalFile"/> row through the gate: the order's row lock (the customer's operation carries no version), the decision's facts, the row's rule,
    /// and only for an open verdict the write - the item's file is looked up and the right drops by the counter's one conditional update; a missing file spends no right. A refused
    /// result (<see cref="GatedOperationResult.IsRejected"/>) rolls the transaction back and names the reason; an issued one carries the one-hour signed value of the link.
    /// The caller has already checked that the item is a digital item of this order (404 otherwise).
    /// </summary>
    Task<DownloadOperationResult> TakeDownloadAsync(Guid orderId, Guid orderItemId, CancellationToken ct);
}

/// <summary>
/// The answer of <see cref="IOrderPage.TakeDownloadAsync"/>: <see cref="Gate"/> is what the runner reads (a rejected one rolls back), <see cref="LinkValue"/> the signed value of an issued
/// link and <see cref="Refusal"/> the reason of a refused one.
/// </summary>
public sealed record DownloadOperationResult(GatedOperationResult Gate, string? LinkValue, DownloadRefusal? Refusal);

/// <summary>
/// The order page's lists (04 5.16.3). <see cref="CreatedAt"/> is the order's date in the heading. <see cref="IsCardPaymentAwaited"/> is a card order still waiting for the provider's answer
/// (Alındı + Bekliyor; 04 5.16.4, 5.16.17); <see cref="IsCardPaymentFailed"/> is a card order cancelled because its payment failed (İptal edildi + Başarısız; 03 2.5.1.3). The privacy notice's
/// frozen version (<see cref="PrivacyNoticeVersionId"/>) is the one in force on the confirmation day (02 3.23.3, 3.33.3).
/// </summary>
public sealed record OrderPageDetail(
    Guid OrderId,
    DateTimeOffset CreatedAt,
    PaymentMethod PaymentMethod,
    bool IsCardPaymentAwaited,
    bool IsCardPaymentFailed,
    IReadOnlyList<OrderPageItem> Items,
    OrderPageBreakdown Breakdown,
    IReadOnlyList<OrderPageRefund> Refunds,
    OrderPageAddresses Addresses,
    IReadOnlyList<OrderPageCheckbox> Checkboxes,
    Guid PrivacyNoticeVersionId);

/// <summary>
/// One item row as it was frozen (04 5.16.5; 02 3.23.1): the name with its option values, the image, the type, the quantity and the open quantity (an item some of whose units closed shows
/// both, K-790), the applied unit price, the line's amount after the coupon's share, the withdrawal exemption's note and the delivery mark. The item's own status badge does not exist (04 2.5.2).
/// </summary>
public sealed record OrderPageItem(
    Guid OrderItemId,
    string Name,
    Guid? ImageId,
    ProductType ProductType,
    int Quantity,
    int OpenQuantity,
    decimal UnitPrice,
    decimal LineAmount,
    string? ExemptionNote,
    DateTimeOffset? DeliveryMarkedAt,
    DateOnly? DeliveryDate);

/// <summary>
/// The frozen breakdown of the amount (04 5.16.7; 02 3.23.2): the items' total at the applied prices, the coupon's share, the VAT included, the shipping fee - zero with
/// <see cref="IsShippingFree"/> is "Kargo ücretsiz", zero without it is an order with no physical item - and the total. It is not worked out again after a cancellation or a refund
/// (02 7.2.7, 7.4.6).
/// </summary>
public sealed record OrderPageBreakdown(decimal ItemsTotal, decimal CouponShare, string? CouponCode, decimal Vat, decimal ShippingFee, bool IsShippingFree, decimal Total);

/// <summary>
/// One refund that was carried out (04 5.16.7; K-826): its date, amount and way - to the card or to an IBAN, the IBAN itself never shown - and the item and the units it covered; an
/// amount-based refund and the refund of a late payment carry no item. A refund still waiting to be processed or settled is not a row.
/// </summary>
public sealed record OrderPageRefund(DateTimeOffset At, decimal Amount, PaymentMethod Way, string? ItemName, int? Quantity);

/// <summary>The order's addresses and contact e-mail as they stand on the order (04 5.16.3 (7); 02 3.23.3); a disposed order's fields are empty.</summary>
public sealed record OrderPageAddresses(string? ContactEmail, OrderPageAddress? Shipping, OrderPageAddress? Billing);

/// <summary>One address of the order with the province's name.</summary>
public sealed record OrderPageAddress(string RecipientName, string? Phone, string ProvinceName, string District, string AddressLine);

/// <summary>One ticked box of the confirmation (04 5.16.9; 02 3.24.6; K-595): the text as it was shown and the names of the items it covered, read only.</summary>
public sealed record OrderPageCheckbox(CheckboxType Type, string ShownText, IReadOnlyList<string> ItemNames);
