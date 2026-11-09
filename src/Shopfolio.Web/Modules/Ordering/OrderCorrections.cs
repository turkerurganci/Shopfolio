using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Validation;
using Shopfolio.Modules.Administration.Contracts;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Membership.Contracts;
using Shopfolio.Modules.Notifications.Contracts;
using Shopfolio.Modules.Ordering.Contracts;

namespace Shopfolio.Modules.Ordering;

/// <summary>
/// The administrator's corrections of an order's contact data (T375; 02 7.6.6, 10.4.2, 10.4.11, 8.3.6, 8.3.7): the address and a guest order's e-mail. Both are forward records and write no
/// amount, item, status or period. They work in the transition unit's order (09 9.1 item 4): the order's row lock, the screen's version, the operation decision of 03 1.11 on the locked order,
/// the confirmation the product asks, the change with its history row, the order's version raised once, the customer notices and the trail row - all in the caller's transaction. The
/// buyer has no way in: no method here is reached from the order page (02 8.3.6, 8.3.7). The product does not check the firm's confirmation; it asks that it was given (the proof is the
/// firm's, 02 12.5). No amount, quantity or price is read or written anywhere in this file, and nothing here adds an item (02 7.6.6).
/// </summary>
internal sealed class OrderCorrections : IOrderCorrections
{
    private static readonly InvalidEmailAttribute EmailRule = new();

    private readonly AppDbContext _db;
    private readonly OrderLocks _locks;
    private readonly OrderOperations _operations;
    private readonly IMembershipModule _membership;
    private readonly INotificationsModule _notifications;
    private readonly IOrderContactNotices _contactNotices;
    private readonly IAuditTrail _trail;
    private readonly TimeProvider _time;

    public OrderCorrections(
        AppDbContext db,
        OrderLocks locks,
        OrderOperations operations,
        IMembershipModule membership,
        INotificationsModule notifications,
        IOrderContactNotices contactNotices,
        IAuditTrail trail,
        TimeProvider time)
    {
        _db = db;
        _locks = locks;
        _operations = operations;
        _membership = membership;
        _notifications = notifications;
        _contactNotices = contactNotices;
        _trail = trail;
        _time = time;
    }

