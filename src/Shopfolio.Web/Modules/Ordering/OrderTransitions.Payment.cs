using System.Diagnostics.CodeAnalysis;

using Microsoft.EntityFrameworkCore;

using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Notifications.Contracts;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Modules.Payment.Contracts;

namespace Shopfolio.Modules.Ordering;

/// <summary>
/// The payment axis's operations of the transition unit (T317; 03 1.3; 02 5.5; 05 4.5.3, 4.6). Every status write goes through the core
/// (<see cref="ApplyAsync"/>): this part takes the order's lock, checks the operation's own condition, asks the core for the transitions by their
/// identity and writes the side effects after it, in the same transaction - the payment and refund records through the Payment module, the notices
/// through the Notifications module (09 9.1 item 6). An operation raises the order's version once, whether or not an axis moved (06 2.6.4).
/// </summary>
internal sealed partial class OrderTransitions
{
    public async Task<PaymentTransitionResult> ApplyCardPaymentAsync(CardPaymentSucceeded payment, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(payment);
        ArgumentException.ThrowIfNullOrWhiteSpace(payment.ProviderTransactionId);
        Order? order = await LockAndLoadAsync(payment.OrderId, ct);
        if (order is null)
        {
            return new PaymentTransitionResult([], isOrderNotFound: true);
        }

        if (order.PaymentMethod != PaymentMethod.Card)
        {
            return new PaymentTransitionResult([], isNotOpen: true);
        }

        // The same payment again - by the notification, the final query or the job row coming back - does nothing (05 4.5.3; K-970).
        if (await _payment.IsPaymentRecordedAsync(payment.ProviderTransactionId, ct))
        {
            return new PaymentTransitionResult([], isAlreadyApplied: true);
        }

        if (order.PaymentStatus == PaymentStatus.Pending)
        {
            // Ö1 by the system and, on an order with a physical item, S1 with it (03 1.2 S1; 02 5.6): one lock, one version raise.
            TransitionCode[] codes = (await OpenItemsAsync(order, physicalOnly: true, ct)).Count > 0 ? [TransitionCode.O1, TransitionCode.S1] : [TransitionCode.O1];
            TransitionOutcome approval = await ApplyAsync(new TransitionRequest(order.Id, codes, TransitionActor.System), null, ct);
            if (approval.IsRejected)
            {
                return PaymentRefused(approval);
            }

            await _payment.RecordCardPaymentAsync(order.Id, payment.ProviderTransactionId, approval.TransitionRecordIds[0], ct);
            await QueueNoticeAsync(order.Id, new PaymentConfirmedNotice(order.Id), ct);
            return new PaymentTransitionResult(codes);
        }

        // A payment to a cancelled order, or a second payment with a new transaction id to a paid one (02 6.2.16; K-1211): it stays in the payment
        // record without an Ö1 and the system pays it back to the card - the refund and its job row in this transaction. The axes do not move.
        Guid paymentRecordId = await _payment.RecordCardPaymentAsync(order.Id, payment.ProviderTransactionId, null, ct);
        Guid refundId = await _payment.StartLatePaymentRefundAsync(order.Id, paymentRecordId, order.Total, ct);
        RaiseVersionOnce(order);
        return new PaymentTransitionResult([], latePaymentRefundId: refundId);
    }

    public async Task<PaymentTransitionResult> MarkTransferPaidAsync(TransferPaidMark mark, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(mark);
        if (!mark.IsIrreversibleConfirmed)
        {
            return new PaymentTransitionResult([], isConfirmationRequired: true);
        }

        Order? order = await LockAndLoadAsync(mark.OrderId, ct);
        if (order is null)
        {
            return new PaymentTransitionResult([], isOrderNotFound: true);
        }

        if (order.Version != mark.ExpectedVersion)
        {
            return new PaymentTransitionResult([], isVersionConflict: true);
        }

        if (order.PaymentMethod != PaymentMethod.BankTransfer)
        {
            return new PaymentTransitionResult([], isNotOpen: true);
        }

        // Ö1 is the administrator's, S1 the system's (03 1.2 S1; D-291 (a)): two requests under the same lock; the core raises the version once.
        TransitionOutcome approval = await ApplyAsync(
            new TransitionRequest(order.Id, TransitionCode.O1, TransitionActor.Admin(mark.AdminId, mark.AdminName), expectedVersion: mark.ExpectedVersion), null, ct);
        if (approval.IsRejected)
        {
            return PaymentRefused(approval);
        }

        List<TransitionCode> applied = [TransitionCode.O1];
        if ((await OpenItemsAsync(order, physicalOnly: true, ct)).Count > 0)
        {
            TransitionOutcome preparing = await ApplyAsync(new TransitionRequest(order.Id, TransitionCode.S1, TransitionActor.System), null, ct);
            if (preparing.IsRejected)
            {
                return PaymentRefused(preparing);
            }

            applied.Add(TransitionCode.S1);
        }

        await _payment.RecordTransferMarkAsync(order.Id, approval.TransitionRecordIds[0], ct);
        await QueueNoticeAsync(order.Id, new PaymentConfirmedNotice(order.Id), ct);
        return new PaymentTransitionResult(applied);
    }

