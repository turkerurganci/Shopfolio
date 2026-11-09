using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Payment.Contracts;

namespace Shopfolio.Modules.Ordering.Contracts;

/// <summary>
/// The panel home's counts the orders alone decide (T382; 02 10.6.1): the orders that wait for their bank transfer's payment approval, and the Teslim edilemedi orders that came back and
/// are to be sent again or cancelled (02 5.8; K-590).
/// </summary>
/// <param name="PaymentApproval">Received, payment pending, paid by bank transfer.</param>
/// <param name="DeliveryFailed">The orders in the Teslim edilemedi state.</param>
public sealed record PendingOrderCounts(int PaymentApproval, int DeliveryFailed);

/// <summary>Which order set a page of <see cref="IOrderingModule.ListPendingWorkItemsAsync"/> reads (T382; 02 10.6.1).</summary>
public enum PendingWorkSet
{
    /// <summary>The items of Hazırlanıyor orders - the physical ones: the order is "kargoya verilecek" when one of them has an open quantity (02 5.4 S5).</summary>
    PreparingPhysical,

    /// <summary>The items of Shipped orders - the physical ones: the order waits for the delivery mark (S6) while one of them has an open quantity.</summary>
    ShippedPhysical,

    /// <summary>The service items of orders that are not cancelled, whose payment is not Pending or Failed and that carry no "completed" mark: the order waits for the mark while one has an open quantity (02 3.20.9).</summary>
    ServiceUnmarked,
}

/// <summary>One item of an order in a pending work set, with the ordered quantity: the caller takes the open quantity from the item records' one rule.</summary>
public sealed record PendingWorkItem(Guid OrderId, Guid OrderItemId, int Quantity);

/// <summary>What the refund work reads of an item and its order (T382; 02 10.6.1): the type, the delivery date, the way the order was paid, its payment state and whether the order was ever shipped.</summary>
public sealed record RefundWorkItemFacts(
    Guid OrderItemId,
    Guid OrderId,
    ProductType ProductType,
    DateOnly? DeliveryDate,
    PaymentMethod PaymentMethod,
    PaymentStatus PaymentStatus,
    bool IsShipped);
