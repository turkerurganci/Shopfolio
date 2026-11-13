using System.Globalization;

using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Forms;
using Shopfolio.Modules.Administration.Contracts;
using Shopfolio.Modules.AfterSales.Contracts;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Ordering.Contracts;

namespace Shopfolio.Modules.Ordering;

/// <summary>
/// The panel's six line steps (T527; 07 UC-123...UC-128; 09 8.3 item 3): under the order's row lock the version the screen showed is compared first (409; 07 9.9), then the operation's one rule - the
/// <see cref="OrderOperationCatalog"/> instance the GET drew the button from, read over the same facts (<see cref="OrderOperations"/>) - decides (422); only an open operation reaches the transition
/// unit, which writes. This class adds the audit row of each step (02 10.3.1 (3); 09 9.2) and the delivery mark's order question on a withdrawal declaration's day (K-608) - the same rule as the delivery
/// date's correction (<see cref="OrderCorrections"/>). The row carries the order number and codes only, never the customer's data (09 9.2 item 4). Joins the caller's transaction; nothing is saved here.
/// </summary>
internal sealed class PanelOrderSteps : IPanelOrderSteps
{
    private readonly AppDbContext _db;
    private readonly OrderOperations _operations;
    private readonly OrderLocks _locks;
    private readonly IOrderTransitions _transitions;
    private readonly IAfterSalesModule _afterSales;
    private readonly IAuditTrail _trail;

    public PanelOrderSteps(AppDbContext db, OrderOperations operations, OrderLocks locks, IOrderTransitions transitions, IAfterSalesModule afterSales, IAuditTrail trail)
    {
        _db = db;
        _operations = operations;
        _locks = locks;
        _transitions = transitions;
        _afterSales = afterSales;
        _trail = trail;
    }

    public async Task<PanelStepResult> MarkPaidAsync(Guid orderId, TransitionAdmin admin, int expectedVersion, bool confirmed, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(admin);
        if (await GateAsync(orderId, expectedVersion, OrderOperation.MarkTransferPaid, ct) is { } refused)
        {
            return refused;
        }

        if (!confirmed)
        {
            return new PanelStepResult(TransitionRefusal.ConfirmationRequired);
        }

        PaymentTransitionResult result = await _transitions.MarkTransferPaidAsync(new TransferPaidMark(orderId, admin.AdminId, admin.AdminName, expectedVersion, confirmed), ct);
        if (RefusalOf(result) is { } refusal)
        {
            return new PanelStepResult(refusal);
        }

        await WriteAsync(AuditOperation.MarkBankTransferPaid, orderId, [new AuditValue("TransitionCode", TransitionCode.O1.ToString())], ct);
        return PanelStepResult.Done;
    }

    public Task<PanelStepResult> MarkShippedAsync(ShipOrderRequest request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        return SendAsync(request, false, ct);
    }

    public Task<PanelStepResult> MarkReshippedAsync(ShipOrderRequest request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        return SendAsync(request, true, ct);
    }

    public async Task<PanelStepResult> MarkDeliveredAsync(MarkDeliveredRequest request, bool? deliveredBeforeDeclaration, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (await GateAsync(request.OrderId, request.ExpectedVersion, OrderOperation.MarkDelivered, ct) is { } refused)
        {
            return refused;
        }

        OrderTransitionResult result = await _transitions.MarkDeliveredAsync(request, ct);
        if (result.Refusal is { } refusal)
        {
            return new PanelStepResult(refusal) { DateObstacle = result.DateObstacle };
        }

        // The physical items of the order: a declaration made on the delivery date asks which came first (K-608; 02 7.4.1). The transition is rolled back with the refusal when there is no answer.
        Guid[] physical = await _db.OrderItems.AsNoTracking()
            .Where(i => i.OrderId == request.OrderId && i.ProductType == ProductType.Physical)
            .Select(i => i.Id)
            .ToArrayAsync(ct);
        List<WithdrawalDeclarationDay> sameDay = [.. (await _afterSales.WithdrawalDeclarationDaysAsync(physical, ct)).Where(w => w.DeclarationDate == request.DeliveryDate)];
        if (sameDay.Count > 0 && deliveredBeforeDeclaration is null)
        {
            return new PanelStepResult(TransitionRefusal.InvalidInput) { AsksDeliveryOrder = true, DeclarationMoments = [.. sameDay.Select(w => w.DeclaredAt).Order()] };
        }

        await WriteAsync(AuditOperation.MarkDelivered, request.OrderId, [new AuditValue("TransitionCode", TransitionCode.S6.ToString()), new AuditValue("DeliveryDate", DateText(request.DeliveryDate))], ct);
        if (sameDay.Count > 0)
        {
            bool before = deliveredBeforeDeclaration!.Value; // non-null: the check above refuses a same-day date without the answer
            await _afterSales.SetDeliveryOrderAsync([.. sameDay.Select(w => w.WithdrawalId)], before, ct);
            await WriteAsync(
                AuditOperation.SetDeliveryOrderAgainstWithdrawal,
                request.OrderId,
                [new AuditValue("DeliveryDate", DateText(request.DeliveryDate)), new AuditValue("Reason", before ? "DeliveredBeforeDeclaration" : "DeliveredAfterDeclaration")],
                ct);
        }

        return PanelStepResult.Done;
    }

