using Shopfolio.Infrastructure;
using Shopfolio.Modules.Administration.Contracts;
using Shopfolio.Modules.AfterSales.Contracts;
using Shopfolio.Modules.CorporateContent.Contracts;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Modules.Settings.Contracts;

namespace Shopfolio.Modules.Administration;

/// <summary>
/// The sales summary (02 10.6.2, 10.6.3; 11 T383; KP-53): the Administration module owns it (05 3.3.9) and reads the records only through the modules' surfaces - the orders from
/// Ordering, the item records from AfterSales, the contact requests from CorporateContent, the first day sales opened from Settings. It has no table, no database context and no request: no visitor data or cookie reaches
/// it, nothing is stored and there is no export. Every call computes the period again from the records as they are (K-483).
/// </summary>
internal sealed class SalesSummaryService : ISalesSummary
{
    private readonly IOrderingModule _ordering;
    private readonly IAfterSalesModule _afterSales;
    private readonly ICorporateContentModule _content;
    private readonly ISettingsModule _settings;
    private readonly TimeProvider _time;

    public SalesSummaryService(IOrderingModule ordering, IAfterSalesModule afterSales, ICorporateContentModule content, ISettingsModule settings, TimeProvider time)
    {
        ArgumentNullException.ThrowIfNull(ordering);
        ArgumentNullException.ThrowIfNull(afterSales);
        ArgumentNullException.ThrowIfNull(content);
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(time);
        _ordering = ordering;
        _afterSales = afterSales;
        _content = content;
        _settings = settings;
        _time = time;
    }

    public async Task<SalesSummary> GetAsync(SalesSummaryPeriod period, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(period);
        DateTimeOffset now = _time.GetUtcNow();
        DateTimeOffset from = TurkeyTime.StartOf(period.StartDay);
        DateTimeOffset to = TurkeyTime.EndOfRange(period.EndDay);

        SalesPeriodFacts facts = await _ordering.ReadSalesPeriodAsync(from, to, now, ct);

        // The count is by event day (K-580): the items with a record in the period, of approved orders only (a Failed order is the payment completion ratio's concern).
        IReadOnlyList<Guid> lossItems = await _afterSales.ListItemsWithLossInRangeAsync(period.StartDay, period.EndDay, ct);
        IReadOnlyList<OrderOfItem> lossOrders = await _ordering.FindPaidOrdersOfItemsAsync(lossItems, ct);
        int cancellationAndReturnCount = lossOrders.Select(o => o.OrderId).Distinct().Count();

        // The ratio is the period's cohort: the approved orders born in it and whether any of their items has a record, whenever it was written.
        IReadOnlySet<Guid> cohortLoss = await _afterSales.FindItemsWithLossAsync([.. facts.PaidItems.Select(i => i.OrderItemId)], ct);
        int contactRequests = await _content.CountSummaryContactRequestsAsync(from, to, ct);

        // The promise ratio's denominator (K-535): the open quantity of every physical item at its order's promise end, from the item records' one open quantity rule.
        IReadOnlyDictionary<Guid, int> openAtPromiseEnd = await _afterSales.OpenQuantitiesAtAsync(
            [.. facts.ShippingPromises.SelectMany(o => o.PhysicalItems.Select(i => new ItemQuantityAt(i.OrderItemId, i.Quantity, o.PromiseEndsAt)))],
            ct);

        return new SalesSummary(
            period,
            facts.OrderCount,
            facts.Revenue,
            cancellationAndReturnCount,
            SalesSummaryRules.TopSellers(facts.Products),
            facts.PaymentMethods,
            SalesSummaryRules.PaymentCompletion(facts.Attempts, from, to, now),
            contactRequests,
            SalesSummaryRules.ShippingPromiseCompliance(facts.ShippingPromises, openAtPromiseEnd),
            SalesSummaryRules.CancellationAndReturn(facts.PaidItems, cohortLoss));
    }

    public async Task<DateOnly?> FindSalesOpenDayAsync(CancellationToken ct)
    {
        // The Settings module owns the gate and derives its first open moment (06 10.6 m6); the day is the Turkey day of that moment (02 10.6.3 m6).
        DateTimeOffset? openedAt = await _settings.FindSalesFirstOpenedAtAsync(ct);
        return openedAt is { } at ? TurkeyTime.DateOf(at) : null;
    }

    public async Task<int> CountDailyOrdersAsync(DateOnly day, CancellationToken ct)
    {
        SalesSummary summary = await GetAsync(SalesSummaryPeriod.OfDay(day), ct);
        return summary.OrderCount;
    }
}
