namespace Shopfolio.Modules.AfterSales.Contracts;

/// <summary>A refund IBAN whose deletion day (Z-38) has come: its id, the order it belongs to and the day's moment (03 4.2.8; 05 5.2.5).</summary>
public sealed record DueRefundIbanDeletion(Guid RefundIbanId, Guid OrderId, DateTimeOffset DeletionAt);

/// <summary>
/// What a refund IBAN is used by, as the deletion day reads it (03 4.1.47, 4.2.8; 02 4.3; K-1110, K-1127; D-910): the IBAN's order, its deletion moment and
/// the ids of the refund subjects that may still be paid to it - the cancellations and late delivery terminations linked to it, a withdrawal's goods' receipts
/// after the delivery, the withdrawal itself before the delivery and on a service item (its refund is born with the declaration), the refund a request
/// fulfilled with it names, the records without an IBAN of their own of an item whose request it fulfilled, and the order's transfer refunds still pending
/// that would be paid to it. A withdrawal after the delivery whose goods did not arrive has no receipt, so no subject: that is the case Z-38 deletes the IBAN for.
/// </summary>
public sealed record RefundIbanUse(Guid RefundIbanId, Guid OrderId, DateTimeOffset? DeletionAt, IReadOnlyList<Guid> SubjectIds);
