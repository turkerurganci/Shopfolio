using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.DataModel;

namespace Shopfolio.Modules.AfterSales;

/// <summary>
/// The firm's removal of an item, or of some of its units, from the panel after the payment's confirmation (06 4.23): a correction that
/// asks for no reason; the removed units count as cancelled on the shipping line. Only an administrator removes. The record is never
/// deleted (<see cref="AfterSalesRecordsGuard"/>): the item link is restrictive and the administrator link is emptied (06 7.2; 2.2.5).
/// </summary>
internal sealed class ItemRemoval
{
    public Guid Id { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public Guid OrderItemId { get; set; }

    public int Quantity { get; set; }

    /// <summary>The administrator who removed; emptied when the administrator is removed and by the disposal.</summary>
    public Guid? AdminId { get; set; }

    /// <summary>The administrator's name as a value; emptied by the disposal.</summary>
    public string? AdminName { get; set; }

    public decimal ProductAmount { get; set; }

    public decimal CouponShare { get; set; }

    public decimal VatAmount { get; set; }

    public static ItemRemoval Create(Guid orderItemId, int quantity, Guid adminId, string adminName, ItemAmounts amounts, DateTimeOffset now)
    {
        return new ItemRemoval
        {
            Id = EntityIds.New(),
            CreatedAt = now,
            OrderItemId = orderItemId,
            Quantity = quantity,
            AdminId = adminId,
            AdminName = adminName,
            ProductAmount = amounts.ProductAmount,
            CouponShare = amounts.CouponShare,
            VatAmount = amounts.VatAmount,
        };
    }
}

internal sealed class ItemRemovalConfiguration : IEntityTypeConfiguration<ItemRemoval>
{
    // The item and the administrator are other modules' entities (09 6 item 2; 10.1 item 1): the links are database foreign keys, so the
    // principals are named.
    private const string OrderingOrderItemType = "Shopfolio.Modules.Ordering.OrderItem";
    private const string AdministrationAdminType = "Shopfolio.Modules.Administration.Admin";

    public void Configure(EntityTypeBuilder<ItemRemoval> builder)
    {
        builder.HasKey(r => r.Id);
        builder.Property(r => r.AdminName).HasColumnType("text");
        builder.Property(r => r.ProductAmount).AsMoney();
        builder.Property(r => r.CouponShare).AsMoney().HasDefaultValue(0m);
        builder.Property(r => r.VatAmount).AsMoney();

        // The item link is restrictive; the administrator link is emptied (06 7.2; 2.2.5).
        builder.HasOne(OrderingOrderItemType, navigationName: null).WithMany().HasForeignKey(nameof(ItemRemoval.OrderItemId)).OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(AdministrationAdminType, navigationName: null).WithMany().HasForeignKey(nameof(ItemRemoval.AdminId)).OnDelete(DeleteBehavior.SetNull);

        ItemRecordRules.HasQuantityAndAmountRules(builder);
    }
}
