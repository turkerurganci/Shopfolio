using Microsoft.Extensions.DependencyInjection;

using Shopfolio.Infrastructure.Disposal;
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

    public CorporateContentModule(
        CorporateContentSitemapReader sitemap,
        StorefrontMenuReader menu,
        HomepageReader homepage,
        BrandingSettings branding,
        AnnouncementReader announcement,
        ContactFormSending contactForm,
        ContactRequests requests,
        ContactRequestExport contactRequestExport,
        SocialLinks socialLinks)
    {
        _contactRequestExport = contactRequestExport;
        _socialLinks = socialLinks;
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

    public Task<BrandingSection> GetBrandingAsync(CancellationToken ct) => _branding.ReadSectionAsync(ct);

    public Task<BrandingSaveResult> SaveBrandingAsync(BrandingInput input, int formVersion, CancellationToken ct) => _branding.SaveAsync(input, formVersion, ct);

    public Task<BrandIdentity> GetBrandIdentityAsync(CancellationToken ct) => _branding.ReadIdentityAsync(ct);

    public async Task<IReadOnlyList<SocialLinkItem>> ListSocialLinksAsync(CancellationToken ct) =>
        [.. (await _socialLinks.ListAsync(ct)).Select(l => new SocialLinkItem(l.Platform, l.Url))];

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
        services.AddScoped<StorefrontMenuReader>();
        services.AddScoped<HomepageReader>();
        services.AddScoped<AnnouncementReader>();
        services.AddScoped<ContentPageReader>();
        services.AddDisposalPart<ContactRequestDisposalPart>();
        services.AddScoped<ICorporateContentModule, CorporateContentModule>();
        return services;
    }
}
