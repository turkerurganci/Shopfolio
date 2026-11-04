using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.DataModel;

namespace Shopfolio.Modules.CorporateContent;

/// <summary>
/// The brand identity (06 4.54): brand name, logo, site icon, brand colour and the switch of the platform signature in the footer; one row.
/// The row comes from the initial data (T119), so it is never deleted (<see cref="CorporateContentRecordsGuard"/>). The logo and icon links
/// are restrictive: a replaced image is left unlinked and the disposal deletes it. The table has one row, so it carries no foreign-key
/// index (06 7.5.2).
/// </summary>
internal sealed class Branding
{
    public Guid Id { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>The brand name; empty at setup, and once filled the application does not let it be emptied.</summary>
    public string? BrandName { get; set; }

    public Guid? LogoId { get; set; }

    public Guid? FaviconId { get; set; }

    /// <summary><c>#RRGGBB</c> with capital hexadecimal digits (06 4.54).</summary>
    public string? BrandColor { get; set; }

    public bool IsPlatformSignatureEnabled { get; set; } = true;

    public int Version { get; set; } = 1;

    public static Branding Create(DateTimeOffset now)
    {
        return new Branding { Id = EntityIds.New(), CreatedAt = now };
    }
}

internal sealed class BrandingConfiguration : IEntityTypeConfiguration<Branding>
{
    private const string CatalogImageType = "Shopfolio.Modules.Catalog.Image";

    public void Configure(EntityTypeBuilder<Branding> builder)
    {
        builder.HasKey(b => b.Id);
        builder.AsSingleRow();
        builder.Property(b => b.BrandName).HasColumnType("text");
        builder.Property(b => b.BrandColor).HasColumnType("text");
        builder.Property(b => b.IsPlatformSignatureEnabled).HasDefaultValue(true);
        builder.Property(b => b.Version).HasDefaultValue(1);

        // The image belongs to the Catalog module, whose types a module reaches only through Contracts (09 6 item 2): the links are declared
        // by the entity's name. Both are restrictive (06 4.54).
        builder.HasOne(CatalogImageType, navigationName: null).WithMany().HasForeignKey(nameof(Branding.LogoId)).OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(CatalogImageType, navigationName: null).WithMany().HasForeignKey(nameof(Branding.FaviconId)).OnDelete(DeleteBehavior.NoAction);

        builder.HasRule("brand_color_format", "brand_color ~ '^#[0-9A-F]{6}$'");

        // The single row (06 2.3.2) is kept by a unique index on a constant expression; EF cannot model it, so the migration writes it.
    }
}
