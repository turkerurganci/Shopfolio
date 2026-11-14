using System.Globalization;

using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Forms;
using Shopfolio.Modules.Administration.Contracts;
using Shopfolio.Modules.AfterSales.Contracts;
using Shopfolio.Modules.Notifications.Contracts;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Modules.Payment.Contracts;

namespace Shopfolio.Modules.Ordering;

/// <summary>
/// The panel's refund steps (T529; 07 UC-131...UC-137; 09 8.3 item 3): under the order's row lock the version the screen showed is compared first (409; 07 9.9), then the operation's one rule - the
/// <see cref="OrderOperationCatalog"/> row the GET drew the button from, and for the card refund's two steps <see cref="OrderOperationCatalog.CheckCardRefundStep"/> (D-657, D-660) - decides (422);
/// only then the Payment module and the transition unit write, with the audit row of the step (02 10.3.1 (3); 09 9.2; D-898). The row carries the order number, codes, ids and amounts only, never an IBAN
/// (09 9.2 item 4). The refund's amount is <see cref="FirmRefundAmounts"/>'s - the screen's line and the POST read the same function. Joins the caller's transaction; nothing is saved here.
/// </summary>
internal sealed class PanelRefundSteps : IPanelRefundSteps
{
    private readonly AppDbContext _db;
    private readonly OrderOperations _operations;
    private readonly OrderLocks _locks;
    private readonly IOrderTransitions _transitions;
    private readonly IAfterSalesModule _afterSales;
    private readonly IPaymentModule _payment;
    private readonly INotificationsModule _notifications;
    private readonly IAuditTrail _trail;
    private readonly TimeProvider _time;

    public PanelRefundSteps(
        AppDbContext db,
        OrderOperations operations,
        OrderLocks locks,
        IOrderTransitions transitions,
        IAfterSalesModule afterSales,
        IPaymentModule payment,
        INotificationsModule notifications,
        IAuditTrail trail,
        TimeProvider time)
    {
        _db = db;
        _operations = operations;
        _locks = locks;
        _transitions = transitions;
        _afterSales = afterSales;
        _payment = payment;
        _notifications = notifications;
        _trail = trail;
        _time = time;
    }

    public async Task<IReadOnlyList<PanelRefundLine>> ReadLinesAsync(Guid orderId, CancellationToken ct)
    {
        Order? order = await _db.Orders.AsNoTracking().SingleOrDefaultAsync(o => o.Id == orderId, ct);
        OrderOperationFacts? facts = order is null ? null : await _operations.ReadFactsAsync(orderId, ct);
        return order is null || facts is null ? [] : await LinesAsync(order, facts, ct);
    }

