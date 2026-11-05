using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Shopfolio.Infrastructure.DataModel;
using Shopfolio.Infrastructure.Values;

namespace Shopfolio.Infrastructure.Security;

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

    // The digest follows from the id (09 15.1 item 4), so it is written after the id is made.
    private void UseDerivedDigest(LinkKeys keys)
    {
        string digest = keys.DigestFor(Id);
        TokenHash = digest;
    }

    /// <summary>
    /// A link record for a customer whose digest is the derived key's digest (09 15.1 item 4; 05 6.4.4): the id is made first and the digest
    /// follows from it, so the record never holds the key and the e-mail body can derive the key again from the id alone.
    /// </summary>
    public static LinkRecord ForCustomer(LinkKeys keys, LinkType linkType, Guid customerId, DateTimeOffset expiresAt, DateTimeOffset now)
    {
        LinkRecord record = ForCustomer(linkType, customerId, string.Empty, expiresAt, now);
        record.UseDerivedDigest(keys);
        return record;
    }

    /// <summary>The administrator's counterpart of <see cref="ForCustomer(LinkKeys, LinkType, Guid, DateTimeOffset, DateTimeOffset)"/>.</summary>
    public static LinkRecord ForAdmin(LinkKeys keys, LinkType linkType, Guid adminId, DateTimeOffset expiresAt, DateTimeOffset now)
    {
        LinkRecord record = ForAdmin(linkType, adminId, string.Empty, expiresAt, now);
        record.UseDerivedDigest(keys);
        return record;
    }

    /// <summary>The invitation's counterpart of <see cref="ForCustomer(LinkKeys, LinkType, Guid, DateTimeOffset, DateTimeOffset)"/>.</summary>
    public static LinkRecord ForInvitation(LinkKeys keys, Guid adminInvitationId, DateTimeOffset expiresAt, DateTimeOffset now)
    {
        LinkRecord record = ForInvitation(adminInvitationId, string.Empty, expiresAt, now);
        record.UseDerivedDigest(keys);
        return record;
    }

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
    private const string MembershipCustomerType = "Shopfolio.Modules.Membership.Customer";

    public void Configure(EntityTypeBuilder<LinkRecord> builder)
    {
        builder.HasKey(l => l.Id);
        builder.Property(l => l.TokenHash).HasColumnType("text");
        builder.Property(l => l.PreviousEmail).HasColumnType("text");

        // The owner is deleted: the link goes with it (06 7.2; 2.2.5; Z-31).
        builder.HasOne(MembershipCustomerType, navigationName: null).WithMany().HasForeignKey(nameof(LinkRecord.CustomerId)).OnDelete(DeleteBehavior.Cascade);
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