    public async Task<PaymentTransitionResult> CancelUnpaidOrderAsync(UnpaidOrderCancellation cancellation, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(cancellation);
        if (!TryReadCanceller(cancellation, out TransitionActor? actor, out TransitionReasons? reasons))
        {
            return new PaymentTransitionResult([], isNotOpen: true);
        }

        // The administrator's operation always compares the screen's version (05 7.3.2; 09 10.2 item 3; D-291 (b)).
        if (cancellation.Actor == ActorKind.Admin && cancellation.ExpectedVersion is null)
        {
            return new PaymentTransitionResult([], isVersionConflict: true);
        }

        // S3 and Ö2 in one request (03 1.3 Ö2): the core refuses a paid order - Ö2 leaves only Bekliyor - and writes nothing.
        TransitionCode[] codes = [TransitionCode.S3, TransitionCode.O2];
        TransitionOutcome outcome = await ApplyAsync(new TransitionRequest(cancellation.OrderId, codes, actor, reasons, cancellation.ExpectedVersion), null, ct);
        if (outcome.IsRejected)
        {
            return PaymentRefused(outcome);
        }

        Order order = await _db.Orders.SingleAsync(o => o.Id == cancellation.OrderId, ct);
        await QueueNoticeAsync(
            order.Id,
            new CancelledNotice(order.Id, cancellation.Actor, cancellation.FirmReason, cancellation.ReasonNote, cancellation.SystemReason, null),
            ct);
        return new PaymentTransitionResult(codes);
    }

    public async Task<PaymentTransitionResult> ApplyCardRefundOutcomeAsync(CardRefundOutcome outcome, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(outcome);
        Order? order = await LockAndLoadAsync(outcome.OrderId, ct);
        if (order is null)
        {
            return new PaymentTransitionResult([], isOrderNotFound: true);
        }

        RefundFacts? refund = await _payment.WriteCardRefundOutcomeAsync(order.Id, outcome.RefundRecordId, outcome.Outcome, ct);
        if (refund is null)
        {
            return new PaymentTransitionResult([], isNotOpen: true);
        }

        // A failed or unknown outcome leaves the axis and sends no notice; the panel mark is read from the refund record just written (05 4.6.2).
        if (!refund.IsSent)
        {
            RaiseVersionOnce(order);
            return new PaymentTransitionResult([]);
        }

        TransitionActor actor = refund is { ActorKind: ActorKind.Admin, AdminId: { } adminId, AdminName: { } adminName }
            ? TransitionActor.Admin(adminId, adminName)
            : TransitionActor.System;
        return await ApplyRefundSentAsync(order, refund, actor, ct);
    }

    public async Task<PaymentTransitionResult> ProcessTransferRefundAsync(TransferRefundProcessing processing, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(processing);
        if (!processing.IsIrreversibleConfirmed)
        {
            return new PaymentTransitionResult([], isConfirmationRequired: true);
        }

        Order? order = await LockAndLoadAsync(processing.OrderId, ct);
        if (order is null)
        {
            return new PaymentTransitionResult([], isOrderNotFound: true);
        }

        if (order.Version != processing.ExpectedVersion)
        {
            return new PaymentTransitionResult([], isVersionConflict: true);
        }

        RefundFacts? refund = await _payment.MarkTransferRefundSentAsync(order.Id, processing.RefundId, ct);
        if (refund is null)
        {
            return new PaymentTransitionResult([], isNotOpen: true);
        }

        return await ApplyRefundSentAsync(order, refund, TransitionActor.Admin(processing.AdminId, processing.AdminName), ct);
    }

