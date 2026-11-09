using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;

namespace Shopfolio.Modules.CorporateContent;

/// <summary>The way a record is moved in the firm's order: one place up or one place down (07 9.13.5).</summary>
internal enum MoveDirection
{
    Up,
    Down,
}

/// <summary>What became of a move in the manual order (T359).</summary>
internal enum OrderingOutcome
{
    /// <summary>The two records changed places.</summary>
    Moved,

    /// <summary>The order of the two records is no longer the one the screen showed; nothing was written and the list is shown as it is (409).</summary>
    OrderChanged,

    /// <summary>The kind has no manual order: the about page and the announcement are one record each (422).</summary>
    NotOrderable,

    /// <summary>The record is first and is moved up, or last and is moved down (422).</summary>
    AtEdge,

    NotFound,
}

/// <summary>The result of a move; a refusal is a return value and the transaction runner rolls back (09 9.1 item 3).</summary>
internal sealed record OrderingResult(OrderingOutcome Outcome) : IOperationResult
{
    public bool IsRejected => Outcome != OrderingOutcome.Moved;
}

/// <summary>
/// The manual order of the five lists the firm arranges (T359; 02 3.27.11; 07 9.13.5; 06 4.47-4.51; K-1063, K-1215 (3)): service introductions, reference works, questions, branches and general
/// pages. "Up" and "down" write the places of two records in one transaction, and only while both still carry the order the screen showed: the write of each is conditional on the value it
/// replaces, so two moves that arrive together cannot both be written. When the two values are equal (two records created together) the move renumbers the whole list one by one and then swaps
/// the two, so the order that was shown - by the order, the time of creation and the key - is the order that is kept. The record's version is not raised: the order is not a field of its form
/// (06 2.6.5), and nothing is written to the audit trail (02 10.3.1). The storefront's list pages, the home page blocks and the menu read the same column, so a move drops the menu and the
/// home page areas of the cache. A product list has no manual order (02 3.5.3) and is not touched. The move joins the caller's transaction and never saves.
/// </summary>
internal sealed class ContentOrdering
{
    private readonly AppDbContext _db;
    private readonly CacheInvalidations _invalidations;

    public ContentOrdering(AppDbContext db, CacheInvalidations invalidations)
    {
        ArgumentNullException.ThrowIfNull(db);
        ArgumentNullException.ThrowIfNull(invalidations);
        _db = db;
        _invalidations = invalidations;
    }

    /// <summary>The kinds that have a manual order (02 3.27.11).</summary>
    public static IReadOnlyList<ContentRecordKind> Orderable { get; } =
        [ContentRecordKind.Offering, ContentRecordKind.PortfolioItem, ContentRecordKind.FaqItem, ContentRecordKind.Branch, ContentRecordKind.CustomPage];

    /// <summary>
    /// Moves the record one place. <paramref name="shownOrder"/> and <paramref name="shownNeighbourOrder"/> are the orders of the record and of the record it changes places with, as the screen
    /// showed them (07 2.4.3).
    /// </summary>
    public async Task<OrderingResult> MoveAsync(ContentRecordKind kind, Guid id, MoveDirection direction, int shownOrder, int shownNeighbourOrder, CancellationToken ct)
    {
        if (!Orderable.Contains(kind))
        {
            return new OrderingResult(OrderingOutcome.NotOrderable);
        }

        List<OrderRow> rows = await ReadAsync(kind, ct);
        int at = rows.FindIndex(r => r.Id == id);
        if (at < 0)
        {
            return new OrderingResult(OrderingOutcome.NotFound);
        }

        int neighbour = direction == MoveDirection.Up ? at - 1 : at + 1;
        if (neighbour < 0 || neighbour >= rows.Count)
        {
            return new OrderingResult(OrderingOutcome.AtEdge);
        }

        if (rows[at].SortOrder != shownOrder || rows[neighbour].SortOrder != shownNeighbourOrder)
        {
            return new OrderingResult(OrderingOutcome.OrderChanged);
        }

        // The places each record will hold: a swap of the two values, or - when they are equal and the swap would change nothing - a renumbering of the whole list with the two swapped.
        Dictionary<Guid, int> target = [];
        if (rows[at].SortOrder == rows[neighbour].SortOrder)
        {
            List<OrderRow> arranged = [.. rows];
            (arranged[at], arranged[neighbour]) = (arranged[neighbour], arranged[at]);
            for (int i = 0; i < arranged.Count; i++)
            {
                target[arranged[i].Id] = i + 1;
            }
        }
        else
        {
            target[rows[at].Id] = rows[neighbour].SortOrder;
            target[rows[neighbour].Id] = rows[at].SortOrder;
        }

        foreach (OrderRow row in rows.Where(r => target.TryGetValue(r.Id, out int place) && place != r.SortOrder))
        {
            // Conditional on the value this call read: a record another move has changed since affects no row, and the whole move is refused.
            if (await WriteAsync(kind, row.Id, row.SortOrder, target[row.Id], ct) != 1)
            {
                return new OrderingResult(OrderingOutcome.OrderChanged);
            }
        }

        _invalidations.Add(CacheArea.Menu);
        _invalidations.Add(CacheArea.HomepageBlocks);
        return new OrderingResult(OrderingOutcome.Moved);
    }

