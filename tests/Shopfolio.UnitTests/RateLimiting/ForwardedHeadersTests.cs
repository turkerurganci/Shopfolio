using System.Net;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

using Shopfolio.Infrastructure.Security;

namespace Shopfolio.UnitTests.RateLimiting;

/// <summary>
/// The forwarded headers rule (T215 KK3; D-112; 09 15.1 item 8; 05 6.3.3): the client address and the scheme are taken from the reverse proxy's inner
/// network and from nobody else; a header from the loopback address or from outside is ignored; no header changes nothing.
/// </summary>
public sealed class ForwardedHeadersTests
{
    private static async Task<HttpContext> RunAsync(string? from, string? forwardedFor = null, string? forwardedProto = null)
    {
        ForwardedHeadersOptions options = new();
        ForwardedHeadersSetup.Configure(options);
        ForwardedHeadersMiddleware middleware = new(_ => Task.CompletedTask, NullLoggerFactory.Instance, Options.Create(options));

        DefaultHttpContext context = new();
        context.Request.Scheme = "http";
        if (from is not null)
        {
            context.Connection.RemoteIpAddress = IPAddress.Parse(from);
        }

        if (forwardedFor is not null)
        {
            context.Request.Headers["X-Forwarded-For"] = forwardedFor;
        }

        if (forwardedProto is not null)
        {
            context.Request.Headers["X-Forwarded-Proto"] = forwardedProto;
        }

        await middleware.Invoke(context);
        return context;
    }

    [Theory]
    [InlineData("172.30.1.2")]
    [InlineData("172.30.1.254")]
    [InlineData("fd7a:5f0c:9e11:1::2")]
    [InlineData("fd7a:5f0c:9e11:1:aaaa:bbbb:cccc:1")]
    public async Task FromTheProxysInnerNetwork_TheForwardedAddressAndSchemeAreApplied(string proxy)
    {
        HttpContext context = await RunAsync(proxy, "203.0.113.50", "https");

        Assert.Equal(IPAddress.Parse("203.0.113.50"), context.Connection.RemoteIpAddress);
        Assert.Equal("https", context.Request.Scheme);
        Assert.True(context.Request.IsHttps);
    }

    [Fact]
    public async Task AnIpv6VisitorBehindTheProxy_KeepsTheOwnV6Address_AndItsAxisIsTheV64Prefix()
    {
        HttpContext context = await RunAsync("172.30.1.2", "2001:db8:7:7:1:2:3:4", "https");

        Assert.Equal(IPAddress.Parse("2001:db8:7:7:1:2:3:4"), context.Connection.RemoteIpAddress);
        Assert.Equal("2001:db8:7:7::/64", ClientAddress.AxisOf(context));
    }

    [Theory]
    [InlineData("127.0.0.1")]
    [InlineData("::1")]
    [InlineData("203.0.113.9")]
    [InlineData("172.30.2.5")]
    [InlineData("172.30.0.255")]
    [InlineData("172.30.3.1")]
    [InlineData("10.0.0.1")]
    [InlineData("fd7a:5f0c:9e11:2::2")]
    public async Task FromAnyOtherSource_TheForwardedHeadersAreIgnored_AndTheConnectionKeepsItsAddressAndScheme(string source)
    {
        HttpContext context = await RunAsync(source, "203.0.113.50", "https");

        Assert.Equal(IPAddress.Parse(source), context.Connection.RemoteIpAddress);
        Assert.Equal("http", context.Request.Scheme);
        Assert.False(context.Request.IsHttps);
    }

    [Fact]
    public async Task WithoutAHeader_NothingChanges_EvenFromTheProxy()
    {
        HttpContext context = await RunAsync("172.30.1.2");

        Assert.Equal(IPAddress.Parse("172.30.1.2"), context.Connection.RemoteIpAddress);
        Assert.Equal("http", context.Request.Scheme);
    }

    [Fact]
    public async Task OnlyOneHopIsBelieved_TheOuterOfTwoForwardedAddressesIsNotTaken()
    {
        HttpContext context = await RunAsync("172.30.1.2", "198.51.100.1, 203.0.113.50", "https");

        // The last hop is the one the trusted proxy appended; the first one is the visitor's own claim and is not believed.
        Assert.Equal(IPAddress.Parse("203.0.113.50"), context.Connection.RemoteIpAddress);
    }

    [Fact]
    public void TheOptions_TrustTheEdgeNetworksOnly_OneHop_AddressAndSchemeOnly()
    {
        ForwardedHeadersOptions options = new();

        ForwardedHeadersSetup.Configure(options);

        Assert.Equal(ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto, options.ForwardedHeaders);
        Assert.Equal(1, options.ForwardLimit);
        Assert.Empty(options.KnownProxies);
        Assert.Equal(
            ReverseProxyTrust.TrustedNetworks.Select(n => System.Net.IPNetwork.Parse(n).ToString()).Order(StringComparer.Ordinal),
            options.KnownIPNetworks.Select(n => n.ToString()).Order(StringComparer.Ordinal));
    }

    [Fact]
    public void TheTrustedNetworkList_IsTheTwoConstants()
    {
        Assert.Equal([ReverseProxyTrust.TrustedNetwork, ReverseProxyTrust.TrustedNetworkV6], ReverseProxyTrust.TrustedNetworks);
    }
}
