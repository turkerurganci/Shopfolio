using Microsoft.EntityFrameworkCore;

namespace Shopfolio.Infrastructure;

/// <summary>
/// The one place of the advisory lock (09 10.2 item 5): <c>pg_advisory_xact_lock</c>, transaction scoped - it falls when the transaction ends
/// and is never released by hand. The key is the 64-bit hash (<c>hashtextextended</c>) of the scope's name and, for the cart, the id; two keys
/// that collide only queue once more and give no wrong result. The lock is the first statement of its transaction: the cart's lock heads the
/// order side (05 4.2.2, 7.3.5) and the category tree's and the administrator set's locks are taken alone - <see cref="LockSequence"/> refuses
/// any other order before the statement is sent. The wait is bounded by the session's <c>lock_timeout</c> (09 3.5, 10.7).
/// </summary>
internal sealed class AdvisoryLocks
{
    private readonly AppDbContext _db;

    public AdvisoryLocks(AppDbContext db)
    {
        _db = db;
    }

    public async Task LockAsync(AdvisoryLockScope scope, Guid? key, CancellationToken ct)
    {
        if (_db.Database.CurrentTransaction is null)
        {
            throw new InvalidOperationException("An advisory lock is transaction scoped and is taken only inside a transaction (09 10.2 item 5).");
        }

        if ((scope == AdvisoryLockScope.Cart) != key.HasValue)
        {
            throw new ArgumentException($"The {scope} lock takes {(scope == AdvisoryLockScope.Cart ? "the cart's id" : "no key")} (09 10.2 item 5).", nameof(key));
        }

        LockSequence.Enter(_db, KindOf(scope), key);
        string name = key is null ? scope.ToString() : $"{scope}:{key:D}";
        await _db.Database.ExecuteSqlAsync($"SELECT pg_advisory_xact_lock(hashtextextended({name}, 0))", ct);
    }

    private static LockKind KindOf(AdvisoryLockScope scope)
    {
        return scope switch
        {
            AdvisoryLockScope.Cart => LockKind.Cart,
            AdvisoryLockScope.CategoryTree => LockKind.CategoryTree,
            AdvisoryLockScope.AdminSet => LockKind.AdminSet,
            _ => throw new ArgumentOutOfRangeException(nameof(scope)),
        };
    }
}
