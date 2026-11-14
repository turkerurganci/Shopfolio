using Shopfolio.Modules.Payment.Contracts;

namespace Shopfolio.Modules.AfterSales.Contracts;

/// <summary>
/// An item record that owes the firm a refund step and has no refund row yet (T529; 06 4.33; 07 UC-131): the record's kind as the refund type names it - a cancellation, a removal, a late delivery
/// termination, a withdrawal owed at its declaration, or a withdrawal's goods receipt (<see cref="ReturnId"/> set) -, its id, the item, the units and the moment it was recorded.
/// </summary>
public sealed record OwedRefundRecord(RefundType Type, Guid RecordId, Guid OrderItemId, int Quantity, DateTimeOffset RecordedAt)
{
    /// <summary>The receipt of a withdrawal after delivery; the <see cref="RecordId"/> is then the receipt's withdrawal (06 4.33: the withdrawal's refund carries both).</summary>
    public Guid? ReturnId { get; init; }

    /// <summary>The key the order screen's form carries (07 UC-131: the record that causes the refund): the receipt for a receipt, the record itself otherwise.</summary>
    public Guid CauseId => ReturnId ?? RecordId;
}