    public async Task<PanelRefundStepResult> ProcessRefundAsync(PanelRefundRequest request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        (Order? Order, OrderOperationFacts? Facts, PanelRefundStepResult? Refused) gate = await GateAsync(request.OrderId, request.ExpectedVersion, ct);
        if (gate is not ({ } order, { } facts, _))
        {
            return gate.Refused ?? new PanelRefundStepResult(TransitionRefusal.OrderNotFound);
        }

        Lines lines = await ReadAsync(order, ct);
        PanelRefundFacts? born = lines.BornOf(request.Key);
        OwedRefundRecord? owed = born is null ? lines.Owed.FirstOrDefault(o => o.CauseId == request.Key) : null;
        if (born is null && owed is null)
        {
            return new PanelRefundStepResult(TransitionRefusal.OrderNotFound);
        }

        PaymentMethod way = born?.Way ?? Way(order);
        // A born refund may have no item (the late payment's, an amount based one; 04 9.9.21): its own item, never the owed record's, then its own IBAN.
        Guid? itemId = born is not null ? born.OrderItemId : owed!.OrderItemId; // non-null: without a born refund the owed record was found above
        string? iban = way == PaymentMethod.BankTransfer ? lines.IbanOf(born, owed) : null;

        // The system's own card refund and a refund that was sent are not the firm's step (02 7.2.8); the transfer line without an IBAN asks for one first (K-686).
        if (born is not null && (born.IsSent || born.Way == PaymentMethod.Card))
        {
            return new PanelRefundStepResult(TransitionRefusal.ConditionNotMet);
        }

        if (way == PaymentMethod.BankTransfer && iban is null)
        {
            return new PanelRefundStepResult(TransitionRefusal.ConditionNotMet) { Obstacle = PanelRefundObstacle.IbanMissing };
        }

        if (!OrderOperationCatalog.Check(facts, OrderOperation.ProcessRefund, null, null).IsAllowed)
        {
            return new PanelRefundStepResult(TransitionRefusal.ConditionNotMet);
        }

        (decimal amount, bool fee) = born is not null ? (born.Amount, born.IsShippingFeeIncluded) : await FirmRefundAmounts.OfAsync(_db, _afterSales, order, owed!, lines.Refunds, ct); // non-null: without a born refund the owed record was found above
        if (!request.Confirmed)
        {
            return new PanelRefundStepResult(TransitionRefusal.ConfirmationRequired) { Amount = amount, Iban = iban };
        }

        Guid refundId;
        if (born is not null)
        {
            // A born transfer refund - a defect remedy's, a late payment's, a card refund the customer's IBAN turned - is sent by the axis's one path (T317).
            PaymentTransitionResult sent = await _transitions.ProcessTransferRefundAsync(
                new TransferRefundProcessing(order.Id, born.RefundId, request.Admin.AdminId, request.Admin.AdminName, request.ExpectedVersion, true), ct);
            if (RefusalOf(sent) is { } refusal)
            {
                return new PanelRefundStepResult(refusal);
            }

            refundId = born.RefundId;
        }
        else
        {
            RefundFacts started = await _payment.StartFirmRefundAsync(
                new FirmRefundStart(
                    order.Id, order.PaymentMethod, owed!.Type, amount, fee, owed.OrderItemId, owed.Quantity, owed.RecordId, owed.ReturnId, null, request.Admin.AdminId, request.Admin.AdminName), // non-null: without a born refund the owed record was found above
                ct);
            if (await SentAsync(order, started, request.Admin, ct) is { } refusal)
            {
                return new PanelRefundStepResult(refusal);
            }

            refundId = started.RefundId;
        }

        await WriteAsync(AuditOperation.RefundManually, order.Id, RefundValues(refundId, itemId, amount), ct);
        return PanelRefundStepResult.Done;
    }

    public async Task<PanelRefundStepResult> MarkBankTransferRefundFailedAsync(PanelRefundRequest request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        (Order? Order, OrderOperationFacts? Facts, PanelRefundStepResult? Refused) gate = await GateAsync(request.OrderId, request.ExpectedVersion, ct);
        if (gate is not ({ } order, { } facts, _))
        {
            return gate.Refused ?? new PanelRefundStepResult(TransitionRefusal.OrderNotFound);
        }

        Lines lines = await ReadAsync(order, ct);
        PanelRefundFacts? born = lines.BornOf(request.Key);
        OwedRefundRecord? owed = born is null ? lines.Owed.FirstOrDefault(o => o.CauseId == request.Key) : null;
        if (born is null && owed is null)
        {
            return new PanelRefundStepResult(TransitionRefusal.OrderNotFound);
        }

        // A processed transfer's return is outside the product (03 8.3.3.8): the step is open only before the refund was sent, and only on the transfer - the born
        // refund's own way (a card refund turned to the transfer on a card order; D-1563), else the order's.
        if ((born is not null && born.IsSent) || (born?.Way ?? Way(order)) != PaymentMethod.BankTransfer
            || !OrderOperationCatalog.Check(facts, OrderOperation.ReportTransferRefundFailed, null, null).IsAllowed)
        {
            return new PanelRefundStepResult(TransitionRefusal.ConditionNotMet);
        }

        // The line draws the step only with the IBAN the refund goes to: without one there is no transfer to fail (09 8.3 item 5).
        Guid? itemId = born?.OrderItemId ?? owed?.OrderItemId;
        if (lines.IbanOf(born, owed) is null)
        {
            return new PanelRefundStepResult(TransitionRefusal.ConditionNotMet);
        }

        // The line's own subject: only the IBAN this refund is paid to goes (the one rule; the line read it above).
        OrderTransitionResult result = await _transitions.ReportTransferFailedAsync(
            new ReportTransferFailedRequest(order.Id, itemId, itemId is null ? born?.RefundId : null, request.Admin, request.ExpectedVersion, born is not null ? Lines.SubjectOf(born) : Lines.SubjectOf(owed!)), ct); // non-null: a born refund or an owed record was found above
        if (result.Refusal is { } refusal)
        {
            return new PanelRefundStepResult(refusal);
        }

        await WriteAsync(AuditOperation.ReportRefundTransferNotCompleted, order.Id, RefundValues(born?.RefundId, itemId, null), ct);
        return PanelRefundStepResult.Done;
    }

