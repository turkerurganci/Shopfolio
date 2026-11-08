using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Modules.AfterSales.Contracts;
using Shopfolio.Modules.Catalog.Contracts;

namespace Shopfolio.Modules.AfterSales;

/// <summary>
/// The reads of the order module's operation decision (T323; 03 1.11): the periods and the unit standings of an order's items, and the refund IBAN of the order. They only
/// read; the rules they apply are the ones the writers apply (<see cref="AfterSalesPeriodRules"/>, <see cref="ItemQuantities"/>), so the screen and the POST never differ.
/// </summary>
internal sealed partial class AfterSalesModule
{
    public async Task<IReadOnlyDictionary<Guid, ItemPeriodState>> ItemPeriodStatesAsync(IReadOnlyCollection<ItemPeriodQuery> items, DateTimeOffset now, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(items);
        Dictionary<Guid, ItemPeriodQuery> queries = items.GroupBy(i => i.OrderItemId).ToDictionary(g => g.Key, g => g.First());
        Guid[] ids = [.. queries.Keys];
        Dictionary<Guid, ItemPeriodState> states = [];
        if (ids.Length == 0)
        {
            return states;
        }

        IReadOnlyDictionary<Guid, int> closed = await ClosedQuantitiesAsync(ids, ct);
        Dictionary<Guid, DefectClaimStatus> claims = await _db.DefectClaims.AsNoTracking()
            .Where(c => ids.Contains(c.OrderItemId))
            .ToDictionaryAsync(c => c.OrderItemId, c => c.DefectClaimStatus, ct);
        IReadOnlyList<WithdrawalStanding> standings = await WithdrawalStandingsAsync(ids, ct);

        foreach (Guid id in ids)
        {
            ItemPeriodQuery q = queries[id];
            WithdrawalWindowFacts window = new(q.ProductType, q.DeliveryDate, q.DeliveryMarkedAt, q.OrderCreatedAt, q.OrderHasPhysicalItem, q.OrderDeliveryDate);
            bool windowOpen = AfterSalesPeriodRules.IsWithdrawalWindowOpen(window, now);
            DateOnly? windowStart = q.ProductType switch
            {
                ProductType.Physical => q.DeliveryDate,
                ProductType.Service => AfterSalesPeriodRules.ServiceWithdrawalStartDay(q.OrderCreatedAt, q.OrderHasPhysicalItem, q.OrderDeliveryDate),
                _ => null,
            };
            DateOnly? windowLastDay = windowStart is { } start ? TurkeyTime.DateOf(AfterSalesPeriodRules.WithdrawalWindowEnd(start).AddTicks(-1)) : null;
            bool defectOpen = AfterSalesPeriodRules.IsDefectClaimChannelOpen(q.ProductType, q.OrderStatus, q.DeliveryDate, now);
            DateOnly? defectLastDay = q.DeliveryDate?.AddYears(AfterSalesPeriodRules.DefectClaimYears);
            bool conditional = WithdrawalExemptions.IsConditional(q.Exemption);

            int receivable = 0;
            int rejectable = 0;
            int closable = 0;
            foreach (WithdrawalStanding w in standings.Where(s => s.OrderItemId == id))
            {
                if (!AfterSalesPeriodRules.IsAfterDelivery(q.ProductType, q.DeliveryDate, w.DeclarationDate, w.IsDeliveredBeforeDeclaration))
                {
                    continue;
                }

                int awaiting = ItemQuantities.AwaitingReceipt(w.Quantity, w.Received, w.Rejected, w.NotReturned);
                receivable += awaiting + w.NotReturned;
                rejectable += conditional ? awaiting : 0;
                closable += AfterSalesPeriodRules.IsNotReturnedClosureOpen(true, awaiting, w.DeclarationDate, now) ? awaiting : 0;
            }

            states[id] = new ItemPeriodState(
                closed.GetValueOrDefault(id),
                windowOpen,
                windowLastDay,
                defectOpen,
                defectLastDay,
                claims.TryGetValue(id, out DefectClaimStatus claim) ? claim : null,
                q.Exemption is not null,
                conditional,
                receivable,
                rejectable,
                closable);
        }

        return states;
    }

    public async Task<RefundIbanState> RefundIbanStateAsync(Guid orderId, IReadOnlyCollection<Guid> orderItemIds, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(orderItemIds);
        Guid[] ids = [.. orderItemIds];
        bool hasIban = await _db.RefundIbans.AsNoTracking().AnyAsync(i => i.OrderId == orderId, ct);
        bool hasOpenRequest = ids.Length > 0
            && await _db.RefundIbanRequests.AsNoTracking()
                .AnyAsync(r => r.OrderItemId != null && ids.Contains(r.OrderItemId.Value) && r.FulfilledAt == null && r.ClosedAt == null, ct);
        return new RefundIbanState(hasIban, hasOpenRequest);
    }

    /// <summary>The withdrawal declarations of the items with the units received, rejected and closed as not come back against each (K-794).</summary>
    private async Task<IReadOnlyList<WithdrawalStanding>> WithdrawalStandingsAsync(Guid[] itemIds, CancellationToken ct)
    {
        var withdrawals = await _db.Withdrawals.AsNoTracking().Where(w => itemIds.Contains(w.OrderItemId))
            .Select(w => new { w.Id, w.OrderItemId, w.Quantity, w.DeclarationDate, w.IsDeliveredBeforeDeclaration }).ToListAsync(ct);
        Guid[] withdrawalIds = [.. withdrawals.Select(w => w.Id)];
        Dictionary<Guid, int> received = await _db.Returns.AsNoTracking().Where(r => r.WithdrawalId != null && withdrawalIds.Contains(r.WithdrawalId.Value))
            .GroupBy(r => r.WithdrawalId!.Value).Select(g => new { g.Key, Sum = g.Sum(r => r.Quantity) }).ToDictionaryAsync(g => g.Key, g => g.Sum, ct);
        Dictionary<Guid, int> rejected = await _db.ReturnRejections.AsNoTracking().Where(r => withdrawalIds.Contains(r.WithdrawalId))
            .GroupBy(r => r.WithdrawalId).Select(g => new { g.Key, Sum = g.Sum(r => r.Quantity) }).ToDictionaryAsync(g => g.Key, g => g.Sum, ct);
        Dictionary<Guid, int> notReturned = await _db.NotReturnedClosures.AsNoTracking().Where(n => withdrawalIds.Contains(n.WithdrawalId) && n.ReopenedAt == null)
            .GroupBy(n => n.WithdrawalId).Select(g => new { g.Key, Sum = g.Sum(n => n.Quantity) }).ToDictionaryAsync(g => g.Key, g => g.Sum, ct);
        return [.. withdrawals.Select(w => new WithdrawalStanding(
            w.Id, w.OrderItemId, w.Quantity, w.DeclarationDate, w.IsDeliveredBeforeDeclaration, received.GetValueOrDefault(w.Id), rejected.GetValueOrDefault(w.Id), notReturned.GetValueOrDefault(w.Id)))];
    }

    private sealed record WithdrawalStanding(
        Guid Id, Guid OrderItemId, int Quantity, DateOnly DeclarationDate, bool? IsDeliveredBeforeDeclaration, int Received, int Rejected, int NotReturned);
}
