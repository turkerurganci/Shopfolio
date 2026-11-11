using System.Text.Json;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Binding;
using Shopfolio.Infrastructure.Errors;
using Shopfolio.Infrastructure.Files;
using Shopfolio.Infrastructure.Security;
using Shopfolio.Modules.Catalog.Contracts;

namespace Shopfolio.Endpoints.Files;

/// <summary>
/// UC-83: the chunked upload of a digital product file (07 4.31.2; 09 15.7 items 4 to 7; 05 7.7.3): <c>POST /panel/yukleme</c> (start), <c>POST /panel/yukleme/{id}</c>
/// (chunk) and <c>GET /panel/yukleme/{id}</c> (status). Machine endpoints under the panel: the administrator's session first (a request without one is sent to the
/// panel's sign-in and writes nothing), then, on the two POSTs, the forgery token from the <c>RequestVerificationToken</c> header (400 with no body). The bodies are
/// small JSON of <see cref="MachineJson"/>; 400, 404 and 413 have no body. The chunk is measured here, in the order of 07 4.31.2 - session, 404 (unknown upload), 413, the
/// <c>Content-Range</c> 400, then the module's 409 and overflow 400 under the row's lock - because a ceiling applied in front of the endpoint would answer 413 before the
/// session and before the 404 (the body ceiling middleware leaves the chunk class to this endpoint). The work is in the module (<see cref="ICatalogModule"/>); this class opens
/// the transaction (09 9.1) and writes the answers. An upload is never written to the trace.
/// </summary>
internal static class UploadEndpoints
{
    public static IEndpointRouteBuilder MapUploadEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost(AntiforgeryExemptions.UploadStartPath, StartAsync)
            .WithMetadata(MachineEndpointMetadata.Instance)
            .RequireAdminSession()
            .RequireHeaderForgeryToken();
        endpoints.MapPost(AntiforgeryExemptions.UploadChunkPath, ChunkAsync)
            .WithMetadata(MachineEndpointMetadata.Instance, new RequestBodyClassAttribute(RequestBodyClass.Chunk))
            .RequireAdminSession()
            .RequireHeaderForgeryToken();
        endpoints.MapGet(AntiforgeryExemptions.UploadChunkPath, StatusAsync)
            .WithMetadata(MachineEndpointMetadata.Instance)
            .RequireAdminSession();
        return endpoints;
    }

    private static async Task<IResult> StartAsync(
        HttpContext context,
        ITransactionRunner transactions,
        ICatalogModule catalog,
        RejectionLog rejections,
        CancellationToken ct)
    {
        context.Response.Headers.CacheControl = "no-store";
        StartUploadRequest? request;
        try
        {
            request = await JsonSerializer.DeserializeAsync<StartUploadRequest>(context.Request.Body, MachineJson.Options, ct);
        }
        catch (JsonException)
        {
            // A body that is not the JSON of the model - a byte count written as text included - is an invalid start (07 2.9.6).
            request = null;
        }

        if (request is null)
        {
            return Refuse(rejections, ErrorCode.UploadStartInvalid, StatusCodes.Status400BadRequest);
        }

        UploadStartResult result = await transactions.RunAsync(cancel => catalog.StartUploadAsync(request.FileName, request.SizeBytes, cancel), ct);
        if (!result.IsRejected)
        {
            return Json(
                new UploadStatusResponse { Id = result.Id!.Value, ReceivedBytes = 0, ChunkSizeBytes = DigitalFileDelivery.UploadChunkSizeBytes, IsCompleted = false }, // non-null: a result that is not rejected is Started, which always carries the id
                StatusCodes.Status201Created);
        }

        if (ReferenceEquals(result, UploadStartResult.InvalidRequest))
        {
            return Refuse(rejections, ErrorCode.UploadStartInvalid, StatusCodes.Status400BadRequest);
        }

        rejections.Rejected(ErrorCode.UploadRejected);
        return Json(new UploadRejectedResponse { MaxSizeBytes = ProductConstants.DigitalFileMaxBytes }, StatusCodes.Status422UnprocessableEntity);
    }

    private static async Task<IResult> ChunkAsync(
        string id,
        HttpContext context,
        ITransactionRunner transactions,
        ICatalogModule catalog,
        RejectionLog rejections,
        CancellationToken ct)
    {
        context.Response.Headers.CacheControl = "no-store";

        // 404: the upload is unknown (or was swept). The identity is the row's key; a value that is no key names no upload. Nothing of the body has been read yet.
        if (!Guid.TryParseExact(id, "D", out Guid uploadId) || await catalog.GetUploadStatusAsync(uploadId, ct) is null)
        {
            return Refuse(rejections, ErrorCode.UploadNotFound, StatusCodes.Status404NotFound);
        }

        // 413: a declared length above the chunk is refused without a byte read; a body with no declared length is read to the ceiling plus one byte, into memory.
        HttpRequest request = context.Request;
        Stream body = request.Body;
        long length;
        if (request.ContentLength is long declared)
        {
            if (declared > RequestBodyLimits.ChunkMaxBytes)
            {
                await RequestBodyLimitMiddleware.RefuseAsync(context, context.GetEndpoint());
                return Results.Empty;
            }

            length = declared;
        }
        else
        {
            MemoryStream? buffered = await ReadWithinAsync(body, RequestBodyLimits.ChunkMaxBytes, ct);
            if (buffered is null)
            {
                await RequestBodyLimitMiddleware.RefuseAsync(context, context.GetEndpoint());
                return Results.Empty;
            }

            body = buffered;
            length = buffered.Length;
        }

        // 400: the Content-Range header is missing or malformed, or says another length than the body has (07 4.31.2; the total part is ignored).
        if (!ChunkRange.TryParse(request.Headers.ContentRange, length, out long start))
        {
            return Refuse(rejections, ErrorCode.UploadContentRangeInvalid, StatusCodes.Status400BadRequest);
        }

        // 409 and the overflow 400 are decided under the row's lock, inside the module, in the transaction this entry point opens (09 9.1 item 3).
        UploadChunkResult result = await transactions.RunAsync(cancel => catalog.ReceiveChunkAsync(uploadId, start, body, length, cancel), ct);
        if (!result.IsRejected)
        {
            return Json(new UploadStatusResponse { Id = uploadId, ReceivedBytes = result.ReceivedBytes, IsCompleted = result.IsCompleted }, StatusCodes.Status200OK);
        }

        if (ReferenceEquals(result, UploadChunkResult.NotFound))
        {
            return Refuse(rejections, ErrorCode.UploadNotFound, StatusCodes.Status404NotFound);
        }

        if (ReferenceEquals(result, UploadChunkResult.Overflow))
        {
            return Refuse(rejections, ErrorCode.UploadSizeExceeded, StatusCodes.Status400BadRequest);
        }

        rejections.Rejected(ErrorCode.UploadOffsetMismatch);
        return Json(new UploadStatusResponse { Id = uploadId, ReceivedBytes = result.ReceivedBytes, IsCompleted = result.IsCompleted }, StatusCodes.Status409Conflict);
    }

    private static async Task<IResult> StatusAsync(string id, HttpContext context, ICatalogModule catalog, RejectionLog rejections, CancellationToken ct)
    {
        context.Response.Headers.CacheControl = "no-store";
        if (!Guid.TryParseExact(id, "D", out Guid uploadId) || await catalog.GetUploadStatusAsync(uploadId, ct) is not { } status)
        {
            return Refuse(rejections, ErrorCode.UploadNotFound, StatusCodes.Status404NotFound);
        }

        return Json(new UploadStatusResponse { Id = status.Id, ReceivedBytes = status.ReceivedBytes, IsCompleted = status.IsCompleted }, StatusCodes.Status200OK);
    }

    private static IResult Json<T>(T value, int statusCode) => Results.Json(value, MachineJson.Options, statusCode: statusCode);

    private static IResult Refuse(RejectionLog rejections, ErrorCode code, int statusCode)
    {
        rejections.Rejected(code);
        return Results.StatusCode(statusCode);
    }

    // Reads at most limit bytes; null when the body is longer. A server that throws its own 413 while reading is the same answer.
    private static async Task<MemoryStream?> ReadWithinAsync(Stream body, long limit, CancellationToken ct)
    {
        MemoryStream buffer = new();
        byte[] block = new byte[16 * 1024];
        try
        {
            int read;
            while ((read = await body.ReadAsync(block, ct)) > 0)
            {
                if (buffer.Length + read > limit)
                {
                    await buffer.DisposeAsync();
                    return null;
                }

                buffer.Write(block, 0, read);
            }
        }
        catch (BadHttpRequestException exception) when (exception.StatusCode == StatusCodes.Status413PayloadTooLarge)
        {
            await buffer.DisposeAsync();
            return null;
        }

        buffer.Position = 0;
        return buffer;
    }
}
