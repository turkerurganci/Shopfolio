using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Modules.AfterSales.Contracts;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Notifications.Contracts;
using Shopfolio.Modules.Ordering.Contracts;

namespace Shopfolio.Modules.Ordering;

/// <summary>
/// The shipment axis of the transition unit (T316; 03 1.2, 1.6.1; 02 5.4; 05 4.1): the operations whose condition is written here - the three
/// cancellations S3, S4 and S9, the reasonless S9 closing, S5, S6, S7 and S8. Each one is the unit's core (<see cref="ApplyChosenAsync"/>: lock, version,
/// white list, actor, reason, pair rule, then its own condition) followed, in the same transaction, by what the transition owns beyond the status: the
/// carrier data, the delivery marks, the unpaid cancellation's freed reservation (03 1.4.4.3; 06 4.14 path 3; T324), the customer notice's dispatch row
/// (B-5, B-6, B-7). S1 has no operation of its own: it is written with Ö1 by the payment axis's approval (T318; 03 1.4.2.1). S2, S10 and S11 have no
/// operation here: they are the closure rule's (<see cref="EvaluateClosureAsync"/>), written at the end of the operation that completes it. The stock, the
/// coupon and the refund a cancellation of a paid order frees are the item record tasks' side effects (<see cref="OrderCounters.ReturnAsync"/>), written
/// after this core (09 9.1 item 6).
/// </summary>
internal sealed partial class OrderTransitions
{
    public async Task<OrderTransitionResult> CancelOrderAsync(CancelOrderRequest request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (!TryCancellationActor(request, out TransitionActor? actor, out TransitionReasons? reasons, out TransitionRefusal? refusal))
        {
            return Refused(refusal!.Value);
        }

        bool unpaid = false;
        TransitionOutcome outcome = await ApplyChosenAsync(
            request.OrderId,
            actor!,
            reasons,
            request.ExpectedVersion,
            order =>
            {
                // S3 with Ö2 is chosen only for a placed order whose payment is pending: the unpaid order of both rules (03 1.4.4.3; 06 4.14 path 3).
                ChosenTransitions chosen = ChooseCancellation(order);
                unpaid = chosen.Codes.Contains(TransitionCode.O2);
                return chosen;
            },
            (order, token) => CancellationConditionAsync(order, request, token),
            ct);
        if (outcome.IsRejected)
        {
            return Refused(outcome);
        }

        // The unpaid cancellation (S3 + Ö2) frees what its confirmation reserved in the same transaction (03 1.4.4.3; 06 4.14 path 3); a paid order's rows
        // were deleted at Ö1 and its freed units are its item records' (1.4.4.4).
        if (unpaid)
        {
            await ReleaseReservationAsync(request.OrderId, ct);
        }
        else if (request.Actor == ActorKind.Admin && request.Item is null)
        {
            // The firm's whole-order cancellation of a paid order cancels every open item's open units: the records, linked to this transition, and the counters (D-305; T368).
            await CancelOpenItemsByFirmAsync(request.OrderId, outcome.TransitionRecordIds[0], request, ct);
        }

        // B-7 goes with the cancellation, once, in the customer's, the administrator's or the system's wording (08 2.2.6); an item cancellation names the item.
        await QueueNoticeAsync(
            request.OrderId,
            new CancelledNotice(request.OrderId, request.Actor, request.FirmReason, request.ReasonNote, request.SystemReason, request.Item),
            ct);
        return Applied(outcome, []);
    }

    public async Task<OrderTransitionResult> CloseReturnedOrderAsync(AdminOrderRequest request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (!TryAdminActor(request.Admin, out TransitionActor? actor))
        {
            return Refused(TransitionRefusal.ActorNotAllowed);
        }

        // The closure's reason is the closing itself; the firm chooses none, and B-7 does not go (03 1.2 S9).
        TransitionOutcome outcome = await ApplyChosenAsync(
            request.OrderId,
            actor!,
            TransitionReasons.OfKind(TransitionReason.Closure),
            request.ExpectedVersion,
            _ => new ChosenTransitions([TransitionCode.S9]),
            async (order, token) => await ReturnedOrderHasNoOpenAndNoDeliveredItemAsync(order, token),
            ct);
        return outcome.IsRejected ? Refused(outcome) : Applied(outcome, []);
    }

