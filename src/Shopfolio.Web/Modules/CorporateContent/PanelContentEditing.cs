using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Routing;
using Shopfolio.Infrastructure.Validation;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.CorporateContent.Contracts;
using Shopfolio.Modules.Settings.Contracts;

namespace Shopfolio.Modules.CorporateContent;

/// <summary>
/// The panel's side of the corporate content (T534; 07 UC-96, UC-158...UC-161; 04 9.13): the seven-group list, the record's form, the one save of the whole form - the record, its gallery or branch
/// image and its linked products -, the publish status move, the deletion and the move in the manual order. It composes the writers of T358, T359 and T322 (<see cref="ContentAuthoring"/>,
/// <see cref="ContentGallery"/>, <see cref="ContentProductLinks"/>, <see cref="ContentPublishing"/>, <see cref="ContentOrdering"/>) and owns no rule of its own. Every write joins the caller's transaction and never
/// opens or commits one (09 9.1 item 3); a refusal is a value and the runner rolls back whatever the composed parts already wrote. The status move and the deletion carry no version (K-1193 (1)): the
/// record's current version is read in the same transaction and handed to the writer, which compares it under the record's concurrency token; a save that lands between the read and the write moves the
/// version on and the read is repeated once.
/// </summary>
internal sealed class PanelContentEditing
{
    /// <summary>The form member a refused gallery names.</summary>
    public const string GalleryField = "Images";

    /// <summary>The form member a refused branch image names (07 5.2.3).</summary>
    public const string BranchImageField = "ImageId";

    private const int StatusMoveAttempts = 2;

    private readonly AppDbContext _db;
    private readonly ContentAuthoring _authoring;
    private readonly ContentPublishing _publishing;
    private readonly ContentOrdering _ordering;
    private readonly ContentGallery _gallery;
    private readonly ContentProductLinks _links;
    private readonly ISettingsModule _settings;
    private readonly TimeProvider _time;

    public PanelContentEditing(
        AppDbContext db,
        ContentAuthoring authoring,
        ContentPublishing publishing,
        ContentOrdering ordering,
        ContentGallery gallery,
        ContentProductLinks links,
        ISettingsModule settings,
        TimeProvider time)
    {
        ArgumentNullException.ThrowIfNull(db);
        ArgumentNullException.ThrowIfNull(authoring);
        ArgumentNullException.ThrowIfNull(publishing);
        ArgumentNullException.ThrowIfNull(ordering);
        ArgumentNullException.ThrowIfNull(gallery);
        ArgumentNullException.ThrowIfNull(links);
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(time);
        _db = db;
        _authoring = authoring;
        _publishing = publishing;
        _ordering = ordering;
        _gallery = gallery;
        _links = links;
        _settings = settings;
        _time = time;
    }

    /// <summary>The kind of a panel type (the two enums list the same seven; a test keeps them equal).</summary>
    public static ContentRecordKind KindOf(PanelContentType type) => type switch
    {
        PanelContentType.AboutPage => ContentRecordKind.AboutPage,
        PanelContentType.Offering => ContentRecordKind.Offering,
        PanelContentType.PortfolioItem => ContentRecordKind.PortfolioItem,
        PanelContentType.FaqItem => ContentRecordKind.FaqItem,
        PanelContentType.Branch => ContentRecordKind.Branch,
        PanelContentType.CustomPage => ContentRecordKind.CustomPage,
        PanelContentType.Announcement => ContentRecordKind.Announcement,
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Unknown panel content type."),
    };

