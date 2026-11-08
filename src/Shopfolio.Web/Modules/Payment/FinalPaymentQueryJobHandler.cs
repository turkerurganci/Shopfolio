using System.Diagnostics;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Integrations;
using Shopfolio.Infrastructure.Jobs;
using Shopfolio.Modules.Ordering.Contracts;

namespace Shopfolio.Modules.Payment;

/// <summary>
/// The handler of the card payment's final query (03 4.1.7; 02 6.1.2; 08 2.1.6, 3.1 rules 3 and 4; 05 4.5.3, 5.3.3; T324 criteria 4 and 5): the row
/// is written when the card order's window ends (<c>CardPaymentExpiry</c>). The runner gives it no transaction, so the provider is asked first, with
/// no lock held (05 3.6.3); the answer is read by <see cref="FinalPaymentQueryRule"/> and applied in a transaction of its own:
/// a payment goes to the one function that applies a card payment (Ö1, <see cref="IOrderTransitions.ApplyCardPaymentAsync"/>); "no payment" cancels
/// the unpaid order by the window's rule - S3 with Ö2, B-7 and the reservation's release (<see cref="IOrderTransitions.CancelOrderAsync"/>);
/// no answer leaves the order as it is - it carries the "payment outcome unknown" mark - and the row is asked again after the rule's interval,
/// as long as the provider does not answer. An order that is no longer a waiting card order - paid by the notification, cancelled - completes the row
/// with nothing done; a "no payment" before the window's end cancels nothing (the window's end writes the row again).
/// </summary>
internal sealed class FinalPaymentQueryJobHandler : IJobHandler
{
    private readonly IPaymentGateway _gateway;
    private readonly IOrderingModule _ordering;
    private readonly IOrderTransitions _transitions;
    private readonly ITransactionRunner _runner;
    private readonly TimeProvider _time;

    public FinalPaymentQueryJobHandler(
        IPaymentGateway gateway,
        IOrderingModule ordering,
        IOrderTransitions transitions,
        ITransactionRunner runner,
        TimeProvider time)
    {
        _gateway = gateway;
        _ordering = ordering;
        _transitions = transitions;
        _runner = runner;
        _time = time;
    }

    public async Task<JobOutcome> HandleAsync(JobWork job, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(job);
        if (job.RelatedRecordId is not { } orderId)
        {
            return JobOutcome.Completed;
        }

        CardPaymentQueryTarget? target = await _ordering.GetCardPaymentQueryTargetAsync(orderId, _time.GetUtcNow(), ct);
        if (target is null)
        {
            return JobOutcome.Completed;
        }

        PaymentQueryResult answer = await _gateway.QueryPaymentAsync(target.OrderNumber, ct);
        switch (FinalPaymentQueryRule.Decide(answer))
        {
            case FinalPaymentQueryStep.ApplyPayment:
                PaymentQueryResult.Paid paid = (PaymentQueryResult.Paid)answer;
                await _runner.RunAsync(cancel => _transitions.ApplyCardPaymentAsync(new CardPaymentSucceeded(orderId, paid.ProviderTransactionId), cancel), ct);
                return JobOutcome.Completed;
            case FinalPaymentQueryStep.CancelUnpaidOrder:
                if (target.WindowEnded)
                {
                    // A refusal means the order changed after the read (a payment came in): the runner rolls nothing forward and the row is done.
                    await _runner.RunAsync(
                        cancel => _transitions.CancelOrderAsync(CancelOrderRequest.BySystem(orderId, TransitionReason.PaymentWindowExpired), cancel),
                        ct);
                }

                return JobOutcome.Completed;
            case FinalPaymentQueryStep.AskAgain:
                return FinalPaymentQueryRule.AskAgainOutcome();
            default:
                throw new UnreachableException();
        }
    }
}
