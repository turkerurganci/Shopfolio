using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Shopfolio.Infrastructure;

namespace Shopfolio.UnitTests.Samples.Good;

/// <summary>A sample dependent record with the three delete behaviors of 06 2.2.5 and one relationship nobody configured.</summary>
public sealed class SampleItem
{
    public Guid Id { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>Cascading: the item goes with its owner.</summary>
    public Guid OwnerId { get; set; }

    /// <summary>Emptied: the link is set to null when the other owner goes.</summary>
    public Guid? EmptiedOwnerId { get; set; }

    /// <summary>Restrictive: the other owner cannot be deleted while an item points to it.</summary>
    public Guid RestrictiveOwnerId { get; set; }

    /// <summary>No delete behavior is written for this relationship; the model rule makes it <c>NoAction</c>.</summary>
    public Guid UnwrittenOwnerId { get; set; }

    public string? Description { get; set; }

    public static SampleItem Create(Guid ownerId, Guid emptiedOwnerId, Guid restrictiveOwnerId, Guid unwrittenOwnerId, DateTimeOffset now)
    {
        return new SampleItem
        {
            Id = EntityIds.New(),
            CreatedAt = now,
            OwnerId = ownerId,
            EmptiedOwnerId = emptiedOwnerId,
            RestrictiveOwnerId = restrictiveOwnerId,
            UnwrittenOwnerId = unwrittenOwnerId,
        };
    }
}

internal sealed class SampleItemConfiguration : IEntityTypeConfiguration<SampleItem>
{
    public void Configure(EntityTypeBuilder<SampleItem> builder)
    {
        builder.ToTable("sample_items");
        builder.HasKey(i => i.Id);
        builder.HasOne<SampleOwner>().WithMany().HasForeignKey(i => i.OwnerId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<SampleOwner>().WithMany().HasForeignKey(i => i.EmptiedOwnerId).OnDelete(DeleteBehavior.SetNull);
        builder.HasOne<SampleOwner>().WithMany().HasForeignKey(i => i.RestrictiveOwnerId).OnDelete(DeleteBehavior.NoAction);
        builder.HasOne<SampleOwner>().WithMany().HasForeignKey(i => i.UnwrittenOwnerId);
    }
}
