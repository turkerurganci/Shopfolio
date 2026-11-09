using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Operations;
using Shopfolio.Modules.AfterSales.Contracts;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Notifications.Contracts;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Modules.Payment.Contracts;

namespace Shopfolio.Modules.Ordering;

/// <summary>
/// The customer's cancellation of a paid item's units (T367; 03 1.11.2-1.11.4; 02 7.2.1, 7.2.2, 7.2.5-7.2.7). It works in the unit's order (09 9.1 item 4): the order's
/// row lock, the operation decision's one rule asked again under the lock (<see cref="OrderOperations.CheckAsync"/>; 09 8.3 item 3 - the screen draws from the same
/// rule), the AfterSales writer of the cancellation record, the order's version raised once, then the order's line, then the counters. The line has one path per
/// event (D-306): the cancellation that leaves no open unit and no delivered item is the customer's own S3 or S4 (<see cref="CancelOrderAsync"/>: its B-7, and the
/// record is linked to it); any other cancellation sends its B-7 naming the item and reads the closure rule (a mixed order whose physical units are all cancelled
/// goes on along its delivered items' line, 02 7.2.2). The counters come last: the variant and coupon locks follow the order's (05 7.3.5).
/// </summary>
internal sealed partial class OrderTransitions
{
    public async Task<OrderTransitionResult> CancelItemAsync(CancelItemRequest request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);

        // The quantity's field rule (at least one; 09 3.4) comes before the lock: a quantity under one is a wrong value, not a closed operation.
        if (request.Quantity < 1)
        {
            return Refused(TransitionRefusal.InvalidInput);
        }

        (Order? order, OrderItem? item, TransitionRefusal? refusal) = await LockItemAsync(request.OrderId, request.OrderItemId, null, ct);
        if (refusal is not null || order is null || item is null)
        {
            return Refused(refusal ?? TransitionRefusal.OrderNotFound);
        }

        OperationVerdict verdict = await _operations.CheckAsync(order.Id, CancellationOperationOf(item.ProductType), item.Id, request.Quantity, ct);
        if (!verdict.IsAllowed)
        {
            return Refused(verdict.Decision == OperationDecision.QuantityExceedsOpen ? TransitionRefusal.InvalidInput : TransitionRefusal.ConditionNotMet);
        }

        // The transfer line's refund goes to the customer's IBAN, taken with the cancellation (T370; 02 7.2.8, 7.4.5; E-16): format and check digits; the card
        // line asks for no IBAN and opens no field. An IBAN given on the card line is not kept.
        bool transferRefund = order.PaymentMethod == PaymentMethod.BankTransfer
            && order.PaymentStatus is PaymentStatus.Paid or PaymentStatus.PartiallyRefunded;
        string? iban = null;
        if (transferRefund)
        {
            if (string.IsNullOrWhiteSpace(request.Iban) || !IbanRule.Allows(request.Iban))
            {
                return Refused(TransitionRefusal.InvalidInput);
            }

            iban = RequestFormat.CompactIban(request.Iban);
        }

        DateTimeOffset now = _time.GetUtcNow();
        ItemRecordWriteResult written = await _afterSales.AddCancellationAsync(
            new CustomerCancellationEntry(Subject(order, item), request.Quantity, FrozenOf(item), now, iban), ct);
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
        if (noOpenUnit && !anyDelivered && order.OrderStatus is OrderStatus.Placed or OrderStatus.Preparing)
        {
            // The last open units: the customer's S3 (from Placed) or S4 (from Preparing) with its B-7 (03 1.2 S3, S4); the closure rule does not write it too.
            // A returned shipment (DeliveryFailed) whose last open unit is a service's is not the customer's to close: the firm's reasonless S9 closes it
            // (03 1.2 S9), so that cancellation takes the other branch.
            OrderTransitionResult closing = await CancelOrderAsync(CancelOrderRequest.ByCustomer(order.Id, cancelled), ct);
            if (closing.IsRejected)
            {
                return closing;
            }

            recordIds.AddRange(closing.TransitionRecordIds);
            await _afterSales.LinkClosingTransitionAsync(cancellationId, closing.TransitionRecordIds[0], ct);
        }
        else
        {
            // The order lives on: B-7 is the customer's copy of the item's cancellation (02 7.1.4; 08 2.2.6 B-7), and the closure rule decides the line.
            await QueueNoticeAsync(order.Id, new CancelledNotice(order.Id, ActorKind.Buyer, null, null, null, cancelled), ct);
            ClosureResult closure = await EvaluateClosureAsync(order, TransitionActor.Buyer, ct);
            if (closure.TransitionRecordId is { } closureId)
            {
                recordIds.Add(closureId);
                closureCode = closure.Code;
            }
        }

        // The refund starts with the record (T370; 02 7.2.8): on the card the system starts it without approval and no IBAN field; on the transfer the firm
        // processes it to the IBAN that came with the cancellation.
        await StartRefundOfRecordAsync(order, item, RefundType.Cancellation, cancellationId, iban is not null, null, RefundIbanRequestReason.CancellationOrRemoval, ct);

        // 1.4.4.4: the cancelled units go back to stock or the service capacity (a digital item has no counter, 02 7.2.6); the coupon right goes back only when this
        // record closes the whole order (OrderCounters.IsCouponReturnDueAsync; 02 5.11.3, 7.2.7).
        await _counters.ReturnAsync(order, UnitsOf(item, request.Quantity), ct);

        return new OrderTransitionResult(null, recordIds, closureCode, [item.Id]) { ItemRecordId = cancellationId };
    }

    /// <summary>The customer's cancellation row of 03 1.11 the item's type names: 1.11.2 physical, 1.11.3 service, 1.11.4 digital (always closed).</summary>
    private static OrderOperation CancellationOperationOf(ProductType type) => type switch
    {
        ProductType.Physical => OrderOperation.CancelPhysicalItem,
        ProductType.Service => OrderOperation.CancelServiceItem,
        ProductType.Digital => OrderOperation.CancelDigitalItem,
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Unknown product type."),
    };
}
