using Microsoft.Extensions.DependencyInjection;

using Shopfolio.Infrastructure.Disposal;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.CorporateContent.Contracts;

namespace Shopfolio.Modules.CorporateContent;

/// <summary>The CorporateContent module's implementation of its open surface; internal, reached only through <see cref="ICorporateContentModule"/>.</summary>
internal sealed class CorporateContentModule : ICorporateContentModule
{
    private readonly CorporateContentSitemapReader _sitemap;
    private readonly StorefrontMenuReader _menu;
    private readonly HomepageReader _homepage;
    private readonly BrandingSettings _branding;
    private readonly AnnouncementReader _announcement;
    private readonly ContactFormSending _contactForm;
    private readonly ContactRequests _requests;
    private readonly ContactRequestExport _contactRequestExport;
    private readonly SocialLinks _socialLinks;
    private readonly ContentReaders _readers;
    private readonly StorefrontContentPages _contentPages;
    private readonly PanelContentEditing _panelContent;

    public CorporateContentModule(
        CorporateContentSitemapReader sitemap,
        StorefrontMenuReader menu,
        HomepageReader homepage,
        BrandingSettings branding,
        AnnouncementReader announcement,
        ContactFormSending contactForm,
        ContactRequests requests,
        ContactRequestExport contactRequestExport,
        SocialLinks socialLinks,
        ContentReaders readers,
        StorefrontContentPages contentPages,
        PanelContentEditing panelContent)
    {
        _panelContent = panelContent;
        _readers = readers;
        _contactRequestExport = contactRequestExport;
        _socialLinks = socialLinks;
        _contentPages = contentPages;
        _contactForm = contactForm;
        _requests = requests;
        _sitemap = sitemap;
        _menu = menu;
        _homepage = homepage;
        _branding = branding;
        _announcement = announcement;
    }

    public Task<AnnouncementView?> GetAnnouncementAsync(bool asAdministrator, CancellationToken ct) =>
        _announcement.ReadAsync(asAdministrator ? ContentAudience.Administrator : ContentAudience.Visitor, ct);

    public Task<IReadOnlyList<string>> ListSitemapPathsAsync(CancellationToken ct) => _sitemap.ListAsync(ct);

    public Task<StorefrontMenu> GetMenuAsync(CancellationToken ct) => _menu.GetAsync(ct);

    public Task<HomepageView> GetHomepageAsync(CancellationToken ct) => _homepage.GetAsync(ct);

    public Task<ContentListView> GetContentListAsync(ContentListKind kind, int page, CancellationToken ct) => _contentPages.ListAsync(kind, page, ct);

    public Task<ContentRecordPage?> GetContentPageAsync(ContentPageKind kind, string? slug, bool asAdministrator, CancellationToken ct) => _contentPages.FindAsync(kind, slug, asAdministrator, ct);

    public Task<PanelContentList> GetPanelContentListAsync(CancellationToken ct) => _panelContent.ListAsync(ct);

    public Task<PanelContentForm?> GetPanelContentFormAsync(PanelContentType type, Guid? id, CancellationToken ct) => _panelContent.GetFormAsync(type, id, ct);

    public Task<Guid?> FindSinglePanelContentAsync(PanelContentType type, CancellationToken ct) => _panelContent.FindSingleAsync(type, ct);

    public Task<PanelContentSaveResult> SavePanelContentAsync(PanelContentSaveCommand command, CancellationToken ct) => _panelContent.SaveAsync(command, ct);

    public Task<PanelContentChangeResult> ChangePanelContentStatusAsync(PanelContentType type, Guid id, PublishStatus target, CancellationToken ct) =>
        _panelContent.ChangeStatusAsync(type, id, target, ct);

    public Task<PanelContentDeleteResult> DeletePanelContentAsync(PanelContentType type, Guid id, CancellationToken ct) => _panelContent.DeleteAsync(type, id, ct);

    public Task<PanelContentReorderResult> ReorderPanelContentAsync(PanelContentType type, Guid id, PanelContentMove move, int shownOrder, int shownNeighbourOrder, CancellationToken ct) =>
        _panelContent.ReorderAsync(type, id, move, shownOrder, shownNeighbourOrder, ct);

    public Task<BrandingSection> GetBrandingAsync(CancellationToken ct) => _branding.ReadSectionAsync(ct);

    public Task<BrandingSaveResult> SaveBrandingAsync(BrandingInput input, int formVersion, CancellationToken ct) => _branding.SaveAsync(input, formVersion, ct);

    public Task<BrandIdentity> GetBrandIdentityAsync(CancellationToken ct) => _branding.ReadIdentityAsync(ct);

    public async Task<IReadOnlyList<SocialLinkItem>> ListSocialLinksAsync(CancellationToken ct) =>
        [.. (await _socialLinks.ListAsync(ct)).Select(l => new SocialLinkItem(l.Platform, l.Url))];

