using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Jobs;

namespace Shopfolio.Modules.Ordering;

/// <summary>
/// The base of a due-record source whose record is an order (05 5.1.1, 7.3.2; 09 9.1 item 5, 13.1 item 6): the system's change on an order
/// that the order's own field says is due - the transfer order's cancellation, its reminder. Processing runs in one transaction that takes the
/// order's row lock first, loads the order after it, asks again whether the order is still due (a payment or an administrator's change may have
/// come in between) and only then applies the change. The order's version is raised by this base after <c>Apply</c>, so a
/// subclass cannot forget it.
/// </summary>
internal abstract class ScheduledOrderTransition : IDueRecordSource
{
    public abstract string Name { get; }

    public abstract bool WaitsForFreshHeartbeat { get; }

    public abstract Task<IReadOnlyList<DueRecord>> FindDueAsync(IServiceProvider services, DateTimeOffset now, int limit, CancellationToken ct);

    /// <summary>Whether the order, as it is now, is still due at <paramref name="now"/>.</summary>
    protected abstract bool IsDue(Order order, DateTimeOffset now);

    /// <summary>Makes the change on the locked, current order; the version is raised after it returns.</summary>
    protected abstract Task Apply(IServiceProvider services, Order order, DateTimeOffset now, CancellationToken ct);

    public async Task ProcessAsync(IServiceProvider services, Guid recordId, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(services);
        ITransactionRunner runner = services.GetRequiredService<ITransactionRunner>();
        AppDbContext db = services.GetRequiredService<AppDbContext>();
        OrderLocks locks = services.GetRequiredService<OrderLocks>();
        TimeProvider time = services.GetRequiredService<TimeProvider>();
        await runner.RunAsync(
            async cancel =>
            {
                await locks.LockAsync([recordId], cancel);
                Order? order = await db.Orders.SingleOrDefaultAsync(o => o.Id == recordId, cancel);
                DateTimeOffset now = time.GetUtcNow();
                if (order is not null && IsDue(order, now))
                {
                    await Apply(services, order, now, cancel);
                    order.BumpVersion();
                }

                return new SchedulerStepResult();
            },
            ct);
    }
}
