using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Modules.Catalog.Contracts;

namespace Shopfolio.Modules.CorporateContent;

/// <summary>
/// A corporate page as its own address shows it (T360; 02 3.27.25): the title when the type has one (the about page has none: the firm's menu name heads it), a summary, the cleaned
/// text, and <see cref="IsDraft"/> - true when the administrator is looking at a draft, which the page marks with the "Taslak" band. A visitor never gets a draft: the reader answers null
/// and the page falls to "sayfa bulunamadı".
/// </summary>
internal sealed record ContentPageView(ContentRecordKind Kind, Guid Id, string? Title, string? Summary, string? BodyHtml, bool IsDraft);

/// <summary>
/// The read of the pages that have an address of their own - the about page, a service introduction, a reference work and a general page (T360; 02 3.27.25, 3.27.27). Whether a record
/// is shown is <see cref="ContentVisibility"/>'s decision alone. A record that does not exist - a deleted one, an address nobody has - answers null like a draft does to a visitor, so the
/// two cannot be told apart from outside. A plain read; it writes nothing.
/// </summary>
internal sealed class ContentPageReader
{
    private readonly AppDbContext _db;

    public ContentPageReader(AppDbContext db)
    {
        ArgumentNullException.ThrowIfNull(db);
        _db = db;
    }

    /// <summary>The about page, or null when there is none or it is hidden from <paramref name="audience"/>.</summary>
    public async Task<ContentPageView?> FindAboutPageAsync(ContentAudience audience, CancellationToken ct)
    {
        PublishStatus[] statuses = ContentVisibility.VisibleStatuses(audience);
        var row = await _db.AboutPages.AsNoTracking()
            .Where(a => statuses.Contains(a.PublishStatus))
            .Select(a => new { a.Id, a.ShortIntroduction, a.LongText, a.PublishStatus })
            .SingleOrDefaultAsync(ct);
        return row is null ? null : View(ContentRecordKind.AboutPage, row.Id, null, row.ShortIntroduction, row.LongText, row.PublishStatus, audience);
    }

    /// <summary>
    /// The page of <paramref name="kind"/> at <paramref name="slug"/>; only a service introduction, a reference work and a general page have an address, any other kind is an error
    /// of the caller.
    /// </summary>
    public async Task<ContentPageView?> FindAsync(ContentRecordKind kind, string slug, ContentAudience audience, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(slug);
        PublishStatus[] statuses = ContentVisibility.VisibleStatuses(audience);
        switch (kind)
        {
            case ContentRecordKind.Offering:
                var offering = await _db.Offerings.AsNoTracking()
                    .Where(o => o.Slug == slug && statuses.Contains(o.PublishStatus))
                    .Select(o => new { o.Id, o.Name, o.ShortDescription, o.Text, o.PublishStatus })
                    .SingleOrDefaultAsync(ct);
                return offering is null ? null : View(kind, offering.Id, offering.Name, offering.ShortDescription, offering.Text, offering.PublishStatus, audience);
            case ContentRecordKind.PortfolioItem:
                var item = await _db.PortfolioItems.AsNoTracking()
                    .Where(p => p.Slug == slug && statuses.Contains(p.PublishStatus))
                    .Select(p => new { p.Id, p.Title, p.ShortDescription, p.Text, p.PublishStatus })
                    .SingleOrDefaultAsync(ct);
                return item is null ? null : View(kind, item.Id, item.Title, item.ShortDescription, item.Text, item.PublishStatus, audience);
            case ContentRecordKind.CustomPage:
                var page = await _db.CustomPages.AsNoTracking()
                    .Where(c => c.Slug == slug && statuses.Contains(c.PublishStatus))
                    .Select(c => new { c.Id, c.Title, c.Text, c.PublishStatus })
                    .SingleOrDefaultAsync(ct);
                return page is null ? null : View(kind, page.Id, page.Title, null, page.Text, page.PublishStatus, audience);
            default:
                throw new ArgumentOutOfRangeException(nameof(kind), kind, "Only a service introduction, a reference work and a general page are found by an address.");
        }
    }

    private static ContentPageView View(ContentRecordKind kind, Guid id, string? title, string? summary, string? body, PublishStatus status, ContentAudience audience) =>
        new(kind, id, title, summary, body, IsDraft: ContentVisibility.Decide(status, audience) == ContentShown.ShownWithDraftBand);
}
