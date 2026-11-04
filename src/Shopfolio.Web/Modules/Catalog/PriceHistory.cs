using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.DataModel;

namespace Shopfolio.Modules.Catalog;

/// <summary>
/// Every price applied to a variant, the discounted price included, and the moment it started to apply (06 4.11). The list is only appended
/// to: a row is never updated (06 2.2.3) - every value has a private setter and <see cref="CatalogRecordsGuard"/> refuses a changed row before
/// any SQL is sent. The reference price and the "price applies since" date are read from it, not stored. It goes with its variant.
/// </summary>
internal sealed class PriceHistory
{
    public Guid Id { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public Guid VariantId { get; private set; }

    /// <summary>The applied price: above zero; in a discount the discounted price rounded to the kuruş.</summary>
    public decimal Price { get; private set; }

    /// <summary>The moment the price started to apply.</summary>
    public DateTimeOffset StartedAt { get; private set; }

    public static PriceHistory Create(Guid variantId, decimal price, DateTimeOffset startedAt, DateTimeOffset now)
    {
        return new PriceHistory { Id = EntityIds.New(), CreatedAt = now, VariantId = variantId, Price = price, StartedAt = startedAt };
    }
}

internal sealed class PriceHistoryConfiguration : IEntityTypeConfiguration<PriceHistory>
{
    public void Configure(EntityTypeBuilder<PriceHistory> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Price).AsMoney();
        builder.HasOne<Variant>().WithMany().HasForeignKey(p => p.VariantId).OnDelete(DeleteBehavior.Cascade);

        // The window of the reference price and the last row are read by it; its first column serves the cascade of the variant.
        builder.HasIndex(p => new { p.VariantId, p.StartedAt });

        builder.HasRule("price_above_zero", "price > 0");
    }
}
