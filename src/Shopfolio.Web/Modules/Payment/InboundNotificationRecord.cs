using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.DataModel;
using Shopfolio.Modules.Payment.Contracts;

namespace Shopfolio.Modules.Payment;

/// <summary>
/// The inbound notification record (06 4.35): the payment provider's notification whose signature holds, written before it is processed,
/// with the job that processes it. It carries no personal field: the provider's references, the order's reference, the amount, the outcome
/// and the moments. The same notification is written once — by its notification id when it has one, otherwise by its transaction id, event
/// type and outcome. The record is deleted by its retention (the disposal's explicit deletion; 06 9.3); the order and refund record links
/// are restrictive (06 7.2).
/// </summary>
internal sealed class InboundNotificationRecord : IOrderVersionExempt
{
    public Guid Id { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>The provider's id of the notification; when present, the notification's uniqueness.</summary>
    public string? ProviderNotificationId { get; set; }

    /// <summary>The provider's transaction reference; with the event type and the outcome, the uniqueness of a notification without an id.</summary>
    public string? ProviderTransactionId { get; set; }

    /// <summary>The provider's event type as it came; the list is the integration specification's, so the column has no allowed values.</summary>
    public string EventType { get; set; } = string.Empty;

    /// <summary>The provider's outcome as it came; same pattern.</summary>
    public string Outcome { get; set; } = string.Empty;

    /// <summary>The order's reference as the notification carried it; kept even when it matches no order.</summary>
    public string? OrderReference { get; set; }

    /// <summary>The notification's amount (money; 06 2.5.1).</summary>
    public decimal? Amount { get; set; }

    /// <summary>The order the reference matched; a notification whose order is not found stays unlinked.</summary>
    public Guid? OrderId { get; set; }

    /// <summary>The refund record of a notification that carries a card refund's outcome.</summary>
    public Guid? RefundRecordId { get; set; }

    /// <summary>The moment the job applied the notification or wrote that it is not processed.</summary>
    public DateTimeOffset? ProcessedAt { get; set; }

    /// <summary>Why the notification is not processed; filled only with the processing moment.</summary>
    public UnprocessedNotificationReason? UnprocessedReason { get; set; }

    public static InboundNotificationRecord Create(
        string? providerNotificationId,
        string? providerTransactionId,
        string eventType,
        string outcome,
        string? orderReference,
        decimal? amount,
        DateTimeOffset now)
    {
        return new InboundNotificationRecord
        {
            Id = EntityIds.New(),
            CreatedAt = now,
            ProviderNotificationId = providerNotificationId,
            ProviderTransactionId = providerTransactionId,
            EventType = eventType,
            Outcome = outcome,
            OrderReference = orderReference,
            Amount = amount,
        };
    }
}

internal sealed class InboundNotificationRecordConfiguration : IEntityTypeConfiguration<InboundNotificationRecord>
{
    internal const string NotificationIdIndex = "UniqueNotificationId";
    internal const string NotificationWithoutIdIndex = "UniqueNotificationWithoutId";

    // The order is another module's entity (09 6 item 2; 10.1 item 1): the link is a database foreign key, so the principal is named.
    private const string OrderingOrderType = "Shopfolio.Modules.Ordering.Order";

    public void Configure(EntityTypeBuilder<InboundNotificationRecord> builder)
    {
        builder.HasKey(n => n.Id);
        builder.Property(n => n.ProviderNotificationId).HasColumnType("text");
        builder.Property(n => n.ProviderTransactionId).HasColumnType("text");
        builder.Property(n => n.EventType).HasColumnType("text");
        builder.Property(n => n.Outcome).HasColumnType("text");
        builder.Property(n => n.OrderReference).HasColumnType("text");
        builder.Property(n => n.Amount).AsMoney();

        // Both links are restrictive (06 7.2; 4.17).
        builder.HasOne(OrderingOrderType, navigationName: null).WithMany().HasForeignKey(nameof(InboundNotificationRecord.OrderId)).OnDelete(DeleteBehavior.NoAction);
        builder.HasOne<RefundRecord>().WithMany().HasForeignKey(n => n.RefundRecordId).OnDelete(DeleteBehavior.NoAction);

        // The uniqueness is the notification itself (06 7.1; K-970): the notification id on the filled rows; the transaction id, the
        // event type and the outcome on the rows without a notification id. The disposal's ten-year scan reads the creation moment (06 7.5).
        builder.HasIndex(n => n.ProviderNotificationId, NotificationIdIndex).IsUnique().HasFilter("provider_notification_id IS NOT NULL");
        builder.HasIndex(n => new { n.ProviderTransactionId, n.EventType, n.Outcome }, NotificationWithoutIdIndex)
            .IsUnique()
            .HasFilter("provider_notification_id IS NULL");
        builder.HasIndex(n => n.CreatedAt);

        builder.HasRule("notification_or_transaction", "num_nonnulls(provider_notification_id, provider_transaction_id) >= 1");
        builder.HasRule("amount_not_negative", "amount >= 0");
        builder.HasRule("reason_only_when_processed", "unprocessed_reason IS NULL OR processed_at IS NOT NULL");
    }
}
