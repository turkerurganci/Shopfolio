using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Shopfolio.UnitTests.Samples.Violating;

/// <summary>Deliberate violation sample of the model rules (09 19.2 item 3): a table name that breaks the name pattern and no creation time.</summary>
public sealed class BadParent
{
    public Guid Id { get; set; }
}

/// <summary>Deliberate violation sample: delete behaviors that the rule forbids.</summary>
public sealed class BadChild
{
    public Guid Id { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public Guid RestrictParentId { get; set; }

    public Guid ClientCascadeParentId { get; set; }
}

internal sealed class BadParentConfiguration : IEntityTypeConfiguration<BadParent>
{
    public void Configure(EntityTypeBuilder<BadParent> builder)
    {
        builder.ToTable("Bad_Parents");
        builder.HasKey(p => p.Id);
    }
}

internal sealed class BadChildConfiguration : IEntityTypeConfiguration<BadChild>
{
    public void Configure(EntityTypeBuilder<BadChild> builder)
    {
        builder.ToTable("bad_children");
        builder.HasKey(c => c.Id);
        builder.HasOne<BadParent>().WithMany().HasForeignKey(c => c.RestrictParentId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<BadParent>().WithMany().HasForeignKey(c => c.ClientCascadeParentId).OnDelete(DeleteBehavior.ClientCascade);
    }
}