    public async Task<OrderCorrectionResult> CorrectAddressAsync(CorrectAddressRequest request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (!IsAdmin(request.Admin))
        {
            return Refused(TransitionRefusal.ActorNotAllowed);
        }

        if (!Enum.IsDefined(request.AddressType) || !Enum.IsDefined(request.Confirmation))
        {
            return Refused(TransitionRefusal.InvalidInput);
        }

        // The request is the owner's, and the firm knows it (02 8.3.7; K-601): a request from another channel that the firm did not confirm through the order's own channels changes nothing.
        if (request.Confirmation == AddressRequestConfirmation.None)
        {
            return Refused(TransitionRefusal.ConfirmationRequired);
        }

        (Order? order, TransitionRefusal? locked) = await LockAsync(request.OrderId, request.ExpectedVersion, ct);
        if (locked is { } lockRefusal)
        {
            return Refused(lockRefusal);
        }

        // 1.11.23 on the locked order: the shipping address until "Kargoya verildi", the billing address by the same line - or, with no physical item, until "Teslim edildi" (02 10.4.2).
        OperationVerdict verdict = await _operations.CheckAsync(order!.Id, OrderOperation.CorrectAddress, null, null, ct);
        if (verdict.Decision != OperationDecision.Allowed)
        {
            return Refused(TransitionRefusal.ConditionNotMet);
        }

        bool hasPhysical = await _db.OrderItems.AsNoTracking().AnyAsync(i => i.OrderId == order.Id && i.ProductType == ProductType.Physical, ct);
        bool shipping = request.AddressType == AddressType.ShippingAddress;
        if ((shipping && !hasPhysical) || (request.Confirmation == AddressRequestConfirmation.ConfirmedByCallToOrderPhone && !hasPhysical))
        {
            // An order with no physical item has no shipping address and no shipping telephone to call.
            return Refused(TransitionRefusal.ConditionNotMet);
        }

        // The fields are the address rules' (one home, 02 3.14.6): the billing address is checked alone; a shipping address is checked with the order's own billing address, which stays.
        OrderAddressResolution resolution = await _membership.ResolveOrderAddressesAsync(
            shipping
                ? new OrderAddressRequest(
                    null, [ProductType.Physical], null, request.RecipientName, request.ProvinceId, request.District, request.AddressLine, request.Phone, false, null, false,
                    order.BillingRecipientName, order.BillingProvinceId, order.BillingDistrict, order.BillingAddressLine)
                : new OrderAddressRequest(
                    null, [ProductType.Digital], null, null, null, null, null, null, false, null, false,
                    request.RecipientName, request.ProvinceId, request.District, request.AddressLine),
            ct);
        if (!resolution.IsResolved)
        {
            return new OrderCorrectionResult(TransitionRefusal.InvalidInput, resolution.Fields, null);
        }

        (string RecipientName, string? Phone, Guid ProvinceId, string District, string AddressLine) oldValue = shipping
            ? (order.ShippingRecipientName ?? string.Empty, order.ShippingPhone, order.ShippingProvinceId ?? Guid.Empty, order.ShippingDistrict ?? string.Empty, order.ShippingAddressLine ?? string.Empty)
            : (order.BillingRecipientName ?? string.Empty, null, order.BillingProvinceId ?? Guid.Empty, order.BillingDistrict ?? string.Empty, order.BillingAddressLine ?? string.Empty);
        (string RecipientName, string? Phone, Guid ProvinceId, string District, string AddressLine) newValue = shipping
            ? (resolution.Shipping!.RecipientName, resolution.Shipping.Phone, resolution.Shipping.ProvinceId, resolution.Shipping.District, resolution.Shipping.AddressLine)
            : (resolution.Billing!.RecipientName, null, resolution.Billing.ProvinceId, resolution.Billing.District, resolution.Billing.AddressLine);
        if (oldValue == newValue)
        {
            // Nothing to correct: a history row of two equal values would only be noise.
            return Refused(TransitionRefusal.InvalidInput);
        }

        DateTimeOffset now = _time.GetUtcNow();
        if (shipping)
        {
            order.ShippingRecipientName = newValue.RecipientName;
            order.ShippingPhone = newValue.Phone;
            order.ShippingProvinceId = newValue.ProvinceId;
            order.ShippingDistrict = newValue.District;
            order.ShippingAddressLine = newValue.AddressLine;
        }
        else
        {
            order.BillingRecipientName = newValue.RecipientName;
            order.BillingProvinceId = newValue.ProvinceId;
            order.BillingDistrict = newValue.District;
            order.BillingAddressLine = newValue.AddressLine;
        }

        // The old and the new value stay on the order's own record, with the administrator and the moment (02 10.4.2; K-1008); the trail's row names no value.
        OrderAddressHistory history = OrderAddressHistory.Create(order.Id, request.AddressType, oldValue, newValue, request.Admin!.AdminId, request.Admin.AdminName, now);
        _db.OrderAddressHistories.Add(history);
        OrderTransitions.RaiseVersionOnce(_db, order);

        // B-10 goes to the order's e-mail with the new address; the old address is not a value (08 2.2.6 B-10).
        await _notifications.QueueCustomerNoticeAsync(
            new AddressCorrectedNotice(order.Id, request.AddressType, new NoticeAddress(newValue.RecipientName, newValue.AddressLine, newValue.District, newValue.ProvinceId, newValue.Phone)),
            order.ContactEmail!,
            ct);
        await _trail.WriteAsync(
            new AuditEntry(AuditOperation.CorrectAddress, nameof(Order), order.Id, new AuditDetails(order.OrderNumber, values: [new AuditValue("Field", request.AddressType.ToString())])),
            ct);
        return new OrderCorrectionResult(null, [], history.Id);
    }

