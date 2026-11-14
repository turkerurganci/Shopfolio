using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Operations;
using Shopfolio.Modules.AfterSales.Contracts;

namespace Shopfolio.Modules.Ordering.Contracts;

/// <summary>
/// The outcome of one of the order screen's item steps (T528): the refusal (null on success) with its named obstacle, and - on a notice from another channel - the warnings the window
/// and the form show (K-709). <see cref="TransitionRefusal.ConfirmationRequired"/> is the unconfirmed first POST that passed every check: the page shows the window (200).
/// </summary>
public sealed record PanelItemStepResult(TransitionRefusal? Refusal) : IOperationResult
{
    public bool IsRejected => Refusal is not null;

    /// <summary>What the page's <c>OperationRunner</c> reads (09 8.3 item 3): applied, the version conflict, or closed. A rejected one rolls the transaction back.</summary>
    public GatedOperationResult Gate => Refusal switch
    {
        null => GatedOperationResult.Decided(new OperationVerdict(OperationDecision.Allowed, OperationAvailability.Open())),
        TransitionRefusal.VersionConflict => GatedOperationResult.Conflict,
        _ => GatedOperationResult.Decided(new OperationVerdict(OperationDecision.Closed, OperationAvailability.Closed)),
    };

    public PanelItemObstacle Obstacle { get; init; }

    /// <summary>The notice's warnings (UC-130; K-709, K-1563); none for the other steps.</summary>
    public WithdrawalNoticeWarnings Warnings { get; init; } = WithdrawalNoticeWarnings.None;

    /// <summary>The order's payment way: the cancellation's and the removal's window says where the money goes (K-797).</summary>
    public bool IsTransferLine { get; init; }

    public static PanelItemStepResult Done { get; } = new((TransitionRefusal?)null);

    public static PanelItemStepResult Refused(PanelItemObstacle obstacle) => new(TransitionRefusal.InvalidInput) { Obstacle = obstacle };

    public static PanelItemStepResult NotOpen { get; } = new(TransitionRefusal.ConditionNotMet);
}

/// <summary>One chosen item and its units (07 UC-129: "kalem başına iç anahtar ve adet").</summary>
public sealed record PanelItemUnits(Guid OrderItemId, int Quantity);

/// <summary>
/// UC-129's request: before the payment the whole order (no item), after it the chosen items and units; the reason with "Diğer"'s note and "müşteriyle anlaşıldı"'s owner
/// confirmation (K-607), the screen's version and the irreversible-action confirmation (K-797).
/// </summary>
public sealed record PanelFirmCancellation(
    Guid OrderId,
    TransitionAdmin Admin,
    int ExpectedVersion,
    bool Confirmed,
    CancellationReason? Reason,
    string? ReasonNote,
    bool OwnerConfirmed,
    IReadOnlyList<PanelItemUnits> Items);

/// <summary>UC-140's request: the item, the arrived units and their arrival date; the declaration is the item's oldest one with goods on the way back (K-794).</summary>
public sealed record PanelReturnArrival(Guid OrderId, Guid OrderItemId, TransitionAdmin Admin, int ExpectedVersion, int? Quantity, DateOnly ArrivalDate, bool Confirmed = false);

/// <summary>
/// The panel's item steps on one order (T528; 07 UC-129, UC-130, UC-138, UC-140...UC-142, UC-144, UC-145; 04 9.9.19, 9.9.20, 9.9.25, 9.9.27, 9.9.28, 9.9.30). Each method joins the
/// transaction the page opened with <c>OperationRunner</c>; under the order's row lock it compares the version the screen showed (409 before the rule), asks the operation decision the GET
/// drew the button from (<see cref="IOrderOperations"/>), and only then calls the transition unit - which writes the record, the notices and the IBAN request through the AfterSales
/// module's one IBAN home - and writes the audit row (02 10.3.1 (3), 10.4.1; 09 9.2). An unconfirmed irreversible step stops with <see cref="TransitionRefusal.ConfirmationRequired"/> having
/// written nothing.
/// </summary>
public interface IPanelItemSteps
{
    /// <summary>UC-129: the firm's cancellation - the whole unpaid order (S3 + Ö2), or the chosen items' units of a paid one, all or none.</summary>
    Task<PanelItemStepResult> CancelByFirmAsync(PanelFirmCancellation request, CancellationToken ct);

    /// <summary>UC-130: a withdrawal notified by another channel, on one item and its units.</summary>
    Task<PanelItemStepResult> RecordWithdrawalAsync(RecordWithdrawalRequest request, CancellationToken ct);

    /// <summary>UC-130: a late delivery termination notified by another channel; no item and no unit - it falls on every undelivered physical item's open units.</summary>
    Task<PanelItemStepResult> RecordLateDeliveryTerminationAsync(TerminateForLateDeliveryRequest request, CancellationToken ct);

    /// <summary>UC-138: the removal of an item or the reduction of its units; the confirmation only on the card line.</summary>
    Task<PanelItemStepResult> RemoveItemAsync(RemoveItemRequest request, CancellationToken ct);

    /// <summary>UC-140: the receipt of returned goods - every receipt a record of its own; no refund starts.</summary>
    Task<PanelItemStepResult> ReceiveReturnAsync(PanelReturnArrival request, CancellationToken ct);

    /// <summary>UC-140: the rejection of returned goods - only on a conditional exemption's item, with the confirmation.</summary>
    Task<PanelItemStepResult> RejectReturnAsync(PanelReturnArrival request, CancellationToken ct);

    /// <summary>UC-141: "Stoğa ekle" of one receipt - its units, once.</summary>
    Task<PanelItemStepResult> RestockReturnAsync(Guid orderId, Guid returnId, TransitionAdmin admin, int expectedVersion, CancellationToken ct);

    /// <summary>UC-142: the "mal dönmedi" closure of the units not come back after Z-42; the quantity defaults to all of them.</summary>
    Task<PanelItemStepResult> CloseNotReturnedAsync(Guid orderId, Guid orderItemId, int? quantity, TransitionAdmin admin, int expectedVersion, CancellationToken ct);

    /// <summary>UC-144: an open defect claim to "Çözüldü".</summary>
    Task<PanelItemStepResult> ResolveDefectClaimAsync(Guid orderId, Guid defectClaimId, TransitionAdmin admin, int expectedVersion, CancellationToken ct);

    /// <summary>UC-145: a resolved defect claim back to "Açık" inside Z-18; never a new claim.</summary>
    Task<PanelItemStepResult> ReopenDefectClaimAsync(Guid orderId, Guid defectClaimId, TransitionAdmin admin, int expectedVersion, CancellationToken ct);
}
