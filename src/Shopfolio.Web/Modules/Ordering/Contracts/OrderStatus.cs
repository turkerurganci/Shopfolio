namespace Shopfolio.Modules.Ordering.Contracts;

/// <summary>The closed values of OrderStatus (06 5.2.1). Only the code names live here; the behavior of a value lives in the rule that owns it (06 5.1.3).</summary>
public enum OrderStatus
{
    Placed,
    Preparing,
    Shipped,
    Delivered,
    DeliveryFailed,
    Cancelled,
}
