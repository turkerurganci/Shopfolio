using Shopfolio.Infrastructure.Values;

namespace Shopfolio.Infrastructure.Jobs;

/// <summary>
/// What the runner hands a handler of one row (09 13.1 item 4): the row's identity and the record it points at, never a tracked entity. The
/// row carries no personal data; the handler reads what it needs from the related record.
/// </summary>
internal sealed record JobWork(Guid Id, JobType JobType, Guid? RelatedRecordId, DateTimeOffset CreatedAt, int AttemptCount);

/// <summary>
/// A handler of a job that calls outside (a card refund, a query, an image derivative; 09 13.1 item 4). The runner gives it the row with no
/// transaction open, so the outside call waits for no database lock (05 3.6.3); the handler writes its own database changes through
/// <see cref="ITransactionRunner"/> and tells the outcome by its return value. The runner writes the outcome into the row in a separate short
/// transaction. Delivery is at least once: a handler that sees the same row a second time leaves the result unchanged (05 4.3.3).
/// </summary>
internal interface IJobHandler
{
    Task<JobOutcome> HandleAsync(JobWork job, CancellationToken ct);
}

/// <summary>
/// A handler of a job that only writes to the database (the processing of an inbound notification; 09 13.1 item 4). The runner opens the
/// transaction, the handler writes the effect, and the row's completion is written in the same transaction (05 4.3.3). The handler never calls
/// outside and never opens a transaction of its own. A failure rolls the effect back and the runner records it on the row in another transaction.
/// </summary>
internal interface ITransactionalJobHandler
{
    Task HandleAsync(JobWork job, CancellationToken ct);
}

/// <summary>
/// A condition that must hold before the runner takes any row (D-96 K1). The start-up rewrite of the pending revert links' digests is one: the
/// queue and the e-mail sending must not start before it has run (09 15.1 item 4; 05 6.x.20). The check is asked again each time the runner
/// wakes, so a step that could not run at start - the database was out of reach - runs once the database is back.
/// </summary>
internal interface IJobStartGate
{
    /// <summary>True when the condition holds; a gate that is not open yet tries to make itself true and says so.</summary>
    Task<bool> IsOpenAsync(CancellationToken ct);
}

/// <summary>One registered handler: the job type it serves, the kind of its contract and its type.</summary>
internal sealed record JobHandlerRegistration(JobType JobType, bool IsTransactional, Type HandlerType);

/// <summary>
/// The job types that have a handler (09 13.1 item 3): the runner reads only these; a row of a type without a handler is not read and not
/// touched, so a type that a newer version writes waits for the version that knows it.
/// </summary>
internal sealed class JobHandlerRegistry
{
    private readonly Dictionary<JobType, JobHandlerRegistration> _byType;

    public JobHandlerRegistry(IEnumerable<JobHandlerRegistration> registrations)
    {
        ArgumentNullException.ThrowIfNull(registrations);
        _byType = [];
        foreach (JobHandlerRegistration registration in registrations)
        {
            if (!_byType.TryAdd(registration.JobType, registration))
            {
                throw new InvalidOperationException($"The job type {registration.JobType} has two handlers; one type has one handler.");
            }
        }
    }

    /// <summary>The job types the runner may read, as the text the column holds.</summary>
    public IReadOnlyList<string> KnownTypeNames => [.. _byType.Keys.Select(t => t.ToString())];

    /// <summary>The job types the runner may read.</summary>
    public IReadOnlyList<JobType> KnownTypes => [.. _byType.Keys];

    public bool TryGet(JobType jobType, out JobHandlerRegistration registration)
    {
        return _byType.TryGetValue(jobType, out registration!);
    }
}
