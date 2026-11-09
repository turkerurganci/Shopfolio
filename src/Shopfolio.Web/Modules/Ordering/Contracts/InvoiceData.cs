using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Catalog.Contracts.Pricing;
using Shopfolio.Modules.Payment.Contracts;
using Shopfolio.Modules.Settings.Contracts;

namespace Shopfolio.Modules.Ordering.Contracts;

/// <summary>
/// The invoice data an order carries (T357; 02 3.25.1, 3.25.3; 06 4.17): the product never issues an invoice and keeps none (02 3.25.1, 3.25.2) - it only hands out the values the firm
/// issues one from in its own accounting tool, through the order export (02 10.7.1; T384). Every value is the order's own: the items' frozen amounts, the frozen shipping fee and its VAT
/// breakdown, the billing address, the firm's legal identity of the confirmation day (never the panel's current identity), the payment method, the moment of the payment's approval,
/// the carrier's name and the installation's domain. The moments are read from the transition records, not stored (06 4.17).
/// </summary>
/// <param name="OrderId">The order.</param>
/// <param name="OrderNumber">The order number (P-19).</param>
/// <param name="CreatedAt">The moment the order was born, the moment its values froze (02 3.23.4).</param>
/// <param name="Total">The paid total the customer confirmed (02 3.21.9).</param>
/// <param name="CouponCode">The coupon code as it was at confirmation; null when no coupon was used.</param>
/// <param name="ShippingFee">The frozen shipping fee.</param>
/// <param name="ShippingFeeVatBreakdown">The shipping fee's VAT split over the physical items' rates (02 3.19.2; K-500), as frozen at confirmation; empty when the fee is 0.</param>
/// <param name="BillingAddress">The billing address the order holds; null on an order whose personal data is disposed.</param>
/// <param name="Company">The firm's legal identity frozen into the order at confirmation (02 3.25.3; K-220).</param>
/// <param name="PaymentMethod">The payment method.</param>
/// <param name="PaymentConfirmedAt">The payment's approval - the last Ö1 record no correction took back; null while the payment is not approved.</param>
/// <param name="CarrierName">The carrier's name once the order is shipped: the listed or retired company's name, or the typed name of "Diğer"; null before the shipment and on own-vehicle delivery.</param>
/// <param name="IsOwnVehicleDelivery">Whether the firm delivered with its own vehicle.</param>
/// <param name="Domain">The installation's domain name (02 3.25.1; 05 8.4.3).</param>
/// <param name="Items">The order's items in the order they were created.</param>
public sealed record InvoiceData(
    Guid OrderId,
    string OrderNumber,
    DateTimeOffset CreatedAt,
    decimal Total,
    string? CouponCode,
    decimal ShippingFee,
    IReadOnlyList<ShippingVatPart> ShippingFeeVatBreakdown,
    PartyAddress? BillingAddress,
    FrozenCompanyIdentity Company,
    PaymentMethod PaymentMethod,
    DateTimeOffset? PaymentConfirmedAt,
    string? CarrierName,
    bool IsOwnVehicleDelivery,
    string Domain,
    IReadOnlyList<InvoiceItem> Items);

/// <summary>
/// The firm's legal identity as the order froze it at confirmation (06 4.17; 02 3.23.3): the type, the identity fields of the type, the address and the contact details. A later change
/// of the firm's identity reaches only new orders.
/// </summary>
public sealed record FrozenCompanyIdentity(
    CompanyType CompanyType,
    string CompanyLegalName,
    string? CompanyTaxIdNumber,
    string? CompanyMersisNumber,
    string? CompanyTradeRegistryNumber,
    Guid CompanyProvinceId,
    string CompanyDistrict,
    string CompanyAddressLine,
    string CompanyPhone,
    string CompanyEmail,
    string CompanyKepAddress);

/// <summary>
/// One item's invoice line (06 4.18): the frozen description and amounts - the unit price, the discount, the coupon's share, the VAT rate and amount - and the moment it was handed
/// over (02 3.25.1): the order's shipment for a physical item, the "completed" mark for a service item, the payment's approval for a digital item; null until that happened.
/// </summary>
public sealed record InvoiceItem(
    Guid OrderItemId,
    ProductType ProductType,
    string ProductName,
    string? FirstOptionValue,
    string? SecondOptionValue,
    int Quantity,
    decimal UnitPrice,
    decimal? DiscountPercentage,
    decimal? DiscountedUnitPrice,
    decimal CouponShare,
    decimal VatRate,
    decimal VatAmount,
    DateTimeOffset? ShippedAt);
