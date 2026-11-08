namespace Shopfolio.Modules.Ordering;

/// <summary>
/// One entry of <see cref="Order.ShippingFeeVatBreakdown"/> (06 4.17; 02 3.19.2): a VAT rate of the order's physical items, the part of the shipping fee that follows it and the part's VAT.
/// The order owns this payload's shape (09 12 item 5): the field names are written to the <c>jsonb</c> column and never renamed.
/// </summary>
internal sealed record ShippingFeeVatShare(decimal VatRate, decimal Part, decimal Vat);
