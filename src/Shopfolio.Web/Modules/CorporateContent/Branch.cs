using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.DataModel;
using Shopfolio.Modules.Catalog.Contracts;

namespace Shopfolio.Modules.CorporateContent;

/// <summary>
/// A branch (06 4.50): a place of the firm that can be visited, listed on the contact page. It has no page address and is not a delivery
/// point. It has one image, whose link is the record's column and whose alternative text sits beside it. Deleted by its event; the links to
/// the province and to the image are restrictive, so a replaced or deleted branch leaves its image unlinked and the disposal deletes it.
/// </summary>
internal sealed class Branch : IVersionedRecord
{
    public Guid Id { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>The name; the publish gate.</summary>
    public string Name { get; set; } = string.Empty;

    public Guid? ProvinceId { get; set; }

    public string? District { get; set; }

    public string? AddressLine { get; set; }

    /// <summary>Ten digits, without the leading zero or "+90" (06 2.5.1).</summary>
    public string? Phone { get; set; }

    public string? WorkingHours { get; set; }

    public string? MapUrl { get; set; }

    public Guid? ImageId { get; set; }

    /// <summary>The alternative text of the image; empty while there is no image (06 4.50).</summary>
    public string? ImageAltText { get; set; }

    /// <summary>The manual order; not unique (06 4.50).</summary>
    public int SortOrder { get; set; }

    public PublishStatus PublishStatus { get; set; } = PublishStatus.Draft;

    public int Version { get; private set; } = 1;

    /// <summary>Raises the version by one; every save of the record calls it (06 2.6.1; 09 10.2 item 1).</summary>
    public void BumpVersion() => Version++;

    public static Branch Create(string name, int sortOrder, DateTimeOffset now)
    {
        return new Branch { Id = EntityIds.New(), CreatedAt = now, Name = name, SortOrder = sortOrder };
    }
}

internal sealed class BranchConfiguration : IEntityTypeConfiguration<Branch>
{
    private const string SettingsProvinceType = "Shopfolio.Modules.Settings.Province";
    private const string CatalogImageType = "Shopfolio.Modules.Catalog.Image";

    public void Configure(EntityTypeBuilder<Branch> builder)
    {
        builder.HasKey(b => b.Id);
        builder.Property(b => b.Name).HasMaxLength(ContentRules.TitleMaxLength);
        builder.Property(b => b.District).HasColumnType("text");
        builder.Property(b => b.AddressLine).HasColumnType("text");
        builder.Property(b => b.Phone).HasMaxLength(10);
        builder.Property(b => b.WorkingHours).HasMaxLength(ContentRules.ShortDescriptionMaxLength);
        builder.Property(b => b.MapUrl).HasColumnType("text");
        builder.Property(b => b.ImageAltText).HasColumnType("text");
        builder.Property(b => b.PublishStatus).AsContentPublishStatus();
        builder.Property(b => b.Version).HasDefaultValue(1);

        // The province belongs to the Settings module and the image to the Catalog module, whose types a module reaches only through
        // Contracts (09 6 item 2): the links are declared by the entity's name. Both are restrictive (06 4.50).
        builder.HasOne(SettingsProvinceType, navigationName: null).WithMany().HasForeignKey(nameof(Branch.ProvinceId)).OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(CatalogImageType, navigationName: null).WithMany().HasForeignKey(nameof(Branch.ImageId)).OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(b => new { b.PublishStatus, b.SortOrder });
        builder.HasIndex(b => b.ProvinceId);
        builder.HasIndex(b => b.ImageId);

        builder.HasRule("phone_ten_digits", "phone ~ '^[0-9]{10}$'");
        builder.HasRule("image_alt_text_only_with_image", "image_id IS NOT NULL OR image_alt_text IS NULL");
    }
}
