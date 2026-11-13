using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;

namespace Shopfolio.Modules.Catalog;

/// <summary>
/// The one place of the coupon row lock taken by a panel change (05 7.3.5; 02 3.10.6; K-654): <c>SELECT ... FOR UPDATE</c> over one coupon row, held for the rest of the
/// transaction - the same raw SQL pattern as <c>OrderLocks</c> and <c>DigitalFileLocks</c>. The coupon change takes it first and reads the used and reserved rights after it,
/// so an order that reserves a right in between waits and the total usage count never goes under the rights (the counters' own writes in <see cref="StockCounters"/> lock
/// the same row by their conditional update). The lock is reported to <see cref="LockSequence"/> as <see cref="LockKind.Coupon"/>, the last of the order.
/// </summary>
internal sealed class CouponLocks
{
    private readonly AppDbContext _db;

    public CouponLocks(AppDbContext db)
    {
        _db = db;
    }

    /// <summary>Locks the row and returns it tracked, read after the lock; null when no coupon has <paramref name="id"/>.</summary>
    public async Task<Coupon?> LockForUpdateAsync(Guid id, CancellationToken ct)
    {
        if (_db.Database.CurrentTransaction is null)
        {
            throw new InvalidOperationException("The coupon row lock is taken only inside a transaction (09 9.1 item 5).");
        }

        LockSequence.Enter(_db, LockKind.Coupon, id);
        List<Guid> locked = await _db.Database
            .SqlQuery<Guid>($"SELECT id AS \"Value\" FROM coupons WHERE id = {id} FOR UPDATE")
            .ToListAsync(ct);
        if (locked.Count == 0)
        {
            return null;
        }

        return await _db.Coupons.SingleAsync(c => c.Id == id, ct);
    }
}
