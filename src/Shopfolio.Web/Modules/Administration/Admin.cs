using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Shopfolio.Infrastructure;

namespace Shopfolio.Modules.Administration;

/// <summary>
/// The administrator account (06 4.37): one role, many users, no permission field. The email is unique among administrator accounts in its
/// normalized form (06 2.8.2; 09 9.4 item 1); an administrator has no verified state and carries no version (06 2.6.5). A null password hash is
/// an invalidated password (06 4.37). Every link that points to an administrator is emptied or cascaded by the owner of that link (06 2.2.5):
/// the audit trail keeps the administrator's id, name and email as values and has no link at all (06 4.63).
/// </summary>
internal sealed class Admin
{
    public Guid Id { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>The administrator's name; the administrator changes it from their own account.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>The email as entered. Written only with its normalized form by <see cref="SetEmail"/>.</summary>
    public string Email { get; private set; } = string.Empty;

    /// <summary>The normalized email: the column of the unique index and of every match (06 2.8.2).</summary>
    public string NormalizedEmail { get; private set; } = string.Empty;

    /// <summary>The digest of the password; empty means the password is invalidated (06 4.37).</summary>
    public string? PasswordHash { get; set; }

    public static Admin Create(string name, string email, string? passwordHash, DateTimeOffset now)
    {
        Admin admin = new() { Id = EntityIds.New(), CreatedAt = now, Name = name, PasswordHash = passwordHash };
        admin.SetEmail(email);
        return admin;
    }

    /// <summary>Writes the email and its normalized form together (06 2.8.2).</summary>
    public void SetEmail(string email)
    {
        ArgumentNullException.ThrowIfNull(email);
        Email = email;
        NormalizedEmail = TextNormalizer.NormalizeIdentity(email);
    }
}

internal sealed class AdminConfiguration : IEntityTypeConfiguration<Admin>
{
    public void Configure(EntityTypeBuilder<Admin> builder)
    {
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Name).HasColumnType("text");
        builder.Property(a => a.Email).HasColumnType("text");
        builder.Property(a => a.NormalizedEmail).HasColumnType("text");
        builder.Property(a => a.PasswordHash).HasColumnType("text");

        builder.HasIndex(a => a.NormalizedEmail).IsUnique();
    }
}
