using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Modules.AfterSales.Contracts;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Notifications.Contracts;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Modules.Payment.Contracts;

namespace Shopfolio.Modules.Ordering;

/// <summary>
/// The correction of a wrong transition (T319; 03 1.6.2; 02 10.4.6; 05 4.1.1, 4.1.4): the administrator's new transition, with a reason from the closed list,
/// that sets the axis back. The unit's core (<see cref="CorrectAsync"/>: lock, version, the corrected row still the state, the pair rule, then this file's
/// condition) appends the correction beside the wrong row - neither is changed or deleted - and this file writes what the correction owns beyond the status, in
/// the same transaction: the delivery marks that go (1.6.2.1), the units taken again, the voided refund and the closed IBAN request (1.6.2.2), the closure rule
/// (1.6.2.3), the new payment window (1.6.2.4), and B-13. What went outside - an e-mail, a parcel, money sent to a card - is not taken back. The correctable
/// rows are S6, the firm's S3/S4/S9 of a paid order, S5, S7, S8 and the transfer's Ö1 (with its S1); every other row has no correction - S1 alone, the
/// closure's S2, S10, S11 and cancellations, the customer's and the system's cancellations, Ö2-Ö5, the card's Ö1 - and a withdrawal is not a transition and is
/// never corrected (03 1.6.2; K-688).
/// </summary>
internal sealed partial class OrderTransitions
{
    private static readonly TransitionCode[] CancellationCodes = [TransitionCode.S3, TransitionCode.S4, TransitionCode.S9];

    public async Task<OrderTransitionResult> CorrectTransitionAsync(CorrectTransitionRequest request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (!TryAdminActor(request.Admin, out TransitionActor? actor))
        {
            return Refused(TransitionRefusal.ActorNotAllowed);
        }

        // The reason is from the closed list - a reasonless correction is refused (04 9.9.36) - and "Other" carries the free note, no other reason does (06 4.19).
        string? note = string.IsNullOrWhiteSpace(request.ReasonNote) ? null : request.ReasonNote.Trim();
        if (request.Reason is not { } reason || !Enum.IsDefined(reason) || (reason == CorrectionReason.Other) != (note is not null))
        {
            return Refused(TransitionRefusal.InvalidInput);
        }

        CorrectionPlan plan = new();
        TransitionOutcome outcome = await CorrectAsync(
            new CorrectionRequest(request.OrderId, request.TransitionRecordId, reason, note, actor!, request.ExpectedVersion),
            (order, corrected, token) => CorrectionConditionAsync(order, corrected, plan, token),
            ct);
        if (outcome.IsRejected)
        {
            return outcome.Rejection switch
            {
                TransitionRejection.ConditionNotMet => NotPossible(plan.Obstacle ?? CorrectionObstacle.NotCorrectable, plan.ShortItemIds),
                TransitionRejection.CorrectionNotPossible or TransitionRejection.ForbiddenStatePair => NotPossible(await ObstacleOfAsync(request, ct), []),
                _ => Refused(outcome),
            };
        }

        Order order = await _db.Orders.SingleAsync(o => o.Id == request.OrderId, ct);
        DateTimeOffset? newWindowEnd = null;
        ClosureResult closure = ClosureResult.None;
        List<Guid> itemIds = [];
        switch (plan.Code)
        {
            case TransitionCode.O1:
                // Z-8 starts again from the correction's moment, and B-3 goes once more on the new window's last business day (03 1.6.2.4; 4.1.8, 4.1.9; K-685).
                PaymentWindow window = await _paymentWindows.StartAsync(PaymentMethod.BankTransfer, _time.GetUtcNow(), ct);
                order.PaymentWindowEndsAt = window.EndsAt;
                order.PaymentReminderAt = window.ReminderAt;
                newWindowEnd = window.EndsAt;
                break;

            case TransitionCode.S3 or TransitionCode.S4 or TransitionCode.S9:
                // The cancelled units are taken again and fall at once (1.4.4.7); the read before the write named no short item, so a false here is a race.
                if (!await _counters.RetakeAsync(order, plan.Units, ct))
                {
                    return NotPossible(CorrectionObstacle.StockShort, [.. plan.UnitItems.Keys]);
                }

                // The cancellation is undone (06 4.22): its unsent refunds are voided and their open IBAN requests close; the closure rule is not read.
                IReadOnlyList<Guid> voided = await _payment.VoidCancellationRefundsAsync(plan.CancellationIds, ct);
                await _afterSales.CloseCancellationIbanRequestsAsync(plan.CancelledItemIds, voided, ct);
                itemIds.AddRange(plan.CancelledItemIds);
                break;

            case TransitionCode.S5 or TransitionCode.S8:
                // An order with no open item left passes the closure rule in the same operation (1.6.2.3; 1.4.3); the carrier data stays (02 10.4.6).
                closure = await EvaluateClosureAsync(order, actor!, ct);
                break;

            case TransitionCode.S6:
                // The delivery mark goes from the physical items it covered; the withdrawal and defect periods wait for the new date (1.6.2.1).
                foreach (OrderItem item in plan.UnmarkedItems)
                {
                    item.DeliveryMarkedAt = null;
                    item.DeliveryDate = null;
                    itemIds.Add(item.Id);
                }

                break;
        }

        // Every correction tells the customer the order's state after it (02 10.4.6; 08 2.2.6 B-13).
        await QueueNoticeAsync(order.Id, new StatusCorrectedNotice(order.Id, order.OrderStatus, newWindowEnd), ct);
        return Applied(new TransitionOutcome(null, outcome.TransitionRecordIds, closure), itemIds);
    }

