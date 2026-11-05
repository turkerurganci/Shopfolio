using Shopfolio.Infrastructure.Jobs;

namespace Shopfolio.Infrastructure.Disposal;

/// <summary>
/// The disposal regulator (05 5.2.8, 5.2.9, 7.4.1; 09 13.1 item 9): the one step of the nightly job that runs the disposal parts and the
/// clean-up of the temporary records, one part after the other in the order they were registered. There is one job and one lock - the nightly
/// job's - and no timer of its own. The step reads no period: each part reads the time from its records against <see cref="DisposalRun.Now"/>, so
/// a night that was missed is made up at the next run and a second run on the same day changes nothing (05 7.4.1). A part's exception ends the
/// step and so the job as failed (09 13.1 item 9); what the earlier parts committed stays, and the next run starts again from the records.
/// </summary>
internal sealed class DisposalStep : INightlyStep
{
    public string Name => "Disposal";

    public async Task RunAsync(IServiceProvider services, DateTimeOffset now, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(services);
        DisposalRun run = new(services, now);
        foreach (IDisposalPart part in services.GetServices<IDisposalPart>())
        {
            ct.ThrowIfCancellationRequested();
            await part.RunAsync(run, ct);
        }
    }
}
