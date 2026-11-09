using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.DataModel;
using Shopfolio.Modules.Catalog.Contracts;

namespace Shopfolio.Modules.CorporateContent;

/// <summary>
/// A general page (06 4.51): a title, a text and a gallery (CustomPageImage), for content that fits no ready-made type; listed in the
/// footer and, when marked, in the top menu. Legal texts are not general pages. Deleted by its event; its gallery links go with it.
/// </summary>
internal sealed class CustomPage : IContentRecord, IAddressedContentRecord
{
    public Guid Id { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>The title; the publish gate.</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>Cleaned HTML of the closed text format set (06 2.10.1); written only by <see cref="SetText"/>.</summary>
    public string? Text { get; private set; }

    /// <summary>Writes the text through the cleaner (09 15.3 item 1); an empty text is no text.</summary>
    public void SetText(string? text) => Text = ContentRules.CleanLongText(text);

    /// <summary>The manual order; not unique (06 4.51).</summary>
    public int SortOrder { get; set; }

    public bool IsShownInMenu { get; set; }

    public PublishStatus PublishStatus { get; set; } = PublishStatus.Draft;

    /// <summary>The record's part of the page address (06 4.72).</summary>
    public string Slug { get; set; } = string.Empty;

    /// <summary>The moment the address was fixed: the first publication (06 4.72).</summary>
    public DateTimeOffset? SlugFixedAt { get; set; }

    public int Version { get; private set; } = 1;

    /// <summary>Raises the version by one; every save of the record calls it (06 2.6.1; 09 10.2 item 1).</summary>
    public void BumpVersion() => Version++;

    public ContentRecordKind Kind => ContentRecordKind.CustomPage;

    public IReadOnlyList<ContentGateField> MissingGateFields() =>
        ContentGate.Missing(Kind, field => field == ContentGateField.Title ? ContentGate.IsFilled(Title) : ContentGate.Unreadable(Kind, field));

    public static CustomPage Create(string title, string slug, int sortOrder, DateTimeOffset now)
    {
        return new CustomPage { Id = EntityIds.New(), CreatedAt = now, Title = title, Slug = slug, SortOrder = sortOrder };
    }
}

internal sealed class CustomPageConfiguration : IEntityTypeConfiguration<CustomPage>
{
    public void Configure(EntityTypeBuilder<CustomPage> builder)
    {
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Title).HasMaxLength(ContentRules.TitleMaxLength);
        builder.Property(c => c.Text).HasColumnType("text");
        builder.Property(c => c.IsShownInMenu).HasDefaultValue(false);
        builder.Property(c => c.PublishStatus).AsContentPublishStatus();
        builder.Property(c => c.Version).HasDefaultValue(1);

        builder.HasIndex(c => c.Slug).IsUnique();
        builder.HasIndex(c => new { c.PublishStatus, c.SortOrder });
        builder.HasIndex(c => new { c.IsShownInMenu, c.SortOrder }).HasFilter("is_shown_in_menu AND publish_status = 'Published'");

        builder.HasRule("slug_format", ContentRules.SlugFormat);
    }
}
