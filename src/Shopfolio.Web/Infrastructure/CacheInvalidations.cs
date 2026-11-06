namespace Shopfolio.Infrastructure;

/// <summary>
/// The cache areas the open transaction makes stale (09 10.4 item 3; 05 7.10.1): a transaction that writes a record an area is built from calls
/// <see cref="Add"/> in its body - a save, a raw statement and <c>ExecuteUpdate</c> alike, since there is no hook on <c>SaveChanges</c>. The call
/// only remembers the area. The transaction runner drops the remembered areas from the cache after the commit and forgets them on a rollback or
/// an exception, so a refused or failed save leaves the cache as it was. Scoped: one scope is one unit of work.
/// </summary>
internal sealed class CacheInvalidations
{
    private readonly AppDbContext _db;
    private readonly HashSet<CacheArea> _pending = [];

    public CacheInvalidations(AppDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Remembers that <paramref name="area"/> is dropped once the open transaction commits. Called only inside the transaction that writes
    /// the record: outside one there is no commit to wait for, so the call is refused.
    /// </summary>
    public void Add(CacheArea area)
    {
        if (_db.Database.CurrentTransaction is null)
        {
            throw new InvalidOperationException(
                "A cache area is dropped after the commit of the transaction that writes its record; there is no open transaction (09 10.4 item 3).");
        }

        if (!Enum.IsDefined(area))
        {
            throw new ArgumentOutOfRangeException(nameof(area), area, "Unknown cache area.");
        }

        _pending.Add(area);
    }

    /// <summary>The remembered areas, forgotten as they are handed over; only the transaction runner calls it (09 10.4 item 3).</summary>
    public IReadOnlyList<CacheArea> TakePendingAreas()
    {
        if (_pending.Count == 0)
        {
            return [];
        }

        CacheArea[] areas = [.. _pending];
        _pending.Clear();
        return areas;
    }
}
