using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Validation;
using Shopfolio.Modules.Catalog.Contracts;
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
    MapLinkView? MapLink);

/// <summary>The "Haritada aç" link of a branch: the address of the map the firm gave and the text the page shows. The map opens elsewhere; nothing is embedded.</summary>
internal sealed record MapLinkView(string Url, string Text);

/// <summary>A question as the one list shows it (02 3.27.6).</summary>
internal sealed record FaqView(Guid Id, string Question, string? AnswerHtml);

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

    /// <summary>The published questions on the one FAQ page, in the firm's order (ties by the time they were created).</summary>
    public async Task<IReadOnlyList<FaqView>> ListFaqAsync(CancellationToken ct)
    {
        return await _db.FaqItems.AsNoTracking()
            .Where(f => f.PublishStatus == PublishStatus.Published)
            .OrderBy(f => f.SortOrder).ThenBy(f => f.CreatedAt).ThenBy(f => f.Id)
            .Select(f => new FaqView(f.Id, f.Question, f.Answer))
            .ToListAsync(ct);
    }

    /// <summary>The contact page: the identity, the published branches in the firm's order, and the form.</summary>
    public async Task<ContactPageContent> ReadContactPageAsync(CancellationToken ct)
    {
        LegalIdentity identity = await _settings.GetLegalIdentityAsync(ct);
        var rows = await _db.Branches.AsNoTracking()
            .Where(b => b.PublishStatus == PublishStatus.Published)
            .OrderBy(b => b.SortOrder).ThenBy(b => b.CreatedAt).ThenBy(b => b.Id)
            .Select(b => new { b.Id, b.Name, b.ProvinceId, b.District, b.AddressLine, b.Phone, b.WorkingHours, b.MapUrl })
            .ToListAsync(ct);
        Guid[] provinceIds = [.. rows.Where(r => r.ProvinceId is not null).Select(r => r.ProvinceId!.Value).Distinct()];
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
                MapLinkOf(r.MapUrl))),
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