    public async Task<PanelRefundStepResult> ProcessAmountBasedRefundAsync(AmountBasedRefundRequest request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        (Order? Order, OrderOperationFacts? Facts, PanelRefundStepResult? Refused) gate = await GateAsync(request.OrderId, request.ExpectedVersion, ct);
        if (gate is not ({ } order, { } facts, _))
        {
            return gate.Refused ?? new PanelRefundStepResult(TransitionRefusal.OrderNotFound);
        }

        if (request.OrderItemId is { } item && !await _db.OrderItems.AsNoTracking().AnyAsync(i => i.Id == item && i.OrderId == order.Id, ct))
        {
            return new PanelRefundStepResult(TransitionRefusal.InvalidInput) { Obstacle = PanelRefundObstacle.ItemNotOfOrder };
        }

        // 04 7.2.13.2 (K-1105): on an item with an open "mal dönmedi" closure the closure is chosen too and the refund is bound to it - the chosen one must be the item's own open closure; with none
        // chosen the item's latest open closure binds it, so no refund on such an item stays unbound.
        IReadOnlyList<OpenNotReturnedClosure> open = request.OrderItemId is { } closed ? await _afterSales.ListOpenNotReturnedClosuresAsync([closed], ct) : [];
        if (request.NotReturnedClosureId is { } chosen && !open.Any(c => c.ClosureId == chosen))
        {
            return new PanelRefundStepResult(TransitionRefusal.InvalidInput) { Obstacle = PanelRefundObstacle.ClosureNotOpen };
        }

        Guid? closure = request.NotReturnedClosureId ?? (open.Count > 0 ? open[0].ClosureId : null);
        Lines lines = await ReadAsync(order, ct);
        PaymentMethod way = Way(order);
        // The refund to be born has no record: its item's IBAN, else the order's on file (the one rule; 02 7.4.5).
        string? iban = way == PaymentMethod.BankTransfer
            ? (await _afterSales.ResolveRefundPayeesAsync(order.Id, [new RefundPayeeSubject(null, null, request.OrderItemId)], ct))[0].Iban
            : null;
        if (way == PaymentMethod.BankTransfer && iban is null)
        {
            return new PanelRefundStepResult(TransitionRefusal.ConditionNotMet) { Obstacle = PanelRefundObstacle.IbanMissing };
        }

        if (!OrderOperationCatalog.Check(facts, OrderOperation.ProcessRefund, null, null).IsAllowed)
        {
            return new PanelRefundStepResult(TransitionRefusal.ConditionNotMet);
        }

        // K-1078: the paid money not yet given back, under the order's row lock - the sent and the started refunds, and the records whose refund is owed but not born yet (D-906), are deducted.
        decimal owedTotal = 0m;
        foreach (OwedRefundRecord owed in lines.Owed)
        {
            owedTotal += (await FirmRefundAmounts.OfAsync(_db, _afterSales, order, owed, lines.Refunds, ct)).Amount;
        }

        decimal left = order.Total - await _payment.CommittedRefundTotalAsync(order.Id, ct) - owedTotal;
        if (request.Amount <= 0m || request.Amount > left)
        {
            return new PanelRefundStepResult(TransitionRefusal.InvalidInput) { Obstacle = PanelRefundObstacle.AmountExceeded };
        }

        if (!request.Confirmed)
        {
            return new PanelRefundStepResult(TransitionRefusal.ConfirmationRequired) { Amount = request.Amount, Iban = iban };
        }

        // The closure chosen above binds the refund (K-1105); the closed item stays closed (K-596) - the refund writes no item record.
        RefundFacts started = await _payment.StartFirmRefundAsync(
            new FirmRefundStart(
                order.Id, order.PaymentMethod, RefundType.AmountBased, request.Amount, false, request.OrderItemId, null, null, null, closure, request.Admin.AdminId, request.Admin.AdminName),
            ct);
        if (await SentAsync(order, started, request.Admin, ct) is { } refusal)
        {
            return new PanelRefundStepResult(refusal);
        }

        await WriteAsync(AuditOperation.RefundManually, order.Id, RefundValues(started.RefundId, request.OrderItemId, request.Amount), ct);
        return PanelRefundStepResult.Done;
    }

