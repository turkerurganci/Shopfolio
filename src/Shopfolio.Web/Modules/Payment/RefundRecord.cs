using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.DataModel;
using Shopfolio.Modules.Payment.Contracts;

namespace Shopfolio.Modules.Payment;

/// <summary>
/// The refund record (06 4.34): every request that sends money to a card, and its state. The record is written in the "sending" state and
/// committed in its own transaction before the request goes; the outcome is written in a separate transaction. A refund has at most one
/// open request (sending or outcome pending) and at most one successful request, so the same money is never sent twice. The amount, the
/// item and the units are the refund's. The record is never deleted (<see cref="PaymentRecordsGuard"/>): the refund link is restrictive
/// (06 7.2).
/// </summary>
internal sealed class RefundRecord
{
    public Guid Id { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public Guid RefundId { get; set; }

    public RefundRecordState State { get; set; }

    /// <summary>The moment the outcome was written; filled exactly when the state is succeeded or failed.</summary>
    public DateTimeOffset? OutcomeAt { get; set; }

    /// <summary>Whether the firm marked the pending card refund as failed; the mark makes the state failed.</summary>
    public bool IsMarkedFailedByAdmin { get; set; }

    /// <summary>The moment a success came after the firm's mark; filled only on a marked record.</summary>
    public DateTimeOffset? LateSuccessAt { get; set; }

    /// <summary>The record written before the request goes: the "sending" state.</summary>
    public static RefundRecord Sending(Guid refundId, DateTimeOffset now)
    {
        return new RefundRecord
        {
            Id = EntityIds.New(),
            CreatedAt = now,
            RefundId = refundId,
            State = RefundRecordState.Sending,
        };
    }
}

internal sealed class RefundRecordConfiguration : IEntityTypeConfiguration<RefundRecord>
{
    internal const string OneOpenRequestIndex = "OneOpenRefundRequest";
    internal const string OneSuccessfulRequestIndex = "OneSuccessfulRefundRequest";

    private const string Sending = nameof(RefundRecordState.Sending);
    private const string OutcomePending = nameof(RefundRecordState.OutcomePending);
    private const string Succeeded = nameof(RefundRecordState.Succeeded);
    private const string Failed = nameof(RefundRecordState.Failed);

    public void Configure(EntityTypeBuilder<RefundRecord> builder)
    {
        builder.HasKey(r => r.Id);
        builder.Property(r => r.State).HasDefaultValue(RefundRecordState.Sending);
        builder.Property(r => r.IsMarkedFailedByAdmin).HasDefaultValue(false);

        // The refund link is restrictive (06 7.2; 4.17).
        builder.HasOne<Refund>().WithMany().HasForeignKey(r => r.RefundId).OnDelete(DeleteBehavior.NoAction);

        // The refund's requests and its latest state (plain index); at most one open and at most one successful request per refund
        // (06 7.1; 02 7.2.8; 05 4.6.4). Indexes on the same column are told apart by the model's index name.
        builder.HasIndex(r => r.RefundId);
        builder.HasIndex(r => r.RefundId, OneOpenRequestIndex).IsUnique().HasFilter($"state IN ('{Sending}', '{OutcomePending}')");
        builder.HasIndex(r => r.RefundId, OneSuccessfulRequestIndex).IsUnique().HasFilter($"state = '{Succeeded}'");

        builder.HasRule("outcome_at_with_outcome", $"(state IN ('{Succeeded}', '{Failed}')) = (outcome_at IS NOT NULL)");
        builder.HasRule("admin_mark_is_failed", $"NOT is_marked_failed_by_admin OR state = '{Failed}'");
        builder.HasRule("late_success_only_when_marked", "late_success_at IS NULL OR is_marked_failed_by_admin");
    }
}
