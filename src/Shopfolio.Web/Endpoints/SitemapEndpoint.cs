using System.Text;

using Shopfolio.Infrastructure.Errors;
using Shopfolio.Infrastructure.Routing;
using Shopfolio.Infrastructure.SearchEngine;
using Shopfolio.Modules.Settings.Contracts;

namespace Shopfolio.Endpoints;

/// <summary>
/// UC-79: <c>GET /sitemap.xml</c> and <c>GET /robots.txt</c> (07 4.32.1; 05 3.7.3, 3.7.4; 02 3.30.5). Open machine endpoints for the search engines: no signature, no
/// cookie, no session, no parameter. The site map is made on request from the published records and comes from the cache (<see cref="ISitemapSource"/>); the robots
/// file names the site map with the installation's domain and blocks no page - the closed pages are closed by <c>noindex</c>, not here. Neither answer is closed:
/// they are what the search engines read. The two are minimal endpoints, not pages, so the page handler's error pattern does not touch them (09 4.1, 8.1 item 3).
/// </summary>
internal static class SitemapEndpoint
{
    public const string SitemapPath = "/" + AddressRules.SitemapFile;

    public const string RobotsPath = "/" + AddressRules.RobotsFile;

    public static IEndpointRouteBuilder MapSitemapEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet(SitemapPath, SitemapAsync).WithMetadata(MachineEndpointMetadata.Instance);
        endpoints.MapGet(RobotsPath, Robots).WithMetadata(MachineEndpointMetadata.Instance);
        return endpoints;
    }

    private static async Task<IResult> SitemapAsync(ISitemapSource source, SiteAddress site, CancellationToken ct)
    {
        SitemapView view = await source.GetAsync(ct);
        return Results.Text(SiteMapFiles.Xml(site, view.Paths), "application/xml", new UTF8Encoding(false));
    }

    private static IResult Robots(SiteAddress site)
    {
        return Results.Text(SiteMapFiles.Robots(site), "text/plain", new UTF8Encoding(false));
    }
}
