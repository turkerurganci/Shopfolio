using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Jobs;
using Shopfolio.Modules.AfterSales.Contracts;
using Shopfolio.Modules.Payment.Contracts;

namespace Shopfolio.Modules.Ordering;

/// <summary>
/// The refund IBAN's deletion day on a withdrawal whose goods did not come back (Z-38; 03 4.1.47, 4.2.8; 02 4.3; 05 5.2.5; K-1110, K-1127): when the day
/// comes the IBAN is deleted unless a refund still uses it - then it stays until that refund completes (that deletion is the refund line's, T370). The
/// deletion closes no item: the withdrawal stays valid. It is a scheduler source of its own whose record is the IBAN; it does not wait for a fresh
/// heartbeat (only the transfer order's cancellation does, 05 5.4.3), so a day that came during an outage is handled on the return in its turn
/// (03 4.2.11). Processing runs in one transaction that takes the order's row lock first (09 9.1 item 5), reads the IBAN's users again after it and
/// writes the outcome; the order's version is raised once for the record that changed (06 2.6.4).
/// </summary>
internal sealed class RefundIbanDeletionDay : IDueRecordSource
{
    public string Name => "refund-iban-deletion-day";

    public bool WaitsForFreshHeartbeat => false;

    public async Task<IReadOnlyList<DueRecord>> FindDueAsync(IServiceProvider services, DateTimeOffset now, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(services);
        IReadOnlyList<DueRefundIbanDeletion> due = await services.GetRequiredService<IAfterSalesModule>().FindDueRefundIbanDeletionsAsync(now, ct);
        return [.. due.Select(d => new DueRecord(d.RefundIbanId, d.DeletionAt))];
    }

    public async Task ProcessAsync(IServiceProvider services, Guid recordId, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(services);
        ITransactionRunner runner = services.GetRequiredService<ITransactionRunner>();
        IAfterSalesModule afterSales = services.GetRequiredService<IAfterSalesModule>();
        IPaymentModule payment = services.GetRequiredService<IPaymentModule>();
        OrderLocks locks = services.GetRequiredService<OrderLocks>();
        AppDbContext db = services.GetRequiredService<AppDbContext>();
        TimeProvider time = services.GetRequiredService<TimeProvider>();
        await runner.RunAsync(
            async cancel =>
            {
                RefundIbanUse? before = await afterSales.FindRefundIbanUseAsync(recordId, cancel);
                if (before is null)
                {
                    return new SchedulerStepResult();
                }

                await locks.LockAsync([before.OrderId], cancel);

                // Read again under the lock: a refund, a receipt or the closure may have come in between.
                RefundIbanUse? use = await afterSales.FindRefundIbanUseAsync(recordId, cancel);
                if (use?.DeletionAt is not { } day || day > time.GetUtcNow())
                {
                    return new SchedulerStepResult();
                }

                IReadOnlySet<Guid> settled = await payment.FindSettledRefundSubjectsAsync(use.SubjectIds, cancel);
                await afterSales.ApplyRefundIbanDeletionDayAsync(recordId, RefundIbanDeletionRule.ShouldDelete(use, settled), cancel);
                Order order = await db.Orders.SingleAsync(o => o.Id == use.OrderId, cancel);
                OrderTransitions.RaiseVersionOnce(db, order);
                return new SchedulerStepResult();
            },
            ct);
    }
}

/// <summary>
/// The rule of the deletion day (03 4.1.47, 4.2.8; K-1127): the IBAN goes when no refund still uses it - every subject linked to it (a cancellation, a late
/// delivery termination, a withdrawal's goods' receipt) has a refund that was sent or voided. A withdrawal whose goods did not arrive has no subject, so
/// alone it never keeps the IBAN.
/// </summary>
internal static class RefundIbanDeletionRule
{
    public static bool ShouldDelete(RefundIbanUse use, IReadOnlySet<Guid> settledSubjects)
    {
        ArgumentNullException.ThrowIfNull(use);
        ArgumentNullException.ThrowIfNull(settledSubjects);
        return use.SubjectIds.All(settledSubjects.Contains);
    }
}
