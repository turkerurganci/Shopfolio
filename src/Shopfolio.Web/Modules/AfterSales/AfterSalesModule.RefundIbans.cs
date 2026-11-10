using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Disposal;
using Shopfolio.Infrastructure.Values;
using Shopfolio.Modules.AfterSales.Contracts;

namespace Shopfolio.Modules.AfterSales;

/// <summary>
/// The refund IBAN's deletion day (Z-38; 03 4.1.47, 4.2.8; 05 5.2.5): the scheduler's reads and the day's outcome. The order module's source takes the
/// order's row lock and decides; this part reads the IBAN's users and writes the outcome in the caller's transaction.
/// </summary>
internal sealed partial class AfterSalesModule
{
    public async Task<IReadOnlyList<DueRefundIbanDeletion>> FindDueRefundIbanDeletionsAsync(DateTimeOffset now, CancellationToken ct)
    {
        return await _db.RefundIbans.AsNoTracking()
            .Where(i => i.ScheduledDeletionAt != null && i.ScheduledDeletionAt <= now)
            .OrderBy(i => i.ScheduledDeletionAt)
            .ThenBy(i => i.Id)
            .Select(i => new DueRefundIbanDeletion(i.Id, i.OrderId, i.ScheduledDeletionAt!.Value))
            .ToListAsync(ct);
    }

    public async Task<RefundIbanUse?> FindRefundIbanUseAsync(Guid refundIbanId, CancellationToken ct)
    {
        RefundIban? iban = await _db.RefundIbans.AsNoTracking().SingleOrDefaultAsync(i => i.Id == refundIbanId, ct);
        if (iban is null)
        {
            return null;
        }

        // The same users the refund line deletes by (D-910): a withdrawal after the delivery waits for its goods - no receipt, no subject (Z-38's case) - while one
        // before the delivery and a service withdrawal owe their refund from the declaration; a request fulfilled with the IBAN brings its refund or its item's records.
        RefundIbanUses uses = await LoadRefundIbanUsesAsync(iban.OrderId, [iban.Id], ct);
        IReadOnlySet<Guid> awaitingGoods = await WithdrawalsAwaitingGoodsAsync(uses.Withdrawals, ct);
        return new RefundIbanUse(iban.Id, iban.OrderId, iban.ScheduledDeletionAt, uses.SubjectsOf(iban.Id, awaitingGoods));
    }

    public async Task ApplyRefundIbanDeletionDayAsync(Guid refundIbanId, bool deleteIban, CancellationToken ct)
    {
        RefundIban? iban = await _db.RefundIbans.SingleOrDefaultAsync(i => i.Id == refundIbanId, ct);
        if (iban is null)
        {
            return;
        }

        if (_db.Database.CurrentTransaction is null || !LockSequence.LockedOrders(_db).Contains(iban.OrderId))
        {
            throw new InvalidOperationException("The refund IBAN's deletion day runs inside the caller's transaction, under the order's row lock (05 7.3.2; 09 6 item 3).");
        }

        if (!deleteIban)
        {
            iban.ScheduledDeletionAt = null;
            return;
        }

        _db.RefundIbans.Remove(iban);
        _db.DisposalRecords.Add(DisposalRecord.Counted(DisposalRecordType.RefundIban, 1, _time.GetUtcNow()));
    }
}
