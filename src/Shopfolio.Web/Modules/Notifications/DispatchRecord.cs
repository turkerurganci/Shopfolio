using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.DataModel;
using Shopfolio.Modules.Notifications.Contracts;

namespace Shopfolio.Modules.Notifications;

/// <summary>
/// The dispatch record (06 4.58): every e-mail is one row, and the row is the e-mail's queue. It is written in the "queued" state in the
/// transaction of the event that made it; the body is not stored, it is made at the time of sending. The order link is filled only on a
/// customer notification; the other record the e-mail belongs to (a request, an account, an invitation, an item record, a refund) is carried as
/// a type and an id — a value, not a link (06 2.2.6), because that record may be deleted by its own time before this row. The order link is
/// restrictive: an order with a dispatch record cannot be deleted (06 2.2.5). The deletion by time and the emptying of the recipient and the
/// event values are the disposal's (06 4.58).
/// </summary>
internal sealed class DispatchRecord
{
    public Guid Id { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DispatchType DispatchType { get; set; }

    /// <summary>The recipient's address as entered; emptied only on the unpaid arm of Z-43, filled in every other case (by the application).</summary>
    public string? RecipientEmail { get; set; }

    /// <summary>The order link: filled only on a customer notification (B-1…B-16).</summary>
    public Guid? OrderId { get; set; }

    public DispatchRelatedRecordType? RelatedRecordType { get; set; }

    /// <summary>The id of the related record — a value, not a link (06 2.2.6); filled exactly when <see cref="RelatedRecordType"/> is.</summary>
    public Guid? RelatedRecordId { get; set; }

    /// <summary>The id of the first e-mail's row — a value, not a link (06 2.2.6); filled on a resending and on the B-1 after a correction.</summary>
    public Guid? OriginalDispatchId { get; set; }

    /// <summary>The values of the event as a JSON list (06 2.5.1), stored in a <c>jsonb</c> column; read to make the body, never queried.</summary>
    public string? EventValues { get; set; }

    public DispatchState DispatchState { get; set; } = DispatchState.Queued;

    public DispatchOutcome? LastOutcome { get; set; }

    public int AttemptCount { get; set; }

    /// <summary>The next attempt's moment; empty outside the "queued" state.</summary>
    public DateTimeOffset? NextAttemptAt { get; set; }

    /// <summary>The moment the sending service accepted the message; filled when sent, empty when queued.</summary>
    public DateTimeOffset? SentAt { get; set; }

    /// <summary>A new row in the "queued" state, before any attempt.</summary>
    public static DispatchRecord Queue(
        DispatchType dispatchType,
        string recipientEmail,
        Guid? orderId,
        DispatchRelatedRecordType? relatedRecordType,
        Guid? relatedRecordId,
        DateTimeOffset now)
    {
        return new DispatchRecord
        {
            Id = EntityIds.New(),
            CreatedAt = now,
            DispatchType = dispatchType,
            RecipientEmail = recipientEmail,
            OrderId = orderId,
            RelatedRecordType = relatedRecordType,
            RelatedRecordId = relatedRecordId,
        };
    }
}

internal sealed class DispatchRecordConfiguration : IEntityTypeConfiguration<DispatchRecord>
{
    // The order is another module's entity (09 6 item 2; 10.1 item 1): the link is a database foreign key, so the principal is named.
    private const string OrderingOrderType = "Shopfolio.Modules.Ordering.Order";

    private const string Queued = nameof(DispatchState.Queued);
    private const string Sent = nameof(DispatchState.Sent);

