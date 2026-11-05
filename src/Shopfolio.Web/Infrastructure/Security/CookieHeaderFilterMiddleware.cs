using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;

namespace Shopfolio.Infrastructure.Security;

/// <summary>
/// Until the cookie policy has been published for the first time, the storefront writes no cookie (05 6.7.10; MK-57; 02 12.2.5): this middleware
/// removes the <c>Set-Cookie</c> headers from the storefront's responses at the moment they start, whoever wrote them. It is the one guarantee —
/// the closed doors of the sales and the data collecting entries (02 3.1.5) already close the functions that need a cookie, and this is the second
/// layer against a function that was forgotten. It is registered at the start of the pipeline, after the forwarded headers and the security
/// headers and outside every middleware that can write a cookie (09 15.1 item 6): the callbacks of <c>OnStarting</c> run last-registered first, so
/// this one sees the headers as the other middlewares left them.
/// </summary>
/// <remarks>
/// The gate opens at the first request after the publication and does not close again (<see cref="CookiePolicyGate"/>). The panel's responses are out
/// of scope — the panel is the administrator's browser and not the visitor's (05 3.5.5) — and so are the machine endpoints under <c>/sistem</c>,
/// which write no cookie and must answer when the database does not (07 4.33.1).
/// </remarks>
internal sealed class CookieHeaderFilterMiddleware
{
    private static readonly PathString _panelPath = new("/panel");
    private static readonly PathString _machinePath = new("/sistem");

    private readonly RequestDelegate _next;
    private readonly CookiePolicyGate _gate;

    public CookieHeaderFilterMiddleware(RequestDelegate next, CookiePolicyGate gate)
    {
        ArgumentNullException.ThrowIfNull(next);
        ArgumentNullException.ThrowIfNull(gate);
        _next = next;
        _gate = gate;
    }

    /// <summary>True for the responses the filter applies to: everything except the panel and the machine endpoints.</summary>
    public static bool AppliesTo(PathString path) =>
        !path.StartsWithSegments(_panelPath, StringComparison.OrdinalIgnoreCase) && !path.StartsWithSegments(_machinePath, StringComparison.OrdinalIgnoreCase);

    public async Task InvokeAsync(HttpContext context, ICookiePolicyPublication publication)
    {
        ArgumentNullException.ThrowIfNull(context);
        if (!AppliesTo(context.Request.Path))
        {
            await _next(context);
            return;
        }

        // The removal is registered before the question is asked and is lifted only by a yes. A failing question is not caught here (09 8.1 item 6: the
        // catch layer is the one place): it reaches the catch layer registered outside this middleware (T212, 09 8.1 item 2), and the error page that layer
        // writes passes through the removal like every other storefront response.
        var removal = new Removal(context.Response);
        context.Response.OnStarting(RemoveCookieHeaders, removal);
        removal.Active = !await _gate.IsOpenAsync(publication, context.RequestAborted);

        await _next(context);
    }

    private static Task RemoveCookieHeaders(object state)
    {
        var removal = (Removal)state;
        if (removal.Active)
        {
            removal.Response.Headers.Remove(HeaderNames.SetCookie);
        }

        return Task.CompletedTask;
    }

    private sealed class Removal(HttpResponse response)
    {
        public HttpResponse Response { get; } = response;

        public bool Active { get; set; } = true;
    }
}