    public async Task<PanelRefundStepResult> RequestRefundIbanAsync(PanelRefundRequest request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (request.Key == Guid.Empty)
        {
            return new PanelRefundStepResult(TransitionRefusal.InvalidInput) { Obstacle = PanelRefundObstacle.ItemNotSelected };
        }

        (Order? Order, OrderOperationFacts? Facts, PanelRefundStepResult? Refused) gate = await GateAsync(request.OrderId, request.ExpectedVersion, ct);
        if (gate is not ({ } order, { } facts, _))
        {
            return gate.Refused ?? new PanelRefundStepResult(TransitionRefusal.OrderNotFound);
        }

        // Tracked, under the order lock: the version guard tells the new request's order from its item (D-95; 09 10.2 item 3).
        if (await _db.OrderItems.SingleOrDefaultAsync(i => i.Id == request.Key && i.OrderId == order.Id, ct) is null)
        {
            return new PanelRefundStepResult(TransitionRefusal.InvalidInput) { Obstacle = PanelRefundObstacle.ItemNotSelected };
        }

        if (!OrderOperationCatalog.Check(facts, OrderOperation.RequestRefundIban, null, null).IsAllowed)
        {
            return new PanelRefundStepResult(TransitionRefusal.ConditionNotMet);
        }

        // The reason is the server's (K-1201): the firm never types it, and never the IBAN (02 10.1.2).
        RefundIbanRequestOpened opened = await _afterSales.OpenRefundIbanRequestAsync(
            new RefundIbanRequestEntry(request.Key, null, RefundIbanRequestReason.AdminRequest, request.Admin.AdminId, request.Admin.AdminName, _time.GetUtcNow()), ct);
        if (opened.IsNew && order.ContactEmail is { Length: > 0 } email)
        {
            await _notifications.QueueCustomerNoticeAsync(new RefundIbanRequestedNotice(order.Id, RefundIbanRequestReason.AdminRequest), email, ct);
        }

        OrderTransitions.RaiseVersionOnce(_db, order);
        await WriteAsync(
            AuditOperation.OpenRefundIbanRequest,
            order.Id,
            [new AuditValue("ItemId", request.Key.ToString("D")), new AuditValue("Reason", nameof(RefundIbanRequestReason.AdminRequest))],
            ct);
        return PanelRefundStepResult.Done;
    }

