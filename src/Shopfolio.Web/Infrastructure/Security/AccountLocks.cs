using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure.Values;

namespace Shopfolio.Infrastructure.Security;

/// <summary>
/// The one place of the account row lock (09 9.1 item 5; 09 15.1 item 3; 05 6.1.2): <c>SELECT ... FOR UPDATE</c> over one customer or administrator row, held for the rest of the
/// transaction, the same raw SQL pattern as the order and digital file locks. The sign-in takes it before it reads the digest and keeps it until the session row is written; the password
/// reset, the change and the digest renewal take it before the digest is written. So a sign-in checked against the old digest and a reset or change are one after the other: a sign-in
/// that came first has its session committed before the reset deletes the account's sessions, and a sign-in that came second sees the new digest. The lock is its own family
/// (<see cref="LockKind.Account"/>); it is asked again freely inside the same transaction, and the attempt counters (own transactions, K-972) never wait on it.
/// </summary>
internal static class AccountLocks
{
    /// <summary>Locks the account's row; false when no such account exists. The lock is taken once per transaction.</summary>
    public static async Task<bool> LockAsync(AppDbContext db, AccountType type, Guid accountId, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(db);
        if (db.Database.CurrentTransaction is null)
        {
            throw new InvalidOperationException("The account row lock is taken only inside a transaction (09 9.1 item 5).");
        }

        if (LockSequence.Holds(db, LockKind.Account, accountId))
        {
            return true;
        }

        LockSequence.Enter(db, LockKind.Account, accountId);
        List<Guid> locked = type == AccountType.Admin
            ? await db.Database.SqlQuery<Guid>($"SELECT id AS \"Value\" FROM admins WHERE id = {accountId} FOR UPDATE").ToListAsync(ct)
            : await db.Database.SqlQuery<Guid>($"SELECT id AS \"Value\" FROM customers WHERE id = {accountId} FOR UPDATE").ToListAsync(ct);
        return locked.Count == 1;
    }

    /// <summary>
    /// Finds the account by its normalized e-mail, locks its row and returns its id; the caller reads the row after this call, never before (a digest read before the lock may be stale).
    /// Null when there is no such account. The row is looked up by e-mail without a lock first, then locked by id, and looked up again by e-mail under the lock so an address that
    /// moved in between is not matched.
    /// </summary>
    public static async Task<Guid?> LockByEmailAsync(AppDbContext db, AccountType type, string normalizedEmail, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(db);
        ArgumentNullException.ThrowIfNull(normalizedEmail);
        List<Guid> ids = type == AccountType.Admin
            ? await db.Database.SqlQuery<Guid>($"SELECT id AS \"Value\" FROM admins WHERE normalized_email = {normalizedEmail}").ToListAsync(ct)
            : await db.Database.SqlQuery<Guid>($"SELECT id AS \"Value\" FROM customers WHERE normalized_email = {normalizedEmail}").ToListAsync(ct);
        if (ids.Count != 1 || !await LockAsync(db, type, ids[0], ct))
        {
            return null;
        }

        List<Guid> still = type == AccountType.Admin
            ? await db.Database.SqlQuery<Guid>($"SELECT id AS \"Value\" FROM admins WHERE id = {ids[0]} AND normalized_email = {normalizedEmail}").ToListAsync(ct)
            : await db.Database.SqlQuery<Guid>($"SELECT id AS \"Value\" FROM customers WHERE id = {ids[0]} AND normalized_email = {normalizedEmail}").ToListAsync(ct);
        return still.Count == 1 ? ids[0] : null;
    }
}
