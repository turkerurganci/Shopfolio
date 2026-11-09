using System.Net;

using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Security;

namespace Shopfolio.Modules.Ordering;

/// <summary>
/// The two axes of the limits counted from the orders (L-7, L-8; 02 8.2.2; 05 6.3.2) in one place, so the two limits cut an order the same way: the IP axis is the axis of
/// <see cref="ClientAddress.Axis"/> (an IPv6 address by its /64, an IPv4-mapped address as the IPv4 one) and counts every order, guest or member; the e-mail axis counts only the
/// orders a signed-in member placed, by the account's normalized e-mail - a guest order, signed in to an account later or not, is on the IP axis only (02 8.2.2; K-579).
/// </summary>
internal static class OrderAxes
{
    /// <summary>The orders placed from the IP axis of <paramref name="address"/>; an order that holds no address (only the disposal empties it, 06 4.17) is on no axis.</summary>
    public static IQueryable<Order> OfIp(IQueryable<Order> orders, IPAddress address)
    {
        ArgumentNullException.ThrowIfNull(orders);
        ArgumentNullException.ThrowIfNull(address);

        // An address and the IPv4-mapped form of it are one axis (ClientAddress.Axis); an order may hold either, so both are matched.
        (IPAddress network, int prefixLength) = ClientAddress.Network(address);
        if (prefixLength == ClientAddress.V6PrefixLength)
        {
            IPNetwork cidr = new(network, prefixLength);
            return orders.Where(o => o.IpAddress != null && EF.Functions.ContainedBy(o.IpAddress, cidr));
        }

        IPAddress mapped = network.MapToIPv6();
        return orders.Where(o => o.IpAddress != null && (o.IpAddress == network || o.IpAddress == mapped));
    }

    /// <summary>The orders a signed-in member placed with the (normalized) e-mail of the account; a guest order is not on this axis.</summary>
    public static IQueryable<Order> OfMemberEmail(IQueryable<Order> orders, string email)
    {
        ArgumentNullException.ThrowIfNull(orders);
        ArgumentNullException.ThrowIfNull(email);
        string normalized = TextNormalizer.NormalizeIdentity(email);
        return orders.Where(o => !o.IsGuestOrder && o.NormalizedContactEmail == normalized);
    }
}
