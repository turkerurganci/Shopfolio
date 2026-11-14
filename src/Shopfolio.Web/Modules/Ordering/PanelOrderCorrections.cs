using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Forms;
using Shopfolio.Infrastructure.Operations;
using Shopfolio.Infrastructure.Validation;
using Shopfolio.Modules.Administration.Contracts;
using Shopfolio.Modules.AfterSales.Contracts;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Ordering.Contracts;

namespace Shopfolio.Modules.Ordering;

/// <summary>
/// The order screen's corrections (T530; 07 UC-146...UC-151): the panel's side of the existing writes. The address, the carrier, the dates and the e-mail are <see cref="OrderCorrections"/>' (T375, T386:
/// the lock, the version, the operation decision, the history rows, the notices and the trail rows); the status correction is the transition unit's (T319), and this class writes its trail row - the
/// corrected code and the reason, never a note's text (02 10.3.2; 09 9.2 item 4; D-884). What this class adds is the screen's: the reason of a refused value named for the message (04 9.9.36), the
/// moments beside the delivery order question (K-608), the item of an arrival record, and the status correction's version and operation gate in 09 8.3 item 3's order. Nothing is saved here.
/// </summary>
internal sealed class PanelOrderCorrections : IPanelOrderCorrections
{
    /// <summary>
    /// The panel form's statement of how the firm knows the address request is the owner's (02 10.4.2, 8.3.7): 07 UC-146 takes no confirmation field - the form opens with K-798's reminder (K-601, K-735)
    /// and its sending is the administrator's statement that the request came through the order's own channel. The value is never stored (06: not a field; D-894).
    /// </summary>
    internal const AddressRequestConfirmation FormAddressConfirmation = AddressRequestConfirmation.FromOrderEmail;

    /// <summary>
    /// The panel form's statement of the buyer's identity check (02 8.3.6): 07 UC-151 takes no confirmation field - the form opens with K-798's identity reminder (03 6.2.1.3) and its sending is the
    /// administrator's statement that the name, the shipping telephone, the items and the amount were checked. Never stored.
    /// </summary>
    internal static readonly GuestIdentityConfirmation FormIdentityConfirmation = new(Name: true, ShippingPhone: true, Items: true, Amount: true);

    private static readonly InvalidEmailAttribute EmailRule = new();

    private readonly AppDbContext _db;
    private readonly OrderLocks _locks;
    private readonly OrderOperations _operations;
    private readonly IOrderCorrections _corrections;
    private readonly IOrderTransitions _transitions;
    private readonly IAfterSalesModule _afterSales;
    private readonly IAuditTrail _trail;

    public PanelOrderCorrections(
        AppDbContext db,
        OrderLocks locks,
        OrderOperations operations,
        IOrderCorrections corrections,
        IOrderTransitions transitions,
        IAfterSalesModule afterSales,
        IAuditTrail trail)
    {
        _db = db;
        _locks = locks;
        _operations = operations;
        _corrections = corrections;
        _transitions = transitions;
        _afterSales = afterSales;
        _trail = trail;
    }

    public async Task<PanelCorrectionChoices?> ReadChoicesAsync(Guid orderId, CancellationToken ct)
    {
        Order? order = await _db.Orders.AsNoTracking().SingleOrDefaultAsync(o => o.Id == orderId, ct);
        if (order is null)
        {
            return null;
        }

        List<OrderItem> items = await _db.OrderItems.AsNoTracking().Where(i => i.OrderId == orderId).ToListAsync(ct);
        List<TransitionRecord> records = await _db.TransitionRecords.AsNoTracking().Where(t => t.OrderId == orderId).ToListAsync(ct);
        IReadOnlyList<TransitionRecord> correctable = CorrectableTransitions.Find(order, records, items.Any(i => i.ProductType == ProductType.Digital));
        IReadOnlyList<ArrivalRecordDate> arrivals = items.Count == 0 ? [] : await _afterSales.ArrivalRecordDatesAsync([.. items.Select(i => i.Id)], ct);
        PanelAddressValues? shipping = order.ShippingRecipientName is null
            ? null
            : new PanelAddressValues(order.ShippingRecipientName, order.ShippingProvinceId, order.ShippingDistrict ?? string.Empty, order.ShippingAddressLine ?? string.Empty, order.ShippingPhone);
        PanelAddressValues? billing = order.BillingRecipientName is null
            ? null
            : new PanelAddressValues(order.BillingRecipientName, order.BillingProvinceId, order.BillingDistrict ?? string.Empty, order.BillingAddressLine ?? string.Empty, null);
        return new PanelCorrectionChoices(
            shipping,
            billing,
            [.. correctable.Select(BackOf)],
            [.. arrivals.OrderBy(a => a.ArrivalDate).Select(a => new PanelArrivalRecord(a.RecordId, a.OrderItemId, a.ArrivalDate))]);
    }

