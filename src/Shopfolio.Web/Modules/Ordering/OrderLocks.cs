using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;

namespace Shopfolio.Modules.Ordering;

/// <summary>
/// The one place of the order row lock (09 9.1 item 5; 05 7.3.2, MK-21): <c>SELECT ... FOR UPDATE</c> over the given orders, by id, held for the
/// rest of the transaction. Every order operation takes it first - the administrator's, the customer's and the system's - and loads the order
/// after it (an order loaded before the lock may be stale); the administrator's operation then compares the version it saw, the customer's and
/// the system's compare the transition's condition with the order as it is now (09 10.2 item 3). Several orders are locked in one statement in
/// id order (05 7.3.5); <see cref="LockSequence"/> refuses an order lock asked after a variant or coupon lock or after a higher order id. The lock
/// never waits on an outside call: the transaction that holds it calls no outside service (05 3.6.3). The internal note takes the shared form of the same
/// row lock (<see cref="LockSharedAsync"/>; 09 10.9).
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

    /// <summary>
    /// The shared row lock of the internal note's write (09 10.9 items 1-2; 06 4.20; K-1095): <c>SELECT contact_email ... FOR SHARE</c> over one order, held
    /// for the rest of the transaction, and the order's contact e-mail (null when there is no such order; null or empty when its personal data is disposed).
    /// It raises no version (06 2.6.4) and conflicts with the row lock of <see cref="LockAsync"/> - the disposal's and every administrator operation's: the
    /// note waits for a disposal that locked first and sees the emptied order; a disposal waits for a note that locked first. Two note writes do not wait for
    /// each other (shared locks agree); the note's own condition tells them apart. It is the transaction's first statement.
    /// </summary>
    public async Task<string?> LockSharedAsync(Guid orderId, CancellationToken ct)
    {
        if (_db.Database.CurrentTransaction is null)
        {
            throw new InvalidOperationException("The order row lock is taken only inside a transaction (09 10.9 item 2).");
        }

        LockSequence.Enter(_db, LockKind.Order, orderId);
        List<string?> rows = await _db.Database
            .SqlQuery<string?>($"SELECT contact_email AS \"Value\" FROM orders WHERE id = {orderId} FOR SHARE")
            .ToListAsync(ct);
        return rows.Count == 0 ? null : rows[0];
    }
}
