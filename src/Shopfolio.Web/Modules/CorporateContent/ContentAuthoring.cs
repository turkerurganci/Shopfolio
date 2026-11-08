using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Routing;
using Shopfolio.Infrastructure.Validation;
using Shopfolio.Modules.Settings.Contracts;

using FieldRejection = Shopfolio.Infrastructure.Validation.FieldRejection;

namespace Shopfolio.Modules.CorporateContent;

/// <summary>What became of a corporate content entry (T358).</summary>
internal enum AuthoringOutcome
{
    /// <summary>A new record was added as a draft; the caller's transaction commits it.</summary>
    Created,

    /// <summary>The edit was saved (the version rose).</summary>
    Saved,

    /// <summary>A value is wrong; <see cref="AuthoringResult.Rejections"/> names the fields. Nothing was written.</summary>
    Rejected,

    /// <summary>The about page is there already: there is one, never a second (02 3.27.3).</summary>
    AlreadyExists,

    NotFound,

    /// <summary>The record is no longer at the version the form carried (05 7.3.1).</summary>
    VersionConflict,

    /// <summary>The edit of a published record would empty a gate field; it is not saved (02 3.27.24; <see cref="AuthoringResult.Missing"/>).</summary>
    PublishedGateBroken,
}

/// <summary>The result of a corporate content entry; a refusal is a return value and the transaction runner rolls back (09 9.1 item 3).</summary>
internal sealed record AuthoringResult(
    AuthoringOutcome Outcome,
    IReadOnlyList<FieldRejection> Rejections,
    IReadOnlyList<ContentGateField> Missing,
    Guid? Id = null,
    int? Version = null) : IOperationResult
{
    public bool IsRejected => Outcome is not (AuthoringOutcome.Created or AuthoringOutcome.Saved);

    public static AuthoringResult Of(AuthoringOutcome outcome, Guid? id = null, int? version = null) => new(outcome, [], [], id, version);

    public static AuthoringResult Refused(IReadOnlyList<FieldRejection> rejections) => new(AuthoringOutcome.Rejected, rejections, []);
}

/// <summary>
/// The entry of the corporate content (T358; 02 3.27.1-3.27.9): the ready-made types and the general page are filled in through fixed forms - one method per type, no page
/// builder. A new record is born a draft (02 3.27.23) and needs only its name, title or question to be saved (02 3.27.24); the gate is the publish step's (<see cref="ContentPublishing"/>).
/// The about page is one row: its first save creates it, a second is refused, and it is never deleted. A record's page address is made from its name on the server when the record is
/// born (09 9.4 item 3; the address is fixed at the first publication by T363). The long texts go through the cleaner of the closed format set in the entity's setter; the short plain
/// texts are stored as typed. Every method joins the caller's transaction and never opens one, and a new record is only added - the owner of the transaction saves it (09 9.1 item 3).
/// An edit goes through <see cref="ContentPublishing.SaveEditAsync{TRecord}"/>, so the version check and the rule that an edit may not break the gate of a published record are that
/// method's.
/// </summary>
internal sealed class ContentAuthoring
{
    private readonly AppDbContext _db;
    private readonly ContentPublishing _publishing;
    private readonly IProvinceList _provinces;
    private readonly TimeProvider _time;

    public ContentAuthoring(AppDbContext db, ContentPublishing publishing, IProvinceList provinces, TimeProvider time)
    {
        ArgumentNullException.ThrowIfNull(db);
        ArgumentNullException.ThrowIfNull(publishing);
        ArgumentNullException.ThrowIfNull(provinces);
        ArgumentNullException.ThrowIfNull(time);
        _db = db;
        _publishing = publishing;
        _provinces = provinces;
        _time = time;
    }

