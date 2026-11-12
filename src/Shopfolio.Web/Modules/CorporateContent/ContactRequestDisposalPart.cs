using System.Linq.Expressions;

using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Disposal;
using Shopfolio.Infrastructure.Values;
using Shopfolio.Modules.CorporateContent.Contracts;

namespace Shopfolio.Modules.CorporateContent;

/// <summary>
/// The contact request's retention rule in one place (T532; 06 9.3.1, 4.57; Z-33; 07 UC-93): the nightly disposal deletes the requests this rule calls expired, and the panel's list, count, detail, close
/// and reopening see only the requests it calls kept - a request past its period is gone for the panel before the night's run reaches it, and the "açık talep" counter and the "Açık" list agree.
/// The period starts at <see cref="ContactRequest.RetentionStart"/>; the "about an order" topic keeps <see cref="ProductConstants.OrderContactRequestRetentionYears"/> years, every other topic
/// <see cref="ProductConstants.ContactRequestRetentionYears"/>; years are counted in Turkey time.
/// </summary>
internal static class ContactRequestRetention
{
    /// <summary>The edge for the "about an order" topic: a request whose retention start is before it is past its period.</summary>
    public static DateTimeOffset OrderBefore(DateTimeOffset now) => RetentionCutoff.YearsBefore(now, ProductConstants.OrderContactRequestRetentionYears);

    /// <summary>The edge for every other topic.</summary>
    public static DateTimeOffset OtherBefore(DateTimeOffset now) => RetentionCutoff.YearsBefore(now, ProductConstants.ContactRequestRetentionYears);

    /// <summary>The requests of one topic class (the "about an order" topic or the others) whose retention start is before <paramref name="before"/>: the rows the disposal deletes.</summary>
    public static Expression<Func<ContactRequest, bool>> Expired(bool aboutOrder, DateTimeOffset before) =>
        c => (c.ContactTopic == ContactTopic.AboutOrder) == aboutOrder && (c.LastClosedAt ?? c.CreatedAt) < before;

    /// <summary>The requests still within their period at <paramref name="now"/>: the complement of <see cref="Expired"/> for both topic classes (the edge itself is kept).</summary>
    public static Expression<Func<ContactRequest, bool>> Kept(DateTimeOffset now)
    {
        DateTimeOffset orderBefore = OrderBefore(now);
        DateTimeOffset otherBefore = OtherBefore(now);
        return c => c.ContactTopic == ContactTopic.AboutOrder
            ? (c.LastClosedAt ?? c.CreatedAt) >= orderBefore
            : (c.LastClosedAt ?? c.CreatedAt) >= otherBefore;
    }
}

/// <summary>
/// The contact request's part of the nightly disposal (06 9.3.1, 4.57; Z-33; K-951). A request is deleted when its retention start - the last closing, or the
/// creation moment of a request never closed (<see cref="ContactRequest.RetentionStart"/>) - is older than its period: <see cref="ProductConstants.OrderContactRequestRetentionYears"/>
/// years for the "about an order" topic (the legal period), <see cref="ProductConstants.ContactRequestRetentionYears"/> years for every other topic (P-40). The
/// period does not stop while a request is open: an open or reopened request past its period is deleted too. Years are counted in Turkey time. Each topic's
/// rows go in chunks, with one disposal row of type <see cref="DisposalRecordType.ContactRequest"/> for a chunk that deleted something.
/// </summary>
internal sealed class ContactRequestDisposalPart : IDisposalPart
{
    public string Name => "ContactRequests";

    public async Task RunAsync(DisposalRun run, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(run);
        DateTimeOffset now = run.Now;
        DateTimeOffset orderBefore = ContactRequestRetention.OrderBefore(now);
        DateTimeOffset otherBefore = ContactRequestRetention.OtherBefore(now);
        await run.ChunksAsync(DisposalRecordType.ContactRequest, (db, max, cancel) => DeleteChunkAsync(db, true, orderBefore, max, cancel), ct);
        await run.ChunksAsync(DisposalRecordType.ContactRequest, (db, max, cancel) => DeleteChunkAsync(db, false, otherBefore, max, cancel), ct);
    }

    private static async Task<int> DeleteChunkAsync(AppDbContext db, bool aboutOrder, DateTimeOffset before, int max, CancellationToken ct)
    {
        db.ChangeTracker.Clear();
        List<Guid> ids = await db.ContactRequests
            .Where(ContactRequestRetention.Expired(aboutOrder, before))
            .OrderBy(c => c.CreatedAt)
            .ThenBy(c => c.Id)
            .Select(c => c.Id)
            .Take(max)
            .ToListAsync(ct);
        return await db.ContactRequests.Where(c => ids.Contains(c.Id)).ExecuteDeleteAsync(ct);
    }
}
