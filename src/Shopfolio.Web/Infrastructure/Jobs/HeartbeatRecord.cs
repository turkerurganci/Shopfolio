using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Shopfolio.Infrastructure.DataModel;

namespace Shopfolio.Infrastructure.Jobs;

/// <summary>
/// The heartbeat record (06 4.65), one row (06 2.3.2): the scheduler's last heartbeat and the nightly job's last successful run and last
/// attempt. The row comes from the initial data (T119) and is permanent; the panel never writes it, so it carries no version. Its only index
/// is the single-row index.
/// </summary>
internal sealed class HeartbeatRecord
{
    public Guid Id { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>The last heartbeat's moment; empty at setup.</summary>
    public DateTimeOffset? LastHeartbeatAt { get; set; }

    /// <summary>The nightly job's last successful run.</summary>
    public DateTimeOffset? NightlyJobLastSucceededAt { get; set; }

    /// <summary>The nightly job's last attempt; a failed run is tried again an hour later at the earliest.</summary>
    public DateTimeOffset? NightlyJobLastAttemptedAt { get; set; }

    public static HeartbeatRecord Create(DateTimeOffset now)
    {
        return new HeartbeatRecord { Id = EntityIds.New(), CreatedAt = now };
    }
}

internal sealed class HeartbeatRecordConfiguration : IEntityTypeConfiguration<HeartbeatRecord>
{
    public void Configure(EntityTypeBuilder<HeartbeatRecord> builder)
    {
        builder.HasKey(h => h.Id);
        builder.AsSingleRow();

        // The single row (06 2.3.2) is kept by a unique index on a constant expression; EF cannot model it, so the migration writes it.
    }
}
