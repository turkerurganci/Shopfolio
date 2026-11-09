using Shopfolio.Modules.AfterSales;
using Shopfolio.Modules.AfterSales.Contracts;

namespace Shopfolio.UnitTests.AfterSales;

/// <summary>
/// The refund amount rules of T369 (02 7.1.7, 7.2.7, 7.2.9, 7.4.6): the shares and refunds of the records (<see cref="ItemShares"/>), the parts a withdrawal is received
/// in (KK5) and the shipping fee's one refund (<see cref="ShippingFeeRefunds"/>; KK7, KK8). The example is 02 7.1.7's own: three units of 300 with a coupon share of 100.
/// </summary>
public sealed class RefundAmountRulesTests
{
    private static readonly ItemFrozenAmounts Frozen = new(300m, 100m, 50m);

    private static readonly DateTimeOffset T0 = new(2026, 11, 9, 9, 0, 0, TimeSpan.Zero);

    // The item ids in the database's uuid order (the hexadecimal text order).
    private static readonly Guid ItemA = Guid.Parse("10000000-0000-0000-0000-000000000000");

    private static readonly Guid ItemB = Guid.Parse("20000000-0000-0000-0000-000000000000");

    // ---- KK3, KK4: the shares and the refunds ----

    [Fact]
    public void KK3_ThreeUnitsOf300WithACoupon100_Shares33_33_33_33_33_34_Refunds266_67_266_67_266_66()
    {
        ItemAmounts first = ItemShares.SetAside(Frozen, 3, 1, false, 0m, 0m);
        ItemAmounts second = ItemShares.SetAside(Frozen, 3, 1, false, first.CouponShare, first.VatAmount);
        ItemAmounts third = ItemShares.SetAside(Frozen, 3, 1, true, first.CouponShare + second.CouponShare, first.VatAmount + second.VatAmount);

        Assert.Equal([33.33m, 33.33m, 33.34m], new[] { first, second, third }.Select(a => a.CouponShare));
        Assert.Equal([266.67m, 266.67m, 266.66m], new[] { first, second, third }.Select(ItemShares.RefundOf));
        Assert.Equal((Frozen.UnitPrice * 3) - Frozen.CouponShare, new[] { first, second, third }.Sum(ItemShares.RefundOf));
    }

    [Theory]
    [InlineData(new[] { 1, 1, 1, 1, 1, 1, 1 })]
    [InlineData(new[] { 2, 3, 2 })]
    [InlineData(new[] { 6, 1 })]
    [InlineData(new[] { 1, 5, 1 })]
    public void KK4_WhateverTheSplit_TheSharesAddUpToTheItemsShareAndVat_AndNoRunningTotalPassesThem(int[] split)
    {
        ItemFrozenAmounts frozen = new(19.99m, 12.34m, 23.31m);
        int quantity = split.Sum();
        decimal coupon = 0m;
        decimal vat = 0m;
        int closed = 0;
        foreach (int units in split)
        {
            closed += units;
            ItemAmounts part = ItemShares.SetAside(frozen, quantity, units, closed == quantity, coupon, vat);
            coupon += part.CouponShare;
            vat += part.VatAmount;
            Assert.True(coupon <= frozen.CouponShare && vat <= frozen.VatAmount);
        }

        Assert.Equal((frozen.CouponShare, frozen.VatAmount), (coupon, vat));
    }

    // ---- KK6: the partial cancellation does not spread the coupon over the units left ----

    [Fact]
    public void KK6_APartialCancellation_SetsAsideOnlyItsUnitsShare_TheNextRecordSplitsTheItemsFrozenShare_NotTheRest()
    {
        // 0,05 over three units: the cancelled unit takes 0,01. Spread over the two left the rest would make 0,02 a unit; the rule splits the frozen share again: 0,01.
        ItemFrozenAmounts frozen = new(10m, 0.05m, 0m);
        ItemAmounts cancelled = ItemShares.SetAside(frozen, 3, 1, false, 0m, 0m);
        ItemAmounts next = ItemShares.SetAside(frozen, 3, 1, false, cancelled.CouponShare, 0m);
        ItemAmounts last = ItemShares.SetAside(frozen, 3, 1, true, cancelled.CouponShare + next.CouponShare, 0m);

        Assert.Equal([0.01m, 0.01m, 0.03m], new[] { cancelled.CouponShare, next.CouponShare, last.CouponShare });
    }

