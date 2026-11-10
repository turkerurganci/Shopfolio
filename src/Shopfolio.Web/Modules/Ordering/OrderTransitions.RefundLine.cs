using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure.Forms;
using Shopfolio.Modules.AfterSales.Contracts;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Notifications.Contracts;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Modules.Payment.Contracts;

namespace Shopfolio.Modules.Ordering;

/// <summary>
/// The refund line's operations in the order (T370; 02 7.2.8, 7.4.5; 03 2.8.4.1; 06 4.29, 4.33, 4.34): the refund an item record causes starts in the record's own
/// transaction - on the card the system starts it without approval and opens no IBAN field, on the transfer the firm processes it from the panel and the
/// firm's cancellation asks the customer for an IBAN (B-14); the firm's retry and transfer path of a failed card refund; the customer's IBAN entry and correction;
/// the firm's "havale gerçekleşmedi". Every operation works under the order's row lock (09 9.1 item 4); the way and the open actions are the Payment module's one
/// rule (<c>RefundLineRules</c>), the IBAN's life is the AfterSales module's.
/// </summary>
internal sealed partial class OrderTransitions
{
    public async Task<OrderTransitionResult> RetryCardRefundAsync(RetryCardRefundRequest request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        (Order? order, TransitionRefusal? refusal) = await LockForAdminAsync(request.OrderId, request.Admin, request.ExpectedVersion, ct);
        if (order is null)
        {
            return Refused(refusal ?? TransitionRefusal.OrderNotFound);
        }

        // The retry closes once the customer entered an IBAN on the transfer path: the same money never goes two ways (02 7.2.8; K-569).
        bool ibanEntered = await _afterSales.IsRefundIbanEnteredForRefundAsync(request.RefundId, ct);
        if (!await _payment.RetryCardRefundAsync(order.Id, request.RefundId, ibanEntered, ct))
        {
            return Refused(TransitionRefusal.ConditionNotMet);
        }

        RaiseVersionOnce(order);
        return new OrderTransitionResult(null, [], null, []);
    }

    public async Task<OrderTransitionResult> OpenRefundTransferPathAsync(OpenRefundTransferPathRequest request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        (Order? order, TransitionRefusal? refusal) = await LockForAdminAsync(request.OrderId, request.Admin, request.ExpectedVersion, ct);
        if (order is null)
        {
            return Refused(refusal ?? TransitionRefusal.OrderNotFound);
        }

        CardRefundLine? line = await _payment.ReadRefundLineAsync(order.Id, request.RefundId, ct);
        bool ibanEntered = await _afterSales.IsRefundIbanEnteredForRefundAsync(request.RefundId, ct);
        if (line is null || !ActionsOf(line, ibanEntered).CanOpenTransferPath)
        {
            return Refused(TransitionRefusal.ConditionNotMet);
        }

        RefundIbanRequestOpened opened = await _afterSales.OpenRefundIbanRequestAsync(
            new RefundIbanRequestEntry(line.OrderItemId, line.RefundId, RefundIbanRequestReason.CardRefundFailed, request.Admin.AdminId, request.Admin.AdminName, _time.GetUtcNow()),
            ct);
        if (opened.IsNew)
        {
            await QueueNoticeAsync(order.Id, new RefundIbanRequestedNotice(order.Id, RefundIbanRequestReason.CardRefundFailed), ct);
        }

        RaiseVersionOnce(order);
        return new OrderTransitionResult(null, [], null, line.OrderItemId is { } itemId ? [itemId] : []);
    }

