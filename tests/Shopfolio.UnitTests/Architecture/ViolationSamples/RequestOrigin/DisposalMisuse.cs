// Deliberate violation sample of the attempt record scan in the disposal file (09 15.4 item 5). The scan reads this file as text; it is not compiled.
// Line numbers are part of the test.
internal sealed class AttemptRecordDisposalPart : IDisposalPart
{
    public async Task RunAsync(DisposalRun run, CancellationToken ct)
    {
        await run.ChunksAsync(DisposalRecordType.AttemptRecord, async (db, max, cancel) =>
        {
            IQueryable<AttemptRecord> expired = db.AttemptRecords.Where(a => a.CreatedAt < run.Now);
            await expired.ExecuteUpdateAsync(s => s.SetProperty(a => a.NormalizedValue, "x"), cancel);
            db.AttemptRecords.Add(null);
            db.AttemptRecords.RemoveRange(await expired.ToListAsync(cancel));
            return await expired.ExecuteDeleteAsync(cancel);
        }, ct);
    }
}

internal sealed class SessionDisposalPart : IDisposalPart
{
    public Task RunAsync(DisposalRun run, CancellationToken ct) => run.ChunksAsync(DisposalRecordType.Session, (db, max, cancel) => db.AttemptRecords.ExecuteDeleteAsync(cancel), ct);
}
