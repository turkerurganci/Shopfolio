using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Forms;
using Shopfolio.Infrastructure.Operations;
using Shopfolio.Modules.AfterSales.Contracts;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Notifications.Contracts;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Modules.Payment.Contracts;

namespace Shopfolio.Modules.Ordering;

/// <summary>
/// The late delivery termination (T368; 03 1.11.8, 1.11.41, 1.4.4.5; 02 7.2.3, 5.8, 10.4.10): a record type of its own beside the cancellation (06 4.24). The customer
/// terminates from the order page; the firm records a notice that came by another channel, dated the day it reached the firm. Both read the operation decision's one
/// rule (Z-11, <see cref="DeliveryPeriodRules.IsLateDeliveryTerminationOpen"/>) under the order's lock and both work alike: every undelivered physical item's open units
/// get one record each, no unit is chosen; digital and service items go on. A terminated item is not shipped, not re-sent and not the firm's cancellation (its open
/// quantity is gone); the order closes by the closure rule - a preparing order at once, a returned one with the firm's closing - and no B-7 goes (K-592).
/// </summary>
internal sealed partial class OrderTransitions
{
    public async Task<OrderTransitionResult> TerminateForLateDeliveryAsync(TerminateForLateDeliveryRequest request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        DateTimeOffset now = _time.GetUtcNow();
        DateOnly today = TurkeyTime.DateOf(now);
        TransitionActor? actor = TransitionActor.Buyer;
        ItemRecordAdmin? admin = null;
        OrderOperation operation = OrderOperation.TerminateForLateDelivery;
        if (request.Admin is null)
        {
            // The customer's button carries no version, no confirmation flag and no date: the day is the day of the button (02 7.2.3).
            if (request.ExpectedVersion is not null || request.Confirmed || request.NoticeArrivalDate is not null)
            {
                return Refused(TransitionRefusal.InvalidInput);
            }
        }
        else
        {
            if (!TryAdminActor(request.Admin, out actor))
            {
                return Refused(TransitionRefusal.ActorNotAllowed);
            }

            if (!request.Confirmed)
            {
                return Refused(TransitionRefusal.ConfirmationRequired);
            }

            // The day the notice reached the firm may be in the past, never in the future (02 10.4.10).
            if (request.ExpectedVersion is null || request.NoticeArrivalDate is not { } arrival || arrival > today)
            {
                return Refused(TransitionRefusal.InvalidInput);
            }

            admin = new ItemRecordAdmin(request.Admin.AdminId, request.Admin.AdminName);
            operation = OrderOperation.RecordLateTerminationFromOtherChannel;
        }

        await _locks.LockAsync([request.OrderId], ct);
        Order? order = await _db.Orders.SingleOrDefaultAsync(o => o.Id == request.OrderId, ct);
        if (order is null)
        {
            return Refused(TransitionRefusal.OrderNotFound);
        }

        switch (ShownVersion.Check(order, request.ExpectedVersion, byAdministrator: request.Admin is not null))
        {
            case ShownVersionCheck.Missing:
                return Refused(TransitionRefusal.InvalidInput);
            case ShownVersionCheck.Changed:
                return Refused(TransitionRefusal.VersionConflict);
        }

        DateOnly declarationDate = request.NoticeArrivalDate ?? today;
        if (declarationDate < TurkeyTime.DateOf(order.CreatedAt))
        {
            return Refused(TransitionRefusal.InvalidInput);
        }

        OperationVerdict verdict = await _operations.CheckAsync(order.Id, operation, null, null, ct);
        if (!verdict.IsAllowed)
        {
            return Refused(TransitionRefusal.ConditionNotMet);
        }

        List<OrderItem> items = await _db.OrderItems.Where(i => i.OrderId == order.Id).OrderBy(i => i.CreatedAt).ThenBy(i => i.Id).ToListAsync(ct);
        IReadOnlyDictionary<Guid, int> closed = await _afterSales.UnavailableQuantitiesAsync([.. items.Select(i => i.Id)], ct);
        List<(OrderItem Item, int Open)> terminated =
        [
            .. items
                .Where(i => i.ProductType == ProductType.Physical && i.DeliveryDate is null)
                .Select(i => (Item: i, Open: i.Quantity - closed.GetValueOrDefault(i.Id)))
                .Where(t => t.Open > 0),
        ];
        if (terminated.Count == 0)
        {
            return Refused(TransitionRefusal.ConditionNotMet);
        }

        List<Guid> recordIds = [];
        foreach ((OrderItem item, int open) in terminated)
        {
            ItemRecordWriteResult written = await _afterSales.AddLateDeliveryTerminationAsync(
                new LateDeliveryTerminationEntry(Subject(order, item), open, FrozenOf(item), admin, declarationDate, now), ct);
            if (written.Refusal is { } refused)
            {
                return Refused(Map(refused));
            }

            Guid terminationId = written.RecordId ?? throw new InvalidOperationException("The termination writer returned no record.");
            recordIds.Add(terminationId);

            // The refund starts with the record (T370; 02 7.2.3, 7.2.8): on the card by the system; on the transfer the firm processes it, and the firm's record
            // of a notice from another channel asks for the IBAN with B-14 (03 8.4.8); the IBAN field of the customer's own termination form is D-817's.
            await StartRefundOfRecordAsync(order, item, RefundType.LateDeliveryTermination, terminationId, false, request.Admin, RefundIbanRequestReason.NoticeWithoutIban, ct);
        }

        RaiseVersionOnce(order);

        // The order's line: a preparing order with no open item left closes at once (S4, reason Closure, no B-7); a shipped or returned order waits for the return
        // and the firm's closing (K-592). The closure rule decides; the termination itself writes no transition.
        bool shipped = order.OrderStatus is not (OrderStatus.Placed or OrderStatus.Preparing);
        ClosureResult closure = await EvaluateClosureAsync(order, actor, ct);

        // 1.4.4.5: an item not shipped gives its units back to stock at once; goods coming back from the carrier are added by the receipt step. The coupon right goes
        // back when the order is finished (OrderCounters.IsCouponReturnDueAsync).
        List<VariantQuantity> units = shipped ? [] : [.. terminated.SelectMany(t => UnitsOf(t.Item, t.Open))];
        await _counters.ReturnAsync(order, units, ct);

        // B-9 tells the customer the termination and its date (08 2.2.6 B-9; also the record of a notice from another channel, 03 1.6.1.7); F-3 tells the firm the
        // customer's own termination - the firm's record needs no notice to the firm.
        List<NoticeItem> noticeItems = [.. terminated.Select(t => new NoticeItem(t.Item.ProductName, t.Item.FirstOptionValue, t.Item.SecondOptionValue, t.Open))];
        await QueueNoticeAsync(
            order.Id,
            new DeclarationReceivedNotice(order.Id, DispatchRelatedRecordType.LateDeliveryTermination, recordIds[0], false, declarationDate, null, noticeItems, null),
            ct);
        if (admin is null)
        {
            await _notifications.QueueCompanyNoticeAsync(
                new NewClaimNotice(DispatchRelatedRecordType.LateDeliveryTermination, recordIds[0], order.Id, order.OrderNumber, false, now), ct);
        }

        List<Guid> transitionIds = closure.TransitionRecordId is { } closureId ? [closureId] : [];
        return new OrderTransitionResult(null, transitionIds, closure.Code, [.. terminated.Select(t => t.Item.Id)]) { ItemRecordIds = recordIds };
    }
}
