using System.Globalization;

using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Forms;
using Shopfolio.Infrastructure.Operations;
using Shopfolio.Modules.Administration.Contracts;
using Shopfolio.Modules.AfterSales.Contracts;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Notifications.Contracts;
using Shopfolio.Modules.Ordering.Contracts;

namespace Shopfolio.Modules.Ordering;

/// <summary>
/// The administrator's interventions of T386 (02 10.4.3-10.4.5, 10.4.7): the removal of an item or the reduction of its quantity (the transition unit writes it, this file
/// writes its trail row), the carrier correction with B-12, the order-level delivery date correction and a return's arrival date correction (no notice, the trail keeps the
/// old and the new date) and the internal note (never shown to the customer; its trail row carries no text). They work in the unit's order (09 9.1 item 4): the order's row
/// lock, the screen's version, the operation decision of 03 1.11 on the locked order, the change, the order's version raised once, the notice, the trail. The note is the
/// exception of 09 10.9: a shared row lock, no version, and a write conditional on the text the form showed (<see cref="SaveInternalNote"/>).
/// </summary>
internal sealed partial class OrderCorrections
{
    public async Task<OrderTransitionResult> RemoveItemAsync(RemoveItemRequest request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        OrderTransitionResult result = await _transitions.RemoveItemAsync(request, ct);
        if (result.IsRejected)
        {
            return result;
        }

        // The trail names the item and the units, never an amount's owner or a name (02 10.3.1; 10.4.3: "iz bırakır"); the reduction and the removal are two operations.
        string orderNumber = await _db.Orders.AsNoTracking().Where(o => o.Id == request.OrderId).Select(o => o.OrderNumber).SingleAsync(ct);
        await _trail.WriteAsync(
            new AuditEntry(
                result.ItemFullyRemoved ? AuditOperation.RemoveItem : AuditOperation.ReduceItemQuantity,
                nameof(Order),
                request.OrderId,
                new AuditDetails(
                    orderNumber,
                    values: [new AuditValue("ItemId", request.OrderItemId.ToString("D")), new AuditValue("Quantity", request.Quantity.ToString(CultureInfo.InvariantCulture))])),
            ct);
        return result;
    }

    public async Task<OrderCorrectionResult> CorrectShipmentAsync(CorrectShipmentRequest request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (!IsAdmin(request.Admin))
        {
            return Refused(TransitionRefusal.ActorNotAllowed);
        }

        // The values are the shipment's own (02 5.4 S5): the own-vehicle declaration alone, or one company with the tracking number.
        if (request.Shipment is null || !OrderTransitions.TryNormalizeShipment(request.Shipment, out ShipmentInfo? shipment))
        {
            return Refused(TransitionRefusal.InvalidInput);
        }

        (Order? order, TransitionRefusal? locked) = await LockAsync(request.OrderId, request.ExpectedVersion, ct);
        if (locked is { } lockRefusal)
        {
            return Refused(lockRefusal);
        }

        // 1.11.24 on the locked order: a shipped order with a physical item.
        OperationVerdict verdict = await _operations.CheckAsync(order!.Id, OrderOperation.CorrectCarrier, null, null, ct); // non-null: LockAsync returns the order when it returns no refusal
        if (verdict.Decision != OperationDecision.Allowed)
        {
            return Refused(TransitionRefusal.ConditionNotMet);
        }

        ShipmentInfo current = new(order.Carrier, order.OtherCarrierName, order.TrackingNumber, order.IsOwnVehicleDelivery);
        if (current == shipment)
        {
            // Nothing to correct: B-12 with the same values would only confuse the customer.
            return Refused(TransitionRefusal.InvalidInput);
        }

        order.RecordShipment(shipment.Carrier, shipment.OtherCarrierName, shipment.TrackingNumber, shipment.IsOwnVehicleDelivery);
        OrderTransitions.RaiseVersionOnce(_db, order);

        // B-12 carries the corrected values in B-5's form (02 10.4.4; 08 2.2.6 B-12); the trail names the field only - the number and a company's name are free text.
        if (!string.IsNullOrWhiteSpace(order.ContactEmail))
        {
            await _notifications.QueueCustomerNoticeAsync(
                new CarrierCorrectedNotice(order.Id, shipment.Carrier, shipment.OtherCarrierName, shipment.TrackingNumber, shipment.IsOwnVehicleDelivery), order.ContactEmail, ct);
        }

        await _trail.WriteAsync(
            new AuditEntry(AuditOperation.CorrectShippingInfo, nameof(Order), order.Id, new AuditDetails(order.OrderNumber, values: [new AuditValue("Field", "ShippingInfo")])),
            ct);
        return new OrderCorrectionResult(null, [], null);
    }

