using Shopfolio.Infrastructure;

namespace Shopfolio.Modules.Ordering;

/// <summary>
/// The two legal periods the order's disposal part applies (06 9.1 Z-28, Z-41; 02 4.2 Z-28, Z-41). They are legal periods, not product constants, so
/// they are not rows of the parameter inventory and live with the part that applies them (09 3.5 item 1; 06 9.1.2): the period of a paid order (Z-28)
/// is ten years from the end of the calendar year of its creation, the personal data of an order that was never paid (Z-41) goes three years after
/// the moment the order failed. Calendar years and the three-year step are counted in Turkey time (<see cref="TurkeyTime"/>).
/// </summary>
internal static class OrderRetention
{
    /// <summary>Z-28: a paid order's personal data is kept this many years after the end of the calendar year the order was created in (6563 sayılı Kanun m.11/3).</summary>
    public const int PaidOrderYears = 10;

    /// <summary>Z-41: the personal data of an order whose payment was never taken is kept this many years after the moment the order failed.</summary>
    public const int FailedOrderPersonalDataYears = 3;

    /// <summary>
    /// Z-28's edge at <paramref name="now"/>: a paid order created before the returned instant is past its period. The order's period ends at
    /// 00:00 Turkey time on 1 January of (creation year + 1 + <see cref="PaidOrderYears"/>); an order is past it exactly when its creation year is
    /// before the year of <paramref name="now"/> less <see cref="PaidOrderYears"/>, so the edge is 1 January of that year.
    /// </summary>
    public static DateTimeOffset PaidCreatedBefore(DateTimeOffset now)
    {
        int year = TurkeyTime.DateOf(now).Year - PaidOrderYears;
        return TurkeyTime.StartOf(new DateOnly(year, 1, 1)).ToUniversalTime();
    }

    /// <summary>Z-41's edge at <paramref name="now"/>: an unpaid order that failed before the returned instant is past its personal data period.</summary>
    public static DateTimeOffset FailedBefore(DateTimeOffset now)
    {
        return now.ToOffset(TurkeyTime.Offset).AddYears(-FailedOrderPersonalDataYears).ToUniversalTime();
    }
}
