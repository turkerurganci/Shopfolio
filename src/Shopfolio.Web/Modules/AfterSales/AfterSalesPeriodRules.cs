using Shopfolio.Infrastructure;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Modules.Payment.Contracts;

namespace Shopfolio.Modules.AfterSales;

/// <summary>
/// The withdrawal, refund and defect claim periods' rules (03 4.1.14-4.1.22; 02 4.2 Z-13-Z-19, Z-42, Z-47): one home for when the withdrawal button
/// closes, where a refund's period stands, when the "goods did not come back" closing opens and when the defect claim channel closes. None of these
/// periods is a timer: no job, scheduled source or nightly step acts when one runs out (03 4: "kendiliğinden işlem yok"); the operations that own the
/// records (the withdrawal, the refund, the receipt, the defect claim) and the panel read these rules. The rules take the records' values - an
/// item's type, its delivery date and mark, a declaration's date, an arrival date - and the instant the caller read from the time source.
/// Z-47 (the notice that fulfilment became impossible) has no rule here: the product cannot know when the firm learned of it, so it neither keeps nor
/// shows a period for it; the firm's cancellation sends the notice with its reason at the cancellation (03 4.1.20). Z-19 (the download right) is not a
/// period but a count (P-8); there is no timer for it (03 4.1.22).
/// The four lengths below are legal periods, not product constants: they are not in the parameter inventory (02 11 "Envantere girmeyenler"; 09 3.5
/// item 1) and live with their rules.
/// </summary>
internal static class AfterSalesPeriodRules
{
    /// <summary>Z-13, Z-14 - the withdrawal window in calendar days (02 4.2; K-203, K-205).</summary>
    public const int WithdrawalWindowDays = 14;

    /// <summary>Z-16, Z-17 - the refund period in calendar days (02 4.2; K-491, K-291).</summary>
    public const int RefundPeriodDays = 14;

    /// <summary>Z-42 - the customer's period to send the returned goods back, in calendar days from the declaration (02 4.2; K-494).</summary>
    public const int ReturnShipmentPeriodDays = 14;

    /// <summary>Z-18 - the defect claim's limitation in years from the item's delivery mark (02 4.2; K-226).</summary>
    public const int DefectClaimYears = 2;

    // ---- Z-13, Z-14, Z-15: the withdrawal window ----

    /// <summary>The instant a withdrawal window started on <paramref name="startDay"/> runs out: <see cref="WithdrawalWindowDays"/> calendar days later, at 00:00 of the next day.</summary>
    public static DateTimeOffset WithdrawalWindowEnd(DateOnly startDay)
    {
        return PeriodCounting.EndOf(CountedPeriod.WithdrawalWindow, TurkeyTime.StartOf(startDay), WithdrawalWindowDays, BusinessCalendar.Product);
    }

    /// <summary>
    /// Whether the withdrawal button is open at <paramref name="now"/> as far as the period goes (03 4.1.14-4.1.16; 02 4.2 Z-13, Z-14, Z-15). A closed
    /// window leaves the declarations made before it standing - they are records, and nothing here touches them. The item's other conditions (the
    /// exemption mark, the open quantity, the payment) are the withdrawal's own.
    /// <list type="bullet">
    /// <item>Physical (Z-13): the window runs from the item's delivery date - a corrected date counts as entered. No date, no start: the button does
    /// not close (03 4.1.12; the risk of a late mark is the firm's).</item>
    /// <item>Service (Z-14): the window runs from <see cref="ServiceWithdrawalStartDay"/>; the "tamamlandı" mark closes it too, whichever comes first.
    /// With no start day the right lasts until the mark (03 3.3.29).</item>
    /// <item>Digital (Z-15): the right fell at the payment's approval with the third checkbox - the button is never open.</item>
    /// </list>
    /// </summary>
    public static bool IsWithdrawalWindowOpen(WithdrawalWindowFacts facts, DateTimeOffset now)
    {
        return facts.ProductType switch
        {
            ProductType.Physical => facts.ItemDeliveryDate is not { } delivered || now < WithdrawalWindowEnd(delivered),
            ProductType.Service => facts.ItemDeliveryMarkedAt is null
                && (ServiceWithdrawalStartDay(facts.OrderCreatedAt, facts.OrderHasPhysicalItem, facts.OrderDeliveryDate) is not { } start
                    || now < WithdrawalWindowEnd(start)),
            _ => false,
        };
    }