    /// <summary>The seven groups with every record, in the firm's manual order (04 9.13.3; 06 4.47): the order, the time of creation, the key. Never paged (K-843). A plain read.</summary>
    public async Task<PanelContentList> ListAsync(CancellationToken ct)
    {
        MenuNames names = (await _settings.GetHomepageAsync(ct)).MenuNames;
        DateOnly today = TurkeyTime.DateOf(_time.GetUtcNow());

        List<Head> about = [.. (await _db.AboutPages.AsNoTracking().Select(a => new { a.Id, a.PublishStatus }).ToListAsync(ct)).Select(a => new Head(a.Id, null, a.PublishStatus, false, false, 0))];
        List<Head> offerings = await _db.Offerings.AsNoTracking()
            .OrderBy(r => r.SortOrder).ThenBy(r => r.CreatedAt).ThenBy(r => r.Id)
            .Select(r => new Head(r.Id, r.Name, r.PublishStatus, r.IsShownOnHomepage, false, r.SortOrder)).ToListAsync(ct);
        List<Head> portfolio = await _db.PortfolioItems.AsNoTracking()
            .OrderBy(r => r.SortOrder).ThenBy(r => r.CreatedAt).ThenBy(r => r.Id)
            .Select(r => new Head(r.Id, r.Title, r.PublishStatus, r.IsShownOnHomepage, false, r.SortOrder)).ToListAsync(ct);
        List<Head> faq = await _db.FaqItems.AsNoTracking()
            .OrderBy(r => r.SortOrder).ThenBy(r => r.CreatedAt).ThenBy(r => r.Id)
            .Select(r => new Head(r.Id, r.Question, r.PublishStatus, false, false, r.SortOrder)).ToListAsync(ct);
        List<Head> branches = await _db.Branches.AsNoTracking()
            .OrderBy(r => r.SortOrder).ThenBy(r => r.CreatedAt).ThenBy(r => r.Id)
            .Select(r => new Head(r.Id, r.Name, r.PublishStatus, false, false, r.SortOrder)).ToListAsync(ct);
        List<Head> pages = await _db.CustomPages.AsNoTracking()
            .OrderBy(r => r.SortOrder).ThenBy(r => r.CreatedAt).ThenBy(r => r.Id)
            .Select(r => new Head(r.Id, r.Title, r.PublishStatus, false, r.IsShownInMenu, r.SortOrder)).ToListAsync(ct);
        Announcement? announcement = await _db.Announcements.AsNoTracking().SingleOrDefaultAsync(ct);
        List<PanelContentRow> announcementRows = announcement is null
            ? []
            :
            [
                new PanelContentRow(
                    announcement.Id,
                    announcement.Text,
                    announcement.PublishStatus,
                    false,
                    false,
                    0,
                    null,
                    null,
                    announcement.StartDate,
                    announcement.EndDate,
                    ContentVisibility.Decide(announcement, ContentAudience.Visitor, today) == ContentShown.Shown),
            ];

        return new PanelContentList(
        [
            new PanelContentGroup(PanelContentType.AboutPage, names.AboutPage, RowsOf(about, ordered: false), 0),
            new PanelContentGroup(PanelContentType.Offering, names.Offerings, RowsOf(offerings, ordered: true), offerings.Count(h => h.IsOnHomepage)),
            new PanelContentGroup(PanelContentType.PortfolioItem, names.PortfolioItems, RowsOf(portfolio, ordered: true), portfolio.Count(h => h.IsOnHomepage)),
            new PanelContentGroup(PanelContentType.FaqItem, names.FaqItems, RowsOf(faq, ordered: true), 0),
            new PanelContentGroup(PanelContentType.Branch, null, RowsOf(branches, ordered: true), 0),
            new PanelContentGroup(PanelContentType.CustomPage, null, RowsOf(pages, ordered: true), 0),
            new PanelContentGroup(PanelContentType.Announcement, null, announcementRows, 0),
        ]);
    }

    /// <summary>The key of the one record of Hakkımızda or of the announcement; null when there is none or the type is not a single-record type. A plain read.</summary>
    public async Task<Guid?> FindSingleAsync(PanelContentType type, CancellationToken ct)
    {
        return type switch
        {
            PanelContentType.AboutPage => await _db.AboutPages.AsNoTracking().Select(a => (Guid?)a.Id).SingleOrDefaultAsync(ct),
            PanelContentType.Announcement => await _db.Announcements.AsNoTracking().Select(a => (Guid?)a.Id).SingleOrDefaultAsync(ct),
            _ => null,
        };
    }

