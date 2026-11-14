using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Modules.Payment.Contracts;

namespace Shopfolio.Modules.Ordering;

/// <summary>
/// The order confirmation and waiting screen's reads and its one write (07 UC-28, UC-29; 04 5.14; T510). The read is plain; the unopened form's cancellation takes the
/// order's lock, asks the state again under it - the payment's notification or another tab may have moved the order meanwhile - and writes the cart's mark in the same
/// transaction, so the cart never says "try again later" about an order that was not cancelled.
/// </summary>
internal sealed class OrderConfirmationScreens : IOrderConfirmationScreen
{
    private readonly AppDbContext _db;
    private readonly OrderLocks _locks;
    private readonly IOrderTransitions _transitions;
    private readonly Carts _carts;

    public OrderConfirmationScreens(AppDbContext db, OrderLocks locks, IOrderTransitions transitions, Carts carts)
    {
        _db = db;
        _locks = locks;
        _transitions = transitions;
        _carts = carts;
    }

    public async Task<ConfirmationScreenFacts?> ReadAsync(Guid orderId, CancellationToken ct)
    {
        var order = await _db.Orders.AsNoTracking()
            .Where(o => o.Id == orderId)
            .Select(o => new
            {
                o.Id,
                o.OrderNumber,
                o.OrderStatus,
                o.PaymentStatus,
                o.PaymentMethod,
                o.Total,
                o.BankTransferIban,
                o.PaymentWindowEndsAt,
                o.ContactEmail,
                o.CompanyEmail,
                o.BillingRecipientName,
                o.BillingAddressLine,
                o.ShippingPhone,
                o.CompanyPhone,
                o.OrderAccessKey,
            })
            .SingleOrDefaultAsync(ct);
        if (order is null)
        {
            return null;
        }

        return new ConfirmationScreenFacts(
            order.Id,
            order.OrderNumber,
            IsAwaiting(order.OrderStatus, order.PaymentStatus),
            order.PaymentMethod,
            order.Total,
            order.PaymentMethod == PaymentMethod.BankTransfer ? order.BankTransferIban : null,
            order.PaymentWindowEndsAt,
            order.ContactEmail,
            order.CompanyEmail,
            order.BillingRecipientName,
            order.BillingAddressLine,
            order.ShippingPhone,
            order.CompanyPhone,
            order.OrderAccessKey);
    }

    public async Task<UnopenedPaymentCancellationResult> CancelUnopenedAsync(Guid orderId, TransitionReason reason, CartOwner? cartOwner, CancellationToken ct)
    {
        // 05 7.3.5: the cart's lock is the head of the order - the requesting browser's cart is locked before the order and its variants, and marked under that lock.
        Guid? cartId = cartOwner is null ? null : await _carts.LockCartAsync(cartOwner, ct);
        await _locks.LockAsync([orderId], ct);
        var state = await _db.Orders.AsNoTracking()
            .Where(o => o.Id == orderId)
            .Select(o => new { o.OrderStatus, o.PaymentStatus })
            .SingleOrDefaultAsync(ct);
        if (state is null || !IsAwaiting(state.OrderStatus, state.PaymentStatus))
        {
            return new UnopenedPaymentCancellationResult(UnopenedPaymentCancellation.NoLongerAwaiting);
        }

        OrderTransitionResult cancelled = await _transitions.CancelOrderAsync(CancelOrderRequest.BySystem(orderId, reason), ct);
        if (cancelled.IsRejected)
        {
            return new UnopenedPaymentCancellationResult(UnopenedPaymentCancellation.NoLongerAwaiting);
        }

        // 07 UC-29; K-1200, K-1212 (2): the requesting browser's cart tells it once; a browser without a cart is told nothing (the remaining risk of 06 4.15).
        if (cartId is Guid lockedCart)
        {
            await _carts.MarkPaymentUnavailableLockedAsync(lockedCart, ct);
        }

        return new UnopenedPaymentCancellationResult(UnopenedPaymentCancellation.Cancelled);
    }

    // Alındı + Bekliyor, the one state the screen opens in (07 UC-28; K-1209 (1)).
    private static bool IsAwaiting(OrderStatus orderStatus, PaymentStatus paymentStatus) => orderStatus == OrderStatus.Placed && paymentStatus == PaymentStatus.Pending;
}
