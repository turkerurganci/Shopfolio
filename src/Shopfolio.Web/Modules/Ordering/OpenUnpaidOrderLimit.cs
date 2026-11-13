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
    public Task<int> CountByIpAsync(IPAddress? address, CancellationToken ct) => CountByIpAsync(address, null, ct);

    /// <summary>
    /// The open unpaid orders of the IP axis without the placed unpaid orders of the cart <paramref name="exceptCartId"/>: the payment step's read counts what the confirmation would count,
    /// which cancels them before it asks (K-837; 07 4.13.1).
    /// </summary>
    public async Task<int> CountByIpAsync(IPAddress? address, Guid? exceptCartId, CancellationToken ct)
    {
        if (address is null)
        {
            return 0;
        }

        return await OrderAxes.OfIp(Open(exceptCartId), address).CountAsync(ct);
    }

    /// <summary>The open unpaid orders of the e-mail axis: the orders the signed-in member placed, read by the member's id (D-808); a guest order is not counted.</summary>
    public Task<int> CountByMemberAsync(Guid customerId, CancellationToken ct) => CountByMemberAsync(customerId, null, ct);

    /// <summary>The open unpaid orders of the e-mail axis without the placed unpaid orders of the cart <paramref name="exceptCartId"/> (see the IP axis).</summary>
    public async Task<int> CountByMemberAsync(Guid customerId, Guid? exceptCartId, CancellationToken ct)
    {
        return await OrderAxes.OfMember(Open(exceptCartId), customerId).CountAsync(ct);
    }

    /// <summary>
    /// The orders whose payment is Pending, as the caller's unit of work sees them: an order this context has already moved off Pending but not yet saved - the confirmation's
    /// earlier unpaid order of the same cart, cancelled in its open transaction (02 3.17.7; T353 criterion 2) - is not open any more, though the database still holds it as Pending.
    /// With <paramref name="exceptCartId"/>, the placed orders of that cart - the ones the confirmation cancels first - are left out of the count before they are cancelled.
    /// </summary>
    private IQueryable<Order> Open(Guid? exceptCartId)
    {
        List<Guid> settled = [.. _db.ChangeTracker.Entries<Order>()
            .Where(e => e.State == EntityState.Modified && e.Entity.PaymentStatus != PaymentStatus.Pending)
            .Select(e => e.Entity.Id)];
        IQueryable<Order> open = _db.Orders.AsNoTracking().Where(o => o.PaymentStatus == PaymentStatus.Pending && !settled.Contains(o.Id));
        return exceptCartId is Guid cartId ? open.Where(o => !(o.CartId == cartId && o.OrderStatus == OrderStatus.Placed)) : open;
    }

    /// <summary>True when the IP axis holds as many open orders as the ceiling allows.</summary>
    public Task<bool> IsFullByIpAsync(IPAddress? address, CancellationToken ct) => IsFullByIpAsync(address, null, ct);

    /// <summary>True when the IP axis, without the cart's placed unpaid orders, holds as many open orders as the ceiling allows.</summary>
    public async Task<bool> IsFullByIpAsync(IPAddress? address, Guid? exceptCartId, CancellationToken ct) => await CountByIpAsync(address, exceptCartId, ct) >= Ceiling;

    /// <summary>True when the e-mail axis holds as many open orders as the ceiling allows.</summary>
    public Task<bool> IsFullByMemberAsync(Guid customerId, CancellationToken ct) => IsFullByMemberAsync(customerId, null, ct);

    /// <summary>True when the e-mail axis, without the cart's placed unpaid orders, holds as many open orders as the ceiling allows.</summary>
    public async Task<bool> IsFullByMemberAsync(Guid customerId, Guid? exceptCartId, CancellationToken ct) => await CountByMemberAsync(customerId, exceptCartId, ct) >= Ceiling;
}
