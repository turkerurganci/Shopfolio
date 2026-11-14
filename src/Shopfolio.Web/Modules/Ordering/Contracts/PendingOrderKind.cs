namespace Shopfolio.Modules.Ordering.Contracts;

/// <summary>The two order sets of <see cref="PendingOrderCounts"/> as the panel's order list filters by them (T527; 04 9.8.4): the same rules, one set each.</summary>
public enum PendingOrderKind
{
    /// <summary>Received, payment pending, paid by bank transfer: the orders waiting for the payment approval (<see cref="PendingOrderCounts.PaymentApproval"/>).</summary>
    TransferPaymentApproval,

    /// <summary>The Teslim edilemedi orders (<see cref="PendingOrderCounts.DeliveryFailed"/>).</summary>
    DeliveryFailed,
}
