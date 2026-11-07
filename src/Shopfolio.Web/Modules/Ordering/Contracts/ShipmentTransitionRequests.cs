using Shopfolio.Infrastructure;
using Shopfolio.Modules.AfterSales.Contracts;
using Shopfolio.Modules.Notifications.Contracts;

namespace Shopfolio.Modules.Ordering.Contracts;

/// <summary>The administrator who asks for a transition from the panel (06 5.4.10): the id and the name the transition record keeps.</summary>
public sealed record TransitionAdmin(Guid AdminId, string AdminName);

/// <summary>
/// The carrier data of a shipment or a re-sending (02 5.4 S5, S8; 03 1.2): a listed company's code (<see cref="CarrierCatalog"/>) or an unlisted company's
/// name - and, with either, the tracking number - or the own-vehicle declaration alone.
/// </summary>
public sealed record ShipmentInfo(string? Carrier, string? OtherCarrierName, string? TrackingNumber, bool IsOwnVehicleDelivery);

/// <summary>
/// An administrator's operation on one order that takes no more than the screen's version (the panel's marking of a returned shipment, the reasonless closing
/// of a returned order). <paramref name="ExpectedVersion"/> is the version the screen showed (05 7.3.2): an older one is refused.
/// </summary>
public sealed record AdminOrderRequest(Guid OrderId, TransitionAdmin Admin, int ExpectedVersion);

/// <summary>S5 or S8: the shipment (or re-sending) of the order's open physical items. <paramref name="Confirmed"/> is the irreversible-action confirmation (03 1.6.1.2; K-677).</summary>
public sealed record ShipOrderRequest(Guid OrderId, TransitionAdmin Admin, int ExpectedVersion, bool Confirmed, ShipmentInfo Shipment);

/// <summary>S6: the delivery mark and the delivery date the administrator enters (02 5.4 S6): not before the day of the shipment, not in the future.</summary>
public sealed record MarkDeliveredRequest(Guid OrderId, TransitionAdmin Admin, int ExpectedVersion, DateOnly DeliveryDate);

/// <summary>
/// S3, S4 or S9: the cancellation of the order (03 1.2; 05 4.1). Which of the three it is follows from the state the order is in once it is locked. The
/// customer's carries no reason and no administrator; the administrator's carries the screen's version, the irreversible-action confirmation
/// (03 1.6.1.3) and a reason from the closed list (with the free note on "Other"); the system's carries the kind of automatic cancellation. An unpaid order
/// is cancelled as a whole - the payment axis closes with it (Ö2) - and a paid order's cancellation names the item whose cancellation closed the last open
/// item (<paramref name="Item"/>; the item record is already written in the caller's transaction). The closure of an order with no open item is not this
/// request: the closure rule writes it (S3, S4) or the administrator's reasonless closing does (<see cref="IOrderTransitions.CloseReturnedOrderAsync"/>).
/// </summary>
public sealed record CancelOrderRequest(
    Guid OrderId,
    ActorKind Actor,
    TransitionAdmin? Admin,
    int? ExpectedVersion,
    bool Confirmed,
    CancellationReason? FirmReason,
    string? ReasonNote,
    TransitionReason? SystemReason,
    CancelledItem? Item)
{
    public static CancelOrderRequest ByCustomer(Guid orderId, CancelledItem? item = null) =>
        new(orderId, ActorKind.Buyer, null, null, false, null, null, null, item);

    public static CancelOrderRequest ByAdministrator(
        Guid orderId,
        TransitionAdmin admin,
        int expectedVersion,
        bool confirmed,
        CancellationReason reason,
        string? reasonNote = null,
        CancelledItem? item = null) =>
        new(orderId, ActorKind.Admin, admin, expectedVersion, confirmed, reason, reasonNote, null, item);

    public static CancelOrderRequest BySystem(Guid orderId, TransitionReason reason) =>
        new(orderId, ActorKind.System, null, null, false, null, null, reason, null);
}

/// <summary>
/// The result of an operation of the transition unit (09 9.1 item 3): a refusal is a value, not an exception, and the runner rolls the transaction back on it.
/// <see cref="TransitionRecordIds"/> are the records the operation wrote, in order; <see cref="ClosureCode"/> is the closure the rule wrote at the end of the
/// same operation, if any; <see cref="ItemIds"/> are the items the operation concerned (the open physical items a shipment sent or a delivery mark covered).
/// </summary>
public sealed record OrderTransitionResult(
    TransitionRefusal? Refusal,
    IReadOnlyList<Guid> TransitionRecordIds,
    TransitionCode? ClosureCode,
    IReadOnlyList<Guid> ItemIds) : IOperationResult
{
    public bool IsRejected => Refusal is not null;
}
