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
    private readonly ContentReaders _readers;
    private readonly StorefrontContentPages _contentPages;

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
        StorefrontContentPages contentPages)
    {
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

    public Task<BrandingSection> GetBrandingAsync(CancellationToken ct) => _branding.ReadSectionAsync(ct);

    public Task<BrandingSaveResult> SaveBrandingAsync(BrandingInput input, int formVersion, CancellationToken ct) => _branding.SaveAsync(input, formVersion, ct);

    public Task<BrandIdentity> GetBrandIdentityAsync(CancellationToken ct) => _branding.ReadIdentityAsync(ct);

    public async Task<IReadOnlyList<SocialLinkItem>> ListSocialLinksAsync(CancellationToken ct) =>
        [.. (await _socialLinks.ListAsync(ct)).Select(l => new SocialLinkItem(l.Platform, l.Url))];

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
        services.AddScoped<StorefrontContentPages>();
        services.AddScoped<AnnouncementReader>();
        services.AddScoped<ContentPageReader>();
        services.AddDisposalPart<ContactRequestDisposalPart>();
        services.AddScoped<ICorporateContentModule, CorporateContentModule>();
        return services;
    }
}
