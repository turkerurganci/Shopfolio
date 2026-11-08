using Shopfolio.Infrastructure;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Ordering;
using Shopfolio.Modules.Ordering.Contracts;

namespace Shopfolio.UnitTests.Ordering;

/// <summary>
/// The delivery periods' rules (T325 criteria 1-4, KK1-KK4; 03 4.1.10-4.1.13; 02 4.2 Z-10, Z-11, Z-12, Z-39, 7.2.3; D-161): the shipping promise counted
/// in business days from the payment's approval - zero days included -, the fulfilment period in calendar days, the legal delivery limit from the order's
/// confirmation, the late delivery termination it opens (shipped orders included, not for an item with a delivery date) and the legal interest warning.
/// The reading of the payment's approval from the Ö1 records is in the integration tests.
/// </summary>
public sealed class DeliveryPeriodRulesTests
{
    // Monday 9 November 2026; the weeks around it carry no public holiday.
    private static readonly DateOnly Monday = new(2026, 11, 9);

    private static readonly DateTimeOffset ApprovedAt = TurkeyTime.StartOf(Monday).AddHours(14);

    private static DateTimeOffset Turkey(DateOnly day, int hour) => TurkeyTime.StartOf(day).AddHours(hour);

    // ---- Z-10 ----

    [Fact]
    public void ShippingPromise_CountsBusinessDays_FromTheApprovalsTurkeyDay_KK1()
    {
        // Two business days from Friday 13 November end on Tuesday 17 November; the weekend is not counted.
        DateTimeOffset friday = Turkey(new DateOnly(2026, 11, 13), 16);

        Assert.Equal(new DateOnly(2026, 11, 17), DeliveryPeriodRules.ShippingLastDay(friday, 2, BusinessCalendar.Product));
        Assert.Equal(
            PeriodCounting.EndOf(CountedPeriod.ShippingPeriod, friday, 2, BusinessCalendar.Product),
            DeliveryPeriodRules.ShippingPeriodEnd(friday, 2, BusinessCalendar.Product));

        // 23:30 in Turkey on Monday is still Monday (20:30 UTC): the count starts from Monday, not from the UTC day.
        DateTimeOffset lateMonday = Turkey(Monday, 23).AddMinutes(30);
        Assert.Equal(Monday.AddDays(1), DeliveryPeriodRules.ShippingLastDay(lateMonday, 1, BusinessCalendar.Product));
    }

    [Fact]
    public void ZeroDayShippingPromise_EndsAtTheEndOfTheApprovalsDay_WithoutAnException_D161()
    {
        // P-5's and P-6's lower fence is zero: the promise is shipping on the approval's day.
        Assert.Equal(0, ParameterFences.ShippingPeriodLower);
        Assert.Equal(Monday, DeliveryPeriodRules.ShippingLastDay(ApprovedAt, ParameterFences.ShippingPeriodLower, BusinessCalendar.Product));
        Assert.Equal(TurkeyTime.StartOf(Monday.AddDays(1)), DeliveryPeriodRules.ShippingPeriodEnd(ApprovedAt, 0, BusinessCalendar.Product));

        PeriodStanding sameDay = DeliveryPeriodRules.ShippingStanding(ApprovedAt, 0, ApprovedAt, BusinessCalendar.Product);
        PeriodStanding nextDay = DeliveryPeriodRules.ShippingStanding(ApprovedAt, 0, TurkeyTime.StartOf(Monday.AddDays(1)), BusinessCalendar.Product);
        Assert.Equal((0, false), (sameDay.DaysLeft, sameDay.IsOver));
        Assert.Equal((1, true), (nextDay.DaysOver, nextDay.IsOver));

        Assert.Throws<ArgumentOutOfRangeException>(() => DeliveryPeriodRules.ShippingLastDay(ApprovedAt, -1, BusinessCalendar.Product));
    }

    [Fact]
    public void ShippingPromise_UpperFence_IsCountedToo()
    {
        Assert.Equal(
            PeriodCounting.LastDay(CountedPeriod.ShippingPeriod, Monday, ParameterFences.ShippingPeriodUpper, BusinessCalendar.Product),
            DeliveryPeriodRules.ShippingLastDay(ApprovedAt, ParameterFences.ShippingPeriodUpper, BusinessCalendar.Product));
    }

    // ---- Z-39 ----

    [Fact]
    public void FulfilmentPeriod_CountsCalendarDays_FromTheApproval_KK1()
    {
        DateTimeOffset end = DeliveryPeriodRules.FulfilmentPeriodEnd(ApprovedAt, 3);

        Assert.Equal(TurkeyTime.StartOf(Monday.AddDays(3 + 1)), end);
        PeriodStanding running = DeliveryPeriodRules.FulfilmentStanding(ApprovedAt, 3, end.AddTicks(-1));
        PeriodStanding over = DeliveryPeriodRules.FulfilmentStanding(ApprovedAt, 3, end);
        Assert.Equal((Monday.AddDays(3), false), (running.LastDay, running.IsOver));
        Assert.True(over.IsOver);
    }

    // ---- Z-11 ----

    [Fact]
    public void LegalDeliveryLimit_IsThirtyCalendarDays_FromTheOrdersConfirmation()
    {
        DateTimeOffset createdAt = Turkey(Monday, 10);

        Assert.Equal(
            TurkeyTime.StartOf(Monday.AddDays(DeliveryPeriodRules.LegalDeliveryLimitDays + 1)),
            DeliveryPeriodRules.LegalDeliveryLimitEnd(createdAt));
    }

