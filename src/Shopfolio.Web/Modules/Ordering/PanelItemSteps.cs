using System.Globalization;

using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Forms;
using Shopfolio.Infrastructure.Operations;
using Shopfolio.Infrastructure.Validation;
using Shopfolio.Modules.Administration.Contracts;
using Shopfolio.Modules.AfterSales.Contracts;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Modules.Payment.Contracts;

namespace Shopfolio.Modules.Ordering;

/// <summary>
/// The panel's item steps (T528; 07 UC-129, UC-130, UC-138, UC-140...UC-142, UC-144, UC-145; 09 8.3 item 3): the field rules first (09 3.4), then under the order's row lock the version the screen
/// showed (409; 07 9.9), then the operation's one rule - <see cref="OrderOperationCatalog.Check"/> over the facts read after the lock, the decision the GET drew the button from - and only then the
/// transition unit, which writes the record, the counters, the notices and the IBAN request (the AfterSales module's one IBAN home; no second IBAN choice here). This class adds the confirmation
/// stop of the irreversible steps and the audit row (02 10.3.1 (3), 10.4.1; 09 9.2); the row carries codes, dates, counts and ids only (09 9.2 item 4). Joins the caller's transaction.
/// </summary>
internal sealed class PanelItemSteps : IPanelItemSteps
{
    private static readonly InvalidIbanAttribute NoticeIbanRule = new();

    private readonly AppDbContext _db;
    private readonly OrderOperations _operations;
    private readonly OrderLocks _locks;
    private readonly IOrderTransitions _transitions;
    private readonly IOrderCorrections _corrections;
    private readonly IAfterSalesModule _afterSales;
    private readonly IAuditTrail _trail;
    private readonly TimeProvider _time;

    public PanelItemSteps(
        AppDbContext db, OrderOperations operations, OrderLocks locks, IOrderTransitions transitions, IOrderCorrections corrections, IAfterSalesModule afterSales, IAuditTrail trail, TimeProvider time)
    {
        _db = db;
        _operations = operations;
        _locks = locks;
        _transitions = transitions;
        _corrections = corrections;
        _afterSales = afterSales;
        _trail = trail;
        _time = time;
    }

    public async Task<PanelItemStepResult> CancelByFirmAsync(PanelFirmCancellation request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);

        // 04 7.2.9.3: no reason, "Diğer" without its note, "müşteriyle anlaşıldı" without the owner's confirmation - no window opens.
        if (request.Reason is not { } reason || !FirmReasonRules.IsComplete(reason, request.ReasonNote, request.OwnerConfirmed))
        {
            return PanelItemStepResult.Refused(PanelItemObstacle.ReasonIncomplete);
        }

        (PanelItemStepResult? refused, OrderOperationFacts? facts) = await LockAsync(request.OrderId, request.ExpectedVersion, ct);
        if (refused is not null)
        {
            return refused;
        }

        bool isTransfer = facts!.PaymentMethod == PaymentMethod.BankTransfer; // non-null: LockAsync returns the facts when it refuses nothing
        if (facts.PaymentStatus == PaymentStatus.Pending)
        {
            // Before the payment the whole order (S3 + Ö2): the request carries no item (07 UC-129).
            if (request.Items.Count > 0 || !OrderOperationCatalog.Check(facts!, OrderOperation.CancelByFirm, null, null).IsAllowed)
            {
                return PanelItemStepResult.NotOpen;
            }

            if (!request.Confirmed)
            {
                return new PanelItemStepResult(TransitionRefusal.ConfirmationRequired) { IsTransferLine = isTransfer };
            }

            OrderTransitionResult whole = await _transitions.CancelOrderAsync(
                CancelOrderRequest.ByAdministrator(request.OrderId, request.Admin, request.ExpectedVersion, true, reason, request.ReasonNote, null, request.OwnerConfirmed), ct);
            if (whole.Refusal is { } wholeRefusal)
            {
                return new PanelItemStepResult(wholeRefusal);
            }

            await WriteAsync(AuditOperation.CancelByCompany, request.OrderId, [new AuditValue("Reason", reason.ToString()), .. Codes(whole)], ct);
            return PanelItemStepResult.Done;
        }

        // After the payment the chosen items' units, each checked by the one rule before anything is written: the request is applied whole or not at all (K-1188 (4)).
        if (request.Items.Count == 0)
        {
            return PanelItemStepResult.Refused(PanelItemObstacle.ItemNotSelected);
        }

