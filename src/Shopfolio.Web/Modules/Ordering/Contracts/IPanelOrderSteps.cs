using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Operations;

namespace Shopfolio.Modules.Ordering.Contracts;

/// <summary>
/// The outcome of one of the order screen's line steps (T527; 07 UC-123...UC-128): the refusal (null on success), the reason a refused delivery date names, and - on a delivery date that falls on a
/// withdrawal declaration's day with no answer - the question of which came first with the declarations' moments (04 2.12.5.2; K-608). A refusal writes nothing.
/// <see cref="TransitionRefusal.ConfirmationRequired"/> is the unconfirmed first POST of an irreversible step that passed the version and the operation's rule: the page shows the window (200).
/// </summary>
public sealed record PanelStepResult(TransitionRefusal? Refusal) : IOperationResult
{
    public bool IsRejected => Refusal is not null;

    /// <summary>
    /// What the page's <c>OperationRunner</c> reads (09 8.3 item 3): applied, the version conflict (409 before the rule; 07 9.9), or the operation closed. A rejected one rolls the transaction back.
    /// </summary>
    public GatedOperationResult Gate => Refusal switch
    {
        null => GatedOperationResult.Decided(new OperationVerdict(OperationDecision.Allowed, OperationAvailability.Open())),
        TransitionRefusal.VersionConflict => GatedOperationResult.Conflict,
        _ => GatedOperationResult.Decided(new OperationVerdict(OperationDecision.Closed, OperationAvailability.Closed)),
    };

    /// <summary>The reason of a refused delivery date (K-554); null otherwise.</summary>
    public DeliveryDateObstacle? DateObstacle { get; init; }

    /// <summary>True when the date is a withdrawal declaration's day and the order of the two was not answered (K-608): nothing was written.</summary>
    public bool AsksDeliveryOrder { get; init; }

    /// <summary>The moments of the declarations made on the date, shown beside the question (04 9.9.17).</summary>
    public IReadOnlyList<DateTimeOffset> DeclarationMoments { get; init; } = [];

    public static PanelStepResult Done { get; } = new((TransitionRefusal?)null);
}

/// <summary>
/// The panel's six line steps on one order (T527; 07 UC-123...UC-128; 04 9.9.15-9.9.18), the write half of operations 03 1.11.15-1.11.19 and 1.11.40 (09 8.3 items 1-3; D-153). Each method joins the
/// transaction the page opened with <c>OperationRunner</c> and, under the order's row lock, compares the version the screen showed (a change is <see cref="TransitionRefusal.VersionConflict"/>, before the
/// rule; 07 9.9) and asks the one operation decision the GET drew the buttons from (<see cref="IOrderOperations"/>; a closed operation is <see cref="TransitionRefusal.ConditionNotMet"/>); only then the
/// transition unit (<see cref="IOrderTransitions"/>) writes, with the audit row in the same transaction (02 10.3.1 (3); 09 9.2 item 5). An irreversible step sent without its confirmation stops there with
/// <see cref="TransitionRefusal.ConfirmationRequired"/>. Nothing is saved here; a refusal writes nothing because the runner rolls the transaction back on <see cref="PanelStepResult.Gate"/>.
/// </summary>
public interface IPanelOrderSteps
{
    /// <summary>UC-123: the transfer order's "paid" mark (Ö1 with S1 or S2); unconfirmed, the checks only.</summary>
    Task<PanelStepResult> MarkPaidAsync(Guid orderId, TransitionAdmin admin, int expectedVersion, bool confirmed, CancellationToken ct);

    /// <summary>
    /// UC-124: the shipment of the open physical items (S5); unconfirmed, the checks only. A carrier value the transition unit would refuse - missing, both a listed company and another's name, a code
    /// not in the list, a value over its cap - is <see cref="TransitionRefusal.InvalidInput"/> before the lock (K-799; 07 UC-124: <c>ShipmentInfoMissing</c>).
    /// </summary>
    Task<PanelStepResult> MarkShippedAsync(ShipOrderRequest request, CancellationToken ct);

    /// <summary>UC-125: the re-sending of a returned order's open physical items (S8); as UC-124.</summary>
    Task<PanelStepResult> MarkReshippedAsync(ShipOrderRequest request, CancellationToken ct);

    /// <summary>
    /// UC-126: the delivery mark and date (S6). On a date that is a withdrawal declaration's day of one of the order's physical items the answer of which came first is required; without it nothing is
    /// written (<see cref="PanelStepResult.AsksDeliveryOrder"/>), with it the answer is written on each such declaration and in the audit row (K-608).
    /// </summary>
    Task<PanelStepResult> MarkDeliveredAsync(MarkDeliveredRequest request, bool? deliveredBeforeDeclaration, CancellationToken ct);

    /// <summary>UC-127: the returned shipment's mark (S7, and S11 when a delivered item leaves no open item).</summary>
    Task<PanelStepResult> MarkDeliveryFailedAsync(AdminOrderRequest request, CancellationToken ct);

    /// <summary>UC-128: the reasonless closing of a returned order with no open and no delivered item (S9 as a closure); no cancellation record and no refund.</summary>
    Task<PanelStepResult> CloseDeliveryFailedAsync(AdminOrderRequest request, CancellationToken ct);
}
