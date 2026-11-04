using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Shopfolio.Infrastructure;

namespace Shopfolio.Modules.CorporateContent;

/// <summary>
/// A product a reference work links from the catalog (06 4.53): the same fields and rules as <see cref="OfferingProduct"/>, with the
/// reference work instead of the service presentation.
/// </summary>
internal sealed class PortfolioItemProduct
{
    public Guid Id { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public Guid PortfolioItemId { get; set; }

    public Guid ProductId { get; set; }

    /// <summary>The order of the linked products on the content page; not unique (06 4.53).</summary>
    public int SortOrder { get; set; }

    public static PortfolioItemProduct Create(Guid portfolioItemId, Guid productId, int sortOrder, DateTimeOffset now)
    {
        return new PortfolioItemProduct { Id = EntityIds.New(), CreatedAt = now, PortfolioItemId = portfolioItemId, ProductId = productId, SortOrder = sortOrder };
    }
}

internal sealed class PortfolioItemProductConfiguration : IEntityTypeConfiguration<PortfolioItemProduct>
{
    private const string CatalogProductType = "Shopfolio.Modules.Catalog.Product";

    public void Configure(EntityTypeBuilder<PortfolioItemProduct> builder)
    {
        builder.HasKey(p => p.Id);

        builder.HasOne<PortfolioItem>().WithMany().HasForeignKey(p => p.PortfolioItemId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(CatalogProductType, navigationName: null).WithMany().HasForeignKey(nameof(PortfolioItemProduct.ProductId)).OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(p => new { p.PortfolioItemId, p.ProductId }).IsUnique();
        builder.HasIndex(p => p.ProductId);
    }
}
