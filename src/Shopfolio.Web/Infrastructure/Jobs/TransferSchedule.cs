namespace Shopfolio.Infrastructure.Jobs;

/// <summary>
/// The two clock times of a bank transfer order (05 5.3.1, 5.4.3; 02 3.21.6, 4.3; MK-43): the reminder's moment and the end of a window that an
/// outage put off. Pure functions; the callers read the clock and the order, and write the result on the order's own fields - the time lives in
/// the record, not in a calendar (05 5.1.1).
/// </summary>
internal static class TransferSchedule
{
    /// <summary>
    /// The reminder's moment: 08:00 Turkey time on the last business day of the payment window. A window ends at the end of its last day
    /// (<see cref="TurkeyTime.EndOfPeriod"/>), so that day is the one that holds the instant just before the end.
    /// </summary>
    public static DateTimeOffset ReminderAt(DateTimeOffset paymentWindowEndsAt)
    {
        DateOnly lastDay = TurkeyTime.DateOf(paymentWindowEndsAt.AddTicks(-1));
        return TurkeyTime.StartOf(lastDay).AddHours(SchedulerTimings.TransferReminderHour);
    }

    /// <summary>
    /// The new end of a window that ran out while the site was down (05 5.4.3; 02 4.3, Z-8): the end of the first business day after the day of
    /// the return - the day of the return is not counted, the same as the start day of every period (02 4.1.4).
    /// </summary>
    public static DateTimeOffset DeferredWindowEnd(DateTimeOffset returnedAt, BusinessCalendar calendar)
    {
        ArgumentNullException.ThrowIfNull(calendar);
        DateOnly firstBusinessDay = calendar.NextBusinessDay(TurkeyTime.DateOf(returnedAt));
        return TurkeyTime.StartOf(firstBusinessDay.AddDays(1));
    }
}
