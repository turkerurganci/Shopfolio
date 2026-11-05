// Deliberate violation sample of the job source scan (09 13.1 items 1, 2 and 10): background work outside its one place.
// The scan reads this file as text; it is not compiled. Line numbers are part of the test.
internal sealed class StrayService : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken stoppingToken) => Task.Run(() => { }, stoppingToken);
    public void Configure(HostOptions options) => options.BackgroundServiceExceptionBehavior = BackgroundServiceExceptionBehavior.Ignore;
    public void Start() => new Thread(() => { }).Start();
    public Task Offload() => Task.Factory.StartNew(() => { });
    public async Task Swallow(CancellationToken ct)
    {
        try { await Task.Delay(1, ct); }
        catch (Exception) { }
        try { await Task.Delay(1, ct); }
        catch (Exception exception) when (!ct.IsCancellationRequested) { _ = exception; }
        try { await Task.Delay(1, ct); }
        catch { }
        try { await Task.Delay(1, ct); }
        catch when (ct.IsCancellationRequested) { }
    }
    public void Touch(AppDbContext db)
    {
        db.Jobs.Add(new Job());
        db.Set<Job>();
        DbSet<Job> set = db.Jobs;
        Job.Enqueue(JobType.CardRefund, null, null, DateTimeOffset.UnixEpoch);
    }

    // Not findings: a comment that says catch (Exception) and Task.Run(, a specific catch, a service that only mentions the base type.
    public async Task Specific(CancellationToken ct)
    {
        try { await Task.Delay(1, ct); }
        catch (OperationCanceledException) { }
        string note = "BackgroundService";
        _ = note;
    }
}
