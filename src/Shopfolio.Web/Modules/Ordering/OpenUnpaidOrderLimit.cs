using System.Net;

using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Security;
using Shopfolio.Infrastructure.Values;
using Shopfolio.Modules.Ordering.Contracts;

namespace Shopfolio.Modules.Ordering;

/// <summary>
/// L-8, the open unpaid order ceiling (02 8.2, 8.2.2, 8.2.3, 11.2 P-46; 05 6.3.2; T376 criterion 8): the number of orders whose payment is Pending, counted
/// from the orders themselves - no attempt row, no window. The count falls by itself when an open order is paid or is cancelled (its payment is then Paid or
/// Failed), so nothing is deleted and nothing expires. The ceiling is <see cref="AttemptLimits.OrderCountedOf"/>'s number (the product constant); a count at
/// the ceiling is full: the order that would be the next is refused ("3 open orders" lets three stand and stops the fourth).
/// <para>
/// The e-mail axis counts only the orders a signed-in member placed (02 8.2.2): a guest order - signed in later or not - is counted on the IP axis only. The IP axis
/// is the axis of <see cref="ClientAddress.Axis"/> (an IPv6 address by its /64, an IPv4-mapped address as the IPv4 one); both are cut by <see cref="OrderAxes"/>. The count
/// holds the open orders of both payment methods - the document counts "the orders whose payment is Pending" (02 8.2) -; what a full axis closes, the bank transfer only, is
/// <see cref="OrderPlacementLimits"/>' (T377 criterion 5; D-781).
/// </para>
/// </summary>
internal sealed class OpenUnpaidOrderLimit
{
    private readonly AppDbContext _db;

    public OpenUnpaidOrderLimit(AppDbContext db)
    {
        ArgumentNullException.ThrowIfNull(db);
        _db = db;
    }

    /// <summary>The ceiling of the limit (P-46).</summary>
    public static int Ceiling => AttemptLimits.OrderCountedOf(AttemptLimit.L8).Max;

    /// <summary>The open unpaid orders of the IP axis of <paramref name="address"/> ; no address counts none (a live order always holds its address - only the disposal empties it, 06 4.17).</summary>
    public async Task<int> CountByIpAsync(IPAddress? address, CancellationToken ct)
    {
        if (address is null)
        {
            return 0;
        }

        return await OrderAxes.OfIp(Open(), address).CountAsync(ct);
    }

    /// <summary>The open unpaid orders of the e-mail axis: the orders a signed-in member placed with that (normalized) address; a guest order is not counted.</summary>
    public async Task<int> CountByEmailAsync(string email, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(email);
        return await OrderAxes.OfMemberEmail(Open(), email).CountAsync(ct);
    }

    private IQueryable<Order> Open() => _db.Orders.AsNoTracking().Where(o => o.PaymentStatus == PaymentStatus.Pending);

    /// <summary>True when the IP axis holds as many open orders as the ceiling allows.</summary>
    public async Task<bool> IsFullByIpAsync(IPAddress? address, CancellationToken ct) => await CountByIpAsync(address, ct) >= Ceiling;

    /// <summary>True when the e-mail axis holds as many open orders as the ceiling allows.</summary>
    public async Task<bool> IsFullByEmailAsync(string email, CancellationToken ct) => await CountByEmailAsync(email, ct) >= Ceiling;
}
