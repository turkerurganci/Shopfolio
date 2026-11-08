using Shopfolio.Infrastructure;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Ordering.Contracts;

namespace Shopfolio.Modules.Ordering;

/// <summary>
/// The delivery periods' rules (03 4.1.10-4.1.13; 02 4.2 Z-10, Z-11, Z-12, Z-39, 7.2.3): one home for when the shipping promise and the service's
/// fulfilment period run out, when the legal delivery limit opens the late delivery termination and when a cancellation carries the legal interest
/// warning. None of these periods is a timer: when one runs out the system does nothing by itself - the order stays where it is and in its panel
/// counter, and the period is shown (03 4: "kendiliğinden işlem yok"). The operations that act on them (the customer's cancellation, the late
/// delivery termination, the panel's counters) read these rules; nothing here reads a clock - the caller passes the instant it read from the time source.
/// </summary>
internal static class DeliveryPeriodRules
{
    /// <summary>
    /// Z-11 - the legal delivery limit in calendar days from the order's confirmation (02 4.2 Z-11, 7.2.3). A legal
    /// period, not a product constant: it is not in the parameter inventory (02 11 "Envantere girmeyenler"; 09 3.5 item 1) and lives with its rule.
    /// </summary>
    public const int LegalDeliveryLimitDays = 30;

    /// <summary>
    /// Z-10 - the last day of the shipping promise: <paramref name="businessDays"/> business days counted from the payment's approval (02 4.1.3, 4.2
    /// Z-10). A promise of zero business days (P-5's and P-6's lower fence is zero) is shipping on the day of the approval: its last day is the approval's
    /// own day and it runs out at 00:00 of the next day - the count's start-day rule with no day added (D-161). The day count itself takes one day or more.
    /// </summary>
    public static DateOnly ShippingLastDay(DateTimeOffset paymentApprovedAt, int businessDays, BusinessCalendar calendar)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(businessDays);
        DateOnly approvalDay = TurkeyTime.DateOf(paymentApprovedAt);
        return businessDays == 0 ? approvalDay : PeriodCounting.LastDay(CountedPeriod.ShippingPeriod, approvalDay, businessDays, calendar);
    }

    /// <summary>Z-10 - the instant the shipping promise runs out: 00:00 Turkey time of the day after <see cref="ShippingLastDay"/>.</summary>
    public static DateTimeOffset ShippingPeriodEnd(DateTimeOffset paymentApprovedAt, int businessDays, BusinessCalendar calendar)
    {
        return TurkeyTime.StartOf(ShippingLastDay(paymentApprovedAt, businessDays, calendar).AddDays(1));
    }

    /// <summary>Z-10 - where the shipping promise stands at <paramref name="now"/>: its last day, days left in business days, days over (04 2.6.3).</summary>
    public static PeriodStanding ShippingStanding(DateTimeOffset paymentApprovedAt, int businessDays, DateTimeOffset now, BusinessCalendar calendar)
    {
        return PeriodStanding.Of(CountedPeriod.ShippingPeriod, ShippingLastDay(paymentApprovedAt, businessDays, calendar), now, calendar);
    }

    /// <summary>
    /// Z-39 - the instant a service item's fulfilment period runs out: the item's frozen period (<see cref="OrderItem.FulfilmentPeriod"/>, at least one
    /// day by P-44's fence) in calendar days from the payment's approval (02 4.2 Z-39, 4.1.3).
    /// </summary>
    public static DateTimeOffset FulfilmentPeriodEnd(DateTimeOffset paymentApprovedAt, int days)
    {
        return PeriodCounting.EndOf(CountedPeriod.ServiceFulfilmentPeriod, paymentApprovedAt, days, BusinessCalendar.Product);
    }

    /// <summary>Z-39 - where the fulfilment period stands at <paramref name="now"/> (04 2.6.3).</summary>
    public static PeriodStanding FulfilmentStanding(DateTimeOffset paymentApprovedAt, int days, DateTimeOffset now)
    {
        DateOnly lastDay = PeriodCounting.LastDay(CountedPeriod.ServiceFulfilmentPeriod, TurkeyTime.DateOf(paymentApprovedAt), days, BusinessCalendar.Product);
        return PeriodStanding.Of(CountedPeriod.ServiceFulfilmentPeriod, lastDay, now, BusinessCalendar.Product);
    }

    /// <summary>Z-11 - the instant the legal delivery limit runs out: <see cref="LegalDeliveryLimitDays"/> calendar days from the order's confirmation (<see cref="Order.CreatedAt"/>).</summary>
    public static DateTimeOffset LegalDeliveryLimitEnd(DateTimeOffset orderCreatedAt)
    {
        return PeriodCounting.EndOf(CountedPeriod.LegalDeliveryLimit, orderCreatedAt, LegalDeliveryLimitDays, BusinessCalendar.Product);
    }

    /// <summary>
    /// Z-11 - whether the late delivery termination is open (03 4.1.11; 02 7.2.3): the legal delivery limit has run out, the order's payment is
    /// confirmed and the order is not cancelled, and it still has a physical item with no delivery date and an open quantity - while the order is
    /// shipped too. The custom-made item is not left out: there the thirty days are the contract's promise (K-597). Z-12 is not a period: an item
    /// whose delivery date is entered no longer opens it, whatever the date (03 4.1.12).
    /// </summary>
    public static bool IsLateDeliveryTerminationOpen(
        OrderStatus orderStatus, PaymentStatus paymentStatus, DateTimeOffset orderCreatedAt, IEnumerable<ItemDeliveryState> items, DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(items);
        return paymentStatus is PaymentStatus.Paid or PaymentStatus.PartiallyRefunded
            && orderStatus != OrderStatus.Cancelled
            && now >= LegalDeliveryLimitEnd(orderCreatedAt)
            && items.Any(i => i.ProductType == ProductType.Physical && i.OpenQuantity > 0 && i.DeliveryDate is null);
    }

    /// <summary>
    /// Whether a cancellation at <paramref name="cancelledAt"/> carries the legal interest warning on the panel (03 4.1.10, 4.1.13; 02 7.2.3): the
    /// item's promise had run out before it - the order's shipping promise (Z-10) for a physical item, the item's fulfilment period (Z-39) for a service
    /// item; both counted from the payment's approval. A digital item has neither period, an order without an approved payment neither.
    /// </summary>
    public static bool IsLegalInterestWarningDue(
        ProductType productType, DateTimeOffset? paymentApprovedAt, int? shippingPeriod, int? fulfilmentPeriod, DateTimeOffset cancelledAt)
    {
        if (paymentApprovedAt is not { } approvedAt)
        {
            return false;
        }

        return productType switch
        {
            ProductType.Physical => shippingPeriod is { } businessDays && cancelledAt >= ShippingPeriodEnd(approvedAt, businessDays, BusinessCalendar.Product),
            ProductType.Service => fulfilmentPeriod is { } days && cancelledAt >= FulfilmentPeriodEnd(approvedAt, days),
            _ => false,
        };
    }
}

/// <summary>An order item as the late delivery termination reads it: its type, its open quantity (its own minus what its item records closed) and its delivery date.</summary>
internal readonly record struct ItemDeliveryState(ProductType ProductType, int OpenQuantity, DateOnly? DeliveryDate);
