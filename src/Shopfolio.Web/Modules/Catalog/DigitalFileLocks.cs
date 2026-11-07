using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;

namespace Shopfolio.Modules.Catalog;

/// <summary>
/// The one place of the digital file row lock (09 9.1 item 5; 05 7.7.3): <c>SELECT ... FOR UPDATE</c> over one file row, held for the rest of the transaction -
/// the same raw SQL pattern as <c>OrderLocks</c>. The chunk request takes it first and reads everything it decides on after it, so two chunks of the same upload
/// are served one after the other. The lock is its own family (<see cref="LockKind.DigitalFile"/>): the transaction that holds it takes no order, variant or
/// coupon lock, and it never waits on an outside call (05 3.6.3) - only on the file store, which is the disk.
/// </summary>
internal sealed class DigitalFileLocks
{
    private readonly AppDbContext _db;

    public DigitalFileLocks(AppDbContext db)
    {
        _db = db;
    }

    /// <summary>Locks the row and returns it tracked, read after the lock; null when there is no such row (the sweep removed it or the id never existed).</summary>
    public async Task<DigitalFile?> LockForUpdateAsync(Guid id, CancellationToken ct)
    {
        if (_db.Database.CurrentTransaction is null)
        {
            throw new InvalidOperationException("The digital file row lock is taken only inside a transaction (09 9.1 item 5).");
        }

        LockSequence.Enter(_db, LockKind.DigitalFile, id);
        List<Guid> locked = await _db.Database
            .SqlQuery<Guid>($"SELECT id AS \"Value\" FROM digital_files WHERE id = {id} FOR UPDATE")
            .ToListAsync(ct);
        if (locked.Count == 0)
        {
            return null;
        }

        return await _db.DigitalFiles.SingleAsync(f => f.Id == id, ct);
    }
}
