namespace Shopfolio.Modules.Payment.Contracts;

/// <summary>The closed values of RefundType (06 5.6.7). Only the code names live here; the behavior of a value lives in the rule that owns it (06 5.1.3).</summary>
public enum RefundType
{
    Cancellation,
    ItemRemoval,
    LateDeliveryTermination,
    Withdrawal,
    DefectClaimRescission,
    AmountBased,
    LatePayment,
}
