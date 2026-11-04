using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.DataModel;
using Shopfolio.Modules.Catalog.Contracts;

namespace Shopfolio.Modules.CorporateContent;

/// <summary>
/// A frequently asked question (06 4.49): a question and its answer, listed on one page in the order the firm gives. It has no page of its
/// own, so no page address. Deleted by its event; its gallery links are FaqItemImage.
/// </summary>
internal sealed class FaqItem
{
    public Guid Id { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>The question; part of the publish gate.</summary>
    public string Question { get; set; } = string.Empty;

    /// <summary>Cleaned HTML of the closed text format set (06 2.10.1); part of the publish gate.</summary>
    public string? Answer { get; set; }

    /// <summary>The manual order; not unique (06 4.49).</summary>
    public int SortOrder { get; set; }

    public PublishStatus PublishStatus { get; set; } = PublishStatus.Draft;

    public int Version { get; set; } = 1;

    public static FaqItem Create(string question, int sortOrder, DateTimeOffset now)
    {
        return new FaqItem { Id = EntityIds.New(), CreatedAt = now, Question = question, SortOrder = sortOrder };
    }
}

internal sealed class FaqItemConfiguration : IEntityTypeConfiguration<FaqItem>
{
    public void Configure(EntityTypeBuilder<FaqItem> builder)
    {
        builder.HasKey(f => f.Id);
        builder.Property(f => f.Question).HasMaxLength(ContentRules.ShortDescriptionMaxLength);
        builder.Property(f => f.Answer).HasColumnType("text");
        builder.Property(f => f.PublishStatus).AsContentPublishStatus();
        builder.Property(f => f.Version).HasDefaultValue(1);

        builder.HasIndex(f => new { f.PublishStatus, f.SortOrder });
    }
}
