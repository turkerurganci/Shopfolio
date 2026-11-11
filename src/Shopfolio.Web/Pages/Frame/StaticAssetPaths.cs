using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.StaticAssets;

using Shopfolio.Infrastructure.Routing;

namespace Shopfolio.Pages.Frame;

/// <summary>
/// The address of a static file as the frame's layouts write it (T540; 07 2.1.2, 2.10.3; 09 16.1; D-116): under <see cref="AddressRules.StaticArea"/> and, because the application serves the
/// files with a fingerprint in the name (<c>MapStaticAssets</c> inside the <c>/statik</c> group), the fingerprinted name, so a changed file gets a new address. The address is read from the
/// endpoints the application really serves, once, and never written by a layout; a file that has no endpoint (a host without static assets) gets the plain address under the same area.
/// </summary>
public sealed class StaticAssetResolver
{
    private readonly EndpointDataSource _endpoints;

    internal StaticAssetResolver(EndpointDataSource endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);
        _endpoints = endpoints;
    }

    /// <summary>The address of <paramref name="file"/> (a path under <c>wwwroot</c>, for example <c>css/base.css</c>).</summary>
    public string AddressOf(string file)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(file);
        string wanted = file.TrimStart('/');
        string plain = $"{AddressRules.StaticArea}/{wanted}";
        foreach (RouteEndpoint endpoint in _endpoints.Endpoints.OfType<RouteEndpoint>())
        {
            StaticAssetDescriptor? descriptor = endpoint.Metadata.GetMetadata<StaticAssetDescriptor>();
            string? label = descriptor?.Properties.FirstOrDefault(p => string.Equals(p.Name, "label", StringComparison.Ordinal))?.Value;
            string? pattern = endpoint.RoutePattern.RawText;
            if (label is not null && pattern is not null
                && string.Equals(label.TrimStart('/'), wanted, StringComparison.Ordinal)
                && !string.Equals("/" + pattern.TrimStart('/'), plain, StringComparison.Ordinal))
            {
                return "/" + pattern.TrimStart('/');
            }
        }

        return plain;
    }
}

/// <summary>How a layout asks for a static file's address (T540): <c>Context.StaticAsset("css/base.css")</c>.</summary>
public static class StaticAssetPaths
{
    /// <summary>The address of <paramref name="file"/> for this request: fingerprinted when the application serves it so.</summary>
    public static string StaticAsset(this HttpContext context, string file)
    {
        ArgumentNullException.ThrowIfNull(context);
        return context.RequestServices.GetRequiredService<StaticAssetResolver>().AddressOf(file);
    }
}
