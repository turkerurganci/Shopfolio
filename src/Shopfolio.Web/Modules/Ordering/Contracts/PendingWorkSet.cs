namespace Shopfolio.Modules.Ordering.Contracts;

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
