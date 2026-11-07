namespace Shopfolio.Infrastructure;

/// <summary>
/// How a period set in days is counted and when it runs out (02 4.1.3, 4.1.4; 09 7.1 item 3). Two periods count business days - the shipping
/// period and the bank transfer payment window; every other period, the legal ones included, counts calendar days, because a firm's holiday
/// calendar cannot shorten a consumer's right (K-337, K-609). Both countings share one rule: the start day is not counted, the period begins on
/// the next day and the last day is included as a whole, so the period ends at 00:00 Turkey time of the day after its last day. The day and
/// the instant come from <see cref="TurkeyTime"/>; this class holds no second Turkey time helper and reads no clock - the caller passes the
/// instant it read from the time source. A period in minutes or hours, and one that runs from the last operation (a session), is not counted
/// in days: it runs from its start instant (<see cref="SpanEnd"/>).
/// </summary>
internal static class PeriodCounting
{
    /// <summary>The counting of a period (02 4.1.3): business days for the shipping period and the bank transfer payment window, calendar days for the rest.</summary>
    public static DayCounting CountingOf(CountedPeriod period)
    {
        return period switch
        {
            CountedPeriod.ShippingPeriod or CountedPeriod.BankTransferPaymentWindow => DayCounting.BusinessDay,
            CountedPeriod.ServiceFulfilmentPeriod
                or CountedPeriod.WithdrawalWindow
                or CountedPeriod.ReturnShipmentPeriod
                or CountedPeriod.RefundPeriod
                or CountedPeriod.DefectClaimPeriod
                or CountedPeriod.DataRequestAnswerPeriod
                or CountedPeriod.ImpossibilityNoticePeriod
                or CountedPeriod.LegalDeliveryLimit => DayCounting.CalendarDay,
            _ => throw new ArgumentOutOfRangeException(nameof(period)),
        };
    }

    /// <summary>The last day of a period of <paramref name="days"/> days started on <paramref name="startDay"/>, counted as <paramref name="period"/> is.</summary>
    public static DateOnly LastDay(CountedPeriod period, DateOnly startDay, int days, BusinessCalendar calendar)
    {
        ArgumentNullException.ThrowIfNull(calendar);
        return CountingOf(period) == DayCounting.BusinessDay
            ? calendar.LastDayOfBusinessPeriod(startDay, days)
            : TurkeyTime.LastDayOfPeriod(startDay, days);
    }

    /// <summary>
    /// The instant a period of <paramref name="days"/> days started at <paramref name="start"/> runs out: 00:00 Turkey time of the day after its
    /// last day. The day of <paramref name="start"/> is the Turkey day of the instant, whatever the hour.
    /// </summary>
    public static DateTimeOffset EndOf(CountedPeriod period, DateTimeOffset start, int days, BusinessCalendar calendar)
    {
        return TurkeyTime.StartOf(LastDay(period, TurkeyTime.DateOf(start), days, calendar).AddDays(1));
    }

    /// <summary>
    /// Whether <paramref name="instant"/> falls in the period: from 00:00 of the day after the start day up to, not including, the end
    /// (09 7.1 item 3). The last moment of the last day is inside; 00:00 of the day after is outside. No clock time is compared.
    /// </summary>
    public static bool IsWithin(CountedPeriod period, DateTimeOffset start, int days, DateTimeOffset instant, BusinessCalendar calendar)
    {
        return TurkeyTime.StartOf(TurkeyTime.DateOf(start).AddDays(1)) <= instant && instant < EndOf(period, start, days, calendar);
    }

    /// <summary>
    /// The end of a period set in minutes or hours, or in any span that is not a number of days (02 4.1.4; K-459): it runs from the start instant.
    /// It never goes through the day count - a one-hour link started at 23:30 ends at 00:30 of the next day, not at the end of the day.
    /// </summary>
    public static DateTimeOffset SpanEnd(DateTimeOffset start, TimeSpan span)
    {
        return start + span;
    }
}
