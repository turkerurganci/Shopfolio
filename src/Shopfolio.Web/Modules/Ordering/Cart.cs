using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.DataModel;

namespace Shopfolio.Modules.Ordering;

/// <summary>
/// The cart of a member or of a guest browser (06 4.15). Exactly one owner is filled: the member's account or the digest of the guest cart's
/// key (the key itself is never stored; 06 2.3.3). A guest cart carries its last access moment, a member's cart does not. Each owner has at
/// most one cart. The member's cart goes with the account; the items go with the cart (06 2.2.5). An order carries the cart's id as a value,
/// not a link (06 4.17, 2.2.6).
/// </summary>
internal sealed class Cart
{
    public Guid Id { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>The member's account; empty on a guest cart.</summary>
    public Guid? CustomerId { get; set; }

    /// <summary>The digest of the guest cart's key; empty on a member's cart.</summary>
    public string? GuestCartKeyHash { get; set; }

    /// <summary>The last access moment of a guest cart; filled exactly when <see cref="GuestCartKeyHash"/> is.</summary>
    public DateTimeOffset? LastAccessedAt { get; set; }

    /// <summary>The mark that the cart tells once that no payment method was available (06 4.15; K-1212).</summary>
    public bool IsPaymentUnavailableNoticePending { get; set; }

    public static Cart ForCustomer(Guid customerId, DateTimeOffset now)
    {
        return new Cart { Id = EntityIds.New(), CreatedAt = now, CustomerId = customerId };
    }

    public static Cart ForGuest(string guestCartKeyHash, DateTimeOffset now)
    {
        return new Cart { Id = EntityIds.New(), CreatedAt = now, GuestCartKeyHash = guestCartKeyHash, LastAccessedAt = now };
    }
}

internal sealed class CartConfiguration : IEntityTypeConfiguration<Cart>
{
    // The account is another module's entity (09 6 item 2; 10.1 item 1): the link is a database foreign key, so the principal is named.
    private const string MembershipCustomerType = "Shopfolio.Modules.Membership.Customer";

    public void Configure(EntityTypeBuilder<Cart> builder)
    {
        builder.HasKey(c => c.Id);
        builder.Property(c => c.GuestCartKeyHash).HasColumnType("text");
        builder.Property(c => c.IsPaymentUnavailableNoticePending).HasDefaultValue(false);

        // The member's cart goes with the account (06 7.2; 2.2.5; Z-32).
        builder.HasOne(MembershipCustomerType, navigationName: null).WithMany().HasForeignKey(nameof(Cart.CustomerId)).OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(c => c.CustomerId).IsUnique();
        builder.HasIndex(c => c.GuestCartKeyHash).IsUnique();
        builder.HasIndex(c => c.LastAccessedAt).HasFilter("guest_cart_key_hash IS NOT NULL");

        builder.HasRule("exactly_one_owner", "num_nonnulls(customer_id, guest_cart_key_hash) = 1");
        builder.HasRule("last_accessed_at_only_for_guest", "(guest_cart_key_hash IS NULL) = (last_accessed_at IS NULL)");
    }
}
