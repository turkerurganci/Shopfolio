using Microsoft.EntityFrameworkCore;

using Shopfolio.Modules.AfterSales.Contracts;

namespace Shopfolio.Modules.AfterSales;

/// <summary>
/// The reads of the panel's return steps (T528; 07 UC-140...UC-142; 04 9.9.27, 9.9.28): the withdrawals whose goods are still on the way back - or closed as not come back - and the
/// receipts with their stock state. The units are counted by the writers' one formula (<see cref="ItemQuantities.AwaitingReceipt"/>; D-568), so the screen offers exactly what
/// the POST accepts.
/// </summary>
internal sealed partial class AfterSalesModule
{
    public async Task<ReturnStepFacts> ReturnStepFactsAsync(IReadOnlyCollection<Guid> orderItemIds, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(orderItemIds);
        Guid[] ids = [.. orderItemIds.Distinct()];
        if (ids.Length == 0)
        {
            return ReturnStepFacts.Empty;
        }

        IReadOnlyList<WithdrawalStanding> standings = await WithdrawalStandingsAsync(ids, ct);
        Dictionary<Guid, DateTimeOffset> declaredAt = await _db.Withdrawals.AsNoTracking().Where(w => ids.Contains(w.OrderItemId))
            .ToDictionaryAsync(w => w.Id, w => w.CreatedAt, ct);
        List<ReturnableWithdrawal> withdrawals =
        [
            .. standings
                .Select(s => new ReturnableWithdrawal(
                    s.Id, s.OrderItemId, s.DeclarationDate, declaredAt.GetValueOrDefault(s.Id), ItemQuantities.AwaitingReceipt(s.Quantity, s.Received, s.Rejected, s.NotReturned), s.NotReturned))
                .Where(w => w.Awaiting + w.NotReturned > 0)
                .OrderBy(w => w.DeclaredAt),
        ];
        List<ReturnReceiptFact> receipts = await _db.Returns.AsNoTracking().Where(r => ids.Contains(r.OrderItemId))
            .OrderBy(r => r.CreatedAt)
            .Select(r => new ReturnReceiptFact(r.Id, r.OrderItemId, r.Quantity, r.ArrivalDate, r.StockAddedAt))
            .ToListAsync(ct);
        return new ReturnStepFacts(withdrawals, receipts);
    }
}
