using System.Net;

using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Security;
using Shopfolio.Infrastructure.Values;
using Shopfolio.Modules.Ordering.Contracts;

namespace Shopfolio.Modules.Ordering;

/// <summary>
/// L-7, the unpaid orders the system cancelled (02 8.2, 8.2.3, 3.17.8, 11.2 P-37; 05 6.3.2; T377 criterion 4): the number of orders whose cancellation (S3, by the system) was made
/// because the payment window ran out or the card payment failed, in the sliding window ending now, counted from the transition records - no attempt row. The cancellations the
/// document leaves out are not counted by their reason: the customer's and the firm's cancellation (their actor), the cancellation a new order of the same cart makes, and the
/// cancellation of an order whose payment page was never opened because the provider could not be reached (02 6.2.15; K-614) - the closed list <see cref="CountedReasons"/> is the only
/// thing that counts, so a new reason does not enter the limit by accident. The axes are <see cref="OrderAxes"/>': the IP axis counts every order, the e-mail axis a signed-in
/// member's orders only. A count at the maximum is full: with "5 in 24 hours" the sixth confirmation is the one that is refused, as L-8's fourth is (<see cref="AttemptLimits.OrderCountedOf"/>).
/// </summary>
internal sealed class UnpaidCancellationLimit
{
    /// <summary>The automatic cancellation reasons L-7 counts (02 8.2.3): the window that ran out and the card payment that failed - and no other.</summary>
    public static readonly IReadOnlyList<TransitionReason> CountedReasons = [TransitionReason.PaymentWindowExpired, TransitionReason.CardPaymentFailed];

    private readonly AppDbContext _db;
    private readonly TimeProvider _time;

    public UnpaidCancellationLimit(AppDbContext db, TimeProvider time)
    {
        ArgumentNullException.ThrowIfNull(db);
        ArgumentNullException.ThrowIfNull(time);
        _db = db;
        _time = time;
    }

    /// <summary>The maximum of the limit (P-37).</summary>
    public static int Max => AttemptLimits.OrderCountedOf(AttemptLimit.L7).Max;

    /// <summary>The counted cancellations of the IP axis of <paramref name="address"/>; no address counts none (a live order always holds its address, 06 4.17).</summary>
    public async Task<int> CountByIpAsync(IPAddress? address, CancellationToken ct)
    {
        return address is null ? 0 : await CountedOrders(OrderAxes.OfIp(_db.Orders.AsNoTracking(), address)).CountAsync(ct);
    }

    /// <summary>The counted cancellations of the e-mail axis: the orders the signed-in member placed, read by the member's id (D-808); a guest order is not counted.</summary>
    public async Task<int> CountByMemberAsync(Guid customerId, CancellationToken ct)
    {
        return await CountedOrders(OrderAxes.OfMember(_db.Orders.AsNoTracking(), customerId)).CountAsync(ct);
    }

    /// <summary>True when the IP axis holds as many counted cancellations as the limit allows.</summary>
    public async Task<bool> IsFullByIpAsync(IPAddress? address, CancellationToken ct) => await CountByIpAsync(address, ct) >= Max;

    /// <summary>True when the e-mail axis holds as many counted cancellations as the limit allows.</summary>
    public async Task<bool> IsFullByMemberAsync(Guid customerId, CancellationToken ct) => await CountByMemberAsync(customerId, ct) >= Max;

    // The orders of the axis that have a counted cancellation in the window. The window ends now and starts one window ago; a row exactly one window old still counts, like the
    // attempt record's rows (AttemptCounter.IsExceededAsync).
    private IQueryable<Order> CountedOrders(IQueryable<Order> axis)
    {
        DateTimeOffset from = _time.GetUtcNow() - AttemptLimits.OrderCountedOf(AttemptLimit.L7).Window!.Value;
        TransitionReason[] counted = [.. CountedReasons];
        IQueryable<Guid> cancelled = _db.TransitionRecords.AsNoTracking()
            .Where(t => t.TransitionCode == TransitionCode.S3
                && t.ActorKind == ActorKind.System
                && t.TransitionReason != null
                && counted.Contains(t.TransitionReason.Value)
                && t.CreatedAt >= from)
            .Select(t => t.OrderId);
        return axis.Where(o => cancelled.Contains(o.Id));
    }
}
