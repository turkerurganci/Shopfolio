using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Integrations;
using Shopfolio.Infrastructure.Jobs;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Modules.Payment.Contracts;

namespace Shopfolio.Modules.Payment;

/// <summary>
/// The processing of an inbound payment notification (05 4.5.2, 4.5.3, 4.3.3; 07 4.30.1 side effects; 09 13.1 item 4): a job that only writes to
/// the database, so the runner's transaction holds the effect and the row's completion together. The outcome goes to the state machine by its one
/// path - a success to <see cref="IOrderTransitions.ApplyCardPaymentAsync"/>, the same function the final query calls; a failure to the system's
/// cancellation of the unpaid order with the card failure's reason (S3 with Ö2; 02 6.2.1; 08 2.1.3) -, which takes the order's lock and applies the
/// outcome to the state the order is in at that moment: a payment to a cancelled order is recorded and paid back (02 6.2.16; T317), a failure on an
/// order that is no longer unpaid changes nothing. The handler writes the record's processing moment; a row that comes back finds the moment written
/// and does nothing again (05 4.3.3; K-970).
/// </summary>
internal sealed class InboundNotificationJobHandler : ITransactionalJobHandler
{
    private readonly AppDbContext _db;
    private readonly IOrderTransitions _transitions;
    private readonly TimeProvider _time;
    private readonly IPaymentGateway? _gateway;

    /// <summary>The gateway is the provider task's registration (<see cref="PaymentNotificationEndpoint"/>); a job row exists only for a notification it verified.</summary>
    public InboundNotificationJobHandler(AppDbContext db, IOrderTransitions transitions, TimeProvider time, IPaymentGateway? gateway = null)
    {
        _db = db;
        _transitions = transitions;
        _time = time;
        _gateway = gateway;
    }

    public async Task HandleAsync(JobWork job, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(job);
        if (job.RelatedRecordId is not Guid recordId)
        {
            throw new InvalidOperationException("A notification's job row points at its inbound record.");
        }

        InboundNotificationRecord? record = await _db.InboundNotificationRecords.SingleOrDefaultAsync(n => n.Id == recordId, ct);
        if (record is null || record.ProcessedAt is not null)
        {
            return;
        }

        // The endpoint writes a job row only for a notification of a found card order with a transaction id (PaymentNotificationRule).
        if (record.OrderId is not Guid orderId || record.ProviderTransactionId is not { } transactionId)
        {
            throw new InvalidOperationException("A processed notification names its order and its transaction.");
        }

        IPaymentGateway gateway = _gateway ?? throw new InvalidOperationException("A notification is read only by the payment gateway.");
        switch (gateway.ReadNotificationResult(record.EventType, record.Outcome))
        {
            case NotificationResult.PaymentSucceeded:
                await _transitions.ApplyCardPaymentAsync(new CardPaymentSucceeded(orderId, transactionId), ct);
                break;

            case NotificationResult.PaymentFailed:
                await _transitions.CancelOrderAsync(CancelOrderRequest.BySystem(orderId, TransitionReason.CardPaymentFailed), ct);
                break;

            default:
                record.UnprocessedReason = UnprocessedNotificationReason.UnrecognizedNotification;
                break;
        }

        record.ProcessedAt = _time.GetUtcNow();
    }
}
