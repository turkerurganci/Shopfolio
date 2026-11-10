using Shopfolio.Modules.AfterSales.Contracts;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Modules.Payment.Contracts;

namespace Shopfolio.Modules.Ordering;

/// <summary>
/// An item as the operation decision reads it (03 1.11; T323): plain values only - its type and quantities, its delivery mark, what the after-sales records and periods say
/// (<see cref="Period"/>, worked out by the AfterSales module's own rules) and what is left of a digital item's download right. The decision reads no clock and no database.
/// </summary>
internal sealed record OperationItemFacts(
    Guid ItemId,
    ProductType ProductType,
    int Quantity,
    int OpenQuantity,
    DateOnly? DeliveryDate,
    bool IsDeliveryMarked,
    int DownloadsLeft,
    ItemPeriodState Period);

/// <summary>
/// The order as the operation decision reads it (03 1.11; T323): the two statuses, the payment method, the order's flags (guest order, personal data disposed, transfer details
/// frozen, e-mail marked as not delivered), the refund IBAN standing, the card refund marks, the money still refundable, whether the correction has a transition to take back,
/// and the items. Built once from the records by <see cref="OrderOperations"/>; the same value feeds the screen's read and the POST's check.
/// </summary>
internal sealed record OrderOperationFacts(
    Guid OrderId,
    int Version,
    OrderStatus OrderStatus,
    PaymentStatus PaymentStatus,
    PaymentMethod PaymentMethod,
    DateTimeOffset CreatedAt,
    DateTimeOffset Now,
    bool IsGuestOrder,
    bool PersonalDataDisposed,
    bool HasTransferDetails,
    bool EmailNotDelivered,
    bool HasRefundIban,
    bool HasOpenIbanRequest,
    bool CardRefundNotRealized,
    bool CardRefundOutcomePending,
    decimal RefundableRemaining,
    bool HasCorrectableTransition,
    IReadOnlyList<OperationItemFacts> Items)
{
    /// <summary>The payment was confirmed at some moment (03 1.11.2-1.11.3: "ödeme onaylandıktan sonra"): Paid and the refunded statuses after it.</summary>
    public bool PaymentApproved => PaymentApproval.IsApproved(PaymentStatus);

    /// <summary>The order has passed to "Kargoya verildi" (03 1.11.5): Shipped, DeliveryFailed or Delivered.</summary>
    public bool HasShipped => OrderStatus is OrderStatus.Shipped or OrderStatus.DeliveryFailed or OrderStatus.Delivered;

    /// <summary>The order carries a physical item (frozen at confirmation; a cancelled one counts).</summary>
    public bool HasPhysicalItem => Items.Any(i => i.ProductType == ProductType.Physical);

    /// <summary>A physical item has open units: the shipment line is still open (03 1.4.3).</summary>
    public bool HasOpenPhysicalItem => Items.Any(i => i.ProductType == ProductType.Physical && i.OpenQuantity > 0);

    /// <summary>The transfer line's refund waits for an IBAN: the order is a transfer order, money is refundable and no IBAN is on file.</summary>
    public bool IbanMissingForRefund => PaymentMethod == PaymentMethod.BankTransfer && !HasRefundIban;
}

/// <summary>What one rule reads: the order and, for an operation on an item, the item.</summary>
internal readonly record struct OperationInput(OrderOperationFacts Order, OperationItemFacts? Item);
