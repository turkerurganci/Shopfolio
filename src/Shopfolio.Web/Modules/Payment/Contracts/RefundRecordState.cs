namespace Shopfolio.Modules.Payment.Contracts;

/// <summary>The closed values of RefundRecordState (06 5.4.1). Only the code names live here; the behavior of a value lives in the rule that owns it (06 5.1.3).</summary>
public enum RefundRecordState
{
    Sending,
    OutcomePending,
    Succeeded,
    Failed,
}
