namespace Shopfolio.Modules.Ordering.Contracts;

/// <summary>The closed values of PaymentStatus (06 5.2.2). Only the code names live here; the behavior of a value lives in the rule that owns it (06 5.1.3).</summary>
public enum PaymentStatus
{
    Pending,
    Paid,
    Failed,
    PartiallyRefunded,
    Refunded,
}
