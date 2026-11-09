using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.DataModel;
using Shopfolio.Modules.Ordering.Contracts;

namespace Shopfolio.Modules.Ordering;

/// <summary>
/// The correction of an order's shipping or billing address (06 4.62): the old and the new value, the phone of a shipping address included,
/// the administrator and the moment, kept on the order's own record. One row per corrected address type. Only appended
/// (<see cref="OrderingRecordsGuard"/>); the one change is the database emptying the administrator link when the administrator is removed.
/// The order and both province links are restrictive; the administrator link is emptied.
/// </summary>
internal sealed class OrderAddressHistory
{
    public Guid Id { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public Guid OrderId { get; private set; }

    public AddressType AddressType { get; private set; }

    public string OldRecipientName { get; private set; } = string.Empty;

    /// <summary>The shipping phone before the correction; filled on a shipping address, empty on a billing address.</summary>
    public string? OldPhone { get; private set; }

    public Guid OldProvinceId { get; private set; }

    public string OldDistrict { get; private set; } = string.Empty;

    public string OldAddressLine { get; private set; } = string.Empty;

    public string NewRecipientName { get; private set; } = string.Empty;

    /// <summary>The shipping phone after the correction; filled on a shipping address, empty on a billing address.</summary>
    public string? NewPhone { get; private set; }

    public Guid NewProvinceId { get; private set; }

    public string NewDistrict { get; private set; } = string.Empty;

    public string NewAddressLine { get; private set; } = string.Empty;

    /// <summary>The administrator who corrected; emptied when the administrator is removed.</summary>
    public Guid? AdminId { get; private set; }

    /// <summary>The administrator's name, as a value.</summary>
    public string AdminName { get; private set; } = string.Empty;

    public static OrderAddressHistory Create(
        Guid orderId,
        AddressType addressType,
        (string RecipientName, string? Phone, Guid ProvinceId, string District, string AddressLine) oldValue,
        (string RecipientName, string? Phone, Guid ProvinceId, string District, string AddressLine) newValue,
        Guid adminId,
        string adminName,
        DateTimeOffset now)
    {
        return new OrderAddressHistory
        {
            Id = EntityIds.New(),
            CreatedAt = now,
            OrderId = orderId,
            AddressType = addressType,
            OldRecipientName = oldValue.RecipientName,
            OldPhone = oldValue.Phone,
            OldProvinceId = oldValue.ProvinceId,
            OldDistrict = oldValue.District,
            OldAddressLine = oldValue.AddressLine,
            NewRecipientName = newValue.RecipientName,
            NewPhone = newValue.Phone,
            NewProvinceId = newValue.ProvinceId,
            NewDistrict = newValue.District,
            NewAddressLine = newValue.AddressLine,
            AdminId = adminId,
            AdminName = adminName,
        };
    }
}

internal sealed class OrderAddressHistoryConfiguration : IEntityTypeConfiguration<OrderAddressHistory>
{
    // The administrator and the province are other modules' entities (09 6 item 2; 10.1 item 1): the links are database foreign keys, so the principals are named.
    private const string AdministrationAdminType = "Shopfolio.Modules.Administration.Admin";
    private const string SettingsProvinceType = "Shopfolio.Modules.Settings.Province";

    public void Configure(EntityTypeBuilder<OrderAddressHistory> builder)
    {
        builder.HasKey(h => h.Id);
        builder.Property(h => h.OldRecipientName).HasColumnType("text");
        builder.Property(h => h.OldPhone).HasMaxLength(10);
        builder.Property(h => h.OldDistrict).HasColumnType("text");
        builder.Property(h => h.OldAddressLine).HasColumnType("text");
        builder.Property(h => h.NewRecipientName).HasColumnType("text");
        builder.Property(h => h.NewPhone).HasMaxLength(10);
        builder.Property(h => h.NewDistrict).HasColumnType("text");
        builder.Property(h => h.NewAddressLine).HasColumnType("text");
        builder.Property(h => h.AdminName).HasColumnType("text");

        // The order and both province links are restrictive; the administrator link is emptied (06 7.2; 2.2.5).
        builder.HasOne<Order>().WithMany().HasForeignKey(h => h.OrderId).OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(SettingsProvinceType, navigationName: null).WithMany().HasForeignKey(nameof(OrderAddressHistory.OldProvinceId)).OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(SettingsProvinceType, navigationName: null).WithMany().HasForeignKey(nameof(OrderAddressHistory.NewProvinceId)).OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(AdministrationAdminType, navigationName: null).WithMany().HasForeignKey(nameof(OrderAddressHistory.AdminId)).OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(h => new { h.OrderId, h.AddressType, h.CreatedAt });
        builder.HasIndex(h => h.OldProvinceId);
        builder.HasIndex(h => h.NewProvinceId);
        builder.HasIndex(h => h.AdminId);

        builder.HasRule(
            "phones_by_address_type",
            "(address_type = 'ShippingAddress' AND old_phone IS NOT NULL AND new_phone IS NOT NULL) "
            + "OR (address_type = 'BillingAddress' AND old_phone IS NULL AND new_phone IS NULL)");
        builder.HasRule("old_phone_ten_digits", "old_phone ~ '^[0-9]{10}$'");
        builder.HasRule("new_phone_ten_digits", "new_phone ~ '^[0-9]{10}$'");
    }
}
