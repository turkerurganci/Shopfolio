using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Shopfolio.UnitTests.Samples.EnumWithoutText;

/// <summary>An enum that is not one of the application's: the context's rule does not convert it, so it would be stored as a number.</summary>
public enum SampleColor
{
    Red,
    Green,
}

/// <summary>Deliberate violation sample of the model rules (09 19.2 item 3): an enum property that is not stored as text.</summary>
public sealed class UnconvertedColor
{
    public Guid Id { get; set; }

    public SampleColor Color { get; set; }
}

internal sealed class UnconvertedColorConfiguration : IEntityTypeConfiguration<UnconvertedColor>
{
    public void Configure(EntityTypeBuilder<UnconvertedColor> builder)
    {
        builder.ToTable("unconverted_colors");
        builder.HasKey(c => c.Id);
    }
}
