using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Disposal;
using Shopfolio.Infrastructure.Values;
using Shopfolio.Modules.CorporateContent.Contracts;

namespace Shopfolio.Modules.CorporateContent;

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
        DateTimeOffset orderBefore = RetentionCutoff.YearsBefore(now, ProductConstants.OrderContactRequestRetentionYears);
        DateTimeOffset otherBefore = RetentionCutoff.YearsBefore(now, ProductConstants.ContactRequestRetentionYears);
        await run.ChunksAsync(DisposalRecordType.ContactRequest, (db, max, cancel) => DeleteChunkAsync(db, true, orderBefore, max, cancel), ct);
        await run.ChunksAsync(DisposalRecordType.ContactRequest, (db, max, cancel) => DeleteChunkAsync(db, false, otherBefore, max, cancel), ct);
    }

    private static async Task<int> DeleteChunkAsync(AppDbContext db, bool aboutOrder, DateTimeOffset before, int max, CancellationToken ct)
    {
        db.ChangeTracker.Clear();
        List<Guid> ids = await db.ContactRequests
            .Where(c => (c.ContactTopic == ContactTopic.AboutOrder) == aboutOrder && (c.LastClosedAt ?? c.CreatedAt) < before)
            .OrderBy(c => c.CreatedAt)
            .ThenBy(c => c.Id)
            .Select(c => c.Id)
            .Take(max)
            .ToListAsync(ct);
        return await db.ContactRequests.Where(c => ids.Contains(c.Id)).ExecuteDeleteAsync(ct);
    }
}
