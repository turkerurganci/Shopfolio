using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure.Operations;
using Shopfolio.Modules.AfterSales.Contracts;
using Shopfolio.Modules.Notifications.Contracts;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Modules.Payment.Contracts;

namespace Shopfolio.Modules.Ordering;

/// <summary>
/// The firm's removal of an item or the reduction of its quantity (T386; 02 10.4.3; 03 1.11.22; K-374, K-537, K-577, K-604): a correction that follows the cancellation's
/// regime and asks for no reason. No item is added and no price changes - the request carries neither. It works in the unit's order (09 9.1 item 4): the order's row lock
/// and the screen's version, the operation decision's one rule asked again under the lock (open from the payment's approval; a physical item until "Kargoya verildi", a
/// digital or service item until its delivery mark), the irreversible-action confirmation when the refund goes to the card (02 5.9; K-225), the AfterSales writer of the
/// removal record, the order's version raised once, the closure rule (the removed item counts as cancelled: S3, S4 or S10), B-11, the refund of the record (02 7.1.7,
/// 7.2.8; the shipping fee by 7.2.9's trigger; the transfer line's IBAN request with the record, K-690) and the counters (stock, service capacity and coupon right as on
/// a cancellation, 1.4.4.4). The trail row is the caller's (<see cref="OrderCorrections"/>).
/// </summary>
internal sealed partial class OrderTransitions
{
    internal async Task<OrderTransitionResult> RemoveItemAsync(RemoveItemRequest request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (!TryAdminActor(request.Admin, out TransitionActor? actor))
        {
            return Refused(TransitionRefusal.ActorNotAllowed);
        }

        if (request.Quantity < 1)
        {
            return Refused(TransitionRefusal.InvalidInput);
        }

        (Order? order, OrderItem? item, TransitionRefusal? refusal) = await LockItemAsync(request.OrderId, request.OrderItemId, request.ExpectedVersion, ct);
        if (refusal is not null || order is null || item is null)
        {
            return Refused(refusal ?? TransitionRefusal.OrderNotFound);
        }

        // 1.11.22 on the locked order: before the payment's approval the order is cancelled whole (02 7.2.2); a physical item not after "Kargoya verildi", a delivered digital
        // or completed service item never (02 10.4.3 (2), (5)).
        OperationVerdict verdict = await _operations.CheckAsync(order.Id, OrderOperation.RemoveOrReduceItem, item.Id, request.Quantity, ct);
        if (!verdict.IsAllowed)
        {
            return Refused(verdict.Decision == OperationDecision.QuantityExceedsOpen ? TransitionRefusal.InvalidInput : TransitionRefusal.ConditionNotMet);
        }

        // A removal that sends money to the card is irreversible and asks the panel's confirmation (02 10.4.3 (3), 5.9; 03 1.6.1).
        bool refundsToCard = order.PaymentMethod == PaymentMethod.Card && order.PaymentStatus is PaymentStatus.Paid or PaymentStatus.PartiallyRefunded;
        if (refundsToCard && !request.Confirmed)
        {
            return Refused(TransitionRefusal.ConfirmationRequired);
        }

        DateTimeOffset now = _time.GetUtcNow();
        ItemRecordWriteResult written = await _afterSales.AddItemRemovalAsync(
            new ItemRemovalEntry(Subject(order, item), request.Quantity, FrozenOf(item), new ItemRecordAdmin(request.Admin.AdminId, request.Admin.AdminName.Trim()), now), ct);
        if (written.Refusal is { } refused)
        {
            return Refused(Map(refused));
        }

        Guid removalId = written.RecordId ?? throw new InvalidOperationException("The removal writer returned no record.");
        RaiseVersionOnce(order);

        // B-11 names the item and the removed units, no amount (08 2.2.6 B-11); the refund has its own notice.
        await QueueNoticeAsync(order.Id, new ItemRemovedNotice(order.Id, new NoticeItem(item.ProductName, item.FirstOptionValue, item.SecondOptionValue, request.Quantity)), ct);

        // The removed item counts as a cancelled one on the shipping line (02 10.4.3 (1); 5.4 S3, S4, S10): the closure rule decides whether the order closes.
        List<Guid> recordIds = [];
        ClosureResult closure = await EvaluateClosureAsync(order, actor, ct);
        if (closure.TransitionRecordId is { } closureId)
        {
            recordIds.Add(closureId);
        }

        // The refund starts with the record (02 7.2.8): on the card by the system, on the transfer with the IBAN request and B-14 (K-690).
        await StartRefundOfRecordAsync(order, item, RefundType.ItemRemoval, removalId, false, request.Admin, RefundIbanRequestReason.CancellationOrRemoval, ct);

        // 1.4.4.4: the removed units go back to stock or the service capacity; the coupon right only with the order's last unit (02 7.2.6).
        await _counters.ReturnAsync(order, UnitsOf(item, request.Quantity), ct);

        // Whether the item has units left decides the trail's operation: a reduction or a removal (02 10.4.3).
        IReadOnlyDictionary<Guid, int> closed = await _afterSales.ClosedQuantitiesAsync([item.Id], ct);
        bool itemLeft = item.Quantity - closed.GetValueOrDefault(item.Id) > 0;
        return new OrderTransitionResult(null, recordIds, closure.Code, [item.Id]) { ItemRecordId = removalId, ItemFullyRemoved = !itemLeft };
    }
}
