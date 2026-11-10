namespace Shopfolio.Modules.Administration.Contracts;

/// <summary>The list a counter leads to, filtered to the counted records (10 KP-50). The lists' screens are T52x's; this is the target they take.</summary>
public enum PanelListTarget
{
    /// <summary>The order list, filtered to the orders awaiting the payment approval.</summary>
    OrdersAwaitingPaymentApproval,

    /// <summary>The order list, filtered to the orders to ship.</summary>
    OrdersToShip,

    /// <summary>The order list, filtered to the shipped orders awaiting the delivery mark.</summary>
    OrdersAwaitingDeliveryMark,

    /// <summary>The order list, filtered to the orders with a service to complete.</summary>
    OrdersWithServicePending,

    /// <summary>The open requests: the contact requests and the defect claims together (the one counter has one list - 04 E-38, GA-9).</summary>
    OpenRequests,

    /// <summary>The refund and return work, the "IBAN bekleniyor" list being a separate list that is not counted (02 10.6.1).</summary>
    RefundAndReturnWork,
}
