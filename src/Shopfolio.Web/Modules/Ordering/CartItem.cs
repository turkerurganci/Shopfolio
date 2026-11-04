using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.DataModel;

namespace Shopfolio.Modules.Ordering;

/// <summary>
/// A variant and its quantity in a cart (06 4.16). The item is live: it reads its price and availability from the current product and keeps
/// only the unit price of the moment the variant was first added. An item taken off the storefront carries the product's name
/// (<see cref="RemovedProductName"/>); an item whose variant link is empty must carry it, so a permanent deletion that empties the link
/// without writing the mark is stopped by the database. A variant is one unmarked item in a cart (a filtered unique index).
/// </summary>
internal sealed class CartItem
{
    public Guid Id { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public Guid CartId { get; set; }

    /// <summary>The item's variant; emptied when the variant is permanently deleted.</summary>
    public Guid? VariantId { get; set; }

    public int Quantity { get; set; }

    /// <summary>The unit price of the moment the variant was first added to the cart; a quantity increase does not change it.</summary>
    public decimal FirstAddedUnitPrice { get; set; }

    /// <summary>The mark of an item taken off the storefront: the product's name, written in the operation that takes it off.</summary>
    public string? RemovedProductName { get; set; }

    /// <summary>The mark of an item or quantity the sign-in merge added (06 4.16; K-1183).</summary>
    public bool IsMergeNoticePending { get; set; }

    public static CartItem Create(Guid cartId, Guid variantId, int quantity, decimal firstAddedUnitPrice, DateTimeOffset now)
    {
        return new CartItem
        {
            Id = EntityIds.New(),
            CreatedAt = now,
            CartId = cartId,
            VariantId = variantId,
            Quantity = quantity,
            FirstAddedUnitPrice = firstAddedUnitPrice,
        };
    }
}

internal sealed class CartItemConfiguration : IEntityTypeConfiguration<CartItem>
{
    // The variant is another module's entity (09 6 item 2; 10.1 item 1): the link is a database foreign key, so the principal is named.
    private const string CatalogVariantType = "Shopfolio.Modules.Catalog.Variant";

    public void Configure(EntityTypeBuilder<CartItem> builder)
    {
        builder.HasKey(i => i.Id);
        builder.Property(i => i.FirstAddedUnitPrice).AsMoney();
        builder.Property(i => i.RemovedProductName).HasColumnType("text");
        builder.Property(i => i.IsMergeNoticePending).HasDefaultValue(false);

        // The items go with the cart; the variant link is emptied (06 7.2; 2.2.5; K-1044 (3)).
        builder.HasOne<Cart>().WithMany().HasForeignKey(i => i.CartId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(CatalogVariantType, navigationName: null).WithMany().HasForeignKey(nameof(CartItem.VariantId)).OnDelete(DeleteBehavior.SetNull);

        // The filtered unique index does not cover the marked items, so the cart's items and its cascade read their own index (K-1090).
        builder.HasIndex(i => new { i.CartId, i.VariantId }).IsUnique().HasFilter("removed_product_name IS NULL");
        builder.HasIndex(i => i.CartId);

        builder.HasRule("quantity_above_zero", "quantity > 0");
        builder.HasRule("first_added_unit_price_above_zero", "first_added_unit_price > 0");
        builder.HasRule("removed_name_without_variant", "variant_id IS NOT NULL OR removed_product_name IS NOT NULL");
    }
}
