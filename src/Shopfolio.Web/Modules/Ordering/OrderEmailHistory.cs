using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Shopfolio.Infrastructure;

namespace Shopfolio.Modules.Ordering;

/// <summary>
/// The correction of a guest order's contact e-mail (06 4.61): the old and the new address, the administrator and the moment, kept on the
/// order's own record. Only appended (<see cref="OrderingRecordsGuard"/>); the one change is the database emptying the administrator link when
/// the administrator is removed. The order link is restrictive (the disposal deletes the rows explicitly); the administrator link is emptied.
/// </summary>
internal sealed class OrderEmailHistory
{
    public Guid Id { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public Guid OrderId { get; set; }

    /// <summary>The address before the correction, as it was entered.</summary>
    public string OldEmail { get; set; } = string.Empty;

    /// <summary>The address after the correction; written with the order's contact e-mail in one transaction.</summary>
    public string NewEmail { get; set; } = string.Empty;

    /// <summary>The administrator who corrected; emptied when the administrator is removed.</summary>
    public Guid? AdminId { get; set; }

    /// <summary>The administrator's name, as a value.</summary>
    public string AdminName { get; set; } = string.Empty;

    public static OrderEmailHistory Create(Guid orderId, string oldEmail, string newEmail, Guid adminId, string adminName, DateTimeOffset now)
    {
        return new OrderEmailHistory
        {
            Id = EntityIds.New(),
            CreatedAt = now,
            OrderId = orderId,
            OldEmail = oldEmail,
            NewEmail = newEmail,
            AdminId = adminId,
            AdminName = adminName,
        };
    }
}

internal sealed class OrderEmailHistoryConfiguration : IEntityTypeConfiguration<OrderEmailHistory>
{
    // The administrator is another module's entity (09 6 item 2; 10.1 item 1): the link is a database foreign key, so the principal is named.
    private const string AdministrationAdminType = "Shopfolio.Modules.Administration.Admin";

    public void Configure(EntityTypeBuilder<OrderEmailHistory> builder)
    {
        builder.HasKey(h => h.Id);
        builder.Property(h => h.OldEmail).HasColumnType("text");
        builder.Property(h => h.NewEmail).HasColumnType("text");
        builder.Property(h => h.AdminName).HasColumnType("text");

        // The order link is restrictive; the administrator link is emptied (06 7.2; 2.2.5).
        builder.HasOne<Order>().WithMany().HasForeignKey(h => h.OrderId).OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(AdministrationAdminType, navigationName: null).WithMany().HasForeignKey(nameof(OrderEmailHistory.AdminId)).OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(h => new { h.OrderId, h.CreatedAt });
        builder.HasIndex(h => h.AdminId);
    }
}
