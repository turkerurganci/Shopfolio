using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;

namespace Shopfolio.Modules.Catalog;

/// <summary>
/// The one writer of the price history (06 4.11 "Satırın yazıldığı anlar"; 02 3.9.3, 5.11.1; K-1023 (1), (4), K-1041). It brings a product's variants' history up to
/// the unit of work: first the bounds the stored discount owes each variant - its start with the discounted price and its end with the variant's stored price, each
/// at the range's own bound even when written late (K-1023 (4)), and only when the discount was defined, and the variant created, before the bound -, then, when the
/// price that applies now - the variant's price as the unit of work leaves it, discounted while the discount as the unit of work leaves it runs - is not the last
/// row's price, a row at now. An operation that does not change the applied price writes nothing (06 4.11). It is called by every writer of a price or a discount
/// and by the discount's scheduled bound (<see cref="DiscountBoundarySource"/>): the rows are added to the open unit of work and saved by its owner. Two writers
/// racing over one bound write the same row twice at most - the same moment and price -, which neither the reference (a minimum) nor the label's date can tell.
/// </summary>
internal sealed class PriceHistoryRecorder
{
    private readonly AppDbContext _db;
    private readonly TimeProvider _time;

    public PriceHistoryRecorder(AppDbContext db, TimeProvider time)
    {
        _db = db;
        _time = time;
    }

    /// <summary>
    /// Adds the rows <paramref name="productId"/>'s variants owe and returns them. An applied price of zero or below is a programming error: the price and the
    /// discount rules refuse it before (02 3.8.4, 3.9.1).
    /// </summary>
    public async Task<IReadOnlyList<PriceHistory>> RecordAsync(Guid productId, CancellationToken ct)
    {
        DateTimeOffset now = _time.GetUtcNow();
        StoredDiscount? stored = await PriceHistoryReads.StoredDiscountAsync(_db, productId, ct);
        DiscountTerms? applied = PriceHistoryReads.DiscountAsSaved(_db, productId, stored?.Terms);
        List<ReferenceInput> variants = await PriceHistoryReads.VariantsAsSavedAsync(_db, productId, ct);
        Dictionary<Guid, (decimal Price, DateTimeOffset CreatedAt)> storedVariants = await _db.Variants.AsNoTracking()
            .Where(v => v.ProductId == productId)
            .Select(v => new { v.Id, v.Price, v.CreatedAt })
            .ToDictionaryAsync(v => v.Id, v => (v.Price, v.CreatedAt), ct);
        Dictionary<Guid, PricePoint> last = await PriceHistoryReads.LastRowsAsync(_db, [.. variants.Select(v => v.VariantId)], ct);

        List<PriceHistory> added = [];
        foreach (ReferenceInput variant in variants)
        {
            DateTimeOffset? lastAt = last.TryGetValue(variant.VariantId, out PricePoint known) ? known.StartedAt : null;
            decimal? lastPrice = lastAt is null ? null : known.Price;

            // A variant added in this unit of work owes no bound: it did not exist at any bound that has come.
            if (stored is not null && storedVariants.TryGetValue(variant.VariantId, out (decimal Price, DateTimeOffset CreatedAt) before))
            {
                DateTimeOffset since = stored.DefinedAt > before.CreatedAt ? stored.DefinedAt : before.CreatedAt;
                foreach (PricePoint bound in DiscountRules.OwedBounds(stored.Terms, since, before.Price, lastAt, now))
                {
                    // One moment holds one price: a bound that falls at now is left to the row at now, which carries the price as this unit of work leaves it.
                    if (bound.StartedAt >= now)
                    {
                        continue;
                    }

                    added.Add(Add(variant.VariantId, bound.Price, bound.StartedAt, now));
                    (lastAt, lastPrice) = (bound.StartedAt, bound.Price);
                }
            }

            decimal price = DiscountRules.AppliedPrice(variant.Price, applied, now);
            if (lastPrice != price)
            {
                added.Add(Add(variant.VariantId, price, now, now));
            }
        }

        return added;
    }

    private PriceHistory Add(Guid variantId, decimal price, DateTimeOffset startedAt, DateTimeOffset now)
    {
        if (price <= 0m)
        {
            throw new InvalidOperationException("An applied price is above zero (02 3.8.4, 3.9.1; 06 4.11): the price and the discount rules refuse a zero before the history is written.");
        }

        PriceHistory row = PriceHistory.Create(variantId, price, startedAt, now);
        _db.PriceHistories.Add(row);
        return row;
    }
}
