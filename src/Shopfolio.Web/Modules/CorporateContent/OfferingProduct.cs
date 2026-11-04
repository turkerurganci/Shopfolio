using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Shopfolio.Infrastructure;

namespace Shopfolio.Modules.CorporateContent;

/// <summary>
/// A product a service presentation links from the catalog (06 4.53), shown as "related products". The link is at the product level (no
/// variant) and carries the order of the choice. A product is linked to a presentation once. It goes with its presentation and with its
/// product (06 2.2.5); the removal of the link from the form is an event.
/// </summary>
internal sealed class OfferingProduct
{
    public Guid Id { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public Guid OfferingId { get; set; }

    public Guid ProductId { get; set; }

    /// <summary>The order of the linked products on the content page; not unique (06 4.53).</summary>
    public int SortOrder { get; set; }

    public static OfferingProduct Create(Guid offeringId, Guid productId, int sortOrder, DateTimeOffset now)
    {
        return new OfferingProduct { Id = EntityIds.New(), CreatedAt = now, OfferingId = offeringId, ProductId = productId, SortOrder = sortOrder };
    }
}

internal sealed class OfferingProductConfiguration : IEntityTypeConfiguration<OfferingProduct>
{
    private const string CatalogProductType = "Shopfolio.Modules.Catalog.Product";

    public void Configure(EntityTypeBuilder<OfferingProduct> builder)
    {
        builder.HasKey(o => o.Id);

        // The product belongs to the Catalog module, whose types a module reaches only through Contracts (09 6 item 2): the link is declared
        // by the entity's name.
        builder.HasOne<Offering>().WithMany().HasForeignKey(o => o.OfferingId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(CatalogProductType, navigationName: null).WithMany().HasForeignKey(nameof(OfferingProduct.ProductId)).OnDelete(DeleteBehavior.Cascade);

        // The pair's unique index starts with the presentation, so it also serves the presentation's links (06 7.5.2).
        builder.HasIndex(o => new { o.OfferingId, o.ProductId }).IsUnique();
        builder.HasIndex(o => o.ProductId);
    }
}