    /// <summary>
    /// The about page's first save (02 3.27.3): the one row is born as a draft, with whatever the form holds - an empty about page is saved too (02 3.27.24). A second one is
    /// <see cref="AuthoringOutcome.AlreadyExists"/>; the single-row index of the table is the guarantee when two first saves arrive together.
    /// </summary>
    public async Task<AuthoringResult> CreateAboutPageAsync(AboutPageInput input, CancellationToken ct)
    {
        List<FieldRejection> rejections = ContentInputRules.Check(input);
        if (await _db.AboutPages.AnyAsync(ct))
        {
            return AuthoringResult.Of(AuthoringOutcome.AlreadyExists);
        }

        if (rejections.Count > 0)
        {
            return AuthoringResult.Refused(rejections);
        }

        AboutPage page = AboutPage.Create(_time.GetUtcNow());
        ApplyAbout(page, input);
        _db.AboutPages.Add(page);
        return AuthoringResult.Of(AuthoringOutcome.Created, page.Id, page.Version);
    }

    /// <summary>The edit of the about page (02 3.27.3) against the version the form carried.</summary>
    public async Task<AuthoringResult> SaveAboutPageAsync(AboutPageInput input, int formVersion, CancellationToken ct)
    {
        AboutPage? page = await _db.AboutPages.SingleOrDefaultAsync(ct);
        return await EditAsync(page, formVersion, ContentInputRules.Check(input), p => ApplyAbout(p, input), ct);
    }

    /// <summary>A new service introduction (02 3.27.4): a draft with its address made from the name.</summary>
    public async Task<AuthoringResult> CreateOfferingAsync(OfferingInput input, CancellationToken ct)
    {
        List<FieldRejection> rejections = ContentInputRules.Check(input);
        if (rejections.Count > 0)
        {
            return AuthoringResult.Refused(rejections);
        }

        string name = ContentInputRules.Text(input.Name)!;
        string slug = await NewSlugAsync(_db.Offerings.Select(o => o.Slug), name, AddressRules.ServicePrefix, ct);
        int order = (await _db.Offerings.MaxAsync(o => (int?)o.SortOrder, ct) ?? 0) + 1;
        Offering offering = Offering.Create(name, slug, order, _time.GetUtcNow());
        ApplyOffering(offering, input);
        _db.Offerings.Add(offering);
        return AuthoringResult.Of(AuthoringOutcome.Created, offering.Id, offering.Version);
    }

    /// <summary>The edit of a service introduction against the version the form carried.</summary>
    public async Task<AuthoringResult> SaveOfferingAsync(Guid id, OfferingInput input, int formVersion, CancellationToken ct)
    {
        Offering? offering = await _db.Offerings.SingleOrDefaultAsync(o => o.Id == id, ct);
        return await EditAsync(offering, formVersion, ContentInputRules.Check(input), o => ApplyOffering(o, input), ct);
    }

    /// <summary>A new reference work (02 3.27.5): a draft with its address made from the title.</summary>
    public async Task<AuthoringResult> CreatePortfolioItemAsync(PortfolioItemInput input, CancellationToken ct)
    {
        List<FieldRejection> rejections = ContentInputRules.Check(input);
        if (rejections.Count > 0)
        {
            return AuthoringResult.Refused(rejections);
        }

        string title = ContentInputRules.Text(input.Title)!;
        string slug = await NewSlugAsync(_db.PortfolioItems.Select(p => p.Slug), title, AddressRules.ReferencePrefix, ct);
        int order = (await _db.PortfolioItems.MaxAsync(p => (int?)p.SortOrder, ct) ?? 0) + 1;
        PortfolioItem item = PortfolioItem.Create(title, slug, order, _time.GetUtcNow());
        ApplyPortfolio(item, input);
        _db.PortfolioItems.Add(item);
        return AuthoringResult.Of(AuthoringOutcome.Created, item.Id, item.Version);
    }

    /// <summary>The edit of a reference work against the version the form carried.</summary>
    public async Task<AuthoringResult> SavePortfolioItemAsync(Guid id, PortfolioItemInput input, int formVersion, CancellationToken ct)
    {
        PortfolioItem? item = await _db.PortfolioItems.SingleOrDefaultAsync(p => p.Id == id, ct);
        return await EditAsync(item, formVersion, ContentInputRules.Check(input), p => ApplyPortfolio(p, input), ct);
    }