    public async Task<OrderCorrectionResult> CorrectDeliveryDateAsync(CorrectDeliveryDateRequest request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (!IsAdmin(request.Admin))
        {
            return Refused(TransitionRefusal.ActorNotAllowed);
        }

        DateOnly today = TurkeyTime.DateOf(_time.GetUtcNow());
        if (DeliveryMarkRules.CheckDeliveryDate(request.DeliveryDate, today, null) is { } future)
        {
            return DateRefused(future);
        }

        (Order? order, TransitionRefusal? locked) = await LockAsync(request.OrderId, request.ExpectedVersion, ct);
        if (locked is { } lockRefusal)
        {
            return Refused(lockRefusal);
        }

        OperationVerdict verdict = await _operations.CheckAsync(order!.Id, OrderOperation.CorrectDeliveryOrArrivalDate, null, null, ct); // non-null: LockAsync returns the order when it returns no refusal
        List<OrderItem> delivered = await _db.OrderItems
            .Where(i => i.OrderId == order.Id && i.ProductType == ProductType.Physical && i.DeliveryDate != null)
            .ToListAsync(ct);
        if (verdict.Decision != OperationDecision.Allowed || delivered.Count == 0)
        {
            return Refused(TransitionRefusal.ConditionNotMet);
        }

        // Not before the day of the last shipment - the delivery mark's own rule (02 3.20.11) -, and not after a recorded arrival of returned goods (K-1474).
        DateTimeOffset? shippedAt = await OrderPeriods.Shipments(_db).Where(t => t.OrderId == order.Id).MaxAsync(t => (DateTimeOffset?)t.CreatedAt, ct);
        if (DeliveryMarkRules.CheckDeliveryDate(request.DeliveryDate, today, shippedAt is { } at ? TurkeyTime.DateOf(at) : null) is { } obstacle)
        {
            return DateRefused(obstacle);
        }

        Guid[] itemIds = [.. delivered.Select(i => i.Id)];
        IReadOnlyList<ArrivalRecordDate> arrivals = await _afterSales.ArrivalRecordDatesAsync(itemIds, ct);
        if (arrivals.Any(a => request.DeliveryDate > a.ArrivalDate))
        {
            return DateRefused(DeliveryDateObstacle.AfterReturnArrival);
        }

        DateOnly oldDate = delivered.Select(i => i.DeliveryDate!.Value).Max(); // non-null: delivered holds only items with a delivery date (the Where above)
        if (delivered.All(i => i.DeliveryDate == request.DeliveryDate))
        {
            return Refused(TransitionRefusal.InvalidInput);
        }

        // On a declaration's day the firm says which came first (K-608; 02 7.4.1); without the answer nothing is written. The declaration itself stays (02 7.6.7).
        List<Guid> sameDay = [.. (await _afterSales.WithdrawalDeclarationDaysAsync(itemIds, ct)).Where(w => w.DeclarationDate == request.DeliveryDate).Select(w => w.WithdrawalId)];
        if (sameDay.Count > 0 && request.IsDeliveredBeforeDeclaration is null)
        {
            return new OrderCorrectionResult(TransitionRefusal.InvalidInput, [], null) { AsksDeliveryOrder = true };
        }

        if (sameDay.Count > 0)
        {
            await _afterSales.SetDeliveryOrderAsync(sameDay, request.IsDeliveredBeforeDeclaration!.Value, ct); // non-null: the check above refuses a same-day correction without the answer
        }

        // The order-level date: every delivered physical item takes it (02 3.20.11); the windows run from it because they are read from it.
        foreach (OrderItem item in delivered)
        {
            item.DeliveryDate = request.DeliveryDate;
        }

        OrderTransitions.RaiseVersionOnce(_db, order);
        await _trail.WriteAsync(
            new AuditEntry(
                AuditOperation.CorrectDeliveryDate,
                nameof(Order),
                order.Id,
                new AuditDetails(order.OrderNumber, changes: [new AuditValueChange("DeliveryDate", DateText(oldDate), DateText(request.DeliveryDate))])),
            ct);
        return new OrderCorrectionResult(null, [], null);
    }

    public async Task<OrderCorrectionResult> CorrectReturnArrivalDateAsync(CorrectReturnArrivalDateRequest request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (!IsAdmin(request.Admin))
        {
            return Refused(TransitionRefusal.ActorNotAllowed);
        }

        if (request.ArrivalDate > TurkeyTime.DateOf(_time.GetUtcNow()))
        {
            return Refused(TransitionRefusal.InvalidInput);
        }

        (Order? order, TransitionRefusal? locked) = await LockAsync(request.OrderId, request.ExpectedVersion, ct);
        if (locked is { } lockRefusal)
        {
            return Refused(lockRefusal);
        }

        OrderItem? item = await _db.OrderItems.SingleOrDefaultAsync(i => i.Id == request.OrderItemId && i.OrderId == order!.Id, ct); // non-null: LockAsync returns the order when it returns no refusal
        if (item is null)
        {
            return Refused(TransitionRefusal.InvalidInput);
        }

        OperationVerdict verdict = await _operations.CheckAsync(order!.Id, OrderOperation.CorrectDeliveryOrArrivalDate, null, null, ct); // non-null: LockAsync returns the order when it returns no refusal
        if (verdict.Decision != OperationDecision.Allowed)
        {
            return Refused(TransitionRefusal.ConditionNotMet);
        }

        (ItemRecordRefusal? refusal, DateOnly? oldDate) = await _afterSales.CorrectArrivalDateAsync(OrderTransitions.Subject(order, item), request.ArrivalRecordId, request.ArrivalDate, ct);
        if (refusal is not null || oldDate is not { } old)
        {
            return Refused(TransitionRefusal.InvalidInput);
        }

        if (old == request.ArrivalDate)
        {
            return Refused(TransitionRefusal.InvalidInput);
        }

        OrderTransitions.RaiseVersionOnce(_db, order);
        await _trail.WriteAsync(
            new AuditEntry(
                AuditOperation.CorrectReturnArrivalDate,
                nameof(Order),
                order.Id,
                new AuditDetails(
                    order.OrderNumber,
                    changes: [new AuditValueChange("ReturnArrivalDate", DateText(old), DateText(request.ArrivalDate))],
                    values: [new AuditValue("ItemId", item.Id.ToString("D"))])),
            ct);
        return new OrderCorrectionResult(null, [], null);
    }

