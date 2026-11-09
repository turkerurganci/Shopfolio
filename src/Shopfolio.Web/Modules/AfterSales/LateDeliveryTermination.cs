using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.DataModel;
using Shopfolio.Modules.Ordering.Contracts;

namespace Shopfolio.Modules.AfterSales;

/// <summary>
/// The customer's termination of the contract after the thirty-day legal delivery limit (06 4.24): one row per undelivered physical item,
/// covering all its open units, written in one transaction with the same moment and declaration date. The customer terminates from the
/// order page; the firm records a termination that came by another channel. The record is never deleted
/// (<see cref="AfterSalesRecordsGuard"/>): the item link is restrictive, the administrator and IBAN links are emptied (06 7.2; 2.2.5).
/// </summary>
internal sealed class LateDeliveryTermination
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

    /// <summary>The declaration date: the termination's date stamp, written once.</summary>
    public DateOnly DeclarationDate { get; set; }

    public decimal ProductAmount { get; set; }

    public decimal CouponShare { get; set; }

    public decimal VatAmount { get; set; }

    /// <summary>The IBAN entered with the termination on the bank-transfer line; emptied when the IBAN is deleted.</summary>
    public Guid? RefundIbanId { get; set; }

    public static LateDeliveryTermination ByBuyer(
        Guid orderItemId,
        int quantity,
        DateOnly declarationDate,
        ItemAmounts amounts,
        Guid? refundIbanId,
        DateTimeOffset now)
    {
        return New(orderItemId, quantity, ActorKind.Buyer, null, null, declarationDate, amounts, refundIbanId, now);
    }

    public static LateDeliveryTermination ByAdmin(
        Guid orderItemId,
        int quantity,
        Guid adminId,
        string adminName,
        DateOnly declarationDate,
        ItemAmounts amounts,
        Guid? refundIbanId,
        DateTimeOffset now)
    {
        return New(orderItemId, quantity, ActorKind.Admin, adminId, adminName, declarationDate, amounts, refundIbanId, now);
    }

    private static LateDeliveryTermination New(
        Guid orderItemId,
        int quantity,
        ActorKind actorKind,
        Guid? adminId,
        string? adminName,
        DateOnly declarationDate,
        ItemAmounts amounts,
        Guid? refundIbanId,
        DateTimeOffset now)
    {
        return new LateDeliveryTermination
        {
            Id = EntityIds.New(),
            CreatedAt = now,
            OrderItemId = orderItemId,
            Quantity = quantity,
            ActorKind = actorKind,
            AdminId = adminId,
            AdminName = adminName,
            DeclarationDate = declarationDate,
            ProductAmount = amounts.ProductAmount,
            CouponShare = amounts.CouponShare,
            VatAmount = amounts.VatAmount,
            RefundIbanId = refundIbanId,
        };
    }
}

internal sealed class LateDeliveryTerminationConfiguration : IEntityTypeConfiguration<LateDeliveryTermination>
{
    // The item and the administrator are other modules' entities (09 6 item 2; 10.1 item 1): the links are database foreign keys, so the
    // principals are named.
    private const string OrderingOrderItemType = "Shopfolio.Modules.Ordering.OrderItem";
    private const string AdministrationAdminType = "Shopfolio.Modules.Administration.Admin";

    public void Configure(EntityTypeBuilder<LateDeliveryTermination> builder)
    {
        builder.HasKey(t => t.Id);
        builder.Property(t => t.ActorKind).HasAllowedValues(ActorKind.Buyer, ActorKind.Admin);
        builder.Property(t => t.AdminName).HasColumnType("text");
        builder.Property(t => t.ProductAmount).AsMoney();
        builder.Property(t => t.CouponShare).AsMoney().HasDefaultValue(0m);
        builder.Property(t => t.VatAmount).AsMoney();

        // The item link is restrictive; the administrator and IBAN links are emptied (06 7.2; 2.2.5).
        builder.HasOne(OrderingOrderItemType, navigationName: null).WithMany().HasForeignKey(nameof(LateDeliveryTermination.OrderItemId)).OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(AdministrationAdminType, navigationName: null).WithMany().HasForeignKey(nameof(LateDeliveryTermination.AdminId)).OnDelete(DeleteBehavior.SetNull);
        builder.HasOne<RefundIban>().WithMany().HasForeignKey(t => t.RefundIbanId).OnDelete(DeleteBehavior.SetNull);

        // The sales summary's cancellation and return count reads the terminations of a period by their notice's date stamp (02 10.6.3).
        builder.HasIndex(t => t.DeclarationDate);

        ItemRecordRules.HasQuantityAndAmountRules(builder);
        ItemRecordRules.HasBuyerWithoutAdminRule(builder);
    }
}
