using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Modules.AfterSales.Contracts;
using Shopfolio.Modules.Notifications.Contracts;
using Shopfolio.Modules.Ordering.Contracts;

namespace Shopfolio.Modules.Ordering;

/// <summary>
/// The item record operations of the transition unit (T320; 03 1.7.1, 1.9.1; 02 5.8, 7.5): the defect claim's opening, its "resolved" mark and its reopening, and the
/// rejection of returned goods. Each works in the unit's order (09 9.1 item 4): the order's row lock, the screen's version, the operation's own condition, the
/// AfterSales writer (the records and the claim's status are that module's - 09 6 item 2; the claim's status is written only by its writer, reached from here), the
/// order's version raised once, then the notices. The order's statuses never change: a claim does not touch the shipment line and a remedy that needs money runs
/// through the payment axis' refund transitions (03 1.7.1, 1.9.1; 02 7.5.3).
/// </summary>
internal sealed partial class OrderTransitions
{
    public async Task<ItemRecordResult> OpenDefectClaimAsync(OpenDefectClaimRequest request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        string description = request.Description?.Trim() ?? string.Empty;
        if (description.Length == 0)
        {
            return ItemRefused(TransitionRefusal.InvalidInput);
        }

        (Order? order, OrderItem? item, TransitionRefusal? refusal) = await LockItemAsync(request.OrderId, request.OrderItemId, null, ct);
        if (refusal is { } locked)
        {
            return ItemRefused(locked);
        }

        DateTimeOffset now = _time.GetUtcNow();
        ItemRecordWriteResult written = await _afterSales.OpenDefectClaimAsync(new DefectClaimOpening(Subject(order!, item!), request.Quantity, description, now), ct);
        if (written.Refusal is { } refused)
        {
            return ItemRefused(Map(refused));
        }

        RaiseVersionOnce(order!);
        await QueueClaimNoticesAsync(order!, item!, written.Claim!, reopening: false, now, ct);
        return new ItemRecordResult(null, written.RecordId);
    }

    public async Task<ItemRecordResult> ResolveDefectClaimAsync(ResolveDefectClaimRequest request, CancellationToken ct)
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

        // The "resolved" mark goes to no one (03 1.9.1; 02 9.2).
        DefectClaimStatusChange change = new(Subject(order!, item!), DefectClaimStatus.Resolved, ActorKind.Admin, null, _time.GetUtcNow());
        ItemRecordWriteResult written = await _afterSales.ChangeDefectClaimStatusAsync(change, ct);
        if (written.Refusal is { } refused)
        {
            return ItemRefused(Map(refused));
        }

