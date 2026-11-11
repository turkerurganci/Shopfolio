using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Routing;
using Shopfolio.Infrastructure.Validation;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.CorporateContent.Contracts;
using Shopfolio.Modules.Settings.Contracts;

using FieldRejection = Shopfolio.Infrastructure.Validation.FieldRejection;

namespace Shopfolio.Modules.CorporateContent;

/// <summary>A branch as the contact page lists it (02 3.27.7): the map link carries the fixed text and is null when the branch has none.</summary>
internal sealed record BranchView(
    Guid Id,
    string Name,
    string? ProvinceName,
    string? District,
    string? AddressLine,
    string? Phone,
    string? WorkingHours,
    MapLinkView? MapLink,
    bool IsDraft = false);

/// <summary>The "Haritada aç" link of a branch: the address of the map the firm gave and the text the page shows. The map opens elsewhere; nothing is embedded.</summary>
internal sealed record MapLinkView(string Url, string Text);

/// <summary>One page of a content list page's cards (04 5.7.3): the service introductions or the reference works in the firm's order, 24 a page.</summary>
internal sealed record ContentCardPage(IReadOnlyList<HomepageCard> Cards, int PageNumber, int PageSize, int TotalCount, int PageCount);

/// <summary>A question as the one list shows it (02 3.27.6).</summary>
internal sealed record FaqView(Guid Id, string Question, string? AnswerHtml, bool IsDraft = false);

/// <summary>
/// The contact page (02 3.27.8): not a record but a composition - the contact details and legal identity of the company, the published branches in the firm's order, and the contact form
/// that is on the page. The firm enters no text of its own for it.
/// </summary>
internal sealed record ContactPageContent(LegalIdentity Identity, IReadOnlyList<BranchView> Branches, bool HasContactForm);

/// <summary>
/// The reads of the visitor-facing corporate lists (T358; 02 3.27.6, 3.27.7, 3.27.8, 3.27.11): the questions in one list in the firm's order - a heading to group them under does not
/// exist - and the contact page composed from the identity, the branches and the form. Only published records are listed; a draft is never shown to a visitor (02 3.27.25). Plain
/// reads; they write nothing.
/// </summary>
internal sealed class ContentReaders
{
    private readonly AppDbContext _db;
    private readonly ISettingsModule _settings;
    private readonly IProvinceList _provinces;

    public ContentReaders(AppDbContext db, ISettingsModule settings, IProvinceList provinces)
    {
        ArgumentNullException.ThrowIfNull(db);
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(provinces);
        _db = db;
        _settings = settings;
        _provinces = provinces;
    }

    /// <summary>
    /// A page of the published service introductions, in the firm's manual order (T359; 02 3.27.11; 04 5.7.3): 24 a page, ties by the time they were created and the key. A draft is not on the
    /// list. Each card carries the image marked main (or the first of the gallery's order).
    /// </summary>
    public async Task<ContentCardPage> ListOfferingsAsync(int page, CancellationToken ct)
    {
        PagedList<Offering> result = await _db.Offerings.AsNoTracking()
            .Where(o => o.PublishStatus == PublishStatus.Published)
            .OrderBy(o => o.SortOrder).ThenBy(o => o.CreatedAt).ThenBy(o => o.Id)
            .ToPageAsync(page, PageSizes.ContentList, ct);
        Guid[] ids = [.. result.Items.Select(o => o.Id)];
        Dictionary<Guid, ContentImage> images = ContentMainImages.Of(
            await _db.OfferingImages.AsNoTracking()
                .Where(i => ids.Contains(i.OfferingId))
                .Select(i => new ContentGalleryRow(i.OfferingId, i.ImageId, i.IsMain, i.SortOrder, i.CreatedAt, i.AltText))
                .ToListAsync(ct));
        return new ContentCardPage(
            [.. result.Items.Select(o => new HomepageCard(o.Id, o.Name, o.ShortDescription, AddressRules.RecordAddress(AddressRules.ServicePrefix, o.Slug), images.GetValueOrDefault(o.Id)))],
            result.PageNumber,
            result.PageSize,
            result.TotalCount,
            result.PageCount);
    }

