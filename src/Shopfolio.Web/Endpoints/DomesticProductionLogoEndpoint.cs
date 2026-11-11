using Shopfolio.Infrastructure.Errors;
using Shopfolio.Modules.Catalog.Contracts.Pricing;

namespace Shopfolio.Endpoints;

/// <summary>
/// The domestic production logo file (T501; 07 2.1.2, 3.1.6; 08 4.3; 09 11.1 item 7): <c>GET /statik/yerli-uretim-logosu.{hash}.jpg</c>. Static file serving without a handler of
/// the application's own: the bytes are the assembly's embedded resource (<see cref="DomesticProductionLogo"/>), the address carries a hash of the content like the files of
/// <c>wwwroot</c>, so the answer is cached for good under that address and a replaced file gets a new one. No setting, cookie or session is read and no validator header is
/// written (07 2.4.7): the address is the validator.
/// </summary>
internal static class DomesticProductionLogoEndpoint
{
    private const string CacheControl = "public, max-age=31536000, immutable";

    public static IEndpointRouteBuilder MapDomesticProductionLogoEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet(DomesticProductionLogo.Address, Handle).WithMetadata(MachineEndpointMetadata.Instance);
        return endpoints;
    }

    private static IResult Handle(HttpContext context)
    {
        context.Response.Headers.CacheControl = CacheControl;
        return Results.Bytes(DomesticProductionLogo.Content.ToArray(), DomesticProductionLogo.ContentType);
    }
}
