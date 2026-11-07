using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Shopfolio.Infrastructure;
using Shopfolio.Modules.AfterSales.Contracts;

namespace Shopfolio.Modules.AfterSales;

/// <summary>The AfterSales module's implementation of its open surface; internal, reached only through <see cref="IAfterSalesModule"/>.</summary>
internal sealed class AfterSalesModule : IAfterSalesModule
{
    private readonly AppDbContext _db;

    public AfterSalesModule(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyDictionary<Guid, int>> ClosedQuantitiesAsync(IReadOnlyCollection<Guid> orderItemIds, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(orderItemIds);
        Guid[] ids = [.. orderItemIds.Distinct()];
        Dictionary<Guid, int> closed = [];
        if (ids.Length == 0)
        {
            return closed;
        }

        // The four closing records, each from the database and from the context's added rows: the record the caller wrote a moment ago is part of the answer.
        Add(closed, await _db.Cancellations.Where(c => ids.Contains(c.OrderItemId)).GroupBy(c => c.OrderItemId)
            .Select(g => new KeyValuePair<Guid, int>(g.Key, g.Sum(c => c.Quantity))).ToListAsync(ct));
        Add(closed, await _db.ItemRemovals.Where(r => ids.Contains(r.OrderItemId)).GroupBy(r => r.OrderItemId)
            .Select(g => new KeyValuePair<Guid, int>(g.Key, g.Sum(r => r.Quantity))).ToListAsync(ct));
        Add(closed, await _db.LateDeliveryTerminations.Where(t => ids.Contains(t.OrderItemId)).GroupBy(t => t.OrderItemId)
            .Select(g => new KeyValuePair<Guid, int>(g.Key, g.Sum(t => t.Quantity))).ToListAsync(ct));
        Add(closed, await _db.Withdrawals.Where(w => ids.Contains(w.OrderItemId)).GroupBy(w => w.OrderItemId)
            .Select(g => new KeyValuePair<Guid, int>(g.Key, g.Sum(w => w.Quantity))).ToListAsync(ct));

        Add(closed, Added<Cancellation>(ids, c => c.OrderItemId, c => c.Quantity));
        Add(closed, Added<ItemRemoval>(ids, r => r.OrderItemId, r => r.Quantity));
        Add(closed, Added<LateDeliveryTermination>(ids, t => t.OrderItemId, t => t.Quantity));
        Add(closed, Added<Withdrawal>(ids, w => w.OrderItemId, w => w.Quantity));
        return closed;
    }

    private List<KeyValuePair<Guid, int>> Added<TRecord>(Guid[] ids, Func<TRecord, Guid> itemOf, Func<TRecord, int> quantityOf)
        where TRecord : class
    {
        return [.. _db.ChangeTracker.Entries<TRecord>()
            .Where(e => e.State == EntityState.Added && ids.Contains(itemOf(e.Entity)))
            .GroupBy(e => itemOf(e.Entity))
            .Select(g => new KeyValuePair<Guid, int>(g.Key, g.Sum(e => quantityOf(e.Entity))))];
    }

    private static void Add(Dictionary<Guid, int> closed, IEnumerable<KeyValuePair<Guid, int>> part)
    {
        foreach ((Guid itemId, int quantity) in part)
        {
            closed[itemId] = closed.GetValueOrDefault(itemId) + quantity;
        }
    }
}

/// <summary>The module's single registration (09 6 item 3): <c>Program.cs</c> calls <c>AddAfterSalesModule</c> once.</summary>
internal static class AfterSalesModuleRegistration
{
    public static IServiceCollection AddAfterSalesModule(this IServiceCollection services)
    {
        services.AddScoped<IAfterSalesModule, AfterSalesModule>();
        return services;
    }
}
