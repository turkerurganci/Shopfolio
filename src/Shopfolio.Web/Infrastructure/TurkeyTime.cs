namespace Shopfolio.Infrastructure;

/// <summary>
/// The one Turkey time helper (09 7.1 item 3; 05 7.2.1, 7.2.2; 02 4.1.1, 4.1.4). Time is stored as a UTC instant; days are counted in
/// Turkey time, which is a fixed +03:00 offset - the product has one time zone and no daylight saving change. The server's time zone takes
/// no part: nothing here reads the local zone or the time zone database. Every day and period count goes through <see cref="DateOf"/> and
/// <see cref="StartOf"/>.
/// </summary>
internal static class TurkeyTime
{
    /// <summary>The fixed offset of Turkey time (02 4.1.1).</summary>
    public static readonly TimeSpan Offset = TimeSpan.FromHours(3);

    /// <summary>The Turkey day of an instant: 1 March 21:00 UTC is 2 March 00:00 in Turkey, so it is 2 March.</summary>
    public static DateOnly DateOf(DateTimeOffset instant)
    {
        return DateOnly.FromDateTime(instant.ToOffset(Offset).DateTime);
    }

    /// <summary>The UTC instant of 00:00 Turkey time on <paramref name="day"/>; the result's offset is zero.</summary>
    public static DateTimeOffset StartOf(DateOnly day)
    {
        return new DateTimeOffset(day.ToDateTime(TimeOnly.MinValue), Offset).ToUniversalTime();
    }

    /// <summary>
    /// The last day of a period set in days (02 4.1.4): the start day is not counted, the period begins on the next day, so a period of
    /// <paramref name="days"/> days started on <paramref name="startDay"/> ends on <c>startDay + days</c>.
    /// </summary>
    public static DateOnly LastDayOfPeriod(DateOnly startDay, int days)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(days, 1);
        return startDay.AddDays(days);
    }

    /// <summary>
    /// The instant a period set in days runs out (05 7.2.2): the end of its last day, that is 00:00 Turkey time of the day after. The period
    /// holds every instant before it - the last moment of the last day is inside, the next day's 00:00 is outside; no clock time such as
    /// 23:59 is compared. The instant is what a record stores as the period's end (05 5.1.1).
    /// </summary>
    public static DateTimeOffset EndOfPeriod(DateOnly startDay, int days)
    {
        return StartOf(LastDayOfPeriod(startDay, days).AddDays(1));
    }

    /// <summary>
    /// The instant a date range entered as two dates runs out (02 4.1.4; K-1559): the 00:00 of the day after the end date, so the end date is
    /// included as a whole day. A discount's end is written to the price history with this instant.
    /// </summary>
    public static DateTimeOffset EndOfRange(DateOnly endDay)
    {
        return StartOf(endDay.AddDays(1));
    }

    /// <summary>
    /// Whether <paramref name="instant"/> falls in the date range <paramref name="startDay"/> to <paramref name="endDay"/> (02 4.1.4): the
    /// range begins at 00:00 of the start day and the end day is included as a whole - <c>StartOf(start) &lt;= instant &lt; StartOf(end + 1)</c>
    /// (09 7.1 item 3). A range that ends before it starts is a caller error; a one-day range has equal dates.
    /// </summary>
    public static bool IsWithinRange(DateTimeOffset instant, DateOnly startDay, DateOnly endDay)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(endDay, startDay);
        return StartOf(startDay) <= instant && instant < EndOfRange(endDay);
    }

    /// <summary>
    /// Whether <paramref name="instant"/> falls in a period of <paramref name="days"/> days started on <paramref name="startDay"/>:
    /// <c>StartOf(startDay + 1) &lt;= instant &lt; StartOf(startDay + days + 1)</c> (09 7.1 item 3).
    /// </summary>
    public static bool IsWithinPeriod(DateTimeOffset instant, DateOnly startDay, int days)
    {
        return StartOf(startDay.AddDays(1)) <= instant && instant < EndOfPeriod(startDay, days);
    }
}
