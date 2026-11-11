using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Integrations;
using Shopfolio.Infrastructure.Jobs;
using Shopfolio.Infrastructure.Values;
using Shopfolio.Modules.Notifications.Contracts;

namespace Shopfolio.Modules.Notifications;

/// <summary>
/// The handler of the returned-address query (08 2.2.3; 05 4.3.4, 5.2.16; K-1241): every fifteen minutes the job reads the service's account-level list of the permanently
/// bounced addresses of the last 24 hours and marks every "sent" row of the same window whose recipient is on it "undelivered" with the last outcome "recipient error"; the sending
/// moment stays. The match is case-free through the one identity normalization (06 2.8.1): the list holds an address as it was written, the row holds it as it was entered. The
/// channel state is not touched - a bounce is not an attempt - and a row that is already "undelivered" is not touched either, so a second application changes nothing.
/// The product never deletes an address from the service's list and has no call that could (the account's list is cleared by hand in the console).
/// <para>
/// A failed round passes empty: nothing is written, no cursor is kept and the row is not failed - the service call has written its failure to the application log
/// (<see cref="ExternalCallRunner"/>), and the next round asks again from 24 hours before its own start, so the window catches the round that was missed. Every round, whatever its
/// outcome, leaves the one pending row to be taken again after the interval (<see cref="JobOutcome.RescheduleAfter"/>). The service is called with no transaction open.
/// </para>
/// </summary>
internal sealed class BounceQueryJobHandler : IJobHandler
{
    private readonly IServiceProvider _services;
    private readonly AppDbContext _db;
    private readonly ITransactionRunner _runner;
    private readonly TimeProvider _time;

    // The gateway is optional (it is T407's and a host may run without one, 09 4.3): it is asked of the container at each round, not taken by the constructor, so the
    // application opens without a gateway; the row source keeps no row while none is registered, and a round that finds none only leaves the row to be taken again.
    public BounceQueryJobHandler(IServiceProvider services, AppDbContext db, ITransactionRunner runner, TimeProvider time)
    {
        _services = services;
        _db = db;
        _runner = runner;
        _time = time;
    }

    public async Task<JobOutcome> HandleAsync(JobWork job, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(job);
        if (_services.GetService<IEmailGateway>() is not { } gateway)
        {
            return JobOutcome.RescheduleAfter(JobTimings.BounceQueryInterval);
        }

        DateTimeOffset windowStart = _time.GetUtcNow() - JobTimings.BounceQueryWindow;
        BounceQueryResult answer = await gateway.QueryBouncesAsync(windowStart, ct);
        if (answer is BounceQueryResult.List list && list.Addresses.Count > 0)
        {
            HashSet<string> bounced = [.. list.Addresses.Select(TextNormalizer.NormalizeIdentity)];
            await _runner.RunAsync(cancel => MarkAsync(bounced, windowStart, cancel), ct);
        }

        return JobOutcome.RescheduleAfter(JobTimings.BounceQueryInterval);
    }

    private async Task<SchedulerStepResult> MarkAsync(HashSet<string> bounced, DateTimeOffset windowStart, CancellationToken ct)
    {
        // The rows of the window are read as a stream (the window holds a day of sending) and only the ids of the matching ones are kept.
        List<Guid> matching = [];
        await foreach (var row in _db.DispatchRecords.AsNoTracking()
            .Where(d => d.DispatchState == DispatchState.Sent && d.SentAt >= windowStart && d.RecipientEmail != null)
            .Select(d => new { d.Id, d.RecipientEmail })
            .AsAsyncEnumerable()
            .WithCancellation(ct))
        {
            if (bounced.Contains(TextNormalizer.NormalizeIdentity(row.RecipientEmail!))) // non-null: the Where above keeps only records with a recipient
            {
                matching.Add(row.Id);
            }
        }

        if (matching.Count > 0)
        {
            // The state is asked again in the statement: a row that changed meanwhile is not written a second time.
            await _db.DispatchRecords
                .Where(d => matching.Contains(d.Id) && d.DispatchState == DispatchState.Sent)
                .ExecuteUpdateAsync(
                    set => set
                        .SetProperty(d => d.DispatchState, DispatchState.Undelivered)
                        .SetProperty(d => d.LastOutcome, DispatchOutcome.RecipientError),
                    ct);
        }

        return new SchedulerStepResult();
    }
}
