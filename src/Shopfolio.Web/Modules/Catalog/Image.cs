using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.DataModel;
using Shopfolio.Modules.Catalog.Contracts;

namespace Shopfolio.Modules.Catalog;

/// <summary>
/// An image: one row per upload (06 4.69). The re-encoded source copy is at most 2560 pixels on its longest side. The storage key is not unique:
/// two uploads of the same content point to the same file. Every link to an image is restrictive; an image with no link left is deleted by the
/// disposal job, not by a cascade. Order, main mark and alternative text live on the link, not here.
/// </summary>
internal sealed class Image
{
    /// <summary>The longest side of the stored source copy, in pixels (06 4.69; 05 7.7.2 design value).</summary>
    public const int SourceMaxEdge = 2560;

    public Guid Id { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>The key in the file store: a fingerprint of the content.</summary>
    public string StorageKey { get; set; } = string.Empty;

    public ImageFormat ImageFormat { get; set; }

    public int Width { get; set; }

    public int Height { get; set; }

    /// <summary>The moment the derivatives are ready; empty while the source copy is served.</summary>
    public DateTimeOffset? DerivativesReadyAt { get; set; }

    public static Image Create(string storageKey, ImageFormat imageFormat, int width, int height, DateTimeOffset now)
    {
        return new Image { Id = EntityIds.New(), CreatedAt = now, StorageKey = storageKey, ImageFormat = imageFormat, Width = width, Height = height };
    }
}

internal sealed class ImageConfiguration : IEntityTypeConfiguration<Image>
{
    public void Configure(EntityTypeBuilder<Image> builder)
    {
        builder.HasKey(i => i.Id);

        // Not unique (06 4.69): the disposal sweep finds the file of a row through it; the creation time serves the scan for unlinked images.
        builder.HasIndex(i => i.StorageKey);
        builder.HasIndex(i => i.CreatedAt);

        builder.HasRule(
            "dimensions_within_limits",
            $"width > 0 AND height > 0 AND width <= {Image.SourceMaxEdge} AND height <= {Image.SourceMaxEdge}");
    }
}
