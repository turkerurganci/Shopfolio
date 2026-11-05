using System.Text;

using Microsoft.AspNetCore.Http.Features;

namespace Shopfolio.Infrastructure.SignedEndpoints;

/// <summary>
/// The common pipeline of a signed machine endpoint (08 3.2; 05 3.6.1, 4.5.2; 07 4.30.1, 2.8.1, 2.8.7; 09 11.1 item 5). Its order is fixed and
/// is the guarantee:
/// (1) the body limit (<see cref="SignedEndpointLimits.BodyMaxBytes"/>) before the body is read - a declared length above it is answered 413
/// without reading a byte, an undeclared one is read no further than one byte past it - and before the signature;
/// (2) the signature, by the link layer (<see cref="ISignedEndpoint{TNotice}.Verify"/>), before any handler, record or job row: a request whose
/// signature is missing or does not hold - or whose body is no readable form - is not a notification, writes nothing, gets 400 with an empty
/// body and a thinned security line (<see cref="SignatureFailureLog"/>);
/// (3) the record of the verified notice with its job row in one transaction, and "received" (200 and the service's own body) only after that
/// transaction has committed; the same notice again is "received" and writes nothing;
/// (4) a notice that cannot be recorded - a database error on the write or on the commit - is never "received": the pipeline does not catch
/// the error, it goes to the single catching layer (09 8.1), whose machine endpoint row answers 500 with an empty body and writes the line.
/// The endpoint is outside the protection against cross-site forgery (07 2.5.5): it carries no form token and is mapped without the check.
/// Counting a failed signature towards the form submission limit and its 429 belong to the rate limit (T215).
/// </summary>
internal sealed class SignedEndpointHandler
{
    private const string AcknowledgementContentType = "text/plain; charset=utf-8";

    private readonly SignatureFailureLog _failures;

    public SignedEndpointHandler(SignatureFailureLog failures)
    {
        _failures = failures;
    }

    public async Task HandleAsync<TNotice>(HttpContext context, ISignedEndpoint<TNotice> endpoint, ITransactionRunner transactions)
        where TNotice : class
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(endpoint);
        ArgumentNullException.ThrowIfNull(transactions);
        CancellationToken ct = context.RequestAborted;

        byte[]? body = await ReadBodyWithinLimitAsync(context, ct);
        if (body is null)
        {
            context.Response.StatusCode = StatusCodes.Status413PayloadTooLarge;
            return;
        }

        IReadOnlyDictionary<string, string>? fields = await ReadFormFieldsAsync(context.Request, body, ct);
        TNotice? notice = fields is null ? null : endpoint.Verify(fields);
        if (notice is null)
        {
            _failures.Record(context.Connection.RemoteIpAddress, fields is null ? SignatureFailureReason.BodyUnreadable : SignatureFailureReason.SignatureInvalid);
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            return;
        }

        // Not caught: a database error on the write or on the commit is an unexpected error and goes to the single catching layer (09 8.1
        // items 3, 6, 7), whose machine endpoint row answers 500 with an empty body; nothing has been written to the response by then.
        await transactions.RunAsync(work => endpoint.RecordAsync(notice, work), ct);

        context.Response.StatusCode = StatusCodes.Status200OK;
        context.Response.ContentType = AcknowledgementContentType;
        await context.Response.WriteAsync(endpoint.Acknowledgement, Encoding.UTF8, ct);
    }

    /// <summary>
    /// Reads the body when it is within the limit and returns null when it is not. The declared length is checked first, so a longer body is
    /// refused unread; a body without a declared length is read into a buffer one byte longer than the limit, and the server's own body size
    /// limit is set to the same value where the server has one.
    /// </summary>
    private static async Task<byte[]?> ReadBodyWithinLimitAsync(HttpContext context, CancellationToken ct)
    {
        if (context.Request.ContentLength is long declared && declared > SignedEndpointLimits.BodyMaxBytes)
        {
            return null;
        }

        if (context.Features.Get<IHttpMaxRequestBodySizeFeature>() is { IsReadOnly: false } serverLimit)
        {
            serverLimit.MaxRequestBodySize = SignedEndpointLimits.BodyMaxBytes;
        }

        byte[] buffer = new byte[SignedEndpointLimits.BodyMaxBytes + 1];
        int length = 0;
        try
        {
            int read;
            while (length < buffer.Length && (read = await context.Request.Body.ReadAsync(buffer.AsMemory(length), ct)) > 0)
            {
                length += read;
            }
        }
        catch (BadHttpRequestException exception) when (exception.StatusCode == StatusCodes.Status413PayloadTooLarge)
        {
            return null;
        }

        return length > SignedEndpointLimits.BodyMaxBytes ? null : buffer[..length];
    }

    /// <summary>
    /// The fields of a form body (URL-encoded or multipart), one value each; null when the body is no form, is malformed, carries a file or gives
    /// a field twice - such a body cannot carry a signature that holds.
    /// </summary>
    private static async Task<IReadOnlyDictionary<string, string>?> ReadFormFieldsAsync(HttpRequest request, byte[] body, CancellationToken ct)
    {
        if (!request.HasFormContentType)
        {
            return null;
        }

        request.Body = new MemoryStream(body, writable: false);
        IFormCollection form;
        try
        {
            form = await request.ReadFormAsync(ct);
        }
        catch (InvalidDataException)
        {
            return null;
        }
        catch (IOException)
        {
            return null;
        }

        if (form.Files.Count > 0 || form.Any(field => field.Value.Count != 1))
        {
            return null;
        }

        return form.ToDictionary(field => field.Key, field => field.Value.ToString(), StringComparer.Ordinal);
    }
}
