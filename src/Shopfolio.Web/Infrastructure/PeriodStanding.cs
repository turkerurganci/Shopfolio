namespace Shopfolio.Infrastructure;

/// <summary>
/// Where a running period set in days stands at an instant, as the panel shows it (04 2.6.3, 2.6.6; K-767): the last day, the instant the period
/// runs out (00:00 Turkey time of the day after the last day; 02 4.1.4), the days left and, once it has run out, the days over. The count is in
/// days only - no hour, minute or second is shown. A period counted in business days counts its days left in business days (the unit the page
/// writes as "iş günü"); the days over are calendar days since the last day. The standing is a value computed from the record's own dates at the
/// moment of the read: nothing stores it and nothing acts when it turns over (03 4: "kendiliğinden işlem yok").
/// </summary>
/// <param name="LastDay">The period's last day; it is inside the period as a whole.</param>
/// <param name="EndsAt">The instant the period runs out: the last moment of the last day is inside, this instant is outside.</param>
/// <param name="DaysLeft">The days after today up to and including the last day - zero on the last day; zero once over.</param>
/// <param name="DaysOver">The calendar days since the last day once the period has run out - one on the day after the last day; zero while it runs.</param>
internal readonly record struct PeriodStanding(DateOnly LastDay, DateTimeOffset EndsAt, int DaysLeft, int DaysOver)
{
    /// <summary>Whether the period has run out: <c>now &gt;= EndsAt</c>.</summary>
    public bool IsOver => DaysOver > 0;

    /// <summary>
    /// The standing at <paramref name="now"/> of a period of the kind <paramref name="period"/> whose last day is <paramref name="lastDay"/>. The day of
    /// <paramref name="now"/> is its Turkey day (<see cref="TurkeyTime.DateOf"/>); the business days come from <paramref name="calendar"/>.
    /// </summary>
    public static PeriodStanding Of(CountedPeriod period, DateOnly lastDay, DateTimeOffset now, BusinessCalendar calendar)
    {
        ArgumentNullException.ThrowIfNull(calendar);
        DateOnly today = TurkeyTime.DateOf(now);
        DateTimeOffset endsAt = TurkeyTime.StartOf(lastDay.AddDays(1));
        if (today > lastDay)
        {
            return new PeriodStanding(lastDay, endsAt, 0, today.DayNumber - lastDay.DayNumber);
        }

        int left = 0;
        bool business = PeriodCounting.CountingOf(period) == DayCounting.BusinessDay;
        for (DateOnly day = today.AddDays(1); day <= lastDay; day = day.AddDays(1))
        {
            if (!business || calendar.IsBusinessDay(day))
            {
                left++;
            }
        }

        return new PeriodStanding(lastDay, endsAt, left, 0);
    }
}
