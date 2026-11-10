namespace Shopfolio.Modules.Administration.Contracts;

/// <summary>The six counters, by name (02 10.6.1). The order is the order the page lists them.</summary>
public enum PanelCounterKind
{
    /// <summary>Bank transfer orders that wait for the payment approval.</summary>
    PaymentApproval,

    /// <summary>Orders to ship: Hazırlanıyor with an open physical item and the Teslim edilemedi orders.</summary>
    ToShip,

    /// <summary>Shipped orders that wait for the delivery mark.</summary>
    DeliveryMarkPending,

    /// <summary>Orders with a service item still to be marked completed.</summary>
    ServicePending,

    /// <summary>Open contact requests and open defect claims.</summary>
    OpenRequest,

    /// <summary>Orders with returned goods to receive, a refund to process, or a card refund that failed or awaits its result.</summary>
    RefundAndReturnPending,
}
