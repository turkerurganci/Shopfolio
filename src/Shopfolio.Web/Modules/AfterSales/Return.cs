using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.DataModel;

namespace Shopfolio.Modules.AfterSales;

/// <summary>
/// The receipt of a returned good (06 4.26): the arrival date the firm enters and the units that arrived; each partial arrival is its own
/// record. The record belongs to a withdrawal declaration, to a late delivery termination (a terminated good back from the carrier) or to
/// a defect claim (the good taken back on a return from the contract) — when written exactly one, in the database at most one, because the
/// claim's link is emptied when the claim is deleted. The record is never deleted (<see cref="AfterSalesRecordsGuard"/>): the item,
/// withdrawal and termination links are restrictive; the defect claim and administrator links are emptied (06 7.2; 2.2.5).
/// </summary>
internal sealed class Return
{
    public Guid Id { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public Guid OrderItemId { get; set; }

    public int Quantity { get; set; }

    /// <summary>The arrival date the firm enters; its correction is written in place.</summary>
    public DateOnly ArrivalDate { get; set; }

    /// <summary>The administrator who received; emptied when the administrator is removed and by the disposal.</summary>
    public Guid? AdminId { get; set; }

    /// <summary>The administrator's name as a value; emptied by the disposal.</summary>
    public string? AdminName { get; set; }

    public Guid? WithdrawalId { get; set; }

    public Guid? LateDeliveryTerminationId { get; set; }

    /// <summary>The defect claim of a good taken back; emptied when the claim is deleted.</summary>
    public Guid? DefectClaimId { get; set; }

    /// <summary>The moment the received units were added to the stock; the step is taken once.</summary>
    public DateTimeOffset? StockAddedAt { get; set; }

    public static Return ForWithdrawal(Guid orderItemId, Guid withdrawalId, int quantity, DateOnly arrivalDate, Guid adminId, string adminName, DateTimeOffset now)
    {
        Return received = New(orderItemId, quantity, arrivalDate, adminId, adminName, now);
        received.WithdrawalId = withdrawalId;
        return received;
    }

    public static Return ForLateDeliveryTermination(
        Guid orderItemId,
        Guid lateDeliveryTerminationId,
        int quantity,
        DateOnly arrivalDate,
        Guid adminId,
        string adminName,
        DateTimeOffset now)
    {
        Return received = New(orderItemId, quantity, arrivalDate, adminId, adminName, now);
        received.LateDeliveryTerminationId = lateDeliveryTerminationId;
        return received;
    }

    public static Return ForDefectClaim(Guid orderItemId, Guid defectClaimId, int quantity, DateOnly arrivalDate, Guid adminId, string adminName, DateTimeOffset now)
    {
        Return received = New(orderItemId, quantity, arrivalDate, adminId, adminName, now);
        received.DefectClaimId = defectClaimId;
        return received;
    }

    private static Return New(Guid orderItemId, int quantity, DateOnly arrivalDate, Guid adminId, string adminName, DateTimeOffset now)
    {
        return new Return
        {
            Id = EntityIds.New(),
            CreatedAt = now,
            OrderItemId = orderItemId,
            Quantity = quantity,
            ArrivalDate = arrivalDate,
            AdminId = adminId,
            AdminName = adminName,
        };
    }
}

internal sealed class ReturnConfiguration : IEntityTypeConfiguration<Return>
{
    // The item and the administrator are other modules' entities (09 6 item 2; 10.1 item 1): the links are database foreign keys, so the
    // principals are named.
    private const string OrderingOrderItemType = "Shopfolio.Modules.Ordering.OrderItem";
    private const string AdministrationAdminType = "Shopfolio.Modules.Administration.Admin";

    public void Configure(EntityTypeBuilder<Return> builder)
    {
        builder.HasKey(r => r.Id);
        builder.Property(r => r.AdminName).HasColumnType("text");

        // The item, withdrawal and termination links are restrictive; the defect claim and administrator links are emptied (06 7.2; 2.2.5).
        builder.HasOne(OrderingOrderItemType, navigationName: null).WithMany().HasForeignKey(nameof(Return.OrderItemId)).OnDelete(DeleteBehavior.NoAction);
        builder.HasOne<Withdrawal>().WithMany().HasForeignKey(r => r.WithdrawalId).OnDelete(DeleteBehavior.NoAction);
        builder.HasOne<LateDeliveryTermination>().WithMany().HasForeignKey(r => r.LateDeliveryTerminationId).OnDelete(DeleteBehavior.NoAction);
        builder.HasOne<DefectClaim>().WithMany().HasForeignKey(r => r.DefectClaimId).OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(AdministrationAdminType, navigationName: null).WithMany().HasForeignKey(nameof(Return.AdminId)).OnDelete(DeleteBehavior.SetNull);

        ItemRecordRules.HasQuantityRule(builder);
        builder.HasRule("at_most_one_link", "num_nonnulls(withdrawal_id, late_delivery_termination_id, defect_claim_id) <= 1");
    }
}
