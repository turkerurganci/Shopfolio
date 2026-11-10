using System.Diagnostics;

using Shopfolio.Infrastructure.Integrations;
using Shopfolio.Infrastructure.Jobs;

namespace Shopfolio.Modules.Payment;

/// <summary>What the final query's handler does with the answer of the payment's outcome (08 2.1.6, 3.1 rules 3 and 4, 3.3; 05 5.3.3).</summary>
internal enum FinalPaymentQueryStep
{
    /// <summary>A successful payment exists: hand it to the one function that applies a payment's result (05 4.5.3) when <see cref="FinalPaymentQueryRule.NotAppliedReason"/> lets it.</summary>
    ApplyPayment,

    /// <summary>The provider says there is no payment: the unpaid order is cancelled by the payment period's rule.</summary>
    CancelUnpaidOrder,

    /// <summary>No answer - the result is unknown: the order is not touched (it stays received and waiting) and the query is asked again after five minutes.</summary>
    AskAgain,

    /// <summary>
    /// No answer that waiting does not change (08 2.1.6: <c>001</c>-<c>003</c>, an amount text that is not recognized): the order is not touched and the row is
    /// repeated by the job's code error rule (05 4.3.4 - 1, 5, 15, 60, 240 minutes, then failed and the status endpoint's warning).
    /// </summary>
    RetryByCodeErrorRule,
}

/// <summary>
/// The core's rule over the final query's answer (08 2.1.6, 3.1 rule 4; 3.3 "Ödeme süresi işler, iptal ertelenir"): the layer tells the two kinds of unanswered call
/// apart (<see cref="PaymentQueryResult.NoResponse.CodeError"/>); an unanswered query is not a "no payment" - the order is not cancelled. The outage, the timeout and
/// the unlisted answer repeat every five minutes (<see cref="JobTimings.FinalPaymentQueryInterval"/>); the request errors and an unrecognized amount repeat by the code
/// error rule (<see cref="JobRetryPolicy"/>): the handler throws <see cref="CodeError"/> and the runner applies the default retry rule. A successful answer is applied
/// only when its five conditions hold (<see cref="NotAppliedReason"/>); one that does not is handled as a code error too and written to the application log. The
/// payment period is not stopped by the outage; only its cancellation waits. The handler is <see cref="FinalPaymentQueryJobHandler"/> (T324, T403).
/// </summary>
internal static class FinalPaymentQueryRule
{
    /// <summary>The reason code of a successful answer whose amounts do not hold against the order's total (08 2.1.6).</summary>
    public const string AmountReason = "amount";

    /// <summary>The reason code of a successful answer whose payment type is not the card (08 2.1.6 <c>odeme_tipi</c>).</summary>
    public const string PaymentTypeReason = "payment-type";

    /// <summary>The reason code of a successful answer whose currency is not lira (08 2.1.6 <c>currency</c>).</summary>
    public const string CurrencyReason = "currency";

    /// <summary>The reason code of a test transaction's successful answer on the live installation (08 2.1.6 <c>test_mode</c>, 2.1.10).</summary>
    public const string TestModeReason = "test-mode";

    public static FinalPaymentQueryStep Decide(PaymentQueryResult answer)
    {
        ArgumentNullException.ThrowIfNull(answer);
        return answer switch
        {
            PaymentQueryResult.Paid => FinalPaymentQueryStep.ApplyPayment,
            PaymentQueryResult.NoPayment => FinalPaymentQueryStep.CancelUnpaidOrder,
            PaymentQueryResult.NoResponse { CodeError: true } => FinalPaymentQueryStep.RetryByCodeErrorRule,
            PaymentQueryResult.NoResponse => FinalPaymentQueryStep.AskAgain,
            _ => throw new UnreachableException(),
        };
    }

    /// <summary>
    /// Why a successful answer is not applied, or null when it is (08 2.1.6): <c>payment_amount</c> equals the order's total and <c>payment_total</c> is at least the
    /// same in whole kuruş (<see cref="PaymentAmounts.QueryMatches"/>), the payment is by card, in lira, and not a test transaction on the live installation. The
    /// conditions are checked in this order and the first that fails names the reason.
    /// </summary>
    public static string? NotAppliedReason(PaymentQueryResult.Paid paid, decimal orderTotal, bool isLiveInstallation)
    {
        ArgumentNullException.ThrowIfNull(paid);
        if (!PaymentAmounts.QueryMatches(paid.PaymentAmount, paid.PaymentTotal, orderTotal))
        {
            return AmountReason;
        }

        if (!paid.PaidByCard)
        {
            return PaymentTypeReason;
        }

        if (!paid.InLira)
        {
            return CurrencyReason;
        }

        return paid.TestMode && isLiveInstallation ? TestModeReason : null;
    }

    /// <summary>What the handler tells the runner after an unanswered query: the row stays and is asked again after five minutes, its attempt count unchanged.</summary>
    public static JobOutcome AskAgainOutcome() => JobOutcome.RescheduleAfter(JobTimings.FinalPaymentQueryInterval);

    /// <summary>
    /// The failure the handler throws for an answer that waiting does not change (09 13.1 item 4: a failure is not an outcome; the runner applies the default retry
    /// rule). The message carries the reason code only - no reference, no amount, no field of the answer.
    /// </summary>
    public static InvalidOperationException CodeError(string reasonCode) => new("The final payment query's answer is repeated by the code error rule: " + reasonCode);
}