    // ---- KK5: the parts of a withdrawal received piece by piece ----

    [Fact]
    public void KK5_ThePartsOfADeclaration_TakeTheirUnitsShareOfTheDeclarationsShare_TheLastPartTheRest()
    {
        // The declaration of all three units took the item's whole share (100, VAT 50); received in three pieces.
        ItemAmounts declaration = ItemShares.SetAside(Frozen, 3, 3, true, 0m, 0m);

        IReadOnlyList<ItemAmounts> parts = ItemShares.SetAsideParts(declaration, 3, [1, 1, 1]);

        Assert.Equal([33.33m, 33.33m, 33.34m], parts.Select(p => p.CouponShare));
        Assert.Equal([16.66m, 16.66m, 16.68m], parts.Select(p => p.VatAmount));
        Assert.Equal([300m, 300m, 300m], parts.Select(p => p.ProductAmount));
        Assert.Equal([266.67m, 266.67m, 266.66m], parts.Select(ItemShares.RefundOf));
    }

    [Fact]
    public void KK5_ThePartsSplitTheDeclarationsOwnShare_NotTheItems()
    {
        // Two of three units declared: the declaration took 66,66; its two parts split that, the last taking the rest: 33,33 and 33,33.
        ItemAmounts declaration = ItemShares.SetAside(Frozen, 3, 2, false, 0m, 0m);

        IReadOnlyList<ItemAmounts> parts = ItemShares.SetAsideParts(declaration, 2, [1, 1]);

        Assert.Equal(66.66m, declaration.CouponShare);
        Assert.Equal([33.33m, 33.33m], parts.Select(p => p.CouponShare));
        Assert.Equal(declaration.CouponShare, parts.Sum(p => p.CouponShare));
    }

    [Fact]
    public void KK5_ARejectedOrNotReturnedUnit_KeepsItsShare_NoPartTakesTheRest()
    {
        ItemAmounts declaration = ItemShares.SetAside(Frozen, 3, 3, true, 0m, 0m);

        // One unit rejected: the two received units are two parts of three; neither reaches the declared units.
        IReadOnlyList<ItemAmounts> parts = ItemShares.SetAsideParts(declaration, 3, [1, 1]);

        Assert.Equal([33.33m, 33.33m], parts.Select(p => p.CouponShare));
        Assert.True(parts.Sum(p => p.CouponShare) < declaration.CouponShare);
    }

    [Fact]
    public void KK5_PartsAboveTheDeclaredUnits_OrAnEmptyPart_Throw()
    {
        ItemAmounts declaration = new(600m, 66.66m, 33.33m);

        Assert.Throws<ArgumentOutOfRangeException>(() => ItemShares.SetAsideParts(declaration, 2, [2, 1]));
        Assert.Throws<ArgumentOutOfRangeException>(() => ItemShares.SetAsideParts(declaration, 2, [0]));
    }

    // ---- KK7: the shipping fee on the cancellation ----

    [Fact]
    public void KK7_TheLastCancellationOfAllPhysicalUnitsBeforeTheShipment_CarriesTheFee_TheEarlierOneNot()
    {
        Dictionary<Guid, int> units = new() { [ItemA] = 2, [ItemB] = 1 };
        UnitMove first = new(ItemA, Guid.NewGuid(), T0, 2);
        UnitMove last = new(ItemB, Guid.NewGuid(), T0.AddHours(1), 1);

        Assert.False(ShippingFeeRefunds.OnCancellationBeforeShipment(units, [first, last], first));
        Assert.True(ShippingFeeRefunds.OnCancellationBeforeShipment(units, [first, last], last));
    }

    [Fact]
    public void KK7_OneUnitStillGoing_KeepsTheFee()
    {
        Dictionary<Guid, int> units = new() { [ItemA] = 2, [ItemB] = 1 };
        UnitMove partial = new(ItemA, Guid.NewGuid(), T0, 1);
        UnitMove other = new(ItemB, Guid.NewGuid(), T0.AddHours(1), 1);

        Assert.False(ShippingFeeRefunds.OnCancellationBeforeShipment(units, [partial, other], other));
    }

