using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Modules.Catalog.Contracts.Pricing;

namespace Shopfolio.Modules.Catalog;

/// <summary>
/// A product's dated percentage discount (T337; 02 3.9.1-3.9.5, 5.11.1; 06 4.11, 4.12; 04 9.5.8, 9.5.17). Saving checks the rate and the range, replaces the
/// product's discount row - a new definition replaces the record, the row is never updated (06 4.12; <see cref="CatalogRecordsGuard"/>) -, brings the price history up
/// to the change (<see cref="PriceHistoryRecorder"/>) and checks every variant against its own rounded discounted price and its reference price, which the system
/// computes and nobody enters (02 3.9.3). A refusal names the variant and writes nothing: the changes this call tracked are taken back, and the caller's runner rolls
/// back a refused result as well. The writes join the caller's open transaction and never open or save one (09 9.1 item 3); outside one they throw. The discount is
/// a part of the product form: the form that saves it raises the product's version once (06 2.6.2; <see cref="ProductVersionGuard"/>), as for the gallery.
/// </summary>
internal sealed class Discounts
{
    private readonly AppDbContext _db;
    private readonly TimeProvider _time;
    private readonly PriceHistoryRecorder _recorder;

    public Discounts(AppDbContext db, TimeProvider time, PriceHistoryRecorder recorder)
    {
        _db = db;
        _time = time;
        _recorder = recorder;
    }

    /// <summary>Saves <paramref name="productId"/>'s discount: the rate in percent and the two dates of its range, a past or a future range alike (04 7.2.4.18).</summary>
    public async Task<DiscountSaveResult> SaveAsync(Guid productId, decimal percentage, DateOnly startDate, DateOnly endDate, CancellationToken ct)
    {
        RequireTransaction();

        // The product form's save (UC-114) gives the discount to a product it writes in the same unit of work - a new product is not in the table yet, and a changed type is not
        // stored yet: the product as this unit of work leaves it is the one whose rule applies.
        var product = _db.Products.Local.Where(p => p.Id == productId).Select(p => new { p.ProductType, p.WithdrawalExemptionReason }).SingleOrDefault()
            ?? await _db.Products.AsNoTracking()
                .Where(p => p.Id == productId)
                .Select(p => new { p.ProductType, p.WithdrawalExemptionReason })
                .SingleOrDefaultAsync(ct);
        if (product is null)
        {
            return DiscountSaveResult.ProductNotFound;
        }

        DiscountTerms terms = new(percentage, startDate, endDate);
        switch (DiscountRules.CheckTerms(terms))
        {
            case DiscountProblem.InvalidPercentage:
                return DiscountSaveResult.InvalidPercentage;
            case DiscountProblem.EndBeforeStart:
                return DiscountSaveResult.EndBeforeStart;
        }

        // A zero is refused before anything is written: the history never takes a price of zero (06 4.11).
        ReferenceRule rule = ReferencePriceRules.RuleOf(product.ProductType, product.WithdrawalExemptionReason);
        List<ReferenceInput> variants = await PriceHistoryReads.VariantsAsSavedAsync(_db, productId, ct);
        if (variants.FirstOrDefault(v => DiscountRules.ReachesZero(v.Price, percentage)) is { } zero)
        {
            Dictionary<Guid, decimal> known = await PriceHistoryReads.ReferencesAsync(_db, rule, DiscountRules.StartsAt(terms), [zero], ct);
            return DiscountSaveResult.DiscountedPriceZero(zero.VariantId, known[zero.VariantId]);
        }

        Discount? replaced = await _db.Discounts.SingleOrDefaultAsync(d => d.ProductId == productId, ct);
        if (replaced is not null)
        {
            _db.Discounts.Remove(replaced);
        }

        Discount discount = Discount.Create(productId, percentage, startDate, endDate, _time.GetUtcNow());
        _db.Discounts.Add(discount);
        IReadOnlyList<PriceHistory> rows = await _recorder.RecordAsync(productId, ct);

        // The reference reads the history as this unit of work leaves it: a bound the earlier discount owed is a price that applied (02 3.9.3).
        Dictionary<Guid, decimal> references = await PriceHistoryReads.ReferencesAsync(_db, rule, DiscountRules.StartsAt(terms), variants, ct);
        foreach (ReferenceInput variant in variants)
        {
            decimal reference = references[variant.VariantId];
            if (DiscountRules.CheckVariant(variant.Price, reference, percentage) != DiscountProblem.None)
            {
                Undo(discount, replaced, rows);
                return DiscountSaveResult.NotBelowReference(variant.VariantId, DiscountRules.DiscountedPrice(variant.Price, percentage), reference);
            }
        }

        return DiscountSaveResult.Saved;
    }

    /// <summary>Removes <paramref name="productId"/>'s discount (06 4.12): a running discount's removal writes the variants' own prices to the history (K-1041 (3)).</summary>
    public async Task<DiscountRemoveResult> RemoveAsync(Guid productId, CancellationToken ct)
    {
        RequireTransaction();
        Discount? discount = await _db.Discounts.SingleOrDefaultAsync(d => d.ProductId == productId, ct);
        if (discount is null)
        {
            return DiscountRemoveResult.NotFound;
        }

        _db.Discounts.Remove(discount);
        await _recorder.RecordAsync(productId, ct);
        return DiscountRemoveResult.Removed;
    }

    /// <summary>
    /// The product's discount as the form's section shows it (04 9.5.8): the rate, the dates, the state now, and per variant its price, its reference price and its
    /// discounted price. Null when the product has no discount. A plain read.
    /// </summary>
    public async Task<DiscountView?> GetAsync(Guid productId, CancellationToken ct)
    {
        var row = await _db.Discounts.AsNoTracking()
            .Where(d => d.ProductId == productId)
            .Join(_db.Products.AsNoTracking(), d => d.ProductId, p => p.Id, (d, p) => new { d.Percentage, d.StartDate, d.EndDate, p.ProductType, p.WithdrawalExemptionReason })
            .SingleOrDefaultAsync(ct);
        if (row is null)
        {
            return null;
        }

        DiscountTerms terms = new(row.Percentage, row.StartDate, row.EndDate);
        List<ReferenceInput> variants = await PriceHistoryReads.VariantsAsSavedAsync(_db, productId, ct);
        Dictionary<Guid, decimal> references = await PriceHistoryReads.ReferencesAsync(
            _db,
            ReferencePriceRules.RuleOf(row.ProductType, row.WithdrawalExemptionReason),
            DiscountRules.StartsAt(terms),
            variants,
            ct);
        return new DiscountView(
            terms.Percentage,
            terms.StartDate,
            terms.EndDate,
            DiscountRules.StateAt(terms, _time.GetUtcNow()),
            [.. variants.Select(v => new DiscountVariantLine(v.VariantId, v.Price, references[v.VariantId], DiscountRules.DiscountedPrice(v.Price, terms.Percentage)))]);
    }

    // The refusal writes nothing (02 3.9.1, 3.9.5): the rows and the discount this call added leave the unit of work and the replaced row stays.
    private void Undo(Discount added, Discount? replaced, IReadOnlyList<PriceHistory> rows)
    {
        foreach (PriceHistory row in rows)
        {
            _db.Entry(row).State = EntityState.Detached;
        }

        _db.Entry(added).State = EntityState.Detached;
        if (replaced is not null)
        {
            _db.Entry(replaced).State = EntityState.Unchanged;
        }
    }

    private void RequireTransaction()
    {
        if (_db.Database.CurrentTransaction is null)
        {
            throw new InvalidOperationException("A discount is written only inside the caller's transaction (09 9.1 item 3).");
        }
    }
}
