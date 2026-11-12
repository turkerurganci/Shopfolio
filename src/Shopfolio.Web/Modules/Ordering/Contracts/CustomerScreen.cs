namespace Shopfolio.Modules.Ordering.Contracts;

/// <summary>The two addresses of the screen under the order's root (07 2.1.3, 4.17.1).</summary>
public enum CustomerScreen
{
    /// <summary><c>{kök}/iptal</c>: the order's cancellation or the item cancellation by the payment status (K-1162 (3)).</summary>
    Cancellation,

    /// <summary><c>{kök}/gecikme-feshi</c>.</summary>
    LateDeliveryTermination,
}
