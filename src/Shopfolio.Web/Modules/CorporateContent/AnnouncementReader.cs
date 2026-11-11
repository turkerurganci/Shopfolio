using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Modules.CorporateContent.Contracts;

namespace Shopfolio.Modules.CorporateContent;

/// <summary>
/// The read of the announcement strip (T360; 02 3.27.21, 3.27.25): the visibility is decided from the status and the dates on the Turkey day of the time source, by
/// <see cref="ContentVisibility"/> - the one home of the rule - and is never stored. There is no cache: the row is one row, and a date range makes the answer change at midnight with
/// no write to drop a cache for. A plain read; it writes nothing.
/// </summary>
internal sealed class AnnouncementReader
{
    private readonly AppDbContext _db;
    private readonly TimeProvider _time;

    public AnnouncementReader(AppDbContext db, TimeProvider time)
    {
        ArgumentNullException.ThrowIfNull(db);
        ArgumentNullException.ThrowIfNull(time);
        _db = db;
        _time = time;
    }

    /// <summary>The strip <paramref name="audience"/> sees now, or null when there is none (no row, hidden, or a draft without a text to preview).</summary>
    public async Task<AnnouncementView?> ReadAsync(ContentAudience audience, CancellationToken ct)
    {
        Announcement? announcement = await _db.Announcements.AsNoTracking().SingleOrDefaultAsync(ct);
        if (announcement is null || !ContentGate.IsFilled(announcement.Text))
        {
            return null;
        }

        DateOnly today = TurkeyTime.DateOf(_time.GetUtcNow());
        ContentShown shown = ContentVisibility.Decide(announcement, audience, today);
        return shown == ContentShown.Hidden
            ? null
            : new AnnouncementView(announcement.Text!, announcement.LinkUrl, IsDraft: shown == ContentShown.ShownWithDraftBand); // non-null: the guard above returns early when the text is not filled
    }
}
