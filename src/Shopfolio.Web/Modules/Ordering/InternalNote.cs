using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Shopfolio.Infrastructure;

namespace Shopfolio.Modules.Ordering;

/// <summary>
/// The one free-text note an administrator writes on an order (06 4.20); never shown to the customer. At most one row per order (a unique
/// index); an emptied note deletes the row. The note does not raise the order's version: it is written by its own conditional update
/// (09 10.9). The disposal deletes it with an explicit delete; the order link is restrictive, the administrator link is emptied.
/// </summary>
internal sealed class InternalNote
{
    public Guid Id { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public Guid OrderId { get; set; }

    /// <summary>The note's text; an empty text is not stored (the row is deleted instead; K-1051).</summary>
    public string Text { get; set; } = string.Empty;

    /// <summary>The administrator who last wrote the note; emptied when the administrator is removed.</summary>
    public Guid? AdminId { get; set; }

    /// <summary>The name of the administrator who last wrote the note, as a value.</summary>
    public string AdminName { get; set; } = string.Empty;

    public static InternalNote Create(Guid orderId, string text, Guid adminId, string adminName, DateTimeOffset now)
    {
        return new InternalNote { Id = EntityIds.New(), CreatedAt = now, OrderId = orderId, Text = text, AdminId = adminId, AdminName = adminName };
    }
}

internal sealed class InternalNoteConfiguration : IEntityTypeConfiguration<InternalNote>
{
    // The administrator is another module's entity (09 6 item 2; 10.1 item 1): the link is a database foreign key, so the principal is named.
    private const string AdministrationAdminType = "Shopfolio.Modules.Administration.Admin";

    public void Configure(EntityTypeBuilder<InternalNote> builder)
    {
        builder.HasKey(n => n.Id);
        builder.Property(n => n.Text).HasColumnType("text");
        builder.Property(n => n.AdminName).HasColumnType("text");

        // The order link is restrictive (the disposal deletes the note explicitly); the administrator link is emptied (06 7.2; 2.2.5).
        builder.HasOne<Order>().WithMany().HasForeignKey(n => n.OrderId).OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(AdministrationAdminType, navigationName: null).WithMany().HasForeignKey(nameof(InternalNote.AdminId)).OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(n => n.OrderId).IsUnique();
    }
}