    public Task<OrderTransitionResult> ShipOrderAsync(ShipOrderRequest request, CancellationToken ct) => SendAsync(request, TransitionCode.S5, ct);

    public Task<OrderTransitionResult> ResendOrderAsync(ShipOrderRequest request, CancellationToken ct) => SendAsync(request, TransitionCode.S8, ct);

    public async Task<OrderTransitionResult> MarkDeliveredAsync(MarkDeliveredRequest request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (!TryAdminActor(request.Admin, out TransitionActor? actor))
        {
            return Refused(TransitionRefusal.ActorNotAllowed);
        }

        DateTimeOffset now = _time.GetUtcNow();
        DateOnly today = TurkeyTime.DateOf(now);
        if (DeliveryMarkRules.CheckDeliveryDate(request.DeliveryDate, today, null) is { } future)
        {
            return DateRefused(future);
        }

        List<OrderItem> marked = [];
        DeliveryDateObstacle? dateObstacle = null;
        TransitionOutcome outcome = await ApplyChosenAsync(
            request.OrderId,
            actor!,
            null,
            request.ExpectedVersion,
            _ => new ChosenTransitions([TransitionCode.S6]),
            async (order, token) =>
            {
                // The delivery date is not before the day of the shipment - the moment of the last S5 or S8 record no correction undid (OrderPeriods.Shipments; 06 4.17: read, not stored).
                DateTimeOffset? shippedAt = await OrderPeriods.Shipments(_db)
                    .Where(t => t.OrderId == order.Id)
                    .MaxAsync(t => (DateTimeOffset?)t.CreatedAt, token);
                DateOnly? shippingDay = shippedAt is { } at ? TurkeyTime.DateOf(at) : null;
                dateObstacle = DeliveryMarkRules.CheckDeliveryDate(request.DeliveryDate, today, shippingDay);
                if (dateObstacle is not null)
                {
                    return false;
                }

                // The mark is the open physical items' (02 5.8): a closed item - cancelled, withdrawn, terminated - is not delivered by it.
                marked.AddRange(await OpenItemsAsync(order, physicalOnly: true, token));
                return marked.Count > 0;
            },
            ct);
        if (outcome.IsRejected)
        {
            return dateObstacle is { } obstacle ? DateRefused(obstacle) : Refused(outcome);
        }

        foreach (OrderItem item in marked)
        {
            item.DeliveryMarkedAt = now;
            item.DeliveryDate = request.DeliveryDate;
        }

        return Applied(outcome, [.. marked.Select(i => i.Id)]);
    }

    public async Task<OrderTransitionResult> MarkDeliveryFailedAsync(AdminOrderRequest request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (!TryAdminActor(request.Admin, out TransitionActor? actor))
        {
            return Refused(TransitionRefusal.ActorNotAllowed);
        }

        // The core reads the closure rule at the end: a returned shipment whose physical items are all closed and whose rest is delivered ends in S11 (03 1.2 S11).
        TransitionOutcome outcome = await ApplyChosenAsync(
            request.OrderId,
            actor!,
            null,
            request.ExpectedVersion,
            _ => new ChosenTransitions([TransitionCode.S7]),
            null,
            ct);
        if (outcome.IsRejected)
        {
            return Refused(outcome);
        }

        await QueueNoticeAsync(request.OrderId, new DeliveryFailedNotice(request.OrderId), ct);
        return Applied(outcome, []);
    }

