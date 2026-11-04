using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.DataModel;
using Shopfolio.Modules.AfterSales.Contracts;

namespace Shopfolio.Modules.AfterSales;

/// <summary>
/// The IBAN request (06 4.29): on the bank-transfer line, a refund that needs an IBAN puts a request on the item — on the refund of a late
/// payment to a cancelled order, on that refund and through it on the order (K-1075). An administrator's step gives birth to every request;
/// the customer fulfils it by entering the IBAN, or it closes unfulfilled. The open request — neither fulfilled nor closed — is the "IBAN
/// pending" mark. The row is deleted by the disposal's explicit delete with the order's personal data (06 2.2.5, 4.17): the item and refund
/// links are restrictive, the administrator and IBAN links are emptied (06 7.2).
/// </summary>
internal sealed class RefundIbanRequest
{
    public Guid Id { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>The item the request is on; empty on the refund of a late payment, whose request is on the refund (K-1075).</summary>
    public Guid? OrderItemId { get; set; }

    public RefundIbanRequestReason Reason { get; set; }

    /// <summary>The administrator whose step gave birth to the request; emptied when the administrator is removed.</summary>
    public Guid? AdminId { get; set; }

    /// <summary>The administrator's name as a value; filled until the disposal deletes the row.</summary>
    public string AdminName { get; set; } = string.Empty;

    /// <summary>The refund whose transfer path was opened after a failed card refund, or whose transfer failed on an itemless request.</summary>
    public Guid? RefundId { get; set; }

    /// <summary>The IBAN that fulfilled the request; emptied when the IBAN is deleted (06 4.31).</summary>
    public Guid? RefundIbanId { get; set; }

    /// <summary>The moment the customer entered the IBAN.</summary>
    public DateTimeOffset? FulfilledAt { get; set; }

    /// <summary>The moment the request closed unfulfilled: the card refund succeeded on the transfer path, or the firm cancellation was corrected.</summary>
    public DateTimeOffset? ClosedAt { get; set; }

    /// <summary>A request on the item; the refund is named only when the transfer path of a failed card refund is opened.</summary>
    public static RefundIbanRequest ForItem(
        Guid orderItemId,
        RefundIbanRequestReason reason,
        Guid? refundId,
        Guid adminId,
        string adminName,
        DateTimeOffset now)
    {
        RefundIbanRequest request = New(reason, adminId, adminName, now);
        request.OrderItemId = orderItemId;
        request.RefundId = refundId;
        return request;
    }

    /// <summary>An itemless request on the refund of a late payment to a cancelled order (K-1075, K-1131).</summary>
    public static RefundIbanRequest ForRefund(Guid refundId, RefundIbanRequestReason reason, Guid adminId, string adminName, DateTimeOffset now)
    {
        RefundIbanRequest request = New(reason, adminId, adminName, now);
        request.RefundId = refundId;
        return request;
    }

    private static RefundIbanRequest New(RefundIbanRequestReason reason, Guid adminId, string adminName, DateTimeOffset now)
    {
        return new RefundIbanRequest
        {
            Id = EntityIds.New(),
            CreatedAt = now,
            Reason = reason,
            AdminId = adminId,
            AdminName = adminName,
        };
    }
}

internal sealed class RefundIbanRequestConfiguration : IEntityTypeConfiguration<RefundIbanRequest>
{
    internal const string OneOpenRequestPerItemIndex = "OneOpenIbanRequestPerItem";
    internal const string OneOpenRequestPerRefundIndex = "OneOpenIbanRequestPerRefund";

    // The item, the refund and the administrator are other modules' entities (09 6 item 2; 10.1 item 1): the links are database foreign
    // keys, so the principals are named.
    private const string OrderingOrderItemType = "Shopfolio.Modules.Ordering.OrderItem";
    private const string PaymentRefundType = "Shopfolio.Modules.Payment.Refund";
    private const string AdministrationAdminType = "Shopfolio.Modules.Administration.Admin";

    private const string CardRefundFailed = nameof(RefundIbanRequestReason.CardRefundFailed);
    private const string TransferFailed = nameof(RefundIbanRequestReason.TransferFailed);

    public void Configure(EntityTypeBuilder<RefundIbanRequest> builder)
    {
        builder.HasKey(r => r.Id);
        builder.Property(r => r.AdminName).HasColumnType("text");

        // The item and refund links are restrictive; the administrator and IBAN links are emptied (06 7.2; 2.2.5).
        builder.HasOne(OrderingOrderItemType, navigationName: null).WithMany().HasForeignKey(nameof(RefundIbanRequest.OrderItemId)).OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(PaymentRefundType, navigationName: null).WithMany().HasForeignKey(nameof(RefundIbanRequest.RefundId)).OnDelete(DeleteBehavior.NoAction);
        builder.HasOne<RefundIban>().WithMany().HasForeignKey(r => r.RefundIbanId).OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(AdministrationAdminType, navigationName: null).WithMany().HasForeignKey(nameof(RefundIbanRequest.AdminId)).OnDelete(DeleteBehavior.SetNull);

        // The item's requests and the refund's requests keep their plain indexes (06 7.2); at most one open request per item and, on an
        // itemless request, per refund (06 7.1; 02 7.4.5; K-1075). Two indexes on the same column are told apart by the model's index name.
        builder.HasIndex(r => r.OrderItemId);
        builder.HasIndex(r => r.OrderItemId, OneOpenRequestPerItemIndex).IsUnique().HasFilter("fulfilled_at IS NULL AND closed_at IS NULL");
        builder.HasIndex(r => r.RefundId);
        builder.HasIndex(r => r.RefundId, OneOpenRequestPerRefundIndex).IsUnique()
            .HasFilter("order_item_id IS NULL AND fulfilled_at IS NULL AND closed_at IS NULL");

        builder.HasRule("fulfilled_or_closed", "num_nonnulls(fulfilled_at, closed_at) <= 1");
        builder.HasRule("iban_only_when_fulfilled", "refund_iban_id IS NULL OR fulfilled_at IS NOT NULL");

        // The refund link by reason (06 4.29): filled on a failed card refund and, on an itemless request, on a failed transfer; empty on
        // every other reason. An itemless request is one of those two and carries the refund (K-1075, K-1131).
        builder.HasRule(
            "refund_by_reason",
            $"(refund_id IS NOT NULL) = (reason = '{CardRefundFailed}' OR (reason = '{TransferFailed}' AND order_item_id IS NULL))");
        builder.HasRule(
            "itemless_on_refund_failure",
            $"order_item_id IS NOT NULL OR (reason IN ('{CardRefundFailed}', '{TransferFailed}') AND refund_id IS NOT NULL)");
    }
}
