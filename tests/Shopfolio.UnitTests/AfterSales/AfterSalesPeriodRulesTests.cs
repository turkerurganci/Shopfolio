using Shopfolio.Infrastructure;
using Shopfolio.Modules.AfterSales;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Modules.Payment.Contracts;

namespace Shopfolio.UnitTests.AfterSales;

/// <summary>
/// The withdrawal, refund and defect claim periods' rules (T325 criteria 4-8, KK4-KK8; 03 4.1.14-4.1.21; 02 4.2 Z-13-Z-18, Z-42): the physical window from
/// the delivery date and none without it, the service window from the order's date or the order's delivery date and closed by the "tamamlandı" mark,
/// the digital right that fell at the approval, the refund's start before and after the delivery, the card line that shows no period, the sending
/// period that opens the "goods did not come back" closing, and the two-year defect claim channel. Every edge is the last moment of the last day
/// against 00:00 of the next day in Turkey time; the lengths come from the rule's constants.
/// </summary>
public sealed class AfterSalesPeriodRulesTests
{
    // Monday 9 November 2026.
    private static readonly DateOnly Day = new(2026, 11, 9);

    private static readonly DateTimeOffset OrderedAt = TurkeyTime.StartOf(Day).AddHours(10);

    private static DateTimeOffset EndAfter(DateOnly start, int days) => TurkeyTime.StartOf(start.AddDays(days + 1));

    private static WithdrawalWindowFacts Physical(DateOnly? delivered) => new(ProductType.Physical, delivered, delivered is null ? null : OrderedAt, OrderedAt, true, delivered);

    private static WithdrawalWindowFacts Service(bool orderHasPhysical, DateOnly? orderDelivered, DateTimeOffset? completedAt = null) =>
        new(ProductType.Service, completedAt is null ? null : TurkeyTime.DateOf(completedAt.Value), completedAt, OrderedAt, orderHasPhysical, orderDelivered);

    // ---- Z-13 ----

    [Fact]
    public void PhysicalWindow_RunsFourteenDaysFromTheDeliveryDate_AndThenCloses_KK5()
    {
        DateTimeOffset end = EndAfter(Day, AfterSalesPeriodRules.WithdrawalWindowDays);

        Assert.Equal(end, AfterSalesPeriodRules.WithdrawalWindowEnd(Day));
        Assert.True(AfterSalesPeriodRules.IsWithdrawalWindowOpen(Physical(Day), end.AddTicks(-1)));
        Assert.False(AfterSalesPeriodRules.IsWithdrawalWindowOpen(Physical(Day), end));
    }

    [Fact]
    public void PhysicalWindow_WithoutADeliveryDate_DoesNotStart_AndTheButtonDoesNotClose_KK4()
    {
        Assert.True(AfterSalesPeriodRules.IsWithdrawalWindowOpen(Physical(null), OrderedAt.AddYears(3)));
    }

    // ---- Z-14 ----

    [Fact]
    public void ServiceWindow_OnAnOrderWithoutAPhysicalItem_RunsFromTheOrdersDate()
    {
        DateTimeOffset end = EndAfter(Day, AfterSalesPeriodRules.WithdrawalWindowDays);

        Assert.Equal(Day, AfterSalesPeriodRules.ServiceWithdrawalStartDay(OrderedAt, false, null));
        Assert.True(AfterSalesPeriodRules.IsWithdrawalWindowOpen(Service(false, null), end.AddTicks(-1)));
        Assert.False(AfterSalesPeriodRules.IsWithdrawalWindowOpen(Service(false, null), end));
    }

    [Fact]
    public void ServiceWindow_TheCompletedMark_ClosesTheButtonBeforeTheWindowsEnd_KK5()
    {
        DateTimeOffset completedAt = OrderedAt.AddDays(2);

        Assert.False(AfterSalesPeriodRules.IsWithdrawalWindowOpen(Service(false, null, completedAt), completedAt.AddMinutes(1)));
    }