    public async Task<PanelRefundStepResult> RetryCardRefundAsync(PanelRefundRequest request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        (Order? Order, OrderOperationFacts? Facts, PanelRefundStepResult? Refused) gate = await GateAsync(request.OrderId, request.ExpectedVersion, ct);
        if (gate is not ({ } order, { } facts, _))
        {
            return gate.Refused ?? new PanelRefundStepResult(TransitionRefusal.OrderNotFound);
        }

        PanelRefundFacts? refund = (await _payment.ListPanelRefundsAsync(order.Id, ct)).FirstOrDefault(r => r.RefundId == request.Key);
        if (refund is null)
        {
            return new PanelRefundStepResult(TransitionRefusal.OrderNotFound);
        }

        if (!OrderOperationCatalog.CheckCardRefundStep(facts, CardRefundOperationStep.RetryOrRedirect).IsAllowed)
        {
            return new PanelRefundStepResult(TransitionRefusal.ConditionNotMet);
        }

        if (!request.Confirmed)
        {
            return new PanelRefundStepResult(TransitionRefusal.ConfirmationRequired) { Amount = refund.Amount };
        }

        OrderTransitionResult result = await _transitions.RetryCardRefundAsync(new RetryCardRefundRequest(order.Id, refund.RefundId, request.Admin, request.ExpectedVersion), ct);
        if (result.Refusal is { } refusal)
        {
            return new PanelRefundStepResult(refusal);
        }

        await WriteAsync(AuditOperation.RetryCardRefund, order.Id, RefundValues(refund.RefundId, refund.OrderItemId, refund.Amount), ct);
        return PanelRefundStepResult.Done;
    }

    public async Task<PanelRefundStepResult> SwitchRefundToBankTransferAsync(PanelRefundRequest request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        (Order? Order, OrderOperationFacts? Facts, PanelRefundStepResult? Refused) gate = await GateAsync(request.OrderId, request.ExpectedVersion, ct);
        if (gate is not ({ } order, { } facts, _))
        {
            return gate.Refused ?? new PanelRefundStepResult(TransitionRefusal.OrderNotFound);
        }

        PanelRefundFacts? refund = (await _payment.ListPanelRefundsAsync(order.Id, ct)).FirstOrDefault(r => r.RefundId == request.Key);
        if (refund is null)
        {
            return new PanelRefundStepResult(TransitionRefusal.OrderNotFound);
        }

        if (!OrderOperationCatalog.CheckCardRefundStep(facts, CardRefundOperationStep.RetryOrRedirect).IsAllowed)
        {
            return new PanelRefundStepResult(TransitionRefusal.ConditionNotMet);
        }

        OrderTransitionResult result = await _transitions.OpenRefundTransferPathAsync(new OpenRefundTransferPathRequest(order.Id, refund.RefundId, request.Admin, request.ExpectedVersion), ct);
        if (result.Refusal is { } refusal)
        {
            return new PanelRefundStepResult(refusal);
        }

        await WriteAsync(
            AuditOperation.RefundByBankTransfer,
            order.Id,
            [.. RefundValues(refund.RefundId, refund.OrderItemId, null), new AuditValue("Reason", nameof(RefundIbanRequestReason.CardRefundFailed))],
            ct);
        return PanelRefundStepResult.Done;
    }

