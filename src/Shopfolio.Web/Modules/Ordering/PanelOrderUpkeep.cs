using System.Globalization;

using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Forms;
using Shopfolio.Modules.Administration.Contracts;
using Shopfolio.Modules.Notifications.Contracts;
using Shopfolio.Modules.Ordering.Contracts;

namespace Shopfolio.Modules.Ordering;

/// <summary>
/// The panel order page's service mark, download renewal, internal note and e-mail re-sendings (T531; 07 UC-139, UC-143, UC-152...UC-154; 09 8.3 item 3). The versioned four run in
/// <see cref="PanelOrderSteps"/>' order: the order's row lock, the version the screen showed (409 before the rule), the operation's one rule - the <see cref="OrderOperationCatalog"/> row the GET drew the
/// button from, over the facts read after the lock -, and only an open operation reaches the writer: the transition unit's service mark, the download counter, the notifications module's re-sending. The
/// note keeps its own unit (<see cref="OrderCorrections.SaveInternalNoteAsync"/>; 09 10.9): a shared row lock and the shown text. Joins the caller's transaction; nothing is saved here.
/// </summary>
internal sealed class PanelOrderUpkeep : IPanelOrderUpkeep
{
    private readonly AppDbContext _db;
    private readonly OrderOperations _operations;
    private readonly OrderLocks _locks;
    private readonly IOrderTransitions _transitions;
    private readonly IOrderCorrections _corrections;
    private readonly DownloadCounters _downloads;
    private readonly INotificationsModule _notifications;
    private readonly IAuditTrail _trail;

    public PanelOrderUpkeep(
        AppDbContext db,
        OrderOperations operations,
        OrderLocks locks,
        IOrderTransitions transitions,
        IOrderCorrections corrections,
        DownloadCounters downloads,
        INotificationsModule notifications,
        IAuditTrail trail)
    {
        _db = db;
        _operations = operations;
        _locks = locks;
        _transitions = transitions;
        _corrections = corrections;
        _downloads = downloads;
        _notifications = notifications;
        _trail = trail;
    }

    public async Task<PanelStepResult> CompleteServiceItemAsync(PanelItemStepRequest request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (await GateAsync(request.OrderId, request.ExpectedVersion, OrderOperation.MarkServiceCompleted, request.OrderItemId, ct) is { } refused)
        {
            return refused;
        }

        OrderTransitionResult result = await _transitions.MarkServiceCompletedAsync(new MarkServiceCompletedRequest(request.OrderId, request.OrderItemId, request.Admin, request.ExpectedVersion), ct);
        if (result.Refusal is { } refusal)
        {
            return new PanelStepResult(refusal);
        }

        // The mark is an item mark with no transition of its own; the closure's code, when the last open item ended the line, goes with it (02 10.3.1 (3); 09 9.2 item 3; D-646).
        List<AuditValue> values = [new AuditValue("ItemId", request.OrderItemId.ToString("D", CultureInfo.InvariantCulture))];
        if (result.ClosureCode is { } closure)
        {
            values.Add(new AuditValue("TransitionCode", closure.ToString()));
        }

        await WriteAsync(AuditOperation.CompleteService, request.OrderId, values, ct);
        return PanelStepResult.Done;
    }

    public async Task<PanelStepResult> ResetDownloadCountAsync(PanelItemStepRequest request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (await GateAsync(request.OrderId, request.ExpectedVersion, OrderOperation.RenewDownloadAllowance, request.OrderItemId, ct) is { } refused)
        {
            return refused;
        }

        // The counter's own write (02 3.12.6): under the same lock, against the same version; the renewal is not written to the trail and no notice goes (02 3.12.6, 9.2; K-683).
        DownloadRenewalResult renewal = await _downloads.RenewAsync(request.OrderItemId, request.ExpectedVersion, ct);
        return renewal.IsRenewed
            ? PanelStepResult.Done
            : new PanelStepResult(renewal == DownloadRenewalResult.VersionConflict ? TransitionRefusal.VersionConflict : TransitionRefusal.ConditionNotMet);
    }

    public async Task<PanelNoteResult> SaveInternalNoteAsync(SaveInternalNoteRequest request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        return new PanelNoteResult(await _corrections.SaveInternalNoteAsync(request, ct));
    }

    public async Task<PanelStepResult> ResendNotificationAsync(PanelResendRequest request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (await GateAsync(request.OrderId, request.ExpectedVersion, OrderOperation.ResendEmail, null, ct) is { } refused)
        {
            return refused;
        }

        Order order = await ReadOrderAsync(request.OrderId, ct);
        NoticeResendResult result = await _notifications.ResendCustomerNoticeAsync(new NoticeResendRequest(order.Id, order.OrderNumber, request.DispatchRecordId, order.ContactEmail ?? string.Empty), ct);
        return result == NoticeResendResult.Queued ? PanelStepResult.Done : new PanelStepResult(TransitionRefusal.ConditionNotMet);
    }

    public async Task<PanelStepResult> ResendAllNotificationsAsync(AdminOrderRequest request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (await GateAsync(request.OrderId, request.ExpectedVersion, OrderOperation.ResendEmail, null, ct) is { } refused)
        {
            return refused;
        }

        // Every marked notice of this one order, each in its own group; one refused row refuses the whole request and the caller's transaction rolls the others back (K-761: all or none).
        Order order = await ReadOrderAsync(request.OrderId, ct);
        IReadOnlyList<UndeliveredNotice> marked = await _notifications.FindUndeliveredNoticesAsync(request.OrderId, ct);
        if (marked.Count == 0)
        {
            return new PanelStepResult(TransitionRefusal.ConditionNotMet);
        }

        foreach (UndeliveredNotice notice in marked)
        {
            NoticeResendResult result = await _notifications.ResendCustomerNoticeAsync(
                new NoticeResendRequest(order.Id, order.OrderNumber, notice.LatestDispatchId, order.ContactEmail ?? string.Empty),
                ct);
            if (result != NoticeResendResult.Queued)
            {
                return new PanelStepResult(TransitionRefusal.ConditionNotMet);
            }
        }

        return PanelStepResult.Done;
    }

    /// <summary>
    /// The re-check of 09 8.3 item 3 in its order: the order's row lock, the version the screen showed (a change is the conflict and the rule is not asked), then the operation's rule over the facts read
    /// after the lock - the decision the GET drew the button from (<see cref="OrderOperationCatalog.Check"/>; an item that is not the order's is closed). Null: the operation may go on.
    /// </summary>
    private async Task<PanelStepResult?> GateAsync(Guid orderId, int expectedVersion, OrderOperation operation, Guid? orderItemId, CancellationToken ct)
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
        return facts is not null && OrderOperationCatalog.Check(facts, operation, orderItemId, null).IsAllowed ? null : new PanelStepResult(TransitionRefusal.ConditionNotMet);
    }

    private Task<Order> ReadOrderAsync(Guid orderId, CancellationToken ct) => _db.Orders.AsNoTracking().SingleAsync(o => o.Id == orderId, ct);

    private async Task WriteAsync(AuditOperation operation, Guid orderId, IReadOnlyList<AuditValue> values, CancellationToken ct)
    {
        string number = await _db.Orders.AsNoTracking().Where(o => o.Id == orderId).Select(o => o.OrderNumber).SingleAsync(ct);
        await _trail.WriteAsync(new AuditEntry(operation, nameof(Order), orderId, new AuditDetails(number, values: values)), ct);
    }
}