    /// <summary>A page of the published reference works, in the firm's manual order (T359; 02 3.27.11; 04 5.7.3): 24 a page, like <see cref="ListOfferingsAsync"/>.</summary>
    public async Task<ContentCardPage> ListPortfolioItemsAsync(int page, CancellationToken ct)
    {
        PagedList<PortfolioItem> result = await _db.PortfolioItems.AsNoTracking()
            .Where(p => p.PublishStatus == PublishStatus.Published)
            .OrderBy(p => p.SortOrder).ThenBy(p => p.CreatedAt).ThenBy(p => p.Id)
            .ToPageAsync(page, PageSizes.ContentList, ct);
        Guid[] ids = [.. result.Items.Select(p => p.Id)];
        Dictionary<Guid, ContentImage> images = ContentMainImages.Of(
            await _db.PortfolioItemImages.AsNoTracking()
                .Where(i => ids.Contains(i.PortfolioItemId))
                .Select(i => new ContentGalleryRow(i.PortfolioItemId, i.ImageId, i.IsMain, i.SortOrder, i.CreatedAt, i.AltText))
                .ToListAsync(ct));
        return new ContentCardPage(
            [.. result.Items.Select(p => new HomepageCard(p.Id, p.Title, p.ShortDescription, AddressRules.RecordAddress(AddressRules.ReferencePrefix, p.Slug), images.GetValueOrDefault(p.Id)))],
            result.PageNumber,
            result.PageSize,
            result.TotalCount,
            result.PageCount);
    }

    /// <summary>The published questions on the one FAQ page, in the firm's order (ties by the time they were created).</summary>
    public Task<IReadOnlyList<FaqView>> ListFaqAsync(CancellationToken ct) => QueryFaqAsync(ContentAudience.Visitor, ct);

    /// <summary>
    /// The FAQ page as the signed-in administrator previews it (02 3.27.25): the draft questions stand in their place in the one list, each marked <see cref="FaqView.IsDraft"/>
    /// for the "Taslak" band. A visitor's list is <see cref="ListFaqAsync(CancellationToken)"/>.
    /// </summary>
    public Task<IReadOnlyList<FaqView>> ListFaqForAdministratorAsync(CancellationToken ct) => QueryFaqAsync(ContentAudience.Administrator, ct);

    /// <summary>The contact page: the identity, the published branches in the firm's order, and the form.</summary>
    public Task<ContactPageContent> ReadContactPageAsync(CancellationToken ct) => QueryContactPageAsync(ContentAudience.Visitor, ct);

    /// <summary>The contact page as the signed-in administrator previews it (02 3.27.25): the draft branches stand in their place, each marked <see cref="BranchView.IsDraft"/>.</summary>
    public Task<ContactPageContent> ReadContactPageForAdministratorAsync(CancellationToken ct) => QueryContactPageAsync(ContentAudience.Administrator, ct);

    private async Task<IReadOnlyList<FaqView>> QueryFaqAsync(ContentAudience audience, CancellationToken ct)
    {
        PublishStatus[] statuses = ContentVisibility.VisibleStatuses(audience);
        return await _db.FaqItems.AsNoTracking()
            .Where(f => statuses.Contains(f.PublishStatus))
            .OrderBy(f => f.SortOrder).ThenBy(f => f.CreatedAt).ThenBy(f => f.Id)
            .Select(f => new FaqView(f.Id, f.Question, f.Answer, f.PublishStatus == PublishStatus.Draft))
            .ToListAsync(ct);
    }

    private async Task<ContactPageContent> QueryContactPageAsync(ContentAudience audience, CancellationToken ct)
    {
        LegalIdentity identity = await _settings.GetLegalIdentityAsync(ct);
        PublishStatus[] statuses = ContentVisibility.VisibleStatuses(audience);
        var rows = await _db.Branches.AsNoTracking()
            .Where(b => statuses.Contains(b.PublishStatus))
            .OrderBy(b => b.SortOrder).ThenBy(b => b.CreatedAt).ThenBy(b => b.Id)
            .Select(b => new { b.Id, b.Name, b.ProvinceId, b.District, b.AddressLine, b.Phone, b.WorkingHours, b.MapUrl, b.PublishStatus })
            .ToListAsync(ct);
        Guid[] provinceIds = [.. rows.Where(r => r.ProvinceId is not null).Select(r => r.ProvinceId!.Value).Distinct()]; // non-null: the Where on this line keeps only branches with a province
        IReadOnlyDictionary<Guid, string> names = await _provinces.GetNamesAsync(provinceIds, ct);
        List<BranchView> branches =
        [
            .. rows.Select(r => new BranchView(
                r.Id,
                r.Name,
                r.ProvinceId is { } id && names.TryGetValue(id, out string? name) ? name : null,
                r.District,
                r.AddressLine,
                r.Phone,
                r.WorkingHours,
                MapLinkOf(r.MapUrl),
                r.PublishStatus == PublishStatus.Draft)),
        ];
        return new ContactPageContent(identity, branches, HasContactForm: true);
    }

    /// <summary>The map link of a branch: the link with the fixed text when the firm gave an address of the form of a link, else nothing.</summary>
    public static MapLinkView? MapLinkOf(string? mapUrl)
    {
        string? url = ContentInputRules.Text(mapUrl);
        List<FieldRejection> rejections = [];
        ContentInputRules.Apply(rejections, nameof(Branch.MapUrl), url, new InvalidUrlAttribute(), FieldCodes.InvalidUrl);
        return url is null || rejections.Count > 0 ? null : new MapLinkView(url, ContentPresentation.BranchMapLinkText);
    }
}
