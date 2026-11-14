using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.DataModel;
using Shopfolio.Infrastructure.Forms;
using Shopfolio.Modules.Administration.Contracts;
using Shopfolio.Modules.Catalog.Contracts;

namespace Shopfolio.Modules.CorporateContent;

/// <summary>What became of a corporate content write (T322).</summary>
internal enum ContentOutcome
{
    /// <summary>The status changed (and the version rose).</summary>
    Changed,

    /// <summary>The record was already in the asked status; nothing was written.</summary>
    Unchanged,

    /// <summary>An edit was saved.</summary>
    Saved,

    /// <summary>The record was deleted.</summary>
    Deleted,

    NotFound,

    /// <summary>The record is no longer at the version the form carried (05 7.3.1).</summary>
    VersionConflict,

    /// <summary>Draft to published refused: the gate has empty fields (<see cref="ContentResult.Missing"/>) - <c>PublishGateIncomplete</c> (07 6.2.6).</summary>
    GateIncomplete,

    /// <summary>The edit of a published record would empty a gate field; it is not saved - <c>PublishedRecordGateBroken</c> (07 6.2.6).</summary>
    PublishedGateBroken,

    /// <summary>The about page is never deleted; it is put back to draft (02 5.2).</summary>
    Forbidden,

    /// <summary>The deletion cannot be undone and the administrator has not confirmed the warning (02 5.2, 7.6.4).</summary>
    ConfirmationRequired,
}

/// <summary>The result of a corporate content write; a refusal is a return value and the transaction runner rolls back (09 9.1 item 3).</summary>
internal sealed record ContentResult(ContentOutcome Outcome, IReadOnlyList<ContentGateField> Missing, int? Version = null) : IOperationResult
{
    public bool IsRejected => Outcome is not (ContentOutcome.Changed or ContentOutcome.Unchanged or ContentOutcome.Saved or ContentOutcome.Deleted);

    public static ContentResult Of(ContentOutcome outcome, int? version = null) => new(outcome, [], version);
}

/// <summary>
/// The publish rules of the corporate content (T322; 02 5.2, 3.27.23; 03 1.8.4): the move from draft to published through the gate, the way back to
/// draft, the save of an edit that may not break the gate of a published record, and the deletion with its warning. There are two statuses and no
/// archive, no later-dated publication and no version history (10 KD-38). Every method joins the caller's transaction and never opens one (09 9.1
/// item 3). A change of the status - the publication and the way back to draft - writes one trail row in the same transaction, who and when from the signed-in
/// administrator (T360; 02 3.27.26, 10.3.1 (2)); an edit of a record's text writes none and neither does the deletion (02 3.27.26, 10.3.1). The first publication of a
/// record that has an address of its own fixes the address here (T363; <see cref="IAddressedContentRecord"/>).
/// </summary>
internal sealed class ContentPublishing
{
    /// <summary>The field name of the status change in the trail row (the same as the catalog's).</summary>
    private const string StatusField = nameof(IContentRecord.PublishStatus);

    private readonly AppDbContext _db;
    private readonly CacheInvalidations _invalidations;
    private readonly IAuditTrail _trail;
    private readonly TimeProvider _time;

    public ContentPublishing(AppDbContext db, CacheInvalidations invalidations, IAuditTrail trail, TimeProvider time)
    {
        ArgumentNullException.ThrowIfNull(db);
        ArgumentNullException.ThrowIfNull(invalidations);
        ArgumentNullException.ThrowIfNull(trail);
        ArgumentNullException.ThrowIfNull(time);
        _db = db;
        _invalidations = invalidations;
        _trail = trail;
        _time = time;
    }

    /// <summary>Draft to published; refused with the empty gate fields when a required field is empty. No second approval exists - any administrator publishes.</summary>
    public Task<ContentResult> PublishAsync(ContentRecordKind kind, Guid id, int expectedVersion, CancellationToken ct) =>
        ChangeAsync(kind, id, expectedVersion, PublishStatus.Published, ct);

    /// <summary>Published to draft: a record taken off the site goes back to draft.</summary>
    public Task<ContentResult> UnpublishAsync(ContentRecordKind kind, Guid id, int expectedVersion, CancellationToken ct) =>
        ChangeAsync(kind, id, expectedVersion, PublishStatus.Draft, ct);

    /// <summary>
    /// Saves the edit of a record (02 5.2): a draft saves whatever it holds; a published record is on the site the moment it is saved, so an edit that
    /// leaves a gate field empty is not saved - the panel shows the missing field and sends the administrator to put the record back to draft first.
    /// <paramref name="apply"/> writes the form's values to the tracked record and does not touch the version.
    /// </summary>
    public async Task<ContentResult> SaveEditAsync<TRecord>(TRecord record, int formVersion, Action<TRecord> apply, CancellationToken ct)
        where TRecord : class, IContentRecord
    {
        ArgumentNullException.ThrowIfNull(record);
        ArgumentNullException.ThrowIfNull(apply);
        if (record.Version != formVersion)
        {
            return ContentResult.Of(ContentOutcome.VersionConflict);
        }

        apply(record);
        IReadOnlyList<ContentGateField> missing = record.MissingGateFields();
        if (record.PublishStatus == PublishStatus.Published && missing.Count > 0)
        {
            return new ContentResult(ContentOutcome.PublishedGateBroken, missing);
        }

        RecordSaveResult saved = await RecordSave.SaveAsync(_db, record, formVersion, _ => { }, ct);
        if (!saved.IsSaved)
        {
            return ContentResult.Of(ContentOutcome.VersionConflict);
        }

        DropStaleAreas();
        return ContentResult.Of(ContentOutcome.Saved, record.Version);
    }