    [Fact]
    public void ServiceWindow_OnAnOrderWithAPhysicalItem_RunsFromTheOrdersDeliveryDate_AndWithoutItLastsUntilTheMark()
    {
        DateOnly delivered = Day.AddDays(20);
        DateTimeOffset end = EndAfter(delivered, AfterSalesPeriodRules.WithdrawalWindowDays);

        Assert.Equal(delivered, AfterSalesPeriodRules.ServiceWithdrawalStartDay(OrderedAt, true, delivered));
        Assert.True(AfterSalesPeriodRules.IsWithdrawalWindowOpen(Service(true, delivered), end.AddTicks(-1)));
        Assert.False(AfterSalesPeriodRules.IsWithdrawalWindowOpen(Service(true, delivered), end));

        // Not entered, or the physical items all closed before the delivery: no start, and not pulled back to the order's date (03 3.3.29).
        Assert.Null(AfterSalesPeriodRules.ServiceWithdrawalStartDay(OrderedAt, true, null));
        Assert.True(AfterSalesPeriodRules.IsWithdrawalWindowOpen(Service(true, null), OrderedAt.AddYears(1)));
        Assert.False(AfterSalesPeriodRules.IsWithdrawalWindowOpen(Service(true, null, OrderedAt.AddYears(1)), OrderedAt.AddYears(1)));
    }

    // ---- Z-15 ----

    [Fact]
    public void DigitalItem_HasNoWithdrawalWindow()
    {
        WithdrawalWindowFacts digital = new(ProductType.Digital, Day, OrderedAt, OrderedAt, false, null);

        Assert.False(AfterSalesPeriodRules.IsWithdrawalWindowOpen(digital, OrderedAt));
    }

    // ---- Z-16 ----

    [Fact]
    public void AfterDelivery_IsTheDeliveryBeforeTheDeclaration_OrTheFirmsAnswerOnTheSameDay()
    {
        Assert.True(AfterSalesPeriodRules.IsAfterDelivery(ProductType.Physical, Day, Day.AddDays(1), null));
        Assert.True(AfterSalesPeriodRules.IsAfterDelivery(ProductType.Physical, Day, Day, true));
        Assert.False(AfterSalesPeriodRules.IsAfterDelivery(ProductType.Physical, Day, Day, false));
        Assert.False(AfterSalesPeriodRules.IsAfterDelivery(ProductType.Physical, Day.AddDays(1), Day, null));
        Assert.False(AfterSalesPeriodRules.IsAfterDelivery(ProductType.Physical, null, Day, null));
        Assert.False(AfterSalesPeriodRules.IsAfterDelivery(ProductType.Service, Day, Day.AddDays(1), null));
    }

    [Fact]
    public void WithdrawalRefund_StartsAtTheDeclaration_BeforeTheDelivery_AndAtTheLaterOfArrivalAndDeclaration_After()
    {
        DateOnly declared = Day.AddDays(3);

        Assert.Equal(declared, AfterSalesPeriodRules.WithdrawalRefundStartDay(false, declared, null));
        Assert.Equal(declared.AddDays(4), AfterSalesPeriodRules.WithdrawalRefundStartDay(true, declared, declared.AddDays(4)));
        Assert.Equal(declared, AfterSalesPeriodRules.WithdrawalRefundStartDay(true, declared, declared.AddDays(-1)));
        Assert.Null(AfterSalesPeriodRules.WithdrawalRefundStartDay(true, declared, null));
    }

    [Fact]
    public void RefundPeriod_ShowsTheDaysLeft_AndTurnsOverWithoutAnAction_KK6()
    {
        DateTimeOffset end = EndAfter(Day, AfterSalesPeriodRules.RefundPeriodDays);

        PeriodStanding first = AfterSalesPeriodRules.RefundStanding(Day, TurkeyTime.StartOf(Day.AddDays(1)));
        PeriodStanding last = AfterSalesPeriodRules.RefundStanding(Day, end.AddTicks(-1));
        PeriodStanding over = AfterSalesPeriodRules.RefundStanding(Day, end);

        Assert.Equal(AfterSalesPeriodRules.RefundPeriodDays - 1, first.DaysLeft);
        Assert.Equal((0, false), (last.DaysLeft, last.IsOver));
        Assert.Equal((end, true), (over.EndsAt, over.IsOver));
    }

    // ---- Z-17 ----

    [Fact]
    public void CancellationRefund_ShowsItsPeriod_OnlyOnTheBankTransferLine_KK6()
    {
        DateTimeOffset cancelledAt = OrderedAt.AddDays(1);
        DateOnly cancelledDay = TurkeyTime.DateOf(cancelledAt);

        PeriodStanding? transfer = AfterSalesPeriodRules.CancellationRefundStanding(PaymentMethod.BankTransfer, cancelledAt, cancelledAt);
        Assert.NotNull(transfer);
        Assert.Equal(cancelledDay.AddDays(AfterSalesPeriodRules.RefundPeriodDays), transfer.Value.LastDay);
        Assert.Equal(AfterSalesPeriodRules.RefundPeriodDays, transfer.Value.DaysLeft);
        Assert.Null(AfterSalesPeriodRules.CancellationRefundStanding(PaymentMethod.Card, cancelledAt, cancelledAt));
    }

