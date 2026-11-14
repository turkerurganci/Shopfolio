using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Operations;

namespace Shopfolio.Modules.Ordering.Contracts;

/// <summary>
/// The note's write as the page's <c>OperationRunner</c> reads it (T531; 07 UC-152; 09 10.9): the module's own answer (<see cref="Note"/>) and its gate - saved or unchanged is applied; the shown text
/// no longer the note's is the conflict (409, the current note goes back); an order that takes no note is closed (422). A rejected one rolls the transaction back.
/// </summary>
public sealed record PanelNoteResult(SaveInternalNoteResult Note) : IOperationResult
{
    public bool IsRejected => Note.IsRejected;

    public GatedOperationResult Gate => Note.Code switch
    {
        null => GatedOperationResult.Decided(new OperationVerdict(OperationDecision.Allowed, OperationAvailability.Open())),
        Shopfolio.Infrastructure.Errors.ErrorCode.DisplayedValueConflict => GatedOperationResult.Conflict,
        _ => GatedOperationResult.Decided(new OperationVerdict(OperationDecision.Closed, OperationAvailability.Closed)),
    };
}

/// <summary>
/// The panel order page's four item and record operations of T531 (07 UC-139, UC-143, UC-152...UC-154; 04 9.9.26, 9.9.29, 9.9.32, 9.9.33), the write half of 03 1.11.20, 1.11.27, 1.11.36 and
/// 1.11.38 (09 8.3 items 1-3; D-153). Each method joins the transaction the page opened with <c>OperationRunner</c>. The versioned ones - the service item's mark, the download right's renewal and the
/// two re-sendings - lock the order's row, compare the version the screen showed (a change is <see cref="TransitionRefusal.VersionConflict"/>, before the rule) and ask the one operation decision the GET
/// drew the buttons from (<see cref="IOrderOperations"/>; a closed operation, an item of another order or a notice that is no longer marked is <see cref="TransitionRefusal.ConditionNotMet"/>); only then
/// they write. The note carries no version: the shown text is its condition (09 10.9; <see cref="IOrderCorrections.SaveInternalNoteAsync"/>).
/// </summary>
public interface IPanelOrderUpkeep
{
    /// <summary>
    /// UC-139: the service item's "completed" mark on its whole open quantity (K-792), only while the payment is approved (K-536); the withdrawal right falls and "sorun bildir" opens with the mark, and
    /// when it was the last open item the closure rule ends the order's line (S2, S10 or S11). The trail row is <c>CompleteService</c> (02 10.3.1 (3); D-646). No confirmation, no notice.
    /// </summary>
    Task<PanelStepResult> CompleteServiceItemAsync(PanelItemStepRequest request, CancellationToken ct);

    /// <summary>UC-143: the digital item's download counter back at zero (02 3.12.6; K-683); no notice and no trail row (02 3.12.6, 9.2). An item that is not digital is closed.</summary>
    Task<PanelStepResult> ResetDownloadCountAsync(PanelItemStepRequest request, CancellationToken ct);

    /// <summary>UC-152: the internal note written, changed or - emptied - deleted (K-1051); the order's version does not move and the note's text never reaches the trail (02 10.4.7).</summary>
    Task<PanelNoteResult> SaveInternalNoteAsync(SaveInternalNoteRequest request, CancellationToken ct);

    /// <summary>
    /// UC-153: a new "queued" row in the marked notice's group, which the queue sends (05 4.4.3, 5.1.4; K-1262). The condition is read under the order's row lock, so a concurrent second request finds the
    /// group's latest row queued and writes nothing (K-1066, K-1084); the order's version does not move.
    /// </summary>
    Task<PanelStepResult> ResendNotificationAsync(PanelResendRequest request, CancellationToken ct);

    /// <summary>UC-154: a new "queued" row for every marked notice of the order in the one transaction - all or none (K-761); there is no re-sending across orders.</summary>
    Task<PanelStepResult> ResendAllNotificationsAsync(AdminOrderRequest request, CancellationToken ct);
}

/// <summary>An item operation of the panel's order page: the order, the item, the administrator and the order's version the screen showed.</summary>
public sealed record PanelItemStepRequest(Guid OrderId, Guid OrderItemId, TransitionAdmin Admin, int ExpectedVersion);

/// <summary>UC-153's input: the order, the latest row of the marked notice's group (06 4.58), the administrator and the order's version the screen showed (K-1188 (2)).</summary>
public sealed record PanelResendRequest(Guid OrderId, Guid DispatchRecordId, TransitionAdmin Admin, int ExpectedVersion);
