using Shopfolio.Modules.Ordering.Contracts;

namespace Shopfolio.Modules.Ordering;

/// <summary>
/// The one rule of which orders the customer's order list shows (T348 criterion 8; 02 3.17.8, 6.2.15; K-184, K-528, K-589, K-614): an order that was never paid and that the system
/// cancelled by itself - its payment window ran out, its card payment failed, its payment page never opened, or a new order was confirmed from the same cart - is not shown to the
/// customer; the firm's panel shows every order and does not use this rule. An unpaid order the customer or the firm cancelled, and an order that was paid once and cancelled later -
/// a late card payment the system refunded included (an Ö1 record exists) - stay in the list. The customer's list (T518) reads its orders through <see cref="ShownToCustomer"/>.
/// </summary>
internal static class OrderVisibility
{
    /// <summary>The system's cancellation reasons that hide a never-paid order (02 3.17.8, 6.2.15).</summary>
    public static readonly IReadOnlyList<TransitionReason> HidingReasons =
    [
        TransitionReason.PaymentWindowExpired,
        TransitionReason.CardPaymentFailed,
        TransitionReason.PaymentPageNotOpened,
        TransitionReason.SameCartReorder,
    ];

    /// <summary>
    /// <paramref name="orders"/> without the never-paid orders the system cancelled by itself: no Ö1 record and an S3 or Ö2 record the system wrote with one of
    /// <see cref="HidingReasons"/>. Translates to one query; nothing is read into memory.
    /// </summary>
    public static IQueryable<Order> ShownToCustomer(IQueryable<Order> orders, IQueryable<TransitionRecord> records)
    {
        ArgumentNullException.ThrowIfNull(orders);
        ArgumentNullException.ThrowIfNull(records);
        TransitionReason[] hiding = [.. HidingReasons];
        return orders.Where(o =>
            records.Any(r => r.OrderId == o.Id && r.TransitionCode == TransitionCode.O1)
            || !records.Any(r => r.OrderId == o.Id
                && (r.TransitionCode == TransitionCode.S3 || r.TransitionCode == TransitionCode.O2)
                && r.ActorKind == ActorKind.System
                && r.TransitionReason != null
                && hiding.Contains(r.TransitionReason.Value)));
    }
}
