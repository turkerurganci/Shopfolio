using Shopfolio.Infrastructure;

namespace Shopfolio.Modules.AfterSales;

/// <summary>
/// The amount of the units an operation takes from an item (02 7.1.7 (1)-(4); K-788, K-1104): the one split of the item records. The product amount is the
/// frozen unit price times the units (no rounding); the coupon share and the VAT are the item's frozen share and VAT in the ratio of the units to the item's quantity,
/// rounded down to the kuruş by <see cref="Money.ShareOfUnits"/> (09 14.1 item 1); the operation that closes the item's last open units takes what is left of both, so the shares of all the item's records add up to the
/// item's share and VAT exactly and no running total exceeds them.
/// </summary>
internal static class ItemAmountSplit
{
    /// <summary>
    /// The amounts of <paramref name="quantity"/> units of an item of <paramref name="itemQuantity"/> units. <paramref name="closesLastOpenUnits"/> - the units are the last
    /// open ones; <paramref name="takenCouponShare"/> and <paramref name="takenVat"/> - what the item's earlier records already took.
    /// </summary>
    public static ItemAmounts Of(
        decimal unitPrice,
        int itemQuantity,
        decimal itemCouponShare,
        decimal itemVat,
        int quantity,
        bool closesLastOpenUnits,
        decimal takenCouponShare,
        decimal takenVat)
    {
        if (itemQuantity <= 0 || quantity <= 0 || quantity > itemQuantity)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity), quantity, "The units are at least one and at most the item's quantity (02 7.1.6).");
        }

        decimal product = unitPrice * quantity;
        if (closesLastOpenUnits)
        {
            return new ItemAmounts(product, itemCouponShare - takenCouponShare, itemVat - takenVat);
        }

        return new ItemAmounts(product, Money.ShareOfUnits(itemCouponShare, quantity, itemQuantity), Money.ShareOfUnits(itemVat, quantity, itemQuantity));
    }
}
