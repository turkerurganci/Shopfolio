using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Disposal;
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

    public async Task ErasePersonalDataAsync(OrderPersonalDataScope scope, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(scope);
        if (_db.Database.CurrentTransaction is null)
        {
            throw new InvalidOperationException("The after-sales records' personal data is emptied inside the caller's transaction (09 6 item 3).");
        }

        if (scope.OrderIds.Count == 0)
        {
            return;
        }

        Guid[] orderIds = [.. scope.OrderIds];
        Guid[] itemIds = [.. scope.OrderItemIds];
        Guid[] refundIds = [.. scope.RefundIds];

        // The records are loaded and tracked, never written by a bulk statement: the order side's version guard reads the tracked rows and finds
        // the order of each through its tracked item or refund (09 10.2; 06 2.6.4). The ids are the caller's: its items and refunds are tracked.
        foreach (Cancellation cancellation in await _db.Cancellations.Where(c => itemIds.Contains(c.OrderItemId)).ToListAsync(ct))
        {
            cancellation.AdminId = null;
            cancellation.AdminName = null;
            cancellation.ReasonNote = null;
            cancellation.RefundIbanId = null;
        }

        foreach (ItemRemoval removal in await _db.ItemRemovals.Where(r => itemIds.Contains(r.OrderItemId)).ToListAsync(ct))
        {
            removal.AdminId = null;
            removal.AdminName = null;
        }

        foreach (LateDeliveryTermination termination in await _db.LateDeliveryTerminations.Where(t => itemIds.Contains(t.OrderItemId)).ToListAsync(ct))
        {
            termination.AdminId = null;
            termination.AdminName = null;
            termination.RefundIbanId = null;
        }

        foreach (Withdrawal withdrawal in await _db.Withdrawals.Where(w => itemIds.Contains(w.OrderItemId)).ToListAsync(ct))
        {
            withdrawal.AdminId = null;
            withdrawal.AdminName = null;
            withdrawal.RefundIbanId = null;
        }

        foreach (Return receipt in await _db.Returns.Where(r => itemIds.Contains(r.OrderItemId)).ToListAsync(ct))
        {
            receipt.AdminId = null;
            receipt.AdminName = null;
        }

        foreach (ReturnRejection rejection in await _db.ReturnRejections.Where(r => itemIds.Contains(r.OrderItemId)).ToListAsync(ct))
        {
            rejection.AdminId = null;
            rejection.AdminName = null;
        }

        foreach (NotReturnedClosure closure in await _db.NotReturnedClosures.Where(c => itemIds.Contains(c.OrderItemId)).ToListAsync(ct))
        {
            closure.AdminId = null;
            closure.AdminName = null;
        }

        // The IBAN requests (linked to an item or to a refund) and the standing IBANs of the orders are deleted; the links to them were emptied above.
        List<RefundIbanRequest> requests = await _db.RefundIbanRequests
            .Where(r => (r.OrderItemId != null && itemIds.Contains(r.OrderItemId.Value)) || (r.RefundId != null && refundIds.Contains(r.RefundId.Value)))
            .ToListAsync(ct);
        _db.RefundIbanRequests.RemoveRange(requests);

        List<RefundIban> ibans = await _db.RefundIbans.Where(i => orderIds.Contains(i.OrderId)).ToListAsync(ct);
        _db.RefundIbans.RemoveRange(ibans);
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
        await AddSavedClosedAsync(closed, ids, ct);
        Add(closed, Added<Cancellation>(ids, c => c.OrderItemId, c => c.Quantity));
        Add(closed, Added<ItemRemoval>(ids, r => r.OrderItemId, r => r.Quantity));
        Add(closed, Added<LateDeliveryTermination>(ids, t => t.OrderItemId, t => t.Quantity));
        Add(closed, Added<Withdrawal>(ids, w => w.OrderItemId, w => w.Quantity));
        return closed;
    }

    public async Task<IReadOnlyDictionary<Guid, ItemUnitFacts>> UnitFactsAsync(IReadOnlyCollection<Guid> orderItemIds, bool includeUnsaved, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(orderItemIds);
        Guid[] ids = [.. orderItemIds.Distinct()];
        Dictionary<Guid, ItemUnitFacts> facts = ids.ToDictionary(id => id, _ => new ItemUnitFacts(0, 0, 0, 0, 0));
        if (ids.Length == 0)
        {
            return facts;
        }

        // The closing records: from ClosedQuantitiesAsync with the unsaved rows, or from the saved rows alone.
        Dictionary<Guid, int> closed = [];
        if (includeUnsaved)
        {
            foreach ((Guid id, int quantity) in await ClosedQuantitiesAsync(ids, ct))
            {
                closed[id] = quantity;
            }
        }
        else
        {
            await AddSavedClosedAsync(closed, ids, ct);
        }

        Dictionary<Guid, int> withdrawn = [];
        Add(withdrawn, await _db.Withdrawals.Where(w => ids.Contains(w.OrderItemId)).GroupBy(w => w.OrderItemId)
            .Select(g => new KeyValuePair<Guid, int>(g.Key, g.Sum(w => w.Quantity))).ToListAsync(ct));

        // The withdrawal's receipt is the return record linked to a withdrawal (06 4.26); the termination's and the defect's receipts are not a withdrawal's.
        Dictionary<Guid, int> received = [];
        Add(received, await _db.Returns.Where(r => ids.Contains(r.OrderItemId) && r.WithdrawalId != null).GroupBy(r => r.OrderItemId)
            .Select(g => new KeyValuePair<Guid, int>(g.Key, g.Sum(r => r.Quantity))).ToListAsync(ct));
        Dictionary<Guid, int> rejected = [];
        Add(rejected, await _db.ReturnRejections.Where(r => ids.Contains(r.OrderItemId)).GroupBy(r => r.OrderItemId)
            .Select(g => new KeyValuePair<Guid, int>(g.Key, g.Sum(r => r.Quantity))).ToListAsync(ct));
        Dictionary<Guid, int> notReturned = [];
        Add(notReturned, await _db.NotReturnedClosures.Where(n => ids.Contains(n.OrderItemId) && n.ReopenedAt == null).GroupBy(n => n.OrderItemId)
            .Select(g => new KeyValuePair<Guid, int>(g.Key, g.Sum(n => n.Quantity))).ToListAsync(ct));
        if (includeUnsaved)
        {
            Add(withdrawn, Added<Withdrawal>(ids, w => w.OrderItemId, w => w.Quantity));
            Add(received, AddedWhere<Return>(ids, r => r.WithdrawalId != null, r => r.OrderItemId, r => r.Quantity));
            Add(rejected, Added<ReturnRejection>(ids, r => r.OrderItemId, r => r.Quantity));
            Add(notReturned, AddedWhere<NotReturnedClosure>(ids, n => n.ReopenedAt == null, n => n.OrderItemId, n => n.Quantity));
        }

        foreach (Guid id in ids)
        {
            facts[id] = new ItemUnitFacts(
                closed.GetValueOrDefault(id), withdrawn.GetValueOrDefault(id), received.GetValueOrDefault(id), rejected.GetValueOrDefault(id), notReturned.GetValueOrDefault(id));
        }

        return facts;
    }

    private async Task AddSavedClosedAsync(Dictionary<Guid, int> closed, Guid[] ids, CancellationToken ct)
    {
        Add(closed, await _db.Cancellations.Where(c => ids.Contains(c.OrderItemId)).GroupBy(c => c.OrderItemId)
            .Select(g => new KeyValuePair<Guid, int>(g.Key, g.Sum(c => c.Quantity))).ToListAsync(ct));
        Add(closed, await _db.ItemRemovals.Where(r => ids.Contains(r.OrderItemId)).GroupBy(r => r.OrderItemId)
            .Select(g => new KeyValuePair<Guid, int>(g.Key, g.Sum(r => r.Quantity))).ToListAsync(ct));
        Add(closed, await _db.LateDeliveryTerminations.Where(t => ids.Contains(t.OrderItemId)).GroupBy(t => t.OrderItemId)
            .Select(g => new KeyValuePair<Guid, int>(g.Key, g.Sum(t => t.Quantity))).ToListAsync(ct));
        Add(closed, await _db.Withdrawals.Where(w => ids.Contains(w.OrderItemId)).GroupBy(w => w.OrderItemId)
            .Select(g => new KeyValuePair<Guid, int>(g.Key, g.Sum(w => w.Quantity))).ToListAsync(ct));
    }

    private List<KeyValuePair<Guid, int>> AddedWhere<TRecord>(Guid[] ids, Func<TRecord, bool> filter, Func<TRecord, Guid> itemOf, Func<TRecord, int> quantityOf)
        where TRecord : class
    {
        return [.. _db.ChangeTracker.Entries<TRecord>()
            .Where(e => e.State == EntityState.Added && ids.Contains(itemOf(e.Entity)) && filter(e.Entity))
            .GroupBy(e => itemOf(e.Entity))
            .Select(g => new KeyValuePair<Guid, int>(g.Key, g.Sum(e => quantityOf(e.Entity))))];
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
        services.AddDisposalPart<DefectClaimDisposalPart>();
        return services;
    }
}
