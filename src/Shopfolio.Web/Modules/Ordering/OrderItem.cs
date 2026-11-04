using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.DataModel;
using Shopfolio.Modules.AfterSales.Contracts;
using Shopfolio.Modules.Catalog.Contracts;

namespace Shopfolio.Modules.Ordering;

/// <summary>
/// A variant and its quantity in an order (06 4.18): frozen at confirmation; it tells itself without reading the live product. The item
/// carries no status; its open quantity is read from the item records (06 2.7.2). The fields that belong to one product type are held by
/// check constraints (K-1049), and the download counter never exceeds the frozen allowance (the invariant of its conditional update; K-980).
/// The item is never deleted (<see cref="OrderingRecordsGuard"/>); its product and variant links are emptied by the database, its image and
/// file links are restrictive and protect what they point to (K-1029).
/// </summary>
internal sealed class OrderItem
{
    public Guid Id { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public Guid OrderId { get; set; }

    /// <summary>The item's product; emptied when the product is permanently deleted.</summary>
    public Guid? ProductId { get; set; }

    /// <summary>The item's variant; emptied when the variant is permanently deleted.</summary>
    public Guid? VariantId { get; set; }

    /// <summary>The quantity; 1 on a digital item.</summary>
    public int Quantity { get; set; }

    /// <summary>The frozen product name, with the product name's ceiling (P-27).</summary>
    public string ProductName { get; set; } = string.Empty;

    public string? FirstOptionValue { get; set; }

    public string? SecondOptionValue { get; set; }

    /// <summary>The frozen image link; it protects the image from the disposal (K-1029 (2)).</summary>
    public Guid? ImageId { get; set; }

    /// <summary>The frozen product type: it decides the delivery line and the withdrawal regime.</summary>
    public ProductType ProductType { get; set; }

    /// <summary>The frozen unit price, VAT included, without the discount.</summary>
    public decimal UnitPrice { get; set; }

    public decimal? DiscountPercentage { get; set; }

    /// <summary>The frozen discounted unit price, rounded to the kuruş; never rounded again (K-1049).</summary>
    public decimal? DiscountedUnitPrice { get; set; }

    public decimal? ReferencePrice { get; set; }

    /// <summary>The coupon's share of the item; set and frozen in the item.</summary>
    public decimal CouponShare { get; set; }

    public decimal VatRate { get; set; }

    public decimal VatAmount { get; set; }

    /// <summary>The frozen withdrawal exemption; the mark is this field being filled. Only on a physical item.</summary>
    public WithdrawalExemptionReason? WithdrawalExemptionReason { get; set; }

    /// <summary>The frozen fulfilment period in days; filled exactly on a service item.</summary>
    public int? FulfilmentPeriod { get; set; }

    /// <summary>The frozen download allowance; filled exactly on a digital item.</summary>
    public int? DownloadAllowance { get; set; }

    /// <summary>The download counter: written only by a conditional update in the database (09 10.2 item 4), never read and written back.</summary>
    public int DownloadCount { get; private set; }

    /// <summary>The last known file link of a digital item; empty on any other item.</summary>
    public Guid? DigitalFileId { get; set; }

    /// <summary>The moment the delivery mark was set; the mark is this field being filled.</summary>
    public DateTimeOffset? DeliveryMarkedAt { get; set; }

    public DateOnly? DeliveryDate { get; set; }

    public static OrderItem Create(
        Guid orderId,
        Guid? productId,
        Guid? variantId,
        ProductType productType,
        string productName,
        int quantity,
        decimal unitPrice,
        decimal vatRate,
        decimal vatAmount,
        DateTimeOffset now)
    {
        return new OrderItem
        {
            Id = EntityIds.New(),
            CreatedAt = now,
            OrderId = orderId,
            ProductId = productId,
            VariantId = variantId,
            ProductType = productType,
            ProductName = productName,
            Quantity = quantity,
            UnitPrice = unitPrice,
            VatRate = vatRate,
            VatAmount = vatAmount,
        };
    }
}

internal sealed class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    // The product, variant, image and file are another module's entities (09 6 item 2; 10.1 item 1): the links are database foreign keys,
    // so the principals are named, not referenced.
    private const string CatalogProductType = "Shopfolio.Modules.Catalog.Product";
    private const string CatalogVariantType = "Shopfolio.Modules.Catalog.Variant";
    private const string CatalogImageType = "Shopfolio.Modules.Catalog.Image";
    private const string CatalogDigitalFileType = "Shopfolio.Modules.Catalog.DigitalFile";