    // ---- Z-42 ----

    [Fact]
    public void NotReturnedClosure_OpensWhenTheSendingPeriodRunsOut_ForAnAwaitedWithdrawalAfterDelivery_KK7()
    {
        DateOnly declared = Day.AddDays(3);
        DateTimeOffset end = EndAfter(declared, AfterSalesPeriodRules.ReturnShipmentPeriodDays);

        Assert.Equal(end, AfterSalesPeriodRules.ReturnShipmentStanding(declared, end).EndsAt);
        Assert.False(AfterSalesPeriodRules.IsNotReturnedClosureOpen(true, 1, declared, end.AddTicks(-1)));
        Assert.True(AfterSalesPeriodRules.IsNotReturnedClosureOpen(true, 1, declared, end));
        Assert.False(AfterSalesPeriodRules.IsNotReturnedClosureOpen(true, 0, declared, end));
        Assert.False(AfterSalesPeriodRules.IsNotReturnedClosureOpen(false, 1, declared, end));
    }

    [Fact]
    public void AwaitedGoods_CountTheDaysSinceTheDeclaration()
    {
        DateOnly declared = Day.AddDays(3);

        Assert.Equal(0, AfterSalesPeriodRules.DaysSinceDeclaration(declared, TurkeyTime.StartOf(declared).AddHours(20)));
        Assert.Equal(16, AfterSalesPeriodRules.DaysSinceDeclaration(declared, TurkeyTime.StartOf(declared.AddDays(16))));
    }

    // ---- Z-18 ----

    [Fact]
    public void DefectClaimChannel_ClosesTwoYearsAfterTheDeliveryDate_KK8()
    {
        DateTimeOffset end = TurkeyTime.StartOf(Day.AddYears(AfterSalesPeriodRules.DefectClaimYears).AddDays(1));

        Assert.Equal(end, AfterSalesPeriodRules.DefectClaimChannelEnd(Day));
        Assert.True(AfterSalesPeriodRules.IsDefectClaimChannelOpen(ProductType.Physical, OrderStatus.Delivered, Day, end.AddTicks(-1)));
        Assert.False(AfterSalesPeriodRules.IsDefectClaimChannelOpen(ProductType.Physical, OrderStatus.Delivered, Day, end));
        Assert.False(AfterSalesPeriodRules.IsDefectClaimChannelOpen(ProductType.Service, OrderStatus.Delivered, Day, end));
        Assert.False(AfterSalesPeriodRules.IsDefectClaimChannelOpen(ProductType.Digital, OrderStatus.Delivered, Day, end));
    }

    [Fact]
    public void DefectClaimChannel_FromALeapDay_EndsAfterTheLastDayOfFebruary()
    {
        Assert.Equal(TurkeyTime.StartOf(new DateOnly(2030, 3, 1)), AfterSalesPeriodRules.DefectClaimChannelEnd(new DateOnly(2028, 2, 29)));
    }

    [Fact]
    public void DefectClaimChannel_OfAShippedPhysicalItemWithoutADeliveryDate_IsOpen_AndDoesNotClose_KK4()
    {
        Assert.True(AfterSalesPeriodRules.IsDefectClaimChannelOpen(ProductType.Physical, OrderStatus.Shipped, null, OrderedAt.AddYears(5)));
        Assert.False(AfterSalesPeriodRules.IsDefectClaimChannelOpen(ProductType.Physical, OrderStatus.Preparing, null, OrderedAt));
    }

    [Fact]
    public void DefectClaimChannel_OfAServiceItem_OpensWithTheCompletedMark_AndOfADigitalItem_WithTheApproval()
    {
        Assert.False(AfterSalesPeriodRules.IsDefectClaimChannelOpen(ProductType.Service, OrderStatus.Placed, null, OrderedAt));
        Assert.True(AfterSalesPeriodRules.IsDefectClaimChannelOpen(ProductType.Service, OrderStatus.Delivered, Day, OrderedAt));
        Assert.True(AfterSalesPeriodRules.IsDefectClaimChannelOpen(ProductType.Digital, OrderStatus.Delivered, Day, OrderedAt));
    }
}
