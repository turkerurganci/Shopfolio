using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.DataModel;

namespace Shopfolio.Modules.AfterSales;

/// <summary>
/// The "good not returned" closure (06 4.28): after the sending period of a withdrawal after delivery, the firm closes the declaration's
/// units that did not arrive. A later receipt reopens the closure in the same transaction; the closure is not deleted. The record is never
/// deleted (<see cref="AfterSalesRecordsGuard"/>): the item and withdrawal links are restrictive, the administrator link is emptied
/// (06 7.2; 2.2.5).
/// </summary>
internal sealed class NotReturnedClosure
{
    public Guid Id { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public Guid OrderItemId { get; set; }

    public Guid WithdrawalId { get; set; }

    public int Quantity { get; set; }

    /// <summary>The administrator who closed; emptied when the administrator is removed and by the disposal.</summary>
    public Guid? AdminId { get; set; }

    /// <summary>The administrator's name as a value; emptied by the disposal.</summary>
    public string? AdminName { get; set; }

    /// <summary>The moment a later receipt reopened the closure.</summary>
    public DateTimeOffset? ReopenedAt { get; set; }

    public static NotReturnedClosure Create(Guid orderItemId, Guid withdrawalId, int quantity, Guid adminId, string adminName, DateTimeOffset now)
    {
        return new NotReturnedClosure
        {
            Id = EntityIds.New(),
            CreatedAt = now,
            OrderItemId = orderItemId,
            WithdrawalId = withdrawalId,
            Quantity = quantity,
            AdminId = adminId,
            AdminName = adminName,
        };
    }
}

internal sealed class NotReturnedClosureConfiguration : IEntityTypeConfiguration<NotReturnedClosure>
{
    // The item and the administrator are other modules' entities (09 6 item 2; 10.1 item 1): the links are database foreign keys, so the
    // principals are named.
    private const string OrderingOrderItemType = "Shopfolio.Modules.Ordering.OrderItem";
    private const string AdministrationAdminType = "Shopfolio.Modules.Administration.Admin";

    public void Configure(EntityTypeBuilder<NotReturnedClosure> builder)
    {
        builder.HasKey(c => c.Id);
        builder.Property(c => c.AdminName).HasColumnType("text");

        // The item and withdrawal links are restrictive; the administrator link is emptied (06 7.2; 2.2.5).
        builder.HasOne(OrderingOrderItemType, navigationName: null).WithMany().HasForeignKey(nameof(NotReturnedClosure.OrderItemId)).OnDelete(DeleteBehavior.NoAction);
        builder.HasOne<Withdrawal>().WithMany().HasForeignKey(c => c.WithdrawalId).OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(AdministrationAdminType, navigationName: null).WithMany().HasForeignKey(nameof(NotReturnedClosure.AdminId)).OnDelete(DeleteBehavior.SetNull);

        ItemRecordRules.HasQuantityRule(builder);
    }
}
