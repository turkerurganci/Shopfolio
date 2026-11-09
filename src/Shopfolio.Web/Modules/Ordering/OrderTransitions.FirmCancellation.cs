using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure.Operations;
using Shopfolio.Modules.AfterSales.Contracts;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Notifications.Contracts;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Modules.Payment.Contracts;

namespace Shopfolio.Modules.Ordering;

/// <summary>
/// The firm's cancellation after the payment (T368; 03 1.11.21, 8.3.1.1; 02 7.2.4, 7.2.6; D-305): by item and units from the panel, and the side of the whole-order
/// cancellation of a paid order that the item records carry. It works in the unit's order (09 9.1 item 4): the order's row lock and the screen's version, the operation
/// decision's one rule asked again under the lock (<see cref="OrderOperations.CheckAsync"/>), the AfterSales writer of the cancellation record, the order's version
/// raised once, then the order's line, then the counters (1.4.4.4). The reason is from the closed list (<see cref="FirmReasonRules"/>) and goes to the customer with
/// B-7 - the legal notice of the impossibility of performance at the moment of the cancellation (02 7.2.4; Yönetmelik m.16/4).
/// </summary>
internal sealed partial class OrderTransitions
{
    public async Task<OrderTransitionResult> CancelItemByFirmAsync(CancelItemByFirmRequest request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (!TryAdminActor(request.Admin, out TransitionActor? actor))
        {
            return Refused(TransitionRefusal.ActorNotAllowed);
        }

        if (!request.Confirmed)
        {
            return Refused(TransitionRefusal.ConfirmationRequired);
        }

        // The field rules come before the lock: a quantity under one, a missing note on "Other" or a missing confirmation of the owner are wrong values (09 3.4).
        if (request.Quantity < 1 || !FirmReasonRules.IsComplete(request.Reason, request.ReasonNote, request.OwnerConfirmed))
        {
            return Refused(TransitionRefusal.InvalidInput);
        }

        string? note = request.Reason == CancellationReason.Other ? request.ReasonNote!.Trim() : null;
        (Order? order, OrderItem? item, TransitionRefusal? refusal) = await LockItemAsync(request.OrderId, request.OrderItemId, request.ExpectedVersion, ct);
        if (refusal is not null || order is null || item is null)
        {
            return Refused(refusal ?? TransitionRefusal.OrderNotFound);
        }

        OperationVerdict verdict = await _operations.CheckAsync(order.Id, OrderOperation.CancelByFirm, item.Id, request.Quantity, ct);
        if (!verdict.IsAllowed)
        {
            return Refused(verdict.Decision == OperationDecision.QuantityExceedsOpen ? TransitionRefusal.InvalidInput : TransitionRefusal.ConditionNotMet);
        }

        DateTimeOffset now = _time.GetUtcNow();
        ItemRecordAdmin admin = new(request.Admin.AdminId, request.Admin.AdminName);
        ItemRecordWriteResult written = await _afterSales.AddFirmCancellationAsync(
            new FirmCancellationEntry(Subject(order, item), request.Quantity, FrozenOf(item), admin, request.Reason, note, now), ct);
        if (written.Refusal is { } refused)
        {
            return Refused(Map(refused));
        }

        Guid cancellationId = written.RecordId ?? throw new InvalidOperationException("The cancellation writer returned no record.");

        RaiseVersionOnce(order);

        List<OrderItem> items = await _db.OrderItems.Where(i => i.OrderId == order.Id).ToListAsync(ct);
        IReadOnlyDictionary<Guid, int> closed = await _afterSales.ClosedQuantitiesAsync([.. items.Select(i => i.Id)], ct);
        bool noOpenUnit = items.All(i => i.Quantity - closed.GetValueOrDefault(i.Id) <= 0);
        bool anyDelivered = items.Any(i => i.DeliveryMarkedAt is not null);
        CancelledItem cancelled = new(item.ProductName, item.FirstOptionValue, item.SecondOptionValue, request.Quantity);

        List<Guid> recordIds = [];
        TransitionCode? closureCode = null;
        if (noOpenUnit && !anyDelivered && order.OrderStatus is OrderStatus.Placed or OrderStatus.Preparing or OrderStatus.DeliveryFailed)
        {
            // The last open units: the firm's S3, S4 or S9 with its reason and its one B-7 naming the item (03 1.2); the record is linked to it (06 4.22).
            CancelOrderRequest closingRequest = CancelOrderRequest.ByAdministrator(
                order.Id, request.Admin, order.Version, true, request.Reason, note, cancelled, request.OwnerConfirmed);
            OrderTransitionResult closing = await CancelOrderAsync(closingRequest, ct);
            if (closing.IsRejected)
            {
                return closing;
            }

            recordIds.AddRange(closing.TransitionRecordIds);
            await _afterSales.LinkClosingTransitionAsync(cancellationId, closing.TransitionRecordIds[0], ct);
        }
        else
        {
            // The order lives on: B-7 tells the customer the item, the units and the firm's reason (02 7.2.4; 08 2.2.6 B-7), and the closure rule decides the line.
            await QueueNoticeAsync(order.Id, new CancelledNotice(order.Id, ActorKind.Admin, request.Reason, note, null, cancelled), ct);
            ClosureResult closure = await EvaluateClosureAsync(order, actor!, ct);
            if (closure.TransitionRecordId is { } closureId)
            {
                recordIds.Add(closureId);
                closureCode = closure.Code;
            }
        }

        // The refund starts with the record (T370; 02 7.2.8): on the card by the system, on the transfer with the IBAN request and B-14 (K-690).
        await StartRefundOfRecordAsync(order, item, RefundType.Cancellation, cancellationId, false, request.Admin, RefundIbanRequestReason.CancellationOrRemoval, ct);

        // 1.4.4.4: the cancelled units go back to stock or the service capacity; the coupon right only with the order's last unit (02 7.2.6).
        await _counters.ReturnAsync(order, UnitsOf(item, request.Quantity), ct);
        return new OrderTransitionResult(null, recordIds, closureCode, [item.Id]) { ItemRecordId = cancellationId };
    }