    public async Task<PanelCorrectionResult> CorrectAddressAsync(PanelAddressCorrection request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        OrderCorrectionResult result = await _corrections.CorrectAddressAsync(
            new CorrectAddressRequest(
                request.OrderId, request.Admin, request.AddressType, request.RecipientName, request.ProvinceId, request.District, request.AddressLine, request.Phone, FormAddressConfirmation,
                request.ExpectedVersion),
            ct);
        return result.Refusal switch
        {
            null => PanelCorrectionResult.Done,
            TransitionRefusal.InvalidInput when result.Fields.Count > 0 => new PanelCorrectionResult(TransitionRefusal.InvalidInput) { Problem = CorrectionValueProblem.Invalid, Fields = result.Fields },
            TransitionRefusal.InvalidInput => new PanelCorrectionResult(TransitionRefusal.InvalidInput) { Problem = CorrectionValueProblem.Unchanged },
            { } refusal => new PanelCorrectionResult(refusal),
        };
    }

    public async Task<PanelCorrectionResult> CorrectCarrierAsync(CorrectShipmentRequest request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);

        // The carrier data is whole before the lock, by the shipment's own rule (07 UC-147: ShipmentInfoMissing; 04 7.2.14.2): the same rule as the shipment's form (K-799).
        if (request.Shipment is null || !OrderTransitions.TryNormalizeShipment(request.Shipment, out _))
        {
            return new PanelCorrectionResult(TransitionRefusal.InvalidInput) { Problem = CorrectionValueProblem.Missing };
        }

