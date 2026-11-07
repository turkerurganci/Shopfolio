using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.DataModel;
using Shopfolio.Modules.Catalog.Contracts;

namespace Shopfolio.Modules.CorporateContent;

/// <summary>
/// A service presentation (06 4.47): a price-less introduction of what the firm does, with its own page. The manual order is not unique
/// (reordering changes rows); the page address is unique over every row, draft ones included (06 4.72.1). The record is deleted by its
/// event; its content-product and gallery links, which cascade, are OfferingProduct and OfferingImage.
/// </summary>
internal sealed class Offering : IContentRecord
{
    public Guid Id { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>The name; the publish gate.</summary>
    public string Name { get; set; } = string.Empty;

    public string? ShortDescription { get; set; }

    /// <summary>Cleaned HTML of the closed text format set (06 2.10.1).</summary>
    public string? Text { get; set; }

    /// <summary>The manual order; not unique (06 4.47).</summary>
    public int SortOrder { get; set; }

    public bool IsShownOnHomepage { get; set; }

    public PublishStatus PublishStatus { get; set; } = PublishStatus.Draft;

    /// <summary>The record's part of the page address (06 4.72).</summary>
    public string Slug { get; set; } = string.Empty;

    /// <summary>The moment the address was fixed: the first publication; once filled the address never changes (06 4.72).</summary>
    public DateTimeOffset? SlugFixedAt { get; set; }

    public int Version { get; private set; } = 1;

    /// <summary>Raises the version by one; every save of the record calls it (06 2.6.1; 09 10.2 item 1).</summary>
    public void BumpVersion() => Version++;

    public ContentRecordKind Kind => ContentRecordKind.Offering;

    public IReadOnlyList<ContentGateField> MissingGateFields() =>
        ContentGate.Missing(Kind, field => field == ContentGateField.Name ? ContentGate.IsFilled(Name) : ContentGate.Unreadable(Kind, field));

    public static Offering Create(string name, string slug, int sortOrder, DateTimeOffset now)
    {
        return new Offering { Id = EntityIds.New(), CreatedAt = now, Name = name, Slug = slug, SortOrder = sortOrder };
    }
}

internal sealed class OfferingConfiguration : IEntityTypeConfiguration<Offering>
{
    public void Configure(EntityTypeBuilder<Offering> builder)
    {
        builder.HasKey(o => o.Id);
        builder.Property(o => o.Name).HasMaxLength(ContentRules.TitleMaxLength);
        builder.Property(o => o.ShortDescription).HasMaxLength(ContentRules.ShortDescriptionMaxLength);
        builder.Property(o => o.Text).HasColumnType("text");
        builder.Property(o => o.IsShownOnHomepage).HasDefaultValue(false);
        builder.Property(o => o.PublishStatus).AsContentPublishStatus();
        builder.Property(o => o.Version).HasDefaultValue(1);

        // The address is unique without a filter (06 4.72.1); the two list indexes serve the storefront list and the homepage block (06 4.47).
        builder.HasIndex(o => o.Slug).IsUnique();
        builder.HasIndex(o => new { o.PublishStatus, o.SortOrder });
        builder.HasIndex(o => new { o.IsShownOnHomepage, o.SortOrder }).HasFilter("is_shown_on_homepage AND publish_status = 'Published'");

        builder.HasRule("slug_format", ContentRules.SlugFormat);
    }
}