    private async Task<OrderTransitionResult> SendAsync(ShipOrderRequest request, TransitionCode code, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (!TryAdminActor(request.Admin, out TransitionActor? actor))
        {
            return Refused(TransitionRefusal.ActorNotAllowed);
        }

        // S5 and S8 are irreversible (03 1.6.1.2; K-677): the confirmation is part of the request, and the carrier data is whole before the lock is taken.
        if (!request.Confirmed)
        {
            return Refused(TransitionRefusal.ConfirmationRequired);
        }

        if (request.Shipment is null || !TryNormalizeShipment(request.Shipment, out ShipmentInfo? shipment))
        {
            return Refused(TransitionRefusal.InvalidInput);
        }

        List<OrderItem> sent = [];
        TransitionOutcome outcome = await ApplyChosenAsync(
            request.OrderId,
            actor!,
            null,
            request.ExpectedVersion,
            _ => new ChosenTransitions([code]),
            async (order, token) =>
            {
                // Only the open physical items go (02 5.4 S5, S8): none cancelled, removed, terminated or withdrawn; the order carries at least one.
                sent.AddRange(await OpenItemsAsync(order, physicalOnly: true, token));
                return sent.Count > 0;
            },
            ct);
        if (outcome.IsRejected)
        {
            return Refused(outcome);
        }

        Order order = await _db.Orders.SingleAsync(o => o.Id == request.OrderId, ct);
        order.RecordShipment(shipment!.Carrier, shipment.OtherCarrierName, shipment.TrackingNumber, shipment.IsOwnVehicleDelivery);
        await QueueNoticeAsync(
            request.OrderId,
            new ShippedNotice(request.OrderId, shipment.Carrier, shipment.OtherCarrierName, shipment.TrackingNumber, shipment.IsOwnVehicleDelivery),
            ct);
        return Applied(outcome, [.. sent.Select(i => i.Id)]);
    }

    /// <summary>S3 from Placed (with Ö2 on an unpaid order), S4 from Preparing, S9 from DeliveryFailed; any other state names S3, which the white list refuses.</summary>
    private static ChosenTransitions ChooseCancellation(Order order)
    {
        return order.OrderStatus switch
        {
            OrderStatus.Placed when order.PaymentStatus == PaymentStatus.Pending => new ChosenTransitions([TransitionCode.S3, TransitionCode.O2]),
            OrderStatus.Preparing => new ChosenTransitions([TransitionCode.S4]),
            OrderStatus.DeliveryFailed => new ChosenTransitions([TransitionCode.S9]),
            _ => new ChosenTransitions([TransitionCode.S3]),
        };
    }

    /// <summary>The cancellation's own condition (03 1.2 S3, S4, S9), read against the locked order after the white list and the actor.</summary>
    private async Task<bool> CancellationConditionAsync(Order order, CancelOrderRequest request, CancellationToken ct)
    {
        bool unpaid = order.PaymentStatus == PaymentStatus.Pending;
        if (request.Actor == ActorKind.System)
        {
            // The system cancels only an unpaid order: the payment window ran out, the card failed, or a new order of the same cart was confirmed (S3).
            return unpaid && order.OrderStatus == OrderStatus.Placed;
        }

        if (request.Item is not null)
        {
            // The last open item's cancellation: only on a paid order (an unpaid one is cancelled as a whole), on a returned shipment only the firm's (S9 is the
            // firm's cancellation or closing; T368 - its item cancellation of the last open units is the firm's S9 with its reason), and only when the item
            // record the caller wrote left no open item.
            bool returnedAllowed = order.OrderStatus != OrderStatus.DeliveryFailed || request.Actor == ActorKind.Admin;
            return !unpaid && returnedAllowed && (await OpenItemsAsync(order, physicalOnly: false, ct)).Count == 0;
        }

        // A whole-order cancellation never takes an item marked delivered (a delivered digital item, a completed service): such an order does not end in
        // Cancelled (02 5.4 Cancelled, 7.2.2, 7.2.5; 03 1.2).
        if (await _db.OrderItems.AnyAsync(i => i.OrderId == order.Id && i.DeliveryMarkedAt != null, ct))
        {
            return false;
        }

        if (request.Actor == ActorKind.Buyer)
        {
            // The customer cancels an unpaid order as a whole; a paid order's cancellation is its last open item's (the branch above).
            return unpaid;
        }

        // The firm's S9 cancels what is still open; with no open item the transition is a closure and the firm chooses no reason (03 1.2 S9).
        return order.OrderStatus != OrderStatus.DeliveryFailed || (await OpenItemsAsync(order, physicalOnly: false, ct)).Count > 0;
    }

    private async Task<bool> ReturnedOrderHasNoOpenAndNoDeliveredItemAsync(Order order, CancellationToken ct)
    {
        List<OrderItem> items = await _db.OrderItems.Where(i => i.OrderId == order.Id).ToListAsync(ct);
        return items.Count > 0
            && items.All(i => i.DeliveryMarkedAt is null)
            && (await OpenItemsAsync(order, physicalOnly: false, ct)).Count == 0;
    }

