using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Ordering.Contracts;

namespace Shopfolio.Modules.Ordering;

/// <summary>
/// The rules of the delivery mark (T351; 02 3.20.9-3.20.12; 03 8.2.6, 8.2.9): the mark is what an item holds - the moment and the date it was delivered - and there is no state machine
/// of the item beside it (02 3.20.10). The one place that says when the "completed" mark of a service is offered and accepted, and which delivery dates of a physical delivery are refused.
/// The physical item's mark is the administrator's (S6), the digital item's the system's at the payment's approval (T318), the service item's the administrator's "completed". The operations (<c>MarkDeliveredAsync</c>, <c>MarkServiceCompletedAsync</c>) and the panel's offer read these; nothing here reads a clock.
/// </summary>
internal static class DeliveryMarkRules
{
    /// <summary>
    /// Whether the payment lets the "completed" mark of a service item be set (02 3.20.9, 5.6.1; K-536, K-671): it does unless the payment is Pending or Failed. A partial refund
    /// (<see cref="PaymentStatus.PartiallyRefunded"/>) does not close the mark - the refund of some of the items leaves the rest to be completed.
    /// </summary>
    public static bool IsPaymentOpenToMark(PaymentStatus payment) => payment is not (PaymentStatus.Pending or PaymentStatus.Failed);

    /// <summary>
    /// Whether the panel offers the "completed" mark of a service item (03 8.2.9, 3.5.2.14): the item is a service item, it has an open quantity, it carries no mark and the payment is
    /// open to the mark. An unpaid order (Pending, Failed) is not offered the mark - its customer's way is the whole order's cancellation (02 7.3.3).
    /// </summary>
    public static bool IsServiceMarkOffered(ProductType type, PaymentStatus payment, int openQuantity, bool isMarked) =>
        type == ProductType.Service && openQuantity > 0 && !isMarked && IsPaymentOpenToMark(payment);

    /// <summary>
    /// The refusal of a physical delivery's date (02 3.20.11; K-554): a date in the past is allowed; one after <paramref name="today"/> (Turkey's day) and one before the day the order was
    /// shipped (<paramref name="shippingDay"/>; null when no shipment is recorded) are not. Null when the date is allowed; the day of the shipment itself is.
    /// </summary>
    public static DeliveryDateObstacle? CheckDeliveryDate(DateOnly date, DateOnly today, DateOnly? shippingDay)
    {
        if (date > today)
        {
            return DeliveryDateObstacle.InTheFuture;
        }

        return shippingDay is { } shipped && date < shipped ? DeliveryDateObstacle.BeforeShipment : null;
    }
}
