using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Shopfolio.Infrastructure.DataModel;

namespace Shopfolio.Infrastructure.Jobs;

/// <summary>
/// The outage record (06 4.66): a gap of more than two minutes between two heartbeats is one row, from the last heartbeat to the first one of
/// the return. A shorter gap is no outage. The row is written in the return's transaction and never updated; it is permanent and carries no
/// personal data. It is never scanned by a query, so it has no index besides its primary key (06 7.5.2).
/// </summary>
internal sealed class OutageRecord
{
    public Guid Id { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>The last heartbeat before the gap.</summary>
    public DateTimeOffset StartedAt { get; set; }

    /// <summary>The first heartbeat of the return.</summary>
    public DateTimeOffset EndedAt { get; set; }

    public static OutageRecord Create(DateTimeOffset startedAt, DateTimeOffset endedAt, DateTimeOffset now)
    {
        return new OutageRecord { Id = EntityIds.New(), CreatedAt = now, StartedAt = startedAt, EndedAt = endedAt };
    }
}

internal sealed class OutageRecordConfiguration : IEntityTypeConfiguration<OutageRecord>
{
    public void Configure(EntityTypeBuilder<OutageRecord> builder)
    {
        builder.HasKey(o => o.Id);

        // The design value of 05 5.4.1 (K-1068): a gap of two minutes or less is not an outage.
        builder.HasRule("longer_than_two_minutes", "ended_at - started_at > interval '2 minutes'");
    }
}