    /// <summary>
    /// The money of a refund went (02 5.5: the axis tells the moment the money is actually sent): when the sent refunds reach the order's paid total,
    /// Ö4 from Ödendi or Ö5 from Kısmen geri ödendi; below it Ö3 from Ödendi and nothing from Kısmen geri ödendi. The late payment's refund moves no
    /// axis (02 6.2.16) and a terminal state is not left (03 1.3). B-8 goes on every sent refund; F-4 when the system refunded the card by itself and
    /// the success is not the firm's retry (03 1.3; 7.1.18).
    /// </summary>
    private async Task<PaymentTransitionResult> ApplyRefundSentAsync(Order order, RefundFacts refund, TransitionActor actor, CancellationToken ct)
    {
        List<TransitionCode> applied = [];
        if (refund.RefundType != RefundType.LatePayment && !TransitionTable.IsTerminal(order.PaymentStatus))
        {
            decimal sent = await _payment.SentRefundTotalAsync(order.Id, ct);
            PaymentStatus target = sent >= order.Total ? PaymentStatus.Refunded : PaymentStatus.PartiallyRefunded;
            PaymentTransition? row = TransitionTable.FindPayment(order.PaymentStatus, target);
            if (row is not null)
            {
                TransitionOutcome outcome = await ApplyAsync(new TransitionRequest(order.Id, row.Code, actor), null, ct);
                if (outcome.IsRejected)
                {
                    return PaymentRefused(outcome);
                }

                applied.Add(row.Code);
            }
        }

        RaiseVersionOnce(order);
        await QueueNoticeAsync(order.Id, new RefundProcessedNotice(order.Id, refund.RefundType, refund.Amount, refund.PaymentMethod), ct);
        if (refund is { ActorKind: ActorKind.System, PaymentMethod: PaymentMethod.Card, IsFirmRetry: false })
        {
            await _notifications.QueueCompanyNoticeAsync(
                new CardRefundNotice(refund.RefundId, order.Id, order.OrderNumber, refund.RefundType, refund.Amount, _time.GetUtcNow()), ct);
        }

        return new PaymentTransitionResult(applied);
    }

    /// <summary>
    /// The canceller of an unpaid order and the reason its records carry (06 4.19; 03 1.3 Ö2): the customer gives none; the administrator the firm's
    /// reason and the screen's version; the system an automatic kind - never the closure, which is the closure rule's (05 4.1.3).
    /// </summary>
    private static bool TryReadCanceller(
        UnpaidOrderCancellation cancellation, [NotNullWhen(true)] out TransitionActor? actor, out TransitionReasons? reasons)
    {
        actor = null;
        reasons = null;
        bool hasAdmin = cancellation.AdminId is not null || cancellation.AdminName is not null;
        switch (cancellation.Actor)
        {
            case ActorKind.Buyer when !hasAdmin && cancellation is { FirmReason: null, ReasonNote: null, SystemReason: null }:
                actor = TransitionActor.Buyer;
                return true;
            case ActorKind.Admin when cancellation is { AdminId: { } adminId, AdminName: { } adminName, FirmReason: { } firm, SystemReason: null }:
                actor = TransitionActor.Admin(adminId, adminName);
                reasons = TransitionReasons.OfFirm(firm, cancellation.ReasonNote);
                return true;
            case ActorKind.System when !hasAdmin && cancellation is { SystemReason: { } kind, FirmReason: null, ReasonNote: null } && kind != TransitionReason.Closure:
                actor = TransitionActor.System;
                reasons = TransitionReasons.OfKind(kind);
                return true;
            default:
                return false;
        }
    }

    private static PaymentTransitionResult PaymentRefused(TransitionOutcome outcome)
    {
        return outcome.Rejection switch
        {
            TransitionRejection.OrderNotFound => new PaymentTransitionResult([], isOrderNotFound: true),
            TransitionRejection.VersionConflict => new PaymentTransitionResult([], isVersionConflict: true),
            _ => new PaymentTransitionResult([], isNotOpen: true),
        };
    }

    private async Task<Order?> LockAndLoadAsync(Guid orderId, CancellationToken ct)
    {
        await _locks.LockAsync([orderId], ct);
        return await _db.Orders.SingleOrDefaultAsync(o => o.Id == orderId, ct);
    }
}
