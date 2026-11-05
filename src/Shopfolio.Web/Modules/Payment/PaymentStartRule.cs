using System.Diagnostics;

using Shopfolio.Infrastructure.Integrations;

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
/// (<see cref="PaymentStartResult.FromFailure"/>). The page model that cancels, keeps the card "unavailable" for two minutes and counts the
/// token requests' error run is UC-29's (T404) and calls this rule first.
/// </summary>
internal static class PaymentStartRule
{
    public static PaymentStartStep Decide(PaymentStartResult answer)
    {
        ArgumentNullException.ThrowIfNull(answer);
        if (answer is PaymentStartResult.Started)
        {
            return PaymentStartStep.OpenPaymentForm;
        }

        if (answer is PaymentStartResult.ConnectionFailed)
        {
            return PaymentStartStep.CancelAsUnavailable;
        }

        if (answer is PaymentStartResult.SecondTokenRejected)
        {
            return PaymentStartStep.StayOnConfirmation;
        }

        throw new UnreachableException();
    }
}
