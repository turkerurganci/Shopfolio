using Shopfolio.Modules.Ordering.Contracts;

namespace Shopfolio.Modules.Administration.Contracts;

/// <summary>
/// The sales summary (02 10.6.2, 10.6.3; 11 T383): a report computed from the product's own records - orders, payments and contact requests - at the moment it is asked for. It is
/// not analytics: no visitor data, no cookie, no new personal data; it is neither stored nor exported (K-483). The screen and its period form are T524's (07 UC-105).
/// </summary>
public interface ISalesSummary
{
    /// <summary>The summary of <paramref name="period"/>, computed now from the records; nothing is written.</summary>
    Task<SalesSummary> GetAsync(SalesSummaryPeriod period, CancellationToken ct);

    /// <summary>
    /// The daily number the peak day trigger reads (02 H-2; K-474): the orders born on <paramref name="day"/> (a Turkey day) whose payment was approved - a Failed order is not
    /// counted. It is the order count of the one-day summary of that day.
    /// </summary>
    Task<int> CountDailyOrdersAsync(DateOnly day, CancellationToken ct);
}

/// <summary>
/// The summary's period: two Turkey days, both included (02 4.1.4). The start is at 00:00 Turkey time of <see cref="StartDay"/>, the end at 00:00 of the day after
/// <see cref="EndDay"/>. A period of one day is the peak day trigger's daily number (02 H-2).
/// </summary>
public sealed record SalesSummaryPeriod
{
    private SalesSummaryPeriod(DateOnly startDay, DateOnly endDay)
    {
        StartDay = startDay;
        EndDay = endDay;
    }

    public DateOnly StartDay { get; }

    public DateOnly EndDay { get; }

    public bool IsSingleDay => StartDay == EndDay;

    /// <summary>One Turkey day.</summary>
    public static SalesSummaryPeriod OfDay(DateOnly day) => new(day, day);

    /// <summary>
    /// A ready period on <paramref name="today"/> (04 9.22.3; 07 9.22.1; K-1215): Last 7 / Last 30 days include today (today−6 / today−29 … today), This month runs from the 1st to
    /// today, Last month is the previous month's first and last day, and the Measurement window runs from the day sales first opened to the day before the same day three calendar
    /// months later, cut to today. Null for the Measurement window when sales never opened (<paramref name="salesOpenDay"/> is null): the choice is then not offered (K-486).
    /// </summary>
    public static SalesSummaryPeriod? Of(SalesSummaryPreset preset, DateOnly today, DateOnly? salesOpenDay)
    {
        return preset switch
        {
            SalesSummaryPreset.Today => new(today, today),
            SalesSummaryPreset.Yesterday => new(today.AddDays(-1), today.AddDays(-1)),
            SalesSummaryPreset.Last7Days => new(today.AddDays(-6), today),
            SalesSummaryPreset.Last30Days => new(today.AddDays(-29), today),
            SalesSummaryPreset.ThisMonth => new(new DateOnly(today.Year, today.Month, 1), today),
            SalesSummaryPreset.LastMonth => LastMonth(today),
            SalesSummaryPreset.MeasurementWindow => salesOpenDay is { } open && open <= today ? new(open, Earlier(open.AddMonths(3).AddDays(-1), today)) : null,
            _ => throw new ArgumentOutOfRangeException(nameof(preset), preset, "Not a ready period of the sales summary."),
        };
    }

    /// <summary>
    /// The custom range (04 9.22.9; OB-16): both dates are required, neither may be after <paramref name="today"/> and the end may not be before the start. A refused range gives no
    /// period and the summary is not computed.
    /// </summary>
    public static (SalesSummaryPeriod? Period, SalesSummaryPeriodRefusal? Refusal) Custom(DateOnly? startDay, DateOnly? endDay, DateOnly today)
    {
        if (startDay is not { } start || endDay is not { } end)
        {
            return (null, SalesSummaryPeriodRefusal.DateMissing);
        }

        if (start > today || end > today)
        {
            return (null, SalesSummaryPeriodRefusal.FutureDate);
        }

        return end < start ? (null, SalesSummaryPeriodRefusal.EndBeforeStart) : (new SalesSummaryPeriod(start, end), null);
    }

    private static SalesSummaryPeriod LastMonth(DateOnly today)
    {
        DateOnly firstOfThisMonth = new(today.Year, today.Month, 1);
        DateOnly firstOfLastMonth = firstOfThisMonth.AddMonths(-1);
        return new(firstOfLastMonth, firstOfThisMonth.AddDays(-1));
    }

    private static DateOnly Earlier(DateOnly a, DateOnly b) => a < b ? a : b;
}

/// <summary>
/// A ratio of the summary as its numerator and denominator (04 9.22.5): the screen shows the percentage with both numbers under it. A ratio whose denominator is zero is "not
/// assessable" and shows no percentage - it is never zero percent (02 10.6.3; K-490).
/// </summary>
public sealed record SummaryRatio(int Numerator, int Denominator)
{
    public bool IsAssessable => Denominator > 0;
}

/// <summary>One of the best-selling products (04 9.22.6): the units sold and their amount; product level, its variants together.</summary>
public sealed record TopSeller(Guid? ProductId, string ProductName, int Quantity, decimal Amount);

/// <summary>
/// The summary of one period (02 10.6.2, 10.6.3; 04 9.22.4-9.22.6). The order count and the revenue are the approved orders born in the period (a Failed or pending order is out;
/// a later refund is not subtracted). The cancellation and return count is the number of approved orders with a cancellation, removal, late delivery termination or withdrawal
/// whose event day is in the period (K-580). The four store measures are the payment completion ratio, the contact request count, the shipping promise compliance ratio and the
/// cancellation and return ratio (M-1…M-4; K-416).
/// </summary>
public sealed record SalesSummary(
    SalesSummaryPeriod Period,
    int OrderCount,
    decimal Revenue,
    int CancellationAndReturnCount,
    IReadOnlyList<TopSeller> TopSellers,
    IReadOnlyList<PaymentMethodCount> PaymentMethods,
    SummaryRatio PaymentCompletion,
    int ContactRequestCount,
    SummaryRatio ShippingPromiseCompliance,
    SummaryRatio CancellationAndReturn);
