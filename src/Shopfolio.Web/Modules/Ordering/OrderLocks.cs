using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;

namespace Shopfolio.Modules.Ordering;

/// <summary>
/// The one place of the order row lock (09 9.1 item 5; 05 7.3.2, MK-21): <c>SELECT ... FOR UPDATE</c> over the given orders, by id, held for the
/// rest of the transaction. Every order operation takes it first - the administrator's, the customer's and the system's - and loads the order
/// after it (an order loaded before the lock may be stale); the administrator's operation then compares the version it saw, the customer's and
/// the system's compare the transition's condition with the order as it is now (09 10.2 item 3). Several orders are locked in one statement in
/// id order (05 7.3.5); <see cref="LockSequence"/> refuses an order lock asked after a variant or coupon lock or after a higher order id. The lock
/// never waits on an outside call: the transaction that holds it calls no outside service (05 3.6.3).
/// </summary>
internal sealed class OrderLocks
{
    private readonly AppDbContext _db;

    public OrderLocks(AppDbContext db)
    {
        _db = db;
    }

    public async Task LockAsync(IReadOnlyCollection<Guid> orderIds, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(orderIds);
        if (_db.Database.CurrentTransaction is null)
        {
            throw new InvalidOperationException("The order row lock is taken only inside a transaction (09 9.1 item 5).");
        }

        Guid[] ids = [.. orderIds.Distinct().Order(Comparer<Guid>.Create(LockSequence.CompareKeys))];
        if (ids.Length == 0)
        {
            return;
        }

        foreach (Guid id in ids)
        {
            LockSequence.Enter(_db, LockKind.Order, id);
        }

        await _db.Database
            .SqlQuery<Guid>($"SELECT id AS \"Value\" FROM orders WHERE id = ANY({ids}) ORDER BY id FOR UPDATE")
            .ToListAsync(ct);
    }
}
