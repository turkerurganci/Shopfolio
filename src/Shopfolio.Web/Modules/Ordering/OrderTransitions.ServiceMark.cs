using Shopfolio.Infrastructure;
using Shopfolio.Modules.Ordering.Contracts;

namespace Shopfolio.Modules.Ordering;

/// <summary>
/// The service item's "completed" mark of the transition unit (T351; 02 3.20.9, 3.20.12, 7.3.3; 03 8.2.9). It works in the unit's order (09 9.1 item 4): the order's row lock, the screen's
/// version, the operation's own condition, the mark, the closure rule at the end, the order's version raised once. It writes no transition of its own - the item holds the mark and has no
/// state beside it (02 3.20.10) - and the only record it can write is the closure's: the last open item's mark ends the order's line (S2 from Placed, S10 from Preparing, S11 from
/// DeliveryFailed) as the administrator's event. No notice goes (02 9.2).
/// </summary>
internal sealed partial class OrderTransitions
{
    public async Task<OrderTransitionResult> MarkServiceCompletedAsync(MarkServiceCompletedRequest request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (!TryAdminActor(request.Admin, out TransitionActor? actor))
        {
            return Refused(TransitionRefusal.ActorNotAllowed);
        }

        (Order? order, OrderItem? item, TransitionRefusal? locked) = await LockItemAsync(request.OrderId, request.OrderItemId, request.ExpectedVersion, ct);
        if (locked is { } lockRefusal)
        {
            return Refused(lockRefusal);
        }

        // The mark is a service item's, once, on its open quantity, while the payment is not Pending or Failed (02 3.20.9; K-536, K-671).
        IReadOnlyDictionary<Guid, int> closed = await _afterSales.ClosedQuantitiesAsync([item!.Id], ct); // non-null: LockItemAsync returns the order and the item when it returns no refusal
        int open = item.Quantity - closed.GetValueOrDefault(item.Id);
        if (!DeliveryMarkRules.IsServiceMarkOffered(item.ProductType, order!.PaymentStatus, open, item.DeliveryMarkedAt is not null)) // non-null: LockItemAsync returns the order and the item when it returns no refusal
        {
            return Refused(TransitionRefusal.ConditionNotMet);
        }

        // The mark and its date go together (the delivery_mark_and_date_together rule): the day of the mark in Turkey, as on a digital item at the approval.
        DateTimeOffset now = _time.GetUtcNow();
        item.DeliveryMarkedAt = now;
        item.DeliveryDate = TurkeyTime.DateOf(now);

        // The closure rule reads the mark just written (the same tracked item) and ends the line when this was the last open item; it raises the version itself when it writes a row.
        ClosureResult closure = await EvaluateClosureAsync(order, actor, ct);
        RaiseVersionOnce(order);
        return new OrderTransitionResult(null, closure.TransitionRecordId is { } recordId ? [recordId] : [], closure.Code, [item.Id]);
    }
}
