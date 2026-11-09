using Shopfolio.Infrastructure;
using Shopfolio.Modules.AfterSales;
using Shopfolio.Modules.AfterSales.Contracts;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Ordering;
using Shopfolio.Modules.Ordering.Contracts;

namespace Shopfolio.UnitTests.AfterSales;

/// <summary>
/// The pure rules of the withdrawal declaration (T372 criteria 4, 6, 7; KK4, KK6, KK7; 02 7.1.7, 10.4.10, 4.2 Z-38): the one split of an item's amounts - the units'
/// coupon share and VAT rounded down to the kuruş and the last open units taking the rest, so the item's records add up to its frozen values -, the warnings the firm
/// is shown on a notice from another channel (outside the window, beyond its year, an absolute exemption, the delivery day's question) and the IBAN's Z-38 deletion day.
/// </summary>
public sealed class WithdrawalDeclarationRulesTests
{
    private static readonly DateOnly LastDay = new(2026, 11, 23);

    private static ItemPeriodState Period(DateOnly? lastDay, bool exempt = false, bool conditional = false) =>
        new(0, false, lastDay, false, null, null, exempt, conditional, 0, 0, 0, false);

    private static OperationItemFacts Item(ProductType type, ItemPeriodState period, DateOnly? delivery = null, bool marked = false) =>
        new(Guid.NewGuid(), type, 1, 1, delivery, marked, 0, period);

    // ---- 02 7.1.7: the one split ----

    [Fact]
    public void ThreeUnitsWithdrawnOneByOne_TakeTheRoundedDownShares_AndTheLastTakesTheRest_KK4()
    {
        // 3 x 300 TL with a 100 TL coupon share: 33.33 + 33.33 + 33.34 (02 7.1.7 (4)).
        const decimal unitPrice = 300m;
        const decimal coupon = 100m;
        const decimal vat = 150m;
        ItemAmounts first = ItemAmountSplit.Of(unitPrice, 3, coupon, vat, 1, false, 0m, 0m);
        ItemAmounts second = ItemAmountSplit.Of(unitPrice, 3, coupon, vat, 1, false, first.CouponShare, first.VatAmount);
        ItemAmounts last = ItemAmountSplit.Of(unitPrice, 3, coupon, vat, 1, true, first.CouponShare + second.CouponShare, first.VatAmount + second.VatAmount);

        Assert.Equal((unitPrice, 33.33m, 50m), (first.ProductAmount, first.CouponShare, first.VatAmount));
        Assert.Equal((33.33m, 50m), (second.CouponShare, second.VatAmount));
        Assert.Equal((33.34m, 50m), (last.CouponShare, last.VatAmount));
        Assert.Equal(coupon, first.CouponShare + second.CouponShare + last.CouponShare);
        Assert.Equal(vat, first.VatAmount + second.VatAmount + last.VatAmount);
    }

    [Fact]
    public void TheWholeItemAtOnce_TakesItsWholeShareAndVat_AndTheProductAmountIsNotRounded()
    {
        ItemAmounts whole = ItemAmountSplit.Of(33.335m, 2, 10.01m, 11.11m, 2, true, 0m, 0m);
        Assert.Equal((66.67m, 10.01m, 11.11m), (whole.ProductAmount, whole.CouponShare, whole.VatAmount));

        ItemAmounts part = ItemAmountSplit.Of(33.335m, 2, 10.01m, 11.11m, 1, false, 0m, 0m);
        Assert.Equal((33.335m, 5.00m, 5.55m), (part.ProductAmount, part.CouponShare, part.VatAmount));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(4)]
    public void UnitsOutsideTheItem_AreAProgrammingError(int quantity) =>
        Assert.Throws<ArgumentOutOfRangeException>(() => ItemAmountSplit.Of(10m, 3, 0m, 0m, quantity, false, 0m, 0m));

    // ---- 02 10.4.10: the warnings of a notice from another channel ----

    [Fact]
    public void TheWindowsLastDay_IsInside_AndTheNextDayIsOutside_KK6()
    {
        OperationItemFacts item = Item(ProductType.Physical, Period(LastDay), LastDay.AddDays(-14), marked: true);

        Assert.Equal(WithdrawalNoticeWarnings.None, WithdrawalNoticeRules.Warnings(item, LastDay));
        WithdrawalNoticeWarnings late = WithdrawalNoticeRules.Warnings(item, LastDay.AddDays(1));
        Assert.True(late.OutsideWindow);
        Assert.False(late.BeyondOneYear);
        Assert.True(late.NeedsConfirmation);
    }

