using System.Net;
using System.Threading.RateLimiting;

using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

using Shopfolio.Infrastructure.Errors;
using Shopfolio.Infrastructure.InstallationSettings;

namespace Shopfolio.Infrastructure.Security;

/// <summary>
/// The window of the general rate limit (07 2.8.2; 09 3.5 table): sixty seconds in six segments. They are constants of the limiter, not settings
/// (09 15.4 item 2); the signed endpoints' thinned log line uses the same sixty seconds (<see cref="SignedEndpoints.SignedEndpointLimits.FailureLogWindow"/>).
/// </summary>
internal static class RateLimitDefaults
{
    public static readonly TimeSpan Window = TimeSpan.FromSeconds(60);

    public const int Segments = 6;
}

/// <summary>The neutral message of a limit that was exceeded (02 6.1.5): it names neither the remaining attempts, the length of the block nor the axis.</summary>
internal static class NeutralMessages
{
    public const string LimitExceeded = "Çok fazla deneme yapıldı, birazdan tekrar deneyin.";
}

/// <summary>
/// What the general limit counts (07 2.8.3; 09 15.4 item 3), in one place. Static files and images (<c>/statik</c>, <c>/gorsel</c>) are outside both
/// limits. The payment provider's notification endpoint is outside both limits unless its signature failed, and then it counts towards the form
/// submission limit (<see cref="CountSignatureFailure"/>). The chunked upload (<c>/panel/yukleme</c>) is under the page limit only. The probes,
/// the sitemap, robots.txt and the download endpoint are GETs and so under the page limit only. The comparison is on whole path segments and
/// ignores the letter case (the routes do): <c>/statikx</c> is not the static area.
/// </summary>
internal static class RateLimitScope
{
    private static readonly PathString _staticPath = new(Routing.AddressRules.StaticArea);
    private static readonly PathString _imagePath = new("/gorsel");
    private static readonly PathString _chunkedUploadPath = new("/panel/yukleme");
    private static readonly PathString _notificationPath = new(AntiforgeryExemptions.PaymentNotificationPath);

    /// <summary>The key under which a failed signature marks its request (<see cref="CountSignatureFailure"/>).</summary>
    public const string SignatureFailureItemKey = "Shopfolio.RateLimit.SignatureFailure";

    /// <summary>True when the request is under the page limit: every request except the static files, the images and the notification endpoint.</summary>
    public static bool IsPageLimited(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        PathString path = context.Request.Path;
        return !Under(path, _staticPath) && !Under(path, _imagePath) && !Under(path, _notificationPath);
    }

    /// <summary>
    /// True when the request is under the form submission limit: a POST, except the static files, the images, the chunked upload and the notification
    /// endpoint; a request whose signature failed is marked and is counted even on the notification endpoint (K-941).
    /// </summary>
    public static bool IsFormLimited(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        if (context.Items.ContainsKey(SignatureFailureItemKey))
        {
            return true;
        }

        PathString path = context.Request.Path;
        return HttpMethods.IsPost(context.Request.Method)
            && !Under(path, _staticPath) && !Under(path, _imagePath) && !Under(path, _chunkedUploadPath) && !Under(path, _notificationPath);
    }

    /// <summary>
    /// A machine endpoint answers a refusal with no body (07 2.8.4, 2.7.2): the probes and the status under <c>/sistem</c>, the download under
    /// <c>/indir</c> and the chunked upload (09 15.4 item 4).
    /// </summary>
    public static bool IsMachineEndpoint(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        PathString path = context.Request.Path;
        return Under(path, new PathString("/sistem")) || Under(path, new PathString("/indir")) || Under(path, _chunkedUploadPath);
    }

