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
/// The firm's rescission on an open defect claim (02 7.5.3; 03 2.9.4, 8.4.10): the units taken back - at most the claim's defective units not rescinded yet - with the
/// screen's version. The refund is started with the amounts of 02 7.1.7; on the bank transfer line without an IBAN the customer is asked for one (B-14).
/// </summary>
public sealed record RescindDefectClaimRequest(Guid OrderId, Guid OrderItemId, TransitionAdmin Admin, int ExpectedVersion, int Quantity);

/// <summary>
/// The firm's price reduction on an open defect claim (02 7.5.3, 5.5; 03 8.3.3.7; K-228, K-539): the amount paid back - at most the order's paid money not refunded yet -
/// with the screen's version. The item is not returned; on the bank transfer line without an IBAN the customer is asked for one (B-14).
/// </summary>
public sealed record ReduceDefectClaimPriceRequest(Guid OrderId, Guid OrderItemId, TransitionAdmin Admin, int ExpectedVersion, decimal Amount);

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
/// The receipt of a withdrawal's returned goods (03 1.7.1.6; 02 7.4.7; K-794): the declaration, the units that arrived and their arrival date - in the past or today,
/// not before the item's entered delivery date -, with the screen's version. The stock is added by its own step (<see cref="AddReturnedStockRequest"/>).
/// </summary>
public sealed record ReceiveReturnRequest(Guid OrderId, Guid OrderItemId, Guid WithdrawalId, TransitionAdmin Admin, int ExpectedVersion, int Quantity, DateOnly ArrivalDate);

/// <summary>
/// The "mal dönmedi" closure (03 1.7.1.8; 02 10.4.9; K-496, K-794): the declaration and the units not come back, after the sending period (Z-42), with the screen's
/// version. No notice and no refund; on the transfer line the declaration's IBAN goes unless a pending refund still uses it (K-497, K-1127).
/// </summary>
public sealed record CloseAsNotReturnedRequest(Guid OrderId, Guid OrderItemId, Guid WithdrawalId, TransitionAdmin Admin, int ExpectedVersion, int Quantity);

/// <summary>The "Stoğa ekle" step of a receipt (02 7.4.7, 3.6.6; 06 4.26; K-215): the receipt's units go back to the item's variant once, with the screen's version.</summary>
public sealed record AddReturnedStockRequest(Guid OrderId, Guid OrderItemId, Guid ReturnId, TransitionAdmin Admin, int ExpectedVersion);

/// <summary>
/// The result of an item record operation of the transition unit (09 9.1 item 3): a refusal is a value and the runner rolls the transaction back on it;
/// <see cref="RecordId"/> is the record the operation wrote or changed. The order's statuses never change by these operations (03 1.7.1: the claim does not touch the line).
/// </summary>
public sealed record ItemRecordResult(TransitionRefusal? Refusal, Guid? RecordId) : IOperationResult
{
    public bool IsRejected => Refusal is not null;
}
