using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.DataModel;

namespace Shopfolio.Modules.Ordering;

/// <summary>
/// The stock, service capacity or coupon right an order reserved at its confirmation (06 4.14). A row lives only while the reservation lasts:
/// the final deduction and the release delete it. A variant row carries the variant and the quantity; a coupon row carries the coupon and the
/// quantity 1. The variant link cascades (the catalog's deletion takes the reservation); the order and coupon links are restrictive.
/// </summary>
internal sealed class StockReservation
{
    public Guid Id { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public Guid OrderId { get; set; }

    /// <summary>The variant of a stock or capacity reservation; empty on a coupon right's row.</summary>
    public Guid? VariantId { get; set; }

    /// <summary>The coupon of a coupon right's row; empty on a variant reservation's row.</summary>
    public Guid? CouponId { get; set; }

    public int Quantity { get; set; }

    public static StockReservation ForVariant(Guid orderId, Guid variantId, int quantity, DateTimeOffset now)
    {
        return new StockReservation { Id = EntityIds.New(), CreatedAt = now, OrderId = orderId, VariantId = variantId, Quantity = quantity };
    }

    public static StockReservation ForCoupon(Guid orderId, Guid couponId, DateTimeOffset now)
    {
        return new StockReservation { Id = EntityIds.New(), CreatedAt = now, OrderId = orderId, CouponId = couponId, Quantity = 1 };
    }
}

internal sealed class StockReservationConfiguration : IEntityTypeConfiguration<StockReservation>
{
    // The variant and the coupon are another module's entities (09 6 item 2; 10.1 item 1): the links are database foreign keys, so the principals are named.
    private const string CatalogVariantType = "Shopfolio.Modules.Catalog.Variant";
    private const string CatalogCouponType = "Shopfolio.Modules.Catalog.Coupon";

    public void Configure(EntityTypeBuilder<StockReservation> builder)
    {
        builder.HasKey(r => r.Id);

        // The variant link cascades (K-1043); the order and coupon links are restrictive: an order and a coupon are never deleted (06 7.2).
        builder.HasOne<Order>().WithMany().HasForeignKey(r => r.OrderId).OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(CatalogVariantType, navigationName: null).WithMany().HasForeignKey(nameof(StockReservation.VariantId)).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(CatalogCouponType, navigationName: null).WithMany().HasForeignKey(nameof(StockReservation.CouponId)).OnDelete(DeleteBehavior.NoAction);

        // Two filtered unique indexes start with the order and read the order's rows (06 7.1, 7.5).
        builder.HasIndex(r => new { r.OrderId, r.VariantId }).IsUnique().HasFilter("variant_id IS NOT NULL");
        builder.HasIndex(r => r.OrderId).IsUnique().HasFilter("coupon_id IS NOT NULL");
        builder.HasIndex(r => r.VariantId);
        builder.HasIndex(r => r.CouponId);

        builder.HasRule("exactly_one_of_variant_and_coupon", "num_nonnulls(variant_id, coupon_id) = 1");
        builder.HasRule("quantity_above_zero_one_if_coupon", "quantity > 0 AND (coupon_id IS NULL OR quantity = 1)");
    }
}
