using Shopfolio.Modules.Ordering.Contracts;

namespace Shopfolio.Modules.Ordering;

/// <summary>The payment axis of the firm's refund step on the panel (T529; 07 UC-131, UC-133): the transfer refund born and sent with the step moves the axis by the one rule of the sent refund.</summary>
internal sealed partial class OrderTransitions
{
    public async Task<PaymentTransitionResult> ApplyFirmRefundSentAsync(FirmRefundSent sent, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(sent);
        if (!TryAdminActor(sent.Admin, out TransitionActor? actor))
        {
            return new PaymentTransitionResult([], isNotOpen: true);
        }

        Order? order = await LockAndLoadAsync(sent.OrderId, ct);
        if (order is null || sent.Refund.OrderId != order.Id || !sent.Refund.IsSent)
        {
            return new PaymentTransitionResult([], isOrderNotFound: order is null, isNotOpen: order is not null);
        }

        PaymentTransitionResult applied = await ApplyRefundSentAsync(order, sent.Refund, actor, ct);
        if (!applied.IsRejected)
        {
            // The IBAN goes once the last pending refund that uses it is processed, each deletion with its disposal record (T370; 02 7.4.5; K-1110; Z-44).
            await DeleteUnusedIbansAsync(order.Id, ct);
        }

        return applied;
    }
}
