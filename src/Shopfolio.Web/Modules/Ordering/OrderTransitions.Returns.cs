using Shopfolio.Infrastructure;
using Shopfolio.Modules.AfterSales.Contracts;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Notifications.Contracts;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Modules.Payment.Contracts;

namespace Shopfolio.Modules.Ordering;

/// <summary>
/// The return's operations of the transition unit (T373; 03 1.7.1.6, 1.7.1.8, 1.4.4.6; 02 7.4.4, 7.4.5, 7.4.7, 7.4.9, 10.4.9): the receipt of a withdrawal's goods, the
/// "Stoğa ekle" step and the "mal dönmedi" closure. Each works in the unit's order (09 9.1 item 4): the order's row lock, the screen's version, the AfterSales writer (the
/// records and the IBAN are that module's - 09 6 item 2), the counters, the order's version raised once, then the notices. None changes an order status and none starts a
/// refund: after the delivery the withdrawal's refund is the firm's step, its period running from the arrival date (02 7.4.1). The return's shipping is the firm's and has no
/// setting, label, carrier or price here (02 7.4.2-7.4.4), and no deduction or loss of value is recorded (02 7.4.10): the operations carry no such field.
/// </summary>
internal sealed partial class OrderTransitions
{
    public async Task<ItemRecordResult> ReceiveReturnAsync(ReceiveReturnRequest request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (!TryAdminActor(request.Admin, out _))
        {
            return ItemRefused(TransitionRefusal.ActorNotAllowed);
        }

        // The arrival date may be in the past, never in the future (02 7.4.7); its lower bound - the item's entered delivery date - is the writer's.
        DateTimeOffset now = _time.GetUtcNow();
        if (request.ArrivalDate > TurkeyTime.DateOf(now))
        {
            return ItemRefused(TransitionRefusal.InvalidInput);
        }

        (Order? order, OrderItem? item, TransitionRefusal? refusal) = await LockItemAsync(request.OrderId, request.OrderItemId, request.ExpectedVersion, ct);
        if (refusal is { } locked)
        {
            return ItemRefused(locked);
        }

        ReturnReceiptEntry entry = new(
            Subject(order!, item!),
            request.WithdrawalId,
            request.Quantity,
            request.ArrivalDate,
            new ItemRecordAdmin(request.Admin.AdminId, request.Admin.AdminName.Trim()),
            now,
            order!.PaymentMethod == PaymentMethod.BankTransfer);
        ItemRecordWriteResult written = await _afterSales.AddReturnReceiptAsync(entry, ct);
        if (written.Refusal is { } refused)
        {
            return ItemRefused(Map(refused));
        }

        // 1.4.4.6: the received goods go back to the stock only by the firm's own step after its check (02 7.4.7); the coupon right goes back when this receipt
        // brings the order's last unit to its end (06 4.14; K-791, K-1104).
        await _counters.ReturnAsync(order, [], ct);
        RaiseVersionOnce(order);

        // The IBAN asked for again goes to the customer with B-14 (02 7.4.5; K-497); the receipt itself is told to no one (03 1.7.1.6).
        if (written.IsIbanRequested)
        {
            await QueueNoticeAsync(order.Id, new RefundIbanRequestedNotice(order.Id, RefundIbanRequestReason.ReturnAfterIbanDeleted), ct);
        }

        return new ItemRecordResult(null, written.RecordId);
    }

    public async Task<ItemRecordResult> CloseAsNotReturnedAsync(CloseAsNotReturnedRequest request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (!TryAdminActor(request.Admin, out _))
        {
            return ItemRefused(TransitionRefusal.ActorNotAllowed);
        }

        (Order? order, OrderItem? item, TransitionRefusal? refusal) = await LockItemAsync(request.OrderId, request.OrderItemId, request.ExpectedVersion, ct);
        if (refusal is { } locked)
        {
            return ItemRefused(locked);
        }

        NotReturnedClosureEntry entry = new(
            Subject(order!, item!), request.WithdrawalId, request.Quantity, new ItemRecordAdmin(request.Admin.AdminId, request.Admin.AdminName.Trim()), _time.GetUtcNow());
        ItemRecordWriteResult written = await _afterSales.AddNotReturnedClosureAsync(entry, ct);
        if (written.Refusal is { } refused)
        {
            return ItemRefused(Map(refused));
        }

        // No refund and no notice (02 10.4.9; 9.2); the declaration's IBAN goes with the closure unless a pending refund still uses it (K-497, K-1127).
        await DeleteUnusedIbansAsync(order!.Id, ct);
        RaiseVersionOnce(order);
        return new ItemRecordResult(null, written.RecordId);
    }

    public async Task<ItemRecordResult> AddReturnedStockAsync(AddReturnedStockRequest request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (!TryAdminActor(request.Admin, out _))
        {
            return ItemRefused(TransitionRefusal.ActorNotAllowed);
        }

        (Order? order, OrderItem? item, TransitionRefusal? refusal) = await LockItemAsync(request.OrderId, request.OrderItemId, request.ExpectedVersion, ct);
        if (refusal is { } locked)
        {
            return ItemRefused(locked);
        }

        // The step is not offered when the variant is gone (06 4.26 StockAddedAt): there is no counter to return the units to.
        if (item!.VariantId is not { } variant)
        {
            return ItemRefused(TransitionRefusal.ConditionNotMet);
        }

        ReturnStockResult marked = await _afterSales.MarkReturnStockAddedAsync(new ReturnStockEntry(Subject(order!, item), request.ReturnId, _time.GetUtcNow()), ct);
        if (marked.Refusal is { } refused)
        {
            return ItemRefused(Map(refused));
        }

        // 1.4.4.6: as many units as were received (02 7.4.7, 3.6.6; K-215).
        await _counters.ReturnAsync(order!, [new VariantQuantity(variant, marked.Quantity)], ct);
        RaiseVersionOnce(order!);
        return new ItemRecordResult(null, request.ReturnId);
    }
}
