using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.DataModel;
using Shopfolio.Modules.AfterSales.Contracts;
using Shopfolio.Modules.Ordering.Contracts;

namespace Shopfolio.Modules.AfterSales;

/// <summary>
/// One entry of the defect claim's status history (06 4.30): the moment of the change (<see cref="At"/>, the name the disposal reads - Z-29, K-1054), the new
/// status, the kind of actor (the customer or an administrator) and, when the customer raised the quantity on reopening, the new quantity.
/// </summary>
internal sealed record DefectClaimHistoryEntry(DateTimeOffset At, DefectClaimStatus Status, ActorKind ActorKind, int? Quantity);

/// <summary>
/// The defect claim a customer opens from the order page (06 4.30): one per item (the unique item link), opened with the defective units
/// and a description. A resolved claim is reopened, never opened again; every change of the status appends an entry to the status history.
/// Only the customer opens it; the opening moment is the creation moment. The claim is deleted by its retention (Z-29) in the disposal's
/// explicit delete; the item and province links are restrictive, and the receipt's link to it is emptied when it goes (06 7.2; 2.2.5).
/// </summary>
internal sealed class DefectClaim
{
    public Guid Id { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public Guid OrderItemId { get; set; }

    /// <summary>The defective units; on reopening it may only grow, and only by <see cref="ApplyStatus"/>.</summary>
    public int Quantity { get; private set; }

    /// <summary>The description of the problem; free text that may carry personal data.</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// The claim's status, a line of its own (06 4.30): written only by <see cref="ApplyStatus"/>, which only <see cref="DefectClaimStatusWriter"/> calls - the
    /// AfterSales writer that the transition unit's operations reach through <see cref="IAfterSalesModule"/> (09 9.1 item 4; 09 6 item 2).
    /// </summary>
    public DefectClaimStatus DefectClaimStatus { get; private set; }

    /// <summary>
    /// The status history (06 2.5.1, 4.30): a typed list of values in a <c>jsonb</c> column through the one payload setting (<see cref="PayloadJson"/>, 09 12 item 5).
    /// The claim is born Open with an empty history; the history holds the changes after the opening and is only appended, in the same call that changes the status.
    /// </summary>
    public List<DefectClaimHistoryEntry> StatusHistory { get; private set; } = [];

    /// <summary>The return address's recipient; filled on a physical item.</summary>
    public string? ReturnRecipientName { get; set; }

    public Guid? ReturnProvinceId { get; set; }

    public string? ReturnDistrict { get; set; }

    public string? ReturnAddressLine { get; set; }

    /// <summary>The return address's phone, ten digits; filled only with the address, when the setting has one.</summary>
    public string? ReturnPhone { get; set; }

    public static DefectClaim Create(Guid orderItemId, int quantity, string description, ReturnAddress? returnAddress, DateTimeOffset now)
    {
        return new DefectClaim
        {
            Id = EntityIds.New(),
            CreatedAt = now,
            OrderItemId = orderItemId,
            Quantity = quantity,
            Description = description,
            DefectClaimStatus = DefectClaimStatus.Open,
            StatusHistory = [],
            ReturnRecipientName = returnAddress?.RecipientName,
            ReturnProvinceId = returnAddress?.ProvinceId,
            ReturnDistrict = returnAddress?.District,
            ReturnAddressLine = returnAddress?.AddressLine,
            ReturnPhone = returnAddress?.Phone,
        };
    }

    /// <summary>
    /// The one write of the status after the birth (09 9.1 item 4: the item record's status property has a private setter and one internal method). The status and
    /// its history entry change together (06 4.30): the entry carries the moment, the new status, the actor's kind and the new quantity when the customer raised it.
    /// The rules of the change - which status may follow, who may change it, the window - are the writer's (<see cref="DefectClaimStatusWriter"/>).
    /// </summary>
    internal void ApplyStatus(DefectClaimStatus status, ActorKind actor, DateTimeOffset at, int? quantity)
    {
        if (quantity is { } raised)
        {
            Quantity = raised;
        }

        DefectClaimStatus = status;
        StatusHistory.Add(new DefectClaimHistoryEntry(at, status, actor, quantity));
    }
}

internal sealed class DefectClaimConfiguration : IEntityTypeConfiguration<DefectClaim>
{
    // The item and the province are other modules' entities (09 6 item 2; 10.1 item 1): the links are database foreign keys, so the
    // principals are named.
    private const string OrderingOrderItemType = "Shopfolio.Modules.Ordering.OrderItem";
    private const string SettingsProvinceType = "Shopfolio.Modules.Settings.Province";

    private const string OpenStatus = nameof(DefectClaimStatus.Open);

    public void Configure(EntityTypeBuilder<DefectClaim> builder)
    {
        builder.HasKey(d => d.Id);
        builder.Property(d => d.Description).HasColumnType("text");
        builder.Property(d => d.DefectClaimStatus).HasDefaultValue(DefectClaimStatus.Open);
        // The payload mapping takes the nullable builder; the list itself is never null (the default is the empty list).
        builder.Property(d => d.StatusHistory)!.HasPayloadConversion().HasDefaultValueSql("'[]'::jsonb"); // non-null: the payload mapping takes the nullable builder of the same property
        builder.Property(d => d.ReturnRecipientName).HasColumnType("text");
        builder.Property(d => d.ReturnDistrict).HasColumnType("text");
        builder.Property(d => d.ReturnAddressLine).HasColumnType("text");
        builder.Property(d => d.ReturnPhone).HasMaxLength(10);

        // The item and province links are restrictive (06 7.2; 4.17, 4.3).
        builder.HasOne(OrderingOrderItemType, navigationName: null).WithMany().HasForeignKey(nameof(DefectClaim.OrderItemId)).OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(SettingsProvinceType, navigationName: null).WithMany().HasForeignKey(nameof(DefectClaim.ReturnProvinceId)).OnDelete(DeleteBehavior.NoAction);

        // One claim per item (06 7.1; K-676); the "open claim" counter and list read the open rows (06 7.5).
        builder.HasIndex(d => d.OrderItemId).IsUnique();
        builder.HasIndex(d => d.DefectClaimStatus).HasFilter($"defect_claim_status = '{OpenStatus}'");

        ItemRecordRules.HasQuantityRule(builder);
        ItemRecordRules.HasReturnAddressRules(builder);
    }
}
