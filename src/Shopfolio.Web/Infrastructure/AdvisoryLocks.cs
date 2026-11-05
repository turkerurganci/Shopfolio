using Microsoft.EntityFrameworkCore;

using Npgsql;

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

    /// <summary>
    /// The single-run lock of a background worker (09 13.1 item 7; 05 5.1.3): <c>pg_try_advisory_lock</c>, session scoped, on a connection of its
    /// own that the holder keeps for as long as it works. A worker waits outside any transaction, so the transaction scoped lock would fall at
    /// once. Returns the holder, or <c>null</c> when another instance holds the lock. The key is the same 64-bit hash of the name as above. The
    /// lock falls when the holder is disposed, and when the connection breaks (the process died).
    /// </summary>
    public static async Task<SessionAdvisoryLock?> TryHoldAsync(NpgsqlDataSource dataSource, string workName, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(dataSource);
        ArgumentException.ThrowIfNullOrWhiteSpace(workName);
        NpgsqlConnection connection = await dataSource.OpenConnectionAsync(ct);
        bool handedOver = false;
        try
        {
            await using NpgsqlCommand command = new("SELECT pg_try_advisory_lock(hashtextextended($1, 0))", connection);
            command.Parameters.Add(new NpgsqlParameter { Value = workName });
            bool held = (bool)(await command.ExecuteScalarAsync(ct))!;
            if (!held)
            {
                return null;
            }

            handedOver = true;
            return new SessionAdvisoryLock(connection, workName);
        }
        finally
        {
            if (!handedOver)
            {
                await connection.DisposeAsync();
            }
        }
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

/// <summary>
/// The holder of a session scoped single-run lock (09 13.1 item 7). It owns the connection the lock lives on; no transaction is ever opened on
/// it, so the idle-in-transaction time does not cut it. <see cref="EnsureAliveAsync"/> is asked once per loop round: a broken connection means
/// the lock is gone, and the exception ends the service (the process stops and the container restarts; 05 4.3.2). Disposing releases the lock
/// by hand first, because the pool keeps the physical connection and would keep the lock with it.
/// </summary>
internal sealed class SessionAdvisoryLock : IAsyncDisposable
{
    private readonly NpgsqlConnection _connection;
    private readonly string _workName;

    internal SessionAdvisoryLock(NpgsqlConnection connection, string workName)
    {
        _connection = connection;
        _workName = workName;
    }

    /// <summary>Sends <c>SELECT 1</c> on the lock's connection; a failure is thrown, not hidden.</summary>
    public async Task EnsureAliveAsync(CancellationToken ct)
    {
        await using NpgsqlCommand command = new("SELECT 1", _connection);
        await command.ExecuteScalarAsync(ct);
    }

    public async ValueTask DisposeAsync()
    {
        try
        {
            await using NpgsqlCommand command = new("SELECT pg_advisory_unlock(hashtextextended($1, 0))", _connection);
            command.Parameters.Add(new NpgsqlParameter { Value = _workName });
            await command.ExecuteScalarAsync(CancellationToken.None);
        }
        catch (NpgsqlException)
        {
            // A connection that broke has already lost the lock with its session: there is nothing left to release, and disposing must not throw.
        }
        finally
        {
            await _connection.DisposeAsync();
        }
    }
}