    /// <summary>
    /// Marks the request as a failed signature and takes one permit from the same chain the middleware uses; the lease is returned at once and its
    /// result is ignored (the answer is the endpoint's 400, <c>11.1</c>). The endpoint is outside the page limit, so only the form submission counter
    /// moves - no second limiter is built (K-941; 09 15.4 item 3).
    /// </summary>
    public static void CountSignatureFailure(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        context.Items[SignatureFailureItemKey] = true;
        PartitionedRateLimiter<HttpContext>? limiter = context.RequestServices?.GetService<IOptions<RateLimiterOptions>>()?.Value.GlobalLimiter;
        if (limiter is null)
        {
            return;
        }

        using RateLimitLease lease = limiter.AttemptAcquire(context);
    }

    private static bool Under(PathString path, PathString area) => path.StartsWithSegments(area, StringComparison.OrdinalIgnoreCase);
}

/// <summary>
/// The general request rate limit (09 15.4 items 1, 2 and 4; 05 6.3.5; 07 2.8): two sliding windows per client address axis - the page limit and the
/// form submission limit - chained, so a request that is both counts in both. The axis is the address the reverse proxy forwarded, an IPv6 address by
/// its /64 prefix (<see cref="ClientAddress.AxisOf"/>). The window is sixty seconds in six segments and the two numbers come from the installation
/// settings; the answer to a refusal is 429, with no <c>Retry-After</c> (the block's length is not told - 02 6.1.5).
/// </summary>
internal static class RequestRateLimiting
{
    public static IServiceCollection AddRequestRateLimiting(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.TryAddSingleton<RejectionLog>();
        services.AddRateLimiter(_ => { });
        services.AddOptions<RateLimiterOptions>()
            .Configure<IOptions<RateLimitOptions>, TimeProvider>((options, settings, time) => Configure(options, settings.Value, time));
        return services;
    }

    public static IApplicationBuilder UseRequestRateLimiting(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);
        return app.UseRateLimiter();
    }

    public static void Configure(RateLimiterOptions options, RateLimitOptions settings, TimeProvider time)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(time);

        options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
        options.GlobalLimiter = PartitionedRateLimiter.CreateChained(
            PartitionedRateLimiter.Create<HttpContext, string>(context => RateLimitScope.IsPageLimited(context)
                ? RateLimitPartition.Get("page:" + ClientAddress.AxisOf(context), _ => Window(settings.PageRequestsPerMinute, time))
                : RateLimitPartition.GetNoLimiter("page:exempt")),
            PartitionedRateLimiter.Create<HttpContext, string>(context => RateLimitScope.IsFormLimited(context)
                ? RateLimitPartition.Get("form:" + ClientAddress.AxisOf(context), _ => Window(settings.FormSubmissionsPerMinute, time))
                : RateLimitPartition.GetNoLimiter("form:exempt")));
        options.OnRejected = WriteRejectionAsync;
    }

    private static TimeDrivenSlidingWindowLimiter Window(int permits, TimeProvider time)
        => new(permits, RateLimitDefaults.Window, RateLimitDefaults.Segments, time);

    private static async ValueTask WriteRejectionAsync(OnRejectedContext rejection, CancellationToken cancellationToken)
    {
        HttpContext context = rejection.HttpContext;
        HttpResponse response = context.Response;
        response.StatusCode = StatusCodes.Status429TooManyRequests;
        response.Headers.CacheControl = "no-store";

        // 429 is no unexpected error (09 8.1): the line is the rate limit's own and the error counter does not see it (05 9.4.8).
        context.RequestServices.GetRequiredService<RejectionLog>().RateLimited();

        // A machine endpoint answers with no body; so does a request that has no page to show (07 2.8.4).
        if (RateLimitScope.IsMachineEndpoint(context))
        {
            return;
        }

        response.ContentType = "text/html; charset=utf-8";
        string message = WebUtility.HtmlEncode(NeutralMessages.LimitExceeded);
        await response.WriteAsync(
            "<!DOCTYPE html><html lang=\"tr\"><head><meta charset=\"utf-8\"><title>Çok fazla deneme</title></head><body><main>" +
            "<h1>Çok fazla deneme</h1><p>" + message + "</p><p><a href=\"/\">Ana sayfaya dön</a></p></main></body></html>",
            cancellationToken);
    }
}
