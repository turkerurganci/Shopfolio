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

    public CorporateContentModule(CorporateContentSitemapReader sitemap, StorefrontMenuReader menu, HomepageReader homepage, BrandingSettings branding)
    {
        _sitemap = sitemap;
        _menu = menu;
        _homepage = homepage;
        _branding = branding;
    }

    public Task<IReadOnlyList<string>> ListSitemapPathsAsync(CancellationToken ct) => _sitemap.ListAsync(ct);

    public Task<StorefrontMenu> GetMenuAsync(CancellationToken ct) => _menu.GetAsync(ct);

    public Task<HomepageView> GetHomepageAsync(CancellationToken ct) => _homepage.GetAsync(ct);

    public Task<BrandingSection> GetBrandingAsync(CancellationToken ct) => _branding.ReadSectionAsync(ct);

    public Task<BrandingSaveResult> SaveBrandingAsync(BrandingInput input, int formVersion, CancellationToken ct) => _branding.SaveAsync(input, formVersion, ct);

    public Task<BrandIdentity> GetBrandIdentityAsync(CancellationToken ct) => _branding.ReadIdentityAsync(ct);
}

/// <summary>The module's single registration (09 6 item 3): <c>Program.cs</c> calls <c>AddCorporateContentModule</c> once.</summary>
internal static class CorporateContentModuleRegistration
{
    public static IServiceCollection AddCorporateContentModule(this IServiceCollection services)
    {
        services.AddScoped<CorporateContentSitemapReader>();
        services.AddScoped<ContentPublishing>();
        services.AddScoped<ContactRequests>();
        services.AddScoped<BrandingSettings>();
        services.AddScoped<ContentAuthoring>();
        services.AddScoped<SocialLinks>();
        services.AddScoped<ContentReaders>();
        services.AddScoped<ContentProductLinks>();
        services.AddScoped<ContentOrdering>();
        services.AddScoped<ContentGallery>();
        services.AddScoped<StorefrontMenuReader>();
        services.AddScoped<HomepageReader>();
        services.AddDisposalPart<ContactRequestDisposalPart>();
        services.AddScoped<ICorporateContentModule, CorporateContentModule>();
        return services;
    }
}
