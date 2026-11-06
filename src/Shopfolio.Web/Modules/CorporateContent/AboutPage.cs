using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.DataModel;
using Shopfolio.Modules.Catalog.Contracts;

namespace Shopfolio.Modules.CorporateContent;

/// <summary>
/// The about page (06 4.46): the one corporate record with a short introduction, a long text and a gallery (AboutPageImage). It is not
/// there at setup; the row is born at its first save and is at most one (the single-row index of the migration, 06 2.3.2). It is never
/// deleted, only put back to draft (<see cref="CorporateContentRecordsGuard"/>).
/// </summary>
internal sealed class AboutPage : IVersionedRecord
{
    public Guid Id { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>Plain text; the publish gate (06 4.46).</summary>
    public string? ShortIntroduction { get; set; }

    /// <summary>Cleaned HTML of the closed text format set (06 2.10.1).</summary>
    public string? LongText { get; set; }

    public PublishStatus PublishStatus { get; set; } = PublishStatus.Draft;

    public int Version { get; private set; } = 1;

    /// <summary>Raises the version by one; every save of the record calls it (06 2.6.1; 09 10.2 item 1).</summary>
    public void BumpVersion() => Version++;

    public static AboutPage Create(DateTimeOffset now)
    {
        return new AboutPage { Id = EntityIds.New(), CreatedAt = now };
    }
}

internal sealed class AboutPageConfiguration : IEntityTypeConfiguration<AboutPage>
{
    public void Configure(EntityTypeBuilder<AboutPage> builder)
    {
        builder.HasKey(a => a.Id);
        builder.AsSingleRow();
        builder.Property(a => a.ShortIntroduction).HasMaxLength(ContentRules.ShortIntroductionMaxLength);
        builder.Property(a => a.LongText).HasColumnType("text");
        builder.Property(a => a.PublishStatus).AsContentPublishStatus();
        builder.Property(a => a.Version).HasDefaultValue(1);

        // The single row (06 2.3.2) is kept by a unique index on a constant expression; EF cannot model it, so the migration writes it.
    }
}
