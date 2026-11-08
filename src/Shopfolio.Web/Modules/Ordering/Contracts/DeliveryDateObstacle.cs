namespace Shopfolio.Modules.Ordering.Contracts;

/// <summary>
/// Why the delivery date the administrator entered is refused (02 3.20.11; K-554): the panel names the reason beside the field. It is the
/// <see cref="OrderTransitionResult.DateObstacle"/> of a <see cref="TransitionRefusal.InvalidInput"/> refusal of the delivery mark (S6); a result code, never stored.
/// A date in the past is allowed - the carrier may have delivered on Tuesday and the firm marks it on Thursday.
/// </summary>
public enum DeliveryDateObstacle
{
    /// <summary>The date is after today in Turkey: a delivery that has not happened can not be marked.</summary>
    InTheFuture,

    /// <summary>The date is before the day the order was shipped: the goods can not arrive before they left.</summary>
    BeforeShipment,
}
