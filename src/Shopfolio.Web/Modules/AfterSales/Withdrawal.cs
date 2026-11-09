using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.DataModel;
using Shopfolio.Modules.Ordering.Contracts;

namespace Shopfolio.Modules.AfterSales;

/// <summary>
/// The withdrawal declaration (06 4.25): the consumer's return from the contract without a reason, for some units of an item too; made
/// from the order page or recorded by the firm for a notice by another channel. Successive declarations are separate records, each with
/// its own date, return address, sending period and refund. The return address shown at the declaration is copied as a value. The record
/// is never deleted (<see cref="AfterSalesRecordsGuard"/>): the item and province links are restrictive, the administrator and IBAN links
/// are emptied (06 7.2; 2.2.5).
/// </summary>
internal sealed class Withdrawal
{
    public Guid Id { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public Guid OrderItemId { get; set; }

    public int Quantity { get; set; }

    /// <summary>The actor: a customer (the order page) or an administrator (the record of a notice by another channel).</summary>
    public ActorKind ActorKind { get; set; }

    /// <summary>The administrator who recorded the notice; emptied when the administrator is removed and by the disposal.</summary>
    public Guid? AdminId { get; set; }

    /// <summary>The administrator's name as a value; emptied by the disposal.</summary>
    public string? AdminName { get; set; }

    /// <summary>The declaration date, written once.</summary>
    public DateOnly DeclarationDate { get; set; }

    /// <summary>The firm's answer whether the delivery came before the declaration, asked when the two fall on the same day.</summary>
    public bool? IsDeliveredBeforeDeclaration { get; set; }

    /// <summary>The return address's recipient; filled on a physical item, empty on a service item.</summary>
    public string? ReturnRecipientName { get; set; }

    public Guid? ReturnProvinceId { get; set; }

    public string? ReturnDistrict { get; set; }

    public string? ReturnAddressLine { get; set; }

    /// <summary>The return address's phone, ten digits; filled only with the address, when the setting has one.</summary>
    public string? ReturnPhone { get; set; }

    public decimal ProductAmount { get; set; }

    public decimal CouponShare { get; set; }

    public decimal VatAmount { get; set; }

    /// <summary>The IBAN entered with the declaration or carried over from the notice; emptied when the IBAN is deleted.</summary>
    public Guid? RefundIbanId { get; set; }

    public static Withdrawal ByBuyer(
        Guid orderItemId,
        int quantity,
        DateOnly declarationDate,
        ReturnAddress? returnAddress,
        ItemAmounts amounts,
        Guid? refundIbanId,
        DateTimeOffset now)
    {
        return New(orderItemId, quantity, ActorKind.Buyer, null, null, declarationDate, returnAddress, amounts, refundIbanId, now);
    }

    public static Withdrawal ByAdmin(
        Guid orderItemId,
        int quantity,
        Guid adminId,
        string adminName,
        DateOnly declarationDate,
        ReturnAddress? returnAddress,
        ItemAmounts amounts,
        Guid? refundIbanId,
        DateTimeOffset now)
    {
        return New(orderItemId, quantity, ActorKind.Admin, adminId, adminName, declarationDate, returnAddress, amounts, refundIbanId, now);
    }

    private static Withdrawal New(
        Guid orderItemId,
        int quantity,
        ActorKind actorKind,
        Guid? adminId,
        string? adminName,
        DateOnly declarationDate,
        ReturnAddress? returnAddress,
        ItemAmounts amounts,
        Guid? refundIbanId,
        DateTimeOffset now)
    {
        return new Withdrawal
        {
            Id = EntityIds.New(),
            CreatedAt = now,
            OrderItemId = orderItemId,
            Quantity = quantity,
            ActorKind = actorKind,
            AdminId = adminId,
            AdminName = adminName,
            DeclarationDate = declarationDate,
            ReturnRecipientName = returnAddress?.RecipientName,
            ReturnProvinceId = returnAddress?.ProvinceId,
            ReturnDistrict = returnAddress?.District,
            ReturnAddressLine = returnAddress?.AddressLine,
            ReturnPhone = returnAddress?.Phone,
            ProductAmount = amounts.ProductAmount,
            CouponShare = amounts.CouponShare,
            VatAmount = amounts.VatAmount,
            RefundIbanId = refundIbanId,
        };
    }
}

internal sealed class WithdrawalConfiguration : IEntityTypeConfiguration<Withdrawal>
{
    // The item, the administrator and the province are other modules' entities (09 6 item 2; 10.1 item 1): the links are database foreign
    // keys, so the principals are named.
    private const string OrderingOrderItemType = "Shopfolio.Modules.Ordering.OrderItem";
    private const string AdministrationAdminType = "Shopfolio.Modules.Administration.Admin";
    private const string SettingsProvinceType = "Shopfolio.Modules.Settings.Province";

    public void Configure(EntityTypeBuilder<Withdrawal> builder)
    {
        builder.HasKey(w => w.Id);
        builder.Property(w => w.ActorKind).HasAllowedValues(ActorKind.Buyer, ActorKind.Admin);
        builder.Property(w => w.AdminName).HasColumnType("text");
        builder.Property(w => w.ReturnRecipientName).HasColumnType("text");
        builder.Property(w => w.ReturnDistrict).HasColumnType("text");
        builder.Property(w => w.ReturnAddressLine).HasColumnType("text");
        builder.Property(w => w.ReturnPhone).HasMaxLength(10);
        builder.Property(w => w.ProductAmount).AsMoney();
        builder.Property(w => w.CouponShare).AsMoney().HasDefaultValue(0m);
        builder.Property(w => w.VatAmount).AsMoney();

        // The item and province links are restrictive; the administrator and IBAN links are emptied (06 7.2; 2.2.5).
        builder.HasOne(OrderingOrderItemType, navigationName: null).WithMany().HasForeignKey(nameof(Withdrawal.OrderItemId)).OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(SettingsProvinceType, navigationName: null).WithMany().HasForeignKey(nameof(Withdrawal.ReturnProvinceId)).OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(AdministrationAdminType, navigationName: null).WithMany().HasForeignKey(nameof(Withdrawal.AdminId)).OnDelete(DeleteBehavior.SetNull);
        builder.HasOne<RefundIban>().WithMany().HasForeignKey(w => w.RefundIbanId).OnDelete(DeleteBehavior.SetNull);

        // The sales summary's cancellation and return count reads the withdrawals of a period by their declaration's date (02 10.6.3).
        builder.HasIndex(w => w.DeclarationDate);

        ItemRecordRules.HasQuantityAndAmountRules(builder);
        ItemRecordRules.HasBuyerWithoutAdminRule(builder);
        ItemRecordRules.HasReturnAddressRules(builder);
    }
}
