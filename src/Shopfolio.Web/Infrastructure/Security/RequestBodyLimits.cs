using System.Net;

using Microsoft.AspNetCore.Http.Features;

using Shopfolio.Infrastructure.Errors;
using Shopfolio.Infrastructure.SignedEndpoints;

namespace Shopfolio.Infrastructure.Security;

/// <summary>
/// The classes of the request body ceiling (07 2.8.1; 05 6.3.6; 09 3.5 table). An endpoint belongs to one class and the class has one ceiling; a GET carries
/// no body at all.
/// </summary>
internal enum RequestBodyClass
{
    /// <summary>The general form: 64 KB. The class of every endpoint that declares none.</summary>
    General,

    /// <summary>A panel form with long text - product, corporate content, legal text, company identity: 1 MB.</summary>
    LongText,

    /// <summary>The image upload handler (UC-117): P-23 plus 1 MB.</summary>
    ImageUpload,

    /// <summary>The chunk of the chunked upload (UC-83): the chunk's own size, 5 MB.</summary>
    Chunk,

    /// <summary>The payment provider's notification (UC-80): 64 KB; the signed endpoint's pipeline applies the same number itself.</summary>
    Notification,
}

/// <summary>
/// The ceilings of <see cref="RequestBodyClass"/>, in bytes, in one place (07 2.8.1). The numbers are design values of the document, not settings
/// (09 3.5 item 3), and the reverse proxy applies the same ceiling to the same endpoint (<c>DEPLOY_RUNBOOK</c>). Every number is written here once: the image
/// ceiling reads P-23 (<see cref="ProductConstants.ImageMaxBytes"/>) and the notification ceiling reads the signed endpoint's own constant.
/// </summary>
internal static class RequestBodyLimits
{
    private const long Kilobyte = 1024;

    private const long Megabyte = 1024 * Kilobyte;

    /// <summary>The general form and the start body of the chunked upload: 64 KB.</summary>
    public const long GeneralMaxBytes = 64 * Kilobyte;

    /// <summary>A panel form with long text: 1 MB.</summary>
    public const long LongTextMaxBytes = Megabyte;

    /// <summary>The image upload handler: the image ceiling P-23 and 1 MB for the rest of the form.</summary>
    public const long ImageUploadMaxBytes = ProductConstants.ImageMaxBytes + Megabyte;

    /// <summary>A chunk of the chunked upload: 5 MB.</summary>
    public const long ChunkMaxBytes = 5 * Megabyte;

    /// <summary>The payment provider's notification: 64 KB, the signed endpoint pipeline's number.</summary>
    public const long NotificationMaxBytes = SignedEndpointLimits.BodyMaxBytes;

    public static long Of(RequestBodyClass bodyClass) => bodyClass switch
    {
        RequestBodyClass.General => GeneralMaxBytes,
        RequestBodyClass.LongText => LongTextMaxBytes,
        RequestBodyClass.ImageUpload => ImageUploadMaxBytes,
        RequestBodyClass.Chunk => ChunkMaxBytes,
        RequestBodyClass.Notification => NotificationMaxBytes,
        _ => throw new ArgumentOutOfRangeException(nameof(bodyClass)),
    };

    /// <summary>The ceiling of a request: zero for a GET or a HEAD, the endpoint's class otherwise (the general class when it declares none).</summary>
    public static long Of(string method, RequestBodyClass? declared)
    {
        ArgumentNullException.ThrowIfNull(method);
        return HttpMethods.IsGet(method) || HttpMethods.IsHead(method) ? 0 : Of(declared ?? RequestBodyClass.General);
    }
}

/// <summary>
/// Puts a page or an endpoint into a body ceiling class (07 2.8.1). Without it the endpoint is in the general class, 64 KB; the classes with a larger
/// ceiling are declared on the page model or in the endpoint's metadata by the task that builds the endpoint.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
internal sealed class RequestBodyClassAttribute : Attribute
{
    public RequestBodyClassAttribute(RequestBodyClass bodyClass)
    {
        BodyClass = bodyClass;
    }

