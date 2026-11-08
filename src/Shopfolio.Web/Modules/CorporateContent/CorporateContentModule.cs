using Microsoft.Extensions.DependencyInjection;

using Shopfolio.Infrastructure.Disposal;
using Shopfolio.Modules.CorporateContent.Contracts;

namespace Shopfolio.Modules.CorporateContent;

/// <summary>The CorporateContent module's implementation of its open surface; internal, reached only through <see cref="ICorporateContentModule"/>.</summary>
internal sealed class CorporateContentModule : ICorporateContentModule
{
    private readonly CorporateContentSitemapReader _sitemap;

    public CorporateContentModule(CorporateContentSitemapReader sitemap)
    {
        _sitemap = sitemap;
    }

    public Task<IReadOnlyList<string>> ListSitemapPathsAsync(CancellationToken ct) => _sitemap.ListAsync(ct);
}

/// <summary>The module's single registration (09 6 item 3): <c>Program.cs</c> calls <c>AddCorporateContentModule</c> once.</summary>
internal static class CorporateContentModuleRegistration
{
    public static IServiceCollection AddCorporateContentModule(this IServiceCollection services)
    {
        services.AddScoped<CorporateContentSitemapReader>();
        services.AddScoped<ContentPublishing>();
        services.AddScoped<ContactRequests>();
        services.AddDisposalPart<ContactRequestDisposalPart>();
        services.AddScoped<ICorporateContentModule, CorporateContentModule>();
        return services;
    }
}
