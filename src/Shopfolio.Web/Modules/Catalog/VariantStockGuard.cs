using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

using Shopfolio.Modules.Catalog.Contracts;

namespace Shopfolio.Modules.Catalog;

/// <summary>
/// The last defense of the stock rules by type (06 4.6 "Uygulamada"; 02 3.6.2): a save is refused, before any SQL is sent, when it adds a variant whose stock does not fit
/// its product's type - a physical variant without a number, a digital variant with one - or when it changes a product's type under variants whose stock would no longer fit.
/// The stock field is written by a save only when the variant is added (06 2.6.5; <see cref="ProductVersionGuard"/> refuses the later writes), so adding is where a save can
/// break the rule; the panel's later writes are checked by <see cref="StockCounters.SaveStockQuantityAsync"/> with the same <see cref="StockRules"/>. The product is the
/// tracked one when the same save holds it, otherwise it is read from the database.
/// </summary>
internal sealed class VariantStockGuard : SaveChangesInterceptor
{
    public static VariantStockGuard Instance { get; } = new();

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        if (eventData.Context is { } context)
        {
            Plan plan = PlanOf(context);
            Dictionary<Guid, ProductType> read = plan.ProductIdsToRead.Count == 0
                ? []
                : context.Set<Product>().AsNoTracking().Where(p => plan.ProductIdsToRead.Contains(p.Id)).Select(p => new { p.Id, p.ProductType }).ToDictionary(p => p.Id, p => p.ProductType);
            Dictionary<Guid, List<int?>> stocks = plan.ChangedTypeProductIds.Count == 0
                ? []
                : Group(context.Set<Variant>().AsNoTracking().Where(v => plan.ChangedTypeProductIds.Contains(v.ProductId)).Select(v => new { v.ProductId, v.StockQuantity }).ToList().Select(v => (v.ProductId, v.StockQuantity)));
            Apply(plan, read, stocks);
        }

        return result;
    }

    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is { } context)
        {
            Plan plan = PlanOf(context);
            Dictionary<Guid, ProductType> read = plan.ProductIdsToRead.Count == 0
                ? []
                : await context.Set<Product>().AsNoTracking().Where(p => plan.ProductIdsToRead.Contains(p.Id)).Select(p => new { p.Id, p.ProductType }).ToDictionaryAsync(p => p.Id, p => p.ProductType, cancellationToken);
            Dictionary<Guid, List<int?>> stocks = plan.ChangedTypeProductIds.Count == 0
                ? []
                : Group((await context.Set<Variant>().AsNoTracking().Where(v => plan.ChangedTypeProductIds.Contains(v.ProductId)).Select(v => new { v.ProductId, v.StockQuantity }).ToListAsync(cancellationToken)).Select(v => (v.ProductId, v.StockQuantity)));
            Apply(plan, read, stocks);
        }

        return result;
    }

    private static Dictionary<Guid, List<int?>> Group(IEnumerable<(Guid ProductId, int? Stock)> rows)
    {
        return rows.GroupBy(r => r.ProductId).ToDictionary(g => g.Key, g => g.Select(r => r.Stock).ToList());
    }

    private static Plan PlanOf(DbContext context)
    {
        List<(Guid VariantId, Guid ProductId, int? Stock)> added = [];
        Dictionary<Guid, ProductType> tracked = [];
        List<(Guid ProductId, ProductType Type)> changed = [];
        foreach (EntityEntry entry in context.ChangeTracker.Entries())
        {
            switch (entry.Entity)
            {
                case Variant variant when entry.State == EntityState.Added:
                    added.Add((variant.Id, variant.ProductId, variant.StockQuantity));
                    break;
                case Product product when entry.State is EntityState.Added or EntityState.Modified or EntityState.Unchanged:
                    tracked[product.Id] = product.ProductType;
                    if (entry.State == EntityState.Modified && entry.Property(nameof(Product.ProductType)).IsModified)
                    {
                        changed.Add((product.Id, product.ProductType));
                    }

                    break;
                default:
                    break;
            }
        }

        HashSet<Guid> toRead = [.. added.Select(a => a.ProductId).Where(id => !tracked.ContainsKey(id))];
        return new Plan(added, tracked, changed, toRead, [.. changed.Select(c => c.ProductId)]);
    }

    private static void Apply(Plan plan, Dictionary<Guid, ProductType> read, Dictionary<Guid, List<int?>> storedStocks)
    {
        foreach ((Guid variantId, Guid productId, int? stock) in plan.Added)
        {
            if (!plan.Tracked.TryGetValue(productId, out ProductType type) && !read.TryGetValue(productId, out type))
            {
                continue; // The product does not exist; the foreign key refuses the row.
            }

            Refuse(StockRules.Check(type, stock), $"Variant {variantId}", type);
        }

        foreach ((Guid productId, ProductType type) in plan.Changed)
        {
            // The variants the same save adds are checked above against the new type; the stored ones are checked here.
            foreach (int? stock in storedStocks.GetValueOrDefault(productId) ?? [])
            {
                Refuse(StockRules.Check(type, stock), $"A stored variant of product {productId}", type);
            }
        }
    }

    private static void Refuse(IReadOnlyList<StockProblem> problems, string what, ProductType type)
    {
        foreach (StockProblem problem in problems)
        {
            throw new InvalidOperationException(problem switch
            {
                StockProblem.PhysicalWithoutStock =>
                    $"{what} has no stock number under the {type} type: a physical product's variant has a number (02 3.6.2).",
                StockProblem.DigitalWithStock =>
                    $"{what} carries a stock number under the {type} type: a digital product has no stock field and is sold without limit (02 3.6.2).",
                _ =>
                    $"{what} has a stock number below zero: the stock is a whole number that never goes below zero (02 3.6.1).",
            });
        }
    }

    private sealed record Plan(
        List<(Guid VariantId, Guid ProductId, int? Stock)> Added,
        Dictionary<Guid, ProductType> Tracked,
        List<(Guid ProductId, ProductType Type)> Changed,
        HashSet<Guid> ProductIdsToRead,
        HashSet<Guid> ChangedTypeProductIds);
}
