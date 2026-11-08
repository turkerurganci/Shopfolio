using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

using Shopfolio.Modules.Catalog.Contracts;

namespace Shopfolio.Modules.Catalog;

/// <summary>
/// The last defense of the price and the price label fields (02 3.8.4, 3.8.5; 06 4.6 "Uygulamada"; T336 criteria 7, 8). A save is refused before any SQL is sent
/// when a variant's price or a price history row's price has more than two decimals - the <c>numeric(12,2)</c> column would round it silently and the number the
/// visitor sees would no longer be the number the administrator typed (02 3.8.1) -, when a variant of a product with a measurement unit has no net quantity, when a
/// variant of a product without one carries a net quantity, or when a net quantity has more than three decimals. A price of zero and a net quantity of zero are
/// also the database's checks (<c>price_above_zero</c>, <c>net_quantity_above_zero</c>). The pairing of unit and net quantity is checked from both sides: a written
/// variant against its product's unit (the tracked product, else the stored one), and a product whose unit is written against all its variants (the tracked
/// ones as they would be saved, the rest as stored). The rules are <see cref="PriceLabelRules"/> and <see cref="ItemPricing"/>; this guard only applies them.
/// A row written by a bulk statement does not pass through a save and is not covered here.
/// </summary>
internal sealed class PriceLabelGuard : SaveChangesInterceptor
{
    public static PriceLabelGuard Instance { get; } = new();

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        if (eventData.Context is { } context)
        {
            Plan plan = Collect(context);
            Dictionary<Guid, MeasurementUnit?> units = plan.ProductsToRead.Count == 0
                ? []
                : UnitsQuery(context, plan.ProductsToRead).ToDictionary(p => p.Id, p => p.MeasurementUnit);
            List<StoredVariant> variants = plan.VariantsOf.Count == 0 ? [] : VariantsQuery(context, plan.VariantsOf).ToList();
            Apply(context, plan, units, variants);
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
            Plan plan = Collect(context);
            Dictionary<Guid, MeasurementUnit?> units = plan.ProductsToRead.Count == 0
                ? []
                : await UnitsQuery(context, plan.ProductsToRead).ToDictionaryAsync(p => p.Id, p => p.MeasurementUnit, cancellationToken);
            List<StoredVariant> variants = plan.VariantsOf.Count == 0 ? [] : await VariantsQuery(context, plan.VariantsOf).ToListAsync(cancellationToken);
            Apply(context, plan, units, variants);
        }

