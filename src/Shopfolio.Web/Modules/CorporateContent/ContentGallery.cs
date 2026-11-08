using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Settings.Contracts;

namespace Shopfolio.Modules.CorporateContent;

/// <summary>The link row every content gallery shares (06 4.71): the same columns in the five link tables, so one set of rules reads and writes them all.</summary>
internal interface IContentGalleryLink
{
    Guid Id { get; }

    DateTimeOffset CreatedAt { get; }

    Guid ImageId { get; }

    int SortOrder { get; set; }

    bool IsMain { get; set; }

    string? AltText { get; set; }
}

/// <summary>What became of a content gallery or branch image save (T359).</summary>
internal enum ContentGalleryOutcome
{
    Saved,

    /// <summary>The record does not exist.</summary>
    OwnerNotFound,

    /// <summary>The kind has no gallery of its own: the branch has one image, the announcement none (02 3.27.15).</summary>
    NotAGalleryKind,

    /// <summary>The record would hold more images than P-25 allows; nothing was written.</summary>
    LimitExceeded,

    /// <summary>An image of the list is not an uploaded image; nothing was written.</summary>
    ImageNotFound,

    /// <summary>The list names one image twice; nothing was written.</summary>
    DuplicateImage,

    /// <summary>The branch is no longer at the version the form carried (05 7.3.1).</summary>
    VersionConflict,
}

/// <summary>The result of a content gallery save; a refusal is a return value and the transaction runner rolls back (09 9.1 item 3).</summary>
internal sealed record ContentGalleryResult(ContentGalleryOutcome Outcome, int? Version = null) : IOperationResult
{
    public bool IsRejected => Outcome != ContentGalleryOutcome.Saved;
}

/// <summary>The image of a branch as it is shown: the image and the text the page uses, which is never empty.</summary>
internal sealed record BranchImageView(Guid ImageId, string? AltText, string EffectiveAltText);

/// <summary>
/// The images of the corporate content (T359; 02 3.27.15, 3.11.2, 3.11.4; 06 4.71; 10 KD-44): the about page, the service introduction, the reference work, the question and the general page
/// each have a gallery that follows the product's rules - the order is the firm's, the main image is a mark of its own independent of the order, a gallery that has an image has exactly one main
/// image, and the alternative text is optional: when it is empty the text is the record's name - the question for a question, the type's name for the about page - with the image's place in the
/// gallery ("Ad — görsel 2 / 5"), so it is never empty (the pure rules are <see cref="GalleryText"/>, shared with the product's gallery). The formats and the size of one file are the parameters of
/// the upload (P-22, P-23); the number of images of one record is P-25. A branch has one image beside its text. An announcement has none. Order, mark and text live on the link row. The save
/// joins the caller's transaction and never saves; the one statement that is sent at once - the clearing of the old main mark, because the database allows one main image per record at every
/// moment - runs inside that transaction.
/// </summary>
internal sealed class ContentGallery
{
    private readonly AppDbContext _db;
    private readonly ICatalogModule _catalog;
    private readonly ISettingsModule _settings;
    private readonly ContentPublishing _publishing;
    private readonly TimeProvider _time;

    public ContentGallery(AppDbContext db, ICatalogModule catalog, ISettingsModule settings, ContentPublishing publishing, TimeProvider time)
    {
        ArgumentNullException.ThrowIfNull(db);
        ArgumentNullException.ThrowIfNull(catalog);
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(publishing);
        ArgumentNullException.ThrowIfNull(time);
        _db = db;
        _catalog = catalog;
        _settings = settings;
        _publishing = publishing;
        _time = time;
    }

    /// <summary>The kinds with a gallery (02 3.27.15): not the branch (one image) and not the announcement (none).</summary>
    public static IReadOnlyList<ContentRecordKind> WithGallery { get; } =
        [ContentRecordKind.AboutPage, ContentRecordKind.Offering, ContentRecordKind.PortfolioItem, ContentRecordKind.FaqItem, ContentRecordKind.CustomPage];