    public async Task<PanelStepResult> MarkDeliveryFailedAsync(AdminOrderRequest request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (await GateAsync(request.OrderId, request.ExpectedVersion, OrderOperation.MarkDeliveryFailed, ct) is { } refused)
        {
            return refused;
        }

        OrderTransitionResult result = await _transitions.MarkDeliveryFailedAsync(request, ct);
        if (result.Refusal is { } refusal)
        {
            return new PanelStepResult(refusal);
        }

        await WriteAsync(AuditOperation.MarkDeliveryFailed, request.OrderId, CodesOf(TransitionCode.S7, result.ClosureCode), ct);
        return PanelStepResult.Done;
    }

    public async Task<PanelStepResult> CloseDeliveryFailedAsync(AdminOrderRequest request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (await GateAsync(request.OrderId, request.ExpectedVersion, OrderOperation.CloseOrder, ct) is { } refused)
        {
            return refused;
        }

        OrderTransitionResult result = await _transitions.CloseReturnedOrderAsync(request, ct);
        if (result.Refusal is { } refusal)
        {
            return new PanelStepResult(refusal);
        }

        // 09 9.2 item 3 has no code of its own for the closing of a returned order: it is the returned shipment's line, so its row is MarkDeliveryFailed with the closure's S9 (D-1383).
        await WriteAsync(AuditOperation.MarkDeliveryFailed, request.OrderId, [new AuditValue("TransitionCode", TransitionCode.S9.ToString())], ct);
        return PanelStepResult.Done;
    }

    private async Task<PanelStepResult> SendAsync(ShipOrderRequest request, bool isResending, CancellationToken ct)
    {
        // The carrier data is whole before the lock, by the transition unit's own rule (K-799; 07 UC-124: ShipmentInfoMissing), so the unconfirmed window never offers a value the write refuses.
        if (request.Shipment is null || !OrderTransitions.TryNormalizeShipment(request.Shipment, out _))
        {
            return new PanelStepResult(TransitionRefusal.InvalidInput);
        }

        if (await GateAsync(request.OrderId, request.ExpectedVersion, isResending ? OrderOperation.ResendOrder : OrderOperation.ShipOrder, ct) is { } refused)
        {
            return refused;
        }

        if (!request.Confirmed)
        {
            return new PanelStepResult(TransitionRefusal.ConfirmationRequired);
        }

        OrderTransitionResult result = isResending ? await _transitions.ResendOrderAsync(request, ct) : await _transitions.ShipOrderAsync(request, ct);
        if (result.Refusal is { } refusal)
        {
            return new PanelStepResult(refusal);
        }

        await WriteAsync(AuditOperation.MarkShipped, request.OrderId, [new AuditValue("TransitionCode", (isResending ? TransitionCode.S8 : TransitionCode.S5).ToString())], ct);
        return PanelStepResult.Done;
    }

    /// <summary>
    /// The re-check of 09 8.3 item 3 in its order: the order's row lock, the version the screen showed (<see cref="ShownVersion.Matches"/>; a change is the conflict and the rule is not asked), then the
    /// operation's rule over the facts read after the lock - the decision the GET drew the button from (<see cref="OrderOperationCatalog.Check"/>). Null: the step may go on.
    /// </summary>
    private async Task<PanelStepResult?> GateAsync(Guid orderId, int expectedVersion, OrderOperation operation, CancellationToken ct)
    {
        await _locks.LockAsync([orderId], ct);
        Order? order = await _db.Orders.AsNoTracking().SingleOrDefaultAsync(o => o.Id == orderId, ct);
        if (order is null)
        {
            return new PanelStepResult(TransitionRefusal.OrderNotFound);
        }

        if (!ShownVersion.Matches(order, expectedVersion))
        {
            return new PanelStepResult(TransitionRefusal.VersionConflict);
        }

        OrderOperationFacts? facts = await _operations.ReadFactsAsync(orderId, ct);
        return facts is not null && OrderOperationCatalog.Check(facts, operation, null, null).IsAllowed ? null : new PanelStepResult(TransitionRefusal.ConditionNotMet);
    }

    private static TransitionRefusal? RefusalOf(PaymentTransitionResult result) => result switch
    {
        { IsOrderNotFound: true } => TransitionRefusal.OrderNotFound,
        { IsVersionConflict: true } => TransitionRefusal.VersionConflict,
        { IsConfirmationRequired: true } => TransitionRefusal.ConfirmationRequired,
        { IsNotOpen: true } => TransitionRefusal.ConditionNotMet,
        _ => null,
    };

    private static IReadOnlyList<AuditValue> CodesOf(TransitionCode code, TransitionCode? closure) =>
        closure is { } closed
            ? [new AuditValue("TransitionCode", code.ToString()), new AuditValue("TransitionCode", closed.ToString())]
            : [new AuditValue("TransitionCode", code.ToString())];

    private async Task WriteAsync(AuditOperation operation, Guid orderId, IReadOnlyList<AuditValue> values, CancellationToken ct)
    {
        string number = await _db.Orders.AsNoTracking().Where(o => o.Id == orderId).Select(o => o.OrderNumber).SingleAsync(ct);
        await _trail.WriteAsync(new AuditEntry(operation, nameof(Order), orderId, new AuditDetails(number, values: values)), ct);
    }

    private static string DateText(DateOnly date) => date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
}
