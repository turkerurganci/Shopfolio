using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.DataModel;
using Shopfolio.Infrastructure.Values;

namespace Shopfolio.Modules.Membership;

/// <summary>
/// The link record (06 4.42): the technical record of an account link (verification, reset, new address, Google confirmation, revert,
/// invitation). Only the digest of the link key is stored. Exactly one of the customer, the administrator and the invitation is linked, and the
/// invitation link is filled exactly for the invitation type. The state of a link is read from the row: used, superseded or expired. The row
/// goes with its owner (06 2.2.5): an invitation's link goes when the invitation is deleted.
/// </summary>
internal sealed class LinkRecord
{
    public Guid Id { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public LinkType LinkType { get; set; }

    /// <summary>The digest of the link key; the key is derived when the email body is made and never stored (06 4.42; 09 9.4 item 4).</summary>
    public string TokenHash { get; set; } = string.Empty;

    public Guid? CustomerId { get; set; }

    public Guid? AdminId { get; set; }

    public Guid? AdminInvitationId { get; set; }

    public DateTimeOffset ExpiresAt { get; set; }

    /// <summary>The moment the link was consumed; at most one of this and <see cref="SupersededAt"/> is filled.</summary>
    public DateTimeOffset? UsedAt { get; set; }

    public DateTimeOffset? SupersededAt { get; set; }

    /// <summary>The previous address of an email change revert link, as entered; empty in every other type.</summary>
    public string? PreviousEmail { get; set; }

    public static LinkRecord ForCustomer(LinkType linkType, Guid customerId, string tokenHash, DateTimeOffset expiresAt, DateTimeOffset now)
    {
        return new LinkRecord { Id = EntityIds.New(), CreatedAt = now, LinkType = linkType, CustomerId = customerId, TokenHash = tokenHash, ExpiresAt = expiresAt };
    }

    public static LinkRecord ForAdmin(LinkType linkType, Guid adminId, string tokenHash, DateTimeOffset expiresAt, DateTimeOffset now)
    {
        return new LinkRecord { Id = EntityIds.New(), CreatedAt = now, LinkType = linkType, AdminId = adminId, TokenHash = tokenHash, ExpiresAt = expiresAt };
    }

    public static LinkRecord ForInvitation(Guid adminInvitationId, string tokenHash, DateTimeOffset expiresAt, DateTimeOffset now)
    {
        return new LinkRecord
        {
            Id = EntityIds.New(),
            CreatedAt = now,
            LinkType = LinkType.AdminInvitation,
            AdminInvitationId = adminInvitationId,
            TokenHash = tokenHash,
            ExpiresAt = expiresAt,
        };
    }
}

internal sealed class LinkRecordConfiguration : IEntityTypeConfiguration<LinkRecord>
{
    // The administrator and the invitation are another module's entities (09 6 item 2; 10.1 item 1): the links are database foreign keys,
    // so the principals are named.
    private const string AdministrationAdminType = "Shopfolio.Modules.Administration.Admin";
    private const string AdministrationAdminInvitationType = "Shopfolio.Modules.Administration.AdminInvitation";

    public void Configure(EntityTypeBuilder<LinkRecord> builder)
    {
        builder.HasKey(l => l.Id);
        builder.Property(l => l.TokenHash).HasColumnType("text");
        builder.Property(l => l.PreviousEmail).HasColumnType("text");

        // The owner is deleted: the link goes with it (06 7.2; 2.2.5; Z-31).
        builder.HasOne<Customer>().WithMany().HasForeignKey(l => l.CustomerId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(AdministrationAdminType, navigationName: null).WithMany().HasForeignKey(nameof(LinkRecord.AdminId)).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(AdministrationAdminInvitationType, navigationName: null).WithMany().HasForeignKey(nameof(LinkRecord.AdminInvitationId)).OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(l => l.TokenHash).IsUnique();
        builder.HasIndex(l => new { l.LinkType, l.ExpiresAt });
        builder.HasIndex(l => l.ExpiresAt).HasFilter("previous_email IS NOT NULL");

        builder.HasRule("exactly_one_owner", "num_nonnulls(customer_id, admin_id, admin_invitation_id) = 1");
        builder.HasRule("invitation_link_only_for_invitation_type", "(link_type = 'AdminInvitation') = (admin_invitation_id IS NOT NULL)");
        builder.HasRule("customer_only_types_have_customer", "link_type NOT IN ('EmailVerification', 'GoogleLinkConfirmation') OR customer_id IS NOT NULL");
        builder.HasRule("previous_email_only_for_revert_type", "previous_email IS NULL OR link_type = 'EmailChangeRevert'");
        builder.HasRule("at_most_one_of_used_and_superseded", "num_nonnulls(used_at, superseded_at) <= 1");
    }
}