    /// <summary>
    /// The order's items with units open to an operation (02 5.8, 7.1.6; 06 2.7.2): the quantity minus what the item records - the caller's unsaved ones included - closed and
    /// minus the units rescinded on a defect claim (K-1077). An item whose units are all closed or rescinded is not open: S5/S8 do not send it, the delivery mark does not take it,
    /// and an order on a returned shipment left with only such items has no open item - the firm's S9 is the closure, not a cancellation with a reason (D-905; 02 7.1.6).
    /// </summary>
    private async Task<List<OrderItem>> OpenItemsAsync(Order order, bool physicalOnly, CancellationToken ct)
    {
        List<OrderItem> items = await _db.OrderItems
            .Where(i => i.OrderId == order.Id && (!physicalOnly || i.ProductType == ProductType.Physical))
            .ToListAsync(ct);
        if (items.Count == 0)
        {
            return [];
        }

        IReadOnlyDictionary<Guid, int> unavailable = await _afterSales.UnavailableQuantitiesAsync([.. items.Select(i => i.Id)], ct);
        return [.. items.Where(i => i.Quantity - unavailable.GetValueOrDefault(i.Id) > 0)];
    }

    /// <summary>The actor and the reasons of a cancellation request (03 1.2; 06 4.19), checked before anything is read: the customer's has neither, the firm's both, the system's the kind.</summary>
    private static bool TryCancellationActor(
        CancelOrderRequest request,
        out TransitionActor? actor,
        out TransitionReasons? reasons,
        out TransitionRefusal? refusal)
    {
        actor = null;
        reasons = null;
        refusal = null;
        switch (request.Actor)
        {
            case ActorKind.Buyer:
                if (request.Admin is not null || request.FirmReason is not null || request.ReasonNote is not null || request.SystemReason is not null || request.OwnerConfirmed)
                {
                    refusal = request.Admin is not null ? TransitionRefusal.ActorNotAllowed : TransitionRefusal.ReasonNotAllowed;
                    return false;
                }

                actor = TransitionActor.Buyer;
                return true;

            case ActorKind.Admin:
                if (!TryAdminActor(request.Admin, out actor) || request.SystemReason is not null)
                {
                    refusal = TransitionRefusal.ActorNotAllowed;
                    return false;
                }

                if (!request.Confirmed)
                {
                    refusal = TransitionRefusal.ConfirmationRequired;
                    return false;
                }

                // The firm's reason is from the closed list; "Other" carries the free note, no other reason does (06 4.19).
                if (request.ExpectedVersion is null
                    || request.FirmReason is not { } firmReason
                    || !FirmReasonRules.IsComplete(firmReason, request.ReasonNote, request.OwnerConfirmed))
                {
                    refusal = TransitionRefusal.InvalidInput;
                    return false;
                }

                reasons = TransitionReasons.OfFirm(firmReason, firmReason == CancellationReason.Other ? request.ReasonNote!.Trim() : null);
                return true;

            case ActorKind.System:
                if (request.Admin is not null || request.FirmReason is not null || request.ReasonNote is not null || request.Item is not null || request.OwnerConfirmed)
                {
                    refusal = TransitionRefusal.ActorNotAllowed;
                    return false;
                }

                // The kind of automatic cancellation; the closure is not one - it makes no notice and is written by the closure rule (08 2.2.6 B-7).
                if (request.SystemReason is not { } kind || kind == TransitionReason.Closure)
                {
                    refusal = TransitionRefusal.ReasonNotAllowed;
                    return false;
                }

                actor = TransitionActor.System;
                reasons = TransitionReasons.OfKind(kind);
                return true;

            default:
                refusal = TransitionRefusal.ActorNotAllowed;
                return false;
        }
    }

    private static bool TryAdminActor(TransitionAdmin? admin, out TransitionActor? actor)
    {
        actor = null;
        if (admin is null || admin.AdminId == Guid.Empty || string.IsNullOrWhiteSpace(admin.AdminName))
        {
            return false;
        }

        actor = TransitionActor.Admin(admin.AdminId, admin.AdminName);
        return true;
    }

