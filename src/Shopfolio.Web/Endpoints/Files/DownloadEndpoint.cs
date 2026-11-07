using Microsoft.Net.Http.Headers;

using Shopfolio.Infrastructure.Errors;
using Shopfolio.Infrastructure.Files;
using Shopfolio.Infrastructure.Security;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Ordering.Contracts;

namespace Shopfolio.Endpoints.Files;

/// <summary>
/// UC-37: <c>GET /indir/{signed value}</c> (07 4.31.1; 09 15.7 items 1 to 3; 05 7.7.4; 02 3.12.5, 3.12.7). An open machine endpoint: the signed value carries the order item's id and
/// the end of its life, and no session or order root is looked up. The value, the item and the item's file are read in this order; a value that is not valid (bad
/// signature or past its life), an item that cannot be downloaded and an item whose file is missing or whose upload did not complete are all the same 404 with no body
/// (E-06) - the answer does not say which. The life is checked once, when the request is accepted, and never while the file is sent. The endpoint writes nothing: not the
/// download counter (the right dropped at the order page's button), not the trace. The answer is the framework's file result with range processing (K-884): a strong
/// <c>ETag</c> and <c>Last-Modified</c> made from the file row, an attachment under the safe name, a general binary type and <c>no-store</c>; the other headers are the
/// site's (<c>nosniff</c>, <c>X-Robots-Tag</c>). A file the store cannot read is an unexpected error (500).
/// </summary>
internal static class DownloadEndpoint
{
    public const string Prefix = "/indir";

    public const string Pattern = Prefix + "/{value}";

    public static IEndpointRouteBuilder MapDownloadEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet(Pattern, HandleAsync).WithMetadata(MachineEndpointMetadata.Instance);
        return endpoints;
    }

    private static async Task<IResult> HandleAsync(
        string value,
        DownloadLinks links,
        IOrderingModule ordering,
        ICatalogModule catalog,
        IFileStore files,
        TimeProvider clock,
        RejectionLog rejections,
        HttpContext context,
        CancellationToken ct)
    {
        // The life is read here, once, at the acceptance of the request; the transfer below never looks at it again.
        DownloadLinkResult link = links.Resolve(value, clock);
        if (!link.IsValid)
        {
            return NotFound(rejections);
        }

        DownloadableItem? item = await ordering.FindDownloadableItemAsync(link.OrderItemId, ct);
        if (item is null)
        {
            return NotFound(rejections);
        }

        DigitalFileLocation? file = await catalog.FindDownloadFileAsync(item.VariantId, item.DigitalFileId, ct);
        if (file is null)
        {
            return NotFound(rejections);
        }

        DigitalFileDownload? download = await DigitalFileDelivery.OpenAsync(files, file.StorageKey, file.FileName, ct);
        if (download is null)
        {
            // The row says the file is there and the store has none: not a missing answer but an unexpected state (09 15.7 item 3; 09 8.1).
            throw new InvalidOperationException("A completed digital file row has no file in the store.");
        }

        context.Response.Headers.CacheControl = "no-store";
        EntityTagHeaderValue tag = new("\"" + DownloadEntityTag.Value(file.StorageKey, file.SizeBytes) + "\"", isWeak: false);

        // The framework disposes the stream when the answer is done. The stream is seekable (IFileStore.OpenReadAsync), so a range is served from it.
        return Results.File(
            download.Content,
            DigitalFileDelivery.ContentType,
            fileDownloadName: download.FileName,
            lastModified: file.UploadCompletedAt,
            entityTag: tag,
            enableRangeProcessing: true);
    }

    private static IResult NotFound(RejectionLog rejections)
    {
        rejections.Rejected(ErrorCode.DownloadLinkInvalid);
        return Results.NotFound();
    }
}