    public async Task<OrderTransitionResult> EnterRefundIbanAsync(EnterRefundIbanRequest request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        Order? order = await LockAndLoadAsync(request.OrderId, ct);
        if (order is null)
        {
            return Refused(TransitionRefusal.OrderNotFound);
        }

        OpenRefundIbanRequest? open = await _afterSales.FindOpenRefundIbanRequestAsync(request.RequestId, ct);
        if (open is null || !await IsOrdersAsync(order.Id, open, ct))
        {
            return Refused(TransitionRefusal.ConditionNotMet);
        }

        // On a failed card refund's transfer path the IBAN turns the refund's way to the transfer - only while its latest card request is still failed;
        // a refund that went meanwhile, or one whose result is pending, takes no IBAN (02 7.2.8; K-569).
        if (open.RefundId is { } refundId && open.Reason == RefundIbanRequestReason.CardRefundFailed)
        {
            CardRefundLine? line = await _payment.ReadRefundLineAsync(order.Id, refundId, ct);
            if (line is null || !ActionsOf(line, true).CanTurnToTransfer)
            {
                return Refused(TransitionRefusal.ConditionNotMet);
            }
        }

        RefundIbanWriteResult written = await _afterSales.EnterRefundIbanAsync(new RefundIbanEntry(order.Id, open.RequestId, request.Iban, _time.GetUtcNow()), ct);
        if (written.RefundIbanId is null)
        {
            return Refused(written.IsInvalidIban ? TransitionRefusal.InvalidInput : TransitionRefusal.ConditionNotMet);
        }

        if (open.RefundId is { } turned && open.Reason == RefundIbanRequestReason.CardRefundFailed)
        {
            await _payment.TurnRefundToTransferAsync(order.Id, turned, ct);
        }

        RaiseVersionOnce(order);
        return new OrderTransitionResult(null, [], null, open.OrderItemId is { } itemId ? [itemId] : []);
    }

    public async Task<OrderTransitionResult> ChangeRefundIbanAsync(ChangeRefundIbanRequest request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        Order? order = await LockAndLoadAsync(request.OrderId, ct);
        if (order is null)
        {
            return Refused(TransitionRefusal.OrderNotFound);
        }

        RefundIbanWriteResult written = await _afterSales.ChangeRefundIbanAsync(new RefundIbanCorrection(order.Id, request.RefundIbanId, request.Iban, _time.GetUtcNow()), ct);
        if (written.RefundIbanId is null)
        {
            return Refused(written.IsInvalidIban ? TransitionRefusal.InvalidInput : TransitionRefusal.ConditionNotMet);
        }

        RaiseVersionOnce(order);
        return new OrderTransitionResult(null, [], null, []);
    }

    public async Task<OrderTransitionResult> ReportTransferFailedAsync(ReportTransferFailedRequest request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        if ((request.OrderItemId is null) == (request.RefundId is null))
        {
            return Refused(TransitionRefusal.InvalidInput);
        }

        (Order? order, TransitionRefusal? refusal) = await LockForAdminAsync(request.OrderId, request.Admin, request.ExpectedVersion, ct);
        if (order is null)
        {
            return Refused(refusal ?? TransitionRefusal.OrderNotFound);
        }

        // The item (or the refund, read tracked) is loaded under the lock: the request opened on it is a record whose root the version guard must tell (D-95; 09 10.2 item 3).
        if (order.PaymentMethod != PaymentMethod.BankTransfer
            || (request.OrderItemId is { } itemId && await _db.OrderItems.SingleOrDefaultAsync(i => i.Id == itemId && i.OrderId == order.Id, ct) is null)
            || (request.RefundId is { } refundId && await _payment.ReadRefundLineAsync(order.Id, refundId, ct) is null))
        {
            return Refused(TransitionRefusal.ConditionNotMet);
        }

        // The IBAN is deleted with its disposal record and the field opens again; the period does not stop (02 7.4.5; K-575, K-1131).
        RefundIbanRequestOpened? opened = await _afterSales.ReportTransferFailedAsync(
            new TransferFailure(order.Id, request.OrderItemId, request.RefundId, request.Admin.AdminId, request.Admin.AdminName, _time.GetUtcNow()), ct);
        if (opened is null)
        {
            return Refused(TransitionRefusal.ConditionNotMet);
        }

        if (opened.IsNew)
        {
            await QueueNoticeAsync(order.Id, new RefundIbanRequestedNotice(order.Id, RefundIbanRequestReason.TransferFailed), ct);
        }

        RaiseVersionOnce(order);
        return new OrderTransitionResult(null, [], null, request.OrderItemId is { } item ? [item] : []);
    }