    /// <summary>
    /// Saves the gallery as the form posts it: the links are made to match the list - those not in it go (the image row stays and the disposal removes it once nothing links it), new ones
    /// come, the position in the list is the order, the alternative text is the firm's - and the main mark is set by <see cref="GalleryText.MainOf"/>.
    /// </summary>
    public async Task<ContentGalleryResult> SaveAsync(ContentRecordKind kind, Guid ownerId, IReadOnlyList<GalleryEntry> entries, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(entries);
        RequireTransaction();
        if (!WithGallery.Contains(kind))
        {
            return new ContentGalleryResult(ContentGalleryOutcome.NotAGalleryKind);
        }

        if (await NameOfAsync(kind, ownerId, ct) is null)
        {
            return new ContentGalleryResult(ContentGalleryOutcome.OwnerNotFound);
        }

        DateTimeOffset now = _time.GetUtcNow();
        switch (kind)
        {
            case ContentRecordKind.AboutPage:
                return await SaveCoreAsync(
                    await _db.AboutPageImages.Where(l => l.AboutPageId == ownerId).ToListAsync(ct),
                    _db.AboutPageImages,
                    (image, order, main) => AboutPageImage.Create(ownerId, image, order, main, now),
                    () => _db.AboutPageImages.Where(l => l.AboutPageId == ownerId && l.IsMain).ExecuteUpdateAsync(s => s.SetProperty(l => l.IsMain, false), ct),
                    entries,
                    ct);
            case ContentRecordKind.Offering:
                return await SaveCoreAsync(
                    await _db.OfferingImages.Where(l => l.OfferingId == ownerId).ToListAsync(ct),
                    _db.OfferingImages,
                    (image, order, main) => OfferingImage.Create(ownerId, image, order, main, now),
                    () => _db.OfferingImages.Where(l => l.OfferingId == ownerId && l.IsMain).ExecuteUpdateAsync(s => s.SetProperty(l => l.IsMain, false), ct),
                    entries,
                    ct);
            case ContentRecordKind.PortfolioItem:
                return await SaveCoreAsync(
                    await _db.PortfolioItemImages.Where(l => l.PortfolioItemId == ownerId).ToListAsync(ct),
                    _db.PortfolioItemImages,
                    (image, order, main) => PortfolioItemImage.Create(ownerId, image, order, main, now),
                    () => _db.PortfolioItemImages.Where(l => l.PortfolioItemId == ownerId && l.IsMain).ExecuteUpdateAsync(s => s.SetProperty(l => l.IsMain, false), ct),
                    entries,
                    ct);
            case ContentRecordKind.FaqItem:
                return await SaveCoreAsync(
                    await _db.FaqItemImages.Where(l => l.FaqItemId == ownerId).ToListAsync(ct),
                    _db.FaqItemImages,
                    (image, order, main) => FaqItemImage.Create(ownerId, image, order, main, now),
                    () => _db.FaqItemImages.Where(l => l.FaqItemId == ownerId && l.IsMain).ExecuteUpdateAsync(s => s.SetProperty(l => l.IsMain, false), ct),
                    entries,
                    ct);
            case ContentRecordKind.CustomPage:
                return await SaveCoreAsync(
                    await _db.CustomPageImages.Where(l => l.CustomPageId == ownerId).ToListAsync(ct),
                    _db.CustomPageImages,
                    (image, order, main) => CustomPageImage.Create(ownerId, image, order, main, now),
                    () => _db.CustomPageImages.Where(l => l.CustomPageId == ownerId && l.IsMain).ExecuteUpdateAsync(s => s.SetProperty(l => l.IsMain, false), ct),
                    entries,
                    ct);
            default:
                return new ContentGalleryResult(ContentGalleryOutcome.NotAGalleryKind);
        }
    }

