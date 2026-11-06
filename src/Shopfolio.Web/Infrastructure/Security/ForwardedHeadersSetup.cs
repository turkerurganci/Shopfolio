using Microsoft.AspNetCore.HttpOverrides;

namespace Shopfolio.Infrastructure.Security;

/// <summary>
/// The inner network of the reverse proxy (09 15.1 item 8; 05 6.3.3): the only source whose forwarded headers are believed. It is the fixed subnet
/// of the <c>edge</c> network in <c>deploy/compose.yaml</c> - the proxy's container address can change, the network does not - and a test keeps the
/// two equal.
/// </summary>
internal static class ReverseProxyTrust
{
    /// <summary>The IPv4 subnet of the edge network (<c>deploy/compose.yaml</c>, <c>networks.edge.ipam</c>).</summary>
    public const string TrustedNetwork = "172.30.1.0/24";

    /// <summary>The IPv6 subnet of the edge network; IPv6 is on there so that the proxy sees an IPv6 visitor's own address (05 6.3.3).</summary>
    public const string TrustedNetworkV6 = "fd7a:5f0c:9e11:1::/64";

    /// <summary>The trusted networks, in the order of the constants.</summary>
    public static IReadOnlyList<string> TrustedNetworks { get; } = [TrustedNetwork, TrustedNetworkV6];
}

/// <summary>
/// The forwarded headers middleware, the first of the pipeline (09 15.1 item 8; 05 6.3.3): the client address (<c>X-Forwarded-For</c>) and the scheme
/// (<c>X-Forwarded-Proto</c>) are taken from the reverse proxy and from nobody else. One hop is believed (<c>ForwardLimit = 1</c>); the trusted
/// networks are the proxy's inner network only and the framework's default trust of the loopback address is cleared (a header from another source is
/// ignored, and the request keeps its connection's address and scheme). The scheme matters beyond the address: the proxy ends TLS, and without the
/// header the application would take every request for HTTP and could not write a <c>Secure</c> cookie (D-112).
/// </summary>
internal static class ForwardedHeadersSetup
{
    public static IServiceCollection AddForwardedHeadersRules(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.Configure<ForwardedHeadersOptions>(Configure);
        return services;
    }

    public static IApplicationBuilder UseForwardedHeadersRules(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);
        return app.UseForwardedHeaders();
    }

    internal static void Configure(ForwardedHeadersOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
        options.ForwardLimit = 1;
        options.KnownProxies.Clear();
        options.KnownIPNetworks.Clear();
        foreach (string network in ReverseProxyTrust.TrustedNetworks)
        {
            options.KnownIPNetworks.Add(System.Net.IPNetwork.Parse(network));
        }
    }
}
