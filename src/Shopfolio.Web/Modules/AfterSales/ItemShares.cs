using Shopfolio.Infrastructure;
using Shopfolio.Modules.AfterSales.Contracts;

namespace Shopfolio.Modules.AfterSales;

/// <summary>
/// The amounts an item record sets aside from its item (02 7.1.7 (1)-(4); 09 14.1 item 2; K-788): the one place the rule is written, a pure function. (1) The product
/// amount is the unit price times the units, not rounded. (2) The coupon share and (3) the VAT are the units' share of the item's frozen values, rounded down to the
/// kuruş (<see cref="Money.ShareOfUnits"/>). (4) The record that closes the item's last open units takes what the earlier records left; no record takes more than is
/// left, so no running total passes the item's own share. Three units × 300 with a coupon share of 100: 33,33, 33,33 and the last 33,34 - refunds 266,67, 266,67, 266,66.
/// </summary>
internal static class ItemShares
{
    public static ItemAmounts SetAside(ItemFrozenAmounts frozen, int itemQuantity, int quantity, bool closesTheItem, decimal couponSetAside, decimal vatSetAside)
    {
        ArgumentNullException.ThrowIfNull(frozen);
        decimal couponLeft = Math.Max(0m, frozen.CouponShare - couponSetAside);
        decimal vatLeft = Math.Max(0m, frozen.VatAmount - vatSetAside);
        decimal coupon = closesTheItem ? couponLeft : Math.Min(couponLeft, Money.ShareOfUnits(frozen.CouponShare, quantity, itemQuantity));
        decimal vat = closesTheItem ? vatLeft : Math.Min(vatLeft, Money.ShareOfUnits(frozen.VatAmount, quantity, itemQuantity));
        return new ItemAmounts(frozen.UnitPrice * quantity, coupon, vat);
    }

    /// <summary>
    /// The amounts of the parts a withdrawal declaration's units are received in (02 7.1.7 (5); K-1057): each part's coupon share and VAT are split from the
    /// declaration's own - the shares (1)-(4) set aside for it - by the part's units over the declared units with the same rounding (<see cref="SetAside"/> with the
    /// declaration in the item's place); the product amount is the unit price times the part's units. The part that refunds the declaration's last units takes what
    /// the earlier parts left; while units were rejected or closed as not come back no part reaches the declared units, so their share is never refunded. The parts
    /// come in their receipt order and add up to at most the declared units.
    /// </summary>
    public static IReadOnlyList<ItemAmounts> SetAsideParts(ItemAmounts declaration, int declaredQuantity, IReadOnlyList<int> partQuantities)
    {
        ArgumentNullException.ThrowIfNull(partQuantities);
        ArgumentOutOfRangeException.ThrowIfLessThan(declaredQuantity, 1);
        if (partQuantities.Any(q => q < 1) || partQuantities.Sum() > declaredQuantity)
        {
            throw new ArgumentOutOfRangeException(nameof(partQuantities), "The parts are at least one unit each and at most the declared units together.");
        }

        ItemFrozenAmounts asItem = new(declaration.ProductAmount / declaredQuantity, declaration.CouponShare, declaration.VatAmount);
        List<ItemAmounts> parts = [];
        int received = 0;
        decimal couponSetAside = 0m;
        decimal vatSetAside = 0m;
        foreach (int quantity in partQuantities)
        {
            received += quantity;
            ItemAmounts part = SetAside(asItem, declaredQuantity, quantity, received == declaredQuantity, couponSetAside, vatSetAside);
            couponSetAside += part.CouponShare;
            vatSetAside += part.VatAmount;
            parts.Add(part);
        }

        return parts;
    }

    /// <summary>The amount a record's refund pays back (02 7.1.7 (2); 06 4.33 <c>Amount</c>): the product amount less the coupon share; the shipping fee is added apart.</summary>
    public static decimal RefundOf(ItemAmounts amounts) => amounts.ProductAmount - amounts.CouponShare;
}
