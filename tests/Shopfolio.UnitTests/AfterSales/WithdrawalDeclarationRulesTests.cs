using Shopfolio.Infrastructure;
using Shopfolio.Modules.AfterSales;
using Shopfolio.Modules.AfterSales.Contracts;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Ordering;
using Shopfolio.Modules.Ordering.Contracts;

namespace Shopfolio.UnitTests.AfterSales;

/// <summary>
/// The pure rules of the withdrawal declaration (T372 criteria 6, 7; KK6, KK7; 02 10.4.10, 4.2 Z-38): the warnings the firm is shown on a notice from another channel
/// (outside the window, beyond its year, an absolute exemption, the delivery day's question) and the IBAN's Z-38 deletion day. The split of an item's amounts (criterion 4,
/// KK4; 02 7.1.7) is the item records' one function, <see cref="ItemShares"/>, tested in <see cref="ItemSharesTests"/>; the declaration's own records in Integration.
/// </summary>
public sealed class WithdrawalDeclarationRulesTests
{
    private static readonly DateOnly LastDay = new(2026, 11, 23);

    private static ItemPeriodState Period(DateOnly? lastDay, bool exempt = false, bool conditional = false) =>
        new(0, false, lastDay, false, null, null, exempt, conditional, 0, 0, 0, false);

    private static OperationItemFacts Item(ProductType type, ItemPeriodState period, DateOnly? delivery = null, bool marked = false) =>
        new(Guid.NewGuid(), type, 1, 1, delivery, marked, 0, period);

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