    /// <summary>
    /// The refund an item record causes, in the record's transaction under the order's lock (02 7.2.8, 7.4.5; 06 4.33; D-791, D-818, D-843): only on an order whose
    /// money came (Ödendi, Kısmen geri ödendi). The amount is the record's product amount less its coupon share, with the shipping fee when the fee rule includes it
    /// (02 7.2.9). On the card the system starts the refund and its job; on the transfer the firm processes it - and when no IBAN came with the record the firm's
    /// step opens the IBAN request and B-14 (K-690); the customer's own cancellation brings its IBAN with it. The period does not wait for the IBAN (02 7.2.8).
    /// </summary>
    private async Task StartRefundOfRecordAsync(
        Order order, OrderItem item, RefundType type, Guid recordId, bool hasIban, TransitionAdmin? admin, RefundIbanRequestReason reason, CancellationToken ct)
    {
        if (order.PaymentStatus is not (PaymentStatus.Paid or PaymentStatus.PartiallyRefunded))
        {
            return;
        }

        RefundSubject subject = new(type, recordId);
        RefundAmount amount = await _afterSales.RefundAmountAsync(subject, ct)
            ?? throw new InvalidOperationException("The record whose refund starts is not in the caller's context.");
        bool feeIncluded = await IsShippingFeeIncludedAsync(order, subject, ct);
        decimal total = amount.Amount + (feeIncluded ? order.ShippingFee : 0m);
        RecordRefundStarted started = await _payment.StartRecordRefundAsync(
            new RecordRefundStart(order.Id, order.PaymentMethod, type, total, feeIncluded, item.Id, amount.Quantity, recordId, hasIban), ct);
        if (started.NeedsIbanField && admin is not null)
        {
            RefundIbanRequestOpened opened = await _afterSales.OpenRefundIbanRequestAsync(
                new RefundIbanRequestEntry(item.Id, null, reason, admin.AdminId, admin.AdminName, _time.GetUtcNow()), ct);
            if (opened.IsNew)
            {
                await QueueNoticeAsync(order.Id, new RefundIbanRequestedNotice(order.Id, reason), ct);
            }
        }
    }

    /// <summary>The shipping fee rule's question for the record (02 7.2.9, 7.4.6): the order's physical items and the moment the carrier first took the order.</summary>
    private async Task<bool> IsShippingFeeIncludedAsync(Order order, RefundSubject subject, CancellationToken ct)
    {
        List<ShippedItem> physical = await _db.OrderItems
            .Where(i => i.OrderId == order.Id && i.ProductType == ProductType.Physical)
            .Select(i => new ShippedItem(i.Id, i.Quantity, i.DeliveryDate))
            .ToListAsync(ct);
        if (physical.Count == 0)
        {
            return false;
        }

        DateTimeOffset? handed = await OrderPeriods.Shipments(_db)
            .Where(t => t.OrderId == order.Id && t.TransitionCode == TransitionCode.S5)
            .OrderBy(t => t.CreatedAt)
            .Select(t => (DateTimeOffset?)t.CreatedAt)
            .FirstOrDefaultAsync(ct);
        return await _afterSales.IsShippingFeeIncludedAsync(new ShippingFeeQuery(physical, handed, subject), ct);
    }

    /// <summary>After a refund settled or a record was undone: the order's IBANs no pending refund uses any more go, each with its disposal record (02 7.4.5; Z-44).</summary>
    private async Task DeleteUnusedIbansAsync(Guid orderId, CancellationToken ct) => await _afterSales.DeleteUnusedRefundIbansAsync(orderId, ct);

    private async Task<bool> IsOrdersAsync(Guid orderId, OpenRefundIbanRequest open, CancellationToken ct)
    {
        if (open.OrderItemId is { } itemId)
        {
            return await _db.OrderItems.AnyAsync(i => i.Id == itemId && i.OrderId == orderId, ct);
        }

        return open.RefundId is { } refundId && await _payment.ReadRefundLineAsync(orderId, refundId, ct) is not null;
    }

    private async Task<(Order? Order, TransitionRefusal? Refusal)> LockForAdminAsync(Guid orderId, TransitionAdmin admin, int expectedVersion, CancellationToken ct)
    {
        if (!TryAdminActor(admin, out _))
        {
            return (null, TransitionRefusal.ActorNotAllowed);
        }

        Order? order = await LockAndLoadAsync(orderId, ct);
        if (order is null)
        {
            return (null, TransitionRefusal.OrderNotFound);
        }

        return !ShownVersion.Matches(order, expectedVersion) ? (null, TransitionRefusal.VersionConflict) : (order, null);
    }

    private CardRefundActions ActionsOf(CardRefundLine line, bool ibanEntered) => _payment.RefundActionsOf(line, ibanEntered);
}
