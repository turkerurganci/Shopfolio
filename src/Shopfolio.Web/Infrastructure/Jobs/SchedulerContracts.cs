namespace Shopfolio.Infrastructure.Jobs;

/// <summary>A record whose time is in its own fields: its id, and the moment the scheduler orders it by (05 5.1.1, 5.4.2).</summary>
internal sealed record DueRecord(Guid Id, DateTimeOffset DueAt);

/// <summary>
/// A kind of record whose time has come when its own field says so (05 5.1.1, 5.2.1; 09 13.1 item 6): the payment window of a transfer order,
/// the reminder, the IBAN's deletion day, an e-mail's next attempt. A module that owns such a record registers a source with
/// <see cref="SchedulerRegistration.AddDueRecordSource{TSource}"/>; the scheduler asks every source at each wake-up, puts the records of all
/// sources in order of their due moments, and has each processed in a scope and a transaction of its own. The source reads the time from the
/// record's field: there is no job calendar (05 5.1.1). Processing a record that was handled meanwhile changes nothing (05 5.1.1: seeing the
/// same job twice gives the same result).
/// </summary>
internal interface IDueRecordSource
{
    /// <summary>A short name for the log; it is not the record's data.</summary>
    string Name { get; }

    /// <summary>
    /// True for a source whose processing must not run while the last heartbeat is older than two minutes - the transfer order's cancellation
    /// (05 5.4.3): without a fresh heartbeat the scheduler cannot know the site was not down.
    /// </summary>
    bool WaitsForFreshHeartbeat { get; }

    /// <summary>
    /// The records whose moment is at or before <paramref name="now"/>, earliest first, at most <paramref name="limit"/>. It reads in the given
    /// scope and writes nothing.
    /// </summary>
    Task<IReadOnlyList<DueRecord>> FindDueAsync(IServiceProvider services, DateTimeOffset now, int limit, CancellationToken ct);

    /// <summary>
    /// Processes one record in the given scope, in a transaction of its own that the source opens through <see cref="ITransactionRunner"/>.
    /// The source reads the record again and checks that it is still due; a record that is not does nothing.
    /// </summary>
    Task ProcessAsync(IServiceProvider services, Guid recordId, CancellationToken ct);
}

/// <summary>
/// A change that the return from an outage makes (05 5.4.3). The heartbeat's return transaction calls every effect inside itself, after the
/// outage record is written, so the first heartbeat, the outage record and the effects are one commit or none (K-975).
/// </summary>
internal interface IOutageEffect
{
    /// <summary>
    /// Called inside the return's transaction. <paramref name="startedAt"/> is the last heartbeat before the gap and <paramref name="endedAt"/>
    /// the first one of the return.
    /// </summary>
    Task ApplyAsync(DateTimeOffset startedAt, DateTimeOffset endedAt, CancellationToken ct);
}

/// <summary>
/// One step of the nightly job (05 5.2.8, 5.2.9; 09 13.1 item 9): the disposal and the clean-up of temporary records are steps. The step does its
/// work in small parts, each in a transaction of its own (05 5.1.3, 7.4.1); an exception ends the job as failed and the scheduler tries it again
/// an hour later at the earliest.
/// </summary>
internal interface INightlyStep
{
    string Name { get; }

    Task RunAsync(IServiceProvider services, DateTimeOffset now, CancellationToken ct);
}

/// <summary>The result of a scheduler transaction: never refused, so the transaction commits what the step did.</summary>
internal sealed record SchedulerStepResult(bool IsRejected = false) : IOperationResult;
