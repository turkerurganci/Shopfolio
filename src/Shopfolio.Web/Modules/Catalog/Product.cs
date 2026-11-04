using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.DataModel;
using Shopfolio.Modules.AfterSales.Contracts;
using Shopfolio.Modules.Catalog.Contracts;

namespace Shopfolio.Modules.Catalog;

/// <summary>
/// The showcase and catalog unit (06 4.5): it is not sold itself, the variant is. Name, description, type, VAT rate, origin, measurement
/// unit, the product-level periods and limit and the withdrawal exemption live here; stock, price and net quantity live on the variant.
/// Deleting a product is physical and cascades to its variants, options, category links, image links (and, in later tasks, its discount).
/// </summary>
internal sealed class Product
{
    public Guid Id { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public string Name { get; private set; } = string.Empty;

    /// <summary>The searched form of the name; written only together with the name by <see cref="SetName"/> (06 2.7.1).</summary>
    public string NameSearchForm { get; private set; } = string.Empty;

    public string? Description { get; set; }

    public ProductType ProductType { get; set; }

    public PublishStatus PublishStatus { get; set; } = PublishStatus.Draft;

    /// <summary>VAT rate in percent: 0 up to, not including, 100.</summary>
    public decimal VatRate { get; set; }

    public Guid? OriginCountryId { get; set; }

    public MeasurementUnit? MeasurementUnit { get; set; }

    /// <summary>Shipping period in business days; empty inherits the company default (P-6).</summary>
    public int? ShippingPeriod { get; set; }

    /// <summary>Fulfilment period of a service product, in days (P-44).</summary>
    public int? FulfilmentPeriod { get; set; }

    /// <summary>Most units in one order across all variants; empty means the only limit is stock (P-9).</summary>
    public int? MaxQuantityPerOrder { get; set; }

    /// <summary>The reason of the withdrawal exemption; the exemption is this field being filled (06 4.5).</summary>
    public WithdrawalExemptionReason? WithdrawalExemptionReason { get; set; }

    public string Slug { get; set; } = string.Empty;

    /// <summary>The moment the address was fixed (the first publication); filled means the address no longer changes.</summary>
    public DateTimeOffset? SlugFixedAt { get; set; }

    /// <summary>The version of the product form (06 2.6.2); a concurrency token.</summary>
    public int Version { get; private set; } = 1;

    public static Product Create(string name, string slug, ProductType productType, decimal vatRate, DateTimeOffset now)
    {
        Product product = new() { Id = EntityIds.New(), CreatedAt = now, Slug = slug, ProductType = productType, VatRate = vatRate };
        product.SetName(name);
        return product;
    }

    /// <summary>Writes the name and its searched form together (06 2.7.1).</summary>
    public void SetName(string name)
    {
        ArgumentNullException.ThrowIfNull(name);
        Name = name;
        NameSearchForm = TextNormalizer.Simplify(name);
    }

    public void BumpVersion() => Version++;
}

internal sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    private const string CountryEntity = "Shopfolio.Modules.Settings.Country";

    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Name).HasMaxLength(ProductConstants.ProductNameMaxLength);
        builder.Property(p => p.NameSearchForm).HasColumnType("text");
        builder.Property(p => p.Description).HasColumnType("text");
        builder.Property(p => p.VatRate).AsPercent();
        builder.Property(p => p.PublishStatus).HasDefaultValue(PublishStatus.Draft);
        builder.Property(p => p.Version).HasDefaultValue(1);

        // The country list belongs to the Settings module, whose types a module reaches only through Contracts (09 6 item 2): the
        // relationship is therefore declared by the entity's name, not by its type. A restrictive link (06 4.5).
        builder.HasOne(CountryEntity, navigationName: null)
            .WithMany()
            .HasForeignKey(nameof(Product.OriginCountryId))
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(p => p.Slug).IsUnique();
        builder.HasIndex(p => p.NameSearchForm).HasMethod("gin").HasOperators("gin_trgm_ops");
        builder.HasIndex(p => new { p.PublishStatus, p.CreatedAt, p.Id });

        builder.HasRule("vat_rate_range", "vat_rate >= 0 AND vat_rate < 100");
        builder.HasRule("shipping_period_fence", $"shipping_period BETWEEN 0 AND {ParameterFences.ShippingPeriodUpper}");
        builder.HasRule("fulfilment_period_fence", $"fulfilment_period >= {ParameterFences.FulfilmentPeriodLower}");
        builder.HasRule("max_quantity_per_order_fence", $"max_quantity_per_order >= {ParameterFences.MaxQuantityPerOrderLower}");
        builder.HasRule("slug_format", "slug ~ '^[a-z0-9]+(-[a-z0-9]+)*$'");
    }
}
