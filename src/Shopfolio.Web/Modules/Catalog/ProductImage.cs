using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Shopfolio.Infrastructure;

namespace Shopfolio.Modules.Catalog;

/// <summary>
/// The gallery link of a product (06 4.71): order, main mark and alternative text live here, not on the image. It goes with its product; the
/// link to the image is restrictive. A gallery has at most one main image; that a non-empty gallery has exactly one is a rule of the application.
/// </summary>
internal sealed class ProductImage
{
    public Guid Id { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public Guid ProductId { get; set; }

    public Guid ImageId { get; set; }

    /// <summary>The place in the gallery; not unique: reordering changes rows.</summary>
    public int SortOrder { get; set; }

    public bool IsMain { get; set; }

    public string? AltText { get; set; }

    public static ProductImage Create(Guid productId, Guid imageId, int sortOrder, bool isMain, DateTimeOffset now)
    {
        return new ProductImage { Id = EntityIds.New(), CreatedAt = now, ProductId = productId, ImageId = imageId, SortOrder = sortOrder, IsMain = isMain };
    }
}

internal sealed class ProductImageConfiguration : IEntityTypeConfiguration<ProductImage>
{
    public void Configure(EntityTypeBuilder<ProductImage> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.AltText).HasColumnType("text");
        builder.Property(p => p.IsMain).HasDefaultValue(false);
        builder.HasOne<Product>().WithMany().HasForeignKey(p => p.ProductId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<Image>().WithMany().HasForeignKey(p => p.ImageId).OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(p => new { p.ProductId, p.ImageId }).IsUnique();
        builder.HasIndex(p => p.ProductId).IsUnique().HasFilter("is_main");
    }
}
