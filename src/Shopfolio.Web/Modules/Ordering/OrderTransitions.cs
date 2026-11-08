using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.DataModel;
using Shopfolio.Modules.AfterSales.Contracts;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Notifications.Contracts;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Modules.Payment.Contracts;

namespace Shopfolio.Modules.Ordering;

/// <summary>
/// The transition unit (09 9.1; 05 4.1, MK-35): the one class that writes the status fields of the order. Every operation of it works in the same
/// order (05 4.1.2; 09 9.1 item 4): the order's row lock, the white list (<see cref="TransitionTable"/>), the operation's own condition against
/// the order as it is now, the status field, the appended <see cref="TransitionRecord"/>, then the closure rule (05 4.1.3). The side effects of a
/// transition - the stock and coupon return, the trace row, the jobs - are calls the operation's own method writes after this core, in the same
/// transaction (09 9.1 item 6); the unit knows no outside service and holds no <c>HttpClient</c>. A refusal is a returned value and writes nothing:
/// the whole check is done on the order's values before the first write, so a refused call leaves the tracked order as it was.
/// The core is called with the transaction the entry point opened (<c>ITransactionRunner</c>, 09 9.1 item 3); the unit opens none.
/// </summary>
internal sealed partial class OrderTransitions : IOrderTransitions
{
    private static readonly TransitionCode[] ReasonCodes = [TransitionCode.S3, TransitionCode.S4, TransitionCode.S9, TransitionCode.O2];
    private static readonly TransitionCode[] ClosureCodes = [TransitionCode.S3, TransitionCode.S4, TransitionCode.S9];

    private readonly AppDbContext _db;
    private readonly OrderLocks _locks;
    private readonly IAfterSalesModule _afterSales;
    private readonly IPaymentModule _payment;
    private readonly INotificationsModule _notifications;
    private readonly ICatalogModule _catalog;
    private readonly TimeProvider _time;

    public OrderTransitions(
        AppDbContext db,
        OrderLocks locks,
        IAfterSalesModule afterSales,
        IPaymentModule payment,
        INotificationsModule notifications,
        ICatalogModule catalog,
        TimeProvider time)
    {
        _db = db;
        _locks = locks;
        _afterSales = afterSales;
        _payment = payment;
        _notifications = notifications;
        _catalog = catalog;
        _time = time;
    }

    /// <summary>
    /// Applies the white-list transitions of the request. <paramref name="condition"/> is the operation's own condition (gecikme feshi, an open item,
    /// the service window; 05 4.1.2): it is read after the table, against the locked order, and returns false to refuse.
    /// </summary>
    public Task<TransitionOutcome> ApplyAsync(
        TransitionRequest request,
        Func<Order, CancellationToken, Task<bool>>? condition,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Codes);
        if (request.Codes.Count == 0)
        {
            throw new ArgumentException("A request names at least one transition.", nameof(request));
        }

