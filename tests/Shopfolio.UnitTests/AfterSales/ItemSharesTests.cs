using Shopfolio.Modules.AfterSales;
using Shopfolio.Modules.AfterSales.Contracts;

namespace Shopfolio.UnitTests.AfterSales;

/// <summary>
/// The amounts an item record sets aside from its item (T367 criterion 7, KK7; 02 7.1.7 (1)-(4); 09 14.1 item 2): the product amount is the unit price times
/// the units, the coupon share and the VAT are the units' share rounded down, the record that closes the item's last open units takes what is left, and the
/// shares of all the item's records add up to the item's own share to the kuruş. The example is 02 7.1.7's own: three units of 300 with a coupon share of 100.
/// </summary>
public sealed class ItemSharesTests
{
    private static readonly ItemFrozenAmounts Frozen = new(300m, 100m, 50m);

    [Fact]
    public void ThreeSingleUnitCancellations_SetAside33_33_33_33_AndTheLast33_34_TheRefunds266_67_266_67_266_66()
    {
        ItemAmounts first = ItemShares.SetAside(Frozen, 3, 1, false, 0m, 0m);
        ItemAmounts second = ItemShares.SetAside(Frozen, 3, 1, false, first.CouponShare, first.VatAmount);
        ItemAmounts last = ItemShares.SetAside(Frozen, 3, 1, true, first.CouponShare + second.CouponShare, first.VatAmount + second.VatAmount);

        Assert.Equal([33.33m, 33.33m, 33.34m], new[] { first.CouponShare, second.CouponShare, last.CouponShare });
        Assert.Equal([266.67m, 266.67m, 266.66m], new[] { first, second, last }.Select(a => a.ProductAmount - a.CouponShare));
        Assert.Equal([16.66m, 16.66m, 16.68m], new[] { first.VatAmount, second.VatAmount, last.VatAmount });
        Assert.Equal(Frozen.CouponShare, first.CouponShare + second.CouponShare + last.CouponShare);
        Assert.Equal(Frozen.VatAmount, first.VatAmount + second.VatAmount + last.VatAmount);
        Assert.All(new[] { first, second, last }, a => Assert.Equal(Frozen.UnitPrice, a.ProductAmount));
    }

    [Fact]
    public void TwoUnitsThenTheLastUnit_SetAside66_66_AndTheRest()
    {
        ItemAmounts two = ItemShares.SetAside(Frozen, 3, 2, false, 0m, 0m);
        ItemAmounts last = ItemShares.SetAside(Frozen, 3, 1, true, two.CouponShare, two.VatAmount);

        Assert.Equal((600m, 66.66m, 33.33m), (two.ProductAmount, two.CouponShare, two.VatAmount));
        Assert.Equal((300m, 33.34m, 16.67m), (last.ProductAmount, last.CouponShare, last.VatAmount));
    }

    [Fact]
    public void TheWholeItemAtOnce_TakesTheItemsWholeShare()
    {
        ItemAmounts all = ItemShares.SetAside(Frozen, 3, 3, true, 0m, 0m);

        Assert.Equal((900m, Frozen.CouponShare, Frozen.VatAmount), (all.ProductAmount, all.CouponShare, all.VatAmount));
    }

    [Fact]
    public void NoRecordTakesMoreThanIsLeft_SoNoRunningTotalPassesTheItemsShare()
    {
        // Earlier records already set aside the whole share (an earlier rounding of another record type): the next one takes nothing, the last one too.
        ItemAmounts next = ItemShares.SetAside(Frozen, 3, 1, false, 100m, 50m);
        ItemAmounts last = ItemShares.SetAside(Frozen, 3, 1, true, 100m, 50m);

        Assert.Equal((0m, 0m), (next.CouponShare, next.VatAmount));
        Assert.Equal((0m, 0m), (last.CouponShare, last.VatAmount));
    }

    [Fact]
    public void TheProductAmountIsNotRounded_AndTheWholeItemTakesItsWholeShareAndVat()
    {
        // 02 7.1.7 (1): the unit price times the units, no rounding (a half-kuruş unit price stays as it is) - T372's withdrawal example.
        ItemFrozenAmounts frozen = new(33.335m, 10.01m, 11.11m);

        ItemAmounts whole = ItemShares.SetAside(frozen, 2, 2, true, 0m, 0m);
        ItemAmounts part = ItemShares.SetAside(frozen, 2, 1, false, 0m, 0m);

        Assert.Equal((66.67m, 10.01m, 11.11m), (whole.ProductAmount, whole.CouponShare, whole.VatAmount));
        Assert.Equal((33.335m, 5.00m, 5.55m), (part.ProductAmount, part.CouponShare, part.VatAmount));
    }

    [Fact]
    public void AnItemWithoutACoupon_SetsAsideNoCouponShare()
    {
        ItemAmounts amounts = ItemShares.SetAside(Frozen with { CouponShare = 0m }, 3, 1, false, 0m, 0m);

        Assert.Equal(0m, amounts.CouponShare);
    }
}
