using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.DataModel;
using Shopfolio.Modules.Catalog.Contracts;

namespace Shopfolio.Modules.CorporateContent;

/// <summary>
/// A reference work (06 4.48): the same fields and rules as the service presentation (<see cref="Offering"/>); the difference is a title
/// instead of a name. Deleted by its event; its content-product and gallery links are PortfolioItemProduct and PortfolioItemImage.
/// </summary>
internal sealed class PortfolioItem : IContentRecord
{
    public Guid Id { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>The title; the publish gate.</summary>
    public string Title { get; set; } = string.Empty;

    public string? ShortDescription { get; set; }

    /// <summary>Cleaned HTML of the closed text format set (06 2.10.1).</summary>
    public string? Text { get; set; }

    /// <summary>The manual order; not unique (06 4.47).</summary>
    public int SortOrder { get; set; }

    public bool IsShownOnHomepage { get; set; }

    public PublishStatus PublishStatus { get; set; } = PublishStatus.Draft;

    /// <summary>The record's part of the page address (06 4.72).</summary>
    public string Slug { get; set; } = string.Empty;

    /// <summary>The moment the address was fixed: the first publication (06 4.72).</summary>
    public DateTimeOffset? SlugFixedAt { get; set; }

    public int Version { get; private set; } = 1;

    /// <summary>Raises the version by one; every save of the record calls it (06 2.6.1; 09 10.2 item 1).</summary>
    public void BumpVersion() => Version++;

    public ContentRecordKind Kind => ContentRecordKind.PortfolioItem;

    public IReadOnlyList<ContentGateField> MissingGateFields() =>
        ContentGate.Missing(Kind, field => field == ContentGateField.Title ? ContentGate.IsFilled(Title) : ContentGate.Unreadable(Kind, field));

    public static PortfolioItem Create(string title, string slug, int sortOrder, DateTimeOffset now)
    {
        return new PortfolioItem { Id = EntityIds.New(), CreatedAt = now, Title = title, Slug = slug, SortOrder = sortOrder };
    }
}

internal sealed class PortfolioItemConfiguration : IEntityTypeConfiguration<PortfolioItem>
{
    public void Configure(EntityTypeBuilder<PortfolioItem> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Title).HasMaxLength(ContentRules.TitleMaxLength);
        builder.Property(p => p.ShortDescription).HasMaxLength(ContentRules.ShortDescriptionMaxLength);
        builder.Property(p => p.Text).HasColumnType("text");
        builder.Property(p => p.IsShownOnHomepage).HasDefaultValue(false);
        builder.Property(p => p.PublishStatus).AsContentPublishStatus();
        builder.Property(p => p.Version).HasDefaultValue(1);

        builder.HasIndex(p => p.Slug).IsUnique();
        builder.HasIndex(p => new { p.PublishStatus, p.SortOrder });
        builder.HasIndex(p => new { p.IsShownOnHomepage, p.SortOrder }).HasFilter("is_shown_on_homepage AND publish_status = 'Published'");

        builder.HasRule("slug_format", ContentRules.SlugFormat);
    }
}
