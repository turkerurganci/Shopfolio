using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.DataModel;
using Shopfolio.Modules.Catalog.Contracts;

namespace Shopfolio.Modules.Catalog;

/// <summary>
/// The sold unit (06 4.6): stock code, price, net quantity, publication status, stock or capacity and the reserved quantity live here. A product
/// without options has one variant; a variant is bound to at most two option values. It is deleted with its product; the links to the option
/// values and the image are restrictive, which the same-statement cascade of a product deletion does not trip.
/// </summary>
internal sealed class Variant
{
    public Guid Id { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public Guid ProductId { get; set; }

    public Guid? FirstOptionValueId { get; set; }

    public Guid? SecondOptionValueId { get; set; }

    /// <summary>The stock code as typed; shown as typed. Written only with its normalized form by <see cref="SetSku"/>.</summary>
    public string Sku { get; private set; } = string.Empty;

    /// <summary>The normalized stock code: the column of the unique index (09 9.4 item 1).</summary>
    public string NormalizedSku { get; private set; } = string.Empty;

    /// <summary>Price, VAT included; above zero.</summary>
    public decimal Price { get; set; }

    /// <summary>Net quantity in the product's measurement unit; filled when the product has a unit.</summary>
    public decimal? NetQuantity { get; set; }

    public PublishStatus PublishStatus { get; set; } = PublishStatus.Draft;

    /// <summary>Stock (physical) or capacity (service); empty is unlimited (service) or not applicable (digital).</summary>
    public int? StockQuantity { get; set; }

    /// <summary>Written only by the transition unit (06 4.6, 4.14); the model keeps it at zero or above and not above the stock.</summary>
    public int ReservedQuantity { get; private set; }

    public Guid? ImageId { get; set; }

    public DateTimeOffset? FirstOnSaleAt { get; set; }

    public static Variant Create(Guid productId, string sku, decimal price, DateTimeOffset now)
    {
        Variant variant = new() { Id = EntityIds.New(), CreatedAt = now, ProductId = productId, Price = price };
        variant.SetSku(sku);
        return variant;
    }

    /// <summary>Writes the stock code and its normalized form together (06 2.8.2).</summary>
    public void SetSku(string sku)
    {
        ArgumentNullException.ThrowIfNull(sku);
        Sku = sku;
        NormalizedSku = TextNormalizer.NormalizeIdentity(sku);
    }
}

internal sealed class VariantConfiguration : IEntityTypeConfiguration<Variant>
{
    public void Configure(EntityTypeBuilder<Variant> builder)
    {
        builder.HasKey(v => v.Id);
        builder.Property(v => v.Sku).HasColumnType("text");
        builder.Property(v => v.NormalizedSku).HasColumnType("text");
        builder.Property(v => v.Price).AsMoney();
        builder.Property(v => v.NetQuantity).AsNetQuantity();

        builder.HasOne<Product>().WithMany().HasForeignKey(v => v.ProductId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<OptionValue>().WithMany().HasForeignKey(v => v.FirstOptionValueId).OnDelete(DeleteBehavior.NoAction);
        builder.HasOne<OptionValue>().WithMany().HasForeignKey(v => v.SecondOptionValueId).OnDelete(DeleteBehavior.NoAction);
        builder.HasOne<Image>().WithMany().HasForeignKey(v => v.ImageId).OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(v => v.NormalizedSku).IsUnique();

        // The combination is unique with empty values counted equal: the one variant of a product without options and the one variant per
        // value of a single-dimension product are kept by this index. Its first column is the product, so the product's variants are read by it.
        builder.HasIndex(v => new { v.ProductId, v.FirstOptionValueId, v.SecondOptionValueId }).IsUnique().AreNullsDistinct(false);

        builder.HasRule("second_value_needs_first", "second_option_value_id IS NULL OR first_option_value_id IS NOT NULL");
        builder.HasRule("price_above_zero", "price > 0");
        builder.HasRule("net_quantity_above_zero", "net_quantity IS NULL OR net_quantity > 0");
        builder.HasRule("reserved_quantity_not_negative", "reserved_quantity >= 0");
        builder.HasRule("stock_not_below_reserved", "stock_quantity IS NULL OR (stock_quantity >= 0 AND stock_quantity >= reserved_quantity)");
    }
}
