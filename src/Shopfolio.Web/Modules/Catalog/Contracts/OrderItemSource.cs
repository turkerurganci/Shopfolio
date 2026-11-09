using Shopfolio.Modules.AfterSales.Contracts;
using Shopfolio.Modules.Catalog.Contracts.Pricing;

namespace Shopfolio.Modules.Catalog.Contracts;

/// <summary>
/// What an order item freezes of its variant at the moment the order is born (T355; 02 3.23.1, 3.23.4): read once from the live product and copied into the item, which never reads the
/// product again (K-77, K-79). The values that produce the paid amount - the price as stored, the product's VAT rate and, while the product's discount runs, its declaration with the
/// discounted price, the reference price and the percentage (02 3.9.6) -; the product's description of that moment - name, option values, image -; and the attributes that decide the
/// item's delivery line and withdrawal regime - the type, the physical product's exemption, the service's fulfilment period - and, on a digital item, the variant's current file.
/// </summary>
/// <param name="VariantId">The variant.</param>
/// <param name="ProductId">The variant's product.</param>
/// <param name="ProductType">The product's type (02 3.2.2): the item's delivery line and withdrawal regime.</param>
/// <param name="ProductName">The product's name.</param>
/// <param name="FirstOptionValue">The variant's first option value ("Kırmızı"); empty on a product without options.</param>
/// <param name="SecondOptionValue">The variant's second option value ("M").</param>
/// <param name="ImageId">The image the variant shows (02 3.11.1): its own, else the product's main image; empty when it has neither.</param>
/// <param name="UnitPrice">The variant's price as stored, VAT included, without the discount (02 3.8.1).</param>
/// <param name="VatRate">The product's VAT rate, which the variant inherits (02 3.8.2).</param>
/// <param name="Discount">The running discount's declaration (02 3.9.1, 3.9.6); empty when no discount runs now.</param>
/// <param name="WithdrawalExemptionReason">The physical product's withdrawal exemption (02 7.3.4); the mark is this value being filled; empty on any other type.</param>
/// <param name="FulfilmentPeriod">The service's fulfilment period in days (Z-39); filled exactly on a service.</param>
/// <param name="DigitalFileId">The digital variant's current file (02 3.12.7): its own, else the product's; empty on any other type or when no file is attached.</param>
public sealed record OrderItemSource(
    Guid VariantId,
    Guid ProductId,
    ProductType ProductType,
    string ProductName,
    string? FirstOptionValue,
    string? SecondOptionValue,
    Guid? ImageId,
    decimal UnitPrice,
    decimal VatRate,
    DiscountLine? Discount,
    WithdrawalExemptionReason? WithdrawalExemptionReason,
    int? FulfilmentPeriod,
    Guid? DigitalFileId);
