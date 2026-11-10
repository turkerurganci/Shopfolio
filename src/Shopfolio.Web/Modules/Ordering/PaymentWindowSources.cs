using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Jobs;
using Shopfolio.Infrastructure.Values;
using Shopfolio.Modules.Notifications.Contracts;
using Shopfolio.Modules.Ordering.Contracts;

namespace Shopfolio.Modules.Ordering;

/// <summary>
/// Z-8 (03 4.1.8; 02 6.1.2; 05 5.4.3): the bank transfer order whose payment window ended unpaid is cancelled by the system - S3 with Ö2, B-7 and the
/// release of its reservation, all in the transition unit's cancellation (<see cref="IOrderTransitions.CancelOrderAsync"/>). The source waits for a
/// fresh heartbeat: a window that ran out while the site was down is put off on the return (<see cref="TransferWindowDeferral"/>), not cancelled.
/// </summary>
internal sealed class TransferPaymentExpiry : ScheduledOrderTransition
{
    public override string Name => "TransferPaymentExpiry";

    public override bool WaitsForFreshHeartbeat => true;

    protected override bool IsStatusTransition => true;

    public override async Task<IReadOnlyList<DueRecord>> FindDueAsync(IServiceProvider services, DateTimeOffset now, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(services);
        return await services.GetRequiredService<AppDbContext>().Orders.AsNoTracking()
            .Where(PaymentWindowRules.TransferWindowEnded(now))
            .OrderBy(o => o.PaymentWindowEndsAt).ThenBy(o => o.Id)
            .Select(o => new DueRecord(o.Id, o.PaymentWindowEndsAt))
            .ToListAsync(ct);
    }

    protected override bool IsDue(Order order, DateTimeOffset now) => PaymentWindowRules.TransferWindowEnded(now).Compile()(order);

    protected override async Task Apply(IServiceProvider services, Order order, DateTimeOffset now, CancellationToken ct)
    {
        OrderTransitionResult result = await services.GetRequiredService<IOrderTransitions>()
            .CancelOrderAsync(CancelOrderRequest.BySystem(order.Id, TransitionReason.PaymentWindowExpired), ct);
        if (result.IsRejected)
        {
            // The order was asked again under its lock; a refusal here is a broken rule, not a race - the record stays due and the log tells it.
            throw new InvalidOperationException($"The system's cancellation of the expired transfer order {order.Id} was refused: {result.Refusal}.");
        }
    }
}

/// <summary>
/// Z-9 (03 4.1.9; 02 3.21.6; K-685; D-100): B-3 goes when the order's reminder moment has come and its window still runs; the moment is then emptied,
/// so the reminder goes once. A correction of the "paid" mark writes a new moment for the new window (T319), and the reminder goes once more. A
/// reminder missed in an outage goes on the return only while the window runs - also on the wake-up before the return's first heartbeat, when the
/// deferral has not yet emptied it (05 5.4.3).
/// </summary>
internal sealed class TransferPaymentReminder : ScheduledOrderTransition
{
    public override string Name => "TransferPaymentReminder";

    public override bool WaitsForFreshHeartbeat => false;

    public override async Task<IReadOnlyList<DueRecord>> FindDueAsync(IServiceProvider services, DateTimeOffset now, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(services);
        return await services.GetRequiredService<AppDbContext>().Orders.AsNoTracking()
            .Where(PaymentWindowRules.TransferReminderDue(now))
            .OrderBy(o => o.PaymentReminderAt).ThenBy(o => o.Id)
            .Select(o => new DueRecord(o.Id, o.PaymentReminderAt!.Value))
            .ToListAsync(ct);
    }

    protected override bool IsDue(Order order, DateTimeOffset now) => PaymentWindowRules.TransferReminderDue(now).Compile()(order);

    protected override async Task Apply(IServiceProvider services, Order order, DateTimeOffset now, CancellationToken ct)
    {
        if (!string.IsNullOrWhiteSpace(order.ContactEmail))
        {
            await services.GetRequiredService<INotificationsModule>()
                .QueueCustomerNoticeAsync(new TransferReminderNotice(order.Id, order.PaymentWindowEndsAt), order.ContactEmail, ct);
        }

        order.PaymentReminderAt = null;
    }
}

/// <summary>
/// Z-7 (03 4.1.7; 02 6.1.2; 08 2.1.6): when the card order's window ends unpaid, the final query is put in the job queue - one pending row per order
/// (06 4.59) - and its handler asks the provider, applies Ö1 or cancels. The order itself is not changed here, so its version stays; while the query
/// has no answer the order carries the "payment outcome unknown" mark (<see cref="PaymentWindowRules.IsPaymentOutcomeUnknown"/>) and stays due, so a
/// row the runner gave up on is written again at the next wake-up: the query never stops until the provider answers (03 4.1.7).
/// </summary>
internal sealed class CardPaymentExpiry : IDueRecordSource
{
    public string Name => "CardPaymentExpiry";

    public bool WaitsForFreshHeartbeat => false;

    public async Task<IReadOnlyList<DueRecord>> FindDueAsync(IServiceProvider services, DateTimeOffset now, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(services);
        return await services.GetRequiredService<AppDbContext>().Orders.AsNoTracking()
            .Where(PaymentWindowRules.CardWindowEnded(now))
            .OrderBy(o => o.PaymentWindowEndsAt).ThenBy(o => o.Id)
            .Select(o => new DueRecord(o.Id, o.PaymentWindowEndsAt))
            .ToListAsync(ct);
    }

    public async Task ProcessAsync(IServiceProvider services, Guid recordId, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(services);
        AppDbContext db = services.GetRequiredService<AppDbContext>();
        IJobQueue queue = services.GetRequiredService<IJobQueue>();
        DateTimeOffset now = services.GetRequiredService<TimeProvider>().GetUtcNow();
        await services.GetRequiredService<ITransactionRunner>().RunAsync(
            async cancel =>
            {
                // A payment or a cancellation that came in meanwhile ends it; the row is written once (a pending row of the same order is the same row).
                if (await db.Orders.AsNoTracking().Where(o => o.Id == recordId).AnyAsync(PaymentWindowRules.CardWindowEnded(now), cancel))
                {
                    await queue.EnqueueAsync(JobType.FinalPaymentQuery, recordId, null, cancel);
                }

                return new SchedulerStepResult();
            },
            ct);
    }
}
