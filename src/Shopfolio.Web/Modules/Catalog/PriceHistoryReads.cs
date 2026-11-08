using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

using Shopfolio.Infrastructure;

namespace Shopfolio.Modules.Catalog;

/// <summary>A variant as the reference price reads it: its own price and the first moment it was on sale (06 4.6).</summary>
internal sealed record ReferenceInput(Guid VariantId, decimal Price, DateTimeOffset? FirstOnSaleAt);

/// <summary>
/// The reads of the price history and of the discount the price rules need (06 4.11, 4.12; 02 3.9.3, 3.9.4): the stored rows together with the rows the open unit
/// of work has added but not yet saved - a price change and a discount written in one transaction see each other's rows -, the variants and the discount as the
/// unit of work leaves them, and the reference prices computed from them by <see cref="ReferencePriceRules"/>. Plain reads: nothing is written and no lock is taken.
/// </summary>
internal static class PriceHistoryReads
{
    /// <summary>The last row of each variant (by its moment, the key breaking a tie); a variant without a row is not in the result.</summary>
    public static async Task<Dictionary<Guid, PricePoint>> LastRowsAsync(AppDbContext db, IReadOnlyCollection<Guid> variantIds, CancellationToken ct)
    {
        Dictionary<Guid, PricePoint> last = variantIds.Count == 0
            ? []
            : (await db.Variants.AsNoTracking()
                .Where(v => variantIds.Contains(v.Id))
                .Select(v => new
                {
                    v.Id,
                    Last = db.PriceHistories.Where(h => h.VariantId == v.Id).OrderByDescending(h => h.StartedAt).ThenByDescending(h => h.Id)
                        .Select(h => new { h.StartedAt, h.Price }).FirstOrDefault(),
                })
                .ToListAsync(ct))
                .Where(r => r.Last is not null)
                .ToDictionary(r => r.Id, r => new PricePoint(r.Last!.StartedAt, r.Last.Price));

        foreach (PriceHistory row in AddedRows(db).Where(r => variantIds.Contains(r.VariantId)))
        {
            if (!last.TryGetValue(row.VariantId, out PricePoint known) || known.StartedAt <= row.StartedAt)
            {
                last[row.VariantId] = new PricePoint(row.StartedAt, row.Price);
            }
        }

        return last;
    }

    /// <summary>
    /// The reference price of each variant for a discount starting at <paramref name="discountStart"/> (02 3.9.3, 3.9.4): the rows before the start - stored and
    /// added - go to <see cref="ReferencePriceRules.Reference"/>.
    /// </summary>
    public static async Task<Dictionary<Guid, decimal>> ReferencesAsync(
        AppDbContext db,
        ReferenceRule rule,
        DateTimeOffset discountStart,
        IReadOnlyList<ReferenceInput> variants,
        CancellationToken ct)
    {
        List<Guid> ids = [.. variants.Select(v => v.VariantId)];
        List<(Guid VariantId, PricePoint Point)> rows = ids.Count == 0
            ? []
            : [.. (await db.PriceHistories.AsNoTracking()
                .Where(h => ids.Contains(h.VariantId) && h.StartedAt < discountStart)
                .Select(h => new { h.VariantId, h.StartedAt, h.Price })
                .ToListAsync(ct))
                .Select(h => (h.VariantId, new PricePoint(h.StartedAt, h.Price)))];
        rows.AddRange(AddedRows(db).Where(r => ids.Contains(r.VariantId) && r.StartedAt < discountStart).Select(r => (r.VariantId, new PricePoint(r.StartedAt, r.Price))));

        return variants.ToDictionary(
            v => v.VariantId,
            v => ReferencePriceRules.Reference(rule, discountStart, v.FirstOnSaleAt, rows.Where(r => r.VariantId == v.VariantId).Select(r => r.Point), v.Price));
    }

    /// <summary>
    /// The product's variants as the unit of work leaves them (06 4.6): the stored ones with the tracked values over them, the added ones, without the deleted ones.
    /// </summary>
    public static async Task<List<ReferenceInput>> VariantsAsSavedAsync(AppDbContext db, Guid productId, CancellationToken ct)
    {
        Dictionary<Guid, ReferenceInput> variants = await db.Variants.AsNoTracking()
            .Where(v => v.ProductId == productId)
            .Select(v => new ReferenceInput(v.Id, v.Price, v.FirstOnSaleAt))
            .ToDictionaryAsync(v => v.VariantId, ct);
        foreach (EntityEntry<Variant> entry in db.ChangeTracker.Entries<Variant>().Where(e => e.Entity.ProductId == productId))
        {
            if (entry.State == EntityState.Deleted)
            {
                variants.Remove(entry.Entity.Id);
            }
            else if (entry.State != EntityState.Detached)
            {
                variants[entry.Entity.Id] = new ReferenceInput(entry.Entity.Id, entry.Entity.Price, entry.Entity.FirstOnSaleAt);
            }
        }

        return [.. variants.Values.OrderBy(v => v.VariantId)];
    }

    /// <summary>
    /// The product's discount as the unit of work leaves it (06 4.12): a tracked row that is not deleted, else none when the tracked row is deleted, else the
    /// stored one. <paramref name="stored"/> is the stored row's terms, read by the caller.
    /// </summary>
    public static DiscountTerms? DiscountAsSaved(AppDbContext db, Guid productId, DiscountTerms? stored)
    {
        List<EntityEntry<Discount>> tracked =
            [.. db.ChangeTracker.Entries<Discount>().Where(e => e.Entity.ProductId == productId && e.State != EntityState.Detached)];
        if (tracked.FirstOrDefault(e => e.State != EntityState.Deleted) is { } kept)
        {
            return new DiscountTerms(kept.Entity.Percentage, kept.Entity.StartDate, kept.Entity.EndDate);
        }

        return tracked.Count > 0 ? null : stored;
    }

    /// <summary>The stored discount of a product with the moment it was defined (its row's creation: a new definition replaces the row, 06 4.12); null without one.</summary>
    public static Task<StoredDiscount?> StoredDiscountAsync(AppDbContext db, Guid productId, CancellationToken ct)
    {
        return db.Discounts.AsNoTracking()
            .Where(d => d.ProductId == productId)
            .Select(d => new StoredDiscount(new DiscountTerms(d.Percentage, d.StartDate, d.EndDate), d.CreatedAt))
            .SingleOrDefaultAsync(ct);
    }

    private static IEnumerable<PriceHistory> AddedRows(AppDbContext db)
    {
        return db.ChangeTracker.Entries<PriceHistory>().Where(e => e.State == EntityState.Added).Select(e => e.Entity);
    }
}

/// <summary>A stored discount: its terms and the moment it was defined.</summary>
internal sealed record StoredDiscount(DiscountTerms Terms, DateTimeOffset DefinedAt);
