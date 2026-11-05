using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.DataModel;
using Shopfolio.Modules.AfterSales.Contracts;
using Shopfolio.Modules.Ordering.Contracts;

namespace Shopfolio.Modules.Ordering;

/// <summary>
/// The appended record of an order's status transition (06 4.19; 05 4.1.2): who (customer, administrator or system), when (the creation
/// moment), which transition and why. At most one of the three reasons is filled; the correction fields are filled on the correction row and
/// only there. The row is only appended (<see cref="OrderingRecordsGuard"/>): the one change is the disposal emptying the administrator's link,
/// name and note (<see cref="ErasePersonalData"/>), and the database emptying the link when the administrator is removed.
/// </summary>
internal sealed class TransitionRecord : IOrderVersionExempt
{
    public Guid Id { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public Guid OrderId { get; private set; }

    public TransitionCode TransitionCode { get; private set; }

    public ActorKind ActorKind { get; private set; }

    /// <summary>The administrator who made the transition; emptied when the administrator is removed and by the disposal.</summary>
    public Guid? AdminId { get; private set; }

    /// <summary>The administrator's name as a value; emptied by the disposal.</summary>
    public string? AdminName { get; private set; }

    /// <summary>The kind of an automatic cancellation, or the closure (06 5.6.5).</summary>
    public TransitionReason? TransitionReason { get; private set; }

    /// <summary>The reason of the firm's cancellation (06 5.3.6).</summary>
    public CancellationReason? CancellationReason { get; private set; }

    /// <summary>The reason of a correction; required on the correction row.</summary>
    public CorrectionReason? CorrectionReason { get; private set; }

    /// <summary>The explanation of "Other"; free text, emptied by the disposal.</summary>
    public string? ReasonNote { get; private set; }

    /// <summary>The row a correction corrects; filled exactly on the correction row.</summary>
    public Guid? CorrectedTransitionRecordId { get; private set; }

    public static TransitionRecord Create(
        Guid orderId,
        TransitionCode transitionCode,
        ActorKind actorKind,
        Guid? adminId,
        string? adminName,
        TransitionReason? transitionReason,
        CancellationReason? cancellationReason,
        CorrectionReason? correctionReason,
        string? reasonNote,
        Guid? correctedTransitionRecordId,
        DateTimeOffset now)
    {
        return new TransitionRecord
        {
            Id = EntityIds.New(),
            CreatedAt = now,
            OrderId = orderId,
            TransitionCode = transitionCode,
            ActorKind = actorKind,
            AdminId = adminId,
            AdminName = adminName,
            TransitionReason = transitionReason,
            CancellationReason = cancellationReason,
            CorrectionReason = correctionReason,
            ReasonNote = reasonNote,
            CorrectedTransitionRecordId = correctedTransitionRecordId,
        };
    }

    /// <summary>The disposal's one change of the row: the administrator's link, name and the free note are emptied (06 4.19; K-1030 (1), K-1052).</summary>
    public void ErasePersonalData()
    {
        AdminId = null;
        AdminName = null;
        ReasonNote = null;
    }
}

internal sealed class TransitionRecordConfiguration : IEntityTypeConfiguration<TransitionRecord>
{
    // The administrator is another module's entity (09 6 item 2; 10.1 item 1): the link is a database foreign key, so the principal is named.
    private const string AdministrationAdminType = "Shopfolio.Modules.Administration.Admin";

    private const string CorrectionCode = nameof(TransitionCode.Correction);
    private const string AdminActor = nameof(ActorKind.Admin);
    private const string ClosureReason = nameof(TransitionReason.Closure);

    public void Configure(EntityTypeBuilder<TransitionRecord> builder)
    {
        builder.HasKey(t => t.Id);
        builder.Property(t => t.AdminName).HasColumnType("text");
        builder.Property(t => t.ReasonNote).HasColumnType("text");

        // The order link and the self link are restrictive (the order side's links; K-1047); the administrator link is emptied (06 7.2; 2.2.5).
        builder.HasOne<Order>().WithMany().HasForeignKey(t => t.OrderId).OnDelete(DeleteBehavior.NoAction);
        builder.HasOne<TransitionRecord>().WithMany().HasForeignKey(t => t.CorrectedTransitionRecordId).OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(AdministrationAdminType, navigationName: null).WithMany().HasForeignKey(nameof(TransitionRecord.AdminId)).OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(t => new { t.OrderId, t.CreatedAt });

        builder.HasRule("at_most_one_reason", "num_nonnulls(transition_reason, cancellation_reason, correction_reason) <= 1");
        builder.HasRule(
            "correction_fields_only_on_correction",
            $"(transition_code = '{CorrectionCode}') = (correction_reason IS NOT NULL) "
            + $"AND (transition_code = '{CorrectionCode}') = (corrected_transition_record_id IS NOT NULL) "
            + $"AND (transition_code <> '{CorrectionCode}' OR actor_kind = '{AdminActor}')");
        builder.HasRule(
            "reason_only_on_cancellation_codes",
            "num_nonnulls(transition_reason, cancellation_reason) = 0 OR transition_code IN ('S3', 'S4', 'S9', 'O2')");
        builder.HasRule(
            "auto_cancellation_only_system_s3_o2",
            $"transition_reason IS NULL OR transition_reason = '{ClosureReason}' "
            + $"OR (actor_kind = '{nameof(ActorKind.System)}' AND transition_code IN ('S3', 'O2'))");
        builder.HasRule("closure_only_s3_s4_s9", $"transition_reason IS DISTINCT FROM '{ClosureReason}' OR transition_code IN ('S3', 'S4', 'S9')");
        builder.HasRule("cancellation_reason_by_admin", $"cancellation_reason IS NULL OR actor_kind = '{AdminActor}'");
    }
}
