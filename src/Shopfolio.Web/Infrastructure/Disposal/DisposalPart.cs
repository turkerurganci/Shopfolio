using Shopfolio.Infrastructure.Values;

namespace Shopfolio.Infrastructure.Disposal;

/// <summary>
/// One part of the nightly disposal (05 7.4.1; 06 9.3.1, 9.4.3): a kind of record, or of file, that is deleted or emptied when its time is over.
/// The disposal step runs the registered parts one after the other, in the order of their registration. A part does its work through the
/// <see cref="DisposalRun"/> it is given, so that each small part of its work is one transaction and writes its disposal record in it.
/// </summary>
internal interface IDisposalPart
{
    /// <summary>A short name for the log; it is not the records' data.</summary>
    string Name { get; }

    /// <summary>Handles every record of the kind whose time is over at <see cref="DisposalRun.Now"/>; the time is read from the records.</summary>
    Task RunAsync(DisposalRun run, CancellationToken ct);
}

/// <summary>The result of one disposal transaction: how many records went. It is never refused; a failure is an exception.</summary>
internal sealed record DisposalChunkResult(int Deleted) : IOperationResult
{
    public bool IsRejected => false;
}

/// <summary>
/// What a part works with (05 7.4.1, 7.4.2): the moment of the run, and the two ways to write - <see cref="ChunksAsync"/> for rows (each chunk
/// deletes and writes its disposal record in one transaction, so the record failing undoes the deletion) and <see cref="RecordAsync"/> for a part
/// that deletes files (the row is written first, in its own transaction; a crash in between leaves a surplus count, never a missing one - K-985).
/// </summary>
internal sealed class DisposalRun
{
    private readonly IServiceProvider _services;

    public DisposalRun(IServiceProvider services, DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(services);
        _services = services;
        Now = now;
    }

    /// <summary>The run's moment, from the clock the nightly job was started with; every period is counted against it.</summary>
    public DateTimeOffset Now { get; }

    public AppDbContext Db => _services.GetRequiredService<AppDbContext>();

    /// <summary>
    /// Calls <paramref name="deleteChunk"/> - which deletes or empties at most the part size and returns how many - in a transaction of its own,
    /// again and again, until a chunk returns fewer than the part size. A chunk that deleted something writes one disposal row of
    /// <paramref name="type"/> with that number in the same transaction; a chunk that deleted nothing writes no row (06 9.3.1). Returns the total.
    /// </summary>
    public async Task<int> ChunksAsync(DisposalRecordType type, Func<AppDbContext, int, CancellationToken, Task<int>> deleteChunk, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(deleteChunk);
        ITransactionRunner runner = _services.GetRequiredService<ITransactionRunner>();
        AppDbContext db = Db;
        int total = 0;
        while (true)
        {
            DisposalChunkResult chunk = await runner.RunAsync(
                async cancel =>
                {
                    int deleted = await deleteChunk(db, DisposalTimings.PartSize, cancel);
                    if (deleted > 0)
                    {
                        db.DisposalRecords.Add(DisposalRecord.Counted(type, deleted, Now));
                    }

                    return new DisposalChunkResult(deleted);
                },
                ct);
            total += chunk.Deleted;
            if (chunk.Deleted < DisposalTimings.PartSize)
            {
                return total;
            }
        }
    }

    /// <summary>Writes one disposal row in a transaction of its own; the part deletes the files after it returns.</summary>
    public async Task RecordAsync(DisposalRecord row, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(row);
        ITransactionRunner runner = _services.GetRequiredService<ITransactionRunner>();
        AppDbContext db = Db;
        await runner.RunAsync(
            _ =>
            {
                db.DisposalRecords.Add(row);
                return Task.FromResult(new DisposalChunkResult(1));
            },
            ct);
    }
}