    public async Task<PanelRefundStepResult> MarkCardRefundFailedAsync(PanelRefundRequest request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        (Order? Order, OrderOperationFacts? Facts, PanelRefundStepResult? Refused) gate = await GateAsync(request.OrderId, request.ExpectedVersion, ct);
        if (gate is not ({ } order, { } facts, _))
        {
            return gate.Refused ?? new PanelRefundStepResult(TransitionRefusal.OrderNotFound);
        }

        IReadOnlySet<Guid> orders = await _payment.OrderRefundIdsAsync(order.Id, [request.Key], ct);
        if (!orders.Contains(request.Key))
        {
            return new PanelRefundStepResult(TransitionRefusal.OrderNotFound);
        }

        // The mark is open only while the result is awaited (K-940); no notice goes to the customer (02 7.2.8).
        if (!OrderOperationCatalog.CheckCardRefundStep(facts, CardRefundOperationStep.MarkNotRealized).IsAllowed
            || !await _payment.MarkCardRefundFailedByAdminAsync(order.Id, request.Key, ct))
        {
            return new PanelRefundStepResult(TransitionRefusal.ConditionNotMet);
        }

        OrderTransitions.RaiseVersionOnce(_db, order);
        await WriteAsync(AuditOperation.MarkCardRefundNotCompleted, order.Id, RefundValues(request.Key, null, null), ct);
        return PanelRefundStepResult.Done;
    }

    /// <summary>
    /// The re-check of 09 8.3 item 3 in its order: the order's row lock and the version the screen showed (a change is the conflict and the rule is not asked); then the facts the operation's rule reads,
    /// after the lock. The order is loaded tracked: a step that writes raises its version once.
    /// </summary>
    private async Task<(Order? Order, OrderOperationFacts? Facts, PanelRefundStepResult? Refused)> GateAsync(Guid orderId, int expectedVersion, CancellationToken ct)
    {
        await _locks.LockAsync([orderId], ct);
        Order? order = await _db.Orders.SingleOrDefaultAsync(o => o.Id == orderId, ct);
        if (order is null)
        {
            return (null, null, new PanelRefundStepResult(TransitionRefusal.OrderNotFound));
        }

        if (!ShownVersion.Matches(order, expectedVersion))
        {
            return (null, null, new PanelRefundStepResult(TransitionRefusal.VersionConflict));
        }

        OrderOperationFacts? facts = await _operations.ReadFactsAsync(orderId, ct);
        return facts is null ? (null, null, new PanelRefundStepResult(TransitionRefusal.OrderNotFound)) : (order, facts, null);
    }

    // The transfer refund was sent with the step: the axis moves by its one path. The card refund's job went: the version rises once (the provider's result moves the axis later).
    private async Task<TransitionRefusal?> SentAsync(Order order, RefundFacts started, TransitionAdmin admin, CancellationToken ct)
    {
        if (!started.IsSent)
        {
            OrderTransitions.RaiseVersionOnce(_db, order);
            return null;
        }

        return RefusalOf(await _transitions.ApplyFirmRefundSentAsync(new FirmRefundSent(order.Id, started, admin), ct));
    }

