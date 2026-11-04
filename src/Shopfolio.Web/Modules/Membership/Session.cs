using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.DataModel;
using Shopfolio.Infrastructure.Values;

namespace Shopfolio.Modules.Membership;

/// <summary>
/// The server-side session (06 4.39): a signed-in account's state. The cookie carries only the random key; the row keeps its digest, the
/// account, whether the session is long-lived and the last activity moment. Exactly one of the customer and the administrator is linked (the
/// account type is read from which link is filled). The two fields of the one-operation reauthentication permission are filled together or
/// empty together, and an administrator session never holds the account deletion permission. Ending a session deletes the row; the row goes
/// with its account (06 2.2.5).
/// </summary>
internal sealed class Session
{
    public Guid Id { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>The digest of the session key; the key itself is never stored (06 2.3.3).</summary>
    public string TokenHash { get; set; } = string.Empty;

    public Guid? CustomerId { get; set; }

    public Guid? AdminId { get; set; }

    public bool IsLongLived { get; set; }

    public DateTimeOffset LastActivityAt { get; set; }

    /// <summary>The operation the reauthentication permits; empty when no permission is held.</summary>
    public ReauthenticationOperation? ReauthenticatedOperation { get; set; }

    /// <summary>The digest of the single-use value the permission is bound to; filled exactly when <see cref="ReauthenticatedOperation"/> is.</summary>
    public string? ReauthenticationTokenHash { get; set; }

    public static Session ForCustomer(Guid customerId, string tokenHash, bool isLongLived, DateTimeOffset now)
    {
        return new Session { Id = EntityIds.New(), CreatedAt = now, CustomerId = customerId, TokenHash = tokenHash, IsLongLived = isLongLived, LastActivityAt = now };
    }

    public static Session ForAdmin(Guid adminId, string tokenHash, bool isLongLived, DateTimeOffset now)
    {
        return new Session { Id = EntityIds.New(), CreatedAt = now, AdminId = adminId, TokenHash = tokenHash, IsLongLived = isLongLived, LastActivityAt = now };
    }
}

internal sealed class SessionConfiguration : IEntityTypeConfiguration<Session>
{
    // The administrator is another module's entity (09 6 item 2; 10.1 item 1): the link is a database foreign key, so the principal is named.
    private const string AdministrationAdminType = "Shopfolio.Modules.Administration.Admin";

    public void Configure(EntityTypeBuilder<Session> builder)
    {
        builder.HasKey(s => s.Id);
        builder.Property(s => s.TokenHash).HasColumnType("text");
        builder.Property(s => s.ReauthenticationTokenHash).HasColumnType("text");

        // The account is deleted: its sessions go with it (06 7.2; 2.2.5; Z-31).
        builder.HasOne<Customer>().WithMany().HasForeignKey(s => s.CustomerId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(AdministrationAdminType, navigationName: null).WithMany().HasForeignKey(nameof(Session.AdminId)).OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(s => s.TokenHash).IsUnique();
        builder.HasIndex(s => s.LastActivityAt);

        builder.HasRule("exactly_one_account", "num_nonnulls(customer_id, admin_id) = 1");
        builder.HasRule("reauthentication_fields_together", "(reauthenticated_operation IS NULL) = (reauthentication_token_hash IS NULL)");
        builder.HasRule("admin_without_account_deletion", "admin_id IS NULL OR reauthenticated_operation IS DISTINCT FROM 'AccountDeletion'");
    }
}
