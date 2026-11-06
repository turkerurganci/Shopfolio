using System.Net;
using System.Net.Sockets;

namespace Shopfolio.Infrastructure.Security;

/// <summary>
/// The client address of a request and its axis (09 15.1 item 8; 05 6.3.3). The address is the one the reverse proxy forwarded - the forwarded
/// headers middleware (<see cref="ForwardedHeadersSetup"/>) has put it into the connection's remote address, and only for a request that came from the
/// proxy's inner network - so this file is the one place in the application that reads <c>HttpContext.Connection.RemoteIpAddress</c> (a source scan
/// keeps it so). The attempt record, the access record and the internal record use <see cref="Axis"/>, never the raw address.
/// </summary>
internal static class ClientAddress
{
    /// <summary>The axis text of a request whose address is unknown (no remote address on the connection): one axis value for all of them.</summary>
    public const string UnknownAxis = "unknown";

    /// <summary>The client address of the request; null when the connection has none.</summary>
    public static IPAddress? Of(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        return context.Connection.RemoteIpAddress;
    }

    /// <summary>The axis of the request's client address (<see cref="Axis"/>).</summary>
    public static string AxisOf(HttpContext context) => Axis(Of(context));

    /// <summary>
    /// The axis value of an address: an IPv4 address as it is, an IPv6 address as its /64 prefix (the first eight bytes, the rest zero), an IPv4-mapped
    /// IPv6 address (<c>::ffff:a.b.c.d</c>) as the IPv4 address it maps. A visitor who holds a /64 cannot change his way out of the counter by changing the
    /// low half of the address.
    /// </summary>
    public static string Axis(IPAddress? address)
    {
        if (address is null)
        {
            return UnknownAxis;
        }

        if (address.IsIPv4MappedToIPv6)
        {
            address = address.MapToIPv4();
        }

        if (address.AddressFamily != AddressFamily.InterNetworkV6)
        {
            return address.ToString();
        }

        byte[] bytes = address.GetAddressBytes();
        Array.Clear(bytes, 8, 8);
        return new IPAddress(bytes) + "/64";
    }
}
