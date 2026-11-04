using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.DataModel;

namespace Shopfolio.Modules.Administration;

/// <summary>
/// The invitation of a new administrator (06 4.38): single use, sent by an administrator to an email address. The row is born when the
/// invitation is sent. At most one of the withdrawn, invalidated and accepted moments is filled; while all three are empty the normalized
/// address is unique (one valid invitation per address). The sender link is emptied when the sender is removed, and the sender's name stays
/// as a value (06 2.2.5).
/// </summary>
internal sealed class AdminInvitation
{
    public Guid Id { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>The invited address as entered. Written only with its normalized form by <see cref="SetEmail"/>.</summary>
    public string Email { get; private set; } = string.Empty;

    /// <summary>The normalized address: the column of the filtered unique index and of the "address belongs to an administrator" check.</summary>
    public string NormalizedEmail { get; private set; } = string.Empty;

    /// <summary>The sending administrator; emptied when the sender is removed.</summary>
    public Guid? SenderAdminId { get; set; }

    /// <summary>The sender's name as a value; the panel's invitation row shows it.</summary>
    public string SenderAdminName { get; set; } = string.Empty;

    public DateTimeOffset? WithdrawnAt { get; set; }

    public DateTimeOffset? InvalidatedAt { get; set; }

    public DateTimeOffset? AcceptedAt { get; set; }

    public static AdminInvitation Create(string email, Guid? senderAdminId, string senderAdminName, DateTimeOffset now)
    {
        AdminInvitation invitation = new() { Id = EntityIds.New(), CreatedAt = now, SenderAdminId = senderAdminId, SenderAdminName = senderAdminName };
        invitation.SetEmail(email);
        return invitation;
    }

    /// <summary>Writes the address and its normalized form together (06 2.8.2).</summary>
    public void SetEmail(string email)
    {
        ArgumentNullException.ThrowIfNull(email);
        Email = email;
        NormalizedEmail = TextNormalizer.NormalizeIdentity(email);
    }
}

internal sealed class AdminInvitationConfiguration : IEntityTypeConfiguration<AdminInvitation>
{
    public void Configure(EntityTypeBuilder<AdminInvitation> builder)
    {
        builder.HasKey(i => i.Id);
        builder.Property(i => i.Email).HasColumnType("text");
        builder.Property(i => i.NormalizedEmail).HasColumnType("text");
        builder.Property(i => i.SenderAdminName).HasColumnType("text");

        // The sender is removed: the link is emptied, the name stays (06 7.2; 2.2.5).
        builder.HasOne<Admin>().WithMany().HasForeignKey(i => i.SenderAdminId).OnDelete(DeleteBehavior.SetNull);

        // One valid invitation per address: unique while none of the three moments is filled (06 4.38; K-661).
        builder.HasIndex(i => i.NormalizedEmail).IsUnique().HasFilter("withdrawn_at IS NULL AND invalidated_at IS NULL AND accepted_at IS NULL");

        builder.HasRule("at_most_one_outcome", "num_nonnulls(withdrawn_at, invalidated_at, accepted_at) <= 1");
    }
}