    /// <summary>A new question (02 3.27.6): a draft; it has no page address, and the list is one list - no heading to put it under.</summary>
    public async Task<AuthoringResult> CreateFaqItemAsync(FaqItemInput input, CancellationToken ct)
    {
        List<FieldRejection> rejections = ContentInputRules.Check(input);
        if (rejections.Count > 0)
        {
            return AuthoringResult.Refused(rejections);
        }

        int order = (await _db.FaqItems.MaxAsync(f => (int?)f.SortOrder, ct) ?? 0) + 1;
        FaqItem item = FaqItem.Create(ContentInputRules.Text(input.Question)!, order, _time.GetUtcNow());
        item.SetAnswer(input.Answer);
        _db.FaqItems.Add(item);
        return AuthoringResult.Of(AuthoringOutcome.Created, item.Id, item.Version);
    }

    /// <summary>The edit of a question against the version the form carried.</summary>
    public async Task<AuthoringResult> SaveFaqItemAsync(Guid id, FaqItemInput input, int formVersion, CancellationToken ct)
    {
        FaqItem? item = await _db.FaqItems.SingleOrDefaultAsync(f => f.Id == id, ct);
        return await EditAsync(item, formVersion, ContentInputRules.Check(input), f => ApplyFaq(f, input), ct);
    }

    /// <summary>A new branch (02 3.27.7): a draft with the name alone saved; the province must be on the closed list. A branch has no page address.</summary>
    public async Task<AuthoringResult> CreateBranchAsync(BranchInput input, CancellationToken ct)
    {
        List<FieldRejection> rejections = await CheckBranchAsync(input, ct);
        if (rejections.Count > 0)
        {
            return AuthoringResult.Refused(rejections);
        }

        int order = (await _db.Branches.MaxAsync(b => (int?)b.SortOrder, ct) ?? 0) + 1;
        Branch branch = Branch.Create(ContentInputRules.Text(input.Name)!, order, _time.GetUtcNow());
        ApplyBranch(branch, input);
        _db.Branches.Add(branch);
        return AuthoringResult.Of(AuthoringOutcome.Created, branch.Id, branch.Version);
    }

    /// <summary>The edit of a branch against the version the form carried.</summary>
    public async Task<AuthoringResult> SaveBranchAsync(Guid id, BranchInput input, int formVersion, CancellationToken ct)
    {
        Branch? branch = await _db.Branches.SingleOrDefaultAsync(b => b.Id == id, ct);
        return await EditAsync(branch, formVersion, await CheckBranchAsync(input, ct), b => ApplyBranch(b, input), ct);
    }

    /// <summary>A new general page (02 3.27.9): a draft with its address made from the title; there is no limit on how many.</summary>
    public async Task<AuthoringResult> CreateCustomPageAsync(CustomPageInput input, CancellationToken ct)
    {
        List<FieldRejection> rejections = ContentInputRules.Check(input);
        if (rejections.Count > 0)
        {
            return AuthoringResult.Refused(rejections);
        }

        string title = ContentInputRules.Text(input.Title)!;
        string slug = await NewSlugAsync(_db.CustomPages.Select(c => c.Slug), title, AddressRules.GeneralPagePrefix, ct);
        int order = (await _db.CustomPages.MaxAsync(c => (int?)c.SortOrder, ct) ?? 0) + 1;
        CustomPage page = CustomPage.Create(title, slug, order, _time.GetUtcNow());
        ApplyCustomPage(page, input);
        _db.CustomPages.Add(page);
        return AuthoringResult.Of(AuthoringOutcome.Created, page.Id, page.Version);
    }

    /// <summary>The edit of a general page against the version the form carried.</summary>
    public async Task<AuthoringResult> SaveCustomPageAsync(Guid id, CustomPageInput input, int formVersion, CancellationToken ct)
    {
        CustomPage? page = await _db.CustomPages.SingleOrDefaultAsync(c => c.Id == id, ct);
        return await EditAsync(page, formVersion, ContentInputRules.Check(input), c => ApplyCustomPage(c, input), ct);
    }

