using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Shopfolio.Infrastructure.DataModel;
using Shopfolio.Infrastructure.Values;

namespace Shopfolio.Infrastructure.Jobs;

/// <summary>
/// The job queue's row (06 4.59): a side effect of a transition (a card refund, a final query, an image derivative) is written as one row in
/// the transition's transaction and run after it; the inbound notification's processing, the query of a refund with an unknown outcome and the
/// bounce query are rows too. The row carries no personal data: it points at its record by id — a value, not a link (06 2.2.6), whose kind is
/// read from the job type. Only the bounce query has no related record. Taking a row is the database's row lock; there is no lock column.
/// </summary>
internal sealed class Job
{
    public Guid Id { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public JobType JobType { get; set; }

    /// <summary>The related record's id — a value, not a link; empty exactly on the bounce query (K-1241).</summary>
    public Guid? RelatedRecordId { get; set; }

    public JobState JobState { get; set; } = JobState.Pending;

    /// <summary>The number of attempts; never negative.</summary>
    public int AttemptCount { get; set; }

    /// <summary>The next attempt's moment; a row whose moment has not come is not taken, an empty one is taken at once.</summary>
    public DateTimeOffset? NextAttemptAt { get; set; }

    /// <summary>The last error's summary, without personal data (the job type's name and an SQLSTATE at most).</summary>
    public string? LastErrorSummary { get; set; }

    /// <summary>The moment the job ended or the installer closed the row; filled exactly in the completed state.</summary>
    public DateTimeOffset? CompletedAt { get; set; }

    /// <summary>A new pending row, before any attempt.</summary>
    public static Job Enqueue(JobType jobType, Guid? relatedRecordId, DateTimeOffset? nextAttemptAt, DateTimeOffset now)
    {
        return new Job
        {
            Id = EntityIds.New(),
            CreatedAt = now,
            JobType = jobType,
            RelatedRecordId = relatedRecordId,
            NextAttemptAt = nextAttemptAt,
        };
    }
}

internal sealed class JobConfiguration : IEntityTypeConfiguration<Job>
{
    private const string Pending = nameof(JobState.Pending);
    private const string Completed = nameof(JobState.Completed);
    private const string Failed = nameof(JobState.Failed);
    private const string BounceQuery = nameof(JobType.BounceQuery);

    public void Configure(EntityTypeBuilder<Job> builder)
    {
        builder.HasKey(j => j.Id);
        builder.Property(j => j.LastErrorSummary).HasColumnType("text");
        builder.Property(j => j.JobState).HasDefaultValue(JobState.Pending);
        builder.Property(j => j.AttemptCount).HasDefaultValue(0);

        // No link to any table: the related record is a value (06 2.2.6, 7.2.3).
        // One pending job of a type per record; the empty related record counts as equal, so the bounce query has one pending row (K-1072, K-1241).
        builder.HasIndex(j => new { j.JobType, j.RelatedRecordId }).IsUnique().AreNullsDistinct(false).HasFilter($"job_state = '{Pending}'");

        // The queue's scan and the stuck condition; the status endpoint's failed rows; the nightly clean-up of the completed rows (06 4.59, 7.5).
        builder.HasIndex(j => new { j.JobState, j.NextAttemptAt }).HasFilter($"job_state = '{Pending}'");
        builder.HasIndex(j => j.JobState).HasFilter($"job_state = '{Failed}'");
        builder.HasIndex(j => j.CompletedAt).HasFilter($"job_state = '{Completed}'");

        builder.HasRule("completed_at_with_state", $"(job_state = '{Completed}') = (completed_at IS NOT NULL)");
        builder.HasRule("attempt_count_not_negative", "attempt_count >= 0");
        builder.HasRule("related_record_by_type", $"(job_type = '{BounceQuery}') = (related_record_id IS NULL)");
    }
}
