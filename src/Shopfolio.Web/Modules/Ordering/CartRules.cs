using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Modules.Settings.Contracts;

namespace Shopfolio.Modules.Ordering;

/// <summary>
/// The cart's display and order-entry rules as pure functions (T346; 02 3.16.5-3.16.7). A line is live: it reads its price and availability from the current product. Nothing here reads the
/// database; <see cref="Carts"/> applies it to the lines it loads.
/// </summary>
internal static class CartRules
{
    /// <summary>
    /// Whether the line carries the "Sepete eklediğinden beri fiyatı değişti" mark (02 3.16.5): the price that applies now differs from the unit price of the first add - in either direction. The mark
    /// goes when the price returns to the reference. A quantity increase never moves the reference, so it never shows here.
    /// </summary>
    public static bool PriceChanged(decimal firstAddedUnitPrice, decimal currentUnitPrice) => firstAddedUnitPrice != currentUnitPrice;

    /// <summary>
    /// Whether the line enters the order and the amounts (02 3.16.6, 3.16.7): the product and the variant stand on the showcase and the variant can be bought. A sold-out line stays in the cart, marked,
    /// and is bought again when the stock returns; it is outside the coupon's minimum, the shipping fee and the thresholds.
    /// </summary>
    public static bool EntersOrder(bool isOnShowcase, bool isSoldOut) => isOnShowcase && !isSoldOut;

    /// <summary>
    /// The payment step does not open when no line can enter an order (02 3.16.7): an empty cart and a cart of sold-out lines alike; nor when the basis is under the minimum order amount
    /// (02 3.18.1; T349) - the cart shows the missing amount (<see cref="ShippingAssessment.MinimumOrderMessage"/>).
    /// </summary>
    public static CartPaymentBlock PaymentBlockOf(IReadOnlyCollection<CartLine> lines, ShippingAssessment shipping)
    {
        ArgumentNullException.ThrowIfNull(lines);
        ArgumentNullException.ThrowIfNull(shipping);
        if (!lines.Any(l => l.EntersOrder))
        {
            return CartPaymentBlock.NoOrderableItem;
        }

        return shipping.Refusal is null ? CartPaymentBlock.None : CartPaymentBlock.MinimumOrderAmountNotMet;
    }

    /// <summary>
    /// The shipping terms of the cart (02 3.18, 3.19; T349): on <see cref="OrderableSubtotal"/>, with a physical item only when a physical line enters the order - a sold-out physical line
    /// carries neither the fee nor the minimum (02 3.18.3, 3.19.3).
    /// </summary>
    public static ShippingAssessment AssessShipping(IReadOnlyCollection<CartLine> lines, ShippingFeeSection terms)
    {
        ArgumentNullException.ThrowIfNull(lines);
        return ShippingCharge.Assess(terms, OrderableSubtotal(lines), lines.Any(l => l.EntersOrder && l.ProductType == ProductType.Physical));
    }

    /// <summary>The basis the coupon's minimum, the shipping fee and the thresholds read (02 3.16.7): unit price times quantity of the lines that enter the order only.</summary>
    public static decimal OrderableSubtotal(IReadOnlyCollection<CartLine> lines)
    {
        ArgumentNullException.ThrowIfNull(lines);
        return lines.Where(l => l.EntersOrder).Sum(l => l.UnitPrice * l.Quantity);
    }

    /// <summary>
    /// Whether the merge shows the customer something new (02 3.16.4): the member's line has a variant the guest cart did not hold, or a larger quantity than the guest cart's (the larger one stays,
    /// 02 3.16.3). A guest line that is equal or larger shows nothing the customer had not seen.
    /// </summary>
    public static bool MergeShowsNew(int? guestQuantity, int memberQuantity) => guestQuantity is null || memberQuantity > guestQuantity;
}
