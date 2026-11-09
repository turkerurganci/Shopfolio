using Shopfolio.Infrastructure;
using Shopfolio.Modules.Administration.Contracts;
using Shopfolio.Modules.AfterSales.Contracts;
using Shopfolio.Modules.CorporateContent.Contracts;
using Shopfolio.Modules.Ordering.Contracts;

namespace Shopfolio.Modules.Administration;

/// <summary>
/// The sales summary (02 10.6.2, 10.6.3; 11 T383; KP-53): the Administration module owns it (05 3.3.9) and reads the records only through the modules' surfaces - the orders from
/// Ordering, the item records from AfterSales, the contact requests from CorporateContent. It has no table, no database context and no request: no visitor data or cookie reaches
/// it, nothing is stored and there is no export. Every call computes the period again from the records as they are (K-483).
/// </summary>
internal sealed class SalesSummaryService : ISalesSummary
{
    private readonly IOrderingModule _ordering;
    private readonly IAfterSalesModule _afterSales;
    private readonly ICorporateContentModule _content;
    private readonly TimeProvider _time;

    public SalesSummaryService(IOrderingModule ordering, IAfterSalesModule afterSales, ICorporateContentModule content, TimeProvider time)
    {
        ArgumentNullException.ThrowIfNull(ordering);
        ArgumentNullException.ThrowIfNull(afterSales);
        ArgumentNullException.ThrowIfNull(content);
        ArgumentNullException.ThrowIfNull(time);
        _ordering = ordering;
        _afterSales = afterSales;
        _content = content;
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

        return new SalesSummary(
            period,
            facts.OrderCount,
            facts.Revenue,
            cancellationAndReturnCount,
            SalesSummaryRules.TopSellers(facts.Products),
            facts.PaymentMethods,
            SalesSummaryRules.PaymentCompletion(facts.Attempts, from, to, now),
            contactRequests,
            SalesSummaryRules.ShippingPromiseCompliance(facts.ShippingPromises),
            SalesSummaryRules.CancellationAndReturn(facts.PaidItems, cohortLoss));
    }

    public async Task<int> CountDailyOrdersAsync(DateOnly day, CancellationToken ct)
    {
        SalesSummary summary = await GetAsync(SalesSummaryPeriod.OfDay(day), ct);
        return summary.OrderCount;
    }
}
