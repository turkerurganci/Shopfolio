using Shopfolio.Infrastructure;

namespace Shopfolio.Modules.Ordering.Contracts;

/// <summary>The customer's defect claim from the order page (03 2.9; 02 7.5): the item, the defective units and the description of the problem.</summary>
public sealed record OpenDefectClaimRequest(Guid OrderId, Guid OrderItemId, int Quantity, string Description);

/// <summary>The administrator's "resolved" mark on an open defect claim (03 1.9.1, 8.4), with the screen's version (05 7.3.2).</summary>
public sealed record ResolveDefectClaimRequest(Guid OrderId, Guid OrderItemId, TransitionAdmin Admin, int ExpectedVersion);

/// <summary>
/// The reopening of a resolved defect claim (03 1.9.1; K-681): by the customer from the order page - who may raise the defective units (<paramref name="Quantity"/>) -
/// or by an administrator from the panel with the screen's version.
/// </summary>
public sealed record ReopenDefectClaimRequest(Guid OrderId, Guid OrderItemId, ActorKind Actor, TransitionAdmin? Admin, int? ExpectedVersion, int? Quantity)
{
    public static ReopenDefectClaimRequest ByCustomer(Guid orderId, Guid orderItemId, int? quantity = null) =>
        new(orderId, orderItemId, ActorKind.Buyer, null, null, quantity);

    public static ReopenDefectClaimRequest ByAdministrator(Guid orderId, Guid orderItemId, TransitionAdmin admin, int expectedVersion) =>
        new(orderId, orderItemId, ActorKind.Admin, admin, expectedVersion, null);
}

/// <summary>
/// The rejection of returned goods at the receipt step (03 1.7.1.7; 02 7.4.9; K-656): the withdrawal declaration, the units and their arrival date, with the screen's
/// version and the irreversible-action confirmation (the rejection is never taken back; 03 1.6.1).
/// </summary>
public sealed record RejectReturnRequest(
    Guid OrderId,
    Guid OrderItemId,
    Guid WithdrawalId,
    TransitionAdmin Admin,
    int ExpectedVersion,
    bool Confirmed,
    int Quantity,
    DateOnly ArrivalDate);

/// <summary>
/// The result of an item record operation of the transition unit (09 9.1 item 3): a refusal is a value and the runner rolls the transaction back on it;
/// <see cref="RecordId"/> is the record the operation wrote or changed. The order's statuses never change by these operations (03 1.7.1: the claim does not touch the line).
/// </summary>
public sealed record ItemRecordResult(TransitionRefusal? Refusal, Guid? RecordId) : IOperationResult
{
    public bool IsRejected => Refusal is not null;
}
