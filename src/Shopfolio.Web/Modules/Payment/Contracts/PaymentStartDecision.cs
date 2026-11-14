namespace Shopfolio.Modules.Payment.Contracts;

/// <summary>
/// What the payment page does with a confirmed card order's start (07 UC-29; 08 2.1.8, 3.1 rule 4): the module's rule over the link layer's answer, in the page's words.
/// </summary>
public enum PaymentStartDecision
{
    /// <summary>The payment may start: the page shows the provider's form in its frame (<see cref="PaymentStartAnswer.FrameAddress"/>).</summary>
    OpenPaymentForm,

    /// <summary>The provider could not be reached: the order is cancelled with <see cref="IPaymentStart.UnopenedCancellationReason"/> and the customer goes back to the cart.</summary>
    CancelAsUnavailable,

    /// <summary>The provider refused a second token for the same order: the order is not cancelled and the confirmation screen opens again.</summary>
    StayOnConfirmation,
}
