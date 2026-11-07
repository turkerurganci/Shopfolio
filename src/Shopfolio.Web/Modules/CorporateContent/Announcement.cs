using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.DataModel;
using Shopfolio.Modules.Catalog.Contracts;

namespace Shopfolio.Modules.CorporateContent;

/// <summary>
/// The announcement (06 4.52): the one-line text at the top of every page and an optional link. It is a corporate content record, not a
/// company setting: it has a publish status and is previewed. It is not there at setup; the row is born at its first save and is at most
/// one (the single-row index of the migration, 06 2.3.2). It may be deleted, and a new row may then be opened. The visibility inside the
/// date range is read from the status and the dates, never stored.
/// </summary>
internal sealed class Announcement : IContentRecord
{
    public Guid Id { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>Plain text; the publish gate.</summary>
    public string? Text { get; set; }

    public string? LinkUrl { get; set; }

    /// <summary>The first day it shows, from 00:00 Turkey time (06 2.5.2).</summary>
    public DateOnly? StartDate { get; set; }

    /// <summary>The last day it shows, inclusive (06 2.5.2).</summary>
    public DateOnly? EndDate { get; set; }

    public PublishStatus PublishStatus { get; set; } = PublishStatus.Draft;

    public int Version { get; private set; } = 1;

    /// <summary>Raises the version by one; every save of the record calls it (06 2.6.1; 09 10.2 item 1).</summary>
    public void BumpVersion() => Version++;

    public ContentRecordKind Kind => ContentRecordKind.Announcement;

    public IReadOnlyList<ContentGateField> MissingGateFields() =>
        ContentGate.Missing(Kind, field => field == ContentGateField.Text ? ContentGate.IsFilled(Text) : ContentGate.Unreadable(Kind, field));

    public static Announcement Create(DateTimeOffset now)
    {
        return new Announcement { Id = EntityIds.New(), CreatedAt = now };
    }
}

internal sealed class AnnouncementConfiguration : IEntityTypeConfiguration<Announcement>
{
    public void Configure(EntityTypeBuilder<Announcement> builder)
    {
        builder.HasKey(a => a.Id);
        builder.AsSingleRow();
        builder.Property(a => a.Text).HasMaxLength(ContentRules.AnnouncementTextMaxLength);
        builder.Property(a => a.LinkUrl).HasColumnType("text");
        builder.Property(a => a.PublishStatus).AsContentPublishStatus();
        builder.Property(a => a.Version).HasDefaultValue(1);

        builder.HasRule("end_not_before_start", "start_date IS NULL OR end_date IS NULL OR end_date >= start_date");

        // The single row (06 2.3.2) is kept by a unique index on a constant expression; EF cannot model it, so the migration writes it.
    }
}
