using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Shopfolio.Infrastructure;

namespace Shopfolio.Modules.Catalog;

/// <summary>
/// The categories a product hangs on and the primary category mark (06 4.10). It goes with its product; the link to the category is restrictive,
/// so a category that holds a product is not deleted. At most one link per product is primary.
/// </summary>
internal sealed class ProductCategory
{
    public Guid Id { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public Guid ProductId { get; set; }

    public Guid CategoryId { get; set; }

    public bool IsPrimaryCategory { get; set; }

    public static ProductCategory Create(Guid productId, Guid categoryId, bool isPrimaryCategory, DateTimeOffset now)
    {
        return new ProductCategory { Id = EntityIds.New(), CreatedAt = now, ProductId = productId, CategoryId = categoryId, IsPrimaryCategory = isPrimaryCategory };
    }
}

internal sealed class ProductCategoryConfiguration : IEntityTypeConfiguration<ProductCategory>
{
    public void Configure(EntityTypeBuilder<ProductCategory> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.IsPrimaryCategory).HasDefaultValue(false);
        builder.HasOne<Product>().WithMany().HasForeignKey(p => p.ProductId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<Category>().WithMany().HasForeignKey(p => p.CategoryId).OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(p => new { p.ProductId, p.CategoryId }).IsUnique();
        builder.HasIndex(p => p.ProductId).IsUnique().HasFilter("is_primary_category");
    }
}
