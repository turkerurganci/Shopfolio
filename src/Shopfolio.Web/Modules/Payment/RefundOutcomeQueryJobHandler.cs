using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Integrations;
using Shopfolio.Infrastructure.Jobs;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Modules.Payment.Contracts;

namespace Shopfolio.Modules.Payment;

/// <summary>
/// The handler of the query of a card refund whose outcome is unknown (T405; 08 2.1.9; 05 4.3.4, 4.6.2; K-1238 (3)): the row points at the pending refund record and
/// is written with its "pending" state. The provider is asked with no transaction open; "completed" - the item with the record's reference and the refund's amount - is
/// applied by the one function of both outcome paths (<see cref="IOrderTransitions.ApplyCardRefundOutcomeAsync"/>), which keeps a success that comes after the firm's
/// mark on the record without applying it; "no result" waits <see cref="RefundOutcomeQueryRule.NextDelay"/> and asks again, with no upper limit. A record that is no
/// longer pending - the firm marked it failed, or a result was applied - completes the row with no query: after the mark nothing is asked.
/// </summary>
internal sealed class RefundOutcomeQueryJobHandler : IJobHandler
{
    private readonly AppDbContext _db;
    private readonly IOrderingModule _ordering;
    private readonly IOrderTransitions _transitions;
    private readonly ITransactionRunner _runner;
    private readonly TimeProvider _time;
    private readonly IPaymentGateway? _gateway;

    public RefundOutcomeQueryJobHandler(
        AppDbContext db,
        IOrderingModule ordering,
        IOrderTransitions transitions,
        ITransactionRunner runner,
        TimeProvider time,
        IPaymentGateway? gateway = null)
    {
        _db = db;
        _ordering = ordering;
        _transitions = transitions;
        _runner = runner;
        _time = time;
        _gateway = gateway;
    }

    public async Task<JobOutcome> HandleAsync(JobWork job, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(job);
        if (job.RelatedRecordId is not { } recordId)
        {
            return JobOutcome.Completed;
        }

        QueryTarget? target = await _db.RefundRecords.AsNoTracking()
            .Where(r => r.Id == recordId)
            .Join(_db.Refunds.AsNoTracking(), r => r.RefundId, refund => refund.Id, (r, refund) => new QueryTarget(r.State, refund.OrderId, refund.Amount))
            .SingleOrDefaultAsync(ct);
        if (target is not { State: RefundRecordState.OutcomePending })
        {
            return JobOutcome.Completed;
        }

        string? orderNumber = await _ordering.GetOrderNumberAsync(target.OrderId, ct);
        if (orderNumber is null)
        {
            return JobOutcome.Completed;
        }

        IPaymentGateway gateway = _gateway ?? throw CardRefundJobRule.CodeError("no-gateway");
        RefundQueryResult answer = await gateway.QueryRefundAsync(new CardRefund(orderNumber, target.Amount, recordId), ct);
        if (answer is RefundQueryResult.Completed)
        {
            // A known answer is applied even while the host stops; a second application of it changes nothing (the record is no longer open).
            await _runner.RunAsync(
                cancel => _transitions.ApplyCardRefundOutcomeAsync(new CardRefundOutcome(target.OrderId, recordId, RefundRecordState.Succeeded), cancel),
                CancellationToken.None);
            return JobOutcome.Completed;
        }

        // No result: the firm's mark may have come while the provider was asked; then the query stops here.
        bool isPending = await _db.RefundRecords.AsNoTracking().AnyAsync(r => r.Id == recordId && r.State == RefundRecordState.OutcomePending, ct);
        return isPending ? JobOutcome.RescheduleAfter(RefundOutcomeQueryRule.NextDelay(job.CreatedAt, _time.GetUtcNow())) : JobOutcome.Completed;
    }

    /// <summary>The pending record's state, its refund's order and amount.</summary>
    private sealed record QueryTarget(RefundRecordState State, Guid OrderId, decimal Amount);
}
