namespace Shopfolio.Infrastructure.Values;

/// <summary>The closed values of JobType (06 5.4.4). Only the code names live here; the behavior of a value lives in the rule that owns it (06 5.1.3).</summary>
public enum JobType
{
    CardRefund,
    FinalPaymentQuery,
    ImageDerivative,
    InboundNotificationProcessing,
    RefundOutcomeQuery,
    BounceQuery,
}