    /// <summary>The list in the order the firm's lists show: the order, the time of creation, the key (09 10.5 item 1).</summary>
    private async Task<List<OrderRow>> ReadAsync(ContentRecordKind kind, CancellationToken ct)
    {
        return kind switch
        {
            ContentRecordKind.Offering => await _db.Offerings.AsNoTracking()
                .OrderBy(r => r.SortOrder).ThenBy(r => r.CreatedAt).ThenBy(r => r.Id).Select(r => new OrderRow(r.Id, r.SortOrder)).ToListAsync(ct),
            ContentRecordKind.PortfolioItem => await _db.PortfolioItems.AsNoTracking()
                .OrderBy(r => r.SortOrder).ThenBy(r => r.CreatedAt).ThenBy(r => r.Id).Select(r => new OrderRow(r.Id, r.SortOrder)).ToListAsync(ct),
            ContentRecordKind.FaqItem => await _db.FaqItems.AsNoTracking()
                .OrderBy(r => r.SortOrder).ThenBy(r => r.CreatedAt).ThenBy(r => r.Id).Select(r => new OrderRow(r.Id, r.SortOrder)).ToListAsync(ct),
            ContentRecordKind.Branch => await _db.Branches.AsNoTracking()
                .OrderBy(r => r.SortOrder).ThenBy(r => r.CreatedAt).ThenBy(r => r.Id).Select(r => new OrderRow(r.Id, r.SortOrder)).ToListAsync(ct),
            ContentRecordKind.CustomPage => await _db.CustomPages.AsNoTracking()
                .OrderBy(r => r.SortOrder).ThenBy(r => r.CreatedAt).ThenBy(r => r.Id).Select(r => new OrderRow(r.Id, r.SortOrder)).ToListAsync(ct),
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "The kind has no manual order."),
        };
    }

    /// <summary>The one conditional write: the place is set only where the record still holds <paramref name="expected"/>; the number of rows written is returned.</summary>
    private async Task<int> WriteAsync(ContentRecordKind kind, Guid id, int expected, int place, CancellationToken ct)
    {
        if (_db.Database.CurrentTransaction is null)
        {
            throw new InvalidOperationException("A move in the manual order is written only inside the caller's transaction (09 9.1 item 3).");
        }

        return kind switch
        {
            ContentRecordKind.Offering => await _db.Offerings.Where(r => r.Id == id && r.SortOrder == expected).ExecuteUpdateAsync(s => s.SetProperty(r => r.SortOrder, place), ct),
            ContentRecordKind.PortfolioItem => await _db.PortfolioItems.Where(r => r.Id == id && r.SortOrder == expected).ExecuteUpdateAsync(s => s.SetProperty(r => r.SortOrder, place), ct),
            ContentRecordKind.FaqItem => await _db.FaqItems.Where(r => r.Id == id && r.SortOrder == expected).ExecuteUpdateAsync(s => s.SetProperty(r => r.SortOrder, place), ct),
            ContentRecordKind.Branch => await _db.Branches.Where(r => r.Id == id && r.SortOrder == expected).ExecuteUpdateAsync(s => s.SetProperty(r => r.SortOrder, place), ct),
            ContentRecordKind.CustomPage => await _db.CustomPages.Where(r => r.Id == id && r.SortOrder == expected).ExecuteUpdateAsync(s => s.SetProperty(r => r.SortOrder, place), ct),
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "The kind has no manual order."),
        };
    }

    private sealed record OrderRow(Guid Id, int SortOrder);
}