    public async Task<OrderCorrectionResult> CorrectGuestEmailAsync(CorrectGuestEmailRequest request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (!IsAdmin(request.Admin))
        {
            return Refused(TransitionRefusal.ActorNotAllowed);
        }

        string? newEmail = string.IsNullOrWhiteSpace(request.NewEmail) ? null : request.NewEmail.Trim();
        if (newEmail is null || !EmailRule.Allows(newEmail))
        {
            return Refused(TransitionRefusal.InvalidInput);
        }

        // The firm confirms the buyer before anything is written; the order number alone is no confirmation (02 8.3.6; 3.22.5).
        if (request.Confirmation is not { } confirmation || !confirmation.Name || !confirmation.Items || !confirmation.Amount)
        {
            return Refused(TransitionRefusal.ConfirmationRequired);
        }

        (Order? order, TransitionRefusal? locked) = await LockAsync(request.OrderId, request.ExpectedVersion, ct);
        if (locked is { } lockRefusal)
        {
            return Refused(lockRefusal);
        }

        // 1.11.35 on the locked order: a guest order whose personal data is not disposed - an account's order has the account's verified e-mail, which is not corrected (02 10.4.11).
        OperationVerdict verdict = await _operations.CheckAsync(order!.Id, OrderOperation.CorrectGuestEmail, null, null, ct);
        if (verdict.Decision != OperationDecision.Allowed)
        {
            return Refused(TransitionRefusal.ConditionNotMet);
        }

        // The shipping telephone is a confirmation of an order that has a physical item - it has no other (02 8.3.6).
        bool hasPhysical = await _db.OrderItems.AsNoTracking().AnyAsync(i => i.OrderId == order.Id && i.ProductType == ProductType.Physical, ct);
        if (hasPhysical && !confirmation.ShippingPhone)
        {
            return Refused(TransitionRefusal.ConfirmationRequired);
        }

        string oldEmail = order.ContactEmail!;
        if (TextNormalizer.NormalizeIdentity(newEmail) == order.NormalizedContactEmail)
        {
            return Refused(TransitionRefusal.InvalidInput);
        }

        DateTimeOffset now = _time.GetUtcNow();
        order.SetContactEmail(newEmail);

        // The old key opens nothing from here on: the links of the old e-mail and the number with the old address are dead (02 10.4.11 (2)).
        order.RenewAccessKey();

        // The account link follows the new address: a link the old address made is cut; a verified account of the new one takes the order at once; an order whose account was deleted is never linked again (02 10.4.11 (5); K-1116).
        order.CustomerId = null;
        if (!order.IsAccountDeleted)
        {
            OrderAccountResolution account = await _membership.ResolveOrderAccountAsync(null, newEmail, ct);
            if (account.Kind == OrderAccountKind.GuestBoundToAccount)
            {
                order.CustomerId = account.CustomerId;
            }
        }

        OrderEmailHistory history = OrderEmailHistory.Create(order.Id, oldEmail, newEmail, request.Admin!.AdminId, request.Admin.AdminName, now);
        _db.OrderEmailHistories.Add(history);
        OrderTransitions.RaiseVersionOnce(_db, order);

        // The notices that were not sent yet go to the new address (K-1138); then B-1 to the new address - the first e-mail's chain, with the new key's link - and B-15 to the old one.
        await _contactNotices.RedirectUnsentCustomerNoticesAsync(order.Id, newEmail, ct);
        Guid? root = await _contactNotices.FindFirstOrderReceivedAsync(order.Id, ct);
        await _notifications.QueueCustomerNoticeAsync(new OrderReceivedNotice(order.Id, root), newEmail, ct);
        await _notifications.QueueCustomerNoticeAsync(new OrderEmailChangedNotice(order.Id, now), oldEmail, ct);
        await _trail.WriteAsync(
            new AuditEntry(AuditOperation.CorrectContactEmail, nameof(Order), order.Id, new AuditDetails(order.OrderNumber, values: [new AuditValue("Field", "ContactEmail")])),
            ct);
        return new OrderCorrectionResult(null, [], history.Id) { LinkedCustomerId = order.CustomerId };
    }

    private async Task<(Order? Order, TransitionRefusal? Refusal)> LockAsync(Guid orderId, int? expectedVersion, CancellationToken ct)
    {
        await _locks.LockAsync([orderId], ct);
        Order? order = await _db.Orders.SingleOrDefaultAsync(o => o.Id == orderId, ct);
        if (order is null)
        {
            return (null, TransitionRefusal.OrderNotFound);
        }

        return expectedVersion is { } shown && shown != order.Version ? (null, TransitionRefusal.VersionConflict) : (order, null);
    }

    private static bool IsAdmin(TransitionAdmin? admin) => admin is not null && admin.AdminId != Guid.Empty && !string.IsNullOrWhiteSpace(admin.AdminName);

    private static OrderCorrectionResult Refused(TransitionRefusal refusal) => new(refusal, [], null);
}
