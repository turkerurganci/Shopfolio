namespace Shopfolio.Modules.Ordering.Contracts;

/// <summary>The closed values of TransitionReason (06 5.6.5). Only the code names live here; the behavior of a value lives in the rule that owns it (06 5.1.3).</summary>
public enum TransitionReason
{
    PaymentWindowExpired,
    CardPaymentFailed,
    PaymentPageNotOpened,
    SameCartReorder,
    Closure,
}
