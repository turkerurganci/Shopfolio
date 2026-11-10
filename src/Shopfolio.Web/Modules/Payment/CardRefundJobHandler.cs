using System.Diagnostics;

using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Integrations;
using Shopfolio.Infrastructure.Jobs;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Modules.Payment.Contracts;

namespace Shopfolio.Modules.Payment;

/// <summary>
/// The handler of the card refund's job row (T405; 08 2.1.9; 05 4.3.3, 4.6.1, 4.6.2; MK-40). The runner gives it no transaction; it decides from the refund record
/// (<see cref="CardRefundJobRule"/>), so a row seen again never sends twice:
/// <list type="bullet">
/// <item>no record of this row - the record is written in the "sending" state under the order's lock in a transaction of its own and committed
/// (<see cref="IOrderTransitions.BeginCardRefundRequestAsync"/>; nothing when the refund is voided, turned to the transfer, sent or has an open request - D-518,
/// D-908), then the request goes once, with no lock held, and its answer is applied in another transaction by the one function of both outcome paths
/// (<see cref="IOrderTransitions.ApplyCardRefundOutcomeAsync"/>): accepted - succeeded, refused or not sent - failed, unknown - pending, which writes the query's row;</item>
/// <item>a "sending" record of this row - the request went or may have gone: it is not sent again, the record turns pending and is asked;</item>
/// <item>a pending or closed record - nothing to do.</item>
/// </list>
/// Without the payment gateway in the host (the command process, a test host of other modules) the row fails by the code error rule before anything is written.
/// </summary>
internal sealed class CardRefundJobHandler : IJobHandler
{
    private readonly AppDbContext _db;
    private readonly IOrderTransitions _transitions;
    private readonly ITransactionRunner _runner;
    private readonly IPaymentGateway? _gateway;

    public CardRefundJobHandler(AppDbContext db, IOrderTransitions transitions, ITransactionRunner runner, IPaymentGateway? gateway = null)
    {
        _db = db;
        _transitions = transitions;
        _runner = runner;
        _gateway = gateway;
    }

    public async Task<JobOutcome> HandleAsync(JobWork job, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(job);
        if (job.RelatedRecordId is not { } refundId)
        {
            return JobOutcome.Completed;
        }

        Guid? orderId = await _db.Refunds.AsNoTracking().Where(r => r.Id == refundId).Select(r => (Guid?)r.OrderId).SingleOrDefaultAsync(ct);
        if (orderId is not { } order)
        {
            return JobOutcome.Completed;
        }

        CardRefundJobRule.RowRecord? latest = await CardRefundJobRule.ReadLatestRecordAsync(_db, refundId, job.CreatedAt, ct);
        switch (CardRefundJobRule.Decide(latest?.State))
        {
            case CardRefundStep.Complete:
                return JobOutcome.Completed;
            case CardRefundStep.AskOutcome:
                if (latest is { State: RefundRecordState.Sending } sending)
                {
                    // The row came back after its record: the request is not sent again, its outcome is unknown and is asked (05 4.3.3; 08 3.1 rule 3).
                    await _runner.RunAsync(cancel => _transitions.ApplyCardRefundOutcomeAsync(new CardRefundOutcome(order, sending.Id, RefundRecordState.OutcomePending), cancel), ct);
                }

                return JobOutcome.Completed;
            case CardRefundStep.SendRequest:
                return await SendAsync(order, refundId, job.CreatedAt, ct);
            default:
                throw new UnreachableException();
        }
    }

    private async Task<JobOutcome> SendAsync(Guid orderId, Guid refundId, DateTimeOffset jobCreatedAt, CancellationToken ct)
    {
        IPaymentGateway gateway = _gateway ?? throw CardRefundJobRule.CodeError("no-gateway");
        CardRefundRequestStart start = await _runner.RunAsync(
            cancel => _transitions.BeginCardRefundRequestAsync(new CardRefundRequestBegin(orderId, refundId, jobCreatedAt), cancel),
            ct);
        if (start is not { IsStarted: true, OrderNumber: { } orderNumber })
        {
            return JobOutcome.Completed;
        }

        // The record is committed: only now the request goes, once (MK-40). The layer limits it to the job call's 30 seconds and retries nothing.
        RefundRequestResult answer = await gateway.RequestRefundAsync(new CardRefund(orderNumber, start.Amount, start.RecordId), ct);

        // A known answer is applied even while the host stops: a refusal lost here would stay pending, which the query never closes (the provider does not list it).
        RefundRecordState outcome = CardRefundJobRule.OutcomeOf(answer);
        await _runner.RunAsync(
            cancel => _transitions.ApplyCardRefundOutcomeAsync(new CardRefundOutcome(orderId, start.RecordId, outcome), cancel),
            CancellationToken.None);
        return JobOutcome.Completed;
    }
}
