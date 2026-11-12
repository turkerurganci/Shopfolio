using Shopfolio.Infrastructure;

namespace Shopfolio.Modules.Ordering.Contracts;

/// <summary>
/// The outcome of one of the order screen's line steps (T527; 07 UC-123...UC-128): the refusal (null on success), the reason a refused delivery date names, and - on a delivery date that falls on a
/// withdrawal declaration's day with no answer - the question of which came first with the declarations' moments (04 2.12.5.2; K-608). A refusal writes nothing.
/// </summary>
public sealed record PanelStepResult(TransitionRefusal? Refusal) : IOperationResult
{
    public bool IsRejected => Refusal is not null;

    /// <summary>The reason of a refused delivery date (K-554); null otherwise.</summary>
    public DeliveryDateObstacle? DateObstacle { get; init; }

    /// <summary>True when the date is a withdrawal declaration's day and the order of the two was not answered (K-608): nothing was written.</summary>
    public bool AsksDeliveryOrder { get; init; }

    /// <summary>The moments of the declarations made on the date, shown beside the question (04 9.9.17).</summary>
    public IReadOnlyList<DateTimeOffset> DeclarationMoments { get; init; } = [];

    public static PanelStepResult Done { get; } = new((TransitionRefusal?)null);
}

/// <summary>
/// The panel's six line steps on one order (T527; 07 UC-123...UC-128; 04 9.9.15-9.9.18): each calls the transition unit (<see cref="IOrderTransitions"/>), which owns the condition, the version check and the
/// records, and writes the audit row in the same transaction (02 10.3.1 (3); 09 9.2 item 5). Every method joins the caller's transaction and saves nothing; a refusal writes no audit row because the caller's
/// transaction is rolled back on <see cref="PanelStepResult.IsRejected"/>.
/// </summary>
public interface IPanelOrderSteps
{
    /// <summary>UC-123: the transfer order's "paid" mark (Ö1 with S1 or S2).</summary>
    Task<PanelStepResult> MarkPaidAsync(Guid orderId, TransitionAdmin admin, int expectedVersion, bool confirmed, CancellationToken ct);

    /// <summary>UC-124: the shipment of the open physical items (S5).</summary>
    Task<PanelStepResult> MarkShippedAsync(ShipOrderRequest request, CancellationToken ct);

    /// <summary>UC-125: the re-sending of a returned order's open physical items (S8).</summary>
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