    /// <summary>
    /// The permanent deletion (02 5.2, 3.27.27): the about page is never deleted; any other record is deleted only after the administrator confirmed
    /// the "this cannot be undone" warning. Deletion is not a status.
    /// </summary>
    public async Task<ContentResult> DeleteAsync(ContentRecordKind kind, Guid id, bool irreversibleWarningConfirmed, CancellationToken ct)
    {
        if (kind == ContentRecordKind.AboutPage)
        {
            return ContentResult.Of(ContentOutcome.Forbidden);
        }

        IContentRecord? record = await FindAsync(kind, id, ct);
        if (record is null)
        {
            return ContentResult.Of(ContentOutcome.NotFound);
        }

        if (!irreversibleWarningConfirmed)
        {
            return ContentResult.Of(ContentOutcome.ConfirmationRequired);
        }

        // The deletion is tracked, not saved: the transaction's owner saves it with its commit (09 9.1 item 3; D-673).
        _db.Remove(record);
        DropStaleAreas();
        return ContentResult.Of(ContentOutcome.Deleted);
    }

    private async Task<ContentResult> ChangeAsync(ContentRecordKind kind, Guid id, int expectedVersion, PublishStatus target, CancellationToken ct)
    {
        IContentRecord? record = await FindAsync(kind, id, ct);
        if (record is null)
        {
            return ContentResult.Of(ContentOutcome.NotFound);
        }

        if (record.Version != expectedVersion)
        {
            return ContentResult.Of(ContentOutcome.VersionConflict);
        }

        if (record.PublishStatus == target)
        {
            return ContentResult.Of(ContentOutcome.Unchanged, record.Version);
        }

        if (target == PublishStatus.Published)
        {
            IReadOnlyList<ContentGateField> missing = record.MissingGateFields();
            if (missing.Count > 0)
            {
                return new ContentResult(ContentOutcome.GateIncomplete, missing);
            }
        }

        PublishStatus from = record.PublishStatus;
        // The first publication fixes the address in the same save as the status (02 3.30.2; 06 4.72); a record that goes back to draft and out again keeps the moment.
        DateTimeOffset now = _time.GetUtcNow();
        RecordSaveResult saved = await RecordSave.SaveAsync(
            _db,
            record,
            expectedVersion,
            r =>
            {
                r.PublishStatus = target;
                if (target == PublishStatus.Published && r is IAddressedContentRecord addressed)
                {
                    addressed.SlugFixedAt ??= now;
                }
            },
            ct);
        if (!saved.IsSaved)
        {
            return ContentResult.Of(ContentOutcome.VersionConflict);
        }

        DropStaleAreas();
        await _trail.WriteAsync(
            new AuditEntry(
                AuditOperation.ChangePublishStatus,
                kind.ToString(),
                record.Id,
                new AuditDetails(TrailNameOf(record), [new AuditValueChange(StatusField, from.ToString(), target.ToString())])),
            ct);
        return ContentResult.Of(ContentOutcome.Changed, record.Version);
    }

    /// <summary>
    /// The name the trail row shows for a record (06 4.63): the record's own name, title or question, never its long text. The about page and the announcement carry no name
    /// of their own; they are shown by the type's fixed name.
    /// </summary>
    private static string TrailNameOf(IContentRecord record)
    {
        return record switch
        {
            AboutPage => nameof(AboutPage),
            Announcement => nameof(Announcement),
            Offering offering => offering.Name,
            PortfolioItem item => item.Title,
            FaqItem faq => faq.Question,
            Branch branch => branch.Name,
            CustomPage page => page.Title,
            _ => throw new ArgumentOutOfRangeException(nameof(record), record.Kind, "Unknown content record kind."),
        };
    }

    /// <summary>The cached parts a content record shows in: the menu, the home page blocks and the site map (05 7.10.1).</summary>
    private void DropStaleAreas()
    {
        _invalidations.Add(CacheArea.Menu);
        _invalidations.Add(CacheArea.HomepageBlocks);
        _invalidations.Add(CacheArea.Sitemap);
    }

    private async Task<IContentRecord?> FindAsync(ContentRecordKind kind, Guid id, CancellationToken ct)
    {
        switch (kind)
        {
            case ContentRecordKind.AboutPage:
                return await _db.AboutPages.SingleOrDefaultAsync(r => r.Id == id, ct);
            case ContentRecordKind.Offering:
                return await _db.Offerings.SingleOrDefaultAsync(r => r.Id == id, ct);
            case ContentRecordKind.PortfolioItem:
                return await _db.PortfolioItems.SingleOrDefaultAsync(r => r.Id == id, ct);
            case ContentRecordKind.FaqItem:
                return await _db.FaqItems.SingleOrDefaultAsync(r => r.Id == id, ct);
            case ContentRecordKind.Branch:
                return await _db.Branches.SingleOrDefaultAsync(r => r.Id == id, ct);
            case ContentRecordKind.CustomPage:
                return await _db.CustomPages.SingleOrDefaultAsync(r => r.Id == id, ct);
            case ContentRecordKind.Announcement:
                return await _db.Announcements.SingleOrDefaultAsync(r => r.Id == id, ct);
            default:
                throw new ArgumentOutOfRangeException(nameof(kind), kind, "Unknown content record kind.");
        }
    }
}