    [Fact]
    public void KK7_OneOperationOverTwoItems_TheFeeGoesToTheSmallestItemId()
    {
        Dictionary<Guid, int> units = new() { [ItemA] = 1, [ItemB] = 1 };
        UnitMove a = new(ItemA, Guid.NewGuid(), T0, 1);
        UnitMove b = new(ItemB, Guid.NewGuid(), T0, 1);

        Assert.True(ShippingFeeRefunds.OnCancellationBeforeShipment(units, [b, a], a));
        Assert.False(ShippingFeeRefunds.OnCancellationBeforeShipment(units, [b, a], b));
    }

    [Theory]
    [InlineData(CancellationReason.UndeliveredBuyerCaused, false)]
    [InlineData(CancellationReason.CannotShipToAddress, true)]
    [InlineData(CancellationReason.DamagedOrUnsellable, true)]
    [InlineData(CancellationReason.Other, true)]
    public void KK7_TheS9Cancellation_PaysTheFeeBack_UnlessTheReasonIsTheCustomers(CancellationReason reason, bool included)
    {
        UnitMove s9 = new(ItemA, Guid.NewGuid(), T0, 1);

        Assert.Equal(included, ShippingFeeRefunds.OnCancellationAfterShipment(reason, [s9], s9));
    }

    [Fact]
    public void KK7_ACancellationAfterTheShipmentWithoutAReason_IsNotS9_AndCarriesNoFee()
    {
        UnitMove move = new(ItemA, Guid.NewGuid(), T0, 1);

        Assert.False(ShippingFeeRefunds.OnCancellationAfterShipment(null, [move], move));
    }

    [Fact]
    public void KK7_TheTermination_AlwaysCarriesTheFee_OnceForTheOperation()
    {
        UnitMove a = new(ItemA, Guid.NewGuid(), T0, 2);
        UnitMove b = new(ItemB, Guid.NewGuid(), T0, 1);

        Assert.True(ShippingFeeRefunds.OnTermination([a, b], a));
        Assert.False(ShippingFeeRefunds.OnTermination([a, b], b));
    }

    // ---- KK8: the shipping fee on the withdrawal ----

    [Fact]
    public void KK8_TheReceiptOfTheLastShippedUnits_CarriesTheFee_TheEarlierReceiptNot()
    {
        Dictionary<Guid, int> shipped = new() { [ItemA] = 2 };
        UnitMove first = new(ItemA, Guid.NewGuid(), T0, 1);
        UnitMove last = new(ItemA, Guid.NewGuid(), T0.AddDays(1), 1);

        Assert.False(ShippingFeeRefunds.OnWithdrawal(shipped, [first, last], 0, first));
        Assert.True(ShippingFeeRefunds.OnWithdrawal(shipped, [first, last], 0, last));
    }

    [Fact]
    public void KK8_ARejectedOrNotReturnedUnit_StaysWithTheCustomer_AndKeepsTheFee()
    {
        Dictionary<Guid, int> shipped = new() { [ItemA] = 2 };
        UnitMove first = new(ItemA, Guid.NewGuid(), T0, 1);
        UnitMove last = new(ItemA, Guid.NewGuid(), T0.AddDays(1), 1);

        Assert.False(ShippingFeeRefunds.OnWithdrawal(shipped, [first, last], 1, last));
    }

    [Fact]
    public void KK8_AnItemCancelledBeforeTheShipment_IsLeftOut_OfTheUnitsToComeBack()
    {
        // B was cancelled before the shipment: nothing of it was shipped, so A's units alone are the trigger.
        Dictionary<Guid, int> shipped = new() { [ItemA] = 1, [ItemB] = 0 };
        UnitMove back = new(ItemA, Guid.NewGuid(), T0, 1);

        Assert.True(ShippingFeeRefunds.OnWithdrawal(shipped, [back], 0, back));
    }

    [Fact]
    public void KK8_AnOrderWithNothingShipped_HasNoTrigger()
    {
        Dictionary<Guid, int> shipped = new() { [ItemA] = 0 };
        UnitMove move = new(ItemA, Guid.NewGuid(), T0, 1);

        Assert.False(ShippingFeeRefunds.OnWithdrawal(shipped, [move], 0, move));
    }
}
