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
}
