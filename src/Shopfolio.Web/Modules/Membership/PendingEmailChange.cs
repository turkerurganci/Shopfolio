using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.DataModel;

namespace Shopfolio.Modules.Membership;

/// <summary>
/// The pending email change (06 4.40): the new address a user wrote and that waits for verification; at most one per account. Exactly one of
/// the customer and the administrator is linked, and each of the two links is unique on its own among the filled rows. The new address is not
/// unique: two accounts may wait for the same address and the account's own unique index stops the second at verification. The row goes with
/// its account (06 2.2.5).
/// </summary>
internal sealed class PendingEmailChange
{
    public Guid Id { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public Guid? CustomerId { get; set; }

    public Guid? AdminId { get; set; }

    /// <summary>The new address as entered. Written only with its normalized form by <see cref="SetNewEmail"/>.</summary>
    public string NewEmail { get; private set; } = string.Empty;

    /// <summary>The normalized new address: the column of the uniqueness checks at the request and at verification (06 2.8.2).</summary>
    public string NormalizedNewEmail { get; private set; } = string.Empty;

    public static PendingEmailChange ForCustomer(Guid customerId, string newEmail, DateTimeOffset now)
    {
        PendingEmailChange change = new() { Id = EntityIds.New(), CreatedAt = now, CustomerId = customerId };
        change.SetNewEmail(newEmail);
        return change;
    }

    public static PendingEmailChange ForAdmin(Guid adminId, string newEmail, DateTimeOffset now)
    {
        PendingEmailChange change = new() { Id = EntityIds.New(), CreatedAt = now, AdminId = adminId };
        change.SetNewEmail(newEmail);
        return change;
    }

    /// <summary>Writes the new address and its normalized form together (06 2.8.2).</summary>
    public void SetNewEmail(string newEmail)
    {
        ArgumentNullException.ThrowIfNull(newEmail);
        NewEmail = newEmail;
        NormalizedNewEmail = TextNormalizer.NormalizeIdentity(newEmail);
    }
}

internal sealed class PendingEmailChangeConfiguration : IEntityTypeConfiguration<PendingEmailChange>
{
    // The administrator is another module's entity (09 6 item 2): the link is a database foreign key, so the principal is named, not referenced.
    private const string AdministrationAdminType = "Shopfolio.Modules.Administration.Admin";

    public void Configure(EntityTypeBuilder<PendingEmailChange> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.NewEmail).HasColumnType("text");
        builder.Property(p => p.NormalizedNewEmail).HasColumnType("text");

        // The account is deleted: the pending change goes with it (06 7.2; 2.2.5).
        builder.HasOne<Customer>().WithMany().HasForeignKey(p => p.CustomerId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(AdministrationAdminType, navigationName: null).WithMany().HasForeignKey(nameof(PendingEmailChange.AdminId)).OnDelete(DeleteBehavior.Cascade);

        // One pending change per account: each link is unique among the filled rows (06 4.40; 7.1).
        builder.HasIndex(p => p.CustomerId).IsUnique().HasFilter("customer_id IS NOT NULL");
        builder.HasIndex(p => p.AdminId).IsUnique().HasFilter("admin_id IS NOT NULL");

        builder.HasRule("exactly_one_account", "num_nonnulls(customer_id, admin_id) = 1");
    }
}
