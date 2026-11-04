using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.DataModel;

namespace Shopfolio.Modules.AfterSales;

/// <summary>
/// The IBAN a customer enters for a bank-transfer refund (06 4.31): kept only for that refund, never on the account, and deleted by its
/// event (the last refund using it completed, the not-returned closure, Z-38, the return rejection, the failed transfer, the corrected firm
/// cancellation) or with the order's disposal. The link goes from the record to the IBAN: the cancellation, the late delivery termination
/// and the withdrawal point to it and their link is emptied when it is deleted. The stored form has no spaces and is uppercase (06 2.5.1);
/// the format and the check digits are the input's.
/// </summary>
internal sealed class RefundIban
{
    public Guid Id { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public Guid OrderId { get; set; }

    /// <summary>The customer's IBAN, without spaces and uppercase; the customer may correct it in place until the refund is processed.</summary>
    public string Iban { get; set; } = string.Empty;

    /// <summary>The moment the IBAN in the field was entered: the creation moment, or the moment of its correction.</summary>
    public DateTimeOffset EnteredAt { get; set; }

    /// <summary>The moment the IBAN is to be deleted (Z-38); written only for a withdrawal after delivery, read by the scheduler.</summary>
    public DateTimeOffset? ScheduledDeletionAt { get; set; }

    public static RefundIban Create(Guid orderId, string iban, DateTimeOffset now)
    {
        return new RefundIban
        {
            Id = EntityIds.New(),
            CreatedAt = now,
            OrderId = orderId,
            Iban = iban,
            EnteredAt = now,
        };
    }
}

internal sealed class RefundIbanConfiguration : IEntityTypeConfiguration<RefundIban>
{
    // The order is another module's entity (09 6 item 2; 10.1 item 1): the link is a database foreign key, so the principal is named.
    private const string OrderingOrderType = "Shopfolio.Modules.Ordering.Order";

    public void Configure(EntityTypeBuilder<RefundIban> builder)
    {
        builder.HasKey(i => i.Id);
        builder.Property(i => i.Iban).HasColumnType("text");

        // The order link is restrictive: the IBAN is deleted by its event or by the disposal's explicit delete (06 7.2; 4.31).
        builder.HasOne(OrderingOrderType, navigationName: null).WithMany().HasForeignKey(nameof(RefundIban.OrderId)).OnDelete(DeleteBehavior.NoAction);

        // The scheduler's scan reads only the rows with a deletion moment (06 7.5; 05 5.1.1).
        builder.HasIndex(i => i.ScheduledDeletionAt).HasFilter("scheduled_deletion_at IS NOT NULL");

        builder.HasRule("iban_capitals_and_digits", "iban ~ '^[A-Z0-9]+$'");
    }
}
