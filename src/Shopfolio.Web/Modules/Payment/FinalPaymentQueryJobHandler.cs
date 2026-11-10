using System.Diagnostics;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Integrations;
using Shopfolio.Infrastructure.Jobs;
using Shopfolio.Infrastructure.Logging;
using Shopfolio.Modules.Ordering.Contracts;

namespace Shopfolio.Modules.Payment;

/// <summary>
/// The handler of the card payment's final query (03 4.1.7; 02 6.1.2; 08 2.1.6, 3.1 rules 3 and 4; 05 4.5.3, 5.3.3; T324 criteria 4 and 5; T403): the row
/// is written when the card order's window ends (<c>CardPaymentExpiry</c>). The runner gives it no transaction, so the provider is asked first, with
/// no lock held (05 3.6.3); the answer is read by <see cref="FinalPaymentQueryRule"/> and applied in a transaction of its own:
/// a payment whose five conditions hold goes to the one function that applies a card payment (Ö1, <see cref="IOrderTransitions.ApplyCardPaymentAsync"/>) - the
/// payment record's provider id makes a second application of the same payment a repeat (08 2.1.7); "no payment" cancels the unpaid order by the window's rule -
/// S3 with Ö2, B-7 and the reservation's release (<see cref="IOrderTransitions.CancelOrderAsync"/>); no answer leaves the order as it is - it carries the
/// "payment outcome unknown" mark - and the row is asked again after five minutes as long as the provider does not answer; an answer that waiting does not change -
/// the request errors, an unrecognized amount, a successful answer that is not applied (written to the application log) - leaves the order as it is too and the
/// row repeats by the code error rule, then fails and opens the status endpoint's warning (08 2.1.6; 05 4.3.4, 9.4.16). An order that is no longer a waiting card
/// order - paid by the notification, cancelled - completes the row with nothing done; a "no payment" before the window's end cancels nothing (the window's end writes
/// the row again).
/// </summary>
internal sealed partial class FinalPaymentQueryJobHandler : IJobHandler
{
    private readonly IOrderingModule _ordering;
    private readonly IOrderTransitions _transitions;
    private readonly ITransactionRunner _runner;
    private readonly TimeProvider _time;
    private readonly ILogger<FinalPaymentQueryJobHandler> _logger;
    private readonly IPaymentGateway? _gateway;
    private readonly bool _isLiveInstallation;

    /// <summary>
    /// The gateway is the payment provider's registration (<c>AddIntegrations</c>); a host without it - the command process, a test host of other modules - still
    /// builds, and a row taken there fails by the code error rule. The live installation is the production environment, the same sense the notification's test mark
    /// is checked against (<see cref="PaymentNotificationEndpoint"/>).
    /// </summary>
    public FinalPaymentQueryJobHandler(
        IOrderingModule ordering,
        IOrderTransitions transitions,
        ITransactionRunner runner,
        TimeProvider time,
        ILogger<FinalPaymentQueryJobHandler> logger,
        IPaymentGateway? gateway = null,
        IHostEnvironment? environment = null)
    {
        _ordering = ordering;
        _transitions = transitions;
        _runner = runner;
        _time = time;
        _logger = logger;
        _gateway = gateway;
        _isLiveInstallation = environment?.IsProduction() ?? false;
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

        IPaymentGateway gateway = _gateway ?? throw FinalPaymentQueryRule.CodeError("no-gateway");
        PaymentQueryResult answer = await gateway.QueryPaymentAsync(target.OrderNumber, ct);
        switch (FinalPaymentQueryRule.Decide(answer))
        {
            case FinalPaymentQueryStep.ApplyPayment:
                PaymentQueryResult.Paid paid = (PaymentQueryResult.Paid)answer;
                if (FinalPaymentQueryRule.NotAppliedReason(paid, target.Total, _isLiveInstallation) is { } notApplied)
                {
                    LogNotApplied(_logger, LogEvents.FinalPaymentQueryNotApplied, notApplied, orderId);
                    throw FinalPaymentQueryRule.CodeError(notApplied);
                }

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
            case FinalPaymentQueryStep.RetryByCodeErrorRule:
                // The layer wrote its failure line with the reason code (ExternalCallFailed); the runner writes the row's failure and applies the default rule.
                throw FinalPaymentQueryRule.CodeError(((PaymentQueryResult.NoResponse)answer).ReasonCode);
            default:
                throw new UnreachableException();
        }
    }

    // Warning: a payment the provider reports may exist but is not applied; the installer reads it in the provider's panel and closes the order (05 8.5.12).
    [LoggerMessage(Level = LogLevel.Warning, Message = "{Event} {Reason} {OrderId}")]
    private static partial void LogNotApplied(ILogger logger, string @event, string reason, Guid orderId);
}