    /// <summary>
    /// Z-14 - the day a service item's window starts (03 4.1.15; 02 4.2 Z-14): the order's date; on an order that carried a physical item when it was
    /// confirmed, the order's delivery date instead - null when it is not entered, or when the physical items were all cancelled, removed or terminated
    /// before the delivery (no date is ever entered then, and the window is not pulled back to the order's date; 03 3.3.29).
    /// </summary>
    public static DateOnly? ServiceWithdrawalStartDay(DateTimeOffset orderCreatedAt, bool orderHasPhysicalItem, DateOnly? orderDeliveryDate)
    {
        return orderHasPhysicalItem ? orderDeliveryDate : TurkeyTime.DateOf(orderCreatedAt);
    }

    // ---- Z-16: the withdrawal's refund; Z-42: the goods' sending period ----

    /// <summary>
    /// Whether a withdrawal came after the item's delivery (02 7.3.5, 4.2 Z-16): a service item's never does - its refund runs from the declaration; a
    /// physical item's does when its delivery date is before the declaration's date, and on the same day when the firm answered that the delivery came
    /// first (<see cref="Withdrawal.IsDeliveredBeforeDeclaration"/>). With no delivery date it came before the delivery.
    /// </summary>
    public static bool IsAfterDelivery(ProductType productType, DateOnly? itemDeliveryDate, DateOnly declarationDate, bool? isDeliveredBeforeDeclaration)
    {
        if (productType != ProductType.Physical || itemDeliveryDate is not { } delivered)
        {
            return false;
        }

        return delivered < declarationDate || (delivered == declarationDate && isDeliveredBeforeDeclaration == true);
    }

    /// <summary>
    /// Z-16 - the day the withdrawal's refund period starts (03 4.1.17; 02 4.2 Z-16): before the delivery and on a service item, the declaration's
    /// date; after the delivery, the goods' arrival date the firm entered at the receipt - the declaration's date when the goods arrived before it,
    /// the later of the two. After the delivery and with no arrival the period does not start (null): the panel shows "iade malı bekleniyor" with
    /// the days since the declaration and the sending period (<see cref="ReturnShipmentStanding"/>). This is the start rule, not a right to hold.
    /// </summary>
    public static DateOnly? WithdrawalRefundStartDay(bool isAfterDelivery, DateOnly declarationDate, DateOnly? arrivalDate)
    {
        if (!isAfterDelivery)
        {
            return declarationDate;
        }

        return arrivalDate is { } arrived ? (arrived > declarationDate ? arrived : declarationDate) : null;
    }

    /// <summary>
    /// Z-16, Z-17 - where a refund period started on <paramref name="startDay"/> stands at <paramref name="now"/> (04 2.6.3): the remaining days the
    /// panel shows. It runs while an IBAN is awaited too (02 4.2 Z-16); running out does nothing by itself - the refund is the firm's duty.
    /// </summary>
    public static PeriodStanding RefundStanding(DateOnly startDay, DateTimeOffset now)
    {
        DateOnly lastDay = PeriodCounting.LastDay(CountedPeriod.RefundPeriod, startDay, RefundPeriodDays, BusinessCalendar.Product);
        return PeriodStanding.Of(CountedPeriod.RefundPeriod, lastDay, now, BusinessCalendar.Product);
    }

    /// <summary>
    /// Z-17 - where a paid cancellation's refund stands at <paramref name="now"/> (03 4.1.19; 02 4.2 Z-17): the period runs from the cancellation's
    /// moment; the panel shows what is left only on the bank transfer line. On the card line the refund started at the cancellation (the card refund
    /// job), so no period is shown: null.
    /// </summary>
    public static PeriodStanding? CancellationRefundStanding(PaymentMethod paymentMethod, DateTimeOffset cancelledAt, DateTimeOffset now)
    {
        return paymentMethod == PaymentMethod.BankTransfer ? RefundStanding(TurkeyTime.DateOf(cancelledAt), now) : null;
    }