        OrderCorrectionResult result = await _corrections.CorrectShipmentAsync(request, ct);
        return result.Refusal switch
        {
            null => PanelCorrectionResult.Done,
            TransitionRefusal.InvalidInput => new PanelCorrectionResult(TransitionRefusal.InvalidInput) { Problem = CorrectionValueProblem.Unchanged },
            { } refusal => new PanelCorrectionResult(refusal),
        };
    }

    public async Task<PanelCorrectionResult> CorrectDeliveryDateAsync(CorrectDeliveryDateRequest request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        OrderCorrectionResult result = await _corrections.CorrectDeliveryDateAsync(request, ct);
        if (result.Refusal is not { } refusal)
        {
            return PanelCorrectionResult.Done;
        }

        if (result.AsksDeliveryOrder)
        {
            // The moments of the declarations on that day, beside the question (04 9.9.17): read in the transaction the runner rolls back.
            Guid[] physical = await _db.OrderItems.AsNoTracking()
                .Where(i => i.OrderId == request.OrderId && i.ProductType == ProductType.Physical)
                .Select(i => i.Id)
                .ToArrayAsync(ct);
            IReadOnlyList<WithdrawalDeclarationDay> days = await _afterSales.WithdrawalDeclarationDaysAsync(physical, ct);
            return new PanelCorrectionResult(refusal)
            {
                AsksDeliveryOrder = true,
                DeclarationMoments = [.. days.Where(d => d.DeclarationDate == request.DeliveryDate).Select(d => d.DeclaredAt).Order()],
            };
        }

        return refusal switch
        {
            TransitionRefusal.InvalidInput when result.DateObstacle is { } obstacle => new PanelCorrectionResult(refusal) { Problem = CorrectionValueProblem.Invalid, DateObstacle = obstacle },
            TransitionRefusal.InvalidInput => new PanelCorrectionResult(refusal) { Problem = CorrectionValueProblem.Unchanged },
            _ => new PanelCorrectionResult(refusal),
        };
    }

    public async Task<PanelCorrectionResult> CorrectArrivalDateAsync(PanelArrivalDateCorrection request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        Guid[] itemIds = await _db.OrderItems.AsNoTracking().Where(i => i.OrderId == request.OrderId).Select(i => i.Id).ToArrayAsync(ct);
        if (itemIds.Length == 0)
        {
            return new PanelCorrectionResult(TransitionRefusal.OrderNotFound);
        }

        // The record is one of the order's own receipts or rejections; any other id is no choice of the form (07 UC-149: the record's key).
        ArrivalRecordDate? record = (await _afterSales.ArrivalRecordDatesAsync(itemIds, ct)).FirstOrDefault(a => a.RecordId == request.ArrivalRecordId);
        if (record is null)
        {
            return new PanelCorrectionResult(TransitionRefusal.ConditionNotMet);
        }

        OrderCorrectionResult result = await _corrections.CorrectReturnArrivalDateAsync(
            new CorrectReturnArrivalDateRequest(request.OrderId, record.OrderItemId, record.RecordId, request.Admin, request.ArrivalDate, request.ExpectedVersion),
            ct);
        return result.Refusal switch
        {
            null => PanelCorrectionResult.Done,
            TransitionRefusal.InvalidInput when record.ArrivalDate == request.ArrivalDate => new PanelCorrectionResult(TransitionRefusal.InvalidInput) { Problem = CorrectionValueProblem.Unchanged },

            // K-554: a future date or one before the item's delivery date; the two have one message (04 7.2.8.3).
            TransitionRefusal.InvalidInput => new PanelCorrectionResult(TransitionRefusal.InvalidInput) { Problem = CorrectionValueProblem.Invalid },
            { } refusal => new PanelCorrectionResult(refusal),
        };
    }

    public async Task<PanelCorrectionResult> CorrectTransitionAsync(CorrectTransitionRequest request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);

        // The reason is from the closed list and "diğer" carries its note (04 7.2.14.3): the form is not sent without them, before the lock (04 9.9.36).
        if (request.Reason is not { } reason || !Enum.IsDefined(reason) || (reason == CorrectionReason.Other && string.IsNullOrWhiteSpace(request.ReasonNote)))
        {
            return new PanelCorrectionResult(TransitionRefusal.InvalidInput) { Problem = CorrectionValueProblem.Missing };
        }

        // 09 8.3 item 3: the order's row lock, the version the screen showed (409 before the rule), then the operation's rule - the decision the GET drew the form from.
        await _locks.LockAsync([request.OrderId], ct);
        Order? order = await _db.Orders.AsNoTracking().SingleOrDefaultAsync(o => o.Id == request.OrderId, ct);
        if (order is null)
        {
            return new PanelCorrectionResult(TransitionRefusal.OrderNotFound);
        }

        if (!ShownVersion.Matches(order, request.ExpectedVersion))
        {
            return new PanelCorrectionResult(TransitionRefusal.VersionConflict);
        }

        OperationVerdict verdict = await _operations.CheckAsync(request.OrderId, OrderOperation.CorrectTransition, null, null, ct);
        if (verdict.Decision != OperationDecision.Allowed)
        {
            return new PanelCorrectionResult(TransitionRefusal.ConditionNotMet);
        }

        TransitionCode? corrected = await _db.TransitionRecords.AsNoTracking()
            .Where(t => t.Id == request.TransitionRecordId && t.OrderId == request.OrderId)
            .Select(t => (TransitionCode?)t.TransitionCode)
            .SingleOrDefaultAsync(ct);
        string? note = reason == CorrectionReason.Other ? request.ReasonNote : null;
        OrderTransitionResult result = await _transitions.CorrectTransitionAsync(request with { ReasonNote = note }, ct);
        if (result.Refusal is { } refusal)
        {
            return refusal == TransitionRefusal.CorrectionNotPossible
                ? new PanelCorrectionResult(refusal) { Obstacle = result.Obstacle ?? CorrectionObstacle.NotCorrectable }
                : new PanelCorrectionResult(refusal);
        }

        // The trail names the corrected transition and the reason's code (02 10.3.2); the note is free text and stays on the transition record only (09 9.2 item 4).
        await _trail.WriteAsync(
            new AuditEntry(
                AuditOperation.CorrectTransition,
                nameof(Order),
                order.Id,
                new AuditDetails(
                    order.OrderNumber,
                    values: [new AuditValue("TransitionCode", corrected!.Value.ToString()), new AuditValue("Reason", reason.ToString())])), // non-null: the core applied the correction of this order's record
            ct);
        return PanelCorrectionResult.Done;
    }

    public async Task<PanelCorrectionResult> CorrectOrderEmailAsync(PanelEmailCorrection request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        string? email = string.IsNullOrWhiteSpace(request.NewEmail) ? null : request.NewEmail.Trim();
        if (email is null)
        {
            return new PanelCorrectionResult(TransitionRefusal.InvalidInput) { Problem = CorrectionValueProblem.Missing };
        }

        if (!EmailRule.Allows(email))
        {
            return new PanelCorrectionResult(TransitionRefusal.InvalidInput) { Problem = CorrectionValueProblem.Invalid };
        }

        OrderCorrectionResult result = await _corrections.CorrectGuestEmailAsync(new CorrectGuestEmailRequest(request.OrderId, request.Admin, email, FormIdentityConfirmation, request.ExpectedVersion), ct);
        return result.Refusal switch
        {
            null => PanelCorrectionResult.Done,
            TransitionRefusal.InvalidInput => new PanelCorrectionResult(TransitionRefusal.InvalidInput) { Problem = CorrectionValueProblem.Unchanged },
            { } refusal => new PanelCorrectionResult(refusal),
        };
    }

    // The state the correction goes back to is the record's (07 UC-150): the shipment row's origin, or the payment's before the transfer's "paid" mark.
    private static PanelCorrectableTransition BackOf(TransitionRecord record) =>
        TransitionTable.FindShipment(record.TransitionCode) is { } shipment
            ? new PanelCorrectableTransition(record.Id, record.TransitionCode, record.CreatedAt, shipment.From, null)
            : new PanelCorrectableTransition(record.Id, record.TransitionCode, record.CreatedAt, null, TransitionTable.FindPayment(record.TransitionCode)?.From);
}
