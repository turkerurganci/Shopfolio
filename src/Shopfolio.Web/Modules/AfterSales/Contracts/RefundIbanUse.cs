namespace Shopfolio.Modules.AfterSales.Contracts;

/// <summary>A refund IBAN whose deletion day (Z-38) has come: its id, the order it belongs to and the day's moment (03 4.2.8; 05 5.2.5).</summary>
public sealed record DueRefundIbanDeletion(Guid RefundIbanId, Guid OrderId, DateTimeOffset DeletionAt);

/// <summary>
/// What a refund IBAN is used by, as the deletion day reads it (03 4.1.47, 4.2.8; 02 4.3; K-1110, K-1127): the IBAN's order, its deletion moment and
/// the refund subjects linked to it. A withdrawal's subjects are its goods' receipts: a withdrawal whose goods did not arrive has none - that is the
/// case Z-38 deletes the IBAN for. A cancellation and a late delivery termination are subjects themselves.
/// </summary>
public sealed record RefundIbanUse(Guid RefundIbanId, Guid OrderId, DateTimeOffset? DeletionAt, IReadOnlyList<RefundIbanSubject> Subjects);

/// <summary>One record whose refund goes to the IBAN: a cancellation, a late delivery termination or a withdrawal's receipt of goods.</summary>
public sealed record RefundIbanSubject(Guid RecordId, RefundIbanSubjectKind Kind);

/// <summary>The kind of a record whose refund goes to a refund IBAN.</summary>
public enum RefundIbanSubjectKind
{
    Cancellation,
    LateDeliveryTermination,
    Receipt,
}