    /// <summary>Z-42 - where the customer's sending period stands at <paramref name="now"/>: <see cref="ReturnShipmentPeriodDays"/> calendar days from the declaration's date.</summary>
    public static PeriodStanding ReturnShipmentStanding(DateOnly declarationDate, DateTimeOffset now)
    {
        DateOnly lastDay = PeriodCounting.LastDay(CountedPeriod.ReturnShipmentPeriod, declarationDate, ReturnShipmentPeriodDays, BusinessCalendar.Product);
        return PeriodStanding.Of(CountedPeriod.ReturnShipmentPeriod, lastDay, now, BusinessCalendar.Product);
    }

    /// <summary>The calendar days from the declaration's date to <paramref name="now"/>'s day: what the "iade malı bekleniyor" indicator counts (04 2.6.3; 02 4.2 Z-16).</summary>
    public static int DaysSinceDeclaration(DateOnly declarationDate, DateTimeOffset now)
    {
        return TurkeyTime.DateOf(now).DayNumber - declarationDate.DayNumber;
    }

    /// <summary>
    /// Z-42 - whether the administrator's "goods did not come back" closing is open (03 4.1.18; 02 10.4.9): a withdrawal after the delivery whose
    /// sending period has run out and some of whose units are still awaited (<see cref="ItemUnitFacts.AwaitingReceipt"/>). Running out closes nothing
    /// by itself: the item stays "iade malı bekleniyor" until the firm closes it or the goods arrive.
    /// </summary>
    public static bool IsNotReturnedClosureOpen(bool isAfterDelivery, int awaitingUnits, DateOnly declarationDate, DateTimeOffset now)
    {
        return isAfterDelivery && awaitingUnits > 0 && ReturnShipmentStanding(declarationDate, now).IsOver;
    }

    // ---- Z-18: the defect claim channel ----

    /// <summary>
    /// Z-18 - the instant the defect claim channel of an item delivered on <paramref name="deliveryDay"/> closes: its last day is the same day
    /// <see cref="DefectClaimYears"/> years later, and the channel closes at 00:00 of the next day (02 4.1.4: the start day is not counted).
    /// </summary>
    public static DateTimeOffset DefectClaimChannelEnd(DateOnly deliveryDay)
    {
        return TurkeyTime.StartOf(deliveryDay.AddYears(DefectClaimYears).AddDays(1));
    }

    /// <summary>
    /// Z-18 - whether the order page's defect claim channel of an item is open at <paramref name="now"/> (03 1.11.9, 2.9.1, 4.1.21; 02 4.2 Z-18,
    /// 7.5.2): open from the moment the order reached "Kargoya verildi" for a physical item - "Kargoya verildi", "Teslim edilemedi" and "Teslim
    /// edildi", the statuses after it, as 03 1.11.5 reads the same words (T320 B1, D-532) - from the "tamamlandı" mark for a service item and from
    /// the payment's approval for a digital item - the last two are the item's delivery mark, written with its date. The two years run from the item's
    /// delivery date (the delivery date, the completion's day, the approval's day); a physical item with no delivery date has no running period and
    /// its channel does not close (VAL-C024; the risk is the firm's). When the channel is closed no new claim is opened and a resolved claim is not
    /// reopened - by the customer or by an administrator (03 1.9.1). The order page, the panel and the claim's writers read this one rule.
    /// </summary>
    public static bool IsDefectClaimChannelOpen(ProductType productType, OrderStatus orderStatus, DateOnly? itemDeliveryDate, DateTimeOffset now)
    {
        bool opened = itemDeliveryDate is not null
            || (productType == ProductType.Physical && orderStatus is OrderStatus.Shipped or OrderStatus.DeliveryFailed or OrderStatus.Delivered);
        return opened && (itemDeliveryDate is not { } delivered || now < DefectClaimChannelEnd(delivered));
    }
}

/// <summary>
/// An item as the withdrawal window reads it (03 4.1.14-4.1.16): its type, delivery date and delivery mark ("tamamlandı" on a service item), and its
/// order's confirmation moment, whether the order carried a physical item when confirmed, and the order's delivery date (the physical items' date).
/// </summary>
internal readonly record struct WithdrawalWindowFacts(
    ProductType ProductType,
    DateOnly? ItemDeliveryDate,
    DateTimeOffset? ItemDeliveryMarkedAt,
    DateTimeOffset OrderCreatedAt,
    bool OrderHasPhysicalItem,
    DateOnly? OrderDeliveryDate);