        return result;
    }

    // The written rows and what must be read for them: the stored unit of a written variant's untracked product, and the stored variants of a product whose unit is written.
    private static Plan Collect(DbContext context)
    {
        List<EntityEntry> written = [.. context.ChangeTracker.Entries().Where(e => e.State is EntityState.Added or EntityState.Modified)];
        Dictionary<Guid, Product> trackedProducts = context.ChangeTracker.Entries<Product>()
            .Where(e => e.State != EntityState.Deleted)
            .ToDictionary(e => e.Entity.Id, e => e.Entity);

        foreach (EntityEntry entry in written)
        {
            (decimal? price, string owner) = entry.Entity switch
            {
                Variant variant => ((decimal?)variant.Price, $"variant {variant.Id}"),
                PriceHistory history => (history.Price, $"price history row {history.Id}"),
                _ => ((decimal?)null, string.Empty),
            };
            if (price is { } value && !ItemPricing.HasAtMostDecimals(value, ItemPricing.AmountDecimals))
            {
                throw new InvalidOperationException(
                    $"The price of {owner} has more than two decimals: a price is in kuruş and the column would round it (02 3.8.4; K-62).");
            }
        }

        List<Variant> writtenVariants = [.. written.Select(e => e.Entity).OfType<Variant>()];
        HashSet<Guid> productsToRead = [.. writtenVariants.Select(v => v.ProductId).Where(id => !trackedProducts.ContainsKey(id))];
        List<Product> unitWritten = [.. written
            .Where(e => e.Entity is Product && (e.State == EntityState.Added || e.Property(nameof(Product.MeasurementUnit)).IsModified))
            .Select(e => (Product)e.Entity)];
        HashSet<Guid> variantsOf = [.. unitWritten.Where(p => context.Entry(p).State == EntityState.Modified).Select(p => p.Id)];
        return new Plan(writtenVariants, unitWritten, trackedProducts, productsToRead, variantsOf);
    }

    private static void Apply(DbContext context, Plan plan, Dictionary<Guid, MeasurementUnit?> storedUnits, List<StoredVariant> storedVariants)
    {
        foreach (Variant variant in plan.WrittenVariants)
        {
            if (plan.TrackedProducts.TryGetValue(variant.ProductId, out Product? product))
            {
                Check(variant.Id, product.MeasurementUnit, variant.NetQuantity);
            }
            else if (storedUnits.TryGetValue(variant.ProductId, out MeasurementUnit? unit))
            {
                Check(variant.Id, unit, variant.NetQuantity);
            }

            // A variant whose product exists nowhere is the foreign key's refusal, not this guard's.
        }

        Dictionary<Guid, Variant> trackedVariants = context.ChangeTracker.Entries<Variant>()
            .Where(e => e.State != EntityState.Deleted)
            .ToDictionary(e => e.Entity.Id, e => e.Entity);
        HashSet<Guid> deletedVariants = [.. context.ChangeTracker.Entries<Variant>().Where(e => e.State == EntityState.Deleted).Select(e => e.Entity.Id)];
        foreach (Product product in plan.UnitWritten)
        {
            foreach (Variant variant in trackedVariants.Values.Where(v => v.ProductId == product.Id))
            {
                Check(variant.Id, product.MeasurementUnit, variant.NetQuantity);
            }

            foreach (StoredVariant stored in storedVariants.Where(v => v.ProductId == product.Id && !trackedVariants.ContainsKey(v.Id) && !deletedVariants.Contains(v.Id)))
            {
                Check(stored.Id, product.MeasurementUnit, stored.NetQuantity);
            }
        }
    }

    private static void Check(Guid variantId, MeasurementUnit? unit, decimal? netQuantity)
    {
        string? message = PriceLabelRules.CheckNetQuantity(unit, netQuantity) switch
        {
            NetQuantityProblem.Missing =>
                $"Variant {variantId} has no net quantity while its product is sold by measure: each variant carries its own (02 3.8.5; K-1020).",
            NetQuantityProblem.WithoutUnit =>
                $"Variant {variantId} carries a net quantity while its product has no measurement unit (02 3.8.5; 06 4.6).",
            NetQuantityProblem.TooManyDecimals =>
                $"The net quantity of variant {variantId} has more than three decimals, which the column would round (04 7.2.4.13; 06 4.6).",

            // Zero or below is the database's check (net_quantity_above_zero), which names the constraint.
            _ => null,
        };
        if (message is not null)
        {
            throw new InvalidOperationException(message);
        }
    }

    private static IQueryable<StoredUnit> UnitsQuery(DbContext context, HashSet<Guid> productIds)
    {
        return context.Set<Product>().AsNoTracking().Where(p => productIds.Contains(p.Id)).Select(p => new StoredUnit(p.Id, p.MeasurementUnit));
    }

    private static IQueryable<StoredVariant> VariantsQuery(DbContext context, HashSet<Guid> productIds)
    {
        return context.Set<Variant>().AsNoTracking().Where(v => productIds.Contains(v.ProductId)).Select(v => new StoredVariant(v.Id, v.ProductId, v.NetQuantity));
    }

    private sealed record Plan(
        List<Variant> WrittenVariants,
        List<Product> UnitWritten,
        Dictionary<Guid, Product> TrackedProducts,
        HashSet<Guid> ProductsToRead,
        HashSet<Guid> VariantsOf);

    private sealed record StoredUnit(Guid Id, MeasurementUnit? MeasurementUnit);

    private sealed record StoredVariant(Guid Id, Guid ProductId, decimal? NetQuantity);
}
