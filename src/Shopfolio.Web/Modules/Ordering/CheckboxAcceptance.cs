using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Shopfolio.Infrastructure;
using Shopfolio.Modules.Ordering.Contracts;

namespace Shopfolio.Modules.Ordering;

/// <summary>
/// The frozen record of a box ticked at the confirmation step for digital and service items (06 4.21): which box, the text shown and, in
/// <see cref="CheckboxAcceptanceOrderItem"/>, the items it covers. The boxes concern the contract and are not a consent record. One record per
/// box type in an order. The record is never deleted (<see cref="OrderingRecordsGuard"/>); its order link is restrictive.
/// </summary>
internal sealed class CheckboxAcceptance
{
    public Guid Id { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public Guid OrderId { get; set; }

    public CheckboxType CheckboxType { get; set; }

    /// <summary>The text shown at the confirmation step; a past order shows its own text.</summary>
    public string ShownText { get; set; } = string.Empty;

    public static CheckboxAcceptance Create(Guid orderId, CheckboxType checkboxType, string shownText, DateTimeOffset now)
    {
        return new CheckboxAcceptance { Id = EntityIds.New(), CreatedAt = now, OrderId = orderId, CheckboxType = checkboxType, ShownText = shownText };
    }
}

/// <summary>An item a checkbox record covers (06 4.21): the row's existence says the item is in the box's scope. Never deleted; both links are restrictive.</summary>
internal sealed class CheckboxAcceptanceOrderItem
{
    public Guid Id { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public Guid CheckboxAcceptanceId { get; set; }

    public Guid OrderItemId { get; set; }

    public static CheckboxAcceptanceOrderItem Create(Guid checkboxAcceptanceId, Guid orderItemId, DateTimeOffset now)
    {
        return new CheckboxAcceptanceOrderItem { Id = EntityIds.New(), CreatedAt = now, CheckboxAcceptanceId = checkboxAcceptanceId, OrderItemId = orderItemId };
    }
}

internal sealed class CheckboxAcceptanceConfiguration : IEntityTypeConfiguration<CheckboxAcceptance>
{
    public void Configure(EntityTypeBuilder<CheckboxAcceptance> builder)
    {
        builder.HasKey(a => a.Id);
        builder.Property(a => a.ShownText).HasColumnType("text");

        // The order side's links are restrictive (06 7.2; K-1047).
        builder.HasOne<Order>().WithMany().HasForeignKey(a => a.OrderId).OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(a => new { a.OrderId, a.CheckboxType }).IsUnique();
    }
}

internal sealed class CheckboxAcceptanceOrderItemConfiguration : IEntityTypeConfiguration<CheckboxAcceptanceOrderItem>
{
    public void Configure(EntityTypeBuilder<CheckboxAcceptanceOrderItem> builder)
    {
        builder.HasKey(l => l.Id);

        // The order side's links are restrictive (06 7.2; K-1047).
        builder.HasOne<CheckboxAcceptance>().WithMany().HasForeignKey(l => l.CheckboxAcceptanceId).OnDelete(DeleteBehavior.NoAction);
        builder.HasOne<OrderItem>().WithMany().HasForeignKey(l => l.OrderItemId).OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(l => new { l.CheckboxAcceptanceId, l.OrderItemId }).IsUnique();
    }
}