    /// <summary>
    /// The carrier data of 02 5.4 S5 / 02 6.7.4: the own-vehicle declaration alone, or exactly one of a listed company's code and an unlisted company's name,
    /// with the tracking number. A listed code is one of the catalog's active companies (08 2.4); the values are trimmed, and the trimmed tracking number and
    /// company name are within <see cref="ShipmentLimits"/> (09 3.5; K-1586).
    /// </summary>
    internal static bool TryNormalizeShipment(ShipmentInfo info, out ShipmentInfo? normalized)
    {
        normalized = null;
        string? carrier = Trimmed(info.Carrier);
        string? other = Trimmed(info.OtherCarrierName);
        string? tracking = Trimmed(info.TrackingNumber);
        if (info.IsOwnVehicleDelivery)
        {
            if (carrier is not null || other is not null || tracking is not null)
            {
                return false;
            }

            normalized = new ShipmentInfo(null, null, null, true);
            return true;
        }

        if ((carrier is null) == (other is null) || tracking is null)
        {
            return false;
        }

        // The caps of 09 3.5 (K-1586) on the trimmed values.
        if (tracking.Length > ShipmentLimits.TrackingNumberMaxLength || other?.Length > ShipmentLimits.OtherCarrierNameMaxLength)
        {
            return false;
        }

        if (carrier is not null && !CarrierCatalog.Default.Active.Any(c => c.Code == carrier))
        {
            return false;
        }

        normalized = new ShipmentInfo(carrier, other, tracking, false);
        return true;
    }

    private static string? Trimmed(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    /// <summary>Queues the customer notice's dispatch row to the order's contact e-mail, in the operation's transaction; a disposed order (no e-mail) gets none.</summary>
    private async Task QueueNoticeAsync(Guid orderId, CustomerNotice notice, CancellationToken ct)
    {
        string? email = await _db.Orders.AsNoTracking().Where(o => o.Id == orderId).Select(o => o.ContactEmail).SingleAsync(ct);
        if (!string.IsNullOrWhiteSpace(email))
        {
            await _notifications.QueueCustomerNoticeAsync(notice, email, ct);
        }
    }

    private static OrderTransitionResult Refused(TransitionRefusal refusal) => new(refusal, [], null, []);

    /// <summary>The refusal of the delivery date: the wrong value, with the reason the panel tells beside the field (02 3.20.11).</summary>
    private static OrderTransitionResult DateRefused(DeliveryDateObstacle obstacle) =>
        new(TransitionRefusal.InvalidInput, [], null, []) { DateObstacle = obstacle };

    private static OrderTransitionResult Refused(TransitionOutcome outcome)
    {
        TransitionRefusal refusal = outcome.Rejection switch
        {
            TransitionRejection.OrderNotFound => TransitionRefusal.OrderNotFound,
            TransitionRejection.NotInTheTable => TransitionRefusal.NotInTheTable,
            TransitionRejection.ActorNotAllowed => TransitionRefusal.ActorNotAllowed,
            TransitionRejection.ReasonNotAllowed => TransitionRefusal.ReasonNotAllowed,
            TransitionRejection.VersionConflict => TransitionRefusal.VersionConflict,
            TransitionRejection.ForbiddenStatePair => TransitionRefusal.ForbiddenStatePair,
            TransitionRejection.ConfirmationRequired => TransitionRefusal.ConfirmationRequired,
            TransitionRejection.InvalidInput => TransitionRefusal.InvalidInput,
            TransitionRejection.CorrectionRequiresPanel => TransitionRefusal.ActorNotAllowed,
            TransitionRejection.CorrectionReasonRequired => TransitionRefusal.InvalidInput,
            TransitionRejection.CorrectionNotPossible => TransitionRefusal.CorrectionNotPossible,
            _ => TransitionRefusal.ConditionNotMet,
        };
        return Refused(refusal);
    }

    private static OrderTransitionResult Applied(TransitionOutcome outcome, IReadOnlyList<Guid> itemIds)
    {
        List<Guid> ids = [.. outcome.TransitionRecordIds];
        if (outcome.Closure.TransitionRecordId is { } closureId)
        {
            ids.Add(closureId);
        }

        return new OrderTransitionResult(null, ids, outcome.Closure.Code, itemIds);
    }
}
