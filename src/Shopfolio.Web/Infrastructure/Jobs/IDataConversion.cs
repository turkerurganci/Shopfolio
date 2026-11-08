using Shopfolio.Infrastructure.Values;

namespace Shopfolio.Infrastructure.Jobs;

/// <summary>
/// A large data conversion that runs as batches of a background job (09 13.3 item 2; 05 8.7.5): one class each, registered as an <see cref="IDataConversion"/> by the PR that adds
/// the conversion. The progress is not stored: the remaining work is the rows that still meet the unconverted condition, so the server's status command reads it from the data
/// (<see cref="CountRemainingAsync"/>; 09 13.3 item 5). The generic job handler that calls <see cref="ConvertBatchAsync"/> arrives with the first conversion.
/// </summary>
internal interface IDataConversion
{
    /// <summary>The job type of the conversion (06 5.4.4); also the name the status command prints.</summary>
    JobType Type { get; }

    /// <summary>The unconverted condition as SQL (the narrowing migration's pre-check is its copy; 09 13.3 item 6).</summary>
    string RemainingPredicateSql { get; }

    /// <summary>Converts one batch in its own transaction and returns the number of rows converted.</summary>
    Task<int> ConvertBatchAsync(int batchSize, CancellationToken ct);

    /// <summary>The number of rows that still meet the unconverted condition.</summary>
    Task<long> CountRemainingAsync(CancellationToken ct);
}