    /// <summary>
    /// The record's form (07 UC-96): with <paramref name="id"/> null the empty form of a new record - the version 0 -, else the record as stored; null when the record is not there or is another type's
    /// (404). A plain read.
    /// </summary>
    public async Task<PanelContentForm?> GetFormAsync(PanelContentType type, Guid? id, CancellationToken ct)
    {
        int homepageMarked = type switch
        {
            PanelContentType.Offering => await _db.Offerings.AsNoTracking().CountAsync(o => o.IsShownOnHomepage, ct),
            PanelContentType.PortfolioItem => await _db.PortfolioItems.AsNoTracking().CountAsync(p => p.IsShownOnHomepage, ct),
            _ => 0,
        };
        if (id is null)
        {
            return new PanelContentForm(type, null, 0, PublishStatus.Draft, new PanelContentValues(), [], [], [], null, false, homepageMarked);
        }

        ContentRecordKind kind = KindOf(type);
        Guid key = id.Value;
        PanelContentValues? values;
        IContentRecord? record;
        string? pagePath = null;
        bool fixedAddress = false;
        switch (type)
        {
            case PanelContentType.AboutPage:
                AboutPage? about = await _db.AboutPages.AsNoTracking().SingleOrDefaultAsync(a => a.Id == key, ct);
                record = about;
                values = about is null ? null : new PanelContentValues { ShortIntroduction = about.ShortIntroduction, LongText = about.LongText };
                pagePath = "/" + AddressRules.AboutPage;
                fixedAddress = true;
                break;
            case PanelContentType.Offering:
                Offering? offering = await _db.Offerings.AsNoTracking().SingleOrDefaultAsync(o => o.Id == key, ct);
                record = offering;
                values = offering is null ? null : new PanelContentValues { Name = offering.Name, ShortDescription = offering.ShortDescription, Text = offering.Text, IsShownOnHomepage = offering.IsShownOnHomepage };
                pagePath = offering is null ? null : AddressRules.RecordAddress(AddressRules.ServicePrefix, offering.Slug);
                fixedAddress = offering?.SlugFixedAt is not null;
                break;
            case PanelContentType.PortfolioItem:
                PortfolioItem? item = await _db.PortfolioItems.AsNoTracking().SingleOrDefaultAsync(p => p.Id == key, ct);
                record = item;
                values = item is null ? null : new PanelContentValues { Title = item.Title, ShortDescription = item.ShortDescription, Text = item.Text, IsShownOnHomepage = item.IsShownOnHomepage };
                pagePath = item is null ? null : AddressRules.RecordAddress(AddressRules.ReferencePrefix, item.Slug);
                fixedAddress = item?.SlugFixedAt is not null;
                break;
            case PanelContentType.FaqItem:
                FaqItem? faq = await _db.FaqItems.AsNoTracking().SingleOrDefaultAsync(f => f.Id == key, ct);
                record = faq;
                values = faq is null ? null : new PanelContentValues { Question = faq.Question, Answer = faq.Answer };
                break;
            case PanelContentType.Branch:
                Branch? branch = await _db.Branches.AsNoTracking().SingleOrDefaultAsync(b => b.Id == key, ct);
                record = branch;
                values = branch is null
                    ? null
                    : new PanelContentValues
                    {
                        Name = branch.Name,
                        ProvinceId = branch.ProvinceId,
                        District = branch.District,
                        AddressLine = branch.AddressLine,
                        Phone = branch.Phone,
                        WorkingHours = branch.WorkingHours,
                        MapUrl = branch.MapUrl,
                        ImageId = branch.ImageId,
                        ImageAltText = branch.ImageAltText,
                    };
                break;
            case PanelContentType.CustomPage:
                CustomPage? page = await _db.CustomPages.AsNoTracking().SingleOrDefaultAsync(c => c.Id == key, ct);
                record = page;
                values = page is null ? null : new PanelContentValues { Title = page.Title, Text = page.Text, IsShownInMenu = page.IsShownInMenu };
                pagePath = page is null ? null : AddressRules.RecordAddress(AddressRules.GeneralPagePrefix, page.Slug);
                fixedAddress = page?.SlugFixedAt is not null;
                break;
            case PanelContentType.Announcement:
                Announcement? announcement = await _db.Announcements.AsNoTracking().SingleOrDefaultAsync(a => a.Id == key, ct);
                record = announcement;
                values = announcement is null
                    ? null
                    : new PanelContentValues { Text = announcement.Text, LinkUrl = announcement.LinkUrl, StartDate = announcement.StartDate, EndDate = announcement.EndDate };
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, "Unknown panel content type.");
        }

        if (record is null || values is null)
        {
            return null;
        }