        if (request.Items.GroupBy(i => i.OrderItemId).Any(g => g.Count() > 1))
        {
            return PanelItemStepResult.Refused(PanelItemObstacle.ItemSelectionInvalid);
        }

        if (request.Items.Any(i => i.Quantity < 1))
        {
            return PanelItemStepResult.Refused(PanelItemObstacle.QuantityInvalid);
        }

        foreach (PanelItemUnits units in request.Items)
        {
            if (VerdictObstacle(OrderOperationCatalog.Check(facts!, OrderOperation.CancelByFirm, units.OrderItemId, units.Quantity)) is { } obstacle)
            {
                return obstacle;
            }
        }

        if (!request.Confirmed)
        {
            return new PanelItemStepResult(TransitionRefusal.ConfirmationRequired) { IsTransferLine = isTransfer };
        }

        foreach (PanelItemUnits units in request.Items)
        {
            // Every cancellation raises the version once; the next one in the same request reads the version as it now stands - the screen's was compared under the lock above.
            int version = await CurrentVersionAsync(request.OrderId, ct);
            OrderTransitionResult result = await _transitions.CancelItemByFirmAsync(
                new CancelItemByFirmRequest(request.OrderId, units.OrderItemId, units.Quantity, request.Admin, version, true, reason, request.ReasonNote, request.OwnerConfirmed), ct);
            if (result.Refusal is { } refusal)
            {
                return new PanelItemStepResult(refusal) { Obstacle = refusal == TransitionRefusal.InvalidInput ? PanelItemObstacle.QuantityExceedsOpen : PanelItemObstacle.None };
            }

            await WriteAsync(
                AuditOperation.CancelByCompany,
                request.OrderId,
                [new AuditValue("ItemId", units.OrderItemId.ToString("D")), new AuditValue("Quantity", Count(units.Quantity)), new AuditValue("Reason", reason.ToString()), .. Codes(result)],
                ct);
        }

