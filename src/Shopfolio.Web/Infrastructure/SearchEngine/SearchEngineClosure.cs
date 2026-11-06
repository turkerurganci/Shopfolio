using System.Net.Mime;

using Shopfolio.Infrastructure.Routing;

namespace Shopfolio.Infrastructure.SearchEngine;

/// <summary>
/// The pages the search engines may not index (02 3.30.6; 05 3.7.4): the cart, the payment step, the order pages, the order tracking, the account pages, the
/// panel (all of it), the site search and the downloads. A closed page's answer says so in every shape: an HTML answer carries the robots meta tag
/// (<see cref="SearchEngineData.Head"/>), an answer that is not HTML carries the <c>X-Robots-Tag: noindex</c> header (<see cref="SearchEngineClosureMiddleware"/>);
/// <c>robots.txt</c> never blocks them, because a blocked page cannot be read and its <c>noindex</c> would not be seen (05 3.7.4; the file is T313's). What
/// is closed is decided by the address here, and by the page itself where the address does not tell: a list with a filter (<c>IListPage</c>, 07 2.6.7), a
/// draft and an archived product (02 3.30.6) call <see cref="MarkClosed"/> or hand the head data the mark.
/// </summary>
internal static class SearchEngineClosure
{
    /// <summary>The header of a closed answer that is not HTML (05 3.7.4).</summary>
    public const string HeaderName = "X-Robots-Tag";

    /// <summary>The value of the header and of the robots meta tag.</summary>
    public const string Directive = "noindex";

    private const string MarkKey = "Shopfolio.ClosedToSearchEngines";

    /// <summary>
    /// The first path segments whose every page is closed, each a member of the closed list of <see cref="AddressRules.ClosedFirstSegments"/>: the fixed pages
    /// the cart, the payment step, the order tracking and the search; the order root; the account links and the account area; the panel; the download area.
    /// </summary>
    public static readonly IReadOnlyList<string> ClosedFirstSegments =
    [
        "sepet", "odeme", "siparis-takibi", "arama", AddressRules.OrderArea, .. AddressRules.AccountPages, AddressRules.PanelArea, "indir",
    ];

    private static readonly HashSet<string> _closed = new(ClosedFirstSegments, StringComparer.OrdinalIgnoreCase);

    /// <summary>Whether the first segment of <paramref name="path"/> is one of <see cref="ClosedFirstSegments"/> (the lookup ignores letter case, 07 2.1.7).</summary>
    public static bool IsClosedPath(PathString path)
    {
        string value = path.Value ?? string.Empty;
        ReadOnlySpan<char> rest = value.AsSpan().TrimStart('/');
        int end = rest.IndexOf('/');
        return _closed.GetAlternateLookup<ReadOnlySpan<char>>().Contains(end < 0 ? rest : rest[..end]);
    }

    /// <summary>Whether the request's answer is closed: its address is a closed one, or the page marked it (<see cref="MarkClosed"/>).</summary>
    public static bool IsClosed(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        return IsClosedPath(context.Request.Path) || context.Items.ContainsKey(MarkKey);
    }

    /// <summary>Marks the answer of the request closed where the address does not say so - a draft, an archived product, a filtered list.</summary>
    public static void MarkClosed(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        context.Items[MarkKey] = true;
    }

    /// <summary>Whether a content type is HTML, whose closing is the meta tag and not the header (05 3.7.4).</summary>
    public static bool IsHtml(string? contentType)
    {
        if (string.IsNullOrWhiteSpace(contentType))
        {
            return false;
        }

        return contentType.StartsWith(MediaTypeNames.Text.Html, StringComparison.OrdinalIgnoreCase)
            || contentType.StartsWith("application/xhtml+xml", StringComparison.OrdinalIgnoreCase);
    }
}

/// <summary>
/// Writes <c>X-Robots-Tag: noindex</c> on a closed answer that is not HTML (05 3.7.4; 07 4.32.2): a download, the CSV the panel exports, the JSON of the
/// chunked upload, the panel's redirect and its not-found answer. It writes when the response starts, so whoever answers - a page, an endpoint, the catch
/// layer - the closed address is covered; an HTML answer is closed by its meta tag and gets no header here. The header is set, never added, so an
/// answer that already carries it (the CSV result) carries it once.
/// </summary>
internal sealed class SearchEngineClosureMiddleware
{
    private readonly RequestDelegate _next;

    public SearchEngineClosureMiddleware(RequestDelegate next)
    {
        ArgumentNullException.ThrowIfNull(next);
        _next = next;
    }

    public Task InvokeAsync(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        context.Response.OnStarting(
            static state =>
            {
                HttpContext http = (HttpContext)state;
                if (SearchEngineClosure.IsClosed(http) && !SearchEngineClosure.IsHtml(http.Response.ContentType))
                {
                    http.Response.Headers[SearchEngineClosure.HeaderName] = SearchEngineClosure.Directive;
                }

                return Task.CompletedTask;
            },
            context);
        return _next(context);
    }
}

/// <summary>The registration of the search engine data pattern (05 3.7.2, 3.7.4).</summary>
internal static class SearchEngineSetup
{
    /// <summary>Registers <see cref="SiteAddress"/> and <see cref="SearchEngineData"/> as the application's singletons.</summary>
    public static IServiceCollection AddSearchEngineData(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.AddSingleton<SiteAddress>();
        services.AddSingleton<SearchEngineData>();
        return services;
    }

    /// <summary>Puts the closing middleware in the pipeline: right after the security headers, before the routing, because the address decides.</summary>
    public static IApplicationBuilder UseSearchEngineClosure(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);
        return app.UseMiddleware<SearchEngineClosureMiddleware>();
    }
}
