namespace Shopfolio.Infrastructure.Jobs;

/// <summary>The nightly job's clean-up of the job queue (05 4.3.4; 09 13.1 item 9): the completed rows go, the failed rows stay for the panel.</summary>
internal sealed class JobQueueCleanupStep : INightlyStep
{
    public string Name => "JobQueueCleanup";

    public async Task RunAsync(IServiceProvider services, DateTimeOffset now, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(services);
        AppDbContext db = services.GetRequiredService<AppDbContext>();
        await JobCleanup.DeleteCompletedAsync(db, ct);
    }
}
