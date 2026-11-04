using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Shopfolio.Infrastructure;

namespace Shopfolio.Modules.CorporateContent;

/// <summary>
/// The gallery link of the service presentation (06 4.71): order, main mark and alternative text live here, not on the image. It goes with its service presentation
/// (06 2.2.5); the link to the image is restrictive, so a linked image cannot be disposed of. A gallery has at most one main image; that a
/// non-empty gallery has exactly one is a rule of the application.
/// </summary>
internal sealed class OfferingImage
{
    public Guid Id { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public Guid OfferingId { get; set; }

    public Guid ImageId { get; set; }

    /// <summary>The place in the gallery; not unique: reordering changes rows.</summary>
    public int SortOrder { get; set; }

    public bool IsMain { get; set; }

    public string? AltText { get; set; }

    public static OfferingImage Create(Guid offeringId, Guid imageId, int sortOrder, bool isMain, DateTimeOffset now)
    {
        return new OfferingImage { Id = EntityIds.New(), CreatedAt = now, OfferingId = offeringId, ImageId = imageId, SortOrder = sortOrder, IsMain = isMain };
    }
}

internal sealed class OfferingImageConfiguration : IEntityTypeConfiguration<OfferingImage>
{
    private const string CatalogImageType = "Shopfolio.Modules.Catalog.Image";

    public void Configure(EntityTypeBuilder<OfferingImage> builder)
    {
        builder.HasKey(g => g.Id);
        builder.Property(g => g.AltText).HasColumnType("text");
        builder.Property(g => g.IsMain).HasDefaultValue(false);
        builder.HasOne<Offering>().WithMany().HasForeignKey(g => g.OfferingId).OnDelete(DeleteBehavior.Cascade);

        // The image belongs to the Catalog module, whose types a module reaches only through Contracts (09 6 item 2): the link is declared by
        // the entity's name. It is restrictive (06 4.71).
        builder.HasOne(CatalogImageType, navigationName: null).WithMany().HasForeignKey(nameof(OfferingImage.ImageId)).OnDelete(DeleteBehavior.NoAction);

        // The pair's unique index starts with the owner, so it also serves the owner's gallery (06 7.5.2); one main image per owner; the image
        // index serves the restrictive link and the disposal scan of unlinked images (06 4.71).
        builder.HasIndex(g => new { g.OfferingId, g.ImageId }).IsUnique();
        builder.HasIndex(g => g.OfferingId).IsUnique().HasFilter("is_main");
        builder.HasIndex(g => g.ImageId);
    }
}
