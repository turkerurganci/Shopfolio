using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Operations;
using Shopfolio.Modules.Membership.Contracts;

namespace Shopfolio.Modules.Ordering.Contracts;

/// <summary>
/// The order screen's "Düzeltmeler" (T530; 07 UC-146...UC-151; 04 9.9.31): the address, the carrier, the delivery date, a return's arrival date, a status transition and a guest order's e-mail. Each method
/// joins the transaction the page opened with <c>OperationRunner</c>; the writes are the existing units' (<see cref="IOrderCorrections"/>, <see cref="IOrderTransitions"/>) - this facade only orders the
/// panel's checks around them, names the reason of a refusal for the screen and writes the status correction's audit row (02 10.3.2; D-884). None asks the irreversible-action confirmation (04 9.9.31).
/// A refusal writes nothing: the runner rolls the transaction back on <see cref="PanelCorrectionResult.Gate"/>.
/// </summary>
public interface IPanelOrderCorrections
{
    /// <summary>What the correction forms show (04 9.9.31): the order's current addresses, the transition the status correction would take back and the return receipts with their arrival dates.</summary>
    Task<PanelCorrectionChoices?> ReadChoicesAsync(Guid orderId, CancellationToken ct);

    /// <summary>UC-146: the shipping address with its telephone, or the billing address.</summary>
    Task<PanelCorrectionResult> CorrectAddressAsync(PanelAddressCorrection request, CancellationToken ct);

    /// <summary>UC-147: the carrier and the tracking number, or the own-vehicle declaration.</summary>
    Task<PanelCorrectionResult> CorrectCarrierAsync(CorrectShipmentRequest request, CancellationToken ct);

    /// <summary>UC-148: the order-level delivery date; on a withdrawal declaration's day the answer of which came first is required (K-608).</summary>
    Task<PanelCorrectionResult> CorrectDeliveryDateAsync(CorrectDeliveryDateRequest request, CancellationToken ct);

    /// <summary>UC-149: a return receipt's arrival date (K-712); the item is the record's.</summary>
    Task<PanelCorrectionResult> CorrectArrivalDateAsync(PanelArrivalDateCorrection request, CancellationToken ct);

    /// <summary>UC-150: the correction of a wrong transition with a reason; the state to go back to is the record's, never the request's.</summary>
    Task<PanelCorrectionResult> CorrectTransitionAsync(CorrectTransitionRequest request, CancellationToken ct);

    /// <summary>UC-151: a guest order's contact e-mail.</summary>
    Task<PanelCorrectionResult> CorrectOrderEmailAsync(PanelEmailCorrection request, CancellationToken ct);
}

/// <summary>UC-146's input: one address of the order - the shipping address with the telephone, or the billing address with none - and the version the screen showed.</summary>
public sealed record PanelAddressCorrection(
    Guid OrderId,
    TransitionAdmin Admin,
    AddressType AddressType,
    string? RecipientName,
    Guid? ProvinceId,
    string? District,
    string? AddressLine,
    string? Phone,
    int ExpectedVersion);

/// <summary>UC-149's input: the receipt's or the rejection's record and its new arrival date.</summary>
public sealed record PanelArrivalDateCorrection(Guid OrderId, Guid ArrivalRecordId, TransitionAdmin Admin, DateOnly ArrivalDate, int ExpectedVersion);

/// <summary>UC-151's input: the new e-mail.</summary>
public sealed record PanelEmailCorrection(Guid OrderId, TransitionAdmin Admin, string? NewEmail, int ExpectedVersion);

/// <summary>The values of a correction form (04 9.9.31): the order's addresses as they are now, with the province's id the form selects.</summary>
public sealed record PanelCorrectionChoices(
    PanelAddressValues? Shipping,
    PanelAddressValues? Billing,
    IReadOnlyList<PanelCorrectableTransition> Transitions,
    IReadOnlyList<PanelArrivalRecord> ArrivalRecords);

/// <summary>One address of the order as the correction form shows it.</summary>
public sealed record PanelAddressValues(string RecipientName, Guid? ProvinceId, string District, string AddressLine, string? Phone);

/// <summary>
/// A transition the status correction can take back (03 1.11.26; 04 9.9.31: the form shows the corrected transition and the state it goes back to): the record, its code and the state it moved from -
/// the order status of a shipment-axis row, the payment status of the transfer's "paid" mark.
/// </summary>
public sealed record PanelCorrectableTransition(Guid RecordId, TransitionCode Code, DateTimeOffset At, OrderStatus? BackToOrderStatus, PaymentStatus? BackToPaymentStatus);

/// <summary>One receipt or rejection of returned goods with its arrival date (06 4.26; K-712).</summary>
public sealed record PanelArrivalRecord(Guid RecordId, Guid OrderItemId, DateOnly ArrivalDate);

/// <summary>
/// The outcome of a correction (T530): applied, or refused with its reason. <see cref="TransitionRefusal.VersionConflict"/> is 409 (before the rule; 07 9.9), <see cref="TransitionRefusal.OrderNotFound"/>
/// 404, every other refusal 422 - the closed operation (<see cref="TransitionRefusal.ConditionNotMet"/>), a value (<see cref="Problem"/>, <see cref="Fields"/>, <see cref="DateObstacle"/>) or the status
/// correction's obstacle (<see cref="Obstacle"/>; K-703).
/// </summary>
public sealed record PanelCorrectionResult(TransitionRefusal? Refusal) : IOperationResult
{
    public bool IsRejected => Refusal is not null;

    /// <summary>What the page's <c>OperationRunner</c> reads (09 8.3 item 3): applied, the version conflict, or closed; a rejected one rolls the transaction back.</summary>
    public GatedOperationResult Gate => Refusal switch
    {
        null => GatedOperationResult.Decided(new OperationVerdict(OperationDecision.Allowed, OperationAvailability.Open())),
        TransitionRefusal.VersionConflict => GatedOperationResult.Conflict,
        _ => GatedOperationResult.Decided(new OperationVerdict(OperationDecision.Closed, OperationAvailability.Closed)),
    };

    public CorrectionValueProblem Problem { get; init; }

    /// <summary>The address rules' refused fields (UC-146; 02 3.14.6).</summary>
    public IReadOnlyList<MembershipFieldError> Fields { get; init; } = [];

    /// <summary>Why a delivery date was refused (K-554); null otherwise.</summary>
    public DeliveryDateObstacle? DateObstacle { get; init; }

    /// <summary>The corrected delivery date is a withdrawal declaration's day and the order of the two was not answered (K-608).</summary>
    public bool AsksDeliveryOrder { get; init; }

    /// <summary>The moments of the declarations made on that day, shown beside the question (04 9.9.17).</summary>
    public IReadOnlyList<DateTimeOffset> DeclarationMoments { get; init; } = [];

    /// <summary>Why the status correction can not be made (K-703); null otherwise.</summary>
    public CorrectionObstacle? Obstacle { get; init; }

    public static PanelCorrectionResult Done { get; } = new((TransitionRefusal?)null);
}
