namespace Shopfolio.Modules.Ordering.Contracts;

/// <summary>The answer of the unopened payment form's cancellation (07 UC-29): cancelled with the cart's once-told mark, or the order no longer waited for its payment.</summary>
public enum UnopenedPaymentCancellation
{
    Cancelled,
    NoLongerAwaiting,
}