    public async Task<SocialLinksSaveResult> SaveSocialLinksAsync(IReadOnlyDictionary<string, string?> links, CancellationToken ct)
    {
        SocialLinksResult result = await _socialLinks.SaveAsync(links, ct);
        return result.IsRejected
            ? SocialLinksSaveResult.Refused([.. result.Rejections.Select(r => new SocialLinkRejection(r.Field, r.Code))])
            : SocialLinksSaveResult.Saved(result.Changes ?? []);
    }

    public async Task<IReadOnlyList<FaqQuestionView>> GetFaqAsync(bool asAdministrator, CancellationToken ct)
    {
        IReadOnlyList<FaqView> items = asAdministrator ? await _readers.ListFaqForAdministratorAsync(ct) : await _readers.ListFaqAsync(ct);
        return [.. items.Select(f => new FaqQuestionView(f.Id, f.Question, f.AnswerHtml, f.IsDraft))];
    }

    public async Task<ContactPageView> GetContactPageAsync(bool asAdministrator, CancellationToken ct)
    {
        ContactPageContent content = asAdministrator ? await _readers.ReadContactPageForAdministratorAsync(ct) : await _readers.ReadContactPageAsync(ct);
        return new ContactPageView(
            content.Identity,
            [.. content.Branches.Select(b => new ContactBranchView(
                b.Id, b.Name, b.ProvinceName, b.District, b.AddressLine, b.Phone, b.WorkingHours, b.ImageId, b.ImageAltText, b.MapLink?.Url, b.MapLink?.Text, b.IsDraft))]);
    }

    public Task<ContactFormView> GetContactFormAsync(string? memberName, string? memberEmail, CancellationToken ct) => _contactForm.ReadAsync(memberName, memberEmail, ct);

    public Task<ContactFormResult> SendContactRequestAsync(ContactFormInput input, System.Net.IPAddress? ipAddress, CancellationToken ct) => _contactForm.SendAsync(input, ipAddress, ct);

    public Task<int> CountSummaryContactRequestsAsync(DateTimeOffset periodStart, DateTimeOffset periodEnd, CancellationToken ct) => _requests.CountForSummaryAsync(periodStart, periodEnd, ct);

    public Task<int> CountOpenContactRequestsAsync(CancellationToken ct) => _requests.CountOpenAsync(ct);

    public Task<int> CountPanelContactRequestsAsync(IReadOnlyCollection<ContactRequestStatus> statuses, CancellationToken ct) => _requests.CountAsync(statuses, ct);

    public Task<IReadOnlyList<PanelContactRequestRow>> ListPanelContactRequestsAsync(IReadOnlyCollection<ContactRequestStatus> statuses, int take, CancellationToken ct) =>
        _requests.ListAsync(statuses, take, ct);

    public Task<ContactRequestDetail?> FindContactRequestAsync(Guid requestId, CancellationToken ct) => _requests.FindAsync(requestId, ct);

    public async Task<ContactRequestChangeResult> CloseContactRequestAsync(Guid requestId, Guid adminId, CancellationToken ct) =>
        ChangeOf(await _requests.CloseAsync(requestId, adminId, ct));

    public async Task<ContactRequestChangeResult> ReopenContactRequestAsync(Guid requestId, Guid adminId, CancellationToken ct) =>
        ChangeOf(await _requests.ReopenAsync(requestId, adminId, ct));

    private static ContactRequestChangeResult ChangeOf(ContactRequestResult result) => new(result.Outcome switch
    {
        ContactRequestOutcome.Done => ContactRequestChangeOutcome.Changed,
        ContactRequestOutcome.Unchanged => ContactRequestChangeOutcome.Unchanged,
        _ => ContactRequestChangeOutcome.NotFound,
    });

    public Task<int> WriteContactRequestExportAsync(Stream destination, long maxBytes, CancellationToken ct) => _contactRequestExport.WriteAsync(destination, maxBytes, ct);
}

/// <summary>The module's single registration (09 6 item 3): <c>Program.cs</c> calls <c>AddCorporateContentModule</c> once.</summary>
internal static class CorporateContentModuleRegistration
{
    public static IServiceCollection AddCorporateContentModule(this IServiceCollection services)
    {
        services.AddScoped<CorporateContentSitemapReader>();
        services.AddScoped<ContentPublishing>();
        services.AddScoped<ContactRequests>();
        services.AddScoped<ContactFormSending>();
        services.AddScoped<ContactRequestExport>();
        services.AddScoped<BrandingSettings>();
        services.AddScoped<ContentAuthoring>();
        services.AddScoped<SocialLinks>();
        services.AddScoped<ContentReaders>();
        services.AddScoped<ContentProductLinks>();
        services.AddScoped<ContentOrdering>();
        services.AddScoped<ContentGallery>();
        services.AddScoped<PanelContentEditing>();
        services.AddScoped<StorefrontMenuReader>();
        services.AddScoped<HomepageReader>();
        services.AddScoped<StorefrontContentPages>();
        services.AddScoped<AnnouncementReader>();
        services.AddScoped<ContentPageReader>();
        services.AddDisposalPart<ContactRequestDisposalPart>();
        services.AddScoped<ICorporateContentModule, CorporateContentModule>();
        return services;
    }
}