        IReadOnlyList<GalleryEntry> images = [.. (await _gallery.ReadAsync(kind, key, ct)).Select(i => new GalleryEntry(i.ImageId, i.AltText, i.IsMain))];
        IReadOnlyList<Guid> products = type switch
        {
            PanelContentType.Offering => await _links.ListLinkedIdsAsync(ContentLinkOwner.Offering, key, ct),
            PanelContentType.PortfolioItem => await _links.ListLinkedIdsAsync(ContentLinkOwner.PortfolioItem, key, ct),
            _ => [],
        };
        return new PanelContentForm(type, key, record.Version, record.PublishStatus, values, images, products, MissingOf(record.MissingGateFields()), pagePath, fixedAddress, homepageMarked);
    }

    /// <summary>
    /// The save of the whole form (07 UC-158): the record - created as a draft at the end of its group, or edited against the version the form carried -, then its gallery or its branch image, then its
    /// linked products. The order of the refusals is the writers': the record is not there, the fields' 422s, the version's 409, the published record's gate; the gallery's and the links' refusals come
    /// after the record is written and roll it back with them. A product that is gone meanwhile is skipped (06 4.53).
    /// </summary>
    public async Task<PanelContentSaveResult> SaveAsync(PanelContentSaveCommand command, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(command);
        ContentRecordKind kind = KindOf(command.Type);
        bool isNew = command.Id is null;
        if (command.Id is { } given && await FindSingleAsync(command.Type, ct) is { } single && single != given)
        {
            return PanelContentSaveResult.Refused(PanelContentSaveOutcome.NotFound);
        }

        int version = command.Version ?? 0;
        AuthoringResult authored = await AuthorAsync(command.Type, command.Id, command.Values, version, ct);
        switch (authored.Outcome)
        {
            case AuthoringOutcome.Created:
            case AuthoringOutcome.Saved:
                break;
            case AuthoringOutcome.AlreadyExists:
                return PanelContentSaveResult.Refused(PanelContentSaveOutcome.AlreadyExists, await FindSingleAsync(command.Type, ct));
            case AuthoringOutcome.NotFound:
                return PanelContentSaveResult.Refused(PanelContentSaveOutcome.NotFound);
            case AuthoringOutcome.VersionConflict:
                return PanelContentSaveResult.Refused(PanelContentSaveOutcome.VersionConflict, command.Id);
            case AuthoringOutcome.PublishedGateBroken:
                return PanelContentSaveResult.GateBroken(authored.Id ?? Guid.Empty, MissingOf(authored.Missing));
            default:
                return PanelContentSaveResult.Rejected([.. authored.Rejections.Select(r => new PanelContentRejection(r.Field, r.Code))]);
        }

        Guid id = authored.Id ?? throw new InvalidOperationException("A saved content record carries its key.");
        if (isNew)
        {
            // The gallery and the links read the owner from the database; the new row is written first, inside the same transaction.
            await _db.SaveChangesAsync(ct);
        }

        if (command.Type == PanelContentType.Branch)
        {
            if (await SaveBranchImageAsync(id, command.Values, ct) is { } branchRefusal)
            {
                return branchRefusal;
            }
        }
        else if (ContentGallery.WithGallery.Contains(kind))
        {
            ContentGalleryResult gallery = await _gallery.SaveAsync(kind, id, command.Images, ct);
            if (gallery.Outcome != ContentGalleryOutcome.Saved)
            {
                return PanelContentSaveResult.Rejected([new PanelContentRejection(GalleryField, GalleryCode(gallery.Outcome))]);
            }
        }

        if (command.Type is PanelContentType.Offering or PanelContentType.PortfolioItem)
        {
            ContentLinkOwner owner = command.Type == PanelContentType.Offering ? ContentLinkOwner.Offering : ContentLinkOwner.PortfolioItem;
            await _links.SaveAsync(owner, id, [.. command.ProductIds.Distinct()], ct);
        }

        return PanelContentSaveResult.Saved(id);
    }

    /// <summary>
    /// Moves the record to <paramref name="target"/> (07 UC-159): absolute, so a record already there is written nothing; the gate is checked for the way to Published and names its empty fields. The
    /// version is the record's current one, read here (K-1193 (1)).
    /// </summary>
    public async Task<PanelContentChangeResult> ChangeStatusAsync(PanelContentType type, Guid id, PublishStatus target, CancellationToken ct)
    {
        if (target is not (PublishStatus.Draft or PublishStatus.Published))
        {
            throw new ArgumentOutOfRangeException(nameof(target), target, "A corporate content record is a draft or published (02 5.2).");
        }

        ContentRecordKind kind = KindOf(type);
        for (int attempt = 0; attempt < StatusMoveAttempts; attempt++)
        {
            int? version = await VersionOfAsync(type, id, ct);
            if (version is null)
            {
                return PanelContentChangeResult.Of(PanelContentChangeOutcome.NotFound);
            }

            ContentResult result = target == PublishStatus.Published
                ? await _publishing.PublishAsync(kind, id, version.Value, ct)
                : await _publishing.UnpublishAsync(kind, id, version.Value, ct);
            switch (result.Outcome)
            {
                case ContentOutcome.Changed:
                    return PanelContentChangeResult.Of(PanelContentChangeOutcome.Changed);
                case ContentOutcome.Unchanged:
                    return PanelContentChangeResult.Of(PanelContentChangeOutcome.Unchanged);
                case ContentOutcome.GateIncomplete:
                    return PanelContentChangeResult.Incomplete(MissingOf(result.Missing));
                case ContentOutcome.NotFound:
                    return PanelContentChangeResult.Of(PanelContentChangeOutcome.NotFound);
                default:
                    // A save landed between the read and the write: read the new version and write again.
                    _db.ChangeTracker.Clear();
                    break;
            }
        }

        return PanelContentChangeResult.Of(PanelContentChangeOutcome.NotFound);
    }

    /// <summary>Deletes the record for good (07 UC-160): the asking for the confirmation is the page's. Hakkımızda is refused. The deletion is tracked, the transaction's owner saves it (D-673).</summary>
    public async Task<PanelContentDeleteResult> DeleteAsync(PanelContentType type, Guid id, CancellationToken ct)
    {
        ContentResult result = await _publishing.DeleteAsync(KindOf(type), id, irreversibleWarningConfirmed: true, ct);
        return PanelContentDeleteResult.Of(result.Outcome switch
        {
            ContentOutcome.Deleted => PanelContentDeleteOutcome.Deleted,
            ContentOutcome.Forbidden => PanelContentDeleteOutcome.Forbidden,
            _ => PanelContentDeleteOutcome.NotFound,
        });
    }

    /// <summary>Moves the record one place in its group (07 UC-161) while both records still carry the orders the list showed.</summary>
    public async Task<PanelContentReorderResult> ReorderAsync(PanelContentType type, Guid id, PanelContentMove move, int shownOrder, int shownNeighbourOrder, CancellationToken ct)
    {
        OrderingResult result = await _ordering.MoveAsync(KindOf(type), id, move == PanelContentMove.Up ? MoveDirection.Up : MoveDirection.Down, shownOrder, shownNeighbourOrder, ct);
        return PanelContentReorderResult.Of(result.Outcome switch
        {
            OrderingOutcome.Moved => PanelContentReorderOutcome.Moved,
            OrderingOutcome.OrderChanged => PanelContentReorderOutcome.OrderChanged,
            OrderingOutcome.NotOrderable => PanelContentReorderOutcome.NotOrderable,
            OrderingOutcome.AtEdge => PanelContentReorderOutcome.AtEdge,
            _ => PanelContentReorderOutcome.NotFound,
        });
    }

    private async Task<PanelContentSaveResult?> SaveBranchImageAsync(Guid branchId, PanelContentValues values, CancellationToken ct)
    {
        Branch branch = await _db.Branches.SingleAsync(b => b.Id == branchId, ct);
        Guid? image = values.ImageId == Guid.Empty ? null : values.ImageId;
        string? alt = image is null ? null : GalleryText.CleanAltText(values.ImageAltText);
        if (branch.ImageId == image && branch.ImageAltText == alt)
        {
            return null;
        }

        ContentGalleryResult result = await _gallery.SetBranchImageAsync(branchId, image, alt, branch.Version, ct);
        return result.Outcome switch
        {
            ContentGalleryOutcome.Saved => null,
            ContentGalleryOutcome.ImageNotFound => PanelContentSaveResult.Rejected([new PanelContentRejection(BranchImageField, FieldCodes.InvalidImage)]),
            ContentGalleryOutcome.VersionConflict => PanelContentSaveResult.Refused(PanelContentSaveOutcome.VersionConflict, branchId),
            _ => PanelContentSaveResult.Refused(PanelContentSaveOutcome.NotFound),
        };
    }

    private static string GalleryCode(ContentGalleryOutcome outcome) => outcome == ContentGalleryOutcome.LimitExceeded ? FieldCodes.ImageLimitExceeded : FieldCodes.InvalidImage;

    private Task<AuthoringResult> AuthorAsync(PanelContentType type, Guid? id, PanelContentValues v, int version, CancellationToken ct)
    {
        switch (type)
        {
            case PanelContentType.AboutPage:
                AboutPageInput about = new(v.ShortIntroduction, v.LongText);
                return id is null ? _authoring.CreateAboutPageAsync(about, ct) : _authoring.SaveAboutPageAsync(about, version, ct);
            case PanelContentType.Offering:
                OfferingInput offering = new(v.Name, v.ShortDescription, v.Text, v.IsShownOnHomepage);
                return id is { } offeringId ? _authoring.SaveOfferingAsync(offeringId, offering, version, ct) : _authoring.CreateOfferingAsync(offering, ct);
            case PanelContentType.PortfolioItem:
                PortfolioItemInput item = new(v.Title, v.ShortDescription, v.Text, v.IsShownOnHomepage);
                return id is { } itemId ? _authoring.SavePortfolioItemAsync(itemId, item, version, ct) : _authoring.CreatePortfolioItemAsync(item, ct);
            case PanelContentType.FaqItem:
                FaqItemInput faq = new(v.Question, v.Answer);
                return id is { } faqId ? _authoring.SaveFaqItemAsync(faqId, faq, version, ct) : _authoring.CreateFaqItemAsync(faq, ct);
            case PanelContentType.Branch:
                BranchInput branch = new(v.Name, v.ProvinceId, v.District, v.AddressLine, v.Phone, v.WorkingHours, v.MapUrl);
                return id is { } branchId ? _authoring.SaveBranchAsync(branchId, branch, version, ct) : _authoring.CreateBranchAsync(branch, ct);
            case PanelContentType.CustomPage:
                CustomPageInput page = new(v.Title, v.Text, v.IsShownInMenu);
                return id is { } pageId ? _authoring.SaveCustomPageAsync(pageId, page, version, ct) : _authoring.CreateCustomPageAsync(page, ct);
            case PanelContentType.Announcement:
                AnnouncementInput announcement = new(v.Text, v.LinkUrl, v.StartDate, v.EndDate);
                return id is null ? _authoring.CreateAnnouncementAsync(announcement, ct) : _authoring.SaveAnnouncementAsync(announcement, version, ct);
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, "Unknown panel content type.");
        }
    }

    private async Task<int?> VersionOfAsync(PanelContentType type, Guid id, CancellationToken ct)
    {
        return type switch
        {
            PanelContentType.AboutPage => await _db.AboutPages.AsNoTracking().Where(r => r.Id == id).Select(r => (int?)r.Version).SingleOrDefaultAsync(ct),
            PanelContentType.Offering => await _db.Offerings.AsNoTracking().Where(r => r.Id == id).Select(r => (int?)r.Version).SingleOrDefaultAsync(ct),
            PanelContentType.PortfolioItem => await _db.PortfolioItems.AsNoTracking().Where(r => r.Id == id).Select(r => (int?)r.Version).SingleOrDefaultAsync(ct),
            PanelContentType.FaqItem => await _db.FaqItems.AsNoTracking().Where(r => r.Id == id).Select(r => (int?)r.Version).SingleOrDefaultAsync(ct),
            PanelContentType.Branch => await _db.Branches.AsNoTracking().Where(r => r.Id == id).Select(r => (int?)r.Version).SingleOrDefaultAsync(ct),
            PanelContentType.CustomPage => await _db.CustomPages.AsNoTracking().Where(r => r.Id == id).Select(r => (int?)r.Version).SingleOrDefaultAsync(ct),
            PanelContentType.Announcement => await _db.Announcements.AsNoTracking().Where(r => r.Id == id).Select(r => (int?)r.Version).SingleOrDefaultAsync(ct),
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Unknown panel content type."),
        };
    }

    private static IReadOnlyList<PanelContentField> MissingOf(IReadOnlyList<ContentGateField> fields) =>
        [.. fields.Select(f => Enum.Parse<PanelContentField>(f.ToString()))];

    private static List<PanelContentRow> RowsOf(List<Head> heads, bool ordered)
    {
        List<PanelContentRow> rows = [];
        for (int i = 0; i < heads.Count; i++)
        {
            Head h = heads[i];
            rows.Add(new PanelContentRow(
                h.Id,
                h.Title,
                h.Status,
                h.IsOnHomepage,
                h.IsInMenu,
                h.SortOrder,
                ordered && i > 0 ? heads[i - 1].SortOrder : null,
                ordered && i < heads.Count - 1 ? heads[i + 1].SortOrder : null,
                null,
                null,
                h.Status == PublishStatus.Published));
        }

        return rows;
    }

    private sealed record Head(Guid Id, string? Title, PublishStatus Status, bool IsOnHomepage, bool IsInMenu, int SortOrder);
}