    private async Task<List<PanelRefundLine>> LinesAsync(Order order, OrderOperationFacts facts, CancellationToken ct)
    {
        Lines lines = await ReadAsync(order, ct);
        bool processOpen = OrderOperationCatalog.Check(facts, OrderOperation.ProcessRefund, null, null).IsAllowed;
        bool reportOpen = OrderOperationCatalog.Check(facts, OrderOperation.ReportTransferRefundFailed, null, null).IsAllowed;
        bool retryOpen = OrderOperationCatalog.IsCardRefundStepOpen(facts, CardRefundOperationStep.RetryOrRedirect);
        bool markOpen = OrderOperationCatalog.IsCardRefundStepOpen(facts, CardRefundOperationStep.MarkNotRealized);
        PaymentMethod orderWay = Way(order);
        List<PanelRefundLine> result = [];
        foreach (OwedRefundRecord owed in lines.Owed)
        {
            (decimal amount, _) = await FirmRefundAmounts.OfAsync(_db, _afterSales, order, owed, lines.Refunds, ct);
            string? iban = orderWay == PaymentMethod.BankTransfer ? lines.IbanOf(null, owed) : null;
            result.Add(new PanelRefundLine(owed.CauseId, null, owed.OrderItemId, owed.Type, owed.Quantity, amount, orderWay, iban, owed.RecordedAt)
            {
                KeyName = KeyNameOf(owed),
                CanProcess = processOpen && (orderWay == PaymentMethod.Card || iban is not null),
                CanReportTransferFailed = reportOpen && iban is not null,
            });
        }

        foreach (PanelRefundFacts refund in lines.Refunds.Where(r => !r.IsVoided && (!r.IsSent || r.LateSuccessAt is not null)))
        {
            string? iban = refund.Way == PaymentMethod.BankTransfer ? lines.IbanOf(refund, null) : null;
            bool open = !refund.IsSent;
            bool ibanEntered = open && refund is { Way: PaymentMethod.Card, LatestState: RefundRecordState.Failed } && await _afterSales.IsRefundIbanEnteredForRefundAsync(refund.RefundId, ct);
            result.Add(new PanelRefundLine(KeyOf(refund), refund.RefundId, refund.OrderItemId, refund.RefundType, refund.Quantity, refund.Amount, refund.Way, iban, refund.CreatedAt)
            {
                KeyName = KeyNameOf(refund),
                CanProcess = open && processOpen && refund.Way == PaymentMethod.BankTransfer && iban is not null,
                CanReportTransferFailed = open && reportOpen && refund.Way == PaymentMethod.BankTransfer && iban is not null,
                CanRetryOrSwitch = open && retryOpen && refund is { Way: PaymentMethod.Card, LatestState: RefundRecordState.Failed } && !ibanEntered,
                CanMarkFailed = open && markOpen && refund is { Way: PaymentMethod.Card, LatestState: RefundRecordState.OutcomePending },
                HasLateSuccess = refund.LateSuccessAt is not null,
                IsSent = refund.IsSent,
            });
        }

        return result;
    }

    private async Task<Lines> ReadAsync(Order order, CancellationToken ct)
    {
        Guid[] itemIds = await _db.OrderItems.AsNoTracking().Where(i => i.OrderId == order.Id).Select(i => i.Id).ToArrayAsync(ct);
        IReadOnlyList<OwedRefundRecord> owed = await _afterSales.ListOwedRefundRecordsAsync(itemIds, ct);
        IReadOnlyList<PanelRefundFacts> refunds = await _payment.ListPanelRefundsAsync(order.Id, ct);
        RefundPayeeSubject[] subjects = [.. owed.Select(Lines.SubjectOf), .. refunds.Where(r => r.Way == PaymentMethod.BankTransfer).Select(Lines.SubjectOf)];
        return new Lines(owed, refunds, await _afterSales.ResolveRefundPayeesAsync(order.Id, subjects, ct));
    }

    // The key a born refund's line carries (07 UC-131): the record that causes it, the refund itself when it has none (an amount based refund).
    private static Guid KeyOf(PanelRefundFacts refund) =>
        refund.ReturnId ?? refund.DefectClaimId ?? refund.PaymentRecordId ?? refund.CancellationId ?? refund.ItemRemovalId ?? refund.LateDeliveryTerminationId ?? refund.WithdrawalId ?? refund.RefundId;

    // The request field that carries the key (07 UC-131: exactly one of the seven links), in the order of KeyOf.
    private static string KeyNameOf(PanelRefundFacts refund) => refund switch
    {
        { ReturnId: not null } => PanelRefundKeys.ReturnId,
        { DefectClaimId: not null } => PanelRefundKeys.DefectClaimId,
        { PaymentRecordId: not null } => PanelRefundKeys.PaymentRecordId,
        { CancellationId: not null } => PanelRefundKeys.CancellationId,
        { ItemRemovalId: not null } => PanelRefundKeys.ItemRemovalId,
        { LateDeliveryTerminationId: not null } => PanelRefundKeys.LateDeliveryTerminationId,
        { WithdrawalId: not null } => PanelRefundKeys.WithdrawalId,
        _ => PanelRefundKeys.RefundId,
    };

