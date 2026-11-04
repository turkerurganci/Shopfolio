using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.DataModel;

namespace Shopfolio.Modules.AfterSales;

/// <summary>
/// The return rejection (06 4.27): on an item frozen with a conditional withdrawal exemption, the firm refuses the good whose protective
/// packaging was opened instead of receiving it; the rejection carries the received units and the arrival date. No refund is paid and the
/// rejection is never taken back. The record is never deleted (<see cref="AfterSalesRecordsGuard"/>): the item and withdrawal links are
/// restrictive, the administrator link is emptied (06 7.2; 2.2.5).
/// </summary>
internal sealed class ReturnRejection
{
    public Guid Id { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public Guid OrderItemId { get; set; }

    public Guid WithdrawalId { get; set; }

    public int Quantity { get; set; }

    /// <summary>The date the good arrived, within the receipt's bounds (06 4.26).</summary>
    public DateOnly ArrivalDate { get; set; }

    /// <summary>The administrator who rejected; emptied when the administrator is removed and by the disposal.</summary>
    public Guid? AdminId { get; set; }

    /// <summary>The administrator's name as a value; emptied by the disposal.</summary>
    public string? AdminName { get; set; }

    public static ReturnRejection Create(
        Guid orderItemId,
        Guid withdrawalId,
        int quantity,
        DateOnly arrivalDate,
        Guid adminId,
        string adminName,
        DateTimeOffset now)
    {
        return new ReturnRejection
        {
            Id = EntityIds.New(),
            CreatedAt = now,
            OrderItemId = orderItemId,
            WithdrawalId = withdrawalId,
            Quantity = quantity,
            ArrivalDate = arrivalDate,
            AdminId = adminId,
            AdminName = adminName,
        };
    }
}

internal sealed class ReturnRejectionConfiguration : IEntityTypeConfiguration<ReturnRejection>
{
    // The item and the administrator are other modules' entities (09 6 item 2; 10.1 item 1): the links are database foreign keys, so the
    // principals are named.
    private const string OrderingOrderItemType = "Shopfolio.Modules.Ordering.OrderItem";
    private const string AdministrationAdminType = "Shopfolio.Modules.Administration.Admin";

    public void Configure(EntityTypeBuilder<ReturnRejection> builder)
    {
        builder.HasKey(r => r.Id);
        builder.Property(r => r.AdminName).HasColumnType("text");

        // The item and withdrawal links are restrictive; the administrator link is emptied (06 7.2; 2.2.5).
        builder.HasOne(OrderingOrderItemType, navigationName: null).WithMany().HasForeignKey(nameof(ReturnRejection.OrderItemId)).OnDelete(DeleteBehavior.NoAction);
        builder.HasOne<Withdrawal>().WithMany().HasForeignKey(r => r.WithdrawalId).OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(AdministrationAdminType, navigationName: null).WithMany().HasForeignKey(nameof(ReturnRejection.AdminId)).OnDelete(DeleteBehavior.SetNull);

        ItemRecordRules.HasQuantityRule(builder);
    }
}