    /// <summary>The gallery in its order, each image with the firm's text and the text the page uses (02 3.11.4); empty for a record without images, without a row, or of a kind with no gallery.</summary>
    public async Task<IReadOnlyList<GalleryImage>> ReadAsync(ContentRecordKind kind, Guid ownerId, CancellationToken ct)
    {
        if (!WithGallery.Contains(kind))
        {
            return [];
        }

        string? name = await NameOfAsync(kind, ownerId, ct);
        if (name is null)
        {
            return [];
        }

        List<GalleryRow> links = kind switch
        {
            ContentRecordKind.AboutPage => await _db.AboutPageImages.AsNoTracking().Where(l => l.AboutPageId == ownerId)
                .OrderBy(l => l.SortOrder).ThenBy(l => l.CreatedAt).ThenBy(l => l.Id).Select(l => new GalleryRow(l.ImageId, l.IsMain, l.AltText)).ToListAsync(ct),
            ContentRecordKind.Offering => await _db.OfferingImages.AsNoTracking().Where(l => l.OfferingId == ownerId)
                .OrderBy(l => l.SortOrder).ThenBy(l => l.CreatedAt).ThenBy(l => l.Id).Select(l => new GalleryRow(l.ImageId, l.IsMain, l.AltText)).ToListAsync(ct),
            ContentRecordKind.PortfolioItem => await _db.PortfolioItemImages.AsNoTracking().Where(l => l.PortfolioItemId == ownerId)
                .OrderBy(l => l.SortOrder).ThenBy(l => l.CreatedAt).ThenBy(l => l.Id).Select(l => new GalleryRow(l.ImageId, l.IsMain, l.AltText)).ToListAsync(ct),
            ContentRecordKind.FaqItem => await _db.FaqItemImages.AsNoTracking().Where(l => l.FaqItemId == ownerId)
                .OrderBy(l => l.SortOrder).ThenBy(l => l.CreatedAt).ThenBy(l => l.Id).Select(l => new GalleryRow(l.ImageId, l.IsMain, l.AltText)).ToListAsync(ct),
            _ => await _db.CustomPageImages.AsNoTracking().Where(l => l.CustomPageId == ownerId)
                .OrderBy(l => l.SortOrder).ThenBy(l => l.CreatedAt).ThenBy(l => l.Id).Select(l => new GalleryRow(l.ImageId, l.IsMain, l.AltText)).ToListAsync(ct),
        };
        return [.. links.Select((l, i) => new GalleryImage(l.ImageId, i + 1, l.IsMain, l.AltText, GalleryText.Choose(l.AltText, () => GalleryText.Generated(name, i + 1, links.Count))))];
    }

    /// <summary>
    /// Gives the branch its one image with the firm's text, or - with null - takes it away (the text goes with it); the image is a column of the branch, so the save carries the form's version
    /// like any edit of the record (05 7.3.1). An image that is not an uploaded image is refused.
    /// </summary>
    public async Task<ContentGalleryResult> SetBranchImageAsync(Guid branchId, Guid? imageId, string? altText, int formVersion, CancellationToken ct)
    {
        RequireTransaction();
        Branch? branch = await _db.Branches.SingleOrDefaultAsync(b => b.Id == branchId, ct);
        if (branch is null)
        {
            return new ContentGalleryResult(ContentGalleryOutcome.OwnerNotFound);
        }

        if (imageId is { } id && !await _catalog.ImageExistsAsync(id, ct))
        {
            return new ContentGalleryResult(ContentGalleryOutcome.ImageNotFound);
        }

        ContentResult saved = await _publishing.SaveEditAsync(
            branch,
            formVersion,
            b =>
            {
                b.ImageId = imageId;
                b.ImageAltText = imageId is null ? null : GalleryText.CleanAltText(altText);
            },
            ct);
        return saved.Outcome switch
        {
            ContentOutcome.Saved => new ContentGalleryResult(ContentGalleryOutcome.Saved, saved.Version),
            ContentOutcome.VersionConflict => new ContentGalleryResult(ContentGalleryOutcome.VersionConflict),
            _ => throw new InvalidOperationException($"The save of a branch's image cannot end as {saved.Outcome}."),
        };
    }

    /// <summary>The image of a branch with the text the page uses (the firm's, else the branch's name with the place "görsel 1 / 1"); null when the branch has none or does not exist.</summary>
    public async Task<BranchImageView?> ReadBranchImageAsync(Guid branchId, CancellationToken ct)
    {
        var row = await _db.Branches.AsNoTracking().Where(b => b.Id == branchId).Select(b => new { b.Name, b.ImageId, b.ImageAltText }).SingleOrDefaultAsync(ct);
        if (row?.ImageId is not { } imageId)
        {
            return null;
        }

        return new BranchImageView(imageId, row.ImageAltText, GalleryText.Choose(row.ImageAltText, () => GalleryText.Generated(row.Name, 1, 1)));
    }

