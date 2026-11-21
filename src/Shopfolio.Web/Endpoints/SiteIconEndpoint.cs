using System.Text;

using Shopfolio.Infrastructure.Errors;
using Shopfolio.Modules.CorporateContent.Contracts;
using Shopfolio.Pages.Frame;

namespace Shopfolio.Endpoints;

/// <summary>
/// The fallback site icon (02 3.29.4; D-1135): <c>GET /gorsel/simge</c>. While the firm has uploaded no site icon, the brand name's first letter stands on the brand colour as an SVG - the
/// layout links it so the browser's own <c>/favicon.ico</c> request never reaches a 404 and no icon of the product ever becomes the site's (K-261). The address lies in the closed image area
/// (07 2.1.2, 3.1.6): no new first segment, no handler of a page, no cookie, no session. When an icon is uploaded the address answers 404 - the icon is served by its file id.
/// </summary>
internal static class SiteIconEndpoint
{
    private const string CacheControl = "public, max-age=300";

    public static IEndpointRouteBuilder MapSiteIconEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet(FrameStyle.SiteIconPath, HandleAsync).WithMetadata(MachineEndpointMetadata.Instance);
        return endpoints;
    }

    private static async Task<IResult> HandleAsync(HttpContext context, ICorporateContentModule content, CancellationToken ct)
    {
        BrandIdentity brand = await content.GetBrandIdentityAsync(ct);
        if (brand.FaviconId is not null)
        {
            return Results.NotFound();
        }

        context.Response.Headers.CacheControl = CacheControl;
        return Results.Text(FrameStyle.FallbackIconSvg(brand.FaviconInitial, brand.Color, brand.TextColor), FrameStyle.SiteIconContentType, new UTF8Encoding(false));
    }
}
