using Shopfolio.Infrastructure;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Catalog.Contracts.Pricing;

namespace Shopfolio.Modules.Catalog;

/// <summary>
/// The price and VAT calculation of an item and of an order as pure functions (02 3.8.1-3.8.4, 3.19.2; 09 14.1; 11 T336 criteria 3-7). The order of an item is
/// fixed (02 3.8.3): (1) the amount is the unit price - the discounted one already rounded to the kuruş when a discount runs - times the quantity, not rounded;
/// (2) the coupon share set for the item is taken off; (3) the VAT is separated from what is left at the item's rate and rounded to the kuruş in the item, and the
/// net amount is the rest. The order total is the sum of the items' payable amounts and the shipping fee and is never derived again from net plus VAT (K-61).
/// Rounding happens only through <see cref="Money"/> and only where 02 counts it (09 14.1 items 2, 4). A value outside its rule is a programming error and throws.
/// </summary>
internal static class ItemPricing
{
    /// <summary>The decimals of a money amount: the kuruş (02 3.8.4; 06 2.5.1 <c>numeric(12,2)</c>).</summary>
    public const int AmountDecimals = 2;

    /// <summary>Whether a price can be saved (02 3.8.4; K-62, K-600): above zero, at most two decimals, at most the money type's limit.</summary>
    public static bool IsValidPrice(decimal price) => price > 0 && HasAtMostDecimals(price, AmountDecimals) && price <= Money.MaxAmount;

    /// <summary>Whether a number has at most <paramref name="decimals"/> decimals; trailing zeros do not count and nothing is rounded.</summary>
    public static bool HasAtMostDecimals(decimal value, int decimals)
    {
        decimal shifted = value;
        for (int i = 0; i < decimals; i++)
        {
            shifted *= 10m;
        }

        return shifted % 1 == 0;
    }

    /// <summary>The amounts of one item in the fixed order of 02 3.8.3.</summary>
    public static ItemAmounts Price(ItemPriceInput input)
    {
        ArgumentNullException.ThrowIfNull(input);
        if (!IsValidPrice(input.UnitPrice))
        {
            throw new ArgumentOutOfRangeException(nameof(input), input.UnitPrice, "The unit price is not a price: above zero, in kuruş.");
        }

        if (input.Quantity < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(input), input.Quantity, "The quantity is under one.");
        }

        decimal amount = input.UnitPrice * input.Quantity;
        if (input.CouponShare < 0 || !HasAtMostDecimals(input.CouponShare, AmountDecimals) || input.CouponShare > amount)
        {
            throw new ArgumentOutOfRangeException(nameof(input), input.CouponShare, "The coupon share is not a kuruş amount from zero up to the item's amount.");
        }

        decimal payable = amount - input.CouponShare;
        decimal vat = Money.VatOf(payable, input.VatRate);
        return new ItemAmounts(amount, input.CouponShare, payable, vat, payable - vat);
    }

    /// <summary>
    /// The shipping fee's VAT split (02 3.19.2; 06 4.17 <c>ShippingFeeVatBreakdown</c>): the fee follows the rates of the order's physical items - a digital or a
    /// service item never gives its rate to it. With one rate the whole fee is that rate's part; with several the fee is split in proportion to the amounts of the
    /// physical items of each rate, the parts rounded down and the left-over kuruş on the largest part (02 3.8.3; <see cref="Money.Allocate"/>), and each part's VAT
    /// is separated at its rate. The parts come in the order the rates first appear among the items. A fee of zero has no parts; a fee without a physical item is
    /// a programming error (only a physical item carries a shipping fee, 02 3.19.1).
    /// </summary>
    public static IReadOnlyList<ShippingVatPart> SplitShippingFee(decimal shippingFee, IReadOnlyList<ShippingBasisLine> lines)
    {
        ArgumentNullException.ThrowIfNull(lines);
        if (shippingFee < 0 || !HasAtMostDecimals(shippingFee, AmountDecimals))
        {
            throw new ArgumentOutOfRangeException(nameof(shippingFee), shippingFee, "The shipping fee is not a kuruş amount of zero or more.");
        }

        if (shippingFee == 0)
        {
            return [];
        }

        List<decimal> rates = [];
        List<decimal> weights = [];
        foreach (ShippingBasisLine line in lines.Where(l => l.ProductType == ProductType.Physical))
        {
            if (line.Amount <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(lines), line.Amount, "An item's amount is not above zero.");
            }

            int at = rates.IndexOf(line.VatRate);
            if (at < 0)
            {
                rates.Add(line.VatRate);
                weights.Add(line.Amount);
            }
            else
            {
                weights[at] += line.Amount;
            }
        }

        if (rates.Count == 0)
        {
            throw new ArgumentException("A shipping fee without a physical item has no rate to follow.", nameof(lines));
        }

        IReadOnlyList<decimal> parts = Money.Allocate(shippingFee, weights);
        return [.. rates.Select((rate, i) => new ShippingVatPart(rate, parts[i], Money.VatOf(parts[i], rate)))];
    }

    /// <summary>The order total (02 3.8.3; K-61): the sum of the items' payable amounts, each rounded in its item, and the shipping fee - never net plus VAT again.</summary>
    public static decimal SumOrderTotal(IReadOnlyList<ItemAmounts> items, decimal shippingFee)
    {
        ArgumentNullException.ThrowIfNull(items);
        if (shippingFee < 0 || !HasAtMostDecimals(shippingFee, AmountDecimals))
        {
            throw new ArgumentOutOfRangeException(nameof(shippingFee), shippingFee, "The shipping fee is not a kuruş amount of zero or more.");
        }

        return items.Sum(i => i.Payable) + shippingFee;
    }
}
