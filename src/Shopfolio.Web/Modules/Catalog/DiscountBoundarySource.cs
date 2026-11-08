using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Jobs;

namespace Shopfolio.Modules.Catalog;

/// <summary>
/// The scheduler's source of the discounts' bounds (T337; 02 3.9.2, 5.11.1; 06 2.5.2, 2.5.3, 4.11, 4.12; K-1023 (4), K-1041 (2); 09 13.1 item 6): the system starts and
/// ends a discount by itself, and the start's discounted price and the end's own price are written to each variant's price history at the range's own bound, even
/// when the scheduler comes late after an outage. The moment is the discount's own dates - there is no separate moment field (06 2.5.3) -, and whether a bound is
/// written is read from the variant's last row (K-1041 (2)). The record is the product; it is due at its earliest owed bound. Processing runs
/// <see cref="PriceHistoryRecorder"/> in a transaction of its own, which reads everything again: a bound another writer wrote in between is not owed any more.
/// </summary>
internal sealed class DiscountBoundarySource : IDueRecordSource
{
    public string Name => "DiscountBoundary";

    public bool WaitsForFreshHeartbeat => false;

    public async Task<IReadOnlyList<DueRecord>> FindDueAsync(IServiceProvider services, DateTimeOffset now, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(services);
        AppDbContext db = services.GetRequiredService<AppDbContext>();
        DateOnly today = TurkeyTime.DateOf(now);
        var discounts = await db.Discounts.AsNoTracking()
            .Where(d => d.StartDate <= today)
            .Select(d => new { d.ProductId, d.Percentage, d.StartDate, d.EndDate, d.CreatedAt })
            .ToListAsync(ct);
        if (discounts.Count == 0)
        {
            return [];
        }

        List<Guid> productIds = [.. discounts.Select(d => d.ProductId)];
        var variants = await db.Variants.AsNoTracking()
            .Where(v => productIds.Contains(v.ProductId))
            .Select(v => new
            {
                v.ProductId,
                v.Price,
                v.CreatedAt,
                LastAt = db.PriceHistories.Where(h => h.VariantId == v.Id).Max(h => (DateTimeOffset?)h.StartedAt),
            })
            .ToListAsync(ct);

        List<DueRecord> due = [];
        foreach (var discount in discounts)
        {
            DiscountTerms terms = new(discount.Percentage, discount.StartDate, discount.EndDate);
            DateTimeOffset? earliest = null;
            foreach (var variant in variants.Where(v => v.ProductId == discount.ProductId))
            {
                DateTimeOffset since = discount.CreatedAt > variant.CreatedAt ? discount.CreatedAt : variant.CreatedAt;
                IReadOnlyList<PricePoint> owed = DiscountRules.OwedBounds(terms, since, variant.Price, variant.LastAt, now);
                if (owed.Count > 0 && (earliest is null || owed[0].StartedAt < earliest))
                {
                    earliest = owed[0].StartedAt;
                }
            }

            if (earliest is { } at)
            {
                due.Add(new DueRecord(discount.ProductId, at));
            }
        }

        return [.. due.OrderBy(d => d.DueAt).ThenBy(d => d.Id)];
    }

    public async Task ProcessAsync(IServiceProvider services, Guid recordId, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(services);
        ITransactionRunner runner = services.GetRequiredService<ITransactionRunner>();
        PriceHistoryRecorder recorder = services.GetRequiredService<PriceHistoryRecorder>();
        await runner.RunAsync(
            async cancel =>
            {
                await recorder.RecordAsync(recordId, cancel);
                return new SchedulerStepResult();
            },
            ct);
    }
}
