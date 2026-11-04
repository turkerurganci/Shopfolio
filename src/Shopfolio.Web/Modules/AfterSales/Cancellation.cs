using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.DataModel;
using Shopfolio.Modules.AfterSales.Contracts;
using Shopfolio.Modules.Ordering.Contracts;

namespace Shopfolio.Modules.AfterSales;

/// <summary>
/// The cancellation of an item or of some of its units after the payment's confirmation, by the customer or by the firm (06 4.22). The
/// record carries its actor and moment (the creation moment) and the amounts it sets aside from the item; the firm's cancellation carries
/// its reason. The record is never deleted (<see cref="AfterSalesRecordsGuard"/>): the item and transition links are restrictive, the
/// administrator and IBAN links are emptied by the database (06 7.2; 2.2.5).
/// </summary>
internal sealed class Cancellation
{
    public Guid Id { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public Guid OrderItemId { get; set; }

    public int Quantity { get; set; }

    /// <summary>The actor: a customer or an administrator.</summary>
    public ActorKind ActorKind { get; set; }

    /// <summary>The administrator of the firm's cancellation; emptied when the administrator is removed and by the disposal.</summary>
    public Guid? AdminId { get; set; }

    /// <summary>The administrator's name as a value; emptied by the disposal.</summary>
    public string? AdminName { get; set; }

    /// <summary>The reason of the firm's cancellation; filled exactly on the administrator's record.</summary>
    public CancellationReason? CancellationReason { get; set; }

    /// <summary>The explanation of "Other"; free text, emptied by the disposal.</summary>
    public string? ReasonNote { get; set; }

    public decimal ProductAmount { get; set; }

    public decimal CouponShare { get; set; }

    public decimal VatAmount { get; set; }

    /// <summary>The transition that closed the order when the cancellation closed its last open units (S3, S4 or S9).</summary>
    public Guid? TransitionRecordId { get; set; }

    /// <summary>The IBAN the customer entered with the cancellation on the bank-transfer line; emptied when the IBAN is deleted.</summary>
    public Guid? RefundIbanId { get; set; }

    public static Cancellation ByBuyer(Guid orderItemId, int quantity, ItemAmounts amounts, Guid? refundIbanId, DateTimeOffset now)
    {
        Cancellation cancellation = New(orderItemId, quantity, ActorKind.Buyer, amounts, now);
        cancellation.RefundIbanId = refundIbanId;
        return cancellation;
    }

    public static Cancellation ByAdmin(
        Guid orderItemId,
        int quantity,
        Guid adminId,
        string adminName,
        CancellationReason reason,
        string? reasonNote,
        ItemAmounts amounts,
        DateTimeOffset now)
    {
        Cancellation cancellation = New(orderItemId, quantity, ActorKind.Admin, amounts, now);
        cancellation.AdminId = adminId;
        cancellation.AdminName = adminName;
        cancellation.CancellationReason = reason;
        cancellation.ReasonNote = reasonNote;
        return cancellation;
    }

    private static Cancellation New(Guid orderItemId, int quantity, ActorKind actorKind, ItemAmounts amounts, DateTimeOffset now)
    {
        return new Cancellation
        {
            Id = EntityIds.New(),
            CreatedAt = now,
            OrderItemId = orderItemId,
            Quantity = quantity,
            ActorKind = actorKind,
            ProductAmount = amounts.ProductAmount,
            CouponShare = amounts.CouponShare,
            VatAmount = amounts.VatAmount,
        };
    }
}

internal sealed class CancellationConfiguration : IEntityTypeConfiguration<Cancellation>
{
    // The item, the transition record and the administrator are other modules' entities (09 6 item 2; 10.1 item 1): the links are database
    // foreign keys, so the principals are named.
    private const string OrderingOrderItemType = "Shopfolio.Modules.Ordering.OrderItem";
    private const string OrderingTransitionRecordType = "Shopfolio.Modules.Ordering.TransitionRecord";
    private const string AdministrationAdminType = "Shopfolio.Modules.Administration.Admin";

    private const string OtherReason = nameof(CancellationReason.Other);

    public void Configure(EntityTypeBuilder<Cancellation> builder)
    {
        builder.HasKey(c => c.Id);
        builder.Property(c => c.ActorKind).HasAllowedValues(ActorKind.Buyer, ActorKind.Admin);
        builder.Property(c => c.AdminName).HasColumnType("text");
        builder.Property(c => c.ReasonNote).HasColumnType("text");
        builder.Property(c => c.ProductAmount).AsMoney();
        builder.Property(c => c.CouponShare).AsMoney().HasDefaultValue(0m);
        builder.Property(c => c.VatAmount).AsMoney();

        // The item and transition links are restrictive (the order side's links); the administrator and IBAN links are emptied (06 7.2; 2.2.5).
        builder.HasOne(OrderingOrderItemType, navigationName: null).WithMany().HasForeignKey(nameof(Cancellation.OrderItemId)).OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(OrderingTransitionRecordType, navigationName: null).WithMany().HasForeignKey(nameof(Cancellation.TransitionRecordId)).OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(AdministrationAdminType, navigationName: null).WithMany().HasForeignKey(nameof(Cancellation.AdminId)).OnDelete(DeleteBehavior.SetNull);
        builder.HasOne<RefundIban>().WithMany().HasForeignKey(c => c.RefundIbanId).OnDelete(DeleteBehavior.SetNull);

        ItemRecordRules.HasQuantityAndAmountRules(builder);

        // The administrator's record carries the reason and the customer's none; the customer's carries no administrator, and the note
        // only comes with "Other" — so the customer's record carries no note either (06 4.22).
        ItemRecordRules.HasBuyerWithoutAdminRule(builder);
        builder.HasRule("reason_by_admin", $"(actor_kind = '{ItemRecordRules.AdminActor}') = (cancellation_reason IS NOT NULL)");
        builder.HasRule("reason_note_only_on_other", $"reason_note IS NULL OR cancellation_reason IS NOT DISTINCT FROM '{OtherReason}'");
        builder.HasRule("refund_iban_only_by_buyer", $"refund_iban_id IS NULL OR actor_kind = '{ItemRecordRules.BuyerActor}'");
    }
}