    /// <summary>
    /// The item side of the firm's whole-order cancellation of a paid order (D-305; 02 7.2.4, 7.2.6): every open item's open units get a firm cancellation record with the
    /// same reason, linked to the transition that closed the order, and the units go back (1.4.4.4). Called after the transition, in its transaction and under its lock.
    /// </summary>
    private async Task CancelOpenItemsByFirmAsync(Guid orderId, Guid transitionRecordId, CancelOrderRequest request, CancellationToken ct)
    {
        Order order = await _db.Orders.SingleAsync(o => o.Id == orderId, ct);
        List<OrderItem> items = await _db.OrderItems.Where(i => i.OrderId == orderId).OrderBy(i => i.CreatedAt).ThenBy(i => i.Id).ToListAsync(ct);
        IReadOnlyDictionary<Guid, int> closed = await _afterSales.ClosedQuantitiesAsync([.. items.Select(i => i.Id)], ct);
        ItemRecordAdmin admin = new(request.Admin!.AdminId, request.Admin.AdminName);
        string? note = request.FirmReason == CancellationReason.Other ? request.ReasonNote!.Trim() : null;
        DateTimeOffset now = _time.GetUtcNow();
        List<VariantQuantity> units = [];
        foreach (OrderItem item in items)
        {
            int open = item.Quantity - closed.GetValueOrDefault(item.Id);
            if (open <= 0)
            {
                continue;
            }

            ItemRecordWriteResult written = await _afterSales.AddFirmCancellationAsync(
                new FirmCancellationEntry(Subject(order, item), open, FrozenOf(item), admin, request.FirmReason!.Value, note, now), ct);
            Guid cancellationId = written.RecordId ?? throw new InvalidOperationException("The whole-order cancellation's item record was refused under the order's lock.");
            await _afterSales.LinkClosingTransitionAsync(cancellationId, transitionRecordId, ct);
            await StartRefundOfRecordAsync(order, item, RefundType.Cancellation, cancellationId, false, request.Admin, RefundIbanRequestReason.CancellationOrRemoval, ct);
            units.AddRange(UnitsOf(item, open));
        }

        await _counters.ReturnAsync(order, units, ct);
    }

    private static ItemFrozenAmounts FrozenOf(OrderItem item) => new(item.DiscountedUnitPrice ?? item.UnitPrice, item.CouponShare, item.VatAmount);

    /// <summary>The units an item record gives back to a counter: a physical item's stock or a service item's capacity; a digital item has no counter (02 7.2.6).</summary>
    private static IReadOnlyList<VariantQuantity> UnitsOf(OrderItem item, int quantity) =>
        item.ProductType != ProductType.Digital && item.VariantId is { } variantId ? [new VariantQuantity(variantId, quantity)] : [];
}
