using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Jobs;
using Shopfolio.Infrastructure.Values;

namespace Shopfolio.Modules.Notifications;

/// <summary>
/// The writer of the returned-address query's one row (06 4.59: the type's single pending row; 05 5.1.4). The row is not made by a transition, so the scheduler makes sure it exists:
/// at each wake-up, while an e-mail gateway is registered and no pending row of the type is there, the source asks for one and writes it - a closed or never written row comes
/// back within a minute. The write is the queue's own insert, which leaves an existing pending row as it is, so two wake-ups or two instances write it once. While no gateway
/// is registered there is nothing to ask and no row is written.
/// </summary>
internal sealed class BounceQueryRowSource : IDueRecordSource
{
    public string Name => "bounce-query-row";

    public bool WaitsForFreshHeartbeat => false;

    public async Task<IReadOnlyList<DueRecord>> FindDueAsync(IServiceProvider services, DateTimeOffset now, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(services);
        if (!DispatchAttempts.GatewayIsRegistered(services))
        {
            return [];
        }

        bool pending = await services.GetRequiredService<AppDbContext>().Jobs.AsNoTracking()
            .AnyAsync(j => j.JobType == JobType.BounceQuery && j.JobState == JobState.Pending, ct);
        return pending ? [] : [new DueRecord(Guid.Empty, now)];
    }

    public async Task ProcessAsync(IServiceProvider services, Guid recordId, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(services);
        IJobQueue queue = services.GetRequiredService<IJobQueue>();
        await services.GetRequiredService<ITransactionRunner>().RunAsync(
            async cancel =>
            {
                await queue.EnqueueAsync(JobType.BounceQuery, null, null, cancel);
                return new SchedulerStepResult();
            },
            ct);
    }
}
