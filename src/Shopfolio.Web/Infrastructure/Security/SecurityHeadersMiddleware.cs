using Microsoft.Net.Http.Headers;

namespace Shopfolio.Infrastructure.Security;

/// <summary>
/// Gives every response the security headers (09 15.2 item 3; 05 6.5.4): the error response, the static file and the machine endpoint too. It is
/// registered at the start of the pipeline and writes the headers when the response starts (<c>OnStarting</c>), so whoever answers - a page, an
/// endpoint, the catch layer, a closed door - the headers are there. The nonce of the content security policy is made here, once per response, and
/// kept in <c>HttpContext.Items</c> for the common layout (<see cref="SecurityHeaders.NonceOf"/>).
/// </summary>
internal sealed class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;

    public SecurityHeadersMiddleware(RequestDelegate next)
    {
        ArgumentNullException.ThrowIfNull(next);
        _next = next;
    }

    public Task InvokeAsync(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        string nonce = SecurityHeaders.NewNonce();
        context.Items[SecurityHeaders.NonceKey] = nonce;
        context.Response.OnStarting(static state => Write((HttpContext)state, (string)((HttpContext)state).Items[SecurityHeaders.NonceKey]!), context); // non-null: InvokeAsync writes the nonce to Items before it registers this callback
        return _next(context);
    }

    private static Task Write(HttpContext context, string nonce)
    {
        IHeaderDictionary headers = context.Response.Headers;
        headers[ContentSecurityPolicy.HeaderName] = ContentSecurityPolicy.Build(nonce, PolicyOptionsOf(context));
        headers.XContentTypeOptions = SecurityHeaders.NoSniff;
        headers["Referrer-Policy"] = context.GetEndpoint()?.Metadata.GetMetadata<KeyedLinkPageAttribute>() is null
            ? SecurityHeaders.ReferrerPolicyDefault
            : SecurityHeaders.ReferrerPolicyKeyedPage;
        headers["Permissions-Policy"] = SecurityHeaders.PermissionsPolicy;

        // Every 404 is the one answer of a denied or missing record (07 2.3.3): the panel and the account folders write no-store on all their answers, so the
        // other 404s - the order pages, a route that matches nothing - carry it too, and no header says which of them a request reached.
        if (context.Response.StatusCode == StatusCodes.Status404NotFound && !headers.ContainsKey(HeaderNames.CacheControl))
        {
            headers.CacheControl = SecurityHeaders.NoStore;
        }

        return Task.CompletedTask;
    }

    // The two responses that change the policy (09 15.2 item 4): the payment page's answer that draws the provider's frame - its page carries the mark and the answer marked
    // itself - and the hand-over page. Anything else, the error answers of those pages included, keeps the default.
    private static ContentSecurityPolicyOptions PolicyOptionsOf(HttpContext context)
    {
        EndpointMetadataCollection? metadata = context.GetEndpoint()?.Metadata;
        string? frameOrigin = metadata?.GetMetadata<AllowPaymentFrameAttribute>() is not null
            && PaymentFrameResponse.IsMarked(context)
            && context.Response.StatusCode == StatusCodes.Status200OK
            ? context.RequestServices.GetService<Integrations.PaymentFrameSource>()?.Origin
            : null;
        bool selfFraming = metadata?.GetMetadata<AllowFramingAttribute>() is not null && context.Response.StatusCode == StatusCodes.Status200OK;
        return frameOrigin is null && !selfFraming ? ContentSecurityPolicyOptions.Default : new ContentSecurityPolicyOptions(frameOrigin, selfFraming);
    }
}

/// <summary>
/// Marks a page whose address holds a key - an account link (09 15.1 item 4) or the access-key order page (07 UC-34): the response sends no referrer
/// (<see cref="SecurityHeaders.ReferrerPolicyKeyedPage"/>, 05 6.4.8). The page also carries <see cref="NoStoreAttribute"/>; the tasks of those pages
/// put both on them.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
internal sealed class KeyedLinkPageAttribute : Attribute
{
}
