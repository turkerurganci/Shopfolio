using System.Net;

using Microsoft.AspNetCore.Http;

using Shopfolio.Infrastructure.Security;

namespace Shopfolio.UnitTests.RateLimiting;

/// <summary>
/// The axis of a client address (T215 KK3; 09 15.1 item 8; 05 6.3.3; 07 2.8.2): an IPv4 address as it is, an IPv6 address by its /64 prefix, an
/// IPv4-mapped IPv6 address as the IPv4 address, no address as one shared value.
/// </summary>
public sealed class ClientAddressTests
{
    [Theory]
    [InlineData("192.0.2.10", "192.0.2.10")]
    [InlineData("198.51.100.7", "198.51.100.7")]
    [InlineData("2001:db8:1:2:3:4:5:6", "2001:db8:1:2::/64")]
    [InlineData("2001:db8:1:2:ffff:ffff:ffff:ffff", "2001:db8:1:2::/64")]
    [InlineData("2001:db8:1:2::", "2001:db8:1:2::/64")]
    [InlineData("::ffff:192.0.2.10", "192.0.2.10")]
    public void Axis_OfAnAddress_IsTheV4AddressOrTheV6Prefix(string address, string expected)
    {
        Assert.Equal(expected, ClientAddress.Axis(IPAddress.Parse(address)));
    }

    [Fact]
    public void Axis_OfTwoAddressesInTheSameV6Prefix_IsOneValue_AndOfDifferentPrefixesTwo()
    {
        string first = ClientAddress.Axis(IPAddress.Parse("2001:db8:1:2:aaaa::1"));
        string sameSlash64 = ClientAddress.Axis(IPAddress.Parse("2001:db8:1:2:bbbb::2"));
        string otherSlash64 = ClientAddress.Axis(IPAddress.Parse("2001:db8:1:3:aaaa::1"));

        Assert.Equal(first, sameSlash64);
        Assert.NotEqual(first, otherSlash64);
    }

    [Fact]
    public void Axis_OfNoAddress_IsTheSharedUnknownValue()
    {
        Assert.Equal(ClientAddress.UnknownAxis, ClientAddress.Axis(null));
    }

    [Fact]
    public void AxisOf_ReadsTheConnectionsAddress_AndOfIsNullWhenTheConnectionHasNone()
    {
        DefaultHttpContext context = new();
        Assert.Null(ClientAddress.Of(context));
        Assert.Equal(ClientAddress.UnknownAxis, ClientAddress.AxisOf(context));

        context.Connection.RemoteIpAddress = IPAddress.Parse("2001:db8:9:9:1:2:3:4");

        Assert.Equal(IPAddress.Parse("2001:db8:9:9:1:2:3:4"), ClientAddress.Of(context));
        Assert.Equal("2001:db8:9:9::/64", ClientAddress.AxisOf(context));
    }
}
