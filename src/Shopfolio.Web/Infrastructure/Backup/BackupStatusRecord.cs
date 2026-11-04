using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Shopfolio.Infrastructure.DataModel;

namespace Shopfolio.Infrastructure.Backup;

/// <summary>
/// The backup status record (06 4.67), one row (06 2.3.2): the result of the nightly full backup taken into the repository outside the server
/// (the local copy's backup does not count). The database container's scheduler writes it through the local socket as the backup status user,
/// whose only privilege is the update of the two moments (09 10.8 item 3; the migration grants it). The file backup's result is not here (it is
/// the backup tool's status file), nor is the state of the log archiving (it is read from the database's own statistics). The row comes from
/// the initial data (T119) and is permanent; the panel never writes it, so it carries no version.
/// </summary>
internal sealed class BackupStatusRecord
{
    public Guid Id { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>The last successful full backup outside the server; empty at setup.</summary>
    public DateTimeOffset? LastSucceededAt { get; set; }

    /// <summary>The last failed attempt; the last attempt's result is the newer of the two moments.</summary>
    public DateTimeOffset? LastFailedAt { get; set; }

    public static BackupStatusRecord Create(DateTimeOffset now)
    {
        return new BackupStatusRecord { Id = EntityIds.New(), CreatedAt = now };
    }
}

internal sealed class BackupStatusRecordConfiguration : IEntityTypeConfiguration<BackupStatusRecord>
{
    public void Configure(EntityTypeBuilder<BackupStatusRecord> builder)
    {
        builder.HasKey(b => b.Id);
        builder.AsSingleRow();

        // The single row (06 2.3.2) is kept by a unique index on a constant expression; EF cannot model it, so the migration writes it.
    }
}