    private async Task<List<FieldRejection>> CheckBranchAsync(BranchInput input, CancellationToken ct)
    {
        List<FieldRejection> rejections = ContentInputRules.Check(input);
        if (StoredProvince(input) is Guid province && !await _provinces.ExistsAsync(province, ct))
        {
            rejections.Add(new FieldRejection(nameof(BranchInput.ProvinceId), FieldCodes.InvalidProvince));
        }

        return rejections;
    }

    private async Task<AuthoringResult> EditAsync<TRecord>(
        TRecord? record,
        int formVersion,
        List<FieldRejection> rejections,
        Action<TRecord> apply,
        CancellationToken ct)
        where TRecord : class, IContentRecord
    {
        if (record is null)
        {
            return AuthoringResult.Of(AuthoringOutcome.NotFound);
        }

        if (rejections.Count > 0)
        {
            return AuthoringResult.Refused(rejections);
        }

        ContentResult saved = await _publishing.SaveEditAsync(record, formVersion, apply, ct);
        return saved.Outcome switch
        {
            ContentOutcome.Saved => AuthoringResult.Of(AuthoringOutcome.Saved, record.Id, saved.Version),
            ContentOutcome.VersionConflict => AuthoringResult.Of(AuthoringOutcome.VersionConflict, record.Id),
            ContentOutcome.PublishedGateBroken => new AuthoringResult(AuthoringOutcome.PublishedGateBroken, [], saved.Missing, record.Id),
            _ => throw new InvalidOperationException($"An edit cannot end as {saved.Outcome}."),
        };
    }

    private static void ApplyAbout(AboutPage page, AboutPageInput input)
    {
        page.ShortIntroduction = ContentInputRules.Text(input.ShortIntroduction);
        page.SetLongText(input.LongText);
    }

    private static void ApplyOffering(Offering offering, OfferingInput input)
    {
        offering.Name = ContentInputRules.Text(input.Name)!;
        offering.ShortDescription = ContentInputRules.Text(input.ShortDescription);
        offering.SetText(input.Text);
        offering.IsShownOnHomepage = input.IsShownOnHomepage;
    }

    private static void ApplyPortfolio(PortfolioItem item, PortfolioItemInput input)
    {
        item.Title = ContentInputRules.Text(input.Title)!;
        item.ShortDescription = ContentInputRules.Text(input.ShortDescription);
        item.SetText(input.Text);
        item.IsShownOnHomepage = input.IsShownOnHomepage;
    }

    private static void ApplyFaq(FaqItem item, FaqItemInput input)
    {
        item.Question = ContentInputRules.Text(input.Question)!;
        item.SetAnswer(input.Answer);
    }

    private static void ApplyBranch(Branch branch, BranchInput input)
    {
        branch.Name = ContentInputRules.Text(input.Name)!;
        branch.ProvinceId = StoredProvince(input);
        branch.District = ContentInputRules.Text(input.District);
        branch.AddressLine = ContentInputRules.Text(input.AddressLine);
        branch.Phone = ContentInputRules.Phone(input.Phone);
        branch.WorkingHours = ContentInputRules.Text(input.WorkingHours);
        branch.MapUrl = ContentInputRules.Text(input.MapUrl);
    }

    private static void ApplyCustomPage(CustomPage page, CustomPageInput input)
    {
        page.Title = ContentInputRules.Text(input.Title)!;
        page.SetText(input.Text);
        page.IsShownInMenu = input.IsShownInMenu;
    }

    /// <summary>The province as stored: an empty id is no province.</summary>
    private static Guid? StoredProvince(BranchInput input) => input.ProvinceId == Guid.Empty ? null : input.ProvinceId;

    /// <summary>The free address of a new record (09 9.4 item 3): the name's address, or the first numbered one no row of the table holds (a draft's address counts as used).</summary>
    private static async Task<string> NewSlugAsync(IQueryable<string> slugs, string name, string root, CancellationToken ct)
    {
        string baseSlug = Slug.From(name, root);
        string numbered = baseSlug + "-";
        List<string> used = await slugs.Where(s => s == baseSlug || s.StartsWith(numbered)).ToListAsync(ct);
        return Slug.FirstFree(baseSlug, used);
    }
}