    [Fact]
    public void LateDeliveryTermination_OpensWhenTheLimitRunsOut_ForAPhysicalItemWithoutADeliveryDate_KK3()
    {
        DateTimeOffset createdAt = Turkey(Monday, 10);
        DateTimeOffset end = DeliveryPeriodRules.LegalDeliveryLimitEnd(createdAt);
        ItemDeliveryState[] waiting = [new(ProductType.Physical, 1, null)];

        Assert.False(DeliveryPeriodRules.IsLateDeliveryTerminationOpen(OrderStatus.Preparing, PaymentStatus.Paid, createdAt, waiting, end.AddTicks(-1)));
        Assert.True(DeliveryPeriodRules.IsLateDeliveryTerminationOpen(OrderStatus.Preparing, PaymentStatus.Paid, createdAt, waiting, end));
        Assert.True(DeliveryPeriodRules.IsLateDeliveryTerminationOpen(OrderStatus.Preparing, PaymentStatus.PartiallyRefunded, createdAt, waiting, end));
    }

    [Fact]
    public void LateDeliveryTermination_OpensOnAShippedOrderToo_KK3()
    {
        DateTimeOffset createdAt = Turkey(Monday, 10);
        DateTimeOffset end = DeliveryPeriodRules.LegalDeliveryLimitEnd(createdAt);

        Assert.True(DeliveryPeriodRules.IsLateDeliveryTerminationOpen(
            OrderStatus.Shipped, PaymentStatus.Paid, createdAt, [new(ProductType.Physical, 2, null)], end));
    }

    [Fact]
    public void LateDeliveryTermination_StaysClosed_WithADeliveryDate_NoOpenQuantity_NoPhysicalItem_OrNoConfirmedPayment_KK4()
    {
        DateTimeOffset createdAt = Turkey(Monday, 10);
        DateTimeOffset late = DeliveryPeriodRules.LegalDeliveryLimitEnd(createdAt).AddDays(5);

        // Z-12 is not a period: an entered delivery date closes the way, whatever the date.
        Assert.False(DeliveryPeriodRules.IsLateDeliveryTerminationOpen(
            OrderStatus.Delivered, PaymentStatus.Paid, createdAt, [new(ProductType.Physical, 1, Monday.AddDays(40))], late));
        Assert.False(DeliveryPeriodRules.IsLateDeliveryTerminationOpen(
            OrderStatus.Preparing, PaymentStatus.Paid, createdAt, [new(ProductType.Physical, 0, null)], late));
        Assert.False(DeliveryPeriodRules.IsLateDeliveryTerminationOpen(
            OrderStatus.Placed, PaymentStatus.Paid, createdAt, [new(ProductType.Service, 1, null), new(ProductType.Digital, 1, null)], late));
        Assert.False(DeliveryPeriodRules.IsLateDeliveryTerminationOpen(
            OrderStatus.Placed, PaymentStatus.Pending, createdAt, [new(ProductType.Physical, 1, null)], late));
        Assert.False(DeliveryPeriodRules.IsLateDeliveryTerminationOpen(
            OrderStatus.Cancelled, PaymentStatus.Refunded, createdAt, [new(ProductType.Physical, 1, null)], late));
    }

    // ---- The legal interest warning ----

    [Fact]
    public void InterestWarning_OnAPhysicalItem_FollowsTheShippingPromisesEnd_KK2()
    {
        DateTimeOffset end = DeliveryPeriodRules.ShippingPeriodEnd(ApprovedAt, 2, BusinessCalendar.Product);

        Assert.False(DeliveryPeriodRules.IsLegalInterestWarningDue(ProductType.Physical, ApprovedAt, 2, null, end.AddTicks(-1)));
        Assert.True(DeliveryPeriodRules.IsLegalInterestWarningDue(ProductType.Physical, ApprovedAt, 2, null, end));
        Assert.True(DeliveryPeriodRules.IsLegalInterestWarningDue(ProductType.Physical, ApprovedAt, 0, null, TurkeyTime.StartOf(Monday.AddDays(1))));
    }

    [Fact]
    public void InterestWarning_OnAServiceItem_FollowsTheFulfilmentPeriodsEnd_KK2()
    {
        DateTimeOffset end = DeliveryPeriodRules.FulfilmentPeriodEnd(ApprovedAt, 5);

        Assert.False(DeliveryPeriodRules.IsLegalInterestWarningDue(ProductType.Service, ApprovedAt, null, 5, end.AddTicks(-1)));
        Assert.True(DeliveryPeriodRules.IsLegalInterestWarningDue(ProductType.Service, ApprovedAt, null, 5, end));
    }

    [Fact]
    public void InterestWarning_IsNeverDue_ForADigitalItem_OrWithoutAnApprovedPayment()
    {
        DateTimeOffset late = ApprovedAt.AddDays(60);

        Assert.False(DeliveryPeriodRules.IsLegalInterestWarningDue(ProductType.Digital, ApprovedAt, 2, null, late));
        Assert.False(DeliveryPeriodRules.IsLegalInterestWarningDue(ProductType.Physical, null, 2, null, late));
        Assert.False(DeliveryPeriodRules.IsLegalInterestWarningDue(ProductType.Service, null, null, 5, late));
    }
}
