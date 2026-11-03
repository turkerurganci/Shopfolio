using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.DataModel;

namespace Shopfolio.Modules.Catalog;

/// <summary>The option dimension of a product ("Color", "Size"), at most two per product (06 4.7). It goes with its product.</summary>
internal sealed class Option
{
    public Guid Id { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public Guid ProductId { get; set; }

    public string Name { get; set; } = string.Empty;

    /// <summary>1 or 2: the variant's first and second value link follow this order.</summary>
    public int SortOrder { get; set; }

    public static Option Create(Guid productId, string name, int sortOrder, DateTimeOffset now)
    {
        return new Option { Id = EntityIds.New(), CreatedAt = now, ProductId = productId, Name = name, SortOrder = sortOrder };
    }
}

internal sealed class OptionConfiguration : IEntityTypeConfiguration<Option>
{
    public void Configure(EntityTypeBuilder<Option> builder)
    {
        builder.HasKey(o => o.Id);
        builder.HasOne<Product>().WithMany().HasForeignKey(o => o.ProductId).OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(o => new { o.ProductId, o.SortOrder }).IsUnique();
        builder.HasRule("sort_order_fence", $"sort_order BETWEEN 1 AND {ProductConstants.OptionDimensionMax}");
    }
}
