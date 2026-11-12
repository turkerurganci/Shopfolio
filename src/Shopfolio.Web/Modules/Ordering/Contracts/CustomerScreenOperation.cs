namespace Shopfolio.Modules.Ordering.Contracts;

/// <summary>The three operations the one screen carries (04 5.17 m1; K-744).</summary>
public enum CustomerScreenOperation
{
    CancelOrder,
    CancelOrderItems,
    TerminateForLateDelivery,
}
