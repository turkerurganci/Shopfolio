using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.DataModel;

namespace Shopfolio.Modules.Payment;

/// <summary>
/// The order's payment record (06 4.32): every collection of the order is one row — the provider's successful card payment (a late payment
/// to a cancelled order included) or the firm's "paid" mark on a bank transfer. The card payment carries the provider's transaction id,
/// unique on the filled rows; the row carries the O1 transition that approved the payment, empty on a payment that did not move the axis
/// (06 4.32; 02 6.2.16). The amount is not stored: it is the order's
/// paid total. The record is never deleted (<see cref="PaymentRecordsGuard"/>): the order and transition links are restrictive (06 7.2).
/// </summary>
internal sealed class PaymentRecord
{
    public Guid Id { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public Guid OrderId { get; set; }

    /// <summary>The provider's payment transaction id: filled on a card payment, empty on a bank transfer; unique on the filled rows.</summary>
    public string? ProviderTransactionId { get; set; }

    /// <summary>The O1 transition written in the same transaction; empty on a late payment to a cancelled order or a second payment to a paid one.</summary>
    public Guid? TransitionRecordId { get; set; }

    /// <summary>The provider's card payment: the transaction id, and the O1 transition unless the payment came late to a cancelled order or a second time to a paid one.</summary>
    public static PaymentRecord ForCardPayment(Guid orderId, string providerTransactionId, Guid? transitionRecordId, DateTimeOffset now)
    {
        return new PaymentRecord
        {
            Id = EntityIds.New(),
            CreatedAt = now,
            OrderId = orderId,
            ProviderTransactionId = providerTransactionId,
            TransitionRecordId = transitionRecordId,
        };
    }

    /// <summary>The firm's "paid" mark on a bank transfer: no transaction id, the O1 transition that carries the administrator.</summary>
    public static PaymentRecord ForBankTransferMark(Guid orderId, Guid transitionRecordId, DateTimeOffset now)
    {
        return new PaymentRecord
        {
            Id = EntityIds.New(),
            CreatedAt = now,
            OrderId = orderId,
            TransitionRecordId = transitionRecordId,
        };
    }
}

internal sealed class PaymentRecordConfiguration : IEntityTypeConfiguration<PaymentRecord>
{
    // The order and the transition record are another module's entities (09 6 item 2; 10.1 item 1): the links are database foreign keys,
    // so the principals are named.
    private const string OrderingOrderType = "Shopfolio.Modules.Ordering.Order";
    private const string OrderingTransitionRecordType = "Shopfolio.Modules.Ordering.TransitionRecord";

    public void Configure(EntityTypeBuilder<PaymentRecord> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.ProviderTransactionId).HasColumnType("text");

        // Both links are restrictive (06 7.2; 4.17).
        builder.HasOne(OrderingOrderType, navigationName: null).WithMany().HasForeignKey(nameof(PaymentRecord.OrderId)).OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(OrderingTransitionRecordType, navigationName: null).WithMany().HasForeignKey(nameof(PaymentRecord.TransitionRecordId)).OnDelete(DeleteBehavior.NoAction);

        // The same payment is processed once: the transaction id is unique on the filled rows (06 7.1; K-970).
        builder.HasIndex(p => p.ProviderTransactionId).IsUnique().HasFilter("provider_transaction_id IS NOT NULL");

        builder.HasRule("transaction_or_transition", "num_nonnulls(provider_transaction_id, transition_record_id) >= 1");
    }
}
