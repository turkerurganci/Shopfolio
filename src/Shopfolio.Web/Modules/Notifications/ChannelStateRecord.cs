using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.DataModel;

namespace Shopfolio.Modules.Notifications;

/// <summary>
/// The channel state record (06 4.60): the e-mail sender's channel state, one row (06 2.3.2) — the last successful sending, the number of
/// consecutive channel errors and the moment of the first error of the run. The channel warning is read from it, not stored. The row comes
/// from the initial data (T119) and is permanent. It has no link, so it carries no foreign-key index; its only index is the single-row index.
/// </summary>
internal sealed class ChannelStateRecord
{
    public Guid Id { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>The moment of the last successful sending; empty at setup.</summary>
    public DateTimeOffset? LastSucceededAt { get; set; }

    /// <summary>Raised by one on a channel error and reset to zero on a success; a recipient error neither counts nor resets.</summary>
    public int ConsecutiveChannelErrorCount { get; set; }

    /// <summary>The moment of the run's first error: written when the count goes from zero to one, emptied on a success; empty exactly when the count is zero.</summary>
    public DateTimeOffset? FirstChannelErrorAt { get; set; }

    public static ChannelStateRecord Create(DateTimeOffset now)
    {
        return new ChannelStateRecord { Id = EntityIds.New(), CreatedAt = now };
    }
}

internal sealed class ChannelStateRecordConfiguration : IEntityTypeConfiguration<ChannelStateRecord>
{
    public void Configure(EntityTypeBuilder<ChannelStateRecord> builder)
    {
        builder.HasKey(c => c.Id);
        builder.AsSingleRow();
        builder.Property(c => c.ConsecutiveChannelErrorCount).HasDefaultValue(0);

        builder.HasRule("error_count_not_negative", "consecutive_channel_error_count >= 0");
        builder.HasRule("first_error_at_with_count", "(consecutive_channel_error_count = 0) = (first_channel_error_at IS NULL)");

        // The single row (06 2.3.2) is kept by a unique index on a constant expression; EF cannot model it, so the migration writes it.
    }
}
