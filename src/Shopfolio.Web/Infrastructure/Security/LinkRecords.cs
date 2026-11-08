using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure.Values;

namespace Shopfolio.Infrastructure.Security;

/// <summary>
/// What a read of a link found (05 6.4.6): no record, or a record and its state. "No record" is not a state (06 5.4.3), so the five outcomes
/// are a missing record and the four <see cref="LinkState"/> values.
/// </summary>
internal sealed record LinkReading(LinkRecord? Record, LinkState? State)
{
    /// <summary>No record carries the digest of the arriving key.</summary>
    public static LinkReading Missing { get; } = new(null, null);

    public bool Exists => Record is not null;
}

/// <summary>
/// The reading and the consuming of link records (09 15.1 items 3 and 4; 05 6.4.6, 6.4.7; 07 2.5.3). <see cref="ReadAsync"/> is what a link's
/// GET handler calls: it only reads (no tracking, no write), so opening a link changes no record (MK-51). <see cref="TryConsumeAsync"/> is what
/// the button's POST handler calls, in the caller's own transaction: one conditional update that takes only a link that is not used, not
/// superseded and not expired, so of two requests at once exactly one succeeds. A source scan keeps the consuming out of every handler that is
/// not an <c>OnPost…</c> handler.
/// </summary>
internal sealed class LinkRecords
{
    private readonly AppDbContext _db;
    private readonly TimeProvider _time;

    public LinkRecords(AppDbContext db, TimeProvider time)
    {
        ArgumentNullException.ThrowIfNull(db);
        ArgumentNullException.ThrowIfNull(time);
        _db = db;
        _time = time;
    }

    /// <summary>
    /// The state of a record at <paramref name="now"/> (05 6.4.6): used first (the link did its work), then superseded (a newer link was asked
    /// for), then expired (the lifetime ended; a link whose lifetime was drawn to "now" is expired), else valid. A lifetime ends at its moment:
    /// a link is valid strictly before it.
    /// </summary>
    public static LinkState StateOf(LinkRecord record, DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(record);
        if (record.UsedAt is not null)
        {
            return LinkState.Used;
        }

        if (record.SupersededAt is not null)
        {
            return LinkState.Superseded;
        }

        return record.ExpiresAt <= now ? LinkState.Expired : LinkState.Valid;
    }

    /// <summary>Reads the link of an arriving key: the key is digested and the digest is searched; the secret is not read. Writes nothing.</summary>
    public async Task<LinkReading> ReadAsync(string key, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(key);
        if (key.Length == 0)
        {
            return LinkReading.Missing;
        }

        string digest = SecretKey.Hash(key);
        LinkRecord? record = await _db.LinkRecords.AsNoTracking().SingleOrDefaultAsync(l => l.TokenHash == digest, ct);
        return record is null ? LinkReading.Missing : new LinkReading(record, StateOf(record, _time.GetUtcNow()));
    }

    /// <summary>
    /// Consumes the link with one conditional update (05 6.4.6; 07 2.5.3): the row is taken only while it is unused, not superseded and not
    /// expired. False means no row changed - the caller does nothing and shows the link's present state. Runs in the caller's transaction.
    /// </summary>
    public async Task<bool> TryConsumeAsync(Guid linkId, CancellationToken ct)
    {
        DateTimeOffset now = _time.GetUtcNow();
        int changed = await _db.LinkRecords
            .Where(l => l.Id == linkId && l.UsedAt == null && l.SupersededAt == null && l.ExpiresAt > now)
            .ExecuteUpdateAsync(set => set.SetProperty(l => l.UsedAt, now), ct);
        return changed == 1;
    }

    /// <summary>
    /// Supersedes the earlier unused links of the same type and owner (05 6.4.6: a link asked for again makes the earlier ones invalid), with the
    /// same kind of conditional update: only links that are unused, not yet superseded and not expired are taken. Returns the number taken.
    /// </summary>
    public async Task<int> SupersedeEarlierAsync(LinkType linkType, Guid? customerId, Guid? adminId, Guid exceptLinkId, CancellationToken ct)
    {
        // An empty owner would match every invitation link of the type (D-96): the owner is a customer or an administrator, exactly one. The invitation's own call arrives with T379.
        if ((customerId is null) == (adminId is null))
        {
            throw new ArgumentException("A superseding call names exactly one owner, a customer or an administrator (05 6.4.6; D-96).", nameof(customerId));
        }

        DateTimeOffset now = _time.GetUtcNow();
        return await _db.LinkRecords
            .Where(l => l.LinkType == linkType && l.CustomerId == customerId && l.AdminId == adminId && l.Id != exceptLinkId
                && l.UsedAt == null && l.SupersededAt == null && l.ExpiresAt > now)
            .ExecuteUpdateAsync(set => set.SetProperty(l => l.SupersededAt, now), ct);
    }
}