    private const string Physical = nameof(ProductType.Physical);
    private const string Digital = nameof(ProductType.Digital);
    private const string Service = nameof(ProductType.Service);

    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.HasKey(i => i.Id);
        builder.Property(i => i.ProductName).HasMaxLength(ProductConstants.ProductNameMaxLength);
        builder.Property(i => i.FirstOptionValue).HasColumnType("text");
        builder.Property(i => i.SecondOptionValue).HasColumnType("text");
        builder.Property(i => i.UnitPrice).AsMoney();
        builder.Property(i => i.DiscountPercentage).AsPercent();
        builder.Property(i => i.DiscountedUnitPrice).AsMoney();
        builder.Property(i => i.ReferencePrice).AsMoney();
        builder.Property(i => i.CouponShare).AsMoney().HasDefaultValue(0m);
        builder.Property(i => i.VatRate).AsPercent();
        builder.Property(i => i.VatAmount).AsMoney();
        builder.Property(i => i.DownloadCount).HasDefaultValue(0);

        // The order link is restrictive (the order side's links; K-1047); the product and variant links are emptied; the image and file
        // links are restrictive and protect them from the disposal (06 7.2; 2.2.5; K-1029).
        builder.HasOne<Order>().WithMany().HasForeignKey(i => i.OrderId).OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(CatalogProductType, navigationName: null).WithMany().HasForeignKey(nameof(OrderItem.ProductId)).OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(CatalogVariantType, navigationName: null).WithMany().HasForeignKey(nameof(OrderItem.VariantId)).OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(CatalogImageType, navigationName: null).WithMany().HasForeignKey(nameof(OrderItem.ImageId)).OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(CatalogDigitalFileType, navigationName: null).WithMany().HasForeignKey(nameof(OrderItem.DigitalFileId)).OnDelete(DeleteBehavior.NoAction);

        builder.HasRule("quantity_above_zero_one_if_digital", $"quantity > 0 AND (product_type <> '{Digital}' OR quantity = 1)");
        builder.HasRule("unit_price_above_zero", "unit_price > 0");
        builder.HasRule("discount_fields_together", "num_nonnulls(discount_percentage, discounted_unit_price, reference_price) IN (0, 3)");
        builder.HasRule("discount_percentage_range", "discount_percentage IS NULL OR (discount_percentage > 0 AND discount_percentage < 100)");
        builder.HasRule("discounted_unit_price_range", "discounted_unit_price IS NULL OR (discounted_unit_price > 0 AND discounted_unit_price < unit_price)");
        builder.HasRule("reference_price_above_zero", "reference_price IS NULL OR reference_price > 0");
        builder.HasRule("coupon_share_not_negative", "coupon_share >= 0");
        builder.HasRule("vat_rate_range", "vat_rate >= 0 AND vat_rate < 100");
        builder.HasRule("vat_amount_not_negative", "vat_amount >= 0");

        // The fields of one product type (K-1049): the type is in the same row.
        builder.HasRule("withdrawal_exemption_only_physical", $"withdrawal_exemption_reason IS NULL OR product_type = '{Physical}'");
        builder.HasRule("fulfilment_period_only_service", $"(fulfilment_period IS NOT NULL) = (product_type = '{Service}')");
        builder.HasRule("download_allowance_only_digital", $"(download_allowance IS NOT NULL) = (product_type = '{Digital}')");
        builder.HasRule("digital_file_only_digital", $"digital_file_id IS NULL OR product_type = '{Digital}'");
        builder.HasRule(
            "download_count_within_allowance",
            $"download_count >= 0 AND (product_type = '{Digital}' OR download_count = 0) AND (download_allowance IS NULL OR download_count <= download_allowance)");
        builder.HasRule("second_option_needs_first", "second_option_value IS NULL OR first_option_value IS NOT NULL");
        builder.HasRule("delivery_mark_and_date_together", "(delivery_marked_at IS NULL) = (delivery_date IS NULL)");
    }
}
