using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

using Shopfolio.Infrastructure.DataModel;

namespace Shopfolio.Modules.Catalog;

/// <summary>
/// The product's version covers its form (06 2.6.2; 05 7.3.1; MK-20, K-980): the variants, the options and their values, the category links,
/// the image links and the discount are saved with the product and raise its version, but the stock and capacity field is not: the panel writes
/// it with its own conditional update and the system's reservation, final decrease and return are the counters' conditional updates
/// (<see cref="StockCounters"/>), which never touch the product's version. The guard refuses, before any SQL is sent, a save that writes a member
/// record of a tracked product without raising that product's version by one (the product form loads the product tracked with its rows;
/// 09 10.2 item 2), and any save that writes the stock field, the reserved quantity or a coupon's counts - those are written only by their
/// conditional updates (06 2.6.5; 09 10.2 item 4). A member written while its product is not tracked (a test's seed) is not looked at. It does
/// only this check: events and cache invalidation are not attached to it.
/// </summary>
internal sealed class ProductVersionGuard : SaveChangesInterceptor
{
    /// <summary>The records saved with the product form (06 2.6.2).</summary>
    public static readonly IReadOnlyList<string> FormRecords =
    [
        typeof(Variant).FullName!,
        typeof(Option).FullName!,
        typeof(OptionValue).FullName!,
        typeof(ProductCategory).FullName!,
        typeof(ProductImage).FullName!,
        typeof(Discount).FullName!,
    ];

    /// <summary>The fields written only by a conditional update (06 2.6.5): never by a save.</summary>
    public static readonly IReadOnlyList<(Type Type, string Field)> ConditionalFields =
    [
        (typeof(Variant), nameof(Variant.StockQuantity)),
        (typeof(Variant), nameof(Variant.ReservedQuantity)),
        (typeof(Coupon), nameof(Coupon.UsedCount)),
        (typeof(Coupon), nameof(Coupon.ReservedCount)),
    ];

    public static ProductVersionGuard Instance { get; } = new();

    public static VersionScope Scope { get; } = new(typeof(Product).FullName!, FormRecords, null, [], []);

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        Check(eventData.Context);
        return result;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        Check(eventData.Context);
        return ValueTask.FromResult(result);
    }

    private static void Check(DbContext? context)
    {
        if (context is null)
        {
            return;
        }

        foreach (EntityEntry entry in context.ChangeTracker.Entries().Where(e => e.State == EntityState.Modified))
        {
            foreach ((Type type, string field) in ConditionalFields)
            {
                if (entry.Metadata.ClrType == type && entry.Property(field).IsModified)
                {
                    throw new InvalidOperationException(
                        $"{type.Name}.{field} is written only by its conditional update, never by a save (06 2.6.5; 09 10.2 item 4).");
                }
            }
        }

        foreach ((EntityEntry entry, Guid? productId) in Scope.Writes(context))
        {
            if (productId is Guid id && Scope.IsTracked(context, id) && !Scope.IsRaised(context, id))
            {
                throw new InvalidOperationException(
                    $"A {entry.Metadata.ClrType.Name} record of product {id} is saved and the product's version is not raised by one: call BumpVersion (06 2.6.2; 09 10.2 item 2).");
            }
        }
    }
}