        return ApplyChosenAsync(request.OrderId, request.Actor, request.Reasons, request.ExpectedVersion, _ => new ChosenTransitions(request.Codes), condition, ct);
    }

    /// <summary>
    /// The core of <see cref="ApplyAsync"/> for an operation whose transition depends on the state the order is in once it is locked (the cancellation is
    /// S3 from Placed, S4 from Preparing, S9 from DeliveryFailed): the lock is taken, the order loaded and the screen's version compared first, then
    /// <paramref name="choose"/> names the codes against the order as it is now. A choice that carries a refusal ends the call before anything is written.
    /// </summary>
    internal async Task<TransitionOutcome> ApplyChosenAsync(
        Guid orderId,
        TransitionActor actor,
        TransitionReasons? requestedReasons,
        int? expectedVersion,
        Func<Order, ChosenTransitions> choose,
        Func<Order, CancellationToken, Task<bool>>? condition,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(actor);
        ArgumentNullException.ThrowIfNull(choose);

        // 1. The lock; the order is loaded after it (an order loaded before the lock may be stale).
        await _locks.LockAsync([orderId], ct);
        Order? order = await _db.Orders.SingleOrDefaultAsync(o => o.Id == orderId, ct);
        if (order is null)
        {
            return TransitionOutcome.Rejected(TransitionRejection.OrderNotFound);
        }

        if (expectedVersion is { } shown && shown != order.Version)
        {
            return TransitionOutcome.Rejected(TransitionRejection.VersionConflict);
        }

        ChosenTransitions chosen = choose(order);
        if (chosen.Refusal is { } chosenRefusal)
        {
            return TransitionOutcome.Rejected(chosenRefusal);
        }

        TransitionRequest request = new(orderId, chosen.Codes, actor, requestedReasons, expectedVersion);

        // 2. The white list, the actor and the reasons - all on the order's values; nothing is written yet.
        if (!TryPlan(order, request, out List<(ShipmentTransition? Shipment, PaymentTransition? Payment)> steps, out OrderStatus orderStatus, out PaymentStatus paymentStatus, out TransitionRejection? refusal))
        {
            return TransitionOutcome.Rejected(refusal!.Value);
        }

        if (!TransitionTable.IsPairAllowed(orderStatus, paymentStatus))
        {
            return TransitionOutcome.Rejected(TransitionRejection.ForbiddenStatePair);
        }

        // 3. The operation's own condition, against the order as it is now.
        if (condition is not null && !await condition(order, ct))
        {
            return TransitionOutcome.Rejected(TransitionRejection.ConditionNotMet);
        }

        // 4. The status fields; 5. the records.
        DateTimeOffset now = _time.GetUtcNow();
        List<Guid> recordIds = [];
        foreach ((ShipmentTransition? shipment, PaymentTransition? payment) in steps)
        {
            TransitionCode code;
            if (shipment is not null)
            {
                order.ApplyTransition(shipment);
                code = shipment.Code;
            }
            else
            {
                order.ApplyTransition(payment!);
                code = payment!.Code;
            }

            TransitionReasons? reasons = request.Reasons is { IsEmpty: false } ? request.Reasons : null;
            TransitionRecord record = TransitionRecord.Create(
                order.Id, code, request.Actor.Kind, request.Actor.AdminId, request.Actor.AdminName,
                reasons?.Automatic, reasons?.Firm, null, reasons?.Note, null, now);
            _db.TransitionRecords.Add(record);
            recordIds.Add(record.Id);
        }

        RaiseVersionOnce(order);

        // The closure rule is read at the end of the operation (05 4.1.3).
        ClosureResult closure = await EvaluateClosureAsync(order, request.Actor, ct);
        return TransitionOutcome.Applied(recordIds, closure);
    }

    /// <summary>
    /// The administrator's correction (05 4.1.1; 02 5.4): outside the white list, a new transition that returns the axis the corrected transition moved
    /// to where it was; the corrected row stays and the correction is appended beside it. Asked only from the panel: the actor is an administrator
    /// with the screen's version. What makes a particular correction allowed (the axes' tie, the payment and the stock the cancellation freed) is
    /// <paramref name="condition"/>, the correction operation's own (T319).
    /// </summary>
    public async Task<TransitionOutcome> CorrectAsync(
        CorrectionRequest request,
        Func<Order, TransitionRecord, CancellationToken, Task<bool>>? condition,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (request.Actor.Kind != ActorKind.Admin || !request.Actor.IsConsistent)
        {
            return TransitionOutcome.Rejected(TransitionRejection.CorrectionRequiresPanel);
        }

        if (request.Reason is null)
        {
            return TransitionOutcome.Rejected(TransitionRejection.CorrectionReasonRequired);
        }

        await _locks.LockAsync([request.OrderId], ct);
        Order? order = await _db.Orders.SingleOrDefaultAsync(o => o.Id == request.OrderId, ct);
        if (order is null)
        {
            return TransitionOutcome.Rejected(TransitionRejection.OrderNotFound);
        }

        if (request.ExpectedVersion != order.Version)
        {
            return TransitionOutcome.Rejected(TransitionRejection.VersionConflict);
        }

        TransitionRecord? corrected = await _db.TransitionRecords
            .SingleOrDefaultAsync(t => t.Id == request.CorrectedTransitionRecordId && t.OrderId == order.Id, ct);
        if (corrected is null || await _db.TransitionRecords.AnyAsync(t => t.CorrectedTransitionRecordId == corrected.Id, ct))
        {
            return TransitionOutcome.Rejected(TransitionRejection.CorrectionNotPossible);
        }

        // The correction returns the axis to the corrected row's source - only while the order is still where that row left it.
        OrderStatus orderStatus = order.OrderStatus;
        PaymentStatus paymentStatus = order.PaymentStatus;
        if (TransitionTable.FindShipment(corrected.TransitionCode) is { } shipment && order.OrderStatus == shipment.To)
        {
            orderStatus = shipment.From;
        }
        else if (TransitionTable.FindPayment(corrected.TransitionCode) is { } payment && order.PaymentStatus == payment.To)
        {
            paymentStatus = payment.From;
        }
        else
        {
            return TransitionOutcome.Rejected(TransitionRejection.CorrectionNotPossible);
        }

        if (!TransitionTable.IsPairAllowed(orderStatus, paymentStatus))
        {
            return TransitionOutcome.Rejected(TransitionRejection.ForbiddenStatePair);
        }

        if (condition is not null && !await condition(order, corrected, ct))
        {
            return TransitionOutcome.Rejected(TransitionRejection.ConditionNotMet);
        }

        order.ApplyCorrection(orderStatus, paymentStatus);
        TransitionRecord record = TransitionRecord.Create(
            order.Id, TransitionCode.Correction, ActorKind.Admin, request.Actor.AdminId, request.Actor.AdminName,
            null, null, request.Reason, request.Note, corrected.Id, _time.GetUtcNow());
        _db.TransitionRecords.Add(record);
        RaiseVersionOnce(order);
        return TransitionOutcome.Applied([record.Id], ClosureResult.None);
    }

    /// <summary>
    /// The closure rule (05 4.1.3; 03 1.4.3; 02 5.8): an order with no open item left closes at the end of the same operation. An item is open while
    /// its open quantity - its own minus what its cancellation, removal, termination and withdrawal records closed - is above zero. A physical open
    /// item keeps the order on the physical line (S5, S6); with none, the order ends its line when the last open item without a delivery mark is
    /// closed or marked: it becomes Delivered (S2, S10 or S11) when any item carries the mark and the payment is confirmed, or Cancelled by closure
    /// (S3 from Placed, S4 from Preparing; the reason is <see cref="TransitionReason.Closure"/>, the actor the system) when none was delivered.
    /// A shipped order is never closed here (the firm's S6, S7); a returned one is closed to Cancelled by the firm's reasonless S9, never by the system.
    /// The caller holds the order's lock and has written its item record into the context; the closure's own version raise is skipped when the
    /// operation raised it already. The closing item record links the returned record id (06 4.22).
    /// </summary>
    public async Task<ClosureResult> EvaluateClosureAsync(Order order, TransitionActor eventActor, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(order);
        ArgumentNullException.ThrowIfNull(eventActor);
        if (!LockSequence.LockedOrders(_db).Contains(order.Id))
        {
            throw new InvalidOperationException("The closure rule is read under the order's row lock (05 4.1.2).");
        }

        if (order.OrderStatus is OrderStatus.Delivered or OrderStatus.Cancelled or OrderStatus.Shipped)
        {
            return ClosureResult.None;
        }

        List<OrderItem> items = await _db.OrderItems.Where(i => i.OrderId == order.Id).ToListAsync(ct);
        if (items.Count == 0)
        {
            return ClosureResult.None;
        }

        IReadOnlyDictionary<Guid, int> closed = await _afterSales.ClosedQuantitiesAsync([.. items.Select(i => i.Id)], ct);
        List<OrderItem> open = [.. items.Where(i => i.Quantity - closed.GetValueOrDefault(i.Id) > 0)];
        if (open.Any(i => i.ProductType == ProductType.Physical) || open.Any(i => i.DeliveryMarkedAt is null))
        {
            return ClosureResult.None;
        }

        ShipmentTransition? row;
        TransitionActor actor;
        TransitionReasons? reasons = null;
        if (items.Any(i => i.DeliveryMarkedAt is not null))
        {
            // The delivered end needs a confirmed payment: Delivered with Pending or Failed is a forbidden pair (02 5.6.1).
            if (order.PaymentStatus is PaymentStatus.Pending or PaymentStatus.Failed)
            {
                return ClosureResult.None;
            }

            row = TransitionTable.FindShipment(order.OrderStatus, OrderStatus.Delivered);
            actor = eventActor;
        }
        else
        {
            row = order.OrderStatus is OrderStatus.Placed or OrderStatus.Preparing
                ? TransitionTable.FindShipment(order.OrderStatus, OrderStatus.Cancelled)
                : null;
            actor = TransitionActor.System;
            reasons = TransitionReasons.OfKind(TransitionReason.Closure);
        }

        if (row is null || !TransitionTable.IsPairAllowed(row.To, order.PaymentStatus))
        {
            return ClosureResult.None;
        }

        if (reasons is not null && !ClosureCodes.Contains(row.Code))
        {
            throw new InvalidOperationException($"{row.Code} is not a closure that takes a reason.");
        }

        order.ApplyTransition(row);
        TransitionRecord record = TransitionRecord.Create(
            order.Id, row.Code, actor.Kind, actor.AdminId, actor.AdminName, reasons?.Automatic, null, null, null, null, _time.GetUtcNow());
        _db.TransitionRecords.Add(record);
        RaiseVersionOnce(order);
        return new ClosureResult(row.Code, record.Id);
    }

    private static bool TryPlan(
        Order order,
        TransitionRequest request,
        out List<(ShipmentTransition? Shipment, PaymentTransition? Payment)> steps,
        out OrderStatus orderStatus,
        out PaymentStatus paymentStatus,
        out TransitionRejection? refusal)
    {
        steps = [];
        orderStatus = order.OrderStatus;
        paymentStatus = order.PaymentStatus;
        refusal = null;
        TransitionReasons? reasons = request.Reasons is { IsEmpty: false } ? request.Reasons : null;
        foreach (TransitionCode code in request.Codes)
        {
            ShipmentTransition? shipment = TransitionTable.FindShipment(code);
            PaymentTransition? payment = TransitionTable.FindPayment(code);
            if (shipment is null && payment is null)
            {
                refusal = TransitionRejection.NotInTheTable;
                return false;
            }

            if (shipment is not null)
            {
                if (orderStatus != shipment.From)
                {
                    refusal = TransitionRejection.NotInTheTable;
                    return false;
                }

                orderStatus = shipment.To;
                steps.Add((shipment, null));
            }
            else
            {
                if (paymentStatus != payment!.From)
                {
                    refusal = TransitionRejection.NotInTheTable;
                    return false;
                }

                paymentStatus = payment.To;
                steps.Add((null, payment));
            }

            IReadOnlyList<ActorKind> actors = TransitionTable.ActorsOf(code)!;
            if (!actors.Contains(request.Actor.Kind) || !request.Actor.IsConsistent)
            {
                refusal = TransitionRejection.ActorNotAllowed;
                return false;
            }

            if (reasons is not null && !ReasonsAllowed(code, request.Actor.Kind, reasons))
            {
                refusal = TransitionRejection.ReasonNotAllowed;
                return false;
            }
        }

        return true;
    }

    /// <summary>The reason rules of the transition record (06 4.19), checked before the write so that a wrong call is a refusal, not a database error.</summary>
    private static bool ReasonsAllowed(TransitionCode code, ActorKind actor, TransitionReasons reasons)
    {
        if (!ReasonCodes.Contains(code) || (reasons.Automatic is not null && reasons.Firm is not null))
        {
            return false;
        }

        if (reasons.Note is not null && reasons.Firm is null)
        {
            return false;
        }

        if (reasons.Firm is not null)
        {
            return actor == ActorKind.Admin;
        }

        return reasons.Automatic switch
        {
            TransitionReason.Closure => ClosureCodes.Contains(code),
            null => true,
            _ => actor == ActorKind.System && code is TransitionCode.S3 or TransitionCode.O2,
        };
    }

    /// <summary>The operation raises the order's version by exactly one, however many writes it makes (06 2.6.4; <see cref="OrderVersionGuard"/>).</summary>
    private void RaiseVersionOnce(Order order) => RaiseVersionOnce(_db, order);

    /// <summary>
    /// The one rule of "raised by exactly one" (06 2.6.4): the version is raised unless the unit of work already raised it by one. The scheduled
    /// order change calls it after its own step, which may have been an operation of this unit that raised the version already.
    /// </summary>
    internal static void RaiseVersionOnce(AppDbContext db, Order order)
    {
        ArgumentNullException.ThrowIfNull(db);
        ArgumentNullException.ThrowIfNull(order);
        db.ChangeTracker.DetectChanges();
        PropertyEntry version = db.Entry(order).Property(ModelBuilderExtensions.VersionPropertyName);
        bool raised = version.IsModified && version.CurrentValue is int now && version.OriginalValue is int was && now == was + 1;
        if (!raised)
        {
            order.BumpVersion();
        }
    }

    /// <summary>
    /// The release of the order's reservation (06 4.14 path 3; 02 3.17.5): the payment window ran out, the card failed, the unpaid order was
    /// cancelled. Written in the transition unit, in the cancellation's transaction, after the order lock (05 7.3.5: the variants and the coupon
    /// follow the order): the reserved quantities and the reserved coupon right go down by the order's rows, and the rows are deleted. An order
    /// whose rows are gone - paid, deleted variant, digital only - frees nothing.
    /// </summary>
    private async Task ReleaseReservationAsync(Guid orderId, CancellationToken ct)
    {
        List<StockReservation> rows = await _db.StockReservations.Where(r => r.OrderId == orderId).ToListAsync(ct);
        if (rows.Count == 0)
        {
            return;
        }

        VariantQuantity[] variants = [.. rows.Where(r => r.VariantId is not null).Select(r => new VariantQuantity(r.VariantId!.Value, r.Quantity))];
        Guid? couponId = rows.Where(r => r.CouponId is not null).Select(r => r.CouponId).SingleOrDefault();
        await _catalog.ReleaseAsync(new StockClaim(variants, couponId), ct);
        _db.StockReservations.RemoveRange(rows);
    }
}
