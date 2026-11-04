using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Shopfolio.Infrastructure.DataModel;

namespace Shopfolio.Infrastructure.Security;

/// <summary>
/// The recognized browser mark (06 4.41): the server-side record of the cookie left on a browser after a successful sign-in. Exactly one of
/// the customer and the administrator is linked; the digest of the mark is unique. An invalidated mark is not a state: its expiry is pulled to
/// the invalidation moment (Z-46). The row goes with its account (06 2.2.5).
/// </summary>
internal sealed class RecognizedBrowserMark
{
    public Guid Id { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>The digest of the cookie's random value; the value itself is never stored (06 2.3.3).</summary>
    public string MarkHash { get; set; } = string.Empty;

    public Guid? CustomerId { get; set; }

    public Guid? AdminId { get; set; }

    /// <summary>The last moment the mark is valid.</summary>
    public DateTimeOffset ExpiresAt { get; set; }

    public static RecognizedBrowserMark ForCustomer(Guid customerId, string markHash, DateTimeOffset expiresAt, DateTimeOffset now)
    {
        return new RecognizedBrowserMark { Id = EntityIds.New(), CreatedAt = now, CustomerId = customerId, MarkHash = markHash, ExpiresAt = expiresAt };
    }

    public static RecognizedBrowserMark ForAdmin(Guid adminId, string markHash, DateTimeOffset expiresAt, DateTimeOffset now)
    {
        return new RecognizedBrowserMark { Id = EntityIds.New(), CreatedAt = now, AdminId = adminId, MarkHash = markHash, ExpiresAt = expiresAt };
    }
}

internal sealed class RecognizedBrowserMarkConfiguration : IEntityTypeConfiguration<RecognizedBrowserMark>
{
    // The account owners are module entities and shared infrastructure refers to no module type (09 6 item 2; 10.1 item 1): the links are database foreign keys, so the principals are named.
    private const string AdministrationAdminType = "Shopfolio.Modules.Administration.Admin";
    private const string MembershipCustomerType = "Shopfolio.Modules.Membership.Customer";

    public void Configure(EntityTypeBuilder<RecognizedBrowserMark> builder)
    {
        builder.HasKey(m => m.Id);
        builder.Property(m => m.MarkHash).HasColumnType("text");

        // The account is deleted: its marks go with it (06 7.2; 2.2.5).
        builder.HasOne(MembershipCustomerType, navigationName: null).WithMany().HasForeignKey(nameof(RecognizedBrowserMark.CustomerId)).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(AdministrationAdminType, navigationName: null).WithMany().HasForeignKey(nameof(RecognizedBrowserMark.AdminId)).OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(m => m.MarkHash).IsUnique();
        builder.HasIndex(m => m.ExpiresAt);

        builder.HasRule("exactly_one_account", "num_nonnulls(customer_id, admin_id) = 1");
    }
}
