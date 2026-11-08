namespace Shopfolio.Modules.Catalog.Contracts.Pricing;

/// <summary>
/// One item of a cart or an order as the price calculation takes it (02 3.8.3): <paramref name="UnitPrice"/> is the VAT-included unit price that applies -
/// the discounted unit price rounded to the kuruş when a discount runs (02 3.9.1) -, <paramref name="CouponShare"/> the item's coupon share already set for it
/// (02 3.10.3; zero without a coupon) and <paramref name="VatRate"/> the product's rate in percent (20 is 20%).
/// </summary>
public sealed record ItemPriceInput(decimal UnitPrice, int Quantity, decimal CouponShare, decimal VatRate);

/// <summary>
/// The amounts of one item in the fixed order of 02 3.8.3: <paramref name="Amount"/> is the unit price times the quantity (not rounded); <paramref name="Payable"/>
/// is the amount less the coupon share - what the customer pays for the item; <paramref name="Vat"/> is separated from the payable amount at the item's rate and
/// rounded to the kuruş in the item; <paramref name="Net"/> is the payable amount less the VAT (not rounded again).
/// </summary>
public sealed record ItemAmounts(decimal Amount, decimal CouponShare, decimal Payable, decimal Vat, decimal Net);

/// <summary>An item of the order as the shipping fee's VAT split reads it (02 3.19.2): its type, its VAT rate and its amount (unit price times quantity).</summary>
public sealed record ShippingBasisLine(ProductType ProductType, decimal VatRate, decimal Amount);

/// <summary>One rate's part of the shipping fee (02 3.19.2; 06 4.17 <c>ShippingFeeVatBreakdown</c>): the rate, the part of the fee and the part's VAT.</summary>
public sealed record ShippingVatPart(decimal VatRate, decimal Part, decimal Vat);
