using Shopfolio.Infrastructure.Errors;
using Shopfolio.Infrastructure.Files;
using Shopfolio.Infrastructure.Routing;
using Shopfolio.Modules.Catalog.Contracts;

namespace Shopfolio.Endpoints.Files;

/// <summary>
/// The image address <c>GET /gorsel/{image id}</c> (07 2.1.2, 3.1.6; 05 7.7.1; D-1136, D-1711; K-1631 (1)): the pages write an image by its row's id (<see cref="Pages.Frame.FrameStyle.ImagePath"/>),
/// and this sends the row's source copy from the image section with P-22's media type. A row is one upload and never changes its content, so the answer is cached for good
/// (<c>public, max-age=31536000, immutable</c> - the reverse proxy's header for the fingerprinted files) and a changed image is a new row with a new address. Outside both
/// request limits (<see cref="Infrastructure.Security.RateLimitScope"/>: <c>/gorsel</c>). An id that names no row and a row whose file is not in the store are the same bare
/// 404: an image is not a page. No validator header is written (07 2.4.7, as the domestic production logo): the address is the validator. The handler reads no cookie or
/// setting; the digital file section is never reached (<see cref="FileArea.Image"/> only).
/// </summary>
internal static class ImageEndpoint
{
    public const string Pattern = AddressRules.ImageArea + "/{id:guid}";

    private const string CacheControl = "public, max-age=31536000, immutable";

    public static IEndpointRouteBuilder MapImageEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet(Pattern, HandleAsync).WithMetadata(MachineEndpointMetadata.Instance);
        return endpoints;
    }

    private static async Task<IResult> HandleAsync(Guid id, ICatalogModule catalog, IFileStore files, HttpContext context, CancellationToken ct)
    {
        ServedImage? image = await catalog.FindServedImageAsync(id, ct);
        if (image is null || !await files.ExistsAsync(FileArea.Image, image.StorageKey, ct))
        {
            return Results.NotFound();
        }

        context.Response.Headers.CacheControl = CacheControl;

        // The framework disposes the stream when the answer is done.
        Stream content = await files.OpenReadAsync(FileArea.Image, image.StorageKey, ct);
        return Results.File(content, image.ContentType);
    }
}