    public void Configure(EntityTypeBuilder<DispatchRecord> builder)
    {
        builder.HasKey(d => d.Id);
        builder.Property(d => d.RecipientEmail).HasColumnType("text");
        builder.Property(d => d.EventValues).HasColumnType("jsonb");
        builder.Property(d => d.DispatchState).HasDefaultValue(DispatchState.Queued);
        builder.Property(d => d.AttemptCount).HasDefaultValue(0);

        // The order link is restrictive (06 2.2.5, 7.2); its index is the first column of (OrderId, CreatedAt) (06 7.5.2). The related record
        // and the first e-mail are values, not links (06 2.2.6, 7.2.3).
        builder.HasOne(OrderingOrderType, navigationName: null).WithMany().HasForeignKey(nameof(DispatchRecord.OrderId)).OnDelete(DeleteBehavior.NoAction);

        // The queue's scan on the "queued" rows; the e-mail history of an order and its disposal; the mark of a request, an invitation and a
        // defect claim; the firm notifications warning and B-3; the time scan of Z-34 (06 4.58, 7.5).
        builder.HasIndex(d => new { d.DispatchState, d.NextAttemptAt }).HasFilter($"dispatch_state = '{Queued}'");
        builder.HasIndex(d => new { d.OrderId, d.CreatedAt });
        builder.HasIndex(d => new { d.RelatedRecordType, d.RelatedRecordId, d.CreatedAt });
        builder.HasIndex(d => new { d.DispatchType, d.CreatedAt });
        builder.HasIndex(d => d.CreatedAt);

        builder.HasRule("order_and_related_record_by_type", OrderAndRelatedRecordByType());
        builder.HasRule("related_record_type_with_id", "(related_record_type IS NULL) = (related_record_id IS NULL)");
        builder.HasRule("sent_at_with_state", $"(dispatch_state <> '{Sent}' OR sent_at IS NOT NULL) AND (dispatch_state <> '{Queued}' OR sent_at IS NULL)");
        builder.HasRule("next_attempt_only_when_queued", $"dispatch_state = '{Queued}' OR next_attempt_at IS NULL");
        builder.HasRule("attempt_count_not_negative", "attempt_count >= 0");
    }

    /// <summary>
    /// The table of 06 4.58 "Türe göre sipariş ve ilgili kayıt" as one check: for every type the order link is filled or empty and the related
    /// record's type is empty or one of the listed values. A null type never passes a list, so each list names "not null" itself.
    /// </summary>
    private static string OrderAndRelatedRecordByType()
    {
        const string withOrder = "order_id IS NOT NULL";
        const string withoutOrder = "order_id IS NULL";
        const string noRelated = "related_record_type IS NULL";

        static string Related(params DispatchRelatedRecordType[] types) =>
            "related_record_type IS NOT NULL AND related_record_type IN (" + string.Join(", ", types.Select(t => $"'{t}'")) + ")";

        string[] customerNotices = [.. Enumerable.Range(1, 16).Where(n => n != 9).Select(n => $"'B{n}'")];
        string claimKinds = Related(DispatchRelatedRecordType.Withdrawal, DispatchRelatedRecordType.LateDeliveryTermination, DispatchRelatedRecordType.DefectClaim);

        return "CASE dispatch_type"
            + $" WHEN {string.Join(" WHEN ", customerNotices.Select(n => $"{n} THEN {withOrder} AND {noRelated}"))}"
            + $" WHEN 'B9' THEN {withOrder} AND {claimKinds}"
            + $" WHEN 'F1' THEN {withoutOrder} AND {Related(DispatchRelatedRecordType.Order)}"
            + $" WHEN 'F2' THEN {withoutOrder} AND {Related(DispatchRelatedRecordType.ContactRequest)}"
            + $" WHEN 'F3' THEN {withoutOrder} AND {claimKinds}"
            + $" WHEN 'F4' THEN {withoutOrder} AND {Related(DispatchRelatedRecordType.Refund)}"
            + $" WHEN 'F5' THEN {withoutOrder} AND {noRelated}"
            + $" WHEN 'F6' THEN {withoutOrder} AND {Related(DispatchRelatedRecordType.AdminInvitation, DispatchRelatedRecordType.Admin)}"
            + $" WHEN '{nameof(DispatchType.EmailVerificationLink)}' THEN {withoutOrder} AND {Related(DispatchRelatedRecordType.Customer)}"
            + $" WHEN '{nameof(DispatchType.AccountDeletedNotice)}' THEN {withoutOrder} AND {Related(DispatchRelatedRecordType.Customer)}"
            + $" WHEN '{nameof(DispatchType.GoogleLinkConfirmationLink)}' THEN {withoutOrder} AND {Related(DispatchRelatedRecordType.Customer)}"
            + $" WHEN '{nameof(DispatchType.PasswordResetLink)}' THEN {withoutOrder} AND {Related(DispatchRelatedRecordType.Customer, DispatchRelatedRecordType.Admin)}"
            + $" WHEN '{nameof(DispatchType.NewEmailVerificationLink)}' THEN {withoutOrder} AND {Related(DispatchRelatedRecordType.Customer, DispatchRelatedRecordType.Admin)}"
            + $" WHEN '{nameof(DispatchType.EmailChangeNotice)}' THEN {withoutOrder} AND {Related(DispatchRelatedRecordType.Customer, DispatchRelatedRecordType.Admin)}"
            + $" WHEN '{nameof(DispatchType.AdminInvitation)}' THEN {withoutOrder} AND {Related(DispatchRelatedRecordType.AdminInvitation)}"
            + " ELSE false END";
    }
}