    [Fact]
    public void AYearAfterTheWindowsEnd_IsStillOnlyOutside_AndTheDayAfterIsBeyondTheYear_KK6()
    {
        OperationItemFacts item = Item(ProductType.Physical, Period(LastDay), LastDay.AddDays(-14), marked: true);

        DateOnly yearEnd = LastDay.AddYears(WithdrawalNoticeRules.ExtendedYears);
        Assert.False(WithdrawalNoticeRules.Warnings(item, yearEnd).BeyondOneYear);
        WithdrawalNoticeWarnings beyond = WithdrawalNoticeRules.Warnings(item, yearEnd.AddDays(1));
        Assert.True(beyond.BeyondOneYear && beyond.OutsideWindow && beyond.NeedsConfirmation);
    }

    [Fact]
    public void AServiceItemsCompletedMark_PutsANoticeOnOrAfterItsDayOutside_EvenInsideTheWindow_KK6()
    {
        DateOnly marked = LastDay.AddDays(-5);
        OperationItemFacts item = Item(ProductType.Service, Period(LastDay), marked, marked: true);

        Assert.False(WithdrawalNoticeRules.Warnings(item, marked.AddDays(-1)).OutsideWindow);
        Assert.True(WithdrawalNoticeRules.Warnings(item, marked).OutsideWindow);

        // A service item is never asked the delivery day's question (K-608 is the goods' delivery).
        Assert.False(WithdrawalNoticeRules.Warnings(item, marked).AsksDeliveryOrder);
    }

    [Fact]
    public void AnAbsoluteExemption_AsksTheConfirmation_AndAConditionalOneDoesNot_KK6()
    {
        DateOnly delivered = LastDay.AddDays(-14);
        WithdrawalNoticeWarnings absolute = WithdrawalNoticeRules.Warnings(Item(ProductType.Physical, Period(LastDay, exempt: true), delivered, true), LastDay);
        WithdrawalNoticeWarnings conditional = WithdrawalNoticeRules.Warnings(
            Item(ProductType.Physical, Period(LastDay, exempt: true, conditional: true), delivered, true), LastDay);

        Assert.True(absolute.AbsoluteExemption && absolute.NeedsConfirmation);
        Assert.False(conditional.AbsoluteExemption || conditional.NeedsConfirmation);
    }

    [Fact]
    public void ANoticeOnThePhysicalItemsDeliveryDay_AsksWhetherTheDeliveryCameFirst_WithoutAConfirmation_KK6()
    {
        DateOnly delivered = LastDay.AddDays(-14);
        OperationItemFacts item = Item(ProductType.Physical, Period(LastDay), delivered, marked: true);

        WithdrawalNoticeWarnings sameDay = WithdrawalNoticeRules.Warnings(item, delivered);
        Assert.True(sameDay.AsksDeliveryOrder);
        Assert.False(sameDay.NeedsConfirmation);
        Assert.False(WithdrawalNoticeRules.Warnings(item, delivered.AddDays(1)).AsksDeliveryOrder);

        // In transit there is no delivery date and no window yet: nothing to warn about.
        Assert.Equal(WithdrawalNoticeWarnings.None, WithdrawalNoticeRules.Warnings(Item(ProductType.Physical, Period(null)), delivered));
    }

    // ---- Z-38: the IBAN's deletion day ----

    [Fact]
    public void TheReturnIbansDeletionDay_IsP43DaysAfterTheSendingPeriodsEnd_KK7()
    {
        DateOnly declared = new(2026, 11, 9);
        DateOnly sendingLastDay = PeriodCounting.LastDay(
            CountedPeriod.ReturnShipmentPeriod, declared, AfterSalesPeriodRules.ReturnShipmentPeriodDays, BusinessCalendar.Product);

        DateTimeOffset deletion = AfterSalesPeriodRules.ReturnIbanDeletionAt(declared);

        Assert.Equal(TurkeyTime.StartOf(sendingLastDay.AddDays(1)).AddDays(ProductConstants.ReturnIbanRetentionDays), deletion);
        Assert.True(deletion > TurkeyTime.StartOf(declared.AddDays(AfterSalesPeriodRules.ReturnShipmentPeriodDays + ProductConstants.ReturnIbanRetentionDays)));
    }
}
