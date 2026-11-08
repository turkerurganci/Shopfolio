using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.DataModel;

namespace Shopfolio.Modules.Catalog;

/// <summary>
/// A node of the three-level category tree (06 4.9), an adjacency list. A category is not deleted while it holds a product or a child category:
/// the link to the parent and the product link are restrictive. Its address is fixed when it is created and it carries no version.
/// </summary>
internal sealed class Category
{
    public Guid Id { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public Guid? ParentCategoryId { get; set; }

    public string Name { get; set; } = string.Empty;

    /// <summary>1 at the root, the parent's level plus one below; written only by the category tree (09 9.4 item 4).</summary>
    public int Level { get; private set; }

    public string Slug { get; set; } = string.Empty;

    /// <summary>Puts the category under another parent at another level; the only way a level changes after the category is created, called by the tree's move under its lock.</summary>
    public void Relocate(Guid? parentCategoryId, int level)
    {
        ParentCategoryId = parentCategoryId;
        Level = level;
    }

    public static Category Create(string name, string slug, Guid? parentCategoryId, int level, DateTimeOffset now)
    {
        return new Category { Id = EntityIds.New(), CreatedAt = now, Name = name, Slug = slug, ParentCategoryId = parentCategoryId, Level = level };
    }
}

internal sealed class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.HasKey(c => c.Id);
        builder.HasOne<Category>().WithMany().HasForeignKey(c => c.ParentCategoryId).OnDelete(DeleteBehavior.NoAction);

        // The children of a node are read alphabetically; the self reference is checked through this index as well.
        builder.HasIndex(c => new { c.ParentCategoryId, c.Name });
        builder.HasIndex(c => c.Slug).IsUnique();

        builder.HasRule("level_fence", $"level BETWEEN 1 AND {ProductConstants.CategoryLevelMax}");
        builder.HasRule("level_follows_parent", "(parent_category_id IS NULL) = (level = 1)");
        builder.HasRule("slug_format", "slug ~ '^[a-z0-9]+(-[a-z0-9]+)*$'");
    }
}