    public RequestBodyClass BodyClass { get; }
}

/// <summary>
/// Applies the ceiling of the endpoint's class before the body is read and before the anti-forgery check and the signature (07 2.8.1, 2.8.7; 09 15.4):
/// a declared length above it is answered 413 without reading a byte; a body without a declared length is read no further than one byte past the
/// ceiling into memory, and answered 413 when it is longer. A request that is refused has no form to show again (the body was not read). The server's own
/// ceiling is set to the same value where the server has one. A machine endpoint answers with no body; any other request gets the plain operation-level page.
/// </summary>
internal sealed class RequestBodyLimitMiddleware
{
    /// <summary>The message of the 413 page (07 2.8.7): the operation did not happen and the sent values were not read.</summary>
    public const string Message = "Gönderdiğiniz veri izin verilen boyutu aşıyor; işlem yapılmadı.";

    private readonly RequestDelegate _next;

    public RequestBodyLimitMiddleware(RequestDelegate next)
    {
        ArgumentNullException.ThrowIfNull(next);
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        Endpoint? endpoint = context.GetEndpoint();
        RequestBodyClass? declared = endpoint?.Metadata.GetMetadata<RequestBodyClassAttribute>()?.BodyClass;
        long limit = RequestBodyLimits.Of(context.Request.Method, declared);
        HttpRequest request = context.Request;

        bool hasBody = request.ContentLength is > 0 || request.Headers.ContainsKey("Transfer-Encoding");
        if (!hasBody)
        {
            await _next(context);
            return;
        }

        if (request.ContentLength is long length && length > limit)
        {
            await RefuseAsync(context, endpoint);
            return;
        }

        if (context.Features.Get<IHttpMaxRequestBodySizeFeature>() is { IsReadOnly: false } serverLimit)
        {
            serverLimit.MaxRequestBodySize = limit;
        }

        if (request.ContentLength is null)
        {
            byte[]? body = await ReadWithinAsync(request.Body, limit, context.RequestAborted);
            if (body is null)
            {
                await RefuseAsync(context, endpoint);
                return;
            }

            request.Body = new MemoryStream(body, writable: false);
        }

        await _next(context);
    }

    // Reads at most limit + 1 bytes; null when the body is longer than the limit.
    private static async Task<byte[]?> ReadWithinAsync(Stream body, long limit, CancellationToken ct)
    {
        using MemoryStream buffer = new();
        byte[] chunk = new byte[16 * 1024];
        int read;
        while ((read = await body.ReadAsync(chunk, ct)) > 0)
        {
            if (buffer.Length + read > limit)
            {
                return null;
            }

            buffer.Write(chunk, 0, read);
        }

        return buffer.ToArray();
    }

    private static async Task RefuseAsync(HttpContext context, Endpoint? endpoint)
    {
        HttpResponse response = context.Response;
        response.StatusCode = StatusCodes.Status413PayloadTooLarge;
        response.Headers.CacheControl = "no-store";
        bool machine = endpoint?.Metadata.GetMetadata<MachineEndpointMetadata>() is not null || RateLimitScope.IsMachineEndpoint(context);
        if (machine)
        {
            return;
        }

        response.ContentType = "text/html; charset=utf-8";
        await response.WriteAsync(
            "<!DOCTYPE html><html lang=\"tr\"><head><meta charset=\"utf-8\"><title>İşlem yapılamadı</title></head><body><main>" +
            "<h1>İşlem yapılamadı</h1><p>" + WebUtility.HtmlEncode(Message) + "</p><p><a href=\"/\">Ana sayfaya dön</a></p></main></body></html>",
            context.RequestAborted);
    }
}

/// <summary>The registration of the body ceiling middleware (07 2.8.1).</summary>
internal static class RequestBodyLimitSetup
{
    /// <summary>After the routing, which finds the endpoint's class, and before the authentication, the anti-forgery check and the endpoint.</summary>
    public static IApplicationBuilder UseRequestBodyLimits(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);
        return app.UseMiddleware<RequestBodyLimitMiddleware>();
    }
}
