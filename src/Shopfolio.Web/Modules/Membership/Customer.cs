using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.DataModel;

namespace Shopfolio.Modules.Membership;

/// <summary>
/// The customer account (06 4.36): the member's record. The unverified pending registration is a row of this table too: the moment the email
/// was verified is empty, the password digest is filled and the Google link is empty (a check constraint). The email is unique among customer
/// accounts in its normalized form, pending rows included (06 2.8.2; 09 9.4 item 1). The account carries no version (06 2.6.5). Everything
/// that belongs to the account (cart, address book, sessions, pending email change, links) is cascaded with it (06 2.2.5).
/// </summary>
internal sealed class Customer
{
    public Guid Id { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>The account's name; the member changes it from their own account.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>The email as entered. Written only with its normalized form by <see cref="SetEmail"/>.</summary>
    public string Email { get; private set; } = string.Empty;

    /// <summary>The normalized email: the column of the unique index and of every match (06 2.8.2).</summary>
    public string NormalizedEmail { get; private set; } = string.Empty;

    /// <summary>The digest of the password; empty means a passwordless account (Google-born or password invalidated).</summary>
    public string? PasswordHash { get; set; }

    /// <summary>The moment the email was verified; empty means an unverified pending registration (06 4.36).</summary>
    public DateTimeOffset? EmailVerifiedAt { get; set; }

    /// <summary>The moment the Google sign-in was linked to the account; empty means the account has no Google sign-in.</summary>
    public DateTimeOffset? GoogleLinkedAt { get; set; }

    public static Customer Create(
        string name,
        string email,
        string? passwordHash,
        DateTimeOffset? emailVerifiedAt,
        DateTimeOffset? googleLinkedAt,
        DateTimeOffset now)
    {
        Customer customer = new()
        {
            Id = EntityIds.New(),
            CreatedAt = now,
            Name = name,
            PasswordHash = passwordHash,
            EmailVerifiedAt = emailVerifiedAt,
            GoogleLinkedAt = googleLinkedAt,
        };
        customer.SetEmail(email);
        return customer;
    }

    /// <summary>Replaces the stored digest with a newer one made with the current hashing settings.</summary>
    public void RenewDigest(string digest) => PasswordHash = digest;

    /// <summary>Writes the email and its normalized form together (06 2.8.2).</summary>
    public void SetEmail(string email)
    {
        ArgumentNullException.ThrowIfNull(email);
        Email = email;
        NormalizedEmail = TextNormalizer.NormalizeIdentity(email);
    }
}

internal sealed class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Name).HasColumnType("text");
        builder.Property(c => c.Email).HasColumnType("text");
        builder.Property(c => c.NormalizedEmail).HasColumnType("text");
        builder.Property(c => c.PasswordHash).HasColumnType("text");

        builder.HasIndex(c => c.NormalizedEmail).IsUnique();

        // An unverified pending registration is born from the sign-up form, with a password, and has no Google link (06 4.36; K-696).
        builder.HasRule("unverified_has_password_and_no_google", "email_verified_at IS NOT NULL OR (password_hash IS NOT NULL AND google_linked_at IS NULL)");
    }
}
