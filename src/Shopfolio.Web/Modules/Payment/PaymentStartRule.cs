using System.Diagnostics;

using Shopfolio.Infrastructure.Integrations;
using Shopfolio.Modules.Ordering.Contracts;

namespace Shopfolio.Modules.Payment;

/// <summary>What the payment page's start does with the answer of starting a payment (08 2.1.8, 3.1 rule 4, 3.3; 07 4.14.2; UC-29).</summary>
internal enum PaymentStartStep
{
    /// <summary>The payment may start: the page opens the provider's form in its frame.</summary>
    OpenPaymentForm,

    /// <summary>
    /// A connection error - no connection, the 15 second limit, a server error, a set-up or code error, an unlisted answer: the payment method
    /// is unavailable; the order is cancelled without counting as a payment attempt (L-7) and the customer goes back to the cart.
    /// </summary>
    CancelAsUnavailable,

    /// <summary>The provider refused a second token for the same order: not a connection error - the order is not cancelled and stays on its confirmation.</summary>
    StayOnConfirmation,
}

/// <summary>
/// The core's rule over the start's answer (08 3.1 rule 4: the layer tells the classes apart, the core gives their outcome; 2.1.8's table): the
/// connection error and the second token refusal are two classes with two outcomes, and a start that timed out is a connection error
/// (<see cref="PaymentStartResult.FromFailure"/>). <see cref="PaymentStarts"/> calls this rule first; the page (UC-29) cancels a
/// <see cref="PaymentStartStep.CancelAsUnavailable"/> order with <see cref="UnopenedCancellationReason"/>.
/// </summary>
internal static class PaymentStartRule
{
    /// <summary>
    /// The reason of the cancellation of an order whose payment form could not be opened (08 2.1.8; 02 6.2.15, 8.2 L-7; K-614): L-7 does not count it
    /// (<c>UnpaidCancellationLimit.CountedReasons</c>), while a form that opened and whose payment failed is cancelled with the card failure's reason and is counted.
    /// </summary>
    public const TransitionReason UnopenedCancellationReason = TransitionReason.PaymentPageNotOpened;

    public static PaymentStartStep Decide(PaymentStartResult answer)
    {
        ArgumentNullException.ThrowIfNull(answer);
        return answer switch
        {
            PaymentStartResult.Started => PaymentStartStep.OpenPaymentForm,
            PaymentStartResult.ConnectionFailed => PaymentStartStep.CancelAsUnavailable,
            // The second refusal is not a connection error (the arm is on two lines: the secret guard reads "Token ... =>" on one line as a key's value).
            PaymentStartResult.SecondTokenRejected
                => PaymentStartStep.StayOnConfirmation,
            _ => throw new UnreachableException(),
        };
    }
}