    private async Task<ContentGalleryResult> SaveCoreAsync<TLink>(
        List<TLink> links,
        DbSet<TLink> set,
        Func<Guid, int, bool, TLink> create,
        Func<Task<int>> clearMain,
        IReadOnlyList<GalleryEntry> entries,
        CancellationToken ct)
        where TLink : class, IContentGalleryLink
    {
        List<Guid> imageIds = [.. entries.Select(e => e.ImageId)];
        if (imageIds.Count > ProductConstants.ImagesPerContentRecordMax)
        {
            return new ContentGalleryResult(ContentGalleryOutcome.LimitExceeded);
        }

        if (imageIds.Distinct().Count() != imageIds.Count)
        {
            return new ContentGalleryResult(ContentGalleryOutcome.DuplicateImage);
        }

        foreach (Guid imageId in imageIds)
        {
            if (!await _catalog.ImageExistsAsync(imageId, ct))
            {
                return new ContentGalleryResult(ContentGalleryOutcome.ImageNotFound);
            }
        }

        TLink? currentMain = links.FirstOrDefault(l => l.IsMain);
        Guid? mainImageId = GalleryText.MainOf([.. entries.Select(e => (e.ImageId, e.IsMain))], currentMain?.ImageId);

        // The database allows one main image per record at every moment, not only at the end of the save: the old mark is cleared before the new one is written.
        if (currentMain is not null && currentMain.ImageId != mainImageId)
        {
            await clearMain();
            currentMain.IsMain = false;
        }

        set.RemoveRange(links.Where(l => !imageIds.Contains(l.ImageId)));
        for (int i = 0; i < entries.Count; i++)
        {
            GalleryEntry entry = entries[i];
            string? altText = GalleryText.CleanAltText(entry.AltText);
            bool isMain = entry.ImageId == mainImageId;
            TLink? link = links.FirstOrDefault(l => l.ImageId == entry.ImageId);
            if (link is null)
            {
                TLink created = create(entry.ImageId, i + 1, isMain);
                created.AltText = altText;
                set.Add(created);
                continue;
            }

            link.SortOrder = i + 1;
            link.AltText = altText;
            link.IsMain = isMain;
        }

        return new ContentGalleryResult(ContentGalleryOutcome.Saved);
    }

    /// <summary>The name the generated text starts with (02 3.27.15): the record's name or title, the question for a question, the type's name - the firm's menu name for it - for the about page.</summary>
    private async Task<string?> NameOfAsync(ContentRecordKind kind, Guid ownerId, CancellationToken ct)
    {
        switch (kind)
        {
            case ContentRecordKind.AboutPage:
                return await _db.AboutPages.AsNoTracking().AnyAsync(a => a.Id == ownerId, ct) ? (await _settings.GetHomepageAsync(ct)).MenuNames.AboutPage : null;
            case ContentRecordKind.Offering:
                return await _db.Offerings.AsNoTracking().Where(o => o.Id == ownerId).Select(o => o.Name).SingleOrDefaultAsync(ct);
            case ContentRecordKind.PortfolioItem:
                return await _db.PortfolioItems.AsNoTracking().Where(p => p.Id == ownerId).Select(p => p.Title).SingleOrDefaultAsync(ct);
            case ContentRecordKind.FaqItem:
                return await _db.FaqItems.AsNoTracking().Where(f => f.Id == ownerId).Select(f => f.Question).SingleOrDefaultAsync(ct);
            default:
                return await _db.CustomPages.AsNoTracking().Where(c => c.Id == ownerId).Select(c => c.Title).SingleOrDefaultAsync(ct);
        }
    }

    private void RequireTransaction()
    {
        if (_db.Database.CurrentTransaction is null)
        {
            throw new InvalidOperationException("A content gallery is written only inside the caller's transaction (09 9.1 item 3).");
        }
    }

    private sealed record GalleryRow(Guid ImageId, bool IsMain, string? AltText);
}