    /// <summary>The correction's own condition (03 1.6.2), read against the locked order and the corrected row after the core's checks; it writes nothing.</summary>
    private async Task<bool> CorrectionConditionAsync(Order order, TransitionRecord corrected, CorrectionPlan plan, CancellationToken ct)
    {
        plan.Code = corrected.TransitionCode;
        switch (corrected.TransitionCode)
        {
            case TransitionCode.O1:
                return await TransferMarkIsCorrectableAsync(order, plan, ct);

            case TransitionCode.S3 or TransitionCode.S4 or TransitionCode.S9:
                return await CancellationIsCorrectableAsync(order, corrected, plan, ct);

            case TransitionCode.S6:
                plan.UnmarkedItems.AddRange(await _db.OrderItems
                    .Where(i => i.OrderId == order.Id && i.ProductType == ProductType.Physical && i.DeliveryMarkedAt != null)
                    .ToListAsync(ct));
                return true;

            // 1.6.2.3: the shipment, the re-sending and the returned shipment go back with the axes' tie kept (the core's pair rule); what was recorded meanwhile stays.
            case TransitionCode.S5 or TransitionCode.S7 or TransitionCode.S8:
                return true;

            default:
                plan.Obstacle = CorrectionObstacle.NotCorrectable;
                return false;
        }
    }

    /// <summary>
    /// 1.6.2.4: the transfer's "paid" mark is corrected only on the transfer line (the card has none), never on an order with a digital item (the download opened;
    /// 02 10.4.6), and only while no item carries a delivery mark, nothing was shipped, no refund was made and no item has a cancellation, removal, late
    /// delivery termination or withdrawal record (K-703).
    /// </summary>
    private async Task<bool> TransferMarkIsCorrectableAsync(Order order, CorrectionPlan plan, CancellationToken ct)
    {
        if (order.PaymentMethod != PaymentMethod.BankTransfer)
        {
            plan.Obstacle = CorrectionObstacle.CardPayment;
            return false;
        }

        List<OrderItem> items = await _db.OrderItems.Where(i => i.OrderId == order.Id).ToListAsync(ct);
        if (items.Any(i => i.ProductType == ProductType.Digital))
        {
            plan.Obstacle = CorrectionObstacle.DigitalItem;
            return false;
        }

        // A shipment is an S5 or S8 row that was not itself corrected: a wrong shipment mark that was corrected sent nothing (1.6.2.3).
        bool shipped = await _db.TransitionRecords.AnyAsync(
            t => t.OrderId == order.Id
                && (t.TransitionCode == TransitionCode.S5 || t.TransitionCode == TransitionCode.S8)
                && !_db.TransitionRecords.Any(c => c.CorrectedTransitionRecordId == t.Id),
            ct);
        if (items.Any(i => i.DeliveryMarkedAt is not null)
            || shipped
            || await _payment.HasRefundAsync(order.Id, ct)
            || await _afterSales.HasClosingRecordAsync([.. items.Select(i => i.Id)], ct))
        {
            plan.Obstacle = CorrectionObstacle.ItemRecordExists;
            return false;
        }

        return true;
    }

