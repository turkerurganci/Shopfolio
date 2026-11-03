using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Shopfolio.UnitTests.Samples.UntypedDecimal;

/// <summary>Deliberate violation sample of the model rules (09 19.2 item 3): a decimal property that does not say money, percent or net quantity.</summary>
public sealed class UntypedPrice
{
    public Guid Id { get; set; }

    public decimal Amount { get; set; }
}

internal sealed class UntypedPriceConfiguration : IEntityTypeConfiguration<UntypedPrice>
{
    public void Configure(EntityTypeBuilder<UntypedPrice> builder)
    {
        builder.ToTable("untyped_prices");
        builder.HasKey(p => p.Id);
    }
}
