using System.Diagnostics;

using Shopfolio.Infrastructure.Integrations;
using Shopfolio.Infrastructure.Jobs;

namespace Shopfolio.Modules.Payment;

/// <summary>What the final query's handler does with the answer of the payment's outcome (08 2.1.6, 3.1 rules 3 and 4, 3.3; 05 5.3.3).</summary>
internal enum FinalPaymentQueryStep
{
    /// <summary>A successful payment exists: hand it to the one function that applies a payment's result (05 4.5.3), which checks it first.</summary>
    ApplyPayment,

    /// <summary>The provider says there is no payment: the unpaid order is cancelled by the payment period's rule.</summary>
    CancelUnpaidOrder,

    /// <summary>No answer - the result is unknown: the order is not touched (it stays received and waiting) and the query is asked again.</summary>
    AskAgain,
}

/// <summary>
/// The core's rule over the final query's answer (08 3.1 rule 4; 3.3 "Ödeme süresi işler, iptal ertelenir"): the layer only tells the
/// unanswered call apart (<see cref="PaymentQueryResult.NoResponse"/>, the timeout, the unreachable provider and the unlisted answer included);
/// an unanswered query is not a "no payment" - the order is not cancelled and the query repeats every five minutes while the provider is down
/// (<see cref="JobTimings.FinalPaymentQueryInterval"/>). The payment period is not stopped by the outage; only its cancellation waits. The
/// handler that reads the order, applies the result and cancels is <see cref="FinalPaymentQueryJobHandler"/> (T324) and calls this rule first; a
/// successful answer that cannot be applied, and the code errors' own interval, are the payment provider task's rules (T403; 08 2.1.6).
/// </summary>
internal static class FinalPaymentQueryRule
{
    public static FinalPaymentQueryStep Decide(PaymentQueryResult answer)
    {
        ArgumentNullException.ThrowIfNull(answer);
        return answer switch
        {
            PaymentQueryResult.Paid => FinalPaymentQueryStep.ApplyPayment,
            PaymentQueryResult.NoPayment => FinalPaymentQueryStep.CancelUnpaidOrder,
            PaymentQueryResult.NoResponse => FinalPaymentQueryStep.AskAgain,
            _ => throw new UnreachableException(),
        };
    }

    /// <summary>What the handler tells the runner after an unanswered query: the row stays and is asked again after five minutes, its attempt count unchanged.</summary>
    public static JobOutcome AskAgainOutcome() => JobOutcome.RescheduleAfter(JobTimings.FinalPaymentQueryInterval);
}