        RaiseVersionOnce(order!);
        return new ItemRecordResult(null, written.RecordId);
    }

    public async Task<ItemRecordResult> ReopenDefectClaimAsync(ReopenDefectClaimRequest request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        switch (request.Actor)
        {
            case ActorKind.Buyer when request.Admin is null && request.ExpectedVersion is null:
                break;
            case ActorKind.Admin when TryAdminActor(request.Admin, out _) && request.ExpectedVersion is not null && request.Quantity is null:
                break;
            default:
                return ItemRefused(TransitionRefusal.ActorNotAllowed);
        }

        (Order? order, OrderItem? item, TransitionRefusal? refusal) = await LockItemAsync(request.OrderId, request.OrderItemId, request.ExpectedVersion, ct);
        if (refusal is { } locked)
        {
            return ItemRefused(locked);
        }

        DateTimeOffset now = _time.GetUtcNow();
        DefectClaimStatusChange change = new(Subject(order!, item!), DefectClaimStatus.Open, request.Actor, request.Quantity, now);
        ItemRecordWriteResult written = await _afterSales.ChangeDefectClaimStatusAsync(change, ct);
        if (written.Refusal is { } refused)
        {
            return ItemRefused(Map(refused));
        }

        RaiseVersionOnce(order!);

        // The customer's reopening is told to both sides (K-681); the administrator's goes to no one (03 1.9.1; 02 9.2).
        if (request.Actor == ActorKind.Buyer)
        {
            await QueueClaimNoticesAsync(order!, item!, written.Claim!, reopening: true, now, ct);
        }

        return new ItemRecordResult(null, written.RecordId);
    }

    public async Task<ItemRecordResult> RejectReturnAsync(RejectReturnRequest request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (!TryAdminActor(request.Admin, out _))
        {
            return ItemRefused(TransitionRefusal.ActorNotAllowed);
        }

        if (!request.Confirmed)
        {
            return ItemRefused(TransitionRefusal.ConfirmationRequired);
        }

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

        ReturnRejectionEntry entry = new(
            Subject(order!, item!), request.WithdrawalId, request.Quantity, request.ArrivalDate, new ItemRecordAdmin(request.Admin.AdminId, request.Admin.AdminName.Trim()), now);
        ItemRecordWriteResult written = await _afterSales.AddReturnRejectionAsync(entry, ct);
        if (written.Refusal is { } refused)
        {
            return ItemRefused(Map(refused));
        }

        RaiseVersionOnce(order!);

        // B-16 names the item, the rejected units and the conditional exemption the rejection rests on (08 2.2.6 B-16).
        NoticeItem rejected = new(item!.ProductName, item.FirstOptionValue, item.SecondOptionValue, request.Quantity);
        await QueueNoticeAsync(order!.Id, new ReturnRejectedNotice(order.Id, rejected, item.WithdrawalExemptionReason!.Value), ct);
        return new ItemRecordResult(null, written.RecordId);
    }

    /// <summary>The order's row lock, the order, the screen's version and the order's item - in that order (05 7.3.2; 09 9.1 item 4).</summary>
    private async Task<(Order? Order, OrderItem? Item, TransitionRefusal? Refusal)> LockItemAsync(Guid orderId, Guid orderItemId, int? expectedVersion, CancellationToken ct)
    {
        await _locks.LockAsync([orderId], ct);
        Order? order = await _db.Orders.SingleOrDefaultAsync(o => o.Id == orderId, ct);
        if (order is null)
        {
            return (null, null, TransitionRefusal.OrderNotFound);
        }

        if (expectedVersion is { } shown && shown != order.Version)
        {
            return (null, null, TransitionRefusal.VersionConflict);
        }

        OrderItem? item = await _db.OrderItems.SingleOrDefaultAsync(i => i.Id == orderItemId && i.OrderId == orderId, ct);
        return item is null ? (null, null, TransitionRefusal.InvalidInput) : (order, item, null);
    }

    private static ItemRecordSubject Subject(Order order, OrderItem item) =>
        new(order.Id, item.Id, item.Quantity, item.ProductType, order.OrderStatus, item.DeliveryMarkedAt, item.DeliveryDate, item.WithdrawalExemptionReason);

    /// <summary>B-9 to the customer and F-3 to the firm (03 1.7.1.10, 1.9.1; 02 9.3.2): the claim's birth or the customer's reopening, with the claim's units and return address.</summary>
    private async Task QueueClaimNoticesAsync(Order order, OrderItem item, DefectClaimInfo claim, bool reopening, DateTimeOffset now, CancellationToken ct)
    {
        NoticeItem claimed = new(item.ProductName, item.FirstOptionValue, item.SecondOptionValue, claim.Quantity);
        NoticeAddress? address = claim.ReturnAddress is { } a ? new NoticeAddress(a.RecipientName, a.AddressLine, a.District, a.ProvinceId, a.Phone) : null;
        await QueueNoticeAsync(
            order.Id,
            new DeclarationReceivedNotice(order.Id, DispatchRelatedRecordType.DefectClaim, claim.Id, reopening, null, now, [claimed], address),
            ct);
        await _notifications.QueueCompanyNoticeAsync(new NewClaimNotice(DispatchRelatedRecordType.DefectClaim, claim.Id, order.Id, order.OrderNumber, reopening, now), ct);
    }

    /// <summary>The writer's refusal as the unit's (07 6.2.7): a quantity outside the open units is a wrong value; the rest are the operation's condition.</summary>
    private static TransitionRefusal Map(ItemRecordRefusal refusal) => refusal switch
    {
        ItemRecordRefusal.QuantityExceedsOpen or ItemRecordRefusal.ArrivalDateOutOfRange => TransitionRefusal.InvalidInput,
        ItemRecordRefusal.ActorNotAllowed => TransitionRefusal.ActorNotAllowed,
        _ => TransitionRefusal.ConditionNotMet,
    };

    private static ItemRecordResult ItemRefused(TransitionRefusal refusal) => new(refusal, null);
}