    public async Task<SaveInternalNoteResult> SaveInternalNoteAsync(SaveInternalNoteRequest request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (!IsAdmin(request.Admin))
        {
            return SaveInternalNoteResult.OperationNotAvailable;
        }

        // The shared row lock is the first statement (09 10.9 items 1-2): it raises no version, waits for a disposal that locked first and sees the emptied
        // order; another note write does not wait for it. No order row is tracked or written here - the note is outside the order's version (06 4.20).
        string? contactEmail = await _locks.LockSharedAsync(request.OrderId, ct);
        if (string.IsNullOrEmpty(contactEmail))
        {
            return SaveInternalNoteResult.OperationNotAvailable;
        }

        // 1.11.27 on the locked order: an order whose personal data is disposed takes no note (K-1095).
        OperationVerdict verdict = await _operations.CheckAsync(request.OrderId, OrderOperation.WriteInternalNote, null, null, ct);
        if (verdict.Decision != OperationDecision.Allowed)
        {
            return SaveInternalNoteResult.OperationNotAvailable;
        }

        // The stored text and the shown text are in one form (LF line breaks), so the database's condition compares them as ShownValue would.
        string text = ShownValue.NormalizeText(request.Text).Trim();
        string shown = ShownValue.NormalizeText(request.OriginalText);
        if (text == shown)
        {
            // Nothing to write; the note must still be the shown text, or the panel learns that it changed.
            string? current = await CurrentNoteAsync(request.OrderId, ct);
            return ShownValue.Matches(shown, current) ? SaveInternalNoteResult.Unchanged : SaveInternalNoteResult.DisplayedValueConflict(current);
        }

        if (text.Length == 0)
        {
            // An emptied note is deleted, only while it is still the shown text (K-1051; 09 10.9 item 3.3).
            if (await SaveInternalNote.DeleteShownAsync(_db, request.OrderId, shown, ct) == 0)
            {
                return SaveInternalNoteResult.DisplayedValueConflict(await CurrentNoteAsync(request.OrderId, ct));
            }
        }
        else if (await SaveInternalNote.UpdateShownAsync(_db, request.OrderId, shown, text, request.Admin.AdminId, request.Admin.AdminName.Trim(), ct) == 0)
        {
            // No row with the shown text: a note shown empty is inserted while there is still none (a concurrent first write breaks the unique index and the
            // caller runs the write again - then the note exists and this is the conflict); any other case is the conflict (09 10.9 item 3.2).
            string? current = await CurrentNoteAsync(request.OrderId, ct);
            if (shown.Length != 0 || current is not null)
            {
                return SaveInternalNoteResult.DisplayedValueConflict(current);
            }

            _db.InternalNotes.Add(InternalNote.Create(request.OrderId, text, request.Admin.AdminId, request.Admin.AdminName.Trim(), _time.GetUtcNow()));
        }

        // The writing and the change go to the trail; the text never does (02 10.4.7; 09 9.2 item 4). A refusal writes no trail row.
        string orderNumber = await _db.Orders.AsNoTracking().Where(o => o.Id == request.OrderId).Select(o => o.OrderNumber).SingleAsync(ct);
        await _trail.WriteAsync(new AuditEntry(AuditOperation.SaveInternalNote, nameof(Order), request.OrderId, new AuditDetails(orderNumber)), ct);
        return SaveInternalNoteResult.Saved;
    }

    /// <summary>The note's text as it is now, or null when the order has none.</summary>
    private Task<string?> CurrentNoteAsync(Guid orderId, CancellationToken ct)
        => _db.InternalNotes.AsNoTracking().Where(n => n.OrderId == orderId).Select(n => (string?)n.Text).SingleOrDefaultAsync(ct);

    private static string DateText(DateOnly date) => date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

    private static OrderCorrectionResult DateRefused(DeliveryDateObstacle obstacle) => new(TransitionRefusal.InvalidInput, [], null) { DateObstacle = obstacle };
}