        return PanelItemStepResult.Done;
    }

    public async Task<PanelItemStepResult> RecordWithdrawalAsync(RecordWithdrawalRequest request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (request.OrderItemId == Guid.Empty)
        {
            return PanelItemStepResult.Refused(PanelItemObstacle.ItemNotSelected);
        }

        if (request.Quantity < 1)
        {
            return PanelItemStepResult.Refused(PanelItemObstacle.QuantityInvalid);
        }

        if (request.NoticeDate > TurkeyTime.DateOf(_time.GetUtcNow()))
        {
            return PanelItemStepResult.Refused(PanelItemObstacle.DateInvalid);
        }

        // OB-14: an IBAN given with a notice from the order's contact e-mail passes the order page's check; otherwise the record would drop it silently.
        if (request.CameFromOrderEmail && !string.IsNullOrWhiteSpace(request.NoticeIban) && !NoticeIbanRule.Allows(request.NoticeIban))
        {
            return PanelItemStepResult.Refused(PanelItemObstacle.IbanInvalid);
        }

        (PanelItemStepResult? refused, OrderOperationFacts? facts) = await LockAsync(request.OrderId, request.ExpectedVersion, ct);
        if (refused is not null)
        {
            return refused;
        }

        OperationItemFacts? item = facts!.Items.FirstOrDefault(i => i.ItemId == request.OrderItemId); // non-null: LockAsync returns the facts when it refuses nothing
        OperationVerdict verdict = OrderOperationCatalog.Check(facts!, OrderOperation.RecordWithdrawalFromOtherChannel, request.OrderItemId, request.Quantity);
        if (VerdictObstacle(verdict) is { } closed)
        {
            // K-604: a physical item not handed to the carrier is the "müşteriyle anlaşıldı" cancellation's - the form says so.
            return item is { ProductType: ProductType.Physical } && !facts.HasShipped && closed.Obstacle == PanelItemObstacle.None
                ? PanelItemStepResult.Refused(PanelItemObstacle.ItemNotShipped)
                : closed;
        }

        WithdrawalNoticeWarnings warnings = WithdrawalNoticeRules.Warnings(item!, request.NoticeDate); // non-null: an allowed verdict names one of the order's items
        if (warnings.AsksDeliveryOrder && request.IsDeliveredBeforeDeclaration is null)
        {
            return new PanelItemStepResult(TransitionRefusal.InvalidInput) { Obstacle = PanelItemObstacle.DeliveryOrderMissing, Warnings = warnings };
        }

        // 04 7.2.10.5: the separate box is required and unticked - the window does not open (422 before the confirmation state), the form says it.
        if (warnings.NeedsConfirmation && !request.WarningsConfirmed)
        {
            return new PanelItemStepResult(TransitionRefusal.InvalidInput) { Obstacle = PanelItemObstacle.WarningsUnconfirmed, Warnings = warnings };
        }

        if (!request.Confirmed)
        {
            return new PanelItemStepResult(TransitionRefusal.ConfirmationRequired) { Warnings = warnings };
        }

        WithdrawalResult result = await _transitions.RecordWithdrawalAsync(request, ct);
        if (result.Refusal is { } refusal)
        {
            return new PanelItemStepResult(refusal) { Warnings = warnings };
        }

        await WriteAsync(
            AuditOperation.RecordWithdrawalFromOtherChannel,
            request.OrderId,
            [new AuditValue("ItemId", request.OrderItemId.ToString("D")), new AuditValue("Quantity", Count(request.Quantity)), new AuditValue("Date", DateText(request.NoticeDate)), .. Closure(result.Closure)],
            ct);
        return PanelItemStepResult.Done;
    }

    public async Task<PanelItemStepResult> RecordLateDeliveryTerminationAsync(TerminateForLateDeliveryRequest request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (request.NoticeArrivalDate is not { } date || date > TurkeyTime.DateOf(_time.GetUtcNow()))
        {
            return PanelItemStepResult.Refused(PanelItemObstacle.DateInvalid);
        }

        (PanelItemStepResult? refused, OrderOperationFacts? facts) = await LockAsync(request.OrderId, request.ExpectedVersion ?? -1, ct);
        if (refused is not null)
        {
            return refused;
        }

        if (!OrderOperationCatalog.Check(facts!, OrderOperation.RecordLateTerminationFromOtherChannel, null, null).IsAllowed) // non-null: LockAsync returns the facts when it refuses nothing
        {
            return PanelItemStepResult.NotOpen;
        }

        if (!request.Confirmed)
        {
            return new PanelItemStepResult(TransitionRefusal.ConfirmationRequired);
        }

        OrderTransitionResult result = await _transitions.TerminateForLateDeliveryAsync(request, ct);
        if (result.Refusal is { } refusal)
        {
            return new PanelItemStepResult(refusal) { Obstacle = refusal == TransitionRefusal.InvalidInput ? PanelItemObstacle.DateInvalid : PanelItemObstacle.None };
        }

        await WriteAsync(AuditOperation.RecordLateDeliveryTerminationFromOtherChannel, request.OrderId, [new AuditValue("Date", DateText(date)), .. Codes(result)], ct);
        return PanelItemStepResult.Done;
    }

    public async Task<PanelItemStepResult> RemoveItemAsync(RemoveItemRequest request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (request.Quantity < 1)
        {
            return PanelItemStepResult.Refused(PanelItemObstacle.QuantityInvalid);
        }

        (PanelItemStepResult? refused, OrderOperationFacts? facts) = await LockAsync(request.OrderId, request.ExpectedVersion ?? -1, ct);
        if (refused is not null)
        {
            return refused;
        }

        if (VerdictObstacle(OrderOperationCatalog.Check(facts!, OrderOperation.RemoveOrReduceItem, request.OrderItemId, request.Quantity)) is { } closed) // non-null: LockAsync returns the facts
        {
            return closed;
        }

        // 03 1.6.1.5: only the removal that sends money to the card asks the confirmation; the transfer line has none (07 UC-138).
        bool isCard = facts!.PaymentMethod == PaymentMethod.Card;
        if (isCard && !request.Confirmed)
        {
            return new PanelItemStepResult(TransitionRefusal.ConfirmationRequired);
        }

        // The correction writes the record, the refund line and its own trail row (OrderCorrections; 02 10.4.1 (2)).
        OrderTransitionResult result = await _corrections.RemoveItemAsync(request with { Confirmed = isCard }, ct);
        return result.Refusal is { } refusal
            ? new PanelItemStepResult(refusal) { Obstacle = refusal == TransitionRefusal.InvalidInput ? PanelItemObstacle.QuantityExceedsOpen : PanelItemObstacle.None }
            : PanelItemStepResult.Done;
    }

    public Task<PanelItemStepResult> ReceiveReturnAsync(PanelReturnArrival request, CancellationToken ct) => ArriveAsync(request, false, ct);

    public Task<PanelItemStepResult> RejectReturnAsync(PanelReturnArrival request, CancellationToken ct) => ArriveAsync(request, true, ct);

    public async Task<PanelItemStepResult> RestockReturnAsync(Guid orderId, Guid returnId, TransitionAdmin admin, int expectedVersion, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(admin);
        (PanelItemStepResult? refused, _) = await LockAsync(orderId, expectedVersion, ct);
        if (refused is not null)
        {
            return refused;
        }

        Guid[] itemIds = await ItemIdsAsync(orderId, ct);
        ReturnReceiptFact? receipt = (await _afterSales.ReturnStepFactsAsync(itemIds, ct)).Receipts.FirstOrDefault(r => r.ReturnId == returnId);

        // 07 UC-141: a rejection, a receipt of another order, an added receipt - not open; a deleted variant is the transition's refusal.
        if (receipt is null || receipt.StockAddedAt is not null)
        {
            return PanelItemStepResult.NotOpen;
        }

        ItemRecordResult result = await _transitions.AddReturnedStockAsync(new AddReturnedStockRequest(orderId, receipt.OrderItemId, returnId, admin, expectedVersion), ct);

        // 02 10.3.1: the stock entry is not written to the trail.
        return result.Refusal is null ? PanelItemStepResult.Done : PanelItemStepResult.NotOpen;
    }

    public async Task<PanelItemStepResult> CloseNotReturnedAsync(Guid orderId, Guid orderItemId, int? quantity, TransitionAdmin admin, int expectedVersion, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(admin);
        if (quantity is < 1)
        {
            return PanelItemStepResult.Refused(PanelItemObstacle.QuantityInvalid);
        }

        (PanelItemStepResult? refused, OrderOperationFacts? facts) = await LockAsync(orderId, expectedVersion, ct);
        if (refused is not null)
        {
            return refused;
        }

        if (VerdictObstacle(OrderOperationCatalog.Check(facts!, OrderOperation.CloseAsNotReturned, orderItemId, quantity)) is { } closed) // non-null: LockAsync returns the facts
        {
            return closed;
        }

        // The declarations' units not come back, oldest first: the closure is written per declaration (06 4.28), the quantity spread over them in that order.
        IReadOnlyList<ReturnableWithdrawal> withdrawals = [.. (await _afterSales.ReturnStepFactsAsync([orderItemId], ct)).Withdrawals.Where(w => w.Awaiting > 0)];
        int left = quantity ?? withdrawals.Sum(w => w.Awaiting);
        foreach (ReturnableWithdrawal withdrawal in withdrawals)
        {
            if (left == 0)
            {
                break;
            }

            int units = Math.Min(left, withdrawal.Awaiting);
            ItemRecordResult result = await _transitions.CloseAsNotReturnedAsync(
                new CloseAsNotReturnedRequest(orderId, orderItemId, withdrawal.WithdrawalId, admin, await CurrentVersionAsync(orderId, ct), units), ct);
            if (result.Refusal is { } refusal)
            {
                return new PanelItemStepResult(refusal);
            }

            left -= units;
        }

        if (left > 0)
        {
            return PanelItemStepResult.Refused(PanelItemObstacle.QuantityExceedsOpen);
        }

        await WriteAsync(
            AuditOperation.CloseNotReturned, orderId, [new AuditValue("ItemId", orderItemId.ToString("D")), new AuditValue("Quantity", Count(quantity ?? withdrawals.Sum(w => w.Awaiting)))], ct);
        return PanelItemStepResult.Done;
    }

    public Task<PanelItemStepResult> ResolveDefectClaimAsync(Guid orderId, Guid defectClaimId, TransitionAdmin admin, int expectedVersion, CancellationToken ct) =>
        ClaimStepAsync(orderId, defectClaimId, admin, expectedVersion, DefectClaimStatus.Open, ct);

    public Task<PanelItemStepResult> ReopenDefectClaimAsync(Guid orderId, Guid defectClaimId, TransitionAdmin admin, int expectedVersion, CancellationToken ct) =>
        ClaimStepAsync(orderId, defectClaimId, admin, expectedVersion, DefectClaimStatus.Resolved, ct);

    // UC-144, UC-145: the claim of one of the order's items in the status the step starts from; the transition writes the status and its history (06 4.30). Z-18 is the reopening's
    // condition, asked by the same rule the button was drawn from. No notice (02 9.2) and no trail code of its own (09 9.2 item 3: the claim's status is not one of the 43).
    private async Task<PanelItemStepResult> ClaimStepAsync(Guid orderId, Guid defectClaimId, TransitionAdmin admin, int expectedVersion, DefectClaimStatus from, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(admin);
        (PanelItemStepResult? refused, OrderOperationFacts? facts) = await LockAsync(orderId, expectedVersion, ct);
        if (refused is not null)
        {
            return refused;
        }

        OrderDefectClaim? claim = (await _afterSales.ListDefectClaimsAsync(await ItemIdsAsync(orderId, ct), ct)).FirstOrDefault(c => c.Id == defectClaimId);
        if (claim is null || claim.Status != from || !OrderOperationCatalog.Check(facts!, OrderOperation.ResolveOrReopenDefectClaim, claim.OrderItemId, null).IsAllowed) // non-null: LockAsync returns the facts
        {
            return PanelItemStepResult.NotOpen;
        }

        ItemRecordResult result = from == DefectClaimStatus.Open
            ? await _transitions.ResolveDefectClaimAsync(new ResolveDefectClaimRequest(orderId, claim.OrderItemId, admin, expectedVersion), ct)
            : await _transitions.ReopenDefectClaimAsync(ReopenDefectClaimRequest.ByAdministrator(orderId, claim.OrderItemId, admin, expectedVersion), ct);
        return result.Refusal is { } refusal ? new PanelItemStepResult(refusal) : PanelItemStepResult.Done;
    }

    // UC-140: the receipt or the rejection on the item's oldest declaration with goods on the way back (K-794). The quantity defaults to that declaration's units still awaited.
    private async Task<PanelItemStepResult> ArriveAsync(PanelReturnArrival request, bool isRejection, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (request.Quantity is < 1)
        {
            return PanelItemStepResult.Refused(PanelItemObstacle.QuantityInvalid);
        }

        if (request.ArrivalDate > TurkeyTime.DateOf(_time.GetUtcNow()))
        {
            return PanelItemStepResult.Refused(PanelItemObstacle.DateInvalid);
        }

        (PanelItemStepResult? refused, OrderOperationFacts? facts) = await LockAsync(request.OrderId, request.ExpectedVersion, ct);
        if (refused is not null)
        {
            return refused;
        }

        IReadOnlyList<ReturnableWithdrawal> returnable = (await _afterSales.ReturnStepFactsAsync([request.OrderItemId], ct)).Withdrawals;
        ReturnableWithdrawal? withdrawal = returnable.Count > 0 ? returnable[0] : null;
        int quantity = request.Quantity ?? withdrawal?.Awaiting ?? 0;
        OrderOperation operation = isRejection ? OrderOperation.RejectReturnedGoods : OrderOperation.ReceiveReturnedGoods;
        if (withdrawal is null)
        {
            return PanelItemStepResult.NotOpen;
        }

        if (VerdictObstacle(OrderOperationCatalog.Check(facts!, operation, request.OrderItemId, quantity)) is { } closed) // non-null: LockAsync returns the facts
        {
            return closed;
        }

        if (quantity > withdrawal.Awaiting + withdrawal.NotReturned)
        {
            return PanelItemStepResult.Refused(PanelItemObstacle.QuantityExceedsOpen);
        }

        if (isRejection && !request.Confirmed)
        {
            return new PanelItemStepResult(TransitionRefusal.ConfirmationRequired);
        }

        ItemRecordResult result = isRejection
            ? await _transitions.RejectReturnAsync(
                new RejectReturnRequest(request.OrderId, request.OrderItemId, withdrawal.WithdrawalId, request.Admin, request.ExpectedVersion, true, quantity, request.ArrivalDate), ct)
            : await _transitions.ReceiveReturnAsync(
                new ReceiveReturnRequest(request.OrderId, request.OrderItemId, withdrawal.WithdrawalId, request.Admin, request.ExpectedVersion, quantity, request.ArrivalDate), ct);
        if (result.Refusal is { } refusal)
        {
            // The writer's date bound (before the item's delivery date) is the date's refusal; the rest is the step's.
            return new PanelItemStepResult(refusal) { Obstacle = refusal == TransitionRefusal.InvalidInput ? PanelItemObstacle.DateInvalid : PanelItemObstacle.None };
        }

        bool ibanRequested = !isRejection && await _db.Orders.AsNoTracking().AnyAsync(o => o.Id == request.OrderId && o.PaymentMethod == PaymentMethod.BankTransfer, ct)
            && (await _afterSales.ListAwaitingIbanLinesAsync(ct)).Any(l => l.OrderItemId == request.OrderItemId && l.Reason == RefundIbanRequestReason.ReturnAfterIbanDeleted);
        AuditOperation code = isRejection ? AuditOperation.RejectReturn : ibanRequested ? AuditOperation.ReceiveReturnWithIbanRequest : AuditOperation.ReceiveReturn;
        await WriteAsync(
            code,
            request.OrderId,
            [new AuditValue("ItemId", request.OrderItemId.ToString("D")), new AuditValue("Quantity", Count(quantity)), new AuditValue("ArrivalDate", DateText(request.ArrivalDate))],
            ct);
        return PanelItemStepResult.Done;
    }

    /// <summary>
    /// The re-check of 09 8.3 item 3 in its order: the order's row lock, the version the screen showed (<see cref="ShownVersion.Matches"/>; a change is the conflict and the rule is not asked),
    /// then the facts the operation's rule reads, after the lock.
    /// </summary>
    private async Task<(PanelItemStepResult? Refused, OrderOperationFacts? Facts)> LockAsync(Guid orderId, int expectedVersion, CancellationToken ct)
    {
        await _locks.LockAsync([orderId], ct);
        Order? order = await _db.Orders.AsNoTracking().SingleOrDefaultAsync(o => o.Id == orderId, ct);
        if (order is null)
        {
            return (new PanelItemStepResult(TransitionRefusal.OrderNotFound), null);
        }

        if (!ShownVersion.Matches(order, expectedVersion))
        {
            return (new PanelItemStepResult(TransitionRefusal.VersionConflict), null);
        }

        OrderOperationFacts? facts = await _operations.ReadFactsAsync(orderId, ct);
        return facts is null ? (new PanelItemStepResult(TransitionRefusal.OrderNotFound), null) : (null, facts);
    }

    // A quantity over the units open to the operation is the field's refusal (K-1188 (4)); any other closed verdict is "not open".
    private static PanelItemStepResult? VerdictObstacle(OperationVerdict verdict) => verdict.Decision switch
    {
        OperationDecision.Allowed => null,
        OperationDecision.QuantityExceedsOpen => PanelItemStepResult.Refused(PanelItemObstacle.QuantityExceedsOpen),
        _ => PanelItemStepResult.NotOpen,
    };

    private async Task<int> CurrentVersionAsync(Guid orderId, CancellationToken ct)
    {
        Order order = await _db.Orders.SingleAsync(o => o.Id == orderId, ct);
        return order.Version;
    }

    private Task<Guid[]> ItemIdsAsync(Guid orderId, CancellationToken ct) =>
        _db.OrderItems.AsNoTracking().Where(i => i.OrderId == orderId).Select(i => i.Id).ToArrayAsync(ct);

    private static IEnumerable<AuditValue> Codes(OrderTransitionResult result) =>
        result.ClosureCode is { } code ? [new AuditValue("TransitionCode", code.ToString())] : [];

    private static IEnumerable<AuditValue> Closure(TransitionCode? code) => code is { } c ? [new AuditValue("TransitionCode", c.ToString())] : [];

    private static string Count(int value) => value.ToString(CultureInfo.InvariantCulture);

    private static string DateText(DateOnly date) => date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

    private async Task WriteAsync(AuditOperation operation, Guid orderId, IReadOnlyList<AuditValue> values, CancellationToken ct)
    {
        string number = await _db.Orders.AsNoTracking().Where(o => o.Id == orderId).Select(o => o.OrderNumber).SingleAsync(ct);
        await _trail.WriteAsync(new AuditEntry(operation, nameof(Order), orderId, new AuditDetails(number, values: values)), ct);
    }
}