    /// <summary>
    /// 1.6.2.2: the firm's cancellation (a reason from the closed list - not the closure, not the customer's or the system's) is corrected only while the payment
    /// is Paid and no refund of it is under way (K-940), and only if every variant still has the cancelled units left (1.4.4.7); a deleted variant asks for
    /// none and does not stop it (K-1091).
    /// </summary>
    private async Task<bool> CancellationIsCorrectableAsync(Order order, TransitionRecord corrected, CorrectionPlan plan, CancellationToken ct)
    {
        if (!IsFirmCancellation(corrected))
        {
            plan.Obstacle = CorrectionObstacle.NotCorrectable;
            return false;
        }

        if (order.PaymentStatus != PaymentStatus.Paid)
        {
            plan.Obstacle = CorrectionObstacle.PaymentNotPaid;
            return false;
        }

        IReadOnlyList<CancelledUnits> cancellations = await _afterSales.FindCancellationsOfTransitionAsync(corrected.Id, ct);
        plan.CancellationIds.AddRange(cancellations.Select(c => c.CancellationId));
        plan.CancelledItemIds.AddRange(cancellations.Select(c => c.OrderItemId).Distinct());
        if (await _payment.IsCancellationRefundUnderwayAsync(plan.CancellationIds, ct))
        {
            plan.Obstacle = CorrectionObstacle.RefundPending;
            return false;
        }

        // The physical and service items' variants are taken again; a digital item reserves nothing and an item whose variant is gone asks for nothing.
        List<OrderItem> items = await _db.OrderItems.Where(i => plan.CancelledItemIds.Contains(i.Id)).ToListAsync(ct);
        foreach (CancelledUnits cancellation in cancellations)
        {
            OrderItem item = items.Single(i => i.Id == cancellation.OrderItemId);
            if (item.ProductType != ProductType.Digital && item.VariantId is { } variantId)
            {
                plan.Units.Add(new VariantQuantity(variantId, cancellation.Quantity));
                plan.UnitItems[item.Id] = variantId;
            }
        }

        IReadOnlyList<Guid> shortVariants = await _counters.ShortVariantsAsync(plan.Units, ct);
        if (shortVariants.Count > 0)
        {
            plan.Obstacle = CorrectionObstacle.StockShort;
            plan.ShortItemIds.AddRange(plan.UnitItems.Where(u => shortVariants.Contains(u.Value)).Select(u => u.Key));
            return false;
        }

        return true;
    }

    /// <summary>
    /// The obstacle of a correction the core refused before the condition was read (the row is not correctable, its result is no longer the state, or the
    /// axes would be left in a forbidden pair): read under the lock the core took, in the transaction the runner rolls back.
    /// </summary>
    private async Task<CorrectionObstacle> ObstacleOfAsync(CorrectTransitionRequest request, CancellationToken ct)
    {
        TransitionRecord? record = await _db.TransitionRecords.AsNoTracking()
            .SingleOrDefaultAsync(t => t.Id == request.TransitionRecordId && t.OrderId == request.OrderId, ct);
        if (record is null || await _db.TransitionRecords.AnyAsync(t => t.CorrectedTransitionRecordId == record.Id, ct))
        {
            return CorrectionObstacle.NotCorrectable;
        }

        Order order = await _db.Orders.AsNoTracking().SingleAsync(o => o.Id == request.OrderId, ct);
        if (record.TransitionCode == TransitionCode.O1)
        {
            // The transfer mark of an order that went on - shipped, delivered, refunded or cancelled - has a record that stops it (1.6.2.4).
            if (order.PaymentMethod != PaymentMethod.BankTransfer)
            {
                return CorrectionObstacle.CardPayment;
            }

            return await _db.OrderItems.AnyAsync(i => i.OrderId == order.Id && i.ProductType == ProductType.Digital, ct)
                ? CorrectionObstacle.DigitalItem
                : CorrectionObstacle.ItemRecordExists;
        }

        return CancellationCodes.Contains(record.TransitionCode) && IsFirmCancellation(record) && order.PaymentStatus != PaymentStatus.Paid
            ? CorrectionObstacle.PaymentNotPaid
            : CorrectionObstacle.NotCorrectable;
    }

    /// <summary>The firm's cancellation: the administrator's S3, S4 or S9 with a reason from the closed list - not the closure, which carries the automatic reason.</summary>
    private static bool IsFirmCancellation(TransitionRecord record) =>
        record.ActorKind == ActorKind.Admin && record.CancellationReason is not null && record.TransitionReason is null;

    private static OrderTransitionResult NotPossible(CorrectionObstacle obstacle, IReadOnlyList<Guid> itemIds) =>
        new(TransitionRefusal.CorrectionNotPossible, [], null, itemIds) { Obstacle = obstacle };

    /// <summary>What the condition found, for the writes after the core: the corrected code, the obstacle of a refusal, and the rows the correction changes.</summary>
    private sealed class CorrectionPlan
    {
        public TransitionCode? Code { get; set; }

        public CorrectionObstacle? Obstacle { get; set; }

        public List<Guid> ShortItemIds { get; } = [];

        public List<OrderItem> UnmarkedItems { get; } = [];

        public List<Guid> CancellationIds { get; } = [];

        public List<Guid> CancelledItemIds { get; } = [];

        public List<VariantQuantity> Units { get; } = [];

        /// <summary>The items whose units are taken again, with their variant.</summary>
        public Dictionary<Guid, Guid> UnitItems { get; } = [];
    }
}