    private static string KeyNameOf(OwedRefundRecord owed) => owed switch
    {
        { ReturnId: not null } => PanelRefundKeys.ReturnId,
        { Type: RefundType.Cancellation } => PanelRefundKeys.CancellationId,
        { Type: RefundType.ItemRemoval } => PanelRefundKeys.ItemRemovalId,
        { Type: RefundType.LateDeliveryTermination } => PanelRefundKeys.LateDeliveryTerminationId,
        _ => PanelRefundKeys.WithdrawalId,
    };

    private static PaymentMethod Way(Order order) => order.PaymentMethod == PaymentMethod.Card ? PaymentMethod.Card : PaymentMethod.BankTransfer;

    private static TransitionRefusal? RefusalOf(PaymentTransitionResult result) => result switch
    {
        { IsOrderNotFound: true } => TransitionRefusal.OrderNotFound,
        { IsVersionConflict: true } => TransitionRefusal.VersionConflict,
        { IsConfirmationRequired: true } => TransitionRefusal.ConfirmationRequired,
        { IsNotOpen: true } => TransitionRefusal.ConditionNotMet,
        _ => null,
    };

    private static List<AuditValue> RefundValues(Guid? refundId, Guid? itemId, decimal? amount)
    {
        List<AuditValue> values = [];
        if (refundId is { } refund)
        {
            values.Add(new AuditValue("RefundId", refund.ToString("D")));
        }

        if (itemId is { } item)
        {
            values.Add(new AuditValue("ItemId", item.ToString("D")));
        }

        if (amount is { } money)
        {
            values.Add(new AuditValue("Amount", money.ToString("0.00", CultureInfo.InvariantCulture)));
        }

        return values;
    }

    private async Task WriteAsync(AuditOperation operation, Guid orderId, IReadOnlyList<AuditValue> values, CancellationToken ct)
    {
        string number = await _db.Orders.AsNoTracking().Where(o => o.Id == orderId).Select(o => o.OrderNumber).SingleAsync(ct);
        await _trail.WriteAsync(new AuditEntry(operation, nameof(Order), orderId, new AuditDetails(number, values: values)), ct);
    }

    /// <summary>What a refund step reads of the order's refunds under the lock: the records whose refund is owed, the refund rows and the IBAN each is paid to.</summary>
    private sealed record Lines(IReadOnlyList<OwedRefundRecord> Owed, IReadOnlyList<PanelRefundFacts> Refunds, IReadOnlyList<RefundPayee> Payees)
    {
        /// <summary>The refund row the key names - by one of its cause links or by its own id -, an open one first.</summary>
        public PanelRefundFacts? BornOf(Guid key) =>
            Refunds.Where(r => !r.IsVoided && (r.RefundId == key || r.IsCausedBy(key))).OrderBy(r => r.IsSent).FirstOrDefault();

        /// <summary>
        /// The IBAN a transfer refund goes to (04 9.9.6; OB-14; 02 7.4.5): the After-sales module's one rule (<see cref="IAfterSalesModule.ResolveRefundPayeesAsync"/>), the one the
        /// IBAN's life reads too; none while the refund waits for its IBAN. The line, the window and the steps read it here: the born refund's, else the owed record's.
        /// </summary>
        public string? IbanOf(PanelRefundFacts? born, OwedRefundRecord? owed)
        {
            RefundPayeeSubject subject = born is not null ? SubjectOf(born) : SubjectOf(owed!); // non-null: a step names a born refund or an owed record
            return Payees.FirstOrDefault(p => p.Subject == subject)?.Iban;
        }

        public static RefundPayeeSubject SubjectOf(PanelRefundFacts refund) =>
            RefundPayeeSubject.Of(refund.RefundId, refund.CancellationId, refund.LateDeliveryTerminationId, refund.WithdrawalId, refund.OrderItemId);

        public static RefundPayeeSubject SubjectOf(OwedRefundRecord owed) => new(null, owed.RecordId, owed.OrderItemId);
    }
}
