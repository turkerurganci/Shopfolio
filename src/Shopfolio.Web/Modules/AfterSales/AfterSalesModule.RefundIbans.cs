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

        List<Guid> subjects = [];
        subjects.AddRange(await _db.Cancellations.AsNoTracking()
            .Where(c => c.RefundIbanId == refundIbanId)
            .Select(c => c.Id)
            .ToListAsync(ct));
        subjects.AddRange(await _db.LateDeliveryTerminations.AsNoTracking()
            .Where(t => t.RefundIbanId == refundIbanId)
            .Select(t => t.Id)
            .ToListAsync(ct));

        // A withdrawal's refund is born with its goods' receipt: a withdrawal whose goods did not arrive has no subject (Z-38's case).
        subjects.AddRange(await _db.Returns.AsNoTracking()
            .Where(r => r.WithdrawalId != null && _db.Withdrawals.Any(w => w.Id == r.WithdrawalId && w.RefundIbanId == refundIbanId))
            .Select(r => r.Id)
            .ToListAsync(ct));
        return new RefundIbanUse(iban.Id, iban.OrderId, iban.ScheduledDeletionAt, subjects);
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
